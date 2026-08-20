using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthorizeNet.Api.Contracts.V1;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Infrastructure;
using Nop.Plugin.Payments.AuthorizeNetHosted.Domain;
using Nop.Plugin.Payments.AuthorizeNetHosted.Logging;
using Nop.Plugin.Payments.AuthorizeNetHosted.Services;
using Nop.Plugin.Payments.AuthorizeNetHosted.Validators;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Customizations.Custom;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;

namespace Nop.Plugin.Payments.AuthorizeNetHosted.Models
{

    public class AuthorizeNetPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly AuthorizeNetHostedPaymentSettings _authorizeNetHostedPaymentSettings;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly IStoreContext _storeContext;
        private readonly IAuthorizeNetManager _authorizeNetManager;
        private readonly ILogger _logger;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly PaymentLogger _paymentLogger;
        private readonly ICustomerService _customerService;
        private readonly IPaymentProfileService _paymentProfileService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public AuthorizeNetPaymentProcessor(
            AuthorizeNetHostedPaymentSettings authorizeNetHostedPaymentSettings,
            ISettingService settingService,
            ILocalizationService localizationService,
            IWebHelper webHelper,
            IStoreContext storeContext,
            IAuthorizeNetManager authorizeNetManager,
            ILogger logger,
            IPaymentService paymentService,
            IOrderService orderService,
            PaymentLogger paymentLogger,
            ICustomerService customerService,
            IPaymentProfileService paymentProfileService,
            IWorkflowMessageService workflowMessageService,
            IShoppingCartService shoppingCartService,
            IWorkContext workContext)
        {
            _authorizeNetHostedPaymentSettings = authorizeNetHostedPaymentSettings;
            _settingService = settingService;
            _localizationService = localizationService;
            _webHelper = webHelper;
            _storeContext = storeContext;
            _authorizeNetManager = authorizeNetManager;
            _logger = logger;
            _paymentService = paymentService;
            _orderService = orderService;
            _paymentLogger = paymentLogger;
            _customerService = customerService;
            _paymentProfileService = paymentProfileService;
            _workflowMessageService = workflowMessageService;
            _shoppingCartService = shoppingCartService;
            _workContext = workContext;
        }

        #endregion

        #region IPaymentMethod — Core

        /// <summary>
        /// Main payment processing. Reads Accept.js tokens from CustomValues,
        /// submits an authCapture (or authOnly) transaction to Authorize.NET.
        /// </summary>
        public async Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            if (processPaymentRequest.CustomValues.Where(k => k.Key == "InternalOrderId").Any())
            {
                return await ProcessPaymentAcceptForm(processPaymentRequest);
            }
            else
            {
                return await ProcessPaymentHostedForm(processPaymentRequest);
            }
        }

        /// <summary>
        /// Not used for Hosted/inline payment — no redirect needed.
        /// </summary>
        public Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
            => Task.CompletedTask;



        public Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
            => Task.FromResult(false);

        public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {


            return await _paymentService.CalculateAdditionalFeeAsync(cart,
                _authorizeNetHostedPaymentSettings.AdditionalFee, _authorizeNetHostedPaymentSettings.AdditionalFeePercentage);
        }

        #endregion

        #region IPaymentMethod — Capture / Refund / Void

        public async Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
        {
            var codes = capturePaymentRequest.Order.AuthorizationTransactionCode.Split(',');
            string transactionId = codes[0];
            return await this.CaptureTransaction(transactionId);
        }

        public async Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            var codes = (string.IsNullOrEmpty(refundPaymentRequest.Order.CaptureTransactionId)
    ? refundPaymentRequest.Order.AuthorizationTransactionCode
    : refundPaymentRequest.Order.CaptureTransactionId).Split(',');
            var transactionId = codes[0];
            var result = new RefundPaymentResult();
            try
            {
                var authorizeNetSettings = _authorizeNetHostedPaymentSettings;
                var transactionDetails = await _authorizeNetManager.GetTransactionDetailsAsync(
                  transactionId);

                if (transactionDetails == null || transactionDetails.messages.resultCode != messageTypeEnum.Ok)
                {
                    result.AddError(transactionDetails?.messages?.message?[0]?.text ?? "No response.");
                    return result;
                }

                if (transactionDetails.transaction.transactionStatus == "capturedPendingSettlement")
                {
                    if (refundPaymentRequest.IsPartialRefund)
                    {
                        result.Errors.Add("This transaction is supports only full refund");

                        return result;
                    }
                    var voidResult = await VoidAsync(new VoidPaymentRequest
                    {
                        Order = refundPaymentRequest.Order
                    });
                    if (!voidResult.Success)
                    {
                        foreach (var voidResultError in voidResult.Errors)
                            result.Errors.Add(voidResultError);

                        return result;
                    }
                    result.NewPaymentStatus = PaymentStatus.Refunded;

                    return result;
                }

                var merchantAuth = _authorizeNetManager.PrepareAuthorizeNet();
                string maskedCardNumber = string.Empty;

                if (transactionDetails.transaction.payment.Item is creditCardMaskedType card)
                    maskedCardNumber = card.cardNumber;   // e.g. "XXXX1111"

                else if (transactionDetails.transaction.payment.Item is bankAccountMaskedType bankAccount)
                    maskedCardNumber = bankAccount.accountNumber;  // also masked

                var creditCard = new AuthorizeNet.Api.Contracts.V1.creditCardType
                {
                    cardNumber = maskedCardNumber,
                    expirationDate = "XXXX"
                };

                var transactionRequest = new AuthorizeNet.Api.Contracts.V1.transactionRequestType
                {
                    transactionType = AuthorizeNet.Api.Contracts.V1.transactionTypeEnum.refundTransaction.ToString(),
                    amount = refundPaymentRequest.AmountToRefund,
                    refTransId = transactionId,
                    payment = new AuthorizeNet.Api.Contracts.V1.paymentType
                    {
                        Item = creditCard
                    }
                };

                var request = new AuthorizeNet.Api.Contracts.V1.createTransactionRequest
                {
                    merchantAuthentication = merchantAuth,
                    transactionRequest = transactionRequest
                };

                var controller = new AuthorizeNet.Api.Controllers.createTransactionController(request);
                List<string> errors = new List<string>();
                var response = await _authorizeNetManager.GetApiResponseAsync(controller, errors);

                if (response?.transactionResponse?.responseCode == "1")
                {
                    result.NewPaymentStatus = refundPaymentRequest.IsPartialRefund
                        ? PaymentStatus.PartiallyRefunded
                        : PaymentStatus.Refunded;
                }
                else
                {
                    result.AddError(response?.transactionResponse?.errors?[0]?.errorText ?? "Refund failed.");
                }
            }
            catch (Exception ex)
            {
                result.AddError(ex.Message);
            }

            return result;
        }

        public async Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
        {
            var codes = (string.IsNullOrEmpty(voidPaymentRequest.Order.CaptureTransactionId) ? voidPaymentRequest.Order.AuthorizationTransactionCode : voidPaymentRequest.Order.CaptureTransactionId).Split(',');
            string transactionId = codes[0];
            var result = new VoidPaymentResult();
            try
            {
                var merchantAuth = _authorizeNetManager.PrepareAuthorizeNet();
                var transactionRequest = new AuthorizeNet.Api.Contracts.V1.transactionRequestType
                {
                    transactionType = AuthorizeNet.Api.Contracts.V1.transactionTypeEnum.voidTransaction.ToString(),
                    refTransId = transactionId
                };

                var request = new AuthorizeNet.Api.Contracts.V1.createTransactionRequest
                {
                    merchantAuthentication = merchantAuth,
                    transactionRequest = transactionRequest
                };

                var controller = new AuthorizeNet.Api.Controllers.createTransactionController(request);
                List<string> errors = new List<string>();
                var response = await _authorizeNetManager.GetApiResponseAsync(controller, errors);

                if (response?.transactionResponse?.responseCode == "1")
                    result.NewPaymentStatus = PaymentStatus.Voided;
                else
                    result.AddError(response?.transactionResponse?.errors?[0]?.errorText ?? "Void failed.");
            }
            catch (Exception ex)
            {
                result.AddError(ex.Message);
            }

            return result;
        }

        #endregion

        #region IPaymentMethod — Recurring

        public Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return Task.FromResult(new ProcessPaymentResult
            {
                Errors = new[] { "Recurring payments not supported in this version." }
            });
        }

        public async Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(
            CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();

            if (string.IsNullOrEmpty(cancelPaymentRequest.Order.SubscriptionTransactionId))
            {
                result.AddError("Authorize.NET: subscription transaction ID is empty.");
                return result;
            }

            try
            {
                var merchantAuth = _authorizeNetManager.PrepareAuthorizeNet();
                var cancelRequest = new AuthorizeNet.Api.Contracts.V1.ARBCancelSubscriptionRequest
                {
                    merchantAuthentication = merchantAuth,
                    subscriptionId = cancelPaymentRequest.Order.SubscriptionTransactionId
                };

                var controller = new AuthorizeNet.Api.Controllers.ARBCancelSubscriptionController(cancelRequest);
                controller.Execute();
                var response = controller.GetApiResponse();

                if (response == null || response.messages.resultCode != AuthorizeNet.Api.Contracts.V1.messageTypeEnum.Ok)
                    result.AddError(response?.messages?.message?[0]?.text ?? "Cancel failed.");
            }
            catch (Exception ex)
            {
                result.AddError(ex.Message);
            }

            return result;
        }

        #endregion

        #region IPaymentMethod — Form / Info

        public Task<bool> CanRePostProcessPaymentAsync(Order order)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));

            return Task.FromResult(
                order.PaymentStatus == PaymentStatus.Pending &&
                (DateTime.UtcNow - order.CreatedOnUtc).TotalSeconds > 5);
        }

        /// <summary>
        /// Validates that Accept.js tokens are present in the form submission.
        /// </summary>
        public async Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            // Custom changes in main code
            if (!string.IsNullOrWhiteSpace(form["OrderId"]))
            {
                var errors = new List<string>();

                var errorsString = form["AuthorizeNetHostedPaymentErrors"].ToString();
                if (!string.IsNullOrEmpty(errorsString))
                    errors.Add(errorsString);
                var validator = new PaymentAcceptInfoValidator(_localizationService);
                var model = new PaymentInfoModel
                {
                    AuthorizeNetHostedPaymentDataValue = form[AuthorizeNetHostedPaymentDefaults.AuthorizeNetHostedPaymentDataValue],
                    AuthorizeNetHostedPaymentDataDescriptor = form[AuthorizeNetHostedPaymentDefaults.AuthorizeNetHostedPaymentDataDescriptor]
                };

                var validationResult = validator.Validate(model);

                if (!validationResult.IsValid)
                    errors.AddRange(validationResult.Errors.Select(error => error.ErrorMessage));

                return errors;
            }
            else
            {
                var errors = new List<string>();
                var validator = new PaymentInfoValidator(_localizationService);
                var model = new PaymentInfoModel
                {
                    TransactionId = form[AuthorizeNetHostedPaymentDefaults.TransactionValue]

                };

                var validationResult = validator.Validate(model);

                if (!validationResult.IsValid)
                    errors.AddRange(validationResult.Errors.Select(error => error.ErrorMessage));
                if (string.IsNullOrWhiteSpace(form["OrderId"]))
                {
                    if (validationResult.IsValid)
                    {
                        var customer = await _workContext.GetCurrentCustomerAsync();
                        var store = await _storeContext.GetCurrentStoreAsync();
                        var cart = await _shoppingCartService.GetShoppingCartAsync(
                            customer, ShoppingCartType.ShoppingCart, store.Id);
                        var _orderTotalCalculationService = EngineContext.Current.Resolve<IOrderTotalCalculationService>();

                        if (!cart.Any())
                        {
                            errors.Add("Your shopping cart is empty.");
                            return errors;
                        }

                        var (cartTotal, _, _, _, _, _) =
                            await _orderTotalCalculationService.GetShoppingCartTotalAsync(cart, usePaymentMethodAdditionalFee: false);

                        if (!cartTotal.HasValue || cartTotal.Value <= decimal.Zero)
                            errors.Add(await _localizationService.GetResourceAsync("Payments.Transaction.Amount.Mismatch"));
                    }
                }

                return errors;
            }
        }

        /// <summary>
        /// Extracts Accept.js opaque data tokens from the posted form
        /// and stores them in CustomValues for ProcessPaymentAsync.
        /// </summary>
        public async Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            var paymentRequest = new ProcessPaymentRequest();

            var transactionId = form["TransactionId"].ToString();

            if (!string.IsNullOrEmpty(transactionId))
                paymentRequest.CustomValues[AuthorizeNetHostedPaymentDefaults.TransactionValue] = transactionId;

            var authorizeNetHostedPaymentDataValue = form[AuthorizeNetHostedPaymentDefaults.AuthorizeNetHostedPaymentDataValue].ToString();

            if (!string.IsNullOrEmpty(authorizeNetHostedPaymentDataValue))
                paymentRequest.CustomValues[AuthorizeNetHostedPaymentDefaults.AuthorizeNetHostedPaymentDataValue] = authorizeNetHostedPaymentDataValue;

            var authorizeNetHostedPaymentDataDescriptor = form[AuthorizeNetHostedPaymentDefaults.AuthorizeNetHostedPaymentDataDescriptor].ToString();

            if (!string.IsNullOrEmpty(authorizeNetHostedPaymentDataDescriptor))
                paymentRequest.CustomValues[AuthorizeNetHostedPaymentDefaults.AuthorizeNetHostedPaymentDataDescriptor] = authorizeNetHostedPaymentDataDescriptor;

            var orderIdRef = form[AuthorizeNetHostedPaymentDefaults.OrderId].ToString();

            if (!string.IsNullOrEmpty(orderIdRef))
                paymentRequest.CustomValues[AuthorizeNetHostedPaymentDefaults.OrderId] = orderIdRef;

            return paymentRequest;
        }

        public override string GetConfigurationPageUrl()
            => $"{_webHelper.GetStoreLocation()}Admin/AuthorizeNetHostedPaymentSettings/Configure";

        public string GetPublicViewComponentName() => "PaymentAuthorizeNetHosted";

        public async Task<string> GetPaymentMethodDescriptionAsync()
            => await _localizationService.GetResourceAsync(
                "Plugins.Payments.AuthorizeNetHostedPayment.PaymentMethodDescription");

        #endregion

        #region Properties

        public bool SupportCapture => true;
        public bool SupportPartiallyRefund => true;
        public bool SupportRefund => true;
        public bool SupportVoid => true;
        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;
        public PaymentMethodType PaymentMethodType => PaymentMethodType.Standard;
        public bool SkipPaymentInfo => false;

        #endregion

        #region Install / Uninstall

        public override async Task InstallAsync()
        {
            var settings = new AuthorizeNetHostedPaymentSettings
            {
                UseSandbox = true,
                TransactMode = TransactMode.AuthorizeAndCapture,
                AdditionalFee = 0m,
                AdditionalFeePercentage = false,
                EnableL2orL3 = false,
                EnableVisaClicktoPay = false,
                EnableBankAccount = false,
                showDebugInfo = false
            };
            await _settingService.SaveSettingAsync(settings);

            await _localizationService.AddLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.Payments.AuthorizeNetHostedPayment.Fields.LoginId"] = "API Login ID",
                ["Plugins.Payments.AuthorizeNetHostedPayment.Fields.LoginId.Hint"] = "Your Authorize.NET API Login ID.",
                ["Plugins.Payments.AuthorizeNetHostedPayment.Fields.PublicClientKey"] = "Public Client Key",
                ["Plugins.Payments.AuthorizeNetHostedPayment.Fields.PublicClientKey.Hint"] = "Your Authorize.NET Public Client Key for Accept.js.",
                ["Plugins.Payments.AuthorizeNetHostedPayment.Fields.TransactionKey"] = "Transaction Key",
                ["Plugins.Payments.AuthorizeNetHostedPayment.Fields.TransactionKey.Hint"] = "Your Authorize.NET Transaction Key.",
                ["Plugins.Payments.AuthorizeNetHostedPayment.Fields.UseSandbox"] = "Use Sandbox",
                ["Plugins.Payments.AuthorizeNetHostedPayment.Fields.UseSandbox.Hint"] = "Check for sandbox testing.",
                ["Plugins.Payments.AuthorizeNetHostedPayment.Fields.TransactMode"] = "Transaction Mode",
                ["Plugins.Payments.AuthorizeNetHostedPayment.Fields.TransactMode.Hint"] = "Authorize only, or Authorize & Capture.",
                ["Plugins.Payments.AuthorizeNetHostedPayment.Fields.EnableL2orL3"] = "Enable L2/L3 Line Items",
                ["Plugins.Payments.AuthorizeNetHostedPayment.Fields.EnableL2orL3.Hint"] = "Send order line items to Authorize.NET (Level 2/3 data).",
                ["Plugins.Payments.AuthorizeNetHostedPayment.Fields.EnableVisaClicktoPay"] = "Enable Visa Click to Pay",
                ["Plugins.Payments.AuthorizeNetHostedPayment.Fields.VisaClicktoPayAPIKey"] = "Visa Click to Pay API Key",
                ["Plugins.Payments.AuthorizeNetHostedPayment.Fields.EnableBankAccount"] = "Enable Bank Account (eCheck)",
                ["Plugins.Payments.AuthorizeNetHostedPayment.Fields.EnableBankAccount.Hint"] = "Allow eCheck/bank account payments.",
                ["Plugins.Payments.AuthorizeNetHostedPayment.Fields.SignatureKey"] = "Webhook Signature Key",
                ["Plugins.Payments.AuthorizeNetHostedPayment.Fields.SignatureKey.Hint"] = "HMAC-SHA512 signature key for verifying webhook authenticity.",
                ["Plugins.Payments.AuthorizeNetHostedPayment.Fields.AdditionalFee"] = "Additional Fee",
                ["Plugins.Payments.AuthorizeNetHostedPayment.Fields.AdditionalFee.Hint"] = "Additional fee per transaction.",
                ["Plugins.Payments.AuthorizeNetHostedPayment.Fields.AdditionalFeePercentage"] = "Fee is Percentage",
                ["Plugins.Payments.AuthorizeNetHostedPayment.Fields.showDebugInfo"] = "Debug Mode",
                ["Plugins.Payments.AuthorizeNetHostedPayment.Fields.showDebugInfo.Hint"] = "Log detailed debug info.",
                ["Plugins.Payments.AuthorizeNetHostedPayment.Fields.SerialNumber"] = "License Serial Number",
                ["Plugins.Payments.AuthorizeNetHostedPayment.PaymentMethodDescription"] = "Pay securely with your credit/debit card or bank account.",
                ["Plugins.Payments.AuthorizeNetHostedPayment.WebhookHandler"] = "Authorize.NET Webhook"
            });

            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await _settingService.DeleteSettingAsync<AuthorizeNetHostedPaymentSettings>();
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.Payments.AuthorizeNetHostedPayment");
            await base.UninstallAsync();
        }

        #endregion

        #region Private
        private async Task<CapturePaymentResult> CaptureTransaction(string transactionId)
        {
            var result = new CapturePaymentResult();

            try
            {
                var merchantAuth = _authorizeNetManager.PrepareAuthorizeNet();
                var transactionRequest = new AuthorizeNet.Api.Contracts.V1.transactionRequestType
                {
                    transactionType = AuthorizeNet.Api.Contracts.V1.transactionTypeEnum.priorAuthCaptureTransaction.ToString(),
                    refTransId = transactionId
                };

                var request = new AuthorizeNet.Api.Contracts.V1.createTransactionRequest
                {
                    merchantAuthentication = merchantAuth,
                    transactionRequest = transactionRequest
                };

                var controller = new AuthorizeNet.Api.Controllers.createTransactionController(request);
                List<string> errors = new List<string>();
                var response = await _authorizeNetManager.GetApiResponseAsync(controller, errors);

                if (response?.transactionResponse?.responseCode == "1")
                {
                    result.CaptureTransactionId =
                $"{response.transactionResponse.transId}";
                    result.CaptureTransactionResult =
                        $"Approved ({response.transactionResponse.responseCode}: {response.transactionResponse.messages[0].description},{response.transactionResponse.authCode})";
                    result.NewPaymentStatus = PaymentStatus.Paid;
                }
                else
                {
                    result.AddError(response?.transactionResponse?.errors?[0]?.errorText ?? "Capture failed.");
                }
            }
            catch (Exception ex)
            {
                result.AddError(ex.Message);
            }

            return result;
        }
        private async Task LogMessageAsync(string message)
        {
            await _logger.WarningAsync($"[AuthorizeNetHostedPayment] {message}");
        }


        private async Task<ProcessPaymentResult> ProcessPaymentHostedForm(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            var customer = await _customerService.GetCustomerByIdAsync(processPaymentRequest.CustomerId);
            bool isCustomOrder = false;
            int customOrderNumber = 0;
            if (processPaymentRequest.CustomValues.Where(k => k.Key == "InternalOrderId").Any())
            {
                int.TryParse(processPaymentRequest.CustomValues.Where(k => k.Key == "InternalOrderId").FirstOrDefault().Value.ToString(), out customOrderNumber);
                isCustomOrder = true;
            }
            #region Validate Transaction Details

            string transactionId = processPaymentRequest.CustomValues[AuthorizeNetHostedPaymentDefaults.TransactionValue].ToString();
            string orderId = processPaymentRequest.CustomValues[AuthorizeNetHostedPaymentDefaults.OrderId].ToString();
            if (transactionId == "0")
            {
                await _paymentLogger.InformationAsync(
    $"ProcessPayment: Transaction Id is empty, Going to get latest transaction from authorize " +
    $"CustomerId={processPaymentRequest.CustomerId}, OrderGuid={processPaymentRequest.OrderGuid}, " +
    $"IsCustomOrder={isCustomOrder}  CustomOrderNumber={customOrderNumber}");

                transactionId = await _authorizeNetManager.FindTransactionIdByInvoiceAsync(string.IsNullOrEmpty(orderId) ? processPaymentRequest.OrderGuid.ToString() : orderId, processPaymentRequest.CustomerId);

                await _paymentLogger.InformationAsync(
    $"ProcessPayment: Transaction Id is empty, Transaction Id Found {transactionId}" +
    $"CustomerId={processPaymentRequest.CustomerId}, OrderGuid={processPaymentRequest.OrderGuid}, " +
    $"IsCustomOrder={isCustomOrder}  CustomOrderNumber={customOrderNumber}");

            }

            if (string.IsNullOrWhiteSpace(transactionId) && _authorizeNetHostedPaymentSettings.EnablePendingOrderOnMissingTransactionId)
            {

                await _paymentLogger.WarningAsync(
                                  $"ProcessPayment: transaction id not found (missing/empty and backup lookup failed). " +
                                  $"Placing order as Pending for manual review instead of rejecting it. " +
                                  $"CustomerId={processPaymentRequest.CustomerId}, OrderGuid={processPaymentRequest.OrderGuid}, " +
                                  $"IsCustomOrder={isCustomOrder}  CustomOrderNumber={customOrderNumber} orderId {orderId}");

                await _logger.InsertLogAsync(
                    Core.Domain.Logging.LogLevel.Warning,
                    $"Order placed as Pending - transaction id not found. CustomerId={processPaymentRequest.CustomerId} OrderGuid={processPaymentRequest.OrderGuid}",
                    $"Backup transaction lookup (by order id, then by customer name) did not return a usable " +
                    $"transaction id. Order was created as Pending; please verify the payment in Authorize.Net " +
                    $"and update the order manually if it was actually charged." +
                     $"IsCustomOrder={isCustomOrder}  CustomOrderNumber={customOrderNumber} orderId {orderId}",
                    customer);

                result.NewPaymentStatus = PaymentStatus.Pending;
                result.AuthorizationTransactionResult = "Pending - transaction id not found, manual review required.";

                return result;
            }

            if (string.IsNullOrWhiteSpace(transactionId))
            {
                await _paymentLogger.WarningAsync(
          $"ProcessPayment: missing/empty transaction id. " +
          $"CustomerId={processPaymentRequest.CustomerId}, OrderGuid={processPaymentRequest.OrderGuid}, " +
          $"IsCustomOrder={isCustomOrder}  CustomOrderNumber={customOrderNumber}");

                result.AddError(await _localizationService.GetResourceAsync(
                    "Payments.Invalid.Transaction"));

                return result;
            }

            await _paymentLogger.InformationAsync(
    $"ProcessPayment started. TransId={transactionId}, " +
    $"CustomerId={processPaymentRequest.CustomerId}, OrderGuid={processPaymentRequest.OrderGuid}, " +
    $"ExpectedTotal={processPaymentRequest.OrderTotal}, IsCustomOrder={isCustomOrder}  CustomOrderNumber={customOrderNumber}");

            var transactionDetails = await _authorizeNetManager.GetTransactionDetailsAsync(transactionId);

            if (transactionDetails == null || transactionDetails.messages.resultCode != messageTypeEnum.Ok)
            {
                await _paymentLogger.ErrorAsync(
        $"ProcessPayment: GetTransactionDetails failed. TransId={transactionId}, " +
        $"GatewayMessage={transactionDetails?.messages?.message?[0]?.text ?? "No response."}" +
         $"CustomerId={processPaymentRequest.CustomerId}, OrderGuid={processPaymentRequest.OrderGuid}, " +
    $"ExpectedTotal={processPaymentRequest.OrderTotal}, IsCustomOrder={isCustomOrder}  CustomOrderNumber={customOrderNumber}");

                result.AddError(transactionDetails?.messages?.message?[0]?.text ?? "No response.");
                return result;
            }
            await _paymentLogger.InformationAsync(
    $"ProcessPayment: TRANSACTION VERIFIED SUCCESSFULLY. " +
    $"TransId={transactionId}, " +
    $"CustomerId={processPaymentRequest.CustomerId}, " +
    $"OrderGuid={processPaymentRequest.OrderGuid}, " +
    $"PaidAmount={transactionDetails.transaction.settleAmount}, ExpectedAmount={processPaymentRequest.OrderTotal}, IsCustomOrder={isCustomOrder}  CustomOrderNumber={customOrderNumber}");

            if (transactionDetails.transaction.customer.id != processPaymentRequest.CustomerId.ToString())
            {
                await _paymentLogger.ErrorAsync(
           $"ProcessPayment: customer mismatch. TransId={transactionId}, " +
           $"TransactionCustomerId={transactionDetails.transaction.customer.id},RequestCustomerId ={processPaymentRequest.CustomerId}");

                result.AddError(await _localizationService.GetResourceAsync("Payments.Transaction.Customer.Mismatch"));
                return result;
            }
            await _paymentLogger.InformationAsync(
    $"ProcessPayment: CUSTOMER VERIFIED SUCCESSFULLY. " +
    $"TransId={transactionId}, " +
    $"TransactionCustomerId={transactionDetails.transaction.customer.id}, " +
    $"RequestCustomerId={processPaymentRequest.CustomerId}, " +
    $"OrderGuid={processPaymentRequest.OrderGuid}");
            if (transactionDetails.transaction.responseCode == 1)
            {
                if (await _orderService.GetOrderByTransactionId(transactionId, processPaymentRequest.PaymentMethodSystemName) != null)
                {
                    await _paymentLogger.WarningAsync(
         $"ProcessPayment: transaction already processed. TransId={transactionId}, " +
         $"CustomerId={processPaymentRequest.CustomerId}");
                    result.AddError(await _localizationService.GetResourceAsync("Payments.Transaction.Already.Processed"));
                    return result;
                }
                var expectedAmount = processPaymentRequest.OrderTotal;
                var paidAmount = transactionDetails.transaction.settleAmount;
                if (Math.Round(paidAmount, 2) != Math.Round(expectedAmount, 2))
                {
                    await _paymentLogger.ErrorAsync(
                        $"ProcessPayment: AMOUNT MISMATCH. TransId={transactionId}, " +
                        $"Paid={paidAmount}, Expected={expectedAmount}, " +
                        $"TransactionStatus={transactionDetails.transaction.transactionStatus}, CustomerId={processPaymentRequest.CustomerId}. " +
                        $"Customer charged but order will not be created — reversal/manual action required.");

                    var cartItems = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart);

                    await _workflowMessageService.SendSupportOrderTotalMismatchEmailMessage(customer, cartItems.Select(c => c.Id).ToArray(), (await this._workContext.GetWorkingLanguageAsync()).Id, transactionId, paidAmount, expectedAmount, "Authorize.Net");
                    result.AddError(await _localizationService.GetResourceAsync("Payments.Transaction.Amount.Mismatch"));
                    return result;
                }

                await _paymentLogger.InformationAsync(
    $"ProcessPayment: ORDER TOTAL VERIFIED SUCCESSFULLY. " +
    $"TransId={transactionId}, " +
    $"PaidAmount={paidAmount}, ExpectedAmount={expectedAmount}, " +
    $"CustomerId={processPaymentRequest.CustomerId}, " +
    $"OrderGuid={processPaymentRequest.OrderGuid}");

                switch (_authorizeNetHostedPaymentSettings.TransactMode)
                {
                    case TransactMode.Authorize:
                        result.AuthorizationTransactionId = transactionDetails.transaction.transId;
                        result.AuthorizationTransactionCode =
                            $"{transactionDetails.transaction.transId},{transactionDetails.transaction.authCode}";
                        break;
                    case TransactMode.AuthorizeAndCapture:
                        result.CaptureTransactionId =
                            $"{transactionDetails.transaction.transId}";
                        break;
                }
                result.NewPaymentStatus = transactionDetails.transaction.responseCode == 4 ? PaymentStatus.Pending : _authorizeNetHostedPaymentSettings.TransactMode == TransactMode.Authorize ? PaymentStatus.Authorized : PaymentStatus.Paid;
                if (_authorizeNetHostedPaymentSettings.TransactMode == TransactMode.Authorize && transactionDetails.transaction.responseCode == 1)
                {
                    await _paymentLogger.InformationAsync(
$"ProcessPayment Process started to capture transaction. TransId={transactionId}, " +
$"CustomerId={processPaymentRequest.CustomerId}, OrderGuid={processPaymentRequest.OrderGuid}");
                    var captureResult = await this.CaptureTransaction(transactionDetails.transaction.transId);
                    if (captureResult.Errors.Count > 0)
                    {
                        await _paymentLogger.ErrorAsync(
$"ProcessPayment Process failed to capture transaction. TransId={transactionId}, " +
$"CustomerId={processPaymentRequest.CustomerId}, OrderGuid={processPaymentRequest.OrderGuid}");
                        await _logger.InsertLogAsync
                            (Core.Domain.Logging.LogLevel.Error,
                            $"Failed to capture transaction {result.AuthorizationTransactionId} Customerid {processPaymentRequest.CustomerId}  Order Id {processPaymentRequest.OrderGuid} ",
                           string.Join(" ", captureResult.Errors),
                           await _customerService.GetCustomerByIdAsync(processPaymentRequest.CustomerId));
                    }
                    else
                    {
                        await _paymentLogger.ErrorAsync(
$"ProcessPayment Process completed to capture transaction. TransId={transactionId}, " +
$"CustomerId={processPaymentRequest.CustomerId}, OrderGuid={processPaymentRequest.OrderGuid}");
                        result.CaptureTransactionId =
                          $"{captureResult.CaptureTransactionId}";
                        result.CaptureTransactionResult =
                          $"{captureResult.CaptureTransactionResult}";
                        result.NewPaymentStatus = PaymentStatus.Paid;
                    }
                }


                result.AuthorizationTransactionResult =
                    $"Approved ({transactionDetails.transaction.responseCode}: {transactionDetails.messages.message[0].text}),{transactionDetails.transaction.authCode}";
                result.AvsResult = transactionDetails.transaction.AVSResponse;



                if (processPaymentRequest.CreateCim)
                {
                    await _paymentLogger.InformationAsync(
                       $"ProcessPayment: Process started to create CIM {transactionDetails.transaction.transId} CustomerId={processPaymentRequest.CustomerId}, OrderGuid={processPaymentRequest.OrderGuid}, " +
   $"IsCustomOrder={isCustomOrder} CustomOrderNumber={customOrderNumber}, " +
   $"TransId={transactionDetails.transaction.transId}");
                    string maskedCardNumber = string.Empty;
                    string cardType = string.Empty;
                    string expiryDate = string.Empty;
                    string expirationMonth = string.Empty;
                    string expirationYear = string.Empty;
                    if (transactionDetails.transaction.payment.Item is creditCardMaskedType card)
                    {
                        maskedCardNumber = card.cardNumber;
                        cardType = card.cardType;
                        expiryDate = card.expirationDate;
                        if (!string.IsNullOrEmpty(expiryDate) && expiryDate.Contains("-"))
                        {
                            var parts = expiryDate.Split('-');
                            if (parts.Length == 2)
                            {
                                expirationYear = parts[0];                       // "2030"
                                expirationMonth = parts[1].PadLeft(2, '0');      // "12"
                            }
                        }
                    }
                    try
                    {
                        string _paymentProfileId = string.Empty;
                        string _customerProfileId = string.Empty;
                        _customerProfileId = customer?.CIM_ProfileId.ToString() ?? string.Empty;
                        _customerProfileId = _customerProfileId == "0" ? string.Empty : _customerProfileId;
                        (_customerProfileId, _paymentProfileId) = await _authorizeNetManager.CreateProfileFromTransaction(transactionDetails.transaction.transId, customer, _customerProfileId);

                        long.TryParse(_customerProfileId, out long customerProfileId);
                        long.TryParse(_paymentProfileId, out long customerPaymentProfileId);

                        customer.CIM_ProfileId = customerProfileId;
                        await _customerService.UpdateCustomerAsync(customer);

                        int addressId = processPaymentRequest.BillingAddressId.HasValue ? processPaymentRequest.BillingAddressId.Value :
                (processPaymentRequest.ShippingAddressId.HasValue ? processPaymentRequest.ShippingAddressId.Value : 0);
                        await _paymentProfileService.SavePaymentProfile(new Core.Domain.Customization.Orders.PaymentProfile()
                        {
                            AddressId = addressId,
                            AuthorizeNetProfileId = customerPaymentProfileId,
                            CardType = cardType,
                            CreatedOn = DateTime.Now,
                            CustomerId = customer.Id,
                            CustomOrderNo = customOrderNumber,
                            ExpirationMonth = expirationMonth,
                            ExpirationYear = expirationYear,
                            UpdatedOn = DateTime.Now
                        });
                        await _paymentLogger.InformationAsync(
                      $"ProcessPayment: Process started to create CIM {transactionDetails.transaction.transId} CustomerId={processPaymentRequest.CustomerId}, OrderGuid={processPaymentRequest.OrderGuid}, " +
  $"IsCustomOrder={isCustomOrder} CustomOrderNumber={customOrderNumber}, " +
  $"TransId={transactionDetails.transaction.transId}");
                    }
                    catch (Exception exp)
                    {
                        await _paymentLogger.ErrorAsync(
                      $"ProcessPayment: Process started to create CIM {transactionDetails.transaction.transId} CustomerId={processPaymentRequest.CustomerId}, OrderGuid={processPaymentRequest.OrderGuid}, " +
  $"IsCustomOrder={isCustomOrder} CustomOrderNumber={customOrderNumber}, " +
  $"TransId={transactionDetails.transaction.transId}", exp);
                    }
                }

                await _paymentLogger.InformationAsync(
$"ProcessPayment Completed. TransId={transactionId}, " +
$"CustomerId={processPaymentRequest.CustomerId}, OrderGuid={processPaymentRequest.OrderGuid}");

                return result;
            }
            else
            {
                await _paymentLogger.WarningAsync(
       $"ProcessPayment: transaction not approved. TransId={transactionId}, " +
       $"ResponseCode={transactionDetails.transaction.responseCode}");
                result.AddError(await _localizationService.GetResourceAsync("Payments.Transaction.Payment.NotApproved"));
                return result;
            }




            #endregion


        }

        private async Task<ProcessPaymentResult> ProcessPaymentAcceptForm(ProcessPaymentRequest processPaymentRequest)
        {
            try
            {
                var result = new ProcessPaymentResult();
                var customer = await _customerService.GetCustomerByIdAsync(processPaymentRequest.CustomerId);
                processPaymentRequest.CustomValues.TryGetValue(
                  AuthorizeNetHostedPaymentDefaults.AuthorizeNetHostedPaymentDataValue, out var dataValueObj);
                processPaymentRequest.CustomValues.TryGetValue(
                    AuthorizeNetHostedPaymentDefaults.AuthorizeNetHostedPaymentDataDescriptor, out var dataDescriptorObj);

                var dataValue = dataValueObj?.ToString();
                var dataDescriptor = dataDescriptorObj?.ToString();
                bool isCustomOrder = false;
                int customOrderNumber = 0;
                if (processPaymentRequest.CustomValues.Where(k => k.Key == "InternalOrderId").Any())
                {
                    int.TryParse(processPaymentRequest.CustomValues.Where(k => k.Key == "InternalOrderId").FirstOrDefault().Value.ToString(), out customOrderNumber);
                    isCustomOrder = true;
                }

                await _paymentLogger.InformationAsync(
     $"ProcessPayment: Started  CustomerId={processPaymentRequest.CustomerId}, OrderGuid={processPaymentRequest.OrderGuid}, " +
     $"IsCustomOrder={isCustomOrder} CustomOrderNumber={customOrderNumber}, " +
     $"AuthorizenetHostedaymentDataValue={dataValueObj} AuthorizenetHostedPaymentdataDescriptor={dataDescriptorObj}");

                if (string.IsNullOrEmpty(dataValue) || string.IsNullOrEmpty(dataDescriptor))
                {
                    await _paymentLogger.ErrorAsync(
        $"ProcessPayment: Failed to Process,DataValue or DataDescriptor InValid  CustomerId={processPaymentRequest.CustomerId}, OrderGuid={processPaymentRequest.OrderGuid}, " +
        $"IsCustomOrder={isCustomOrder} CustomOrderNumber={customOrderNumber}, " +
        $"AuthorizenetHostedaymentDataValue={dataValueObj} AuthorizenetHostedPaymentdataDescriptor={dataDescriptorObj}");

                    result.AddError("Authorize.NET: payment token not found. Please re-enter card details.");
                    return result;
                }

                await _paymentLogger.InformationAsync(
    $"ProcessPayment: Going to process Transaction, CustomerId={processPaymentRequest.CustomerId}, OrderGuid={processPaymentRequest.OrderGuid}, " +
    $"IsCustomOrder={isCustomOrder} CustomOrderNumber={customOrderNumber}, " +
    $"AuthorizenetHostedaymentDataValue={dataValueObj} AuthorizenetHostedPaymentdataDescriptor={dataDescriptorObj}");

                var transactionDetails = await this._authorizeNetManager.CreateTransactionAsync(dataValue, dataDescriptor, processPaymentRequest, result.Errors);


                if (transactionDetails == null)
                {
                    await _paymentLogger.ErrorAsync(
     $"ProcessPayment: Failed to Process, Transaction Failed to Process,  CustomerId={processPaymentRequest.CustomerId}, OrderGuid={processPaymentRequest.OrderGuid}, " +
     $"IsCustomOrder={isCustomOrder} CustomOrderNumber={customOrderNumber}, " +
     $"AuthorizenetHostedaymentDataValue={dataValueObj} AuthorizenetHostedPaymentdataDescriptor={dataDescriptorObj},{string.Join(',', result.Errors)}");
                    result.AddError("Authorize.NET: no response received.");
                    return result;
                }

                await _paymentLogger.InformationAsync(
    $"ProcessPayment: Transaction processed successfully, CustomerId={processPaymentRequest.CustomerId}, OrderGuid={processPaymentRequest.OrderGuid}, " +
    $"IsCustomOrder={isCustomOrder} CustomOrderNumber={customOrderNumber}, " +
    $"AuthorizenetHostedaymentDataValue={dataValueObj} AuthorizenetHostedPaymentdataDescriptor={dataDescriptorObj}");



                #region Validate Transaction Details



                if (transactionDetails.transactionResponse.responseCode == "1" || transactionDetails.transactionResponse.responseCode == "4")
                {
                    if (await _orderService.GetOrderByTransactionId(transactionDetails.transactionResponse.transId, processPaymentRequest.PaymentMethodSystemName) != null)
                    {
                        await _paymentLogger.ErrorAsync(
                             $"ProcessPayment: transaction already processed  CustomerId={processPaymentRequest.CustomerId}, OrderGuid={processPaymentRequest.OrderGuid}, " +
     $"IsCustomOrder={isCustomOrder} CustomOrderNumber={customOrderNumber}, " +
     $"AuthorizenetHostedaymentDataValue={dataValueObj} AuthorizenetHostedPaymentdataDescriptor={dataDescriptorObj} " +
     $"TransId={transactionDetails.transactionResponse.transId}");

                        result.AddError(await _localizationService.GetResourceAsync("Payments.Transaction.Already.Processed"));
                        return result;
                    }



                    switch (_authorizeNetHostedPaymentSettings.TransactMode)
                    {
                        case TransactMode.Authorize:
                            result.AuthorizationTransactionId = transactionDetails.transactionResponse.transId;
                            result.AuthorizationTransactionCode =
                                $"{transactionDetails.transactionResponse.transId},{transactionDetails.transactionResponse.authCode}";
                            break;
                        case TransactMode.AuthorizeAndCapture:
                            result.CaptureTransactionId =
                                $"{transactionDetails.transactionResponse.transId}";
                            break;
                    }
                    result.NewPaymentStatus = transactionDetails.transactionResponse.responseCode == "4" ? PaymentStatus.Pending : _authorizeNetHostedPaymentSettings.TransactMode == TransactMode.Authorize ? PaymentStatus.Authorized : PaymentStatus.Paid;
                    if (_authorizeNetHostedPaymentSettings.TransactMode == TransactMode.Authorize && transactionDetails.transactionResponse.responseCode == "1")
                    {
                        await _paymentLogger.InformationAsync(
                           $"ProcessPayment: Process started to capture transaction {transactionDetails.transactionResponse.transId} CustomerId={processPaymentRequest.CustomerId}, OrderGuid={processPaymentRequest.OrderGuid}, " +
    $"IsCustomOrder={isCustomOrder} CustomOrderNumber={customOrderNumber}, " +
    $"AuthorizenetHostedaymentDataValue={dataValueObj} AuthorizenetHostedPaymentdataDescriptor={dataDescriptorObj} " +
    $"TransId={transactionDetails.transactionResponse.transId}");
                        var captureResult = await this.CaptureTransaction(transactionDetails.transactionResponse.transId);
                        if (captureResult.Errors.Count > 0)
                        {
                            await _paymentLogger.ErrorAsync(
                          $"ProcessPayment: Failed to capture transaction {transactionDetails.transactionResponse.transId} CustomerId={processPaymentRequest.CustomerId}, OrderGuid={processPaymentRequest.OrderGuid}, " +
    $"IsCustomOrder={isCustomOrder} CustomOrderNumber={customOrderNumber}, " +
    $"AuthorizenetHostedaymentDataValue={dataValueObj} AuthorizenetHostedPaymentdataDescriptor={dataDescriptorObj} " +
    $"TransId={transactionDetails.transactionResponse.transId},{string.Join(" ", captureResult.Errors)}");

                            await _logger.InsertLogAsync
                                (Core.Domain.Logging.LogLevel.Error,
                                $"Failed to capture transaction {result.AuthorizationTransactionId} Customerid {processPaymentRequest.CustomerId}  Order Id {processPaymentRequest.OrderGuid} ",
                               string.Join(" ", captureResult.Errors),
                               customer);
                        }
                        else
                        {
                            await _paymentLogger.InformationAsync(
                       $"ProcessPayment: Transaction capture successfully, {transactionDetails.transactionResponse.transId} CustomerId={processPaymentRequest.CustomerId}, OrderGuid={processPaymentRequest.OrderGuid}, " +
    $"IsCustomOrder={isCustomOrder} CustomOrderNumber={customOrderNumber}, " +
    $"AuthorizenetHostedaymentDataValue={dataValueObj} AuthorizenetHostedPaymentdataDescriptor={dataDescriptorObj} " +
    $"TransId={transactionDetails.transactionResponse.transId}");

                            result.CaptureTransactionId =
                              $"{captureResult.CaptureTransactionId}";
                            result.CaptureTransactionResult =
                              $"{captureResult.CaptureTransactionResult}";
                            result.NewPaymentStatus = PaymentStatus.Paid;
                        }
                    }


                    result.AuthorizationTransactionResult =
                        $"Approved ({transactionDetails.transactionResponse.responseCode}: {transactionDetails.messages.message[0].text}),{transactionDetails.transactionResponse.authCode}";
                    result.AvsResult = transactionDetails.transactionResponse.avsResultCode;



                    if (processPaymentRequest.CreateCim)
                    {
                        await _paymentLogger.InformationAsync(
                        $"ProcessPayment: Process started to create CIM {transactionDetails.transactionResponse.transId} CustomerId={processPaymentRequest.CustomerId}, OrderGuid={processPaymentRequest.OrderGuid}, " +
    $"IsCustomOrder={isCustomOrder} CustomOrderNumber={customOrderNumber}, " +
    $"AuthorizenetHostedaymentDataValue={dataValueObj} AuthorizenetHostedPaymentdataDescriptor={dataDescriptorObj} " +
    $"TransId={transactionDetails.transactionResponse.transId}");
                        string maskedCardNumber = string.Empty;
                        string cardType = string.Empty;
                        string expiryDate = string.Empty;
                        string expirationMonth = string.Empty;
                        string expirationYear = string.Empty;
                        //if (transactionDetails.transactionResponse..Item is creditCardMaskedType card)
                        //{
                        //    maskedCardNumber = card.cardNumber;
                        //    cardType = card.cardType;
                        //    expiryDate = card.expirationDate;
                        //    if (!string.IsNullOrEmpty(expiryDate) && expiryDate.Contains("-"))
                        //    {
                        //        var parts = expiryDate.Split('-');
                        //        if (parts.Length == 2)
                        //        {
                        //            expirationYear = parts[0];                       // "2030"
                        //            expirationMonth = parts[1].PadLeft(2, '0');      // "12"
                        //        }
                        //    }
                        //}
                        try
                        {
                            string _paymentProfileId = string.Empty;
                            string _customerProfileId = string.Empty;
                            _customerProfileId = customer?.CIM_ProfileId.ToString() ?? string.Empty;
                            _customerProfileId = _customerProfileId == "0" ? string.Empty : _customerProfileId;
                            (_customerProfileId, _paymentProfileId) = await _authorizeNetManager.CreateProfileFromTransaction(transactionDetails.transactionResponse.transId, customer, _customerProfileId);

                            long.TryParse(_customerProfileId, out long customerProfileId);
                            long.TryParse(_paymentProfileId, out long customerPaymentProfileId);

                            customer.CIM_ProfileId = customerProfileId;
                            await _customerService.UpdateCustomerAsync(customer);

                            int addressId = processPaymentRequest.BillingAddressId.HasValue ? processPaymentRequest.BillingAddressId.Value :
                    (processPaymentRequest.ShippingAddressId.HasValue ? processPaymentRequest.ShippingAddressId.Value : 0);
                            await _paymentProfileService.SavePaymentProfile(new Core.Domain.Customization.Orders.PaymentProfile()
                            {
                                AddressId = addressId,
                                AuthorizeNetProfileId = customerPaymentProfileId,
                                CardType = cardType,
                                CreatedOn = DateTime.Now,
                                CustomerId = customer.Id,
                                CustomOrderNo = customOrderNumber,
                                ExpirationMonth = expirationMonth,
                                ExpirationYear = expirationYear,
                                UpdatedOn = DateTime.Now
                            });
                            await _paymentLogger.InformationAsync(
                   $"Payment Profile: Process Complete to create CIM {transactionDetails.transactionResponse.transId} CustomerId={processPaymentRequest.CustomerId}, OrderGuid={processPaymentRequest.OrderGuid}, " +
    $"IsCustomOrder={isCustomOrder} CustomOrderNumber={customOrderNumber}, " +
    $"AuthorizenetHostedaymentDataValue={dataValueObj} AuthorizenetHostedPaymentdataDescriptor={dataDescriptorObj} " +
    $"TransId={transactionDetails.transactionResponse.transId}");
                        }
                        catch (Exception exp)
                        {

                            await _paymentLogger.ErrorAsync(
                      $"Payment Profile: Failed to create CIM {transactionDetails.transactionResponse.transId} CustomerId={processPaymentRequest.CustomerId}, OrderGuid={processPaymentRequest.OrderGuid}, " +
    $"IsCustomOrder={isCustomOrder} CustomOrderNumber={customOrderNumber}, " +
    $"AuthorizenetHostedaymentDataValue={dataValueObj} AuthorizenetHostedPaymentdataDescriptor={dataDescriptorObj} " +
    $"TransId={transactionDetails.transactionResponse.transId}", exp);


                        }
                    }

                    await _paymentLogger.InformationAsync(
$"ProcessPayment Completed. TransId={transactionDetails.transactionResponse.transId}, " +
$"CustomerId={processPaymentRequest.CustomerId}, OrderGuid={processPaymentRequest.OrderGuid}");
                    return result;
                }
                else
                {
                    await _paymentLogger.WarningAsync(
           $"ProcessPayment: transaction not approved. TransId={transactionDetails.transactionResponse.transId}, " +
           $"ResponseCode={transactionDetails.transactionResponse.responseCode}" +
           $"{string.Join(',', transactionDetails.transactionResponse.errors.Select(e => e.errorText))}");
                    result.AddError(await _localizationService.GetResourceAsync("Payments.Transaction.Payment.NotApproved"));
                    await _paymentLogger.InformationAsync(
$"ProcessPayment Completed. TransId={transactionDetails.transactionResponse.transId}, " +
$"CustomerId={processPaymentRequest.CustomerId}, OrderGuid={processPaymentRequest.OrderGuid}");
                    return result;
                }
                #endregion


            }
            catch (Exception exp)
            {

                await _paymentLogger.ErrorAsync(
$"ProcessPayment Failed " +
$"CustomerId={processPaymentRequest.CustomerId}, OrderGuid={processPaymentRequest.OrderGuid}", exp);
                await _paymentLogger.InformationAsync(
$"ProcessPayment Completed " +
$"CustomerId={processPaymentRequest.CustomerId}, OrderGuid={processPaymentRequest.OrderGuid}");
                throw exp;
            }
        }


        #endregion
    }
}