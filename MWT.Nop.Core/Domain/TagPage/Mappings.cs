using Nop.Core;
using Nop.Core.Domain.Localization;
using System;

namespace MWT.Nop.Core.Domain.TagPage
{
    /// <summary>
    /// First URL segment — maps a slug to a nopCommerce ProductTag
    /// Example: "new" → TagId 5 (New Arrivals)
    /// </summary>
    public class TagSlugMapping : BaseEntity, ILocalizedEntity
    {
        public string Slug { get; set; }
        public int TagId { get; set; }
        public string Label { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public string MetaTitle { get; set; }
        public bool EnableInfiniteScroll { get; set; }
        public string Description { get; set; }
        public string AdditionalDescription { get; set; }
        public string ExploreMoreLinks { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
    }

    /// <summary>
    /// Second URL segment — maps a slug to either a Category or
    /// a Specification Attribute Option (or nothing = "all")
    /// Example: "furniture" → Category 5
    ///          "wood"      → SpecAttr 3, Option 12
    ///          "all"       → no filter
    /// </summary>
    public class SegmentSlugMapping : BaseEntity
    {
        /// <summary>URL slug, e.g. "furniture", "wood", "all-in-stock"</summary>
        public string Slug { get; set; }

        /// <summary>Display label, e.g. "Furniture", "Wood", "All In Stock"</summary>
        public string Label { get; set; }

        /// <summary>Filter type: 0=All, 1=Category, 2=Specification</summary>
        public int SlugType { get; set; }

        // --- Category filter fields ---
        public int? CategoryId { get; set; }

        public int TagId { get; set; }

        // --- Specification filter fields ---
        public int? SpecificationAttributeId { get; set; }
        public int? SpecificationAttributeOptionId { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public string MetaTitle { get; set; }
        public bool EnableInfiniteScroll { get; set; }
        public string Description { get; set; }
        public string AdditionalDescription { get; set; }

    }

    public enum SegmentSlugType
    {
        All = 0,
        Category = 1,
        Specification = 2
    }
}
