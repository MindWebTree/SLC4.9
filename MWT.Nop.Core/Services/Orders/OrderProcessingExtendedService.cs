using Microsoft.AspNetCore.Http;
using MWT.Nop.Core.Domain.CustomOrders;
using MWT.Nop.Core.Service.Zoho;
using MWT.Nop.Core.Services.AbandonedCarts;
using MWT.Nop.Core.Services.Customers;
using MWT.Nop.Core.Services.Customizations.CustomOrders;
using MWT.Nop.Core.Services.Manage;
using MWT.Nop.Core.Services.Message;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using Nop.Services.Affiliates;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using System.Globalization;
using OrderStatus = Nop.Core.Domain.Orders.OrderStatus;

namespace MWT.Nop.Core.Services.Orders
{
    public partial class OrderProcessingExtendedService : OrderProcessingService, IOrderProcessingExtendedService
    {

        #region Fields

        private readonly Message.ICustomWorkflowMessageService _customWorkflowMessageService;
        private readonly ICustomOrderService _customOrderService;
        private readonly IShoppingCartExtendedService _shoppingCartExtendedCartService;
        private readonly IOrderTotalCalculationExtendedService _orderTotalCalculationExtendedService;
        private readonly ICustomerExtendedService _customerExtendedService;
        private readonly ISettingService _settingService;
        private readonly IZohoService _zohoService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAbandonedCartService _abandonedCartService;
        private readonly IManageService _manageService;

        #endregion

        #region Ctor
        public OrderProcessingExtendedService(CurrencySettings currencySettings, IAddressService addressService, IAffiliateService affiliateService,
            ICheckoutAttributeFormatter checkoutAttributeFormatter, ICountryService countryService, ICurrencyService currencyService, ICustomerActivityService customerActivityService,
            ICustomerService customerService, ICustomNumberFormatter customNumberFormatter, IDiscountService discountService, IEncryptionService encryptionService,
            IEventPublisher eventPublisher, IGenericAttributeService genericAttributeService, IGiftCardService giftCardService, ILanguageService languageService,
            ILocalizationService localizationService, ILogger logger, IOrderService orderService, IOrderTotalCalculationService orderTotalCalculationService, IPaymentPluginManager paymentPluginManager,
            IPaymentService paymentService, IPdfService pdfService, IPriceCalculationService priceCalculationService, IPriceFormatter priceFormatter, IProductAttributeFormatter productAttributeFormatter,
            IProductAttributeParser productAttributeParser, IProductService productService, IReturnRequestService returnRequestService, IRewardPointService rewardPointService, IShipmentService shipmentService,
            IShippingService shippingService, IShoppingCartService shoppingCartService, IStateProvinceService stateProvinceService, IStaticCacheManager staticCacheManager, IStoreContext storeContext,
            IStoreMappingService storeMappingService, IStoreService storeService, ITaxService taxService, IVendorService vendorService, IWebHelper webHelper, IWorkContext workContext,
          ICustomWorkflowMessageService workflowMessageService, LocalizationSettings localizationSettings, OrderSettings orderSettings, PaymentSettings paymentSettings,
            RewardPointsSettings rewardPointsSettings, ShippingSettings shippingSettings, TaxSettings taxSettings,
            Message.ICustomWorkflowMessageService customWorkflowMessageService, ICustomOrderService customOrderService, IShoppingCartExtendedService shoppingCartExtendedCartService,
            IOrderTotalCalculationExtendedService orderTotalCalculationExtendedService, ICustomerExtendedService customerExtendedService, ISettingService settingService,
            IZohoService zohoService, IHttpContextAccessor httpContextAccessor, IAbandonedCartService abandonedCartService,
            IManageService manageService) :
            base(currencySettings, addressService, affiliateService, checkoutAttributeFormatter, countryService, currencyService, customerActivityService, customerService, customNumberFormatter,
                discountService, encryptionService, eventPublisher, genericAttributeService, giftCardService, languageService, localizationService, logger, orderService, orderTotalCalculationService,
                paymentPluginManager, paymentService, pdfService, priceCalculationService, priceFormatter, productAttributeFormatter, productAttributeParser, productService, returnRequestService,
                rewardPointService, shipmentService, shippingService, shoppingCartService, stateProvinceService, staticCacheManager, storeContext, storeMappingService, storeService, taxService, vendorService,
                webHelper, workContext, workflowMessageService, localizationSettings, orderSettings, paymentSettings, rewardPointsSettings, shippingSettings, taxSettings)
        {
            _customWorkflowMessageService = customWorkflowMessageService;
            _customOrderService = customOrderService;
            _shoppingCartExtendedCartService = shoppingCartExtendedCartService;
            _orderTotalCalculationExtendedService = orderTotalCalculationExtendedService;
            _customerExtendedService = customerExtendedService;
            _settingService = settingService;
            _zohoService = zohoService;
            _httpContextAccessor = httpContextAccessor;
            _abandonedCartService = abandonedCartService;
            _manageService = manageService;
        }

        #endregion

        #region Methods

        public virtual async Task CustomSendNotificationsAndSaveNotesAsync(Order order)
        {
            await AddOrderNoteAsync(order, _workContext.OriginalCustomerIfImpersonated != null
    ? $"Order placed by a store owner ('{_workContext.OriginalCustomerIfImpersonated.Email}'. ID = {_workContext.OriginalCustomerIfImpersonated.Id}) impersonating the customer."
    : "Order placed");

            var orderPlacedAttachmentFilePath = _orderSettings.AttachPdfInvoiceToOrderPlacedEmail ?
                (await _pdfService.SaveOrderPdfToDiskAsync(order)) : null;

            var orderPlacedAttachmentFileName = _orderSettings.AttachPdfInvoiceToOrderPlacedEmail ?
                (string.Format(await _localizationService.GetResourceAsync("PDFInvoice.FileName"), order.CustomOrderNumber) + ".pdf") : null;

            var orderPlacedCustomerNotificationQueuedEmailIds = await _customWorkflowMessageService
                .CustomSendOrderPlacedCustomerNotificationAsync(order, order.CustomerLanguageId, orderPlacedAttachmentFilePath, orderPlacedAttachmentFileName);

            if (orderPlacedCustomerNotificationQueuedEmailIds.Any())
                await AddOrderNoteAsync(order, $"\"Order placed\" email (to customer) has been queued. Queued email identifiers: {string.Join(", ", orderPlacedCustomerNotificationQueuedEmailIds)}.");

            var vendors = await GetVendorsInOrderAsync(order);
            foreach (var vendor in vendors)
            {
                var orderPlacedVendorNotificationQueuedEmailIds = await _customWorkflowMessageService.CustomSendOrderPlacedVendorNotificationAsync(order, vendor, _localizationSettings.DefaultAdminLanguageId);
                if (orderPlacedVendorNotificationQueuedEmailIds.Any())
                    await AddOrderNoteAsync(order, $"\"Order placed\" email (to vendor) has been queued. Queued email identifiers: {string.Join(", ", orderPlacedVendorNotificationQueuedEmailIds)}.");
            }

            if (order.AffiliateId == 0)
                return;

            var orderPlacedAffiliateNotificationQueuedEmailIds = await _customWorkflowMessageService.CustomSendOrderPlacedAffiliateNotificationAsync(order, _localizationSettings.DefaultAdminLanguageId);
            if (orderPlacedAffiliateNotificationQueuedEmailIds.Any())
                await AddOrderNoteAsync(order, $"\"Order placed\" email (to affiliate) has been queued. Queued email identifiers: {string.Join(", ", orderPlacedAffiliateNotificationQueuedEmailIds)}.");

        }
        public virtual async Task<(PlaceOrderResult result, ProcessPaymentResult request)> CustomPlaceOrderAsync(ProcessPaymentRequest processPaymentRequest, CustomOrder customOrder, int paidBy, dynamic orderSummary, bool saveOrderDetails = true, int refOrderno = 0, bool chargeFromInitialaOrder = false)
        {
            ArgumentNullException.ThrowIfNull(processPaymentRequest);

            if (processPaymentRequest.OrderGuid == Guid.Empty)
                throw new Exception("Order GUID is not generated");

            //prepare order details
            PlaceOrderContainer details = await PrepareCustomOrderPlaceOrderDetailsAsync(processPaymentRequest, customOrder, orderSummary);
            var customer = await _customerService.GetCustomerByIdAsync(processPaymentRequest.CustomerId);
            async Task<(PlaceOrderResult result, ProcessPaymentResult request)> placeOrder(PlaceOrderContainer placeOrderContainer)
            {
                var result = new PlaceOrderResult();
                var processPaymentResult = new ProcessPaymentResult();

                try
                {
                    if (refOrderno != 0 && chargeFromInitialaOrder)
                    {
                        details.InitialOrder = processPaymentRequest.InitialOrder;
                        var paymentMethod = await _paymentPluginManager
                            .LoadPluginBySystemNameAsync(processPaymentRequest.PaymentMethodSystemName, customer, details.InitialOrder.StoreId)
                            ?? throw new NopException("Payment method couldn't be loaded");

                        if (!_paymentPluginManager.IsPluginActive(paymentMethod))
                            throw new NopException("Payment method is not active");


                        if (details.InitialOrder.AllowStoringCreditCardNumber)
                        {
                            processPaymentRequest.CreditCardType = _encryptionService.DecryptText(details.InitialOrder.CardType);
                            processPaymentRequest.CreditCardName = _encryptionService.DecryptText(details.InitialOrder.CardName);
                            processPaymentRequest.CreditCardNumber = _encryptionService.DecryptText(details.InitialOrder.CardNumber);
                            processPaymentRequest.CreditCardCvv2 = _encryptionService.DecryptText(details.InitialOrder.CardCvv2);
                            try
                            {
                                processPaymentRequest.CreditCardExpireMonth = Convert.ToInt32(_encryptionService.DecryptText(details.InitialOrder.CardExpirationMonth));
                                processPaymentRequest.CreditCardExpireYear = Convert.ToInt32(_encryptionService.DecryptText(details.InitialOrder.CardExpirationYear));
                            }
                            catch
                            {
                                // ignored
                            }
                        }
                        ProcessPaymentResult paymentResult = new ProcessPaymentResult();
                        paymentResult = null;
                        processPaymentResult = (await _paymentService.GetRecurringPaymentTypeAsync(processPaymentRequest.PaymentMethodSystemName)) switch
                        {
                            RecurringPaymentType.NotSupported => throw new NopException("Recurring payments are not supported by selected payment method"),
                            RecurringPaymentType.Manual => await _paymentService.ProcessRecurringPaymentAsync(processPaymentRequest),
                            //payment is processed on payment gateway site, info about last transaction in paymentResult parameter
                            RecurringPaymentType.Automatic => paymentResult ?? new ProcessPaymentResult(),
                            _ => throw new NopException("Not supported recurring payment type"),
                        };
                    }
                    else
                        processPaymentResult =
                                               await GetProcessPaymentResultAsync(processPaymentRequest, placeOrderContainer)
                                               ?? throw new NopException("processPaymentResult is not available");


                    if (processPaymentResult.Success)
                    {
                        if (saveOrderDetails)
                        {
                            var order = await SaveCustomOrderDetailsAsync(processPaymentRequest, processPaymentResult,
                                placeOrderContainer);
                            result.PlacedOrder = order;

                            //move shopping cart items to order items
                            await CustomOrderMoveShoppingCartItemsToOrderItemsAsync(placeOrderContainer, order, customOrder);


                            await CustomOrderSendNotificationsAndSaveNotesAsync(order, customOrder);

                            //reset checkout data
                            await _customerActivityService.InsertActivityAsync(customer, "CustomerRelatedActivity", string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.PlaceOrder"), order.Id));

                            await _customerActivityService.InsertActivityAsync("PublicStore.PlaceOrder",
                                string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.PlaceOrder"),
                                    order.Id), order);

                            //raise event       
                            await _eventPublisher.PublishAsync(new OrderPlacedEvent(order));

                            //check order status
                            await CheckOrderStatusAsync(order);

                            if (order.PaymentStatus == PaymentStatus.Paid)
                                await ProcessOrderPaidAsync(order);

                            await UpdateCustomOrderChargesAsync(processPaymentRequest.PaymentMethodSystemName, placeOrderContainer, result,
          processPaymentResult, processPaymentRequest, order
        , customOrder, orderSummary, paidBy);
                        }
                        else
                        {
                            Order order = await _orderService.GetOrderByIdAsync(refOrderno);
                            result.PlacedOrder = order;
                            await CustomOrderSendNotificationsAndSaveNotesAsync(order, customOrder);
                            await UpdateCustomOrderChargesAsync(processPaymentRequest.PaymentMethodSystemName, placeOrderContainer, result,
      processPaymentResult, processPaymentRequest, order
    , customOrder, orderSummary, paidBy);
                        }

                    }
                    else
                        foreach (var paymentError in processPaymentResult.Errors)
                            result.AddError(string.Format(
                                await _localizationService.GetResourceAsync("Checkout.PaymentError"), paymentError));
                }
                catch (Exception exc)
                {
                    await _logger.ErrorAsync(exc.Message, exc);
                    result.AddError(exc.Message);
                }

                if (result.Success)
                    return (result, processPaymentResult);

                //log errors
                var logError = result.Errors.Aggregate("Error while placing order. ",
                    (current, next) => $"{current}Error {result.Errors.IndexOf(next) + 1}: {next}. ");
                await _logger.ErrorAsync(logError, customer: customer);

                return (result, processPaymentResult);
            }

            if (!_orderSettings.PlaceOrderWithLock)
                return await placeOrder(details);

            PlaceOrderResult result;
            var processPaymentResult = new ProcessPaymentResult();
            var resource = details.Customer.Id.ToString();

            //the named mutex helps to avoid creating the same order in different threads,
            //and does not decrease performance significantly, because the code is blocked only for the specific cart.
            //you should be very careful, mutexes cannot be used in with the await operation
            //we can't use semaphore here, because it produces PlatformNotSupportedException exception on UNIX based systems
            using var mutex = new Mutex(false, resource);

            mutex.WaitOne();

            try
            {
                var cacheKey = _staticCacheManager.PrepareKey(NopOrderDefaults.OrderWithLockCacheKey, resource);
                cacheKey.CacheTime = _orderSettings.MinimumOrderPlacementInterval;

                var exist = _staticCacheManager.Get(cacheKey, () => false);

                if (exist)
                {
                    result = new PlaceOrderResult();
                    result.Errors.Add(_localizationService.GetResourceAsync("Checkout.MinOrderPlacementInterval").Result);
                }
                else
                {
                    (result, processPaymentResult) = placeOrder(details).Result;

                    if (result.Success)
                        _staticCacheManager.SetAsync(cacheKey, true).Wait();
                }
            }
            finally
            {
                mutex.ReleaseMutex();
            }

            return (result, processPaymentResult);
        }
        public virtual async Task SetProcessPaymentRequestAsync(ProcessPaymentRequest processPaymentRequest, Customer customer, bool useNewOrderGuid = false)
        {

            var store = await _storeContext.GetCurrentStoreAsync();

            if (processPaymentRequest is null)
            {
                await _genericAttributeService.SaveAttributeAsync<string>(customer, NopCustomerDefaults.ProcessPaymentRequestAttribute, null, store.Id);

                return;
            }

            if (_paymentSettings.RegenerateOrderGuidInterval > 0 && !useNewOrderGuid)
            {
                //we should use the same GUID for multiple payment attempts
                //this way a payment gateway can prevent security issues such as credit card brute-force attacks
                //in order to avoid any possible limitations by payment gateway we reset GUID periodically
                var previousPaymentRequest = await GetProcessPaymentRequestAsync(customer);

                //set previous order GUID (if exists)
                if (previousPaymentRequest is { OrderGuidGeneratedOnUtc: not null })
                {
                    var interval = DateTime.UtcNow - previousPaymentRequest.OrderGuidGeneratedOnUtc.Value;
                    if (interval.TotalSeconds < _paymentSettings.RegenerateOrderGuidInterval)
                    {
                        processPaymentRequest.OrderGuid = previousPaymentRequest.OrderGuid;
                        processPaymentRequest.OrderGuidGeneratedOnUtc = previousPaymentRequest.OrderGuidGeneratedOnUtc;
                    }
                }
            }

            var json = JsonConvert.SerializeObject(processPaymentRequest);
            await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.ProcessPaymentRequestAttribute, json, store.Id);
        }
        public virtual async Task<ProcessPaymentRequest> GetProcessPaymentRequestAsync(Customer customer)
        {

            var store = await _storeContext.GetCurrentStoreAsync();
            var json = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.ProcessPaymentRequestAttribute, store.Id);

            return string.IsNullOrEmpty(json) ? null : JsonConvert.DeserializeObject<ProcessPaymentRequest>(json);
        }
        public override async Task<PlaceOrderResult> PlaceOrderAsync(ProcessPaymentRequest processPaymentRequest)
        {
            ArgumentNullException.ThrowIfNull(processPaymentRequest);

            if (processPaymentRequest.OrderGuid == Guid.Empty)
                throw new Exception("Order GUID is not generated");

            //prepare order details
            var details = await PreparePlaceOrderDetailsAsync(processPaymentRequest);

            async Task<PlaceOrderResult> placeOrder(PlaceOrderContainer placeOrderContainer)
            {
                var result = new PlaceOrderResult();

                try
                {
                    var processPaymentResult =
                        await GetProcessPaymentResultAsync(processPaymentRequest, placeOrderContainer)
                        ?? throw new NopException("processPaymentResult is not available");

                    if (processPaymentResult.Success)
                    {
                        var order = await SaveOrderDetailsAsync(processPaymentRequest, processPaymentResult,
                            placeOrderContainer);
                        result.PlacedOrder = order;

                        //move shopping cart items to order items
                        decimal orderitemDiscount = await MoveShoppingCartItemsToOrderItemsExtendedAsync(placeOrderContainer, order);

                        await UpdateProductSalesFromCartAsync(details);

                        await UpdateOrderChargesAsync(details, order, orderitemDiscount);

                        await ApplyMemberShipAsync(placeOrderContainer);

                        await ProcessAbandonedCartAndZohoLeadAsync(placeOrderContainer, order);
                        //discount usage history
                        await SaveDiscountUsageHistoryAsync(placeOrderContainer, order);

                        //gift card usage history
                        await SaveGiftCardUsageHistoryAsync(placeOrderContainer, order);

                        //recurring orders
                        if (placeOrderContainer.IsRecurringShoppingCart)
                            await CreateFirstRecurringPaymentAsync(processPaymentRequest, order);

                        //notifications
                        await SendNotificationsAndSaveNotesExtendedAsync(order);

                        //reset checkout data
                        await _customerService.ResetCheckoutDataAsync(placeOrderContainer.Customer,
                            processPaymentRequest.StoreId, clearCouponCodes: true, clearCheckoutAttributes: true);
                        await _customerActivityService.InsertActivityAsync("PublicStore.PlaceOrder",
                            string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.PlaceOrder"),
                                order.Id), order);

                        //raise event       
                        await _eventPublisher.PublishAsync(new OrderPlacedEvent(order));

                        //check order status
                        await CheckOrderStatusAsync(order);

                        if (order.PaymentStatus == PaymentStatus.Paid)
                            await ProcessOrderPaidAsync(order);
                    }
                    else
                        foreach (var paymentError in processPaymentResult.Errors)
                            result.AddError(string.Format(
                                await _localizationService.GetResourceAsync("Checkout.PaymentError"), paymentError));
                }
                catch (Exception exc)
                {
                    await _logger.ErrorAsync(exc.Message, exc);
                    result.AddError(exc.Message);
                }

                if (result.Success)
                    return result;

                //log errors
                var logError = result.Errors.Aggregate("Error while placing order. ",
                    (current, next) => $"{current}Error {result.Errors.IndexOf(next) + 1}: {next}. ");
                var customer = await _customerService.GetCustomerByIdAsync(processPaymentRequest.CustomerId);
                await _logger.ErrorAsync(logError, customer: customer);

                return result;
            }

            if (!_orderSettings.PlaceOrderWithLock)
                return await placeOrder(details);

            PlaceOrderResult result;
            var resource = details.Customer.Id.ToString();

            //the named mutex helps to avoid creating the same order in different threads,
            //and does not decrease performance significantly, because the code is blocked only for the specific cart.
            //you should be very careful, mutexes cannot be used in with the await operation
            //we can't use semaphore here, because it produces PlatformNotSupportedException exception on UNIX based systems
            using var mutex = new Mutex(false, resource);

            mutex.WaitOne();

            try
            {
                var cacheKey = _staticCacheManager.PrepareKey(NopOrderDefaults.OrderWithLockCacheKey, resource);
                cacheKey.CacheTime = _orderSettings.MinimumOrderPlacementInterval;

                var exist = _staticCacheManager.Get(cacheKey, () => false);

                if (exist)
                {
                    result = new PlaceOrderResult();
                    result.Errors.Add(_localizationService.GetResourceAsync("Checkout.MinOrderPlacementInterval").Result);
                }
                else
                {
                    result = placeOrder(details).Result;

                    if (result.Success)
                        _staticCacheManager.SetAsync(cacheKey, true).Wait();
                }
            }
            finally
            {
                mutex.ReleaseMutex();
            }

            return result;
        }


        #endregion

        #region Utilities
        protected virtual async Task<PlaceOrderContainer> PrepareCustomOrderPlaceOrderDetailsAsync(ProcessPaymentRequest processPaymentRequest,
            CustomOrder customOrder, dynamic orderSummary)
        {
            var details = new PlaceOrderContainer();
            var customer = await _customerService.GetCustomerByIdAsync(processPaymentRequest.CustomerId);
            #region Cim Profile
            try
            {
                var orderTypes = await _customOrderService.GetOrderTypes();
                string OrderType = (orderTypes.Where(o => o.Id == customOrder.OrderTypeId)).FirstOrDefault()?.Name;
                if (OrderType == OrderTypes.CustomOrder.ToString() && (customOrder.AlreadyFee ?? 0) > 0
                    && (customOrder.LiveOrderNumber ?? 0) == 0)
                {
                    // need to dynamic on the bases of order is partail or not 
                    processPaymentRequest.CreateCim = true;
                    processPaymentRequest.CustomOrderNumber = customOrder.Id;
                    processPaymentRequest.BillingAddressId = customer.BillingAddressId;
                    processPaymentRequest.ShippingAddressId = customer.ShippingAddressId;
                    //session save
                }
            }
            catch (Exception exp)
            {

            }

            #endregion


            var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
            await PrepareAndValidateCustomerAsync(details, processPaymentRequest, currentCurrency);
            await PrepareCustomOrderShoppingCartAndCheckoutAttributesAsync(details, processPaymentRequest, currentCurrency, customOrder);
            await PrepareAndValidateBillingAddressAsync(details);
            await PrepareCustomOrderShippingInfoAsync(details, processPaymentRequest);
            await PrepareCustomOrderValidateTotalsAsync(details, processPaymentRequest, customOrder, orderSummary);

            //affiliate
            var affiliate = await _affiliateService.GetAffiliateByIdAsync(details.Customer.AffiliateId);
            if (affiliate != null && affiliate.Active && !affiliate.Deleted)
                details.AffiliateId = affiliate.Id;

            //tax display type
            details.CustomerTaxDisplayType = await _customerService.GetCustomerTaxDisplayTypeAsync(details.Customer);

            //recurring or standard shopping cart?
            details.IsRecurringShoppingCart = await _shoppingCartService.ShoppingCartIsRecurringAsync(details.Cart);
            if (!details.IsRecurringShoppingCart)
                return details;

            await PrepareAndValidateRecurringShoppingAsync(details, processPaymentRequest);

            return details;
        }
        protected virtual async Task<Order> SaveCustomOrderDetailsAsync(ProcessPaymentRequest processPaymentRequest,
      ProcessPaymentResult processPaymentResult, PlaceOrderContainer details)
        {
            decimal total = details.OrderTotal;
            decimal subTotal = details.OrderSubTotalExclTax;
            decimal subTotalDiscount = details.OrderSubTotalDiscountExclTax;
            decimal shipping = details.OrderShippingTotalExclTax;
            decimal tax = details.OrderTaxTotal;
            decimal wgs = 0;
            decimal shippingDiscount = 0;
            decimal customDuty = 0;
            decimal customDutyPercentage = 0;
            string taxInfo = "";
            int orderId = 0;
            var orderIdObj = processPaymentRequest.CustomValues.Where(m => m.Name == "InternalOrderId").FirstOrDefault();
            if (orderIdObj != null)
                int.TryParse(orderIdObj.Value.ToString(), out orderId);
            if (orderId != 0)
            {
                var customorder = await _customOrderService.GetById(orderId);
                if (customorder != null)
                {
                    var obj = processPaymentRequest.CustomValues.Where(m => m.Name == "Wgs").FirstOrDefault();
                    if (obj != null)
                        decimal.TryParse(obj.Value.ToString(), out wgs);

                    obj = processPaymentRequest.CustomValues.Where(m => m.Name == "Tax").FirstOrDefault();
                    if (obj != null)
                        decimal.TryParse(obj.Value.ToString(), out tax);

                    obj = processPaymentRequest.CustomValues.Where(m => m.Name == "SubTotal").FirstOrDefault();
                    if (obj != null)
                        decimal.TryParse(obj.Value.ToString(), out subTotal);


                    obj = processPaymentRequest.CustomValues.Where(m => m.Name == "Total").FirstOrDefault();
                    if (obj != null)
                        decimal.TryParse(obj.Value.ToString(), out total);

                    obj = processPaymentRequest.CustomValues.Where(m => m.Name == "SubtotalDiscount").FirstOrDefault();
                    if (obj != null)
                        decimal.TryParse(obj.Value.ToString(), out subTotalDiscount);

                    obj = processPaymentRequest.CustomValues.Where(m => m.Name == "ShippingDiscount").FirstOrDefault();
                    if (obj != null)
                        decimal.TryParse(obj.Value.ToString(), out shippingDiscount);

                    obj = processPaymentRequest.CustomValues.Where(m => m.Name == "Shipping").FirstOrDefault();
                    if (obj != null)
                        decimal.TryParse(obj.Value.ToString(), out shipping);
                    obj = processPaymentRequest.CustomValues.Where(m => m.Name == "CustomDuty").FirstOrDefault();
                    if (obj != null)
                        decimal.TryParse(obj.Value.ToString(), out customDuty);
                    obj = processPaymentRequest.CustomValues.Where(m => m.Name == "CustomDutyPercentage").FirstOrDefault();
                    if (obj != null)
                        decimal.TryParse(obj.Value.ToString(), out customDutyPercentage);

                    obj = processPaymentRequest.CustomValues.Where(m => m.Name == "TaxInfo").FirstOrDefault();
                    if (obj != null)
                        taxInfo = obj.Value.ToString();
                }


            }

            var order = new Order
            {
                StoreId = processPaymentRequest.StoreId,
                OrderGuid = processPaymentRequest.OrderGuid,
                CustomerId = details.Customer.Id,
                CustomerLanguageId = details.CustomerLanguage.Id,
                CustomerTaxDisplayType = details.CustomerTaxDisplayType,
                CustomerIp = _webHelper.GetCurrentIpAddress(),
                OrderSubtotalInclTax = subTotal,
                OrderSubtotalExclTax = subTotal,
                OrderSubTotalDiscountInclTax = subTotalDiscount,
                OrderSubTotalDiscountExclTax = subTotalDiscount,
                OrderShippingInclTax = shipping,
                OrderShippingExclTax = shipping,
                PaymentMethodAdditionalFeeInclTax = details.PaymentAdditionalFeeInclTax,
                PaymentMethodAdditionalFeeExclTax = details.PaymentAdditionalFeeExclTax,
                TaxRates = details.TaxRates,
                OrderTax = tax,
                OrderTotal = total,
                RefundedAmount = decimal.Zero,
                OrderDiscount = details.OrderDiscountAmount,
                CheckoutAttributeDescription = details.CheckoutAttributeDescription,
                CheckoutAttributesXml = details.CheckoutAttributesXml,
                CustomerCurrencyCode = details.CustomerCurrencyCode,
                CurrencyRate = details.CustomerCurrencyRate,
                AffiliateId = details.AffiliateId,
                OrderStatus = OrderStatus.Pending,
                AllowStoringCreditCardNumber = processPaymentResult.AllowStoringCreditCardNumber,
                CardType = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardType) : string.Empty,
                CardName = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardName) : string.Empty,
                CardNumber = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardNumber) : string.Empty,
                MaskedCreditCardNumber = _encryptionService.EncryptText(_paymentService.GetMaskedCreditCardNumber(processPaymentRequest.CreditCardNumber)),
                CardCvv2 = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardCvv2) : string.Empty,
                CardExpirationMonth = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardExpireMonth.ToString()) : string.Empty,
                CardExpirationYear = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardExpireYear.ToString()) : string.Empty,
                PaymentMethodSystemName = processPaymentRequest.PaymentMethodSystemName,
                AuthorizationTransactionId = processPaymentResult.AuthorizationTransactionId,
                AuthorizationTransactionCode = processPaymentResult.AuthorizationTransactionCode,
                AuthorizationTransactionResult = processPaymentResult.AuthorizationTransactionResult,
                CaptureTransactionId = processPaymentResult.CaptureTransactionId,
                CaptureTransactionResult = processPaymentResult.CaptureTransactionResult,
                SubscriptionTransactionId = processPaymentResult.SubscriptionTransactionId,
                PaymentStatus = processPaymentResult.NewPaymentStatus,
                PaidDateUtc = null,
                PickupInStore = details.PickupInStore,
                ShippingStatus = details.ShippingStatus,
                ShippingMethod = details.ShippingMethodName,
                ShippingRateComputationMethodSystemName = details.ShippingRateComputationMethodSystemName,
                CustomValuesXml = processPaymentRequest.CustomValues.SerializeToXml(),
                VatNumber = details.VatNumber,
                CreatedOnUtc = DateTime.UtcNow,
                CustomOrderNumber = string.Empty,
                WgsChargesInclTax = wgs,
                ShippingDiscountInclTax = shippingDiscount,
                IsCustomOrder = true,
                CustomDutyExclTax = customDuty,
                CustomDutyInclTax = customDuty,
                CustomDutyPercentage = customDutyPercentage,
                TaxInfo = taxInfo
            };

            if (details.BillingAddress is null)
                throw new NopException("Billing address is not provided");

            await _addressService.InsertAddressAsync(details.BillingAddress);
            order.BillingAddressId = details.BillingAddress.Id;

            if (details.PickupAddress != null)
            {
                await _addressService.InsertAddressAsync(details.PickupAddress);
                order.PickupAddressId = details.PickupAddress.Id;
            }

            if (details.ShippingAddress != null)
            {
                await _addressService.InsertAddressAsync(details.ShippingAddress);
                order.ShippingAddressId = details.ShippingAddress.Id;
            }

            await _orderService.InsertOrderAsync(order);

            //generate and set custom order number
            order.CustomOrderNumber = _customNumberFormatter.GenerateOrderCustomNumber(order);
            await _orderService.UpdateOrderAsync(order);

            //reward points history
            if (details.RedeemedRewardPointsAmount <= decimal.Zero)
                return order;

            order.RedeemedRewardPointsEntryId = await _rewardPointService.AddRewardPointsHistoryEntryAsync(details.Customer, -details.RedeemedRewardPoints, order.StoreId,
                string.Format(await _localizationService.GetResourceAsync("RewardPoints.Message.RedeemedForOrder", order.CustomerLanguageId), order.CustomOrderNumber),
                order, details.RedeemedRewardPointsAmount);
            await _customerService.UpdateCustomerAsync(details.Customer);
            await _orderService.UpdateOrderAsync(order);

            return order;
        }
        protected virtual async Task CustomOrderMoveShoppingCartItemsToOrderItemsAsync(PlaceOrderContainer details, Order order
           , CustomOrder customOrder)
        {

            var items = await _customOrderService.GetOrderItems(customOrder.Id);
            foreach (var _item in items)
            {
                string notes = "";

                var product = await _productService.GetProductByIdAsync(_item.ProductId);
                var _priceAdjustment = await _customOrderService.GetPriceAdjustmentsByCartId(_item.Id);
                decimal price = 0;
                decimal totalAdjustment = 0;
                if (_priceAdjustment != null)
                {
                    decimal discountAmount = _priceAdjustment.Discountamount == null ? 0 : Convert.ToDecimal(_priceAdjustment.Discountamount);
                    decimal discountPercentage = _priceAdjustment.DiscountPercentage == null ? 0 : Convert.ToDecimal(_priceAdjustment.DiscountPercentage);
                    price = _priceAdjustment.ShoppingCartProductPrice == null ? 0 : Convert.ToDecimal(_priceAdjustment.ShoppingCartProductPrice);
                    totalAdjustment = (_priceAdjustment.Discounttype == DiscountType.Percentage.ToString() && discountPercentage != 0 ?
                       (_priceAdjustment.ShoppingCartProductPrice == null ? 0 : Convert.ToDecimal(_priceAdjustment.ShoppingCartProductPrice)
                       * Convert.ToDecimal(_priceAdjustment.DiscountPercentage)) / 100
                       : (_priceAdjustment.Discounttype == DiscountType.Fixed.ToString() && discountAmount != 0
                       ? discountAmount : 0));


                    if (_priceAdjustment.Chargestype == ChargeType.Subtract.ToString())
                        totalAdjustment = -totalAdjustment;


                    notes = _priceAdjustment.AdjustmentNotes;
                }

                var orderItem = new OrderItem
                {
                    OrderItemGuid = Guid.NewGuid(),
                    OrderId = order.Id,
                    ProductId = product.Id,
                    UnitPriceInclTax = price,
                    UnitPriceExclTax = price,
                    PriceInclTax = price,
                    PriceExclTax = price,
                    ItemPriceIncTax = price,
                    AttributeDescription = string.IsNullOrEmpty(_item.AttributesDescription) ? _item.CustomAttributesDescription : _item.AttributesDescription,
                    AttributesXml = "",
                    Quantity = _item.Quantity,
                    DiscountAmountInclTax = 0,
                    DiscountAmountExclTax = 0,
                    DownloadCount = 0,
                    IsDownloadActivated = false,
                    LicenseDownloadId = 0,
                    ItemWeight = await _shippingService.GetShoppingCartItemWeightAsync(product, null, false),
                    RentalStartDateUtc = null,
                    RentalEndDateUtc = null,
                    OfferDiscountIncTax = totalAdjustment * _item.Quantity
                };

                await _orderService.InsertOrderItemAsync(orderItem);

            }
            //clear shopping cart
            details.Cart.ToList().ForEach(async sci => await _shoppingCartService.DeleteShoppingCartItemAsync(sci, false));
        }
        protected virtual async Task PrepareCustomOrderShoppingCartAndCheckoutAttributesAsync(PlaceOrderContainer details, ProcessPaymentRequest processPaymentRequest, Currency currentCurrency, CustomOrder customOrder)
        {
            //checkout attributes
            details.CheckoutAttributesXml = await _genericAttributeService.GetAttributeAsync<string>(details.Customer, NopCustomerDefaults.CheckoutAttributes, processPaymentRequest.StoreId);
            details.CheckoutAttributeDescription = await _checkoutAttributeFormatter.FormatAttributesAsync(details.CheckoutAttributesXml, details.Customer);

            await CustomOrderAddItemsToCart(customOrder, details.Customer, processPaymentRequest);
            //load shopping cart
            details.Cart = await _shoppingCartService.GetShoppingCartAsync(details.Customer, ShoppingCartType.ShoppingCart, processPaymentRequest.StoreId);

            if (!details.Cart.Any())
                throw new NopException("Cart is empty");
        }
        public async Task CustomOrderAddItemsToCart(CustomOrder order, Customer customer,
     ProcessPaymentRequest processPaymentRequest)
        {
            var cartItems = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, processPaymentRequest.StoreId);
            foreach (var cartItem in cartItems)
            {
                await _shoppingCartService.DeleteShoppingCartItemAsync(cartItem);
            }

            var items = await _customOrderService.GetOrderItems(order.Id);
            foreach (var _item in items)
            {
                var product = await _productService.GetProductByIdAsync(_item.ProductId);
                var _priceAdjustment = await _customOrderService.GetPriceAdjustmentsByCartId(_item.Id);
                decimal price = 0;
                if (_priceAdjustment != null)
                {
                    decimal discountAmount = _priceAdjustment.Discountamount == null ? 0 : Convert.ToDecimal(_priceAdjustment.Discountamount);
                    decimal discountPercentage = _priceAdjustment.DiscountPercentage == null ? 0 : Convert.ToDecimal(_priceAdjustment.DiscountPercentage);
                    price = _priceAdjustment.ShoppingCartProductPrice == null ? 0 : Convert.ToDecimal(_priceAdjustment.ShoppingCartProductPrice);
                    decimal totalAdjustment = (_priceAdjustment.Discounttype == DiscountType.Percentage.ToString() && discountPercentage != 0 ?
                        (_priceAdjustment.ShoppingCartProductPrice == null ? 0 : Convert.ToDecimal(_priceAdjustment.ShoppingCartProductPrice)
                        * Convert.ToDecimal(_priceAdjustment.DiscountPercentage)) / 100
                        : (_priceAdjustment.Discounttype == DiscountType.Fixed.ToString() && discountAmount != 0
                        ? discountAmount : 0));


                    if (_priceAdjustment.Chargestype == ChargeType.Subtract.ToString())
                        price = ((
        _priceAdjustment.ShoppingCartProductPrice == null ? 0 : Convert.ToDecimal(_priceAdjustment.ShoppingCartProductPrice)))
        - totalAdjustment;
                    else if (_priceAdjustment.Chargestype == ChargeType.Add.ToString())
                        price = ((
     _priceAdjustment.ShoppingCartProductPrice == null ? 0 : Convert.ToDecimal(_priceAdjustment.ShoppingCartProductPrice)))
     + totalAdjustment;

                    else
                        price = ((
  _priceAdjustment.ShoppingCartProductPrice == null ? 0 : Convert.ToDecimal(_priceAdjustment.ShoppingCartProductPrice)));



                }
                await _shoppingCartExtendedCartService.CustomAddToCartAsync(customer, product, ShoppingCartType.ShoppingCart, processPaymentRequest.StoreId,
               String.IsNullOrEmpty(_item.AttributesDescription) ?
                _item.CustomAttributesDescription : _item.AttributesDescription, price, null, null, _item.Quantity, false);
            }
        }
        protected virtual async Task CustomOrderSendNotificationsAndSaveNotesAsync(Order order, CustomOrder customOrder)
        {
            //notes, messages
            await AddOrderNoteAsync(order, _workContext.OriginalCustomerIfImpersonated != null
                ? $"Order placed by a store owner ('{_workContext.OriginalCustomerIfImpersonated.Email}'. ID = {_workContext.OriginalCustomerIfImpersonated.Id}) impersonating the customer."
                : "Order placed");

            //send email notifications
            var orderPlacedStoreOwnerNotificationQueuedEmailIds = await _workflowMessageService.SendOrderPlacedStoreOwnerNotificationAsync(order, _localizationSettings.DefaultAdminLanguageId);
            if (orderPlacedStoreOwnerNotificationQueuedEmailIds.Any())
                await AddOrderNoteAsync(order, $"\"Order placed\" email (to store owner) has been queued. Queued email identifiers: {string.Join(", ", orderPlacedStoreOwnerNotificationQueuedEmailIds)}.");

            var orderPlacedAttachmentFilePath = _orderSettings.AttachPdfInvoiceToOrderPlacedEmail ?
              (await _pdfService.SaveOrderPdfToDiskAsync(order)) : null;

            var orderPlacedAttachmentFileName = _orderSettings.AttachPdfInvoiceToOrderPlacedEmail ?
                (string.Format(await _localizationService.GetResourceAsync("PDFInvoice.FileName"), order.CustomOrderNumber) + ".pdf") : null;


            var vendors = await GetVendorsInOrderAsync(order);
            foreach (var vendor in vendors)
            {
                var orderPlacedVendorNotificationQueuedEmailIds = await _workflowMessageService.SendOrderPlacedVendorNotificationAsync(order, vendor, _localizationSettings.DefaultAdminLanguageId);
                if (orderPlacedVendorNotificationQueuedEmailIds.Any())
                    await AddOrderNoteAsync(order, $"\"Order placed\" email (to vendor) has been queued. Queued email identifiers: {string.Join(", ", orderPlacedVendorNotificationQueuedEmailIds)}.");
            }

            if (order.AffiliateId == 0)
                return;

            var orderPlacedAffiliateNotificationQueuedEmailIds = await _workflowMessageService.SendOrderPlacedAffiliateNotificationAsync(order, _localizationSettings.DefaultAdminLanguageId);
            if (orderPlacedAffiliateNotificationQueuedEmailIds.Any())
                await AddOrderNoteAsync(order, $"\"Order placed\" email (to affiliate) has been queued. Queued email identifiers: {string.Join(", ", orderPlacedAffiliateNotificationQueuedEmailIds)}.");


        }
        protected virtual async Task PrepareCustomOrderShippingInfoAsync(PlaceOrderContainer details, ProcessPaymentRequest processPaymentRequest)
        {
            //shipping info


            if (details.Customer.ShippingAddressId == null)
                throw new NopException("Shipping address is not provided");

            var shippingAddress = await _customerService.GetCustomerShippingAddressAsync(details.Customer);

            if (!CommonHelper.IsValidEmail(shippingAddress?.Email))
                throw new NopException("Email is not valid");

            //clone shipping address
            details.ShippingAddress = _addressService.CloneAddress(shippingAddress);

            if (await _countryService.GetCountryByAddressAsync(details.ShippingAddress) is Country shippingCountry && !shippingCountry.AllowsShipping)
                throw new NopException($"Country '{shippingCountry.Name}' is not allowed for shipping");
            details.ShippingStatus = ShippingStatus.NotYetShipped;

        }
        protected virtual async Task PrepareCustomOrderValidateTotalsAsync(PlaceOrderContainer details, ProcessPaymentRequest processPaymentRequest,
            CustomOrder customOrder, dynamic orderSummary)
        {
            #region Order totals

            decimal subTotal = 0;
            decimal subtotalDiscount = 0;
            decimal shipping = 0;
            decimal shippingDiscount = 0;
            string shippingMethod = "";
            decimal wgs = 0;
            decimal tax = 0;
            decimal payableAmount = 0;

            if (customOrder.Shipping != null && customOrder.Shipping > 0)
                shipping = Convert.ToDecimal(customOrder.Shipping);
            if (customOrder.Wgs != null && customOrder.Wgs > 0)
                wgs = Convert.ToDecimal(customOrder.Wgs);
            if (customOrder.OrderTax != null && customOrder.OrderTax > 0)
                tax = Convert.ToDecimal(customOrder.OrderTax);

            if (wgs != 0)
                processPaymentRequest.CustomValues.Add(new CustomValue("Wgs", wgs.ToString()));
            if (shipping != 0)
                processPaymentRequest.CustomValues.Add(new CustomValue("Shipping", shipping.ToString()));
            if (tax != 0)
                processPaymentRequest.CustomValues.Add(new CustomValue("Tax", tax.ToString()));

            processPaymentRequest.CustomValues.Add(new CustomValue("CustomDuty", customOrder.CustomDuty.ToString()));
            processPaymentRequest.CustomValues.Add(new CustomValue("CustomDutyPercentage", customOrder.CustomDutyPercentage.ToString()));
            processPaymentRequest.CustomValues.Add(new CustomValue("TaxInfo", customOrder.TaxInfo ?? "".ToString()));
            shippingMethod = customOrder.ShippingMethod;




            if (!string.IsNullOrEmpty((string)orderSummary.SubTotal))
            {
                decimal.TryParse(orderSummary.SubTotal, NumberStyles.Currency,
             CultureInfo.CurrentCulture.NumberFormat, out subTotal);
            }
            processPaymentRequest.CustomValues.Add(new CustomValue("SubTotal", subTotal.ToString()));

            if (!string.IsNullOrEmpty((string)orderSummary.PayableAmount))
            {
                decimal.TryParse(orderSummary.PayableAmount, NumberStyles.Currency,
             CultureInfo.CurrentCulture.NumberFormat, out payableAmount);
            }
            processPaymentRequest.CustomValues.Add(new CustomValue("Total", payableAmount.ToString()));
            if (orderSummary.SubTotalDiscountDetails != null)
            {
                if (!string.IsNullOrEmpty((string)orderSummary.SubTotalDiscountDetails.TotalAdjustment))
                {
                    decimal.TryParse(orderSummary.SubTotalDiscountDetails.TotalAdjustment, NumberStyles.Currency,
                 CultureInfo.CurrentCulture.NumberFormat, out subtotalDiscount);

                    if (subtotalDiscount != 0)
                        processPaymentRequest.CustomValues.Add(new CustomValue("SubtotalDiscount", subtotalDiscount < 0 ? "-" : "" + subtotalDiscount.ToString()));

                    if (((string)orderSummary.SubTotalDiscountDetails.ChargeType) == ChargeType.Add.ToString())
                    {
                        subTotal = subTotal + subtotalDiscount;
                        subtotalDiscount = 0;
                    }
                }
            }



            if (orderSummary.ShippingDiscountDetails != null)
            {
                if (!string.IsNullOrEmpty((string)orderSummary.ShippingDiscountDetails.TotalAdjustment))
                {
                    decimal.TryParse(orderSummary.ShippingDiscountDetails.TotalAdjustment, NumberStyles.Currency,
                 CultureInfo.CurrentCulture.NumberFormat, out shippingDiscount);
                    if (shippingDiscount != 0)
                        processPaymentRequest.CustomValues.Add(new CustomValue("ShippingDiscount", shippingDiscount < 0 ? "-" : "" + shippingDiscount.ToString()));
                    if (((string)orderSummary.ShippingDiscountDetails.ChargeType) == ChargeType.Add.ToString())
                    {
                        shipping = shipping + shippingDiscount;
                        shippingDiscount = 0;
                    }
                }
            }

            details.OrderSubTotalInclTax = subTotal;
            details.OrderSubTotalDiscountInclTax = subtotalDiscount;

            details.OrderSubTotalExclTax = subTotal;
            details.OrderSubTotalDiscountExclTax = subtotalDiscount;

            details.ShippingMethodName = shippingMethod;
            details.ShippingRateComputationMethodSystemName = shippingMethod;


            details.OrderShippingTotalInclTax = shipping;
            details.OrderShippingTotalExclTax = shipping;
            details.OrderTaxTotal = tax;
            details.OrderTotal = payableAmount;
            processPaymentRequest.CustomValues.Add(new CustomValue("InternalOrderId", customOrder.Id.ToString()));
            processPaymentRequest.OrderTotal = details.OrderTotal;
            #endregion
        }
        protected virtual async Task<decimal> MoveShoppingCartItemsToOrderItemsExtendedAsync(PlaceOrderContainer details, Order order)
        {
            decimal orderitemDiscount = 0;
            var subTotalIncludingTax = await _workContext.GetTaxDisplayTypeAsync() == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal;
            var (orderSubTotalDiscountAmountBase, _, subTotal, _, _) = await _orderTotalCalculationService.GetShoppingCartSubTotalAsync(details.Cart, subTotalIncludingTax);
            var customShoppingcartTotal = await this._shoppingCartExtendedCartService.GetCustomCartItemTotalsAsync(details.Cart, subTotal, details.Customer);

            foreach (var sc in details.Cart)
            {
                var product = await _productService.GetProductByIdAsync(sc.ProductId);

                //prices
                var scUnitPrice = (await _shoppingCartService.GetUnitPriceAsync(sc, true)).unitPrice;
                var (scSubTotal, discountAmount, scDiscounts, _) = await _shoppingCartService.GetSubTotalAsync(sc, true);
                var scUnitPriceInclTax = await _taxService.GetProductPriceAsync(product, scUnitPrice, true, details.Customer);
                var scUnitPriceExclTax = await _taxService.GetProductPriceAsync(product, scUnitPrice, false, details.Customer);
                var scSubTotalInclTax = await _taxService.GetProductPriceAsync(product, scSubTotal, true, details.Customer);
                var scSubTotalExclTax = await _taxService.GetProductPriceAsync(product, scSubTotal, false, details.Customer);
                var discountAmountInclTax = await _taxService.GetProductPriceAsync(product, discountAmount, true, details.Customer);
                var discountAmountExclTax = await _taxService.GetProductPriceAsync(product, discountAmount, false, details.Customer);
                foreach (var disc in scDiscounts)
                    if (!_discountService.ContainsDiscount(details.AppliedDiscounts, disc))
                        details.AppliedDiscounts.Add(disc);

                //attributes
                var store = await _storeService.GetStoreByIdAsync(sc.StoreId);
                var attributeDescription = await _productAttributeFormatter.FormatAttributesAsync(product, sc.AttributesXml, details.Customer, store);

                var itemWeight = await _shippingService.GetShoppingCartItemWeightAsync(sc);

                //save order item
                var orderItem = new OrderItem
                {
                    OrderItemGuid = Guid.NewGuid(),
                    OrderId = order.Id,
                    ProductId = product.Id,
                    UnitPriceInclTax = scUnitPriceInclTax.price,
                    UnitPriceExclTax = scUnitPriceExclTax.price,
                    PriceInclTax = scSubTotalInclTax.price,
                    PriceExclTax = scSubTotalExclTax.price,
                    OriginalProductCost = await _priceCalculationService.GetProductCostAsync(product, sc.AttributesXml),
                    AttributeDescription = attributeDescription,
                    AttributesXml = sc.AttributesXml,
                    Quantity = sc.Quantity,
                    DiscountAmountInclTax = discountAmountInclTax.price,
                    DiscountAmountExclTax = discountAmountExclTax.price,
                    DownloadCount = 0,
                    IsDownloadActivated = false,
                    LicenseDownloadId = 0,
                    ItemWeight = itemWeight,
                    RentalStartDateUtc = sc.RentalStartDateUtc,
                    RentalEndDateUtc = sc.RentalEndDateUtc
                };

                #region CustomLogic
                try
                {
                    var item = customShoppingcartTotal.FirstOrDefault(x => x.Id == sc.Id);
                    if (item != null)
                    {


                        orderItem.ItemPriceIncTax = await _currencyService.ConvertToPrimaryStoreCurrencyAsync(string.IsNullOrEmpty(item.ItemTotal) ? 0 : (Decimal.Parse(item.ItemTotal.Replace("\"", ""), NumberStyles.Currency) / item.Quantity), await _workContext.GetWorkingCurrencyAsync());
                        orderItem.MembershipDiscountIncTax = await _currencyService.ConvertToPrimaryStoreCurrencyAsync(string.IsNullOrEmpty(item.MembershipDiscount) ? 0 : Decimal.Parse(item.MembershipDiscount.Replace("\"", ""), NumberStyles.Currency), await _workContext.GetWorkingCurrencyAsync());
                        orderItem.OfferDiscountIncTax = await _currencyService.ConvertToPrimaryStoreCurrencyAsync(string.IsNullOrEmpty(item.OfferDiscount) ? 0 : Decimal.Parse(item.OfferDiscount.Replace("\"", ""), NumberStyles.Currency), await _workContext.GetWorkingCurrencyAsync());
                        orderItem.BuyMoreSaveMoreDiscountIncTax = await _currencyService.ConvertToPrimaryStoreCurrencyAsync(string.IsNullOrEmpty(item.BuyMoreSaveMoreDiscount) ? 0 : Decimal.Parse(item.BuyMoreSaveMoreDiscount.Replace("\"", ""), NumberStyles.Currency), await _workContext.GetWorkingCurrencyAsync());
                        orderItem.SpecialInstructions = sc.SpecialInstructions;
                        if ((orderItem.DiscountAmountExclTax > 0 || orderItem.DiscountAmountInclTax > 0)
                                       && orderItem.UnitPriceInclTax > 0)
                        {
                            var price = (orderItem.UnitPriceInclTax * orderItem.Quantity) + orderItem.DiscountAmountInclTax;
                            var percentage = (orderItem.DiscountAmountInclTax / price) * 100;
                            price = (orderItem.ItemPriceIncTax * orderItem.Quantity) + orderItem.MembershipDiscountIncTax + orderItem.OfferDiscountIncTax + orderItem.BuyMoreSaveMoreDiscountIncTax;
                            orderItem.TotalDiscount = Math.Round((price * percentage) / 100, 2);
                            orderitemDiscount += orderItem.TotalDiscount;
                        }

                    }
                }
                catch (Exception ex)
                {
                    await _logger.ErrorAsync(
            $"Failed to update order item pricing/discounts. " +
            $"Order Id: {order.Id}, " +
            $"ShoppingCartItemId: {sc.Id}, " +
            $"OrderItemId: {orderItem.Id}, " +
            $"ProductId: {orderItem.ProductId}",
            ex);
                }
                #endregion

                await _orderService.InsertOrderItemAsync(orderItem);

                //gift cards
                await AddGiftCardsAsync(product, sc.AttributesXml, sc.Quantity, orderItem, scUnitPriceExclTax.price);

                //inventory
                await _productService.AdjustInventoryAsync(product, -sc.Quantity, sc.AttributesXml,
                    string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.PlaceOrder"), order.Id));

                await _eventPublisher.PublishAsync(new ShoppingCartItemMovedToOrderItemEvent(sc, orderItem));
            }

            await _shoppingCartService.ClearShoppingCartAsync(details.Customer, order.StoreId);
            return orderitemDiscount;
        }

        #region Order
        protected virtual async Task UpdateOrderChargesAsync(PlaceOrderContainer details, Order order, decimal orderitemDiscount)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            (decimal buyMoreSaveMoreDiscount, decimal membershipdiscount, decimal offerDiscount, decimal offerDiscountDefault, decimal productItemsDiscount, _) =
                await _orderTotalCalculationExtendedService.GetCustomBuyMoreSaveMoreDiscountAndMemberShipDiscountAsync(details.Cart);

            (decimal customDutyPercentage, decimal customDuty) = await _orderTotalCalculationExtendedService.GetCustomDuty(details.Cart);
            decimal memberShipFee = 0;
            decimal memberShipFeeDiscount = 0;
            if (await _customerExtendedService.IsMemberShipAddedInCart(details.Customer))
                (memberShipFee, memberShipFeeDiscount) = await _orderTotalCalculationExtendedService.GetMemberShipFee();

            var shippingOption = await _genericAttributeService.GetAttributeAsync<ShippingOption>(details.Customer,
        NopCustomerDefaults.SelectedShippingOptionAttribute, store.Id);

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


            #region SubTotalDiscount

            var customSubTotal = await _orderTotalCalculationExtendedService.GetCustomShoppingCartSubTotalAsync(details.Cart);
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

            var (shoppingCartTaxBase, taxRates, taxes) = await _orderTotalCalculationExtendedService.CustomGetTaxTotalAsync(details.Cart);
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
                await _logger.InsertLogAsync(LogLevel.Error, $"Order {order.Id} failed to update tax info", exp.Message);
            }
            #endregion

            order.OrderSubtotalExclTax = customSubTotal;
            order.OrderSubtotalInclTax = customSubTotal;
            order.CustomerEmail = await _customerExtendedService.GetCustomerEmailAsync(details.Customer);
            await _orderService.UpdateOrderAsync(order);

        }
        protected virtual async Task SendNotificationsAndSaveNotesExtendedAsync(Order order)
        {
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
            await this.CustomSendNotificationsAndSaveNotesAsync(order);
        }
        protected virtual async Task ApplyMemberShipAsync(PlaceOrderContainer details)
        {
            if (await _customerExtendedService.IsMemberShipAddedInCart(details.Customer))
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
                        CustomerId = details.Customer.Id,
                        CustomerRoleId = customerRole.Id
                    });
                await this._genericAttributeService.SaveAttributeAsync(details.Customer, NopCustomerDefaults.MemberShipLabelAttribute, false,
                    (await _storeContext.GetCurrentStoreAsync()).Id);
            }

        }
        protected virtual async Task ProcessAbandonedCartAndZohoLeadAsync(PlaceOrderContainer details, Order order)
        {
            var orderItems = await _orderService.GetOrderItemsAsync(order.Id);
            string description = "";
            foreach (var item in orderItems)
            {
                var product = await _productService.GetProductByIdAsync(item.ProductId);
                description += "ProductId:" + item.ProductId + ";SKU:" + product?.Sku ?? "" + "|";
            }
            var iPAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress == null ? "" : _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            await _zohoService.CreateUpdateOrderContactPotential(order.Id, string.Empty, details.Customer, "Paid", description, iPAddress, string.Empty, order.OrderTotal, -1, string.Empty, false);

            await _abandonedCartService.MarkAbandonedInvoiceAsPaid(order.CustomerId, details.Cart.Select(c => c.Id).ToArray(), order.Id, order.OrderTotal);
        }
        protected virtual async Task UpdateProductSalesFromCartAsync(PlaceOrderContainer details)
        {
            foreach (var item in details.Cart)
            {
                var product = await _productService.GetProductByIdAsync(item.ProductId);
                if (product != null)
                {
                    product.NoOfSales = product.NoOfSales + item.Quantity;
                    await _productService.UpdateProductAsync(product);
                }

            }
        }

        #endregion

        #region CustomOrder

        protected async Task UpdateCustomOrderChargesAsync(string paymentMethodName, PlaceOrderContainer details, PlaceOrderResult result, ProcessPaymentResult processPaymentResult,
            ProcessPaymentRequest processPaymentRequest, Order orderEntity
           , CustomOrder order, dynamic orderSummary, int paidBy)
        {

            decimal amountPaid = processPaymentRequest.OrderTotal;
            #region Update Product Sales

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

            #endregion

            var orderStatuses = await _customOrderService.GetOrderStatuses();
            var paidStatus = orderStatuses.Where(m => m.Name == MWT.Nop.Core.Domain.CustomOrders.OrderStatus.Paid.ToString()).FirstOrDefault();


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
                    await _manageService.SyncPendingOrderPartialPayment(orderEntity.Id, processPaymentResult.CaptureTransactionId, processPaymentRequest.OrderTotal, DateTime.Now, paymentMethodName);
                }
            }


            if (order.ParentOrderID > 0)
                orderEntity.ParentOrderID = order.ParentOrderID;
            string email = await this._customerExtendedService.GetCustomerEmailAsync(details.Customer);
            orderEntity.CustomerEmail = email;

            var orderTypes = await _customOrderService.GetOrderTypes();
            var orderType = orderTypes.Where(t => t.Id == order.OrderTypeId).FirstOrDefault();
            if (orderType != null && orderType.Name != OrderTypes.CustomOrder.ToString())
            {
                orderEntity.OrderStatus = OrderStatus.Processing;
                orderEntity.PaymentStatus = PaymentStatus.Paid;
            }
            await this._orderService.UpdateOrderAsync(orderEntity);

            if (paidStatus != null)
                order.StatusId = paidStatus.Id;

            order.LiveOrderNumber = orderEntity.Id;
            order.FullPaid = fullPaid;

            if (order.CreatedOn == null)
                order.CreatedOn = DateTime.Now;
            if (order.ParentOrderID == 0 &&
                (order.SubOrderTypeId ?? 0) == 0 && (orderSummary.OrderType == OrderTypes.CustomOrder.ToString() || orderSummary.OrderType == OrderTypes.AlreadyPaid.ToString())
                )
            {
                string description = "";
                foreach (var item in _items)
                {
                    var product = await _productService.GetProductByIdAsync(item.ProductId);
                    description += "ProductId:" + item.ProductId + ";SKU:" + product?.Sku ?? "" + "|";
                }
                var iPAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress == null ? "" : _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
                order.ZohoPotentialId = await _zohoService.CreateUpdateOrderContactPotential(order.LiveOrderNumber ?? order.Id, order.ZohoPotentialId, details.Customer, MWT.Nop.Core.Domain.CustomOrders.OrderStatus.Paid.ToString(), description, iPAddress, string.Empty, (order.SubTotal ?? 0) + (order.TotalDiscount ?? 0), order.CreatedBy, $"{_storeContext.GetCurrentStore().Url}checkoutCustomOrder?orderid={order.Id}&customerid={order.CustomerId}", true);

            }

            await _customOrderService.UpdateAsync(order);

            #region Manage Update Status of Order

            if (order.ParentOrderID > 0)
            {
                await _manageService.MarkWgsAsPaid(order.ParentOrderID, email, order.OrderTotal ?? 0, order.PairedOrderIds ?? string.Empty);
            }

            #endregion
            if (orderSummary.OrderType != OrderTypes.HouzzOrder.ToString())
            {
                var orderPlacedCustomerNotificationQueuedEmailIds = await _customWorkflowMessageService
             .CustomOrder_SendCustomerNotificationAsync(order, orderEntity.CustomerLanguageId);
            }

            if (processPaymentResult != null && (!string.IsNullOrEmpty(processPaymentResult.AuthorizationTransactionId) || !string.IsNullOrEmpty(processPaymentResult.CaptureTransactionId)))
                await this._customOrderService.InsertOrderStatusLogAsync(new CustomorderOrderStatusLog()
                {
                    CreatedOn = DateTime.UtcNow,
                    OrderId = order.Id,
                    StatusId = paidStatus != null ? paidStatus.Id : 0,
                    UserId = paidBy,
                    AmountPaid = amountPaid,
                    PaymentResponse = processPaymentResult == null ? string.Empty : JsonConvert.SerializeObject(processPaymentResult)
                });
        }

        #endregion

        #endregion
    }
}
