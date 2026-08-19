using Microsoft.AspNetCore.Mvc;
using Nop.Services.Security;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Factories;
using System.Threading.Tasks;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Orders;
using Nop.Services.Customizations.Phone_Order;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Common;
using Nop.Services.Localization;
using System;
using Nop.Services.Logging;
using Newtonsoft.Json;
using System.Linq;
using Nop.MWT.Nop.Core.Domain.CustomOrders;
using Nop.Core.Domain.Customers;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Extensions;
using Nop.Services.Customers;
using Nop.Services.Common;
using Nop.Core;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Nop.Services.Payments;
using Nop.Core.Domain.Common;
using Nop.Core.Http.Extensions;
using Nop.Services.Orders;
using Nop.Services.Catalog;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using static LinqToDB.SqlQuery.SqlPredicate;
using System.Globalization;
using Nop.Services.Customizations.Custom;
using Nop.Core.Infrastructure;
using Nop.Services.Messages;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Controllers
{
    public class OrderController : BaseCustomOrderController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly ICustomOrderModelFactory _customOrderModelFactory;
        private readonly ICustomOrderService _customOrderService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _loggerService;
        private readonly CustomerSettings _customerSettings;
        private readonly ICustomerService _customerService;
        private readonly IAddressService _addressService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IStoreContext _storeContext;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly AddressSettings _addressSettings;
        private readonly IPaymentService _paymentService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;
        private readonly IOrderService _orderService;
        private readonly IZohoService _zohoService;
        private readonly INopFileProvider _nopFileProvider;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IManageService _manageService;
        #endregion

        #region Ctor

        public OrderController(IPermissionService permissionService,
                               ICustomOrderModelFactory customOrderModelFactory,
                               ICustomOrderService customOrderService,
                               ILocalizationService localizationService,
                               ILogger loggerService,
                               CustomerSettings customerSettings,
                               ICustomerService customerService,
                               IAddressService addressService,
                               IGenericAttributeService genericAttributeService,
                               IStoreContext storeContext,
                               ICustomerRegistrationService customerRegistrationService,
                               IPaymentPluginManager paymentPluginManager,
                               AddressSettings addressSettings,
                               IPaymentService paymentService,
                               IOrderProcessingService orderProcessingService,
                               IProductService productService,
                               IWorkContext workContext,
                               IOrderService orderService,
                                IZohoService zohoService,
                                INopFileProvider nopFileProvider,
                                IWorkflowMessageService workflowMessageService,
                                IHttpContextAccessor httpContextAccessor,
                                IProductAttributeService productAttributeService,
                                IProductAttributeParser productAttributeParser,
                                IManageService manageService)
        {
            this._permissionService = permissionService;
            this._customOrderModelFactory = customOrderModelFactory;
            this._customOrderService = customOrderService;
            this._localizationService = localizationService;
            this._loggerService = loggerService;
            this._customerSettings = customerSettings;
            this._customerService = customerService;
            this._addressService = addressService;
            this._genericAttributeService = genericAttributeService;
            this._storeContext = storeContext;
            this._customerRegistrationService = customerRegistrationService;
            this._paymentPluginManager = paymentPluginManager;
            this._addressSettings = addressSettings;
            this._paymentService = paymentService;
            this._orderProcessingService = orderProcessingService;
            this._productService = productService;
            this._workContext = workContext;
            this._orderService = orderService;
            this._zohoService = zohoService;
            this._nopFileProvider = nopFileProvider;
            this._workflowMessageService = workflowMessageService;
            this._httpContextAccessor = httpContextAccessor;
            this._productAttributeService = productAttributeService;
            this._productAttributeParser = productAttributeParser;
            this._manageService = manageService;
        }

        #endregion

        #region Methods

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();
            //prepare model
            var model = await _customOrderModelFactory.PrepareCustomerOrderSearchModelAsync(new CustomOrderSearchModel());

            return View(model);
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> OrderList(CustomOrderSearchModel searchModel, bool IsPartialOrderScreen)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _customOrderModelFactory.PrepareCustomOrderListModelAsync(searchModel, IsPartialOrderScreen);

            return Json(model);
        }

        public virtual async Task<IActionResult> GetBriefSummaryOfOrder(int orderId)
        {
            return PartialView("_orderBriefInfo", await this._customOrderModelFactory.PrepareBriefSummaryOfOrder(orderId));
        }

        public virtual async Task<IActionResult> CreateOrder()
        {
            var order = new Nop.MWT.Nop.Core.Domain.CustomOrders.CustomOrder();
            order.CreatedBy = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0;
            var orderTypes = await _customOrderService.GetOrderTypes();
            await _customOrderService.InsertAsync(order);
            return RedirectToAction("OrderDetails", new { id = order.Id });
        }
        public virtual async Task<IActionResult> OrderDetails(int Id)
        {
            var order = await _customOrderService.GetById(Id);
            /* Added check so that cannnot call the addition service order here*/
            if (order == null || order.ParentOrderID != 0)
                return RedirectToAction("List");
            else return View(await this._customOrderModelFactory.PrepareCustomerOrderModel(order));
        }


        #region Order Steps


        //public virtual async Task<IActionResult> GetOrderTypeSubSection()
        //{
        //    return View("_order_type_section", await this._customOrderModelFactory.PrepareOrderTypesModel());
        //}

        //public virtual async Task<IActionResult> GetCustomerSection(int orderId)
        //{
        //    return View("_customer_section", await this._customOrderModelFactory.PrepareCustomerSection(orderId));
        //}
        #region Order Type Section


        [HttpPost]
        public virtual async Task<IActionResult> UpdateOrderType(int orderId, int typeId, int SubOrderTypeId)
        {
            try
            {
                await _customOrderModelFactory.UpdateOrderType(orderId, typeId, SubOrderTypeId);
                return Json(new
                {
                    response = await PrepareResponse(
                        orderid: orderId,
                      statuscode: 200,
                      html: await RenderViewComponentToStringAsync("CustomOrder_OrderTypeSection", new { orderId = orderId }),
                      message: "",
                      bindSectionId: "UpdateOrderType"
                 )
                });
            }
            catch (Exception exp)
            {
                await this._loggerService.ErrorAsync($"Failed to Update Order type for OrderId {orderId} function UpdateOrderType", exp);
                return Json(new
                {
                    response = await PrepareResponse(
                         orderid: orderId,
                      statuscode: 500,
                      message: await _localizationService.GetResourceAsync("CustomOrder.Failed.Update.OrderType"),
                      bindSectionId: "UpdateOrderType"
                 )
                });
            }
        }


        [HttpPost]
        public virtual async Task<IActionResult> SaveOrderTypeDetails(CustomOrderOrderTypeSectionModel model, bool isReset)
        {

            try
            {
                if (ModelState.IsValid)
                {
                    await _customOrderModelFactory.UpdateOrderTypeDetails(model, isReset);
                    if (isReset == true)
                    {
                        var obj = new
                        {
                            response = await PrepareResponse(
                                 orderid: model.Id,
                   statuscode: 200,
                   html: await RenderViewComponentToStringAsync("CustomOrder_OrderTypeSection", new { orderId = model.Id }),
                   message: "",
                   bindSectionId: "UpdateOrderType"
              )
                        };
                        return Json(obj);
                    }
                    else
                    {
                        return Json(new
                        {
                            response = await PrepareResponse(
                               orderid: model.Id,
                              statuscode: 200,
                              html: "",
                              message: "",
                              bindSectionId: ""
                         )
                        });
                    }
                }
                else
                {
                    string messages = string.Join("; ", ModelState.Values
                                         .SelectMany(x => x.Errors)
                                         .Select(x => x.ErrorMessage));
                    return Json(new
                    {
                        response = await PrepareResponse(
                     orderid: model.Id,
                     statuscode: 500,
                     message: messages,
                     bindSectionId: ""
                )
                    });
                }
            }
            catch (Exception exp)
            {
                await this._loggerService.ErrorAsync($"Failed to Save Order Type Details for OrderId {model.Id} function SaveOrderTypeDetails", exp);
                return Json(new
                {
                    response = await PrepareResponse(
                               orderid: model.Id,
                      statuscode: 500,
                      message: await _localizationService.GetResourceAsync("CustomOrder.Failed.Save.OrderTypeSection"),
                      bindSectionId: ""
                 )
                });
            }
        }


        #endregion

        #region Customer Step

        [HttpPost]
        public virtual async Task<IActionResult> UpdateCustomer(int customerId, int orderId)
        {
            try
            {
                await _customOrderModelFactory.UpdateCustomer(customerId, orderId);
                return Json(new
                {
                    response = await PrepareResponse(
                       orderid: orderId,
                      statuscode: 200,
                      html: await RenderViewComponentToStringAsync("CustomOrder_AddressSection",
                      new
                      {
                          orderId = orderId,
                          customerId = 0
                      }),
                      message: "",
                      bindSectionId: "UpdateCustomerAddress"
                 )
                });
            }
            catch (Exception exp)
            {
                await this._loggerService.ErrorAsync($"Failed to Update Customer for OrderId {orderId} function UpdateCustomer", exp);
                return Json(new
                {
                    response = await PrepareResponse(
                        orderid: orderId,
                      statuscode: 500,
                      message: await _localizationService.GetResourceAsync("CustomOrder.Failed.Update.Customer"),
                      bindSectionId: "UpdateCustomerAddress"
                 )
                });
            }
        }


        //public virtual async Task<IActionResult> GetCustomOrder_OrderTypeSection(int orderId)
        //{
        //    try
        //    {
        //        return Json(new
        //        {
        //            response = PrepareResponse(
        //              statuscode: 200,
        //              html:await RenderViewComponentToStringAsync("CustomOrder_OrderTypeSection", new { orderId = orderId }),
        //              message: "",
        //              bindSectionId: "customorder_addresses_section"
        //         )
        //        });
        //    }
        //    catch (Exception exp)
        //    {
        //        await this._loggerService.ErrorAsync($"Failed to get OrderTypeSection for OrderId {orderId} function Searchcustomer", exp);
        //        return Json(new
        //        {
        //            response = PrepareResponse(
        //              statuscode: 500,
        //              message: await _localizationService.GetResourceAsync("CustomOrder.Failed.OrderTypeSection"),
        //              bindSectionId: "GetCustomOrder_OrderTypeSection"
        //         )
        //        });
        //    }
        //}
        public virtual async Task<IActionResult> CreateUpdateCustomer(int customerId, int orderId)
        {
            try
            {
                return Json(new
                {
                    response = await PrepareResponse(
                        orderid: orderId,
                      statuscode: 200,
                      html: await RenderViewComponentToStringAsync("CustomOrder_CustomerSection", new { customerId = customerId }),
                      message: "",
                      bindSectionId: "CustomOrder_CustomerSection",
                      isPopup: true,
                      showContainer: true
                 )
                });
            }
            catch (Exception exp)
            {
                await this._loggerService.ErrorAsync($"Failed to get customer for customerid {customerId} function CreateUpdateCustomer", exp);
                return Json(new
                {
                    response = await PrepareResponse(
                                orderid: orderId,
                      statuscode: 500,
                      message: await _localizationService.GetResourceAsync("CustomOrder.Failed.GetCustomer"),
                      bindSectionId: "CustomOrder_CustomerSection"
                 )
                });
            }
        }


        [HttpPost]
        public virtual async Task<IActionResult> CreateUpdateCustomer(CustomOrderCustomerSectionModel model, IFormCollection form)
        {
            int.TryParse(form["orderId"], out var orderId);
            bool.TryParse(form["renderCartSummary"], out bool renderCartSummary);
            try
            {

                if (orderId != 0)
                {
                    if (ModelState.IsValid)
                    {
                        model.Email = (model.Email ?? "").Trim();
                        var customer = new Customer();
                        if (model.Id > 0)
                            customer = await _customerService.GetCustomerByIdAsync(model.Id);
                        else
                            customer = await _customerService.InsertGuestCustomerAsync();

                        var isRegistered = await _customerService.IsRegisteredAsync(customer);
                        if (_customerSettings.UsernamesEnabled && _customerSettings.AllowUsersToChangeUsernames && isRegistered)
                        {
                            var userName = model.Email;
                            if (string.IsNullOrEmpty(customer.Username) || !customer.Username.Equals(userName, StringComparison.InvariantCultureIgnoreCase))
                                await _customerRegistrationService.SetUsernameAsync(customer, userName);
                        }
                        //email
                        var email = model.Email;
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
                        await _genericAttributeService.SaveAttributeAsync(customer, "Email", model.Email);
                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.PhoneAttribute, model.ShippingAddress?.PhoneNumber);
                        //form fields
                        if (_customerSettings.FirstNameEnabled)
                            await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.FirstNameAttribute, model.FirstName);
                        if (_customerSettings.LastNameEnabled)
                            await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.LastNameAttribute, model.LastName);
                        if (_customerSettings.StreetAddress2Enabled)
                            await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.ZipPostalCodeAttribute, model.ShippingAddress?.ZipPostalCode);
                        if (_customerSettings.PhoneEnabled)
                            await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.PhoneAttribute, model.ShippingAddress?.PhoneNumber);


                        // Save Shipping Address
                        if (model.ShippingAddress.Id > 0)
                        {
                            //existing address
                            var shippingAddress = model.ShippingAddress;
                            var address = shippingAddress.ToEntity();
                            address.Email = model.Email;
                            address.FirstName = model.FirstName;
                            address.LastName = model.LastName;
                            await _addressService.UpdateAddressAsync(address);

                            // Save Billing
                            if (customer?.ShippingAddressId == null)
                                customer.ShippingAddressId = address.Id;
                            if (customer?.BillingAddressId == null || customer?.ShippingAddressId == null)
                                await _customerService.UpdateCustomerAsync(customer);
                        }
                        else
                        {
                            var newAddress = model.ShippingAddress;
                            var address = _addressService.FindAddress((await _customerService.GetAddressesByCustomerIdAsync(customer.Id)).ToList(),
                        model.FirstName, model.LastName, newAddress.PhoneNumber,
                        model.Email, newAddress.FaxNumber, newAddress.Company,
                        newAddress.Address1, newAddress.Address2, newAddress.City,
                        newAddress.County, newAddress.StateProvinceId, newAddress.ZipPostalCode,
                        newAddress.CountryId, "");
                            if (address == null)
                            {
                                address = newAddress.ToEntity();
                                address.FirstName = model.FirstName;
                                address.LastName = model.LastName;
                                address.Email = model.Email;
                                address.CreatedOnUtc = DateTime.UtcNow;

                                await _addressService.InsertAddressAsync(address);

                                await _customerService.InsertCustomerAddressAsync(customer, address);
                            }
                             (customer).ShippingAddressId = address.Id;

                            await _customerService.UpdateCustomerAsync(customer);

                        }


                        // Save Billing Address
                        if (model.BillingAddress.Id > 0)
                        {
                            //existing address
                            var billingAddress = model.BillingAddress;
                            var address = billingAddress.ToEntity();
                            address.Email = model.Email;
                            address.FirstName = model.FirstName;
                            address.LastName = model.LastName;

                            await _addressService.UpdateAddressAsync(address);

                            // Save Billing
                            if (customer?.BillingAddressId == null)
                                customer.BillingAddressId = address.Id;
                            if (customer?.BillingAddressId == null || customer?.BillingAddressId == null)
                                await _customerService.UpdateCustomerAsync(customer);
                        }
                        else
                        {
                            var newAddress = model.BillingAddress;
                            var address = _addressService.FindAddress((await _customerService.GetAddressesByCustomerIdAsync(customer.Id)).ToList(),
                        model.FirstName, model.LastName, newAddress.PhoneNumber,
                        model.Email, newAddress.FaxNumber, newAddress.Company,
                        newAddress.Address1, newAddress.Address2, newAddress.City,
                        newAddress.County, newAddress.StateProvinceId, newAddress.ZipPostalCode,
                        newAddress.CountryId, "");
                            if (address == null)
                            {
                                address = newAddress.ToEntity();
                                address.FirstName = model.FirstName;
                                address.LastName = model.LastName;
                                address.Email = model.Email;
                                address.CustomAttributes = "";
                                address.CreatedOnUtc = DateTime.UtcNow;

                                await _addressService.InsertAddressAsync(address);

                                await _customerService.InsertCustomerAddressAsync(customer, address);
                            }
                             (customer).BillingAddressId = address.Id;
                            await _customerService.UpdateCustomerAsync(customer);

                        }

                        await _customOrderModelFactory.UpdateCustomer(customer.Id, orderId);

                        await _customOrderModelFactory.UpdateOrderTotal(orderId, false);
                        return Json(new
                        {
                            response = await PrepareResponse(
                                        orderid: orderId,
                            statuscode: 200,
                            html: await RenderViewComponentToStringAsync("CustomOrder_AddressSection",
                            new
                            {
                                orderId = orderId,
                                customerId = 0
                            }),
                              message: "",
                              goto_section: "UpdateCustomerAddress",
                              closeContainer: true,
                              isPopup: true,
                              bindSectionId: "CustomOrder_CustomerSection"
                           )
                        });

                    }
                    else
                    {
                        string messages = string.Join("; ", ModelState.Values
                                             .SelectMany(x => x.Errors)
                                             .Select(x => x.ErrorMessage));
                        return Json(new
                        {
                            response = await PrepareResponse(
                                        orderid: orderId,
                         statuscode: 500,
                         message: messages,
                         bindSectionId: "CustomOrder_CustomerSection"
                    )
                        });
                    }
                }
                else
                {
                    await this._loggerService.ErrorAsync($"Failed to get Order Id from post Data function CreateUpdateCustomer", null);
                    return Json(new
                    {
                        response = await PrepareResponse(
                                    orderid: orderId,
                      statuscode: 500,
                      message: await _localizationService.GetResourceAsync("CustomOrder.Failed.OrderId"),
                      bindSectionId: "CustomOrder_CustomerSection"
                 )
                    });
                }
            }
            catch (Exception exp)
            {
                return Json(new
                {
                    response = await PrepareResponse(
                                     orderid: orderId,
                      statuscode: 500,
                      message: exp.Message,
                      bindSectionId: "CustomOrder_CustomerSection"
                 )
                });
            }
        }


        public virtual async Task<IActionResult> Searchcustomer(string searchTerm)
        {
            try
            {
                return Json(new
                {
                    response = await PrepareResponse(
                        orderid: 0,
                      statuscode: 200,
                      html: "",
                      json: JsonConvert.SerializeObject(await _customOrderModelFactory.PrepareCustomerListModelAsync(searchTerm)),
                      message: "",
                      bindSectionId: "customorder_addresses_section"
                 )
                });
            }
            catch (Exception exp)
            {
                await this._loggerService.ErrorAsync($"Failed to get Address for SearchTerm {searchTerm} function Searchcustomer", exp);
                return Json(new
                {
                    response = await PrepareResponse(
                      orderid: 0,
                      statuscode: 500,
                      message: await _localizationService.GetResourceAsync("CustomOrder.Failed.Searchcustomer"),
                      bindSectionId: "customorder_customers_section"
                 )
                });
            }
        }


        [HttpPost]
        public virtual async Task<IActionResult> GetCustomerDetailsFromZoho(string email, string phone)
        {
            email = email == null ? "" : email;
            phone = phone == null ? "" : phone;
            var obj = await _zohoService.GetContactDetailsFromZoho(email, phone);
            if (obj == null)
            {
                return Json(new
                {
                    response = await PrepareResponse(
                   orderid: 0,
                   statuscode: 204,
                   message: await _localizationService.GetResourceAsync("CustomOrder.Zoho.Contact.NotFound"),
                   bindSectionId: ""
              )
                });
            }
            else
            {
                return Json(new
                {
                    response = await PrepareResponse(
                        contactDetails: obj,
                       orderid: 0,
          statuscode: 200,
          message: await _localizationService.GetResourceAsync("CustomOrder.Zoho.Contact.NotFound"),
          bindSectionId: ""
     )
                });
            }
        }


        #endregion

        #region Prouduct Step

        public virtual async Task<IActionResult> SearchProduts(string searchTerm)
        {
            try
            {
                return Json(new
                {
                    response = await PrepareResponse(
                      orderid: 0,
                      statuscode: 200,
                      html: await RenderPartialViewToStringAsync("_customorder_productsearch", await _customOrderModelFactory.PrepareProductListModelAsync(searchTerm)),
                      message: "",
                      bindSectionId: "SearchProduct_section",
                      isPopup: true,
                      showContainer: true
                 )
                });
            }
            catch (Exception exp)
            {
                await this._loggerService.ErrorAsync($"Failed to SearchProduts for SearchTerm {searchTerm} function SearchProduts", exp);
                return Json(new
                {
                    response = await PrepareResponse(
                      orderid: 0,
                      statuscode: 500,
                      message: await _localizationService.GetResourceAsync("CustomOrder.Failed.SearchProduts"),
                      bindSectionId: "SearchProduct_section"
                 )
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddUpdateCartItems(List<ItemModel> Items, IFormCollection form)
        {
            int orderId = 0;
            try
            {
                bool isItemNotes = false;
                string test = form["orderId"];
                int.TryParse(form["orderId"], out orderId);

                bool.TryParse(form["isItemNotes"], out isItemNotes);

                bool.TryParse(form["renderCartSummary"], out bool renderCartSummary);

                string imagepath = "";
                if (form["items[0].base64"].ToString() != null && form["items[0].base64"].ToString() != "" && form["items[0].base64"].ToString() != "undefined")
                {
                    var path = _nopFileProvider.MapPath("/wwwroot/images/customorder/");
                    var imagename = orderId + "_" + (Items.Count > 0 ? Items.FirstOrDefault().Id : "1") + DateTime.Now.Ticks + form["items[0].ext"].ToString();
                    imagepath = "/images/customorder/" + imagename;
                    string base64 = form["items[0].base64"].ToString();
                    base64 = base64.Substring(base64.IndexOf(",") + 1);
                    System.IO.File.WriteAllBytes(path + imagename, Convert.FromBase64String(base64));
                }
                if (Items.Count > 0)
                {
                    if (imagepath != "")
                        Items[0].Notes = Items[0].Notes + "<br/>" + "<img style=\"width:50px;height:50px\" src=\"" + (await _storeContext.GetCurrentStoreAsync()).Url + imagepath.TrimStart('/') + "\"/>";
                }
                // save File


                // end
                foreach (var item in Items)
                {
                    if (!string.IsNullOrEmpty(item.Attributes))
                    {
                        int[] attributesArray = item.Attributes
        .Split(',')
        .Select(int.Parse)
        .ToArray();
                        foreach (var combination in await _productAttributeService.GetAllProductAttributeCombinationsAsync(item.ProductId))
                        {
                            var _attributeValues = await _productAttributeParser.ParseProductAttributeValuesAsync(combination.AttributesXml);

                            if (attributesArray.Length == _attributeValues.Count &&
                                 attributesArray.All(id => _attributeValues.Any(attr => attr.Id == id)))
                            {
                                if ((combination.OverriddenOldPrice ?? combination.OverriddenPrice ?? 0) > 0)
                                {
                                    item.Price = (combination.OverriddenOldPrice ?? combination.OverriddenPrice ?? 0).ToString();
                                }
                                break;
                            }
                        }
                    }
                }

                await _customOrderModelFactory.AddUpdateCartItems(Items, orderId, isItemNotes);
                string orderSummaryHtml = "";
                if (renderCartSummary)
                    orderSummaryHtml = await RenderViewComponentToStringAsync("CustomOrder_OrderSummary",
                       new
                       {
                           orderId = orderId
                       });
                return Json(new
                {
                    response = await PrepareResponse(
                      orderid: orderId,
                      statuscode: 200,
                       html: await RenderViewComponentToStringAsync("CustomOrder_Items",
                        new
                        {
                            orderId = orderId
                        }),
                      message: "",
                      goto_section: "Items_section",
                      isPopup: true,
                      showContainer: false,
                      closeContainer: true,
                      bindSectionId: "SearchProduct_section",
                      orderSummaryHtml: orderSummaryHtml
                 )
                });
            }
            catch (Exception exp)
            {
                await this._loggerService.ErrorAsync($"Failed to AddUpdateCartItems for orderId {orderId} function AddUpdateCartItems", exp);
                return Json(new
                {
                    response = await PrepareResponse(
                               orderid: orderId,
                      statuscode: 500,
                      message: await _localizationService.GetResourceAsync("CustomOrder.Failed.AddItems"),
                      goto_section: "Items_section"
                 )
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCartItems(ItemModel item, int orderId)
        {
            try
            {

                await _customOrderModelFactory.DeleteCartItem(item, orderId);
                return Json(new
                {
                    response = await PrepareResponse(
                               orderid: orderId,
                      statuscode: 200,
                       html: await RenderViewComponentToStringAsync("CustomOrder_Items",
                        new
                        {
                            orderId = orderId
                        }),
                      message: "",
                      bindSectionId: "Items_section",
                      isPopup: true,
                      showContainer: false,
                      closeContainer: true
                 )
                });
            }
            catch (Exception exp)
            {
                await this._loggerService.ErrorAsync($"Failed to AddUpdateCartItems for orderId {orderId} function AddUpdateCartItems", exp);
                return Json(new
                {
                    response = await PrepareResponse(
                               orderid: orderId,
                      statuscode: 500,
                      message: await _localizationService.GetResourceAsync("CustomOrder.Failed.AddItems"),
                      bindSectionId: "Items_section"
                 )
                });
            }
        }


        #endregion

        #region order Summary

        public async Task<IActionResult> UpdateOrder(CustomOrderModel model, string orderTypeUpdate)
        {
            Enum.TryParse<OrderTypeUpdate>(orderTypeUpdate, out OrderTypeUpdate updateType);
            try
            {
                await _customOrderModelFactory.UpdateOrderSections(model, updateType);
                if (updateType != OrderTypeUpdate.PartialOrderPaymentLink)
                {
                    return Json(new
                    {
                        response = await PrepareResponse(
                            orderid: model.Id,
                          statuscode: 200,
                           html: await RenderViewComponentToStringAsync("CustomOrder_OrderSummary",
                            new
                            {
                                orderId = model.Id
                            }),
                          message: "",
                          goto_section: "orderSummary_section",
                           orderDetails: await RenderViewComponentToStringAsync("CustomOrderStatusLog", new
                           {
                               orderId = model.Id
                           }),
                           notesLog: updateType == OrderTypeUpdate.Notes ? await RenderViewComponentToStringAsync("CustomOrderNotesLog", new
                           {
                               orderId = model.Id
                           }) : "",
                          isPopup: true,
                          showContainer: false,
                          closeContainer: true,
                          bindSectionId: "orderSummary_" + updateType.ToString(),
                          notificationMessage: updateType ==
                          OrderTypeUpdate.StatusUpdate ?
                          (model.OrderStatus == OrderStatus.SavedDraft.ToString() ?
                          await _localizationService.GetResourceAsync("CustomOrder.Message.StatusUpdate.Draft")
                          : (model.OrderStatus == OrderStatus.InvoiceSent.ToString() ?
                             await _localizationService.GetResourceAsync("CustomOrder.Message.StatusUpdate.InvoiceSent") : null)) : null
                     )
                    });
                }
                else
                {
                    return Json(new
                    {
                        response = await PrepareResponse(
                       orderid: 0,
                     statuscode: 200,
                      html: "",
                     message: "",
                    notificationMessage:
                     await _localizationService.GetResourceAsync("CustomOrder.Message.PartialOrderLinkNotification")
                )
                    });
                }
            }
            catch (Exception exp)
            {
                await this._loggerService.ErrorAsync($"Failed to AddUpdateCartItems for orderId {model.Id} function AddUpdateCartItems", exp);
                return Json(new
                {
                    response = await PrepareResponse(
                               orderid: model.Id,
                      statuscode: 500,
                      message: await _localizationService.GetResourceAsync("CustomOrder.Failed.UpdateOrder"),
                      goto_section: "orderSummary_section"
                 )
                });
            }

        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment(string paymentMethod, int orderId, IFormCollection form)
        {
            try
            {
                await this._customOrderModelFactory.UpdateOrderTotal(orderId);
                var order = await _customOrderService.GetById(orderId);
                var orderTypes = await _customOrderService.GetOrderTypes();
                var orderType = orderTypes.Where(t => t.Id == order.OrderTypeId).FirstOrDefault();
                if (orderType != null && orderType.Name == OrderTypes.AlreadyPaid.ToString())
                {
                    if (order.SubOrderTypeId != null && order.SubOrderTypeId > 0)
                    {
                        orderType = orderTypes.Where(t => t.Id == order.SubOrderTypeId).FirstOrDefault();
                        if (string.Equals(orderType.Name, "Replacement Order", StringComparison.InvariantCultureIgnoreCase) && string.IsNullOrEmpty(order.PurchaseOrderNumber))
                        {
                            return Json(new
                            {
                                response = await PrepareResponse(
                  orderid: 0,
                statuscode: 500,
                 html: "",
                message: await _localizationService.GetResourceAsync("CustomOrder.Message.PurchaseOrderNumber.Required"),

               notificationMessage:
                await _localizationService.GetResourceAsync("CustomOrder.Message.PurchaseOrderNumber.Required")
               )
                            });
                        }
                    }
                }


                if ((order.LiveOrderNumber != null && order.LiveOrderNumber > 0 && (order.AlreadyFee == null || order.AlreadyFee == 0))
                         || (order.LiveOrderNumber != null && order.LiveOrderNumber > 0 && order.AlreadyFee != null && order.AlreadyFee > 0
                    && order.FullPaid))
                    throw new Exception(await _localizationService.GetResourceAsync("CustomOrder.Message.Order.AlreadyPaid"));
                if (order != null)
                {
                    var customer = new Customer();
                    if (order.CustomerId != null)
                        customer = await _customerService.GetCustomerByIdAsync(Convert.ToInt32(order.CustomerId));
                    var _paymentMethod = await _paymentPluginManager
                        .LoadPluginBySystemNameAsync(paymentMethod, customer, (await _storeContext.GetCurrentStoreAsync()).Id)
                        ?? throw new Exception("Payment method is not selected");

                    var warnings = await _paymentMethod.ValidatePaymentFormAsync(form);
                    string error = "";
                    foreach (var warning in warnings)
                        error = error + warning + @"<br\>";
                    if (error.Length > 0)
                        throw new Exception(error);
                    var filterByCountryId = 0;
                    if (_addressSettings.CountryEnabled)
                    {
                        filterByCountryId = (await _customerService.GetCustomerBillingAddressAsync(customer))?.CountryId ?? 0;
                    }

                    //get payment info
                    var paymentInfo = await _paymentMethod.GetPaymentInfoAsync(form);
                    //set previous order GUID (if exists)
                    _paymentService.GenerateOrderGuid(paymentInfo);
                    paymentInfo.StoreId = (await _storeContext.GetCurrentStoreAsync()).Id;
                    paymentInfo.CustomerId = customer.Id;
                    paymentInfo.PaymentMethodSystemName = paymentMethod;



                    return await ConfirmOrder(order, customer, paymentMethod, paymentInfo, filterByCountryId, _paymentMethod);
                }
                else
                {
                    return Json(new
                    {
                        response = await PrepareResponse(
                     orderid: 0,
                   statuscode: 500,
                    html: "",
                   message: "",

                  notificationMessage:
                   await _localizationService.GetResourceAsync("CustomOrder.Message.Payment.OrderNotValid")
                  )
                    });

                }
            }
            catch (Exception exp)
            {
                return Json(new
                {
                    response = await PrepareResponse(
                 orderid: 0,
               statuscode: 500,
                html: "",
               message: exp.Message,
              notificationMessage:
               exp.Message
              )
                });
            }
        }


        [HttpPost]
        public async Task<IActionResult> ProcessPartialPayment(int orderId)
        {
            try
            {
                await this._customOrderModelFactory.UpdateOrderTotal(orderId);
                var order = await _customOrderService.GetById(orderId);
                if ((order.LiveOrderNumber == null && order.LiveOrderNumber == 0) || (order.AlreadyFee == null || order.AlreadyFee == 0) || !order.FullPaid)
                    throw new Exception(await _localizationService.GetResourceAsync("CustomOrder.Message.Order.Not.PartialOrder"));
                if (order != null)
                {
                    var customer = new Customer();
                    if (order.CustomerId != null)
                        customer = await _customerService.GetCustomerByIdAsync(Convert.ToInt32(order.CustomerId));

                    var filterByCountryId = 0;
                    if (_addressSettings.CountryEnabled)
                        filterByCountryId = (await _customerService.GetCustomerBillingAddressAsync(customer))?.CountryId ?? 0;

                    var initialOrder = await _orderService.GetOrderByIdAsync(Convert.ToInt32(order.LiveOrderNumber));
                    var processPaymentRequest = new ProcessPaymentRequest
                    {
                        StoreId = initialOrder.StoreId,
                        CustomerId = customer.Id,
                        OrderGuid = Guid.NewGuid(),
                        InitialOrder = initialOrder,
                        CustomValues = _paymentService.DeserializeCustomValues(initialOrder)
                    };
                    var _paymentMethod = await _paymentPluginManager
                       .LoadPluginBySystemNameAsync(initialOrder.PaymentMethodSystemName, customer, (await _storeContext.GetCurrentStoreAsync()).Id)
                       ?? throw new Exception("Payment method is not selected");

                    return await ConfirmOrder(order, customer, initialOrder.PaymentMethodSystemName, processPaymentRequest, filterByCountryId, _paymentMethod, 0, true);
                }
                else
                {
                    return Json(new
                    {
                        response = await PrepareResponse(
                     orderid: 0,
                   statuscode: 500,
                    html: "",
                   message: "",

                  notificationMessage:
                   await _localizationService.GetResourceAsync("CustomOrder.Message.Payment.OrderNotValid")
                  )
                    });

                }
            }
            catch (Exception exp)
            {
                return Json(new
                {
                    response = await PrepareResponse(
                 orderid: 0,
               statuscode: 500,
                html: "",
               message: exp.Message,
              notificationMessage:
               exp.Message
              )
                });
            }
        }

        #endregion


        #endregion

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> Archive(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                await AccessDeniedDataTablesJson();

            await _customOrderModelFactory.ArchiveCustomOrderAsync(id);
            return Ok();
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> RestoreOrder(int Id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                await AccessDeniedDataTablesJson();

            await _customOrderModelFactory.RestoreCustomOrderAsync(Id);
            return Ok();
        }

        #endregion

        #region utilities

        public async Task<AjaxReponseModel> PrepareResponse(int orderid, int statuscode = 200, string message = "", string html = "", string json = "",
            string goto_section = "", string bindSectionId = "", bool closeContainer = false, bool isPopup = false, bool showContainer = false,
            string orderSummaryHtml = "",
            string notificationMessage = "", bool redirect = false, Zoho contactDetails = null, string receipt = "", string orderDetails = "", string notesLog = "")
        {
            AjaxReponseModel model = new AjaxReponseModel();
            model.closeContainer = closeContainer;
            model.receipt = receipt;
            model.orderDetails = orderDetails;
            model.showContainer = showContainer;
            model.statuscode = statuscode;
            model.isPopup = isPopup;
            model.message = message;
            model.html = html;
            model.json = json;
            model.bindSectionId = bindSectionId;
            model.goto_section = goto_section;
            model.redirect = redirect;
            model.ContactDetails = contactDetails;
            model.NotesLog = notesLog;
            if (orderid != 0 && statuscode == 200)
            {
                var order = await _customOrderService.GetById(orderid);
                if (order != null)
                {
                    model.EnableDisableSections = true;
                    model.EnableCustomerSearch = await _customOrderModelFactory.CustomerSearchEnableDisable(order);
                    model.EnableProductSearch = await _customOrderModelFactory.ProductSearchEnableDisable(order);
                    model.EnableCartSummary = !(await _customOrderModelFactory.IsOrderPaid(order));
                    model.DisplayCartSummary = (await this._customOrderService.GetOrderItems(order.Id)).Count > 0 ? true : false;
                    if (model.DisplayCartSummary != null && Convert.ToBoolean(model.DisplayCartSummary))
                        model.orderSummary = await this._customOrderModelFactory.PrepareOderSummaryModel(orderid);
                }
                else
                {
                    model.EnableCustomerSearch = null;
                    model.EnableProductSearch = null;
                    model.EnableCartSummary = null;
                    model.DisplayCartSummary = null;
                }

            }
            else
            {
                model.EnableCustomerSearch = null;
                model.EnableProductSearch = null;
                model.EnableCartSummary = null;
                model.DisplayCartSummary = null;
            }
            model.orderSummaryHtml = orderSummaryHtml;
            model.notificationMessage = notificationMessage;

            return model;

        }

        public virtual async Task<IActionResult> ConfirmOrder(Nop.MWT.Nop.Core.Domain.CustomOrders.CustomOrder order,
            Customer customer, string paymentMethodName,
            ProcessPaymentRequest processPaymentRequest, int filterByCountryId, IPaymentMethod paymentMethod, int customerId = 0, bool chargeFromInitialaOrder = false)
        {
            try
            {
                var orderSummary = await this._customOrderModelFactory.PrepareOderSummaryModel(order.Id);
                int refOrderno = 0;
                bool saveOrderDetails = true;
                if (order.LiveOrderNumber != null && order.LiveOrderNumber != 0 && order.AlreadyFee != null && order.AlreadyFee > 0)
                {
                    saveOrderDetails = false;
                    refOrderno = Convert.ToInt32(order.LiveOrderNumber);
                }
                (var placeOrderResult, var paymentResponse) = await _orderProcessingService.CustomPlaceOrderAsync(processPaymentRequest, order, orderSummary, saveOrderDetails, refOrderno, chargeFromInitialaOrder);
                if (placeOrderResult.Success)
                {
                    var postProcessPaymentRequest = new PostProcessPaymentRequest
                    {
                        Order = placeOrderResult.PlacedOrder
                    };
                    if (paymentMethod == null)
                        //payment method could be null if order total is 0
                        //success
                        throw new Exception("Order Total 0");

                    var _items = await _customOrderService.GetOrderItems(order.Id);

                    foreach (var item in _items)

                    {
                        var product = await _productService.GetProductByIdAsync(item.ProductId);
                        if (product != null)
                        {
                            product.NoOfSales = product.NoOfSales + item.Quantity;
                            await _productService.UpdateProductAsync(product);
                        }

                    }



                    var orderStatuses = await _customOrderService.GetOrderStatuses();
                    var paidStatus = orderStatuses.Where(m => m.Name == OrderStatus.Paid.ToString()).FirstOrDefault();


                    decimal paidAmount = placeOrderResult.PlacedOrder.OrderTotal;
                    bool fullPaid = true;
                    var orderEntity = placeOrderResult.PlacedOrder;
                    if (orderSummary.OrderType == OrderTypes.CustomOrder.ToString() && order.AlreadyFee != null && order.AlreadyFee > 0)
                    {
                        if (paidStatus != null && order.StatusId != paidStatus.Id)
                        {
                            fullPaid = false;
                            decimal.TryParse(orderSummary.OrderTotal, NumberStyles.Currency,
                                      CultureInfo.CurrentCulture.NumberFormat, out decimal orderTotal);


                            orderEntity.OrderTotal = orderTotal;

                        }
                        else
                        {
                            await _manageService.SyncPendingOrderPartialPayment(orderEntity.Id, paymentResponse.CaptureTransactionId, processPaymentRequest.OrderTotal, DateTime.Now, paymentMethodName);
                        }
                    }
                    // update Parent Order ref
                    if (order.ParentOrderID > 0)
                        orderEntity.ParentOrderID = order.ParentOrderID;
                    string email = customer?.Email;
                    if (string.IsNullOrEmpty(email))
                    {
                        email = (await _addressService.GetAddressByIdAsync(orderEntity.BillingAddressId))?.Email;
                        if (string.IsNullOrEmpty(email) && orderEntity.ShippingAddressId.HasValue)
                            email = (await _addressService.GetAddressByIdAsync(Convert.ToInt32(orderEntity.ShippingAddressId)))?.Email;
                    }

                    orderEntity.CustomerEmail = email;
                    var orderTypes = await _customOrderService.GetOrderTypes();
                    var orderType = orderTypes.Where(t => t.Id == order.OrderTypeId).FirstOrDefault();
                    if (orderType != null && orderType.Name != OrderTypes.CustomOrder.ToString())
                    {
                        orderEntity.OrderStatus = Core.Domain.Orders.OrderStatus.Processing;
                        orderEntity.PaymentStatus = Core.Domain.Payments.PaymentStatus.Paid;
                    }
                    await this._orderService.UpdateOrderAsync(orderEntity);
                    // end 
                    // Update OrderStatus

                    if (paidStatus != null)
                        order.StatusId = paidStatus.Id;

                    //

                    order.LiveOrderNumber = placeOrderResult.PlacedOrder.Id;
                    order.FullPaid = fullPaid;
                    if (order.CreatedOn == null)
                        order.CreatedOn = DateTime.Now;

                    #region Order Zoho Lead

                    var items = await _orderService.GetOrderItemsAsync(order.Id);
                    string description = "";
                    foreach (var item in items)
                    {
                        var product = await _productService.GetProductByIdAsync(item.ProductId);
                        description += "ProductId:" + item.ProductId + ";SKU:" + product?.Sku ?? "" + "|";
                    }

                    var iPAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress == null ? "" : _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
                    var glclidCookie = _httpContextAccessor.HttpContext.Request.Cookies["gclid"];

                    if ((order.SubOrderTypeId ?? 0) == 0 && (orderSummary.OrderType == OrderTypes.CustomOrder.ToString() || orderSummary.OrderType == OrderTypes.AlreadyPaid.ToString()))
                    {
                        order.ZohoPotentialId = await _zohoService.CreateUpdateOrderContactPotential(order.LiveOrderNumber ?? order.Id, order.ZohoPotentialId, customer, MWT.Nop.Core.Domain.CustomOrders.OrderStatus.Paid.ToString(), description, iPAddress, glclidCookie, (order.SubTotal ?? 0) + (order.TotalDiscount ?? 0), order.CreatedBy, $"{_storeContext.GetCurrentStore().Url}checkoutCustomOrder?orderid={order.Id}&customerid={order.CustomerId}", true);
                    }

                    #endregion

                    await _customOrderService.UpdateAsync(order);

                    if (orderSummary.OrderType != OrderTypes.HouzzOrder.ToString())
                        await _workflowMessageService
                 .CustomOrder_SendCustomerNotificationAsync(order, placeOrderResult.PlacedOrder.CustomerLanguageId);

                    //

                    await this._customOrderService.InsertOrderStatusLogAsync(new CustomorderOrderStatusLog()
                    {
                        CreatedOn = DateTime.UtcNow,
                        OrderId = order.Id,
                        StatusId = paidStatus != null ? paidStatus.Id : 0,
                        UserId = customerId == 0 ? (await _workContext.GetCurrentCustomerAsync()).Id : customerId,
                        AmountPaid = paidAmount,
                        PaymentResponse = JsonConvert.SerializeObject(paymentResponse)
                    });

                    // end



                    var content = await this._workflowMessageService.CustomOrderReceiptContentAsync(order, (await this._workContext.GetWorkingLanguageAsync()).Id);
                    content = "<div data-orderid=\"" + placeOrderResult.PlacedOrder.Id + "\">" + content + "</div>";
                    return Json(new
                    {
                        response = await PrepareResponse(
             orderid: 0,
           statuscode: 200,
              redirect: true,
            html: "",
           message: "",
           receipt: content,
          notificationMessage:
           await _localizationService.GetResourceAsync("CustomOrder.Message.OrderPlacedSuccessfully")
          )
                    });
                }
                else
                {
                    var exception = "";
                    foreach (var error in placeOrderResult.Errors)
                        exception = exception + error + @"<br\>";
                    throw new Exception(exception);
                }
            }
            catch (Exception exp)
            {
                return Json(new
                {
                    response = await PrepareResponse(
           orderid: 0,
         statuscode: 500,
          html: "",
         message: exp.Message,
        notificationMessage:
         exp.Message
        )
                });
            }
        }

        #endregion
    }
}