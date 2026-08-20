using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.AuthorizeNetHosted.Logging;
using Nop.Plugin.Payments.AuthorizeNetHosted.Models;
using Nop.Plugin.Payments.AuthorizeNetHosted.Services;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Payments.AuthorizeNetHosted.Controllers
{

    public class AuthorizeNetHostedPaymentController : Nop.Web.Framework.Controllers.BasePluginController
    {
        private readonly AuthorizeNetHostedPaymentSettings _settings;
        private readonly IAuthorizeNetManager _authorizeNetManager;
        private readonly ILogger _logger;
        private readonly IPaymentService _paymentService;
        private readonly PaymentLogger _paymentLogger;

        public AuthorizeNetHostedPaymentController(
            AuthorizeNetHostedPaymentSettings settings,
            IAuthorizeNetManager authorizeNetManager,
            ILogger logger, IPaymentService paymentService,
            PaymentLogger paymentLogger)
        {
            _settings = settings;
            _authorizeNetManager = authorizeNetManager;
            _logger = logger;
            _paymentService = paymentService;
            _paymentLogger = paymentLogger;
        }

        [HttpPost]
        [Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryToken]
        public async Task<IActionResult> WebhookEventsHandler()
        {
            var webhookId = Guid.NewGuid().ToString("N").Substring(0, 8);

            await _paymentLogger.InformationAsync(
                $"Webhook received. WebhookId={webhookId}, " +
                $"RemoteIp={HttpContext?.Connection?.RemoteIpAddress}");

            try
            {
                // Read raw request body for signature verification
                string json;
                using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
                    json = await reader.ReadToEndAsync();

           
               // string rawBody = "{\"notificationId\":\"202b3e06-71f9-4680-8a17-b06da32dbda8\",\"eventType\":\"net.authorize.payment.authcapture.created\",\"eventDate\":\"2026-05-28T10:33:52.7311466Z\",\"webhookId\":\"0fa58a42-b90e-4bbb-b794-b7bf701e323a\",\"payload\":{\"responseCode\":1,\"authCode\":\"XEABCC\",\"avsResponse\":\"Y\",\"authAmount\":3380.88,\"fraudList\":[{\"fraudFilter\":\"Amount Filter\",\"fraudAction\":\"report\"}],\"entityName\":\"transaction\",\"id\":\"120083615082\"}}";
                await _paymentLogger.InformationAsync(
                    $"Webhook body read. WebhookId={webhookId}, " +
                    $"BodyLength={json?.Length}, Body={json}");

                // Verify X-Anet-Signature header using HMAC-SHA512 + SignatureKey
                var X_Anet_Signature = Request.Headers["X-Anet-Signature"].ToString();

                await _paymentLogger.InformationAsync(
                    $"Webhook signature header. WebhookId={webhookId}, " +
                    $"SignaturePresent={!string.IsNullOrEmpty(X_Anet_Signature)}, " +
                    $"SignatureKeyConfigured={!string.IsNullOrEmpty(_settings.SignatureKey)}");

                if (!string.IsNullOrEmpty(_settings.SignatureKey) &&
                    !string.IsNullOrEmpty(X_Anet_Signature))
                {
                   

                  

                    if (!IsValidSignature(json, X_Anet_Signature))
                    {
                        await _paymentLogger.ErrorAsync(
                            $"Webhook signature invalid — request rejected. " +
                            $"WebhookId={webhookId}, " +
                            $"Received={X_Anet_Signature}, Expected={""}");
                        await LogMessageAsync("Webhook: invalid signature — request rejected.");
                        return new StatusCodeResult(401);
                    }

                    await _paymentLogger.InformationAsync(
                        $"Webhook signature verified. WebhookId={webhookId}");
                }
                else
                {
                    await _paymentLogger.InformationAsync(
                        $"Webhook signature verification skipped. WebhookId={webhookId}");
                }

                // Parse the event envelope
                var eventObject = JsonConvert.DeserializeObject<eventResponse>(json);
                if (eventObject == null)
                {
                    await _paymentLogger.ErrorAsync(
                        $"Webhook eventResponse parse failed. WebhookId={webhookId}");
                    await LogMessageAsync("Webhook: could not parse eventResponse.");
                    return Ok();
                }

                await _paymentLogger.InformationAsync(
                    $"Webhook event parsed. WebhookId={webhookId}, " +
                    $"EventType={eventObject.eventType}, " +
                    $"EventId={eventObject.notificationId}");

                // Determine event type and dispatch
                var jsonEventType = eventObject.eventType?.ToLower() ?? "";
                if (jsonEventType.StartsWith("net.authorize.payment."))
                {
                    await _paymentLogger.InformationAsync(
                        $"Webhook dispatching payment event. WebhookId={webhookId}, " +
                        $"EventType={eventObject.eventType}");

                    // Payment event — this is what updates order status
                    var payEvent = JsonConvert.DeserializeObject<paymentEvent>(eventObject.responseBody ?? json);
                    if (payEvent != null)
                    {
                        await _authorizeNetManager.WebhookPaymentEventAsync(payEvent);

                        await _paymentLogger.InformationAsync(
                            $"Webhook payment event handled. WebhookId={webhookId}, " +
                            $"EventType={eventObject.eventType}");
                    }
                    else
                    {
                        await _paymentLogger.ErrorAsync(
                            $"Webhook paymentEvent deserialize returned null. " +
                            $"WebhookId={webhookId}, EventType={eventObject.eventType}");
                    }
                }
                else
                {
                    await _paymentLogger.InformationAsync(
                        $"Webhook event type not handled. WebhookId={webhookId}, " +
                        $"EventType={eventObject.eventType}");
                }
                // Other event types (fraud, customer, subscription) can be added here

                if (_settings.showDebugInfo)
                    await LogMessageAsync($"Webhook received: {eventObject.eventType}");

                await _paymentLogger.InformationAsync(
                    $"Webhook processing completed. WebhookId={webhookId}");
            }
            catch (Exception ex)
            {
                await _paymentLogger.ErrorAsync(
                    $"Webhook exception. WebhookId={webhookId}, Error={ex.Message}", ex);
                await LogMessageAsync($"Webhook exception: {ex.Message}");
            }
            return Ok();
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Communicator()
        {
           

            const string html = @"<!DOCTYPE html>
<html>
<head>
<meta charset=""utf-8"">
<script type=""text/javascript"">
function callParentFunction(str) {
    if (str && str.length > 0 && window.parent && window.parent.parent &&
        typeof window.parent.parent.AuthorizeNetIFrameResponse === 'function') {
        window.parent.parent.AuthorizeNetIFrameResponse(str);
    }
}

function receiveMessage() {
    if (window.location.hash && window.location.hash.length > 1) {
        callParentFunction(window.location.hash.substring(1));
    }
}

if (window.addEventListener) {
    window.addEventListener('load', receiveMessage, false);
} else if (window.attachEvent) {
    window.attachEvent('onload', receiveMessage);
}
</script>
</head>
<body></body>
</html>";

            return Content(html, "text/html");
        }


        [HttpGet]

        public async Task<IActionResult> GetToken(int invoiceId)
        {
            string token = string.Empty;


            var paymentRequest = new ProcessPaymentRequest();
            _paymentService.GenerateOrderGuid(paymentRequest);
            await _paymentLogger.InformationAsync(
   $"GetToken: hosted payment token refresh started. " +
   $"OrderGuid={paymentRequest.OrderGuid}" +
   (invoiceId > 0 ? $", CustomOrder, InvoiceId={invoiceId}" : string.Empty));
            try
            {
                token = await _authorizeNetManager.GetHostedFormToken(paymentRequest, invoiceId, null);
                await _paymentLogger.InformationAsync(
 $"GetToken: new hosted payment token generated successfully for iframe reload. " +
 $"OrderGuid={paymentRequest.OrderGuid}, " +
 $"TokenLength={token?.Length}" +
 (invoiceId > 0 ? $", CustomOrder, InvoiceId={invoiceId}" : string.Empty));
                var formUrl = _settings.UseSandbox
             ? AuthorizeNetHostedPaymentDefaults.SandboxFormUrl
             : AuthorizeNetHostedPaymentDefaults.ProductionFormUrl;


                return Json(new { success = true, formUrl = formUrl, token = token, orderId= paymentRequest.OrderGuid });
            }
            catch (Exception ex)
            {
                await _paymentLogger.ErrorAsync(
   $"GetToken: hosted payment token refresh failed. " +
   $"OrderGuid={paymentRequest.OrderGuid}, " +
   $"Error={ex.Message}" +
   (invoiceId > 0 ? $", CustomOrder, InvoiceId={invoiceId}" : string.Empty),
   ex);
                return Json(new { success = false });
            }




        }
        private bool IsValidSignature(string payload, string receivedSignature)
        {
            // Strip the "sha512=" prefix from the incoming header if present
            if (receivedSignature.StartsWith("sha512=", StringComparison.OrdinalIgnoreCase))
            {
                receivedSignature = receivedSignature.Substring(7);
            }

            // Authorize.net uses standard ISO 8859-1 or UTF-8 byte arrays for the HMAC payload
            byte[] keyBytes = Encoding.UTF8.GetBytes(_settings.SignatureKey);
            byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hashBytes = hmac.ComputeHash(payloadBytes);

                // Convert bytes to a lowercase hexadecimal string to compare
                string computedSignature = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

                return string.Equals(computedSignature, receivedSignature, StringComparison.OrdinalIgnoreCase);
            }
        }
        private async Task LogMessageAsync(string message)
        {
            await _logger.WarningAsync($"[AuthorizeNetHostedPayment] {message}");
        }
    }
}
