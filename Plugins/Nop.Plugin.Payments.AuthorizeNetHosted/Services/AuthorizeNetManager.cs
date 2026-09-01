using AuthorizeNet.Api.Contracts.V1;
using AuthorizeNet.Api.Controllers;
using AuthorizeNet.Api.Controllers.Bases;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using MWT.Nop.Core.Services.Customers;
using MWT.Nop.Core.Services.Customizations.CustomOrders;
using MWT.Nop.Core.Services.Orders;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.AuthorizeNetHosted.Helpers;
using Nop.Plugin.Payments.AuthorizeNetHosted.Logging;
using Nop.Plugin.Payments.AuthorizeNetHosted.Models;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Tax;
using System.Globalization;
using System.Text;
using Customer = Nop.Core.Domain.Customers.Customer;
using Order = Nop.Core.Domain.Orders.Order;

namespace Nop.Plugin.Payments.AuthorizeNetHosted.Services
{
    public class AuthorizeNetManager : IAuthorizeNetManager
    {
        private readonly IOrderExtendedService _orderService;
        private readonly IOrderProcessingExtendedService _orderProcessingService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly IAddressService _addressService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ICountryService _countryService;
        private readonly IProductService _productService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ITaxService _taxService;
        private readonly ICustomerExtendedService _customerService;
        private readonly IWebHelper _webHelper;
        private readonly ILogger _logger;
        private readonly AuthorizeNetHostedPaymentSettings _authorizeNetHostedPaymentSettings;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly ICustomOrderService _customOrderService;
        private readonly PaymentLogger _paymentLogger;
        private readonly HttpClient _httpClient;
        private readonly IGenericAttributeService _genericAttributeService;
        public AuthorizeNetManager(
            IOrderExtendedService orderService,
            IOrderProcessingExtendedService orderProcessingService,
            IOrderTotalCalculationService orderTotalCalculationService,
            ICurrencyService currencyService,
            CurrencySettings currencySettings,
            IAddressService addressService,
            IStateProvinceService stateProvinceService,
            ICountryService countryService,
            IProductService productService,
            IPriceCalculationService priceCalculationService,
            ITaxService taxService,
            ICustomerExtendedService customerService,
            IWebHelper webHelper,
            ILogger logger,
            AuthorizeNetHostedPaymentSettings authorizeNetHostedPaymentSettings,
            IWorkContext workContext,
             IStoreContext storeContext,
             IShoppingCartService shoppingCartService,
             IUrlHelperFactory urlHelperFactory,
             IActionContextAccessor actionContextAccessor,
             ICustomOrderService customOrderService,
             PaymentLogger paymentLogger,
             HttpClient httpClient,
             IGenericAttributeService genericAttributeService)
        {
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _currencyService = currencyService;
            _currencySettings = currencySettings;
            _addressService = addressService;
            _stateProvinceService = stateProvinceService;
            _countryService = countryService;
            _productService = productService;
            _priceCalculationService = priceCalculationService;
            _taxService = taxService;
            _customerService = customerService;
            _webHelper = webHelper;
            _logger = logger;
            _authorizeNetHostedPaymentSettings = authorizeNetHostedPaymentSettings;
            _workContext = workContext;
            _storeContext = storeContext;
            _shoppingCartService = shoppingCartService;
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
            _customOrderService = customOrderService;
            _paymentLogger = paymentLogger;
            _httpClient = httpClient;
            _genericAttributeService = genericAttributeService;
        }

        // RVA 0x0000C3DF confirmed
        public string GetLibraryUrl(bool useSandbox) =>
            useSandbox
                ? "https://jstest.authorize.net/v3/AcceptUI.js"
                : "https://js.authorize.net/v3/AcceptUI.js";

        // RVA 0x0000C4F8 — calls SANDBOX, PRODUCTION, set_RunEnvironment, name, ItemElementName, Item
        public merchantAuthenticationType PrepareAuthorizeNet()
        {
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment =
                _authorizeNetHostedPaymentSettings.UseSandbox
                    ? AuthorizeNet.Environment.SANDBOX
                    : AuthorizeNet.Environment.PRODUCTION;

            return new merchantAuthenticationType
            {
                name = _authorizeNetHostedPaymentSettings.LoginId,
                ItemElementName = ItemChoiceType.transactionKey,
                Item = _authorizeNetHostedPaymentSettings.TransactionKey
            };
        }

        // RVA 0x0000C4A4 — calls Execute, GetApiResponse, GetErrorResponse, GetResults
        public async Task<createTransactionResponse> GetApiResponseAsync(
            createTransactionController controller, IList<string> errors)
        {
            controller.Execute();
            var response = controller.GetApiResponse();
            if (response != null)
            {
                if (response.transactionResponse?.errors != null)
                {
                    foreach (var transactionResponseError in response.transactionResponse.errors)
                        errors.Add($"Error #{transactionResponseError.errorCode}: {transactionResponseError.errorText}");

                    return null;
                }

                if (response.transactionResponse != null/* && response.messages.resultCode == messageTypeEnum.Ok*/)
                    switch (response.transactionResponse.responseCode)
                    {
                        case "1":
                            return response;

                        case "2":
                            var description = response.transactionResponse.messages.Any()
                                ? response.transactionResponse.messages.First().description
                                : string.Empty;
                            errors.Add($"Declined ({response.transactionResponse.responseCode}: {description})".TrimEnd(':', ' '));
                            return null;
                    }
                else if (response.transactionResponse != null && response.messages.resultCode == messageTypeEnum.Error)
                    if (response.messages?.message != null && response.messages.message.Any())
                    {
                        var message = response.messages.message.First();

                        errors.Add($"Error #{message.code}: {message.text}");
                        return null;
                    }
            }
            else
            {
                var error = controller.GetErrorResponse();
                if (error?.messages?.message != null && error.messages.message.Any())
                {
                    var message = error.messages.message.First();

                    errors.Add($"Error #{message.code}: {message.text}");
                    return null;
                }
            }

            var controllerResult = controller.GetResults().FirstOrDefault();

            if (controllerResult?.StartsWith("I00001", StringComparison.InvariantCultureIgnoreCase) ?? false)
                return null;

            const string unknownError = "Authorize.NET unknown error";
            errors.Add(string.IsNullOrEmpty(controllerResult) ? unknownError : $"{unknownError} ({controllerResult})");

            return null;
        }

        // RVA 0x0000C450 — calls GetCurrencyByIdAsync, ConvertFromPrimaryStoreCurrencyAsync, Round
        public async Task<(decimal orderTotal, string currencyCode)> GetOrderTotalAsync(int currencyId, decimal orderTotal)
        {
            var currency = await _currencyService.GetCurrencyByIdAsync(currencyId);
            var converted = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(orderTotal, currency);
            return (Math.Round(converted, 2), currency?.CurrencyCode ?? "USD");
        }


        public async Task WebhookPaymentEventAsync(paymentEvent paymentEvent)
        {
            var transactionIdStr = paymentEvent?.payload?.id;
            if (string.IsNullOrEmpty(transactionIdStr))
            {
                await LogMessageAsync("WebhookPaymentEvent: merchantReferenceId empty.");
                return;
            }



            // Small delay for race-condition guard (Delay confirmed in MemberRef)
            await Task.Delay(500);

            var order = await _orderService.GetOrderByTransactionId(transactionIdStr, string.Empty);
            if (order == null)
            {
                await LogMessageAsync($"WebhookPaymentEvent: order not found for TransactionId {transactionIdStr}");
                return;
            }


            var responseCode = paymentEvent.payload?.responseCode;
            var authCode = paymentEvent.payload?.authCode;
            var message = $"Authorize.NET Webhook | EventType={paymentEvent.eventType} | TransID={transactionIdStr} | AuthCode={authCode} | ResponseCode={responseCode}";

            await SaveOrderNoteAsync(order, message);


            // Webhook eventType comes from the payload root: payload.eventType
            switch (paymentEvent.eventType)
            {
                case "net.authorize.payment.authcapture.created":
                case "net.authorize.payment.capture.created":
                    {
                        if (responseCode == "1") // Approved
                        {
                            order.AuthorizationTransactionId = transactionIdStr;
                            order.CaptureTransactionId = transactionIdStr;
                            await _orderService.UpdateOrderAsync(order);

                            if (_orderProcessingService.CanMarkOrderAsAuthorized(order))
                                await _orderProcessingService.MarkAsAuthorizedAsync(order);

                            if (_orderProcessingService.CanMarkOrderAsPaid(order))
                                await _orderProcessingService.MarkOrderAsPaidAsync(order);
                        }
                        else
                        {
                            await LogMessageAsync($"WebhookPaymentEvent: capture not approved. Code={responseCode}, TransId={transactionIdStr}");
                        }
                        break;
                    }

                case "net.authorize.payment.refund.created":
                    {
                        // Refund webhook fires when a settled transaction is refunded.
                        // The transId in the webhook is the NEW refund transaction; the original
                        // transaction ID is in payload.refTransId (you'll need to fetch the full
                        // transaction details via Authorize.Net API if you only get the webhook payload).

                        if (order.PaymentStatus == PaymentStatus.Paid || order.PaymentStatus == PaymentStatus.PartiallyRefunded)
                        {
                            // Decide: full refund vs partial. The webhook payload has authAmount.
                            var refundAmount = paymentEvent.payload.authAmount; // decimal from payload

                            if (refundAmount >= order.OrderTotal && _orderProcessingService.CanRefundOffline(order))
                            {
                                await _orderProcessingService.RefundOfflineAsync(order);
                            }
                            else if (_orderProcessingService.CanPartiallyRefundOffline(order, refundAmount))
                            {
                                await _orderProcessingService.PartiallyRefundOfflineAsync(order, refundAmount);
                            }
                            else
                            {
                                await LogMessageAsync($"WebhookPaymentEvent: refund cannot be applied. OrderId={order.Id}, Amount={refundAmount}, Status={order.PaymentStatus}");
                            }
                        }
                        else
                        {
                            await LogMessageAsync($"WebhookPaymentEvent: refund received but order not in refundable state. OrderId={order.Id}, Status={order.PaymentStatus}");
                        }
                        break;
                    }

                case "net.authorize.payment.void.created":
                    {
                        // Void webhook fires when an unsettled authorization is voided.
                        // Only valid on orders in Authorized (not yet captured/settled) state.

                        if (_orderProcessingService.CanVoidOffline(order))
                        {
                            await _orderProcessingService.VoidOfflineAsync(order);
                        }
                        else
                        {
                            await LogMessageAsync($"WebhookPaymentEvent: void cannot be applied. OrderId={order.Id}, Status={order.PaymentStatus}");
                        }
                        break;
                    }

                default:
                    await LogMessageAsync($"WebhookPaymentEvent: unhandled eventType={paymentEvent.eventType}");
                    break;
            }

        }

        // RVA 0x0000C548 — state-machine vars: sb, transactionDetails
        // Calls: getTransactionDetailsController, Execute, GetApiResponse,
        //        transId, transactionType, authCode, authAmount, responseCode,
        //        order.Split('-'), payment.Item (creditCardMaskedType cast), cardNumber
        public async Task<getTransactionDetailsResponse> GetTransactionDetailsAsync(
             string transactionId)
        {
            var sb = new StringBuilder();
            var result = new AuthorizeNetTransactionDetails();

            var merchantAuth = PrepareAuthorizeNet();
            var request = new getTransactionDetailsRequest
            {
                merchantAuthentication = merchantAuth,
                transId = transactionId
            };

            var controller = new getTransactionDetailsController(request);
            controller.Execute();
            var response = controller.GetApiResponse();
            return response;
        }
        public async Task<string> FindTransactionIdByInvoiceAsync(string expectedInvoiceNumber, int customerId)
        {
            string firstName = string.Empty;
            string lastName = string.Empty;

            var customer = await _customerService.GetCustomerByIdAsync(customerId);
            if (customer == null)
                return null;

            var address = await _addressService.GetAddressByIdAsync(
                customer.BillingAddressId ?? customer.ShippingAddressId ?? 0);
            if (address != null)
            {
                firstName = address.FirstName;
                lastName = address.LastName;
            }

            AuthorizeNet.Api.Controllers.Bases.ApiOperationBase<ANetApiRequest, ANetApiResponse>
                .RunEnvironment = _authorizeNetHostedPaymentSettings.UseSandbox
                    ? AuthorizeNet.Environment.SANDBOX
                    : AuthorizeNet.Environment.PRODUCTION;

            var merchantAuth = PrepareAuthorizeNet();

            const int maxAttempts = 20;
            const int delayMs = 200;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                if (attempt > 1)
                    await Task.Delay(delayMs);

                try
                {
                    await _paymentLogger.InformationAsync(
                        $"FindTransactionIdByInvoice: attempt {attempt}/{maxAttempts}, " +
                        $"InvoiceNumber={expectedInvoiceNumber}, CustomerId={customerId}");

                    // Query the unsettled list (fresh iframe payments sit here before nightly settlement)
                    var request = new getUnsettledTransactionListRequest
                    {
                        merchantAuthentication = merchantAuth,
                        sorting = new TransactionListSorting
                        {
                            orderBy = TransactionListOrderFieldEnum.submitTimeUTC,
                            orderDescending = true
                        },
                        paging = new Paging { limit = 10, offset = 1 }
                    };

                    var controller = new getUnsettledTransactionListController(request);
                    controller.Execute();
                    var response = controller.GetApiResponse();

                    if (response?.messages?.resultCode == messageTypeEnum.Ok && response.transactions != null)
                    {
                        // First pass: match by invoice number (strongest match)
                        foreach (var tx in response.transactions)
                        {
                            await _paymentLogger.InformationAsync(
                                $"ProcessPayment: Recent Transaction Id {tx.transId} " +
                                $"Invoice No {tx.invoiceNumber} {expectedInvoiceNumber}");

                            if (!string.IsNullOrEmpty(tx.invoiceNumber)
                                && expectedInvoiceNumber.StartsWith(tx.invoiceNumber))
                            {
                                var details = await this.GetTransactionDetailsAsync(tx.transId);

                                await _paymentLogger.InformationAsync(
                                    $"ProcessPayment: Transaction Info " +
                                    $"{details?.transaction?.order?.description} {expectedInvoiceNumber}");

                                if (details?.transaction?.order?.description != null
                                    && details.transaction.order.description.Contains(expectedInvoiceNumber))
                                {
                                    await _paymentLogger.InformationAsync(
                                        $"ProcessPayment: Transaction Found {tx.transId} " +
                                        $"on attempt {attempt}/{maxAttempts}");
                                    return tx.transId;
                                }
                            }
                        }

                        //// Second pass: fallback match by customer first/last name
                        //foreach (var tx in response.transactions)
                        //{
                        //    if (!string.IsNullOrEmpty(tx.firstName)
                        //        && !string.IsNullOrEmpty(tx.lastName)
                        //        && string.Equals(tx.firstName.Trim(), firstName, StringComparison.InvariantCultureIgnoreCase)
                        //        && string.Equals(tx.lastName.Trim(), lastName, StringComparison.InvariantCultureIgnoreCase))
                        //    {
                        //        await _paymentLogger.InformationAsync(
                        //            $"ProcessPayment: Transaction Found {tx.transId} " +
                        //            $"(name-based match) on attempt {attempt}/{maxAttempts}");
                        //        return tx.transId;
                        //    }
                        //}
                    }
                    else
                    {
                        await _paymentLogger.WarningAsync(
                            $"FindTransactionIdByInvoice: attempt {attempt} returned no usable response. " +
                            $"ResultCode={response?.messages?.resultCode}, " +
                            $"Message={response?.messages?.message?[0]?.text}");
                    }
                }
                catch (Exception ex)
                {
                    await _paymentLogger.WarningAsync(
                        $"FindTransactionIdByInvoice: attempt {attempt}/{maxAttempts} threw. " +
                        $"InvoiceNumber={expectedInvoiceNumber}, CustomerId={customerId}",
                        ex);
                    // Continue to next retry rather than failing outright.
                }
            }

            await _paymentLogger.ErrorAsync(
                $"ProcessPayment: Transaction not found for invoice no {expectedInvoiceNumber} " +
                $"after {maxAttempts} attempts. CustomerId={customerId}. " +
                $"Manual reconciliation may be required in Authorize.Net Merchant Interface.");

            return null;
        }

        public async Task<createTransactionResponse> CreateTransactionAsync(
            string dataValue, string dataDescriptor,
            ProcessPaymentRequest processPaymentRequest, IList<string> errors)
        {
            var merchantAuth = PrepareAuthorizeNet();

            var opaqueData = new opaqueDataType { dataDescriptor = dataDescriptor, dataValue = dataValue };
            var paymentType = new paymentType { Item = opaqueData };

            var transactionType = _authorizeNetHostedPaymentSettings.TransactMode == Domain.TransactMode.Authorize
                ? transactionTypeEnum.authOnlyTransaction
                : transactionTypeEnum.authCaptureTransaction;

            var (orderTotal, currencyCode) = await GetOrderTotalAsync(
                _currencySettings.PrimaryStoreCurrencyId,
                processPaymentRequest.OrderTotal);

            var customer = await _customerService.GetCustomerByIdAsync(processPaymentRequest.CustomerId);

            var billTo = await GetTransactionRequestAddressAsync((customer?.BillingAddressId ?? 0) != 0 ? (int)customer.BillingAddressId : customer.ShippingAddressId ?? 0,true,false, customer);

            var transactionRequest = new transactionRequestType
            {
                transactionType = transactionType.ToString(),
                amount = orderTotal,
                payment = paymentType,
                currencyCode = currencyCode,
                billTo = billTo,
                customerIP = _webHelper.GetCurrentIpAddress(),
                order = new orderType
                {
                    invoiceNumber = processPaymentRequest.OrderGuid.ToString().Substring(0, 19),
                    description = $"Full order #{processPaymentRequest.OrderGuid}",
                    //purchaseOrderDateUTC = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, DateTime.UtcNow.Second, DateTimeKind.Utc)
                },
                //solution = new solutionType
                //{
                //    id = AuthorizeNetHostedPaymentDefaults.SolutionID,
                //    name = AuthorizeNetHostedPaymentDefaults.SolutionName
                //},
                customer = new customerDataType
                {
                    id = customer.Id.ToString(),
                    email = await _customerService.GetCustomerEmailAsync(customer)
                }
            };
            if (customer?.ShippingAddressId != null)
            {
       

                var shipTo = await GetTransactionRequestAddressAsync(customer.ShippingAddressId.Value,false,true, customer);

       
                transactionRequest.shipTo = shipTo;
            }


            //// L2/L3 line items
            //if (authorizeNetSettings.EnableL2orL3)
            //{
            //    var cart = await _orderService.GetOrderItemsAsync(processPaymentRequest.Order?.Id ?? 0);
            //    var lineItems = new List<lineItemType>();

            //    // GetTaxTotalAsync confirmed
            //    await _taxService.GetTaxTotalAsync(Array.Empty<Nop.Core.Domain.Orders.ShoppingCartItem>());

            //    foreach (var cartItem in cart)
            //    {
            //        var product = await _productService.GetProductByIdAsync(cartItem.ProductId);
            //        if (product == null) continue;
            //        var (unitPrice, _) = await _priceCalculationService.GetUnitPriceAsync(cartItem, true);
            //        lineItems.Add(new lineItemType
            //        {
            //            itemId = cartItem.Id.ToString(),
            //            name = product.Name?.Length > 31 ? product.Name.Substring(0, 31) : product.Name,
            //            quantity = cartItem.Quantity,
            //            unitPrice = unitPrice,
            //            taxable = !product.IsTaxExempt
            //        });
            //    }

            //    if (lineItems.Count > 0)
            //        transactionRequest.lineItems = lineItems.ToArray();
            //}

            var req = new createTransactionRequest { merchantAuthentication = merchantAuth, transactionRequest = transactionRequest };
            var controller = new createTransactionController(req);

            return await GetApiResponseAsync(controller, errors);
        }
        public async Task<(string, ProcessPaymentRequest)> GetHostedFormToken(int invoiceId, dynamic additionalData)
        {
            var customer = new Customer();
            ProcessPaymentRequest paymentRequest = new ProcessPaymentRequest();
            string payload = string.Empty;
            try
            {
                var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
                var store = await _storeContext.GetCurrentStoreAsync();
                var communicatorUrl = $"{store.Url}plugins/Payments.AuthorizeNetHosted/content/Communicator.html";
                var cancelUrl = $"{store.Url}{urlHelper.RouteUrl("Checkout")}";
                var merchantAuth = PrepareAuthorizeNet();
                var currencyCode = (await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId))?.CurrencyCode;
                decimal orderTotal = 0;

                var transactionType = _authorizeNetHostedPaymentSettings.TransactMode == Domain.TransactMode.Authorize
                    ? "authOnlyTransaction"
                    : "authCaptureTransaction";



                if (invoiceId != 0)
                {
                    var customOrder = await _customOrderService.GetById(invoiceId);
                    int.TryParse(Convert.ToString(customOrder.CustomerId), out int customerId);
                    customer = await _customerService.GetCustomerByIdAsync(customerId);
                    if (additionalData != null)
                    {
                        decimal.TryParse(Convert.ToString(additionalData.PayableAmount), NumberStyles.Currency,
                           CultureInfo.CurrentCulture.NumberFormat, out orderTotal);
                    }
                    else
                    {
                        orderTotal = await _customOrderService.GetPayableAmount(customOrder);
                    }
                }
                else
                {
                    customer = await _workContext.GetCurrentCustomerAsync();
                    var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);
                    var (_orderTotal, _, _, _, _, _) = await _orderTotalCalculationService.GetShoppingCartTotalAsync(cart);
                    orderTotal = _orderTotal ?? 0;
                }

                paymentRequest = await this._orderProcessingService.GetProcessPaymentRequestAsync(customer);
                if (paymentRequest == null)
                {
                    paymentRequest = new ProcessPaymentRequest();
                }
                await this._orderProcessingService.SetProcessPaymentRequestAsync(paymentRequest, customer);
               


                var billToAddress = formatBillingAddress(
                    await GetTransactionRequestAddressForTokenAsync((customer?.BillingAddressId ?? 0) != 0 ? (int)customer.BillingAddressId : customer.ShippingAddressId ?? 0,true,false, customer));

                object shipToAddress = null;
                if (customer?.ShippingAddressId != null)
                {
                    shipToAddress = formatShippingAddress(await GetTransactionRequestAddressForTokenAsync(customer.ShippingAddressId.Value, false, true,customer));
                }

                // 1. Build an anonymous structure mapping strictly to the Authorize.Net REST endpoint.
                // This strips out any SDK-enforced default min-dates or missing schema models.
                var rawPayload = new
                {
                    getHostedPaymentPageRequest = new
                    {
                        merchantAuthentication = new
                        {
                            name = merchantAuth.name,
                            transactionKey = merchantAuth.Item
                        },
                        transactionRequest = new
                        {
                            transactionType = transactionType,
                            amount = orderTotal.ToString("0.00", CultureInfo.InvariantCulture), // Clean decimal payload
                            currencyCode = currencyCode,
                            order = new
                            {
                                invoiceNumber = paymentRequest.OrderGuid.ToString().Substring(0, 19),
                                description = $"Full order #{paymentRequest.OrderGuid}"
                                // Notice: purchaseOrderDateUTC is cleanly omitted entirely so it doesn't default to 0001-01-01
                            }
                            ,
                            customer = new
                            {
                                id = customer.Id.ToString(),
                                email = await _customerService.GetCustomerEmailAsync(customer)
                            },
                            //solution = new
                            //{
                            //    id = AuthorizeNetHostedPaymentDefaults.SolutionID,
                            //    name = AuthorizeNetHostedPaymentDefaults.SolutionName
                            //},
                            billTo = billToAddress,
                            shipTo = shipToAddress,
                            customerIP = _webHelper.GetCurrentIpAddress(),


                        },
                        hostedPaymentSettings = new
                        {
                            setting = new[]
                            {
                    new { settingName = "hostedPaymentPaymentOptions", settingValue = $"{{\"cardCodeRequired\":true,\"showCreditCard\":true,\"showBankAccount\":{_authorizeNetHostedPaymentSettings.EnableBankAccount.ToString().ToLower()}}}" },
                    new { settingName = "hostedPaymentButtonOptions", settingValue = "{\"text\": \"Pay Now\"}" },
                    new { settingName = "hostedPaymentOrderOptions", settingValue = "{\"show\": false}" },
                    new { settingName = "hostedPaymentReturnOptions", settingValue = "{\"showReceipt\":false}" },
                    new { settingName = "hostedPaymentIFrameCommunicatorUrl", settingValue = $"{{\"url\":\"{communicatorUrl}\"}}" },
                    new { settingName = "hostedPaymentBillingAddressOptions", settingValue = "{\"show\": false,\"required\": false}" },
                    new { settingName = "hostedPaymentStyleOptions", settingValue = "{\"bgColor\": \"#444444\"}" }
                }
                        }
                    }
                };

                payload = JsonConvert.SerializeObject(rawPayload, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                var jsonContent = new StringContent(payload, Encoding.UTF8, "application/json");


                string endpoint = _authorizeNetHostedPaymentSettings.UseSandbox
                    ? "https://apitest.authorize.net/xml/v1/request.api"
                    : "https://api.authorize.net/xml/v1/request.api";

                var responseMessage = await _httpClient.PostAsync(endpoint, jsonContent);
                var responseString = await responseMessage.Content.ReadAsStringAsync();

                if (!responseMessage.IsSuccessStatusCode)
                {
                    await _paymentLogger.ErrorAsync($"Authorize.NET: Connection failed with status code {responseMessage.StatusCode}");
                    throw new NopException($"Authorize.NET: Connection failed with status code {responseMessage.StatusCode}");
                }


                dynamic apiResponse = JsonConvert.DeserializeObject(responseString);

                if (apiResponse == null || apiResponse.messages == null)
                {
                    await _paymentLogger.ErrorAsync($"Authorize.NET: Connection failed with status code {responseString}");
                    throw new NopException("Authorize.NET: No valid response from GetHostedPaymentPage API.");
                }

                if (apiResponse.messages.resultCode.ToString().ToLower() != "ok")
                {
                    var msg = apiResponse.messages.message?[0]?.text?.ToString() ?? "Unknown error";
                    await _paymentLogger.ErrorAsync($"Authorize.NET error: {msg}");
                    throw new NopException($"Authorize.NET error: {msg}");
                }

                return (apiResponse.token.ToString(), paymentRequest);
            }
            catch (Exception exp)
            {
                await _paymentLogger.ErrorAsync(
           $"Hosted payment token generation failed. " +
           $"OrderGuid={paymentRequest.OrderGuid}, " +
           $"Error={exp.Message}" + $"CustomerId={customer?.Id ?? 0}" + $"payload={payload}" +
           (invoiceId > 0 ? $", CustomOrder, InvoiceId={invoiceId}, CustomerId={customer?.Id ?? 0}" : string.Empty), exp);
                throw exp;
            }
        }
        //public async Task<string> GetHostedFormToken(ProcessPaymentRequest paymentRequest, int invoiceId, dynamic additionalData)
        //{
        //    var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
        //    var store = await _storeContext.GetCurrentStoreAsync();
        //    var communicatorUrl = $"{store.Url}plugins/Payments.AuthorizeNetHosted/content/Communicator.html";
        //    var cancelUrl = $"{store.Url}{urlHelper.RouteUrl("Checkout")}";
        //    var merchantAuth = PrepareAuthorizeNet();
        //    var currencyCode = (await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId))?.CurrencyCode;
        //    decimal orderTotal = 0;
        //    // var opaqueData = new opaqueDataType { dataDescriptor = dataDescriptor, dataValue = dataValue };
        //    // var paymentType = new paymentType { Item = opaqueData };


        //    var transactionType = _authorizeNetHostedPaymentSettings.TransactMode == Domain.TransactMode.Authorize
        //        ? transactionTypeEnum.authOnlyTransaction
        //        : transactionTypeEnum.authCaptureTransaction;

        //    var customer = new Customer();

        //    if (invoiceId != 0)
        //    {
        //        var customOrder = await _customOrderService.GetById(invoiceId);
        //        int.TryParse(Convert.ToString(customOrder.CustomerId), out int customerId);
        //        customer = await _customerService.GetCustomerByIdAsync(customerId);
        //        if (additionalData != null)
        //        {
        //            decimal.TryParse(Convert.ToString(additionalData.PayableAmount), NumberStyles.Currency,
        //       CultureInfo.CurrentCulture.NumberFormat, out orderTotal);
        //        }
        //        else
        //        {
        //            orderTotal = await _customOrderService.GetPayableAmount(customOrder);
        //        }

        //    }
        //    else
        //    {
        //        customer = await _workContext.GetCurrentCustomerAsync();
        //        var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);
        //        var (_orderTotal, _, _, _, _, _) = await _orderTotalCalculationService.GetShoppingCartTotalAsync(cart);
        //        orderTotal = _orderTotal ?? 0;
        //    }

        //    var billTo = await GetTransactionRequestAddressAsync((customer?.BillingAddressId ?? 0) != 0 ? (int)customer.BillingAddressId : customer.ShippingAddressId ?? 0, customer);

        //    var transactionRequest = new transactionRequestType
        //    {
        //        transactionType = transactionType.ToString(),
        //        amount = orderTotal,
        //        currencyCode = currencyCode,
        //        amountSpecified = true,
        //        billTo = billTo,
        //        customerIP = _webHelper.GetCurrentIpAddress(),
        //        order = new orderType
        //        {
        //            invoiceNumber = paymentRequest.OrderGuid.ToString().Substring(0, 19),
        //            description = $"Full order #{paymentRequest.OrderGuid}",
        //            purchaseOrderDateUTC = DateTime.UtcNow
        //            //,
        //            //   purchaseOrderDateUTC = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, DateTime.UtcNow.Second, DateTimeKind.Utc),


        //        },
        //        solution = new solutionType
        //        {
        //            id = AuthorizeNetHostedPaymentDefaults.SolutionID,
        //            name = AuthorizeNetHostedPaymentDefaults.SolutionName
        //        },
        //        customer = new customerDataType
        //        {
        //            id = customer.Id.ToString(),
        //            email = await _customerService.GetCustomerEmail(customer)
        //        }
        //    };
        //    if (customer?.ShippingAddressId != null)
        //    {

        //        var shipTo = await GetTransactionRequestAddressAsync(customer.ShippingAddressId.Value, customer);

  
        //        transactionRequest.shipTo = shipTo;
        //    }

        //    var hostedSettings = new settingType[]
        //    {
        //        new settingType { settingName = "hostedPaymentPaymentOptions", settingValue = "{\"cardCodeRequired\":true,\"showCreditCard\":true,\"showBankAccount\":"+_authorizeNetHostedPaymentSettings.EnableBankAccount.ToString().ToLower()+"}" },
        //    new settingType { settingName = "hostedPaymentButtonOptions", settingValue = "{\"text\": \"Pay Now\"}" },
        //    new settingType { settingName = "hostedPaymentOrderOptions", settingValue = "{\"show\": false}" },
        //   new settingType { settingName = "hostedPaymentReturnOptions",settingValue =  $"{{\"showReceipt\":false}}"},
        //    new settingType { settingName = "hostedPaymentIFrameCommunicatorUrl",  settingValue = $"{{\"url\":\"{communicatorUrl}\"}}" },
        //  new settingType { settingName = "hostedPaymentBillingAddressOptions", settingValue = "{\"show\": false,\"required\": false}" },
        //      new settingType { settingName = "hostedPaymentStyleOptions", settingValue = "{\"bgColor\": \"#444444\"}" },


        //};


        //    var request = new getHostedPaymentPageRequest
        //    {
        //        merchantAuthentication = merchantAuth,
        //        transactionRequest = transactionRequest,
        //        hostedPaymentSettings = hostedSettings
        //    };
        //    string DATA = Newtonsoft.Json.JsonConvert.SerializeObject(request);
        //    var controller = new getHostedPaymentPageController(request);
        //    controller.Execute();

        //    var response = controller.GetApiResponse();

        //    if (response == null)
        //        throw new NopException("Authorize.NET: No response from GetHostedPaymentPage API.");

        //    if (response.messages.resultCode != messageTypeEnum.Ok)
        //    {
        //        var msg = response.messages.message?[0]?.text ?? "Unknown error";
        //        throw new NopException($"Authorize.NET error: {msg}");
        //    }
        //    return response.token;
        //}

        public async Task<(string customerProfileId, string paymentProfileId)> CreateProfileFromTransaction(
    string transId, Customer customer, string existingProfileId)
        {
            var merchantAuth = PrepareAuthorizeNet();

            await _paymentLogger.InformationAsync(
                $"CreateProfileFromTransaction started. " +
                $"CustomerId={customer?.Id}, TransId={transId}, " +
                $"ExistingProfileId={existingProfileId}");

            // Validate the stored profile id against Authorize.Net before trusting it
            bool existingProfileValid = false;
            if (!string.IsNullOrEmpty(existingProfileId))
            {
                existingProfileValid = await IsProfileValid(merchantAuth, existingProfileId);
                if (!existingProfileValid)
                {
                    await _paymentLogger.InformationAsync(
                        $"Stored profile id invalid at Authorize.Net, will create new profile. " +
                        $"CustomerId={customer.Id}, StaleProfileId={existingProfileId}");
                }
            }

            createCustomerProfileFromTransactionRequest request;

            if (existingProfileValid)
            {
                // Valid customer profile exists -> only add a payment profile to it
                request = new createCustomerProfileFromTransactionRequest
                {
                    merchantAuthentication = merchantAuth,
                    transId = transId,
                    customerProfileId = existingProfileId
                };
            }
            else
            {
                // No profile, or stale id -> create customer profile + payment profile
                request = new createCustomerProfileFromTransactionRequest
                {
                    merchantAuthentication = merchantAuth,
                    transId = transId,
                    customer = new customerProfileBaseType
                    {
                        merchantCustomerId = customer.Id.ToString(),
                        email = await _customerService.GetCustomerEmailAsync(customer)
                    }
                };
            }

            var controller = new createCustomerProfileFromTransactionController(request);
            controller.Execute();
            var response = controller.GetApiResponse();

            if (response == null)
                throw new NopException("Authorize.NET: No response from createCustomerProfileFromTransaction API.");

            if (response.messages?.message != null)
            {
                foreach (var m in response.messages.message)
                {
                    await _paymentLogger.InformationAsync(
                        $"CreateProfileFromTransaction message. " +
                        $"TransId={transId}, Code={m.code}, Text={m.text}");
                }
            }

            if (response.messages.resultCode != messageTypeEnum.Ok)
            {
                var msg = response.messages.message?[0]?.text ?? "Unknown error";
                throw new NopException($"Authorize.NET createCustomerProfileFromTransaction failed: {msg}");
            }

            var customerProfileId = response.customerProfileId;
            var paymentProfileId = response.customerPaymentProfileIdList?.FirstOrDefault();



            await _paymentLogger.InformationAsync(
                $"CreateProfileFromTransaction succeeded. " +
                $"CustomerId={customer.Id}, TransId={transId}, " +
                $"CustomerProfileId={customerProfileId}, PaymentProfileId={paymentProfileId}");

            return (customerProfileId, paymentProfileId);
        }


        // Returns true only if the profile id actually exists at Authorize.Net
        private async Task<bool> IsProfileValid(merchantAuthenticationType merchantAuth, string customerProfileId)
        {
            var request = new getCustomerProfileRequest
            {
                merchantAuthentication = merchantAuth,
                customerProfileId = customerProfileId
            };

            var controller = new getCustomerProfileController(request);
            controller.Execute();
            var response = controller.GetApiResponse();

            return response != null
                && response.messages.resultCode == messageTypeEnum.Ok
                && response.profile != null;
        }

        protected virtual async Task<customerAddressType> GetTransactionRequestAddressAsync(int addressId, bool isBillingEmail,
     bool isShippingEmail, Nop.Core.Domain.Customers.Customer customer)
        {
            var address = await _addressService.GetAddressByIdAsync(addressId);

            if (address == null)
                return new customerAddressType();

            var transactionRequestAddress = new customerAddressType
            {
                firstName = CommonHelper.EnsureMaximumLength(address.FirstName, 50),
                lastName = CommonHelper.EnsureMaximumLength(address.LastName, 50),

        

                email = CommonHelper.EnsureMaximumLength(
                    string.IsNullOrEmpty(address.Email) ? await _customerService.GetCustomerEmailAsync(customer, isBillingEmail,isShippingEmail) : address.Email, 50),
                phoneNumber = CommonHelper.EnsureMaximumLength(address.PhoneNumber, 15),
      

                address = CommonHelper.EnsureMaximumLength(address.Address1, 60),
                city = CommonHelper.EnsureMaximumLength(address.City, 40),
                zip = CommonHelper.EnsureMaximumLength(address.ZipPostalCode, 20)
            };

            if (!string.IsNullOrEmpty(address.Company))
                transactionRequestAddress.company = CommonHelper.EnsureMaximumLength(address.Company, 50);

            if (address.StateProvinceId.HasValue)
                transactionRequestAddress.state = (await _stateProvinceService.GetStateProvinceByAddressAsync(address))?.Abbreviation;

            if (address.CountryId.HasValue)
                transactionRequestAddress.country = (await _countryService.GetCountryByIdAsync(address.CountryId.Value))?.TwoLetterIsoCode;

            return transactionRequestAddress;
        }


        protected virtual async Task<object> GetTransactionRequestAddressForTokenAsync(int addressId, bool isBillingEmail = false,bool isShippingEmail = false, Customer customer)
        {
            var address = await _addressService.GetAddressByIdAsync(addressId);

            if (address == null)
                return null;

            string stateAbbreviation = null;
            if (address.StateProvinceId.HasValue)
                stateAbbreviation = (await _stateProvinceService.GetStateProvinceByAddressAsync(address))?.Abbreviation;

            string countryIso = null;
            if (address.CountryId.HasValue)
                countryIso = (await _countryService.GetCountryByIdAsync(address.CountryId.Value))?.TwoLetterIsoCode;

            // CRITICAL: The properties MUST be returned in this exact alphabetical/schema order.
            // Authorize.Net XML validation will fail if 'firstName' is placed before 'company'.

            string firstName = address.FirstName;
            string lastName = address.LastName;
            if (string.IsNullOrEmpty(firstName))
            {
                firstName = customer.FirstName;
                lastName = customer.LastName;
            }
            return new
            {
                company = !string.IsNullOrEmpty(address.Company) ? CommonHelper.EnsureMaximumLength(address.Company, 50) : null,
                firstName = CommonHelper.EnsureMaximumLength(firstName, 50),
                lastName = CommonHelper.EnsureMaximumLength(lastName, 50),
                address = CommonHelper.EnsureMaximumLength(address.Address1, 60),
                city = CommonHelper.EnsureMaximumLength(address.City, 40),
                state = stateAbbreviation,
                zip = CommonHelper.EnsureMaximumLength(address.ZipPostalCode, 20),
                country = countryIso,
                phoneNumber = CommonHelper.EnsureMaximumLength(address.PhoneNumber, 15),
                email = CommonHelper.EnsureMaximumLength(string.IsNullOrEmpty(address.Email) ? await _customerService.GetCustomerEmailAsync(customer, isBillingEmail, isShippingEmail) : address.Email, 50)
            };
        }

        // set_OrderId, set_Note, set_DisplayToCustomer, set_CreatedOnUtc, InsertOrderNoteAsync — all confirmed
        private async Task SaveOrderNoteAsync(Order order, string note)
        {
            await _orderService.InsertOrderNoteAsync(new OrderNote
            {
                OrderId = order.Id,
                Note = note,
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
        }

        private async Task LogMessageAsync(string message)
            => await _logger.WarningAsync($"[AuthorizeNetHostedPayment] {message}");

        Func<dynamic, object> formatShippingAddress = (dynamic addr) =>
        {
            if (addr == null) return null;
            var ordered = new System.Collections.Generic.Dictionary<string, object>();

            ordered.Add("firstName", addr.firstName);
            ordered.Add("lastName", addr.lastName);
            if (!string.IsNullOrEmpty(addr.company)) ordered.Add("company", addr.company);
            ordered.Add("address", addr.address);
            ordered.Add("city", addr.city);
            ordered.Add("state", addr.state);
            ordered.Add("zip", addr.zip);
            ordered.Add("country", addr.country);

            return ordered;
        };
        Func<dynamic, object> formatBillingAddress = (dynamic addr) =>
        {
            if (addr == null) return null;
            var ordered = new System.Collections.Generic.Dictionary<string, object>();

            ordered.Add("firstName", addr.firstName);
            ordered.Add("lastName", addr.lastName);
            if (!string.IsNullOrEmpty(addr.company)) ordered.Add("company", addr.company);
            ordered.Add("address", addr.address);
            ordered.Add("city", addr.city);
            ordered.Add("state", addr.state);
            ordered.Add("zip", addr.zip);
            ordered.Add("country", addr.country);
            ordered.Add("phoneNumber", addr.phoneNumber);
            ordered.Add("email", addr.email);

            return ordered;
        };

    }
}