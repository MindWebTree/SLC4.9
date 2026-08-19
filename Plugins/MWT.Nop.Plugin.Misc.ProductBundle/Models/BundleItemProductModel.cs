using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace MWT.Nop.Plugin.Misc.ProductBundle.Models
{
    public record BundleItemProductModel : BaseNopEntityModel
    {

        [NopResourceDisplayName("MWT.Plugin.ProductBundle.BundleId")]
        public int BundleId { get; set; }

        [NopResourceDisplayName("MWT.Plugin.ProductBundle.ProductId")]
        public int ProductId { get; set; }

        [NopResourceDisplayName("MWT.Plugin.ProductBundle.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("MWT.Plugin.ProductBundle.Picture")]
        public string Picture { get; set; }

        [NopResourceDisplayName("MWT.Plugin.ProductBundle.Published")]
        public bool Published { get; set; }
        [NopResourceDisplayName("MWT.Plugin.ProductBundle.Published")]
        public int Quantity { get; set; }
    }
}