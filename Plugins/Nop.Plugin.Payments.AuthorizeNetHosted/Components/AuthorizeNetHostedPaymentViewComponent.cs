using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.AuthorizeNetHosted.Logging;
using Nop.Plugin.Payments.AuthorizeNetHosted.Models;
using Nop.Services.Payments;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.AuthorizeNetHosted.Components
{
    [ViewComponent(Name = "PaymentAuthorizeNetHosted")]
    public class AuthorizeNetHostedPaymentViewComponent : NopViewComponent
    {
        private readonly AuthorizeNetHostedPaymentSettings _settings;
        private readonly Services.IAuthorizeNetManager _authorizeNetManager;
        private readonly Nop.Services.Payments.IPaymentPluginManager _paymentPluginManager;
        private readonly Nop.Core.IWorkContext _workContext;
        private readonly Nop.Core.IStoreContext _storeContext;
        private readonly OrderSettings _orderSettings;
        private readonly IPaymentService _paymentService;
        private readonly PaymentLogger _paymentLogger;
        public AuthorizeNetHostedPaymentViewComponent(
            AuthorizeNetHostedPaymentSettings settings,
            Services.IAuthorizeNetManager authorizeNetManager,
            Nop.Services.Payments.IPaymentPluginManager paymentPluginManager,
            Nop.Core.IWorkContext workContext,
            Nop.Core.IStoreContext storeContext,
            OrderSettings orderSettings,
            IPaymentService paymentService,
            PaymentLogger paymentLogger)
        {
            _settings = settings;
            _authorizeNetManager = authorizeNetManager;
            _paymentPluginManager = paymentPluginManager;
            _workContext = workContext;
            _storeContext = storeContext;
            _orderSettings = orderSettings;
            _paymentService = paymentService;
            _paymentLogger = paymentLogger;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, dynamic additionalData)
        {
            if (additionalData != null)
            {

                var model = new Models.PaymentInfoModel
                {
                    LoginId = _settings.LoginId,
                    PublicClientKey = _settings.PublicClientKey,
                    UseSandbox = _settings.UseSandbox,
                    EnableBankAccount = _settings.EnableBankAccount,
                    OnePageCheckoutPlugin = _orderSettings.OnePageCheckoutEnabled
                };

                return View("~/Plugins/Payments.AuthorizeNetHosted/Views/AcceptPaymentInfo.cshtml", model);
            }
            else
            {
                string token = string.Empty;

                int invoiceId = 0;
                var paymentRequest = new ProcessPaymentRequest();
                _paymentService.GenerateOrderGuid(paymentRequest);

                if (additionalData == null)
                {
                    //int.TryParse(Convert.ToString(additionalData.Id), out invoiceId);
                    //orderType = additionalData.OrderType;
                    //if (orderType != "CustomOrder")
                    //    invoiceId = 0;
                    await _paymentLogger.InformationAsync(
                                $"Hosted payment token generation started. " +
                                $"OrderGuid={paymentRequest.OrderGuid}" +
                                (invoiceId > 0 ? $", CustomOrder, InvoiceId={invoiceId}" : string.Empty));
                    try
                    {
                        token = await _authorizeNetManager.GetHostedFormToken(paymentRequest, invoiceId, additionalData);
                        await _paymentLogger.InformationAsync(
                $"Hosted payment token generated successfully. " +
                $"OrderGuid={paymentRequest.OrderGuid}, " +
                $"TokenLength={token?.Length}" +
                (invoiceId > 0 ? $", CustomOrder, InvoiceId={invoiceId}" : string.Empty));
                    }
                    catch (Exception ex)
                    {
                        await _paymentLogger.ErrorAsync(
                $"Hosted payment token generation failed. " +
                $"OrderGuid={paymentRequest.OrderGuid}, " +
                $"Error={ex.Message}" +
                (invoiceId > 0 ? $", CustomOrder, InvoiceId={invoiceId}" : string.Empty), ex);
                        return View(
                            "~/Plugins/Payments.AuthorizeNetHosted/Views/ErrorInfo.cshtml",
                            new ErrorMessageModel { Warnings = new List<string>() { ex.Message } });
                    }
                }
                else
                    int.TryParse(Convert.ToString(additionalData.Id), out invoiceId);


                var formUrl = _settings.UseSandbox
                    ? AuthorizeNetHostedPaymentDefaults.SandboxFormUrl
                    : AuthorizeNetHostedPaymentDefaults.ProductionFormUrl;
                var model = new PaymentInfoModel
                {
                    FormActionUrl = formUrl,
                    Token = token,
                    OrderId = paymentRequest.OrderGuid,
                    CustomProperties = new Dictionary<string, object> { { "invoiceId", invoiceId } }
                };
                await _paymentLogger.InformationAsync(
            $"Hosted payment page initialized successfully. " +
            $"OrderGuid={paymentRequest.OrderGuid}" +
            (invoiceId > 0 ? $", CustomOrder, InvoiceId={invoiceId}" : string.Empty));
                return View("~/Plugins/Payments.AuthorizeNetHosted/Views/PaymentInfo.cshtml", model);
            }

        }

    }
}