using Microsoft.CodeAnalysis;
using MWT.Nop.Core.Domain.StoreWideDiscount;
using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Service.Discounts;
using MWT.Nop.Core.Services.Catalog;
using MWTNop.Core.Domain.Catalog;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Logging;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;
using Task = System.Threading.Tasks.Task;

namespace MWT.Plugin.Misc.MwtStorefront.Tasks
{
    public partial class SalesOfferTask : IScheduleTask
    {
        #region Fieds

        private readonly IStoreWideDiscountService _storeWideDiscountService;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly ILogger _loggerService;
        private readonly IProductExtendedService _productService;
        private readonly ICustomProductAttributeService _productAttributeService;
        private readonly IWorkContext _workContext;
        private readonly ICategoryService _categoryService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly IPluginService _pluginService;
        bool enableLog = false;

        #endregion

        #region Ctor

        public SalesOfferTask(IStoreWideDiscountService storeWideDiscountService,
            ILocalizationService localizationService, ISettingService settingService,
            ILogger loggerService, IProductExtendedService productService, ICustomProductAttributeService productAttributeService,
            IWorkContext workContext, ICategoryService categoryService, IStaticCacheManager staticCacheManager,
            IScheduleTaskService scheduleTaskService, IPluginService pluginService)
        {
            _storeWideDiscountService = storeWideDiscountService;
            _localizationService = localizationService;
            _settingService = settingService;
            _loggerService = loggerService;
            _productService = productService;
            _productAttributeService = productAttributeService;
            _workContext = workContext;
            _categoryService = categoryService;
            _staticCacheManager = staticCacheManager;
            _scheduleTaskService = scheduleTaskService;
            _pluginService = pluginService;
        }
        #endregion

        #region Methods
        public async System.Threading.Tasks.Task ExecuteAsync()
        {

            try
            {
                var plugin = await _pluginService
    .GetPluginDescriptorBySystemNameAsync<IPlugin>(
        "MWT.Nop.Plugin.Misc.ProductBundle",
        LoadPluginsMode.All);
                bool isBundlePluginActive = plugin != null && plugin.Installed;
                var hsbundleVariantIds = new HashSet<int>();
                if (isBundlePluginActive)
                {
                    var bundleVariantIds = await _settingService.GetSettingByKeyAsync<string>("ProductBundleSettings.BundleVariantIds");
                    hsbundleVariantIds = (bundleVariantIds ?? string.Empty)
                   .Split(',', StringSplitOptions.RemoveEmptyEntries)
                   .Select(x => int.TryParse(x, out var id) ? id : 0)
                   .Where(x => x > 0)
                   .ToHashSet();
                }


                var isInstalled = plugin != null && plugin.Installed;
                List<StoreWideDiscount> lstStoreWideDiscount = new List<StoreWideDiscount>();
                enableLog = await _settingService.GetSettingByKeyAsync<bool>("Sales.Offer.Task.Enable.Log");
                await this.InsertLog("Task Started", "Task Started", LogLevel.Information);

                List<ProductDiscountInfo> lstProductOfferinfo = new List<ProductDiscountInfo>();
                #region Get All Offers Need to Implement

                var offers = await _storeWideDiscountService.GetStoreWideDiscountByDate(DateTime.Now);
                await this.InsertLog($"No of pending Offer found that need to process at {DateTime.Now} is {offers.Where(o => !o.IsProcessed).Count()}", "", LogLevel.Information);

                #endregion
                await _staticCacheManager.RemoveByPrefixAsync(CustomNopCatalogDefaults.ProductSaleInfoPrefix);
                if (offers.Any())
                {

                    await this.InsertLog($"Going to process in In-Active Offers", "", LogLevel.Information);

                    await this.InsertLog($"No of In-Active Offer found that need to process at {DateTime.Now} is {offers.Where(o => !o.IsProcessed && (o.IsDeleted || !o.Published)).Count()}", "", LogLevel.Information);
                    foreach (var offer in offers.Where(o => !o.IsProcessed && (o.IsDeleted || !o.Published)))
                    {
                        await this.InsertLog($"Going to process offer {offer.Name} at  {DateTime.Now}", "", LogLevel.Information);
                        var settings = await _storeWideDiscountService.GetAllStoreWideDiscountSettingAsync(offer.Id, 0, int.MaxValue, true);
                        await this.InsertLog($"No of setting found {settings.Count} at  {DateTime.Now}", "", LogLevel.Information);
                        foreach (var setting in settings)
                        {

                            setting.Discount = 0;
                            setting.InfoText = string.Empty;
                            setting.InfoHelpText = string.Empty;
                            setting.Threshold = 0;
                            await GetDiscountOnProducts(lstProductOfferinfo, setting);
                        }

                        await this.InsertLog($"Process completed for {offer.Name} at  {DateTime.Now}", "", LogLevel.Information);
                        lstStoreWideDiscount.Add(offer);
                    }


                    await this.InsertLog($"No of Active Offer found that need to process at {DateTime.Now} is {offers.Where(o => !o.IsProcessed && !o.IsDeleted && o.Published).Count()}", "", LogLevel.Information);
                    if (offers.Where(o => !o.IsProcessed && (o.IsDeleted || !o.Published)).Any())
                    {
                        foreach (var _offer in offers.Where(o => o.IsProcessed && !o.IsDeleted && o.Published))
                        {
                            _offer.IsProcessed = false;
                        }
                    }

                    foreach (var offer in offers.Where(o => !o.IsProcessed && !o.IsDeleted && o.Published))
                    {
                        await this.InsertLog($"Going to process offer {offer.Name} at  {DateTime.Now}", "", LogLevel.Information);

                        var settings = await _storeWideDiscountService.GetAllStoreWideDiscountSettingAsync(offer.Id, 0, int.MaxValue, true);
                        await this.InsertLog($"No of setting found {settings.Count} at  {DateTime.Now}", "", LogLevel.Information);
                        foreach (var setting in settings)
                        {
                            await GetDiscountOnProducts(lstProductOfferinfo, setting);
                        }

                        await this.InsertLog($"Process completed for {offer.Name} at  {DateTime.Now}", "",LogLevel.Information);
                        lstStoreWideDiscount.Add(offer);
                    }

                    #region Remove Discount from Products that is excluded

                    if (offers.Where(o => !o.IsProcessed && !o.IsDeleted && o.Published).Any() || (!offers.Where(o => !o.IsProcessed && !o.IsDeleted && o.Published).Any()
                        && offers.Where(o => !o.IsProcessed && (o.IsDeleted || !o.Published)).Any()
                        ))
                    {
                        var allPrds = await _productService.GetAllProducts();
                        await this.InsertLog($"Products from where offer need to remove {string.Join(',', allPrds.Select(p => p.Id))} at  {DateTime.Now}", "", LogLevel.Information);
                        foreach (var product in allPrds)
                        {
                            if (!lstProductOfferinfo.Where(p => p.ProductId == product.Id).Any())
                            {
                                lstProductOfferinfo.Add(new ProductDiscountInfo()
                                {
                                    ProductId = product.Id,
                                    Discount = 0,
                                    StoreWideDiscountId = 0,
                                    InfoHelpText = string.Empty,
                                    InfoText = string.Empty,
                                    Threshold = 0
                                });
                            }
                        }
                        await this.InsertLog($"Process completed where offer need to remove at  {DateTime.Now}", "", LogLevel.Information);
                    }
                    #endregion


                }
                else
                {
                    #region Remove Offer

                    var products = await _storeWideDiscountService.GetStoreWideProductDiscountInfoHaveDiscountAppliedAsync();
                    await this.InsertLog($"No of products found from where offer need to remove at {DateTime.Now} is {products.Count} ", "", LogLevel.Information);

                    foreach (var product in products)
                    {
                        lstProductOfferinfo.Add(new ProductDiscountInfo()
                        {
                            Discount = 0,
                            ProductId = product.ProductId,
                            StoreWideDiscountId = 0,
                            InfoHelpText = string.Empty,
                            InfoText = string.Empty,
                            Threshold = 0
                        });
                    }
                    await this.InsertLog($"Process completed where offer need to remove at  {DateTime.Now}", "", LogLevel.Information);
                    #endregion
                }


                #region Update Discount

                await this.InsertLog($"No of products found offer need to apply at {DateTime.Now} is {lstProductOfferinfo.Count} ", "", LogLevel.Information);

                foreach (var productOffer in lstProductOfferinfo)
                {
                    await this.InsertLog($"Going to apply offer for Product {productOffer.ProductId} {productOffer.Discount} {DateTime.Now}", "", LogLevel.Information);


                    var product = await _productService.GetProductByIdAsync(productOffer.ProductId);
                    if (product != null)
                    {
                        bool isOfferApplied = false;

                        #region Attribute Combinations
                        var productAttributeMapping = await _productAttributeService.CustomGetProductAttributeMappingsByProductIdAsync(productOffer.ProductId);
                        var prdVariants = await _productService.GetProductVariants(productOffer.ProductId);
                        var prdCombinations = await _productAttributeService.CustomGetAllProductAttributeCombinationsAsync(productOffer.ProductId);

                        List<int> attrcombinationIdToskip = new List<int>();
                        List<VariantCombination> variants = new List<VariantCombination>();
                        var bundledVariants = new List<VariantCombination>();

                        foreach (var variant in prdVariants)
                        {
                            if (!hsbundleVariantIds.Contains(variant.VariantId))
                                variants.Add(variant);
                            else
                            {
                                bundledVariants.Add(variant);
                                attrcombinationIdToskip.AddRange((await _productService.GetVariantCombinationAsync(variant, prdCombinations)).Select(x => x.Id).ToList());
                            }
                        }


                        if (productAttributeMapping.Count > 0)
                        {

                            var combinations = prdCombinations
                                          .Where(x => !attrcombinationIdToskip.Contains(x.Id))
                                          .ToList();
                            foreach (var combination in combinations)
                            {
                                if (productOffer.Discount <= 0 || (((combination.OverriddenOldPrice ?? 0) <= 0 ? (combination.OverriddenPrice ?? 0) > 0 ? combination.OverriddenPrice : (product.OldPrice <= 0 ? product.Price : product.OldPrice) : combination.OverriddenOldPrice) < productOffer.Threshold))
                                {

                                    combination.OverriddenPrice = (combination.OverriddenOldPrice ?? 0) <= 0 ? (combination.OverriddenPrice ?? 0) > 0 ? combination.OverriddenPrice : (product.OldPrice <= 0 ? product.Price : product.OldPrice) : combination.OverriddenOldPrice;

                                    combination.OverriddenOldPrice = 0;
                                }
                                else
                                {
                                    isOfferApplied = true;
                                    combination.OverriddenOldPrice = (combination.OverriddenOldPrice ?? 0) <= 0 ? (combination.OverriddenPrice ?? 0) > 0 ? combination.OverriddenPrice : product.OldPrice <= 0 ? product.Price : product.OldPrice : combination.OverriddenOldPrice;
                                    combination.OverriddenPrice = Math.Round(Convert.ToDecimal(combination.OverriddenOldPrice - (combination.OverriddenOldPrice * productOffer.Discount / 100)), 2);

                                    combination.OverriddenPrice = (combination.OverriddenPrice % 1 >= 0.5m) ? Math.Ceiling((decimal)combination.OverriddenPrice) : Math.Floor((decimal)combination.OverriddenPrice);

                                }
                                await _productAttributeService.CustomUpdateProductAttributeCombinationAsync(combination);
                            }
                        }

                        #endregion

                        #region VariantCombination

                        foreach (var variant in variants)
                        {
                            if (productOffer.Discount <= 0 || (((variant.OldPrice ?? 0) <= 0 ? (variant.Price) > 0 ? variant.Price : (product.OldPrice <= 0 ? product.Price : product.OldPrice) : (decimal)variant.OldPrice) < productOffer.Threshold))
                            {
                                variant.Price = (variant.OldPrice ?? 0) <= 0 ? (variant.Price) > 0 ? variant.Price : (product.OldPrice <= 0 ? product.Price : product.OldPrice) : (decimal)variant.OldPrice;
                                variant.OldPrice = 0;
                            }
                            else
                            {
                                variant.OldPrice = (variant.OldPrice ?? 0) <= 0 ? (variant.Price) > 0 ? variant.Price : product.OldPrice <= 0 ? product.Price : product.OldPrice : variant.OldPrice;
                                variant.Price = Math.Round(Convert.ToDecimal(variant.OldPrice - (variant.OldPrice * productOffer.Discount / 100)), 2);

                                variant.Price = (variant.Price % 1 >= 0.5m) ? Math.Ceiling((decimal)variant.Price) : Math.Floor((decimal)variant.Price);
                            }
                            await _productService.UpdateVariant(variant, false);
                        }
                        #endregion

                        if (!bundledVariants.Any())
                        {
                            if (productOffer.Discount <= 0 || !isOfferApplied)
                            {
                                product.Price = product.OldPrice == 0 ? product.Price : product.OldPrice;
                                product.OldPrice = 0;
                                productOffer.Discount = 0;
                                productOffer.StoreWideDiscountId = 0;
                                productOffer.InfoHelpText = string.Empty;
                                productOffer.InfoText = string.Empty;
                                productOffer.Threshold = 0;
                            }
                            else
                            {
                                product.OldPrice = product.OldPrice == 0 ? product.Price : product.OldPrice;
                                product.Price = Math.Round(product.OldPrice - (product.OldPrice * productOffer.Discount / 100), 2);
                                product.Price = (product.Price % 1 >= 0.5m) ? Math.Ceiling((decimal)product.Price) : Math.Floor((decimal)product.Price);
                            }
                            await this._productService.UpdateProductWithoutEvent(product);
                            await this._productService.GetVariantPriceRange(product, false, true, false, false);
                        }

                        // Take Log of Offers 

                        await SaveProductOfferLog(productOffer);
                        // end 

                    }
                    else
                    {
                        await this.InsertLog($"product not found {productOffer.ProductId} {productOffer.Discount} {DateTime.Now}", "", LogLevel.Information);
                    }
                    await this.InsertLog($"Process Completed of Offer implementation {productOffer.ProductId} {productOffer.Discount} {DateTime.Now}", "", LogLevel.Information);
                }

                #region Mark Sales Offer As Processed

                foreach (var storeWideDiscount in lstStoreWideDiscount)
                {
                    storeWideDiscount.IsProcessed = true;
                    storeWideDiscount.UpdatedOn = DateTime.Now;
                    await _storeWideDiscountService.UpdateStoreWideDiscountAsync(storeWideDiscount);
                }

                #endregion

                await this.InsertLog($"Process completed to apply Offer at {DateTime.Now}", "", LogLevel.Information);
                #endregion
            }
            catch (Exception exp)
            {
                await this.InsertLog($"Failed to process offer at {DateTime.Now}", exp?.Message, LogLevel.Information, true);
            }

            #region  reset cache
            await _staticCacheManager.ClearAsync();

            #endregion

            #region Run Schedule

            var scheduleTask = await _scheduleTaskService.GetTaskByTypeAsync("Nop.Web.Customizations.Tasks.ElasticSearchTask");
            if (scheduleTask != null)
            {
                scheduleTask.LastSuccessUtc = scheduleTask.LastSuccessUtc.HasValue ?
                System.DateTime.Now.AddHours(-12).AddMinutes(10) : null;
                scheduleTask.LastStartUtc = scheduleTask.LastStartUtc.HasValue ? System.DateTime.Now.AddHours(-12).AddMinutes(10) : null;
                scheduleTask.LastEndUtc = scheduleTask.LastEndUtc.HasValue ? System.DateTime.Now.AddHours(-12).AddMinutes(10) : null;
                await _scheduleTaskService.UpdateTaskAsync(scheduleTask);
            }

            scheduleTask = await _scheduleTaskService.GetTaskByTypeAsync("Nop.Web.Customizations.Tasks.GenerateProductBundlePriceInVariants");
            if (scheduleTask != null)
            {
                scheduleTask.LastSuccessUtc = scheduleTask.LastSuccessUtc.HasValue ?
                System.DateTime.Now.AddHours(-12).AddMinutes(10) : null;
                scheduleTask.LastStartUtc = scheduleTask.LastStartUtc.HasValue ? System.DateTime.Now.AddHours(-12).AddMinutes(10) : null;
                scheduleTask.LastEndUtc = scheduleTask.LastEndUtc.HasValue ? System.DateTime.Now.AddHours(-12).AddMinutes(10) : null;
                await _scheduleTaskService.UpdateTaskAsync(scheduleTask);
            }
            #endregion

        }

        #endregion

        #region Utilities

        public async Task SaveProductOfferLog(ProductDiscountInfo discountInfo)
        {
            var productDiscountInfo = await _storeWideDiscountService.GetStoreWideProductDiscountInfoByProductIdAsync(discountInfo.ProductId);
            if (productDiscountInfo != null)
            {
                productDiscountInfo.Discount = discountInfo.Discount;
                productDiscountInfo.InfoHelpText = discountInfo.InfoHelpText;
                productDiscountInfo.InfoText = discountInfo.InfoText;
                productDiscountInfo.UpdatedOn = DateTime.Now;
                productDiscountInfo.StoreWideDiscountId = discountInfo.Discount > 0 ? discountInfo.StoreWideDiscountId : 0;
                await _storeWideDiscountService.UpdateStoreWideProductDiscountInfoAsync(productDiscountInfo);
            }
            else
            {
                await _storeWideDiscountService.InsertStoreWideProductDiscountInfoAsync(new StoreWideProductDiscountInfo()
                {
                    InfoHelpText = discountInfo.InfoHelpText,
                    InfoText = discountInfo.InfoText,
                    Discount = discountInfo.Discount,
                    ProductId = discountInfo.ProductId,
                    CreatedOn = DateTime.Now,
                    UpdatedOn = DateTime.Now,
                    StoreWideDiscountId = discountInfo.Discount > 0 ? discountInfo.StoreWideDiscountId : 0
                });
            }

            await _storeWideDiscountService.InsertStoreWideProductDiscountHistoryAsync(new StoreWideProductDiscountHistory()
            {
                CreatedOn = DateTime.Now,
                CustomerId = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0,
                InfoHelpText = discountInfo.InfoHelpText,
                InfoText = discountInfo.InfoText,
                ProductId = discountInfo.ProductId,
                StoreWideDiscountId = discountInfo.StoreWideDiscountId,
                Discount = discountInfo.Discount
            });
        }
        public async System.Threading.Tasks.Task GetDiscountOnProducts(List<ProductDiscountInfo> productOfferinfo, StoreWideDiscountSetting setting)
        {
            List<int> productIds = new List<int>();
            if (setting.FullStore)
            {
                productIds = (await this._productService.GetAllProducts()).Select(p => p.Id).ToList();
            }
            if (!string.IsNullOrEmpty(setting.CategoryIds))
            {
                if (setting.FullStore)
                {
                    productIds = productIds.Except(await GetcategoriesProducts(setting.CategoryIds)).ToList();
                }
                else
                {
                    productIds = await GetcategoriesProducts(setting.CategoryIds);
                }
            }

            if (!string.IsNullOrEmpty(setting.ProductIds) && !setting.FullStore)
            {
                List<int> filteredProductIds = new List<int>();

                foreach (var productid in setting.ProductIds.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    int.TryParse(productid, out int _productId);
                    if (_productId != 0)
                    {
                        if (productIds.Count > 0)
                        {
                            if (productIds.Contains(_productId))
                            {
                                filteredProductIds.Add(_productId);
                            }
                        }
                        else
                        {
                            filteredProductIds.Add(_productId);
                        }

                    }
                }
                productIds = filteredProductIds;
            }

            foreach (var productId in productIds)
            {
                if (productOfferinfo.Where(o => o.ProductId == productId).Any())
                {
                    var productOffer = productOfferinfo.Where(o => o.ProductId == productId).First();
                    if (productOffer.Discount < (setting.IsDeleted ? 0 : setting.Discount))
                    {
                        productOffer.Discount = setting.Discount;
                        productOffer.InfoHelpText = setting.InfoHelpText;
                        productOffer.InfoText = setting.InfoText;
                        productOffer.StoreWideDiscountId = setting.StoreWideDiscountId;
                        productOffer.Threshold = setting.Threshold;
                    }


                }
                else
                {
                    productOfferinfo.Add(new ProductDiscountInfo()
                    {
                        Discount = setting.IsDeleted ? 0 : setting.Discount,
                        InfoHelpText = setting.IsDeleted ? "" : setting.InfoHelpText,
                        InfoText = setting.IsDeleted ? "" : setting.InfoText,
                        ProductId = productId,
                        StoreWideDiscountId = setting.StoreWideDiscountId,
                        Threshold = setting.Threshold
                    });
                }
            }


        }

        public async Task<List<int>> GetcategoriesProducts(string categoryIds)
        {
            List<int> productIds = new List<int>();

            foreach (var categoryId in categoryIds.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                int.TryParse(categoryId, out int _categoryId);
                if (_categoryId != 0)
                {
                    var category = await _categoryService.GetCategoryByIdAsync(_categoryId);
                    if (category != null)
                    {
                        await GetCategoryProducts(_categoryId, productIds);

                        var childCategories = await _categoryService.GetAllCategoriesByParentCategoryIdAsync(_categoryId);
                        foreach (var childCategory in childCategories)
                        {
                            await GetCategoryProducts(childCategory.Id, productIds);

                        }
                    }

                }
            }


            return productIds;
        }

        public async Task GetCategoryProducts(int categoryid, List<int> productIds)
        {
            int pageIndex = 0;
            int pageSize = 50;
            bool hasNextPage = false;
            do
            {

                var result = await this._categoryService.GetProductCategoriesByCategoryIdAsync(categoryid, pageIndex: pageIndex, pageSize: pageSize, showHidden: true);
                hasNextPage = result.HasNextPage;

                foreach (var product in result)
                {
                    if (!productIds.Where(p => p == product.ProductId).Any())
                    {
                        productIds.Add(product.ProductId);
                    }
                }
                pageIndex++;
            } while (hasNextPage);
        }

        public async System.Threading.Tasks.Task InsertLog(string shortMessage, string message, LogLevel logLevel, bool hardEnableLog = false)
        {
            if (enableLog || hardEnableLog)
            {
                await _loggerService.InsertLogAsync(logLevel, shortMessage, message);
            }
        }

        #endregion

    }

    public class ProductDiscountInfo
    {
        public int ProductId { get; set; }
        public decimal Discount { get; set; }
        public string InfoHelpText { get; set; }
        public string InfoText { get; set; }
        public int StoreWideDiscountId { get; set; }
        public decimal Threshold { get; set; }
    }

}