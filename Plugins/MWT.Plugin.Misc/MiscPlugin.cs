using System.Threading.Tasks;
using MWT.Plugin.Misc.Domain;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Plugins;

namespace MWT.Plugin.Misc
{
    public class MiscPlugin : BasePlugin, IMiscPlugin
    {
        private readonly ISettingService _settingService;

        public MiscPlugin(ISettingService settingService)
        {
            _settingService = settingService;
        }

        public override async Task InstallAsync()
        {
            await _settingService.SaveSettingAsync(new UtmSettings
            {
                CookieExpiryDays = 30,
                UseFirstTouchAttribution = true
            });

            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await _settingService.DeleteSettingAsync<UtmSettings>();
            await base.UninstallAsync();
        }
    }
}
