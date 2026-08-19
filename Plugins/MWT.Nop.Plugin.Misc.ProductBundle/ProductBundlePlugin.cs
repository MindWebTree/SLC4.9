using System.Threading.Tasks;
using Nop.Services.Plugins;
using Nop.Services.Localization;
using Nop.Services.Common;
using Nop.Services.Cms;
using System.Collections.Generic;
using Nop.Web.Framework.Infrastructure;
using Nop.Services.Configuration;
using MWT.Nop.Plugin.Misc.ProductBundle.Domain.MWT.Nop.Plugin.Misc.ProductBundle.Domain;
using MWT.Nop.Plugin.Misc.ProductBundle.Services;

namespace MWT.Nop.Plugin.Misc.ProductBundle
{
    public class ProductBundlePlugin : BasePlugin ,IWidgetPlugin
    {
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IBundleService _bundleService;


        public ProductBundlePlugin(ILocalizationService localizationService, ISettingService settingService, IBundleService bundleService)
        {
            _localizationService = localizationService;
            _settingService = settingService;
            _bundleService = bundleService;
        }

        public override async Task InstallAsync()
        {

                await _settingService.SaveSettingAsync(new ProductBundleSettings
                {
                    DiscountPercentage = 0  
                }); 

            await _localizationService.AddLocaleResourceAsync(new System.Collections.Generic.Dictionary<string, string>
            {
                ["MWT.Plugin.ProductBundle.BundleName"] = "Bundle Name",
                ["MWT.Plugin.ProductBundle.ConfigurationValue"] = "Configuration Value",
                ["MWT.Plugin.ProductBundle.AddBundle"] = "Add Bundle",
                ["MWT.Plugin.ProductBundle.BundleItems"] = "Bundle Items",
                ["MWT.Plugin.ProductBundle.Product"] = "Product",
                ["MWT.Plugin.ProductBundle.Quantity"] = "Quantity",
                ["Plugins.Misc.ProductBundle.Fields.DiscountPercentage"] = "Discount (%)",
                ["Plugins.Misc.ProductBundle.Fields.DiscountPercentage.Hint"] = "Enter the discount percentage for bundle products."

            });

            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await _bundleService.ValidateAndRestoreIncompleteBundlesAsync();
            await _localizationService.DeleteLocaleResourcesAsync("MWT.Plugin.ProductBundle");

            await base.UninstallAsync();
        }

        public override string GetConfigurationPageUrl()
        {
            return "/Admin/ProductBundle/Configure";
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string> { PublicWidgetZones.ProductConfigurationBundle
            }
                );
        }

        public string GetWidgetViewComponentName(string widgetZone)
        {
            return "ProductBundleWidget";
        }

        public bool HideInWidgetList => false;
    }
}