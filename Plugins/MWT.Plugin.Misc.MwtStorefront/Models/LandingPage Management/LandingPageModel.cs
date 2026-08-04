using Nop.Web.Framework.Models;

namespace MWT.Plugin.Misc.MwtStorefront.Models.LandingPage_Management
{
    public partial record LandingPageModel : BaseNopEntityModel
    {
        public string Name { get; set; }
        public string SeName { get; set; }
        public string Description { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public int PictureId { get; set; }
        public string PageContent { get; set; }
        public int DisplayOrder { get; set; }
        public bool Published { get; set; }
        public bool Deleted { get; set; }
    }
}
