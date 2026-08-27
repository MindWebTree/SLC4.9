

using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Catalog
{
    /// <summary>
    /// Represents a category model
    /// </summary>
    public partial record CategoryModel : BaseNopEntityModel, IAclSupportedModel, IDiscountSupportedModel,
        ILocalizedModel<CategoryLocalizedModel>, IStoreMappingSupportedModel
    {
        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.MembershipDiscount")]
        public decimal MembershipDiscount { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.QuickFilterHeading")]
        public string QuickFilterHeading { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.EnableInfiniteScroll")]
        public bool EnableInfiniteScroll { get; set; }



        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.DisplayGridListOption")]
        public bool DisplayGridListOption { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.FeaturedListingDisplaySimilarOnTop")]
        public bool FeaturedListingDisplaySimilarOnTop { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.AlternateDisplayOrder")]
        public int AlternateDisplayOrder { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.CustomizationFormTemplateId")]
        public int CustomizationFormTemplateId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.ATCRecommendType")]
        public int ATCRecommendTypeId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.ATCRecommendHeading")]
        public string ATCRecommendHeading { get; set; }
        [NopResourceDisplayName("Admin.Catalog.Products.Fields.ATCRecommendedProductIds")]
        public string ATCRecommendedProductIds { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.EnableHoverImage")]
        public bool EnableHoverImage { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.AdditionalDescription")]
        public string AdditionalDescription { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.UseNewVersionOfTemplate")]
        public bool UseNewVersionOfTemplate { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.ConversionValue")]
        public decimal ConversionValue { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.UseNewVersionCustomizationAction")]
        public bool UseNewVersionCustomizationAction { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.PictureThumbnailUrl")]
        public string PictureThumbnailUrl { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.EnableNewATCLayout")]
        public bool EnableNewATCLayout { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.IsVisibleInCatalog")]
        public bool IsVisibleInCatalog { get; set; }
    }
    public partial record CategoryLocalizedModel : ILocalizedLocaleModel
    {
        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.AdditionalDescription")]
        public string AdditionalDescription { get; set; }
    }
}
