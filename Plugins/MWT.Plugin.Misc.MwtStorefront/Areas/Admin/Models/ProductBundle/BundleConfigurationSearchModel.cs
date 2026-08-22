using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace MWT.Plugin.Misc.MwtStorefront.Area.Admin.Models
{
    public record BundleConfigurationSearchModel : BaseSearchModel
    {
       public int ProductId { get; set; } 
       public bool HasConfigurations { get; set; } 
    }
}