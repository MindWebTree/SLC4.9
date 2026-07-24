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
            return null;
        }

        public Type? GetWidgetViewComponent(string widgetZone)
        {
            return null;
        }
        public bool HideInWidgetList => false;
    }
}
