using Nop.Core;
using Nop.Plugin.ExternalAuth.Google.Components;
using Nop.Services.Authentication.External;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nop.Plugin.ExternalAuth.Google
{
    /// <summary>
    /// Represents method for the authentication with Facebook account
    /// </summary>
    public class GoogleAuthenticationMethod : BasePlugin, IExternalAuthenticationMethod
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public GoogleAuthenticationMethod(ILocalizationService localizationService,
            ISettingService settingService,
            IWebHelper webHelper)
        {
            _localizationService = localizationService;
            _settingService = settingService;
            _webHelper = webHelper;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/GoogleAuthentication/Configure";
        }


        /// <summary>
        /// Gets a name of a view component for displaying plugin in public store
        /// </summary>
        /// <returns>View component name</returns>
      
        public Type GetPublicViewComponent()
        {
           return typeof(GoogleAuthenticationViewComponent);
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task InstallAsync()
        {
            //settings
            await _settingService.SaveSettingAsync(new GoogleExternalAuthSettings());

            //locales
            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.ExternalAuth.Google.ClientKeyIdentifier"] = "Client ID",
                ["Plugins.ExternalAuth.Google.ClientKeyIdentifier.Hint"] = "Enter your Google Client ID here. You can find it in your Google Cloud Console.",
                ["Plugins.ExternalAuth.Google.ClientSecret"] = "Client Secret",
                ["Plugins.ExternalAuth.Google.ClientSecret.Hint"] = "Enter your Google Client Secret here. You can find it in your Google Cloud Console.",
                ["Plugins.ExternalAuth.Google.Instructions"] = "<p>To configure authentication with Google, please follow these steps:<br/><br/><ol><li>Navigate to the <a href=\"https://console.cloud.google.com/\" target =\"_blank\" > Google Cloud Console</a> page and sign in.</li><li>Create a new project or select an existing one.</li><li>Navigate to <b>APIs & Services > Credentials</b> and click <b>Create Credentials > OAuth Client ID</b>.</li><li>Select <b>Web application</b> and enter a name.</li><li>Under <b>Authorized redirect URIs</b>, add: \"{0:s}signin-google\"</li><li>Click <b>Create</b> and copy your Client ID and Client Secret below.</li></ol><br/><br/></p>"
            });

            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<GoogleExternalAuthSettings>();

            //locales
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.ExternalAuth.Google");

            await base.UninstallAsync();
        }

        #endregion
    }
}