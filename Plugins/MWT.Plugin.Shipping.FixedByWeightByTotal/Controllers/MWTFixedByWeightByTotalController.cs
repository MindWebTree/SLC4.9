using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using MWT.Plugin.Shipping.FixedByWeightByTotal.Domain;
using MWT.Plugin.Shipping.FixedByWeightByTotal.Models;
using MWT.Plugin.Shipping.FixedByWeightByTotal.Services;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Areas.Admin.Factories;
using Nop.Services.Catalog;

namespace MWT.Plugin.Shipping.FixedByWeightByTotal.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [AutoValidateAntiforgeryToken]
    public class MWTFixedByWeightByTotalController : BasePluginController
    {
        #region Fields

        private readonly CurrencySettings _currencySettings;
        private readonly FixedByWeightByTotalSettings _fixedByWeightByTotalSettings;
        private readonly ICountryService _countryService;
        private readonly ICurrencyService _currencyService;
        private readonly ILocalizationService _localizationService;
        private readonly IMeasureService _measureService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IMWTShippingByWeightByTotalService _shippingByWeightService;
        private readonly IMWTShippingZoneService _mwtShippingZoneService;
        private readonly IShippingService _shippingService;
        private readonly IWarehouseService _warehouseService;
        private readonly IShippingMethodsService _shippingMethodsService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IStoreService _storeService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IWorkContext _workContext;
        private readonly MeasureSettings _measureSettings;
        private readonly IMWTExpectedDeliveryDateService _mWTExpectedDeliveryDateService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IProductService _productService;

        #endregion

        #region Ctor

        public MWTFixedByWeightByTotalController(CurrencySettings currencySettings,
            FixedByWeightByTotalSettings fixedByWeightByTotalSettings,
            ICountryService countryService,
            ICurrencyService currencyService,
            ILocalizationService localizationService,
            IMeasureService measureService,
            IPermissionService permissionService,
            ISettingService settingService,
            IMWTShippingByWeightByTotalService shippingByWeightService,
            IShippingService shippingService,
            IStateProvinceService stateProvinceService,
            IStoreService storeService,
            IGenericAttributeService genericAttributeService,
            IWorkContext workContext,
            MeasureSettings measureSettings,
            IMWTShippingZoneService mwtShippingZoneService,
            IBaseAdminModelFactory baseAdminModelFactory,
            IProductService productService,
            IMWTExpectedDeliveryDateService mWTExpectedDeliveryDateService,
            IWarehouseService warehouseService,
            IShippingMethodsService shippingMethodsService)
        {
            _currencySettings = currencySettings;
            _fixedByWeightByTotalSettings = fixedByWeightByTotalSettings;
            _countryService = countryService;
            _currencyService = currencyService;
            _localizationService = localizationService;
            _measureService = measureService;
            _permissionService = permissionService;
            _settingService = settingService;
            _shippingByWeightService = shippingByWeightService;
            _stateProvinceService = stateProvinceService;
            _shippingService = shippingService;
            _storeService = storeService;
            _genericAttributeService = genericAttributeService;
            _workContext = workContext;
            _measureSettings = measureSettings;
            _mwtShippingZoneService = mwtShippingZoneService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _productService = productService;
            _mWTExpectedDeliveryDateService = mWTExpectedDeliveryDateService;
            _warehouseService = warehouseService;
            _shippingMethodsService = shippingMethodsService;

        }

        #endregion

        #region Methods

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> Configure(bool showtour = false)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_SHIPPING_SETTINGS))
                return AccessDeniedView();

            var model = new ConfigurationModel
            {
                LimitMethodsToCreated = _fixedByWeightByTotalSettings.LimitMethodsToCreated,
                ShippingByWeightByTotalEnabled = _fixedByWeightByTotalSettings.ShippingByWeightByTotalEnabled
            };

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = "*", Value = "0" });
            foreach (var store in await _storeService.GetAllStoresAsync())
                model.AvailableStores.Add(new SelectListItem { Text = store.Name, Value = store.Id.ToString() });
            //warehouses
            model.AvailableWarehouses.Add(new SelectListItem { Text = "*", Value = "0" });
            foreach (var warehouses in await _warehouseService.GetAllWarehousesAsync())
                    model.AvailableWarehouses.Add(new SelectListItem { Text = warehouses.Name, Value = warehouses.Id.ToString() });
            //shipping methods
            foreach (var sm in await _shippingMethodsService.GetAllShippingMethodsAsync())
                model.AvailableShippingMethods.Add(new SelectListItem { Text = sm.Name, Value = sm.Id.ToString() });
            //countries
            model.AvailableCountries.Add(new SelectListItem { Text = "*", Value = "0" });
            var countries = await _countryService.GetAllCountriesAsync();
            foreach (var c in countries)
                model.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString() });
            //states
            model.AvailableStates.Add(new SelectListItem { Text = "*", Value = "0" });

            model.SetGridPageSize();

            //show configuration tour
            if (showtour)
            {
                var hideCard = await _genericAttributeService.GetAttributeAsync<bool>(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.HideConfigurationStepsAttribute);

                var closeCard = await _genericAttributeService.GetAttributeAsync<bool>(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.CloseConfigurationStepsAttribute);

                if (!hideCard && !closeCard)
                    ViewBag.ShowTour = true;
            }

            return View("~/Plugins/MWT.Shipping.FixedByWeightByTotal/Views/Configure.cshtml", model);
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_SHIPPING_SETTINGS))
                return Content("Access denied");

            //save settings
            _fixedByWeightByTotalSettings.LimitMethodsToCreated = model.LimitMethodsToCreated;
            await _settingService.SaveSettingAsync(_fixedByWeightByTotalSettings);

            return Json(new { Result = true });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> SaveMode(bool value)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_SHIPPING_SETTINGS))
                return Content("Access denied");

            //save settings
            _fixedByWeightByTotalSettings.ShippingByWeightByTotalEnabled = value;
            await _settingService.SaveSettingAsync(_fixedByWeightByTotalSettings);

            return Json(new { Result = true });
        }

        #region Fixed rate

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> FixedShippingRateList(ConfigurationModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_SHIPPING_SETTINGS))
                return await AccessDeniedJsonAsync();

            var shippingMethods = (await _shippingMethodsService.GetAllShippingMethodsAsync()).ToPagedList(searchModel);

            var gridModel = await new FixedRateListModel().PrepareToGridAsync(searchModel, shippingMethods, () =>
            {
                return shippingMethods.SelectAwait(async shippingMethod => new FixedRateModel
                {
                    ShippingMethodId = shippingMethod.Id,
                    ShippingMethodName = shippingMethod.Name,

                    Rate = await _settingService
                        .GetSettingByKeyAsync<decimal>(string.Format(FixedByWeightByTotalDefaults.FixedRateSettingsKey, shippingMethod.Id)),
                    TransitDays = await _settingService
                        .GetSettingByKeyAsync<int?>(string.Format(FixedByWeightByTotalDefaults.TransitDaysSettingsKey, shippingMethod.Id))
                });
            });

            return Json(gridModel);
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> UpdateFixedShippingRate(FixedRateModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_SHIPPING_SETTINGS))
                return Content("Access denied");

            await _settingService.SetSettingAsync(string.Format(FixedByWeightByTotalDefaults.FixedRateSettingsKey, model.ShippingMethodId), model.Rate, 0, false);
            await _settingService.SetSettingAsync(string.Format(FixedByWeightByTotalDefaults.TransitDaysSettingsKey, model.ShippingMethodId), model.TransitDays, 0, false);

            await _settingService.ClearCacheAsync();

            return new NullJsonResult();
        }

        #endregion

        #region Rate by weight

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> RateByWeightByTotalList(ConfigurationModel searchModel, ConfigurationModel filter)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_SHIPPING_SETTINGS))
                return await AccessDeniedJsonAsync();

            //var records = _shippingByWeightService.GetAll(command.Page - 1, command.PageSize);
            var records = await _shippingByWeightService.FindRecordsBackendAsync(
              pageIndex: searchModel.Page - 1,
              pageSize: searchModel.PageSize,
              storeId: filter.SearchStoreId,
              warehouseId: filter.SearchWarehouseId,
              countryId: filter.SearchCountryId,
              stateProvinceId: filter.SearchStateProvinceId,
              zip: filter.SearchZip,
              shippingMethodId: filter.SearchShippingMethodId,
              weight: null,
              orderSubtotal: null
              );

            var gridModel = await new MWTShippingByWeightByTotalListModel().PrepareToGridAsync(searchModel, records, () =>
            {
                return records.SelectAwait(async record =>
                {
                    var model = new MWTShippingByWeightByTotalModel
                    {
                        Id = record.Id,
                        StoreId = record.StoreId,
                        StoreName = (await _storeService.GetStoreByIdAsync(record.StoreId))?.Name ?? "*",
                        WarehouseId = record.WarehouseId,
                        WarehouseName = (await _warehouseService.GetWarehouseByIdAsync(record.WarehouseId))?.Name ?? "*",
                        ShippingMethodId = record.ShippingMethodId,
                        ShippingMethodName = (await _shippingMethodsService.GetShippingMethodByIdAsync(record.ShippingMethodId))?.Name ?? "Unavailable",
                        CountryId = record.CountryId,
                        CountryName = (await _countryService.GetCountryByIdAsync(record.CountryId))?.Name ?? "*",
                        StateProvinceId = record.StateProvinceId,
                        StateProvinceName = (await _stateProvinceService.GetStateProvinceByIdAsync(record.StateProvinceId))?.Name ?? "*",
                        WeightFrom = record.WeightFrom,
                        WeightTo = record.WeightTo,
                        OrderSubtotalFrom = record.OrderSubtotalFrom,
                        OrderSubtotalTo = record.OrderSubtotalTo,
                        AdditionalFixedCost = record.AdditionalFixedCost,
                        Amount = record.Amount,
                        Surcharge = record.Surcharge,
                        PercentageRateOfSubtotal =record.PercentageRateOfSubtotal,
                        RatePerWeightUnit = record.RatePerWeightUnit,
                        LowerWeightLimit = record.LowerWeightLimit,
                        ZoneId = record.ZoneId,
                        ZoneName = record.ZoneId == 0 ? "" : (await _mwtShippingZoneService.GetByIdAsync(record.ZoneId))?.Name
                    };

                    var htmlSb = new StringBuilder("<div>");


                    htmlSb.AppendFormat(model.OrderSubtotalFrom + " - " + model.OrderSubtotalTo);
                    htmlSb.Append("<br />");
                    htmlSb.AppendFormat("{0}: {1}",
                        await _localizationService.GetResourceAsync("MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.AdditionalFixedCost"),
                        model.AdditionalFixedCost);
                    htmlSb.Append("<br />");

                    htmlSb.AppendFormat("{0}: {1}",
                            await _localizationService.GetResourceAsync("MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.Amount"),
                            model.Amount);
                    htmlSb.Append("<br />");
                    htmlSb.AppendFormat("{0}: {1}",
                         await _localizationService.GetResourceAsync("MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.Surcharge"),
                         model.Surcharge);
                    htmlSb.Append("</div>");
                    model.DataHtml = htmlSb.ToString();

                    return model;
                });
            });

            return Json(gridModel);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> AddRateByWeightByTotalPopup()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_SHIPPING_SETTINGS))
                return AccessDeniedView();

            var model = new MWTShippingByWeightByTotalModel
            {
                PrimaryStoreCurrencyCode = (await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId))?.CurrencyCode,
                BaseWeightIn = (await _measureService.GetMeasureWeightByIdAsync(_measureSettings.BaseWeightId))?.Name,
                WeightTo = 1000000,
                OrderSubtotalTo = 1000000
            };

            var shippingMethods = await _shippingMethodsService.GetAllShippingMethodsAsync();
            if (!shippingMethods.Any())
                return Content("No shipping methods can be loaded");

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = "*", Value = "0" });
            foreach (var store in await _storeService.GetAllStoresAsync())
                model.AvailableStores.Add(new SelectListItem { Text = store.Name, Value = store.Id.ToString() });
            //warehouses
            model.AvailableWarehouses.Add(new SelectListItem { Text = "*", Value = "0" });
            foreach (var warehouses in await _warehouseService.GetAllWarehousesAsync())
                model.AvailableWarehouses.Add(new SelectListItem { Text = warehouses.Name, Value = warehouses.Id.ToString() });
            //shipping methods
            foreach (var sm in shippingMethods)
                model.AvailableShippingMethods.Add(new SelectListItem { Text = sm.Name, Value = sm.Id.ToString() });
            //countries
            model.AvailableCountries.Add(new SelectListItem { Text = "*", Value = "0" });
            var countries = await _countryService.GetAllCountriesAsync(showHidden: true);
            foreach (var c in countries)
                model.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString() });
            //states
            model.AvailableStates.Add(new SelectListItem { Text = "*", Value = "0" });

            // Zones

            model.AvailableZones.Add(new SelectListItem { Text = "Please select Zone", Value = "" });
            var zones = await _mwtShippingZoneService.GetZones(0, 100);
            foreach (var zone in zones)
                model.AvailableZones.Add(new SelectListItem { Text = zone.Name, Value = zone.Id.ToString() });

            return View("~/Plugins/MWT.Shipping.FixedByWeightByTotal/Views/AddRateByWeightByTotalPopup.cshtml", model);
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> AddRateByWeightByTotalPopup(MWTShippingByWeightByTotalModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_SHIPPING_SETTINGS))
                return AccessDeniedView();
            if (ModelState.IsValid)
            {
                await _shippingByWeightService.InsertShippingByWeightRecordAsync(new MWTShippingByWeightByTotalRecord
                {
                    StoreId = model.StoreId,
                    WarehouseId = model.WarehouseId,
                    CountryId = model.CountryId,
                    StateProvinceId = model.StateProvinceId,
                    ZoneId = model.ZoneId,
                    ShippingMethodId = model.ShippingMethodId,
                    WeightFrom = model.WeightFrom,
                    WeightTo = model.WeightTo,
                    OrderSubtotalFrom = model.OrderSubtotalFrom,
                    OrderSubtotalTo = model.OrderSubtotalTo,
                    AdditionalFixedCost = model.AdditionalFixedCost,
                    RatePerWeightUnit = model.RatePerWeightUnit,
                    Amount = model.Amount,
                    Surcharge = model.Surcharge,
                    PercentageRateOfSubtotal =model.PercentageRateOfSubtotal,
                    LowerWeightLimit = model.LowerWeightLimit,
                    TransitDays = model.TransitDays
                });

                ViewBag.RefreshPage = true;

                return View("~/Plugins/MWT.Shipping.FixedByWeightByTotal/Views/AddRateByWeightByTotalPopup.cshtml", model);
            }
            else
            {
                var shippingMethods = await _shippingMethodsService.GetAllShippingMethodsAsync();
                //stores
                model.AvailableStores.Add(new SelectListItem { Text = "*", Value = "0" });
                foreach (var store in await _storeService.GetAllStoresAsync())
                    model.AvailableStores.Add(new SelectListItem { Text = store.Name, Value = store.Id.ToString() });
                //warehouses
                model.AvailableWarehouses.Add(new SelectListItem { Text = "*", Value = "0" });
                foreach (var warehouses in await _warehouseService.GetAllWarehousesAsync())
                    model.AvailableWarehouses.Add(new SelectListItem { Text = warehouses.Name, Value = warehouses.Id.ToString() });
                //shipping methods
                foreach (var sm in shippingMethods)
                    model.AvailableShippingMethods.Add(new SelectListItem { Text = sm.Name, Value = sm.Id.ToString() });
                //countries
                model.AvailableCountries.Add(new SelectListItem { Text = "*", Value = "0" });
                var countries = await _countryService.GetAllCountriesAsync(showHidden: true);
                foreach (var c in countries)
                    model.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString() });
                //states
                model.AvailableStates.Add(new SelectListItem { Text = "*", Value = "0" });

                // Zones

                model.AvailableZones.Add(new SelectListItem { Text = "Please select Zone", Value = "" });
                var zones = await _mwtShippingZoneService.GetZones(0, 100);
                foreach (var zone in zones)
                    model.AvailableZones.Add(new SelectListItem { Text = zone.Name, Value = zone.Id.ToString() });

                return View("~/Plugins/MWT.Shipping.FixedByWeightByTotal/Views/AddRateByWeightByTotalPopup.cshtml", model);
            }
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> EditRateByWeightByTotalPopup(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_SHIPPING_SETTINGS))
                return AccessDeniedView();

            var sbw = await _shippingByWeightService.GetByIdAsync(id);
            if (sbw == null)
                //no record found with the specified id
                return RedirectToAction("Configure");

            var model = new MWTShippingByWeightByTotalModel
            {
                Id = sbw.Id,
                StoreId = sbw.StoreId,
                WarehouseId = sbw.WarehouseId,
                CountryId = sbw.CountryId,
                StateProvinceId = sbw.StateProvinceId,
                ZoneId = sbw.ZoneId,
                ShippingMethodId = sbw.ShippingMethodId,
                WeightFrom = sbw.WeightFrom,
                WeightTo = sbw.WeightTo,
                OrderSubtotalFrom = sbw.OrderSubtotalFrom,
                OrderSubtotalTo = sbw.OrderSubtotalTo,
                AdditionalFixedCost = sbw.AdditionalFixedCost,
                Amount = sbw.Amount,
                Surcharge = sbw.Surcharge,
                PercentageRateOfSubtotal = sbw.PercentageRateOfSubtotal,
                RatePerWeightUnit = sbw.RatePerWeightUnit,
                LowerWeightLimit = sbw.LowerWeightLimit,
                PrimaryStoreCurrencyCode = (await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId))?.CurrencyCode,
                BaseWeightIn = (await _measureService.GetMeasureWeightByIdAsync(_measureSettings.BaseWeightId))?.Name,
                TransitDays = sbw.TransitDays
            };

            var shippingMethods = await _shippingMethodsService.GetAllShippingMethodsAsync();
            if (!shippingMethods.Any())
                return Content("No shipping methods can be loaded");

            var selectedStore = await _storeService.GetStoreByIdAsync(sbw.StoreId);
            var selectedWarehouse = await _warehouseService.GetWarehouseByIdAsync(sbw.WarehouseId);
            var selectedShippingMethod = await _shippingMethodsService.GetShippingMethodByIdAsync(sbw.ShippingMethodId);
            var selectedCountry = await _countryService.GetCountryByIdAsync(sbw.CountryId);
            var selectedState = await _stateProvinceService.GetStateProvinceByIdAsync(sbw.StateProvinceId);
            //stores
            model.AvailableStores.Add(new SelectListItem { Text = "*", Value = "0" });
            foreach (var store in await _storeService.GetAllStoresAsync())
                model.AvailableStores.Add(new SelectListItem { Text = store.Name, Value = store.Id.ToString(), Selected = (selectedStore != null && store.Id == selectedStore.Id) });
            //warehouses
            model.AvailableWarehouses.Add(new SelectListItem { Text = "*", Value = "0" });
            foreach (var warehouse in await _warehouseService.GetAllWarehousesAsync())
                model.AvailableWarehouses.Add(new SelectListItem { Text = warehouse.Name, Value = warehouse.Id.ToString(), Selected = (selectedWarehouse != null && warehouse.Id == selectedWarehouse.Id) });
            //shipping methods
            foreach (var sm in shippingMethods)
                model.AvailableShippingMethods.Add(new SelectListItem { Text = sm.Name, Value = sm.Id.ToString(), Selected = (selectedShippingMethod != null && sm.Id == selectedShippingMethod.Id) });
            //countries
            model.AvailableCountries.Add(new SelectListItem { Text = "*", Value = "0" });
            var countries = await _countryService.GetAllCountriesAsync(showHidden: true);
            foreach (var c in countries)
                model.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString(), Selected = (selectedCountry != null && c.Id == selectedCountry.Id) });
            //states
            var states = selectedCountry != null ? (await _stateProvinceService.GetStateProvincesByCountryIdAsync(selectedCountry.Id, showHidden: true)).ToList() : new List<StateProvince>();
            model.AvailableStates.Add(new SelectListItem { Text = "*", Value = "0" });
            foreach (var s in states)
                model.AvailableStates.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString(), Selected = (selectedState != null && s.Id == selectedState.Id) });

            model.AvailableZones.Add(new SelectListItem { Text = "Please select Zone", Value = "" });
            var zones = await _mwtShippingZoneService.GetZones(0, 100);
            foreach (var zone in zones)
                model.AvailableZones.Add(new SelectListItem { Text = zone.Name, Value = zone.Id.ToString(), Selected = (selectedState != null && zone.Id == model.ZoneId) });
            return View("~/Plugins/MWT.Shipping.FixedByWeightByTotal/Views/EditRateByWeightByTotalPopup.cshtml", model);
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> EditRateByWeightByTotalPopup(MWTShippingByWeightByTotalModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_SHIPPING_SETTINGS))
                return AccessDeniedView();

            var sbw = await _shippingByWeightService.GetByIdAsync(model.Id);
            if (sbw == null)
                //no record found with the specified id
                return RedirectToAction("Configure");
            if (ModelState.IsValid)
            {
                sbw.StoreId = model.StoreId;
                sbw.WarehouseId = model.WarehouseId;
                sbw.CountryId = model.CountryId;
                sbw.StateProvinceId = model.StateProvinceId;
                sbw.ZoneId = model.ZoneId;
                sbw.ShippingMethodId = model.ShippingMethodId;
                sbw.WeightFrom = model.WeightFrom;
                sbw.WeightTo = model.WeightTo;
                sbw.OrderSubtotalFrom = model.OrderSubtotalFrom;
                sbw.OrderSubtotalTo = model.OrderSubtotalTo;
                sbw.AdditionalFixedCost = model.AdditionalFixedCost;
                sbw.RatePerWeightUnit = model.RatePerWeightUnit;
                sbw.Amount = model.Amount;
                sbw.Surcharge = model.Surcharge;
                sbw.PercentageRateOfSubtotal = model.PercentageRateOfSubtotal;
                sbw.LowerWeightLimit = model.LowerWeightLimit;
                sbw.TransitDays = model.TransitDays;

                await _shippingByWeightService.UpdateShippingByWeightRecordAsync(sbw);

                ViewBag.RefreshPage = true;

                return View("~/Plugins/MWT.Shipping.FixedByWeightByTotal/Views/EditRateByWeightByTotalPopup.cshtml", model);
            }
            else
            {
                var shippingMethods = await _shippingMethodsService.GetAllShippingMethodsAsync();
                //stores
                model.AvailableStores.Add(new SelectListItem { Text = "*", Value = "0" });
                foreach (var store in await _storeService.GetAllStoresAsync())
                    model.AvailableStores.Add(new SelectListItem { Text = store.Name, Value = store.Id.ToString() });
                //warehouses
                model.AvailableWarehouses.Add(new SelectListItem { Text = "*", Value = "0" });
                foreach (var warehouses in await _warehouseService.GetAllWarehousesAsync())
                    model.AvailableWarehouses.Add(new SelectListItem { Text = warehouses.Name, Value = warehouses.Id.ToString() });
                //shipping methods
                foreach (var sm in shippingMethods)
                    model.AvailableShippingMethods.Add(new SelectListItem { Text = sm.Name, Value = sm.Id.ToString() });
                //countries
                model.AvailableCountries.Add(new SelectListItem { Text = "*", Value = "0" });
                var countries = await _countryService.GetAllCountriesAsync(showHidden: true);
                foreach (var c in countries)
                    model.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString() });
                //states
                model.AvailableStates.Add(new SelectListItem { Text = "*", Value = "0" });

                // Zones

                model.AvailableZones.Add(new SelectListItem { Text = "Please select Zone", Value = "" });
                var zones = await _mwtShippingZoneService.GetZones(0, 100);
                foreach (var zone in zones)
                    model.AvailableZones.Add(new SelectListItem { Text = zone.Name, Value = zone.Id.ToString() });

                return View("~/Plugins/MWT.Shipping.FixedByWeightByTotal/Views/EditRateByWeightByTotalPopup.cshtml", model);
            }
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> DeleteRateByWeightByTotal(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_SHIPPING_SETTINGS))
                return Content("Access denied");

            var sbw = await _shippingByWeightService.GetByIdAsync(id);
            if (sbw != null)
                await _shippingByWeightService.DeleteShippingByWeightRecordAsync(sbw);

            return new NullJsonResult();
        }

        #endregion


        #region Shipping Zone



        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> ShippingZones(ConfigurationModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_SHIPPING_SETTINGS))
              return await AccessDeniedJsonAsync();

            //var records = _shippingByWeightService.GetAll(command.Page - 1, command.PageSize);
            var records = await _mwtShippingZoneService.GetZones(
              pageIndex: searchModel.Page - 1,
              pageSize: searchModel.PageSize
              );

            var gridModel = await new MWTShippingZoneListModel().PrepareToGridAsync(searchModel, records, () =>
            {
                return records.SelectAwait(async record =>
                {
                    var model = new MWTShippingZoneModel
                    {
                        Id = record.Id,
                        StoreId = record.StoreId,
                        StoreName = (await _storeService.GetStoreByIdAsync(record.StoreId))?.Name ?? "*",
                        Name = record.Name,
                        ZipCodes = record.ZipCodes
                    };

                    return model;
                });
            });

            return Json(gridModel);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> AddShippingZonePopup()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_SHIPPING_SETTINGS))
                return AccessDeniedView();

            var model = new MWTShippingZoneModel();

            return View("~/Plugins/MWT.Shipping.FixedByWeightByTotal/Views/AddShippingZone.cshtml", model);
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> AddShippingZonePopup(MWTShippingZoneModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_SHIPPING_SETTINGS))
                return AccessDeniedView();
            if (ModelState.IsValid)
            {
                await _mwtShippingZoneService.Insert(new MWTShippingZone
                {
                    StoreId = model.StoreId,
                    Name = model.Name,
                    ZipCodes = model.ZipCodes
                });

                ViewBag.RefreshPage = true;

                return View("~/Plugins/MWT.Shipping.FixedByWeightByTotal/Views/AddShippingZone.cshtml", model);
            }
            else
                return View("~/Plugins/MWT.Shipping.FixedByWeightByTotal/Views/AddShippingZone.cshtml", model);

        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> EditShippingZonePopup(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_SHIPPING_SETTINGS))
                return AccessDeniedView();

            var sbw = await _mwtShippingZoneService.GetByIdAsync(id);
            if (sbw == null)
                //no record found with the specified id
                return RedirectToAction("Configure");

            var model = new MWTShippingZoneModel
            {
                Id = sbw.Id,
                StoreId = sbw.StoreId,
                Name = sbw.Name,
                ZipCodes = sbw.ZipCodes
            };
            return View("~/Plugins/MWT.Shipping.FixedByWeightByTotal/Views/EditShippingZone.cshtml", model);
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> EditShippingZonePopup(MWTShippingZoneModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_SHIPPING_SETTINGS))
                return AccessDeniedView();
            if (ModelState.IsValid)
            {
                var sbw = await _mwtShippingZoneService.GetByIdAsync(model.Id);
                if (sbw == null)
                    //no record found with the specified id
                    return RedirectToAction("Configure");

                sbw.StoreId = model.StoreId;
                sbw.Name = model.Name;
                sbw.ZipCodes = model.ZipCodes;

                await _mwtShippingZoneService.Update(sbw);

                ViewBag.RefreshPage = true;

                return View("~/Plugins/MWT.Shipping.FixedByWeightByTotal/Views/EditShippingZone.cshtml", model);
            }
            else
                return View("~/Plugins/MWT.Shipping.FixedByWeightByTotal/Views/EditShippingZone.cshtml", model);

        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> DeleteShippingZone(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_SHIPPING_SETTINGS))
                return Content("Access denied");

            var sbw = await _mwtShippingZoneService.GetByIdAsync(id);
            if (sbw != null)
                await _mwtShippingZoneService.Delete(sbw);

            return new NullJsonResult();
        }

        #endregion

        #region MWTExpectedDeliveryDate 

        public async Task<IActionResult> ManageExpectedDeliveryDates(ConfigurationModel searchModel)
        {
            return View("~/Plugins/MWT.Shipping.FixedByWeightByTotal/Views/ManageExpectedDeliveryDates.cshtml", new ConfigurationModel());
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> ExpectedDeliveryDates(ConfigurationModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_SHIPPING_SETTINGS))
                return await AccessDeniedJsonAsync();


            var records = await _mWTExpectedDeliveryDateService.GetMWTExpectedDeliveryDateList(
              pageIndex: searchModel.Page - 1,
              pageSize: searchModel.PageSize
              );

            var gridModel = await new MWTExpectedDeliveryDateListModel().PrepareToGridAsync(searchModel, records, () =>
            {
                return records.SelectAwait(async record =>
                {
                    var model = new MWTExpectedDeliveryDateModel
                    {
                        Id = record.Id,
                        ExpectedMaxNoOfDays = record.ExpectedMaxNoOfDays,
                        ExpectedMinNoOfDays = record.ExpectedMinNoOfDays,
                        ZoneName = record.ZoneID == 0 ? "" : (await _mwtShippingZoneService.GetByIdAsync(record.ZoneID))?.Name,
                        ProductIDs = string.Join(',', (await _mWTExpectedDeliveryDateService.GetEntityMappingsByExpectedDeliveryDateIDAndTypeAsync(record.Id, "Product")).Select(m=>m.EntityID)),
                        CategoryIds = string.Join(',', (await _mWTExpectedDeliveryDateService.GetEntityMappingsByExpectedDeliveryDateIDAndTypeAsync(record.Id, "Category")).Select(m => m.EntityID))
                    };

                    return model;
                });
            });

            return Json(gridModel);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> AddExpectedDeliveryDatePopup()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_SHIPPING_SETTINGS))
                return AccessDeniedView();

            var model = new MWTExpectedDeliveryDateModel();
            await _baseAdminModelFactory.PrepareCategoriesAsync(model.AvailableCategories);
            // Zones

            model.AvailableZones.Add(new SelectListItem { Text = "Please select Zone", Value = "" });
            var zones = await _mwtShippingZoneService.GetZones(0, 100);
            foreach (var zone in zones)
                model.AvailableZones.Add(new SelectListItem { Text = zone.Name, Value = zone.Id.ToString() });
            return View("~/Plugins/MWT.Shipping.FixedByWeightByTotal/Views/AddExpectedDeliveryDate.cshtml", model);
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> AddExpectedDeliveryDatePopup(MWTExpectedDeliveryDateModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_SHIPPING_SETTINGS))
                return AccessDeniedView();
            if (ModelState.IsValid)
            {
                if ((string.IsNullOrEmpty(model.ProductIDs) || model.ProductIDs.Trim() == "") && model.SelectedCategoryIds.Count == 0)
                {
                    ModelState.AddModelError("", "Please specifiy either categories or products");
                    await _baseAdminModelFactory.PrepareCategoriesAsync(model.AvailableCategories);
                    // Zones

                    model.AvailableZones.Add(new SelectListItem { Text = "Please select Zone", Value = "" });
                    var zones = await _mwtShippingZoneService.GetZones(0, 100);
                    foreach (var zone in zones)
                        model.AvailableZones.Add(new SelectListItem { Text = zone.Name, Value = zone.Id.ToString() });
                    return View("~/Plugins/MWT.Shipping.FixedByWeightByTotal/Views/AddExpectedDeliveryDate.cshtml", model);
                }
                else
                {
                    var mWTExpectedDeliveryDate = new MWTExpectedDeliveryDate
                    {
                        ExpectedMinNoOfDays = model.ExpectedMinNoOfDays,
                        ExpectedMaxNoOfDays = model.ExpectedMaxNoOfDays ,
                        ZoneID = model.ZoneID
                    };

                    await _mWTExpectedDeliveryDateService.Insert(mWTExpectedDeliveryDate);
                    // Add Mappings
                    model.SelectedCategoryIds = model.SelectedCategoryIds.Select(c => c).Distinct().ToList();
                    foreach (var categoryId in model.SelectedCategoryIds)
                    {
                        await _mWTExpectedDeliveryDateService.InsertEntityMapping(new MWTExpectedDeliveryDate_Entity_Mapping()
                        {
                            EntityID = categoryId,
                            EntityType = "Category",
                            MWTExpectedDeliveryDateID = mWTExpectedDeliveryDate.Id
                        });
                    }

                    List<int> lstProdutdIds = new List<int>();
                    if (!string.IsNullOrEmpty(model.ProductIDs) && model.ProductIDs.Trim() != "")
                    {
                        foreach (var _productId in model.ProductIDs.Split(','))
                        {
                            int.TryParse(_productId, out int productId);
                            if (productId != 0)
                            {
                                if (!lstProdutdIds.Where(p => p == productId).Any())
                                {
                                    var product = await _productService.GetProductByIdAsync(productId);
                                    if (product != null)
                                    {
                                        lstProdutdIds.Add(productId);
                                        await _mWTExpectedDeliveryDateService.InsertEntityMapping(new MWTExpectedDeliveryDate_Entity_Mapping()
                                        {
                                            EntityID = productId,
                                            EntityType = "Product",
                                            MWTExpectedDeliveryDateID = mWTExpectedDeliveryDate.Id
                                        });
                                    }
                                }
                            }
                        }
                    }

                    //   end

                    ViewBag.RefreshPage = true;

                    return View("~/Plugins/MWT.Shipping.FixedByWeightByTotal/Views/AddExpectedDeliveryDate.cshtml", model);
                }

            }
            else
            {
                await _baseAdminModelFactory.PrepareCategoriesAsync(model.AvailableCategories);
                // Zones

                model.AvailableZones.Add(new SelectListItem { Text = "Please select Zone", Value = "" });
                var zones = await _mwtShippingZoneService.GetZones(0, 100);
                foreach (var zone in zones)
                    model.AvailableZones.Add(new SelectListItem { Text = zone.Name, Value = zone.Id.ToString() });
                return View("~/Plugins/MWT.Shipping.FixedByWeightByTotal/Views/AddExpectedDeliveryDate.cshtml", model);
            }

        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> EditExpectedDeliveryDatePopup(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_SHIPPING_SETTINGS))
                return AccessDeniedView();

            var sbw = await _mWTExpectedDeliveryDateService.GetByIdAsync(id);
            if (sbw == null)
                //no record found with the specified id
                return RedirectToAction("ExpectedDeliveryDates");

            var model = new MWTExpectedDeliveryDateModel
            {
                Id = sbw.Id,
                ExpectedMaxNoOfDays = sbw.ExpectedMaxNoOfDays,
                ExpectedMinNoOfDays = sbw.ExpectedMinNoOfDays,
                ZoneID = sbw.ZoneID
            };
            // Zones

            model.AvailableZones.Add(new SelectListItem { Text = "Please select Zone", Value = "" });
            var zones = await _mwtShippingZoneService.GetZones(0, 100);
            foreach (var zone in zones)
                model.AvailableZones.Add(new SelectListItem { Text = zone.Name, Value = zone.Id.ToString() });

            await _baseAdminModelFactory.PrepareCategoriesAsync(model.AvailableCategories);


            model.SelectedCategoryIds =
                (await _mWTExpectedDeliveryDateService.GetEntityMappingsByExpectedDeliveryDateIDAndTypeAsync(sbw.Id, "Category")).Select(m => m.EntityID).ToList();


            model.ProductIDs =
                string.Join(',',
    (await _mWTExpectedDeliveryDateService.GetEntityMappingsByExpectedDeliveryDateIDAndTypeAsync(sbw.Id, "Product")).Select(m => m.EntityID).ToArray());

            return View("~/Plugins/MWT.Shipping.FixedByWeightByTotal/Views/EditExpectedDeliveryDate.cshtml", model);
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> EditExpectedDeliveryDatePopup(MWTExpectedDeliveryDateModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_SHIPPING_SETTINGS))
                return AccessDeniedView();
            if (ModelState.IsValid)
            {
                var sbw = await _mWTExpectedDeliveryDateService.GetByIdAsync(model.Id);
                if (sbw == null)
                    //no record found with the specified id
                    return RedirectToAction("ExpectedDeliveryDates");

                if ((string.IsNullOrEmpty(model.ProductIDs) || model.ProductIDs.Trim() == "") && model.SelectedCategoryIds.Count == 0)
                {
                    await _baseAdminModelFactory.PrepareCategoriesAsync(model.AvailableCategories);
                    // Zones

                    model.AvailableZones.Add(new SelectListItem { Text = "Please select Zone", Value = "" });
                    var zones = await _mwtShippingZoneService.GetZones(0, 100);
                    foreach (var zone in zones)
                        model.AvailableZones.Add(new SelectListItem { Text = zone.Name, Value = zone.Id.ToString() });
                    ModelState.AddModelError("", "Please specifiy either categories or products");
                    return View("~/Plugins/MWT.Shipping.FixedByWeightByTotal/Views/EditExpectedDeliveryDate.cshtml", model);
                }
                else
                {

                    sbw.ExpectedMinNoOfDays = model.ExpectedMinNoOfDays;
                    sbw.ExpectedMaxNoOfDays = model.ExpectedMinNoOfDays > model.ExpectedMaxNoOfDays ? model.ExpectedMinNoOfDays + 1 : model.ExpectedMaxNoOfDays;
                    sbw.ZoneID = model.ZoneID;
                    await _mWTExpectedDeliveryDateService.Update(sbw);

                    // Category Entity Mapping

                    var mapping = await _mWTExpectedDeliveryDateService.GetEntityMappingsByExpectedDeliveryDateIDAndTypeAsync(sbw.Id, "Category");
                    model.SelectedCategoryIds = model.SelectedCategoryIds.Distinct().ToList();

                    foreach (var _mapping in mapping.Where(m => !model.SelectedCategoryIds.Where(c => c == m.EntityID).Any()).ToList())
                    {
                        await _mWTExpectedDeliveryDateService.DeleteEntityMapping(_mapping);
                    }


                    foreach (var _categoryId in model.SelectedCategoryIds.Where(c => !mapping.Where(m => m.EntityID == c).Any()).ToList())
                    {
                        await _mWTExpectedDeliveryDateService.InsertEntityMapping(new MWTExpectedDeliveryDate_Entity_Mapping()
                        {
                            EntityID = _categoryId,
                            EntityType = "Category",
                            MWTExpectedDeliveryDateID = sbw.Id
                        });
                    }


                    // end


                    //Product

                    List<int> lstProdutdIds = new List<int>();
                    if (!string.IsNullOrEmpty(model.ProductIDs) && model.ProductIDs.Trim() != "")
                    {
                        foreach (var _productId in model.ProductIDs.Split(','))
                        {
                            int.TryParse(_productId, out int productId);
                            if (productId != 0)
                            {
                                if (!lstProdutdIds.Where(p => p == productId).Any())
                                {
                                    var product = await _productService.GetProductByIdAsync(productId);
                                    if (product != null)
                                        lstProdutdIds.Add(productId);
                                }
                            }
                        }
                    }


                    var productMapping = await _mWTExpectedDeliveryDateService.GetEntityMappingsByExpectedDeliveryDateIDAndTypeAsync(sbw.Id, "Product");
                    foreach (var _mapping in productMapping.Where(m => !lstProdutdIds.Where(c => c == m.EntityID).Any()).ToList())
                    {
                        await _mWTExpectedDeliveryDateService.DeleteEntityMapping(_mapping);
                    }


                    foreach (var _prdId in lstProdutdIds.Where(c => !productMapping.Where(m => m.EntityID == c).Any()).ToList())
                    {
                        await _mWTExpectedDeliveryDateService.InsertEntityMapping(new MWTExpectedDeliveryDate_Entity_Mapping()
                        {
                            EntityID = _prdId,
                            EntityType = "Product",
                            MWTExpectedDeliveryDateID = sbw.Id
                        });
                    }

                    // end

                    ViewBag.RefreshPage = true;

                    return View("~/Plugins/MWT.Shipping.FixedByWeightByTotal/Views/EditExpectedDeliveryDate.cshtml", model);
                }
            }
            else
            {
                await _baseAdminModelFactory.PrepareCategoriesAsync(model.AvailableCategories);
                // Zones

                model.AvailableZones.Add(new SelectListItem { Text = "Please select Zone", Value = "" });
                var zones = await _mwtShippingZoneService.GetZones(0, 100);
                foreach (var zone in zones)
                    model.AvailableZones.Add(new SelectListItem { Text = zone.Name, Value = zone.Id.ToString() });
                return View("~/Plugins/MWT.Shipping.FixedByWeightByTotal/Views/EditExpectedDeliveryDate.cshtml", model);
            }

        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> DeleteExpectedDeliveryDate(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_SHIPPING_SETTINGS))
                return Content("Access denied");

            var sbw = await _mWTExpectedDeliveryDateService.GetByIdAsync(id);
            if (sbw != null)
            {
                var _mappings = await _mWTExpectedDeliveryDateService.GetEntityMappingsByExpectedDeliveryDateIDAndTypeAsync(sbw.Id, "Category");
                foreach (var _mapping in _mappings)
                {
                    await _mWTExpectedDeliveryDateService.DeleteEntityMapping(_mapping);
                }
                _mappings = await _mWTExpectedDeliveryDateService.GetEntityMappingsByExpectedDeliveryDateIDAndTypeAsync(sbw.Id, "Product");
                foreach (var _mapping in _mappings)
                {
                    await _mWTExpectedDeliveryDateService.DeleteEntityMapping(_mapping);
                }
                await _mWTExpectedDeliveryDateService.Delete(sbw);
            }

            return new NullJsonResult();
        }

        #endregion
        #endregion
    }
}