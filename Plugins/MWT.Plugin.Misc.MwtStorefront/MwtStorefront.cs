using MWT.Nop.Core.Infrastructure;
using MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Components;
using MWT.Plugin.Misc.MwtStorefront.Components;
using Nop.Services.Cms;
using Nop.Services.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront
{
    public class MwtStorefrontPlugin : BasePlugin, IWidgetPlugin
    {
        public MwtStorefrontPlugin()
        {
            // 🔴 FIRST BREAKPOINT
        }

        public override Task InstallAsync()
        {
            return base.InstallAsync();
        }

        public override Task UninstallAsync()
        {
            return base.UninstallAsync();
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(
               new List<string>
               {
                    CustomPublicWidgetZones.ProductAdminConfigurationBundle
               });
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(ProductAdminAttributeBundleViewComponent);
        }
        public bool HideInWidgetList => false;
    }
}
