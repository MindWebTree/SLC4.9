    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    namespace Nop.Plugin.Payments.AuthorizeNetHosted.Helpers
    {
        public class NoFoundOrderException : Exception
        {
            public NoFoundOrderException(string message) : base(message) { }
        }
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public class Batch
        {
            public string batchId { get; set; }
            public DateTime settlementTimeUTC { get; set; }
            public DateTime settlementTimeLocal { get; set; }
            public string settlementState { get; set; }
        }

        public class BillTo
        {
            public string firstName { get; set; }
            public string lastName { get; set; }
            public string address { get; set; }
            public string city { get; set; }
            public string state { get; set; }
            public string zip { get; set; }
            public string country { get; set; }
            public string phoneNumber { get; set; }
        }

        public class CreditCard
        {
            public string cardNumber { get; set; }
            public string expirationDate { get; set; }
            public string cardType { get; set; }
        }

        public class Customer
        {
            public string type { get; set; }
            public string id { get; set; }
            public string email { get; set; }
        }

        public class Duty
        {
            public string amount { get; set; }
        }

        public class FDSFilter
        {
            public string name { get; set; }
            public string action { get; set; }
        }

        public class FDSFilters
        {
            public List<FDSFilter> FDSFilter { get; set; }
        }

        public class GetTransactionDetailsResponse
        {
            [JsonProperty("-xmlns")]
            public string xmlns { get; set; }

            [JsonProperty("-xmlns:xsd")]
            public string xmlnsxsd { get; set; }

            [JsonProperty("-xmlns:xsi")]
            public string xmlnsxsi { get; set; }
            public Messages messages { get; set; }
            public AuthorizeNetTransactionDetails transaction { get; set; }
        }

        public class LineItem
        {
            public string itemId { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public string quantity { get; set; }
            public string unitPrice { get; set; }
            public string taxable { get; set; }
        }

        public class LineItems
        {
            public List<LineItem> lineItem { get; set; }
        }

        public class Message
        {
            public string code { get; set; }
            public string text { get; set; }
        }

        public class Messages
        {
            public string resultCode { get; set; }
            public Message message { get; set; }
        }

        public class Order
        {
            public string invoiceNumber { get; set; }
            public string description { get; set; }
            public string purchaseOrderNumber { get; set; }
        }

        public class Payment
        {
            public CreditCard creditCard { get; set; }
        }

        public class Profile
        {
            public string customerProfileId { get; set; }
            public string customerPaymentProfileId { get; set; }
        }

        public class ReturnedItem
        {
            public string id { get; set; }
            public DateTime dateUTC { get; set; }
            public DateTime dateLocal { get; set; }
            public string code { get; set; }
            public string description { get; set; }
        }

        public class ReturnedItems
        {
            public ReturnedItem returnedItem { get; set; }
        }

        public class Root
        {
            public GetTransactionDetailsResponse getTransactionDetailsResponse { get; set; }
        }

        public class Shipping
        {
            public string amount { get; set; }
            public string name { get; set; }
            public string description { get; set; }
        }

        public class ShipTo
        {
            public string firstName { get; set; }
            public string lastName { get; set; }
            public string address { get; set; }
            public string city { get; set; }
            public string state { get; set; }
            public string zip { get; set; }
            public string country { get; set; }
        }

        public class Solution
        {
            public string id { get; set; }
            public string name { get; set; }
            public string vendorName { get; set; }
        }

        public class Subscription
        {
            public string id { get; set; }
            public string payNum { get; set; }
            public string marketType { get; set; }
            public string product { get; set; }
            public ReturnedItems returnedItems { get; set; }
            public Solution solution { get; set; }
            public string mobileDeviceId { get; set; }
        }

        public class Tax
        {
            public string amount { get; set; }
            public string name { get; set; }
            public string description { get; set; }
        }

        public class AuthorizeNetTransactionDetails
        {
            public bool IsOk { get; set; }
            public string ErrorMessage { get; set; }
            public string transId { get; set; }
            public string refTransId { get; set; }
            public string splitTenderId { get; set; }
            public DateTime submitTimeUTC { get; set; }
            public DateTime submitTimeLocal { get; set; }
            public string transactionType { get; set; }
            public string transactionStatus { get; set; }
            public string responseCode { get; set; }
            public string responseReasonCode { get; set; }
            public string responseReasonDescription { get; set; }
            public string authCode { get; set; }
            public string AVSResponse { get; set; }
            public string cardCodeResponse { get; set; }
            public string CAVVResponse { get; set; }
            public string FDSFilterAction { get; set; }
            public FDSFilters FDSFilters { get; set; }
            public Batch batch { get; set; }
            public Order order { get; set; }
            public string requestedAmount { get; set; }
            public string authAmount { get; set; }
            public string settleAmount { get; set; }
            public Tax tax { get; set; }
            public Shipping shipping { get; set; }
            public Duty duty { get; set; }
            public LineItems lineItems { get; set; }
            public string prepaidBalanceRemaining { get; set; }
            public string taxExempt { get; set; }
            public Payment payment { get; set; }
            public Customer customer { get; set; }
            public BillTo billTo { get; set; }
            public ShipTo shipTo { get; set; }
            public string recurringBilling { get; set; }
            public string customerIP { get; set; }
            public Subscription subscription { get; set; }
            public Profile profile { get; set; }
            public string networkTransId { get; set; }
            public string originalNetworkTransId { get; set; }
            public string originalAuthAmount { get; set; }
            public string authorizationIndicator { get; set; }
        }


    }
