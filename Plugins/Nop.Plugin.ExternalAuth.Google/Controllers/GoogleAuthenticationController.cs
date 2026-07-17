using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nop.Core;
using Nop.Plugin.ExternalAuth.Google.Models;
using Nop.Services.Authentication.External;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.ExternalAuth.Google.Controllers
{
    public class GoogleAuthenticationController : BasePluginController
    {
        #region Fields

        private readonly GoogleExternalAuthSettings _googleExternalAuthSettings;
        private readonly IAuthenticationPluginManager _authenticationPluginManager;
        private readonly IExternalAuthenticationService _externalAuthenticationService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IOptionsMonitorCache<GoogleOptions> _optionsCache;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public GoogleAuthenticationController(GoogleExternalAuthSettings googleExternalAuthSettings,
            IAuthenticationPluginManager authenticationPluginManager,
            IExternalAuthenticationService externalAuthenticationService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IOptionsMonitorCache<GoogleOptions> optionsCache,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext,
            IWorkContext workContext)
        {
            _googleExternalAuthSettings = googleExternalAuthSettings;
            _authenticationPluginManager = authenticationPluginManager;
            _externalAuthenticationService = externalAuthenticationService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _optionsCache = optionsCache;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        [AuthorizeAdmin]
        [Area(AreaNames.ADMIN)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_SETTINGS))
                return AccessDeniedView();

            var model = new ConfigurationModel
            {
                ClientId = _googleExternalAuthSettings.ClientKeyIdentifier,
                ClientSecret = _googleExternalAuthSettings.ClientSecret
            };

            return View("~/Plugins/ExternalAuth.Google/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [AuthorizeAdmin]
        [Area(AreaNames.ADMIN)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_SETTINGS))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return await Configure();

            //save settings
            _googleExternalAuthSettings.ClientKeyIdentifier = model.ClientId;
            _googleExternalAuthSettings.ClientSecret = model.ClientSecret;
            await _settingService.SaveSettingAsync(_googleExternalAuthSettings);

            //clear google authentication options cache
            _optionsCache.TryRemove(GoogleDefaults.AuthenticationScheme);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> Login(string returnUrl, bool isCheckout)
        {
            var methodIsAvailable = await _authenticationPluginManager
                .IsPluginActiveAsync(GoogleAuthenticationDefaults.SystemName, await _workContext.GetCurrentCustomerAsync(), (await _storeContext.GetCurrentStoreAsync()).Id);
            if (!methodIsAvailable)
                throw new NopException("Google authentication module cannot be loaded");

            if (string.IsNullOrEmpty(_googleExternalAuthSettings.ClientKeyIdentifier) ||
                string.IsNullOrEmpty(_googleExternalAuthSettings.ClientSecret))
            {
                throw new NopException("Google authentication module not configured");
            }

            //configure login callback action
            var authenticationProperties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("LoginCallback", "GoogleAuthentication", new { returnUrl = returnUrl })
            };
            if (!isCheckout)
            {
                authenticationProperties.SetString(GoogleAuthenticationDefaults.ErrorCallback, Url.RouteUrl("Login", new { returnUrl }));
            }
            else
            {
                authenticationProperties.SetString(GoogleAuthenticationDefaults.ErrorCallback, Url.RouteUrl("ShoppingCart"));
            }

            return Challenge(authenticationProperties, GoogleDefaults.AuthenticationScheme);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> LoginCallback(string returnUrl)
        {
 
            var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (!authenticateResult.Succeeded || !authenticateResult.Principal.Claims.Any())
                return RedirectToRoute("Login");

            //create external authentication parameters
            var authenticationParameters = new ExternalAuthenticationParameters
            {
                ProviderSystemName = GoogleAuthenticationDefaults.SystemName,
                AccessToken = await HttpContext.GetTokenAsync(GoogleDefaults.AuthenticationScheme, "access_token"),
                Email = authenticateResult.Principal.FindFirst(claim => claim.Type == ClaimTypes.Email)?.Value,
                ExternalIdentifier = authenticateResult.Principal.FindFirst(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value,
                ExternalDisplayIdentifier = authenticateResult.Principal.FindFirst(claim => claim.Type == ClaimTypes.Name)?.Value,
                Claims = authenticateResult.Principal.Claims.Select(claim => new ExternalAuthenticationClaim(claim.Type, claim.Value)).ToList()
            };

            if (returnUrl.Contains("/cart"))
            {
                await _externalAuthenticationService.AuthenticateAsync(authenticationParameters, returnUrl);

                return Content($@"<script>
            if (window.opener) {{
                window.opener.location.href = '{returnUrl}';
                window.close();
            }} else {{
                window.location.href = '{returnUrl}';
            }}
                </script>", "text/html");
            }
            else
            {
                var result = await _externalAuthenticationService.AuthenticateAsync(authenticationParameters, returnUrl);
                if (result is RedirectResult redirectResult)
                {
                        return Content($@"<script>
            if (window.opener) {{
                window.opener.location.href = '{returnUrl}';
                window.close();
            }} else {{
                window.location.href = '{returnUrl}';
            }}
                </script>", "text/html");
                }
                else
                {
                    return Content($@"<script>
            if (window.opener) {{
                window.opener.location.href = '{Url.RouteUrl("Login", new { returnUrl })}';
                window.close();
            }} else {{
                window.location.href = '{Url.RouteUrl("Login", new { returnUrl })}';
            }}
                </script>", "text/html");
                }
            }
        }

        #endregion
    }
}