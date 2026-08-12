using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customization.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product
    /// </summary>
    public partial class Product : BaseEntity, ILocalizedEntity, ISlugSupported, IAclSupported, IStoreMappingSupported, IDiscountSupported<DiscountProductMapping>, ISoftDeletedEntity
    {
        public int NoOfSales { get; set; }

        public int BestSellerRank { get; set; }
        public decimal Msrp { get; set; }
        public bool EnableCustomizationModule { get; set; }
        public int? AlternateImageSequence { get; set; }
        public int? VisiblityOncategoryPage { get; set; }
        public int TotalInventory { get; set; }
        public decimal? ShippingPrice { get; set; }
        public bool IsServiceTypeProduct { get; set; }
        public string Notes { get; set; }
        public bool IsVariantProduct { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public decimal MinOldprice { get; set; }
        public decimal MaxOldPrice { get; set; }
        public decimal MinMsrp { get; set; }
        public decimal MaxMsrp { get; set; }
        public bool FeaturedListingDisplaySimilarOnTop { get; set; }
        public bool CollectionListingDisplayVertically { get; set; }
        public string ARImageUrl { get; set; }
        public string ARHeading { get; set; }
        public int CustomizationFormTemplateId { get; set; }
        public DateTime? ImproviseDate { get; set; }
        public DateTime? ContentImproviseDate { get; set; }

        public string FeatureDescription { get; set; }
        public string suggestedKeywordsForDesc { get; set; }
        public string MiscText { get; set; }
        public string SuggestedTitle { get; set; }

        public int ATCRecommendTypeId { get; set; }
        public ATCRecommendType ATCRecommendType
        {
            get => (ATCRecommendType)ATCRecommendTypeId;
            set => ATCRecommendTypeId = (int)value;
        }

        public string ATCRecommendHeading { get; set; }
        public string ATCRecommendedProductIds { get; set; }

        public bool EnableCategoryWiseSimilarProducts { get; set; }
        public int MainCollectionProductId { get; set; }

        public string EstimatedDeliveryDate { get; set; }

        public bool DisplayDimensionOfVariant { get; set; }
        public int RelatedProductId { get; set; } 
        public bool EnableConditionalAttributes { get; set; }
        public bool IsBundleProduct { get; set; }
        public bool EnableNewATCLayout { get; set; }
        public bool DisplayBundleConfiguration { get; set; }

        public int VideoThumnailDisplayOrder { get; set; } 
    }
}
