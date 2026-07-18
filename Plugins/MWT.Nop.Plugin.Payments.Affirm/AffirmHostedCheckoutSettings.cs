using MWT.Nop.Plugin.Payments.Affirm.Domain;
using Nop.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Payments.Affirm
{
    public class AffirmCheckoutSettings : ISettings
    {
        public decimal AdditionalFee { get; set; }
        public decimal AdditionalFeePercentage { get; set; }
        public CountryAPIMode CountryAPIMode { get; set; }
        public CreateOrderMode CreateOrderMode { get; set; }
        public bool EnableOnProductBox { get; set; }
        public bool EnableOnProductDetailsPage { get; set; }
        public bool EnableOnShoppingCart { get; set; }
        public string FacingMerchantName { get; set; }
        public decimal? MinimumSubTotalAmount { get; set; }
        public string PrivateApiKey { get; set; }
        public PromotionalMessageColor PromotionalMessageColor { get; set; }
        public PromotionalMessageType PromotionalMessageType { get; set; }
        public string PublicApiKey { get; set; }
        public string SerialNumber { get; set; }
        public bool showDebugInfo { get; set; }
        public bool SkipPaymentInfo { get; set; }
        public TransactMode TransactMode { get; set; }
        public bool UseSandbox { get; set; }
        public string WidgetProductBox { get; set; }
        public string WidgetProductDetailsPage { get; set; }
        public string WidgetZoneShoppingCart { get; set; }
        public string SupportedCountryIds { get; set; }
    }
}
