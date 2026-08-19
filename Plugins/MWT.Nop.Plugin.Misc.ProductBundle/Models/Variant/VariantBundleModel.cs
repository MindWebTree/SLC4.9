using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Misc.ProductBundle.Models.Variant
{
    public partial record VariantBundleModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.Catalog.Products.Variant.Fields.Product.Attributes")]
        public string Attributes { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variant.Fields.Product.VariantId")]
        public int VariantId { get; set; }
        [NopResourceDisplayName("Admin.Catalog.Products.Variant.Fields.Product.Title")]
        public string Title { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variant.Fields.Product.OldPrice")]
        public decimal? OldPrice { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variant.Fields.Product.Price")]
        public decimal Price { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variant.Fields.Product.Msrp")]
        public decimal? Msrp { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variant.Fields.Product.Published")]
        public bool Published { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variant.Fields.Product.CreatedOn")]
        public DateTime? CreatedOn { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variant.Fields.Product.UpdatedOn")]
        public DateTime? UpdatedOn { get; set; }


        [NopResourceDisplayName("Admin.Catalog.Products.Variant.Fields.Product.WgsRequired")]
        public bool WgsRequired { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variant.Fields.Product.EnableSurcharge")]
        public bool EnableSurcharge { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variant.Fields.Product.EstimatedDeliveryDate")]
        public string EstimatedDeliveryDate { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variant.Fields.Product.QueryParameter")]
        public string QueryParameter { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variant.Fields.Product.Weight")]
        public decimal Weight { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variant.Fields.Product.ManufacturerPartNumber")]
        public string ManufacturerPartNumber { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variant.Fields.Product.DimensionPictureId")]
        public int DimensionPictureId { get; set; }

        public int ProductId { get; set; }
        //public IList<ProductPictureModel> ProductPictureModels { get; set; } 
        public bool HasBundleForProduct { get; set; }
        public bool IsValidBundle { get; set; }
    }
}
