using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Orders
{
    public partial record CustomOrderPriceAdjustmentModel: BaseNopEntityModel
    {
        [NopResourceDisplayName("CustomOrder.Item.PriceAdjustment.Fields.ShoppingCartRecID")]
        public int ShoppingCartRecID { get; set; }

        [NopResourceDisplayName("CustomOrder.Item.PriceAdjustment.Fields.Price")]
        public string ShoppingCartProductPrice { get; set; }

        [NopResourceDisplayName("CustomOrder.Item.PriceAdjustment.Fields.Discount")]
        public string Discountamount { get; set; }

        [NopResourceDisplayName("CustomOrder.Item.PriceAdjustment.Fields.ChargeType")]
        public string Chargestype { get; set; }

        [NopResourceDisplayName("CustomOrder.Item.PriceAdjustment.Fields.AdjustmentNotes")]
        public string AdjustmentNotes { get; set; }

        [NopResourceDisplayName("CustomOrder.Item.PriceAdjustment.Fields.DiscountType")]
        public string Discounttype { get; set; }

        [NopResourceDisplayName("CustomOrder.Item.PriceAdjustment.Fields.DiscounPercentage")]
        public string DiscountPercentage { get; set; }

        [NopResourceDisplayName("CustomOrder.Item.PriceAdjustment.Fields.OrderId")]
        public int? OrderId { get; set; }

    }
}
