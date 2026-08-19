using Nop.Core;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Orders
{
    public partial record CustomOrderOrderSummaryAdjustmentModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("CustomOrder.Summary.PriceAdjustment.Fields.SubtotalDiscount")]
        public string SubtotalDiscount { get; set; }

        [NopResourceDisplayName("CustomOrder.Summary.PriceAdjustment.Fields.SubtotalChargeType")]
        public string SubtotalChargeType { get; set; }

        [NopResourceDisplayName("CustomOrder.Summary.PriceAdjustment.Fields.SubTotalDiscountType")]
        public string SubTotalDiscountType { get; set; }

        [NopResourceDisplayName("CustomOrder.Summary.PriceAdjustment.Fields.ShippingDiscount")]
        public string ShippingDiscount { get; set; }

        [NopResourceDisplayName("CustomOrder.Summary.PriceAdjustment.Fields.ShippingChargeType")]
        public string ShippingChargeType { get; set; }

        [NopResourceDisplayName("CustomOrder.Summary.PriceAdjustment.Fields.ShippingDiscountType")]
        public string ShippingDiscountType { get; set; }

        [NopResourceDisplayName("CustomOrder.Summary.PriceAdjustment.Fields.TaxDiscount")]
        public decimal? TaxDiscount { get; set; }

        [NopResourceDisplayName("CustomOrder.Summary.PriceAdjustment.Fields.TaxDiscountType")]
        public string TaxDiscountType { get; set; }

        [NopResourceDisplayName("CustomOrder.Summary.PriceAdjustment.Fields.Customer")]
        public int CustomerID { get; set; }

        [NopResourceDisplayName("CustomOrder.Summary.PriceAdjustment.Fields.OrderId")]
        public int OrderId { get; set; }
    }
}
