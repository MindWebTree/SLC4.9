using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Data.Discounts;
using MWT.Nop.Core.Domain.ProductBundle;
using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Services.Orders;
using MWT.Plugin.Misc.MwtStorefront.Factories;
using MWT.Plugin.Misc.MwtStorefront.Models.Catalog;
using MWT.Plugin.Misc.MwtStorefront.Models.ProductBundle;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Media;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Media;

namespace MWT.Plugin.Misc.MwtStorefront.Components
{
    [ViewComponent(Name = "ProductBundle")]
    public class ProductBundleViewComponent : NopViewComponent
    {
        #region Fields

        private readonly IProductBundleModelFactory _bundleService;
        private readonly ICustomProductModelFactory _productModelFactory;
        private readonly IPictureService _pictureService;
        private readonly ILocalizationService _localizationService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductExtendedService _productService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ISettingService _settingService;
        private readonly IShoppingCartExtendedService _shoppingCartService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly MediaSettings _mediaSettings;
        private ILogger _logger;

        #endregion

        #region Ctor

        public ProductBundleViewComponent(IProductBundleModelFactory bundleService,
            IProductAttributeService productAttributeService,
            ILocalizationService localizationService,
            IProductExtendedService productService,
            IPictureService pictureService,
            ICustomProductModelFactory productModelFactory,
            IProductAttributeParser productAttributeParser,
            IPriceFormatter priceFormatter,
            ISettingService settingService,
            IShoppingCartExtendedService shoppingCartService,
            IStaticCacheManager staticCacheManager,
            IWebHelper webHelper,
            IWorkContext workContext,
            IStoreContext storeContext,
            MediaSettings mediaSettings,
            ILogger logger)
        {
            _bundleService = bundleService;
            _productAttributeService = productAttributeService;
            _localizationService = localizationService;
            _productService = productService;
            _pictureService = pictureService;
            _productModelFactory = productModelFactory;
            _productAttributeParser = productAttributeParser;
            _priceFormatter = priceFormatter;
            _settingService = settingService;
            _shoppingCartService = shoppingCartService;
            _staticCacheManager = staticCacheManager;
            _webHelper = webHelper;
            _workContext = workContext;
            _storeContext = storeContext;
            _mediaSettings = mediaSettings;
            _logger = logger;
        }

        #endregion

        #region Methods

        public async Task<IViewComponentResult> InvokeAsync(CustomProductDetailsModel additionalData)
        {
            if (additionalData is not CustomProductDetailsModel productDetailsModel)
                return Content("");
            try
            {
                var isConnectionSecured = _webHelper.IsCurrentConnectionSecured();
                var workingLanguage = await _workContext.GetWorkingLanguageAsync();
                var currentStore = await _storeContext.GetCurrentStoreAsync();
                var mainProductId = productDetailsModel.Id;

                var selectedVariantId = productDetailsModel.VariantId == 0
                    ? productDetailsModel.DefaultVariantId
                    : productDetailsModel.VariantId;


                var bundleWidgetCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(
       CustomNopCatalogDefaults.BundleWidgetModelKey,
       productDetailsModel.Id, selectedVariantId, workingLanguage, currentStore);

                var model = await _staticCacheManager.GetAsync(bundleWidgetCacheKey, async () =>
                {

                    var mainProduct = await _productService.GetProductByIdAsync(mainProductId);

                    var activeBundles = (await _bundleService.GetBundlesByProductIdAsync(mainProductId))
                        .Where(bundle => bundle.IsActive)
                        .ToList();

                    if (!activeBundles.Any())
                        return new ProductConfigurationBundleModel();

                    var validBundles = new List<BundleConfiguration>();
                    var allBundleItems = new List<BundleItem>();

                    foreach (var bundle in activeBundles)
                    {
                        var bundleItems = await _bundleService.GetBundleItemsAsync(bundle.Id);
                        var isValid = await _bundleService.IsBundleValid(bundle);
                        if (isValid)
                        {
                            validBundles.Add(bundle);
                            allBundleItems.AddRange(bundleItems);
                        }
                    }

                    if (!validBundles.Any())
                        return new ProductConfigurationBundleModel();

                        var uniqueProductIds = allBundleItems.Select(item => item.ProductId).Distinct().ToArray();
                    var bundleProducts = await _productService.GetProductsByIdsAsync(uniqueProductIds);

                    var productOverviewModels = await _productModelFactory.PrepareCustomProductOverviewDetailInfoModelAsync(
                        bundleProducts, true, true, null, false, false, false, false, false, false, 0);

                    var initialBundle = validBundles.FirstOrDefault(bundle => bundle.VariantId == selectedVariantId)
                                        ?? validBundles.First();
                    var initialBundleItems = allBundleItems.Where(item => item.BundleId == initialBundle.Id).ToList();

                    var bundleManifest = new List<object>();


                    decimal defaultClubbedPrice = 0;
                    decimal defaultClubbedOldPrice = 0;
                    decimal defaultBundlePrice = 0;
                    foreach (var bundle in validBundles)
                    {
                        var currentBundleItems = allBundleItems.Where(item => item.BundleId == bundle.Id).ToList();
                        var bundleVariant = await _productService.GetVariantByVariantId(bundle.VariantId);

                        var bundleVariantCombination = await _bundleService.GetBestMatchingCombinationAsync(bundleVariant);
                        var bundletPrice = (bundleVariantCombination?.OverriddenPrice ?? 0) > 0
                                  ? bundleVariantCombination.OverriddenPrice.Value
                                  : mainProduct.Price;
                        var itemMetadataList = new List<object>();

                        if (currentBundleItems.Count > 0)
                        {
                            foreach (var bundleItem in currentBundleItems)
                            {
                                var itemVariant = await _productService.GetVariantByVariantId(bundleItem.VariantId);

                                var matchedCombination = await _bundleService.GetBestMatchingCombinationAsync(itemVariant);
                                var baseProduct = bundleProducts.First(product => product.Id == bundleItem.ProductId);

                                var currentPrice = (matchedCombination?.OverriddenPrice ?? 0) > 0
                                    ? matchedCombination.OverriddenPrice.Value
                                    : baseProduct.Price;

                                var oldPrice = matchedCombination?.OverriddenOldPrice.GetValueOrDefault() > 0
                                    ? matchedCombination.OverriddenOldPrice.GetValueOrDefault()
                                    : currentPrice;

                                var discountPercentage = 0;
                                if (oldPrice > currentPrice && oldPrice > 0)
                                {
                                    discountPercentage = (int)Math.Round((1 - (currentPrice / oldPrice)) * 100);
                                }
                                string dimension = string.Empty;
                                foreach (var attrValueIdStr in itemVariant.ProductAttributeValueIds.Split('-'))
                                {
                                    int attrValueId = int.TryParse(attrValueIdStr, out var parsed) ? parsed : 0;
                                    if (attrValueId > 0)
                                    {
                                        var prdAttrValue = await _productAttributeService.GetProductAttributeValueByIdAsync(attrValueId);
                                        if (!string.IsNullOrEmpty(prdAttrValue.Dimension))
                                        {
                                            dimension = prdAttrValue.Dimension;
                                            break;
                                        }
                                    }
                                }

                                itemMetadataList.Add(new
                                {



                                    productId = bundleItem.ProductId,
                                    qnty = bundleItem.Quantity,
                                    variantId = bundleItem.VariantId,
                                    price = await _priceFormatter.FormatPriceAsync(currentPrice),
                                    oldPrice = await _priceFormatter.FormatPriceAsync(oldPrice),
                                    priceraw = currentPrice,
                                    oldPriceraw = oldPrice,
                                    discountPct = discountPercentage,
                                    dimension = dimension
                                });
                                if (bundle.Id == initialBundle.Id)
                                {
                                    defaultClubbedPrice += currentPrice * bundleItem.Quantity;
                                    defaultClubbedOldPrice += oldPrice * bundleItem.Quantity;

                                }
                            }



                            var bundlePictureCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(
                                CustomNopCatalogDefaults.BundlePictureModelKey,
                                bundle, _mediaSettings.CategoryThumbPictureSize, true, workingLanguage,
                                isConnectionSecured, currentStore);

                            bundleManifest.Add(new
                            {
                                bundleId = bundle.Id,
                                imageurl = await _staticCacheManager.GetAsync(bundlePictureCacheKey, async () =>
                                {
                                    var picture = await _pictureService.GetPictureByIdAsync(bundle.PictureId);
                                    string fullSizeImageUrl, imageUrl;
                                    (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                                    (imageUrl, _) = await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.CategoryThumbPictureSize);

                                    var imageTitleFormat = await _localizationService.GetResourceAsync("Media.Category.ImageLinkTitleFormat");
                                    var imageAltTextFormat = await _localizationService.GetResourceAsync("Media.Category.ImageAlternateTextFormat");

                                    return new PictureModel
                                    {
                                        FullSizeImageUrl = fullSizeImageUrl,
                                        ImageUrl = imageUrl,
                                        Title = string.Format(imageTitleFormat, bundle.Name),
                                        AlternateText = string.Format(imageAltTextFormat, bundle.Name)
                                    };
                                }),
                                displayonproductpage = bundle.DisplayOnProductPage,
                                variantId = bundle.VariantId,
                                price = await _priceFormatter.FormatPriceAsync(bundletPrice),
                                priceraw = bundletPrice,
                                metadata = itemMetadataList,

                            });
                            if (bundle.Id == initialBundle.Id)
                                defaultBundlePrice = bundletPrice;
                        }
                    }



                    var model = new ProductConfigurationBundleModel
                    {
                        ProductOverviewModels = productOverviewModels.ToList(),
                        BundleManifestJson = JsonConvert.SerializeObject(bundleManifest),
                        InitialProductIds = initialBundleItems.Select(item => item.ProductId).ToList(),
                        InitialItems = initialBundleItems,
                        SelectedVariantId = selectedVariantId
                    };


                    var offer = defaultClubbedOldPrice - defaultClubbedPrice;
                    var bundleDiscount = defaultClubbedPrice - defaultBundlePrice;
                    var totalSaving = offer + bundleDiscount;

                    model.DefaultBundlePrice = await _priceFormatter.FormatPriceAsync(defaultBundlePrice);
                    model.Offer = await _priceFormatter.FormatPriceAsync(offer);
                    model.BundleDiscount = await _priceFormatter.FormatPriceAsync(bundleDiscount);
                    model.TotalSaving = await _priceFormatter.FormatPriceAsync(totalSaving);
                    model.BMSMDiscount = await _priceFormatter.FormatPriceAsync(0);

                    var buyMoreSaveMoreConfiguration = await _shoppingCartService.GetBuyMoreSaveMoreDiscountConfiguration();
                    CustomDiscountType discountType = CustomDiscountType.Fixed;
                    decimal buyMoreSaveMoreDiscount = 0;
                    var buyMoreSaveMoreSingleItemThreshold = await _settingService.GetSettingByKeyAsync<decimal>(
                  "MarketingSettings.ApplyBuyMoreSaveMoreOnSingleItemOverThreshold");

                    if (!string.IsNullOrEmpty(buyMoreSaveMoreConfiguration))
                    {
                        buyMoreSaveMoreDiscount =
                       buyMoreSaveMoreConfiguration.Contains("%")
                           ? decimal.Parse(buyMoreSaveMoreConfiguration.Replace("%", ""))
                           : decimal.Parse(
                               buyMoreSaveMoreConfiguration.Replace("$", ""));
                        discountType = buyMoreSaveMoreConfiguration.Contains("%") ? CustomDiscountType.Percent : CustomDiscountType.Fixed;
                    }

                    model.BuyMoreSaveMoreDiscount = buyMoreSaveMoreDiscount;
                    model.SingleItemThreshold = buyMoreSaveMoreSingleItemThreshold;
                    model.DiscountType = discountType.ToString();
                    model.EnableQuickView = mainProduct.EnableBundleQuickView;
                    return model;
                });

                if (model.ProductOverviewModels == null || model.ProductOverviewModels.Count() == 0)
                    return Content(string.Empty);
                return View(model);
            }
            catch (Exception exp)
            {
                await _logger.InsertLogAsync(LogLevel.Error, $"Failed to display Bundle for {productDetailsModel.Id}", exp.Message);
            }
            return Content(string.Empty);
        }

        #endregion
    }
}