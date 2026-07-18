using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MWT.Nop.Plugin.Payments.Affirm.Domain;
using MWT.Nop.Plugin.Payments.Affirm.Models;
using Nop.Core;
using Nop.Core.Domain.Stores;
using Nop.Core.Infrastructure;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Mvc.Filters;



namespace MWT.Nop.Plugin.Payments.Affirm.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    public class AffirmSettingController : BasePluginController
    {

        #region Fields

        private readonly ISettingService _settingService;

        private readonly IPermissionService _permissionService;

        private readonly ILocalizationService _localizationService;

        private readonly INotificationService _notificationService;

        private readonly IStoreContext _storeContext;

        private readonly IStoreService _storeService;

        private readonly IWebHelper _webHelper;

        private readonly ILogger _logger;

        #endregion

        #region Ctor

        public AffirmSettingController(ISettingService settingService, IPermissionService permissionService, ILocalizationService localizationService, INotificationService notificationService, IStoreContext storeContext, IStoreService storeService, IWebHelper webHelper, ILogger logger)
        {
            _settingService = settingService;
            _permissionService = permissionService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _storeContext = storeContext;
            _storeService = storeService;
            _webHelper = webHelper;
            _logger = logger;
        }

        #endregion


        public async Task<IActionResult> Configure()
        {
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var affirmCheckoutSettings = await _settingService.LoadSettingAsync<AffirmCheckoutSettings>(storeScope);
            var model = new ConfigurationModel
            {
                AdditionalFee = affirmCheckoutSettings.AdditionalFee,
                AdditionalFeePercentage = affirmCheckoutSettings.AdditionalFeePercentage,
                CountryAPIModeId = (int)affirmCheckoutSettings.CountryAPIMode,
                CountryAPIModeValues = await CountryAPIMode.Canada.ToSelectListAsync(false),
                CreateOrderModeId = (int)affirmCheckoutSettings.CreateOrderMode,
                CreateOrderModeValues = await CreateOrderMode.AfterPayment.ToSelectListAsync(false),
                EnableOnProductBox = affirmCheckoutSettings.EnableOnProductBox,
                EnableOnProductDetailsPage = affirmCheckoutSettings.EnableOnProductDetailsPage,
                EnableOnShoppingCart = affirmCheckoutSettings.EnableOnShoppingCart,
                FacingMerchantName = affirmCheckoutSettings.FacingMerchantName,
                MinimumSubTotalAmount = affirmCheckoutSettings.MinimumSubTotalAmount,
                PrivateApiKey = affirmCheckoutSettings.PrivateApiKey,
                PromotionalMessageColorId = (int)affirmCheckoutSettings.PromotionalMessageColor,
                PromotionalMessageColorValues = await PromotionalMessageColor.black.ToSelectListAsync(false),
                PromotionalMessageTypeId = (int)affirmCheckoutSettings.PromotionalMessageType,
                PromotionalMessageTypeValues = await PromotionalMessageType.text.ToSelectListAsync(false),
                PublicApiKey = affirmCheckoutSettings.PublicApiKey,
                showDebugInfo = affirmCheckoutSettings.showDebugInfo,
                SkipPaymentInfo = affirmCheckoutSettings.SkipPaymentInfo,
                StoreUrl = _storeContext.GetCurrentStore().Url,
                TransactModeId = (int)affirmCheckoutSettings.TransactMode,
                TransactModeValues = await TransactMode.AuthorizeAndCapture.ToSelectListAsync(false),
                UseSandbox = affirmCheckoutSettings.UseSandbox,
                WidgetProductBox = affirmCheckoutSettings.WidgetProductBox,
                WidgetProductDetailsPage = affirmCheckoutSettings.WidgetProductDetailsPage,
                WidgetZoneShoppingCart = affirmCheckoutSettings.WidgetZoneShoppingCart,
                ActiveStoreScopeConfiguration = storeScope
            };

            if (storeScope > 0)
            {
                model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(affirmCheckoutSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(affirmCheckoutSettings, x => x.AdditionalFeePercentage, storeScope);
                model.CountryAPIModeId_OverrideForStore = await _settingService.SettingExistsAsync(affirmCheckoutSettings, x => x.CountryAPIMode, storeScope);
                model.CreateOrderModeId_OverrideForStore = await _settingService.SettingExistsAsync(affirmCheckoutSettings, x => x.CreateOrderMode, storeScope);
                model.EnableOnProductBox_OverrideForStore = await _settingService.SettingExistsAsync(affirmCheckoutSettings, x => x.EnableOnProductBox, storeScope);
                model.EnableOnProductDetailsPage_OverrideForStore = await _settingService.SettingExistsAsync(affirmCheckoutSettings, x => x.EnableOnProductDetailsPage, storeScope);
                model.EnableOnShoppingCart_OverrideForStore = await _settingService.SettingExistsAsync(affirmCheckoutSettings, x => x.EnableOnShoppingCart, storeScope);
                model.FacingMerchantName_OverrideForStore = await _settingService.SettingExistsAsync(affirmCheckoutSettings, x => x.FacingMerchantName, storeScope);
                model.MinimumSubTotalAmount_OverrideForStore = await _settingService.SettingExistsAsync(affirmCheckoutSettings, x => x.MinimumSubTotalAmount, storeScope);
                model.PrivateApiKey_OverrideForStore = await _settingService.SettingExistsAsync(affirmCheckoutSettings, x => x.PrivateApiKey, storeScope);
                model.PromotionalMessageColorId_OverrideForStore = await _settingService.SettingExistsAsync(affirmCheckoutSettings, x => x.PromotionalMessageColor, storeScope);
                model.PromotionalMessageTypeId_OverrideForStore = await _settingService.SettingExistsAsync(affirmCheckoutSettings, x => x.PromotionalMessageType, storeScope);
                model.PublicApiKey_OverrideForStore = await _settingService.SettingExistsAsync(affirmCheckoutSettings, x => x.PublicApiKey, storeScope);
                model.SkipPaymentInfo_OverrideForStore = await _settingService.SettingExistsAsync(affirmCheckoutSettings, x => x.SkipPaymentInfo, storeScope);
                model.TransactModeId_OverrideForStore = await _settingService.SettingExistsAsync(affirmCheckoutSettings, x => x.TransactMode, storeScope);
                model.UseSandbox_OverrideForStore = await _settingService.SettingExistsAsync(affirmCheckoutSettings, x => x.UseSandbox, storeScope);
                model.WidgetProductBox_OverrideForStore = await _settingService.SettingExistsAsync(affirmCheckoutSettings, x => x.WidgetProductBox, storeScope);
                model.WidgetProductDetailsPage_OverrideForStore = await _settingService.SettingExistsAsync(affirmCheckoutSettings, x => x.WidgetProductDetailsPage, storeScope);
                model.WidgetZoneShoppingCart_OverrideForStore = await _settingService.SettingExistsAsync(affirmCheckoutSettings, x => x.WidgetZoneShoppingCart, storeScope);
            }

            model.AvailableWidgetProductBoxs.Add(new SelectListItem { Text = PublicWidgetZones.ProductBoxAddinfoAfter.ToString(), Value = PublicWidgetZones.ProductBoxAddinfoAfter.ToString() });
            model.AvailableWidgetProductBoxs.Add(new SelectListItem { Text = PublicWidgetZones.ProductBoxAddinfoBefore.ToString(), Value = PublicWidgetZones.ProductBoxAddinfoBefore.ToString() });
            model.AvailableWidgetProductBoxs.Add(new SelectListItem { Text = PublicWidgetZones.ProductBoxAddinfoMiddle.ToString(), Value = PublicWidgetZones.ProductBoxAddinfoMiddle.ToString() });


            model.AvailableWidgetProductDetailsPages.Add(new SelectListItem { Text = PublicWidgetZones.ProductDetailsAfterBreadcrumb.ToString(), Value = PublicWidgetZones.ProductBoxAddinfoAfter.ToString() });
            model.AvailableWidgetProductDetailsPages.Add(new SelectListItem { Text = PublicWidgetZones.ProductDetailsOverviewTop.ToString(), Value = PublicWidgetZones.ProductDetailsOverviewTop.ToString() });
            model.AvailableWidgetProductDetailsPages.Add(new SelectListItem { Text = PublicWidgetZones.ProductDetailsBottom.ToString(), Value = PublicWidgetZones.ProductDetailsBottom.ToString() });


            model.AvailableWidgetZoneShoppingCarts.Add(new SelectListItem { Text = PublicWidgetZones.OrderSummaryCartFooter.ToString(), Value = PublicWidgetZones.OrderSummaryCartFooter.ToString() });


            return View("~/Plugins/MWT.Nop.Plugin.Payments.Affirm/Views/Configure.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var affirmCheckoutSettings = await _settingService.LoadSettingAsync<AffirmCheckoutSettings>(storeScope);
            affirmCheckoutSettings.AdditionalFee = model.AdditionalFee;
            affirmCheckoutSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            affirmCheckoutSettings.CountryAPIMode = (CountryAPIMode)model.CountryAPIModeId;
            affirmCheckoutSettings.CreateOrderMode = (CreateOrderMode)model.CreateOrderModeId;
            affirmCheckoutSettings.EnableOnProductBox = model.EnableOnProductBox;
            affirmCheckoutSettings.EnableOnProductDetailsPage = model.EnableOnProductDetailsPage;
            affirmCheckoutSettings.EnableOnShoppingCart = model.EnableOnShoppingCart;
            affirmCheckoutSettings.FacingMerchantName = model.FacingMerchantName;
            affirmCheckoutSettings.MinimumSubTotalAmount = model.MinimumSubTotalAmount;
            affirmCheckoutSettings.PrivateApiKey = model.PrivateApiKey;
            affirmCheckoutSettings.PromotionalMessageColor = (PromotionalMessageColor)model.PromotionalMessageColorId;
            affirmCheckoutSettings.PromotionalMessageType = (PromotionalMessageType)model.PromotionalMessageTypeId;
            affirmCheckoutSettings.PublicApiKey = model.PublicApiKey;
            affirmCheckoutSettings.showDebugInfo = model.showDebugInfo;
            affirmCheckoutSettings.SkipPaymentInfo = model.SkipPaymentInfo;
            affirmCheckoutSettings.TransactMode= (TransactMode)model.TransactModeId;
            affirmCheckoutSettings.UseSandbox = model.UseSandbox;
            affirmCheckoutSettings.WidgetProductBox = model.WidgetProductBox;
            affirmCheckoutSettings.WidgetProductDetailsPage = model.WidgetProductDetailsPage;
            affirmCheckoutSettings.WidgetZoneShoppingCart = model.WidgetZoneShoppingCart;
            await _settingService.SaveSettingAsync(affirmCheckoutSettings);

            await _settingService.SaveSettingOverridablePerStoreAsync(affirmCheckoutSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(affirmCheckoutSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(affirmCheckoutSettings, x => x.CountryAPIMode, model.CountryAPIModeId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(affirmCheckoutSettings, x => x.CreateOrderMode, model.CreateOrderModeId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(affirmCheckoutSettings, x => x.EnableOnProductBox, model.EnableOnProductBox_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(affirmCheckoutSettings, x => x.EnableOnProductDetailsPage, model.EnableOnProductDetailsPage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(affirmCheckoutSettings, x => x.EnableOnShoppingCart, model.EnableOnShoppingCart_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(affirmCheckoutSettings, x => x.FacingMerchantName, model.FacingMerchantName_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(affirmCheckoutSettings, x => x.MinimumSubTotalAmount, model.MinimumSubTotalAmount_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(affirmCheckoutSettings, x => x.PrivateApiKey, model.PrivateApiKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(affirmCheckoutSettings, x => x.PromotionalMessageColor, model.PromotionalMessageColorId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(affirmCheckoutSettings, x => x.PromotionalMessageType, model.PromotionalMessageTypeId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(affirmCheckoutSettings, x => x.PublicApiKey, model.PublicApiKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(affirmCheckoutSettings, x => x.SkipPaymentInfo, model.SkipPaymentInfo_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(affirmCheckoutSettings, x => x.TransactMode, model.TransactModeId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(affirmCheckoutSettings, x => x.UseSandbox, model.UseSandbox_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(affirmCheckoutSettings, x => x.WidgetProductBox, model.WidgetProductBox_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(affirmCheckoutSettings, x => x.WidgetProductDetailsPage, model.WidgetProductDetailsPage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(affirmCheckoutSettings, x => x.WidgetZoneShoppingCart, model.WidgetZoneShoppingCart_OverrideForStore, storeScope, false);
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }
    }
}
