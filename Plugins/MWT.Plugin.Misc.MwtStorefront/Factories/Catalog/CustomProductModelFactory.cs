
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using MWT.Nop.Core.Infrastructure;
using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Service.Discount;
using MWT.Nop.Core.Services.Catalog;
using MWT.Nop.Core.Services.Configuration;
using MWT.Nop.Core.Services.Media;
using MWT.Nop.Core.Services.Shared;
using MWT.Plugin.Misc.MwtStorefront.Models.Api;
using MWT.Plugin.Misc.MwtStorefront.Models.Catalog;
using MWT.Plugin.Misc.MwtStorefront.Models.Media;
using MWTNop.Core.Domain.Catalog;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping.Date;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;


namespace MWT.Plugin.Misc.MwtStorefront.Factories.Catalog
{
    /// <summary>
    /// Represents the product model factory
    /// </summary>
    /// </summary>
    public partial class CustomProductModelFactory : ProductModelFactory, ICustomProductModelFactory
    {
        private readonly ICustomProductAttributeService _customProductAttributeService;
        private readonly ICustomPictureService _customPictureService;
        private readonly ICustomSpecificationAttributeService _customSpecificationAttributeService;
        private readonly ICustomShoppingCartService _customShoppingCartService;
        private readonly ICustomProductService _customProductService;
        private readonly ISettingService _settingService;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly ICustomProductAttributeFormatter _customProductAttributeFormatter;
        private readonly TagAutomationSettings _tagAutomationSettings;
        private readonly IStoreWideDiscountService _storeWideDiscountService;
        private readonly IGroupedProductConfigurationService _groupedProductConfigurationService;
        private readonly IRepository<ProductPicture> _productPictureRepository;
        private readonly ICommonService _commonService;

        public CustomProductModelFactory(CaptchaSettings captchaSettings,
            CatalogSettings catalogSettings, CustomerSettings customerSettings,
            ICategoryService categoryService, ICurrencyService currencyService,
            ICustomerService customerService, ICustomWishlistService customWishlistService,
            IDateRangeService dateRangeService, IDateTimeHelper dateTimeHelper,
            IDownloadService downloadService, IGenericAttributeService genericAttributeService,
            IJsonLdModelFactory jsonLdModelFactory, ILocalizationService localizationService,
            IManufacturerService manufacturerService, IPermissionService permissionService,
            IPictureService pictureService, IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter, IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService, IProductReviewService productReviewService,
            IProductService productService, IProductTagService productTagService,
            IProductTemplateService productTemplateService, IReviewTypeService reviewTypeService,
            IShoppingCartService shoppingCartService,
            ISpecificationAttributeService specificationAttributeService,
            IStaticCacheManager staticCacheManager, IStoreContext storeContext,
            IStoreService storeService, IShoppingCartModelFactory shoppingCartModelFactory,
            ITaxService taxService, IUrlRecordService urlRecordService, IVendorService vendorService,
            IVideoService videoService, IWebHelper webHelper, IWorkContext workContext,
            MediaSettings mediaSettings, OrderSettings orderSettings, SeoSettings seoSettings,
            ShippingSettings shippingSettings, VendorSettings vendorSettings, ICustomProductAttributeService customProductAttributeService, ICustomPictureService customPictureService,
            ICustomShoppingCartService customShoppingCartService,
            ICustomProductService customProductService,
            ISettingService settingService,
            ICustomSpecificationAttributeService customSpecificationAttributeService,
            IActionContextAccessor actionContextAccessor, IUrlHelperFactory urlHelperFactory, ICustomProductAttributeFormatter customProductAttributeFormatter,
            TagAutomationSettings tagAutomationSettings, IStoreWideDiscountService storeWideDiscountService, IGroupedProductConfigurationService groupedProductConfigurationService,
            IRepository<ProductPicture> productPictureRepository, ICommonService commonService
            ) : base(captchaSettings,
                catalogSettings, customerSettings, categoryService, currencyService, customerService,
                customWishlistService, dateRangeService, dateTimeHelper, downloadService,
                genericAttributeService, jsonLdModelFactory, localizationService, manufacturerService,
                permissionService, pictureService, priceCalculationService, priceFormatter,
                productAttributeParser, productAttributeService, productReviewService,
                productService, productTagService, productTemplateService, reviewTypeService,
                shoppingCartService, specificationAttributeService, staticCacheManager, storeContext,
                storeService, shoppingCartModelFactory, taxService, urlRecordService, vendorService, videoService, webHelper,
                workContext, mediaSettings, orderSettings, seoSettings, shippingSettings, vendorSettings)
        {
            _customProductAttributeService = customProductAttributeService;
            _customPictureService = customPictureService;
            _customShoppingCartService = customShoppingCartService;
            _customProductService = customProductService;
            _settingService = settingService;
            _customSpecificationAttributeService = customSpecificationAttributeService;
            _actionContextAccessor = actionContextAccessor;
            _urlHelperFactory = urlHelperFactory;
            _customProductAttributeFormatter = customProductAttributeFormatter;
            _tagAutomationSettings = tagAutomationSettings;
            _storeWideDiscountService = storeWideDiscountService;
            _groupedProductConfigurationService = groupedProductConfigurationService;
            _productPictureRepository = productPictureRepository;
            _commonService = commonService;
        }

        #region Methods

        public async Task<int> GetVariantIdBySize(int productId, string size)
        {
            int variantId = 0;
            var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(productId);
            foreach (var attribute in productAttributeMapping)
            {
                var productAttrubute = await _productAttributeService.GetProductAttributeByIdAsync(attribute.ProductAttributeId);

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var attributeValues = await _productAttributeService.GetProductAttributeValuesAsync(attribute.Id);
                    if (productAttrubute.Name.Equals(await _localizationService.GetResourceAsync("Product.Attr.Size"), StringComparison.InvariantCultureIgnoreCase))
                    {

                        if (attributeValues.Where(a =>
                        string.Equals(string.IsNullOrEmpty(a.QueryParameter) ? "N/A" : a.QueryParameter.Trim(), size.Trim(), StringComparison.InvariantCultureIgnoreCase) && a.Published == true && a.VariantId > 0).Any())
                            variantId = attributeValues.Where(a =>
                          string.Equals(string.IsNullOrEmpty(a.QueryParameter) ? "" : a.QueryParameter.Trim(), size.Trim(), StringComparison.InvariantCultureIgnoreCase) && a.Published == true && a.VariantId > 0).FirstOrDefault().VariantId;
                    }
                }
            }
            return variantId;
        }

        public async Task<VariantCombination> ValidateVariantID(int productId, int variantId)
        {
            VariantCombination variantCombination = (await _customProductService.GetProductVariants(productId)).Where(v => v.VariantId == variantId).FirstOrDefault();
            if (variantCombination != null)
            {

                if (!string.IsNullOrEmpty(variantCombination.ProductAttributeValueIds))
                {
                    foreach (var attributeValueId in variantCombination.ProductAttributeValueIds
                                            .Split("-")
                                            .Select(id => int.Parse(id))
                                            .ToList())
                    {
                        if (!(await _productAttributeService.GetProductAttributeValueByIdAsync(attributeValueId))?.Published ?? false)
                        {
                            variantCombination = null;
                            break;
                        }
                    }
                }
            }
            return variantCombination;
        }

        public virtual async Task<IEnumerable<CustomProductOverviewModel>> PrepareCustomProductOverviewModelsAsync(IEnumerable<Product> products,
          bool preparePriceModel = true, bool preparePictureModel = true,
          int? productThumbPictureSize = null, bool prepareSpecificationAttributes = false,
          bool forceRedirectionAfterAddingToCart = false, bool prepareShades = false, bool prepareCollectionSpecificationAttribute = false,
          bool prepareSizeShadeAggregation = false,
          bool prepareAlternatePictureModel = false, bool isCategorypage = false)
        {
            if (products == null)
                throw new ArgumentNullException(nameof(products));

            var models = new List<CustomProductOverviewModel>();
            foreach (var product in products)
            {
                var tags = await GetFilteredProductTagsAsync(product);

                #region Thanks giving tag exclude
                var thankGivingTag = await _settingService.GetSettingByKeyAsync<string>("ThanksGiving.ProductTag.Name");
                tags = tags.Where(t => !t.Name.Contains(thankGivingTag, StringComparison.InvariantCultureIgnoreCase)).ToList();
                #endregion

                var model = new CustomProductOverviewModel
                {
                    Id = product.Id,
                    Name = await _localizationService.GetLocalizedAsync(product, x => x.Name),
                    ShortDescription = await _localizationService.GetLocalizedAsync(product, x => x.ShortDescription),
                    FullDescription = await _localizationService.GetLocalizedAsync(product, x => x.FullDescription),
                    SeName = await _urlRecordService.GetSeNameAsync(product),
                    Sku = await GetProductSku(product.Sku, product.Id),
                    ProductType = product.ProductType,
                    MarkAsNew = product.MarkAsNew &&
                        (!product.MarkAsNewStartDateTimeUtc.HasValue || product.MarkAsNewStartDateTimeUtc.Value < DateTime.UtcNow) &&
                        (!product.MarkAsNewEndDateTimeUtc.HasValue || product.MarkAsNewEndDateTimeUtc.Value > DateTime.UtcNow),
                    Tags = tags.ToList(),
                    Published = product.Published,
                    Inventory = product.TotalInventory,
                    EnableCustomizationModule = product.EnableCustomizationModule,
                    MetaKeywords = product.MetaKeywords,
                    NoOfSales = product.NoOfSales,
                };

                //price
                if (preparePriceModel)
                {
                    model.ProductPrice = await PrepareCustomProductOverviewPriceModelAsync(product, forceRedirectionAfterAddingToCart);
                }
                //picture
                if (preparePictureModel && !prepareAlternatePictureModel)
                    model.DefaultPictureModel = await PrepareCustomProductOverviewPictureModelAsync(product, productThumbPictureSize, isCategorypage);
                else if (preparePictureModel)
                    await PrepareCustomProductOverviewPictureModelAsync(product, model, productThumbPictureSize, isCategorypage);

                //specs
                if (prepareSpecificationAttributes)
                    model.ProductSpecificationModel = await PrepareProductSpecificationModelAsync(product);


                if (prepareShades)
                {
                    //  Model
                    var attrName = await _settingService.GetSettingAsync("Catalog.Product.Attribute.Shade.Name");

                    if (attrName != null && !string.IsNullOrEmpty(attrName.Value))
                        model.Attribute = await this.PrepareCustomProductAttributeModelWithImageAsync(model.Id, attrName.Value, productThumbPictureSize);
                }

                if (prepareCollectionSpecificationAttribute)
                {
                    var collectionSpcfAttrId = await _settingService.GetSettingByKeyAsync<int>("Catalog.Product.Collection.SpecificationAttribute.Id");
                    if (collectionSpcfAttrId != 0)
                    {
                        var productSpecificationAttributes = await _customSpecificationAttributeService.GetProductSpecificationAttributesByAttributeIdAsync(model.Id, collectionSpcfAttrId);
                        model.CollectionMessage = productSpecificationAttributes.FirstOrDefault()?.CustomValue;
                    }
                }
                if (prepareSizeShadeAggregation)
                {
                    var shadeAttrName = await _settingService.GetSettingAsync("Catalog.Product.Attribute.Shade.Name");
                    var sizeAttrName = await _settingService.GetSettingAsync("Catalog.Product.Attribute.Size.Name");
                    if (sizeAttrName != null && !string.IsNullOrEmpty(sizeAttrName.Value) && shadeAttrName != null && !string.IsNullOrEmpty(shadeAttrName.Value))
                        model.SizeShadeMessage = await this.ProductShadeSizeMessage(model.Id, shadeAttrName.Value, sizeAttrName.Value);
                }


                //reviews
                model.ReviewOverviewModel = await PrepareProductReviewOverviewModelAsync(product);

                models.Add(model);
            }

            return models;
        }


        public virtual async Task<IEnumerable<CustomProductOverviewModel>> PrepareCustomProductOverviewDetailInfoModelAsync(IEnumerable<Product> products,
  bool preparePriceModel = true, bool preparePictureModel = true,
  int? productThumbPictureSize = null, bool prepareSpecificationAttributes = false,
  bool forceRedirectionAfterAddingToCart = false, bool prepareShades = false, bool prepareCollectionSpecificationAttribute = false,
  bool prepareSizeShadeAggregation = false,
  bool prepareAlternatePictureModel = false, int variantId = 0)
        {
            if (products == null)
                throw new ArgumentNullException(nameof(products));

            var models = new List<CustomProductOverviewModel>();
            foreach (var product in products)
            {
                var model = new CustomProductOverviewModel
                {
                    Id = product.Id,
                    Name = await _localizationService.GetLocalizedAsync(product, x => x.Name),
                    ShortDescription = await _localizationService.GetLocalizedAsync(product, x => x.ShortDescription),
                    FullDescription = await _localizationService.GetLocalizedAsync(product, x => x.FullDescription),
                    SeName = await _urlRecordService.GetSeNameAsync(product),
                    Sku = await GetProductSku(product.Sku, product.Id),
                    ProductType = product.ProductType,
                    MarkAsNew = product.MarkAsNew &&
                        (!product.MarkAsNewStartDateTimeUtc.HasValue || product.MarkAsNewStartDateTimeUtc.Value < DateTime.UtcNow) &&
                        (!product.MarkAsNewEndDateTimeUtc.HasValue || product.MarkAsNewEndDateTimeUtc.Value > DateTime.UtcNow),
                    AllowAddingOnlyExistingAttributeCombinations = product.AllowAddingOnlyExistingAttributeCombinations,
                    DefaultVariantId = await GetProductVariantId(product.Id)
                };
                int mainCategoryId = await _customSpecificationAttributeService.GetMainCategoryOfProduct(product.Id);
                if (mainCategoryId > 0)
                {
                    var category = (await _categoryService.GetCategoryByIdAsync(mainCategoryId));
                    model.ConversionValue = category?.ConversionValue ?? 15;
                }
                model.ConversionValue = model.ConversionValue <= 0 ? 15 : model.ConversionValue;
                if (variantId != 0)
                {
                    var variant = await this.ValidateVariantID(product.Id, variantId);
                    if (variant == null)
                    {
                        variantId = 0;
                    }
                    else
                    {
                        model.VariantSename = (variant.SeName ?? "").Trim() == string.Empty ? model.SeName : variant.SeName;
                        model.VariantId = variantId;
                    }
                }
                //price
                if (preparePriceModel)
                {
                    model.ProductPrice = await PrepareCustomProductOverviewPriceModelAsync(product, variantId);
                }
                //picture



                model.AddToCart = await PrepareProductAddToCartModelAsync(product, null);
                int variantPictureId = 0;
                (model.VariantTitle, model.ProductAttributes, variantPictureId, model.AllCombinations) = await CustomPrepareFeatureProductAttributeWithVariantTitleModelsAsync(product, null, variantId);

                IList<CustomPictureModel> allPictureModels;
                IList<VideoModel> allvideoModels;
                (model.DefaultPictureModel, allPictureModels, allvideoModels) = await CustomPrepareProductDetailsPictureModelAsync(product, false, variantPictureId);
                model.PictureModels = allPictureModels;

                //specs
                if (prepareSpecificationAttributes)
                    model.ProductSpecificationModel = await PrepareProductSpecificationModelAsync(product);


                if (prepareShades)
                {
                    //  Model
                    var attrName = await _settingService.GetSettingAsync("Catalog.Product.Attribute.Shade.Name");

                    if (attrName != null && !string.IsNullOrEmpty(attrName.Value))
                        model.Attribute = await this.PrepareCustomProductAttributeModelWithImageAsync(model.Id, attrName.Value, productThumbPictureSize);
                }

                if (prepareCollectionSpecificationAttribute)
                {
                    var collectionSpcfAttrId = await _settingService.GetSettingByKeyAsync<int>("Catalog.Product.Collection.SpecificationAttribute.Id");
                    if (collectionSpcfAttrId != 0)
                    {
                        var productSpecificationAttributes = await _customSpecificationAttributeService.GetProductSpecificationAttributesByAttributeIdAsync(model.Id, collectionSpcfAttrId);
                        model.CollectionMessage = productSpecificationAttributes.FirstOrDefault()?.CustomValue;
                    }
                }
                if (prepareSizeShadeAggregation)
                {
                    var shadeAttrName = await _settingService.GetSettingAsync("Catalog.Product.Attribute.Shade.Name");
                    var sizeAttrName = await _settingService.GetSettingAsync("Catalog.Product.Attribute.Size.Name");
                    if (sizeAttrName != null && !string.IsNullOrEmpty(sizeAttrName.Value) && shadeAttrName != null && !string.IsNullOrEmpty(shadeAttrName.Value))
                        model.SizeShadeMessage = await this.ProductShadeSizeMessage(model.Id, shadeAttrName.Value, sizeAttrName.Value);
                }


                //reviews
                model.ReviewOverviewModel = await PrepareProductReviewOverviewModelAsync(product);

                models.Add(model);
            }

            return models;
        }

        private async Task<string> ProductShadeSizeMessage(int productId, string shadeAttrName, string sizeAttrName)
        {
            string message = string.Empty;
            string attributeName = string.Empty;
            var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(productId);
            foreach (var attribute in productAttributeMapping)
            {


                var attributeValues = await _productAttributeService.GetProductAttributeValuesAsync(attribute.Id);
                if (attributeValues.Where(v => v.Published).Count() > 0)
                {
                    var productAttribute = await _productAttributeService.GetProductAttributeByIdAsync(attribute.ProductAttributeId);
                    attributeName = productAttribute.Name;
                    if (string.Equals(attributeName, await _localizationService.GetResourceAsync("Product.Attr.Size"), StringComparison.InvariantCultureIgnoreCase) ||
                       string.Equals(attributeName, await _localizationService.GetResourceAsync("Product.Attr.Stain"), StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (attributeValues.Where(v => v.Published).Count() > 1)
                        {
                            attributeName = attributeName.EndsWith("s") ? attributeName : attributeName + "s";
                        }
                        message += $"{attributeValues.Where(a => a.Published).Count()} {attributeName}|";

                    }
                }


            }

            return message;
        }

        public virtual async Task<List<CustomProductOverviewModel.ProductAttributeModelWithImage>> PrepareCustomProductAttributeModelWithImageAsync(int productId, string attributeName, int? productThumbPictureSize = null)
        {
            List<CustomProductOverviewModel.ProductAttributeModelWithImage> attributeModel = new List<CustomProductOverviewModel.ProductAttributeModelWithImage>();
            var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(productId);
            foreach (var attribute in productAttributeMapping)
            {
                var productAttribute = await _productAttributeService.GetProductAttributeByIdAsync(attribute.ProductAttributeId);
                if (productAttribute.Name.CompareTo(attributeName) == 0)
                {
                    var attributeValues = await _productAttributeService.GetProductAttributeValuesAsync(attribute.Id);
                    foreach (var attrValue in attributeValues.OrderBy(m => m.DisplayOrder))
                    {
                        if (attrValue.Published)
                        {
                            attributeModel.Add(new CustomProductOverviewModel.ProductAttributeModelWithImage()
                            {
                                Name = attrValue.Name,
                                PictureUrl = await this.CustomGetPicture((await _productAttributeService.GetProductAttributeValuePicturesAsync(attrValue.Id)).FirstOrDefault()?.PictureId ?? 0, productThumbPictureSize)

                            });
                        }
                    }
                    break;
                }

            }
            return attributeModel;
        }

        private async Task<string> CustomGetPicture(int pictureId, int? productThumbPictureSize = null)
        {
            if (pictureId == 0)
                return "";
            else
            {
                var pictureSize = productThumbPictureSize ?? _mediaSettings.ProductThumbPictureSize;
                var picture = (await _pictureService.GetPictureByIdAsync(pictureId));
                if (picture == null)
                    return "";
                else
                {
                    string imageUrl;
                    (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, pictureSize);
                    return imageUrl;
                }
            }
        }
        protected virtual async Task PrepareCustomProductOverviewPictureModelAsync(Product product, CustomProductOverviewModel model, int? productThumbPictureSize = null, bool isCategorypage = false)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var productName = await _localizationService.GetLocalizedAsync(product, x => x.Name);
            //If a size has been set in the view, we use it in priority
            var pictureSize = productThumbPictureSize ?? _mediaSettings.ProductThumbPictureSize;

            //prepare picture model
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.CustomProductDefaultPictureModelKey,
                product, pictureSize, true, await _workContext.GetWorkingLanguageAsync(), _webHelper.IsCurrentConnectionSecured(),
                await _storeContext.GetCurrentStoreAsync(), isCategorypage ? "category" : "listing");

            bool isServiceCalled = false;
            var pictures = new List<Picture>();
            var defaultPictureModel = await _staticCacheManager.GetAsync(cacheKey, async () =>
            {

                var result = (await _customPictureService.CustomGetPicturesByProductIdAsync(product.Id));
                pictures = result.pictures;

                Picture picture = new Picture();
                if (isCategorypage && result.displayOnCategoryPagePictureId != 0)
                    picture = pictures.Where(p => p.Id == result.displayOnCategoryPagePictureId).FirstOrDefault();
                else if (result.displayOnListingModulesPictureId != 0 && !isCategorypage)
                    picture = pictures.Where(p => p.Id == result.displayOnListingModulesPictureId).FirstOrDefault();
                else
                    picture = pictures.FirstOrDefault();
                isServiceCalled = true;
                string fullSizeImageUrl, imageUrl;
                (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, pictureSize);
                (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);

                var pictureModel = new CustomPictureModel
                {
                    ImageUrl = imageUrl,
                    FullSizeImageUrl = fullSizeImageUrl,
                    //"title" attribute
                    Title = (picture != null && !string.IsNullOrEmpty(picture.TitleAttribute))
                        ? picture.TitleAttribute
                        : string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat"),
                            productName),
                    //"alt" attribute
                    AlternateText = (picture != null && !string.IsNullOrEmpty(picture.AltAttribute))
                        ? picture.AltAttribute
                        : string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat"),
                            productName)
                };

                return pictureModel;
            });

            model.DefaultPictureModel = defaultPictureModel;

            if (((isServiceCalled && pictures.Count > 1) || !isServiceCalled) && product.AlternateImageSequence != null && product.AlternateImageSequence > 0)
            {
                cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.CustomProductDefaultPictureModelKey,
                    product, pictureSize + "_alternate", true, await _workContext.GetWorkingLanguageAsync(), _webHelper.IsCurrentConnectionSecured(),
                    await _storeContext.GetCurrentStoreAsync());

                var alternatePictureModel = await _staticCacheManager.GetAsync(cacheKey, async () =>
                {
                    if (!isServiceCalled)
                        pictures = (await this._pictureService.GetPicturesByProductIdAsync(product.Id)).ToList();

                    var alterNatePictureRef = await _productPictureRepository.Table.Where(m => m.ProductId == product.Id && m.DisplayOrder == product.AlternateImageSequence).FirstOrDefaultAsync();

                    var picture = new Picture();
                    if (alterNatePictureRef == null)
                        return null;
                    else
                    {
                        picture = pictures.Where(m => m.Id == alterNatePictureRef.PictureId).FirstOrDefault();
                        if (picture == null)
                            return null;
                    }


                    string fullSizeImageUrl, imageUrl;
                    (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, pictureSize);
                    (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);

                    var pictureModel = new PictureModel
                    {
                        ImageUrl = imageUrl,
                        FullSizeImageUrl = fullSizeImageUrl,
                        //"title" attribute
                        Title = (picture != null && !string.IsNullOrEmpty(picture.TitleAttribute))
                            ? picture.TitleAttribute
                            : string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat"),
                                productName),
                        //"alt" attribute
                        AlternateText = (picture != null && !string.IsNullOrEmpty(picture.AltAttribute))
                            ? picture.AltAttribute
                            : string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat"),
                                productName)
                    };

                    return pictureModel;
                });
                model.AlternatePictureModel = alternatePictureModel;
            }
        }

        public virtual async Task<CustomProductDetailsModel> PrepareCustomProductDetailsModelAsync(Product product,
                ShoppingCartItem updatecartitem = null, bool isAssociatedProduct = false, int variantId = 0)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            //standard properties
            var model = new CustomProductDetailsModel
            {
                Id = product.Id,
                Name = await _localizationService.GetLocalizedAsync(product, x => x.Name),
                ShortDescription = await _localizationService.GetLocalizedAsync(product, x => x.ShortDescription),
                FullDescription = await _localizationService.GetLocalizedAsync(product, x => x.FullDescription),
                MetaKeywords = await _localizationService.GetLocalizedAsync(product, x => x.MetaKeywords),
                MetaDescription = await _localizationService.GetLocalizedAsync(product, x => x.MetaDescription),
                MetaTitle = await _localizationService.GetLocalizedAsync(product, x => x.MetaTitle),
                SeName = await _urlRecordService.GetSeNameAsync(product),
                ProductType = product.ProductType,
                ShowSku = _catalogSettings.ShowSkuOnProductDetailsPage,
                Sku = await GetProductSku(product.Sku, product.Id),
                ShowManufacturerPartNumber = _catalogSettings.ShowManufacturerPartNumber,
                FreeShippingNotificationEnabled = _catalogSettings.ShowFreeShippingNotification,
                ManufacturerPartNumber = product.ManufacturerPartNumber,
                ShowGtin = _catalogSettings.ShowGtin,
                Gtin = product.Gtin,
                ManageInventoryMethod = product.ManageInventoryMethod,
                StockAvailability = await _productService.FormatStockMessageAsync(product, string.Empty),
                HasSampleDownload = product.IsDownload && product.HasSampleDownload,
                DisplayDiscontinuedMessage = !product.Published && _catalogSettings.DisplayDiscontinuedMessageForUnpublishedProducts,
                AvailableEndDate = product.AvailableEndDateTimeUtc,
                VisibleIndividually = product.VisibleIndividually,
                AllowAddingOnlyExistingAttributeCombinations = product.AllowAddingOnlyExistingAttributeCombinations,
                ParentGroupId = product.ParentGroupedProductId,
                EnableCustomizationModule = product.EnableCustomizationModule,
                Notes = product.Notes,
                TotalInventory = product.TotalInventory,
                CollectionListingDisplayVertically = product.CollectionListingDisplayVertically,
                ARImageUrl = product.ARImageUrl,
                ARHeading = product.ARHeading,
                CustomizationFormTemplateId = product.CustomizationFormTemplateId,
                VariantId = variantId,
                DefaultVariantId = await GetProductVariantId(product.Id),
                DisplayDimensionOfVariant = product.DisplayDimensionOfVariant,
                RelatedProductId = product.RelatedProductId,
                VideoThumnailDisplayOrder = product.VideoThumnailDisplayOrder,
                Variants = await _customProductService.GetPublishedProductVariants(product.Id),
                EnableConditionalAttributes = product.EnableConditionalAttributes,
                IsBundleProduct = product.IsBundleProduct,
                DisplayBundleConfiguration = product.DisplayBundleConfiguration

            };

            model.MainCategoryId = await _customSpecificationAttributeService.GetMainCategoryOfProduct(product.Id);
            if (model.MainCategoryId > 0)
            {
                var category = (await _categoryService.GetCategoryByIdAsync(model.MainCategoryId));
                model.UseNewVersionOfTemplate = category?.UseNewVersionOfTemplate ?? false;
                model.UseNewVersionCustomizationAction = category?.UseNewVersionCustomizationAction ?? false;
                model.ConversionValue = category?.ConversionValue ?? 15;
            }
            model.ConversionValue = model.ConversionValue <= 0 ? 15 : model.ConversionValue;


            model.EstimatedDeliveryDate = await this.GetProductEstimatedDeliveryDate(product.Id, model.VariantId == 0 ? model.DefaultVariantId : model.VariantId, product.EstimatedDeliveryDate ?? string.Empty);
            //automatically generate product description?
            if (_seoSettings.GenerateProductMetaDescription && string.IsNullOrEmpty(model.MetaDescription))
            {
                //based on short description
                model.MetaDescription = model.ShortDescription;
            }

            //shipping info
            model.IsShipEnabled = product.IsShipEnabled;
            if (product.IsShipEnabled)
            {
                model.IsFreeShipping = product.IsFreeShipping;
                //delivery date
                var deliveryDate = await _dateRangeService.GetDeliveryDateByIdAsync(product.DeliveryDateId);
                if (deliveryDate != null)
                {
                    model.DeliveryDate = await _localizationService.GetLocalizedAsync(deliveryDate, dd => dd.Name);
                }
            }

            //email a friend
            model.EmailAFriendEnabled = _catalogSettings.EmailAFriendEnabled;
            //compare products
            model.CompareProductsEnabled = _catalogSettings.CompareProductsEnabled;
            //store name
            model.CurrentStoreName = await _localizationService.GetLocalizedAsync(await _storeContext.GetCurrentStoreAsync(), x => x.Name);

            //vendor details
            if (_vendorSettings.ShowVendorOnProductDetailsPage)
            {
                var vendor = await _vendorService.GetVendorByIdAsync(product.VendorId);
                if (vendor != null && !vendor.Deleted && vendor.Active)
                {
                    model.ShowVendor = true;

                    model.VendorModel = new VendorBriefInfoModel
                    {
                        Id = vendor.Id,
                        Name = await _localizationService.GetLocalizedAsync(vendor, x => x.Name),
                        SeName = await _urlRecordService.GetSeNameAsync(vendor),
                    };
                }
            }

            //page sharing
            if (_catalogSettings.ShowShareButton && !string.IsNullOrEmpty(_catalogSettings.PageShareCode))
            {
                var shareCode = _catalogSettings.PageShareCode;
                if (_webHelper.IsCurrentConnectionSecured())
                {
                    //need to change the add this link to be https linked when the page is, so that the page doesn't ask about mixed mode when viewed in https...
                    shareCode = shareCode.Replace("http://", "https://");
                }

                model.PageShareCode = shareCode;
            }

            switch (product.ManageInventoryMethod)
            {
                case ManageInventoryMethod.ManageStock:
                    model.InStock = product.BackorderMode != BackorderMode.NoBackorders
                        || await _productService.GetTotalStockQuantityAsync(product) > 0;
                    model.DisplayBackInStockSubscription = !model.InStock && product.AllowBackInStockSubscriptions;
                    break;

                case ManageInventoryMethod.ManageStockByAttributes:
                    model.InStock = (await _productAttributeService
                        .GetAllProductAttributeCombinationsAsync(product.Id))
                        ?.Any(c => c.StockQuantity > 0 || c.AllowOutOfStockOrders)
                        ?? false;
                    break;
            }

            //breadcrumb
            //do not prepare this model for the associated products. anyway it's not used
            if (_catalogSettings.CategoryBreadcrumbEnabled && !isAssociatedProduct)
            {
                model.Breadcrumb = await PrepareCustomProductBreadcrumbModelAsync(product);
            }

            //product tags
            //do not prepare this model for the associated products. anyway it's not used

            model.ProductTags = await CustomPrepareProductTagModelsAsync(product);



            //product attributes
            int variantPictureId = 0;
            (model.VariantTitle, model.ProductAttributes, variantPictureId, model.AllCombinations) = await CustomPrepareProductAttributeWithVariantTitleModelsAsync(product, updatecartitem, variantId);




            if (model.DisplayDimensionOfVariant)
            {
                var sizeAttrName = await _localizationService.GetResourceAsync("Product.Attr.Size");
                model.DisplayDimensionOfVariant = model.ProductAttributes.Where(pa => pa.Name.Equals(sizeAttrName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault()?.Values.
                                 Where(pv => !string.IsNullOrEmpty(pv.VariantDimension)).Any() ?? false;
            }

            #region Thanks Giving Section

            var thankGivingTag = await _settingService.GetSettingByKeyAsync<string>("ThanksGiving.ProductTag.Name");
            if (model.ProductTags.Where(t => t.Name.Equals(thankGivingTag, StringComparison.InvariantCultureIgnoreCase)).Any())
            {
                model.IsThanksGivingProduct = true;
            }

            #endregion


            //pictures
            model.DefaultPictureZoomEnabled = _mediaSettings.DefaultPictureZoomEnabled;
            IList<CustomPictureModel> allPictureModels;
            IList<VideoModel> allvideoModels;
            (model.DefaultPictureModel, allPictureModels, allvideoModels) = await CustomPrepareProductDetailsPictureModelAsync(product, isAssociatedProduct, variantPictureId);
            model.PictureModels = allPictureModels;
            model.VideoUrl = allvideoModels.FirstOrDefault()?.VideoUrl ?? String.Empty;


            foreach (var attribute in model.ProductAttributes)
            {
                foreach (var productAttributeValue in attribute.Values)
                {
                    productAttributeValue.VariantDimension = await ReplaceTokens(productAttributeValue.VariantDimension, !allPictureModels.Where(p => p.IsDimensionImage).Any() ? string.Empty : allPictureModels.Where(p => p.IsDimensionImage).FirstOrDefault().ImageUrl,
                        !allPictureModels.Where(p => p.IsDimensionImage).Any() ? string.Empty : allPictureModels.Where(p => p.IsDimensionImage).FirstOrDefault().FullSizeImageUrl,
                                productAttributeValue.PictureDefaultSizeUrl, productAttributeValue.PictureFullSizeUrl, productAttributeValue.IsLargeItem,
                                product.Name);
                }
            }

            //price
            model.ProductPrice = await PrepareCustomProductPriceModelAsync(product, variantId);

            //'Add to cart' model
            model.AddToCart = await PrepareProductAddToCartModelAsync(product, updatecartitem);

            //gift card
            if (product.IsGiftCard)
            {
                model.GiftCard.IsGiftCard = true;
                model.GiftCard.GiftCardType = product.GiftCardType;

                if (updatecartitem == null)
                {
                    model.GiftCard.SenderName = await _customerService.GetCustomerFullNameAsync(await _workContext.GetCurrentCustomerAsync());
                    model.GiftCard.SenderEmail = (await _workContext.GetCurrentCustomerAsync()).Email;
                }
                else
                {
                    _productAttributeParser.GetGiftCardAttribute(updatecartitem.AttributesXml,
                        out var giftCardRecipientName, out var giftCardRecipientEmail,
                        out var giftCardSenderName, out var giftCardSenderEmail, out var giftCardMessage);

                    model.GiftCard.RecipientName = giftCardRecipientName;
                    model.GiftCard.RecipientEmail = giftCardRecipientEmail;
                    model.GiftCard.SenderName = giftCardSenderName;
                    model.GiftCard.SenderEmail = giftCardSenderEmail;
                    model.GiftCard.Message = giftCardMessage;
                }
            }



            //product specifications
            //do not prepare this model for the associated products. anyway it's not used
            if (!isAssociatedProduct)
            {
                var dimensionAttributeName = await _localizationService.GetResourceAsync("product.specification.attr.dimension");
                var mobileDimensionAttributeName = await _localizationService.GetResourceAsync("product.specification.attr.Mobiledimension");
                model.ProductSpecificationModel = await PrepareProductSpecificationModelAsync(product);
                foreach (var specificationGroup in model.ProductSpecificationModel.Groups)
                {
                    foreach (var specificationAttributeModel in specificationGroup.Attributes)
                    {
                        if (specificationAttributeModel.Name.Equals(dimensionAttributeName, StringComparison.InvariantCultureIgnoreCase) ||
                            specificationAttributeModel.Name.Equals(mobileDimensionAttributeName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            foreach (var specificationAttributeValue in specificationAttributeModel.Values)
                            {
                                specificationAttributeValue.ValueRaw =
                                     await ReplaceTokens(specificationAttributeValue.ValueRaw, !allPictureModels.Where(p => p.IsDimensionImage).Any() ? string.Empty : allPictureModels.Where(p => p.IsDimensionImage).FirstOrDefault().ImageUrl,
                                     !allPictureModels.Where(p => p.IsDimensionImage).Any() ? string.Empty : allPictureModels.Where(p => p.IsDimensionImage).FirstOrDefault().FullSizeImageUrl
                                     , string.Empty, string.Empty, false, product.Name);
                            }
                        }
                    }
                }
            }

            //product review overview
            model.ProductReviewOverview = await PrepareProductReviewOverviewModelAsync(product);

            //tier prices
            if (await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.DISPLAY_PRICES))
            {
                model.TierPrices = await PrepareProductTierPriceModelsAsync(product);
            }




            //rental products
            if (product.IsRental)
            {
                model.IsRental = true;
                //set already entered dates attributes (if we're going to update the existing shopping cart item)
                if (updatecartitem != null)
                {
                    model.RentalStartDate = updatecartitem.RentalStartDateUtc;
                    model.RentalEndDate = updatecartitem.RentalEndDateUtc;
                }
            }

            //estimate shipping
            if (_shippingSettings.EstimateShippingProductPageEnabled && !model.IsFreeShipping)
            {
                var wrappedProduct = new ShoppingCartItem
                {
                    StoreId = (await _storeContext.GetCurrentStoreAsync()).Id,
                    ShoppingCartTypeId = (int)ShoppingCartType.ShoppingCart,
                    CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                    ProductId = product.Id,
                    CreatedOnUtc = DateTime.UtcNow
                };

                var estimateShippingModel = await _shoppingCartModelFactory.PrepareEstimateShippingModelAsync(new[] { wrappedProduct });

                model.ProductEstimateShipping.ProductId = product.Id;
                model.ProductEstimateShipping.RequestDelay = estimateShippingModel.RequestDelay;
                model.ProductEstimateShipping.Enabled = estimateShippingModel.Enabled;
                model.ProductEstimateShipping.CountryId = estimateShippingModel.CountryId;
                model.ProductEstimateShipping.StateProvinceId = estimateShippingModel.StateProvinceId;
                model.ProductEstimateShipping.ZipPostalCode = estimateShippingModel.ZipPostalCode;
                model.ProductEstimateShipping.UseCity = estimateShippingModel.UseCity;
                model.ProductEstimateShipping.City = estimateShippingModel.City;
                model.ProductEstimateShipping.AvailableCountries = estimateShippingModel.AvailableCountries;
                model.ProductEstimateShipping.AvailableStates = estimateShippingModel.AvailableStates;
            }

            //associated products
            if (product.ProductType == ProductType.GroupedProduct)
            {
                model.AssociatedProductsConfiguration = await this.PrepareGroupedProductConfiguration(product.Id);
                //ensure no circular references
                if (!isAssociatedProduct)
                {
                    var associatedProducts = await _productService.GetAssociatedProductsAsync(product.Id, (await _storeContext.GetCurrentStoreAsync()).Id);
                    foreach (var associatedProduct in associatedProducts)
                        model.AssociatedProducts.Add(await PrepareCustomProductDetailsModelAsync(associatedProduct, null, true));
                }
            }

            return model;
        }

        protected virtual async Task<CustomProductOverviewModel.ProductPriceModel> PrepareCustomProductOverviewPriceModelAsync(Product product, int variantId = 0)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var model = new CustomProductOverviewModel.ProductPriceModel
            {
                ProductId = product.Id
            };

            if (await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.DISPLAY_PRICES))
            {
                model.HidePrices = false;
                if (product.CustomerEntersPrice)
                {
                    model.CustomerEntersPrice = true;
                }
                else
                {
                    if (product.CallForPrice &&
                        //also check whether the current user is impersonated
                        (!_orderSettings.AllowAdminsToBuyCallForPriceProducts || _workContext.OriginalCustomerIfImpersonated == null))
                    {
                        model.CallForPrice = true;
                    }
                    else
                    {

                        bool isVariantExist = false; decimal variantMsrp = 0; decimal variantOldPrice = 0;
                        decimal variantPrice = 0;
                        if (variantId != 0)
                        {
                            (isVariantExist, variantMsrp, variantOldPrice, variantPrice) = await GetVariantPrice(product, variantId);
                        }
                        if (!isVariantExist)
                        {
                            variantMsrp = product.Msrp;
                            variantOldPrice = product.OldPrice;
                            variantPrice = product.Price;
                        }

                        var (oldPriceBase, _) = await _taxService.GetProductPriceAsync(product, variantOldPrice);
                        var (msrpBase, _) = await _taxService.GetProductPriceAsync(product, variantMsrp);
                        var (finalPriceWithoutDiscountBase, _) = await _taxService.GetProductPriceAsync(product, variantPrice);
                        decimal finalPriceWithDiscountBase = finalPriceWithoutDiscountBase;



                        var oldPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(oldPriceBase, await _workContext.GetWorkingCurrencyAsync());
                        var finalPriceWithoutDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(finalPriceWithoutDiscountBase, await _workContext.GetWorkingCurrencyAsync());
                        var finalPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(finalPriceWithDiscountBase, await _workContext.GetWorkingCurrencyAsync());

                        // membershipprice

                        // Need to confirm
                        //(var memberShipPrice, _) = await _shoppingcartService.MemberShipPriceOfProduct(product.Id, variantMsrp, variantOldPrice, variantPrice);
                        //if (memberShipPrice > decimal.Zero)
                        //{
                        //    memberShipPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(memberShipPrice, await _workContext.GetWorkingCurrencyAsync());
                        //    model.MembershipPrice = await _priceFormatter.FormatPriceAsync(memberShipPrice);
                        //    model.MembershipPriceValue = memberShipPrice;
                        //}

                        // end

                        if (oldPriceBase > decimal.Zero)
                        {
                            model.OldPrice = await _priceFormatter.FormatPriceAsync(oldPrice);
                            model.OldPriceValue = oldPrice;
                        }
                        else
                        {
                            model.OldPrice = await _priceFormatter.FormatPriceAsync(finalPriceWithoutDiscount);
                            model.OldPriceValue = finalPriceWithoutDiscount;
                        }
                        if (msrpBase > decimal.Zero && msrpBase > model.OldPriceValue)
                        {
                            model.Msrp = await _priceFormatter.FormatPriceAsync(msrpBase);
                            model.MsrpValue = msrpBase;
                        }
                        model.Price = await _priceFormatter.FormatPriceAsync(finalPriceWithoutDiscount);

                        if (finalPriceWithoutDiscountBase != finalPriceWithDiscountBase)
                            model.PriceWithDiscount = await _priceFormatter.FormatPriceAsync(finalPriceWithDiscount);

                        model.PriceValue = finalPriceWithDiscount;

                        //property for German market
                        //we display tax/shipping info only with "shipping enabled" for this product
                        //we also ensure this it's not free shipping
                        model.DisplayTaxShippingInfo = _catalogSettings.DisplayTaxShippingInfoProductDetailsPage
                            && product.IsShipEnabled &&
                            !product.IsFreeShipping;

                        //PAngV baseprice (used in Germany)
                        model.BasePricePAngV = await _priceFormatter.FormatBasePriceAsync(product, finalPriceWithDiscountBase);
                        //currency code
                        model.CurrencyCode = (await _workContext.GetWorkingCurrencyAsync()).CurrencyCode;

                        //rental
                        if (product.IsRental)
                        {
                            model.IsRental = true;
                            var priceStr = await _priceFormatter.FormatPriceAsync(finalPriceWithDiscount);
                            model.RentalPrice = await _priceFormatter.FormatRentalProductPeriodAsync(product, priceStr);
                        }
                    }
                }
            }
            else
            {
                model.HidePrices = true;
                model.OldPrice = null;
                model.Price = null;
            }


            model.Price = CustomCommonHelper.FormatCurrencyPriceWithoutDecimal(model.Price);
            model.PriceValue = CustomCommonHelper.FormatPriceWithoutDecimal(model.PriceValue);
            model.OldPrice = CustomCommonHelper.FormatCurrencyPriceWithoutDecimal(model.OldPrice);
            model.OldPriceValue = CustomCommonHelper.FormatPriceWithoutDecimal(model.OldPriceValue);
            model.MinPrice = CustomCommonHelper.FormatCurrencyPriceWithoutDecimal(model.MinPrice);
            model.MinPriceValue = CustomCommonHelper.FormatPriceWithoutDecimal(model.MinPriceValue);
            model.MinOldPrice = CustomCommonHelper.FormatCurrencyPriceWithoutDecimal(model.MinOldPrice);
            model.MinOldPriceValue = CustomCommonHelper.FormatPriceWithoutDecimal(model.MinOldPriceValue);
            model.MaxPrice = CustomCommonHelper.FormatCurrencyPriceWithoutDecimal(model.MaxPrice);
            model.MaxPriceValue = CustomCommonHelper.FormatPriceWithoutDecimal(model.MaxPriceValue);
            model.MaxOldPrice = CustomCommonHelper.FormatCurrencyPriceWithoutDecimal(model.MaxOldPrice);
            model.MaxOldPriceValue = CustomCommonHelper.FormatPriceWithoutDecimal(model.MaxOldPriceValue);
            (model.OfferText, model.OfferPlaceHolder, model.DiscountAmount, model.DiscountPercentage, model.SaleStartDate, model.SaleEndDate) = 
                await _customProductService.GetProductSaleOfferInfo(product, model.OldPriceValue, model.PriceValue);


            return model;
        }

        protected virtual async Task<CustomProductOverviewModel.ProductPriceModel> PrepareCustomProductOverviewPriceModelAsync(Product product, bool forceRedirectionAfterAddingToCart = false)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var priceModel = new CustomProductOverviewModel.ProductPriceModel
            {
                ForceRedirectionAfterAddingToCart = forceRedirectionAfterAddingToCart
            };

            switch (product.ProductType)
            {
                case ProductType.GroupedProduct:
                    //grouped product
                    await PrepareCustomSimpleProductOverviewPriceModelAsync(product, priceModel);

                    break;
                case ProductType.SimpleProduct:
                default:
                    //simple product
                    await PrepareCustomSimpleProductOverviewPriceModelAsync(product, priceModel);

                    break;
            }

            return priceModel;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrepareCustomSimpleProductOverviewPriceModelAsync(Product product, CustomProductOverviewModel.ProductPriceModel priceModel)
        {

            //add to cart button
            priceModel.DisableBuyButton = product.DisableBuyButton ||
                                          !await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.ENABLE_SHOPPING_CART) ||
                                          !await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.DISPLAY_PRICES);

            //add to wishlist button
            priceModel.DisableWishlistButton = product.DisableWishlistButton ||
                                               !await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.ENABLE_WISHLIST) ||
                                               !await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.DISPLAY_PRICES);
            //compare products
            priceModel.DisableAddToCompareListButton = !_catalogSettings.CompareProductsEnabled;

            //rental
            priceModel.IsRental = product.IsRental;

            //pre-order
            if (product.AvailableForPreOrder)
            {
                priceModel.AvailableForPreOrder = !product.PreOrderAvailabilityStartDateTimeUtc.HasValue ||
                                                  product.PreOrderAvailabilityStartDateTimeUtc.Value >=
                                                  DateTime.UtcNow;
                priceModel.PreOrderAvailabilityStartDateTimeUtc = product.PreOrderAvailabilityStartDateTimeUtc;
            }

            //prices
            if (await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.DISPLAY_PRICES))
            {
                if (product.CustomerEntersPrice)
                    return;

                if (product.CallForPrice &&
                    //also check whether the current user is impersonated
                    (!_orderSettings.AllowAdminsToBuyCallForPriceProducts ||
                     _workContext.OriginalCustomerIfImpersonated == null))
                {
                    //call for price
                    priceModel.OldPrice = await _localizationService.GetResourceAsync("Products.CallForPrice");
                    priceModel.Msrp = null;
                    priceModel.Price = null;
                    priceModel.MembershipPrice = null;
                }
                else
                {
                    //prices

                    var (msrpPriceBase, _) = await _taxService.GetProductPriceAsync(product, product.Msrp);
                    var (oldPriceBase, _) = await _taxService.GetProductPriceAsync(product, product.OldPrice);
                    var (priceBase, _) = await _taxService.GetProductPriceAsync(product, product.Price);



                    var oldPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(oldPriceBase, await _workContext.GetWorkingCurrencyAsync());
                    var msrp = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(msrpPriceBase, await _workContext.GetWorkingCurrencyAsync());
                    var price = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(priceBase, await _workContext.GetWorkingCurrencyAsync());

                    // memberShipPrice

                    // Need to confirm
                    //(var memberShipPrice, _) = await _shoppingcartService.MemberShipPriceOfProduct(product.Id, product.Msrp, product.OldPrice, product.Price);
                    //if (memberShipPrice > decimal.Zero)
                    //{
                    //    memberShipPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(memberShipPrice, await _workContext.GetWorkingCurrencyAsync());
                    //    priceModel.MembershipPrice = await _priceFormatter.FormatPriceAsync(memberShipPrice);
                    //    priceModel.MembershipPriceValue = memberShipPrice;
                    //}

                    // end

                    //When there is just one tier price (with  qty 1), there are no actual savings in the list.
                    var strikeThroughPrice = oldPrice;
                    if (strikeThroughPrice > decimal.Zero)
                    {
                        priceModel.OldPrice = await _priceFormatter.FormatPriceAsync(strikeThroughPrice);
                        priceModel.OldPriceValue = strikeThroughPrice;
                    }
                    else
                    {
                        priceModel.OldPrice = await _priceFormatter.FormatPriceAsync(price);
                        priceModel.OldPriceValue = price;
                    }

                    if (msrp > decimal.Zero && msrp > priceModel.OldPriceValue)
                    {
                        priceModel.Msrp = await _priceFormatter.FormatPriceAsync(msrp);
                        priceModel.MsrpValue = msrp;
                    }

                    priceModel.Price = await _priceFormatter.FormatPriceAsync(price);
                    priceModel.PriceValue = price;



                    //property for German market
                    //we display tax/shipping info only with "shipping enabled" for this product
                    //we also ensure this it's not free shipping
                    priceModel.DisplayTaxShippingInfo = _catalogSettings.DisplayTaxShippingInfoProductBoxes && product.IsShipEnabled && !product.IsFreeShipping;

                    //PAngV default baseprice (used in Germany)
                    priceModel.BasePricePAngV = await _priceFormatter.FormatBasePriceAsync(product, price);

                    if (product.IsVariantProduct && product.MinPrice > 0 && product.MaxPrice > 0 && product.MaxPrice > product.MinPrice)
                    {
                        var (minMsrpBase, _) = await _taxService.GetProductPriceAsync(product, product.MinMsrp);
                        var (maxMsrpBase, _) = await _taxService.GetProductPriceAsync(product, product.MaxMsrp);
                        var (minOldPriceBase, _) = await _taxService.GetProductPriceAsync(product, product.MinOldprice);
                        var (maxOldPriceBase, _) = await _taxService.GetProductPriceAsync(product, product.MaxOldPrice);
                        var (minPriceBase, _) = await _taxService.GetProductPriceAsync(product, product.MinPrice);
                        var (maxPriceBase, _) = await _taxService.GetProductPriceAsync(product, product.MaxPrice);

                        var minMsrp = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(minMsrpBase, await _workContext.GetWorkingCurrencyAsync());
                        var maxMsrp = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(maxMsrpBase, await _workContext.GetWorkingCurrencyAsync());
                        var minOldPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(minOldPriceBase, await _workContext.GetWorkingCurrencyAsync());
                        var maxOldPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(maxOldPriceBase, await _workContext.GetWorkingCurrencyAsync());
                        var minPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(minPriceBase, await _workContext.GetWorkingCurrencyAsync());
                        var maxPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(maxPriceBase, await _workContext.GetWorkingCurrencyAsync());

                        // Need to confirm
                        //(var minMembershipPriceValue, _) = await _shoppingcartService.MemberShipPriceOfProduct(product.Id, product.MinMsrp, product.MinOldprice, product.MinPrice);
                        //(var maxMembershipPriceValue, _) = await _shoppingcartService.MemberShipPriceOfProduct(product.Id, product.MaxMsrp, product.MaxOldPrice, product.MaxPrice);
                        //if (minMembershipPriceValue > decimal.Zero && maxMembershipPriceValue > decimal.Zero)
                        //{
                        //    minMembershipPriceValue = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(minMembershipPriceValue, await _workContext.GetWorkingCurrencyAsync());
                        //    priceModel.MinMembershipPrice = await _priceFormatter.FormatPriceAsync(minMembershipPriceValue);
                        //    priceModel.MinMembershipPriceValue = minMembershipPriceValue;

                        //    maxMembershipPriceValue = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(maxMembershipPriceValue, await _workContext.GetWorkingCurrencyAsync());
                        //    priceModel.MaxMembershipPrice = await _priceFormatter.FormatPriceAsync(maxMembershipPriceValue);
                        //    priceModel.MaxMembershipPriceValue = maxMembershipPriceValue;
                        //}


                        strikeThroughPrice = minOldPrice;
                        if (strikeThroughPrice > decimal.Zero)
                        {
                            priceModel.MinOldPrice = await _priceFormatter.FormatPriceAsync(strikeThroughPrice);
                            priceModel.MinOldPriceValue = strikeThroughPrice;

                            priceModel.MaxOldPrice = await _priceFormatter.FormatPriceAsync(maxOldPrice);
                            priceModel.MaxOldPriceValue = maxOldPrice;
                        }
                        else
                        {
                            priceModel.MinOldPrice = await _priceFormatter.FormatPriceAsync(minPriceBase);
                            priceModel.MinOldPriceValue = minPriceBase;
                            priceModel.MaxOldPrice = await _priceFormatter.FormatPriceAsync(maxPrice);
                            priceModel.MaxOldPriceValue = maxPrice;
                        }

                        if (minMsrp > decimal.Zero && minMsrp > (minOldPrice == 0 ? minPrice : minOldPrice))
                        {
                            priceModel.MinMsrp = await _priceFormatter.FormatPriceAsync(minMsrp);
                            priceModel.MinMsrpValue = minMsrp;

                            priceModel.MaxMsrp = await _priceFormatter.FormatPriceAsync(
                                maxMsrp > (maxOldPrice < maxPrice ? maxPrice : maxOldPrice) ? maxMsrp : (maxOldPrice < maxPrice ? maxPrice : maxOldPrice));
                            priceModel.MaxMsrpValue = maxMsrp > (maxOldPrice < maxPrice ? maxPrice : maxOldPrice) ? maxMsrp : (maxOldPrice < maxPrice ? maxPrice : maxOldPrice);
                        }

                        priceModel.MinPrice = await _priceFormatter.FormatPriceAsync(minPrice);
                        priceModel.MinPriceValue = minPrice;


                        priceModel.MaxPrice = await _priceFormatter.FormatPriceAsync(maxPrice);
                        priceModel.MaxPriceValue = maxPrice;
                        priceModel.IsVariantProduct = product.IsVariantProduct;
                    }

                }
            }
            else
            {
                //hide prices
                priceModel.MembershipPrice = null;
                priceModel.Msrp = null;
                priceModel.OldPrice = null;
                priceModel.Price = null;
            }


            priceModel.Price = CustomCommonHelper.FormatCurrencyPriceWithoutDecimal(priceModel.Price);
            priceModel.PriceValue = CustomCommonHelper.FormatPriceWithoutDecimal(priceModel.PriceValue);
            priceModel.OldPrice = CustomCommonHelper.FormatCurrencyPriceWithoutDecimal(priceModel.OldPrice);
            priceModel.OldPriceValue = CustomCommonHelper.FormatPriceWithoutDecimal(priceModel.OldPriceValue);
            priceModel.MinPrice = CustomCommonHelper.FormatCurrencyPriceWithoutDecimal(priceModel.MinPrice);
            priceModel.MinPriceValue = CustomCommonHelper.FormatPriceWithoutDecimal(priceModel.MinPriceValue);
            priceModel.MinOldPrice = CustomCommonHelper.FormatCurrencyPriceWithoutDecimal(priceModel.MinOldPrice);
            priceModel.MinOldPriceValue = CustomCommonHelper.FormatPriceWithoutDecimal(priceModel.MinOldPriceValue);
            priceModel.MaxPrice = CustomCommonHelper.FormatCurrencyPriceWithoutDecimal(priceModel.MaxPrice);
            priceModel.MaxPriceValue = CustomCommonHelper.FormatPriceWithoutDecimal(priceModel.MaxPriceValue);
            priceModel.MaxOldPrice = CustomCommonHelper.FormatCurrencyPriceWithoutDecimal(priceModel.MaxOldPrice);
            priceModel.MaxOldPriceValue = CustomCommonHelper.FormatPriceWithoutDecimal(priceModel.MaxOldPriceValue);
            (priceModel.OfferText, priceModel.OfferPlaceHolder, priceModel.DiscountAmount, priceModel.DiscountPercentage, priceModel.SaleStartDate, priceModel.SaleEndDate) =
                await _customProductService.GetProductSaleOfferInfo(product, priceModel.MinOldPrice != priceModel.MaxOldPrice ? null : priceModel.OldPriceValue, priceModel.MinPrice != priceModel.MaxPrice ? null : priceModel.PriceValue); ;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrepareCustomGroupedProductOverviewPriceModelAsync(Product product, CustomProductOverviewModel.ProductPriceModel priceModel)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var customer = await _workContext.GetCurrentCustomerAsync();
            var associatedProducts = await _productService.GetAssociatedProductsAsync(product.Id,
                store.Id);

            //add to cart button (ignore "DisableBuyButton" property for grouped products)
            priceModel.DisableBuyButton =
                !await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.ENABLE_SHOPPING_CART) ||
                !await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.DISPLAY_PRICES);

            //add to wishlist button (ignore "DisableWishlistButton" property for grouped products)
            priceModel.DisableWishlistButton =
                !await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.ENABLE_WISHLIST) ||
                !await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.DISPLAY_PRICES);

            //compare products
            priceModel.DisableAddToCompareListButton = !_catalogSettings.CompareProductsEnabled;
            if (!associatedProducts.Any())
                return;

            //we have at least one associated product
            if (await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.DISPLAY_PRICES))
            {
                //find a minimum possible price
                decimal? minPossiblePrice = null;
                Product minPriceProduct = null;
                foreach (var associatedProduct in associatedProducts)
                {
                    var (_, tmpMinPossiblePrice, _, _) = await _priceCalculationService.GetFinalPriceAsync(
                     product: associatedProduct,
                      customer: customer,
                    overriddenProductPrice: (associatedProduct.OldPrice > decimal.Zero ? associatedProduct.OldPrice : null),
                    additionalCharge: 0,
                    includeDiscounts: true,
                    quantity: 1,
                    rentalEndDate: null,
                    rentalStartDate: null, store: store);

                    var tierPrices = await _productService.GetTierPricesAsync(product, customer, store);
                    if (tierPrices.Any() && (tierPrices.Count > 1 || tierPrices[0].Quantity > 1))
                    {

                        //calculate price for the maximum quantity if we have tier prices, and choose minimal
                        tmpMinPossiblePrice = Math.Min(tmpMinPossiblePrice,
                            (await _priceCalculationService.GetFinalPriceAsync(associatedProduct, await _workContext.GetCurrentCustomerAsync(), quantity: int.MaxValue,store:store)).Item1);
                    }

                    if (minPossiblePrice.HasValue && tmpMinPossiblePrice >= minPossiblePrice.Value)
                        continue;
                    minPriceProduct = associatedProduct;
                    minPossiblePrice = tmpMinPossiblePrice;
                }

                if (minPriceProduct == null || minPriceProduct.CustomerEntersPrice)
                    return;

                if (minPriceProduct.CallForPrice &&
                    //also check whether the current user is impersonated
                    (!_orderSettings.AllowAdminsToBuyCallForPriceProducts ||
                     _workContext.OriginalCustomerIfImpersonated == null))
                {
                    priceModel.OldPrice = await _localizationService.GetResourceAsync("Products.CallForPrice");
                    priceModel.Msrp = null;
                    priceModel.MembershipPrice = null;
                    priceModel.Price = await _localizationService.GetResourceAsync("Products.CallForPrice");
                }
                else
                {
                    //calculate prices
                    var (finalPriceBase, _) = await _taxService.GetProductPriceAsync(minPriceProduct, minPossiblePrice.Value);
                    var finalPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(finalPriceBase, await _workContext.GetWorkingCurrencyAsync());

                    priceModel.OldPrice = string.Format(await _localizationService.GetResourceAsync("Products.PriceRangeFrom"), await _priceFormatter.FormatPriceAsync(finalPrice));
                    priceModel.OldPriceValue = finalPrice;
                    priceModel.Price = string.Format(await _localizationService.GetResourceAsync("Products.PriceRangeFrom"), await _priceFormatter.FormatPriceAsync(finalPrice));
                    priceModel.PriceValue = finalPrice;

                    //PAngV default baseprice (used in Germany)
                    priceModel.BasePricePAngV = await _priceFormatter.FormatBasePriceAsync(product, finalPriceBase);
                }
            }
            else
            {
                //hide prices
                priceModel.OldPrice = null;
                priceModel.Price = null;
            }
        }
        protected virtual async Task<CustomProductDetailsModel.ProductPriceModel> PrepareCustomProductPriceModelAsync(Product product, int variantId = 0)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var model = new CustomProductDetailsModel.ProductPriceModel
            {
                ProductId = product.Id
            };

            if (await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.DISPLAY_PRICES))
            {
                model.HidePrices = false;
                if (product.CustomerEntersPrice)
                {
                    model.CustomerEntersPrice = true;
                }
                else
                {
                    if (product.CallForPrice &&
                        //also check whether the current user is impersonated
                        (!_orderSettings.AllowAdminsToBuyCallForPriceProducts || _workContext.OriginalCustomerIfImpersonated == null))
                    {
                        model.CallForPrice = true;
                    }
                    else
                    {
                        bool isVariantExist = false; decimal variantMsrp = 0; decimal variantOldPrice = 0;
                        decimal variantPrice = 0;
                        if (variantId != 0)
                        {
                            (isVariantExist, variantMsrp, variantOldPrice, variantPrice) = await GetVariantPrice(product, variantId);
                        }
                        if (!isVariantExist)
                        {
                            variantMsrp = product.Msrp;
                            variantOldPrice = product.OldPrice;
                            variantPrice = product.Price;
                        }
                        var (oldPriceBase, _) = await _taxService.GetProductPriceAsync(product, variantOldPrice);
                        var (msrpBase, _) = await _taxService.GetProductPriceAsync(product, variantMsrp);
                        var (priceBase, _) = await _taxService.GetProductPriceAsync(product, variantPrice);


                        var oldPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(oldPriceBase, await _workContext.GetWorkingCurrencyAsync());
                        var msrp = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(msrpBase, await _workContext.GetWorkingCurrencyAsync());
                        var price = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(priceBase, await _workContext.GetWorkingCurrencyAsync());

                        // Need to confirm
                        //(var memberShipPrice, _) = await _shoppingcartService.MemberShipPriceOfProduct(product.Id, variantMsrp, variantOldPrice, variantPrice);

                        //if (memberShipPrice > decimal.Zero)
                        //{
                        //    memberShipPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(memberShipPrice, await _workContext.GetWorkingCurrencyAsync());
                        //    model.MembershipPrice = await _priceFormatter.FormatPriceAsync(memberShipPrice);
                        //    model.MembershipPriceValue = memberShipPrice;
                        //}


                        //When there is just one tier price (with  qty 1), there are no actual savings in the list.
                        var strikeThroughPrice = oldPrice;
                        if (strikeThroughPrice > decimal.Zero)
                        {
                            model.OldPrice = await _priceFormatter.FormatPriceAsync(strikeThroughPrice);
                            model.OldPriceValue = strikeThroughPrice;
                        }
                        else
                        {
                            model.OldPrice = await _priceFormatter.FormatPriceAsync(price);
                            model.OldPriceValue = price;
                        }


                        if (msrp > decimal.Zero && msrp > model.OldPriceValue)
                        {
                            model.Msrp = await _priceFormatter.FormatPriceAsync(msrp);
                            model.MsrpValue = msrp;
                        }

                        model.Price = await _priceFormatter.FormatPriceAsync(price);
                        model.PriceValue = price;
                        // memberShipPrice

                        #region Range

                        if (product.IsVariantProduct && product.MinPrice > 0 && product.MaxPrice > 0 && product.MaxPrice > product.MinPrice)
                        {
                            var (minMsrpBase, _) = await _taxService.GetProductPriceAsync(product, product.MinMsrp);
                            var (maxMsrpBase, _) = await _taxService.GetProductPriceAsync(product, product.MaxMsrp);
                            var (minOldPriceBase, _) = await _taxService.GetProductPriceAsync(product, product.MinOldprice);
                            var (maxOldPriceBase, _) = await _taxService.GetProductPriceAsync(product, product.MaxOldPrice);
                            var (minPriceBase, _) = await _taxService.GetProductPriceAsync(product, product.MinPrice);
                            var (maxPriceBase, _) = await _taxService.GetProductPriceAsync(product, product.MaxPrice);

                            var minMsrp = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(minMsrpBase, await _workContext.GetWorkingCurrencyAsync());
                            var maxMsrp = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(maxMsrpBase, await _workContext.GetWorkingCurrencyAsync());
                            var minOldPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(minOldPriceBase, await _workContext.GetWorkingCurrencyAsync());
                            var maxOldPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(maxOldPriceBase, await _workContext.GetWorkingCurrencyAsync());
                            var minPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(minPriceBase, await _workContext.GetWorkingCurrencyAsync());
                            var maxPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(maxPriceBase, await _workContext.GetWorkingCurrencyAsync());

                            // Need to confirm
                            //(var minMembershipPriceValue, _) = await _shoppingcartService.MemberShipPriceOfProduct(product.Id, product.MinMsrp, product.MinOldprice, product.MinPrice);
                            //(var maxMembershipPriceValue, _) = await _shoppingcartService.MemberShipPriceOfProduct(product.Id, product.MaxMsrp, product.MaxOldPrice, product.MaxPrice);
                            //if (minMembershipPriceValue > decimal.Zero && maxMembershipPriceValue > decimal.Zero)
                            //{
                            //    minMembershipPriceValue = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(minMembershipPriceValue, await _workContext.GetWorkingCurrencyAsync());
                            //    model.MinMembershipPrice = await _priceFormatter.FormatPriceAsync(minMembershipPriceValue);
                            //    model.MinMembershipPriceValue = minMembershipPriceValue;

                            //    maxMembershipPriceValue = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(maxMembershipPriceValue, await _workContext.GetWorkingCurrencyAsync());
                            //    model.MaxMembershipPrice = await _priceFormatter.FormatPriceAsync(maxMembershipPriceValue);
                            //    model.MaxMembershipPriceValue = maxMembershipPriceValue;
                            //}


                            strikeThroughPrice = minOldPrice;
                            if (strikeThroughPrice > decimal.Zero)
                            {
                                model.MinOldPrice = await _priceFormatter.FormatPriceAsync(strikeThroughPrice);
                                model.MinOldPriceValue = strikeThroughPrice;

                                model.MaxOldPrice = await _priceFormatter.FormatPriceAsync(maxOldPrice);
                                model.MaxOldPriceValue = maxOldPrice;
                            }
                            else
                            {
                                model.MinOldPrice = await _priceFormatter.FormatPriceAsync(minPriceBase);
                                model.MinOldPriceValue = minPriceBase;
                                model.MaxOldPrice = await _priceFormatter.FormatPriceAsync(maxPrice);
                                model.MaxOldPriceValue = maxPrice;
                            }

                            if (minMsrp > decimal.Zero && minMsrp > (minOldPrice == 0 ? minPrice : minOldPrice))
                            {
                                model.MinMsrp = await _priceFormatter.FormatPriceAsync(minMsrp);
                                model.MinMsrpValue = minMsrp;

                                model.MaxMsrp = await _priceFormatter.FormatPriceAsync(
                                    maxMsrp > (maxOldPrice < maxPrice ? maxPrice : maxOldPrice) ? maxMsrp : (maxOldPrice < maxPrice ? maxPrice : maxOldPrice));
                                model.MaxMsrpValue = maxMsrp > (maxOldPrice < maxPrice ? maxPrice : maxOldPrice) ? maxMsrp : (maxOldPrice < maxPrice ? maxPrice : maxOldPrice);
                            }

                            model.MinPrice = await _priceFormatter.FormatPriceAsync(minPrice);
                            model.MinPriceValue = minPrice;


                            model.MaxPrice = await _priceFormatter.FormatPriceAsync(maxPrice);
                            model.MaxPriceValue = maxPrice;
                            model.IsVariantProduct = product.IsVariantProduct;
                        }

                        #endregion




                        //property for German market
                        //we display tax/shipping info only with "shipping enabled" for this product
                        //we also ensure this it's not free shipping
                        model.DisplayTaxShippingInfo = _catalogSettings.DisplayTaxShippingInfoProductDetailsPage
                            && product.IsShipEnabled &&
                            !product.IsFreeShipping;

                        //PAngV baseprice (used in Germany)
                        model.BasePricePAngV = await _priceFormatter.FormatBasePriceAsync(product, price);
                        //currency code
                        model.CurrencyCode = (await _workContext.GetWorkingCurrencyAsync()).CurrencyCode;

                        //rental
                        if (product.IsRental)
                        {
                            model.IsRental = true;
                            var priceStr = await _priceFormatter.FormatPriceAsync(price);
                            model.RentalPrice = await _priceFormatter.FormatRentalProductPeriodAsync(product, priceStr);
                        }

                    }
                }
            }
            else
            {
                model.HidePrices = true;
                model.OldPrice = null;
                model.Price = null;
            }

            model.Price = CustomCommonHelper.FormatCurrencyPriceWithoutDecimal(model.Price);
            model.PriceValue = CustomCommonHelper.FormatPriceWithoutDecimal(model.PriceValue);
            model.OldPrice = CustomCommonHelper.FormatCurrencyPriceWithoutDecimal(model.OldPrice);
            model.OldPriceValue = CustomCommonHelper.FormatPriceWithoutDecimal(model.OldPriceValue);
            model.MinPrice = CustomCommonHelper.FormatCurrencyPriceWithoutDecimal(model.MinPrice);
            model.MinPriceValue = CustomCommonHelper.FormatPriceWithoutDecimal(model.MinPriceValue);
            model.MinOldPrice = CustomCommonHelper.FormatCurrencyPriceWithoutDecimal(model.MinOldPrice);
            model.MinOldPriceValue = CustomCommonHelper.FormatPriceWithoutDecimal(model.MinOldPriceValue);
            model.MaxPrice = CustomCommonHelper.FormatCurrencyPriceWithoutDecimal(model.MaxPrice);
            model.MaxPriceValue = CustomCommonHelper.FormatPriceWithoutDecimal(model.MaxPriceValue);
            model.MaxOldPrice = CustomCommonHelper.FormatCurrencyPriceWithoutDecimal(model.MaxOldPrice);
            model.MaxOldPriceValue = CustomCommonHelper.FormatPriceWithoutDecimal(model.MaxOldPriceValue);
            (model.OfferText, model.OfferPlaceHolder, model.DiscountAmount, model.DiscountPercentage, model.SaleStartDate, model.SaleEndDate) =
                await _customProductService.GetProductSaleOfferInfo(product, model.OldPriceValue, model.PriceValue);


            return model;
        }

        public async Task<CustomizationFormModel> PrepareCustomizationFormModelAsync(Product product)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var address = await _customerService.GetCustomerBillingAddressAsync(customer);
            CustomizationFormModel model = new CustomizationFormModel();
            model.DisplayCaptcha = _captchaSettings.Enabled;
            model.FullName = string.IsNullOrEmpty(address?.FirstName) ? "" : address.FirstName + (string.IsNullOrEmpty(address?.LastName) ? "" : " " + address.LastName);
            model.Email = customer.Email;
            model.ZipCode = string.IsNullOrEmpty(address?.ZipPostalCode) ? "" : address.ZipPostalCode;
            model.Phone = string.IsNullOrEmpty(address?.PhoneNumber) ? "" : address.PhoneNumber;
            string interestedItems = await _localizationService.GetResourceAsync("CustomizationForm.InterestedItems");
            if (!string.IsNullOrEmpty(interestedItems) && interestedItems != "CustomizationForm.InterestedItems")
                model.LstInteresedIn = interestedItems.Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();
            model.ProductAttributes = await PrepareCustomProductAttributeModelsAsync(product, null);
            model.ProductId = product.Id;
            model.ProductName = product.Name;
            model.ProductSpecificationModel = await PrepareProductSpecificationModelAsync(product);
            var picture = (await this.PrepareProductOverviewPicturesModelAsync(product, 200)).FirstOrDefault();
            model.DefaultPicture = string.IsNullOrEmpty(picture.ThumbImageUrl) ? picture.ImageUrl : picture.ThumbImageUrl;
            IList<CustomPictureModel> allPictureModels;
            IList<VideoModel> allvideoModels;
            (model.DefaultPictureModel, allPictureModels, allvideoModels) = await CustomPrepareProductDetailsPictureModelAsync(product, false);
            model.PictureModels = allPictureModels;
            string dimension = "";
            if (product.Length > 0)
                dimension = Math.Round(product.Length, 2).ToString("G29") + "\" L";
            if (product.Width > 0)
                dimension += (dimension.Length > 0 ? " X " : "") + Math.Round(product.Width, 2).ToString("G29") + "\" D";
            if (product.Height > 0)
                dimension += (dimension.Length > 0 ? " X " : "") + Math.Round(product.Height, 2).ToString("G29") + "\" H";
            model.Dimensions = dimension;
            model.Sku = product.Sku;

            return model;
        }



        public async Task<CategoryGroupProductModel> GetCategoryGroupedProducts(int categoryId)
        {

            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.CategoryGroupedProductsCacheKey, categoryId);
            return await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                CategoryGroupProductModel model = new CategoryGroupProductModel();
                List<GroupProduct> grpProducts = new List<GroupProduct>();
                var products = await _customProductService.GetGroupProductsByEntity(categoryId, _storeContext.GetCurrentStore().Id);

                var groupedProducts = products.Where(p => p.ProductType == ProductType.GroupedProduct).ToList();
                foreach (var _groupProduct in groupedProducts)
                {
                    var _associatedProducts = products.Where(p => p.ParentGroupedProductId == _groupProduct.Id).ToList();
                    if (_associatedProducts.Count > 0)
                    {
                        List<Product> lstproducts = new List<Product>();
                        lstproducts.Add(_groupProduct);

                        List<Product> associatedProducts = new List<Product>();
                        foreach (var _associatedProduct in _associatedProducts)
                        {
                            if (!associatedProducts.Where(m => m.Id == _associatedProduct.Id).Any())
                                associatedProducts.Add(_associatedProduct);
                        }

                        GroupProduct groupProduct = new GroupProduct();
                        groupProduct.Product = (await PrepareCustomProductOverviewModelsAsync(lstproducts,
                            productThumbPictureSize: _mediaSettings.ProductDetailsPictureSize, prepareSizeShadeAggregation: true)).FirstOrDefault();

                        groupProduct.AssociatedProducts = (await PrepareCustomProductOverviewModelsAsync(associatedProducts,
                            productThumbPictureSize: _mediaSettings.AssociatedProductPictureSize)).ToList();

                        grpProducts.Add(groupProduct);
                    }
                }
                model.GroupProducts = grpProducts;
                return model;

            });

        }

        protected virtual async Task<(CustomPictureModel pictureModel, IList<CustomPictureModel> allPictureModels, IList<VideoModel>)> CustomPrepareProductDetailsPictureModelAsync(Product product, bool isAssociatedProduct, int pictureId = 0)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));
            //default picture size
            var defaultPictureSize = isAssociatedProduct ?
                _mediaSettings.AssociatedProductPictureSize :
                _mediaSettings.ProductDetailsPictureSize;

            //prepare picture models
            var productPicturesCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.CustomProductDetailsPicturesModelKey
                , product, pictureId, defaultPictureSize, isAssociatedProduct,
                await _workContext.GetWorkingLanguageAsync(), _webHelper.IsCurrentConnectionSecured(), await _storeContext.GetCurrentStoreAsync());
            var cachedPictures = await _staticCacheManager.GetAsync(productPicturesCacheKey, async () =>
            {
                int dimensionImageIndex = await _settingService.GetSettingByKeyAsync<int>("product.Dimension.Image.Index");
                dimensionImageIndex = dimensionImageIndex == 0 ? 3 : dimensionImageIndex;
                var productName = await _localizationService.GetLocalizedAsync(product, x => x.Name);

                var pictures = await _customPictureService.CustomGetPicturesOfProducWithDimensionImageAsync(product.Id);
                var dimensionImage = pictures.FirstOrDefault(p => p.IsDimensionImage);
                if (dimensionImage != null)
                {
                    pictures.Remove(dimensionImage);
                    int targetIndex = pictures.Count >= dimensionImageIndex ? dimensionImageIndex : pictures.Count;
                    pictures.Insert(targetIndex, dimensionImage);
                }




                var defaultPicture = new Picture();
                if (pictureId == 0)
                {
                    defaultPicture = pictures.FirstOrDefault();
                }
                else
                {
                    defaultPicture = pictures.Where(p => p.Id == pictureId).FirstOrDefault() ?? pictures.FirstOrDefault();
                }


                string fullSizeImageUrl, imageUrl, thumbImageUrl;
                (imageUrl, defaultPicture) = await _pictureService.GetPictureUrlAsync(defaultPicture, defaultPictureSize, true);
                (fullSizeImageUrl, defaultPicture) = await _pictureService.GetPictureUrlAsync(defaultPicture, 0, true);

                var defaultPictureModel = new CustomPictureModel
                {
                    ImageUrl = imageUrl,
                    FullSizeImageUrl = fullSizeImageUrl
                };
                //"title" attribute
                defaultPictureModel.Title = (defaultPicture != null && !string.IsNullOrEmpty(defaultPicture.TitleAttribute)) ?
                    defaultPicture.TitleAttribute :
                    string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat.Details"), productName);
                //"alt" attribute
                defaultPictureModel.AlternateText = (defaultPicture != null && !string.IsNullOrEmpty(defaultPicture.AltAttribute)) ?
                    defaultPicture.AltAttribute :
                    string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat.Details"), productName);

                //all pictures
                var pictureModels = new List<CustomPictureModel>();
                for (var i = 0; i < pictures.Count(); i++)
                {
                    var picture = (Picture)pictures[i];

                    (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, defaultPictureSize, !isAssociatedProduct);
                    (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                    (thumbImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage);

                    var pictureModel = new CustomPictureModel
                    {
                        ImageUrl = imageUrl,
                        ThumbImageUrl = pictures[i].IsDimensionImage ? "/images/product/thumb-dimension-images.png" : thumbImageUrl,
                        FullSizeImageUrl = fullSizeImageUrl,
                        Title = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat.Details"), productName),
                        AlternateText = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat.Details"), productName),
                        IsDimensionImage = pictures[i].IsDimensionImage
                    };
                    //"title" attribute
                    pictureModel.Title = !string.IsNullOrEmpty(picture.TitleAttribute) ?
                        picture.TitleAttribute :
                        string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat.Details"), productName);
                    //"alt" attribute
                    pictureModel.AlternateText = !string.IsNullOrEmpty(picture.AltAttribute) ?
                        picture.AltAttribute :
                        string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat.Details"), productName);

                    pictureModels.Add(pictureModel);
                }
                var allvideoModels = new List<VideoModel>();
                var videos = await _videoService.GetVideosByProductIdAsync(product.Id);
                foreach (var video in videos)
                {
                    var videoModel = new VideoModel
                    {
                        VideoUrl = video.VideoUrl,
                        Allow = _mediaSettings.VideoIframeAllow,
                        Width = _mediaSettings.VideoIframeWidth,
                        Height = _mediaSettings.VideoIframeHeight
                    };

                    allvideoModels.Add(videoModel);
                }
                return new { DefaultPictureModel = defaultPictureModel, PictureModels = pictureModels, Videos = allvideoModels };
            });

            var allPictureModels = cachedPictures.PictureModels;
            return (cachedPictures.DefaultPictureModel, allPictureModels, cachedPictures.Videos);
        }

        protected virtual async Task<(CustomPictureModel pictureModel, IList<CustomPictureModel> allPictureModels)> CustomfeedrepareProductDetailsPictureModelAsync(Product product, bool isAssociatedProduct)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            //default picture size
            var defaultPictureSize = isAssociatedProduct ?
                _mediaSettings.AssociatedProductPictureSize :
                _mediaSettings.ProductDetailsPictureSize;

            //prepare picture models
            var productPicturesCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.ProductDetailsPicturesModelKey
                , product, defaultPictureSize, isAssociatedProduct,
                await _workContext.GetWorkingLanguageAsync(), _webHelper.IsCurrentConnectionSecured(), await _storeContext.GetCurrentStoreAsync());
            var cachedPictures = await _staticCacheManager.GetAsync(productPicturesCacheKey, async () =>
            {
                var productName = await _localizationService.GetLocalizedAsync(product, x => x.Name);

                var pictures = await _customPictureService.CustomGetPicturesOfProducAsync(product.Id);
                var defaultPicture = pictures.FirstOrDefault();

                string fullSizeImageUrl, imageUrl, thumbImageUrl;
                (imageUrl, defaultPicture) = await _pictureService.GetPictureUrlAsync(defaultPicture, defaultPictureSize, false);
                (fullSizeImageUrl, defaultPicture) = await _pictureService.GetPictureUrlAsync(defaultPicture, 0, false);

                var defaultPictureModel = new CustomPictureModel
                {
                    ImageUrl = imageUrl,
                    FullSizeImageUrl = fullSizeImageUrl
                };
                //"title" attribute
                defaultPictureModel.Title = (defaultPicture != null && !string.IsNullOrEmpty(defaultPicture.TitleAttribute)) ?
                    defaultPicture.TitleAttribute :
                    string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat.Details"), productName);
                //"alt" attribute
                defaultPictureModel.AlternateText = (defaultPicture != null && !string.IsNullOrEmpty(defaultPicture.AltAttribute)) ?
                    defaultPicture.AltAttribute :
                    string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat.Details"), productName);

                //all pictures
                var pictureModels = new List<CustomPictureModel>();
                for (var i = 0; i < pictures.Count(); i++)
                {
                    var picture = pictures[i];

                    (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, defaultPictureSize, !isAssociatedProduct);
                    (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                    (thumbImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage);

                    var pictureModel = new CustomPictureModel
                    {
                        ImageUrl = imageUrl,
                        ThumbImageUrl = thumbImageUrl,
                        FullSizeImageUrl = fullSizeImageUrl,
                        Title = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat.Details"), productName),
                        AlternateText = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat.Details"), productName),
                    };
                    //"title" attribute
                    pictureModel.Title = !string.IsNullOrEmpty(picture.TitleAttribute) ?
                        picture.TitleAttribute :
                        string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat.Details"), productName);
                    //"alt" attribute
                    pictureModel.AlternateText = !string.IsNullOrEmpty(picture.AltAttribute) ?
                        picture.AltAttribute :
                        string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat.Details"), productName);

                    pictureModels.Add(pictureModel);
                }

                return new { DefaultPictureModel = defaultPictureModel, PictureModels = pictureModels };
            });

            var allPictureModels = cachedPictures.PictureModels;
            return (cachedPictures.DefaultPictureModel, allPictureModels);
        }

        public virtual async Task<CustomPictureModel> PrepareCustomProductOverviewPictureModelAsync(Product product, int? productThumbPictureSize = null, bool isCategorypage = false)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var productName = await _localizationService.GetLocalizedAsync(product, x => x.Name);
            //If a size has been set in the view, we use it in priority
            var pictureSize = productThumbPictureSize ?? _mediaSettings.ProductThumbPictureSize;

            //prepare picture model
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.CustomProductDefaultPictureModelKey,
                product, pictureSize, true, await _workContext.GetWorkingLanguageAsync(), _webHelper.IsCurrentConnectionSecured(),
                await _storeContext.GetCurrentStoreAsync(), isCategorypage ? "category" : "listing");

            var defaultPictureModel = await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                var picture = (await _customPictureService.GetproductListingimage(product.Id, isCategorypage));
                string fullSizeImageUrl, imageUrl;
                (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, pictureSize);
                (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);

                var pictureModel = new CustomPictureModel
                {
                    ImageUrl = imageUrl,
                    FullSizeImageUrl = fullSizeImageUrl,
                    //"title" attribute
                    Title = (picture != null && !string.IsNullOrEmpty(picture.TitleAttribute))
                        ? picture.TitleAttribute
                        : string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat"),
                            productName),
                    //"alt" attribute
                    AlternateText = (picture != null && !string.IsNullOrEmpty(picture.AltAttribute))
                        ? picture.AltAttribute
                        : string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat"),
                            productName)
                };

                return pictureModel;
            });

            return defaultPictureModel;
        }


        public async Task<List<GroupedProductConfigurationModel>> PrepareGroupedProductConfiguration(int productId)
        {
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.AssociatedProductsConfigurationCacheKey, productId);
            var configurationsModel = await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                Dictionary<int, string> attributes = new Dictionary<int, string>();
                attributes.Add(0, "Default");
                var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(productId);
                foreach (var attribute in productAttributeMapping)
                {
                    var productAttrubute = await _productAttributeService.GetProductAttributeByIdAsync(attribute.ProductAttributeId);
                    if (productAttrubute.Name == await _localizationService.GetResourceAsync("Product.Attr.Size"))
                    {
                        var attributeValues = await _productAttributeService.GetProductAttributeValuesAsync(attribute.Id);
                        foreach (var attributeValue in attributeValues)
                        {
                            attributes.Add(attributeValue.Id, attributeValue.Name);
                        }
                    }
                }


                var configurations = await _groupedProductConfigurationService.GetConfigurationOfGroupedProduct(productId);

                List<GroupedProductConfigurationModel> _configurationsModel = new List<GroupedProductConfigurationModel>();
                var associatedProducts = (await _productService.GetAssociatedProductsAsync(showHidden: true,
                   parentGroupedProductId: productId,
                   vendorId: (await _workContext.GetCurrentVendorAsync())?.Id ?? 0));

                foreach (var attribute in attributes)
                {
                    var configuration = configurations.Where(c => c.ProductAttributeOptionId == attribute.Key).FirstOrDefault();
                    if (configuration != null)
                    {
                        GroupedProductConfigurationModel model = new GroupedProductConfigurationModel();
                        model.ProductId = productId;
                        model.ProductAttributeOptionId = attribute.Key;
                        // model.ProductAttributeOptionName = attribute.Value;



                        List<GrpConfiguration> dbConfiguration = new List<GrpConfiguration>();
                        if (!string.IsNullOrEmpty(configuration?.Raw))
                        {
                            dbConfiguration = JsonConvert.DeserializeObject<List<GrpConfiguration>>(configuration.Raw);
                        }
                        model.Id = configuration?.Id ?? 0;
                        List<GrpConfiguration> lstConfigurations = new List<GrpConfiguration>();
                        foreach (var associateProduct in associatedProducts)
                        {
                            GrpConfiguration _configuration = new GrpConfiguration();
                            _configuration.ProductId = associateProduct.Id;
                            _configuration.Sku = associateProduct.Sku;
                            _configuration.Quantity = dbConfiguration.Where(c => c.ProductId == associateProduct.Id).FirstOrDefault()?.Quantity ?? 1;
                            lstConfigurations.Add(_configuration);
                        }
                        model.Configurations = lstConfigurations;
                        _configurationsModel.Add(model);
                    }
                }
                return _configurationsModel;
            });
            return configurationsModel;
        }
        #endregion

        #region Feed
        public async Task<List<ProductModel>> PrepareProductFeed(int pageNumber, int pageSize)
        {
            var products = await _customProductService.ProductsFeedAsync(pageNumber - 1, pageSize);
            var categorySeprator = await _localizationService.GetResourceAsync("Product.Feed.Category.Seprator");
            var excludedCategories = await _settingService.GetSettingByKeyAsync<string>("Product.Feed.Category.Excluded");

            List<int> lstexcludedCategories = new List<int>();
            foreach (var category in excludedCategories.Split(','))
            {
                int.TryParse(category, out int categoryId);
                if (categoryId != 0)
                    lstexcludedCategories.Add(categoryId);
            }

            List<ProductModel> lstModel = new List<ProductModel>();
            foreach (var product in products)
            {

                ProductModel model = new ProductModel();

                var categories = await _categoryService.GetProductCategoriesByProductIdAsync(product.Id);
                List<ProductModel.CategoryModel> lstCategoriesModel = new List<ProductModel.CategoryModel>();
                foreach (var category in categories)
                {
                    ProductModel.CategoryModel categoryModel = new
                        ProductModel.CategoryModel();
                    var categoryObject = await _categoryService.GetCategoryByIdAsync(category.CategoryId);
                    if (categoryObject != null)
                    {

                        categoryModel.Id = categoryObject.Id;
                        categoryModel.Name = categoryObject.Name;
                        categoryModel.Sename = await _urlRecordService.GetSeNameAsync(categoryObject);
                        categoryModel.Path = await this.CategoryPath(categorySeprator, categoryObject.Id,
                            categoryObject.Name, categoryObject.ParentCategoryId, lstexcludedCategories);
                        lstCategoriesModel.Add(categoryModel);
                    }

                }

                #region Individual Products

                model.IndividualProducts = string.Join(',', await _customProductService.GetPairWithProductsByProductId1Async(product.Id));

                #endregion

                var prdManufacturers = await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id, true);
                if (prdManufacturers.Count > 0)
                {
                    var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(prdManufacturers.FirstOrDefault().ManufacturerId);
                    model.Manufacturer = manufacturer.Name;
                }

                model.Categories = lstCategoriesModel;

                var Attributes = await PrepareProductSpecificationAttributeModelAsync(product, null);
                foreach (var attr in Attributes)
                {
                    if (attr.Name.Equals("Froggle_Description", StringComparison.CurrentCultureIgnoreCase))
                        model.FullDescription = attr.Values.FirstOrDefault()?.ValueRaw;
                    else if (attr.Name.Equals("DIMENSIONS", StringComparison.CurrentCultureIgnoreCase))
                        model.Dimensions = attr.Values.FirstOrDefault()?.ValueRaw;
                }

                var groups = await _specificationAttributeService.GetSpecificationAttributeGroupsAsync(0, int.MaxValue);
                var specifications = new List<ProductModel.CustomProductSpecificationModel>();
                foreach (var group in groups)
                {
                    if (group.Id == 4)
                    {
                        Attributes = await CustomfeedPrepareProductSpecificationAttributeModelAsync(product, group);
                        foreach (var attr in Attributes)
                        {
                            specifications.Add(new ProductModel.CustomProductSpecificationModel()
                            {
                                Name = attr.Name,
                                Value = attr.Values.FirstOrDefault()?.ValueRaw
                            });
                            if (attr.Name == "Froggle_Description")
                                model.FullDescription = attr.Values.FirstOrDefault()?.ValueRaw;
                            else if (attr.Name == "Main Category Id")
                            {
                                int.TryParse(attr.Values.FirstOrDefault()?.ValueRaw, out int mainCategoryId);
                                if (mainCategoryId != 0)
                                {
                                    var mainCategory = await _categoryService.GetCategoryByIdAsync(mainCategoryId);
                                    if (mainCategory != null)
                                    {
                                        model.MainCategory = new ProductModel.CategoryModel()
                                        {
                                            Id = mainCategory.Id,
                                            Name = mainCategory.Name,
                                            Sename = await _urlRecordService.GetSeNameAsync(mainCategory),
                                            Path = await this.CategoryPath(categorySeprator, mainCategory.Id, mainCategory.Name, mainCategory.ParentCategoryId, lstexcludedCategories)
                                        };
                                    }
                                }
                            }
                        }
                    }
                }

                model.Gtin = product.Gtin;
                model.Id = product.Id;
                model.ManufacturerPartNumber = product.ManufacturerPartNumber;
                model.MetaDescription = product.MetaDescription;
                model.MetaKeywords = product.MetaKeywords;
                model.MetaTitle = product.MetaTitle;
                model.Name = product.Name;
                IList<CustomPictureModel> allPictureModels;
                (_, allPictureModels) = await CustomfeedrepareProductDetailsPictureModelAsync(product, false);
                model.PictureModels = allPictureModels;
                var productPrice = await PrepareCustomProductPriceModelAsync(product);
                model.ProductPrice = new ProductModel.ProductPriceModel()
                {
                    MsrpValue = productPrice.MsrpValue,
                    OldPriceValue = productPrice.OldPriceValue,
                    PriceValue = productPrice.PriceValue
                };
                model.Specifications = specifications;
                int[] lastRelatedIds = (await _productService.GetRelatedProductsByProductId1Async(product.Id)).Select(r => r.ProductId2).ToArray();

                List<string> relatedProducts = new List<string>();
                foreach (var productid in lastRelatedIds)
                {
                    var variantId = await this.GetProductVariantId(productid);
                    if (variantId != 0)
                        relatedProducts.Add(productid + "-" + variantId + "--");
                }


                model.RelatedProducts = relatedProducts;
                model.SeName = await _urlRecordService.GetSeNameAsync(product);
                model.ShippingCost = product.ShippingPrice;
                model.ShortDescription = product.ShortDescription;
                model.Sku = await GetProductSku(product.Sku, product.Id);
                model.TotaLinventory = product.TotalInventory;



                #region Variant
                List<ProductModel.Variant> Variants = new List<ProductModel.Variant>();


                var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
                foreach (var attribute in productAttributeMapping)
                {
                    var productAttrubute = (await _productAttributeService.GetProductAttributeByIdAsync(attribute.ProductAttributeId));

                    if (attribute.ShouldHaveValues() && productAttrubute.Name.Equals(await _localizationService.GetResourceAsync("Product.Attr.Size"), StringComparison.InvariantCultureIgnoreCase))
                    {
                        List<Models.Api.CustomProductAttributeCombination> lstCombinations = new List<Models.Api.CustomProductAttributeCombination>();

                        var combinations = await _productAttributeService.GetAllProductAttributeCombinationsAsync(product.Id);

                        foreach (var combination in combinations)
                        {
                            var values = await _productAttributeParser
                        .ParseProductAttributeValuesAsync(combination.AttributesXml, attribute.Id);
                            if (values == null || values.Count == 0)
                                continue;

                            lstCombinations.Add(new Models.Api.CustomProductAttributeCombination()
                            {
                                Combination = combination,
                                ValueId = values.FirstOrDefault()?.Id ?? 0

                            });
                        }

                        var attributeValues = (await _productAttributeService.GetProductAttributeValuesAsync(attribute.Id)).OrderBy(o => o.DisplayOrder);
                        //values
                        foreach (var attrValue in attributeValues)
                        {
                            if (attrValue.VariantId > 0 && attrValue.Published)
                            {
                                ProductModel.Variant variant = new ProductModel.Variant();
                                variant.Name = attrValue.Name;
                                variant.Id = attrValue.VariantId;
                                variant.IsDefault = attrValue.IsPreSelected ? true : false;
                                variant.ManufacturerPartNumber = attrValue.ManufacturerPartNumber;
                                variant.Weight = attrValue.WeightAdjustment;
                                variant.VariantTitle = attrValue.VariantTitle;
                                variant.Dimension = attrValue.Dimension;

                                variant.QueryParameter = attrValue.QueryParameter;
                                var combination = lstCombinations.Where(c => c.ValueId == attrValue.Id).FirstOrDefault();
                                if (combination != null)
                                {
                                    variant.MsrpValue = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(combination.Combination.OverriddenMsrp ?? productPrice.MsrpValue, await _workContext.GetWorkingCurrencyAsync());
                                    variant.OldPriceValue = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(combination.Combination.OverriddenOldPrice ?? productPrice.OldPriceValue, await _workContext.GetWorkingCurrencyAsync());
                                    variant.PriceValue = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(combination.Combination.OverriddenPrice ?? productPrice.PriceValue, await _workContext.GetWorkingCurrencyAsync());
                                }
                                else
                                {
                                    variant.MsrpValue = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(productPrice.MsrpValue, await _workContext.GetWorkingCurrencyAsync());

                                    variant.OldPriceValue = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(productPrice.OldPriceValue, await _workContext.GetWorkingCurrencyAsync());

                                    variant.PriceValue = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(productPrice.PriceValue, await _workContext.GetWorkingCurrencyAsync());


                                }

                                Variants.Add(variant);

                            }
                        }
                        break;
                    }
                }
                model.Variants = Variants;
                #endregion

                #region Sale Info

                var discountInfo = await _storeWideDiscountService.GetProductSaleInfo(model.Id);
                if (discountInfo != null)
                {
                    model.SaleStartDate = discountInfo.StartDate;
                    model.SaleEndDate = discountInfo.EndDate;
                }

                #endregion

                lstModel.Add(model);
            }
            return lstModel;
        }
        public async Task<List<ProductModel>> PrepareProductFeedVersion2(int pageNumber, int pageSize)
        {
            var products = await _customProductService.ProductsFeedAsync(pageNumber - 1, pageSize);
            var categorySeprator = await _localizationService.GetResourceAsync("Product.Feed.Category.Seprator");
            var excludedCategories = await _settingService.GetSettingByKeyAsync<string>("Product.Feed.Category.Excluded");
            string sizeAttributeName = await _localizationService.GetResourceAsync("Product.Attr.Size");

            List<int> lstexcludedCategories = new List<int>();
            foreach (var category in excludedCategories.Split(','))
            {
                int.TryParse(category, out int categoryId);
                if (categoryId != 0)
                    lstexcludedCategories.Add(categoryId);
            }

            List<ProductModel> lstModel = new List<ProductModel>();
            foreach (var product in products)
            {

                ProductModel model = new ProductModel();

                var categories = await _categoryService.GetProductCategoriesByProductIdAsync(product.Id);
                List<ProductModel.CategoryModel> lstCategoriesModel = new List<ProductModel.CategoryModel>();
                foreach (var category in categories)
                {
                    ProductModel.CategoryModel categoryModel = new
                        ProductModel.CategoryModel();
                    var categoryObject = await _categoryService.GetCategoryByIdAsync(category.CategoryId);
                    if (categoryObject != null)
                    {

                        categoryModel.Id = categoryObject.Id;
                        categoryModel.Name = categoryObject.Name;
                        categoryModel.Sename = await _urlRecordService.GetSeNameAsync(categoryObject);
                        categoryModel.Path = await this.CategoryPath(categorySeprator, categoryObject.Id,
                            categoryObject.Name, categoryObject.ParentCategoryId, lstexcludedCategories);
                        lstCategoriesModel.Add(categoryModel);
                    }

                }

                #region Individual Products

                model.IndividualProducts = string.Join(',', await _customProductService.GetPairWithProductsByProductId1Async(product.Id));

                #endregion

                var prdManufacturers = await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id, true);
                if (prdManufacturers.Count > 0)
                {
                    var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(prdManufacturers.FirstOrDefault().ManufacturerId);
                    model.Manufacturer = manufacturer.Name;
                }

                model.Categories = lstCategoriesModel;

                var Attributes = await PrepareProductSpecificationAttributeModelAsync(product, null);
                foreach (var attr in Attributes)
                {
                    if (attr.Name.Equals("Froggle_Description", StringComparison.CurrentCultureIgnoreCase))
                        model.FroogleDescription = attr.Values.FirstOrDefault()?.ValueRaw;
                    else if (attr.Name.Equals("DIMENSIONS", StringComparison.CurrentCultureIgnoreCase))
                        model.Dimensions = attr.Values.FirstOrDefault()?.ValueRaw;
                }

                var groups = await _specificationAttributeService.GetSpecificationAttributeGroupsAsync(0, int.MaxValue);
                var specifications = new List<ProductModel.CustomProductSpecificationModel>();
                foreach (var group in groups)
                {
                    if (group.Id == 4)
                    {
                        Attributes = await CustomfeedPrepareProductSpecificationAttributeModelAsync(product, group);
                        foreach (var attr in Attributes)
                        {
                            specifications.Add(new ProductModel.CustomProductSpecificationModel()
                            {
                                Name = attr.Name,
                                Value = attr.Values.FirstOrDefault()?.ValueRaw
                            });
                            if (attr.Name == "Froggle_Description")
                                model.FroogleDescription = attr.Values.FirstOrDefault()?.ValueRaw;
                            else if (attr.Name == "Main Category Id")
                            {
                                int.TryParse(attr.Values.FirstOrDefault()?.ValueRaw, out int mainCategoryId);
                                if (mainCategoryId != 0)
                                {
                                    var mainCategory = await _categoryService.GetCategoryByIdAsync(mainCategoryId);
                                    if (mainCategory != null)
                                    {
                                        model.MainCategory = new ProductModel.CategoryModel()
                                        {
                                            Id = mainCategory.Id,
                                            Name = mainCategory.Name,
                                            Sename = await _urlRecordService.GetSeNameAsync(mainCategory),
                                            Path = await this.CategoryPath(categorySeprator, mainCategory.Id, mainCategory.Name, mainCategory.ParentCategoryId, lstexcludedCategories)
                                        };
                                    }
                                }
                            }
                        }
                    }
                }

                model.Gtin = product.Gtin;
                model.FullDescription = product.FullDescription;
                model.Id = product.Id;
                model.ManufacturerPartNumber = product.ManufacturerPartNumber;
                model.MetaDescription = product.MetaDescription;
                model.MetaKeywords = product.MetaKeywords;
                model.MetaTitle = product.MetaTitle;
                model.Name = product.Name;
                IList<CustomPictureModel> allPictureModels;
                (_, allPictureModels) = await CustomfeedrepareProductDetailsPictureModelAsync(product, false);
                model.PictureModels = allPictureModels;
                var productPrice = await PrepareCustomProductPriceModelAsync(product);
                model.ProductPrice = new ProductModel.ProductPriceModel()
                {
                    MsrpValue = productPrice.MsrpValue,
                    OldPriceValue = productPrice.OldPriceValue,
                    PriceValue = productPrice.PriceValue
                };
                model.Specifications = specifications;
                int[] lastRelatedIds = (await _productService.GetRelatedProductsByProductId1Async(product.Id)).Select(r => r.ProductId2).ToArray();

                List<string> relatedProducts = new List<string>();
                foreach (var productid in lastRelatedIds)
                {
                    var variantId = await this.GetProductVariantId(productid);
                    if (variantId != 0)
                        relatedProducts.Add(productid + "-" + variantId + "--");
                }


                model.RelatedProducts = relatedProducts;
                model.SeName = await _urlRecordService.GetSeNameAsync(product);
                model.ShippingCost = product.ShippingPrice;
                model.ShortDescription = product.ShortDescription;
                model.Sku = await GetProductSku(product.Sku, product.Id);
                model.TotaLinventory = product.TotalInventory;



                #region Variant
                var combinations = await _productAttributeService.GetAllProductAttributeCombinationsAsync(product.Id);
                var validFullCombinationSets = new List<HashSet<int>>();

                foreach (var combo in combinations)
                {
                    var comboValues = await _productAttributeParser.ParseProductAttributeValuesAsync(combo.AttributesXml);
                    // Only consider published combinations
                    if (!comboValues.Any(av => !av.Published))
                    {
                        validFullCombinationSets.Add(new HashSet<int>(comboValues.Select(v => v.Id)));
                    }
                }

                List<ProductModel.Variant> Variants = new List<ProductModel.Variant>();
                foreach (var _variant in await _customProductService.GetProductVariants(product.Id))
                {

                    if (product.EnableConditionalAttributes)
                    {
                        var variantValueIds = (_variant.ProductAttributeValueIds ?? string.Empty)
                      .Split('-', StringSplitOptions.RemoveEmptyEntries)
                      .Select(int.Parse)
                      .ToHashSet();


                        bool isValid = validFullCombinationSets.Any(comboSet => variantValueIds.IsSubsetOf(comboSet));

                        if (!isValid)
                            continue;
                    }
                    bool isDefault = true;
                    bool published = true;

                    string manufacturerPartNumber = string.Empty;
                    decimal defaultWeight = 0;
                    ProductAttribute sizeAttribute = null;
                    ProductAttributeValue sizeAttributeValue = null;
                    Dictionary<string, string> attributes = new Dictionary<string, string>();
                    foreach (var attrValueId in (_variant.ProductAttributeValueIds ?? string.Empty).Split('-'))
                    {
                        int.TryParse(attrValueId, out int _attrValueId);
                        var productAttributeValue = await _productAttributeService.GetProductAttributeValueByIdAsync(_attrValueId);
                        if (productAttributeValue != null)
                        {
                            var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(productAttributeValue.ProductAttributeMappingId);
                            if (productAttributeMapping != null)
                            {
                                var productAttribute = await _productAttributeService.GetProductAttributeByIdAsync(productAttributeMapping.ProductAttributeId);
                                if (productAttribute != null && productAttribute.Name.Equals(sizeAttributeName, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    sizeAttribute = productAttribute;
                                    sizeAttributeValue = productAttributeValue;
                                }
                                if (productAttribute != null && !attributes.Keys.Where(K => K == CustomCommonHelper.SanitizeToLower(productAttribute.Name)).Any()) { }
                                {
                                    attributes.Add(CustomCommonHelper.SanitizeToLower(productAttribute.Name), productAttributeValue.Name);
                                }
                            }
                        }
                        if (productAttributeValue == null || !productAttributeValue.Published)
                        {
                            published = false;
                        }
                        isDefault = !isDefault ? false : productAttributeValue?.IsPreSelected ?? false;

                    }

                    if (_variant.VariantId > 0 && published)
                    {
                        ProductModel.Variant variant = new ProductModel.Variant();
                        variant.Name = CustomCommonHelper.FirstOrEmpty(sizeAttributeValue?.Name, _variant.Title);
                        variant.Id = _variant.VariantId;
                        variant.IsDefault = isDefault;
                        variant.ManufacturerPartNumber = CustomCommonHelper.FirstOrEmpty(_variant?.ManufacturerPartNumber, sizeAttributeValue?.ManufacturerPartNumber ?? string.Empty);
                        variant.Weight = _variant.Weight == 0 ? sizeAttributeValue?.WeightAdjustment ?? 0 : _variant.Weight;
                        variant.VariantTitle = CustomCommonHelper.FirstOrEmpty(_variant?.Title, sizeAttributeValue?.VariantTitle ?? string.Empty);
                        variant.Dimension = CustomCommonHelper.FirstOrEmpty(_variant?.Dimension, sizeAttributeValue?.Dimension ?? String.Empty);
                        variant.QueryParameter = CustomCommonHelper.FirstOrEmpty(_variant?.QueryParameter, sizeAttributeValue?.QueryParameter ?? string.Empty);
                        variant.Attributes = attributes;
                        if ((_variant?.OldPrice ?? 0) == 0 && (_variant?.Price ?? 0) == 0)
                        {
                            variant.MsrpValue = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(productPrice.MsrpValue, await _workContext.GetWorkingCurrencyAsync());
                            variant.OldPriceValue = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(productPrice.OldPriceValue, await _workContext.GetWorkingCurrencyAsync());
                            variant.PriceValue = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(productPrice.PriceValue, await _workContext.GetWorkingCurrencyAsync());

                        }
                        else
                        {
                            variant.MsrpValue = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(_variant.Msrp ?? 0, await _workContext.GetWorkingCurrencyAsync());
                            variant.OldPriceValue = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(_variant.OldPrice ?? 0, await _workContext.GetWorkingCurrencyAsync());
                            variant.PriceValue = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(_variant.Price, await _workContext.GetWorkingCurrencyAsync());
                        }

                        Variants.Add(variant);
                    }
                }
                model.Variants = Variants;
                #endregion

                #region Sale Info

                var discountInfo = await _storeWideDiscountService.GetProductSaleInfo(model.Id);
                if (discountInfo != null)
                {
                    model.SaleStartDate = discountInfo.StartDate;
                    model.SaleEndDate = discountInfo.EndDate;
                }

                #endregion

                lstModel.Add(model);
            }
            return lstModel;
        }
        public virtual async Task<IList<ProductSpecificationAttributeModel>> PrepareCustomProductSpecificationAttributeModelAsync(Product product, SpecificationAttributeGroup group)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var productSpecificationAttributes = await _specificationAttributeService.GetProductSpecificationAttributesAsync(
                    product.Id, specificationAttributeGroupId: group?.Id, showOnProductPage: true);

            var result = new List<ProductSpecificationAttributeModel>();

            foreach (var psa in productSpecificationAttributes)
            {
                var option = await _specificationAttributeService.GetSpecificationAttributeOptionByIdAsync(psa.SpecificationAttributeOptionId);

                var model = result.FirstOrDefault(model => model.Id == option.SpecificationAttributeId);
                if (model == null)
                {
                    var attribute = await _specificationAttributeService.GetSpecificationAttributeByIdAsync(option.SpecificationAttributeId);
                    model = new ProductSpecificationAttributeModel
                    {
                        Id = attribute.Id,
                        Name = await _localizationService.GetLocalizedAsync(attribute, x => x.Name)
                    };
                    result.Add(model);
                }

                var value = new ProductSpecificationAttributeValueModel
                {
                    AttributeTypeId = psa.AttributeTypeId,
                    ColorSquaresRgb = option.ColorSquaresRgb,
                    ValueRaw = psa.AttributeType switch
                    {
                        SpecificationAttributeType.Option => WebUtility.HtmlEncode(await _localizationService.GetLocalizedAsync(option, x => x.Name)),
                        SpecificationAttributeType.CustomText => WebUtility.HtmlEncode(await _localizationService.GetLocalizedAsync(psa, x => x.CustomValue)),
                        SpecificationAttributeType.CustomHtmlText => await _localizationService.GetLocalizedAsync(psa, x => x.CustomValue),
                        SpecificationAttributeType.Hyperlink => $"<a href='{psa.CustomValue}' target='_blank'>{psa.CustomValue}</a>",
                        _ => null
                    }
                };

                model.Values.Add(value);
            }

            return result;
        }
        public virtual async Task<IList<ProductSpecificationAttributeModel>> CustomfeedPrepareProductSpecificationAttributeModelAsync(Product product, SpecificationAttributeGroup group)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var productSpecificationAttributes = await _specificationAttributeService.GetProductSpecificationAttributesAsync(
                    product.Id, specificationAttributeGroupId: group?.Id, showOnProductPage: false);

            var result = new List<ProductSpecificationAttributeModel>();

            foreach (var psa in productSpecificationAttributes)
            {
                var option = await _specificationAttributeService.GetSpecificationAttributeOptionByIdAsync(psa.SpecificationAttributeOptionId);

                var model = result.FirstOrDefault(model => model.Id == option.SpecificationAttributeId);
                if (model == null)
                {
                    var attribute = await _specificationAttributeService.GetSpecificationAttributeByIdAsync(option.SpecificationAttributeId);
                    model = new ProductSpecificationAttributeModel
                    {
                        Id = attribute.Id,
                        Name = await _localizationService.GetLocalizedAsync(attribute, x => x.Name)
                    };
                    result.Add(model);
                }

                var value = new ProductSpecificationAttributeValueModel
                {
                    AttributeTypeId = psa.AttributeTypeId,
                    ColorSquaresRgb = option.ColorSquaresRgb,
                    ValueRaw = psa.AttributeType switch
                    {
                        SpecificationAttributeType.Option => WebUtility.HtmlEncode(await _localizationService.GetLocalizedAsync(option, x => x.Name)),
                        SpecificationAttributeType.CustomText => WebUtility.HtmlEncode(await _localizationService.GetLocalizedAsync(psa, x => x.CustomValue)),
                        SpecificationAttributeType.CustomHtmlText => await _localizationService.GetLocalizedAsync(psa, x => x.CustomValue),
                        SpecificationAttributeType.Hyperlink => $"<a href='{psa.CustomValue}' target='_blank'>{psa.CustomValue}</a>",
                        _ => null
                    }
                };

                model.Values.Add(value);
            }

            return result;
        }
        #endregion

        #region Variant

        #endregion

        #region Utilities


        public async Task<List<ProductAttributeCombination>> GetValidProductAttributeCombinationsAsync(Product product)
        {

            return await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.ProductAttributeValidCombinationsByProductCacheKey, product.Id), async () =>
            {
                var currentCustomer = await _workContext.GetCurrentCustomerAsync();
                List<ProductAttributeCombination> validCombinations = new List<ProductAttributeCombination>();

                var productCombinations = await _productAttributeService.GetAllProductAttributeCombinationsAsync(product.Id);
                foreach (var combination in productCombinations)
                {
                    var warnings = (await _shoppingCartService.GetShoppingCartItemAttributeWarningsAsync(currentCustomer,
                      ShoppingCartType.ShoppingCart, product,
                      attributesXml: combination.AttributesXml,
                      ignoreNonCombinableAttributes: true)
                      );
                    if (!warnings.Any())
                        validCombinations.Add(combination);
                }
                return validCombinations;
            });
        }
        public async Task<string> ReplaceTokens(string content, string dimensionImage, string fullDimensionImage, string sizeImage, string fullSizeUrl, bool isLargeItem, string title)
        {
            if (string.IsNullOrEmpty(content))
                return content;
            else
            {
                string pattern = @"\[%(?'shortcode'[^\]]+)\%]";
                MatchCollection matches = Regex.Matches(content, pattern);
                foreach (Match match in matches)
                {
                    string shortcode = "[%" + match.Groups["shortcode"].Value + "%]";
                    if (match.Groups["shortcode"].Value.ToLower().Contains("dimension_image") && !string.IsNullOrEmpty(dimensionImage))
                    {
                        content = content.Replace(shortcode, $"<a href=\"{fullDimensionImage}\" class=\"default-dimension-zoom\"><img width=\"400\" height=\"400\" src=\"/images/pixel.png\" data-src=\"{dimensionImage}\" style=\"max-width:415px;\" class=\"lazy default-dimension-image\" alt=\"{title} Dimension Image\"/></a>");
                    }
                    else if (match.Groups["shortcode"].Value.ToLower().Contains("size_image") && !string.IsNullOrEmpty(sizeImage))
                    {
                        content = content.Replace(shortcode, $"<a href=\"{fullSizeUrl}\" class=\"dimension-zoom\"><img width=\"400\" height=\"400\" src=\"/images/pixel.png\" data-src=\"{sizeImage}\" style=\"max-width:415px;\" class=\"lazy dimension-image\" alt=\"{title} Size Image\"/></a>");
                    }
                    else if (match.Groups["shortcode"].Value.ToLower().Contains("note") && isLargeItem)
                    {
                        content = content.Replace(shortcode, await _localizationService.GetResourceAsync("Product.Dimension.Note"));
                    }
                    else
                    {
                        content = content.Replace(shortcode, "");
                    }
                }
            }
            return content;
        }
        private async Task<string> GetProductEstimatedDeliveryDate(int productId, int variantId, string estimatedDeliveryDate)
        {
            if (variantId != 0)
            {
                estimatedDeliveryDate = (await _customProductService.GetProductVariants(productId)).Where(v => v.VariantId == variantId).FirstOrDefault()?.EstimatedDeliveryDate ?? estimatedDeliveryDate;
            }
            return estimatedDeliveryDate;
        }
        protected virtual async Task<IList<ProductTag>> GetFilteredProductTagsAsync(Product product)
        {


            var store = await _storeContext.GetCurrentStoreAsync();
            var productsTags = await _productTagService.GetAllProductTagsByProductIdAsync(product.Id);

            var storeTags = await productsTags
                .WhereAwait(async x => await _productTagService.GetProductCountByProductTagIdAsync(x.Id, store.Id) > 0)
                .ToListAsync();

            var otherTags = storeTags
                .Where(t => !t.Name.Equals(_tagAutomationSettings.NewArrivalTagName, StringComparison.OrdinalIgnoreCase)
                         && !t.Name.Equals(_tagAutomationSettings.BestsellerTagName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (otherTags.Any())
                return otherTags;

            bool hasBestSeller = storeTags.Any(t => t.Name.Equals(_tagAutomationSettings.BestsellerTagName, StringComparison.OrdinalIgnoreCase));

            if (hasBestSeller)
                return storeTags.Where(t => t.Name.Equals(_tagAutomationSettings.BestsellerTagName, StringComparison.OrdinalIgnoreCase)).ToList();

            return storeTags;


        }



        protected virtual async Task<IList<ProductTagModel>> CustomPrepareProductTagModelsAsync(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var store = await _storeContext.GetCurrentStoreAsync();

            // Get filtered domain tags from Function 1
            var filteredTags = await GetFilteredProductTagsAsync(product);

            // Map domain entities to models
            return await filteredTags
                   .SelectAwait(async x => new ProductTagModel
                   {
                       Id = x.Id,
                       Name = await _localizationService.GetLocalizedAsync(x, y => y.Name),
                       SeName = await _urlRecordService.GetSeNameAsync(x),
                       ProductCount = await _productTagService.GetProductCountByProductTagIdAsync(x.Id, store.Id)
                   }).ToListAsync();


        }

        protected virtual async Task<ProductDetailsModel.ProductBreadcrumbModel> PrepareCustomProductBreadcrumbModelAsync(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var breadcrumbModel = new ProductDetailsModel.ProductBreadcrumbModel
            {
                Enabled = _catalogSettings.CategoryBreadcrumbEnabled,
                ProductId = product.Id,
                ProductName = await _localizationService.GetLocalizedAsync(product, x => x.Name),
                ProductSeName = await _urlRecordService.GetSeNameAsync(product)
            };
            var productCategories = await _categoryService.GetProductCategoriesByProductIdAsync(product.Id);
            if (!productCategories.Any())
                return breadcrumbModel;

            int categoryId = -1;
            var _commonModelFactory = EngineContext.Current.Resolve<ICommonModelFactory>();
            Category category = new Category();
            (string entityType, int entityId) = await _commonService.GetRefererDetails();

            try
            {
                if (string.Equals(entityType, "category", StringComparison.InvariantCultureIgnoreCase)
                    && entityId > 0)
                {
                    if (productCategories.Where(pc => pc.CategoryId == entityId).Any())
                    {
                        categoryId = productCategories.Where(pc => pc.CategoryId == entityId).First().CategoryId;
                    }
                    else
                    {
                        var childCategories = await _categoryService.GetAllCategoriesByParentCategoryIdAsync(entityId);
                        if (productCategories.Where(pc => childCategories.Select(c => c.Id).Contains(pc.CategoryId)).Any())
                        {
                            categoryId = productCategories.Where(pc => childCategories.Select(c => c.Id).Contains(pc.CategoryId)).First().CategoryId;
                        }
                    }
                }
                if (categoryId == -1)
                {
                    int maincategoryId = await _customSpecificationAttributeService.GetMainCategoryOfProduct(product.Id);
                    if (maincategoryId != 0 && productCategories.Where(pc => pc.CategoryId == maincategoryId).Any())
                    {
                        categoryId = maincategoryId;
                    }
                    else
                    {
                        var childCategories = await _categoryService.GetAllCategoriesByParentCategoryIdAsync(maincategoryId);
                        if (productCategories.Where(pc => childCategories.Select(c => c.Id).Contains(pc.CategoryId)).Any())
                        {
                            categoryId = productCategories.Where(pc => childCategories.Select(c => c.Id).Contains(pc.CategoryId)).First().CategoryId;
                        }
                    }
                }
            }
            catch (Exception exp)
            {

            }

            category = await _categoryService.GetCategoryByIdAsync(categoryId == -1 ? productCategories[0].CategoryId : categoryId);
            if (category == null)
                return breadcrumbModel;

            foreach (var catBr in await _categoryService.GetCategoryBreadCrumbAsync(category))
            {
                breadcrumbModel.CategoryBreadcrumb.Add(new CategorySimpleModel
                {
                    Id = catBr.Id,
                    Name = await _localizationService.GetLocalizedAsync(catBr, x => x.Name),
                    SeName = await _urlRecordService.GetSeNameAsync(catBr)
                });
            }

            return breadcrumbModel;
        }
        protected virtual async Task<IList<CustomProductDetailsModel.ProductAttributeModel>> PrepareCustomProductAttributeModelsAsync(Product product, ShoppingCartItem updatecartitem)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var model = new List<CustomProductDetailsModel.ProductAttributeModel>();
            var store = updatecartitem != null ? await _storeService.GetStoreByIdAsync(updatecartitem.StoreId) : await _storeContext.GetCurrentStoreAsync();
            var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
            foreach (var attribute in productAttributeMapping)
            {
                var productAttrubute = await _productAttributeService.GetProductAttributeByIdAsync(attribute.ProductAttributeId);

                var attributeModel = new CustomProductDetailsModel.ProductAttributeModel
                {
                    Id = attribute.Id,
                    ProductId = product.Id,
                    ProductAttributeId = attribute.ProductAttributeId,
                    Name = await _localizationService.GetLocalizedAsync(productAttrubute, x => x.Name),
                    Description = await _localizationService.GetLocalizedAsync(productAttrubute, x => x.Description),
                    TextPrompt = await _localizationService.GetLocalizedAsync(attribute, x => x.TextPrompt),
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType,
                    DefaultValue = updatecartitem != null ? null : await _localizationService.GetLocalizedAsync(attribute, x => x.DefaultValue),
                    HasCondition = !string.IsNullOrEmpty(attribute.ConditionAttributeXml)
                };
                if (!string.IsNullOrEmpty(attribute.ValidationFileAllowedExtensions))
                {
                    attributeModel.AllowedFileExtensions = attribute.ValidationFileAllowedExtensions
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                }

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var attributeValues = await _productAttributeService.GetProductAttributeValuesAsync(attribute.Id);
                    attributeValues = attributeValues.Where(a => a.Published == true).ToList();
                    foreach (var attributeValue in attributeValues)
                    {
                        var valueModel = new CustomProductDetailsModel.ProductAttributeValueModel
                        {
                            Id = attributeValue.Id,
                            Name = await _localizationService.GetLocalizedAsync(attributeValue, x => x.Name),
                            ColorSquaresRgb = attributeValue.ColorSquaresRgb, //used with "Color squares" attribute type
                            IsPreSelected = attributeValue.IsPreSelected,
                            CustomerEntersQty = attributeValue.CustomerEntersQty,
                            Quantity = attributeValue.Quantity
                        };
                        attributeModel.Values.Add(valueModel);

                        //display price if allowed
                        if (await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.DISPLAY_PRICES))
                        {
                            var customer = updatecartitem?.CustomerId is null ? await _workContext.GetCurrentCustomerAsync() : await _customerService.GetCustomerByIdAsync(updatecartitem.CustomerId);

                            var attributeValuePriceAdjustment = await _priceCalculationService.GetProductAttributeValuePriceAdjustmentAsync(product, attributeValue, customer, store, quantity: updatecartitem?.Quantity ?? 1);
                            var (priceAdjustmentBase, _) = await _taxService.GetProductPriceAsync(product, attributeValuePriceAdjustment);
                            var priceAdjustment = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(priceAdjustmentBase, await _workContext.GetWorkingCurrencyAsync());

                            if (attributeValue.PriceAdjustmentUsePercentage)
                            {
                                var priceAdjustmentStr = attributeValue.PriceAdjustment.ToString("G29");
                                if (attributeValue.PriceAdjustment > decimal.Zero)
                                    valueModel.PriceAdjustment = "+";
                                valueModel.PriceAdjustment += priceAdjustmentStr + "%";
                            }
                            else
                            {
                                if (priceAdjustmentBase > decimal.Zero)
                                    valueModel.PriceAdjustment = "+" + await _priceFormatter.FormatPriceAsync(priceAdjustment, false, false);
                                else if (priceAdjustmentBase < decimal.Zero)
                                    valueModel.PriceAdjustment = "-" + await _priceFormatter.FormatPriceAsync(-priceAdjustment, false, false);
                            }

                            valueModel.PriceAdjustmentValue = priceAdjustment;
                        }

                        //"image square" picture (with with "image squares" attribute type only)
                        if (attributeValue.ImageSquaresPictureId > 0)
                        {
                            var productAttributeImageSquarePictureCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.ProductAttributeImageSquarePictureModelKey
                                , attributeValue.ImageSquaresPictureId,
                                    _webHelper.IsCurrentConnectionSecured(),
                                    await _storeContext.GetCurrentStoreAsync());
                            valueModel.ImageSquaresPictureModel = await _staticCacheManager.GetAsync(productAttributeImageSquarePictureCacheKey, async () =>
                            {
                                var imageSquaresPicture = await _pictureService.GetPictureByIdAsync(attributeValue.ImageSquaresPictureId);
                                string fullSizeImageUrl, imageUrl;
                                (imageUrl, imageSquaresPicture) = await _pictureService.GetPictureUrlAsync(imageSquaresPicture, _mediaSettings.ImageSquarePictureSize);
                                (fullSizeImageUrl, imageSquaresPicture) = await _pictureService.GetPictureUrlAsync(imageSquaresPicture);

                                if (imageSquaresPicture != null)
                                {
                                    return new PictureModel
                                    {
                                        FullSizeImageUrl = fullSizeImageUrl,
                                        ImageUrl = imageUrl
                                    };
                                }

                                return new PictureModel();
                            });
                        }

                        //picture of a product attribute value
                        valueModel.PictureId = (await _productAttributeService.GetProductAttributeValuePicturesAsync(attributeValue.Id)).FirstOrDefault()?.PictureId ?? 0;
                    }
                }

                //set already selected attributes (if we're going to update the existing shopping cart item)
                if (updatecartitem != null)
                {
                    switch (attribute.AttributeControlType)
                    {
                        case AttributeControlType.DropdownList:
                        case AttributeControlType.RadioList:
                        case AttributeControlType.Checkboxes:
                        case AttributeControlType.ColorSquares:
                        case AttributeControlType.ImageSquares:
                            {
                                if (!string.IsNullOrEmpty(updatecartitem.AttributesXml))
                                {
                                    //clear default selection
                                    foreach (var item in attributeModel.Values)
                                        item.IsPreSelected = false;

                                    //select new values
                                    var selectedValues = await _productAttributeParser.ParseProductAttributeValuesAsync(updatecartitem.AttributesXml);
                                    foreach (var attributeValue in selectedValues)
                                        foreach (var item in attributeModel.Values)
                                            if (attributeValue.Id == item.Id)
                                            {
                                                item.IsPreSelected = true;

                                                //set customer entered quantity
                                                if (attributeValue.CustomerEntersQty)
                                                    item.Quantity = attributeValue.Quantity;
                                            }
                                }
                            }

                            break;
                        case AttributeControlType.ReadonlyCheckboxes:
                            {
                                //values are already pre-set

                                //set customer entered quantity
                                if (!string.IsNullOrEmpty(updatecartitem.AttributesXml))
                                {
                                    foreach (var attributeValue in (await _productAttributeParser.ParseProductAttributeValuesAsync(updatecartitem.AttributesXml))
                                        .Where(value => value.CustomerEntersQty))
                                    {
                                        var item = attributeModel.Values.FirstOrDefault(value => value.Id == attributeValue.Id);
                                        if (item != null)
                                            item.Quantity = attributeValue.Quantity;
                                    }
                                }
                            }

                            break;
                        case AttributeControlType.TextBox:
                        case AttributeControlType.MultilineTextbox:
                            {
                                if (!string.IsNullOrEmpty(updatecartitem.AttributesXml))
                                {
                                    var enteredText = _productAttributeParser.ParseValues(updatecartitem.AttributesXml, attribute.Id);
                                    if (enteredText.Any())
                                        attributeModel.DefaultValue = enteredText[0];
                                }
                            }

                            break;
                        case AttributeControlType.Datepicker:
                            {
                                //keep in mind my that the code below works only in the current culture
                                var selectedDateStr = _productAttributeParser.ParseValues(updatecartitem.AttributesXml, attribute.Id);
                                if (selectedDateStr.Any())
                                {
                                    if (DateTime.TryParseExact(selectedDateStr[0], "D", CultureInfo.CurrentCulture, DateTimeStyles.None, out var selectedDate))
                                    {
                                        //successfully parsed
                                        attributeModel.SelectedDay = selectedDate.Day;
                                        attributeModel.SelectedMonth = selectedDate.Month;
                                        attributeModel.SelectedYear = selectedDate.Year;
                                    }
                                }
                            }

                            break;
                        case AttributeControlType.FileUpload:
                            {
                                if (!string.IsNullOrEmpty(updatecartitem.AttributesXml))
                                {
                                    var downloadGuidStr = _productAttributeParser.ParseValues(updatecartitem.AttributesXml, attribute.Id).FirstOrDefault();
                                    Guid.TryParse(downloadGuidStr, out var downloadGuid);
                                    var download = await _downloadService.GetDownloadByGuidAsync(downloadGuid);
                                    if (download != null)
                                        attributeModel.DefaultValue = download.DownloadGuid.ToString();
                                }
                            }

                            break;
                        default:
                            break;
                    }
                }
                if (attributeModel.Values.Count > 0)
                {
                    model.Add(attributeModel);
                }
            }

            return model;
        }
        private async Task<int> GetProductVariantId(int productId)
        {
            var variantId = 0;
            try
            {
                var isVariantExist = false;
                var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(productId);
                foreach (var attribute in productAttributeMapping)
                {
                    var productAttrubute = await _productAttributeService.GetProductAttributeByIdAsync(attribute.ProductAttributeId);

                    if (attribute.ShouldHaveValues())
                    {
                        //values
                        var attributeValues = await _productAttributeService.GetProductAttributeValuesAsync(attribute.Id);
                        if (productAttrubute.Name.Equals(await _localizationService.GetResourceAsync("Product.Attr.Size"), StringComparison.InvariantCultureIgnoreCase))
                        {
                            variantId = attributeValues.Where(v => v.IsPreSelected).FirstOrDefault()?.VariantId ?? 0;
                            if (variantId == 0)
                                variantId = attributeValues.FirstOrDefault()?.VariantId ?? 0;
                        }
                    }
                }
            }
            catch (Exception exp)
            {

            }
            return variantId;
        }


        private async Task<string> CategoryPath(string seprator, int categodyId, string categoryName, int parentCategoryId, List<int> excludedCategories)
        {
            string path = "";
            if (excludedCategories.Contains(categodyId))
            {
                path = categoryName;
                while (parentCategoryId != 0)
                {
                    var category = await _categoryService.GetCategoryByIdAsync(parentCategoryId);
                    if (category != null)
                    {
                        path = category.Name + seprator + path;
                        parentCategoryId = category.ParentCategoryId;
                    }
                    else
                        parentCategoryId = 0;
                }
            }
            return path;
        }

        protected async Task<(bool isVariantExist, decimal msrp, decimal oldPrice, decimal price)> GetVariantPrice(Product product, int variantId)
        {
            bool isVariantExist = false;
            decimal msrp = 0;
            decimal oldPrice = 0;
            decimal price = 0;

            var variant = (await this._customProductService.GetProductVariants(product.Id)).Where(v => v.VariantId == variantId).FirstOrDefault();
            if (variant != null)
            {
                msrp = variant.Msrp ?? product.Msrp;
                oldPrice = variant.OldPrice ?? product.OldPrice;
                price = Convert.ToDecimal(variant.Price == 0 ? product.Price : variant.Price);
                isVariantExist = true;
            }
            return (isVariantExist, msrp, oldPrice, price);
        }


        protected virtual async Task<(string, IList<CustomProductDetailsModel.ProductAttributeModel>, int pictureId, List<List<int>> allCombinations)>
            CustomPrepareProductAttributeWithVariantTitleModelsAsync(Product product, ShoppingCartItem updatecartitem, int variantId = 0)
        {

            var productAttributeCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.ProductAttributeModelKey
    , product.Id, variantId, updatecartitem?.Id ?? 0, _webHelper.IsCurrentConnectionSecured(), await _storeContext.GetCurrentStoreAsync());
            var store = updatecartitem != null ? await _storeService.GetStoreByIdAsync(updatecartitem.StoreId) : await _storeContext.GetCurrentStoreAsync();
            return await _staticCacheManager.GetAsync(productAttributeCacheKey, async () =>
            {
                string variantTitle = "";
                var variantcombinations = await _customProductService.GetProductVariants(product.Id);
                List<int> variantAttrs = new List<int>();
                if (variantId != 0)
                {
                    var combination = variantcombinations.FirstOrDefault(x => x.VariantId == variantId);
                    if (combination != null)
                    {
                        variantTitle = combination.Title;
                        variantAttrs = (combination.ProductAttributeValueIds ?? string.Empty)
                                             .Split("-")
                                             .Select(id => int.Parse(id))
                                             .ToList();
                    }
                }


                string variantSize = "";
                int pictureId = 0;
                if (product == null)
                    throw new ArgumentNullException(nameof(product));

                var model = new List<CustomProductDetailsModel.ProductAttributeModel>();

                var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
                foreach (var attribute in productAttributeMapping)
                {
                    var productAttrubute = await _productAttributeService.GetProductAttributeByIdAsync(attribute.ProductAttributeId);

                    var attributeModel = new CustomProductDetailsModel.ProductAttributeModel
                    {
                        Id = attribute.Id,
                        ProductId = product.Id,
                        ProductAttributeId = attribute.ProductAttributeId,
                        Name = await _localizationService.GetLocalizedAsync(productAttrubute, x => x.Name),
                        Description = await _localizationService.GetLocalizedAsync(productAttrubute, x => x.Description),
                        TextPrompt = await _localizationService.GetLocalizedAsync(attribute, x => x.TextPrompt),
                        IsRequired = attribute.IsRequired,
                        AttributeControlType = attribute.AttributeControlType,
                        DefaultValue = updatecartitem != null ? null : await _localizationService.GetLocalizedAsync(attribute, x => x.DefaultValue),
                        HasCondition = !string.IsNullOrEmpty(attribute.ConditionAttributeXml),
                        IsExpanded = attribute.IsExpanded,
                        EnableHoverImpact = attribute.EnableHoverImpact
                    };
                    if (!string.IsNullOrEmpty(attribute.ValidationFileAllowedExtensions))
                    {
                        attributeModel.AllowedFileExtensions = attribute.ValidationFileAllowedExtensions
                            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .ToList();
                    }

                    if (attribute.ShouldHaveValues())
                    {
                        //values
                        var _attributeValues = await _productAttributeService.GetProductAttributeValuesAsync(attribute.Id);
                        _attributeValues = _attributeValues.Where(av => av.Published).ToList();
                        List<ProductAttributeValue> attributeValues = new List<ProductAttributeValue>();
                        foreach (var attributeValue in _attributeValues)
                        {
                            attributeValues.Add(new ProductAttributeValue()
                            {
                                AssociatedProductId = attributeValue.AssociatedProductId,
                                AttributeValueType = attributeValue.AttributeValueType,
                                AttributeValueTypeId = attributeValue.AttributeValueTypeId,
                                ColorSquaresRgb = attributeValue.ColorSquaresRgb,
                                Cost = attributeValue.Cost,
                                CustomerEntersQty = attributeValue.CustomerEntersQty,
                                Dimension = attributeValue.Dimension,
                                DisplayOrder = attributeValue.DisplayOrder,
                                Id = attributeValue.Id,
                                ImageSquaresPictureId = attributeValue.ImageSquaresPictureId,
                                IsPreSelected = attributeValue.IsPreSelected,
                                ManufacturerPartNumber = attributeValue.ManufacturerPartNumber,
                                Name = attributeValue.Name,
                                FeaturedPictureId = attributeValue.FeaturedPictureId,
                                PriceAdjustment = attributeValue.PriceAdjustment,
                                PriceAdjustmentUsePercentage = attributeValue.PriceAdjustmentUsePercentage,
                                ProductAttributeMappingId = attributeValue.ProductAttributeMappingId,
                                Published = attributeValue.Published,
                                Quantity = attributeValue.Quantity,
                                QueryParameter = attributeValue.QueryParameter,
                                VariantId = attributeValue.VariantId,
                                VariantTitle = attributeValue.VariantTitle,
                                WeightAdjustment = attributeValue.WeightAdjustment,
                                VariantDimension = attributeValue.VariantDimension


                            });
                        }

                        if (variantId != 0)
                        {
                            if (!productAttrubute.Name.Equals(await _localizationService.GetResourceAsync("Product.Attr.Stain"), StringComparison.InvariantCultureIgnoreCase))
                            {
                                foreach (var attributeValue in attributeValues)
                                {
                                    if (variantAttrs.Contains(attributeValue.Id))
                                    {
                                        if (productAttrubute.Name.Equals(await _localizationService.GetResourceAsync("Product.Attr.Size"), StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            variantSize = attributeValue.Name;
                                            pictureId = (await _productAttributeService.GetProductAttributeValuePicturesAsync(attributeValue.Id)).FirstOrDefault()?.PictureId ?? 0;
                                        }
                                        attributeValue.IsPreSelected = true;
                                    }
                                    else
                                        attributeValue.IsPreSelected = false;
                                }
                            }

                        }
                        else if (productAttrubute.Name.Equals(await _localizationService.GetResourceAsync("Product.Attr.Size"), StringComparison.InvariantCultureIgnoreCase))
                        {
                            variantTitle = attributeValues.Where(a => a.IsPreSelected).FirstOrDefault()?.VariantTitle;
                            // pictureId = attributeValues.Where(a => a.IsPreSelected).FirstOrDefault()?.PictureId ?? 0;
                            variantSize = attributeValues.Where(a => a.IsPreSelected).FirstOrDefault()?.Name ?? "";
                        }

                        string pictureDefaultSizeUrl = string.Empty;
                        string pictureFullSizeUrl = string.Empty;
                        foreach (var attributeValue in attributeValues)
                        {
                            pictureDefaultSizeUrl = string.Empty;
                            pictureFullSizeUrl = string.Empty;
                            int attrValuePictureId = (await _productAttributeService.GetProductAttributeValuePicturesAsync(attributeValue.Id)).FirstOrDefault()?.PictureId ?? 0;

                            if (attrValuePictureId > 0)
                            {
                                var productAttributePictureCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.ProductAttributePictureModelKey,
                                    attrValuePictureId, _webHelper.IsCurrentConnectionSecured(), await _storeContext.GetCurrentStoreAsync());
                                var pictureModel = await _staticCacheManager.GetAsync(productAttributePictureCacheKey, async () =>
                                {
                                    var picture = await _pictureService.GetPictureByIdAsync(attrValuePictureId);
                                    string fullSizeImageUrl, imageUrl, thumbImageUrl;

                                    (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                                    (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.ProductDetailsPictureSize);


                                    (thumbImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage);

                                    return picture == null ? new PictureModel() : new PictureModel
                                    {
                                        FullSizeImageUrl = fullSizeImageUrl,
                                        ImageUrl = imageUrl,
                                        ThumbImageUrl = thumbImageUrl
                                    };
                                });

                                pictureDefaultSizeUrl = pictureModel.ImageUrl;
                                pictureFullSizeUrl = pictureModel.FullSizeImageUrl;

                            }


                            var valueModel = new CustomProductDetailsModel.ProductAttributeValueModel
                            {
                                Id = attributeValue.Id,
                                Name = await _localizationService.GetLocalizedAsync(attributeValue, x => x.Name),
                                ColorSquaresRgb = attributeValue.ColorSquaresRgb, //used with "Color squares" attribute type
                                IsPreSelected = attributeValue.IsPreSelected,
                                CustomerEntersQty = attributeValue.CustomerEntersQty,
                                Quantity = attributeValue.Quantity,
                                VariantId = attributeValue.VariantId,
                                Dimension = attributeValue.Dimension,
                                VariantDimension = attributeValue.VariantDimension,
                                PictureDefaultSizeUrl = pictureDefaultSizeUrl,
                                PictureFullSizeUrl = pictureFullSizeUrl,
                                IsLargeItem = productAttrubute.Name.Equals(await _localizationService.GetResourceAsync("Product.Attr.Size")) ?
                                variantcombinations.Where(v => v.VariantId == attributeValue.VariantId).FirstOrDefault()?.WgsRequired ?? false
                                : false

                            };
                            attributeModel.Values.Add(valueModel);

                            //display price if allowed
                            if (await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.DISPLAY_PRICES))
                            {
                                var customer = updatecartitem?.CustomerId is null ? await _workContext.GetCurrentCustomerAsync() : await _customerService.GetCustomerByIdAsync(updatecartitem.CustomerId);

                                var attributeValuePriceAdjustment = await _priceCalculationService.GetProductAttributeValuePriceAdjustmentAsync(product, attributeValue, customer, store, quantity: updatecartitem?.Quantity ?? 1);
                                var (priceAdjustmentBase, _) = await _taxService.GetProductPriceAsync(product, attributeValuePriceAdjustment);
                                var priceAdjustment = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(priceAdjustmentBase, await _workContext.GetWorkingCurrencyAsync());

                                if (attributeValue.PriceAdjustmentUsePercentage)
                                {
                                    var priceAdjustmentStr = attributeValue.PriceAdjustment.ToString("G29");
                                    if (attributeValue.PriceAdjustment > decimal.Zero)
                                        valueModel.PriceAdjustment = "+";
                                    valueModel.PriceAdjustment += priceAdjustmentStr + "%";
                                }
                                else
                                {
                                    if (priceAdjustmentBase > decimal.Zero)
                                        valueModel.PriceAdjustment = "+" + await _priceFormatter.FormatPriceAsync(priceAdjustment, false, false);
                                    else if (priceAdjustmentBase < decimal.Zero)
                                        valueModel.PriceAdjustment = "-" + await _priceFormatter.FormatPriceAsync(-priceAdjustment, false, false);
                                }

                                valueModel.PriceAdjustmentValue = priceAdjustment;
                            }

                            //"image square" picture (with with "image squares" attribute type only)
                            if (attributeValue.ImageSquaresPictureId > 0)
                            {
                                var productAttributeImageSquarePictureCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.ProductAttributeImageSquarePictureModelKey
                                    , attributeValue.ImageSquaresPictureId,
                                        _webHelper.IsCurrentConnectionSecured(),
                                        await _storeContext.GetCurrentStoreAsync());
                                valueModel.ImageSquaresPictureModel = await _staticCacheManager.GetAsync(productAttributeImageSquarePictureCacheKey, async () =>
                                {
                                    var imageSquaresPicture = await _pictureService.GetPictureByIdAsync(attributeValue.ImageSquaresPictureId);
                                    string fullSizeImageUrl, imageUrl;
                                    (imageUrl, imageSquaresPicture) = await _pictureService.GetPictureUrlAsync(imageSquaresPicture, _mediaSettings.ImageSquarePictureSize);
                                    (fullSizeImageUrl, imageSquaresPicture) = await _pictureService.GetPictureUrlAsync(imageSquaresPicture);

                                    if (imageSquaresPicture != null)
                                    {
                                        return new PictureModel
                                        {
                                            FullSizeImageUrl = fullSizeImageUrl,
                                            ImageUrl = imageUrl
                                        };
                                    }

                                    return new PictureModel();
                                });
                            }

                            //picture of a product attribute value
                            valueModel.PictureId = attrValuePictureId;
                        }
                    }

                    //set already selected attributes (if we're going to update the existing shopping cart item)
                    if (updatecartitem != null)
                    {
                        switch (attribute.AttributeControlType)
                        {
                            case AttributeControlType.DropdownList:
                            case AttributeControlType.RadioList:
                            case AttributeControlType.Checkboxes:
                            case AttributeControlType.ColorSquares:
                            case AttributeControlType.ImageSquares:
                                {
                                    if (!string.IsNullOrEmpty(updatecartitem.AttributesXml))
                                    {
                                        //clear default selection
                                        foreach (var item in attributeModel.Values)
                                            item.IsPreSelected = false;

                                        //select new values
                                        var selectedValues = await _productAttributeParser.ParseProductAttributeValuesAsync(updatecartitem.AttributesXml);
                                        foreach (var attributeValue in selectedValues)
                                            foreach (var item in attributeModel.Values)
                                                if (attributeValue.Id == item.Id)
                                                {
                                                    item.IsPreSelected = true;

                                                    //set customer entered quantity
                                                    if (attributeValue.CustomerEntersQty)
                                                        item.Quantity = attributeValue.Quantity;
                                                }
                                    }
                                }

                                break;
                            case AttributeControlType.ReadonlyCheckboxes:
                                {
                                    //values are already pre-set

                                    //set customer entered quantity
                                    if (!string.IsNullOrEmpty(updatecartitem.AttributesXml))
                                    {
                                        foreach (var attributeValue in (await _productAttributeParser.ParseProductAttributeValuesAsync(updatecartitem.AttributesXml))
                                            .Where(value => value.CustomerEntersQty))
                                        {
                                            var item = attributeModel.Values.FirstOrDefault(value => value.Id == attributeValue.Id);
                                            if (item != null)
                                                item.Quantity = attributeValue.Quantity;
                                        }
                                    }
                                }

                                break;
                            case AttributeControlType.TextBox:
                            case AttributeControlType.MultilineTextbox:
                                {
                                    if (!string.IsNullOrEmpty(updatecartitem.AttributesXml))
                                    {
                                        var enteredText = _productAttributeParser.ParseValues(updatecartitem.AttributesXml, attribute.Id);
                                        if (enteredText.Any())
                                            attributeModel.DefaultValue = enteredText[0];
                                    }
                                }

                                break;
                            case AttributeControlType.Datepicker:
                                {
                                    //keep in mind my that the code below works only in the current culture
                                    var selectedDateStr = _productAttributeParser.ParseValues(updatecartitem.AttributesXml, attribute.Id);
                                    if (selectedDateStr.Any())
                                    {
                                        if (DateTime.TryParseExact(selectedDateStr[0], "D", CultureInfo.CurrentCulture, DateTimeStyles.None, out var selectedDate))
                                        {
                                            //successfully parsed
                                            attributeModel.SelectedDay = selectedDate.Day;
                                            attributeModel.SelectedMonth = selectedDate.Month;
                                            attributeModel.SelectedYear = selectedDate.Year;
                                        }
                                    }
                                }

                                break;
                            case AttributeControlType.FileUpload:
                                {
                                    if (!string.IsNullOrEmpty(updatecartitem.AttributesXml))
                                    {
                                        var downloadGuidStr = _productAttributeParser.ParseValues(updatecartitem.AttributesXml, attribute.Id).FirstOrDefault();
                                        Guid.TryParse(downloadGuidStr, out var downloadGuid);
                                        var download = await _downloadService.GetDownloadByGuidAsync(downloadGuid);
                                        if (download != null)
                                            attributeModel.DefaultValue = download.DownloadGuid.ToString();
                                    }
                                }

                                break;
                            default:
                                break;
                        }
                    }
                    if (attributeModel.Values.Count > 0)
                    {

                        model.Add(attributeModel);
                    }
                }
                //if (pictureId != 0)
                //{
                //    var _settingService = EngineContext.Current.Resolve<ISettingService>();
                //    var productIds = (await _settingService.GetSettingByKeyAsync<string>("Valid.ProductIds.To.Display.Variant.Image.By.Route")) ?? "";
                //    List<int> lstproductId = new List<int>();
                //    foreach (var productId in productIds.Split(','))
                //    {
                //        int.TryParse(productId, out int _productId);
                //        lstproductId.Add(_productId);
                //    }

                //    var sizes = (await _settingService.GetSettingByKeyAsync<string>("Valid.Variant.Size.To.Display.Variant.Image.By.Route")) ?? "";
                //    List<string> lstSizes = new List<string>();
                //    foreach (var size in sizes.Split(','))
                //    {
                //        lstSizes.Add(size);
                //    }

                //    if (!lstproductId.Contains(product.Id))
                //    {
                //        pictureId = 0;
                //    }
                //    else
                //    {

                //        if (!lstSizes.Where(s => s.Equals(variantSize ?? "", StringComparison.InvariantCultureIgnoreCase)).Any())
                //        {
                //            pictureId = 0;
                //        }

                //    }

                //}
                List<List<int>> allCombinations = new List<List<int>>();
                if (product.EnableConditionalAttributes)
                {
                    model = (await FilterProductAttributesByCombinationsAsync(model, product)).ToList();
                    allCombinations = await BuildValidProductAttributeCombinationsAsync(product, model);
                }
                return (variantTitle, model, pictureId, allCombinations);
            });
        }
        protected async Task<IList<CustomProductDetailsModel.ProductAttributeModel>> FilterProductAttributesByCombinationsAsync(IList<CustomProductDetailsModel.ProductAttributeModel> attributes, Product product)
        {
            if (attributes == null || !attributes.Any())
                return attributes;

            var combinations = await GetValidProductAttributeCombinationsAsync(product);

            var selectedValues = new List<int>();

            for (int i = 0; i < attributes.Count; i++)
            {
                var attribute = attributes[i];

                if (i > 0)
                {
                    foreach (var value in attribute.Values)
                    {
                        bool isValid = combinations.Any(c =>
                        {
                            var xml = c.AttributesXml;

                            // must contain this value
                            if (!xml.Contains(value.Id.ToString()))
                                return false;

                            // must match previous selections
                            foreach (var prev in selectedValues.Take(i))
                            {
                                if (!xml.Contains(prev.ToString()))
                                    return false;
                            }

                            return true;
                        });

                        value.Hide = !isValid;
                        if (!isValid)
                        {
                            value.IsPreSelected = false;
                        }
                    }
                }
                var selected = attribute.Values.FirstOrDefault(v => v.IsPreSelected && !v.Hide);

                if (selected == null)
                {
                    selected = attribute.Values.FirstOrDefault(v => !v.Hide);
                    if (selected != null)
                        selected.IsPreSelected = true;
                }

                if (selected != null)
                    selectedValues.Add(selected.Id);
            }



            return attributes;
        }

        protected async Task<List<List<int>>> BuildValidProductAttributeCombinationsAsync(Product product, IList<CustomProductDetailsModel.ProductAttributeModel> productAttributes)
        {
            #region Fully Dynamic Combination Builder

            var attributeCombinations = await GetValidProductAttributeCombinationsAsync(product);

            // We only need a list of all valid ID sets
            // Example: [[10, 25, 30], [10, 26, 31]]
            var allCombinations = new List<List<int>>();

            foreach (var combination in attributeCombinations)
            {
                var values = await _productAttributeParser.ParseProductAttributeValuesAsync(combination.AttributesXml);
                var ids = values.Select(v => v.Id).ToList();
                if (ids.Any())
                {
                    allCombinations.Add(ids);
                }
            }

            var validValueIds = allCombinations.SelectMany(x => x).Distinct().ToHashSet();
            foreach (var productAttribute in productAttributes)
            {
                // Remove values that are not part of any valid combination
                productAttribute.Values = productAttribute.Values
                    .Where(v => validValueIds.Contains(v.Id))
                    .ToList();

                // 3. Re-handle Pre-selection logic for the now-filtered list
                var exists = productAttribute.Values.Any(x => x.IsPreSelected);
                if (!exists && productAttribute.Values.Any())
                {
                    productAttribute.Values.First().IsPreSelected = true;
                }
            }

            // 4. Pass the combinations list to the model (Ensure your Model has this property)
            return allCombinations;

            #endregion
        }
        protected virtual async Task<(string, IList<CustomProductDetailsModel.ProductAttributeModel>, int pictureId, List<List<int>> allCombinations)>
            CustomPrepareFeatureProductAttributeWithVariantTitleModelsAsync(Product product, ShoppingCartItem updatecartitem, int variantId = 0)
        {
            var productAttributeCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.FeaturedProductAttributeModelKey
 , product.Id, variantId, updatecartitem?.Id ?? 0, _webHelper.IsCurrentConnectionSecured(), await _storeContext.GetCurrentStoreAsync());
            var store = updatecartitem != null ? await _storeService.GetStoreByIdAsync(updatecartitem.StoreId) : await _storeContext.GetCurrentStoreAsync();
            return await _staticCacheManager.GetAsync(productAttributeCacheKey, async () =>
            {
                string variantTitle = "";
                var variantcombinations = await _customProductService.GetProductVariants(product.Id);
                List<int> variantAttrs = new List<int>();
                if (variantId != 0)
                {
                    var combination = variantcombinations.FirstOrDefault(x => x.VariantId == variantId);
                    if (combination != null)
                    {
                        variantTitle = combination.Title;
                        variantAttrs = (combination.ProductAttributeValueIds ?? string.Empty)
                                             .Split("-")
                                             .Select(id => int.Parse(id))
                                             .ToList();
                    }
                }


                string variantSize = "";
                int pictureId = 0;
                if (product == null)
                    throw new ArgumentNullException(nameof(product));

                var model = new List<CustomProductDetailsModel.ProductAttributeModel>();

                var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
                foreach (var attribute in productAttributeMapping)
                {
                    var productAttrubute = await _productAttributeService.GetProductAttributeByIdAsync(attribute.ProductAttributeId);

                    var attributeModel = new CustomProductDetailsModel.ProductAttributeModel
                    {
                        Id = attribute.Id,
                        ProductId = product.Id,
                        ProductAttributeId = attribute.ProductAttributeId,
                        Name = await _localizationService.GetLocalizedAsync(productAttrubute, x => x.Name),
                        Description = await _localizationService.GetLocalizedAsync(productAttrubute, x => x.Description),
                        TextPrompt = await _localizationService.GetLocalizedAsync(attribute, x => x.TextPrompt),
                        IsRequired = attribute.IsRequired,
                        AttributeControlType = attribute.AttributeControlType,
                        DefaultValue = updatecartitem != null ? null : await _localizationService.GetLocalizedAsync(attribute, x => x.DefaultValue),
                        HasCondition = !string.IsNullOrEmpty(attribute.ConditionAttributeXml),
                        IsExpanded = attribute.IsExpanded,
                        EnableHoverImpact = attribute.EnableHoverImpact
                    };
                    if (!string.IsNullOrEmpty(attribute.ValidationFileAllowedExtensions))
                    {
                        attributeModel.AllowedFileExtensions = attribute.ValidationFileAllowedExtensions
                            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .ToList();
                    }

                    if (attribute.ShouldHaveValues())
                    {
                        //values
                        var _attributeValues = await _productAttributeService.GetProductAttributeValuesAsync(attribute.Id);
                        _attributeValues = _attributeValues.Where(av => av.Published).ToList();
                        List<ProductAttributeValue> attributeValues = new List<ProductAttributeValue>();
                        foreach (var attributeValue in _attributeValues)
                        {
                            attributeValues.Add(new ProductAttributeValue()
                            {
                                AssociatedProductId = attributeValue.AssociatedProductId,
                                AttributeValueType = attributeValue.AttributeValueType,
                                AttributeValueTypeId = attributeValue.AttributeValueTypeId,
                                ColorSquaresRgb = attributeValue.ColorSquaresRgb,
                                Cost = attributeValue.Cost,
                                CustomerEntersQty = attributeValue.CustomerEntersQty,
                                Dimension = attributeValue.Dimension,
                                DisplayOrder = attributeValue.DisplayOrder,
                                Id = attributeValue.Id,
                                ImageSquaresPictureId = attributeValue.ImageSquaresPictureId,
                                IsPreSelected = attributeValue.IsPreSelected,
                                ManufacturerPartNumber = attributeValue.ManufacturerPartNumber,
                                Name = attributeValue.Name,
                                FeaturedPictureId = attributeValue.FeaturedPictureId,
                                PriceAdjustment = attributeValue.PriceAdjustment,
                                PriceAdjustmentUsePercentage = attributeValue.PriceAdjustmentUsePercentage,
                                ProductAttributeMappingId = attributeValue.ProductAttributeMappingId,
                                Published = attributeValue.Published,
                                Quantity = attributeValue.Quantity,
                                QueryParameter = attributeValue.QueryParameter,
                                VariantId = attributeValue.VariantId,
                                VariantTitle = attributeValue.VariantTitle,
                                WeightAdjustment = attributeValue.WeightAdjustment,
                                VariantDimension = attributeValue.VariantDimension

                            });
                        }

                        if (variantId != 0)
                        {
                            if (!productAttrubute.Name.Equals(await _localizationService.GetResourceAsync("Product.Attr.Stain"), StringComparison.InvariantCultureIgnoreCase))
                            {
                                foreach (var attributeValue in attributeValues)
                                {
                                    var attributePictureId = (await _productAttributeService.GetProductAttributeValuePicturesAsync(attributeValue.Id)).FirstOrDefault()?.PictureId ?? 0;
                                    if (variantAttrs.Contains(attributeValue.Id))
                                    {
                                        if (productAttrubute.Name.Equals(await _localizationService.GetResourceAsync("Product.Attr.Size"), StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            variantSize = attributeValue.Name;
                                            pictureId = attributeValue.FeaturedPictureId == 0 ? attributePictureId : attributeValue.FeaturedPictureId;
                                        }
                                        attributeValue.IsPreSelected = true;
                                    }
                                    else
                                        attributeValue.IsPreSelected = false;
                                }
                            }

                        }
                        else if (productAttrubute.Name.Equals(await _localizationService.GetResourceAsync("Product.Attr.Size"), StringComparison.InvariantCultureIgnoreCase))
                        {

                            int attributePictureId = 0;
                            var preselectedAttribute = attributeValues.Where(a => a.IsPreSelected).FirstOrDefault();
                            if (preselectedAttribute != null)
                            {
                                attributePictureId = (await _productAttributeService.GetProductAttributeValuePicturesAsync(preselectedAttribute.Id)).FirstOrDefault()?.PictureId ?? 0;
                            }
                            variantTitle = preselectedAttribute?.VariantTitle;
                            pictureId = preselectedAttribute != null ? preselectedAttribute.FeaturedPictureId == 0 ?
                                attributePictureId : preselectedAttribute.FeaturedPictureId : 0;

                            variantSize = preselectedAttribute?.Name ?? "";
                        }

                        string pictureDefaultSizeUrl = string.Empty;
                        foreach (var attributeValue in attributeValues)
                        {
                            var attributePictureId = (await _productAttributeService.GetProductAttributeValuePicturesAsync(attributeValue.Id)).FirstOrDefault()?.PictureId ?? 0;
                            pictureDefaultSizeUrl = string.Empty;

                            if (attributePictureId > 0)
                            {
                                var productAttributePictureCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.ProductAttributePictureModelKey,
                                    attributePictureId, _webHelper.IsCurrentConnectionSecured(), await _storeContext.GetCurrentStoreAsync());
                                var pictureModel = await _staticCacheManager.GetAsync(productAttributePictureCacheKey, async () =>
                                {
                                    var picture = await _pictureService.GetPictureByIdAsync(attributePictureId);
                                    string fullSizeImageUrl, imageUrl, thumbImageUrl;

                                    (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                                    (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.ProductDetailsPictureSize);


                                    (thumbImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage);

                                    return picture == null ? new PictureModel() : new PictureModel
                                    {
                                        FullSizeImageUrl = fullSizeImageUrl,
                                        ImageUrl = imageUrl,
                                        ThumbImageUrl = thumbImageUrl
                                    };
                                });

                                pictureDefaultSizeUrl = pictureModel.ImageUrl;

                            }


                            var valueModel = new CustomProductDetailsModel.ProductAttributeValueModel
                            {
                                Id = attributeValue.Id,
                                Name = await _localizationService.GetLocalizedAsync(attributeValue, x => x.Name),
                                ColorSquaresRgb = attributeValue.ColorSquaresRgb, //used with "Color squares" attribute type
                                IsPreSelected = attributeValue.IsPreSelected,
                                CustomerEntersQty = attributeValue.CustomerEntersQty,
                                Quantity = attributeValue.Quantity,
                                VariantId = attributeValue.VariantId,
                                Dimension = attributeValue.Dimension,
                                VariantDimension = attributeValue.VariantDimension,
                                PictureDefaultSizeUrl = pictureDefaultSizeUrl

                            };
                            attributeModel.Values.Add(valueModel);

                            //display price if allowed
                            if (await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.DISPLAY_PRICES))
                            {
                                var customer = updatecartitem?.CustomerId is null ? await _workContext.GetCurrentCustomerAsync() : await _customerService.GetCustomerByIdAsync(updatecartitem.CustomerId);

                                var attributeValuePriceAdjustment = await _priceCalculationService.GetProductAttributeValuePriceAdjustmentAsync(product, attributeValue, customer, store, quantity: updatecartitem?.Quantity ?? 1);
                                var (priceAdjustmentBase, _) = await _taxService.GetProductPriceAsync(product, attributeValuePriceAdjustment);
                                var priceAdjustment = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(priceAdjustmentBase, await _workContext.GetWorkingCurrencyAsync());

                                if (attributeValue.PriceAdjustmentUsePercentage)
                                {
                                    var priceAdjustmentStr = attributeValue.PriceAdjustment.ToString("G29");
                                    if (attributeValue.PriceAdjustment > decimal.Zero)
                                        valueModel.PriceAdjustment = "+";
                                    valueModel.PriceAdjustment += priceAdjustmentStr + "%";
                                }
                                else
                                {
                                    if (priceAdjustmentBase > decimal.Zero)
                                        valueModel.PriceAdjustment = "+" + await _priceFormatter.FormatPriceAsync(priceAdjustment, false, false);
                                    else if (priceAdjustmentBase < decimal.Zero)
                                        valueModel.PriceAdjustment = "-" + await _priceFormatter.FormatPriceAsync(-priceAdjustment, false, false);
                                }

                                valueModel.PriceAdjustmentValue = priceAdjustment;
                            }

                            //"image square" picture (with with "image squares" attribute type only)
                            if (attributeValue.ImageSquaresPictureId > 0)
                            {
                                var productAttributeImageSquarePictureCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.ProductAttributeImageSquarePictureModelKey
                                    , attributeValue.ImageSquaresPictureId,
                                        _webHelper.IsCurrentConnectionSecured(),
                                        await _storeContext.GetCurrentStoreAsync());
                                valueModel.ImageSquaresPictureModel = await _staticCacheManager.GetAsync(productAttributeImageSquarePictureCacheKey, async () =>
                                {
                                    var imageSquaresPicture = await _pictureService.GetPictureByIdAsync(attributeValue.ImageSquaresPictureId);
                                    string fullSizeImageUrl, imageUrl;
                                    (imageUrl, imageSquaresPicture) = await _pictureService.GetPictureUrlAsync(imageSquaresPicture, _mediaSettings.ImageSquarePictureSize);
                                    (fullSizeImageUrl, imageSquaresPicture) = await _pictureService.GetPictureUrlAsync(imageSquaresPicture);

                                    if (imageSquaresPicture != null)
                                    {
                                        return new PictureModel
                                        {
                                            FullSizeImageUrl = fullSizeImageUrl,
                                            ImageUrl = imageUrl
                                        };
                                    }

                                    return new PictureModel();
                                });
                            }

                            //picture of a product attribute value
                            valueModel.PictureId = attributePictureId;
                        }
                    }

                    //set already selected attributes (if we're going to update the existing shopping cart item)
                    if (updatecartitem != null)
                    {
                        switch (attribute.AttributeControlType)
                        {
                            case AttributeControlType.DropdownList:
                            case AttributeControlType.RadioList:
                            case AttributeControlType.Checkboxes:
                            case AttributeControlType.ColorSquares:
                            case AttributeControlType.ImageSquares:
                                {
                                    if (!string.IsNullOrEmpty(updatecartitem.AttributesXml))
                                    {
                                        //clear default selection
                                        foreach (var item in attributeModel.Values)
                                            item.IsPreSelected = false;

                                        //select new values
                                        var selectedValues = await _productAttributeParser.ParseProductAttributeValuesAsync(updatecartitem.AttributesXml);
                                        foreach (var attributeValue in selectedValues)
                                            foreach (var item in attributeModel.Values)
                                                if (attributeValue.Id == item.Id)
                                                {
                                                    item.IsPreSelected = true;

                                                    //set customer entered quantity
                                                    if (attributeValue.CustomerEntersQty)
                                                        item.Quantity = attributeValue.Quantity;
                                                }
                                    }
                                }

                                break;
                            case AttributeControlType.ReadonlyCheckboxes:
                                {
                                    //values are already pre-set

                                    //set customer entered quantity
                                    if (!string.IsNullOrEmpty(updatecartitem.AttributesXml))
                                    {
                                        foreach (var attributeValue in (await _productAttributeParser.ParseProductAttributeValuesAsync(updatecartitem.AttributesXml))
                                            .Where(value => value.CustomerEntersQty))
                                        {
                                            var item = attributeModel.Values.FirstOrDefault(value => value.Id == attributeValue.Id);
                                            if (item != null)
                                                item.Quantity = attributeValue.Quantity;
                                        }
                                    }
                                }

                                break;
                            case AttributeControlType.TextBox:
                            case AttributeControlType.MultilineTextbox:
                                {
                                    if (!string.IsNullOrEmpty(updatecartitem.AttributesXml))
                                    {
                                        var enteredText = _productAttributeParser.ParseValues(updatecartitem.AttributesXml, attribute.Id);
                                        if (enteredText.Any())
                                            attributeModel.DefaultValue = enteredText[0];
                                    }
                                }

                                break;
                            case AttributeControlType.Datepicker:
                                {
                                    //keep in mind my that the code below works only in the current culture
                                    var selectedDateStr = _productAttributeParser.ParseValues(updatecartitem.AttributesXml, attribute.Id);
                                    if (selectedDateStr.Any())
                                    {
                                        if (DateTime.TryParseExact(selectedDateStr[0], "D", CultureInfo.CurrentCulture, DateTimeStyles.None, out var selectedDate))
                                        {
                                            //successfully parsed
                                            attributeModel.SelectedDay = selectedDate.Day;
                                            attributeModel.SelectedMonth = selectedDate.Month;
                                            attributeModel.SelectedYear = selectedDate.Year;
                                        }
                                    }
                                }

                                break;
                            case AttributeControlType.FileUpload:
                                {
                                    if (!string.IsNullOrEmpty(updatecartitem.AttributesXml))
                                    {
                                        var downloadGuidStr = _productAttributeParser.ParseValues(updatecartitem.AttributesXml, attribute.Id).FirstOrDefault();
                                        Guid.TryParse(downloadGuidStr, out var downloadGuid);
                                        var download = await _downloadService.GetDownloadByGuidAsync(downloadGuid);
                                        if (download != null)
                                            attributeModel.DefaultValue = download.DownloadGuid.ToString();
                                    }
                                }

                                break;
                            default:
                                break;
                        }
                    }
                    if (attributeModel.Values.Count > 0)
                    {

                        model.Add(attributeModel);
                    }
                }

                List<List<int>> allCombinations = new List<List<int>>();
                if (product.EnableConditionalAttributes)
                {
                    model = (await FilterProductAttributesByCombinationsAsync(model, product)).ToList();
                    allCombinations = await BuildValidProductAttributeCombinationsAsync(product, model);
                }
                return (variantTitle, model, pictureId, allCombinations);
            });
        }

        public async Task<string> GetProductSku(string sku, int id)
        {
            sku = sku ?? "";
            var prdManufacturers = await _manufacturerService.GetProductManufacturersByProductIdAsync(id, true);
            if (prdManufacturers.Count > 0)
            {
                var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(prdManufacturers.FirstOrDefault().ManufacturerId);
                if (manufacturer != null)
                    sku += manufacturer.Country == "Indonesia" ? "IND" : "";

            }
            return sku;
        }
        #endregion

        #region Api Factory Methods

        public async Task<List<ProductApiDetailsModel>> PrepareApiProductListModelAsync(int pageNumber, int productId)
        {

            List<ProductApiDetailsModel> lstModel = new List<ProductApiDetailsModel>();
            if (productId == 0)
            {
                int pageSize = await _settingService.GetSettingByKeyAsync<int>("Api.Catalog.products.PageSize");
                var products = await this._productService.SearchProductsAsync(pageIndex: pageNumber - 1, pageSize:
                        pageSize, showHidden: true);
                foreach (var product in products)
                {
                    lstModel.Add(await PrepareApiCustomProductDetailsModelAsync(product, null, false, 0));
                }
            }
            else
            {
                var product = await _productService.GetProductByIdAsync(productId);
                if (product != null)
                {
                    lstModel.Add(await PrepareApiCustomProductDetailsModelAsync(product, null, false, 0));
                }
            }
            return lstModel;
        }
        private async Task<ProductApiDetailsModel> PrepareApiCustomProductDetailsModelAsync(Product product,
           ShoppingCartItem updatecartitem = null, bool isAssociatedProduct = false, int variantId = 0)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));
            int mainCategorySpecificationAttributeId = await _settingService.GetSettingByKeyAsync<int>("mainCategorySpecificationAttributeId");
            //standard properties
            var model = new ProductApiDetailsModel
            {
                Id = product.Id,
                Name = await _localizationService.GetLocalizedAsync(product, x => x.Name),
                ShortDescription = await _localizationService.GetLocalizedAsync(product, x => x.ShortDescription),
                FullDescription = await _localizationService.GetLocalizedAsync(product, x => x.FullDescription),
                MetaKeywords = await _localizationService.GetLocalizedAsync(product, x => x.MetaKeywords),
                MetaDescription = await _localizationService.GetLocalizedAsync(product, x => x.MetaDescription),
                MetaTitle = await _localizationService.GetLocalizedAsync(product, x => x.MetaTitle),
                SeName = await _urlRecordService.GetSeNameAsync(product),
                ProductType = product.ProductType,
                Sku = await GetProductSku(product.Sku, product.Id),
                ManufacturerPartNumber = product.ManufacturerPartNumber,
                Gtin = product.Gtin,
                ManageInventoryMethod = product.ManageInventoryMethod,
                StockAvailability = await _productService.FormatStockMessageAsync(product, string.Empty),
                ParentGroupId = product.ParentGroupedProductId,
                EnableCustomizationModule = product.EnableCustomizationModule,
                Notes = product.Notes,
                TotalInventory = product.TotalInventory,
            };

            #region Individual Products

            model.IndividualProducts = string.Join(',', await _customProductService.GetPairWithProductsByProductId1Async(product.Id));

            #endregion

            var prdManufacturers = await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id, true);
            if (prdManufacturers.Count > 0)
            {
                var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(prdManufacturers.FirstOrDefault().ManufacturerId);
                model.Manufacturer = manufacturer.Name;
                model.ManufacturerId = manufacturer.Id;
            }
            model.MetaDescription = model.ShortDescription;


            //pictures

            IList<CustomPictureModel> allPictureModels;
            IList<VideoModel> allvideoModels;
            (model.DefaultPictureModel, allPictureModels, allvideoModels) = await CustomPrepareProductDetailsPictureModelAsync(product, isAssociatedProduct);
            model.PictureModels = allPictureModels;

            //price
            model.ProductPrice = await PrepareCustomProductPriceModelAsync(product);


            #region Variants
            var variants = await _customProductService.GetProductVariants(product.Id);
            model.Variants = await variants.SelectAwait(async variant =>
            {
                VariantModel variantModel = new VariantModel();
                var attributes = await _customProductAttributeFormatter.FormatAttributesAsync(product, variant.Combination);
                if (!string.IsNullOrWhiteSpace(attributes))
                {
                    variantModel.AttributeList = attributes
           .Replace("<br />", "|")
           .Replace("<br/>", "|")
           .Replace("<br>", "|")
           .Split('|', StringSplitOptions.RemoveEmptyEntries)
           .Select(x => x.Split(':', 2))
           .Where(x => x.Length == 2)
           .Select(x => new Dictionary<string, string>
           {
        { "Name", x[0].Trim() },
        { "Value", x[1].Trim() }
           })
           .ToList();
                }

                variantModel.VariantId = variant.VariantId;
                variantModel.Id = variant.Id;
                variantModel.OldPrice = variant.OldPrice;
                variantModel.Price = variant.Price;
                variantModel.Msrp = variant.Msrp;
                variantModel.WgsRequired = variant.WgsRequired;
                variantModel.Title = variant.Title;
                variantModel.EnableSurcharge = variant.EnableSurcharge;
                variantModel.QueryParameter = variant.QueryParameter;

                bool publish = true;
                foreach (var attrValueId in (variant.ProductAttributeValueIds ?? string.Empty).Split('-'))
                {
                    int.TryParse(attrValueId, out int _attrValueId);
                    var productAttributeValue = await _productAttributeService.GetProductAttributeValueByIdAsync(_attrValueId);
                    if (productAttributeValue == null || !productAttributeValue.Published)
                    {
                        publish = false;
                        break;
                    }
                }
                variantModel.Published = publish;

                return variantModel;
            }).ToListAsync();

            #endregion


            //product attributes


            int variantPictureId = 0;
            (model.VariantTitle, model.ProductAttributes, variantPictureId) = await CustomPrepareApiProductAttributeModelsAsync(product, variantId);



            var groups = await _specificationAttributeService.GetSpecificationAttributeGroupsAsync(0, int.MaxValue);
            var specifications = new List<ProductModel.CustomProductSpecificationModel>();
            foreach (var group in groups)
            {
                if (group.Id == 4)
                {
                    var Attributes = await CustomfeedPrepareProductSpecificationAttributeModelAsync(product, group);
                    foreach (var attr in Attributes)
                    {
                        specifications.Add(new ProductModel.CustomProductSpecificationModel()
                        {
                            Name = attr.Name,
                            Value = attr.Values.FirstOrDefault()?.ValueRaw
                        });
                        if (attr.Id == mainCategorySpecificationAttributeId)
                        {
                            int.TryParse(attr.Values.FirstOrDefault()?.ValueRaw, out int categoryId);
                            if (categoryId > 0)
                            {
                                var category = await _categoryService.GetCategoryByIdAsync(categoryId);
                                if (category != null)
                                {
                                    specifications.Add(new ProductModel.CustomProductSpecificationModel()
                                    {
                                        Name = "Main Category Name",
                                        Value = category.Name
                                    });
                                }
                            }

                        }

                    }
                }
            }

            model.Specifications = specifications;


            return model;
        }

        public async Task<string> GetProductMainImage(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            return (await this.PrepareProductOverviewPicturesModelAsync(product, null)).FirstOrDefault()?.FullSizeImageUrl ?? "";
        }

        public async Task<bool> IsVariantSurchargeApplicable(int variantId)
        {
            return (await _customProductService.GetVariantByVariantId(variantId))?.EnableSurcharge ?? false;
        }

        public async Task<List<VariantCombination>> PrepareProductVariants(string sku)
        {
            List<VariantCombination> model = new List<VariantCombination>();
            var product = await _productService.GetProductBySkuAsync(sku);
            if (product == null)
                return model;
            else
            {
                var variants = await _customProductService.GetProductVariants(product.Id);
                foreach (var variant in variants)
                {
                    string name = string.Empty;
                    var attributes = await _customProductAttributeFormatter.FormatAttributesAsync(product, variant.Combination);
                    if (!string.IsNullOrWhiteSpace(attributes))
                    {
                        name = string.Join('/', attributes
               .Replace("<br />", "|")
               .Replace("<br/>", "|")
               .Replace("<br>", "|")
               .Split('|', StringSplitOptions.RemoveEmptyEntries)
               .Select(x => x.Split(':', 2))
               .Where(x => x.Length == 2)
               .Select(x => x[1].Trim()
               )
               .ToList());
                    }
                    if (!model.Any(k => k.VariantId == variant.Id))
                        model.Add(new VariantCombination() { VariantId = variant.VariantId, Title = name, EnableSurcharge = variant.EnableSurcharge });
                }

            }
            return model;
        }

        protected virtual async Task<(string, IList<ProductApiDetailsModel.ProductAttributeModel>, int pictureId)> CustomPrepareApiProductAttributeModelsAsync(Product product, int variantId = 0)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            string variantTitle = "";
            var variantcombinations = await _customProductService.GetProductVariants(product.Id);
            List<int> variantAttrs = new List<int>();
            if (variantId != 0)
            {
                var combination = variantcombinations.FirstOrDefault(x => x.VariantId == variantId);
                if (combination != null)
                {
                    variantTitle = combination.Title;
                    variantAttrs = (combination.ProductAttributeValueIds ?? string.Empty)
                                         .Split("-")
                                         .Select(id => int.Parse(id))
                                         .ToList();
                }
            }


            string variantSize = "";
            int pictureId = 0;
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var model = new List<ProductApiDetailsModel.ProductAttributeModel>();

            var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
            foreach (var attribute in productAttributeMapping)
            {
                var productAttrubute = await _productAttributeService.GetProductAttributeByIdAsync(attribute.ProductAttributeId);

                var attributeModel = new ProductApiDetailsModel.ProductAttributeModel
                {
                    Id = attribute.Id,
                    ProductId = product.Id,
                    ProductAttributeId = attribute.ProductAttributeId,
                    Name = await _localizationService.GetLocalizedAsync(productAttrubute, x => x.Name),
                    Description = await _localizationService.GetLocalizedAsync(productAttrubute, x => x.Description),
                    TextPrompt = await _localizationService.GetLocalizedAsync(attribute, x => x.TextPrompt),
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType,
                    DefaultValue = await _localizationService.GetLocalizedAsync(attribute, x => x.DefaultValue),
                    HasCondition = !string.IsNullOrEmpty(attribute.ConditionAttributeXml),
                };
                if (!string.IsNullOrEmpty(attribute.ValidationFileAllowedExtensions))
                {
                    attributeModel.AllowedFileExtensions = attribute.ValidationFileAllowedExtensions
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                }

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var _attributeValues = await _productAttributeService.GetProductAttributeValuesAsync(attribute.Id);
                    _attributeValues = _attributeValues.Where(av => av.Published).ToList();
                    List<ProductAttributeValue> attributeValues = new List<ProductAttributeValue>();
                    foreach (var attributeValue in _attributeValues)
                    {
                        attributeValues.Add(new ProductAttributeValue()
                        {
                            AssociatedProductId = attributeValue.AssociatedProductId,
                            AttributeValueType = attributeValue.AttributeValueType,
                            AttributeValueTypeId = attributeValue.AttributeValueTypeId,
                            ColorSquaresRgb = attributeValue.ColorSquaresRgb,
                            Cost = attributeValue.Cost,
                            CustomerEntersQty = attributeValue.CustomerEntersQty,
                            Dimension = attributeValue.Dimension,
                            DisplayOrder = attributeValue.DisplayOrder,
                            Id = attributeValue.Id,
                            ImageSquaresPictureId = attributeValue.ImageSquaresPictureId,
                            IsPreSelected = attributeValue.IsPreSelected,
                            ManufacturerPartNumber = attributeValue.ManufacturerPartNumber,
                            Name = attributeValue.Name,
                            FeaturedPictureId = attributeValue.FeaturedPictureId,
                            PriceAdjustment = attributeValue.PriceAdjustment,
                            PriceAdjustmentUsePercentage = attributeValue.PriceAdjustmentUsePercentage,
                            ProductAttributeMappingId = attributeValue.ProductAttributeMappingId,
                            Published = attributeValue.Published,
                            Quantity = attributeValue.Quantity,
                            QueryParameter = attributeValue.QueryParameter,
                            VariantId = attributeValue.VariantId,
                            VariantTitle = attributeValue.VariantTitle,
                            WeightAdjustment = attributeValue.WeightAdjustment,
                            VariantDimension = attributeValue.VariantDimension


                        });
                    }

                    if (variantId != 0)
                    {
                        if (!productAttrubute.Name.Equals(await _localizationService.GetResourceAsync("Product.Attr.Stain"), StringComparison.InvariantCultureIgnoreCase))
                        {
                            foreach (var attributeValue in attributeValues)
                            {
                                if (variantAttrs.Contains(attributeValue.Id))
                                {
                                    if (productAttrubute.Name.Equals(await _localizationService.GetResourceAsync("Product.Attr.Size"), StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        variantSize = attributeValue.Name;
                                        pictureId = (await _productAttributeService.GetProductAttributeValuePicturesAsync(attributeValue.Id)).FirstOrDefault()?.PictureId ?? 0;
                                    }
                                    attributeValue.IsPreSelected = true;
                                }
                                else
                                    attributeValue.IsPreSelected = false;
                            }
                        }

                    }
                    else if (productAttrubute.Name.Equals(await _localizationService.GetResourceAsync("Product.Attr.Size"), StringComparison.InvariantCultureIgnoreCase))
                    {


                        var preselectedAttribute = attributeValues.Where(a => a.IsPreSelected).FirstOrDefault();
                        if (preselectedAttribute != null)
                        {
                            variantTitle = preselectedAttribute?.VariantTitle;
                            pictureId = (await _productAttributeService.GetProductAttributeValuePicturesAsync(preselectedAttribute.Id)).FirstOrDefault()?.PictureId ?? 0;
                            variantSize = preselectedAttribute?.Name ?? "";
                        }

                    }

                    string pictureDefaultSizeUrl = string.Empty;
                    string pictureFullSizeUrl = string.Empty;
                    foreach (var attributeValue in attributeValues)
                    {
                        List<int> attributePictures = (await _productAttributeService.GetProductAttributeValuePicturesAsync(attributeValue.Id)).Select(a => a.PictureId).ToList();
                        pictureDefaultSizeUrl = string.Empty;
                        pictureFullSizeUrl = string.Empty;
                        if (attributePictures.Count() > 0)
                        {
                            var productAttributePictureCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.ProductAttributePictureModelKey,
                                attributePictures.First(), _webHelper.IsCurrentConnectionSecured(), await _storeContext.GetCurrentStoreAsync());
                            var pictureModel = await _staticCacheManager.GetAsync(productAttributePictureCacheKey, async () =>
                            {
                                var picture = await _pictureService.GetPictureByIdAsync(attributePictures.First());
                                string fullSizeImageUrl, imageUrl, thumbImageUrl;

                                (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                                (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.ProductDetailsPictureSize);


                                (thumbImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage);

                                return picture == null ? new PictureModel() : new PictureModel
                                {
                                    FullSizeImageUrl = fullSizeImageUrl,
                                    ImageUrl = imageUrl,
                                    ThumbImageUrl = thumbImageUrl
                                };
                            });

                            pictureDefaultSizeUrl = pictureModel.ImageUrl;
                            pictureFullSizeUrl = pictureModel.FullSizeImageUrl;

                        }


                        var valueModel = new ProductApiDetailsModel.ProductAttributeValueModel
                        {
                            Id = attributeValue.Id,
                            Name = await _localizationService.GetLocalizedAsync(attributeValue, x => x.Name),
                            IsPreSelected = attributeValue.IsPreSelected,
                            CustomerEntersQty = attributeValue.CustomerEntersQty,
                            Quantity = attributeValue.Quantity,
                            VariantId = attributeValue.VariantId,
                            Dimension = attributeValue.Dimension,
                            VariantDimension = attributeValue.VariantDimension,
                            PictureFullSizeUrl = pictureFullSizeUrl
                        };


                        #region Gallery Pictures

                        List<PictureModel> gallery = new List<PictureModel>();
                        var productPicturesCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.ProductAttributePictureGalleryModelKey
                                          , attributeValue.Id, _webHelper.IsCurrentConnectionSecured(), await _storeContext.GetCurrentStoreAsync());

                        gallery = await _staticCacheManager.GetAsync(productPicturesCacheKey, async () =>
                        {
                            var productName = await _localizationService.GetLocalizedAsync(product, x => x.Name);

                            string fullSizeImageUrl, imageUrl, thumbImageUrl;

                            var pictureIds = attributePictures.Skip(0);


                            List<Picture> pictures = new List<Picture>();
                            foreach (var pictureId in pictureIds)
                            {
                                if (pictureId != 0)
                                {
                                    var picture = await _pictureService.GetPictureByIdAsync(pictureId);
                                    if (picture != null)
                                    {
                                        pictures.Add(picture);
                                    }
                                }
                            }

                            //all pictures
                            var pictureModels = new List<PictureModel>();
                            for (var i = 0; i < pictures.Count(); i++)
                            {
                                var picture = pictures[i];

                                (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.ProductDetailsPictureSize, true);
                                (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                                (thumbImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage);

                                var pictureModel = new PictureModel
                                {
                                    ImageUrl = imageUrl,
                                    ThumbImageUrl = thumbImageUrl,
                                    FullSizeImageUrl = fullSizeImageUrl,
                                    Title = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat.Details"), productName),
                                    AlternateText = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat.Details"), productName),
                                };
                                //"title" attribute
                                pictureModel.Title = !string.IsNullOrEmpty(picture.TitleAttribute) ?
                                    picture.TitleAttribute :
                                    string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat.Details"), productName);
                                //"alt" attribute
                                pictureModel.AlternateText = !string.IsNullOrEmpty(picture.AltAttribute) ?
                                    picture.AltAttribute :
                                    string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat.Details"), productName);

                                pictureModels.Add(pictureModel);
                            }

                            return pictureModels;
                        });

                        valueModel.Gallery = gallery.Select(img => img.FullSizeImageUrl).ToArray();

                        #endregion

                        attributeModel.Values.Add(valueModel);

                        //display price if allowed
                        if (await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.DISPLAY_PRICES))
                        {
                            var customer = await _workContext.GetCurrentCustomerAsync();

                            var attributeValuePriceAdjustment = await _priceCalculationService.GetProductAttributeValuePriceAdjustmentAsync(product, attributeValue, customer, store);
                            var (priceAdjustmentBase, _) = await _taxService.GetProductPriceAsync(product, attributeValuePriceAdjustment);
                            var priceAdjustment = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(priceAdjustmentBase, await _workContext.GetWorkingCurrencyAsync());

                            if (attributeValue.PriceAdjustmentUsePercentage)
                            {
                                var priceAdjustmentStr = attributeValue.PriceAdjustment.ToString("G29");
                                if (attributeValue.PriceAdjustment > decimal.Zero)
                                    valueModel.PriceAdjustment = "+";
                                valueModel.PriceAdjustment += priceAdjustmentStr + "%";
                            }
                            else
                            {
                                if (priceAdjustmentBase > decimal.Zero)
                                    valueModel.PriceAdjustment = "+" + await _priceFormatter.FormatPriceAsync(priceAdjustment, false, false);
                                else if (priceAdjustmentBase < decimal.Zero)
                                    valueModel.PriceAdjustment = "-" + await _priceFormatter.FormatPriceAsync(-priceAdjustment, false, false);
                            }

                            valueModel.PriceAdjustmentValue = priceAdjustment;
                        }


                    }
                }


                if (attributeModel.Values.Count > 0)
                {

                    model.Add(attributeModel);
                }
            }
            if (pictureId != 0)
            {
                var productIds = (await _settingService.GetSettingByKeyAsync<string>("Valid.ProductIds.To.Display.Variant.Image.By.Route")) ?? "";
                List<int> lstproductId = new List<int>();
                foreach (var productId in productIds.Split(','))
                {
                    int.TryParse(productId, out int _productId);
                    lstproductId.Add(_productId);
                }

                var sizes = (await _settingService.GetSettingByKeyAsync<string>("Valid.Variant.Size.To.Display.Variant.Image.By.Route")) ?? "";
                List<string> lstSizes = new List<string>();
                foreach (var size in sizes.Split(','))
                {
                    lstSizes.Add(size);
                }

                if (!lstproductId.Contains(product.Id))
                {
                    pictureId = 0;
                }
                else
                {

                    if (!lstSizes.Where(s => s.Equals(variantSize ?? "", StringComparison.InvariantCultureIgnoreCase)).Any())
                    {
                        pictureId = 0;
                    }

                }

            }
            return (variantTitle, model, pictureId);
        }
        #endregion



    }


}
