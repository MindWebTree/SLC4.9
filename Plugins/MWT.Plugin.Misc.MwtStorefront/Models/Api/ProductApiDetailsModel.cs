using MWT.Plugin.Misc.MwtStorefront.Models.Catalog;
using MWT.Plugin.Misc.MwtStorefront.Models.Media;
using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Models;


namespace MWT.Plugin.Misc.MwtStorefront.Models.Api
{
    public partial record ProductApiDetailsModel : BaseNopEntityModel
    {
        public ProductApiDetailsModel()
        {
            DefaultPictureModel = new CustomPictureModel();
            PictureModels = new List<CustomPictureModel>();
            ProductPrice = new CustomProductDetailsModel.ProductPriceModel();
            ProductAttributes = new List<ProductAttributeModel>();
            Specifications = new List<ProductModel.CustomProductSpecificationModel>();
        }

        //picture(s)

        public ManageInventoryMethod ManageInventoryMethod { get; set; }
        public int ParentGroupId { get; set; }
        public bool EnableCustomizationModule { get; set; }
        public string Notes { get; set; }

        public int TotalInventory { get; set; }
        public CustomPictureModel DefaultPictureModel { get; set; }
        public IList<CustomPictureModel> PictureModels { get; set; }

        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public string MetaTitle { get; set; }
        public string SeName { get; set; }

        public ProductType ProductType { get; set; }
        public string Sku { get; set; }
        public string ManufacturerPartNumber { get; set; }
        public int ManufacturerId { get; set; }
        public string Manufacturer { get; set; }
        public string Gtin { get; set; }

        public string StockAvailability { get; set; }
        public CustomProductDetailsModel.ProductPriceModel ProductPrice { get; set; }

        public IList<ProductAttributeModel> ProductAttributes { get; set; }
        public List<ProductModel.CustomProductSpecificationModel> Specifications { get; set; }

        public string VariantTitle { get; set; }

        public string IndividualProducts { get; set; }

        public List<VariantModel> Variants { get; set; }
        #region Nested Classes

        public partial record ProductAttributeModel : BaseNopEntityModel
        {
            public ProductAttributeModel()
            {
                AllowedFileExtensions = new List<string>();
                Values = new List<ProductAttributeValueModel>();
            }

            public int ProductId { get; set; }

            public int ProductAttributeId { get; set; }

            public string Name { get; set; }

            public string Description { get; set; }

            public string TextPrompt { get; set; }

            public bool IsRequired { get; set; }

            /// <summary>
            /// Default value for textboxes
            /// </summary>
            public string DefaultValue { get; set; }
            /// <summary>
            /// Selected day value for datepicker
            /// </summary>
            public int? SelectedDay { get; set; }
            /// <summary>
            /// Selected month value for datepicker
            /// </summary>
            public int? SelectedMonth { get; set; }
            /// <summary>
            /// Selected year value for datepicker
            /// </summary>
            public int? SelectedYear { get; set; }

            /// <summary>
            /// A value indicating whether this attribute depends on some other attribute
            /// </summary>
            public bool HasCondition { get; set; }

            /// <summary>
            /// Allowed file extensions for customer uploaded files
            /// </summary>
            public IList<string> AllowedFileExtensions { get; set; }

            public AttributeControlType AttributeControlType { get; set; }

            public IList<ProductAttributeValueModel> Values { get; set; }
        }

        public partial record ProductAttributeValueModel : BaseNopEntityModel
        {
            public ProductAttributeValueModel()
            {
            }
            public string Name { get; set; }
            public string PriceAdjustment { get; set; }
            public bool PriceAdjustmentUsePercentage { get; set; }
            public decimal PriceAdjustmentValue { get; set; }
            public bool IsPreSelected { get; set; }
            public bool CustomerEntersQty { get; set; }
            public int Quantity { get; set; }
            public int VariantId { get; set; }
            public string Dimension { get; set; }
            public string VariantDimension { get; set; }
            public string PictureFullSizeUrl { get; set; }
            public string[] Gallery { get; set; }
        }


        #endregion
    }
}
