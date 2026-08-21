using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.TagAdmin
{
    // ── Tag Slug (first segment) ─────────────────────────────────────────
    public record TagSlugMappingModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.TagPages.TagSlug.Slug")]
        public string Slug { get; set; }

        [NopResourceDisplayName("Admin.TagPages.TagSlug.TagId")]
        [Required]
        public int TagId { get; set; }

        [NopResourceDisplayName("Admin.TagPages.TagSlug.Label")]

        [Required]
        public string Label { get; set; }

        [NopResourceDisplayName("Admin.TagPages.TagSlug.SortOrder")]
        public int SortOrder { get; set; }

        [NopResourceDisplayName("Admin.TagPages.TagSlug.IsActive")]
        public bool IsActive { get; set; } = true;

        // Dropdown options
        public IList<SelectListItem> AvailableTags { get; set; } = new List<SelectListItem>();

        [NopResourceDisplayName("Admin.TagPages.TagSlug.MetaKeywords")]
        public string MetaKeywords { get; set; }

        [NopResourceDisplayName("Admin.TagPages.TagSlug.MetaDescription")]
        public string MetaDescription { get; set; }

        [NopResourceDisplayName("Admin.TagPages.TagSlug.MetaTitle")]
        public string MetaTitle { get; set; }

        [NopResourceDisplayName("Admin.TagPages.TagSlug.EnableInfiniteScroll")]
        public bool EnableInfiniteScroll { get; set; }

        [NopResourceDisplayName("Admin.TagPages.TagSlug.Description")]
        public string Description { get; set; }

        [NopResourceDisplayName("Admin.TagPages.TagSlug.AdditionalDescription")]
        public string AdditionalDescription { get; set; }

        [NopResourceDisplayName("Admin.TagPages.TagSlug.ExploreMoreLinks")]
        public string ExploreMoreLinks { get; set; }
    }

    public record TagSlugMappingListModel : BasePagedListModel<TagSlugMappingModel> { }

    public record TagSlugMappingSearchModel : BaseSearchModel { }

    // ── Segment Slug (second segment) ────────────────────────────────────
    public record SegmentSlugMappingModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.TagPages.SegmentSlug.Slug")]

        public string Slug { get; set; }

        [NopResourceDisplayName("Admin.TagPages.SegmentSlug.Label")]

        [Required]
        public string Label { get; set; }

        [NopResourceDisplayName("Admin.TagPages.SegmentSlug.SlugType")]
        public int SlugType { get; set; }

        // Category
        [NopResourceDisplayName("Admin.TagPages.SegmentSlug.CategoryId")]
        public int? CategoryId { get; set; }

        [Required]
        [NopResourceDisplayName("Admin.TagPages.SegmentSlug.TagId")]
        public int TagId { get; set; }

     
        [NopResourceDisplayName("Admin.TagPages.SegmentSlug.Tag.Name")]
        public string TagSlug { get; set; }

        // Specification
        [NopResourceDisplayName("Admin.TagPages.SegmentSlug.SpecificationAttributeId")]
        public int? SpecificationAttributeId { get; set; }

        [NopResourceDisplayName("Admin.TagPages.SegmentSlug.SpecificationAttributeOptionId")]
        public int? SpecificationAttributeOptionId { get; set; }

        [NopResourceDisplayName("Admin.TagPages.SegmentSlug.SortOrder")]
        public int SortOrder { get; set; }

        [NopResourceDisplayName("Admin.TagPages.SegmentSlug.IsActive")]
        public bool IsActive { get; set; } = true;

        [NopResourceDisplayName("Admin.TagPages.SegmentSlug.MetaKeywords")]
        public string MetaKeywords { get; set; }

        [NopResourceDisplayName("Admin.TagPages.SegmentSlug.MetaDescription")]
        public string MetaDescription { get; set; }

        [NopResourceDisplayName("Admin.TagPages.SegmentSlug.MetaTitle")]
        public string MetaTitle { get; set; }

        [NopResourceDisplayName("Admin.TagPages.SegmentSlug.EnableInfiniteScroll")]
        public bool EnableInfiniteScroll { get; set; }
        
        [NopResourceDisplayName("Admin.TagPages.SegmentSlug.Description")]
        public string Description { get; set; }

        [NopResourceDisplayName("Admin.TagPages.SegmentSlug.AdditionalDescription")]
        public string AdditionalDescription { get; set; }

        // Dropdown options

        public IList<SelectListItem> AvailableTagSlugs { get; set; } = new List<SelectListItem>();
        public IList<SelectListItem> AvailableSlugTypes { get; set; } = new List<SelectListItem>();
        public IList<SelectListItem> AvailableCategories { get; set; } = new List<SelectListItem>();
        public IList<SelectListItem> AvailableSpecAttributes { get; set; } = new List<SelectListItem>();
        public IList<SelectListItem> AvailableSpecOptions { get; set; } = new List<SelectListItem>();
    }

    public record SegmentSlugMappingListModel : BasePagedListModel<SegmentSlugMappingModel> { }

    public record SegmentSlugMappingSearchModel : BaseSearchModel { }
}
