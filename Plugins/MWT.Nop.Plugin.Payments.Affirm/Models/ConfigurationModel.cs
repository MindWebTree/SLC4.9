using Microsoft.AspNetCore.Mvc.Rendering;
using MWT.Nop.Plugin.Payments.Affirm.Domain;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Payments.Affirm.Models
{
    public record ConfigurationModel : BaseNopModel, ISettingsModel
    {
        public ConfigurationModel()
        {
            this.AvailableWidgetZoneShoppingCarts = new List<SelectListItem>();
            this.AvailableWidgetProductDetailsPages = new List<SelectListItem>();
            this.AvailableWidgetProductBoxs = new List<SelectListItem>();
        }
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Admin.MWT.Plugin.Payments.Affirm.AdditionalFee")]
        public decimal AdditionalFee { get; set; }

        public bool AdditionalFee_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.MWT.Plugin.Payments.Affirm.AdditionalFeePercentage")]
        public decimal AdditionalFeePercentage { get; set; }

        public bool AdditionalFeePercentage_OverrideForStore { get; set; }
        public IList<SelectListItem> AvailableWidgetProductBoxs { get; set; }

        public IList<SelectListItem> AvailableWidgetProductDetailsPages { get; set; }

        public IList<SelectListItem> AvailableWidgetZoneShoppingCarts { get; set; }

        [NopResourceDisplayName("Admin.MWT.Plugin.Payments.Affirm.CountryAPIMode")]
        public int CountryAPIModeId { get; set; }

        public bool CountryAPIModeId_OverrideForStore { get; set; }

        public SelectList CountryAPIModeValues { get; set; }

        [NopResourceDisplayName("Admin.MWT.Plugin.Payments.Affirm.CreateOrderMode")]
        public int CreateOrderModeId { get; set; }

        public bool CreateOrderModeId_OverrideForStore { get; set; }

        public SelectList CreateOrderModeValues { get; set; }

        [NopResourceDisplayName("Admin.MWT.Plugin.Payments.Affirm.EnableOnProductBox")]
        public bool EnableOnProductBox { get; set; }

        public bool EnableOnProductBox_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.MWT.Plugin.Payments.Affirm.EnableOnProductDetailsPage")]
        public bool EnableOnProductDetailsPage { get; set; }

        public bool EnableOnProductDetailsPage_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.MWT.Plugin.Payments.Affirm.EnableOnShoppingCart")]
        public bool EnableOnShoppingCart { get; set; }

        public bool EnableOnShoppingCart_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.MWT.Plugin.Payments.Affirm.FacingMerchantName")]
        public string FacingMerchantName { get; set; }

        public bool FacingMerchantName_OverrideForStore { get; set; }

        public bool IsRegisted { get; set; }

        [NopResourceDisplayName("Admin.MWT.Plugin.Payments.Affirm.MinimumSubTotalAmount")]
        public decimal? MinimumSubTotalAmount { get; set; }

        public bool MinimumSubTotalAmount_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.MWT.Plugin.Payments.Affirm.PrivateApiKey")]
        public string PrivateApiKey { get; set; }

        public bool PrivateApiKey_OverrideForStore { get; set; }


        [NopResourceDisplayName("Admin.MWT.Plugin.Payments.Affirm.PromotionalMessageColor")]
        public int PromotionalMessageColorId { get; set; }

        public bool PromotionalMessageColorId_OverrideForStore { get; set; }
        public SelectList PromotionalMessageColorValues { get; set; }
        [NopResourceDisplayName("Admin.MWT.Plugin.Payments.Affirm.PromotionalMessageType")]
        public int PromotionalMessageTypeId { get; set; }

        public bool PromotionalMessageTypeId_OverrideForStore { get; set; }
        public SelectList PromotionalMessageTypeValues { get; set; }

        [NopResourceDisplayName("Admin.MWT.Plugin.Payments.Affirm.PublicApiKey")]
        public string PublicApiKey { get; set; }
        public bool PublicApiKey_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.MWT.Plugin.Payments.Affirm.SerialNumber")]
        public string SerialNumber { get; set; }
        [NopResourceDisplayName("Admin.MWT.Plugin.Payments.Affirm.showDebugInfo")]
        public bool showDebugInfo { get; set; }
        [NopResourceDisplayName("Admin.MWT.Plugin.Payments.Affirm.SkipPaymentInfo")]
        public bool SkipPaymentInfo { get; set; }
        public bool SkipPaymentInfo_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.MWT.Plugin.Payments.Affirm.StoreUrl")]
        public string StoreUrl { get; set; }

        [NopResourceDisplayName("Admin.MWT.Plugin.Payments.Affirm.TransactMode")]
        public int TransactModeId { get; set; }

        public bool TransactModeId_OverrideForStore { get; set; }
        public SelectList TransactModeValues { get; set; }
        [NopResourceDisplayName("Admin.MWT.Plugin.Payments.Affirm.UseSandbox")]
        public bool UseSandbox { get; set; }

        public bool UseSandbox_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.MWT.Plugin.Payments.Affirm.WidgetProductBox")]
        public string WidgetProductBox { get; set; }

        public bool WidgetProductBox_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.MWT.Plugin.Payments.Affirm.WidgetProductDetailsPage")]
        public string WidgetProductDetailsPage { get; set; }

        public bool WidgetProductDetailsPage_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.MWT.Plugin.Payments.Affirm.WidgetZoneShoppingCart")]
        public string WidgetZoneShoppingCart { get; set; }
        public bool WidgetZoneShoppingCart_OverrideForStore { get; set; }
    }
}
