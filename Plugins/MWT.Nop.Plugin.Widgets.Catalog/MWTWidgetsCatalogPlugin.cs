using MWT.Nop.Core.Infrastructure;
using MWT.Nop.Plugin.Widgets.Catalog.Components;
using Nop.Core;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;

namespace MWT.Nop.Plugin.Widgets.Catalog
{
    /// <summary>
    /// PLugin
    /// </summary>
    public class MWTWidgetsCatalogPlugin : BasePlugin, IWidgetPlugin
    {
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;

        public MWTWidgetsCatalogPlugin(ILocalizationService localizationService,
            ISettingService settingService,
            IWebHelper webHelper)
        {
            _localizationService = localizationService;
            _settingService = settingService;
            _webHelper = webHelper;
        }

        /// <summary>
        /// Gets widget zones where this widget should be rendered
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the widget zones
        /// </returns>
        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string> { PublicWidgetZones.CategoryDetailsTop,CustomPublicWidgetZones.CategoryDetailsTopVideo ,
                PublicWidgetZones.CategoryDetailsBottom, CustomPublicWidgetZones.CategoryDetailsProductListMiddle
                , CustomPublicWidgetZones.CategoryDetailsProductListThirdPosition , CustomPublicWidgetZones.CategoryDetailsProductListNinthPosition, CustomPublicWidgetZones.CategoryDetailsProductListSixthPosition
            }
                );
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/MWTWidgetsCatalog/Configure";
        }

        /// <summary>
        /// Gets a name of a view component for displaying widget
        /// </summary>
        /// <param name="widgetZone">Name of the widget zone</param>
        /// <returns>View component name</returns>
        public Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(mwtCatalogWidgetViewComponent);
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task InstallAsync()
        {
            //settings
            await _settingService.SaveSettingAsync(new MWTPluginWidgetsCatalogSetting());

            //locales
            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["MWT.Plugins.Widgets.Catalog.Fields.Category"] = "Category",
                ["mwt.nop.plugin.widgets.catalog.fields.entityname"] = "Entity Name",
                ["mwt.nop.plugin.widgets.catalog.fields.widgetzone"] = "Widget Zone",
                ["mwt.nop.plugin.widgets.catalog.fields.actionlink"] = "Link",
                ["mwt.nop.plugin.widgets.catalog.fields.mobileactionlink"] = "Mobile Link",
                ["mwt.nop.plugin.widgets.catalog.addrecord"] = "Add record",
                ["MWT.Plugins.Widgets.Catalog.Fields.EntityId"] = "Please select entity",
                ["MWT.Plugins.Widgets.Catalog.Fields.WidgetZone"] = "Please select Widget Zone",
                ["MWT.Plugins.Widgets.Catalog.Fields.ActionLink"] = "Link",
                ["MWT.Plugins.Widgets.Catalog.Fields.BannerId"] = "Banner",
                ["mwt.plugins.widgets.catalog.fields.html"] = "Banner Html",
                ["MWT.Plugins.Widgets.Catalog.Fields.MobileActionLink"] = "Mobile Link",
                ["MWT.Plugins.Widgets.Catalog.Fields.MobileBannerId"] = "Mobile Banner",
                ["MWT.Plugins.Widgets.Catalog.Fields.MobileHtml"] = "Mobile Banner Html",
                ["mwt.plugins.widgets.catalog.heading.mobilesection"] = "Mobile Device",
                ["mwt.plugins.widgets.catalog.Heading.desktopsection"] = "Desktop Device",
                ["MWT.Plugins.Widgets.Catalog.Note"] = "Notes: Only 1 Banner Image/Banner Html render on front end, High priority is of Image."

            });

            await base.InstallAsync();

        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<MWTPluginWidgetsCatalogSetting>();

            //locales
            await _localizationService.DeleteLocaleResourcesAsync("Mwt.Widgets.Catalog");

            await base.UninstallAsync();
        }

        /// <summary>
        /// Gets a value indicating whether to hide this plugin on the widget list page in the admin area
        /// </summary>
        public bool HideInWidgetList => false;
    }
}