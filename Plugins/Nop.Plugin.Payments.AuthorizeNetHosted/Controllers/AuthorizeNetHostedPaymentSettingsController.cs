using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Plugin.Payments.AuthorizeNetHosted.Domain;
using Nop.Plugin.Payments.AuthorizeNetHosted.Models;
using Nop.Plugin.Payments.AuthorizeNetHosted.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Payments.AuthorizeNetHosted.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [AutoValidateAntiforgeryToken]
    public class AuthorizeNetHostedPaymentSettingsController : BasePaymentController
    {
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly IWebHelper _webHelper;
        private readonly ILogger _logger;
        private readonly AuthorizeNetHostedPaymentSettings _authorizeNetSettings;
        private readonly IAuthorizeNetWebHookService _authorizeNetWebHookService;

        public AuthorizeNetHostedPaymentSettingsController(
            ISettingService settingService,
            IPermissionService permissionService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IStoreContext storeContext,
            IStoreService storeService,
            IWebHelper webHelper,
            ILogger logger,
            AuthorizeNetHostedPaymentSettings authorizeNetSettings,
            IAuthorizeNetWebHookService authorizeNetWebHookService)
        {
            _settingService = settingService;
            _permissionService = permissionService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _storeContext = storeContext;
            _storeService = storeService;
            _webHelper = webHelper;
            _logger = logger;
            _authorizeNetSettings = authorizeNetSettings;
            _authorizeNetWebHookService = authorizeNetWebHookService;
        }

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<AuthorizeNetHostedPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                ActiveStoreScopeConfiguration = storeScope,
                LoginId = settings.LoginId,
                PublicClientKey = settings.PublicClientKey,
                TransactionKey = settings.TransactionKey,
                UseSandbox = settings.UseSandbox,
                EnableL2orL3 = settings.EnableL2orL3,
                TransactModeId = (int)settings.TransactMode,
                TransactModeValues = new List<SelectListItem>
                {
                    new() { Value = "0", Text = "Authorize Only" },
                    new() { Value = "1", Text = "Authorize & Capture" }
                },
                EnableVisaClicktoPay = settings.EnableVisaClicktoPay,
                VisaClicktoPayAPIKey = settings.VisaClicktoPayAPIKey,
                EnableBankAccount = settings.EnableBankAccount,
                WebhookId = settings.WebhookId,
                SignatureKey = settings.SignatureKey,
                AdditionalFee = settings.AdditionalFee,
                AdditionalFeePercentage = settings.AdditionalFeePercentage,
                showDebugInfo = settings.showDebugInfo,
                SerialNumber = settings.SerialNumber,
                EnableAcceptDirectCheckout = settings.EnableAcceptDirectCheckout,
                StoreUrl = _webHelper.GetStoreLocation()
            };

            if (storeScope > 0)
            {
                model.LoginId_OverrideForStore = await _settingService.SettingExistsAsync(settings, s => s.LoginId, storeScope);
                model.PublicClientKey_OverrideForStore = await _settingService.SettingExistsAsync(settings, s => s.PublicClientKey, storeScope);
                model.TransactionKey_OverrideForStore = await _settingService.SettingExistsAsync(settings, s => s.TransactionKey, storeScope);
                model.UseSandbox_OverrideForStore = await _settingService.SettingExistsAsync(settings, s => s.UseSandbox, storeScope);
                model.EnableL2orL3_OverrideForStore = await _settingService.SettingExistsAsync(settings, s => s.EnableL2orL3, storeScope);
                model.TransactModeId_OverrideForStore = await _settingService.SettingExistsAsync(settings, s => s.TransactMode, storeScope);
                model.EnableVisaClicktoPay_OverrideForStore = await _settingService.SettingExistsAsync(settings, s => s.EnableVisaClicktoPay, storeScope);
                model.VisaClicktoPayAPIKey_OverrideForStore = await _settingService.SettingExistsAsync(settings, s => s.VisaClicktoPayAPIKey, storeScope);
                model.EnableBankAccount_OverrideForStore = await _settingService.SettingExistsAsync(settings, s => s.EnableBankAccount, storeScope);
                model.SignatureKey_OverrideForStore = await _settingService.SettingExistsAsync(settings, s => s.SignatureKey, storeScope);
                model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(settings, s => s.AdditionalFee, storeScope);
                model.EnableAcceptDirectCheckout_OverrideForStore = await _settingService.SettingExistsAsync(settings, s => s.EnableAcceptDirectCheckout, storeScope);
           
            }



            return View("~/Plugins/Payments.AuthorizeNetHosted/Views/Configure/Configure.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid) return await Configure();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<AuthorizeNetHostedPaymentSettings>(storeScope);

            settings.LoginId = model.LoginId;
            settings.PublicClientKey = model.PublicClientKey;
            settings.TransactionKey = model.TransactionKey;
            settings.UseSandbox = model.UseSandbox;
            settings.EnableL2orL3 = model.EnableL2orL3;
            settings.TransactMode = (TransactMode)model.TransactModeId;
            settings.EnableVisaClicktoPay = model.EnableVisaClicktoPay;
            settings.VisaClicktoPayAPIKey = model.VisaClicktoPayAPIKey;
            settings.EnableBankAccount = model.EnableBankAccount;
            settings.SignatureKey = model.SignatureKey;
            settings.AdditionalFee = model.AdditionalFee;
            settings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            settings.showDebugInfo = model.showDebugInfo;
            settings.SerialNumber = model.SerialNumber;
            settings.EnableAcceptDirectCheckout = model.EnableAcceptDirectCheckout;
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, s => s.LoginId, model.LoginId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, s => s.PublicClientKey, model.PublicClientKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, s => s.TransactionKey, model.TransactionKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, s => s.UseSandbox, model.UseSandbox_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, s => s.EnableL2orL3, model.EnableL2orL3_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, s => s.TransactMode, model.TransactModeId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, s => s.EnableVisaClicktoPay, model.EnableVisaClicktoPay_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, s => s.VisaClicktoPayAPIKey, model.VisaClicktoPayAPIKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, s => s.EnableBankAccount, model.EnableBankAccount_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, s => s.SignatureKey, model.SignatureKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, s => s.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, s => s.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, s => s.EnableAcceptDirectCheckout, model.EnableAcceptDirectCheckout_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();
            _notificationService.SuccessNotification(
                await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));
            return await Configure();
        }

        // CreateWebHookAsync(authorizeNetSettings) — param name from metadata
        public async Task<IActionResult> CreateWebHookAsync(AuthorizeNetHostedPaymentSettings authorizeNetSettings)
        {
            var url = $"{(await _storeContext.GetCurrentStoreAsync())?.Url}{AuthorizeNetHostedPaymentDefaults.WebhookUrl}";

            var hook = new AuthorizeNetWebHook
            {
                url = url,
                eventTypes = new[]
{
    "net.authorize.payment.authcapture.created",
    "net.authorize.payment.capture.created",
    "net.authorize.payment.refund.created",
    "net.authorize.payment.void.created"
},
            name = _authorizeNetSettings.UseSandbox ? "SLC Sandbox" : "SLC Production"
            };


            if (string.IsNullOrEmpty(authorizeNetSettings.WebhookId))
                hook.webhookId = await _authorizeNetWebHookService.CreateAsync(hook);
            else
            {
                hook.webhookId = authorizeNetSettings.WebhookId;
                hook = await _authorizeNetWebHookService.UpdateAsync(hook);
            }

            authorizeNetSettings.WebhookId = hook?.webhookId;
            await _settingService.SaveSettingAsync(authorizeNetSettings);
            if (!string.IsNullOrEmpty(hook?.webhookId ?? ""))
                _notificationService.SuccessNotification(
                         await _localizationService.GetResourceAsync("Plugins.Payments.AuthorizeNetHostedPayment.Hook.Created"));
            else
                _notificationService.ErrorNotification(
                      await _localizationService.GetResourceAsync("Plugins.Payments.AuthorizeNetHostedPayment.Hook.Failed.Create"));
            return await Configure();
        }

        // RemoveWebHookAsync(authorizeNetSettings) — param name from metadata
        public async Task<IActionResult> RemoveWebHookAsync(AuthorizeNetHostedPaymentSettings authorizeNetSettings)
        {
            if (!string.IsNullOrEmpty(authorizeNetSettings.WebhookId))
            {

                var response = await _authorizeNetWebHookService.DeleteAsync(authorizeNetSettings.WebhookId);
                if (!response)
                    _notificationService.ErrorNotification(
               await _localizationService.GetResourceAsync("Plugins.Payments.AuthorizeNetHostedPayment.Hook.Failed.Removed"));

                else
                {

                    _notificationService.SuccessNotification(
          await _localizationService.GetResourceAsync("Plugins.Payments.AuthorizeNetHostedPayment.Hook.Removed"));
                    authorizeNetSettings.WebhookId = null;
                    await _settingService.SaveSettingAsync(authorizeNetSettings);
                }
            }
         ;
            return await Configure();
        }

        public async Task<IActionResult> ClearLogFile()
        {
            var logPath = GetLogFilePath();
            if (System.IO.File.Exists(logPath))
                System.IO.File.Delete(logPath);
            return Json(new { success = true });
        }

        public async Task<IActionResult> GetLogFile()
        {
            var logPath = GetLogFilePath();
            if (!System.IO.File.Exists(logPath))
                return Content("Log file is empty.");

            var content = await System.IO.File.ReadAllTextAsync(logPath);
            return Content(content, "text/plain");
        }
        private string GetLogFilePath()
         => System.IO.Path.Combine(
             AppDomain.CurrentDomain.BaseDirectory,
             "Plugins", "Payments.AuthorizeNetHostedPayment",
             "debug.log");

        [HttpPost]
        [ActionName("Configure")]
        [FormValueRequired(new string[] { "createwebhook" })]
        public async Task<IActionResult> GetWebhookId(ConfigurationModel model)
        {
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<AuthorizeNetHostedPaymentSettings>(storeScope);
            if (string.IsNullOrEmpty(settings.WebhookId))
            {
                return await CreateWebHookAsync(settings);
            }
            return await Configure();
        }
        [HttpPost]
        [ActionName("Configure")]
        [FormValueRequired(new string[] { "removewebhook" })]
        public async Task<IActionResult> RemoveWebhook(ConfigurationModel model)
        {
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<AuthorizeNetHostedPaymentSettings>(storeScope);
            return await RemoveWebHookAsync(settings);
        }


    }
}

