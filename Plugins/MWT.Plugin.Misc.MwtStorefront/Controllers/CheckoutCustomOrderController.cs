using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Domain.CustomOrders;
using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Service.Zoho;
using MWT.Nop.Core.Services.Customizations.CustomOrders;
using MWT.Nop.Core.Services.Manage;
using MWT.Nop.Core.Services.Message;
using MWT.Nop.Core.Services.Orders;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Factories;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Common;
using MWT.Plugin.Misc.MwtStorefront.Models.CustomOrder;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Logging;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Controllers
{
    public class CheckoutCustomOrderController : BasePublicController
    {
        #region fields

        private readonly ICustomOrderModelFactory _customOrderModelFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICustomOrderService _customOrderService;
        private string cookieName = "customorder_authenticated";
        private readonly IWebHelper _webHelper;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerService _customerService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IStoreContext _storeContext;
        private readonly AddressSettings _addressSettings;
        private readonly IPaymentService _paymentService;
        private readonly IOrderProcessingExtendedService _orderProcessingService;
        private readonly IProductExtendedService _productService;
        private readonly IWorkContext _workContext;
        private readonly IOrderExtendedService _orderService;
        private readonly ICustomWorkflowMessageService _workflowMessageService;
        private readonly IAddressService _addressService;
        private readonly IZohoService _zohoService;
        private readonly IManageService _manageService;
        private readonly ILogger _logger;
        #endregion

        #region Ctor

        public CheckoutCustomOrderController(
            ICustomOrderModelFactory customOrderModelFactory,
            IHttpContextAccessor httpContextAccessor, ICustomOrderService customOrderService,
            IWebHelper webHelper,
            ICustomerService customerService,
        IPaymentPluginManager paymentPluginManager,
         IStoreContext storeContext,
         AddressSettings addressSettings,
      IPaymentService paymentService,
      IOrderProcessingExtendedService orderProcessingService,
       IProductExtendedService productService,
        IWorkContext workContext,
        IOrderExtendedService orderService,
        ILocalizationService localizationService,
        ICustomWorkflowMessageService workflowMessageServic,
        IAddressService addressService,
        IZohoService zohoService,
        IManageService manageService,
        ILogger logger)
        {
            this._customOrderModelFactory = customOrderModelFactory;
            this._httpContextAccessor = httpContextAccessor;
            this._customOrderService = customOrderService;
            this._webHelper = webHelper;
            this._customerService = customerService;
            this._addressSettings = addressSettings;
            this._paymentPluginManager = paymentPluginManager;
            this._paymentService = paymentService;
            this._storeContext = storeContext;
            this._productService = productService;
            this._orderProcessingService = orderProcessingService;
            this._workContext = workContext;
            this._orderService = orderService;
            this._localizationService = localizationService;
            this._workflowMessageService = workflowMessageServic;
            this._addressService = addressService;
            this._zohoService = zohoService;
            this._manageService = manageService;
            this._logger = logger;
        }

        #endregion
        public async Task<IActionResult> Index(int orderId, int customerId)
        {
            if (orderId == 0 || customerId == 0)
                return View("_NotFound");

            else
            {
                var isOrderValid = await _customOrderModelFactory.ValidateOrder(orderId, customerId);
                if (!isOrderValid)
                    return View("_NotFound");
                else
                {
                    int _orderId = 0;
                    string cookieValue = _httpContextAccessor.HttpContext.Request.Cookies[cookieName];
                    int.TryParse(cookieValue, out _orderId);

                    if (_orderId != orderId)
                    {
                        var order = await _customOrderService.GetById(orderId);
                        var model = new CustomOrderLoginInfo();
                        model.OrderId = orderId;
                        model.ParentOrderId = order.ParentOrderID;
                        return View("_authenticate", model);
                    }
                    else
                    {
                        var order = await _customOrderService.GetById(orderId);
                        return View("_detail", await _customOrderModelFactory.PrepareCustomerOrderModel(order));
                    }
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Index(CustomOrderLoginInfo model, IFormCollection form)
        {
            if (ModelState.IsValid)
            {
                bool isValid = await this._customOrderModelFactory.ValidateOrderCustomerDetails(model.OrderId, model.EmailAddress, model.ZipCode);
                if (isValid)
                {
                    var cookieOptions = new CookieOptions
                    {
                        Expires = DateTime.Now.AddDays(7),
                        HttpOnly = true,
                        Secure = _webHelper.IsCurrentConnectionSecured()
                    };

                    //add cookie
                    _httpContextAccessor.HttpContext.Response.Cookies.Append(cookieName, model.OrderId.ToString(), cookieOptions);
                    var order = await _customOrderService.GetById(model.OrderId);
                    return View("_detail", await _customOrderModelFactory.PrepareCustomerOrderModel(order));

                }
                else
                {
                    ModelState.AddModelError("", "Please provide valid email and zip code.");
                    return View("_authenticate", model);
                }
            }
            else
                return View("_authenticate", model);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment(string paymentMethod, int orderId, IFormCollection form)
        {
            try
            {
                await this._customOrderModelFactory.UpdateOrderTotal(orderId);
                var order = await _customOrderService.GetById(orderId);
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
                    {
                        #region send Order Decline message
                        try
                        {
                            await _workflowMessageService.SendOrderDeclineMessage(null, form, customer, (await _workContext.GetWorkingCurrencyAsync()).Id, $"Payment Method: {paymentMethod ?? ""} Error: " + error, orderId);
                        }
                        catch (Exception exp)
                        {

                            await _logger.InsertLogAsync(LogLevel.Error,
                                "Failed to send order decline Email", exp.Message, await _workContext.GetCurrentCustomerAsync());
                        }

                        #endregion
                        throw new Exception(error);
                    }
                    var filterByCountryId = 0;
                    if (_addressSettings.CountryEnabled)
                    {
                        filterByCountryId = (await _customerService.GetCustomerBillingAddressAsync(customer))?.CountryId ?? 0;
                    }

                    //get payment info

                    var paymentInfo = await _paymentMethod.GetPaymentInfoAsync(form);
                    //set previous order GUID (if exists)
            
                    paymentInfo.StoreId = (await _storeContext.GetCurrentStoreAsync()).Id;
                    paymentInfo.CustomerId = customer.Id;
                    paymentInfo.PaymentMethodSystemName = paymentMethod;
                    await _orderProcessingService.SetProcessPaymentRequestAsync(paymentInfo,customer);
                    //session save

                    return await ConfirmOrder(order, customer, paymentMethod, paymentInfo, filterByCountryId, _paymentMethod, Convert.ToInt32(order.CustomerId));
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

        public async Task<IActionResult> NotInterested(int orderId)
        {
            if (orderId == 0)
                return View("_NotFound");

            else
            {
                var order = await _customOrderService.GetById(orderId);
                if (order == null)
                    return View("_NotFound");
                else
                {
                    var orderStatuses = await _customOrderService.GetOrderStatuses();
                    if (orderStatuses.Where(m => m.Id == order.StatusId).FirstOrDefault()?.Name != OrderStatus.Paid.ToString())
                    {

                        order.NotInterested = true;
                        if (orderStatuses.Where(s => s.Name == OrderStatus.NotInterested.ToString()).Any())
                        {
                            order.StatusId = orderStatuses.Where(s => s.Name ==OrderStatus.NotInterested.ToString()).First().Id;
                        }
                        await _customOrderService.UpdateAsync(order);

                        await this._customOrderService.InsertOrderStatusLogAsync(new CustomorderOrderStatusLog()
                        {
                            CreatedOn = DateTime.UtcNow,
                            InvoiceSendTo = string.Empty,
                            OrderId = order.Id,
                            StatusId = orderStatuses.Where(s => s.Name == OrderStatus.NotInterested.ToString()).Any() ?
                              orderStatuses.Where(s => s.Name == OrderStatus.NotInterested.ToString()).FirstOrDefault().Id : 0,
                            UserId = order.CustomerId ?? 0,
                            NotificationId = 0,
                            Comments = "Customer Not Interested"
                        });
                    }

                }
            }

            return View();
        }


        #region utilities

        public async Task<AjaxReponseModel> PrepareResponse(int orderid, int statuscode = 200, string message = "", string html = "", string json = "",
            string goto_section = "", string bindSectionId = "", bool closeContainer = false, bool isPopup = false, bool showContainer = false, string orderSummaryHtml = "",
            string notificationMessage = "", bool redirect = false)
        {
            AjaxReponseModel model = new AjaxReponseModel();
            model.closeContainer = closeContainer;
            model.showContainer = showContainer;
            model.statuscode = statuscode;
            model.isPopup = isPopup;
            model.message = message;
            model.html = html;
            model.json = json;
            model.bindSectionId = bindSectionId;
            model.goto_section = goto_section;
            model.redirect = redirect;
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

        public virtual async Task<IActionResult> ConfirmOrder(CustomOrder order,
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
                    await this._orderService.UpdateOrderAsync(orderEntity);
                    // end 
                    // Update OrderStatus

                    if (paidStatus != null)
                        order.StatusId = paidStatus.Id;

                    //

                    order.LiveOrderNumber = placeOrderResult.PlacedOrder.Id;
                    order.FullPaid = fullPaid;

                    #region Order Zoho Lead

                    var items = await _customOrderService.GetOrderItems(order.Id);
                    string description = "";
                    foreach (var item in items)
                    {
                        var product = await _productService.GetProductByIdAsync(item.ProductId);
                        description += "ProductId:" + item.ProductId + ";SKU:" + product?.Sku ?? "" + "|";
                    }
                    var iPAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress == null ? "" : _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();


                    if (order.ParentOrderID == 0)
                    {
                        order.ZohoPotentialId = await _zohoService.CreateUpdateOrderContactPotential(order.LiveOrderNumber ?? order.Id, order.ZohoPotentialId, customer, OrderStatus.Paid.ToString(), description, iPAddress, string.Empty, (order.SubTotal ?? 0) + (order.TotalDiscount ?? 0), order.CreatedBy, $"{_storeContext.GetCurrentStore().Url}checkoutCustomOrder?orderid={order.Id}&customerid={order.CustomerId}", true);
                    }

                    #endregion
                    await _customOrderService.UpdateAsync(order);

                    #region Manage Update Status of Order
                    if (order.ParentOrderID > 0)
                    {

                        await _manageService.MarkWgsAsPaid(order.ParentOrderID, placeOrderResult.PlacedOrder.CustomerEmail, order.OrderTotal ?? 0, order.PairedOrderIds ?? string.Empty);
                    }

                    #endregion

                    //

                    var orderPlacedCustomerNotificationQueuedEmailIds = await _workflowMessageService
              .CustomOrder_SendCustomerNotificationAsync(order, placeOrderResult.PlacedOrder.CustomerLanguageId);

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

                    return Json(new
                    {
                        response = await PrepareResponse(
             orderid: 0,
           statuscode: 200,
              redirect: true,
            html: "",
           message: "",
          notificationMessage:
           await _localizationService.GetResourceAsync("CustomOrder.Message.OrderPlacedSuccessfully")
          )
                    });
                }
                else
                {
                    #region send Order Decline message
                    try
                    {

                        await _workflowMessageService.SendOrderDeclineMessage(processPaymentRequest, null, customer, (await _workContext.GetWorkingCurrencyAsync()).Id, $"Payment Method: {paymentMethodName ?? string.Empty} Error: " + (placeOrderResult == null ? "" : string.Join(",", placeOrderResult.Errors)), order.Id);
                    }
                    catch (Exception exp)
                    {
                  
                        await _logger.InsertLogAsync(LogLevel.Error,
                            "Failed to send order decline Email", exp.Message, await _workContext.GetCurrentCustomerAsync());
                    }

                    #endregion

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

        public async Task<IActionResult> ProcessAffirmOrder(int orderId, int liveOrderNumber, string transactionId)
        {
            var orderEntity = await _orderService.GetOrderByIdAsync(liveOrderNumber);
            var order = await _customOrderService.GetById(orderId);
            if (orderEntity == null || order == null || order.CustomerId != orderEntity.CustomerId || (orderEntity.CaptureTransactionId != transactionId && orderEntity.AuthorizationTransactionId != transactionId && (order.AlreadyFee ?? 0) == 0))
            {
                return RedirectToRoute("Homepage");
            }
            var orderSummary = await this._customOrderModelFactory.PrepareOderSummaryModel(order.Id);
            var customer = new Customer();
            if (order.CustomerId != null)
                customer = await _customerService.GetCustomerByIdAsync(Convert.ToInt32(order.CustomerId));
            var postProcessPaymentRequest = new PostProcessPaymentRequest
            {
                Order = orderEntity
            };

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


            decimal paidAmount = orderEntity.OrderTotal;
            bool fullPaid = true;

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
                    if (!string.IsNullOrEmpty((string)orderSummary.PayableAmount))
                    {
                        decimal.TryParse(orderSummary.PayableAmount, NumberStyles.Currency,
                CultureInfo.CurrentCulture.NumberFormat, out decimal payableAmount);
                        if (payableAmount > 0)
                        {
                            await _manageService.SyncPendingOrderPartialPayment(orderEntity.Id, transactionId, payableAmount, DateTime.Now, "Affirm");
                        }
                    }
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
            await this._orderService.UpdateOrderAsync(orderEntity);
            // end 
            // Update OrderStatus

            if (paidStatus != null)
                order.StatusId = paidStatus.Id;

            //

            order.LiveOrderNumber = orderEntity.Id;
            order.FullPaid = fullPaid;

            #region Order Zoho Lead

            var items = await _customOrderService.GetOrderItems(order.Id);
            string description = "";
            foreach (var item in items)
            {
                var product = await _productService.GetProductByIdAsync(item.ProductId);
                description += "ProductId:" + item.ProductId + ";SKU:" + product?.Sku ?? "" + "|";
            }
            var iPAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress == null ? "" : _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();



            if (order.ParentOrderID == 0)
            {
                order.ZohoPotentialId = await _zohoService.CreateUpdateOrderContactPotential(order.LiveOrderNumber ?? order.Id, order.ZohoPotentialId, customer, OrderStatus.Paid.ToString(), description, iPAddress, string.Empty, (order.SubTotal ?? 0) + (order.TotalDiscount ?? 0), order.CreatedBy, $"{_storeContext.GetCurrentStore().Url}checkoutCustomOrder?orderid={order.Id}&customerid={order.CustomerId}", true);
            }

            #endregion
            await _customOrderService.UpdateAsync(order);

            #region Manage Update Status of Order
            if (order.ParentOrderID > 0)
                await _manageService.MarkWgsAsPaid(order.LiveOrderNumber ?? 0, orderEntity.CustomerEmail, order.OrderTotal ?? 0, order.PairedOrderIds ?? string.Empty);

            #endregion

            //

            var orderPlacedCustomerNotificationQueuedEmailIds = await _workflowMessageService
      .CustomOrder_SendCustomerNotificationAsync(order, orderEntity.CustomerLanguageId);

            var paymentResponse = new ProcessPaymentResult();
            paymentResponse.AuthorizationTransactionId = $"Affirm: {transactionId}";

            await this._customOrderService.InsertOrderStatusLogAsync(new CustomorderOrderStatusLog()
            {
                CreatedOn = DateTime.UtcNow,
                OrderId = order.Id,
                StatusId = paidStatus != null ? paidStatus.Id : 0,
                UserId = (customer?.Id ?? 0) == 0 ? (await _workContext.GetCurrentCustomerAsync()).Id : customer.Id,
                AmountPaid = paidAmount,
                PaymentResponse = JsonConvert.SerializeObject(paymentResponse)
            });

            // end

            return Redirect($"/checkoutCustomOrder?orderid={order.Id}&customerid={order.CustomerId}");
        }

        #endregion
    }
}
