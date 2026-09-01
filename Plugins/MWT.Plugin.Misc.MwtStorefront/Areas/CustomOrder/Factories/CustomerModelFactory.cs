using Microsoft.AspNetCore.Mvc.Rendering;
using MWT.Nop.Core.Domain.Address;
using MWT.Nop.Core.Infrastructure;
using MWT.Nop.Core.Services.Customers;
using MWT.Nop.Core.Services.Message;
using MWT.Nop.Core.Services.Orders;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Extensions;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Customers;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Infrastructure;
using Nop.Services.Attributes;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Factories
{
    public partial class CustomerModelFactory : ICustomerModelFactory
    {
        #region Fields

        private readonly CustomerSettings _customerSettings;
        private readonly ICustomerExtendedService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ICountryService _countryService;
        private readonly IOrderExtendedService _orderService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ICustomNewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IStoreContext _storeContext;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly AddressSettings _addressSettings;
        private readonly IWorkContext _workContext;
        private readonly IAttributeParser<AddressAttribute, AddressAttributeValue> _addressAttributeParser;
        private readonly IAttributeService<AddressAttribute, AddressAttributeValue> _addressAttributeService;
        private readonly IAttributeFormatter<AddressAttribute, AddressAttributeValue> _addressAttributeFormatter;
        private readonly IAddressService _addressService;
        private readonly IStoreMappingService _storeMappingService;

        #endregion


        #region Ctor

        public CustomerModelFactory(CustomerSettings customerSettings,
                                    ICustomerExtendedService customerService,
                                    ILocalizationService localizationService,
                                     IStateProvinceService stateProvinceService,
                                     IOrderExtendedService orderService,
                                      IPriceFormatter priceFormatter,
                                      ICustomNewsLetterSubscriptionService newsLetterSubscriptionService,
                                      IStoreContext storeContext,
                                      ICountryService countryService,
                                      IGenericAttributeService genericAttributeService,
                                      AddressSettings addressSettings,
                                      IWorkContext workContext,
                                      IAttributeParser<AddressAttribute, AddressAttributeValue> addressAttributeParser,
                                      IAttributeService<AddressAttribute, AddressAttributeValue> addressAttributeService,
                                      IAttributeFormatter<AddressAttribute, AddressAttributeValue> addressAttributeFormatter,
                                      IAddressService addressService,
                                       IStoreMappingService storeMappingService)
        {
            _customerSettings = customerSettings;
            _customerService = customerService;
            _localizationService = localizationService;
            _stateProvinceService = stateProvinceService;
            _orderService = orderService;
            _priceFormatter = priceFormatter;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _storeContext = storeContext;
            _countryService = countryService;
            _genericAttributeService = genericAttributeService;
            _addressSettings = addressSettings;
            _workContext = workContext;
            _addressAttributeParser = addressAttributeParser;
            _addressAttributeService = addressAttributeService;
            _addressAttributeFormatter = addressAttributeFormatter;
            _addressService = addressService;
            _storeMappingService = storeMappingService;
        }

        #endregion

        #region Methods

        public virtual async Task<CustomerSearchModel> PrepareCustomerSearchModelAsync(CustomerSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual async Task<CustomerListModel> PrepareCustomerListModelAsync(CustomerSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));



            //get customers
            var customers = await _customerService.CustomGetAllCustomersAsync(searchterm: searchModel.SearchTerm,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var model = await new CustomerListModel().PrepareToGridAsync(searchModel, customers, () =>
            {
                return customers.SelectAwait(async customer =>
                {
                    return await PrepareCustomerModel(customer);

                });
            });

            return model;
        }

        public virtual async Task<CustomerModel> PrepareCustomerModel(Customer customer, bool bindShippingAddress = false, bool bindBillingAddress = false)
        {
            var customerModel = new CustomerModel();

            //convert dates to the user time
            customerModel.Email = (await _customerService.IsRegisteredAsync(customer))
                ? customer.Email
                : await _localizationService.GetResourceAsync("Admin.Customers.Guest");

            customerModel.FullName = await _customerService.GetExtendedCustomerFullNameAsync(customer);
            customerModel.Id = customer.Id;
            var shippingaddress = await _customerService.GetCustomerShippingAddressAsync(customer);

            if (bindShippingAddress && shippingaddress != null && shippingaddress.Id != 0)
                customerModel.DefaultShippingAddress = shippingaddress.ToModel<AddressModel>();

            var billingaddress = await _customerService.GetCustomerBillingAddressAsync(customer);

            if (bindBillingAddress && billingaddress != null && billingaddress.Id != 0)
                customerModel.DefaultBillingAddress = billingaddress.ToModel<AddressModel>();

            if (shippingaddress == null || shippingaddress.Id == 0)
                shippingaddress = billingaddress;

            if (shippingaddress != null && shippingaddress.Id != 0)
                customerModel.Location = (string.IsNullOrEmpty(shippingaddress.City) ? "" : shippingaddress.City + ", ") + (await _stateProvinceService.GetStateProvinceByAddressAsync(shippingaddress) is StateProvince stateProvince
                    ? await _localizationService.GetLocalizedAsync(stateProvince, entity => entity.Abbreviation) + " "
                    : string.Empty)
                    + (await _countryService.GetCountryByAddressAsync(shippingaddress) is Country country
                    ? await _localizationService.GetLocalizedAsync(country, entity => entity.Name) + " "
                    : string.Empty)
                    ;
            (int noOfOrders, decimal amount) = await _orderService.GetCustomerStats(customer.Id);
            customerModel.NoOforders = noOfOrders;
            customerModel.Spent = await _priceFormatter.FormatPriceAsync(amount);

            // SUBSCRIPTION

            var newsLetter = (await _newsLetterSubscriptionService.GetNewsLetterSubscriptionsByEmailAsync(customerModel.Email, _storeContext.GetCurrentStore().Id)).FirstOrDefault();
            customerModel.IsSubscribedForEmail = newsLetter != null ? newsLetter.Active : false;
            customerModel.SubscribedOn = newsLetter != null ? newsLetter.UpdatedOnUtc : null;

            // END

            // CUSTOMER FROM

            try
            {
                int years = 0;
                int months = 0;

                double days = Convert.ToInt32((DateTime.UtcNow - customer.CreatedOnUtc).TotalDays);

                var customerFromCreatedStringFormat = await this._localizationService.GetResourceAsync("CustomOrder.Customer.From.CreatedOnFormat");
                string yearLabel = await this._localizationService.GetResourceAsync("Label.Year");
                string monthLabel = await this._localizationService.GetResourceAsync("Label.Month");
                string dayLabel = await this._localizationService.GetResourceAsync("Label.Day");
                string recentCustomerStringFormat = "";
                if (days > 365)
                {
                    years = Convert.ToInt32(days / 365);
                    months = Convert.ToInt32((days - (365 * years)) / 30);
                    days = 0;
                }
                else if (days > 30)
                {
                    months = Convert.ToInt32(days / 30);
                    days = 0;
                }
                else if (days == 0)
                    recentCustomerStringFormat = await this._localizationService.GetResourceAsync("CustomOrder.Customer.RecentAdded");

                customerModel.CustomerFromCreatedString = string.Format(customerFromCreatedStringFormat, years > 0 ? years + " " + yearLabel : "",
                    months > 0 ? months + " " + monthLabel : "", days > 0 ? days + " " + dayLabel : "",
                    recentCustomerStringFormat.Length > 0 ? recentCustomerStringFormat : "");
            }
            catch
            {

            }

            // END


            return customerModel;
        }

        public async Task<CustomerInfoModel> PrepareCustomerInfoModelAsync(Customer customer)
        {
            CustomerInfoModel model = new CustomerInfoModel();
            string fullName = await this._customerService.GetExtendedCustomerFullNameAsync(customer);
            model.FirstName = CustomCommonHelper.GetCustomerFirstName(fullName);
            model.LastName = CustomCommonHelper.GetCustomerLastName(fullName);
            model.Email = await this._customerService.GetCustomerEmailAsync(customer);
            model.Phone = await this._customerService.GetCustomerPhoneAsync(customer);
            return model;
        }

        #region Address


        public virtual async Task<AddressModel> PrepareCustomAddressModelAsync(
            Customer customer, AddressType addressType, int addressId,
            AddressModel address,
            int? selectedCountryId = null, bool prePopulateNewAddressWithCustomerFields = false,
            string overrideAttributesXml = "", bool isEdit = false)
        {
            var model = new CustomerAddressEditModel();
            var _address = new Address();
            if (addressId > 0)
                _address = await _addressService.GetAddressByIdAsync(addressId);
            if (!isEdit)
            {
                /*********** new Address **************/
                if (_address == null || _address.Id == 0)
                {
                    await PrepareCustomAddressModelAsync(model.Address,
               address: null,
               excludeProperties: false,
               addressSettings: _addressSettings,
               prePopulateWithCustomerFields: prePopulateNewAddressWithCustomerFields,
                  customer: customer,
               loadCountries: async () => await _countryService.GetAllCountriesAsync((await _workContext.GetWorkingLanguageAsync()).Id));
                }

                /************ end ******************/
                else
                    await PrepareCustomAddressModelAsync(model.Address,
                    address: _address,
                     excludeProperties: false,
                     addressSettings: _addressSettings,
                     customer: customer,
                     loadCountries: async () => await _countryService.GetAllCountriesAsync((await _workContext.GetWorkingLanguageAsync()).Id));


            }
            else
            {
                model.Address.CountryId = selectedCountryId;
                await PrepareAddressModelAsync(model.Address,
                address: address.ToEntity(),
                    excludeProperties: false,
                    addressSettings: _addressSettings,
                    loadCountries:
                    async () => await _countryService.GetAllCountriesForShippingAsync((await _workContext.GetWorkingLanguageAsync()).Id),
                    prePopulateWithCustomerFields: prePopulateNewAddressWithCustomerFields,
                    customer: customer,
                    overrideAttributesXml: overrideAttributesXml);
            }
            return model.Address;
        }

        public virtual async Task PrepareAddressModelAsync(AddressModel model,
            Address address, bool excludeProperties,
            AddressSettings addressSettings,
            Func<Task<IList<Country>>> loadCountries = null,
            bool prePopulateWithCustomerFields = false,
            Customer customer = null,
            string overrideAttributesXml = "")
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (addressSettings == null)
                throw new ArgumentNullException(nameof(addressSettings));

            if (!excludeProperties && address != null)
            {
                model.Id = address.Id;
                model.FirstName = address.FirstName;
                model.LastName = address.LastName;
                model.Email = address.Email;
                model.Company = address.Company;
                model.CountryId = address.CountryId;
                model.CountryName = await _countryService.GetCountryByAddressAsync(address) is Country country ? await _localizationService.GetLocalizedAsync(country, x => x.Name) : null;
                model.StateProvinceId = address.StateProvinceId;
                model.StateProvinceName = await _stateProvinceService.GetStateProvinceByAddressAsync(address) is StateProvince stateProvince ? await _localizationService.GetLocalizedAsync(stateProvince, x => x.Name) : null;
                model.County = address.County;
                model.City = address.City;
                model.Address1 = address.Address1;
                model.Address2 = address.Address2;
                model.ZipPostalCode = address.ZipPostalCode;
                model.PhoneNumber = address.PhoneNumber;
                model.FaxNumber = address.FaxNumber;
            }

            if (address == null && prePopulateWithCustomerFields)
            {
                if (customer == null)
                    throw new Exception("Customer cannot be null when prepopulating an address");
                model.Email = await this._customerService.GetCustomerEmailAsync(customer);
                model.FirstName = customer.FirstName;
                model.LastName = customer.LastName;
                model.Company = customer.Company;
                model.Address1 = customer.StreetAddress;
                model.Address2 = customer.StreetAddress2;
                model.ZipPostalCode = customer.ZipPostalCode;
                model.City = customer.City;
                model.County = customer.County;
                model.PhoneNumber = await this._customerService.GetCustomerPhoneAsync(customer);
                model.FaxNumber = customer.Fax;
                model.CountryId = customer.CountryId;
                model.CountryName = await _countryService.GetCountryByIdAsync(customer.CountryId) is Country country ? await _localizationService.GetLocalizedAsync(country, x => x.Name) : null;
                model.StateProvinceId = customer.StateProvinceId;
                model.StateProvinceName = await _stateProvinceService.GetStateProvinceByIdAsync(customer.StateProvinceId) is StateProvince stateProvince ? await _localizationService.GetLocalizedAsync(stateProvince, x => x.Name) : null;

            }

            //countries and states
            if (addressSettings.CountryEnabled && loadCountries != null)
            {
                var countries = await loadCountries();

                if (_addressSettings.PreselectCountryIfOnlyOne && countries.Count == 1)
                {
                    model.CountryId = countries[0].Id;
                }
                else
                {
                    model.AvailableCountries.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Address.SelectCountry"), Value = "0" });
                }

                foreach (var c in countries)
                {
                    model.AvailableCountries.Add(new SelectListItem
                    {
                        Text = await _localizationService.GetLocalizedAsync(c, x => x.Name),
                        Value = c.Id.ToString(),
                        Selected = c.Id == model.CountryId
                    });
                }

                if (addressSettings.StateProvinceEnabled)
                {
                    var languageId = (await EngineContext.Current.Resolve<IWorkContext>().GetWorkingLanguageAsync()).Id;
                    var states = (await _stateProvinceService
                        .GetStateProvincesByCountryIdAsync(model.CountryId ?? 0, languageId))
                        .ToList();
                    if (states.Any())
                    {
                        model.AvailableStates.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Address.SelectState"), Value = "0" });

                        foreach (var s in states)
                        {
                            model.AvailableStates.Add(new SelectListItem
                            {
                                Text = await _localizationService.GetLocalizedAsync(s, x => x.Name),
                                Value = s.Id.ToString(),
                                Selected = (s.Id == model.StateProvinceId)
                            });
                        }
                    }
                    else
                    {
                        var anyCountrySelected = model.AvailableCountries.Any(x => x.Selected);
                        model.AvailableStates.Add(new SelectListItem
                        {
                            Text = await _localizationService.GetResourceAsync(anyCountrySelected ? "Address.Other" : "Address.SelectState"),
                            Value = "0"
                        });
                    }
                }
            }

            //form fields
            model.CompanyEnabled = addressSettings.CompanyEnabled;
            model.CompanyRequired = addressSettings.CompanyRequired;
            model.StreetAddressEnabled = addressSettings.StreetAddressEnabled;
            model.StreetAddressRequired = addressSettings.StreetAddressRequired;
            model.StreetAddress2Enabled = addressSettings.StreetAddress2Enabled;
            model.StreetAddress2Required = addressSettings.StreetAddress2Required;
            model.ZipPostalCodeEnabled = addressSettings.ZipPostalCodeEnabled;
            model.ZipPostalCodeRequired = addressSettings.ZipPostalCodeRequired;
            model.CityEnabled = addressSettings.CityEnabled;
            model.CityRequired = addressSettings.CityRequired;
            model.CountyEnabled = addressSettings.CountyEnabled;
            model.CountyRequired = addressSettings.CountyRequired;
            model.CountryEnabled = addressSettings.CountryEnabled;
            model.StateProvinceEnabled = addressSettings.StateProvinceEnabled;
            model.PhoneEnabled = addressSettings.PhoneEnabled;
            model.PhoneRequired = addressSettings.PhoneRequired;
            model.FaxEnabled = addressSettings.FaxEnabled;
            model.FaxRequired = addressSettings.FaxRequired;

            //customer attribute services
            //if (_addressAttributeService != null && _addressAttributeParser != null)
            //{
            //    await PrepareCustomAddressAttributesAsync(model, address, overrideAttributesXml);
            //}
            if (_addressAttributeFormatter != null && address != null)
            {
                model.FormattedCustomAddressAttributes = await _addressAttributeFormatter.FormatAttributesAsync(address.CustomAttributes);
            }
        }

        public virtual async Task PrepareCustomAddressModelAsync(AddressModel model,
        Address address, bool excludeProperties,
        AddressSettings addressSettings,
        Func<Task<IList<Country>>> loadCountries = null,
        bool prePopulateWithCustomerFields = false,
        Customer customer = null,
        string overrideAttributesXml = "")
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (addressSettings == null)
                throw new ArgumentNullException(nameof(addressSettings));

            if (!excludeProperties && address != null)
            {
                model.Id = address.Id;
                model.FirstName = address.FirstName;
                model.LastName = address.LastName;
                model.Email = address.Email;
                model.Company = address.Company;
                model.CountryId = address.CountryId;
                model.CountryName = await _countryService.GetCountryByAddressAsync(address) is Country country ? await _localizationService.GetLocalizedAsync(country, x => x.Name) : null;
                model.StateProvinceId = address.StateProvinceId;
                model.StateProvinceName = await _stateProvinceService.GetStateProvinceByAddressAsync(address) is StateProvince stateProvince ? await _localizationService.GetLocalizedAsync(stateProvince, x => x.Name) : null;
                model.County = address.County;
                model.City = address.City;
                model.Address1 = address.Address1;
                model.Address2 = address.Address2;
                model.ZipPostalCode = address.ZipPostalCode;
                model.PhoneNumber = address.PhoneNumber;
                model.FaxNumber = address.FaxNumber;
            }


            if (address == null && prePopulateWithCustomerFields)
            {
                if (customer != null)
                {
                    model.Email = await this._customerService.GetCustomerEmailAsync(customer);
                    model.FirstName = customer.FirstName;
                    model.LastName = customer.LastName;
                    model.Company = customer.Company;
                    model.Address1 = customer.StreetAddress;
                    model.Address2 = customer.StreetAddress2;
                    model.ZipPostalCode = customer.ZipPostalCode;
                    model.City = customer.City;
                    model.County = customer.County;
                    model.PhoneNumber = customer.Phone;
                    model.FaxNumber = customer.Fax;
                    model.CountryId = customer.CountryId;
                    model.CountryName = await _countryService.GetCountryByIdAsync(customer.CountryId) is Country country ? await _localizationService.GetLocalizedAsync(country, x => x.Name) : null;
                    model.StateProvinceId = customer.StateProvinceId;
                    model.StateProvinceName = await _stateProvinceService.GetStateProvinceByIdAsync(customer.StateProvinceId) is StateProvince stateProvince ? await _localizationService.GetLocalizedAsync(stateProvince, x => x.Name) : null;

                }
            }

            //countries and states
            if (addressSettings.CountryEnabled && loadCountries != null)
            {
                var countries = await loadCountries();

                if (_addressSettings.PreselectCountryIfOnlyOne && countries.Count == 1)
                {
                    model.CountryId = countries[0].Id;
                }
                else
                {
                    if (model.CountryId == 0 || model.CountryId == null)
                        model.CountryId = countries.Where(m => m.Name == "United States of America" || m.Name == "United States").FirstOrDefault()?.Id;

                    model.AvailableCountries.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Address.SelectCountry"), Value = "0" });
                }

                foreach (var c in countries)
                {
                    model.AvailableCountries.Add(new SelectListItem
                    {
                        Text = await _localizationService.GetLocalizedAsync(c, x => x.Name),
                        Value = c.Id.ToString(),
                        Selected = c.Id == model.CountryId
                    });
                }

                if (addressSettings.StateProvinceEnabled)
                {
                    var languageId = (await EngineContext.Current.Resolve<IWorkContext>().GetWorkingLanguageAsync()).Id;
                    var states = (await _stateProvinceService
                        .GetStateProvincesByCountryIdAsync(model.CountryId ?? 0, languageId))
                        .ToList();
                    if (states.Any())
                    {
                        model.AvailableStates.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Address.SelectState"), Value = "0" });

                        foreach (var s in states)
                        {
                            model.AvailableStates.Add(new SelectListItem
                            {
                                Text = await _localizationService.GetLocalizedAsync(s, x => x.Name),
                                Value = s.Id.ToString(),
                                Selected = (s.Id == model.StateProvinceId)
                            });
                        }
                    }
                    else
                    {
                        var anyCountrySelected = model.AvailableCountries.Any(x => x.Selected);
                        model.AvailableStates.Add(new SelectListItem
                        {
                            Text = await _localizationService.GetResourceAsync(anyCountrySelected ? "Address.Other" : "Address.SelectState"),
                            Value = "0"
                        });
                    }
                }
            }

            //form fields
            model.CompanyEnabled = addressSettings.CompanyEnabled;
            model.CompanyRequired = addressSettings.CompanyRequired;
            model.StreetAddressEnabled = addressSettings.StreetAddressEnabled;
            model.StreetAddressRequired = addressSettings.StreetAddressRequired;
            model.StreetAddress2Enabled = addressSettings.StreetAddress2Enabled;
            model.StreetAddress2Required = addressSettings.StreetAddress2Required;
            model.ZipPostalCodeEnabled = addressSettings.ZipPostalCodeEnabled;
            model.ZipPostalCodeRequired = addressSettings.ZipPostalCodeRequired;
            model.CityEnabled = addressSettings.CityEnabled;
            model.CityRequired = addressSettings.CityRequired;
            model.CountyEnabled = addressSettings.CountyEnabled;
            model.CountyRequired = addressSettings.CountyRequired;
            model.CountryEnabled = addressSettings.CountryEnabled;
            model.StateProvinceEnabled = addressSettings.StateProvinceEnabled;
            model.PhoneEnabled = addressSettings.PhoneEnabled;
            model.PhoneRequired = addressSettings.PhoneRequired;
            model.FaxEnabled = addressSettings.FaxEnabled;
            model.FaxRequired = addressSettings.FaxRequired;

            //customer attribute services
            //if (_addressAttributeService != null && _addressAttributeParser != null)
            //{
            //    await PrepareCustomAddressAttributesAsync(model, address, overrideAttributesXml);
            //}
            if (_addressAttributeFormatter != null && address != null)
            {
                model.FormattedCustomAddressAttributes = await _addressAttributeFormatter.FormatAttributesAsync(address.CustomAttributes);
            }
        }


        public virtual async Task<CustomerAddressListModel> PrepareCustomerAddressListModelAsync(Customer customer)
        {
            var addresses = await (await _customerService.GetAddressesByCustomerIdAsync(customer.Id))
                //enabled for the current store
                .WhereAwait(async a => a.CountryId == null || await _storeMappingService.AuthorizeAsync(await _countryService.GetCountryByAddressAsync(a)))
                .ToListAsync();

            var model = new CustomerAddressListModel();
            foreach (var address in addresses)
            {
                var addressModel = new AddressModel();
                await this.PrepareAddressModelAsync(addressModel,
                    address: address,
                    excludeProperties: false,
                    addressSettings: _addressSettings,
                    loadCountries: async () => await _countryService.GetAllCountriesAsync((await _workContext.GetWorkingLanguageAsync()).Id));
                model.Addresses.Add(addressModel);
            }
            return model;
        }

        #endregion

        #endregion

        #region utilities

        //protected virtual async Task PrepareCustomAddressAttributesAsync(AddressModel model,
        //   Address address, string overrideAttributesXml = "")
        //{
        //    var attributes = await _addressAttributeService.GetAllAddressAttributesAsync();
        //    foreach (var attribute in attributes)
        //    {
        //        var attributeModel = new AddressAttributeModel
        //        {
        //            Id = attribute.Id,
        //            Name = await _localizationService.GetLocalizedAsync(attribute, x => x.Name),
        //            IsRequired = attribute.IsRequired,
        //            AttributeControlType = attribute.AttributeControlType,
        //        };

        //        if (attribute.ShouldHaveValues())
        //        {
        //            //values
        //            var attributeValues = await _addressAttributeService.GetAddressAttributeValuesAsync(attribute.Id);
        //            foreach (var attributeValue in attributeValues)
        //            {
        //                var attributeValueModel = new AddressAttributeValueModel
        //                {
        //                    Id = attributeValue.Id,
        //                    Name = await _localizationService.GetLocalizedAsync(attributeValue, x => x.Name),
        //                    IsPreSelected = attributeValue.IsPreSelected
        //                };
        //                attributeModel.Values.Add(attributeValueModel);
        //            }
        //        }

        //        //set already selected attributes
        //        var selectedAddressAttributes = !string.IsNullOrEmpty(overrideAttributesXml) ?
        //            overrideAttributesXml :
        //            address?.CustomAttributes;
        //        switch (attribute.AttributeControlType)
        //        {
        //            case AttributeControlType.DropdownList:
        //            case AttributeControlType.RadioList:
        //            case AttributeControlType.Checkboxes:
        //                {
        //                    if (!string.IsNullOrEmpty(selectedAddressAttributes))
        //                    {
        //                        //clear default selection
        //                        foreach (var item in attributeModel.Values)
        //                            item.IsPreSelected = false;

        //                        //select new values
        //                        var selectedValues = await _addressAttributeParser.ParseAddressAttributeValuesAsync(selectedAddressAttributes);
        //                        foreach (var attributeValue in selectedValues)
        //                            foreach (var item in attributeModel.Values)
        //                                if (attributeValue.Id == item.Id)
        //                                    item.IsPreSelected = true;
        //                    }
        //                }
        //                break;
        //            case AttributeControlType.ReadonlyCheckboxes:
        //                {
        //                    //do nothing
        //                    //values are already pre-set
        //                }
        //                break;
        //            case AttributeControlType.TextBox:
        //            case AttributeControlType.MultilineTextbox:
        //                {
        //                    if (!string.IsNullOrEmpty(selectedAddressAttributes))
        //                    {
        //                        var enteredText = _addressAttributeParser.ParseValues(selectedAddressAttributes, attribute.Id);
        //                        if (enteredText.Any())
        //                            attributeModel.DefaultValue = enteredText[0];
        //                    }
        //                }
        //                break;
        //            case AttributeControlType.ColorSquares:
        //            case AttributeControlType.ImageSquares:
        //            case AttributeControlType.Datepicker:
        //            case AttributeControlType.FileUpload:
        //            default:
        //                //not supported attribute control types
        //                break;
        //        }

        //        model.CustomAddressAttributes.Add(attributeModel);
        //    }
        //}

        #endregion

    }
}
