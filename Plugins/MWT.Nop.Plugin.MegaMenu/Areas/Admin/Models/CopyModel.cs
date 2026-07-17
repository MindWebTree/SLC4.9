using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Text;


namespace MWT.Nop.Plugin.MegaMenu.Areas.Admin.Models
{
    public record CopyModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("MWT.Plugin.Admin.Copy.Name")]
        public string Name { get; set; }

        public bool SupportCopyConditions { get; set; }

        [NopResourceDisplayName("MWT.Plugin.Admin.Copy.CopyConditions")]
        public bool CopyConditions { get; set; }

        public bool SupportCopyMappings { get; set; }

        [NopResourceDisplayName("MWT.Plugin.Admin.Copy.CopyMappings")]
        public bool CopyMappings { get; set; }

        public bool SupportCopyScheduling { get; set; }

        [NopResourceDisplayName("MWT.Plugin.Admin.Copy.CopyScheduling")]
        public bool CopyScheduling { get; set; }

        public string ActionName { get; set; }

        public string ControllerName { get; set; }

      
 
    }
}
