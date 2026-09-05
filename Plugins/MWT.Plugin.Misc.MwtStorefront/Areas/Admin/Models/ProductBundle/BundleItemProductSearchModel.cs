using Nop.Web.Framework.Models;

namespace MWT.Plugin.Misc.MwtStorefront.Area.Admin.Models
{
    public record BundleItemProductSearchModel : BaseSearchModel
    {
        public int BundleId { get; set; }
        public int CopyVariantId { get; set; }
        public string SearchProductName { get; set; }  
        public string VariantIds { get; set; }  
    }
}