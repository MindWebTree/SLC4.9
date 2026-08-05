using MWT.Plugin.Misc.MwtStorefront.Models.Media;
using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;
using static Nop.Web.Models.Catalog.ProductDetailsModel;

namespace MWT.Plugin.Misc.MwtStorefront.Models.Catalog
{
    public record CustomProductOverviewModel : BaseNopEntityModel
    {
        public CustomProductOverviewModel()
        {
            ProductPrice = new ProductPriceModel();
            DefaultPictureModel = new CustomPictureModel();
            ProductSpecificationModel = new ProductSpecificationModel();
            ReviewOverviewModel = new ProductReviewOverviewModel();
            PictureModels = new List<CustomPictureModel>();
            AddToCart = new AddToCartModel();
            ProductAttributes = new List<CustomProductDetailsModel.ProductAttributeModel>();
            AllCombinations = new List<List<int>>();
            Tags = new List<ProductTag>();
            Attribute = new List<ProductAttributeModelWithImage>();
            suggestedwords = new List<EsProductSuggestedKeyWordModel>();
            filters = new List<EsProductSpecificationAttributesModel>();
            entities = new List<EsEntitytModel>();
        }

        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public string SeName { get; set; }
        public string Sku { get; set; }
        public ProductType ProductType { get; set; }
        public bool MarkAsNew { get; set; }

        //price
        public ProductPriceModel ProductPrice { get; set; }

        //picture
        public CustomPictureModel DefaultPictureModel { get; set; }

        //specification attributes
        public ProductSpecificationModel ProductSpecificationModel { get; set; }

        //price
        public ProductReviewOverviewModel ReviewOverviewModel { get; set; }

        //variant / customization / merchandising extensions
        public string VariantTitle { get; set; }
        public int DefaultVariantId { get; set; }
        public int VariantId { get; set; }
        public string VariantSename { get; set; }
        public string ProductRelation { get; set; }
        public IList<CustomPictureModel> PictureModels { get; set; }
        public AddToCartModel AddToCart { get; set; }
        public IList<CustomProductDetailsModel.ProductAttributeModel> ProductAttributes { get; set; }
        public List<List<int>> AllCombinations { get; set; }
        public bool AllowAddingOnlyExistingAttributeCombinations { get; set; }
        public List<ProductTag> Tags { get; set; }
        public PictureModel AlternatePictureModel { get; set; }
        public List<ProductAttributeModelWithImage> Attribute { get; set; }
        public string CollectionMessage { get; set; }
        public string SizeShadeMessage { get; set; }
        public bool IsMainCollection { get; set; }
        public List<EsProductSuggestedKeyWordModel> suggestedwords { get; set; }
        public List<EsProductSpecificationAttributesModel> filters { get; set; }
        public List<EsEntitytModel> entities { get; set; }
        public int Inventory { get; set; }
        public bool EnableCustomizationModule { get; set; }
        public bool Published { get; set; }
        public string MetaKeywords { get; set; }
        public int NoOfSales { get; set; }
        public decimal ConversionValue { get; set; }
        public bool IsSimilarProduct { get; set; }

        #region Nested Classes

       
        public record ProductPriceModel : BaseNopModel
        {
            public string OldPrice { get; set; }
            public string Price { get; set; }
            public decimal PriceValue { get; set; }

            /// <summary>
            /// PAngV baseprice (used in Germany)
            /// </summary>
            public string BasePricePAngV { get; set; }
            public bool DisableBuyButton { get; set; }
            public bool DisableWishlistButton { get; set; }
            public bool DisableAddToCompareListButton { get; set; }
            public bool AvailableForPreOrder { get; set; }
            public DateTime? PreOrderAvailabilityStartDateTimeUtc { get; set; }
            public bool IsRental { get; set; }
            public bool ForceRedirectionAfterAddingToCart { get; set; }

            /// <summary>
            /// A value indicating whether we should display tax/shipping info (used in Germany)
            /// </summary>
            public bool DisplayTaxShippingInfo { get; set; }

            //merchandising / variant pricing extensions
            public string MembershipPrice { get; set; }
            public decimal MembershipPriceValue { get; set; }
            public string Msrp { get; set; }
            public decimal MsrpValue { get; set; }
            public decimal OldPriceValue { get; set; }
            public string CurrencyCode { get; set; }
            public string PriceWithDiscount { get; set; }
            public bool CustomerEntersPrice { get; set; }
            public bool CallForPrice { get; set; }
            public int ProductId { get; set; }
            public bool HidePrices { get; set; }
            public string RentalPrice { get; set; }
            public bool IsVariantProduct { get; set; }
            public decimal MinOldPriceValue { get; set; }
            public decimal MaxOldPriceValue { get; set; }
            public decimal MinPriceValue { get; set; }
            public decimal MaxPriceValue { get; set; }
            public decimal MinMsrpValue { get; set; }
            public decimal MaxMsrpValue { get; set; }
            public decimal MinMembershipPriceValue { get; set; }
            public decimal MaxMembershipPriceValue { get; set; }
            public string MinOldPrice { get; set; }
            public string MaxOldPrice { get; set; }
            public string MinPrice { get; set; }
            public string MaxPrice { get; set; }
            public string MinMsrp { get; set; }
            public string MaxMsrp { get; set; }
            public string MinMembershipPrice { get; set; }
            public string MaxMembershipPrice { get; set; }
            public string OfferText { get; set; }
            public string DiscountAmount { get; set; }
            public decimal DiscountPercentage { get; set; }
            public string OfferPlaceHolder { get; set; }
            public DateTime? SaleStartDate { get; set; }
            public DateTime? SaleEndDate { get; set; }
        }

        public record ProductAttributeModelWithImage : BaseNopModel
        {
            public string Name { get; set; }
            public string PictureUrl { get; set; }
        }

        #endregion
    }
}