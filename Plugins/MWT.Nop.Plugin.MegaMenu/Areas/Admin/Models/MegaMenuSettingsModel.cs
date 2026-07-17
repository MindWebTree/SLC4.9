
using Nop.Core;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace MWT.Nop.Plugin.MegaMenu.Areas.Admin.Models
{
    public record MegaMenuSettingsModel : BaseNopEntityModel
    {
        public bool IsTrialVersion { get; set; }

        [NopResourceDisplayName("MWT.MegaMenu.Admin.Settings.Enabled")]
        public bool Enabled { get; set; }

        public bool Enabled_OverrideForStore { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }
    }
}
