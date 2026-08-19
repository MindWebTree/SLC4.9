using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using MWT.Nop.Core.Infrastructure;
using MWT.Nop.Core.Services.Message;
using MWT.Plugin.Misc.MwtStorefront.Infrastructure.Mapping;
using MWT.Plugin.Misc.MwtStorefront.Models.Common;
using MWT.Plugin.Misc.MwtStorefront.Models.Customer;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using Nop.Core.Http;
using Nop.Core.Http.Extensions;
using Nop.Services.Attributes;
using Nop.Services.Authentication;
using Nop.Services.Authentication.External;
using Nop.Services.Authentication.MultiFactor;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.ExportImport;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Tax;
using Nop.Web.Controllers;
using Nop.Web.Factories;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Validators;
using Nop.Web.Models.Customer;
using System.Text.Encodings.Web;
using ILogger = Nop.Services.Logging.ILogger;



namespace MWT.Plugin.Misc.MwtStorefront.Controllers
{
    [AutoValidateAntiforgeryToken]
    public partial class CustomerExtendedController : CustomerController
    {
        public CustomerExtendedController(AddressSettings addressSettings, CaptchaSettings captchaSettings, CustomerSettings customerSettings,
            DateTimeSettings dateTimeSettings, ForumSettings forumSettings, GdprSettings gdprSettings, HtmlEncoder htmlEncoder, IAddressModelFactory addressModelFactory,
            IAddressService addressService, IAttributeParser<AddressAttribute, AddressAttributeValue> addressAttributeParser,
            IAttributeParser<CustomerAttribute, CustomerAttributeValue> customerAttributeParser, IAttributeService<CustomerAttribute, CustomerAttributeValue> customerAttributeService
            , IAuthenticationService authenticationService, ICountryService countryService, ICurrencyService currencyService,
            ICustomerActivityService customerActivityService, ICustomerModelFactory customerModelFactory, ICustomerRegistrationService customerRegistrationService,
            ICustomerService customerService, IDownloadService downloadService, IEventPublisher eventPublisher, IExportManager exportManager,
            IExternalAuthenticationService externalAuthenticationService, IGdprService gdprService, IGenericAttributeService genericAttributeService,
            IGiftCardService giftCardService, ILocalizationService localizationService, ILogger logger, IMultiFactorAuthenticationPluginManager multiFactorAuthenticationPluginManager
            , INewsLetterSubscriptionService newsLetterSubscriptionService, INotificationService notificationService, IOrderService orderService, IPermissionService permissionService,
            IPictureService pictureService, IPriceFormatter priceFormatter, IProductService productService, IStateProvinceService stateProvinceService,
            IStoreContext storeContext, ITaxService taxService, IWorkContext workContext, IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings, MediaSettings mediaSettings, MultiFactorAuthenticationSettings multiFactorAuthenticationSettings,
            StoreInformationSettings storeInformationSettings, TaxSettings taxSettings) : base(addressSettings, captchaSettings, customerSettings, dateTimeSettings, forumSettings, gdprSettings, htmlEncoder, addressModelFactory, addressService, addressAttributeParser, customerAttributeParser, customerAttributeService, authenticationService, countryService, currencyService, customerActivityService, customerModelFactory, customerRegistrationService, customerService, downloadService, eventPublisher, exportManager, externalAuthenticationService, gdprService, genericAttributeService, giftCardService, localizationService, logger, multiFactorAuthenticationPluginManager, newsLetterSubscriptionService, notificationService, orderService, permissionService, pictureService, priceFormatter, productService, stateProvinceService, storeContext, taxService, workContext, workflowMessageService, localizationSettings, mediaSettings, multiFactorAuthenticationSettings, storeInformationSettings, taxSettings)
        {
        }



        #region Login / logout



        [CheckAccessPublicStore(true)]
        [HttpPost]
        public virtual async Task<IActionResult> LoginRegisterPopup(LoginRegisterModel model)
        {
            int statusCode = 500;
            string message = "";
            string redirectUrl = "";
            var _customer = await _customerService.GetCustomerByEmailAsync(model.Email.Trim());
            if (_customer != null)
                model.ProcessType = "login";
            else
                model.ProcessType = "register";
            switch (model.ProcessType)
            {
                #region Login
                case "login":
                    {
                        var logincustomerEmail = model.Email.Trim();
                        var userNameOrEmail = _customerSettings.UsernamesEnabled ? logincustomerEmail : logincustomerEmail;
                        var loginResult = await _customerRegistrationService.ValidateCustomerAsync(userNameOrEmail, model.Password);
                        switch (loginResult)
                        {
                            case CustomerLoginResults.Successful:
                                {
                                    statusCode = 200;
                                    var customer_login = _customerSettings.UsernamesEnabled
                                        ? await _customerService.GetCustomerByUsernameAsync(logincustomerEmail)
                                        : await _customerService.GetCustomerByEmailAsync(logincustomerEmail);

                                    await _customerRegistrationService.SignInCustomerAsync(customer_login, null, true);
                                    message = await this._localizationService.GetResourceAsync("Customer.Login.Successfull.Message");
                                    break;
                                }
                            case CustomerLoginResults.CustomerNotExist:
                                message = await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.CustomerNotExist");
                                break;
                            case CustomerLoginResults.Deleted:
                                message = await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.Deleted");
                                break;
                            case CustomerLoginResults.NotActive:
                                message = await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.NotActive");
                                break;
                            case CustomerLoginResults.NotRegistered:
                                message = await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.NotRegistered");
                                break;
                            case CustomerLoginResults.LockedOut:
                                message = await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.LockedOut");
                                break;
                            case CustomerLoginResults.WrongPassword:
                            default:
                                message = await _localizationService.GetResourceAsync("Account.Login.WrongCredentials");
                                break;
                        }
                        break;
                    }
                #endregion


                #region Register
                case "register":
                    {
                        //check whether registration is allowed
                        if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
                            message = await this._localizationService.GetResourceAsync("Registration.Disabled.Message");

                        if (await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                        {
                            //Already registered customer. 
                            await _authenticationService.SignOutAsync();

                            //raise logged out event       
                            await _eventPublisher.PublishAsync(new CustomerLoggedOutEvent(await _workContext.GetCurrentCustomerAsync()));

                            //Save a new record
                            await _workContext.SetCurrentCustomerAsync(await _customerService.InsertGuestCustomerAsync());
                        }
                        var customer = await _workContext.GetCurrentCustomerAsync();
                        customer.RegisteredInStoreId = (await _storeContext.GetCurrentStoreAsync()).Id;

                        var customerEmail = model.Email?.Trim();

                        var isApproved = _customerSettings.UserRegistrationType == UserRegistrationType.Standard;
                        var registrationRequest = new CustomerRegistrationRequest(customer,
                            customerEmail,
                            _customerSettings.UsernamesEnabled ? customerEmail : customerEmail,
                            model.Password,
                            _customerSettings.DefaultPasswordFormat,
                            (await _storeContext.GetCurrentStoreAsync()).Id,
                            isApproved);
                        var registrationResult = await _customerRegistrationService.RegisterCustomerAsync(registrationRequest);
                        if (registrationResult.Success)
                        {
                            statusCode = 200;
                            message = await this._localizationService.GetResourceAsync("Customer.Register.Successfull.Message");
                            if (_customerSettings.AcceptPrivacyPolicyEnabled)
                            {
                                //privacy policy is required
                                //GDPR
                                if (_gdprSettings.GdprEnabled && _gdprSettings.LogPrivacyPolicyConsent)
                                {
                                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ConsentAgree, await _localizationService.GetResourceAsync("Gdpr.Consent.PrivacyPolicy"));
                                }
                            }


                            //notifications
                            if (_customerSettings.NotifyNewCustomerRegistration)
                                await _workflowMessageService.SendCustomerRegisteredStoreOwnerNotificationMessageAsync(customer,
                                    _localizationSettings.DefaultAdminLanguageId);

                            //raise event       
                            await _eventPublisher.PublishAsync(new CustomerRegisteredEvent(customer));

                            switch (_customerSettings.UserRegistrationType)
                            {
                                case UserRegistrationType.EmailValidation:
                                    //email validation message
                                    await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.AccountActivationTokenAttribute, Guid.NewGuid().ToString());
                                    await _workflowMessageService.SendCustomerEmailValidationMessageAsync(customer, (await _workContext.GetWorkingLanguageAsync()).Id);
                                    redirectUrl = Url.RouteUrl("RegisterResult", new { resultId = (int)UserRegistrationType.EmailValidation, model.ReturnUrl });
                                    //result
                                    break;

                                case UserRegistrationType.AdminApproval:
                                    redirectUrl = Url.RouteUrl("RegisterResult", new { resultId = (int)UserRegistrationType.AdminApproval, model.ReturnUrl });
                                    break;

                                case UserRegistrationType.Standard:
                                    //send customer welcome message
                                    await _workflowMessageService.SendCustomerWelcomeMessageAsync(customer, (await _workContext.GetWorkingLanguageAsync()).Id);
                                    //raise event       
                                    await _eventPublisher.PublishAsync(new CustomerActivatedEvent(customer));
                                    await _customerRegistrationService.SignInCustomerAsync(customer, null, true);
                                    redirectUrl = "";
                                    break;
                                default:
                                    break;
                            }
                        }

                        //errors
                        foreach (var _error in registrationResult.Errors)
                        {
                            message = message + " " + _error;
                        }
                        break;
                    }

                    #endregion
            }
            return Json(new { statusCode = statusCode, message = message, redirectUrl = redirectUrl });
        }


        [CheckAccessPublicStore(true)]
        [HttpPost]
        public virtual async Task<IActionResult> ValidateCustomer(LoginRegisterModel model)
        {
            var _customer = await _customerService.GetCustomerByEmailAsync(model.Email.Trim());
            return Json(new
            {
                statusCode = 200,
                message = _customer != null ? await _localizationService.GetResourceAsync("cart.popup.wishlist.Password.Title") : "",
                buttonText = await _localizationService.GetResourceAsync(_customer == null ?
            "common.createaccount" : "common.login")
            });
        }



        [HttpPost, ActionName("PasswordRecoveryConfirm")]
        [FormValueRequired("custom-set-password")]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        //available even when a store is closed
        [CheckAccessClosedStore(true)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> CustomPasswordRecoveryConfirmPOST(string token, string email, Guid guid, PasswordRecoveryConfirmModel model)
        {
            //For backward compatibility with previous versions where email was used as a parameter in the URL
            var customer = await _customerService.GetCustomerByEmailAsync(email)
                ?? await _customerService.GetCustomerByGuidAsync(guid);

            if (customer == null)
                return RedirectToRoute("Homepage");

            model.ReturnUrl = Url.RouteUrl("Homepage");

            //validate token
            if (!await _customerService.IsPasswordRecoveryTokenValidAsync(customer, token))
            {
                model.DisablePasswordChanging = true;
                model.Result = await _localizationService.GetResourceAsync("Account.PasswordRecovery.WrongToken");
                return View(model);
            }

            //validate token expiration date
            if (await _customerService.IsPasswordRecoveryLinkExpiredAsync(customer))
            {
                model.DisablePasswordChanging = true;
                model.Result = await _localizationService.GetResourceAsync("Account.PasswordRecovery.LinkExpired");
                return View(model);
            }

            if (!ModelState.IsValid)
                return View(model);

            var response = await _customerRegistrationService
                .ChangePasswordAsync(new ChangePasswordRequest(customer.Email, false, _customerSettings.DefaultPasswordFormat, model.NewPassword));
            if (!response.Success)
            {
                model.Result = string.Join(';', response.Errors);
                return View(model);
            }

            await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.PasswordRecoveryTokenAttribute, "");

            if (customer.IsImported)
            {
                customer.IsImported = false;
                await _customerService.UpdateCustomerAsync(customer);
            }
            //authenticate customer after changing password
            await _customerRegistrationService.SignInCustomerAsync(customer, null, true);

            model.DisablePasswordChanging = true;
            model.Result = await _localizationService.GetResourceAsync("Account.PasswordRecovery.PasswordHasBeenChanged");
            return View(model);
        }

        #endregion


        #region AJAX sign-in sign-up

        [HttpPost]
        [ValidateCaptcha]
        [CheckAccessClosedStore(true)]
        [CheckAccessPublicStore(true)]
        public async Task<IActionResult> CustomSignIn(LoginModel model, string returnUrl, bool captchaValid)
        {
            int statusCode = 500;
            string message = "";
            string redirectUrl = "";

            if (_captchaSettings.Enabled && _captchaSettings.ShowOnLoginPage && !captchaValid)
            {
                message = await _localizationService.GetResourceAsync("Common.WrongCaptchaMessage");
            }
            else if (ModelState.IsValid)
            {
                var customerUserName = model.Username?.Trim();
                var customerEmail = model.Email?.Trim();
                var userNameOrEmail = _customerSettings.UsernamesEnabled ? customerUserName : customerEmail;
                var loginResult = await _customerRegistrationService.ValidateCustomerAsync(userNameOrEmail, model.Password);
                switch (loginResult)
                {
                    case CustomerLoginResults.Successful:
                        {
                            statusCode = 200;
                            var customer_login = _customerSettings.UsernamesEnabled
                                ? await _customerService.GetCustomerByUsernameAsync(customerUserName)
                                : await _customerService.GetCustomerByEmailAsync(customerEmail);

                            await _customerRegistrationService.SignInCustomerAsync(customer_login, null, true);
                            message = await this._localizationService.GetResourceAsync("Customer.Login.Successfull.Message");
                            break;
                        }
                    case CustomerLoginResults.CustomerNotExist:
                        message = await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.CustomerNotExist");
                        break;
                    case CustomerLoginResults.Deleted:
                        message = await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.Deleted");
                        break;
                    case CustomerLoginResults.NotActive:
                        message = await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.NotActive");
                        break;
                    case CustomerLoginResults.NotRegistered:
                        message = await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.NotRegistered");
                        break;
                    case CustomerLoginResults.LockedOut:
                        message = await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.LockedOut");
                        break;
                    case CustomerLoginResults.WrongPassword:
                    default:
                        message = await _localizationService.GetResourceAsync("Account.Login.WrongCredentials");
                        break;
                }
            }

            return Json(new { statusCode = statusCode, message = message, redirectUrl = redirectUrl });
        }

        [HttpPost]
        [ValidateCaptcha]
        [CheckAccessClosedStore(true)]
        [CheckAccessPublicStore(true)]
        public async Task<IActionResult> CustomRegister(RegisterModel model, string returnUrl, IFormCollection form, bool captchaValid)
        {
            int statusCode = 500;
            string message = "";
            string redirectUrl = "";
            if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
                message = await this._localizationService.GetResourceAsync("Registration.Disabled.Message");
            else
            {
                var store = await _storeContext.GetCurrentStoreAsync();
                var customer = await _workContext.GetCurrentCustomerAsync();
                var language = await _workContext.GetWorkingLanguageAsync();
                if (await _customerService.IsRegisteredAsync(customer))
                {
                    //Already registered customer. 
                    await _authenticationService.SignOutAsync();

                    //raise logged out event       
                    await _eventPublisher.PublishAsync(new CustomerLoggedOutEvent(customer));

                    customer = await _customerService.InsertGuestCustomerAsync();

                    //Save a new record
                    await _workContext.SetCurrentCustomerAsync(customer);
                }


                customer.RegisteredInStoreId = store.Id;
                var customerAttributesXml = await ParseCustomCustomerAttributesAsync(form);
                var customerAttributeWarnings = await _customerAttributeParser.GetAttributeWarningsAsync(customerAttributesXml);
                foreach (var error in customerAttributeWarnings)
                {
                    ModelState.AddModelError("", error);
                }

                //validate CAPTCHA
                if (_captchaSettings.Enabled && _captchaSettings.ShowOnRegistrationPage && !captchaValid)
                {
                    ModelState.AddModelError("", await _localizationService.GetResourceAsync("Common.WrongCaptchaMessage"));
                }

                //GDPR
                if (_gdprSettings.GdprEnabled)
                {
                    var consents = (await _gdprService
                        .GetAllConsentsAsync()).Where(consent => consent.DisplayDuringRegistration && consent.IsRequired).ToList();

                    ValidateRequiredConsents(consents, form);
                }


                if (_captchaSettings.Enabled && _captchaSettings.ShowOnRegistrationPage && !captchaValid)
                {
                    message = await _localizationService.GetResourceAsync("Common.WrongCaptchaMessage");
                }
                else if (ModelState.IsValid)
                {
                    var customerUserName = model.Username;
                    var customerEmail = model.Email;

                    var isApproved = _customerSettings.UserRegistrationType == UserRegistrationType.Standard;
                    var registrationRequest = new CustomerRegistrationRequest(customer,
                        customerEmail,
                        _customerSettings.UsernamesEnabled ? customerUserName : customerEmail,
                        model.Password,
                        _customerSettings.DefaultPasswordFormat,
                        store.Id,
                        isApproved);
                    var registrationResult = await _customerRegistrationService.RegisterCustomerAsync(registrationRequest);
                    if (registrationResult.Success)
                    {
                        statusCode = 200;
                        message = await this._localizationService.GetResourceAsync("Customer.Register.Successfull.Message");
                        //properties
                        if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                            customer.TimeZoneId = model.TimeZoneId;
                        //VAT number
                        if (_taxSettings.EuVatEnabled)
                        {
                            var prevVatNumber = customer.VatNumber;
                            customer.VatNumber = model.VatNumber;

                            if (prevVatNumber != model.VatNumber)
                            {
                                var (vatNumberStatus, _, vatAddress) = await _taxService.GetVatNumberStatusAsync(model.VatNumber);
                                customer.VatNumberStatusId = (int)vatNumberStatus;

                                //send VAT number admin notification
                                if (!string.IsNullOrEmpty(model.VatNumber) && _taxSettings.EuVatEmailAdminWhenNewVatSubmitted)
                                    await _workflowMessageService.SendNewVatSubmittedStoreOwnerNotificationAsync(customer,
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
                            customer.DateOfBirth = model.ParseDateOfBirth();
                        if (_customerSettings.CompanyEnabled)
                            customer.Company = model.Company;
                        if (_customerSettings.StreetAddressEnabled)
                            customer.StreetAddress = model.StreetAddress;
                        if (_customerSettings.StreetAddress2Enabled)
                            customer.StreetAddress2 = model.StreetAddress2;
                        if (_customerSettings.ZipPostalCodeEnabled)
                            customer.ZipPostalCode = model.ZipPostalCode;
                        if (_customerSettings.CityEnabled)
                            customer.City = model.City;
                        if (_customerSettings.CountyEnabled)
                            customer.County = model.County;
                        if (_customerSettings.CountryEnabled)
                            customer.CountryId = model.CountryId;
                        if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                            customer.StateProvinceId = model.StateProvinceId;
                        if (_customerSettings.PhoneEnabled)
                            customer.Phone = model.Phone;
                        if (_customerSettings.FaxEnabled)
                            customer.Fax = model.Fax;

                        customer.CustomCustomerAttributesXML = customerAttributesXml;
                        await _customerService.UpdateCustomerAsync(customer);

                        //newsletter
                        if (_customerSettings.NewsletterEnabled)
                        {
                            var currentSubscriptions = await _newsLetterSubscriptionService
                                .GetNewsLetterSubscriptionsByEmailAsync(customer.Email, storeId: store.Id);
                            if (currentSubscriptions.Any())
                            {
                                var subscriptionGuid = currentSubscriptions.FirstOrDefault().NewsLetterSubscriptionGuid;
                                foreach (var newsLetterSubscriptionModel in model.NewsLetterSubscriptions)
                                {
                                    var existingSubscription = currentSubscriptions
                                        .FirstOrDefault(subscription => subscription.TypeId == newsLetterSubscriptionModel.TypeId);
                                    if (existingSubscription is not null && existingSubscription.Active != newsLetterSubscriptionModel.IsActive)
                                    {
                                        existingSubscription.Active = newsLetterSubscriptionModel.IsActive;
                                        await _newsLetterSubscriptionService.UpdateNewsLetterSubscriptionAsync(existingSubscription);
                                    }

                                    if (existingSubscription is null && newsLetterSubscriptionModel.IsActive)
                                    {
                                        await _newsLetterSubscriptionService.InsertNewsLetterSubscriptionAsync(new()
                                        {
                                            NewsLetterSubscriptionGuid = subscriptionGuid,
                                            Email = customer.Email,
                                            Active = true,
                                            TypeId = newsLetterSubscriptionModel.TypeId,
                                            StoreId = store.Id,
                                            LanguageId = customer.LanguageId ?? language.Id,
                                            CreatedOnUtc = DateTime.UtcNow
                                        });
                                    }
                                }
                            }
                            else
                            {
                                var subscriptionGuid = Guid.NewGuid();
                                var activeSubscriptions = model.NewsLetterSubscriptions.Where(subscriptionModel => subscriptionModel.IsActive);
                                foreach (var activeSubscription in activeSubscriptions)
                                {
                                    await _newsLetterSubscriptionService.InsertNewsLetterSubscriptionAsync(new()
                                    {
                                        NewsLetterSubscriptionGuid = subscriptionGuid,
                                        Email = customer.Email,
                                        Active = true,
                                        StoreId = store.Id,
                                        TypeId = activeSubscription.TypeId,
                                        LanguageId = customer.LanguageId ?? language.Id,
                                        CreatedOnUtc = DateTime.UtcNow
                                    });
                                }
                            }
                        }

                        if (_customerSettings.AcceptPrivacyPolicyEnabled)
                        {
                            //privacy policy is required
                            //GDPR
                            if (_gdprSettings.GdprEnabled && _gdprSettings.LogPrivacyPolicyConsent)
                            {
                                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ConsentAgree, await _localizationService.GetResourceAsync("Gdpr.Consent.PrivacyPolicy"));
                            }
                        }

                        //GDPR
                        if (_gdprSettings.GdprEnabled)
                        {
                            var consents = (await _gdprService.GetAllConsentsAsync()).Where(consent => consent.DisplayDuringRegistration).ToList();
                            foreach (var consent in consents)
                            {
                                var controlId = $"consent{consent.Id}";
                                var cbConsent = form[controlId];
                                if (!StringValues.IsNullOrEmpty(cbConsent) && cbConsent.ToString().Equals("on"))
                                {
                                    //agree
                                    await _gdprService.InsertLogAsync(customer, consent.Id, GdprRequestType.ConsentAgree, consent.Message);
                                }
                                else
                                {
                                    //disagree
                                    await _gdprService.InsertLogAsync(customer, consent.Id, GdprRequestType.ConsentDisagree, consent.Message);
                                }
                            }
                        }

                        //insert default address (if possible)
                        var defaultAddress = new Address
                        {
                            FirstName = customer.FirstName,
                            LastName = customer.LastName,
                            Email = customer.Email,
                            Company = customer.Company,
                            CountryId = customer.CountryId > 0
                         ? (int?)customer.CountryId
                         : null,
                            StateProvinceId = customer.StateProvinceId > 0
                         ? (int?)customer.StateProvinceId
                         : null,
                            County = customer.County,
                            City = customer.City,
                            Address1 = customer.StreetAddress,
                            Address2 = customer.StreetAddress2,
                            ZipPostalCode = customer.ZipPostalCode,
                            PhoneNumber = customer.Phone,
                            FaxNumber = customer.Fax,
                            CreatedOnUtc = customer.CreatedOnUtc
                        };
                        if (await _addressService.IsAddressValidAsync(defaultAddress))
                        {
                            //some validation
                            if (defaultAddress.CountryId == 0)
                                defaultAddress.CountryId = null;
                            if (defaultAddress.StateProvinceId == 0)
                                defaultAddress.StateProvinceId = null;
                            //set default address
                            //customer.Addresses.Add(defaultAddress);

                            await _addressService.InsertAddressAsync(defaultAddress);

                            await _customerService.InsertCustomerAddressAsync(customer, defaultAddress);

                            customer.BillingAddressId = defaultAddress.Id;
                            customer.ShippingAddressId = defaultAddress.Id;

                            await _customerService.UpdateCustomerAsync(customer);
                        }

                        //notifications
                        if (_customerSettings.NotifyNewCustomerRegistration)
                            await _workflowMessageService.SendCustomerRegisteredStoreOwnerNotificationMessageAsync(customer,
                                _localizationSettings.DefaultAdminLanguageId);
                        //raise event       
                        await _eventPublisher.PublishAsync(new CustomerRegisteredEvent(customer));

                        switch (_customerSettings.UserRegistrationType)
                        {
                            case UserRegistrationType.EmailValidation:
                                //email validation message
                                await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.AccountActivationTokenAttribute, Guid.NewGuid().ToString());
                                await _workflowMessageService.SendCustomerEmailValidationMessageAsync(customer, (await _workContext.GetWorkingLanguageAsync()).Id);
                                redirectUrl = Url.RouteUrl("RegisterResult", new { resultId = (int)UserRegistrationType.EmailValidation, returnUrl });
                                //result
                                break;

                            case UserRegistrationType.AdminApproval:
                                redirectUrl = Url.RouteUrl("RegisterResult", new { resultId = (int)UserRegistrationType.AdminApproval, returnUrl });
                                break;

                            case UserRegistrationType.Standard:
                                //send customer welcome message
                                await _workflowMessageService.SendCustomerWelcomeMessageAsync(customer, (await _workContext.GetWorkingLanguageAsync()).Id);
                                //raise event       
                                await _eventPublisher.PublishAsync(new CustomerActivatedEvent(customer));
                                await _customerRegistrationService.SignInCustomerAsync(customer, null, true);
                                redirectUrl = "";
                                break;
                            default:
                                break;
                        }

                    }
                    else
                    {
                        statusCode = 500;
                        message = string.Join(',', registrationResult.Errors);
                        redirectUrl = "";
                    }

                }
                else
                {
                    statusCode = 500;
                    message = string.Join(',', ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                }
            }
            return Json(new
            {
                statusCode = statusCode,
                message = message,
                redirectUrl = redirectUrl
            });
        }
        [ValidateCaptcha]
        [HttpPost]
        [ValidateAntiForgeryToken]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        //available even when a store is closed
        [CheckAccessClosedStore(true)]
        public virtual async Task<IActionResult> CustomPasswordRecoverySend(PasswordRecoveryExtendedModel model, bool captchaValid)
        {
            // validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnForgotPasswordPage && !captchaValid)
            {
                model.Result = await _localizationService.GetResourceAsync("Common.WrongCaptchaMessage");
                model.StatusCode = 500;
            }

            else if (ModelState.IsValid)
            {
                var customer = await _customerService.GetCustomerByEmailAsync(model.Email.Trim());
                if (customer != null && customer.Active && !customer.Deleted)
                {
                    //save token and current date
                    var passwordRecoveryToken = Guid.NewGuid();
                    await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.PasswordRecoveryTokenAttribute,
                        passwordRecoveryToken.ToString());
                    DateTime? generatedDateTime = DateTime.UtcNow;
                    await _genericAttributeService.SaveAttributeAsync(customer,
                        NopCustomerDefaults.PasswordRecoveryTokenDateGeneratedAttribute, generatedDateTime);

                    //send email
                    await _workflowMessageService.SendCustomerPasswordRecoveryMessageAsync(customer,
                        (await _workContext.GetWorkingLanguageAsync()).Id);

                    model.Result = await _localizationService.GetResourceAsync("Account.PasswordRecovery.EmailHasBeenSent.Partial");
                    model.StatusCode = 200;
                }
                else
                {
                    model.Result = await _localizationService.GetResourceAsync("Account.PasswordRecovery.EmailNotFound");
                    model.StatusCode = 500;
                }
            }

            var passwordRecoveryModel = await _customerModelFactory.PreparePasswordRecoveryModelAsync(model);
            model.DisplayCaptcha = passwordRecoveryModel.DisplayCaptcha;

            return Json(model);
        }

        #endregion

        #region My account / Info

        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> CustomInfo()
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Challenge();

            var model = new CustomerInfoModel();

            model = await _customerModelFactory.PrepareCustomerInfoModelAsync(model, await _workContext.GetCurrentCustomerAsync(), false);
            var customCustomerInfoModel = model.CustomerInfoModelToCustomerInfoExtendedModel();
            customCustomerInfoModel.FullName = CustomCommonHelper.GetCustomerFullName(model.FirstName, model.LastName);
            return View("Info", customCustomerInfoModel);
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> CustomInfo(CustomerInfoExtendedModel customCustomerInfoModel, IFormCollection form)
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Challenge();
            var store = await _storeContext.GetCurrentStoreAsync();
            var language = await _workContext.GetWorkingLanguageAsync();
            customCustomerInfoModel.FirstName = CustomCommonHelper.GetCustomerFirstName(customCustomerInfoModel.FullName);
            customCustomerInfoModel.LastName = CustomCommonHelper.GetCustomerLastName(customCustomerInfoModel.FullName);

            CustomerInfoModel model = new CustomerInfoModel();
            model = customCustomerInfoModel.CustomerInfoExtendedModelToCustomerInfoModel();
            var oldCustomerModel = new CustomerInfoModel();

            var customer = await _workContext.GetCurrentCustomerAsync();

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

            try
            {
                if (ModelState.IsValid)
                {
                    //username 
                    if (_customerSettings.UsernamesEnabled && _customerSettings.AllowUsersToChangeUsernames)
                    {
                        var userName = model.Username.Trim();
                        if (!customer.Username.Equals(userName, StringComparison.InvariantCultureIgnoreCase))
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
                    if (!customer.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase))
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

                    //properties
                    if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                        customer.TimeZoneId = model.TimeZoneId;
                    //VAT number
                    if (_taxSettings.EuVatEnabled)
                    {
                        var prevVatNumber = customer.VatNumber;
                        customer.VatNumber = model.VatNumber;

                        if (prevVatNumber != model.VatNumber)
                        {
                            var (vatNumberStatus, _, vatAddress) = await _taxService.GetVatNumberStatusAsync(model.VatNumber);
                            customer.VatNumberStatusId = (int)vatNumberStatus;

                            //send VAT number admin notification
                            if (!string.IsNullOrEmpty(model.VatNumber) && _taxSettings.EuVatEmailAdminWhenNewVatSubmitted)
                                await _workflowMessageService.SendNewVatSubmittedStoreOwnerNotificationAsync(customer,
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
                        customer.DateOfBirth = model.ParseDateOfBirth();
                    if (_customerSettings.CompanyEnabled)
                        customer.Company = model.Company;
                    if (_customerSettings.StreetAddressEnabled)
                        customer.StreetAddress = model.StreetAddress;
                    if (_customerSettings.StreetAddress2Enabled)
                        customer.StreetAddress2 = model.StreetAddress2;
                    if (_customerSettings.ZipPostalCodeEnabled)
                        customer.ZipPostalCode = model.ZipPostalCode;
                    if (_customerSettings.CityEnabled)
                        customer.City = model.City;
                    if (_customerSettings.CountyEnabled)
                        customer.County = model.County;
                    if (_customerSettings.CountryEnabled)
                        customer.CountryId = model.CountryId;
                    if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                        customer.StateProvinceId = model.StateProvinceId;
                    if (_customerSettings.PhoneEnabled)
                        customer.Phone = model.Phone;
                    if (_customerSettings.FaxEnabled)
                        customer.Fax = model.Fax;
                    customer.CustomCustomerAttributesXML = customerAttributesXml;
                    await _customerService.UpdateCustomerAsync(customer);


                    //newsletter
                    if (_customerSettings.NewsletterEnabled)
                    {
                        var currentSubscriptions = await _newsLetterSubscriptionService
                            .GetNewsLetterSubscriptionsByEmailAsync(customer.Email, storeId: store.Id);
                        if (currentSubscriptions.Any())
                        {
                            var subscriptionGuid = currentSubscriptions.FirstOrDefault().NewsLetterSubscriptionGuid;
                            foreach (var newsLetterSubscriptionModel in model.NewsLetterSubscriptions)
                            {
                                var existingSubscription = currentSubscriptions
                                    .FirstOrDefault(subscription => subscription.TypeId == newsLetterSubscriptionModel.TypeId);
                                if (existingSubscription is not null && existingSubscription.Active != newsLetterSubscriptionModel.IsActive)
                                {
                                    existingSubscription.Active = newsLetterSubscriptionModel.IsActive;
                                    await _newsLetterSubscriptionService.UpdateNewsLetterSubscriptionAsync(existingSubscription);
                                }

                                if (existingSubscription is null && newsLetterSubscriptionModel.IsActive)
                                {
                                    await _newsLetterSubscriptionService.InsertNewsLetterSubscriptionAsync(new()
                                    {
                                        NewsLetterSubscriptionGuid = subscriptionGuid,
                                        Email = customer.Email,
                                        Active = true,
                                        TypeId = newsLetterSubscriptionModel.TypeId,
                                        StoreId = store.Id,
                                        LanguageId = customer.LanguageId ?? language.Id,
                                        CreatedOnUtc = DateTime.UtcNow
                                    });
                                }
                            }
                        }
                        else
                        {
                            var subscriptionGuid = Guid.NewGuid();
                            var activeSubscriptions = model.NewsLetterSubscriptions.Where(subscriptionModel => subscriptionModel.IsActive);
                            foreach (var activeSubscription in activeSubscriptions)
                            {
                                await _newsLetterSubscriptionService.InsertNewsLetterSubscriptionAsync(new()
                                {
                                    NewsLetterSubscriptionGuid = subscriptionGuid,
                                    Email = customer.Email,
                                    Active = true,
                                    StoreId = store.Id,
                                    TypeId = activeSubscription.TypeId,
                                    LanguageId = customer.LanguageId ?? language.Id,
                                    CreatedOnUtc = DateTime.UtcNow
                                });
                            }
                        }
                    }

                    if (_forumSettings.ForumsEnabled && _forumSettings.SignaturesEnabled)
                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.SignatureAttribute, model.Signature);

                    //GDPR
                    if (_gdprSettings.GdprEnabled)
                        await LogGdprAsync(customer, oldCustomerModel, model, form);

                    return RedirectToRoute("CustomerInfo");
                }
            }
            catch (Exception exc)
            {
                ModelState.AddModelError("", exc.Message);
            }

            //If we got this far, something failed, redisplay form
            model = await _customerModelFactory.PrepareCustomerInfoModelAsync(model, customer, true, customerAttributesXml);
            customCustomerInfoModel = model.CustomerInfoModelToCustomerInfoExtendedModel();
            return View("Info", customCustomerInfoModel);
        }


        #endregion

        #region My account / Addresses

        public virtual async Task<IActionResult> CustomAddressAdd()
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Challenge();

            var model = new CustomerAddressEditExtendedModel();
            await _addressModelFactory.PrepareAddressModelAsync(model.Address,
                address: null,
                excludeProperties: false,
                addressSettings: _addressSettings,
                loadCountries: async () => await _countryService.GetAllCountriesAsync((await _workContext.GetWorkingLanguageAsync()).Id));
            var customAddressModel = new CustomCustomerAddressEditModel();
            customAddressModel.Address = model.Address.AddressExtendedModelToCustomAddressModel(true);
            return View("addressadd", customAddressModel);
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> CustomAddressAdd(CustomCustomerAddressEditModel customAddressModel, IFormCollection form)
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Challenge();

            //custom address attributes
            var customAttributes = await _addressAttributeParser.ParseCustomAttributesAsync(form, NopCommonDefaults.AddressAttributeControlName);
            var customAttributeWarnings = await _addressAttributeParser.GetAttributeWarningsAsync(customAttributes);
            foreach (var error in customAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }
            var model = new CustomerAddressEditExtendedModel();
            model.Address = customAddressModel.Address.CustomAddressModelToAddressExtendedModel();
            model.Address.FirstName = CustomCommonHelper.GetCustomerFirstName(customAddressModel?.Address?.FullName ?? string.Empty);
            model.Address.LastName = CustomCommonHelper.GetCustomerLastName(customAddressModel?.Address?.FullName ?? string.Empty);
            if (ModelState.IsValid)
            {
                var address = model.Address.ToEntity();
                address.CustomAttributes = customAttributes;
                address.CreatedOnUtc = DateTime.UtcNow;
                //some validation
                if (address.CountryId == 0)
                    address.CountryId = null;
                if (address.StateProvinceId == 0)
                    address.StateProvinceId = null;


                await _addressService.InsertAddressAsync(address);

                await _customerService.InsertCustomerAddressAsync(await _workContext.GetCurrentCustomerAsync(), address);

                return RedirectToRoute("CustomerAddresses");
            }

            //If we got this far, something failed, redisplay form
            await _addressModelFactory.PrepareAddressModelAsync(model.Address,
                address: null,
                excludeProperties: true,
                addressSettings: _addressSettings,
                loadCountries: async () => await _countryService.GetAllCountriesAsync((await _workContext.GetWorkingLanguageAsync()).Id),
                overrideAttributesXml: customAttributes);

            customAddressModel.Address = model.Address.AddressExtendedModelToCustomAddressModel(true);

            return View("addressadd", customAddressModel);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> CustomAddressEdit(int addressId)
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Challenge();

            var customer = await _workContext.GetCurrentCustomerAsync();
            //find address (ensure that it belongs to the current customer)
            var address = await _customerService.GetCustomerAddressAsync(customer.Id, addressId);
            if (address == null)
                //address is not found
                return RedirectToRoute("CustomerAddresses");

            var model = new CustomerAddressEditExtendedModel();
            await _addressModelFactory.PrepareAddressModelAsync(model.Address,
                address: address,
                excludeProperties: false,
                addressSettings: _addressSettings,
                loadCountries: async () => await _countryService.GetAllCountriesAsync((await _workContext.GetWorkingLanguageAsync()).Id));
            var customAddressModel = new CustomCustomerAddressEditModel();
            customAddressModel.Address = model.Address.AddressExtendedModelToCustomAddressModel(true);
            customAddressModel.Address.FullName = CustomCommonHelper.GetCustomerFullName(customAddressModel.Address.FirstName, customAddressModel.Address.LastName);
            return View("AddressEdit", customAddressModel);
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> CustomAddressEdit(CustomCustomerAddressEditModel customAddressModel, int addressId, IFormCollection form)
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Challenge();

            var customer = await _workContext.GetCurrentCustomerAsync();
            //find address (ensure that it belongs to the current customer)
            var address = await _customerService.GetCustomerAddressAsync(customer.Id, addressId);
            if (address == null)
                //address is not found
                return RedirectToRoute("CustomerAddresses");

            //custom address attributes
            var customAttributes = await _addressAttributeParser.ParseCustomAttributesAsync(form, NopCommonDefaults.AddressAttributeControlName);
            var customAttributeWarnings = await _addressAttributeParser.GetAttributeWarningsAsync(customAttributes);
            foreach (var error in customAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }
            var model = new CustomerAddressEditExtendedModel();
            model.Address = customAddressModel.Address.CustomAddressModelToAddressExtendedModel();
            model.Address.FirstName = CustomCommonHelper.GetCustomerFirstName(customAddressModel?.Address?.FullName ?? string.Empty);
            model.Address.LastName = CustomCommonHelper.GetCustomerLastName(customAddressModel?.Address?.FullName ?? string.Empty);
            if (ModelState.IsValid)
            {
                address = model.Address.ToEntity(address);
                address.CustomAttributes = customAttributes;
                await _addressService.UpdateAddressAsync(address);

                return RedirectToRoute("CustomerAddresses");
            }

            //If we got this far, something failed, redisplay form
            await _addressModelFactory.PrepareAddressModelAsync(model.Address,
                address: address,
                excludeProperties: true,
                addressSettings: _addressSettings,
                loadCountries: async () => await _countryService.GetAllCountriesAsync((await _workContext.GetWorkingLanguageAsync()).Id),
                overrideAttributesXml: customAttributes);

            customAddressModel.Address = model.Address.AddressExtendedModelToCustomAddressModel(true);
            customAddressModel.Address.FullName = CustomCommonHelper.GetCustomerFullName(customAddressModel.Address.FirstName, customAddressModel.Address.LastName);
            return View("AddressEdit", customAddressModel);
        }

        #endregion
    }
}
