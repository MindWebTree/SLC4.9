
using LinqToDB;
using MWT.Nop.Core.Data.Discounts;
using MWT.Nop.Core.Domain.ProductBundle;
using MWT.Nop.Core.Infrastructure;
using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Services.BundleProduct;
using MWT.Nop.Core.Services.Orders;
using MWT.Plugin.Misc.MwtStorefront.Area.Admin.Models;
using MWT.Plugin.Misc.MwtStorefront.Area.Admin.Models.Variant;
using MWT.Plugin.Misc.MwtStorefront.Area.Admin.Variant;
using MWT.Plugin.Misc.MwtStorefront.EventConsumer;
using MWTNop.Core.Domain.Catalog;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Logging;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Web.Framework.Models.Extensions;
using System.Net;
using System.Text;


namespace MWT.Plugin.Misc.MwtStorefront.Factories
{
    public class ProductBundleModelFactory : IProductBundleModelFactory
    {
        private readonly IRepository<BundleConfiguration> _bundleConfigRepository;
        private readonly IRepository<BundleItem> _bundleItemRepository;
        private readonly IRepository<VariantPriceBackup> _variantbackupRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IProductExtendedService _productService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeService _productAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IShoppingCartExtendedService _shoppingCartService;
        private readonly ISettingService _settingService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IWorkContext _workContext;
        private readonly ProductBundleSettings _productBundleSettings;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IRepository<ProductAttributeMapping> _productAttributeMappingRepository;
        private readonly IRepository<BundleAuditLog> _bundleAuditLogRepository;
        private IProductAttributeFormatter _productAttributeFormatter;
        private readonly IBundleLoggerService _bundleLoggerService;
        private readonly ILogger _logger;

        public ProductBundleModelFactory(
            IRepository<BundleConfiguration> bundleConfigRepository,
            IRepository<BundleItem> bundleItemRepository,
            IRepository<Product> productRepository,
            IProductExtendedService productService,
            IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService,
            ILocalizationService localizationService,
            IShoppingCartExtendedService shoppingCartService,
            ISettingService settingService,
            IPriceCalculationService priceCalculationService,
            IWorkContext workContext,
            ProductBundleSettings productBundleSettings,
             IStaticCacheManager staticCacheManager,
              IRepository<ProductAttributeMapping> productAttributeMappingRepository,
              IProductAttributeFormatter productAttributeFormatter,
              IRepository<VariantPriceBackup> variantbackupRepository,
              IBundleLoggerService bundleLoggerService,
              IRepository<BundleAuditLog> bundleAuditLogRepository,
              ILogger logger
            )
        {
            _bundleConfigRepository = bundleConfigRepository;
            _bundleItemRepository = bundleItemRepository;
            _productRepository = productRepository;
            _productService = productService;
            _productAttributeParser = productAttributeParser;
            _productAttributeService = productAttributeService;
            _localizationService = localizationService;
            _shoppingCartService = shoppingCartService;
            _settingService = settingService;
            _priceCalculationService = priceCalculationService;
            _workContext = workContext;
            _staticCacheManager = staticCacheManager;
            _productAttributeMappingRepository = productAttributeMappingRepository;
            _productBundleSettings = productBundleSettings;
            _productAttributeFormatter = productAttributeFormatter;
            _variantbackupRepository = variantbackupRepository;
            _bundleLoggerService = bundleLoggerService;
            _bundleAuditLogRepository = bundleAuditLogRepository;
            _logger = logger;
        }

        public async Task<IList<BundleConfiguration>> GetBundlesByProductIdAsync(int productId)
        {
            return await _bundleConfigRepository.Table
                .Where(x => x.ProductId == productId && x.IsActive).ToListAsync();
        }

        public async Task<IList<BundleConfiguration>> GetAllBundlesByProductIdAsync(int productId)
        {
            return await _bundleConfigRepository.Table
                .Where(x => x.ProductId == productId).ToListAsync();
        }
        public async Task<IPagedList<Product>> GetBundleProductsAsync(int pageIndex, int pageSize)
        {
            var query = from p in _productRepository.Table
                        join bc in _bundleConfigRepository.Table on p.Id equals bc.ProductId
                        where !p.Deleted
                           && p.Published
                           && bc.IsActive
                           && p.IsBundleProduct
                           && _bundleItemRepository.Table.Any(item => item.BundleId == bc.Id && item.IsActive)

                        select p;

            query = query.Distinct().OrderBy(x => x.Name);
            return await query.ToPagedListAsync(pageIndex, pageSize);
        }
        public async Task<BundleConfiguration> GetBundleByIdAsync(int bundleId)
        {
            return await _bundleConfigRepository.GetByIdAsync(bundleId);
        }



        public async Task InsertBundleAsync(BundleConfiguration bundle)
        {

            await _bundleConfigRepository.InsertAsync(bundle);
            await _bundleLoggerService.LogBundleCreatedAsync(bundle);
        }

        public async Task UpdateBundleAsync(BundleConfiguration bundle)
        {
            await _bundleConfigRepository.UpdateAsync(bundle);
        }


        public async Task DisableBundle(int variantId)
        {
            var bundle = await GetBundleByVariantIdAsync(variantId);
            bundle.DeletedOnUtc = DateTime.UtcNow;
            bundle.DeletedBy = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0;
            bundle.IsActive = false;
            await UpdateBundleAsync(bundle);

            var variant = await _productService.GetVariantByVariantId(variantId);
            await _bundleLoggerService.LogBundleDeletedAsync(bundle, variant);
        }
        public async Task DeleteBundleAsync(BundleConfiguration bundle)
        {

            await _bundleConfigRepository.DeleteAsync(bundle);
        }

        public async Task<IList<BundleItem>> GetBundleItemsAsync(int bundleId)
        {
            var _items = await _bundleItemRepository.Table
                .Where(x => x.BundleId == bundleId && x.IsActive)
                .ToListAsync();
            List<BundleItem> items = new List<BundleItem>();
            foreach (var _item in _items)
            {
                if (await IsVariantValid(_item.VariantId))
                {
                    items.Add(_item);
                }
            }
            return items;
        }

        public async Task<BundleItem> GetBundleItemsexistAsync(int productId, int bundleId, int variantId)
        {
            return await _bundleItemRepository.Table
                .FirstOrDefaultAsync(x => x.BundleId == bundleId && x.ProductId == productId && x.VariantId == variantId && x.IsActive);
        }
        public async Task InsertBundleItemAsync(BundleItem item)
        {
            await _bundleItemRepository.InsertAsync(item);
        }

        public async Task DeleteBundleItemAsync(BundleItem item)
        {
            await _bundleItemRepository.DeleteAsync(item);
        }
        public async Task UpdateBundleItemAsync(BundleItem bundle, bool publishEvent = true)
        {
            await _bundleItemRepository.UpdateAsync(bundle, publishEvent);
        }
        public async Task<BundleItem> GetBundleItemByIdAsync(int Id)
        {
            return await _bundleItemRepository.GetByIdAsync(Id);
        }


        public async Task CalculateBundlePrice(Product product)
        {
            //      CustomDiscountType discountType = CustomDiscountType.Fixed;
            var productVariants = await _productService.GetProductVariants(product.Id);
            //       decimal singleItemThreshold = await _settingService.GetSettingByKeyAsync<decimal>("MarketingSettings.ApplyBuyMoreSaveMoreOnSingleItemOverThreshold");
            //       List<int> existingBackupVariantIds = await GetExistingBackupVariantIdsAsync(product.Id);
            foreach (var variant in productVariants)
            {
                var bundle = await GetBundleByVariantIdAsync(variant.VariantId);
                if (bundle != null)
                {
                    bool isValid = await IsBundleValid(bundle);
                    if (isValid)
                        await CalculateSingleBundlePrice(bundle);
                    else await ValidateAndRestoreBundleVariantAsync(bundle);
                }



                #region Commented

                //var bundleItems = await this.GetBundleItemsAsync(bundle.Id);
                //decimal totalPrice = 0;
                //decimal totalOldPrice = 0;
                //decimal totalMSRP = 0;

                //List<BundleItemModel> items = new List<BundleItemModel>();
                //foreach (var item in bundleItems)
                //{
                //    var itemProduct = await _productService.GetProductByIdAsync(item.ProductId);
                //    if (itemProduct == null)
                //        continue;
                //    var variantdetails = await _productService.GetVariantByVariantId(item.VariantId);

                //    var productattributeCombination = await GetBestMatchingCombinationAsync(variantdetails);


                //    if (productattributeCombination != null)
                //    {
                //        items.Add(new BundleItemModel()
                //        {
                //            ProductId = itemProduct.Id,
                //            Price = ((decimal)(productattributeCombination.OverriddenPrice > 0 ? productattributeCombination.OverriddenPrice : itemProduct.Price)),
                //            OldPrice = (decimal)((productattributeCombination.OverriddenOldPrice ?? 0) > 0 ? (decimal)productattributeCombination.OverriddenOldPrice
                //               : ((productattributeCombination.OverriddenPrice) > 0 ? productattributeCombination.OverriddenPrice : (itemProduct.OldPrice > 0 ? itemProduct.OldPrice : itemProduct.Price))),
                //            MSRP = (productattributeCombination.OverriddenMsrp ?? itemProduct.Msrp),
                //            Quantity = item.Quantity,
                //            Id = item.Id
                //        });
                //    }
                //    else
                //    {
                //        items.Add(new BundleItemModel()
                //        {
                //            ProductId = itemProduct.Id,
                //            Price = itemProduct.Price,
                //            OldPrice = itemProduct.OldPrice > 0 ? itemProduct.OldPrice : itemProduct.Price,
                //            MSRP = itemProduct.Msrp,
                //            Quantity = item.Quantity,
                //            Id = item.Id
                //        });
                //    }
                //}

                //if (items.Sum(itm => itm.Price) <= 0)
                //    continue;

                //int totalQty = bundleItems.Sum(x => x.Quantity);
                //if (bundle.NoOfPieces != totalQty)
                //    continue;
                //if (items.Count > 1 || totalQty > 1)
                //{



                //    var buyMoreSaveMoreConfiguration = await _shoppingCartService.GetBuyMoreSaveMoreDiscountConfiguration();
                //    decimal buyMoreSaveMoreSingleItemThreshold = await _settingService.GetSettingByKeyAsync<decimal>("MarketingSettings.ApplyBuyMoreSaveMoreOnSingleItemOverThreshold");
                //    if (!string.IsNullOrEmpty(buyMoreSaveMoreConfiguration))
                //    {
                //        decimal buyMoreSaveMoreDiscount =
                //        buyMoreSaveMoreConfiguration.Contains("%")
                //            ? decimal.Parse(buyMoreSaveMoreConfiguration.Replace("%", ""))
                //            : decimal.Parse(
                //                buyMoreSaveMoreConfiguration.Replace("$", ""));
                //        discountType = buyMoreSaveMoreConfiguration.Contains("%") ? CustomDiscountType.Percent : CustomDiscountType.Fixed;

                //        int notElgibleForSavemoreDiscountCartId = 0;
                //        decimal costlyItemPrice = 0;
                //        decimal itemPrice = 0;
                //        foreach (var item in items)
                //        {
                //            itemPrice = item.Price;
                //            if (((itemPrice * item.Quantity) > costlyItemPrice && items.Count > 1) ||
                //                (items.Count == 1 && (items.Sum(c => c.Quantity) == 1 || itemPrice < buyMoreSaveMoreSingleItemThreshold)))
                //            {

                //                costlyItemPrice = itemPrice * item.Quantity;
                //                notElgibleForSavemoreDiscountCartId = item.Id;
                //            }
                //        }
                //        foreach (var item in items)
                //        {
                //            if (item.Id != notElgibleForSavemoreDiscountCartId)
                //            {
                //                decimal buyMoreSaveMoreDiscountBase = 0;
                //                if (items.Count > 1)
                //                {
                //                    buyMoreSaveMoreDiscountBase =
                //                discountType == CustomDiscountType.Percent ? (((item.Price * item.Quantity) * buyMoreSaveMoreDiscount) / 100)
                //                : (buyMoreSaveMoreDiscount > item.Price ? item.Price * item.Quantity :
                //                                      buyMoreSaveMoreDiscount * item.Quantity);
                //                }
                //                else
                //                {
                //                    buyMoreSaveMoreDiscountBase =
                //          discountType == CustomDiscountType.Percent ?
                //             (((item.Price * (item.Quantity - 1)) * buyMoreSaveMoreDiscount) / 100) :
                //                                    (buyMoreSaveMoreDiscount > item.Price ? item.Price * (item.Quantity - 1) :
                //                                    buyMoreSaveMoreDiscount * (item.Quantity - 1));
                //                }
                //                item.BuyMoreSaveMoreDiscountBase = buyMoreSaveMoreDiscountBase;
                //            }

                //        }

                //    }
                //}


                //foreach (var item in items)
                //{
                //    var ItemdiscountPercent = ((item.OldPrice - item.Price) / item.OldPrice) * 100;
                //    item.DiscountPercentage = (int)ItemdiscountPercent;

                //    item.Price = ((item.Price * item.Quantity) - item.BuyMoreSaveMoreDiscountBase);
                //    item.OldPrice = item.OldPrice * item.Quantity;
                //    item.MSRP = item.MSRP * item.Quantity;

                //}

                ////totalOldPrice = items.Sum(x => x.OldPrice);
                //totalPrice = items.Sum(x => x.Price);
                //totalMSRP = items.Sum(x => x.MSRP);
                //var maxDiscountPercentage = items.Max(x => x.DiscountPercentage);

                ////        if ((_productBundleSettings.DiscountPercentage != null
                ////? _productBundleSettings.DiscountPercentage
                ////: 0) > 0)
                ////        {
                ////            totalPrice = totalPrice - (totalPrice * (_productBundleSettings.DiscountPercentage / 100));
                ////        }

                //if (!existingBackupVariantIds.Contains(variant.VariantId))
                //{
                //    var productattributeCombination = await GetBestMatchingCombinationAsync(variant);
                //    decimal price = productattributeCombination.OverriddenPrice ?? 0;
                //    decimal oldPrice = productattributeCombination.OverriddenOldPrice ?? 0;
                //    decimal msrp = productattributeCombination.OverriddenMsrp ?? 0;

                //    var backupRecord = new VariantPriceBackup
                //    {
                //        VariantId = variant.VariantId,
                //        ProductId = product.Id,
                //        Price = price > 0 ? price : product.Price,
                //        OldPrice = oldPrice > 0 ? oldPrice : product.OldPrice,
                //        Msrp = msrp > 0 ? msrp : product.Msrp,
                //        BackupDateUtc = DateTime.UtcNow
                //    };
                //    await _variantbackupRepository.InsertAsync(backupRecord);
                //}

                //try
                //{
                //    BundleEventContext.SkipRecalculation.Value = true;
                //    decimal? oldPrice = variant.Price;
                //    totalOldPrice = CommonHelper.RoundToNearest49or99(totalPrice + (totalPrice * maxDiscountPercentage / 100));
                //    totalPrice = totalOldPrice - (totalOldPrice * maxDiscountPercentage / 100);

                //    variant.Msrp = CommonHelper.RoundToNearest49or99(totalMSRP);
                //    variant.OldPrice = totalOldPrice;
                //    variant.Price = (totalPrice % 1 >= 0.5m) ? Math.Ceiling((decimal)totalPrice) : Math.Floor((decimal)totalPrice);

                //    await _productService.UpdateVariant(variant);
                //    await _bundleLoggerService.LogBundleUpdatedAsync(bundle, bundle, bundleItems.ToList(), oldPrice, variant.Price);
                //}
                //finally
                //{
                //    BundleEventContext.SkipRecalculation.Value = false;
                //}

                #endregion
            }
        }
        // made function for single bundle wise not all 
        public async Task CalculateSingleBundlePrice(BundleConfiguration bundle)
        {
            CustomDiscountType discountType = CustomDiscountType.Fixed;
            var variant = await _productService.GetVariantByVariantId(bundle.VariantId);
            if (variant == null)
                return;
            var product = await _productService.GetProductByIdAsync(bundle.ProductId);
            decimal singleItemThreshold = await _settingService.GetSettingByKeyAsync<decimal>("MarketingSettings.ApplyBuyMoreSaveMoreOnSingleItemOverThreshold");


            if (bundle == null || !bundle.IsActive)
                return;

            var bundleItems = await GetBundleItemsAsync(bundle.Id);

            decimal totalPrice = 0;
            decimal totalOldPrice = 0;
            decimal totalMSRP = 0;

            List<BundleItemModel> items = new List<BundleItemModel>();
            foreach (var item in bundleItems)
            {
                var itemProduct = await _productService.GetProductByIdAsync(item.ProductId);
                if (itemProduct == null)
                    continue;
                var variantdetails = await _productService.GetVariantByVariantId(item.VariantId);

                var productattributeCombination = await GetBestMatchingCombinationAsync(variantdetails);


                if (productattributeCombination != null)
                {
                    items.Add(new BundleItemModel()
                    {
                        ProductId = itemProduct.Id,
                        Price = (decimal)(productattributeCombination.OverriddenPrice > 0 ? productattributeCombination.OverriddenPrice : itemProduct.Price),
                        OldPrice = (decimal)((productattributeCombination.OverriddenOldPrice ?? 0) > 0 ? (decimal)productattributeCombination.OverriddenOldPrice
                           : productattributeCombination.OverriddenPrice > 0 ? productattributeCombination.OverriddenPrice : itemProduct.OldPrice > 0 ? itemProduct.OldPrice : itemProduct.Price),
                        MSRP = productattributeCombination.OverriddenMsrp ?? itemProduct.Msrp,
                        Quantity = item.Quantity,
                        Id = item.Id
                    });
                }
                else
                {
                    items.Add(new BundleItemModel()
                    {
                        ProductId = itemProduct.Id,
                        Price = itemProduct.Price,
                        OldPrice = itemProduct.OldPrice > 0 ? itemProduct.OldPrice : itemProduct.Price,
                        MSRP = itemProduct.Msrp,
                        Quantity = item.Quantity,
                        Id = item.Id
                    });
                }
            }

            if (items.Sum(itm => itm.Price) <= 0)
                return;

            int totalQty = bundleItems.Sum(x => x.Quantity);
            if (bundle.NoOfPieces != totalQty)
                return;
            if (items.Count > 1 || totalQty > 1)
            {



                var buyMoreSaveMoreConfiguration = await _shoppingCartService.GetBuyMoreSaveMoreDiscountConfiguration();
                decimal buyMoreSaveMoreSingleItemThreshold = await _settingService.GetSettingByKeyAsync<decimal>("MarketingSettings.ApplyBuyMoreSaveMoreOnSingleItemOverThreshold");
                if (!string.IsNullOrEmpty(buyMoreSaveMoreConfiguration))
                {
                    decimal buyMoreSaveMoreDiscount =
                    buyMoreSaveMoreConfiguration.Contains("%")
                        ? decimal.Parse(buyMoreSaveMoreConfiguration.Replace("%", ""))
                        : decimal.Parse(
                            buyMoreSaveMoreConfiguration.Replace("$", ""));
                    discountType = buyMoreSaveMoreConfiguration.Contains("%") ? CustomDiscountType.Percent : CustomDiscountType.Fixed;

                    int notElgibleForSavemoreDiscountCartId = 0;
                    decimal costlyItemPrice = 0;
                    decimal itemPrice = 0;
                    foreach (var item in items)
                    {
                        itemPrice = item.Price;
                        if (itemPrice * item.Quantity > costlyItemPrice && items.Count > 1 ||
                            items.Count == 1 && (items.Sum(c => c.Quantity) == 1 || itemPrice < buyMoreSaveMoreSingleItemThreshold))
                        {

                            costlyItemPrice = itemPrice * item.Quantity;
                            notElgibleForSavemoreDiscountCartId = item.Id;
                        }
                    }
                    foreach (var item in items)
                    {
                        if (item.Id != notElgibleForSavemoreDiscountCartId)
                        {
                            decimal buyMoreSaveMoreDiscountBase = 0;
                            if (items.Count > 1)
                            {
                                buyMoreSaveMoreDiscountBase =
                            discountType == CustomDiscountType.Percent ? item.Price * item.Quantity * buyMoreSaveMoreDiscount / 100
                            : buyMoreSaveMoreDiscount > item.Price ? item.Price * item.Quantity :
                                                  buyMoreSaveMoreDiscount * item.Quantity;
                            }
                            else
                            {
                                buyMoreSaveMoreDiscountBase =
                      discountType == CustomDiscountType.Percent ?
                         item.Price * (item.Quantity - 1) * buyMoreSaveMoreDiscount / 100 :
                                                buyMoreSaveMoreDiscount > item.Price ? item.Price * (item.Quantity - 1) :
                                                buyMoreSaveMoreDiscount * (item.Quantity - 1);
                            }
                            item.BuyMoreSaveMoreDiscountBase = buyMoreSaveMoreDiscountBase;
                        }

                    }

                }
            }


            foreach (var item in items)
            {
                var ItemdiscountPercent = (item.OldPrice - item.Price) / item.OldPrice * 100;
                item.DiscountPercentage = (int)ItemdiscountPercent;

                item.Price = item.Price * item.Quantity - item.BuyMoreSaveMoreDiscountBase;
                item.OldPrice = item.OldPrice * item.Quantity;
                item.MSRP = item.MSRP * item.Quantity;

            }

            //totalOldPrice = items.Sum(x => x.OldPrice);
            totalPrice = items.Sum(x => x.Price);
            totalMSRP = items.Sum(x => x.MSRP);
            var maxDiscountPercentage = items.Max(x => x.DiscountPercentage);

            //        if ((_productBundleSettings.DiscountPercentage != null
            //? _productBundleSettings.DiscountPercentage
            //: 0) > 0)
            //        {
            //            totalPrice = totalPrice - (totalPrice * (_productBundleSettings.DiscountPercentage / 100));
            //        }

            //getting if record exist 
            var existingBackupVariantId = await GetVariantPriceBackupAsync(bundle.VariantId);

            if (existingBackupVariantId == null)
            {
                var productattributeCombination = await GetBestMatchingCombinationAsync(variant);
                decimal price = productattributeCombination.OverriddenPrice ?? 0;
                decimal oldPrice = productattributeCombination.OverriddenOldPrice ?? 0;
                decimal msrp = productattributeCombination.OverriddenMsrp ?? 0;

                var backupRecord = new VariantPriceBackup
                {
                    VariantId = variant.VariantId,
                    ProductId = product.Id,
                    Price = price > 0 ? price : product.Price,
                    OldPrice = oldPrice > 0 ? oldPrice : product.OldPrice,
                    Msrp = msrp > 0 ? msrp : product.Msrp,
                    BackupDateUtc = DateTime.UtcNow
                };
                await _variantbackupRepository.InsertAsync(backupRecord);
            }

            try
            {
                // skipping true to guarding recursion events
                BundleEventContext.SkipRecalculation.Value = true;
                decimal? oldPrice = variant.Price;
                totalOldPrice = CustomCommonHelper.RoundToNearest49or99(totalPrice + totalPrice * maxDiscountPercentage / 100);
                totalPrice = totalOldPrice - totalOldPrice * maxDiscountPercentage / 100;

                variant.Msrp = CustomCommonHelper.RoundToNearest49or99(totalMSRP);
                variant.OldPrice = totalOldPrice;
                variant.Price = totalPrice % 1 >= 0.5m ? Math.Ceiling(totalPrice) : Math.Floor(totalPrice);

                await _productService.UpdateVariant(variant);
                await _bundleLoggerService.LogBundleUpdatedAsync(bundle, await IsBundleValid(bundle), bundleItems.ToList(), oldPrice, variant.Price);
                await UpdateBundleVariantIdsAsync(variant.VariantId, true);
            }
            finally
            {
                // skipping false after updated  
                BundleEventContext.SkipRecalculation.Value = false;
            }
        }
        public async Task ValidateAndRestoreIncompleteBundlesAsync()
        {
            var allBackups = await _variantbackupRepository.GetAllAsync(query => query);

            foreach (var backup in allBackups)
            {
                var bundle = await GetBundleByVariantIdAsync(backup.VariantId);
                if (bundle != null && !await IsBundleValid(bundle))
                    await ValidateAndRestoreBundleVariantAsync(bundle);
            }
        }

        public async Task ValidateAndRestoreBundleVariantAsync(BundleConfiguration bundle)
        {
            try
            {
                bool shouldRestore = false;
                var bundleItems = await GetBundleItemsAsync(bundle.Id);
                var itemList = bundleItems.ToList();

                if (!bundle.IsActive)
                {
                    shouldRestore = true;
                }
                else
                {
                    int actualTotalItems = itemList
                        .Where(x => x.IsActive)
                        .Sum(x => x.Quantity);

                    var bundleproductsDetails = await _productService.GetProductsByIdsAsync(itemList.Select(x => x.ProductId).ToArray());

                    bool isInvalidBundle = bundleproductsDetails.Any(x => x.Published == false);
                    bool isValid = await IsBundleValid(bundle);
                    if (!isValid || isInvalidBundle)
                    {
                        shouldRestore = true;
                    }
                }

                if (shouldRestore)
                {
                    var variant = await _productService.GetVariantByVariantId(bundle.VariantId);
                    decimal? oldPrice = variant?.Price;

                    await RestoreVariantPriceFromBackupAsync(bundle.VariantId);
                    var restoredVariant = await _productService.GetVariantByVariantId(bundle.VariantId);
                    decimal? newPrice = restoredVariant?.Price;
                    await _bundleLoggerService.LogBundleUpdatedAsync(bundle, false, itemList, oldPrice, newPrice);
                    if (variant != null)
                        await UpdateBundleVariantIdsAsync(variant.VariantId, false);
                }
            }
            catch (Exception ex)
            {
                await _logger.InsertLogAsync(LogLevel.Error, "Bundle Failed to restore product price", $"{bundle?.Id} {bundle?.VariantId}");
            }
        }
        public virtual async Task<List<BundleConfiguration>> GetActiveBundleByProductIdAsync(int productId)
        {
            if (productId == 0)
                return null;
            var query = _bundleConfigRepository.Table;
            try
            {
                var activeBundle = await query.Where(b => b.ProductId == productId && b.IsActive).ToListAsync();
                return activeBundle;

            }
            catch (Exception ex)
            {

                throw;
            }

        }
        public async Task UpdateBundleVariantIdsAsync(int variantId, bool isBundleEnabled)
        {
            var existingValue = _productBundleSettings.BundleVariantIds;

            var ids = (existingValue ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => int.TryParse(x, out var id) ? id : 0)
                .Where(x => x > 0)
                .ToHashSet();

            // Add only if not already present
            if (isBundleEnabled)
            {
                if (!ids.Contains(variantId))
                    ids.Add(variantId);
            }
            else
            {
                // Remove only if exists
                if (ids.Contains(variantId))
                    ids.Remove(variantId);
            }

            var updatedValue = string.Join(",", ids.OrderBy(x => x));

            _productBundleSettings.BundleVariantIds = updatedValue;
            await _settingService.SaveSettingAsync(_productBundleSettings);
        }
        #region Utilities
        public async Task<bool> IsBundleValid(BundleConfiguration bundle)
        {
            if (bundle == null) return false;
            if (!await IsVariantValid(bundle.VariantId))
                return false;
            var items = await GetBundleItemsAsync(bundle.Id);
            var product = await _productService.GetProductByIdAsync(bundle.ProductId);
            int totalItemQuantity = items.Sum(x => x.Quantity);
            return bundle.NoOfPieces == totalItemQuantity && bundle.IsActive && (product?.IsBundleProduct ?? false);
        }
        public async Task<ProductAttributeCombination> GetBestMatchingCombinationAsync(
   VariantCombination variant)
        {

            var variantValues = await _productAttributeParser.ParseProductAttributeValuesAsync(variant.Combination);
            var variantValueIds = variantValues.Select(v => v.Id).ToList();

            var productAttributeCombinations = await _productAttributeService.GetAllProductAttributeCombinationsAsync(variant.ProductId);

            if (!productAttributeCombinations.Any())
                return new ProductAttributeCombination();

            List<ProductAttributeCombination> matchedCombinations = new List<ProductAttributeCombination>();

            foreach (var combination in productAttributeCombinations)
            {
                var comboValues = await _productAttributeParser.ParseProductAttributeValuesAsync(combination.AttributesXml);
                var comboValueIds = comboValues.Select(v => v.Id).ToList();

                if (variantValueIds.All(vId => comboValueIds.Contains(vId)) &&
    comboValueIds.Count <= variantValueIds.Count + 1)
                {
                    matchedCombinations.Add(combination);
                }
            }

            return matchedCombinations.OrderByDescending(x => x.OverriddenPrice ?? 0).FirstOrDefault() ?? new ProductAttributeCombination();
        }
        //private async Task<List<BundleConfiguration>> FindMatchingConfigForVariant(
        //  int productId,
        //  IList<BundleConfiguration> bundleConfigs)
        //{
        //    var attributeMappings = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(productId);

        //    var configResourceName = await _localizationService.GetResourceAsync("Product.Attr.Configuration");

        //    List<ProductAttributeValue> configurationValues = new List<ProductAttributeValue>();

        //    foreach (var pam in attributeMappings)
        //    {
        //        var productAttribute = await _productAttributeService.GetProductAttributeByIdAsync(pam.ProductAttributeId);

        //        if (productAttribute != null &&
        //            productAttribute.Name.Equals(configResourceName, StringComparison.InvariantCultureIgnoreCase))
        //        {
        //            var values = await _productAttributeService.GetProductAttributeValuesAsync(pam.Id);
        //            configurationValues = values.Where(x => x.Published).ToList();
        //            break;
        //        }
        //    }

        //    var configurationValueIds = configurationValues.Select(x => x.Id).ToList();
        //    return bundleConfigs
        //        .Where(config => configurationValueIds.Contains(config.ProductAttributeValueId))
        //        .ToList();
        //}
        protected virtual async Task<string> PrepareProductAttributeMappingValidationRulesStringAsync(ProductAttributeMapping attributeMapping)
        {
            if (!attributeMapping.ValidationRulesAllowed())
                return string.Empty;

            var validationRules = new StringBuilder(string.Empty);
            if (attributeMapping.ValidationMinLength.HasValue)
            {
                validationRules.AppendFormat("{0}: {1}<br />",
                    await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.ValidationRules.MinLength"),
                    attributeMapping.ValidationMinLength);
            }

            if (attributeMapping.ValidationMaxLength.HasValue)
            {
                validationRules.AppendFormat("{0}: {1}<br />",
                    await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.ValidationRules.MaxLength"),
                    attributeMapping.ValidationMaxLength);
            }

            if (!string.IsNullOrEmpty(attributeMapping.ValidationFileAllowedExtensions))
            {
                validationRules.AppendFormat("{0}: {1}<br />",
                    await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.ValidationRules.FileAllowedExtensions"),
                    WebUtility.HtmlEncode(attributeMapping.ValidationFileAllowedExtensions));
            }

            if (attributeMapping.ValidationFileMaximumSize.HasValue)
            {
                validationRules.AppendFormat("{0}: {1}<br />",
                    await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.ValidationRules.FileMaximumSize"),
                    attributeMapping.ValidationFileMaximumSize);
            }

            if (!string.IsNullOrEmpty(attributeMapping.DefaultValue))
            {
                validationRules.AppendFormat("{0}: {1}<br />",
                    await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.ValidationRules.DefaultValue"),
                    WebUtility.HtmlEncode(attributeMapping.DefaultValue));
            }

            return validationRules.ToString();
        }


        #endregion
        #region Variantbackup



        public async Task<VariantPriceBackup> GetVariantPriceBackupAsync(int variantId)
        {
            return await _variantbackupRepository.Table.FirstOrDefaultAsync(X => X.VariantId == variantId);
        }

        public async Task RestoreVariantPriceFromBackupAsync(int variantId)
        {
            try
            {
                BundleEventContext.SkipRecalculation.Value = true;

                var backup = await GetVariantPriceBackupAsync(variantId);
                if (backup == null)
                    return;

                var variant = await _productService.GetVariantByVariantId(variantId);
                if (variant == null)
                    return;

                variant.Price = backup.Price;
                variant.OldPrice = backup.OldPrice;
                variant.Msrp = backup.Msrp;
                await _productService.UpdateVariant(variant);
                await DeleteVariantPriceBackupAsync(variantId);

            }
            finally
            {
                BundleEventContext.SkipRecalculation.Value = false;
            }
        }
        public async Task DeleteVariantPriceBackupAsync(int variantId)
        {
            var backup = await _variantbackupRepository.Table.FirstOrDefaultAsync(X => X.VariantId == variantId);
            await _variantbackupRepository.DeleteAsync(backup);
        }


        #endregion

        #region Variant
        public async Task<VariantBundleSearchListModel> PrepareVariantListModelAsync(VariantBundleSearchModel searchModel, Product product)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var variants = (await _productService.GetProductVariants(searchModel.ProductId)).ToPagedList(searchModel);
            var filteredVariants = new List<VariantCombination>();

            foreach (var variant in variants)
            {
                bool isVariantPublished = true;
                var attrIds = (variant.ProductAttributeValueIds ?? string.Empty)
                                .Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var idStr in attrIds)
                {
                    if (int.TryParse(idStr, out int valId))
                    {
                        var attrValue = await _productAttributeService.GetProductAttributeValueByIdAsync(valId);
                        if (attrValue == null || !attrValue.Published)
                        {
                            isVariantPublished = false;
                            break;
                        }
                    }
                }

                if (isVariantPublished)
                    filteredVariants.Add(variant);
            }
            var pagedVariants = filteredVariants.ToPagedList(searchModel);
            var productActiveBundle = await GetBundlesByProductIdAsync(product.Id);
            //prepare grid model
            return await new VariantBundleSearchListModel().PrepareToGridAsync(searchModel, variants, () =>
            {
                return pagedVariants.SelectAwait(async variant =>
                {
                    VariantBundleModel variantModel = new VariantBundleModel();
                    variantModel.Attributes = await _productAttributeFormatter.FormatAttributesAsync(product, variant.Combination);
                    variantModel.CreatedOn = variant.CreatedOn;
                    variantModel.UpdatedOn = variant.UpdatedOn;
                    variantModel.VariantId = variant.VariantId;
                    variantModel.Id = variant.Id;
                    variantModel.OldPrice = variant.OldPrice;
                    variantModel.Price = variant.Price;
                    variantModel.Msrp = variant.Msrp;
                    variantModel.WgsRequired = variant.WgsRequired;
                    variantModel.Title = variant.Title ?? product.Name;
                    variantModel.EnableSurcharge = variant.EnableSurcharge;
                    variantModel.EstimatedDeliveryDate = variant.EstimatedDeliveryDate;
                    variantModel.Published = true;
                    variantModel.ProductId = variant.ProductId;
                    variantModel.QueryParameter = variant.QueryParameter;
                    variantModel.Weight = variant.Weight;
                    variantModel.ManufacturerPartNumber = variant.ManufacturerPartNumber;
                    variantModel.DimensionPictureId = variant.DimensionPictureId;
                    var bundle = productActiveBundle.FirstOrDefault(x => x.VariantId == variant.VariantId);
                    variantModel.HasBundleForProduct = bundle != null;
                    if (bundle != null)
                    {
                        var bundleItems = await GetBundleItemsAsync(bundle.Id);
                        variantModel.IsValidBundle = await IsBundleValid(bundle); ;
                    }

                    return variantModel;

                });
            });

        }
        public async Task<BundleConfiguration> GetBundleByVariantIdAsync(int variantId)
        {
            if (variantId == 0)
                return null;

            return await _bundleConfigRepository.Table
                .FirstOrDefaultAsync(bc => bc.VariantId == variantId);
        }

        public async Task<BundleItem> GetBundleItemByProductIdAsync(int bundleId, int productId)
        {
            return await _bundleItemRepository.Table.FirstOrDefaultAsync(x => x.BundleId == bundleId && x.ProductId == productId && x.IsActive);
        }

        public async Task<List<BundleConfiguration>> GetAffectedBundlesByProductId(int productId)
        {
            var bundleIds = await _bundleItemRepository.Table
                .Where(x => x.ProductId == productId)
                .Select(x => x.BundleId)
                .Distinct()
                .ToListAsync();

            var bundles = await _bundleConfigRepository.Table
                .Where(x => bundleIds.Contains(x.Id))
                .ToListAsync();

            return bundles;
        }

        private async Task<bool> IsVariantValid(int variantId)
        {
            bool isValid = false;
            var variant = await _productService.GetVariantByVariantId(variantId);
            if (variant != null)
            {
                var product = await _productService.GetProductByIdAsync(variant.ProductId);
                isValid = product?.Published ?? false && !(product?.Deleted ?? true);
                if (isValid)
                {
                    var attrValueIds = (variant.ProductAttributeValueIds ?? string.Empty)
                        .Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var idStr in attrValueIds)
                    {
                        if (int.TryParse(idStr, out int valId))
                        {
                            var attrValue = await _productAttributeService.GetProductAttributeValueByIdAsync(valId);

                            if (attrValue == null || !attrValue.Published)
                            {
                                isValid = false;
                                break;
                            }
                            else
                            {
                                var mapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(attrValue.ProductAttributeMappingId);
                                if (mapping == null)
                                {
                                    isValid = false;
                                    break;
                                }
                            }
                        }
                    }
                }

            }
            return isValid;
        }
        #endregion

    }
}