using MWT.Nop.Plugin.MegaMenu.Components;
using MWT.Nop.Plugin.MegaMenu.Plugin;
using MWT.Nop.Plugin.MegaMenu.Services;
using Nop.Core.Domain.Cms;
using Nop.Services.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.MegaMenu.Infrastructure
{
    public class MegaMenuPlugin : BaseAdminWidgetPluginMwt
    {
        private readonly
    WidgetSettings _widgetSettings;
        private readonly ISettingService _settingService;
        private readonly IMappingInstallerService _mappingInstallerService;
        private static readonly List<MenuItemMwt> MenuItems = new List<MenuItemMwt>()
    {
      new MenuItemMwt()
      {
        SubMenuName = "MWT.MegaMenu.Admin.Submenus.Settings",
        SubMenuRelativePath = "MegaMenuAdmin/Settings"
      },
      new MenuItemMwt()
      {
        SubMenuName = "MWT.MegaMenu.Admin.Submenus.Menus",
        SubMenuRelativePath = "MegaMenuAdmin/ManageMenus"
      }
    };

        private static bool IsTrialVersion => false;

        public MegaMenuPlugin(
          WidgetSettings widgetSettings,
          ISettingService settingService,
          IMappingInstallerService mappingInstallerService)
          : base(MegaMenuPlugin.MenuItems, "MWT.Plugin.MegaMenu.Admin.Menu.MenuName", "MWT.Nop.Plugin.MegaMenu", MegaMenuPlugin.IsTrialVersion, "http://www.nop-templates.com/mega-menu-plugin-for-nopcommerce")
        {
            this._widgetSettings = widgetSettings;
            this._settingService = settingService;
            this._mappingInstallerService = mappingInstallerService;
        }

        protected override async Task InstallAdditionalSettingsAsync()
        {
            if (!this._widgetSettings.ActiveWidgetSystemNames.Contains("MWT.Nop.Plugin.MegaMenu"))
            {
                this._widgetSettings.ActiveWidgetSystemNames.Add("MWT.Nop.Plugin.MegaMenu");
                await this._settingService.SaveSettingAsync<WidgetSettings>(this._widgetSettings, 0);

            }
            await this._mappingInstallerService.InstallMappingsForEntityAsync(MWT.Nop.Plugin.MegaMenu.Infrastructure.Constants.Plugin.EntityType);
        }

        protected override async Task UninstallAdditionalSettingsAsync() => await this._mappingInstallerService.UnInstallMappingsForEntityAsync(MWT.Nop.Plugin.MegaMenu.Infrastructure.Constants.Plugin.EntityType);

        //public override string GetWidgetViewComponentName(string widgetZone) => "MegaMenu";

        public override string GetConfigurationPageUrl() => this.StoreLocation + "Admin/MegaMenuAdmin/Settings";
      
        public override Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(MegaMenuViewComponent);
        }
    }
}
