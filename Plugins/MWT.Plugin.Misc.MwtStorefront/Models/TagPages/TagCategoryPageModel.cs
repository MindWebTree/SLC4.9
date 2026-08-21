using MWT.Nop.Core.Domain.TagPage;
using Nop.Web.Models.Catalog;

namespace MWT.Plugin.Misc.MwtStorefront.Models.TagPages
{
    public class TagCategoryPageModel
    {
        // First segment
        public string TagSlug { get; set; }
        public string TagLabel { get; set; }
        public int TagId { get; set; }

        // Second segment
        public string SegmentSlug { get; set; }
        public string SegmentLabel { get; set; }
        public SegmentSlugType SlugType { get; set; }

        // Resolved filter values (used for product query)
        public int? CategoryId { get; set; }
        public int? SpecificationAttributeId { get; set; }
        public int? SpecificationAttributeOptionId { get; set; }

        // Sub-navigation links (e.g. All | Furniture | Outdoor | Wood | Red)
        public IList<SubNavLink> SubNavLinks { get; set; } = new List<SubNavLink>();

        // nopCommerce standard product list model
        public ProductsByTagModel Products { get; set; }
    }

    public class SubNavLink
    {
        public string Label { get; set; }
        public string Url { get; set; }
        public bool IsActive { get; set; }
    }
}
