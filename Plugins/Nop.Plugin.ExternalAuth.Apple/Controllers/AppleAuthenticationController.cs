using AspNet.Security.OAuth.Apple;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MWT.Nop.Core.Services.Authentication;
using Nop.Core;
using Nop.Plugin.ExternalAuth.Apple.Models;
using Nop.Services.Authentication.External;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Nop.Plugin.ExternalAuth.Apple.Controllers
{
    public class AppleAuthenticationController : BasePluginController
    {
        #region Fields

        private readonly AppleExternalAuthSettings _appleExternalAuthSettings;
        private readonly IAuthenticationPluginManager _authenticationPluginManager;
        private readonly IExternalAuthenticationExtendedService _externalAuthenticationService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IOptionsMonitorCache<Microsoft.AspNetCore.Authentication.OAuth.OAuthOptions> _optionsCache;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public AppleAuthenticationController(
            AppleExternalAuthSettings appleExternalAuthSettings,
            IAuthenticationPluginManager authenticationPluginManager,
            IExternalAuthenticationExtendedService externalAuthenticationService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IOptionsMonitorCache<Microsoft.AspNetCore.Authentication.OAuth.OAuthOptions> optionsCache,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext,
            IWorkContext workContext)
        {
            _appleExternalAuthSettings = appleExternalAuthSettings;
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
        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_SETTINGS))
                return AccessDeniedView();

            var model = new ConfigurationModel
            {
                ClientId = _appleExternalAuthSettings.ClientId,
                TeamId = _appleExternalAuthSettings.TeamId,
                KeyId = _appleExternalAuthSettings.KeyId,
                PrivateKey = _appleExternalAuthSettings.PrivateKey,
            };

            return View("~/Plugins/ExternalAuth.Apple/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [AuthorizeAdmin]
        [Area(AreaNames.ADMIN)]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_SETTINGS))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return await Configure();

            // Save settings
            _appleExternalAuthSettings.ClientId = model.ClientId;
            _appleExternalAuthSettings.TeamId = model.TeamId;
            _appleExternalAuthSettings.KeyId = model.KeyId;
            _appleExternalAuthSettings.PrivateKey = model.PrivateKey;

            await _settingService.SaveSettingAsync(_appleExternalAuthSettings);

            // Clear Apple authentication options cache
            _optionsCache.TryRemove("Apple");

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }

        public async Task<IActionResult> Login(string returnUrl, bool isCheckout)
        {
            var methodIsAvailable = await _authenticationPluginManager
                .IsPluginActiveAsync(AppleAuthenticationDefaults.SystemName, await _workContext.GetCurrentCustomerAsync(), (await _storeContext.GetCurrentStoreAsync()).Id);

            if (!methodIsAvailable)
                throw new NopException("Apple authentication module cannot be loaded");

            if (string.IsNullOrEmpty(_appleExternalAuthSettings.ClientId) ||
                string.IsNullOrEmpty(_appleExternalAuthSettings.PrivateKey))
                throw new NopException("Apple authentication module not configured");

            var authenticationProperties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("logincallback", "appleauthentication", new { returnUrl })
            };
            if (!isCheckout)
            {
                authenticationProperties.SetString(AppleAuthenticationDefaults.ErrorCallback, Url.RouteUrl("Login", new { returnUrl }));
            }
            else
            {
                authenticationProperties.SetString(AppleAuthenticationDefaults.ErrorCallback, Url.RouteUrl("ShoppingCart"));
            }

            return Challenge(authenticationProperties, "Apple");
        }
        
        public async Task<IActionResult> LoginCallback(string returnUrl)
        {
            var authenticateResult = await HttpContext.AuthenticateAsync("Apple");
            if (!authenticateResult.Succeeded || !authenticateResult.Principal.Claims.Any())
            {
                return Content($@"<script>
            if (window.opener) {{ 
                window.close();
            }} else {{
                    window.close();
            }}
                </script>", "text/html");
            }

            var authenticationParameters = new ExternalAuthenticationParameters
            {
                ProviderSystemName = AppleAuthenticationDefaults.SystemName,
                AccessToken = await HttpContext.GetTokenAsync("Apple", "access_token"),
                Email = authenticateResult.Principal.FindFirst(claim => claim.Type == ClaimTypes.Email)?.Value,
                ExternalIdentifier = authenticateResult.Principal.FindFirst(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value,
                ExternalDisplayIdentifier = authenticateResult.Principal.FindFirst(claim => claim.Type == ClaimTypes.Name)?.Value,
                Claims = authenticateResult.Principal.Claims.Select(claim => new ExternalAuthenticationClaim(claim.Type, claim.Value)).ToList()
            };

            if (returnUrl.Contains("/cart"))
            {
                await _externalAuthenticationService.CustomAuthenticateAsync(authenticationParameters, returnUrl);

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
                var result = await _externalAuthenticationService.CustomAuthenticateAsync(authenticationParameters, returnUrl);
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
