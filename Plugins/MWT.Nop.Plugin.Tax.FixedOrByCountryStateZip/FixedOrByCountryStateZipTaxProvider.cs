using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Tax;
using MWT.Tax.FixedOrByCountryStateZip.Domain;
using MWT.Tax.FixedOrByCountryStateZip.Infrastructure.Cache;
using MWT.Tax.FixedOrByCountryStateZip.Services;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Tax;
using MWT.Tax.FixedOrByCountryStateZip;
// using Nop.Core.Domain.Customization.PhoneOrder;
using Nop.Core.Domain.Orders;
using Nop.Services.Customers;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Shipping;
using MWT.Nop.Core.Services.Customers;

namespace MWT.Tax.FixedOrByCountryStateZip
{
    /// <summary>
    /// Fixed or by country & state & zip rate tax provider
    /// </summary>
    public class MWTFixedOrByCountryStateZipTaxProvider : BasePlugin, ITaxProvider
    {
        #region Fields

        private readonly ITaxZarService _taxZarService;
        private readonly IZipTaxService _taxZipService;
        private readonly TaxZarSettings _countryStateZipSettings;
        private readonly IMWTCountryStateZipService _taxRateService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IPaymentService _paymentService;
        private readonly ISettingService _settingService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly ITaxService _taxService;
        private readonly IWebHelper _webHelper;
        private readonly TaxSettings _taxSettings;
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly IAddressService _addressService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public MWTFixedOrByCountryStateZipTaxProvider(TaxZarSettings countryStateZipSettings,
            IMWTCountryStateZipService taxRateService,
            IGenericAttributeService genericAttributeService,
            IHttpContextAccessor httpContextAccessor,
            ILocalizationService localizationService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IPaymentService paymentService,
            ISettingService settingService,
            IStaticCacheManager staticCacheManager,
            ITaxCategoryService taxCategoryService,
            ITaxService taxService,
            IWebHelper webHelper,
            TaxSettings taxSettings,
            ITaxZarService taxZarService,
            ICustomerService customerService,
            IWorkContext workContext,
            IAddressService addressService,
            IStoreContext storeContext,
            IZipTaxService taxZipService)
        {
            _countryStateZipSettings = countryStateZipSettings;
            _taxRateService = taxRateService;
            _genericAttributeService = genericAttributeService;
            _httpContextAccessor = httpContextAccessor;
            _localizationService = localizationService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _paymentService = paymentService;
            _settingService = settingService;
            _staticCacheManager = staticCacheManager;
            _taxCategoryService = taxCategoryService;
            _taxService = taxService;
            _webHelper = webHelper;
            _taxSettings = taxSettings;
            _taxZarService = taxZarService;
            _customerService = customerService;
            _workContext = workContext;
            _addressService = addressService;
            _storeContext = storeContext;
            _taxZipService = taxZipService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets tax rate
        /// </summary>
        /// <param name="taxRateRequest">Tax rate request</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ax
        /// </returns>
        public async Task<TaxRateResult> GetTaxRateAsync(TaxRateRequest taxRateRequest)
        {
            var result = new TaxRateResult();
            if (taxRateRequest.Address == null)
            {
                result.Errors.Add("Address is not set");
                return result;
            }

            result.TaxRate = 0;
            return result;

            //the tax rate calculation by country & state & zip 


            //      //first, load all tax rate records (cached) - loaded only once
            //   var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(ModelCacheEventConsumer.ALL_TAX_RATES_MODEL_KEY);
            //    var allTaxRates = await _staticCacheManager.GetAsync(cacheKey, async () => (await _taxRateService.GetAllTaxRatesAsync()).Select(taxRate => new MWTTaxRate
            //    {
            //        Id = taxRate.Id,
            //        StoreId = taxRate.StoreId,
            //        CountryId = taxRate.CountryId,
            //        StateProvinceId = taxRate.StateProvinceId,
            //        Zip = taxRate.Zip,
            //        Percentage = taxRate.Percentage
            //    }).ToList());

            //    var storeId = taxRateRequest.CurrentStoreId;
            //    var taxCategoryId = taxRateRequest.TaxCategoryId;
            //    var countryId = taxRateRequest.Address.CountryId;
            //    var stateProvinceId = taxRateRequest.Address.StateProvinceId;
            //    var zip = taxRateRequest.Address.ZipPostalCode?.Trim() ?? string.Empty;

            //    var existingRates = allTaxRates.Where(taxRate => taxRate.CountryId == countryId);

            //    //filter by store
            //    var matchedByStore = existingRates.Where(taxRate => storeId == taxRate.StoreId || taxRate.StoreId == 0);

            //    //filter by state/province
            //    var matchedByStateProvince = matchedByStore.Where(taxRate => stateProvinceId == taxRate.StateProvinceId || taxRate.StateProvinceId == 0);

            //    //filter by zip
            //    var matchedByZip = matchedByStateProvince.Where(taxRate => string.IsNullOrWhiteSpace(taxRate.Zip) || taxRate.Zip.Equals(zip, StringComparison.InvariantCultureIgnoreCase));

            //    //sort from particular to general, more particular cases will be the first
            //    var foundRecords = matchedByZip.OrderBy(r => r.StoreId == 0).ThenBy(r => r.StateProvinceId == 0).ThenBy(r => string.IsNullOrEmpty(r.Zip));

            //    var foundRecord = foundRecords.FirstOrDefault();

            //    if (foundRecord != null)
            //        result.TaxRate = foundRecord.Percentage;

            //    return result;
            //
        }

        /// <summary>
        /// Gets tax total
        /// </summary>
        /// <param name="taxTotalRequest">Tax total request</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ax total
        /// </returns>
        public async Task<TaxTotalResult> GetTaxTotalAsync(TaxTotalRequest taxTotalRequest)
        {
            bool taxRetrieved = false;

            if (_httpContextAccessor.HttpContext.Items.TryGetValue("nop.TaxTotal", out var result)
                && result is (TaxTotalResult taxTotalResult, decimal paymentTax) && string.IsNullOrEmpty(taxTotalRequest.ZipCode))
            {
                //short-circuit to avoid circular reference when calculating payment method additional fee during the checkout process
                if (!taxTotalRequest.UsePaymentMethodAdditionalFee)
                    return new TaxTotalResult { TaxTotal = taxTotalResult.TaxTotal - paymentTax, Taxes = taxTotalResult.Taxes ?? new List<TaxInfo>() };

                return taxTotalResult;
            }


            var taxRates = new SortedDictionary<decimal, decimal>();
            List<TaxInfo> taxes = new List<TaxInfo>();
            var total = decimal.Zero;
            string taxZarResponse = "";
            string taxRateInfo = "";
            decimal subtotalBase = 0;
            //if (!taxTotalRequest.IsCustomorder)
            //{
            //    //order sub total (items + checkout attributes)
            //    #region Custom updates Need to shift with Upgrade
            //    var (_, _, _, subTotalWithDiscountBase, _, _) = await _orderTotalCalculationService.GetCustomShoppingCartSubTotalAsync(taxTotalRequest.ShoppingCart, false);
            //    #endregion
            //    //subtotal with discount
            //    subtotalBase = subTotalWithDiscountBase;

            //    #region Custom updates Need to shift with Upgrade

            //    decimal membershipfee = 0;
            //    decimal membershipfeeDiscount = 0;
            //    (decimal buyMoreDiscount, decimal membershipDiscount, decimal offerDiscount, decimal offerDiscountDefault, decimal productItemsDiscount, _) = await _orderTotalCalculationService.GetCustomBuyMoreSaveMoreDiscountAndMemberShipDiscountAsync(taxTotalRequest.ShoppingCart);
            //    if (await _customerService.IsMemberShipAddedInCart(await _workContext.GetCurrentCustomerAsync()))
            //        (membershipfee, membershipfeeDiscount) = await _orderTotalCalculationService.GetMemberShipFee();

            //    subtotalBase = (subtotalBase + membershipfee + (offerDiscountDefault - offerDiscount)) - buyMoreDiscount - membershipDiscount - membershipfeeDiscount - productItemsDiscount;
            //    subtotalBase = subtotalBase + (await this._orderTotalCalculationService.GetCustomDuty(taxTotalRequest.ShoppingCart)).Item2;

            //    #region Custom Duty
            //    total = subtotalBase;

            //    #endregion
            //    #endregion
            //}
            //else
            //{
            //   taxTotalRequest.Total = taxTotalRequest.Total + (await this._orderTotalCalculationService.GetCustomDuty(new List<ShoppingCartItem>(), true, taxTotalRequest.Customer, taxTotalRequest.Total)).Item2;
            taxTotalRequest.Total = taxTotalRequest.Total + 0;
                total = subtotalBase = taxTotalRequest.Total;

            //}


            #region Get Tax from tax Zar Api
            var zipCode = "";
            string city = "";
            int stateId = 0;
            decimal taxRate = 0;
            bool isShippingChargable = true;
            bool isTaxZarRequestProcessed = false;
            int statusCode = 0;
            string url = "";
            string address1 = "";
            string address2 = "";
            var address = new Address();
            if (!string.IsNullOrEmpty(taxTotalRequest.ZipCode))
            {
                address.ZipPostalCode = taxTotalRequest.ZipCode;
                address.Address1 = string.Empty;
                address.Address2 = string.Empty;
                address.City = string.Empty;
                address.CountryId = 1;
                zipCode = taxTotalRequest.ZipCode;
            }
            else if (taxTotalRequest.Customer.ShippingAddressId != null)
            {
                address = await _addressService.GetAddressByIdAsync(Convert.ToInt32(taxTotalRequest.Customer.ShippingAddressId));
                zipCode = address?.ZipPostalCode;
                address1 = address?.Address1 ?? "";
                address2 = address?.Address2 ?? "";
                city = address?.City ?? "";
                stateId = address?.StateProvinceId ?? 0;
            }

            if (address != null)
            {
                var zarResponsecacheKey = _staticCacheManager.PrepareKeyForDefaultCache(ModelCacheEventConsumer.TAXRATE_ZAR_RESPONSE,
                    taxTotalRequest.Customer.Id, zipCode + "-" + city + "-" + stateId + "-" + address1 + "-" + address2);


                if (_countryStateZipSettings.TaxProvider == (int)TaxProviderType.TaxZar && _countryStateZipSettings.IncludedCountries.Split(',').Contains(address.CountryId.ToString()))
                {
                    (isTaxZarRequestProcessed, isShippingChargable, taxRate, taxZarResponse, taxRateInfo, statusCode, url) = await _staticCacheManager.GetAsync(zarResponsecacheKey, async () =>
                     await _taxZarService.GetTaxRate(taxTotalRequest.Customer.Id, zipCode)
                     );
                }
                else if (_countryStateZipSettings.IncludedCountries.Split(',').Contains(address.CountryId.ToString()))
                {
                    (isTaxZarRequestProcessed, isShippingChargable, taxRate, taxZarResponse, taxRateInfo, statusCode, url) = await _staticCacheManager.GetAsync(zarResponsecacheKey, async () =>
                        await _taxZipService.GetTaxRate(taxTotalRequest.Customer.Id, address)
                        );
                }
                taxRetrieved = isTaxZarRequestProcessed;
                if (!isTaxZarRequestProcessed)
                {  //      //first, load all tax rate records (cached) - loaded only once
                    var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(ModelCacheEventConsumer.ALL_TAX_RATES_MODEL_KEY);
                    var allTaxRates = await _staticCacheManager.GetAsync(cacheKey, async () => (await _taxRateService.GetAllTaxRatesAsync()).Select(taxRate => new MWTTaxRate
                    {
                        Id = taxRate.Id,
                        StoreId = taxRate.StoreId,
                        CountryId = taxRate.CountryId,
                        StateProvinceId = taxRate.StateProvinceId,
                        Zip = taxRate.Zip,
                        Percentage = taxRate.Percentage,
                        GSTPercentage = taxRate.GSTPercentage,
                        PSTPercentage = taxRate.PSTPercentage,
                        HSTPercentage = taxRate.HSTPercentage,
                        QSTPercentage = taxRate.QSTPercentage,
                    }).ToList());

                    var storeId = _storeContext.GetCurrentStore()?.Id;

                    var countryId = address.CountryId;
                    var stateProvinceId = address.StateProvinceId;
                    var zip = zipCode?.Trim() ?? string.Empty;

                    var existingRates = allTaxRates.Where(taxRate => taxRate.CountryId == countryId);

                    //filter by store
                    var matchedByStore = existingRates.Where(taxRate => storeId == taxRate.StoreId || taxRate.StoreId == 0);

                    //filter by state/province
                    var matchedByStateProvince = matchedByStore.Where(taxRate => stateProvinceId == taxRate.StateProvinceId || taxRate.StateProvinceId == 0);

                    //filter by zip
                    var matchedByZip = matchedByStateProvince.Where(taxRate => string.IsNullOrWhiteSpace(taxRate.Zip) || taxRate.Zip.Equals(zip, StringComparison.InvariantCultureIgnoreCase));

                    //sort from particular to general, more particular cases will be the first
                    var foundRecords = matchedByZip.OrderBy(r => r.StoreId == 0).ThenBy(r => r.StateProvinceId == 0).ThenBy(r => string.IsNullOrEmpty(r.Zip));

                    var foundRecord = foundRecords.FirstOrDefault();

                    if (foundRecord != null)
                    {
                        taxRate = foundRecord.Percentage;
                        taxes.Add(new TaxInfo()
                        {
                            TaxType = TaxType.Tax,
                            TaxRate = taxRate,
                            Amount = (subtotalBase * taxRate) / 100
                        });
                        taxes.Add(new TaxInfo()
                        {
                            TaxType = TaxType.GST,
                            TaxRate = foundRecord.GSTPercentage < 0 ? 0 : foundRecord.GSTPercentage,
                            Amount = foundRecord.GSTPercentage <= 0 ? 0 : (subtotalBase * foundRecord.GSTPercentage) / 100
                        });
                        taxes.Add(new TaxInfo()
                        {
                            TaxType = TaxType.PST,
                            TaxRate = foundRecord.PSTPercentage < 0 ? 0 : foundRecord.PSTPercentage,
                            Amount = foundRecord.PSTPercentage <= 0 ? 0 : (subtotalBase * foundRecord.PSTPercentage) / 100
                        });
                        taxes.Add(new TaxInfo()
                        {
                            TaxType = TaxType.HST,
                            TaxRate = foundRecord.HSTPercentage < 0 ? 0 : foundRecord.HSTPercentage,
                            Amount = foundRecord.HSTPercentage <= 0 ? 0 : (subtotalBase * foundRecord.HSTPercentage) / 100
                        });
                        taxes.Add(new TaxInfo()
                        {
                            TaxType = TaxType.QST,
                            TaxRate = foundRecord.QSTPercentage < 0 ? 0 : foundRecord.QSTPercentage,
                            Amount = foundRecord.QSTPercentage <= 0 ? 0 : (subtotalBase * foundRecord.QSTPercentage) / 100
                        });
                    }
                   
                }
                else
                {
                    taxes.Add(new TaxInfo()
                    {
                        TaxType = TaxType.Tax,
                        TaxRate = taxRate,
                        Amount = (subtotalBase * taxRate) / 100
                    });

                }
            }
            #endregion




            //shipping
            var shippingOption = await _genericAttributeService.GetAttributeAsync<ShippingOption>(await _workContext.GetCurrentCustomerAsync(),
                           NopCustomerDefaults.SelectedShippingOptionAttribute, (await _storeContext.GetCurrentStoreAsync()).Id);

            if (await _settingService.GetSettingByKeyAsync<bool>($"ShippingIsTaxable_{address?.CountryId ?? 0}") && taxes.Where(t => t.TaxRate > 0).Any() && ((
                isTaxZarRequestProcessed && isShippingChargable && shippingOption != null /*&& shippingOption.Name != await _localizationService.GetResourceAsync("freeshipping.method.name")*/)
                || (!isTaxZarRequestProcessed)))
            {
                if (!taxTotalRequest.IsCustomorder)
                {
                    var (shippingExclTax, _, _) = await _orderTotalCalculationService
                    .GetShoppingCartShippingTotalAsync(taxTotalRequest.ShoppingCart, false);
                    if (shippingExclTax.HasValue)
                    {
                        foreach (var tax in taxes)
                        {
                            tax.Amount += tax.TaxRate <= 0 ? 0 : (Convert.ToDecimal(shippingExclTax) * tax.TaxRate) / 100;
                        }
                        total += Convert.ToDecimal(shippingExclTax);
                    }
                }
                else if (taxTotalRequest.ShippingCharges > 0)
                {
                    foreach (var tax in taxes)
                    {
                        tax.Amount += tax.TaxRate <= 0 ? 0 : (Convert.ToDecimal(taxTotalRequest.ShippingCharges) * tax.TaxRate) / 100;
                    }
                    total += Convert.ToDecimal(taxTotalRequest.ShippingCharges);
                }
            }


            //short-circuit to avoid circular reference when calculating payment method additional fee during the checkout process
            if (!taxTotalRequest.UsePaymentMethodAdditionalFee)
            {
                if (taxes.Sum(t => t.TaxRate) <= 0)
                    taxRates.Add(decimal.Zero, decimal.Zero);
                else
                    taxRates.Add(taxes.Sum(t => t.TaxRate), total);
                return new TaxTotalResult { TaxTotal = taxes.Select(x => x.Amount).Sum(), TaxRates = taxRates, Taxes = taxes, TaxRetrieved = taxRetrieved };
            }
            //payment method additional fee
            var paymentMethodAdditionalFeeTax = decimal.Zero;
            if (!taxTotalRequest.IsCustomorder && _taxSettings.PaymentMethodAdditionalFeeIsTaxable)
            {
                var paymentMethodSystemName = taxTotalRequest.Customer != null
                    ? await _genericAttributeService
                        .GetAttributeAsync<string>(taxTotalRequest.Customer, NopCustomerDefaults.SelectedPaymentMethodAttribute, taxTotalRequest.StoreId)
                    : string.Empty;

                var paymentMethodAdditionalFee = await _paymentService
                    .GetAdditionalHandlingFeeAsync(taxTotalRequest.ShoppingCart, paymentMethodSystemName);
                if (paymentMethodAdditionalFee > decimal.Zero)
                {
                    foreach (var tax in taxes)
                    {
                        tax.Amount += tax.TaxRate <= 0 ? 0 : (Convert.ToDecimal(paymentMethodAdditionalFee) * tax.TaxRate) / 100;
                    }
                    total += paymentMethodAdditionalFee;
                }
            }


            //add at least one tax rate (0%)
            if (taxes.Sum(t => t.TaxRate) <= 0)
                taxRates.Add(decimal.Zero, decimal.Zero);
            else
                taxRates.Add(taxes.Sum(t => t.TaxRate), total);


            taxTotalResult = new TaxTotalResult { TaxTotal = taxes.Select(x => x.Amount).Sum(), TaxRates = taxRates, Taxes = taxes, TaxRetrieved = taxRetrieved };

            //store values within the scope of the request to avoid duplicate calculations
            _httpContextAccessor.HttpContext.Items.TryAdd("nop.TaxTotal", (taxTotalResult, paymentMethodAdditionalFeeTax));

            return taxTotalResult;
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/MWTFixedOrByCountryStateZip/Configure";
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task InstallAsync()
        {
            //settings
            await _settingService.SaveSettingAsync(new TaxZarSettings());

            //locales
            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.MWT.Tax.FixedOrByCountryStateZip.Fixed"] = "Fixed rate",
                ["Plugins.MWT.Tax.FixedOrByCountryStateZip.Tax.Categories.Manage"] = "Manage tax categories",
                ["Plugins.MWT.Tax.FixedOrByCountryStateZip.TaxCategoriesCanNotLoaded"] = "No tax categories can be loaded. You may manage tax categories by <a href='{0}'>this link</a>",
                ["Plugins.MWT.Tax.FixedOrByCountryStateZip.TaxByCountryStateZip"] = "By Country",
                ["Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.TaxCategoryName"] = "Tax category",
                ["Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.Rate"] = "Rate",
                ["Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.Store"] = "Store",
                ["Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.Store.Hint"] = "If an asterisk is selected, then this shipping rate will apply to all stores.",
                ["Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.Country"] = "Country",
                ["Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.Country.Hint"] = "The country.",
                ["Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.StateProvince"] = "State / province",
                ["Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.StateProvince.Hint"] = "If an asterisk is selected, then this tax rate will apply to all customers from the given country, regardless of the state.",
                ["Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.Zip"] = "Zip",
                ["Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.Zip.Hint"] = "Zip / postal code. If zip is empty, then this tax rate will apply to all customers from the given country or state, regardless of the zip code.",
                ["Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.TaxCategory"] = "Tax category",
                ["Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.TaxCategory.Hint"] = "The tax category.",
                ["Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.Percentage"] = "Percentage",
                ["Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.Percentage.Hint"] = "The tax rate.",
                ["Plugins.MWT.Tax.FixedOrByCountryStateZip.AddRecord"] = "Add tax rate",
                ["Plugins.MWT.Tax.FixedOrByCountryStateZip.AddRecordTitle"] = "New tax rate",
                ["Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.ApiLink"] = "Tax Zar Rest Api link",
                ["Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.Token"] = "Tax Zar Token",
                ["Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.IncludeWgs"] = "Tax Include Wgs",
                ["Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.StatusCode"] = "Api Status Code",
                ["Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.Url"] = "Request Url",
                ["Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.Request"] = " Body",
                ["Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.Response"] = "Response",
                ["Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.TaxRateInfo"] = "Tax Rate",
                ["Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.CustomerId"] = "CustomerId",
                ["Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.CreatedDateUtc"] = "CreatedOn",
                ["Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.OrderId"] = "OrderId",
                ["Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.ZipTaxApiLink"] = "Api Link",
                ["Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.ZipTaxKey"] = "Key",
                ["Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.TaxProvider"] = "Tax Provider",
                ["Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.GSTPercentage"] = "GST",
                ["Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.PSTPercentage"] = "PST",
                ["Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.HSTPercentage"] = "HST",
                ["Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.QSTPercentage"] = "QST",
                ["Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.IncludedCountries"] = "Apply for Countries"
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
            await _settingService.DeleteSettingAsync<TaxZarSettings>();

            //fixed rates
            var fixedRates = await (await _taxCategoryService.GetAllTaxCategoriesAsync())
                .SelectAwait(async taxCategory => await _settingService.GetSettingAsync(string.Format(FixedOrByCountryStateZipDefaults.FixedRateSettingsKey, taxCategory.Id)))
                .Where(setting => setting != null).ToListAsync();
            await _settingService.DeleteSettingsAsync(fixedRates);

            //locales
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.MWT.Tax.FixedOrByCountryStateZip");

            await base.UninstallAsync();
        }

        #endregion
    }
}