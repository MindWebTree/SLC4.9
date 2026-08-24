using MWT.Nop.Core.Infrastructure;
using MWT.Nop.Core.Services.Orders;
using MWT.Plugin.Misc.MwtStorefront.Infrastructure.Mapping;
using MWT.Plugin.Misc.MwtStorefront.Models.Checkout;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Gdpr;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Pickup;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Factories;
using Nop.Web.Models.Checkout;
using Nop.Web.Models.Common;
using Nop.Web.Models.Customer;
using Nop.Web.Models.ShoppingCart;
namespace MWT.Plugin.Misc.MwtStorefront.Factories;

public partial class CheckoutExtendedModelFactory : CheckoutModelFactory, ICheckoutExtendedModelFactory
{
    #region Fields

    private readonly IAddressExtendedModelFactory _addressExtendedModelFactory;
    private readonly IShoppingCartExtendedService _shoppingCartExtendedService;
    private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
    private readonly GdprSettings _gdprSettings;
    private readonly IGdprService _gdprService;
    private readonly IStateProvinceService _stateService;
    private readonly ITaxPluginManager _taxPluginManager;
    private readonly CustomerSettings _customerSettings;
    private readonly ForumSettings _forumSettings;


    #endregion


    public CheckoutExtendedModelFactory(AddressSettings addressSettings, CaptchaSettings captchaSettings, CommonSettings commonSettings, IAddressModelFactory addressModelFactory,
        IAddressService addressService, ICountryService countryService, ICurrencyService currencyService, ICustomerService customerService, IGenericAttributeService genericAttributeService,
        ILocalizationService localizationService, IOrderProcessingService orderProcessingService, IOrderTotalCalculationService orderTotalCalculationService, IPaymentPluginManager paymentPluginManager,
        IPaymentService paymentService, IPickupPluginManager pickupPluginManager, IPriceFormatter priceFormatter, IRewardPointService rewardPointService, IShippingPluginManager shippingPluginManager,
        IShippingService shippingService, IShoppingCartService shoppingCartService, IStateProvinceService stateProvinceService, IStoreContext storeContext, IStoreMappingService storeMappingService,
        ITaxService taxService, IWorkContext workContext, OrderSettings orderSettings, PaymentSettings paymentSettings, RewardPointsSettings rewardPointsSettings, ShippingSettings shippingSettings,
        TaxSettings taxSettings, IAddressExtendedModelFactory addressExtendedModelFactory, IShoppingCartExtendedService shoppingCartExtendedService, INewsLetterSubscriptionService newsLetterSubscriptionService,
        GdprSettings gdprSettings, IGdprService gdprService, IStateProvinceService stateService, ITaxPluginManager taxPluginManager, CustomerSettings customerSettings, ForumSettings forumSetting) :
        base(addressSettings, captchaSettings, commonSettings, addressModelFactory, addressService, countryService, currencyService, customerService, genericAttributeService, localizationService, orderProcessingService, orderTotalCalculationService, paymentPluginManager, paymentService, pickupPluginManager, priceFormatter, rewardPointService, shippingPluginManager, shippingService, shoppingCartService, stateProvinceService, storeContext, storeMappingService, taxService, workContext, orderSettings, paymentSettings, rewardPointsSettings, shippingSettings, taxSettings)
    {
        _addressExtendedModelFactory = addressExtendedModelFactory;
        _shoppingCartExtendedService = shoppingCartExtendedService;
        _newsLetterSubscriptionService = newsLetterSubscriptionService;
        _gdprSettings = gdprSettings;
        _gdprService = gdprService;
        _stateService = stateService;
        _taxPluginManager = taxPluginManager;
        _customerSettings = customerSettings;
        _forumSettings = forumSetting;
    }

    public virtual async Task<OnePageCheckoutModel> PrepareCustomOnePageCheckoutModelAsync(IList<ShoppingCartItem> cart)
    {
        if (cart == null)
            throw new ArgumentNullException(nameof(cart));
        var customer = await _workContext.GetCurrentCustomerAsync();

        var model = new OnePageCheckoutModel
        {
            ShippingRequired = await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart),
            DisableBillingAddressCheckoutStep = _orderSettings.DisableBillingAddressCheckoutStep && (await _customerService.GetAddressesByCustomerIdAsync((await _workContext.GetCurrentCustomerAsync()).Id)).Any(),
            DisplayCaptcha = await _customerService.IsGuestAsync(await _customerService.GetShoppingCartCustomerAsync(cart)) && _captchaSettings.Enabled && _captchaSettings.ShowOnCheckoutPageForGuests,
            IsReCaptchaV3 = _captchaSettings.CaptchaType == CaptchaType.ReCaptchaV3,
            ReCaptchaPublicKey = _captchaSettings.ReCaptchaPublicKey
        };
        model.AddressModel = await PrepareCustomShippingAddressModelAsync(customer, null, cart, prePopulateNewAddressWithCustomerFields: true);

        return model;
    }


    public virtual async Task<CheckoutAddressModel> PrepareCustomShippingAddressModelAsync(Customer customer, AddressModel address, IList<ShoppingCartItem> cart,
        int? selectedCountryId = null, bool prePopulateNewAddressWithCustomerFields = false, string overrideAttributesXml = "", bool isEdit = false)
    {
        var model = new CustomerAddressEditModel();
        if (!isEdit)
        {
            var shippingAddress = await _customerService.GetCustomerShippingAddressAsync(await _workContext.GetCurrentCustomerAsync());


            /*********** new Address **************/
            if (shippingAddress == null)
            {
                await _addressExtendedModelFactory.PrepareCustomAddressModelAsync(model.Address,
           address: null,
           excludeProperties: false,
           addressSettings: _addressSettings,
           prePopulateWithCustomerFields: prePopulateNewAddressWithCustomerFields,
              customer: await _workContext.GetCurrentCustomerAsync(),
           loadCountries: async () => await _countryService.GetAllCountriesAsync((await _workContext.GetWorkingLanguageAsync()).Id));
            }

            /************ end ******************/
            else
                await _addressExtendedModelFactory.PrepareCustomAddressModelAsync(model.Address,
                 address: shippingAddress,
                 excludeProperties: false,
                 addressSettings: _addressSettings,
                    customer: await _workContext.GetCurrentCustomerAsync(),
                 loadCountries: async () => await _countryService.GetAllCountriesAsync((await _workContext.GetWorkingLanguageAsync()).Id));


        }
        else
        {
            model.Address.CountryId = selectedCountryId;
            await _addressModelFactory.PrepareAddressModelAsync(model.Address,
                address: address.ToEntity(),
                excludeProperties: false,
                addressSettings: _addressSettings,
                loadCountries: async () => await _countryService.GetAllCountriesForShippingAsync((await _workContext.GetWorkingLanguageAsync()).Id),
                prePopulateWithCustomerFields: prePopulateNewAddressWithCustomerFields,
                customer: await _workContext.GetCurrentCustomerAsync(),
                overrideAttributesXml: overrideAttributesXml);
        }
        if (model.Address.CountryId.HasValue)
        {
            model.Address.CountryTwoLetterSeoCode = (await _countryService.GetCountryByIdAsync((int)model.Address.CountryId))?.TwoLetterIsoCode;
        }

        if (model.Address.StateProvinceId.HasValue)
        {
            model.Address.StateAbbreviation = (await _stateService.GetStateProvinceByIdAsync((int)model.Address.StateProvinceId))?.Abbreviation ?? string.Empty;
        }
        var customAddressModel = model.Address.AddressModelToCheckoutAddressModel();
        customAddressModel = await BindCustomProps(customAddressModel, customer);
        return customAddressModel;
    }
    public virtual async Task<CheckoutShippingMethodModel> PrepareCustomShippingMethodModelAsync(IList<ShoppingCartItem> cart, Address shippingAddress)
    {
        var model = new CheckoutShippingMethodModel
        {
            DisplayPickupInStore = _orderSettings.DisplayPickupInStoreOnShippingMethodPage
        };

        if (_orderSettings.DisplayPickupInStoreOnShippingMethodPage)
            model.PickupPointsModel = await PrepareCheckoutPickupPointsModelAsync(cart);

        var getShippingOptionResponse = await _shippingService.GetShippingOptionsAsync(cart, shippingAddress, await _workContext.GetCurrentCustomerAsync(), storeId: (await _storeContext.GetCurrentStoreAsync()).Id);
        if (getShippingOptionResponse.Success)
        {
            //performance optimization. cache returned shipping options.
            //we'll use them later (after a customer has selected an option).
            await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
                                                   NopCustomerDefaults.OfferedShippingOptionsAttribute,
                                                   getShippingOptionResponse.ShippingOptions,
                                                   (await _storeContext.GetCurrentStoreAsync()).Id);

            foreach (var shippingOption in getShippingOptionResponse.ShippingOptions)
            {
                var soModel = new CheckoutShippingMethodModel.ShippingMethodModel
                {
                    Name = shippingOption.Name,
                    Description = shippingOption.Description,
                    ShippingRateComputationMethodSystemName = shippingOption.ShippingRateComputationMethodSystemName,
                    ShippingOption = shippingOption,
                    CustomShippingMethodDescription = shippingOption.CustomShippingMethodDescription,
                    DisplayOrder = shippingOption.DisplayOrder ?? 0,
                };

                //adjust rate
                var (shippingTotal, _) = await _orderTotalCalculationService.AdjustShippingRateAsync(shippingOption.Rate, cart, shippingOption.IsPickupInStore);

                var (rateBase, _) = await _taxService.GetShippingPriceAsync(shippingTotal, await _workContext.GetCurrentCustomerAsync());
                var rate = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(rateBase, await _workContext.GetWorkingCurrencyAsync());
                soModel.Fee = await _priceFormatter.FormatShippingPriceAsync(rate, true);
                soModel.FeeExcludedAdditionalCharges = soModel.Fee;
                if (Math.Round(rate, 2) == Math.Round(shippingOption.Rate, 2) && shippingOption.AdditionalFee > 0)
                {
                    soModel.FeeExcludedAdditionalCharges = await _priceFormatter.FormatShippingPriceAsync(Math.Round(shippingOption.Rate - shippingOption.AdditionalFee, 2), true);
                    soModel.AdditionalFee = await _priceFormatter.FormatShippingPriceAsync(Math.Round(shippingOption.AdditionalFee, 2), true);
                }
                else
                {
                    decimal _shippingTotal = Math.Round((rate == null ? 0 : Convert.ToDecimal(rate)), 2);
                    if (shippingOption.AdditionalFee > 0 && _shippingTotal > 0)
                        if (_shippingTotal > Math.Round(shippingOption.Rate, 2))
                        {
                            soModel.FeeExcludedAdditionalCharges = await _priceFormatter.FormatShippingPriceAsync(shippingTotal - shippingOption.AdditionalFee, true);
                            soModel.AdditionalFee = await _priceFormatter.FormatShippingPriceAsync(shippingOption.AdditionalFee, true);
                        }
                        else
                        {
                            decimal shippingDiscountPercentage = Math.Round((100 - ((_shippingTotal / (shippingOption.Rate) * 100))), 2);
                            _shippingTotal = (shippingOption.Rate - shippingOption.AdditionalFee);
                            _shippingTotal = Math.Round(_shippingTotal - ((_shippingTotal * shippingDiscountPercentage) / 100), 2);
                            soModel.FeeExcludedAdditionalCharges = await _priceFormatter.FormatShippingPriceAsync(_shippingTotal, true);
                            soModel.AdditionalFee = await _priceFormatter.FormatShippingPriceAsync(
                                Math.Round(shippingOption.AdditionalFee - ((shippingOption.AdditionalFee * shippingDiscountPercentage) / 100), 2), true);
                        }
                }

                model.ShippingMethods.Add(soModel);
            }
            if (model.ShippingMethods.Count > 1)
                model.ShippingMethods = (_shippingSettings.ShippingSorting switch
                {
                    ShippingSortingEnum.ShippingCost => model.ShippingMethods.OrderBy(option => option.Rate),
                    _ => model.ShippingMethods.OrderBy(option => option.DisplayOrder)
                }).ToList();


            bool wgsRequired = false;
            if (await _shoppingCartExtendedService.IsWgsShippingMethodRequired(cart))
            {
                foreach (var shippingmethod in model.ShippingMethods)
                {
                    if (shippingmethod.Name.Contains("white glove service", StringComparison.InvariantCultureIgnoreCase))
                    {
                        wgsRequired = true;
                    }
                }
                if (wgsRequired)
                {
                    //model.ShippingMethods = model.ShippingMethods.OrderByDescending(s=>s.Name).ToList();
                    model.WgsRequired = wgsRequired;
                }

            }
            //find a selected (previously) shipping method
            var selectedShippingOption = await _genericAttributeService.GetAttributeAsync<ShippingOption>(await _workContext.GetCurrentCustomerAsync(),
                    NopCustomerDefaults.SelectedShippingOptionAttribute, (await _storeContext.GetCurrentStoreAsync()).Id);
            if (selectedShippingOption != null)
            {
                var shippingOptionToSelect = model.ShippingMethods.ToList()
                    .Find(so =>
                       !string.IsNullOrEmpty(so.Name) &&
                       so.Name.Equals(selectedShippingOption.Name, StringComparison.InvariantCultureIgnoreCase) &&
                       !string.IsNullOrEmpty(so.ShippingRateComputationMethodSystemName) &&
                       so.ShippingRateComputationMethodSystemName.Equals(selectedShippingOption.ShippingRateComputationMethodSystemName, StringComparison.InvariantCultureIgnoreCase));
                if (shippingOptionToSelect != null)
                {
                    shippingOptionToSelect.Selected = true;
                }
            }
            //if no option has been selected, let's do it for the first one
            if (model.ShippingMethods.FirstOrDefault(so => so.Selected) == null)
            {
                var shippingOptionToSelect = model.ShippingMethods.FirstOrDefault();
                if (shippingOptionToSelect != null)
                {
                    shippingOptionToSelect.Selected = true;
                }
            }

            //notify about shipping from multiple locations
            if (_shippingSettings.NotifyCustomerAboutShippingFromMultipleLocations)
            {
                model.NotifyCustomerAboutShippingFromMultipleLocations = getShippingOptionResponse.ShippingFromMultipleLocations;
            }
        }
        else
        {
            foreach (var error in getShippingOptionResponse.Errors)
                model.Warnings.Add(error);
        }

        return model;
    }

    public virtual async Task<CheckoutAddressModel> PrepareCustomBillingAddressModelAsync(AddressModel address, IList<ShoppingCartItem> cart,
  int? selectedCountryId = null, bool prePopulateNewAddressWithCustomerFields = false, string overrideAttributesXml = "", bool isEdit = false)
    {
        var model = new CustomerAddressEditModel();
        var customer = await _workContext.GetCurrentCustomerAsync();
        int languageId = (await _workContext.GetWorkingLanguageAsync()).Id;
        if (!isEdit)
        {
            var billingAddress = await _customerService.GetCustomerBillingAddressAsync(customer);


            /*********** new Address **************/
            if (billingAddress.Id == 0)
            {
                await _addressModelFactory.PrepareAddressModelAsync(model.Address,
           address: null,
           excludeProperties: false,
           addressSettings: _addressSettings,
           prePopulateWithCustomerFields: prePopulateNewAddressWithCustomerFields,
           loadCountries: async () => await _countryService.GetAllCountriesAsync());
            }

            /************ end ******************/
            else
                await _addressModelFactory.PrepareAddressModelAsync(model.Address,
                 address: billingAddress,
                 excludeProperties: false,
                 addressSettings: _addressSettings,
                 loadCountries: async () => await _countryService.GetAllCountriesAsync(languageId));


        }
        else
        {
            model.Address.CountryId = selectedCountryId;
            await _addressModelFactory.PrepareAddressModelAsync(model.Address,
                address: address.ToEntity(),
                excludeProperties: false,
                addressSettings: _addressSettings,
                loadCountries: async () => await _countryService.GetAllCountriesForShippingAsync(languageId),
                prePopulateWithCustomerFields: prePopulateNewAddressWithCustomerFields,
                customer: customer,
                overrideAttributesXml: overrideAttributesXml);
        }
        model.Address.FullName = CustomCommonHelper.GetCustomerFullName(model.Address.FirstName, model.Address.LastName);
        var addrssModel = model.Address.AddressModelToCheckoutAddressModel();

        if (await _customerService.IsGuestAsync(customer) && _taxSettings.EuVatEnabled)
        {
            addrssModel.VatNumber = customer.VatNumber;
            addrssModel.EuVatEnabled = true;
            addrssModel.EuVatEnabledForGuests = _taxSettings.EuVatEnabledForGuests;
        }
        return addrssModel;

    }
    public virtual async Task<(bool, ZipCodeTaxRateModel model)> GetTaxByZipCode(string zipCode)
    {
        ZipCodeTaxRateModel model = new ZipCodeTaxRateModel();

        var activeTaxProvider = await _taxPluginManager.LoadPrimaryPluginAsync(await _workContext.GetCurrentCustomerAsync(), (await _storeContext.GetCurrentStoreAsync()).Id);
        if (activeTaxProvider == null)
            return (false, model);
        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync());
        var (shoppingCartTotalBase, orderTotalDiscountAmountBase, _, appliedGiftCards, redeemedRewardPoints, redeemedRewardPointsAmount) = await _orderTotalCalculationService.GetShoppingCartTotalAsync(cart);
        decimal total = decimal.Zero;
        if (shoppingCartTotalBase.HasValue)
        {
            total = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartTotalBase.Value, await _workContext.GetWorkingCurrencyAsync());
        }
        var taxTotalRequest = new TaxTotalRequest
        {
            ShoppingCart = cart,
            Customer = await _workContext.GetCurrentCustomerAsync(),
            StoreId = (await _storeContext.GetCurrentStoreAsync()).Id,
            UsePaymentMethodAdditionalFee = false,
            IsCustomorder = false,
            Total = total,
            ShippingCharges = 0,
            ZipCode = zipCode
        };
        var taxTotalResult = await activeTaxProvider.GetTaxTotalAsync(taxTotalRequest);

        if (taxTotalResult.TaxRetrieved)
        {
            model.Tax = await _priceFormatter.FormatPriceAsync(taxTotalResult.TaxTotal, true, false);
            foreach (var tax in taxTotalResult.Taxes)
            {
                tax.Amount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(tax.Amount, await _workContext.GetWorkingCurrencyAsync());
            }
            model.Taxes = taxTotalResult.Taxes;

            foreach (var tr in taxTotalResult.TaxRates)
            {
                model.TaxRates.Add(new OrderTotalsModel.TaxRate
                {
                    Rate = _priceFormatter.FormatTaxRate(tr.Key),
                    Value = await _priceFormatter.FormatPriceAsync(await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(tr.Value, await _workContext.GetWorkingCurrencyAsync()), true, false),
                });
            }
        }
        return (taxTotalResult.TaxRetrieved, model);
    }
    #region Utilities
    public virtual async Task<CheckoutAddressModel> BindCustomProps(CheckoutAddressModel model, Customer customer)
    {
        if (model == null)
        {
            model = new CheckoutAddressModel();
        }

        #region Customer Fields
        if (string.IsNullOrEmpty(model.FirstName))
        {
            model.FirstName = customer.FirstName;
        }
        if (string.IsNullOrEmpty(model.LastName))
        {
            model.LastName = customer.LastName;
        }
        model.FullName = CustomCommonHelper.GetCustomerFullName(model.FirstName, model.LastName);
        model.DisplayVatNumber = _taxSettings.EuVatEnabled;
        model.VatNumber = customer.VatNumber;
        model.GenderEnabled = _customerSettings.GenderEnabled;
        if (string.IsNullOrEmpty(model.Gender))
        {
            model.Gender = customer.Gender;
        }
        model.DateOfBirthEnabled = _customerSettings.DateOfBirthEnabled;
        model.DateOfBirthEnabled = _customerSettings.DateOfBirthEnabled;
        var dateOfBirth = customer.DateOfBirth;
        if (dateOfBirth.HasValue)
        {
            model.DateOfBirthDay = dateOfBirth.Value.Day;
            model.DateOfBirthMonth = dateOfBirth.Value.Month;
            model.DateOfBirthYear = dateOfBirth.Value.Year;
        }

        if (!(await _customerService.IsRegisteredAsync(customer)))
        {
            model.Email = model.Username = await _genericAttributeService.GetAttributeAsync<string>(customer, "Email");
        }
        else
        {
            model.Email = model.Username = customer.Email;
        }

        var newsletter = (await _newsLetterSubscriptionService.GetNewsLetterSubscriptionsByEmailAsync(model.Email, (await _storeContext.GetCurrentStoreAsync()).Id)).FirstOrDefault();
        model.Newsletter = newsletter != null && newsletter.Active;
        model.SignatureEnabled = _forumSettings.ForumsEnabled && _forumSettings.SignaturesEnabled;
        model.Signature = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.SignatureAttribute);
        model.CheckUsernameAvailabilityEnabled = _customerSettings.CheckUsernameAvailabilityEnabled;
        model.AllowUsersToChangeUsernames = _customerSettings.AllowUsersToChangeUsernames;
        model.UsernamesEnabled = _customerSettings.UsernamesEnabled;

        model.Fax = string.IsNullOrEmpty(model.Fax) ? customer.Fax : model.Fax;
        model.FaxEnabled = _customerSettings.FaxEnabled;
        if (_gdprSettings.GdprEnabled)
        {
            var consents = (await _gdprService.GetAllConsentsAsync()).Where(consent => consent.DisplayOnCustomerInfoPage).ToList();
            foreach (var consent in consents)
            {
                var accepted = await _gdprService.IsConsentAcceptedAsync(consent.Id, (await _workContext.GetCurrentCustomerAsync()).Id);

                if (consent == null)
                    throw new ArgumentNullException(nameof(consent));

                var requiredMessage = await _localizationService.GetLocalizedAsync(consent, x => x.RequiredMessage);
                model.GdprConsents.Add(new GdprConsentModel
                {
                    Id = consent.Id,
                    Message = await _localizationService.GetLocalizedAsync(consent, x => x.Message),
                    IsRequired = consent.IsRequired,
                    RequiredMessage = !string.IsNullOrEmpty(requiredMessage) ? requiredMessage : $"'{consent.Message}' is required",
                    Accepted = accepted.HasValue && accepted.Value
                });
            }
        }
        #endregion

        model.PhoneNumber = model.PhoneNumber;

        return model;

    }


    #endregion
}
