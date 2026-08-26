using Microsoft.AspNetCore.Http;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Models.Catalog
{
    /// <summary>
    /// Represents a product model
    /// </summary>
    public partial record ProductModel : BaseNopEntityModel,
        IAclSupportedModel, IDiscountSupportedModel, ILocalizedModel<ProductLocalizedModel>, IStoreMappingSupportedModel
    {

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.Msrp")]
        public decimal Msrp { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.EnableCustomizationModule")]
        public bool EnableCustomizationModule { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.VisiblityOncategoryPage")]
        public bool VisiblityOncategoryPage { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.ShippingPrice")]
        public decimal ShippingPrice { get; set; }

        public FBTProductSearchModel FBTProductSearchModel { get; set; }


        [NopResourceDisplayName("Admin.Catalog.Products.Fields.IsServiceTypeProduct")]
        public bool IsServiceTypeProduct { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.Notes")]
        public string Notes { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.TotalInventory")]
        public string TotalInventory { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.AlternateImageSequence")]
        public int? AlternateImageSequence { get; set; }

        public int Quantity { get; set; }
        public IFormFile? DimensionImage { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.FeaturedListingDisplaySimilarOnTop")]
        public bool FeaturedListingDisplaySimilarOnTop { get; set; }


        [NopResourceDisplayName("Admin.Catalog.Products.Fields.CollectionListingDisplayVertically")]
        public bool CollectionListingDisplayVertically { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.ARImageUrl")]
        public string ARImageUrl { get; set; }


        [NopResourceDisplayName("Admin.Catalog.Products.Fields.ARHeading")]
        public string ARHeading { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.CustomizationFormTemplateId")]
        public string CustomizationFormTemplateId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.ImproviseDate")]
        [UIHint("DateTimeNullable")]
        public DateTime? ImproviseDate { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.ContentImproviseDate")]
        [UIHint("DateTimeNullable")]
        public DateTime? ContentImproviseDate { get; set; }
        [NopResourceDisplayName("Admin.Catalog.Products.Fields.FeatureDescription")]
        public string FeatureDescription { get; set; }
        [NopResourceDisplayName("Admin.Catalog.Products.Fields.suggestedKeywordsForDesc")]
        public string suggestedKeywordsForDesc { get; set; }
        [NopResourceDisplayName("Admin.Catalog.Products.Fields.MiscText")]
        public string MiscText { get; set; }
        [NopResourceDisplayName("Admin.Catalog.Products.Fields.SuggestedTitle")]
        public string SuggestedTitle { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.ATCRecommendType")]
        public int ATCRecommendTypeId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.ATCRecommendHeading")]
        public string ATCRecommendHeading { get; set; }
        [NopResourceDisplayName("Admin.Catalog.Products.Fields.ATCRecommendedProductIds")]
        public string ATCRecommendedProductIds { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.MainCollectionProductId")]
        public int MainCollectionProductId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.EnableCategoryWiseSimilarProducts")]
        public bool EnableCategoryWiseSimilarProducts { get; set; }


        [NopResourceDisplayName("Admin.Catalog.Products.Fields.EstimatedDeliveryDate")]
        public string EstimatedDeliveryDate { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.DisplayDimensionOfVariant")]
        public bool DisplayDimensionOfVariant { get; set; }


        [NopResourceDisplayName("Admin.Catalog.Products.Fields.RelatedProductId")]
        public int RelatedProductId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.VideoUrl")]
        public string VideoUrl { get; set; }


        [NopResourceDisplayName("Admin.Catalog.Products.Fields.VideoThumnailDisplayOrder")]
        public int VideoThumnailDisplayOrder { get; set; }


        [NopResourceDisplayName("Admin.Catalog.Products.Fields.FullAttributeDefinitionsJson")]
        public string FullAttributeDefinitionsJson { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.LogicMasterListJson")]
        public string LogicMasterListJson { get; set; }
        [NopResourceDisplayName("Admin.Catalog.Products.Fields.AvailablePicturesJson")]
        public string AvailablePicturesJson { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.EnableConditionalAttributes")]
        public bool EnableConditionalAttributes { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.IsBundleProduct")]
        public bool IsBundleProduct { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.EnableNewATCLayout")]
        public bool EnableNewATCLayout { get; set; }

        
        [NopResourceDisplayName("Admin.Catalog.Products.Fields.DisplayBundleConfiguration")]
        public bool DisplayBundleConfiguration { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.EnableBundleQuickView")]
        public bool EnableBundleQuickView { get; set; }
    }
}
