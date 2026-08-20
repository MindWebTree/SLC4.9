using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.AuthorizeNetHosted.Models
{
    public enum AuthorizeNetEvent
    {
        CustomerEvent = 0,
        FraudEvent = 1,
        PaymentEvent = 2,
        PaymentProfileEvent = 3,
        SubscriptionEvent = 4,
        UnknownEvent = 5,
        InvalidEvent = 6
    }

    public enum WebhookEventType
    {
        // net.authorize.payment.*
        // net.authorize.customer.*
        // net.authorize.fraud.*
        // net.authorize.customer.paymentProfile.*
        // net.authorize.payment.subscription.*
    }

    // ── Webhook Payload Models ─────────────────────────────────────────────────

    public class FraudPayload
    {
        public string fraudFilter { get; set; }
        public string fraudAction { get; set; }
    }

    public class FraudList
    {
        public string responseCode { get; set; }
        public string authCode { get; set; }
        public string avsResponse { get; set; }
        public decimal authAmount { get; set; }
        public IList<FraudPayload> fraudList { get; set; }
    }

    public class PayloadPayment
    {
        public string responseCode { get; set; }
        public string merchantReferenceId { get; set; }
        public string authCode { get; set; }
        public string avsResponse { get; set; }
        public decimal authAmount { get; set; }
        public string entityName { get; set; }
        public string id { get; set; }
    }

    public class PayloadProfile
    {
        public string customerProfileId { get; set; }
        public string entityName { get; set; }
        public string id { get; set; }
    }

    public class PayloadCustomer
    {
        public string customerType { get; set; }
        public string id { get; set; }
    }

    public class CustomerProfile
    {
        public IList<PaymentProfile> paymentProfiles { get; set; }
        public string merchantCustomerId { get; set; }
        public string description { get; set; }
    }

    public class PaymentProfile
    {
        public string entityName { get; set; }
        public string id { get; set; }
        public string customerType { get; set; }
    }

    public class SubscriptionPayload
    {
        public string entityName { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public decimal amount { get; set; }
        public string status { get; set; }
        public CustomerProfile profile { get; set; }
    }

    public class customerEvent { public PayloadCustomer payload { get; set; } }
    public class fraudEvent { public FraudList payload { get; set; } }
    public class paymentEvent { public string eventType { get; set; } public PayloadPayment payload { get; set; } }
    public class paymentProfileEvent { public PayloadProfile payload { get; set; } }
    public class subscriptionEvent { public SubscriptionPayload payload { get; set; } }

    // ── Event Response Envelope ────────────────────────────────────────────────
    public class eventResponse
    {
        public string notificationId { get; set; }
        public string eventType { get; set; }
        public DateTime eventDate { get; set; }
        public string webhookId { get; set; }
        public string responseBody { get; set; }
    }

    // ── Authorize.NET API Response ─────────────────────────────────────────────
    public class anResponse
    {
        public string status { get; set; }
        public string reason { get; set; }
        public string message { get; set; }
        public string correlationId { get; set; }
        public object details { get; set; }
        public HttpResponseMessage LastResponse { get; set; }
        public bool IsError => status != null && status != "200";
        public Exception Exception { get; set; }
    }

    // ── Webhook Registration Models ────────────────────────────────────────────
    public class self { public string href { get; set; } }
    public class links { public self self { get; set; } }

    public class AuthorizeNetWebHook:anResponse
    {
        public links _links { get; set; }
        public string webhookId { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public IList<string> eventTypes { get; set; }
    }

    public class details
    {
        public string name { get; set; }
        public string message { get; set; }
    }
}
