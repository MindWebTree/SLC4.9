using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace MWT.Nop.Plugin.Misc.ProductBundle.Models
{
    public record BundleItemSearchModel : BaseSearchModel
    {
        [NopResourceDisplayName("MWT.Plugin.ProductBundle.BundleId")]
        public int BundleId { get; set; }
 public string BundleName { get; set; }
        public int AttributeValueId { get; set; }
    }
}