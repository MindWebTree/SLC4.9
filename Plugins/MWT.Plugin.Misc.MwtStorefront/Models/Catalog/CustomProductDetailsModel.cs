using Microsoft.AspNetCore.Mvc.Rendering;
using MWT.Plugin.Misc.MwtStorefront.Models.Catalog;
using MWT.Plugin.Misc.MwtStorefront.Models.Media;
using MWTNop.Core.Domain.Catalog;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customization.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;
using Nop.Web.Models.ShoppingCart;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static Nop.Web.Models.Catalog.ProductDetailsModel;


namespace MWT.Plugin.Misc.MwtStorefront.Models.Catalog
{
    public record CustomProductDetailsModel : BaseNopEntityModel
    {
        public CustomProductDetailsModel()
        {
            DefaultPictureModel = new CustomPictureModel();
            PictureModels = new List<CustomPictureModel>();
            GiftCard = new global::Nop.Web.Models.Catalog.ProductDetailsModel.GiftCardModel();
            ProductPrice = new ProductPriceModel();
            AddToCart = new global::Nop.Web.Models.Catalog.ProductDetailsModel.AddToCartModel();
            ProductAttributes = new List<ProductAttributeModel>();
            AssociatedProducts = new List<CustomProductDetailsModel>();
            VendorModel = new global::Nop.Web.Models.Catalog.VendorBriefInfoModel();
            Breadcrumb = new ProductBreadcrumbModel();
            ProductTags = new List<global::Nop.Web.Models.Catalog.ProductTagModel>();
            ProductSpecificationModel = new global::Nop.Web.Models.Catalog.ProductSpecificationModel();
            ProductManufacturers = new List<global::Nop.Web.Models.Catalog.ManufacturerBriefInfoModel>();
            ProductReviewOverview = new global::Nop.Web.Models.Catalog.ProductReviewOverviewModel();
            TierPrices = new List<global::Nop.Web.Models.Catalog.ProductDetailsModel.TierPriceModel>();
            ProductEstimateShipping = new global::Nop.Web.Models.Catalog.ProductDetailsModel.ProductEstimateShippingModel();
            AssociatedProductsConfiguration = new List<GroupedProductConfigurationModel>();
            Variants = new List<VariantCombination>();
            AllCombinations = new List<List<int>>();
        }

        //picture(s)
        public bool DefaultPictureZoomEnabled { get; set; }
        public CustomPictureModel DefaultPictureModel { get; set; }
        public IList<CustomPictureModel> PictureModels { get; set; }

        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public string MetaTitle { get; set; }
        public string SeName { get; set; }
        public bool VisibleIndividually { get; set; }

        public ProductType ProductType { get; set; }

        public bool ShowSku { get; set; }
        public string Sku { get; set; }

        public bool ShowManufacturerPartNumber { get; set; }
        public string ManufacturerPartNumber { get; set; }

        public bool ShowGtin { get; set; }
        public string Gtin { get; set; }

        public bool ShowVendor { get; set; }
        public VendorBriefInfoModel VendorModel { get; set; }

        public bool HasSampleDownload { get; set; }

        public GiftCardModel GiftCard { get; set; }

        public bool IsShipEnabled { get; set; }
        public bool IsFreeShipping { get; set; }
        public bool FreeShippingNotificationEnabled { get; set; }
        public string DeliveryDate { get; set; }

        public bool IsRental { get; set; }
        public DateTime? RentalStartDate { get; set; }
        public DateTime? RentalEndDate { get; set; }

        public DateTime? AvailableEndDate { get; set; }

        public ManageInventoryMethod ManageInventoryMethod { get; set; }

        public string StockAvailability { get; set; }

        public bool DisplayBackInStockSubscription { get; set; }

        public bool EmailAFriendEnabled { get; set; }
        public bool CompareProductsEnabled { get; set; }

        public string PageShareCode { get; set; }

        public ProductPriceModel ProductPrice { get; set; }

        public AddToCartModel AddToCart { get; set; }

        public ProductBreadcrumbModel Breadcrumb { get; set; }

        public IList<ProductTagModel> ProductTags { get; set; }

        public IList<ProductAttributeModel> ProductAttributes { get; set; }

        public ProductSpecificationModel ProductSpecificationModel { get; set; }

        public IList<ManufacturerBriefInfoModel> ProductManufacturers { get; set; }

        public ProductReviewOverviewModel ProductReviewOverview { get; set; }

        public ProductEstimateShippingModel ProductEstimateShipping { get; set; }

        public IList<TierPriceModel> TierPrices { get; set; }

        //a list of associated products. For example, "Grouped" products could have several child "simple" products
        public IList<CustomProductDetailsModel> AssociatedProducts { get; set; }

        public bool DisplayDiscontinuedMessage { get; set; }

        public string CurrentStoreName { get; set; }

        public bool InStock { get; set; }

        public bool AllowAddingOnlyExistingAttributeCombinations { get; set; }

        //variant / customization / merchandising extensions
        public int VariantId { get; set; }
        public string VariantTitle { get; set; }
        public int ParentGroupId { get; set; }
        public bool EnableCustomizationModule { get; set; }
        public int DefaultVariantId { get; set; }
        public int TotalInventory { get; set; }
        public bool CollectionListingDisplayVertically { get; set; }
        public string ARImageUrl { get; set; }
        public string EstimatedDeliveryDate { get; set; }
        public string ARHeading { get; set; }
        public int CustomizationFormTemplateId { get; set; }

        public bool IsMainProduct { get; set; }
        public string Notes { get; set; }
        public bool IsThanksGivingProduct { get; set; }
        public List<GroupedProductConfigurationModel> AssociatedProductsConfiguration { get; set; }
        public bool DisplayDimensionOfVariant { get; set; }
        public int RelatedProductId { get; set; }
        public string VideoUrl { get; set; }
        public int VideoThumnailDisplayOrder { get; set; }
        public bool UseNewVersionOfTemplate { get; set; }
        public int MainCategoryId { get; set; }
        public decimal ConversionValue { get; set; }
        public bool UseNewVersionCustomizationAction { get; set; }
        public List<VariantCombination> Variants { get; set; } = new();
        public List<List<int>> AllCombinations { get; set; } = new List<List<int>>();
        public bool EnableConditionalAttributes { get; set; }

        #region Nested Classes





        public record ProductPriceModel : BaseNopModel
        {
            /// <summary>
            /// The currency (in 3-letter ISO 4217 format) of the offer price 
            /// </summary>
            public string CurrencyCode { get; set; }

            public string OldPrice { get; set; }

            public string Price { get; set; }
            public string PriceWithDiscount { get; set; }
            public decimal PriceValue { get; set; }

            public bool CustomerEntersPrice { get; set; }

            public bool CallForPrice { get; set; }

            public int ProductId { get; set; }

            public bool HidePrices { get; set; }

            //rental
            public bool IsRental { get; set; }
            public string RentalPrice { get; set; }

            /// <summary>
            /// A value indicating whether we should display tax/shipping info (used in Germany)
            /// </summary>
            public bool DisplayTaxShippingInfo { get; set; }
            /// <summary>
            /// PAngV baseprice (used in Germany)
            /// </summary>
            public string BasePricePAngV { get; set; }

            //merchandising / variant pricing extensions
            public string Msrp { get; set; }
            public decimal MsrpValue { get; set; }
            public decimal OldPriceValue { get; set; }
            public string MembershipPrice { get; set; }
            public decimal MembershipPriceValue { get; set; }
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



       
        public record ProductAttributeModel : BaseNopEntityModel
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

            //UI/display extensions
            public bool IsExpanded { get; set; }
            public bool EnableHoverImpact { get; set; }
        }

        public record ProductAttributeValueModel : BaseNopEntityModel
        {
            public ProductAttributeValueModel()
            {
                ImageSquaresPictureModel = new PictureModel();
            }

            public string Name { get; set; }

            public string ColorSquaresRgb { get; set; }

            //picture model is used with "image square" attribute type
            public PictureModel ImageSquaresPictureModel { get; set; }

            public string PriceAdjustment { get; set; }

            public bool PriceAdjustmentUsePercentage { get; set; }

            public decimal PriceAdjustmentValue { get; set; }

            public bool IsPreSelected { get; set; }

            //product picture ID (associated to this value)
            public int PictureId { get; set; }

            public bool CustomerEntersQty { get; set; }

            public int Quantity { get; set; }

            //variant / merchandising extensions
            public int VariantId { get; set; }
            public string Dimension { get; set; }
            public string VariantDimension { get; set; }
            public string PictureDefaultSizeUrl { get; set; }
            public string PictureFullSizeUrl { get; set; }
            public bool IsLargeItem { get; set; }
            public bool Hide { get; set; }
        }

 

        #endregion
    }
}