using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customization.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Stores;

namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a category
    /// </summary>
    public partial class Category : BaseEntity, ILocalizedEntity, ISlugSupported, IAclSupported, IStoreMappingSupported, IDiscountSupported<DiscountCategoryMapping>, ISoftDeletedEntity
    {
        public decimal MembershipDiscount { get; set; }
        public string QuickFilterHeading { get; set; }
        public bool EnableInfiniteScroll { get; set; }
        public bool FeaturedListingDisplaySimilarOnTop { get; set; }
        public int AlternateDisplayOrder { get; set; }
        public bool DisplayGridListOption { get; set; }
        public int CustomizationFormTemplateId { get; set; }

        public int ATCRecommendTypeId { get; set; }
        public ATCRecommendType ATCRecommendType
        {
            get => (ATCRecommendType)ATCRecommendTypeId;
            set => ATCRecommendTypeId = (int)value;
        }

        public string ATCRecommendHeading { get; set; }
        public int PrimaryCategoryId { get; set; }
        public int SecondaryCategoryId { get; set; }
        public bool EnableHoverImage { get; set; }
        public string AdditionalDescription { get; set; }
        public bool UseNewVersionOfTemplate { get; set; }
        public decimal ConversionValue { get; set; }
        public bool UseNewVersionCustomizationAction { get; set; }
        public bool EnableNewATCLayout { get; set; }
        public bool IsVisibleInCatalog { get; set; }

    }
}
