using Microsoft.AspNetCore.Mvc;
using Nop.Services.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Factories;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Customers;
using Nop.Services.Customers;
using Microsoft.AspNetCore.Http;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Tax;
using Nop.Services.Helpers;
using Nop.Services.Tax;
using Nop.Core.Domain.Customers;
using Nop.Services.Logging;
using Microsoft.Extensions.Primitives;
using Nop.Core.Domain.Catalog;
using Nop.Services.Gdpr;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Shipping;
using Nop.Core;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Extensions;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Common;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Controllers
{
    public class CustomerController : BaseCustomOrderController
    {

        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly ICustomerModelFactory _customerModelFactory;
        private readonly ICustomerService _customerService;
        private readonly ICustomerAttributeParser _customerAttributeParser;
        private readonly GdprSettings _gdprSettings;
        private readonly LocalizationSettings _localizationSettings;
        private readonly ICustomerAttributeService _customerAttributeService;
        private readonly DateTimeSettings _dateTimeSettings;
        private readonly TaxSettings _taxSettings;
        private readonly ITaxService _taxService;
        private readonly ForumSettings _forumSettings;
        private readonly ILogger _logger;
        private readonly IGdprService _gdprService;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly CustomerSettings _customerSettings;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IAddressService _addressService;
        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly IStoreContext _storeContext;

        #endregion


        #region Ctor

        public CustomerController(IPermissionService permissionService,
            ICustomerModelFactory customerModelFactory, ICustomerService customerService,
            ICustomerAttributeParser customerAttributeParser,
            GdprSettings gdprSettings,
            LocalizationSettings localizationSettings,
            ICustomerAttributeService customerAttributeService,
            DateTimeSettings dateTimeSettings,
            TaxSettings taxSettings,
            ITaxService taxService,
            ForumSettings forumSettings,
            ILogger logger,
            IGdprService gdprService,
            ICustomerRegistrationService customerRegistrationService,
            CustomerSettings customerSettings,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            IAddressService addressService,
            IAddressAttributeParser addressAttributeParser,
            IStoreContext storeContext)
        {
            this._permissionService = permissionService;
            this._customerModelFactory = customerModelFactory;
            this._customerService = customerService;
            this._customerAttributeParser = customerAttributeParser;
            this._gdprSettings = gdprSettings;
            this._localizationSettings = localizationSettings;
            this._customerAttributeService = customerAttributeService;
            this._dateTimeSettings = dateTimeSettings;
            this._taxService = taxService;
            this._forumSettings = forumSettings;
            this._logger = logger;
            this._gdprService = gdprService;
            this._customerRegistrationService = customerRegistrationService;
            this._customerSettings = customerSettings;
            this._genericAttributeService = genericAttributeService;
            this._localizationService = localizationService;
            this._addressAttributeParser = addressAttributeParser;
            this._storeContext = storeContext;
            this._addressService = addressService;
        }

        #endregion


        #region Customers

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();
            //prepare model
            var model = await _customerModelFactory.PrepareCustomerSearchModelAsync(new CustomerSearchModel());

            return View(model);
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> CustomerList(CustomerSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _customerModelFactory.PrepareCustomerListModelAsync(searchModel);

            return Json(model);
        }

        public virtual async Task<IActionResult> History(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();
            //prepare model
            //try to get a customer with the specified id
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null || customer.Deleted)
                return RedirectToAction("List");

            return View(await this._customerModelFactory.PrepareCustomerModel(customer, true, true));
        }

        public virtual async Task<IActionResult> CreateUpdateCustomer(int customerId)
        {
            var customer = new Customer();
            if (customerId != 0)
                customer = await _customerService.GetCustomerByIdAsync(customerId);
            else
                customer = await _customerService.InsertGuestCustomerAsync();

            if (customer != null && customer.Id > 0)
            {
                return Json(new
                {
                    response = PrepareResponse(
                        200,
                        "",
                        await RenderPartialViewToStringAsync("_customercreateupdate", await _customerModelFactory.PrepareCustomerInfoModelAsync(customer)),
                        "customer_create_update",
                        false,
                        true,
                        true
                        )
                });
            }
            else
                return Json(new
                {
                    response = PrepareResponse(500, await _localizationService.GetResourceAsync("CustomerOrder.notification.Error.CustomerNotFound"),
                    "", "", false, true, false)
                });
        }

        [HttpPost]
        public virtual async Task<IActionResult> CreateUpdateCustomer(CustomerInfoModel model, IFormCollection form)
        {
            var customer = new Customer();
            if (model.Id > 0)
                customer = await _customerService.GetCustomerByIdAsync(model.Id);
            if (customer != null && customer.Id > 0)
            {
                var isRegistered = await _customerService.IsRegisteredAsync(customer);
                try
                {
                    if (ModelState.IsValid)
                    {
                        //username 
                        if (_customerSettings.UsernamesEnabled && _customerSettings.AllowUsersToChangeUsernames && isRegistered)
                        {
                            var userName = model.Email.Trim();
                            if (string.IsNullOrEmpty(customer.Username) || !customer.Username.Equals(userName, StringComparison.InvariantCultureIgnoreCase))
                                await _customerRegistrationService.SetUsernameAsync(customer, userName);
                        }
                        //email
                        var email = model.Email.Trim();
                        if ((string.IsNullOrEmpty(customer.Email) || !customer.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase)) && isRegistered)
                        {
                            //change email
                            var requireValidation = _customerSettings.UserRegistrationType == UserRegistrationType.EmailValidation;
                            await _customerRegistrationService.SetEmailAsync(customer, email, requireValidation);
                        }

                        if (!isRegistered)
                        {
                            await _genericAttributeService.SaveAttributeAsync(customer, "Email", model.Email);
                            if (_customerSettings.UsernamesEnabled && _customerSettings.AllowUsersToChangeUsernames && isRegistered)
                                await _genericAttributeService.SaveAttributeAsync(customer, "UserName", model.Email);

                        }

                        //form fields
                        if (_customerSettings.FirstNameEnabled)
                            await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.FirstNameAttribute, model.FirstName);
                        if (_customerSettings.LastNameEnabled)
                            await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.LastNameAttribute, model.LastName);
                        if (_customerSettings.StreetAddress2Enabled)
                            await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.ZipPostalCodeAttribute, model.ZipPostalCode);
                        if (_customerSettings.PhoneEnabled)
                            await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.PhoneAttribute, model.Phone);

                        if (model.Id == 0)
                        {
                            var customerAttributesXml = await ParseCustomCustomerAttributesAsync(form, _customerAttributeParser, _customerAttributeService);
                            //save customer attributes
                            await _genericAttributeService.SaveAttributeAsync(customer,
                                NopCustomerDefaults.CustomCustomerAttributes, customerAttributesXml);
                        }
                        return Json(new
                        {
                            response = PrepareResponse(200,
                             await _localizationService.GetResourceAsync(model.Id > 0 ?
                            "CustomerOrder.notification.Customer.Edit.Success" :
                            "CustomerOrder.notification.Customer.Add.Success"),
                             "",
                             "customer_create_update",
                             true,
                             true,
                             false)
                        });


                    }
                    else
                    {
                        var customerModel = new CustomerInfoModel();
                        return Json(new
                        {
                            response = PrepareResponse(422,
                            "",
                            await RenderPartialViewToStringAsync("_customercreateupdate", await _customerModelFactory.PrepareCustomerInfoModelAsync(customer))
                            , "",
                            false,
                            true,
                            false)
                        });
                    }
                }
                catch (Exception exc)
                {
                    await _logger.ErrorAsync("Custom Order -- Action CreateUpdateCustomer Method  Post", exc, customer);
                    return Json(new
                    {
                        response = PrepareResponse(500,
                       await _localizationService.GetResourceAsync("CustomerOrder.Message.Failed.AddCustomer"),
                        "",
                        "",
                        false,
                        true,
                        false)
                    });

                }
            }
            else
            {
                await _logger.InsertLogAsync(
                    Core.Domain.Logging.LogLevel.Error,
                    "Custom Order -- Action CreateUpdateCustomer Method  Post",
                    "Failed to find customer with Id " + model.Id, customer);
                return Json(new
                {
                    response = PrepareResponse(500,
                    await _localizationService.GetResourceAsync("CustomerOrder.Message.Failed.AddCustomer"),
                    "",
                    "",
                    false,
                    true,
                    false)
                });
            }
        }



        #region Address

        public virtual async Task<IActionResult> CreateUpdateAddress(int customerId, int addressId, AddressType addressType)
        {
            var customer = new Customer();
            if (customerId > 0)
                customer = await _customerService.GetCustomerByIdAsync(customerId);
            if (customer != null && customer.Id > 0)
            {
                return Json(new
                {
                    response = PrepareResponse(
                    statuscode: 200,
                    html: await RenderPartialViewToStringAsync("_addresscreateupdate",
                      await _customerModelFactory.PrepareCustomAddressModelAsync
                      (customer, addressType, addressId, null, prePopulateNewAddressWithCustomerFields: true)),
                    goto_section: "address_create_update",
                    showContainer: true,
                    isPopup: true
                )
                });
            }
            else
                return Json(new
                {
                    response = PrepareResponse(
                     statuscode: 500,
                    message: await _localizationService.GetResourceAsync("CustomerOrder.notification.Error.CustomerNotFound"),
                    showContainer: false,
                    isPopup: true
                   )
                });
        }

        [HttpPost]
        public virtual async Task<IActionResult> CreateUpdateAddress(AddressModel model, IFormCollection form)
        {

            int.TryParse(form["shipping_address_id"], out var addressId);
            AddressType.TryParse(form["addresstype"], out AddressType addressType);
            int.TryParse(form["customerId"], out var customerId);
            var customer = new Customer();
            if (customerId > 0)
                customer = await _customerService.GetCustomerByIdAsync(customerId);
            if (customer != null && customer.Id > 0)
            {
                if (addressId > 0)
                {
                    //existing address
                    var address = model.ToEntity();
                    var customAttributes = await _addressAttributeParser.ParseCustomAddressAttributesAsync(form);
                    var customAttributeWarnings = await _addressAttributeParser.GetAttributeWarningsAsync(customAttributes);
                    foreach (var error in customAttributeWarnings)
                    {
                        ModelState.AddModelError("", error);
                    }

                    //validate model
                    if (!ModelState.IsValid)
                    {
                        //model is not valid. redisplay the form with errors
                        var shippingAddressModel = await _customerModelFactory.PrepareCustomAddressModelAsync(customer, addressType, addressId, model,
                            selectedCountryId: address.CountryId,
                            overrideAttributesXml: customAttributes,
                            isEdit: true);
                        return Json(new
                        {
                            statuscode = 422,
                            html = await RenderPartialViewToStringAsync("_addresscreateupdate",
                                       shippingAddressModel),
                            closeContainer = false,
                            isPopup = true
                        });

                    }


                    address.CustomAttributes = customAttributes;
                    await _addressService.UpdateAddressAsync(address);

                    // Save Billing
                    if (customer?.BillingAddressId == null)
                        customer.BillingAddressId = address.Id;
                    if (customer?.ShippingAddressId == null)
                        customer.ShippingAddressId = address.Id;
                    if (customer?.BillingAddressId == null || customer?.ShippingAddressId == null)
                        await _customerService.UpdateCustomerAsync(customer);
                }
                else
                {
                    //new address
                    var newAddress = model;

                    //custom address attributes
                    var customAttributes = await _addressAttributeParser.ParseCustomAddressAttributesAsync(form);
                    var customAttributeWarnings = await _addressAttributeParser.GetAttributeWarningsAsync(customAttributes);
                    foreach (var error in customAttributeWarnings)
                    {
                        ModelState.AddModelError("", error);
                    }

                    //validate model
                    if (!ModelState.IsValid)
                    {
                        //model is not valid. redisplay the form with errors
                        var shippingAddressModel = await _customerModelFactory.PrepareCustomAddressModelAsync(customer, addressType, addressId, model,
                                    selectedCountryId: newAddress.CountryId,
                                    overrideAttributesXml: customAttributes,
                                    isEdit: true);
                        return Json(new
                        {
                            response = PrepareResponse(
                                       statuscode: 422,
                            html: await RenderPartialViewToStringAsync("_addresscreateupdate",
                                       shippingAddressModel),
                            closeContainer: false,
                            isPopup: true
                            )
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

                    (customer).ShippingAddressId = address.Id;

                    // Save Billing
                    if (customer?.BillingAddressId == null)
                        customer.BillingAddressId = address.Id;

                    //reset selected shipping method (in case if "pick up in store" was selected)
                    await _genericAttributeService.SaveAttributeAsync<ShippingOption>(customer, NopCustomerDefaults.SelectedShippingOptionAttribute, null, (await _storeContext.GetCurrentStoreAsync()).Id);
                    await _genericAttributeService.SaveAttributeAsync<PickupPoint>(customer, NopCustomerDefaults.SelectedPickupPointAttribute, null, (await _storeContext.GetCurrentStoreAsync()).Id);
                    //limitation - "Ship to the same address" doesn't properly work in "pick up in store only" case (when no shipping plugins are available) 

                    await _customerService.UpdateCustomerAsync(customer);
                }
                return Json(new
                {
                    response = PrepareResponse(
                        statuscode: 200,
                    message: await _localizationService.GetResourceAsync(addressId > 0 ?
                         "CustomerOrder.notification.Address.Edit.Success" :
                         "CustomerOrder.notification.Address.Add.Success"),
                    goto_section: "address_create_update",
                    closeContainer: true,
                    isPopup: true
                    )
                });

            }
            else
                return Json(new
                {
                    response = PrepareResponse(
                    statuscode: 500,
                    message: await _localizationService.GetResourceAsync("CustomerOrder.notification.Error.CustomerNotFound"),
                    closeContainer: false,
                    isPopup: true
                    )
                });
        }

        #endregion


        public virtual async Task<IActionResult> Addresses(int customerId)
        {
            var customer = new Customer();
            if (customerId > 0)
                customer = await _customerService.GetCustomerByIdAsync(customerId);
            if (customer != null && customer.Id > 0)
            {
                var model = await _customerModelFactory.PrepareCustomerAddressListModelAsync(customer);
                return Json(new
                {
                    response = PrepareResponse(
                   statuscode: 200,
                    html: await RenderPartialViewToStringAsync("_addressmanage", model),
                    goto_section: "Address_manage"
                    )
                });

            }
            else
                return Json(new
                {
                    response = PrepareResponse(
                     statuscode: 500,
                    message: await _localizationService.GetResourceAsync("CustomerOrder.notification.Error.CustomerNotFound")
                )
                });
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> AddressDelete(int addressId, int customerId)
        {
            var customer = new Customer();
            if (customerId > 0)
                customer = await _customerService.GetCustomerByIdAsync(customerId);
            if (customer != null && customer.Id > 0)
            {
                var address = await _customerService.GetCustomerAddressAsync(customer.Id, addressId);
                if (address != null)
                {
                    await _customerService.RemoveCustomerAddressAsync(customer, address);
                    await _customerService.UpdateCustomerAsync(customer);
                    await _addressService.DeleteAddressAsync(address);
                }

                return Json(new
                {
                    response = PrepareResponse(
                     statuscode: 200,
                    message: await _localizationService.GetResourceAsync(
                             "CustomerOrder.notification.Address.Deleted.Success"),
                       goto_section: "Address_manage"
                    )
                });
            }
            else
                return Json(new
                {
                    response = PrepareResponse(
                    statuscode: 500,
                    message: await _localizationService.GetResourceAsync("CustomerOrder.notification.Error.CustomerNotFound")
                    )
                });
        }

        #endregion

        #region Utilities

        protected virtual async Task<string> ParseCustomCustomerAttributesAsync(IFormCollection form, ICustomerAttributeParser _customerAttributeParser,
        ICustomerAttributeService _customerAttributeService)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            var attributesXml = "";
            var attributes = await _customerAttributeService.GetAllCustomerAttributesAsync();
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
                                    attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case AttributeControlType.Checkboxes:
                        {
                            var cblAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(cblAttributes))
                            {
                                foreach (var item in cblAttributes.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    var selectedAttributeId = int.Parse(item);
                                    if (selectedAttributeId > 0)
                                        attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
                                            attribute, selectedAttributeId.ToString());
                                }
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //load read-only (already server-side selected) values
                            var attributeValues = await _customerAttributeService.GetCustomerAttributeValuesAsync(attribute.Id);
                            foreach (var selectedAttributeId in attributeValues
                                .Where(v => v.IsPreSelected)
                                .Select(v => v.Id)
                                .ToList())
                            {
                                attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
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
                                attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
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

        public AjaxReponseModel PrepareResponse(int statuscode = 200, string message = "", string html = "",
        string goto_section = "", bool closeContainer = false, bool isPopup = false, bool showContainer = false)
        {
            AjaxReponseModel model = new AjaxReponseModel();
            model.closeContainer = closeContainer;
            model.showContainer = showContainer;
            model.statuscode = statuscode;
            model.isPopup = isPopup;
            model.message = message;
            model.html = html;
            model.goto_section = goto_section;

            return model;

        }

        #endregion
    }
}
