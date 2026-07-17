using Microsoft.Extensions.DependencyInjection;
using MWT.Nop.Plugin.MegaMenu.Helpers;
using Nop.Core.Infrastructure;
using Nop.Services.Cms;
using Nop.Services.Plugins;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


#nullable enable
namespace MWT.Nop.Plugin.MegaMenu.Plugin
{
    public abstract class BaseAdminWidgetPluginMwt : BaseAdminPluginMwt, IWidgetPlugin, IPlugin
    {

    IInstallHelper _installHelper;

        public BaseAdminWidgetPluginMwt(
          List<MenuItemMwt> menuItems,
          string pluginAdminMenuName,
          string pluginFolderName,
          bool isTrialVersion = false,
          string pluginUrlInStore = "")
          : base(menuItems, pluginAdminMenuName, pluginFolderName, isTrialVersion, pluginUrlInStore)
        {
            this._installHelper = EngineContext.Current.Resolve<IInstallHelper>();
        }

        public async Task<IList<string>> GetWidgetZonesAsync()
        {
        
            List<string> list = (await _installHelper.GetSupportedWidgetZonesAsync(PluginFolderName)).ToList<string>();
            list.AddRange((IEnumerable<string>)GetSupportedAdminWidgetZones());
            return (IList<string>)list;
        }

        public bool HideInWidgetList => false;

        public abstract Type GetWidgetViewComponent(string widgetZone);

        protected virtual IList<string> GetSupportedAdminWidgetZones() => (IList<string>)new List<string>();

        
    }
}
