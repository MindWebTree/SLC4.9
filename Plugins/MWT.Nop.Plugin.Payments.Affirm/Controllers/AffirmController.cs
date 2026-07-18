using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Plugin.Payments.Affirm.Data.Domain;
using MWT.Nop.Plugin.Payments.Affirm.Domain;
using MWT.Nop.Plugin.Payments.Affirm.Infrastructure;
using MWT.Nop.Plugin.Payments.Affirm.Models;
using MWT.Nop.Plugin.Payments.Affirm.Services;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Customers;

//using Nop.Core.Domain.Customization.PaymentMethod;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Core.Http.Extensions;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
//using Nop.Services.Customizations.Phone_Order;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
//using Nop.Web.Areas.CustomOrder.Factories;
using Nop.Web.Framework.Controllers;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static iTextSharp.text.pdf.AcroFields;

namespace MWT.Nop.Plugin.Payments.Affirm.Controllers
{
    public class AffirmController : BasePaymentController
    {
        #region Fields

        private readonly IRepository<AffirmLog> _affirmLogRepository;
        private readonly IRepository<PaymentMethodSession> _affirmSessionRepository;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAffirmService _affirmService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreContext _storeContext;
        private readonly IProductService _productService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ServiceManager _serviceManager;
        private readonly ILocalizationService _localizationService;
      //  private readonly ICustomOrderService _customOrderService;
       private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
       // private readonly ICustomOrderModelFactory _customOrderModelFactory;
        AffirmCheckoutSettings _affirmSettings;

        #endregion

        #region Ctor

        public AffirmController(IRepository<AffirmLog> affirmLogRepository, IWebHelper webHelper, IWorkContext workContext,
                               IHttpContextAccessor httpContextAccessor, IAffirmService affirmService, IRepository<PaymentMethodSession> affirmSessionRepository,
                               IShoppingCartService shoppingCartService, IStoreContext storeContext, IProductService productService,
                               IOrderProcessingService orderProcessingService, IPaymentPluginManager paymentPluginManager,
                               IGenericAttributeService genericAttributeService,
                               ServiceManager serviceManager, ILocalizationService localizationService,
                            //   ICustomOrderService customOrderService,
                               ICustomerService customerService,
                           //    ICustomOrderModelFactory customOrderModelFactory,
                               AffirmCheckoutSettings affirmSettings,
                               OrderService orderService
                               )
        {
            _affirmLogRepository = affirmLogRepository;
            _webHelper = webHelper;
            _workContext = workContext;
            _httpContextAccessor = httpContextAccessor;
            _affirmService = affirmService;
            _affirmSessionRepository = affirmSessionRepository;
            _shoppingCartService = shoppingCartService;
            _storeContext = storeContext;
            _productService = productService;
            _orderProcessingService = orderProcessingService;
            _paymentPluginManager = paymentPluginManager;
            _genericAttributeService = genericAttributeService;
            _serviceManager = serviceManager;
            _localizationService = localizationService;
         //   _customOrderService = customOrderService;
            _customerService = customerService;
         //   _customOrderModelFactory = customOrderModelFactory;
            _affirmSettings = affirmSettings;
            _orderService = orderService;
        }

        #endregion

        #region Methods


        public async Task<IActionResult> ConfirmCallbackHandler()
        {
            try
            {
                if (await _paymentPluginManager.LoadPluginBySystemNameAsync("Payments.Affirm") is not AffirmPaymentMethod processor || !_paymentPluginManager.IsPluginActive(processor))
                    throw new NopException("Affirm module cannot be loaded");
                string token = _webHelper.QueryString<string>("checkout_token");
                if (string.IsNullOrEmpty(token))
                {
                    await _affirmLogRepository.InsertAsync(new AffirmLog()
                    {
                        CreatedOnUtc = DateTime.UtcNow,
                        CustomerId = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0,
                        FullMessage = "Failed to read Checkout Token",
                        IpAddress = _webHelper.GetCurrentIpAddress(),
                        LogLevel = LogLevel.Error,
                        ReferrerUrl = _webHelper.GetUrlReferrer(),
                        PageUrl = _webHelper.GetRawUrl(_httpContextAccessor.HttpContext.Request),
                        ShortMessage = "Affirm Order, Token Missed"

                    });
                    return RedirectToRoute("CheckoutOnePage");
                }
                await _affirmLogRepository.InsertAsync(new AffirmLog()
                {
                    CreatedOnUtc = DateTime.UtcNow,
                    CustomerId = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0,
                    FullMessage = $"Checkout Token {token}",
                    IpAddress = _webHelper.GetCurrentIpAddress(),
                    LogLevel = LogLevel.Information,
                    ReferrerUrl = _webHelper.GetUrlReferrer(),
                    PageUrl = _webHelper.GetRawUrl(_httpContextAccessor.HttpContext.Request),
                    ShortMessage = "Affirm Confirmation Handler Intialized."

                });
                var responseModel = await _affirmService.CheckoutDetails(token);
                if ((responseModel?.checkout_status ?? string.Empty) != AffirmOrderStatus.confirmed.ToString())
                {
                    await _affirmLogRepository.InsertAsync(new AffirmLog()
                    {
                        CreatedOnUtc = DateTime.UtcNow,
                        CustomerId = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0,
                        FullMessage = $"Affirm Order Not Confirmed {responseModel?.meta?.tempOrderId ?? string.Empty} status {responseModel?.checkout_status ?? string.Empty}",
                        IpAddress = _webHelper.GetCurrentIpAddress(),
                        LogLevel = LogLevel.Error,
                        ReferrerUrl = _webHelper.GetUrlReferrer(),
                        PageUrl = _webHelper.GetRawUrl(_httpContextAccessor.HttpContext.Request),
                        ShortMessage = $"Affirm Order, Not Confirmed {responseModel?.meta?.tempOrderId ?? string.Empty}"

                    });
                    TempData["Affirm.Error"] = await _localizationService.GetResourceAsync("Affirm.Error.orderNotConfimed");
                    return RedirectToRoute("CheckoutOnePage", new { AffirmMessage = "Affirm.Error.orderNotConfimed" });
                }

                await _affirmLogRepository.InsertAsync(new AffirmLog()
                {
                    CreatedOnUtc = DateTime.UtcNow,
                    CustomerId = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0,
                    FullMessage = $"Affirm Order Status {responseModel?.meta?.tempOrderId ?? string.Empty}  {responseModel?.checkout_status ?? string.Empty}, Started Validation",
                    IpAddress = _webHelper.GetCurrentIpAddress(),
                    LogLevel = LogLevel.Information,
                    ReferrerUrl = _webHelper.GetUrlReferrer(),
                    PageUrl = _webHelper.GetRawUrl(_httpContextAccessor.HttpContext.Request),
                    ShortMessage = $"Affirm Order Status {responseModel?.meta?.tempOrderId ?? string.Empty}"

                });

                int.TryParse(responseModel.meta?.customerid ?? string.Empty, out int customerId);
                if (customerId != (await _workContext.GetCurrentCustomerAsync()).Id)
                {
                    await _affirmLogRepository.InsertAsync(new AffirmLog()
                    {
                        CreatedOnUtc = DateTime.UtcNow,
                        CustomerId = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0,
                        FullMessage = $"Affirm Order, Affirm Checkout Details Customer Id {customerId} Current Customer ID {(await _workContext.GetCurrentCustomerAsync()).Id} -- Temp Order Id {responseModel?.meta?.tempOrderId ?? string.Empty} ",
                        IpAddress = _webHelper.GetCurrentIpAddress(),
                        LogLevel = LogLevel.Error,
                        ReferrerUrl = _webHelper.GetUrlReferrer(),
                        PageUrl = _webHelper.GetRawUrl(_httpContextAccessor.HttpContext.Request),
                        ShortMessage = $"Affirm Order, Failed to validate Customer"

                    });
                    TempData["Affirm.Error"] = await _localizationService.GetResourceAsync("Affirm.Error.Invalid");
                    return RedirectToRoute("CheckoutOnePage", new { AffirmMessage = "Affirm.Error.Invalid" });
                }
                int.TryParse(responseModel?.meta?.tempOrderId ?? string.Empty, out int tempOrderId);
                if (tempOrderId == 0)
                {
                    await _affirmLogRepository.InsertAsync(new AffirmLog()
                    {
                        CreatedOnUtc = DateTime.UtcNow,
                        CustomerId = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0,
                        FullMessage = $"Affirm Order Failed to read Temp Order Id",
                        IpAddress = _webHelper.GetCurrentIpAddress(),
                        LogLevel = LogLevel.Error,
                        ReferrerUrl = _webHelper.GetUrlReferrer(),
                        PageUrl = _webHelper.GetRawUrl(_httpContextAccessor.HttpContext.Request),
                        ShortMessage = $"Affirm Order, Failed to read Temp Order Id"

                    });
                    TempData["Affirm.Error"] = await _localizationService.GetResourceAsync("Affirm.Error.Invalid");
                    return RedirectToRoute("CheckoutOnePage", new { AffirmMessage = "Affirm.Error.Invalid" });
                }

               var tempOrder = await _affirmSessionRepository.GetByIdAsync(tempOrderId);

                ShippingOption shippingOption = await _genericAttributeService.GetAttributeAsync<ShippingOption>(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.SelectedShippingOptionAttribute, (await _storeContext.GetCurrentStoreAsync()).Id);

                if (tempOrder == null || tempOrder.Id == 0 || tempOrder.CustomerId != customerId)
                {
                    await _affirmLogRepository.InsertAsync(new AffirmLog()
                    {
                        CreatedOnUtc = DateTime.UtcNow,
                        CustomerId = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0,
                        FullMessage = $"Affirm Order Failed ,Temp Order Id invalid {tempOrder?.Id ?? 0} CustomerId {tempOrder?.CustomerId ?? 0} ",
                        IpAddress = _webHelper.GetCurrentIpAddress(),
                        LogLevel = LogLevel.Error,
                        ReferrerUrl = _webHelper.GetUrlReferrer(),
                        PageUrl = _webHelper.GetRawUrl(_httpContextAccessor.HttpContext.Request),
                        ShortMessage = $"Affirm Order Failed ,Temp Order invalid"

                    });
                    if (tempOrder != null)
                    {
                        tempOrder.Status = RequestStatus.Error;
                        tempOrder.Message = $"Affirm Order Failed ,Temp Order Id invalid {tempOrder?.Id ?? 0} CustomerId {tempOrder?.CustomerId ?? 0} ";
                        tempOrder.UpdatedOnUtc = DateTime.UtcNow;
                        await _affirmSessionRepository.UpdateAsync(tempOrder);
                    }
                    TempData["Affirm.Error"] = await _localizationService.GetResourceAsync("Affirm.Error.Invalid");
                    return RedirectToRoute("CheckoutOnePage", new { AffirmMessage = "Affirm.Error.Invalid" });
                }

                if (shippingOption == null)
                {
                    await _genericAttributeService.SaveAttributeAsync<ShippingOption>(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.SelectedShippingOptionAttribute, JsonConvert.DeserializeObject<ShippingOption>(tempOrder.ShippingMethod), (await _storeContext.GetCurrentStoreAsync()).Id);
                }
                tempOrder.Checkout_Token = token;
                tempOrder.UpdatedOnUtc = DateTime.UtcNow;
                await _affirmSessionRepository.UpdateAsync(tempOrder);

                var shoppingCartItems = JsonConvert.DeserializeObject<List<ShoppingCartItem>>(tempOrder.CartItems);


                #region Validate Order Total
                var shoppingCart = (await _shoppingCartService
            .GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, _storeContext.GetCurrentStore()?.Id ?? 0))
              .ToList();


                (List<AffirmItem> items, decimal? shoppingCartTotal, decimal discountTotal, decimal customDuty, decimal shippingTotal, decimal taxTotal, decimal orderTotal) = await _serviceManager.GetCartSummarry(shoppingCart, await _workContext.GetCurrentCustomerAsync());

                if (responseModel.total != AffirmHelper.ConvertDecimalToCents(orderTotal))
                {
                    await _affirmLogRepository.InsertAsync(new AffirmLog()
                    {
                        CreatedOnUtc = DateTime.UtcNow,
                        CustomerId = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0,
                        FullMessage = $"Affirm Order Failed Order Total not Matched {responseModel.total} -- Current Order Total {AffirmHelper.ConvertDecimalToCents(orderTotal)}",
                        IpAddress = _webHelper.GetCurrentIpAddress(),
                        LogLevel = LogLevel.Error,
                        ReferrerUrl = _webHelper.GetUrlReferrer(),
                        PageUrl = _webHelper.GetRawUrl(_httpContextAccessor.HttpContext.Request),
                        ShortMessage = $"Affirm Order Failed ,Order Total not Matched"

                    });
                    tempOrder.Status = RequestStatus.Error;
                    tempOrder.Message = $"Affirm Order Failed Order Total not Matched {responseModel.total} -- Current Order Total {AffirmHelper.ConvertDecimalToCents(orderTotal)}";
                    tempOrder.UpdatedOnUtc = DateTime.UtcNow;
                    await _affirmSessionRepository.UpdateAsync(tempOrder);
                    TempData["Affirm.Error"] = await _localizationService.GetResourceAsync("Affirm.Error.Invalid.Cart");
                    return RedirectToRoute("CheckoutOnePage", new { AffirmMessage = "Affirm.Error.Invalid.Cart" });
                }


                #endregion


                #region Validate Shopping Cart



                foreach (var item in shoppingCart)
                {
                    if (!shoppingCartItems.Where(i => i.Id == item.Id).Any())
                    {
                        await _shoppingCartService.DeleteShoppingCartItemAsync(item);
                        await _affirmLogRepository.InsertAsync(new AffirmLog()
                        {
                            CreatedOnUtc = DateTime.UtcNow,
                            CustomerId = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0,
                            FullMessage = $"Invalid Item {item.ProductId}",
                            IpAddress = _webHelper.GetCurrentIpAddress(),
                            LogLevel = LogLevel.Information,
                            ReferrerUrl = _webHelper.GetUrlReferrer(),
                            PageUrl = _webHelper.GetRawUrl(_httpContextAccessor.HttpContext.Request),
                            ShortMessage = $"Affirm Order, removed Invalid item from cart that's not with Affirm Session"

                        });
                    }
                }

                foreach (var item in shoppingCartItems)
                {

                    if (!shoppingCart.Where(i => i.Id == item.Id).Any())
                    {
                        var product = await _productService.GetProductByIdAsync(item.Id);
                        await _shoppingCartService.AddToCartAsync(await _workContext.GetCurrentCustomerAsync(), product, ShoppingCartType.ShoppingCart,
                            _storeContext.GetCurrentStoreAsync().Id, item.AttributesXml);


                        await _affirmLogRepository.InsertAsync(new AffirmLog()
                        {
                            CreatedOnUtc = DateTime.UtcNow,
                            CustomerId = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0,
                            FullMessage = $"Valid Item {item.ProductId}",
                            IpAddress = _webHelper.GetCurrentIpAddress(),
                            LogLevel = LogLevel.Information,
                            ReferrerUrl = _webHelper.GetUrlReferrer(),
                            PageUrl = _webHelper.GetRawUrl(_httpContextAccessor.HttpContext.Request),
                            ShortMessage = $"Affirm Order, Added valid item  that's  with Affirm Session"

                        });
                    }
                }

                await _affirmLogRepository.InsertAsync(new AffirmLog()
                {
                    CreatedOnUtc = DateTime.UtcNow,
                    CustomerId = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0,
                    FullMessage = $"Validation Completed",
                    IpAddress = _webHelper.GetCurrentIpAddress(),
                    LogLevel = LogLevel.Information,
                    ReferrerUrl = _webHelper.GetUrlReferrer(),
                    PageUrl = _webHelper.GetRawUrl(_httpContextAccessor.HttpContext.Request),
                    ShortMessage = $"Affirm Order Validation Completed"

                });

                #endregion

                if (!Guid.TryParse(responseModel?.meta?.orderGuid, out Guid orderGuid))
                {
                    await _affirmLogRepository.InsertAsync(new AffirmLog()
                    {
                        CreatedOnUtc = DateTime.UtcNow,
                        CustomerId = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0,
                        FullMessage = $"Invalid  OrderId",
                        IpAddress = _webHelper.GetCurrentIpAddress(),
                        LogLevel = LogLevel.Error,
                        ReferrerUrl = _webHelper.GetUrlReferrer(),
                        PageUrl = _webHelper.GetRawUrl(_httpContextAccessor.HttpContext.Request),
                        //ShortMessage = $"Affirm Order Failed,Invalid  OrderId {tempOrder.Id}"

                    });
               //     tempOrder.Status = RequestStatus.Error;
               //     tempOrder.Message = $"Invalid  OrderId";
               //     tempOrder.UpdatedOnUtc = DateTime.UtcNow;
               //     await _affirmSessionRepository.UpdateAsync(tempOrder);
                    TempData["Affirm.Error"] = await _localizationService.GetResourceAsync("Affirm.Error.Invalid.Session");
                    return RedirectToRoute("CheckoutOnePage", new { AffirmMessage = "Affirm.Error.Invalid.Session" });
                }

                #region Captue Transaction

                (string authorizationTransactionId, string captureTransactionId, string authResponse, string captureResponse, HttpStatusCode statusCode) = await _affirmService.CaptureTransaction(token, orderGuid, responseModel.total);
                string response = string.IsNullOrEmpty(captureResponse) ? authResponse : captureResponse;
                if (statusCode != HttpStatusCode.OK)
                {

                    await _affirmLogRepository.InsertAsync(new AffirmLog()
                    {
                        CreatedOnUtc = DateTime.UtcNow,
                        CustomerId = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0,
                        FullMessage = $"{response}",
                        IpAddress = _webHelper.GetCurrentIpAddress(),
                        LogLevel = LogLevel.Error,
                        ReferrerUrl = _webHelper.GetUrlReferrer(),
                        PageUrl = _webHelper.GetRawUrl(_httpContextAccessor.HttpContext.Request),
                        ShortMessage = _affirmSettings.TransactMode == TransactMode.AuthorizeAndCapture ? $"Affirm Order Failed,Failed to Capture" : $"Affirm Order Failed,Failed to Authorize"

                    });
                   // tempOrder.Status = RequestStatus.Error;
                  //  tempOrder.Message = _affirmSettings.TransactMode == TransactMode.AuthorizeAndCapture ? $"Failed to Capture Transaction {response}" : $"Failed to Authorize Transaction {response}";
                  //  tempOrder.UpdatedOnUtc = DateTime.UtcNow;
                  //  await _affirmSessionRepository.UpdateAsync(tempOrder);
                    TempData["Affirm.Error"] = await _localizationService.GetResourceAsync("Affirm.Error.Failed.Capture");
                    return RedirectToRoute("CheckoutOnePage", new { AffirmMessage = "Affirm.Error.Failed.Capture" });
                }

                #endregion


                var processPaymentRequest = new ProcessPaymentRequest();

                processPaymentRequest.OrderGuid = orderGuid;
                processPaymentRequest.StoreId = (await _storeContext.GetCurrentStoreAsync()).Id;
                processPaymentRequest.CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id;
                processPaymentRequest.PaymentMethodSystemName = "Payments.Affirm";
                await HttpContext.Session.SetAsync<ProcessPaymentRequest>("OrderPaymentInfo", processPaymentRequest);
                //processPaymentRequest.CustomValues.Add("TransactionId", _affirmSettings.TransactMode == TransactMode.AuthorizeAndCapture ? captureTransactionId : authorizationTransactionId);
                //processPaymentRequest.CustomValues.Add("AuthTransactionId", authorizationTransactionId);
                //processPaymentRequest.CustomValues.Add("AuthTransactionResult", authResponse);
                //processPaymentRequest.CustomValues.Add("CaptureTransactionId", captureTransactionId);
                //processPaymentRequest.CustomValues.Add("CaptureTransactionResult", captureResponse);
                processPaymentRequest.CustomValues["TransactionId"] = _affirmSettings.TransactMode == TransactMode.AuthorizeAndCapture ? captureTransactionId : authorizationTransactionId;
                processPaymentRequest.CustomValues["AuthTransactionId"] = authorizationTransactionId;
                processPaymentRequest.CustomValues["AuthTransactionResult"] = authResponse;
                processPaymentRequest.CustomValues["CaptureTransactionId"] = captureTransactionId;
                processPaymentRequest.CustomValues["CaptureTransactionResult"] = captureResponse;
                var placeOrderResult = await _orderProcessingService.PlaceOrderAsync(processPaymentRequest);
                if (!placeOrderResult.Errors.Any())
                {
                 //   tempOrder.LiveOrderNumber = placeOrderResult.PlacedOrder.Id;

               //     tempOrder.Status = RequestStatus.Completed;
               //     await _affirmSessionRepository.UpdateAsync(tempOrder);
                    return RedirectToRoute("CheckoutCompleted", new { orderId = placeOrderResult.PlacedOrder.Id });
                }
                else
                {
                    await _affirmLogRepository.InsertAsync(new AffirmLog()
                    {
                        CreatedOnUtc = DateTime.UtcNow,
                        CustomerId = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0,
                        FullMessage = string.Join(',', placeOrderResult.Errors.ToArray()),
                        IpAddress = _webHelper.GetCurrentIpAddress(),
                        LogLevel = LogLevel.Error,
                        ReferrerUrl = _webHelper.GetUrlReferrer(),
                        PageUrl = _webHelper.GetRawUrl(_httpContextAccessor.HttpContext.Request),
                        ShortMessage = $"Failed to place Order {tempOrder.Id}"

                    });
                    TempData["Affirm.Error"] = await _localizationService.GetResourceAsync("Affirm.Error.Failed.Place.Order");
                    return RedirectToRoute("CheckoutOnePage");
                }
            }
            catch (Exception ex)
            {
                await _affirmLogRepository.InsertAsync(new AffirmLog()
                {
                    CreatedOnUtc = DateTime.UtcNow,
                    CustomerId = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0,
                    FullMessage = ex.Message,
                    IpAddress = _webHelper.GetCurrentIpAddress(),
                    LogLevel = LogLevel.Error,
                    ReferrerUrl = _webHelper.GetUrlReferrer(),
                    PageUrl = _webHelper.GetRawUrl(_httpContextAccessor.HttpContext.Request),
                    ShortMessage = $"Failed to place Order"

                });
                return RedirectToRoute("CheckoutOnePage");
            }
        }
        public IActionResult CancelCallbackHandler()
        {
            return RedirectToRoute("CheckoutOnePage");
        }



        //public async Task<IActionResult> CustomOrderConfirmCallbackHandler(int orderId, int customerId)
        //{
        //    var customOrder = await _customOrderService.GetById(orderId);
        //    var customer = await _customerService.GetCustomerByIdAsync(customerId);
        //    if (customOrder == null || customer == null || customOrder.CustomerId != customer.Id || await _customOrderService.IsOrderPaid(customOrder))
        //    {
        //        return RedirectToRoute("Homepage");
        //    }

        //    string referencePage = $"/checkoutCustomOrder?orderid={orderId}&customerid={customOrder.CustomerId}";

        //    try
        //    {
        //        if (await _paymentPluginManager.LoadPluginBySystemNameAsync("Payments.Affirm") is not AffirmPaymentMethod processor || !_paymentPluginManager.IsPluginActive(processor))
        //            throw new NopException("Affirm module cannot be loaded");
        //        string token = _webHelper.QueryString<string>("checkout_token");
        //        if (string.IsNullOrEmpty(token))
        //        {
        //            await _affirmLogRepository.InsertAsync(new AffirmLog()
        //            {
        //                CreatedOnUtc = DateTime.UtcNow,
        //                CustomerId = customer.Id,
        //                FullMessage = "Failed to read Checkout Token",
        //                IpAddress = _webHelper.GetCurrentIpAddress(),
        //                LogLevel = LogLevel.Error,
        //                ReferrerUrl = _webHelper.GetUrlReferrer(),
        //                PageUrl = _webHelper.GetRawUrl(_httpContextAccessor.HttpContext.Request),
        //                ShortMessage = "Affirm Order, Token Missed"

        //            });
        //            return Redirect(referencePage);
        //        }
        //        await _affirmLogRepository.InsertAsync(new AffirmLog()
        //        {
        //            CreatedOnUtc = DateTime.UtcNow,
        //            CustomerId = customer.Id,
        //            FullMessage = $"Checkout Token {token}",
        //            IpAddress = _webHelper.GetCurrentIpAddress(),
        //            LogLevel = LogLevel.Information,
        //            ReferrerUrl = _webHelper.GetUrlReferrer(),
        //            PageUrl = _webHelper.GetRawUrl(_httpContextAccessor.HttpContext.Request),
        //            ShortMessage = "Affirm Confirmation Handler Intialized."

        //        });
        //        var responseModel = await _affirmService.CheckoutDetails(token);
        //        if ((responseModel?.checkout_status ?? string.Empty) != AffirmOrderStatus.confirmed.ToString())
        //        {
        //            await _affirmLogRepository.InsertAsync(new AffirmLog()
        //            {
        //                CreatedOnUtc = DateTime.UtcNow,
        //                CustomerId = customer.Id,
        //                FullMessage = $"Affirm Order Not Confirmed {responseModel?.meta?.tempOrderId ?? string.Empty} status {responseModel?.checkout_status ?? string.Empty}",
        //                IpAddress = _webHelper.GetCurrentIpAddress(),
        //                LogLevel = LogLevel.Error,
        //                ReferrerUrl = _webHelper.GetUrlReferrer(),
        //                PageUrl = _webHelper.GetRawUrl(_httpContextAccessor.HttpContext.Request),
        //                ShortMessage = $"Affirm Order, Not Confirmed {responseModel?.meta?.tempOrderId ?? string.Empty}"

        //            });
        //            TempData["Affirm.Error"] = await _localizationService.GetResourceAsync("Affirm.Error.orderNotConfimed");

        //            return Redirect(referencePage);
        //        }

        //        await _affirmLogRepository.InsertAsync(new AffirmLog()
        //        {
        //            CreatedOnUtc = DateTime.UtcNow,
        //            CustomerId = customer.Id,
        //            FullMessage = $"Affirm Order Status {responseModel?.meta?.tempOrderId ?? string.Empty}  {responseModel?.checkout_status ?? string.Empty}, Started Validation",
        //            IpAddress = _webHelper.GetCurrentIpAddress(),
        //            LogLevel = LogLevel.Information,
        //            ReferrerUrl = _webHelper.GetUrlReferrer(),
        //            PageUrl = _webHelper.GetRawUrl(_httpContextAccessor.HttpContext.Request),
        //            ShortMessage = $"Affirm Order Status {responseModel?.meta?.tempOrderId ?? string.Empty}"

        //        });


        //        int.TryParse(responseModel?.meta?.tempOrderId ?? string.Empty, out int tempOrderId);
        //        if (tempOrderId == 0)
        //        {
        //            await _affirmLogRepository.InsertAsync(new AffirmLog()
        //            {
        //                CreatedOnUtc = DateTime.UtcNow,
        //                CustomerId = customer.Id,
        //                FullMessage = $"Affirm Order Failed to read Temp Order Id",
        //                IpAddress = _webHelper.GetCurrentIpAddress(),
        //                LogLevel = LogLevel.Error,
        //                ReferrerUrl = _webHelper.GetUrlReferrer(),
        //                PageUrl = _webHelper.GetRawUrl(_httpContextAccessor.HttpContext.Request),
        //                ShortMessage = $"Affirm Order, Failed to read Temp Order Id"

        //            });
        //            TempData["Affirm.Error"] = await _localizationService.GetResourceAsync("Affirm.Error.Invalid");
        //            return Redirect(referencePage);
        //        }

        //        var tempOrder = await _affirmSessionRepository.GetByIdAsync(tempOrderId);



        //        if (tempOrder == null || tempOrder.Id == 0 || tempOrder.CustomerId != customerId)
        //        {
        //            await _affirmLogRepository.InsertAsync(new AffirmLog()
        //            {
        //                CreatedOnUtc = DateTime.UtcNow,
        //                CustomerId = customer.Id,
        //                FullMessage = $"Affirm Order Failed ,Temp Order Id invalid {tempOrder?.Id ?? 0} CustomerId {tempOrder?.CustomerId ?? 0} ",
        //                IpAddress = _webHelper.GetCurrentIpAddress(),
        //                LogLevel = LogLevel.Error,
        //                ReferrerUrl = _webHelper.GetUrlReferrer(),
        //                PageUrl = _webHelper.GetRawUrl(_httpContextAccessor.HttpContext.Request),
        //                ShortMessage = $"Affirm Order Failed ,Temp Order invalid"

        //            });
        //            if (tempOrder != null)
        //            {
        //                tempOrder.Status = RequestStatus.Error;
        //                tempOrder.Message = $"Affirm Order Failed ,Temp Order Id invalid {tempOrder?.Id ?? 0} CustomerId {tempOrder?.CustomerId ?? 0} ";
        //                tempOrder.UpdatedOnUtc = DateTime.UtcNow;
        //                await _affirmSessionRepository.UpdateAsync(tempOrder);
        //            }
        //            TempData["Affirm.Error"] = await _localizationService.GetResourceAsync("Affirm.Error.Invalid");
        //            return Redirect(referencePage);
        //        }


        //        tempOrder.Checkout_Token = token;
        //        tempOrder.UpdatedOnUtc = DateTime.UtcNow;
        //        await _affirmSessionRepository.UpdateAsync(tempOrder);




        //        #region Validate Order Total


        //        var payableAmount = await this._customOrderService.GetPayableAmount(customOrder);
        //        decimal.TryParse(Convert.ToString(payableAmount), NumberStyles.Currency,
        //          CultureInfo.CurrentCulture.NumberFormat, out decimal orderTotal);

        //        if (responseModel.total != AffirmHelper.ConvertDecimalToCents(orderTotal))
        //        {
        //            await _affirmLogRepository.InsertAsync(new AffirmLog()
        //            {
        //                CreatedOnUtc = DateTime.UtcNow,
        //                CustomerId = customer.Id,
        //                FullMessage = $"Affirm Order Failed Order Total not Matched {responseModel.total} -- Current Order Total {AffirmHelper.ConvertDecimalToCents(orderTotal)}",
        //                IpAddress = _webHelper.GetCurrentIpAddress(),
        //                LogLevel = LogLevel.Error,
        //                ReferrerUrl = _webHelper.GetUrlReferrer(),
        //                PageUrl = _webHelper.GetRawUrl(_httpContextAccessor.HttpContext.Request),
        //                ShortMessage = $"Affirm Order Failed ,Order Total not Matched"

        //            });
        //            tempOrder.Status = RequestStatus.Error;
        //            tempOrder.Message = $"Affirm Order Failed Order Total not Matched {responseModel.total} -- Current Order Total {AffirmHelper.ConvertDecimalToCents(orderTotal)}";
        //            tempOrder.UpdatedOnUtc = DateTime.UtcNow;
        //            await _affirmSessionRepository.UpdateAsync(tempOrder);
        //            TempData["Affirm.Error"] = await _localizationService.GetResourceAsync("Affirm.Error.Invalid.Cart");
        //            return Redirect(referencePage);
        //        }


        //        #endregion



        //        if (!Guid.TryParse(responseModel?.meta?.orderGuid, out Guid orderGuid))
        //        {
        //            await _affirmLogRepository.InsertAsync(new AffirmLog()
        //            {
        //                CreatedOnUtc = DateTime.UtcNow,
        //                CustomerId = customer.Id,
        //                FullMessage = $"Invalid  OrderId",
        //                IpAddress = _webHelper.GetCurrentIpAddress(),
        //                LogLevel = LogLevel.Error,
        //                ReferrerUrl = _webHelper.GetUrlReferrer(),
        //                PageUrl = _webHelper.GetRawUrl(_httpContextAccessor.HttpContext.Request),
        //                ShortMessage = $"Affirm Order Failed,Invalid  OrderId {tempOrder.Id}"

        //            });
        //            tempOrder.Status = RequestStatus.Error;
        //            tempOrder.Message = $"Invalid  OrderId";
        //            tempOrder.UpdatedOnUtc = DateTime.UtcNow;
        //            await _affirmSessionRepository.UpdateAsync(tempOrder);
        //            TempData["Affirm.Error"] = await _localizationService.GetResourceAsync("Affirm.Error.Invalid.Session");
        //            return Redirect(referencePage);
        //        }




        //        #region Captue Transaction

        //        (string authorizationTransactionId, string captureTransactionId, string authResponse, string captureResponse, HttpStatusCode statusCode) = await _affirmService.CaptureTransaction(token, orderGuid, responseModel.total);

        //        string response = string.IsNullOrEmpty(captureResponse) ? authResponse : captureResponse;
        //        if (statusCode != HttpStatusCode.OK)
        //        {

        //            await _affirmLogRepository.InsertAsync(new AffirmLog()
        //            {
        //                CreatedOnUtc = DateTime.UtcNow,
        //                CustomerId = customer.Id,
        //                FullMessage = $"{response}",
        //                IpAddress = _webHelper.GetCurrentIpAddress(),
        //                LogLevel = LogLevel.Error,
        //                ReferrerUrl = _webHelper.GetUrlReferrer(),
        //                PageUrl = _webHelper.GetRawUrl(_httpContextAccessor.HttpContext.Request),
        //                ShortMessage = _affirmSettings.TransactMode == TransactMode.AuthorizeAndCapture ? $"Affirm Order Failed,Failed to Capture" : $"Affirm Order Failed,Failed to Authorize"

        //            });
        //            tempOrder.Status = RequestStatus.Error;
        //            tempOrder.Message = _affirmSettings.TransactMode == TransactMode.AuthorizeAndCapture ? $"Failed to Capture Transaction {response}" : $"Failed to Authorize Transaction {response}";
        //            tempOrder.UpdatedOnUtc = DateTime.UtcNow;
        //            await _affirmSessionRepository.UpdateAsync(tempOrder);
        //            TempData["Affirm.Error"] = await _localizationService.GetResourceAsync("Affirm.Error.Failed.Capture");
        //            return Redirect(referencePage);
        //        }

        //        #endregion




        //        var processPaymentRequest = new ProcessPaymentRequest();

        //        processPaymentRequest.OrderGuid = orderGuid;
        //        processPaymentRequest.StoreId = (await _storeContext.GetCurrentStoreAsync()).Id;
        //        processPaymentRequest.CustomerId = (int)customOrder.CustomerId;
        //        processPaymentRequest.PaymentMethodSystemName = "Payments.Affirm";
        //       await HttpContext.Session.SetAsync<ProcessPaymentRequest>("OrderPaymentInfo", processPaymentRequest);
        //        processPaymentRequest.CustomValues.Add("TransactionId", _affirmSettings.TransactMode == TransactMode.AuthorizeAndCapture ? captureTransactionId : authorizationTransactionId);
        //        processPaymentRequest.CustomValues.Add("AuthTransactionId", authorizationTransactionId);
        //        processPaymentRequest.CustomValues.Add("AuthTransactionResult", authResponse);
        //        processPaymentRequest.CustomValues.Add("CaptureTransactionId", captureTransactionId);
        //        processPaymentRequest.CustomValues.Add("CaptureTransactionResult", captureResponse);
        //        int refOrderno = 0;
        //        bool saveOrderDetails = true;
        //        if (customOrder.LiveOrderNumber != null && customOrder.LiveOrderNumber != 0 && customOrder.AlreadyFee != null && customOrder.AlreadyFee > 0)
        //        {
        //            saveOrderDetails = false;
        //            refOrderno = Convert.ToInt32(customOrder.LiveOrderNumber);
        //        }

        //        (var placeOrderResult, var paymentResponse) = await _orderProcessingService.CustomPlaceOrderAsync(processPaymentRequest, customOrder, await this._customOrderModelFactory.PrepareOderSummaryModel(customOrder.Id), saveOrderDetails, refOrderno, false);
        //        if (!placeOrderResult.Errors.Any())
        //        {
        //            tempOrder.LiveOrderNumber = placeOrderResult.PlacedOrder.Id;

        //            tempOrder.Status = RequestStatus.Completed;
        //            await _affirmSessionRepository.UpdateAsync(tempOrder);
        //            return Redirect(

        //                _affirmSettings.TransactMode == TransactMode.AuthorizeAndCapture ?
        //                $"/checkoutCustomOrder/ProcessAffirmOrder?orderid={orderId}&liveOrderNumber={placeOrderResult.PlacedOrder.Id}&transactionId={captureTransactionId}" :
        //                $"/checkoutCustomOrder/ProcessAffirmOrder?orderid={orderId}&liveOrderNumber={placeOrderResult.PlacedOrder.Id}&transactionId={authorizationTransactionId}"
        //                );
        //        }
        //        else
        //        {
        //            await _affirmLogRepository.InsertAsync(new AffirmLog()
        //            {
        //                CreatedOnUtc = DateTime.UtcNow,
        //                CustomerId = customer.Id,
        //                FullMessage = string.Join(',', placeOrderResult.Errors.ToArray()),
        //                IpAddress = _webHelper.GetCurrentIpAddress(),
        //                LogLevel = LogLevel.Error,
        //                ReferrerUrl = _webHelper.GetUrlReferrer(),
        //                PageUrl = _webHelper.GetRawUrl(_httpContextAccessor.HttpContext.Request),
        //                ShortMessage = $"Failed to place Order {tempOrder.Id}"

        //            });
        //            TempData["Affirm.Error"] = await _localizationService.GetResourceAsync("Affirm.Error.Failed.Place.Order");
        //            return Redirect(referencePage);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        await _affirmLogRepository.InsertAsync(new AffirmLog()
        //        {
        //            CreatedOnUtc = DateTime.UtcNow,
        //            CustomerId = customer.Id,
        //            FullMessage = ex.Message,
        //            IpAddress = _webHelper.GetCurrentIpAddress(),
        //            LogLevel = LogLevel.Error,
        //            ReferrerUrl = _webHelper.GetUrlReferrer(),
        //            PageUrl = _webHelper.GetRawUrl(_httpContextAccessor.HttpContext.Request),
        //            ShortMessage = $"Failed to place Order"

        //        });
        //        return Redirect(referencePage);
        //    }
        //}
        //public async Task<IActionResult> CustomOrderCancelCallbackHandler(int orderId)
        //{
        //    var customOrder = await _customOrderService.GetById(orderId);
        //    if (customOrder == null)
        //    {
        //        return RedirectToRoute("Homepage");
        //    }
        //    return Redirect($"/checkoutCustomOrder?orderid={orderId}&customerid={customOrder.CustomerId}");
        //}

        #endregion
    }

}
