using Nop.Core.Configuration;
using Nop.Plugin.Payments.AuthorizeNetHosted.Domain;

namespace Nop.Plugin.Payments.AuthorizeNetHosted.Models
{
    public class AuthorizeNetHostedPaymentSettings : ISettings
    {
        public string LoginId { get; set; }
        public string PublicClientKey { get; set; }
        public string TransactionKey { get; set; }
        public bool UseSandbox { get; set; }
        public bool EnableL2orL3 { get; set; }
        public TransactMode TransactMode { get; set; }
        public bool EnableVisaClicktoPay { get; set; }
        public string VisaClicktoPayAPIKey { get; set; }
        public bool EnableBankAccount { get; set; }
        public string WebhookId { get; set; }
        public string SignatureKey { get; set; }
        public bool AdditionalFeePercentage { get; set; }
        public decimal AdditionalFee { get; set; }
        public bool showDebugInfo { get; set; }
        public string SerialNumber { get; set; }
        public bool ShowDebugInfo { get; set; }
        public bool EnableAcceptDirectCheckout { get; set; }
        public bool EnablePendingOrderOnMissingTransactionId { get; set; }
    }
}
