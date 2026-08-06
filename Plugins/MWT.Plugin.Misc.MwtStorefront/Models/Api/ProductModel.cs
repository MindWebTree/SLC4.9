using MWT.Plugin.Misc.MwtStorefront.Models.Media;
using Nop.Core.Domain.Catalog;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Models.Api
{
    public partial record ProductModel : BaseNopEntityModel
    {
        public ProductModel()
        {
            PictureModels = new List<CustomPictureModel>();
            ProductPrice = new ProductPriceModel();
            Specifications = new List<CustomProductSpecificationModel>();
            RelatedProducts = new List<string>();
        }

        //picture(s)
        public IList<CustomPictureModel> PictureModels { get; set; }
        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public string FroogleDescription { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public string MetaTitle { get; set; }
        public string SeName { get; set; }
        public string Dimensions { get; set; }
        public string Sku { get; set; }
        public string ManufacturerPartNumber { get; set; }
        public List<CategoryModel> Categories { get; set; }
        public CategoryModel MainCategory { get; set; }
        public string Gtin { get; set; }
        public ProductPriceModel ProductPrice { get; set; }

        public decimal? ShippingCost { get; set; }
        public List<CustomProductSpecificationModel> Specifications { get; set; }
        public List<string> RelatedProducts { get; set; }
        public int TotaLinventory { get; set; }
        public List<Variant> Variants { get; set; }
        public string Manufacturer { get; set; }
        public string IndividualProducts { get; set; }
        public DateTime? SaleStartDate { get; set; }
        public DateTime? SaleEndDate { get; set; }

        #region Nested Classes

        public class CustomProductSpecificationModel
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }
        public partial record ProductPriceModel : BaseNopModel
        {

            public decimal PriceValue { get; set; }
            public decimal MsrpValue { get; set; }
            public decimal OldPriceValue { get; set; }
        }



        public class CategoryModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Sename { get; set; }
            public string Path { get; set; }
        }

        public partial record Variant : BaseNopEntityModel
        {
            public string Name { get; set; }
            public string VariantTitle { get; set; }
            public decimal PriceValue { get; set; }
            public decimal MsrpValue { get; set; }
            public decimal OldPriceValue { get; set; }
            public string ManufacturerPartNumber { get; set; }
            public decimal Weight { get; set; }
            public bool IsDefault { get; set; }
            public string Dimension { get; set; }
            public string QueryParameter { get; set; }
            public Dictionary<string, string> Attributes { get; set; }
        }





        #endregion
    }

    public class CustomProductAttributeCombination
    {
        public int ValueId { get; set; }
        public ProductAttributeCombination Combination { get; set; }

    }
}