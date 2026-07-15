using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MWT.Tax.FixedOrByCountryStateZip.Services;
using Nop.Core;
using Nop.Core.Domain.Customers;
using MWT.Tax.FixedOrByCountryStateZip.Domain;
using MWT.Tax.FixedOrByCountryStateZip.Models;
using MWT.Tax.FixedOrByCountryStateZip.Services;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Services;

namespace MWT.Tax.FixedOrByCountryStateZip.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [AutoValidateAntiforgeryToken]
    public class MWTFixedOrByCountryStateZipController : BasePluginController
    {
        #region Fields

        private readonly TaxZarSettings _countryStateZipSettings;
        private readonly ICountryService _countryService;
        private readonly IMWTCountryStateZipService _taxRateService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IStoreService _storeService;
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IWorkContext _workContext;
        private readonly ITaxLogService _taxLogService;
        #endregion

        #region Ctor

        public MWTFixedOrByCountryStateZipController(TaxZarSettings countryStateZipSettings,
            ICountryService countryService,
            IMWTCountryStateZipService taxRateService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStateProvinceService stateProvinceService,
            IStoreService storeService,
            ITaxCategoryService taxCategoryService,
            IGenericAttributeService genericAttributeService,
            IWorkContext workContext,
            ITaxLogService taxLogService)

        {
            _countryStateZipSettings = countryStateZipSettings;
            _countryService = countryService;
            _taxRateService = taxRateService;
            _permissionService = permissionService;
            _localizationService = localizationService;
            _settingService = settingService;
            _stateProvinceService = stateProvinceService;
            _storeService = storeService;
            _taxCategoryService = taxCategoryService;
            _genericAttributeService = genericAttributeService;
            _workContext = workContext;
            _taxLogService = taxLogService;
        }

        #endregion

        #region Methods

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> Configure(bool showtour = false)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_SETTINGS))
                return Json(new { error = await _localizationService.GetResourceAsync("Admin.AccessDenied") });


            var model = new ConfigurationModel
            {
                CountryStateZipEnabled = _countryStateZipSettings.CountryStateZipEnabled,
                IncludeWgs = _countryStateZipSettings.IncludeWgs,
                ApiLink = _countryStateZipSettings.ApiLink,
                Token = _countryStateZipSettings.Token,
                ZipTaxApiLink = _countryStateZipSettings.ZipTaxApiLink,
                ZipTaxKey = _countryStateZipSettings.ZipTaxKey,
                TaxProvider = _countryStateZipSettings.TaxProvider,
                IncludedCountries = _countryStateZipSettings.IncludedCountries
            };

            //TaxProviders

            var availableProductTypeItems = await TaxProviderType.TaxZar.ToSelectListAsync(false);
            foreach (var productTypeItem in availableProductTypeItems)
            {
                model.TaxProviderTypes.Add(productTypeItem);
            }

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = "*", Value = "0" });
            var stores = await _storeService.GetAllStoresAsync();
            foreach (var s in stores)
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            //countries
            var countries = await _countryService.GetAllCountriesAsync(showHidden: true);
            foreach (var c in countries)
                model.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString() });
            //states
            model.AvailableStates.Add(new SelectListItem { Text = "*", Value = "0" });
            var defaultCountry = countries.FirstOrDefault();
            if (defaultCountry != null)
            {
                var states = await _stateProvinceService.GetStateProvincesByCountryIdAsync(defaultCountry.Id);
                foreach (var s in states)
                    model.AvailableStates.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });
            }

            //show configuration tour
            if (showtour)
            {
                var hideCard = await _genericAttributeService.GetAttributeAsync<bool>(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.HideConfigurationStepsAttribute);

                var closeCard = await _genericAttributeService.GetAttributeAsync<bool>(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.CloseConfigurationStepsAttribute);

                if (!hideCard && !closeCard)
                    ViewBag.ShowTour = true;
            }

            return View("~/Plugins/MWT.Tax.FixedOrByCountryStateZip/Views/Configure.cshtml", model);
        }
        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model, bool showtour = false)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_SETTINGS))
                return Json(new { error = await _localizationService.GetResourceAsync("Admin.AccessDenied") });

            _countryStateZipSettings.ApiLink = model.ApiLink;
            _countryStateZipSettings.Token = model.Token;
            _countryStateZipSettings.IncludeWgs = model.IncludeWgs;
            _countryStateZipSettings.ZipTaxKey = model.ZipTaxKey;
            _countryStateZipSettings.ZipTaxApiLink = model.ZipTaxApiLink;
            _countryStateZipSettings.TaxProvider = model.TaxProvider;
            _countryStateZipSettings.IncludedCountries = model.IncludedCountries;
            await _settingService.SaveSettingAsync(_countryStateZipSettings);
            var taxCategories = await _taxCategoryService.GetAllTaxCategoriesAsync();

            if (!taxCategories.Any())
            {
                var errorModel = new ConfigurationModel
                {
                    TaxCategoriesCanNotLoadedError = string.Format(
                        await _localizationService.GetResourceAsync(
                            "Plugins.MWT.Tax.FixedOrByCountryStateZip.TaxCategoriesCanNotLoaded"),
                        Url.Action("Categories", "Tax"))
                };

                return View("~/Plugins/MWT.Tax.FixedOrByCountryStateZip/Views/Configure.cshtml", errorModel);
            }

            //TaxProviders

            var availableProductTypeItems = await TaxProviderType.TaxZar.ToSelectListAsync(false);
            foreach (var productTypeItem in availableProductTypeItems)
            {
                model.TaxProviderTypes.Add(productTypeItem);
            }


            //stores
            model.AvailableStores.Add(new SelectListItem { Text = "*", Value = "0" });
            var stores = await _storeService.GetAllStoresAsync();
            foreach (var s in stores)
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });
            //tax categories
            foreach (var tc in taxCategories)
                model.AvailableTaxCategories.Add(new SelectListItem { Text = tc.Name, Value = tc.Id.ToString() });
            //countries
            var countries = await _countryService.GetAllCountriesAsync(showHidden: true);
            foreach (var c in countries)
                model.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString() });
            //states
            model.AvailableStates.Add(new SelectListItem { Text = "*", Value = "0" });
            var defaultCountry = countries.FirstOrDefault();
            if (defaultCountry != null)
            {
                var states = await _stateProvinceService.GetStateProvincesByCountryIdAsync(defaultCountry.Id);
                foreach (var s in states)
                    model.AvailableStates.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });
            }

            //show configuration tour
            if (showtour)
            {
                var hideCard = await _genericAttributeService.GetAttributeAsync<bool>(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.HideConfigurationStepsAttribute);

                var closeCard = await _genericAttributeService.GetAttributeAsync<bool>(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.CloseConfigurationStepsAttribute);

                if (!hideCard && !closeCard)
                    ViewBag.ShowTour = true;
            }

            return View("~/Plugins/MWT.Tax.FixedOrByCountryStateZip/Views/Configure.cshtml", model);
        }
        [HttpPost]
        [IgnoreAntiforgeryToken]
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> SaveMode(bool value)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_SETTINGS))
                return Json(new { error = await _localizationService.GetResourceAsync("Admin.AccessDenied") });

            //save settings
            _countryStateZipSettings.CountryStateZipEnabled = value;

            await _settingService.SaveSettingAsync(_countryStateZipSettings);

            return Json(new { Result = true });
        }

        #region Fixed tax

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> FixedRatesList(ConfigurationModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_SETTINGS))
                return  AccessDeniedView();

            var categories = (await _taxCategoryService.GetAllTaxCategoriesAsync()).ToPagedList(searchModel);

            var gridModel = await new FixedTaxRateListModel().PrepareToGridAsync(searchModel, categories, () =>
            {
                return categories.SelectAwait(async taxCategory => new FixedTaxRateModel
                {
                    TaxCategoryId = taxCategory.Id,
                    TaxCategoryName = taxCategory.Name,

                    Rate = await _settingService
                        .GetSettingByKeyAsync<decimal>(string.Format(FixedOrByCountryStateZipDefaults.FixedRateSettingsKey, taxCategory.Id))
                });
            });

            return Json(gridModel);
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> FixedRateUpdate(FixedTaxRateModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_SETTINGS))
                return Content("Access denied");

            await _settingService.SetSettingAsync(string.Format(FixedOrByCountryStateZipDefaults.FixedRateSettingsKey, model.TaxCategoryId), model.Rate);

            return new NullJsonResult();
        }

        #endregion

        #region Tax by country/state/zip

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> RatesByCountryStateZipList(ConfigurationModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_SETTINGS))
                return AccessDeniedView();

            var records = await _taxRateService.GetAllTaxRatesAsync(searchModel.Page - 1, searchModel.PageSize);

            var gridModel = await new CountryStateZipListModel().PrepareToGridAsync(searchModel, records, () =>
            {
                return records.SelectAwait(async record => new CountryStateZipModel
                {
                    Id = record.Id,
                    StoreId = record.StoreId,
                    StoreName = (await _storeService.GetStoreByIdAsync(record.StoreId))?.Name ?? "*",
                    CountryId = record.CountryId,
                    CountryName = (await _countryService.GetCountryByIdAsync(record.CountryId))?.Name ?? "Unavailable",
                    StateProvinceId = record.StateProvinceId,
                    StateProvinceName = (await _stateProvinceService.GetStateProvinceByIdAsync(record.StateProvinceId))?.Name ?? "*",
                    Zip = !string.IsNullOrEmpty(record.Zip) ? record.Zip : "*",
                    Percentage = record.Percentage,
                    GSTPercentage = record.GSTPercentage,
                    PSTPercentage = record.PSTPercentage,
                    HSTPercentage = record.HSTPercentage,
                    QSTPercentage = record.QSTPercentage
                });
            });

            return Json(gridModel);
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> AddRateByCountryStateZip(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_SETTINGS))
                return Content("Access denied");

            await _taxRateService.InsertTaxRateAsync(new MWTTaxRate
            {
                StoreId = model.AddStoreId,
                CountryId = model.AddCountryId,
                StateProvinceId = model.AddStateProvinceId,
                Zip = model.AddZip,
                Percentage = model.AddPercentage,
                GSTPercentage = model.GSTPercentage,
                HSTPercentage = model.HSTPercentage,
                QSTPercentage = model.QSTPercentage,
                PSTPercentage = model.PSTPercentage
            });

            return Json(new { Result = true });
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> UpdateRateByCountryStateZip(CountryStateZipModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_SETTINGS))
                return Content("Access denied");

            var taxRate = await _taxRateService.GetTaxRateByIdAsync(model.Id);
            taxRate.Zip = model.Zip == "*" ? null : model.Zip;
            taxRate.Percentage = model.Percentage;
            taxRate.GSTPercentage = model.GSTPercentage;
            taxRate.HSTPercentage = model.HSTPercentage;
            taxRate.QSTPercentage = model.QSTPercentage;
            taxRate.PSTPercentage = model.PSTPercentage;
            await _taxRateService.UpdateTaxRateAsync(taxRate);

            return new NullJsonResult();
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> DeleteRateByCountryStateZip(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_SETTINGS))
                return Content("Access denied");

            var taxRate = await _taxRateService.GetTaxRateByIdAsync(id);
            if (taxRate != null)
                await _taxRateService.DeleteTaxRateAsync(taxRate);

            return new NullJsonResult();
        }

        #endregion

        #region Logs

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> GetLogs(ConfigurationModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_SETTINGS))
                return Json(new { error = await _localizationService.GetResourceAsync("Admin.AccessDenied") });

            var records = await _taxLogService.Getlogs(searchModel.OrderId, searchModel.Page - 1, searchModel.PageSize);

            var gridModel = await new LogListModel().PrepareToGridAsync(searchModel, records, () =>
            {
                return records.SelectAwait(async record => new LogModel
                {
                    Id = record.Id,
                    CreatedDateUtc = record.CreatedDateUtc,
                    CustomerId = record.CustomerId,
                    OrderId = record.OrderId,
                    RequestMessage = record.RequestMessage,
                    ResponseMessage = record.ResponseMessage,
                    StatusCode = record.StatusCode,
                    Url = record.Url,
                    TaxRateInfo = record.TaxRateInfo
                });
            });

            return Json(gridModel);
        }
        #endregion

        #endregion
    }
}