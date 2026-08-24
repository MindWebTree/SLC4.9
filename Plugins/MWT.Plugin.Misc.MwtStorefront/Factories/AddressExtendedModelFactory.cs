using Microsoft.AspNetCore.Mvc.Rendering;
using MWT.Plugin.Misc.MwtStorefront.Models.Common;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Infrastructure;
using Nop.Services.Attributes;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Web.Factories;
using Nop.Web.Models.Common;

namespace MWT.Plugin.Misc.MwtStorefront.Factories
{
    public partial class AddressExtendedModelFactory : AddressModelFactory, IAddressExtendedModelFactory
    {
        #region Ctor
        public AddressExtendedModelFactory(AddressSettings addressSettings, IAddressService addressService, IAttributeFormatter<AddressAttribute, AddressAttributeValue> addressAttributeFormatter,
            IAttributeParser<AddressAttribute, AddressAttributeValue> addressAttributeParser, IAttributeService<AddressAttribute, AddressAttributeValue> addressAttributeService, ICountryService countryService,
            ILocalizationService localizationService, IStateProvinceService stateProvinceService, IWorkContext workContext)
            : base(addressSettings, addressService, addressAttributeFormatter, addressAttributeParser, addressAttributeService, countryService, localizationService, stateProvinceService, workContext)
        {
        }


        #endregion

        #region Methods

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
                var country = await _countryService.GetCountryByAddressAsync(address);
                var state = await _stateProvinceService.GetStateProvinceByAddressAsync(address);
                model.Id = address.Id;
                model.FirstName = address.FirstName;
                model.LastName = address.LastName;
                model.Email = address.Email;
                model.Company = address.Company;
                model.CountryId = address.CountryId;
                model.CountryName = country != null ? await _localizationService.GetLocalizedAsync(country, x => x.Name) : null;
                model.CountryTwoLetterSeoCode = country?.TwoLetterIsoCode ?? "";
                model.StateAbbreviation = state?.Abbreviation ?? "";
                model.StateProvinceId = address.StateProvinceId;
                model.StateProvinceName = state != null ? await _localizationService.GetLocalizedAsync(state, x => x.Name) : null;
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
                model.Email = customer.Email;
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
            if (_addressAttributeService != null && _addressAttributeParser != null)
            {
                await PrepareCustomAddressAttributesAsync(model, address, overrideAttributesXml);
            }
            if (_addressAttributeFormatter != null && address != null)
            {
                model.FormattedCustomAddressAttributes = await _addressAttributeFormatter.FormatAttributesAsync(address.CustomAttributes);
            }
        }
    }

    #endregion
}

