using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Common;
using static MWT.Plugin.Misc.MwtStorefront.Models.Api.ProductModel;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Orders
{

    public partial record CustomOrderShoppingCartItemModel : BaseCustomOrderEntityModel
    {
        public CustomOrderShoppingCartItemModel()
        {
            Items = new List<ItemModel>();
        }
        public List<ItemModel> Items { get; set; }
        public string Total { get; set; }
    }
    public partial record ItemModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("CustomOrder.OrderItem.Fields.Name")]
        public string Name { get; set; }
        public string Sku { get; set; }
        public int ProductId { get; set; }
        public string AttributesDescription { get; set; }

        public string AttributesXml { get; set; }

        [NopResourceDisplayName("CustomOrder.OrderItem.Fields.Quantity")]
        public int Quantity { get; set; }

        [NopResourceDisplayName("CustomOrder.OrderItem.Fields.CreatedOnUtc")]
        public DateTime CreatedOnUtc { get; set; }

        [NopResourceDisplayName("CustomOrder.OrderItem.Fields.UpdatedOnUtc")]
        public DateTime UpdatedOnUtc { get; set; }

        [NopResourceDisplayName("CustomOrder.OrderItem.Fields.Notes")]
        public string Notes { get; set; }

        public string CustomAttributesDescription { get; set; }

        [NopResourceDisplayName("CustomOrder.OrderItem.Fields.Price")]
        public string Price { get; set; }

        [NopResourceDisplayName("CustomOrder.OrderItem.Fields.Total")]
        public string Total { get; set; }


        [NopResourceDisplayName("CustomOrder.OrderItem.Fields.Total")]
        public string ItemTotal { get; set; }

        [NopResourceDisplayName("CustomOrder.OrderItem.Fields.Discount")]
        public string TotalAdjustment { get; set; }

        public string DiscountType { get; set; }

        public decimal DiscountAmount { get; set; }

        public decimal DiscountPercentage { get; set; }

        public string ChargeType { get; set; }

        public string Picture { get; set; }

        public List<CustomProductSpecificationModel> Specifications { get; set; }

        public int VariantId { get; set; }

        public List<string> DimensionImages { get; set; }

        public bool IsDeliveryGuaranteed { get; set; }

        public string Attributes { get; set; }

        public string EstimatedDeliveryDate { get; set; }

    }
}
