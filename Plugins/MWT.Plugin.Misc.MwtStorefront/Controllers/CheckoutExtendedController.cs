using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using MWT.Nop.Core.Infrastructure;
using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Service.Zoho;
using MWT.Nop.Core.Services.AbandonedCarts;
using MWT.Nop.Core.Services.Customers;
using MWT.Nop.Core.Services.MailChimp;
using MWT.Nop.Core.Services.Message;
using MWT.Nop.Core.Services.Orders;
using MWT.Nop.Core.Services.Payments;
using MWT.Plugin.Misc.MwtStorefront.Factories;
using MWT.Plugin.Misc.MwtStorefront.Infrastructure.Mapping;
using MWT.Plugin.Misc.MwtStorefront.Models.Checkout;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Services.Attributes;
using Nop.Services.Authentication;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using Nop.Web.Controllers;
using Nop.Web.Factories;
using Nop.Web.Models.Checkout;
using Nop.Web.Models.Checkout.Customizations;
using Nop.Web.Models.Customer;
using Nop.Web.Models.ShoppingCart;
using System.Globalization;

namespace MWT.Plugin.Misc.MwtStorefront.Controllers
{
   [AutoValidateAntiforgeryToken]
 

    public partial class CheckoutExtendedController : CheckoutController
    {

        #region Fields

        private readonly CommonSettings _commonSettings;
        private readonly IGdprService _gdprService;
        private readonly GdprSettings _gdprSettings;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly LocalizationSettings _localizationSettings;
        protected readonly IAttributeParser<CustomerAttribute, CustomerAttributeValue> _customerAttributeParser;
        protected readonly IAttributeService<CustomerAttribute, CustomerAttributeValue> _customerAttributeService;
        protected readonly IAuthenticationService _authenticationService;
        private readonly DateTimeSettings _dateTimeSettings;
        private readonly ICustomWorkflowMessageService _customWorkflowMessageService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly ForumSettings _forumSettings;
        private readonly ICustomerModelFactory _customerModelFactory;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IMailchimpService _mailchimpService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IZohoService _zohoService;
        private readonly ICustomerExtendedService _customerExtendedService;
        private readonly ICheckoutExtendedModelFactory _checkoutExtendedModelFactory;
        private readonly IAbandonedCartService _abandonedCartService;
        private readonly IOrderExtendedModelFactory _orderModelFactory;
        private readonly IPaymentSessionService _paymentSessionService;
        private readonly IOrderTotalCalculationExtendedService _orderTotalCalculationService;
        private readonly IOrderProcessingExtendedService _orderProcessingExtendedService;
        private readonly ICurrencyService _currencyService;
        private readonly IShoppingCartExtendedModelFactory _shoppingCartModelFactory;
        private readonly ISettingService _settingService;
        private static readonly string[] _separator = ["___"];
        #endregion
        public CheckoutExtendedController(AddressSettings addressSettings, CaptchaSettings captchaSettings, CustomerSettings customerSettings, IAddressModelFactory addressModelFactory, IAddressService addressService,
            IAttributeParser<AddressAttribute, AddressAttributeValue> addressAttributeParser, ICheckoutModelFactory checkoutModelFactory, ICountryService countryService, ICustomerService customerService,
            IGenericAttributeService genericAttributeService, ILocalizationService localizationService, ILogger logger, IOrderProcessingService orderProcessingService, IOrderService orderService,
            IPaymentPluginManager paymentPluginManager, IPaymentService paymentService, IProductService productService, IShippingService shippingService, IShoppingCartService shoppingCartService,
            IStoreContext storeContext, ITaxService taxService, IWebHelper webHelper, IWorkContext workContext, OrderSettings orderSettings, PaymentSettings paymentSettings, RewardPointsSettings rewardPointsSettings,
            ShippingSettings shippingSettings, TaxSettings taxSettings,
            CommonSettings commonSettings,
     IGdprService gdprService,
     GdprSettings gdprSettings,
     ICustomerRegistrationService customerRegistrationService,
     LocalizationSettings localizationSettings,
     IAttributeParser<CustomerAttribute, CustomerAttributeValue> customerAttributeParser,
     IAttributeService<CustomerAttribute, CustomerAttributeValue> customerAttributeService,
     IAuthenticationService authenticationService,
     DateTimeSettings dateTimeSettings,
     ICustomWorkflowMessageService customWorkflowMessageService,
     INewsLetterSubscriptionService newsLetterSubscriptionService,
     ICustomerModelFactory customerModelFactory,
     IStateProvinceService stateProvinceService,
     IMailchimpService mailchimpService,
     IHttpContextAccessor httpContextAccessor,
     IZohoService zohoService, ICustomerExtendedService customerExtendedService, ICheckoutExtendedModelFactory checkoutExtendedModelFactory,
     IAbandonedCartService abandonedCartService, IOrderExtendedModelFactory orderModelFactory,
     IPaymentSessionService paymentSessionService, IOrderTotalCalculationExtendedService orderTotalCalculationService, ICurrencyService currencyService,
     IShoppingCartExtendedModelFactory shoppingCartModelFactory, IOrderProcessingExtendedService orderProcessingExtendedService, ISettingService settingService,
     ForumSettings forumSettings) : base(addressSettings, captchaSettings, customerSettings, addressModelFactory, addressService, addressAttributeParser, checkoutModelFactory, countryService, customerService, genericAttributeService, localizationService, logger, orderProcessingService, orderService, paymentPluginManager, paymentService, productService, shippingService, shoppingCartService, storeContext, taxService, webHelper, workContext, orderSettings, paymentSettings, rewardPointsSettings, shippingSettings, taxSettings)
        {
            _commonSettings = commonSettings;
            _gdprService = gdprService;
            _customerRegistrationService = customerRegistrationService;
            _localizationSettings = localizationSettings;
            _customerAttributeParser = customerAttributeParser;
            _customerAttributeService = customerAttributeService;
            _authenticationService = authenticationService;
            _dateTimeSettings = dateTimeSettings;
            _customWorkflowMessageService = customWorkflowMessageService;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _gdprSettings = gdprSettings;
            _customerModelFactory = customerModelFactory;
            _stateProvinceService = stateProvinceService;
            _mailchimpService = mailchimpService;
            _httpContextAccessor = httpContextAccessor;
            _zohoService = zohoService;
            _customerExtendedService = customerExtendedService;
            _checkoutExtendedModelFactory = checkoutExtendedModelFactory;
            _abandonedCartService = abandonedCartService;
            _orderModelFactory = orderModelFactory;
            _paymentSessionService = paymentSessionService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _currencyService = currencyService;
            _shoppingCartModelFactory = shoppingCartModelFactory;
            _orderProcessingExtendedService = orderProcessingExtendedService;
            _forumSettings = forumSettings;
        }

        public virtual async Task<IActionResult> CustomOnePageCheckout()
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                return RedirectToRoute("ShoppingCart");

            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

            if (!cart.Any())
                return RedirectToRoute("ShoppingCart");

            var checkoutAttributesXml = await _genericAttributeService.GetAttributeAsync<string>(customer,
               NopCustomerDefaults.CheckoutAttributes, store.Id);
            var scWarnings = await _shoppingCartService.GetShoppingCartWarningsAsync(cart, checkoutAttributesXml, true);
            if (scWarnings.Any())
                return RedirectToRoute("ShoppingCart");
            if (!_orderSettings.OnePageCheckoutEnabled)
                return RedirectToRoute("Checkout");

            var _isMemberShipAddedInCart = await _customerExtendedService.IsMemberShipAddedInCart(customer);
            if ((await _customerService.IsGuestAsync(customer) && !_orderSettings.AnonymousCheckoutAllowed) ||
                (await _customerService.IsGuestAsync(customer) && _isMemberShipAddedInCart))
                return Challenge();



            var model = await _checkoutExtendedModelFactory.PrepareCustomOnePageCheckoutModelAsync(cart);

            var shippingOption = await _genericAttributeService.GetAttributeAsync<ShippingOption>(await _workContext.GetCurrentCustomerAsync(),
                           NopCustomerDefaults.SelectedShippingOptionAttribute, (await _storeContext.GetCurrentStoreAsync()).Id);
            model.StepActive = "#opc-customerinfo";
            if (shippingOption != null)
            {
                model.StepActive = "#opc-payment_method";
                var shippingMethodModel = await _checkoutExtendedModelFactory.PrepareCustomShippingMethodModelAsync(cart, await _customerService.GetCustomerShippingAddressAsync(await _workContext.GetCurrentCustomerAsync()));
                if (_shippingSettings.BypassShippingMethodSelectionIfOnlyOne)
                    model.CheckoutShippingMethodModel = null;
                else
                    model.CheckoutShippingMethodModel = shippingMethodModel;

                var filterByCountryId = 0;
                if (_addressSettings.CountryEnabled)
                {
                    filterByCountryId = (await _customerService.GetCustomerBillingAddressAsync(await _workContext.GetCurrentCustomerAsync()))?.CountryId ?? 0;
                }

                var paymentMethodModel = await _checkoutModelFactory.PreparePaymentMethodModelAsync(cart, filterByCountryId);
                paymentMethodModel.TermsOfServiceOnOrderConfirmPage = _orderSettings.TermsOfServiceOnOrderConfirmPage;
                paymentMethodModel.TermsOfServicePopup = _commonSettings.PopupForTermsOfServiceLinks;
                //customer have to choose a payment method
                model.CheckoutPaymentMethodModel = paymentMethodModel;
            }
            else if ((await _customerService.GetCustomerShippingAddressAsync(await _workContext.GetCurrentCustomerAsync()))?.Id != null)
            {
                model.StepActive = "#opc-shipping_method";
                var shippingMethodModel = await _checkoutExtendedModelFactory.PrepareCustomShippingMethodModelAsync(cart, await _customerService.GetCustomerShippingAddressAsync(await _workContext.GetCurrentCustomerAsync()));
                if (_shippingSettings.BypassShippingMethodSelectionIfOnlyOne)
                    model.CheckoutShippingMethodModel = null;
                else
                    model.CheckoutShippingMethodModel = shippingMethodModel;
            }
            ViewData["Step"] = model.StepActive;
            return View("onePageCheckout", model);
        }


   
        [HttpPost]
        public virtual async Task<IActionResult> OpcSaveCustomerInfo(CheckoutAddressModel model, IFormCollection form, bool saveData = true, bool moveToNextStep = true)
        {

            if (_orderSettings.CheckoutDisabled)
                throw new Exception(await _localizationService.GetResourceAsync("Checkout.Disabled"));

            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

            if (!cart.Any())
                throw new Exception("Your cart is empty");

            if (!_orderSettings.OnePageCheckoutEnabled)
                throw new Exception("One page checkout is disabled");

            if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !_orderSettings.AnonymousCheckoutAllowed)
                throw new Exception("Anonymous checkout is not allowed");
            var store = await _storeContext.GetCurrentStoreAsync();
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (saveData)
            {
                model.FirstName = CustomCommonHelper.GetCustomerFirstName(model.FullName);
                model.LastName = CustomCommonHelper.GetCustomerLastName(model.FullName);

                var oldCustomerModel = new CustomerInfoModel();


                //get customer info model before changes for gdpr log
                if (_gdprSettings.GdprEnabled & _gdprSettings.LogUserProfileChanges)
                    oldCustomerModel = await _customerModelFactory.PrepareCustomerInfoModelAsync(oldCustomerModel, customer, false);

                //custom customer attributes
                var customerAttributesXml = await ParseCustomCustomerAttributesAsync(form);
                var customerAttributeWarnings = await _customerAttributeParser.GetAttributeWarningsAsync(customerAttributesXml);
                foreach (var error in customerAttributeWarnings)
                {
                    ModelState.AddModelError("", error);
                }

                //GDPR
                if (_gdprSettings.GdprEnabled)
                {
                    var consents = (await _gdprService
                        .GetAllConsentsAsync()).Where(consent => consent.DisplayOnCustomerInfoPage && consent.IsRequired).ToList();

                    ValidateRequiredConsents(consents, form);
                }
                var isRegistered = await _customerService.IsRegisteredAsync(customer);
                try
                {
                    if (ModelState.IsValid)
                    {
                        //username 
                        if (_customerSettings.UsernamesEnabled && _customerSettings.AllowUsersToChangeUsernames && isRegistered)
                        {
                            var userName = model.Username.Trim();
                            if (string.IsNullOrEmpty(customer.Username) || !customer.Username.Equals(userName, StringComparison.InvariantCultureIgnoreCase))
                            {
                                //change username
                                await _customerRegistrationService.SetUsernameAsync(customer, userName);

                                //re-authenticate
                                //do not authenticate users in impersonation mode
                                if (_workContext.OriginalCustomerIfImpersonated == null)
                                    await _authenticationService.SignInAsync(customer, true);
                            }
                        }
                        //email
                        var email = model.Email.Trim();
                        if ((string.IsNullOrEmpty(customer.Email) || !customer.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase)) && isRegistered)
                        {
                            //change email
                            var requireValidation = _customerSettings.UserRegistrationType == UserRegistrationType.EmailValidation;
                            await _customerRegistrationService.SetEmailAsync(customer, email, requireValidation);

                            //do not authenticate users in impersonation mode
                            if (_workContext.OriginalCustomerIfImpersonated == null)
                            {
                                //re-authenticate (if usernames are disabled)
                                if (!_customerSettings.UsernamesEnabled && !requireValidation)
                                    await _authenticationService.SignInAsync(customer, true);
                            }
                        }

                        if (!isRegistered)
                        {
                            await _genericAttributeService.SaveAttributeAsync(customer, "Email", model.Email);
                        }
                        //properties

                        if (isRegistered)
                        {
                            if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                            {
                                customer.TimeZoneId = model.TimeZoneId;
                            }
                            //VAT number
                            if (_taxSettings.EuVatEnabled)
                            {
                                var prevVatNumber = customer.VatNumber;
                                customer.VatNumber = model.VatNumber;

                                if (prevVatNumber != model.VatNumber)
                                {
                                    var (vatNumberStatus, _, vatAddress) = await _taxService.GetVatNumberStatusAsync(model.VatNumber);
                                    customer.VatNumberStatus = vatNumberStatus;
                                    //send VAT number admin notification
                                    if (!string.IsNullOrEmpty(model.VatNumber) && _taxSettings.EuVatEmailAdminWhenNewVatSubmitted)
                                        await _customWorkflowMessageService.SendNewVatSubmittedStoreOwnerNotificationAsync(customer,
                                            model.VatNumber, vatAddress, _localizationSettings.DefaultAdminLanguageId);
                                }
                            }

                            //form fields
                            if (_customerSettings.GenderEnabled)
                                customer.Gender = model.Gender;
                            if (_customerSettings.FirstNameEnabled)
                                customer.FirstName = model.FirstName;
                            if (_customerSettings.LastNameEnabled)
                                customer.LastName = model.LastName;
                            if (_customerSettings.DateOfBirthEnabled)
                            {
                                var dateOfBirth = model.ParseDateOfBirth();
                                customer.DateOfBirth = dateOfBirth;
                            }
                            if (_customerSettings.CompanyEnabled)
                                customer.Company = model.Company;
                            if (_customerSettings.StreetAddressEnabled)
                                customer.StreetAddress = model.Address1;
                            if (_customerSettings.StreetAddress2Enabled)
                                customer.StreetAddress2 = model.Address2;
                            if (_customerSettings.ZipPostalCodeEnabled)
                                customer.ZipPostalCode = model.ZipPostalCode;
                            if (_customerSettings.CityEnabled)
                                customer.City = model.City;
                            if (_customerSettings.CountyEnabled)
                                customer.County = model.County;
                            if (_customerSettings.CountryEnabled)
                                customer.CountryId = model.CountryId ?? 0;
                            if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                                customer.StateProvinceId = model.StateProvinceId ?? 0;
                            if (_customerSettings.PhoneEnabled)
                                customer.Phone = model.PhoneNumber;
                            if (_customerSettings.FaxEnabled)
                                customer.Fax = model.Fax;

                            customer.CustomCustomerAttributesXML = customerAttributesXml;
                            await this._customerService.UpdateCustomerAsync(customer);
                        }

                        string customerEmail = customer.Email;
                        if (!isRegistered)
                            customerEmail = model.Email;

                        //newsletter
                        if (_customerSettings.NewsletterEnabled)
                        {

                            //save newsletter value
                            var newsletter = (await _newsLetterSubscriptionService.GetNewsLetterSubscriptionsByEmailAsync(customerEmail, store.Id)).FirstOrDefault();
                            if (newsletter != null)
                            {
                                if (model.Newsletter)
                                {
                                    newsletter.Active = true;
                                    await _newsLetterSubscriptionService.UpdateNewsLetterSubscriptionAsync(newsletter);
                                }
                                else
                                {
                                    await _newsLetterSubscriptionService.DeleteNewsLetterSubscriptionAsync(newsletter);
                                }
                            }
                            else
                            {
                                if (model.Newsletter)
                                {

                                    await _newsLetterSubscriptionService.InsertNewsLetterSubscriptionAsync(new NewsLetterSubscription
                                    {
                                        NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                        Email = customerEmail,
                                        Active = true,
                                        StoreId = (await _storeContext.GetCurrentStoreAsync()).Id,
                                        CreatedOnUtc = DateTime.UtcNow
                                    });
                                }
                            }
                        }

                        if (_forumSettings.ForumsEnabled && _forumSettings.SignaturesEnabled)
                            await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.SignatureAttribute, model.Signature);

                        //save customer attributes

                        //GDPR
                        if (_gdprSettings.GdprEnabled)
                        {

                            await LogGdprAsync(customer, oldCustomerModel, model, form, _gdprService, _gdprSettings, _stateProvinceService);
                        }


                        #region Mail Chimp

                        string name = "";


                        if (!string.IsNullOrEmpty(customerEmail))
                        {
                            string firstName = model.FirstName;
                            string lastName = model.LastName;
                            name = (firstName ?? "" + " " + lastName ?? "").Trim();
                            string url = _httpContextAccessor.HttpContext?.Request.Headers["Referer"];
                            string absoluteUrl = _httpContextAccessor.HttpContext?.Request.Headers["Referer"];
                            string userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"];


                            await _mailchimpService.CustomerSignup(email, name ?? "", new System.Collections.Generic.List<string>()
                {
                    "WebRegistered"
                }, url, userAgent);
                        }

                        #endregion


                        #region Zoho

                        await _zohoService.InsertQueuedZohoCustomerAsync(customer.Id);

                        #endregion



                        //if (await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
                        //{
                        //    //do not ship to the same address
                        //    var shippingAddressModel = await _checkoutModelFactory.PrepareCustomShippingAddressModelAsync(customer,null, cart, prePopulateNewAddressWithCustomerFields: true);
                        //    if (shippingAddressModel != null)
                        //    {
                        //        var phoneNumber = await _genericAttributeService.GetAttributeAsync<string>(customer, "Phone");
                        //        shippingAddressModel.PhoneNumber = phoneNumber ?? shippingAddressModel.PhoneNumber;
                        //    }
                        //    return Json(new
                        //    {
                        //        update_section = await BindAddressProps("shipping", "customerinfo", await RenderPartialViewToStringAsync("OpcShippingAddress", shippingAddressModel), true),
                        //        goto_section = "shipping"
                        //    });
                        //}

                        #region Save Shipping Address

                        return await OpcCustomSaveShipping(model, form, moveToNextStep);

                        #endregion


                    }
                    else
                    {
                        var shippingAddressModel = await _checkoutExtendedModelFactory.PrepareCustomShippingAddressModelAsync(customer, model, cart, prePopulateNewAddressWithCustomerFields: true, isEdit: true);
                        if (shippingAddressModel != null)
                        {
                            if (string.IsNullOrEmpty(model.PhoneNumber))
                                shippingAddressModel.PhoneNumber = customer.Phone;
                        }

                        return Json(new
                        {
                            update_section = await BindAddressProps("customerinfo", "", await RenderPartialViewToStringAsync("OpcCustomerInfo", shippingAddressModel), false)
                        });
                    }


                }
                catch (Exception exc)
                {
                    await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                    return Json(new { error = 1, message = exc.Message });
                }
            }
            else
            {
                if (moveToNextStep)
                {
                    return await OpcCustomLoadStepAfterShippingAddress(cart);
                }
                else
                {
                    var shippingAddressModel = await _checkoutExtendedModelFactory.PrepareCustomShippingAddressModelAsync(await _workContext.GetCurrentCustomerAsync(), model, cart, prePopulateNewAddressWithCustomerFields: true);
                    if (shippingAddressModel != null)
                    {
                        if (string.IsNullOrEmpty(model.PhoneNumber))
                            shippingAddressModel.PhoneNumber = customer.Phone;
                    }

                    return Json(new
                    {
                        update_section = await BindAddressProps("customerinfo", "", await RenderPartialViewToStringAsync("OpcCustomerInfo", shippingAddressModel), false)
                    });
                }
            }
        }

        #region Methods (one page checkout)

       
        public virtual async Task<IActionResult> OpcCustomLoadBillingAddres()
        {
            var billingAddressModel = await _checkoutExtendedModelFactory.PrepareCustomBillingAddressModelAsync(null, null, prePopulateNewAddressWithCustomerFields: true);
            if (billingAddressModel != null)
            {

                if (string.IsNullOrEmpty(billingAddressModel.PhoneNumber))
                {
                    var customer = await _workContext.GetCurrentCustomerAsync();
                    billingAddressModel.PhoneNumber = customer.Phone;

                }
            }
            return Json(new
            {
                update_section = await BindAddressProps("billing", "payment-method", await RenderPartialViewToStringAsync("OpcBillingAddress", billingAddressModel), true),
                goto_section = "payment_method"
            });
        }


        /// <returns>A task that represents the asynchronous operation</returns>
        /// 

        [HttpPost]
 
        public virtual async Task<IActionResult> OpcCustomSaveBilling(CheckoutAddressModel model, IFormCollection form)
        {
            try
            {
                //validation
                if (_orderSettings.CheckoutDisabled)
                    throw new Exception(await _localizationService.GetResourceAsync("Checkout.Disabled"));

                var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

                if (!cart.Any())
                    throw new Exception("Your cart is empty");

                if (!_orderSettings.OnePageCheckoutEnabled)
                    throw new Exception("One page checkout is disabled");

                if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !_orderSettings.AnonymousCheckoutAllowed)
                    throw new Exception("Anonymous checkout is not allowed");

                var customer = await _workContext.GetCurrentCustomerAsync();
                if (await _customerService.IsGuestAsync(customer) && _taxSettings.EuVatEnabled && _taxSettings.EuVatEnabledForGuests)
                {
                    var warning = await SaveCustomerVatNumberAsync(model.VatNumber, customer);
                    if (!string.IsNullOrEmpty(warning))
                        ModelState.AddModelError("", warning);
                }

             
                int.TryParse(form["billing_address_id"], out var billingAddressId);
                int shippingAddressId = (await _customerService.GetCustomerShippingAddressAsync(customer)).Id;

                model.FirstName = CustomCommonHelper.GetCustomerFirstName(model.FullName);
                model.LastName = CustomCommonHelper.GetCustomerLastName(model.FullName);
                if (billingAddressId > 0 && (billingAddressId != shippingAddressId))
                {
                    //existing address

                    var address = model.ToEntity();
                    var customAttributes = await _addressAttributeParser.ParseCustomAttributesAsync(form, NopCommonDefaults.AddressAttributeControlName);
                    var customAttributeWarnings = await _addressAttributeParser.GetAttributeWarningsAsync(customAttributes);
                    foreach (var error in customAttributeWarnings)
                    {
                        ModelState.AddModelError("", error);
                    }

                    //validate model
                    if (!ModelState.IsValid)
                    {
                        //model is not valid. redisplay the form with errors
                        var billingAddressModel = await _checkoutExtendedModelFactory.PrepareCustomBillingAddressModelAsync(model, cart,
                            selectedCountryId: address.CountryId,
                            overrideAttributesXml: customAttributes,
                            isEdit: true);
                        if (billingAddressModel != null)
                        {
                            if (string.IsNullOrEmpty(billingAddressModel.PhoneNumber))
                                billingAddressModel.PhoneNumber = customer.Phone;
                        }
                        return Json(new
                        {
                            update_section = await BindAddressProps("billing", "payment-method", await RenderPartialViewToStringAsync("OpcBillingAddress", billingAddressModel), true)
                        });
                    }


                    address.CustomAttributes = customAttributes;
                    await _addressService.UpdateAddressAsync(address);

                    return await OpcCustomLoadStepAfterShippingMethod(cart, true);
                }
                else
                {
                    //new address
                    var newAddress = model;

                    //custom address attributes
                    var customAttributes = await _addressAttributeParser.ParseCustomAttributesAsync(form, NopCommonDefaults.AddressAttributeControlName);
                    var customAttributeWarnings = await _addressAttributeParser.GetAttributeWarningsAsync(customAttributes);
                    foreach (var error in customAttributeWarnings)
                    {
                        ModelState.AddModelError("", error);
                    }

                    //validate model
                    if (!ModelState.IsValid)
                    {
                        //model is not valid. redisplay the form with errors
                        var billingAddressModel = await _checkoutExtendedModelFactory.PrepareCustomBillingAddressModelAsync(model, cart,
                            selectedCountryId: newAddress.CountryId,
                            overrideAttributesXml: customAttributes,
                            isEdit: true);
                        if (billingAddressModel != null)
                        {
                            if (string.IsNullOrEmpty(billingAddressModel.PhoneNumber))
                                billingAddressModel.PhoneNumber = customer.Phone;
                        }
                        return Json(new
                        {
                            update_section = await BindAddressProps("billing", "payment-method", await RenderPartialViewToStringAsync("OpcBillingAddress", billingAddressModel), false)
                        });
                    }

                    //try to find an address with the same values (don't duplicate records)
                    var address = _addressService.FindAddress((await _customerService.GetAddressesByCustomerIdAsync((await _workContext.GetCurrentCustomerAsync()).Id)).ToList(),
                        newAddress.FirstName, newAddress.LastName, newAddress.PhoneNumber,
                        newAddress.Email, newAddress.FaxNumber, newAddress.Company,
                        newAddress.Address1, newAddress.Address2, newAddress.City,
                        newAddress.County, newAddress.StateProvinceId, newAddress.ZipPostalCode,
                        newAddress.CountryId, customAttributes);

                    if (address == null)
                    {
                        address = newAddress.ToEntity();
                        address.CustomAttributes = customAttributes;
                        address.CreatedOnUtc = DateTime.UtcNow;

                        await _addressService.InsertAddressAsync(address);

                        await _customerService.InsertCustomerAddressAsync(await _workContext.GetCurrentCustomerAsync(), address);
                    }

                    (await _workContext.GetCurrentCustomerAsync()).BillingAddressId = address.Id;
                    await _customerService.UpdateCustomerAsync(await _workContext.GetCurrentCustomerAsync());
                }

                return await OpcCustomLoadStepAfterShippingMethod(cart, true);
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return Json(new { error = 1, message = exc.Message });
            }
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        /// 
        [HttpPost]
     
        protected virtual async Task<JsonResult> OpcCustomSaveShipping(CheckoutAddressModel customaddressModel, IFormCollection form, bool moveToNextStep)
        {
            var model = customaddressModel.CheckoutAddressModelToAddressModel();
            try
            {
                //validation
                if (_orderSettings.CheckoutDisabled)
                    throw new Exception(await _localizationService.GetResourceAsync("Checkout.Disabled"));

                var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

                if (!cart.Any())
                    throw new Exception("Your cart is empty");

                if (!_orderSettings.OnePageCheckoutEnabled)
                    throw new Exception("One page checkout is disabled");

                if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !_orderSettings.AnonymousCheckoutAllowed)
                    throw new Exception("Anonymous checkout is not allowed");

                if (!await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
                    throw new Exception("Shipping is not required");

                var store = await _storeContext.GetCurrentStoreAsync();
                var customer = await _workContext.GetCurrentCustomerAsync();
                //pickup point
                if (_shippingSettings.AllowPickupInStore && !_orderSettings.DisplayPickupInStoreOnShippingMethodPage)
                {
                    var pickupInStore = ParsePickupInStore(form);
                    if (pickupInStore)
                    {
                        var pickupOption = await ParsePickupOptionAsync(cart, form);
                        await SavePickupOptionAsync(pickupOption);

                        return await OpcCustomLoadStepAfterShippingMethod(cart);
                    }

                    //set value indicating that "pick up in store" option has not been chosen
                    await _genericAttributeService.SaveAttributeAsync<PickupPoint>(customer, NopCustomerDefaults.SelectedPickupPointAttribute, null, store.Id);
                }

                int.TryParse(form["shipping_address_id"], out var shippingAddressId);

                if (shippingAddressId > 0)
                {
                    //existing address
                    var address = model.ToEntity();
                    var customAttributes = await _addressAttributeParser.ParseCustomAttributesAsync(form, NopCommonDefaults.AddressAttributeControlName);
                    var customAttributeWarnings = await _addressAttributeParser.GetAttributeWarningsAsync(customAttributes);
                    foreach (var error in customAttributeWarnings)
                    {
                        ModelState.AddModelError("", error);
                    }

                    //validate model
                    if (!ModelState.IsValid)
                    {
                        //model is not valid. redisplay the form with errors
                        var shippingAddressModel = await _checkoutExtendedModelFactory.PrepareCustomShippingAddressModelAsync(customer, customaddressModel, cart,
                            selectedCountryId: address.CountryId,
                            overrideAttributesXml: customAttributes,
                            isEdit: true);
                        if (shippingAddressModel != null)
                        {
                            if (string.IsNullOrEmpty(shippingAddressModel.PhoneNumber))
                                shippingAddressModel.PhoneNumber = customer.Phone;
                        }
                        return Json(new
                        {
                            update_section = await BindAddressProps("customerinfo", "", await RenderPartialViewToStringAsync("OpcCustomerInfo", shippingAddressModel), false)
                        });
                    }


                    address.CustomAttributes = customAttributes;
                    await _addressService.UpdateAddressAsync(address);

                    // Save Billing
                    if (customer?.BillingAddressId == null)
                    {
                        customer.BillingAddressId = address.Id;
                        await _customerService.UpdateCustomerAsync(customer);
                    }
                    if (moveToNextStep)
                    {
                        return await OpcCustomLoadStepAfterShippingAddress(cart);
                    }
                    else
                    {
                        var shippingAddressModel = await _checkoutExtendedModelFactory.PrepareCustomShippingAddressModelAsync(customer, customaddressModel, cart,
                          selectedCountryId: address.CountryId,
                          overrideAttributesXml: customAttributes,
                          isEdit: true);
                        if (shippingAddressModel != null)
                        {
                            if (string.IsNullOrEmpty(shippingAddressModel.PhoneNumber))
                                shippingAddressModel.PhoneNumber = customer.Phone;
                        }
                        return Json(new
                        {
                            update_section = await BindAddressProps("customerinfo", "", await RenderPartialViewToStringAsync("OpcCustomerInfo", shippingAddressModel), false)
                        });
                    }
                }
                else
                {
                    //new address
                    var newAddress = model;

                    //custom address attributes
                    var customAttributes = await _addressAttributeParser.ParseCustomAttributesAsync(form, NopCommonDefaults.AddressAttributeControlName);
                    var customAttributeWarnings = await _addressAttributeParser.GetAttributeWarningsAsync(customAttributes);
                    foreach (var error in customAttributeWarnings)
                    {
                        ModelState.AddModelError("", error);
                    }

                    //validate model
                    if (!ModelState.IsValid)
                    {
                        //model is not valid. redisplay the form with errors
                        var shippingAddressModel = await _checkoutExtendedModelFactory.PrepareCustomShippingAddressModelAsync(customer, customaddressModel, cart,
                            selectedCountryId: newAddress.CountryId,
                            overrideAttributesXml: customAttributes,
                            isEdit: true);
                        if (shippingAddressModel != null)
                        {
                            if (string.IsNullOrEmpty(shippingAddressModel.PhoneNumber))
                                shippingAddressModel.PhoneNumber = customer.Phone;
                        }
                        return Json(new
                        {
                            update_section = await BindAddressProps("customerinfo", "", await RenderPartialViewToStringAsync("OpcCustomerInfo", shippingAddressModel), false)
                        });
                    }

                    //try to find an address with the same values (don't duplicate records)
                    var address = _addressService.FindAddress((await _customerService.GetAddressesByCustomerIdAsync(customer.Id)).ToList(),
                        newAddress.FirstName, newAddress.LastName, newAddress.PhoneNumber,
                        newAddress.Email, newAddress.FaxNumber, newAddress.Company,
                        newAddress.Address1, newAddress.Address2, newAddress.City,
                        newAddress.County, newAddress.StateProvinceId, newAddress.ZipPostalCode,
                        newAddress.CountryId, customAttributes);

                    if (address == null)
                    {
                        address = newAddress.ToEntity();
                        address.CustomAttributes = customAttributes;
                        address.CreatedOnUtc = DateTime.UtcNow;

                        await _addressService.InsertAddressAsync(address);

                        await _customerService.InsertCustomerAddressAsync(customer, address);
                    }

                    customer.ShippingAddressId = address.Id;

                    // Save Billing
                    if (customer?.BillingAddressId == null)
                        customer.BillingAddressId = address.Id;

                    //reset selected shipping method (in case if "pick up in store" was selected)
                    await _genericAttributeService.SaveAttributeAsync<ShippingOption>(customer, NopCustomerDefaults.SelectedShippingOptionAttribute, null, (await _storeContext.GetCurrentStoreAsync()).Id);
                    await _genericAttributeService.SaveAttributeAsync<PickupPoint>(customer, NopCustomerDefaults.SelectedPickupPointAttribute, null, (await _storeContext.GetCurrentStoreAsync()).Id);
                    //limitation - "Ship to the same address" doesn't properly work in "pick up in store only" case (when no shipping plugins are available) 

                    await _customerService.UpdateCustomerAsync(customer);
                    if (moveToNextStep)
                    {
                        return await OpcCustomLoadStepAfterShippingAddress(cart);
                    }
                    else
                    {
                        var shippingAddressModel = await _checkoutExtendedModelFactory.PrepareCustomShippingAddressModelAsync(customer, customaddressModel, cart,
                             selectedCountryId: address.CountryId,
                             overrideAttributesXml: customAttributes,
                             isEdit: true);
                        if (shippingAddressModel != null)
                        {
                            if (string.IsNullOrEmpty(shippingAddressModel.PhoneNumber))
                                shippingAddressModel.PhoneNumber = customer.Phone;
                        }
                        return Json(new
                        {
                            update_section = await BindAddressProps("customerinfo", "", await RenderPartialViewToStringAsync("OpcCustomerInfo", shippingAddressModel), false)
                        });
                    }
                }

            }
            catch (Exception exc)
            {
                await _logger.ErrorAsync("Checkout Shipping Address Sabve Issue ", exc, await _workContext.GetCurrentCustomerAsync());
                return Json(new { error = 1, message = exc.Message });
            }
        }
        /// <returns>A task that represents the asynchronous operation</returns>
        /// 
 
        protected virtual async Task<JsonResult> OpcCustomLoadStepAfterShippingAddress(IList<ShoppingCartItem> cart)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var shippingMethodModel = await _checkoutExtendedModelFactory.PrepareCustomShippingMethodModelAsync(cart, await _customerService.GetCustomerShippingAddressAsync(customer));
            if (_shippingSettings.BypassShippingMethodSelectionIfOnlyOne)
            {
                //if we have only one shipping method, then a customer doesn't have to choose a shipping method
                await _genericAttributeService.SaveAttributeAsync(customer,
                    NopCustomerDefaults.SelectedShippingOptionAttribute,
                    shippingMethodModel.ShippingMethods.First().ShippingOption,
                    (await _storeContext.GetCurrentStoreAsync()).Id);

                //load next step
                return await OpcCustomLoadStepAfterShippingMethod(cart);
            }

            return Json(new
            {
                update_section = await BindAddressProps("shipping-method", "", await RenderPartialViewToStringAsync("OpcShippingMethods", shippingMethodModel), false),
                goto_section = "shipping_method"
            });
        }

        /// <returns>A task that represents the asynchronous operation</returns>




        /// <returns>A task that represents the asynchronous operation</returns>
        /// 
        [HttpPost]
 
        public virtual async Task<IActionResult> OpcCustomSaveShippingMethod(string shippingoption, IFormCollection form, bool moveToNextStep = true)
        {
            try
            {
                //validation
                if (_orderSettings.CheckoutDisabled)
                    throw new Exception(await _localizationService.GetResourceAsync("Checkout.Disabled"));

                var customer = await _workContext.GetCurrentCustomerAsync();
                var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

                if (!cart.Any())
                    throw new Exception("Your cart is empty");

                if (!_orderSettings.OnePageCheckoutEnabled)
                    throw new Exception("One page checkout is disabled");

                if (await _customerService.IsGuestAsync(customer) && !_orderSettings.AnonymousCheckoutAllowed)
                    throw new Exception("Anonymous checkout is not allowed");

                if (!await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
                    throw new Exception("Shipping is not required");

                //pickup point
                if (_shippingSettings.AllowPickupInStore && _orderSettings.DisplayPickupInStoreOnShippingMethodPage)
                {
                    var pickupInStore = ParsePickupInStore(form);
                    if (pickupInStore)
                    {
                        var pickupOption = await ParsePickupOptionAsync(cart, form);
                        await SavePickupOptionAsync(pickupOption);

                        return await OpcCustomLoadStepAfterShippingMethod(cart);
                    }

                    //set value indicating that "pick up in store" option has not been chosen
                    await _genericAttributeService.SaveAttributeAsync<PickupPoint>(customer, NopCustomerDefaults.SelectedPickupPointAttribute, null, (await _storeContext.GetCurrentStoreAsync()).Id);
                }

                //parse selected method 
                if (string.IsNullOrEmpty(shippingoption))
                    throw new Exception("Selected shipping method can't be parsed");
                var splittedOption = shippingoption.Split(new[] { "___" }, StringSplitOptions.RemoveEmptyEntries);
                if (splittedOption.Length != 2)
                    throw new Exception("Selected shipping method can't be parsed");
                var selectedName = splittedOption[0];
                var shippingRateComputationMethodSystemName = splittedOption[1];

                //find it
                //performance optimization. try cache first
                var shippingOptions = await _genericAttributeService.GetAttributeAsync<List<ShippingOption>>(customer,
                    NopCustomerDefaults.OfferedShippingOptionsAttribute, (await _storeContext.GetCurrentStoreAsync()).Id);
                if (shippingOptions == null || !shippingOptions.Any())
                {
                    //not found? let's load them using shipping service
                    shippingOptions = (await _shippingService.GetShippingOptionsAsync(cart, await _customerService.GetCustomerShippingAddressAsync(customer),
                        customer, shippingRateComputationMethodSystemName, (await _storeContext.GetCurrentStoreAsync()).Id)).ShippingOptions.ToList();
                }
                else
                {
                    //loaded cached results. let's filter result by a chosen shipping rate computation method
                    shippingOptions = shippingOptions.Where(so => so.ShippingRateComputationMethodSystemName.Equals(shippingRateComputationMethodSystemName, StringComparison.InvariantCultureIgnoreCase))
                        .ToList();
                }

                var shippingOption = shippingOptions
                    .Find(so => !string.IsNullOrEmpty(so.Name) && so.Name.Equals(selectedName, StringComparison.InvariantCultureIgnoreCase));
                if (shippingOption == null)
                    throw new Exception("Selected shipping method can't be loaded");

                //save
                await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.SelectedShippingOptionAttribute, shippingOption, (await _storeContext.GetCurrentStoreAsync()).Id);

                //load next step
                if (moveToNextStep)
                {
                    return await OpcCustomLoadStepAfterShippingMethod(cart);
                }
                else
                {
                    return Json(new
                    {
                        update_section = await BindAddressProps("shipping-method", string.Empty, string.Empty, false, false, string.Empty, false),
                        goto_section = "shipping_method"
                    });
                }
            }
            catch (Exception exc)
            {
                await _logger.ErrorAsync("Checkout Failed to save Shipping Method", exc, await _workContext.GetCurrentCustomerAsync());
                return Json(new { error = 1, message = exc.Message });
            }
        }


        /// <returns>A task that represents the asynchronous operation</returns>

 
        protected virtual async Task<JsonResult> OpcCustomLoadStepAfterShippingMethod(IList<ShoppingCartItem> cart, bool loadParent = false)
        {
            //Check whether payment workflow is required
            //filter by country
            var customer = await _workContext.GetCurrentCustomerAsync();
            var filterByCountryId = 0;
            if (_addressSettings.CountryEnabled)
            {
                filterByCountryId = (await _customerService.GetCustomerBillingAddressAsync(customer))?.CountryId ?? 0;
            }

            //payment is required
            var paymentMethodModel = await _checkoutModelFactory.PreparePaymentMethodModelAsync(cart, filterByCountryId);
            paymentMethodModel.TermsOfServiceOnOrderConfirmPage = _orderSettings.TermsOfServiceOnOrderConfirmPage;
            paymentMethodModel.TermsOfServicePopup = _commonSettings.PopupForTermsOfServiceLinks;

            try
            {
                var shippingAddressModel = await _checkoutExtendedModelFactory.PrepareCustomShippingAddressModelAsync(customer, null, await _shoppingCartService.GetShoppingCartAsync(customer)
                                     , prePopulateNewAddressWithCustomerFields: true);
                if (shippingAddressModel != null)
                {
                    if (string.IsNullOrEmpty(shippingAddressModel.PhoneNumber))
                        shippingAddressModel.PhoneNumber = customer.Phone;
                }

                ViewData["ShippingAddress"] = shippingAddressModel;
            }
            catch
            {

            }
            //customer have to choose a payment method
            return Json(new
            {
                update_section = await BindAddressProps((loadParent ? "billing" : "payment-method"), (loadParent ? "payment-method" : ""), await RenderPartialViewToStringAsync("OpcPaymentMethods", paymentMethodModel), loadParent, loadParent),
                goto_section = "payment_method"
            });

        }

   
        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> OpcCustomSavePaymentInfo(string paymentMethod, IFormCollection form)
        {
            try
            {
                bool UseRewardPoints = false;
                //validation
                if (_orderSettings.CheckoutDisabled)
                    throw new Exception(await _localizationService.GetResourceAsync("Checkout.Disabled"));

                var customer = await _workContext.GetCurrentCustomerAsync();
                var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

                if (!cart.Any())
                    throw new Exception("Your cart is empty");

                if (!_orderSettings.OnePageCheckoutEnabled)
                    throw new Exception("One page checkout is disabled");

                if (await _customerService.IsGuestAsync(customer) && !_orderSettings.AnonymousCheckoutAllowed)
                    throw new Exception("Anonymous checkout is not allowed");
                if (_rewardPointsSettings.Enabled)
                    await _genericAttributeService.SaveAttributeAsync(customer,
        NopCustomerDefaults.UseRewardPointsDuringCheckoutAttribute, UseRewardPoints,
        (await _storeContext.GetCurrentStoreAsync()).Id);


                await _genericAttributeService.SaveAttributeAsync(customer,
                    NopCustomerDefaults.SelectedPaymentMethodAttribute, paymentMethod, (await _storeContext.GetCurrentStoreAsync()).Id);


                var _paymentMethod = await _paymentPluginManager
                    .LoadPluginBySystemNameAsync(paymentMethod, customer, (await _storeContext.GetCurrentStoreAsync()).Id)
                    ?? throw new Exception("Payment method is not selected");

                var warnings = await _paymentMethod.ValidatePaymentFormAsync(form);
                foreach (var warning in warnings)
                    ModelState.AddModelError("", warning);
                var filterByCountryId = 0;
                if (_addressSettings.CountryEnabled)
                {
                    filterByCountryId = (await _customerService.GetCustomerBillingAddressAsync(await _workContext.GetCurrentCustomerAsync()))?.CountryId ?? 0;
                }
                ProcessPaymentRequest paymentInfo = new ProcessPaymentRequest();
                if (ModelState.IsValid)
                {
                    //get payment info
                    paymentInfo = await _paymentMethod.GetPaymentInfoAsync(form);
                    await _orderProcessingService.SetProcessPaymentRequestAsync(paymentInfo);


                    return await OpcCustomConfirmOrder(filterByCountryId, _paymentMethod);

                    //var confirmOrderModel = await _checkoutModelFactory.PrepareConfirmOrderModelAsync(cart);
                    //return Json(new
                    //{
                    //    update_section =
                    //    await BindAddressProps("confirm-order", "payment_method", await RenderPartialViewToStringAsync("OpcConfirmOrder", confirmOrderModel), true),
                    //    goto_section = "confirm_order"
                    //});
                }

                //If we got this far, something failed, redisplay form


                //payment is required

                var paymentMethodModel = await _checkoutModelFactory.PreparePaymentMethodModelAsync(cart, filterByCountryId);
                paymentMethodModel.CheckoutPaymentInfoModel = await _checkoutModelFactory.PreparePaymentInfoModelAsync(_paymentMethod);
                paymentMethodModel.TermsOfServiceOnOrderConfirmPage = _orderSettings.TermsOfServiceOnOrderConfirmPage;
                paymentMethodModel.TermsOfServicePopup = _commonSettings.PopupForTermsOfServiceLinks;
                try
                {
                    var shippingAddressModel = await _checkoutExtendedModelFactory.PrepareCustomShippingAddressModelAsync(customer, null, await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync())
                                         , prePopulateNewAddressWithCustomerFields: true);
                    if (shippingAddressModel != null)
                    {
                        if (string.IsNullOrEmpty(shippingAddressModel.PhoneNumber))
                            shippingAddressModel.PhoneNumber = customer.Phone;
                    }

                    ViewData["ShippingAddress"] = shippingAddressModel;
                }
                catch
                {

                }

                #region send Order Decline message
                try
                {


                    await _customWorkflowMessageService.SendOrderDeclineMessage(paymentInfo, form, customer, (await _workContext.GetWorkingCurrencyAsync()).Id, $"Payment Method: {paymentMethod ?? ""} Error: " + (warnings == null ? "" : string.Join(",", warnings)), 0);
                }
                catch (Exception exp)
                {
                    await _logger.InsertLogAsync(LogLevel.Error,
                        "Failed to send order decline Email", exp.Message, await _workContext.GetCurrentCustomerAsync());
                }

                #endregion

                #region Customer Abandoned Card 


                await _abandonedCartService.AbandonedCardForCustomerOnPaymentFail(customer);

                #endregion

                return Json(new
                {
                    update_section =
                    await BindAddressProps("payment-method", "", await RenderPartialViewToStringAsync("OpcPaymentMethods", paymentMethodModel), false)
                });
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return Json(new { error = 1, message = exc.Message });
            }
        }

    
        protected virtual async Task<JsonResult> OpcCustomLoadStepAfterPaymentMethod(IPaymentMethod paymentMethod, IList<ShoppingCartItem> cart)
        {
            if (paymentMethod.SkipPaymentInfo ||
                (paymentMethod.PaymentMethodType == PaymentMethodType.Redirection && _paymentSettings.SkipPaymentInfoStepForRedirectionPaymentMethods))
            {
                //skip payment info page
                var paymentInfo = new ProcessPaymentRequest();

                //session save
                await _orderProcessingService.SetProcessPaymentRequestAsync(paymentInfo);

                var confirmOrderModel = await _checkoutModelFactory.PrepareConfirmOrderModelAsync(cart);


                return Json(new
                {
                    update_section =
                    await BindAddressProps("confirm-order", "payment_method", await RenderPartialViewToStringAsync("OpcConfirmOrder", confirmOrderModel), true),
                    goto_section = "confirm_order"
                });
            }

            //return payment info page
            var paymenInfoModel = await _checkoutModelFactory.PreparePaymentInfoModelAsync(paymentMethod);
            return Json(new
            {
                update_section =
                await BindAddressProps("payment-info", "", await RenderPartialViewToStringAsync("OpcPaymentInfo", paymenInfoModel), false),
                goto_section = "payment_info"
            });
        }
  
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> OpcCustomConfirmOrder(int filterByCountryId, IPaymentMethod paymentMethod)
        {
            try
            {
                var store = await _storeContext.GetCurrentStoreAsync();
                var customer = await _workContext.GetCurrentCustomerAsync();
                var shippingOption = await _genericAttributeService.GetAttributeAsync<ShippingOption>(customer,
                      NopCustomerDefaults.SelectedShippingOptionAttribute, store.Id);
                //validation


                var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

                //prevent 2 orders being placed within an X seconds time frame
                if (!await IsMinimumOrderPlacementIntervalValidAsync(customer))
                    throw new Exception(await _localizationService.GetResourceAsync("Checkout.MinOrderPlacementInterval"));

                //place order
                var processPaymentRequest = await _orderProcessingService.GetProcessPaymentRequestAsync();
                if (processPaymentRequest == null)
                {
                    //Check whether payment workflow is required
                    if (await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart))
                    {
                        throw new Exception("Payment information is not entered");
                    }

                    processPaymentRequest = new ProcessPaymentRequest();
                }
                processPaymentRequest.StoreId = (await _storeContext.GetCurrentStoreAsync()).Id;
                processPaymentRequest.CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id;
                processPaymentRequest.PaymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(),
                    NopCustomerDefaults.SelectedPaymentMethodAttribute, (await _storeContext.GetCurrentStoreAsync()).Id);
                await _orderProcessingService.SetProcessPaymentRequestAsync(processPaymentRequest);
                var placeOrderResult = await _orderProcessingService.PlaceOrderAsync(processPaymentRequest);
                if (placeOrderResult.Success)
                {
                    await _orderProcessingService.SetProcessPaymentRequestAsync(null);
                    var postProcessPaymentRequest = new PostProcessPaymentRequest
                    {
                        Order = placeOrderResult.PlacedOrder
                    };

                    if (paymentMethod == null)
                        //payment method could be null if order total is 0
                        //success
                        return Json(new { success = 1 });

                    if (paymentMethod.PaymentMethodType == PaymentMethodType.Redirection)
                    {
                        //Redirection will not work because it's AJAX request.
                        //That's why we don't process it here (we redirect a user to another page where he'll be redirected)

                        //redirect
                        return Json(new
                        {
                            redirect = $"{_webHelper.GetStoreLocation()}checkout/OpcCompleteRedirectionPayment"
                        });
                    }

                    await UpdateOrderDiscountSendMail(cart, placeOrderResult.PlacedOrder.Id, shippingOption);

                    await _paymentService.PostProcessPaymentAsync(postProcessPaymentRequest);
                    //success
                    return Json(new { success = 1 });
                }

                #region send Order Decline message
                try
                {

                    await _customWorkflowMessageService.SendOrderDeclineMessage(processPaymentRequest, null, customer, (await _workContext.GetWorkingCurrencyAsync()).Id, $"Payment Method: {processPaymentRequest?.PaymentMethodSystemName ?? string.Empty} Error: " + (placeOrderResult == null ? "" : string.Join(",", placeOrderResult.Errors)), 0);
                }
                catch (Exception exp)
                {
                    await _logger.InsertLogAsync(LogLevel.Error,
                        "Failed to send order decline Email", exp.Message, customer);
                }

                #endregion

                #region Customer Abandoned Card 

                await _abandonedCartService.AbandonedCardForCustomerOnPaymentFail(customer);

                #endregion
                //error
                var paymentMethodModel = await _checkoutModelFactory.PreparePaymentMethodModelAsync(cart, filterByCountryId);
                paymentMethodModel.CheckoutPaymentInfoModel = await _checkoutModelFactory.PreparePaymentInfoModelAsync(paymentMethod);
                paymentMethodModel.TermsOfServiceOnOrderConfirmPage = _orderSettings.TermsOfServiceOnOrderConfirmPage;
                paymentMethodModel.TermsOfServicePopup = _commonSettings.PopupForTermsOfServiceLinks;
                try
                {
                    var shippingAddressModel = await _checkoutExtendedModelFactory.PrepareCustomShippingAddressModelAsync(await _workContext.GetCurrentCustomerAsync(), null, await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync())
                                         , prePopulateNewAddressWithCustomerFields: true);
                    if (shippingAddressModel != null)
                    {
                        if (string.IsNullOrEmpty(shippingAddressModel.PhoneNumber))
                            shippingAddressModel.PhoneNumber = customer.Phone;
                    }

                    ViewData["ShippingAddress"] = shippingAddressModel;
                }
                catch
                {

                }
                string errors = "";
                foreach (var error in placeOrderResult.Errors)
                    errors = errors + " " + error;
                return Json(new
                {
                    update_section =
                    await BindAddressProps("payment-method", "", await RenderPartialViewToStringAsync("OpcPaymentMethods", paymentMethodModel), false, false, errors)
                });
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return Json(new { error = 1, message = exc.Message });
            }
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> OpcCustomCompleteRedirectionPayment()
        {
            try
            {
                //validation
                if (!_orderSettings.OnePageCheckoutEnabled)
                    return RedirectToRoute("Homepage");
                var customer = await _workContext.GetCurrentCustomerAsync();

                var _isMemberShipAddedInCart = await _customerExtendedService.IsMemberShipAddedInCart(customer);
                if ((await _customerService.IsGuestAsync(customer) && !_orderSettings.AnonymousCheckoutAllowed) ||
                    (await _customerService.IsGuestAsync(customer) && _isMemberShipAddedInCart))
                    return Challenge();

                //get the order
                var order = (await _orderService.SearchOrdersAsync(storeId: (await _storeContext.GetCurrentStoreAsync()).Id,
                customerId: customer.Id, pageSize: 1)).FirstOrDefault();
                if (order == null)
                    return RedirectToRoute("Homepage");

                var paymentMethod = await _paymentPluginManager
                    .LoadPluginBySystemNameAsync(order.PaymentMethodSystemName, await _workContext.GetCurrentCustomerAsync(), (await _storeContext.GetCurrentStoreAsync()).Id);
                if (paymentMethod == null)
                    return RedirectToRoute("Homepage");
                if (paymentMethod.PaymentMethodType != PaymentMethodType.Redirection)
                    return RedirectToRoute("Homepage");

                //ensure that order has been just placed
                if ((DateTime.UtcNow - order.CreatedOnUtc).TotalMinutes > 3)
                    return RedirectToRoute("Homepage");

                //Redirection will not work on one page checkout page because it's AJAX request.
                //That's why we process it here
                var postProcessPaymentRequest = new PostProcessPaymentRequest
                {
                    Order = order
                };

                await _paymentService.PostProcessPaymentAsync(postProcessPaymentRequest);

                if (_webHelper.IsRequestBeingRedirected || _webHelper.IsPostBeingDone)
                {
                    //redirection or POST has been done in PostProcessPayment
                    return Content(await _localizationService.GetResourceAsync("Checkout.RedirectMessage"));
                }

                //if no redirection has been done (to a third-party payment page)
                //theoretically it's not possible
                return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return Content(exc.Message);
            }
        }

        public virtual async Task<IActionResult> CheckoutOrderSummary(bool isConfirmationPage = false)
        {
            ViewData["isConfirmationPage"] = isConfirmationPage;
            return View();
        }

        public virtual async Task<IActionResult> GetTaxByZipCode(string zipCode)
        {
            (bool status, ZipCodeTaxRateModel model) = await _checkoutExtendedModelFactory.GetTaxByZipCode(zipCode);
            string message = !status ? string.Format(await _localizationService.GetResourceAsync("Checkout.ShoppingCart.Tax.Inavlid.ZipCode.Message"), zipCode) :
                string.Format(await _localizationService.GetResourceAsync("Checkout.ShoppingCart.Tax.Display.Message"), zipCode, model.Tax);
            model.ZipCode = zipCode;
            return Json(new
            {
                update_section = await BindAddressProps("order summary", string.Empty, status ? await RenderPartialViewToStringAsync("OpcTax", model) : string.Empty, false, false, string.Empty, status, message, model.Tax),
                goto_section = "order_summary"
            });
        }

        #endregion
        #region utilities

        public async Task<CustomUpdateSectionJsonModel> BindAddressProps(string name, string ancestorsection, string html, bool isChildSection, bool loadParent = false, string errors = "", bool isStepCompleted = true, string message = "", string taxAmount = "")
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var model = new CustomUpdateSectionJsonModel()
            {
                name = name,
                html = html,
                ancestorsection = ancestorsection,
                isChildSection = isChildSection,
                shippingAddressId = (await _customerService.GetCustomerShippingAddressAsync(customer))?.Id,
                billingAddressId = (await _customerService.GetCustomerShippingAddressAsync(customer))?.Id,
                loadParent = loadParent,
                errors = errors,
                IsStepCompleted = isStepCompleted,
                TaxAmount = taxAmount,
                Message = message

            };

            if (name == "shipping-method")
            {
                var shippingAddressModel = await _checkoutExtendedModelFactory.PrepareCustomShippingAddressModelAsync(customer, null, await _shoppingCartService.GetShoppingCartAsync(customer)
                                 , prePopulateNewAddressWithCustomerFields: true);
                if (shippingAddressModel != null)
                {
                    if (string.IsNullOrEmpty(shippingAddressModel.PhoneNumber))
                        shippingAddressModel.PhoneNumber = customer.Phone;
                }

                model.ShippingAddress = await RenderPartialViewToStringAsync("OpcShippingAddress", shippingAddressModel);
            }
            return model;
        }


        protected virtual async Task<string> ParseCustomCustomerAttributesAsync(IFormCollection form)
        {
            ArgumentNullException.ThrowIfNull(form);

            var attributesXml = "";
            var attributes = await _customerAttributeService.GetAllAttributesAsync();
            foreach (var attribute in attributes)
            {
                var controlId = $"{NopCustomerServicesDefaults.CustomerAttributePrefix}{attribute.Id}";
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                var selectedAttributeId = int.Parse(ctrlAttributes);
                                if (selectedAttributeId > 0)
                                    attributesXml = _customerAttributeParser.AddAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case AttributeControlType.Checkboxes:
                        {
                            var cblAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(cblAttributes))
                            {
                                foreach (var item in cblAttributes.ToString().Split(_separator, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    var selectedAttributeId = int.Parse(item);
                                    if (selectedAttributeId > 0)
                                        attributesXml = _customerAttributeParser.AddAttribute(attributesXml,
                                            attribute, selectedAttributeId.ToString());
                                }
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //load read-only (already server-side selected) values
                            var attributeValues = await _customerAttributeService.GetAttributeValuesAsync(attribute.Id);
                            foreach (var selectedAttributeId in attributeValues
                                         .Where(v => v.IsPreSelected)
                                         .Select(v => v.Id)
                                         .ToList())
                            {
                                attributesXml = _customerAttributeParser.AddAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                var enteredText = ctrlAttributes.ToString().Trim();
                                attributesXml = _customerAttributeParser.AddAttribute(attributesXml,
                                    attribute, enteredText);
                            }
                        }
                        break;
                    case AttributeControlType.Datepicker:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                    case AttributeControlType.FileUpload:
                    //not supported customer attributes
                    default:
                        break;
                }
            }

            return attributesXml;
        }
        protected virtual void ValidateRequiredConsents(List<GdprConsent> consents, IFormCollection form)
        {
            foreach (var consent in consents)
            {
                var controlId = $"consent{consent.Id}";
                var cbConsent = form[controlId];
                if (StringValues.IsNullOrEmpty(cbConsent) || !cbConsent.ToString().Equals("on"))
                {
                    ModelState.AddModelError("", consent.RequiredMessage);
                }
            }
        }
        protected virtual async Task LogGdprAsync(Customer customer, CustomerInfoModel oldCustomerInfoModel,
          CheckoutAddressModel newCustomerInfoModel, IFormCollection form, IGdprService _gdprService, GdprSettings _gdprSettings, IStateProvinceService _stateProvinceService)
        {
            try
            {
                //consents
                var consents = (await _gdprService.GetAllConsentsAsync()).Where(consent => consent.DisplayOnCustomerInfoPage).ToList();
                foreach (var consent in consents)
                {
                    var previousConsentValue = await _gdprService.IsConsentAcceptedAsync(consent.Id, (await _workContext.GetCurrentCustomerAsync()).Id);
                    var controlId = $"consent{consent.Id}";
                    var cbConsent = form[controlId];
                    if (!StringValues.IsNullOrEmpty(cbConsent) && cbConsent.ToString().Equals("on"))
                    {
                        //agree
                        if (!previousConsentValue.HasValue || !previousConsentValue.Value)
                        {
                            await _gdprService.InsertLogAsync(customer, consent.Id, GdprRequestType.ConsentAgree, consent.Message);
                        }
                    }
                    else
                    {
                        //disagree
                        if (!previousConsentValue.HasValue || previousConsentValue.Value)
                        {
                            await _gdprService.InsertLogAsync(customer, consent.Id, GdprRequestType.ConsentDisagree, consent.Message);
                        }
                    }
                }

                //newsletter subscriptions
                if (_gdprSettings.LogNewsletterConsent)
                {
                    if (oldCustomerInfoModel.NewsLetterSubscriptions.Where(n => n.IsActive == true).Count() > 0 && !newCustomerInfoModel.Newsletter)
                        await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ConsentDisagree, await _localizationService.GetResourceAsync("Gdpr.Consent.Newsletter"));
                    if (oldCustomerInfoModel.NewsLetterSubscriptions.Where(n => n.IsActive == true).Count() == 0 && newCustomerInfoModel.Newsletter)
                        await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ConsentAgree, await _localizationService.GetResourceAsync("Gdpr.Consent.Newsletter"));
                }

                //user profile changes
                if (!_gdprSettings.LogUserProfileChanges)
                    return;

                if (oldCustomerInfoModel.Gender != newCustomerInfoModel.Gender)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.Gender")} = {newCustomerInfoModel.Gender}");

                if (oldCustomerInfoModel.FirstName != newCustomerInfoModel.FirstName)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.FirstName")} = {newCustomerInfoModel.FirstName}");

                if (oldCustomerInfoModel.LastName != newCustomerInfoModel.LastName)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.LastName")} = {newCustomerInfoModel.LastName}");

                if (oldCustomerInfoModel.ParseDateOfBirth() != newCustomerInfoModel.ParseDateOfBirth())
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.DateOfBirth")} = {newCustomerInfoModel.ParseDateOfBirth()}");

                if (oldCustomerInfoModel.Email != newCustomerInfoModel.Email)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.Email")} = {newCustomerInfoModel.Email}");

                if (oldCustomerInfoModel.Company != newCustomerInfoModel.Company)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.Company")} = {newCustomerInfoModel.Company}");

                if (oldCustomerInfoModel.StreetAddress != newCustomerInfoModel.Address1)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.StreetAddress")} = {newCustomerInfoModel.Address1}");



                if (oldCustomerInfoModel.StreetAddress2 != newCustomerInfoModel.Address2)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.StreetAddress2")} = {newCustomerInfoModel.Address2}");

                if (oldCustomerInfoModel.ZipPostalCode != newCustomerInfoModel.ZipPostalCode)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.ZipPostalCode")} = {newCustomerInfoModel.ZipPostalCode}");

                if (oldCustomerInfoModel.City != newCustomerInfoModel.City)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.City")} = {newCustomerInfoModel.City}");

                if (oldCustomerInfoModel.County != newCustomerInfoModel.County)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.County")} = {newCustomerInfoModel.County}");

                if (oldCustomerInfoModel.CountryId != newCustomerInfoModel.CountryId && newCustomerInfoModel.CountryId.HasValue)
                {
                    var countryName = (await _countryService.GetCountryByIdAsync(Convert.ToInt32(newCustomerInfoModel.CountryId)))?.Name;
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.Country")} = {countryName}");
                }

                if (oldCustomerInfoModel.StateProvinceId != newCustomerInfoModel.StateProvinceId && newCustomerInfoModel.StateProvinceId.HasValue)
                {
                    var stateProvinceName = (await _stateProvinceService.GetStateProvinceByIdAsync(Convert.ToInt32(newCustomerInfoModel.StateProvinceId)))?.Name;
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.StateProvince")} = {stateProvinceName}");
                }
            }
            catch (Exception exception)
            {
                await _logger.ErrorAsync(exception.Message, exception, customer);
            }
        }


        /// <returns>A task that represents the asynchronous operation</returns>
        /// 

        public virtual async Task<IActionResult> CustomCompleted(int? orderId)
        {
            //validation
            if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !_orderSettings.AnonymousCheckoutAllowed)
                return Challenge();

            Order order = null;
            if (orderId.HasValue)
            {
                //load order by identifier (if provided)
                order = await _orderService.GetOrderByIdAsync(orderId.Value);
            }
            if (order == null)
            {
                order = (await _orderService.SearchOrdersAsync(storeId: (await _storeContext.GetCurrentStoreAsync()).Id,
                customerId: (await _workContext.GetCurrentCustomerAsync()).Id, pageSize: 1))
                    .FirstOrDefault();
            }
            if (order == null || order.Deleted || (await _workContext.GetCurrentCustomerAsync()).Id != order.CustomerId)
            {
                return RedirectToRoute("Homepage");
            }

            //disable "order completed" page?
            if (_orderSettings.DisableOrderCompletedPage)
            {
                return RedirectToRoute("OrderDetails", new { orderId = order.Id });
            }


            if (!order.OrderConfirmed)
            {
                if (order.PaymentMethodSystemName.Contains("Affirm"))
                {

                    var orderSession = await this._paymentSessionService.GetOrderSession(orderId ?? 0);
                    if (orderSession != null)
                    {
                        await UpdateOrderDiscountSendMail(JsonConvert.DeserializeObject<IList<ShoppingCartItem>>(orderSession.CartItems), (int)orderId,
                                                     JsonConvert.DeserializeObject<ShippingOption>(orderSession.ShippingMethod), true
                                                     );
                    }

                }
            }
            //model
            var model = await _orderModelFactory.PrepareCustomOrderDetailsModelAsync(order);
            model.OrderConfirmed = order.OrderConfirmed;
            if (!order.OrderConfirmed)
            {
                order.OrderConfirmed = true;
                await _orderService.UpdateOrderAsync(order);
            }
            return View("Completed", model);
        }

        private async Task UpdateOrderDiscountSendMail(IList<ShoppingCartItem> cart, int orderId, ShippingOption shippingOption, bool isAffirmOrder = false)
        {
            try
            {
                await _logger.InsertLogAsync(LogLevel.Information, $"IsAffirmOrder {isAffirmOrder} Order Updates Started {orderId}", string.Empty, await _workContext.GetCurrentCustomerAsync());
                #region Update No Of Sales

                foreach (var item in cart)
                {
                    var product = await _productService.GetProductByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        product.NoOfSales = product.NoOfSales + item.Quantity;
                        await _productService.UpdateProductAsync(product);
                    }

                }
                #endregion

                #region Updatecustomdiscounts

                #region Order



                (decimal buyMoreSaveMoreDiscount, decimal membershipdiscount, decimal offerDiscount, decimal offerDiscountDefault, decimal productItemsDiscount, _) = await _orderTotalCalculationService.GetCustomBuyMoreSaveMoreDiscountAndMemberShipDiscountAsync(cart);

                (decimal customDutyPercentage, decimal customDuty) = await _orderTotalCalculationService.GetCustomDuty(cart);
                decimal memberShipFee = 0;
                decimal memberShipFeeDiscount = 0;
                if (await _customerExtendedService.IsMemberShipAddedInCart(await _workContext.GetCurrentCustomerAsync()))
                    (memberShipFee, memberShipFeeDiscount) = await _orderTotalCalculationService.GetMemberShipFee();

                var order = await _orderService.GetOrderByIdAsync(orderId);

                // custom Duty
                order.CustomDutyInclTax = order.CustomDutyExclTax = customDuty;
                order.CustomDutyPercentage = customDutyPercentage;
                // end 

                order.MembershipDiscountIncTax = membershipdiscount;
                order.OfferDiscountIncTax = offerDiscount;
                order.BuyMoreSaveMoreDiscountIncTax = buyMoreSaveMoreDiscount;
                order.MembershipFeeInclTax = memberShipFee;
                order.MembershipFeeDiscountInclTax = await _currencyService.ConvertToPrimaryStoreCurrencyAsync(memberShipFeeDiscount, await _workContext.GetWorkingCurrencyAsync());
                if (shippingOption != null && shippingOption.AdditionalFee > 0)
                {
                    if (Math.Round(order.OrderShippingInclTax, 2) == Math.Round(shippingOption.Rate, 2))
                    {
                        order.OrderShippingExclTax = shippingOption.Rate - shippingOption.AdditionalFee;
                        order.OrderShippingInclTax = shippingOption.Rate - shippingOption.AdditionalFee;
                        order.AdditonalShippingChargesInclTax = shippingOption.AdditionalFee;
                    }
                    else
                    {
                        decimal _shippingTotal = Math.Round(order.OrderShippingInclTax, 2);
                        if (_shippingTotal > 0)
                            if (_shippingTotal > Math.Round(shippingOption.Rate, 2))
                            {
                                order.OrderShippingExclTax = order.OrderShippingInclTax = order.OrderShippingInclTax - shippingOption.AdditionalFee;
                                order.AdditonalShippingChargesInclTax = shippingOption.AdditionalFee;
                            }
                            else
                            {
                                decimal shippingDiscountPercentage = Math.Round((100 - ((_shippingTotal / (shippingOption.Rate) * 100))), 2);
                                _shippingTotal = (shippingOption.Rate - shippingOption.AdditionalFee);
                                _shippingTotal = Math.Round(_shippingTotal - ((_shippingTotal * shippingDiscountPercentage) / 100), 2);
                                order.OrderShippingExclTax = order.OrderShippingInclTax = _shippingTotal;
                                order.AdditonalShippingChargesInclTax =
                                    Math.Round(shippingOption.AdditionalFee - ((shippingOption.AdditionalFee * shippingDiscountPercentage) / 100), 2);
                            }
                    }
                }



                #endregion

                #region Order Items



                var model = new ShoppingCartModel();

                model = await _shoppingCartModelFactory.PrepareCustomShoppingCartModelAsync(model, cart, isEditable: false,
                    prepareAndDisplayOrderReviewData: false);
                var orderItems = await _orderService.GetOrderItemsAsync(order.Id);

                decimal orderitemDiscount = 0;
                foreach (var item in model.Items)
                {
                    var cartItem = cart.Where(m => m.Id == item.Id).FirstOrDefault();
                    if (cartItem != null)
                    {
                        var _orderItem = orderItems.Where(m => m.ProductId == cartItem.ProductId && m.AttributesXml == cartItem.AttributesXml).FirstOrDefault();
                        if (_orderItem != null)
                        {
                            _orderItem.ItemPriceIncTax = await _currencyService.ConvertToPrimaryStoreCurrencyAsync(string.IsNullOrEmpty(item.SubTotal) ? 0 : (Decimal.Parse(item.SubTotal.Replace("\"", ""), NumberStyles.Currency) / item.Quantity), await _workContext.GetWorkingCurrencyAsync());
                            _orderItem.MembershipDiscountIncTax = await _currencyService.ConvertToPrimaryStoreCurrencyAsync(string.IsNullOrEmpty(item.MemberShipDiscount) ? 0 : Decimal.Parse(item.MemberShipDiscount.Replace("\"", ""), NumberStyles.Currency), await _workContext.GetWorkingCurrencyAsync());
                            _orderItem.OfferDiscountIncTax = await _currencyService.ConvertToPrimaryStoreCurrencyAsync(string.IsNullOrEmpty(item.OfferDiscount) ? 0 : Decimal.Parse(item.OfferDiscount.Replace("\"", ""), NumberStyles.Currency), await _workContext.GetWorkingCurrencyAsync());
                            _orderItem.BuyMoreSaveMoreDiscountIncTax = await _currencyService.ConvertToPrimaryStoreCurrencyAsync(string.IsNullOrEmpty(item.BuyMoreSaveMoreDiscount) ? 0 : Decimal.Parse(item.BuyMoreSaveMoreDiscount.Replace("\"", ""), NumberStyles.Currency), await _workContext.GetWorkingCurrencyAsync());
                            _orderItem.SpecialInstructions = cartItem.SpecialInstructions;

                            // item Discount
                            if ((_orderItem.DiscountAmountExclTax > 0 || _orderItem.DiscountAmountInclTax > 0)
                                && _orderItem.UnitPriceInclTax > 0)
                            {
                                var price = (_orderItem.UnitPriceInclTax * _orderItem.Quantity) + _orderItem.DiscountAmountInclTax;
                                var percentage = (_orderItem.DiscountAmountInclTax / price) * 100;
                                price = (_orderItem.ItemPriceIncTax * _orderItem.Quantity) + _orderItem.MembershipDiscountIncTax + _orderItem.OfferDiscountIncTax + _orderItem.BuyMoreSaveMoreDiscountIncTax;
                                _orderItem.TotalDiscount = Math.Round((price * percentage) / 100, 2);

                                orderitemDiscount += _orderItem.TotalDiscount;
                            }
                            await _orderService.UpdateOrderItemAsync(_orderItem);
                        }
                    }
                }
                #region SubTotalDiscount

                var customSubTotal = await _orderTotalCalculationService.GetCustomShoppingCartSubTotalAsync(cart);
                customSubTotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(customSubTotal, await _workContext.GetWorkingCurrencyAsync());

                if (order.OrderSubTotalDiscountExclTax != 0)
                {
                    var discountedTotal = (order.OrderSubtotalExclTax +
                                        memberShipFee + (offerDiscountDefault - offerDiscount))
                                        - memberShipFeeDiscount
                                        - buyMoreSaveMoreDiscount
                                        - membershipdiscount - orderitemDiscount;
                    var accuratediscountedTotal = ((customSubTotal - offerDiscountDefault) +
                 memberShipFee + (offerDiscountDefault - offerDiscount))
                 - memberShipFeeDiscount
                 - buyMoreSaveMoreDiscount
                 - membershipdiscount
                 - orderitemDiscount;


                    var percentage = (order.OrderSubTotalDiscountExclTax / discountedTotal) * 100;
                    order.OrderSubTotalDiscountExclTax = order.OrderSubTotalDiscountInclTax =
                        Math.Round((accuratediscountedTotal * percentage) / 100, 2);

                    if (customDutyPercentage > 0)
                    {
                        order.CustomDutyInclTax = Math.Round(((accuratediscountedTotal - order.OrderSubTotalDiscountInclTax) * order.CustomDutyPercentage) / 100, 2);
                    }

                }

                #endregion

                #region Tax

                var (shoppingCartTaxBase, taxRates, taxes) = await _orderTotalCalculationService.CustomGetTaxTotalAsync(cart);
                try
                {
                    string taxInfo = "";
                    foreach (var tax in taxes.Where(t => t.TaxRate > 0))
                    {
                        taxInfo += $"{tax.TaxType.ToString()}:{tax.TaxRate.ToString()}:{tax.Amount};";
                    }
                    order.TaxInfo = taxInfo;
                }
                catch (Exception exp)
                {
                    await _logger.InsertLogAsync(LogLevel.Error, $"IsAffirmOrder {isAffirmOrder} Order {order.Id} failed to update tax info", exp.Message);
                }
                #endregion

                #region Email

                string email = (await _workContext.GetCurrentCustomerAsync())?.Email;
                if (string.IsNullOrEmpty(email))
                {
                    email = (await _addressService.GetAddressByIdAsync(order.BillingAddressId))?.Email;
                    if (string.IsNullOrEmpty(email) && order.ShippingAddressId.HasValue)
                        email = (await _addressService.GetAddressByIdAsync(Convert.ToInt32(order.ShippingAddressId)))?.Email;
                }
                #endregion
                order.OrderSubtotalExclTax = customSubTotal;
                order.OrderSubtotalInclTax = customSubTotal;
                order.CustomerEmail = email;
                await _orderService.UpdateOrderAsync(order);
                #endregion

                #region Order Notes

                if (!string.IsNullOrEmpty(order.CheckoutAttributeDescription))
                {
                    var notes = order.CheckoutAttributeDescription.Replace("Order Notes:", "", StringComparison.InvariantCultureIgnoreCase);
                    await _orderService.InsertOrderNoteAsync(new OrderNote()
                    {
                        CreatedOnUtc = DateTime.UtcNow,
                        DisplayToCustomer = true,
                        DownloadId = 0,
                        Note = notes,
                        OrderId = order.Id
                    });
                }

                #endregion

                #region AbandonedCart Process


                await _abandonedCartService.MarkAbandonedInvoiceAsPaid(order.CustomerId, cart.Select(c => c.Id).ToArray(), order.Id, order.OrderTotal);

                #endregion
                await _logger.InsertLogAsync(LogLevel.Information, $"IsAffirmOrder {isAffirmOrder} Order Updates Completed {orderId}", string.Empty, await _workContext.GetCurrentCustomerAsync());

                #region Notifications
                await _logger.InsertLogAsync(LogLevel.Information, $"IsAffirmOrder {isAffirmOrder} Order Notification process Started {orderId}", string.Empty, await _workContext.GetCurrentCustomerAsync());
                await _orderProcessingExtendedService.CustomSendNotificationsAndSaveNotesAsync(order);
                await _logger.InsertLogAsync(LogLevel.Information, $"IsAffirmOrder {isAffirmOrder} Order Notification process Completed {orderId}", string.Empty, await _workContext.GetCurrentCustomerAsync());

                #endregion

                #endregion

                #region Apply Membership level
                var _customer = await _workContext.GetCurrentCustomerAsync();

                if (await _customerExtendedService.IsMemberShipAddedInCart(_customer))
                {
                    var memberShipRole = await _settingService.GetSettingByKeyAsync<string>("MemberShip.Role.Name");

                    if (memberShipRole == "MemberShip.Role.Name")
                        await _logger.InsertLogAsync(LogLevel.Error, "Membership Program: Setting missed  \"MemberShip.Role.Name\" for Membership Program", "Membership wil not work without this setting.");


                    var customerRole = await _customerService.GetCustomerRoleBySystemNameAsync(memberShipRole);
                    if (customerRole == null)
                        await _logger.InsertLogAsync(LogLevel.Error, "Membership Program: Customer role not exist with name " + memberShipRole,
                            "Customer role not exist with name " + memberShipRole);
                    else
                        await _customerService.AddCustomerRoleMappingAsync(new CustomerCustomerRoleMapping()
                        {
                            CustomerId = _customer.Id,
                            CustomerRoleId = customerRole.Id
                        });
                    await this._genericAttributeService.SaveAttributeAsync(_customer, NopCustomerDefaults.MemberShipLabelAttribute, false,
                        (await _storeContext.GetCurrentStoreAsync()).Id);
                }

                #endregion

                #region Order Zoho Lead

                string description = "";
                foreach (var item in orderItems)
                {
                    var product = await _productService.GetProductByIdAsync(item.ProductId);
                    description += "ProductId:" + item.ProductId + ";SKU:" + product?.Sku ?? "" + "|";
                }

                var iPAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress == null ? "" : _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();

                await _zohoService.CreateUpdateOrderContactPotential(order.Id, string.Empty, _customer, "Paid", description, iPAddress, string.Empty, order.OrderTotal, -1, string.Empty, false);

                #endregion
            }
            catch (Exception exp)
            {
                await _logger.InsertLogAsync(LogLevel.Error, $"IsAffirmOrder {isAffirmOrder} Issue happening in implementing custom updates on order {orderId}", exp.Message, await _workContext.GetCurrentCustomerAsync());
            }
        }



        #endregion

    }
}
