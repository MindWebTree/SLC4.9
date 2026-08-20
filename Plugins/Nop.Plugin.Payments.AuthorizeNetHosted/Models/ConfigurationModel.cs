using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

namespace Nop.Plugin.Payments.AuthorizeNetHosted.Models
{
    public record ConfigurationModel : BaseNopModel, ISettingsModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Payments.AuthorizeNetHostedPayment.Fields.LoginId")]
        public string LoginId { get; set; }
        public bool LoginId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.AuthorizeNetHostedPayment.Fields.PublicClientKey")]
        public string PublicClientKey { get; set; }
        public bool PublicClientKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.AuthorizeNetHostedPayment.Fields.TransactionKey")]
        public string TransactionKey { get; set; }
        public bool TransactionKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.AuthorizeNetHostedPayment.Fields.UseSandbox")]
        public bool UseSandbox { get; set; }
        public bool UseSandbox_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.AuthorizeNetHostedPayment.Fields.EnableL2orL3")]
        public bool EnableL2orL3 { get; set; }
        public bool EnableL2orL3_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.AuthorizeNetHostedPayment.Fields.TransactMode")]
        public int TransactModeId { get; set; }
        public bool TransactModeId_OverrideForStore { get; set; }
        public IList<SelectListItem> TransactModeValues { get; set; }

        [NopResourceDisplayName("Plugins.Payments.AuthorizeNetHostedPayment.Fields.EnableVisaClicktoPay")]
        public bool EnableVisaClicktoPay { get; set; }
        public bool EnableVisaClicktoPay_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.AuthorizeNetHostedPayment.Fields.VisaClicktoPayAPIKey")]
        public string VisaClicktoPayAPIKey { get; set; }
        public bool VisaClicktoPayAPIKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.AuthorizeNetHostedPayment.Fields.EnableBankAccount")]
        public bool EnableBankAccount { get; set; }
        public bool EnableBankAccount_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.AuthorizeNetHostedPayment.Fields.EnableCaptcha")]
        public bool EnableCaptcha { get; set; }
        public bool EnableCaptcha_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.AuthorizeNetHostedPayment.Fields.WebhookId")]
        public string WebhookId { get; set; }

        [NopResourceDisplayName("Plugins.Payments.AuthorizeNetHostedPayment.Fields.SignatureKey")]
        public string SignatureKey { get; set; }
        public bool SignatureKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.AuthorizeNetHostedPayment.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.AuthorizeNetHostedPayment.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.AuthorizeNetHostedPayment.Fields.showDebugInfo")]
        public bool showDebugInfo { get; set; }

        [NopResourceDisplayName("Plugins.Payments.AuthorizeNetHostedPayment.Fields.SerialNumber")]
        public string SerialNumber { get; set; }

        [NopResourceDisplayName("Plugins.Payments.AuthorizeNetHostedPayment.Fields.EnableAcceptDirectCheckout")]
        public bool EnableAcceptDirectCheckout { get; set; }
        public bool EnableAcceptDirectCheckout_OverrideForStore { get; set; }
        public string StoreUrl { get; set; }
        public string LicenseStatus { get; set; }
        public string TrialPeriodEnd { get; set; }
        public IList<string> Warnings { get; set; }
    }
}
