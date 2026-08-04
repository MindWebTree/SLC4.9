using MWT.Nop.Core.Infrastructure;
using MWT.Nop.Core.Services.Customers;
using MWT.Plugin.Shipping.FixedByWeightByTotal.Components;
using MWT.Plugin.Shipping.FixedByWeightByTotal.Domain;
using MWT.Plugin.Shipping.FixedByWeightByTotal.Services;
using Nop.Core;
using Nop.Core.Domain.Shipping;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Plugins;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Tracking;
using Nop.Web.Framework.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MWT.Plugin.Shipping.FixedByWeightByTotal
{
    /// <summary>
    /// Fixed rate or by weight shipping computation method 
    /// </summary>
    public class FixedByWeightByTotalComputationMethod : BasePlugin, IMiscPlugin, IShippingRateComputationMethod, IWidgetPlugin
    {
        #region Fields

        private readonly FixedByWeightByTotalSettings _fixedByWeightByTotalSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ISettingService _settingService;
        private readonly IMWTShippingByWeightByTotalService _shippingByWeightByTotalService;
        private readonly IShippingService _shippingService;
        private readonly IShippingMethodsService _shippingMethodsService;
        private readonly IStoreContext _storeContext;
        private readonly IWebHelper _webHelper;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        #endregion

        #region Ctor

        public FixedByWeightByTotalComputationMethod(FixedByWeightByTotalSettings fixedByWeightByTotalSettings,
            ILocalizationService localizationService,
            IShoppingCartService shoppingCartService,
            ISettingService settingService,
            IMWTShippingByWeightByTotalService shippingByWeightByTotalService,
            IShippingService shippingService,
            IStoreContext storeContext,
            IWebHelper webHelper,
            IOrderTotalCalculationService orderTotalCalculationService,
            ICustomerService customerService,
             IWorkContext workContext,
             IShippingMethodsService shippingMethodsService)
        {
            _fixedByWeightByTotalSettings = fixedByWeightByTotalSettings;
            _localizationService = localizationService;
            _shoppingCartService = shoppingCartService;
            _settingService = settingService;
            _shippingByWeightByTotalService = shippingByWeightByTotalService;
            _shippingService = shippingService;
            _storeContext = storeContext;
            _webHelper = webHelper;
            _orderTotalCalculationService = orderTotalCalculationService;
            _customerService = customerService;
            _workContext = workContext;
            _shippingMethodsService = shippingMethodsService;

        }

        #endregion

        #region Utilities

        /// <summary>
        /// Get fixed rate
        /// </summary>
        /// <param name="shippingMethodId">Shipping method ID</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the rate
        /// </returns>
        private async Task<decimal> GetRateAsync(int shippingMethodId)
        {
            return await _settingService.GetSettingByKeyAsync<decimal>(string.Format(FixedByWeightByTotalDefaults.FixedRateSettingsKey, shippingMethodId));
        }

        /// <summary>
        /// Gets the transit days
        /// </summary>
        /// <param name="shippingMethodId">Shipping method ID</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ransit days
        /// </returns>
        private async Task<int?> GetTransitDaysAsync(int shippingMethodId)
        {
            return await _settingService.GetSettingByKeyAsync<int?>(string.Format(FixedByWeightByTotalDefaults.TransitDaysSettingsKey, shippingMethodId));
        }

        /// <summary>
        /// Get rate by weight and by total
        /// </summary>
        /// <param name="shippingByWeightByTotalRecord">Shipping by weight/by total record</param>
        /// <param name="subTotal">Subtotal</param>
        /// <param name="weight">Weight</param>
        /// <returns>Rate</returns>
        private (decimal, decimal) GetRate(MWTShippingByWeightByTotalRecord shippingByWeightByTotalRecord, decimal subTotal, decimal weight,bool applyWgsSurcharge)
        {
            //additional fixed cost
            decimal defaultWgsAmount = shippingByWeightByTotalRecord.Amount;
            decimal amount = !applyWgsSurcharge
                             ? shippingByWeightByTotalRecord.Amount :
                             shippingByWeightByTotalRecord.Surcharge > shippingByWeightByTotalRecord.Amount ? shippingByWeightByTotalRecord.Surcharge : shippingByWeightByTotalRecord.Amount;
            var shippingTotal = shippingByWeightByTotalRecord.AdditionalFixedCost;
            //percentage rate of subtotal
            if (shippingByWeightByTotalRecord.Amount > decimal.Zero)
            {
                if (shippingByWeightByTotalRecord.PercentageRateOfSubtotal > 0)
                {
                    decimal percentageTotal = (subTotal * shippingByWeightByTotalRecord.PercentageRateOfSubtotal) / 100;
                    shippingTotal += Math.Round(percentageTotal > amount ? percentageTotal : amount, 2);
                    defaultWgsAmount = percentageTotal > defaultWgsAmount ? percentageTotal : defaultWgsAmount;
                }
                else
                    shippingTotal += Math.Round(amount, 2);
            }
            return (Math.Max(shippingTotal, decimal.Zero), Math.Max(defaultWgsAmount, decimal.Zero));
        }

        #endregion

        #region Methods

        /// <summary>
        ///  Gets available shipping options
        /// </summary>
        /// <param name="getShippingOptionRequest">A request for getting shipping options</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the represents a response of getting shipping rate options
        /// </returns>
        public async Task<GetShippingOptionResponse> GetShippingOptionsAsync(GetShippingOptionRequest getShippingOptionRequest)
        {
            if (getShippingOptionRequest == null)
                throw new ArgumentNullException(nameof(getShippingOptionRequest));

            var response = new GetShippingOptionResponse();
            var customer = await _workContext.GetCurrentCustomerAsync();

            if (getShippingOptionRequest.Items == null || !getShippingOptionRequest.Items.Any())
            {
                response.AddError("No shipment items");
                return response;
            }

            //choose the shipping rate calculation method
            if (_fixedByWeightByTotalSettings.ShippingByWeightByTotalEnabled)
            {
                //shipping rate calculation by products weight

                if (getShippingOptionRequest.ShippingAddress == null)
                {
                    response.AddError("Shipping address is not set");
                    return response;
                }

                var storeId = getShippingOptionRequest.StoreId != 0 ? getShippingOptionRequest.StoreId : (await _storeContext.GetCurrentStoreAsync()).Id;
                var countryId = getShippingOptionRequest.ShippingAddress.CountryId ?? 0;
                var stateProvinceId = getShippingOptionRequest.ShippingAddress.StateProvinceId ?? 0;
                var warehouseId = getShippingOptionRequest.WarehouseFrom?.Id ?? 0;
                var zip = getShippingOptionRequest.ShippingAddress.ZipPostalCode;

                //get subtotal of shipped items

                //// Customization for custom order
                var subTotal = decimal.Zero;
                //if (getShippingOptionRequest.UseCustomSubtotal)
                //    subTotal = getShippingOptionRequest.CustomSubtotal;

                //else
                //{
                #region Custom updates Need to shift with Upgrade

                getShippingOptionRequest.IsSurchargeApplicable = false; // await _shoppingCartService.IsSurchargeApplicable(await _shoppingCartService.GetShoppingCartAsync(customer, Nop.Core.Domain.Orders.ShoppingCartType.ShoppingCart));


                subTotal = 0;// await _orderTotalCalculationService.GetCustomShoppingCartSubTotalAsync(getShippingOptionRequest.Items.Select(I => I.ShoppingCartItem).ToList());

                    #endregion
                   
                 

                    //#region Custom updates Need to shift with Upgrade

                    //decimal membershipfee = 0;
                    //decimal membershipfeeDiscount = 0;
                    //(decimal buyMoreDiscount, decimal membershipDiscount, decimal offerDiscount, decimal offerDiscountDefault, decimal productItemsDiscount, _) = await _orderTotalCalculationService.GetCustomBuyMoreSaveMoreDiscountAndMemberShipDiscountAsync(getShippingOptionRequest.Items.Select(I => I.ShoppingCartItem).ToList());
                    //if (await _customerService.IsMemberShipAddedInCart(await _workContext.GetCurrentCustomerAsync()))
                    //    (membershipfee, membershipfeeDiscount) = await _orderTotalCalculationService.GetMemberShipFee();

                    //subTotal = (subTotal + membershipfee + (offerDiscountDefault - offerDiscount)) - buyMoreDiscount - membershipDiscount - membershipfeeDiscount - productItemsDiscount;

                    //#endregion

              //  }


                //get weight of shipped items (excluding items with free shipping)
                var weight = await _shippingService.GetTotalWeightAsync(getShippingOptionRequest, ignoreFreeShippedItems: true);

                foreach (var shippingMethod in await _shippingMethodsService.GetAllShippingMethodsAsync(countryId))
                {
                    int? transitDays = null;
                    var rate = decimal.Zero;
                    var surcharge = decimal.Zero;
                    var wgsAmount = decimal.Zero;
                    var shippingByWeightByTotalRecord = await _shippingByWeightByTotalService.FindRecordsAsync(
                            shippingMethod.Id, storeId, warehouseId, countryId, stateProvinceId, zip, weight, subTotal);
                    if (shippingByWeightByTotalRecord == null)
                    {
                        if (_fixedByWeightByTotalSettings.LimitMethodsToCreated)
                            continue;
                    }
                    else
                    {
                        (rate,wgsAmount)= GetRate(shippingByWeightByTotalRecord, subTotal, weight, getShippingOptionRequest.IsSurchargeApplicable);
                        transitDays = shippingByWeightByTotalRecord.TransitDays;
                    }

                    var localeKeyName = "Custom.Shipping.Method.Description-" + (shippingByWeightByTotalRecord?.ShippingMethodId ?? 0) + "-" + (shippingByWeightByTotalRecord?.ZoneId ?? 0);
                    string customShippingMethodDescription = await _localizationService.GetResourceAsync(localeKeyName);
                    response.ShippingOptions.Add(new ShippingOption
                    {
                        Name = await _localizationService.GetLocalizedAsync(shippingMethod, x => x.Name),
                        Description = await _localizationService.GetLocalizedAsync(shippingMethod, x => x.Description),
                        Rate = rate,
                        TransitDays = transitDays,
                        AdditionalFee = shippingByWeightByTotalRecord == null ? 0 : shippingByWeightByTotalRecord.AdditionalFixedCost,
                        CustomShippingMethodDescription = string.Equals(customShippingMethodDescription, localeKeyName, StringComparison.InvariantCultureIgnoreCase) ? "" : customShippingMethodDescription,
                        DefaultAmount = getShippingOptionRequest.IsSurchargeApplicable ? wgsAmount : 0,
                        SurchargeAmount = getShippingOptionRequest.IsSurchargeApplicable ? shippingByWeightByTotalRecord.Surcharge : 0
                    });
                }
            }
            else
            {
                //shipping rate calculation by fixed rate
                var restrictByCountryId = getShippingOptionRequest.ShippingAddress?.CountryId;
                response.ShippingOptions = await (await _shippingMethodsService.GetAllShippingMethodsAsync(restrictByCountryId)).SelectAwait(async shippingMethod => new ShippingOption
                {
                    Name = await _localizationService.GetLocalizedAsync(shippingMethod, x => x.Name),
                    Description = await _localizationService.GetLocalizedAsync(shippingMethod, x => x.Description),
                    Rate = await GetRateAsync(shippingMethod.Id),
                    TransitDays = await GetTransitDaysAsync(shippingMethod.Id)
                }).ToListAsync();
            }

            return response;
        }

        /// <summary>
        /// Gets fixed shipping rate (if shipping rate computation method allows it and the rate can be calculated before checkout).
        /// </summary>
        /// <param name="getShippingOptionRequest">A request for getting shipping options</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the fixed shipping rate; or null in case there's no fixed shipping rate
        /// </returns>
        public async Task<decimal?> GetFixedRateAsync(GetShippingOptionRequest getShippingOptionRequest)
        {
            if (getShippingOptionRequest == null)
                throw new ArgumentNullException(nameof(getShippingOptionRequest));

            //if the "shipping calculation by weight" method is selected, the fixed rate isn't calculated
            if (_fixedByWeightByTotalSettings.ShippingByWeightByTotalEnabled)
                return null;

            var restrictByCountryId = getShippingOptionRequest.ShippingAddress?.CountryId;
            var rates = await (await _shippingMethodsService.GetAllShippingMethodsAsync(restrictByCountryId))
                .SelectAwait(async shippingMethod => await GetRateAsync(shippingMethod.Id)).Distinct().ToListAsync();

            //return default rate if all of them equal
            if (rates.Count == 1)
                return rates.FirstOrDefault();

            return null;
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/MWTFixedByWeightByTotal/Configure";
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task InstallAsync()
        {
            //settings
            await _settingService.SaveSettingAsync(new FixedByWeightByTotalSettings());

            //locales

            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["MWT.Plugins.Shipping.FixedByWeightByTotal.AddRecord"] = "Add record",
                ["MWT.Plugins.Shipping.FixedByWeightByTotal.ShippingByTotal"] = "By Total",
                ["MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.ZipCodes"] = "ZipCodes",
                ["MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.AdditionalFixedCost"] = "Additional fixed cost",
                ["MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.AdditionalFixedCost.Hint"] = "Specify an additional fixed cost per shopping cart for this option. Set to 0 if you don't want an additional fixed cost to be applied.",
                ["MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.Country"] = "Country",
                ["MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.Country.Hint"] = "If an asterisk is selected, then this shipping rate will apply to all customers, regardless of the country.",
                ["MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.DataHtml"] = "Data",
                ["MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.LimitMethodsToCreated"] = "Limit shipping methods to configured ones",
                ["MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.LimitMethodsToCreated.Hint"] = "If you check this option, then your customers will be limited to shipping options configured here. Otherwise, they'll be able to choose any existing shipping options even they are not configured here (zero shipping fee in this case).",
                ["MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.LowerWeightLimit"] = "Lower weight limit",
                ["MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.LowerWeightLimit.Hint"] = "Lower weight limit. This field can be used for \"per extra weight unit\" scenarios.",
                ["MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.OrderSubtotalFrom"] = "Order subtotal from",
                ["MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.OrderSubtotalFrom.Hint"] = "Order subtotal from.",
                ["MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.OrderSubtotalTo"] = "Order subtotal to",
                ["MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.OrderSubtotalTo.Hint"] = "Order subtotal to.",
                ["MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.Amount"] = "Shipping Amount",
                ["MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.Amount.Hint"] = "Shipping Amount",
                ["MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.Surcharge"] = "Surcharge",
                ["MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.Surcharge.Hint"] = "Surcharge",
                ["MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.Rate"] = "Rate",
                ["MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.RatePerWeightUnit"] = "Rate per weight unit",
                ["MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.RatePerWeightUnit.Hint"] = "Rate per weight unit.",
                ["MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.ShippingMethod"] = "Shipping method",
                ["MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.ShippingMethod.Hint"] = "Choose shipping method.",
                ["MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.StateProvince"] = "State / province",
                ["MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.StateProvince.Hint"] = "If an asterisk is selected, then this shipping rate will apply to all customers from the given country, regardless of the state.",
                ["MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.Store"] = "Store",
                ["MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.Store.Hint"] = "If an asterisk is selected, then this shipping rate will apply to all stores.",
                ["MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.TransitDays"] = "Transit days",
                ["MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.TransitDays.Hint"] = "The number of days of delivery of the goods.",
                ["MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.Warehouse"] = "Warehouse",
                ["MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.Warehouse.Hint"] = "If an asterisk is selected, then this shipping rate will apply to all warehouses.",
                ["MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.WeightFrom"] = "Order weight from",
                ["MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.WeightFrom.Hint"] = "Order weight from.",
                ["MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.WeightTo"] = "Order weight to",
                ["MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.WeightTo.Hint"] = "Order weight to.",
                ["MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.Zip"] = "Zip",
                ["MWT.Plugins.Shipping.FixedByWeightByTotal.Fields.Zip.Hint"] = "Zip / postal code. If zip is empty, then this shipping rate will apply to all customers from the given country or state, regardless of the zip code.",
                ["MWT.Plugins.Shipping.FixedByWeightByTotal.Fixed"] = "Fixed Rate",
                ["MWT.Plugins.Shipping.FixedByWeightByTotal.Formula"] = "Formula to calculate rates",
                ["MWT.Plugins.Shipping.FixedByWeightByTotal.Formula.Value"] = "[additional fixed cost] + ([order total weight] - [lower weight limit]) * [rate per weight unit] + [order subtotal] * [charge percentage]",
                ["MWT.Plugins.Shipping.FixedByWeightByTotal.ShippingByWeight"] = "By Weight",
                ["mwt.plugins.shipping.fixedbyweightbytotal.fields.zonename"] = "Zone",
                ["mwt.plugins.shipping.fixedbyweightbytotal.shippingzone.heading"] = "Shipping Zones",
                ["mwt.plugins.shipping.fixedbyweightbytotal.fields.name"] = "Name",
                ["MWT.Plugins.Shipping.MWTExpectedDeliveryDate.Fields.ExpectedMinNoOfDays"] = "Expected Min No Of Days",
                ["MWT.Plugins.Shipping.MWTExpectedDeliveryDate.Fields.ExpectedMaxNoOfDays"] = "Expected Max No Of Days",
                ["MWT.Plugins.Shipping.MWTExpectedDeliveryDate.Fields.Products"] = "Products",
                ["MWT.Plugins.Shipping.MWTExpectedDeliveryDate.Fields.Categories"] = "Categories",
                ["MWT.Plugins.Shipping.MWTExpectedDeliveryDate.Fields.Zone"] = "Zone",
                ["MWT.Plugins.Shipping.FixedByWeightByTotal.MWTExpectedDeliveryDate.ZoneId.Required"] = "Please select Zone",
                ["MWT.Plugins.Shipping.FixedByWeightByTotal.MWTExpectedDeliveryDate.ExpectedMinNoOfDays.Required"] = "Expected Min No Of Days Required",
                ["MWT.Plugins.Shipping.FixedByWeightByTotal.MWTExpectedDeliveryDate.ExpectedMinNoOfDays.Required"] = "Expected Max No Of Days Required",
            });

            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<FixedByWeightByTotalSettings>();

            //fixed rates
            try
            {

            var fixedRates = await (await _shippingMethodsService.GetAllShippingMethodsAsync())
                .SelectAwait(async shippingMethod => await _settingService.GetSettingAsync(
                    string.Format(FixedByWeightByTotalDefaults.FixedRateSettingsKey, shippingMethod.Id)))
                .Where(setting => setting != null).ToListAsync();
            await _settingService.DeleteSettingsAsync(fixedRates);
            await _localizationService.DeleteLocaleResourcesAsync("MWT.Plugins.Shipping.FixedByWeightByTotal");
            }
            catch (Exception ex)
            {

                throw;
            }

            //locales

            await base.UninstallAsync();
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string> { CustomPublicWidgetZones.ProductExpectedDeliveryDates });
        }

        

        public Task<IShipmentTracker> GetShipmentTrackerAsync()
        {
            return Task.FromResult<IShipmentTracker>(null);
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(WidgetsProductExpectedDeliverydateViewComponent);
        }

        #endregion

        #region Properties 
        public bool HideInWidgetList => false;

        #endregion
    }
}