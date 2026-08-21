using Microsoft.AspNetCore.Mvc;
using Nop.Services.Security;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Factories;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Orders;
using Nop.Services.Customers;
using System.Net;
using Nop.Services.Common;
using Nop.Services.Orders;
using Nop.Services.Messages;
using Nop.Services.Localization;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Customers;
using Microsoft.AspNetCore.Http;
using Nop.Services.Catalog;
using Nop.Core;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Core.Domain.Customers;
using Nop.Services.Payments;
using System.Globalization;
using Newtonsoft.Json;
using Nop.Core.Domain.Common;
using Nop.Services.Customizations.CustomOrders;
using MWT.Nop.Core.Service.Zoho;
using MWT.Nop.Core.Services.Manage;
using MWT.Nop.Core.Services.Message;
using MWT.Plugin.Misc.MwtStorefront.Models.Common;
using MWT.Nop.Core.Services.Orders;
using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Domain.CustomOrders;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Controllers
{
    public class AdditionalServiceController : BaseCustomOrderController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly ICustomOrderModelFactory _customOrderModelFactory;
        private readonly ICustomOrderService _customOrderService;
        private readonly ICustomerService _customerService;
        private readonly IOrderExtendedService _orderService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerModelFactory _customerModelFactory;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IWorkContext _workContext;
        private readonly IOrderProcessingExtendedService _orderProcessingService;
        private readonly IProductExtendedService _productService;
        private readonly IAddressService _addressService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IZohoService _zohoService;
        private readonly IStoreContext _storeContext;
        private readonly IManageService _manageService;
        private readonly ICustomWorkflowMessageService _workflowMessageService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly AddressSettings _addressSettings;
        private readonly IPaymentService _paymentService;

        #endregion

        #region Ctor

        public AdditionalServiceController(IPermissionService permissionService,
                               ICustomOrderModelFactory customOrderModelFactory,
                               ICustomOrderService customOrderService,
                                ICustomerService customerService,
                         IOrderExtendedService orderService,
                         INotificationService notificationService,
                         ILocalizationService localizationService,
                          ICustomerModelFactory customerModelFactory,
                          IPriceFormatter priceFormatter,
                          IWorkContext workContext,
                          IOrderProcessingExtendedService orderProcessingService,
                          IProductExtendedService productService,
                          IHttpContextAccessor httpContextAccessor,
                         IAddressService addressService,
                          IZohoService zohoService,
                           IStoreContext storeContext,
                           IManageService manageService,
                           ICustomWorkflowMessageService workflowMessageService,
                            IPaymentPluginManager paymentPluginManager,
                            AddressSettings addressSettings,
                            IPaymentService paymentService
                          )
        {
            this._permissionService = permissionService;
            this._customOrderModelFactory = customOrderModelFactory;
            this._customOrderService = customOrderService;
            this._customerService = customerService;
            this._orderService = orderService;
            this._notificationService = notificationService;
            this._localizationService = localizationService;
            this._customerModelFactory = customerModelFactory;
            this._priceFormatter = priceFormatter;
            this._workContext = workContext;
            this._orderProcessingService = orderProcessingService;
            this._productService = productService;
            this._httpContextAccessor = httpContextAccessor;
            this._addressService = addressService;
            this._zohoService = zohoService;
            this._storeContext = storeContext;
            this._manageService = manageService;
            this._workflowMessageService = workflowMessageService;
            this._paymentPluginManager = paymentPluginManager;
            this._addressSettings = addressSettings;
            this._paymentService = paymentService;
        }

        #endregion

        #region Methods
        //Additional Service by Nikhil Mind Web tree

        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_ACCESS_CUSTOMORDER)]
        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_ACCESS_CUSTOMORDER)]
        public virtual async Task<IActionResult> List()
        {
  
            //prepare model
            var model = await _customOrderModelFactory.PrepareCustomerOrderSearchModelAsync(new CustomOrderSearchModel());

            return View(model);
        }


        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_ACCESS_CUSTOMORDER)]
        public virtual async Task<IActionResult> OrderList(CustomOrderSearchModel searchModel, bool IsPartialOrderScreen)
        {
            searchModel.DisplayAdditionalService = true;
            //prepare model
            var model = await _customOrderModelFactory.PrepareCustomOrderListModelAsync(searchModel, IsPartialOrderScreen);

            return Json(model);
        }

        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_ACCESS_CUSTOMORDER)]
        public async Task<IActionResult> Create(int? id)
        {
            var model = new AdditionalServiceModel();

            if (id != null)
            {
                var order = await _customOrderService.GetById(Convert.ToInt32(id));


                if (order == null)
                    return RedirectToAction("List");
                else
                {

                    var orderStatuses = await _customOrderService.GetOrderStatuses();
                    var _orderStatus = orderStatuses.Where(s => s.Id == order.StatusId).FirstOrDefault();
                    if (_orderStatus != null)
                    {
                        if (Enum.TryParse<MWT.Nop.Core.Domain.CustomOrders.OrderStatus>(_orderStatus.Name, out MWT.Nop.Core.Domain.CustomOrders.OrderStatus _status))
                            model.Status = _status;
                    }
                    //  we have to get Email id of customer
                    //  CHANGE COLUMN NAME FROM EmailID TO Email
                    if (!string.IsNullOrEmpty(order.CustomerCCEmail))
                        model.Email = order.CustomerCCEmail;
                    else
                    {
                        if (order.CustomerId != null && order.CustomerId != 0)
                        {
                            var customer = await _customerService.GetCustomerByIdAsync(Convert.ToInt32(order.CustomerId));
                            if (customer != null)
                                model.Email = customer.Email;
                        }
                    }
                    List<CustomOrderShoppingCartItem> customOrderShoppingCartItems = await _customOrderService.GetOrderItems(Convert.ToInt32(id));
                    foreach (CustomOrderShoppingCartItem item in customOrderShoppingCartItems)
                    {
                        model.Serviceproductid = item.ProductId;

                        var priceAdjustment = await _customOrderService.GetPriceAdjustmentsByCartId(Convert.ToInt32(item.Id));
                        model.ServicePrice = priceAdjustment.ShoppingCartProductPrice;
                        model.DiscountPrice = priceAdjustment.Discountamount;
                    }
                    model.Id = order.Id;
                    model.OrderId = order.ParentOrderID;
                    model.PairedOrderIds = order.PairedOrderIds;
                    model.Exp = order.PromiseDayDate;
                    model.Comment = order.PrivateOrderNotes;
                    model.ApplyTax = order.ApplyTax;
                    var _order = await _orderService.GetOrderByIdAsync(model.OrderId);
                    return View(await _customOrderModelFactory.PrepareAdditionalServiceModel(model, order, _order, model.Status));
                }

            }

            return View(await _customOrderModelFactory.PrepareAdditionalServiceModel(model));
        }


        [HttpPost, ParameterBasedOnFormName("placeOrder", "processOrder")]
        [HttpPost]
        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_ACCESS_CUSTOMORDER)]
        public virtual async Task<IActionResult> Create(AdditionalServiceModel model, bool processOrder, IFormCollection form)
        {

            var order = await _orderService.GetOrderByIdAsync(model.OrderId);
            var customOrder = new MWT.Nop.Core.Domain.CustomOrders.CustomOrder();
            decimal previousWgsAmount = 0;
            decimal previousWgsDiscount = 0;
            decimal currentWgsAmount = 0;
            decimal currentWgsDiscount = 0;

            if (ModelState.IsValid && order != null)
            {
                var customer = await _customerService.GetCustomerByIdAsync(Convert.ToInt32(order.CustomerId));
                var orderStatuses = await _customOrderService.GetOrderStatuses();
                if (model.Id != 0)
                {   //for CustomOrder_Orders Table Additional service edit
                    customOrder = await _customOrderService.GetById(Convert.ToInt32(model.Id));
                    if (customOrder != null)
                    {
                        previousWgsAmount = customOrder.OrderTotal ?? 0;
                        currentWgsAmount = model.ServicePrice ?? 0;
                        currentWgsDiscount = model.DiscountPrice ?? 0;

                        customOrder.StatusId = orderStatuses.Where(s => s.Name == MWT.Nop.Core.Domain.CustomOrders.OrderStatus.InvoiceSent.ToString()).Any() ?
                            orderStatuses.Where(s => s.Name == MWT.Nop.Core.Domain.CustomOrders.OrderStatus.InvoiceSent.ToString()).FirstOrDefault().Id : 4;
                        customOrder.ParentOrderID = model.OrderId;
                        customOrder.SubTotal = (model.ServicePrice ?? 0) - (model.DiscountPrice ?? 0);
                        customOrder.PairedOrderIds = model.PairedOrderIds;
                        customOrder.TotalDiscount = 0;
                        customOrder.ApplyTax = model.ApplyTax;
                        customOrder.OrderTax = !model.ApplyTax ? 0 : (await _customOrderModelFactory.GetOrderTax(customer, customOrder.SubTotal ?? 0, 0))?.TaxTotal ?? 0;
                        customOrder.OrderTotal = ((model.ServicePrice ?? 0) - (model.DiscountPrice ?? 0)) + customOrder.OrderTax;
                        customOrder.PrivateOrderNotes = model.Comment;
                        customOrder.PromiseDayDate = model.Exp;
                        customOrder.CustomerId = customer.Id;
                        if (customer.Email != model.Email)
                            customOrder.CustomerCCEmail = model.Email;
                        await _customOrderService.UpdateAsync(customOrder);

                        // For CustomOrder_ShoppingCartItem Table additional sevice editing the serive type
                        List<CustomOrderShoppingCartItem> customOrderShoppingCartItems = await _customOrderService.GetOrderItems(Convert.ToInt32(model.Id));
                        if (customOrderShoppingCartItems != null)
                        {
                            foreach (CustomOrderShoppingCartItem item in customOrderShoppingCartItems)
                            {
                                item.ProductId = model.Serviceproductid;
                                await _customOrderService.UpdateCartItemAsync(item);
                                //For CustomOrder_PriceAdjustment table additional service editing the price 
                                var GetPriceAdjustment = await _customOrderService.GetPriceAdjustmentsByCartId(Convert.ToInt32(item.Id));
                                if (GetPriceAdjustment != null)
                                {
                                    GetPriceAdjustment.ShoppingCartProductPrice = model.ServicePrice ?? 0;
                                    previousWgsAmount = previousWgsAmount + (GetPriceAdjustment.Discountamount ?? 0);
                                    previousWgsDiscount = GetPriceAdjustment.Discountamount ?? 0;
                                    GetPriceAdjustment.Discountamount = model.DiscountPrice;
                                    await _customOrderService.UpdatePriceAdjustmentAsync(GetPriceAdjustment);
                                }
                            }
                        }

                    }

                    else
                        return RedirectToAction("List");

                }
                else
                {    //for CustomOrder_Orders Table Additional service create
                    previousWgsAmount = model.ServicePrice ?? 0;
                    previousWgsDiscount = model.DiscountPrice ?? 0;
                    var tax = !model.ApplyTax ? 0 : (await _customOrderModelFactory.GetOrderTax(customer, (model.ServicePrice ?? 0) - (model.DiscountPrice ?? 0), 0))?.TaxTotal ?? 0;
                    customOrder = new MWT.Nop.Core.Domain.CustomOrders.CustomOrder()
                    {
                        ParentOrderID = model.OrderId,
                        CreatedBy = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0,
                        TotalDiscount = 0,
                        SubTotal = (model.ServicePrice ?? 0) - (model.DiscountPrice ?? 0),
                        OrderTax = tax,
                        OrderTotal = ((model.ServicePrice ?? 0) - (model.DiscountPrice ?? 0)) + tax,
                        PrivateOrderNotes = model.Comment,
                        PromiseDayDate = model.Exp,
                        ApplyTax = model.ApplyTax,
                        PairedOrderIds = model.PairedOrderIds,
                        StatusId = orderStatuses.Where(s => s.Name == MWT.Nop.Core.Domain.CustomOrders.OrderStatus.InvoiceSent.ToString()).Any() ?
                            orderStatuses.Where(s => s.Name == MWT.Nop.Core.Domain.CustomOrders.OrderStatus.InvoiceSent.ToString()).FirstOrDefault().Id : 4,
                        CustomerId = customer.Id,
                        OrderTypeId = (int)OrderTypes.CustomOrder

                    };
                    if (customer.Email != model.Email || customer.Email != null)
                        customOrder.CustomerCCEmail = model.Email;
                    // For CustomOrder_ShoppingCartItem Table additional sevice create
                    await _customOrderService.InsertAsync(customOrder);
                    CustomOrderShoppingCartItem item = new CustomOrderShoppingCartItem();
                    item.CustomerId = order.CustomerId;
                    item.StoreId = 0;
                    item.ShoppingCartTypeId = 1;
                    item.CustomerEnteredPrice = 0;
                    item.Quantity = 1;
                    item.ProductId = model.Serviceproductid;
                    item.OrderId = customOrder.Id;
                    await _customOrderService.InsertOrderItemAsync(item);

                    //For CustomOrder_PriceAdjustment table additional service Create
                    CustomOrderPriceAdjustment items = new CustomOrderPriceAdjustment();
                    items.ShoppingCartRecID = item.Id;
                    items.ShoppingCartProductPrice = model.ServicePrice ?? 0;
                    items.Discountamount = model.DiscountPrice;
                    items.Chargestype = "Subtract";
                    items.Discounttype = "Fixed";
                    items.OrderId = customOrder.Id;
                    await _customOrderService.InsertPriceAdjustmentAsync(items);


                }
                if (!processOrder)
                {
                    await this._customOrderModelFactory.AdditionalServiceSendInvoice(customOrder, previousWgsAmount, previousWgsDiscount, currentWgsAmount, currentWgsDiscount, model.DefaultWgsCharges, model.SurchargeAmount);
                    return RedirectToAction("List");
                }
                else
                {
                    string paymentMethod = "Payments.CashOnDelivery";
                    if ((customOrder.LiveOrderNumber != null && customOrder.LiveOrderNumber > 0))
                        throw new Exception(await _localizationService.GetResourceAsync("CustomOrder.Message.Order.AlreadyPaid"));
                    var _paymentMethod = await _paymentPluginManager
                  .LoadPluginBySystemNameAsync(paymentMethod, customer, (await _storeContext.GetCurrentStoreAsync()).Id)
                  ?? throw new Exception("Payment method is not selected");

                    var warnings = await _paymentMethod.ValidatePaymentFormAsync(form);
                    string error = "";
                    foreach (var warning in warnings)
                        error = error + warning + @"<br\>";
                    if (error.Length > 0)
                    {
                        this._notificationService.ErrorNotification(error);
                        model = await _customOrderModelFactory.PrepareAdditionalServiceModel(model);
                        return View(model);
                    }
                    var filterByCountryId = 0;
                    if (_addressSettings.CountryEnabled)
                    {
                        filterByCountryId = (await _customerService.GetCustomerBillingAddressAsync(customer))?.CountryId ?? 0;
                    }
                    //get payment info
                  


                    var paymentInfo = await _paymentMethod.GetPaymentInfoAsync(form);
                    paymentInfo.StoreId = (await _storeContext.GetCurrentStoreAsync()).Id;
                    paymentInfo.CustomerId = customer.Id;
                    paymentInfo.PaymentMethodSystemName = paymentMethod;

                    await this._orderProcessingService.SetProcessPaymentRequestAsync(await _paymentMethod.GetPaymentInfoAsync(form), customer);

                    paymentInfo = await this._orderProcessingService.GetProcessPaymentRequestAsync(customer);

                    //session save

                    (HttpStatusCode statusCode, string message) = await ConfirmOrder(customOrder, customer, paymentMethod, paymentInfo, filterByCountryId, _paymentMethod, Convert.ToInt32(order.CustomerId));
                    if (statusCode == HttpStatusCode.OK)
                    {
                        this._notificationService.SuccessNotification(message);
                        return RedirectToAction("Create", new { id = customOrder.Id });
                    }
                    else
                    {
                        this._notificationService.ErrorNotification(message);
                        model = await _customOrderModelFactory.PrepareAdditionalServiceModel(model);
                        return View(model);
                    }

                }
            }
            else
            {
                this._notificationService.ErrorNotification(await this._localizationService.GetResourceAsync("CustomOrder.Order.Order.NotValid"));
                model = await _customOrderModelFactory.PrepareAdditionalServiceModel(model);
            }

            return View(model);
        }

        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_ACCESS_CUSTOMORDER)]
        public virtual async Task<IActionResult> Validate(int Id, string pairedOrderIds)
        {
            var order = await _orderService.GetOrderByIdAsync(Id);
            string email = " ";
            AddressModel ShippingAddress = new AddressModel();
            AddressModel BillingAddress = new AddressModel();
            decimal orderTotal = 0;
            decimal orderSubtotal = 0;
            bool isSurchargeApplicable = false;
            decimal wgs = 0;
            decimal defaultWgsAmount = 0;
            decimal surchargeAmount = 0;
            Dictionary<int, dynamic> pairedOrders = new Dictionary<int, dynamic>();
            string validOrderIds = string.Empty;

            if (order != null)
            {
                if (order.CustomerId != null && order.CustomerId != 0)
                {

                    orderTotal = order.OrderTotal;
                    var customer = await _customerService.GetCustomerByIdAsync(Convert.ToInt32(order.CustomerId));
                    email = customer.Email;
                    if (email == null)
                        email = customer.Email;
                    MWT.Nop.Core.Domain.CustomOrders.CustomOrder customOrder = await _customOrderService.GetByOrderNumber(Id);


                    (pairedOrders, validOrderIds, orderSubtotal, orderTotal, isSurchargeApplicable) = await this._customOrderModelFactory.GetAdditionalServiceOrderInfo(MWT.Nop.Core.Domain.CustomOrders.OrderStatus.OrderSaved, order, customOrder, pairedOrderIds);

                    if (customOrder != null)
                    {
                        (wgs, defaultWgsAmount, surchargeAmount) = await _customOrderModelFactory.GetWgsCharges(customOrder, orderSubtotal, isSurchargeApplicable);
                    }
                    else
                    {
                        (wgs, defaultWgsAmount, surchargeAmount) = await this._orderService.GetWgsCharges(order, orderSubtotal, isSurchargeApplicable);
                    }
                    if (order.ShippingAddressId != null)
                    {
                        ShippingAddress = await _customerModelFactory.PrepareCustomAddressModelAsync(customer, MWT.Nop.Core.Domain.Address.AddressType.IsShippingAddress, Convert.ToInt32(order.ShippingAddressId),
                            null, prePopulateNewAddressWithCustomerFields: true);
                        ShippingAddress.IsShippingAddress = true;
                    }
                    if (order.BillingAddressId != 0)
                    {
                        BillingAddress = await _customerModelFactory.PrepareCustomAddressModelAsync(customer, MWT.Nop.Core.Domain.Address.AddressType.IsBillingAddress, Convert.ToInt32(order.BillingAddressId),
                            null, prePopulateNewAddressWithCustomerFields: true);
                        if (order.ShippingAddressId == null)
                        {
                            ShippingAddress = BillingAddress;
                            ShippingAddress.IsShippingAddress = true;
                        }
                    }
                }
            }


            return Json(new
            {
                statuscode = order == null ? (int)HttpStatusCode.NotFound : (int)HttpStatusCode.OK,
                email = email,
                shippingAddress = await RenderPartialViewToStringAsync("_addressinfo", ShippingAddress),
                billingAddress = await RenderPartialViewToStringAsync("_addressinfo", BillingAddress),
                wgs = wgs,
                defaultWgsAmount = defaultWgsAmount,
                surchargeAmount = surchargeAmount,
                orderTotal = $"<div><strong>{await _localizationService.GetResourceAsync("order.ordertotal")}</strong>  {await _priceFormatter.FormatPriceAsync(orderTotal)}</div>",
                orderSubtotal = $"<div><strong>{await _localizationService.GetResourceAsync("shoppingcart.totals.subtotal")} </strong>  {await _priceFormatter.FormatPriceAsync(orderSubtotal)}</div>",
                pairedOrders = pairedOrders,
                validOrderIds = validOrderIds
            });
        }

        [HttpPost]
        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_ACCESS_CUSTOMORDER)]
        public virtual async Task<IActionResult> GetTax(AdditionalServiceModel model)
        {
            decimal tax = 0;
            if (model.OrderId != 0)
            {
                if (model.ApplyTax)
                {
                    var _order = await _orderService.GetOrderByIdAsync(model.OrderId);
                    var customer = await _customerService.GetCustomerByIdAsync(_order.CustomerId);
                    var taxTotalResult = await _customOrderModelFactory.GetOrderTax(customer, (model.ServicePrice ?? 0) -
                        (model.DiscountPrice ?? 0), 0);
                    tax = taxTotalResult?.TaxTotal ?? 0;
                }
            }
            return Json(new
            {
                statuscode = (int)HttpStatusCode.OK,
                Tax = tax
            });
        }
        #endregion

        #region Utilities

        public virtual async Task<(HttpStatusCode, string)> ConfirmOrder(MWT.Nop.Core.Domain.CustomOrders.CustomOrder order,
          Customer customer, string paymentMethodName,
          ProcessPaymentRequest processPaymentRequest, int filterByCountryId, IPaymentMethod paymentMethod, int customerId = 0, bool chargeFromInitialaOrder = false)
        {
            HttpStatusCode statusCode = HttpStatusCode.OK;
            string message = string.Empty;
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
                    var paidStatus = orderStatuses.Where(m => m.Name == MWT.Nop.Core.Domain.CustomOrders.OrderStatus.Paid.ToString()).FirstOrDefault();


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


                    await _customOrderService.UpdateAsync(order);

                    #region Manage Update Status of Order

                    await _manageService.MarkWgsAsPaid(order.ParentOrderID, placeOrderResult.PlacedOrder.CustomerEmail, order.OrderTotal ?? 0, order.PairedOrderIds ?? string.Empty);


                    #endregion

                    //

                    var orderPlacedCustomerNotificationQueuedEmailIds = await _workflowMessageService
              .CustomOrder_SendCustomerNotificationAsync(order, placeOrderResult.PlacedOrder.CustomerLanguageId);

                    await this._customOrderService.InsertOrderStatusLogAsync(new CustomorderOrderStatusLog()
                    {
                        CreatedOn = DateTime.UtcNow,
                        OrderId = order.Id,
                        StatusId = paidStatus != null ? paidStatus.Id : 0,
                        UserId = (await _workContext.GetCurrentCustomerAsync()).Id,
                        AmountPaid = paidAmount,
                        PaymentResponse = string.IsNullOrEmpty(paymentResponse?.AuthorizationTransactionId) ? string.Empty : JsonConvert.SerializeObject(paymentResponse)
                    });

                    // end

                    message = await _localizationService.GetResourceAsync("CustomOrder.Message.OrderPlacedSuccessfully");
                }
                else
                {
                    statusCode = HttpStatusCode.InternalServerError;
                    foreach (var error in placeOrderResult.Errors)
                        message = message + error + @"<br\>";

                }
            }
            catch (Exception exp)
            {
                statusCode = HttpStatusCode.InternalServerError;
                message = exp.Message;
            }
            return (statusCode, message);
        }
        #endregion


    }
}