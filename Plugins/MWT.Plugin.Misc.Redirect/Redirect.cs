using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;


namespace Nop.Plugin.Misc.Redirect
{   //test advrediredt
    public class Redirect : BasePlugin, IMiscPlugin, IWidgetPlugin
    {
        private readonly IWebHelper _webHelper;
        private readonly ILocalizationService _localizationService;
        private readonly WidgetSettings _widgetSettings;
        private readonly ISettingService _settingService;

        public bool HideInWidgetList => true;

        public Redirect(  IWebHelper webHelper, ILocalizationService localizationService, WidgetSettings widgetSettings, ISettingService settingService)
        {
            _webHelper = webHelper;
            _localizationService = localizationService;
            _widgetSettings = widgetSettings;
            _settingService = settingService;
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            throw new NotImplementedException();
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string>());
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/Redirect/Configure";
        }

        public override async Task InstallAsync()
        { 
            if (!_widgetSettings.ActiveWidgetSystemNames.Contains(RedirectDefaults.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Add(RedirectDefaults.SystemName);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }

            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await base.UninstallAsync();
        }

		public string GetWidgetViewComponentName(string widgetZone)
		{
			return RedirectDefaults.TRACKING_VIEW_COMPONENT_NAME;
		}
	}
}