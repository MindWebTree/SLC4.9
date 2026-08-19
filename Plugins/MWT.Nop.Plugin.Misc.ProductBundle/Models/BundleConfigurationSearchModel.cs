using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace MWT.Nop.Plugin.Misc.ProductBundle.Models
{
    public record BundleConfigurationSearchModel : BaseSearchModel
    {
       public int ProductId { get; set; } 
       public bool HasConfigurations { get; set; } 
    }
}