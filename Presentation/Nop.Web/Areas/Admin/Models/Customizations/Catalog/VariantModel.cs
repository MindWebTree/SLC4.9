using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;
using System;

namespace Nop.Web.Areas.Admin.Models.Customization.Catalog
{
    public partial record VariantModel : BaseNopEntityModel
    {
        public VariantModel()
        {
            AttributeList = new List<Dictionary<string, string>>();
        }
        public List<Dictionary<string, string>> AttributeList { get; set; }
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
        public IList<ProductPictureModel> ProductPictureModels { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variant.Fields.Product.SeName")]
        public string SeName { get; set; }
    }
}
