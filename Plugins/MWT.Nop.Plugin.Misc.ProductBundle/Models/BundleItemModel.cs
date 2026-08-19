using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace MWT.Nop.Plugin.Misc.ProductBundle.Models
{
    public record BundleItemModel : BaseNopEntityModel
    {
        public int BundleId { get; set; }

        [NopResourceDisplayName("MWT.Plugin.ProductBundle.Product")]
        public int ProductId { get; set; }

        [NopResourceDisplayName("MWT.Plugin.ProductBundle.Quantity")]
        public int Quantity { get; set; }
        [NopResourceDisplayName("MWT.Plugin.ProductBundle.AttributeValueId")]
        public int VariantId { get; set; }

        [NopResourceDisplayName("MWT.Plugin.ProductBundle.ProductName")]
        public string ProductName { get; set; }
        [NopResourceDisplayName("MWT.Plugin.ProductBundle.Picture")]

        public string Picture { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variant.Fields.Product.Attributes")]
        public string Attributes { get; set; }
        public decimal Price { get; set; }
        public decimal BuyMoreSaveMoreDiscountBase { get; set; }
        public decimal OldPrice { get; set; }
        public decimal MSRP { get; set; }
        public decimal DiscountPercentage { get; set; }
    }
}