
//using MWT.Nop.Core.Service.Catalog;
//using MWT.Nop.Core.Service.StoreWideDiscount;
//using MWT.Nop.Core.Services.Media;
//using MWT.Plugin.Misc.MwtStorefront.Factories;
//using MWT.Plugin.Misc.MwtStorefront.Models.Catalog;
//using Nop.Core;
//using Nop.Core.Caching;
//using Nop.Core.Domain.Catalog;
//using Nop.Core.Domain.Customers;
//using Nop.Core.Domain.Media;
//using Nop.Core.Domain.Orders;
//using Nop.Core.Domain.Security;
//using Nop.Core.Domain.Seo;
//using Nop.Core.Domain.Shipping;
//using Nop.Core.Domain.Vendors;
//using Nop.Core.Infrastructure;
//using Nop.Services.Catalog;
//using Nop.Services.Common;
//using Nop.Services.Configuration;
//using Nop.Services.Customers;
//using Nop.Services.Directory;
//using Nop.Services.Helpers;
//using Nop.Services.Localization;
//using Nop.Services.Media;
//using Nop.Services.Orders;
//using Nop.Services.Security;
//using Nop.Services.Seo;
//using Nop.Services.Shipping.Date;
//using Nop.Services.Stores;
//using Nop.Services.Tax;
//using Nop.Services.Vendors;
//using Nop.Web.Factories;
//using Nop.Web.Infrastructure.Cache;
//using Nop.Web.Models.Catalog;
//using Nop.Web.Models.Media;
//using System.Text.RegularExpressions;
//using static Nop.Services.Security.StandardPermission;

//namespace MWT.Plugin.Misc.MwtStorefront.Factories.Catalog
//{
//    /// <summary>
//    /// Represents the product model factory
//    /// </summary>
//    public partial class CustomProductModelFactory : ProductModelFactory, ICustomProductModelFactory
//    {
//        private readonly ICustomPictureService _customPictureService;
//        private readonly ICustomProductService _customProductService;
//        public CustomProductModelFactory(CaptchaSettings captchaSettings,
//            CatalogSettings catalogSettings, CustomerSettings customerSettings,
//            ICategoryService categoryService, ICurrencyService currencyService,
//            ICustomerService customerService, ICustomWishlistService customWishlistService,
//            IDateRangeService dateRangeService, IDateTimeHelper dateTimeHelper,
//            IDownloadService downloadService, IGenericAttributeService genericAttributeService,
//            IJsonLdModelFactory jsonLdModelFactory, ILocalizationService localizationService,
//            IManufacturerService manufacturerService, IPermissionService permissionService,
//            IPictureService pictureService, IPriceCalculationService priceCalculationService,
//            IPriceFormatter priceFormatter, IProductAttributeParser productAttributeParser,
//            IProductAttributeService productAttributeService, IProductReviewService productReviewService,
//            IProductService productService, IProductTagService productTagService,
//            IProductTemplateService productTemplateService, IReviewTypeService reviewTypeService,
//            IShoppingCartService shoppingCartService,
//            ISpecificationAttributeService specificationAttributeService,
//            IStaticCacheManager staticCacheManager, IStoreContext storeContext,
//            IStoreService storeService, IShoppingCartModelFactory shoppingCartModelFactory,
//            ITaxService taxService, IUrlRecordService urlRecordService, IVendorService vendorService,
//            IVideoService videoService, IWebHelper webHelper, IWorkContext workContext,
//            MediaSettings mediaSettings, OrderSettings orderSettings, SeoSettings seoSettings,
//            ShippingSettings shippingSettings, VendorSettings vendorSettings , ICustomPictureService customPictureService,ICustomProductService customProductService) : base( captchaSettings,
//                catalogSettings, customerSettings, categoryService, currencyService, customerService,
//                customWishlistService, dateRangeService, dateTimeHelper, downloadService,
//                genericAttributeService, jsonLdModelFactory, localizationService, manufacturerService,
//                permissionService, pictureService, priceCalculationService, priceFormatter,
//                productAttributeParser, productAttributeService, productReviewService,
//                productService, productTagService, productTemplateService, reviewTypeService,
//                shoppingCartService, specificationAttributeService, staticCacheManager, storeContext,
//                storeService, shoppingCartModelFactory, taxService, urlRecordService, vendorService, videoService, webHelper,
//                workContext, mediaSettings, orderSettings, seoSettings, shippingSettings, vendorSettings)
//        {
//            _customPictureService = customPictureService;
//            _customProductService = customProductService;
//        }

//      //  public virtual async Task<IEnumerable<CustomProductOverviewModel>> PrepareCustomProductOverviewModelsAsync(IEnumerable<Product> products,
//      //bool preparePriceModel = true, bool preparePictureModel = true,
//      //int? productThumbPictureSize = null, bool prepareSpecificationAttributes = false,
//      //bool forceRedirectionAfterAddingToCart = false, bool prepareShades = false, bool prepareCollectionSpecificationAttribute = false,
//      //bool prepareSizeShadeAggregation = false,
//      //bool prepareAlternatePictureModel = false, bool isCategorypage = false)
//      //  {
//      //      if (products == null)
//      //          throw new ArgumentNullException(nameof(products));

//      //      var models = new List<CustomProductOverviewModel>();
//      //      foreach (var product in products)
//      //      {
//      //          var tags = (await _productTagService.GetAllProductTagsByProductIdAsync(product.Id)).ToList();

//      //          #region Thanks giving tag exclude
//      //          var _settingService = EngineContext.Current.Resolve<ISettingService>();
//      //          var thankGivingTag = await _settingService.GetSettingByKeyAsync<string>("ThanksGiving.ProductTag.Name");
//      //          tags = tags.Where(t => !t.Name.Contains(thankGivingTag, StringComparison.InvariantCultureIgnoreCase)).ToList();
//      //          #endregion

//      //          var model = new CustomProductOverviewModel
//      //          {
//      //              Id = product.Id,
//      //              Name = await _localizationService.GetLocalizedAsync(product, x => x.Name),
//      //              ShortDescription = await _localizationService.GetLocalizedAsync(product, x => x.ShortDescription),
//      //              FullDescription = await _localizationService.GetLocalizedAsync(product, x => x.FullDescription),
//      //              SeName = await _urlRecordService.GetSeNameAsync(product),
//      //              Sku = await GetProductSku(product.Sku, product.Id),
//      //              ProductType = product.ProductType,
//      //              MarkAsNew = product.MarkAsNew &&
//      //                  (!product.MarkAsNewStartDateTimeUtc.HasValue || product.MarkAsNewStartDateTimeUtc.Value < DateTime.UtcNow) &&
//      //                  (!product.MarkAsNewEndDateTimeUtc.HasValue || product.MarkAsNewEndDateTimeUtc.Value > DateTime.UtcNow),
//      //              Tags = tags,
//      //              Published = product.Published,
//      //              Inventory = product.TotalInventory,
//      //              EnableCustomizationModule = product.EnableCustomizationModule,
//      //              MetaKeywords = product.MetaKeywords,
//      //              NoOfSales = product.NoOfSales,
//      //          };

//      //          //price
//      //          if (preparePriceModel)
//      //          {
//      //              model.ProductPrice = await PrepareCustomProductOverviewPriceModelAsync(product, forceRedirectionAfterAddingToCart);
//      //          }
//      //          //picture
//      //          if (preparePictureModel && !prepareAlternatePictureModel)
//      //              model.PictureModels = await PrepareCustomProductOverviewPictureModelAsync(product, productThumbPictureSize, isCategorypage);
//      //          else if (preparePictureModel)
//      //              await PrepareCustomProductOverviewPictureModelAsync(product, model, productThumbPictureSize, isCategorypage);

//      //          //specs
//      //          if (prepareSpecificationAttributes)
//      //              model.ProductSpecificationModel = await PrepareProductSpecificationModelAsync(product);


//      //          if (prepareShades)
//      //          {
//      //              //  Model
//      //              var attrName = await EngineContext.Current.Resolve<ISettingService>().GetSettingAsync("Catalog.Product.Attribute.Shade.Name");

//      //              if (attrName != null && !string.IsNullOrEmpty(attrName.Value))
//      //                  model.Attribute = await this.PrepareCustomProductAttributeModelWithImageAsync(model.Id, attrName.Value, productThumbPictureSize);
//      //          }

//      //          if (prepareCollectionSpecificationAttribute)
//      //          {
//      //              var collectionSpcfAttrId = await EngineContext.Current.Resolve<ISettingService>().GetSettingByKeyAsync<int>("Catalog.Product.Collection.SpecificationAttribute.Id");
//      //              if (collectionSpcfAttrId != 0)
//      //              {
//      //                  var productSpecificationAttributes = await _specificationAttributeService.GetProductSpecificationAttributesByAttributeIdAsync(model.Id, collectionSpcfAttrId);
//      //                  model.CollectionMessage = productSpecificationAttributes.FirstOrDefault()?.CustomValue;
//      //              }
//      //          }
//      //          if (prepareSizeShadeAggregation)
//      //          {
//      //              var shadeAttrName = await EngineContext.Current.Resolve<ISettingService>().GetSettingAsync("Catalog.Product.Attribute.Shade.Name");
//      //              var sizeAttrName = await EngineContext.Current.Resolve<ISettingService>().GetSettingAsync("Catalog.Product.Attribute.Size.Name");
//      //              if (sizeAttrName != null && !string.IsNullOrEmpty(sizeAttrName.Value) && shadeAttrName != null && !string.IsNullOrEmpty(shadeAttrName.Value))
//      //                  model.SizeShadeMessage = await this.ProductShadeSizeMessage(model.Id, shadeAttrName.Value, sizeAttrName.Value);
//      //          }


//      //          //reviews
//      //          model.ReviewOverviewModel = await PrepareProductReviewOverviewModelAsync(product);

//      //          models.Add(model);
//      //      }

//      //      return models;
//      //  }

//        #region Helpers

//        public async Task<string> GetProductSku(string sku, int id)
//        {
//            sku = sku ?? "";
//            var prdManufacturers = await _manufacturerService.GetProductManufacturersByProductIdAsync(id, true);
//            if (prdManufacturers.Count > 0)
//            {
//                var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(prdManufacturers.FirstOrDefault().ManufacturerId);
//                if (manufacturer != null)
//                    sku += manufacturer.Country == "Indonesia" ? "IND" : "";

//            }
//            return sku;
//        }
//        private async Task<string> ProductShadeSizeMessage(int productId, string shadeAttrName, string sizeAttrName)
//        {
//            string message = string.Empty;
//            string attributeName = string.Empty;
//            var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(productId);
//            foreach (var attribute in productAttributeMapping)
//            {


//                var attributeValues = await _productAttributeService.GetProductAttributeValuesAsync(attribute.Id);
//                if (attributeValues.Where(v => v.Published).Count() > 0)
//                {
//                    var productAttribute = await _productAttributeService.GetProductAttributeByIdAsync(attribute.ProductAttributeId);
//                    attributeName = productAttribute.Name;
//                    if (string.Equals(attributeName, await _localizationService.GetResourceAsync("Product.Attr.Size"), StringComparison.InvariantCultureIgnoreCase) ||
//                       string.Equals(attributeName, await _localizationService.GetResourceAsync("Product.Attr.Stain"), StringComparison.InvariantCultureIgnoreCase))
//                    {
//                        if (attributeValues.Where(v => v.Published).Count() > 1)
//                        {
//                            attributeName = attributeName.EndsWith("s") ? attributeName : attributeName + "s";
//                        }
//                        message += $"{attributeValues.Where(a => a.Published).Count()} {attributeName}|";

//                    }
//                }


//            }

//            return message;
//        }


//        #endregion

//        protected virtual async Task<CustomProductOverviewModel.CustomProductPriceModel> PrepareCustomProductOverviewPriceModelAsync(Product product, bool forceRedirectionAfterAddingToCart = false)
//        {
//            if (product == null)
//                throw new ArgumentNullException(nameof(product));

//            var priceModel = new CustomProductOverviewModel.CustomProductPriceModel
//            {
//                ForceRedirectionAfterAddingToCart = forceRedirectionAfterAddingToCart
//            };

//            switch (product.ProductType)
//            {
//                case ProductType.GroupedProduct:
//                    //grouped product
//                    await PrepareCustomSimpleProductOverviewPriceModelAsync(product, priceModel);

//                    break;
//                case ProductType.SimpleProduct:
//                default:
//                    //simple product
//                    await PrepareCustomSimpleProductOverviewPriceModelAsync(product, priceModel);

//                    break;
//            }

//            return priceModel;
//        }
//        protected virtual async Task PrepareCustomSimpleProductOverviewPriceModelAsync(Product product, CustomProductOverviewModel.CustomProductPriceModel priceModel)
//        {
//            //add to cart button
//            priceModel.DisableBuyButton = product.DisableBuyButton ||
//                                          !await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.ENABLE_SHOPPING_CART) ||
//                                          !await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.DISPLAY_PRICES);

//            //add to wishlist button
//            priceModel.DisableWishlistButton = product.DisableWishlistButton ||
//                                               !await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.ENABLE_WISHLIST) ||
//                                               !await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.DISPLAY_PRICES);
//            //compare products
//            priceModel.DisableAddToCompareListButton = !_catalogSettings.CompareProductsEnabled;

//            //rental
//            priceModel.IsRental = product.IsRental;

//            //pre-order
//            if (product.AvailableForPreOrder)
//            {
//                priceModel.AvailableForPreOrder = !product.PreOrderAvailabilityStartDateTimeUtc.HasValue ||
//                                                  product.PreOrderAvailabilityStartDateTimeUtc.Value >=
//                                                  DateTime.UtcNow;
//                priceModel.PreOrderAvailabilityStartDateTimeUtc = product.PreOrderAvailabilityStartDateTimeUtc;
//            }

//            //prices
//            if (await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.DISPLAY_PRICES))
//            {
//                if (product.CustomerEntersPrice)
//                    return;

//                if (product.CallForPrice &&
//                    //also check whether the current user is impersonated
//                    (!_orderSettings.AllowAdminsToBuyCallForPriceProducts ||
//                     _workContext.OriginalCustomerIfImpersonated == null))
//                {
//                    //call for price
//                    priceModel.OldPrice = await _localizationService.GetResourceAsync("Products.CallForPrice");
//                    priceModel.Msrp = null;
//                    priceModel.Price = null;
//                    priceModel.MembershipPrice = null;
//                }
//                else
//                {
//                    //prices

//                    var (msrpPriceBase, _) = await _taxService.GetProductPriceAsync(product, product.Msrp);
//                    var (oldPriceBase, _) = await _taxService.GetProductPriceAsync(product, product.OldPrice);
//                    var (priceBase, _) = await _taxService.GetProductPriceAsync(product, product.Price);



//                    var oldPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(oldPriceBase, await _workContext.GetWorkingCurrencyAsync());
//                    var msrp = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(msrpPriceBase, await _workContext.GetWorkingCurrencyAsync());
//                    var price = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(priceBase, await _workContext.GetWorkingCurrencyAsync());

//                    // memberShipPrice

//                    var _shoppingcartService = EngineContext.Current.Resolve<IShoppingCartService>();
//                    //(var memberShipPrice, _) = await _shoppingcartService.MemberShipPriceOfProduct(product.Id, product.Msrp, product.OldPrice, product.Price);
//                    //if (memberShipPrice > decimal.Zero)
//                    //{
//                    //    memberShipPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(memberShipPrice, await _workContext.GetWorkingCurrencyAsync());
//                    //    priceModel.MembershipPrice = await _priceFormatter.FormatPriceAsync(memberShipPrice);
//                    //    priceModel.MembershipPriceValue = memberShipPrice;
//                    //}

//                    // end

//                    //When there is just one tier price (with  qty 1), there are no actual savings in the list.
//                    var strikeThroughPrice = oldPrice;
//                    if (strikeThroughPrice > decimal.Zero)
//                    {
//                        priceModel.OldPrice = await _priceFormatter.FormatPriceAsync(strikeThroughPrice);
//                        priceModel.OldPriceValue = strikeThroughPrice;
//                    }
//                    else
//                    {
//                        priceModel.OldPrice = await _priceFormatter.FormatPriceAsync(price);
//                        priceModel.OldPriceValue = price;
//                    }

//                    if (msrp > decimal.Zero && msrp > priceModel.OldPriceValue)
//                    {
//                        priceModel.Msrp = await _priceFormatter.FormatPriceAsync(msrp);
//                        priceModel.MsrpValue = msrp;
//                    }

//                    priceModel.Price = await _priceFormatter.FormatPriceAsync(price);
//                    priceModel.PriceValue = price;



//                    //property for German market
//                    //we display tax/shipping info only with "shipping enabled" for this product
//                    //we also ensure this it's not free shipping
//                    priceModel.DisplayTaxShippingInfo = _catalogSettings.DisplayTaxShippingInfoProductBoxes && product.IsShipEnabled && !product.IsFreeShipping;

//                    //PAngV default baseprice (used in Germany)
//                    priceModel.BasePricePAngV = await _priceFormatter.FormatBasePriceAsync(product, price);

//                    if (product.IsVariantProduct && product.MinPrice > 0 && product.MaxPrice > 0 && product.MaxPrice > product.MinPrice)
//                    {
//                        var (minMsrpBase, _) = await _taxService.GetProductPriceAsync(product, product.MinMsrp);
//                        var (maxMsrpBase, _) = await _taxService.GetProductPriceAsync(product, product.MaxMsrp);
//                        var (minOldPriceBase, _) = await _taxService.GetProductPriceAsync(product, product.MinOldprice);
//                        var (maxOldPriceBase, _) = await _taxService.GetProductPriceAsync(product, product.MaxOldPrice);
//                        var (minPriceBase, _) = await _taxService.GetProductPriceAsync(product, product.MinPrice);
//                        var (maxPriceBase, _) = await _taxService.GetProductPriceAsync(product, product.MaxPrice);

//                        var minMsrp = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(minMsrpBase, await _workContext.GetWorkingCurrencyAsync());
//                        var maxMsrp = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(maxMsrpBase, await _workContext.GetWorkingCurrencyAsync());
//                        var minOldPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(minOldPriceBase, await _workContext.GetWorkingCurrencyAsync());
//                        var maxOldPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(maxOldPriceBase, await _workContext.GetWorkingCurrencyAsync());
//                        var minPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(minPriceBase, await _workContext.GetWorkingCurrencyAsync());
//                        var maxPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(maxPriceBase, await _workContext.GetWorkingCurrencyAsync());
////ShoppinCartService
//                        //(var minMembershipPriceValue, _) = await _shoppingcartService.MemberShipPriceOfProduct(product.Id, product.MinMsrp, product.MinOldprice, product.MinPrice);
//                        //(var maxMembershipPriceValue, _) = await _shoppingcartService.MemberShipPriceOfProduct(product.Id, product.MaxMsrp, product.MaxOldPrice, product.MaxPrice);
//                        //if (minMembershipPriceValue > decimal.Zero && maxMembershipPriceValue > decimal.Zero)
//                        //{
//                        //    minMembershipPriceValue = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(minMembershipPriceValue, await _workContext.GetWorkingCurrencyAsync());
//                        //    priceModel.MinMembershipPrice = await _priceFormatter.FormatPriceAsync(minMembershipPriceValue);
//                        //    priceModel.MinMembershipPriceValue = minMembershipPriceValue;

//                        //    maxMembershipPriceValue = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(maxMembershipPriceValue, await _workContext.GetWorkingCurrencyAsync());
//                        //    priceModel.MaxMembershipPrice = await _priceFormatter.FormatPriceAsync(maxMembershipPriceValue);
//                        //    priceModel.MaxMembershipPriceValue = maxMembershipPriceValue;
//                        //}


//                        strikeThroughPrice = minOldPrice;
//                        if (strikeThroughPrice > decimal.Zero)
//                        {
//                            priceModel.MinOldPrice = await _priceFormatter.FormatPriceAsync(strikeThroughPrice);
//                            priceModel.MinOldPriceValue = strikeThroughPrice;

//                            priceModel.MaxOldPrice = await _priceFormatter.FormatPriceAsync(maxOldPrice);
//                            priceModel.MaxOldPriceValue = maxOldPrice;
//                        }
//                        else
//                        {
//                            priceModel.MinOldPrice = await _priceFormatter.FormatPriceAsync(minPriceBase);
//                            priceModel.MinOldPriceValue = minPriceBase;
//                            priceModel.MaxOldPrice = await _priceFormatter.FormatPriceAsync(maxPrice);
//                            priceModel.MaxOldPriceValue = maxPrice;
//                        }

//                        if (minMsrp > decimal.Zero && minMsrp > (minOldPrice == 0 ? minPrice : minOldPrice))
//                        {
//                            priceModel.MinMsrp = await _priceFormatter.FormatPriceAsync(minMsrp);
//                            priceModel.MinMsrpValue = minMsrp;

//                            priceModel.MaxMsrp = await _priceFormatter.FormatPriceAsync(
//                                maxMsrp > (maxOldPrice < maxPrice ? maxPrice : maxOldPrice) ? maxMsrp : (maxOldPrice < maxPrice ? maxPrice : maxOldPrice));
//                            priceModel.MaxMsrpValue = maxMsrp > (maxOldPrice < maxPrice ? maxPrice : maxOldPrice) ? maxMsrp : (maxOldPrice < maxPrice ? maxPrice : maxOldPrice);
//                        }

//                        priceModel.MinPrice = await _priceFormatter.FormatPriceAsync(minPrice);
//                        priceModel.MinPriceValue = minPrice;


//                        priceModel.MaxPrice = await _priceFormatter.FormatPriceAsync(maxPrice);
//                        priceModel.MaxPriceValue = maxPrice;
//                        priceModel.IsVariantProduct = product.IsVariantProduct;
//                    }

//                }
//            }
//            else
//            {
//                //hide prices
//                priceModel.MembershipPrice = null;
//                priceModel.Msrp = null;
//                priceModel.OldPrice = null;
//                priceModel.Price = null;
//            }

//            #region Price Offer Module

//            var _storeWideDiscountService = EngineContext.Current.Resolve<IStoreWideDiscountService>();
//            var offerInfo = await _storeWideDiscountService.GetStoreWideProductDiscountInfoByProductIdAsync(product.Id);
//            var _settingService = EngineContext.Current.Resolve<ISettingService>();

//            string offerText = offerInfo?.InfoText ?? "";
//            string offerHelptext = offerInfo?.InfoHelpText ?? "";
//            priceModel.OfferText = "";
//            if (!string.IsNullOrEmpty(offerText) && !string.IsNullOrEmpty(offerText))
//            {
//                priceModel.OfferText = string.Format(await _localizationService.GetResourceAsync("label.limitedoffer.placeholder"), offerText, offerHelptext);
//            }
//            priceModel.Price = CustomCommonHelper.FormatCurrencyPriceWithoutDecimal(priceModel.Price);
//            priceModel.PriceValue = CustomCommonHelper.FormatPriceWithoutDecimal((decimal)priceModel.PriceValue);
//            priceModel.OldPrice = CustomCommonHelper.FormatCurrencyPriceWithoutDecimal(priceModel.OldPrice);
//            priceModel.OldPriceValue = CustomCommonHelper.FormatPriceWithoutDecimal(priceModel.OldPriceValue);
//            priceModel.MinPrice = CustomCommonHelper.FormatCurrencyPriceWithoutDecimal(priceModel.MinPrice);
//            priceModel.MinPriceValue = CustomCommonHelper.FormatPriceWithoutDecimal(priceModel.MinPriceValue);
//            priceModel.MinOldPrice = CustomCommonHelper.FormatCurrencyPriceWithoutDecimal(priceModel.MinOldPrice);
//            priceModel.MinOldPriceValue = CustomCommonHelper.FormatPriceWithoutDecimal(priceModel.MinOldPriceValue);
//            priceModel.MaxPrice = CustomCommonHelper.FormatCurrencyPriceWithoutDecimal(priceModel.MaxPrice);
//            priceModel.MaxPriceValue = CustomCommonHelper.FormatPriceWithoutDecimal(priceModel.MaxPriceValue);
//            priceModel.MaxOldPrice = CustomCommonHelper.FormatCurrencyPriceWithoutDecimal(priceModel.MaxOldPrice);
//            priceModel.MaxOldPriceValue = CustomCommonHelper.FormatPriceWithoutDecimal(priceModel.MaxOldPriceValue);
//            #endregion
//        }

//        protected async Task<(bool isVariantExist, decimal msrp, decimal oldPrice, decimal price)> GetVariantPrice(Product product, int variantId)
//        {
//            bool isVariantExist = false;
//            decimal msrp = 0;
//            decimal oldPrice = 0;
//            decimal price = 0;

//            var variant = (await this._customProductService.GetProductVariants(product.Id)).Where(v => v.VariantId == variantId).FirstOrDefault();
//            if (variant != null)
//            {
//                msrp = variant.Msrp ?? product.Msrp;
//                oldPrice = variant.OldPrice ?? product.OldPrice;
//                price = Convert.ToDecimal(variant.Price == 0 ? product.Price : variant.Price);
//                isVariantExist = true;
//            }
//            return (isVariantExist, msrp, oldPrice, price);
//        }
//        public virtual async Task<PictureModel> PrepareCustomProductOverviewPictureModelAsync(Product product, int? productThumbPictureSize = null, bool isCategorypage = false)
//        {
//            if (product == null)
//                throw new ArgumentNullException(nameof(product));

//            var productName = await _localizationService.GetLocalizedAsync(product, x => x.Name);
//            //If a size has been set in the view, we use it in priority
//            var pictureSize = productThumbPictureSize ?? _mediaSettings.ProductThumbPictureSize;

//            //prepare picture model
//            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.CustomProductDefaultPictureModelKey,
//                product, pictureSize, true, await _workContext.GetWorkingLanguageAsync(), _webHelper.IsCurrentConnectionSecured(),
//                await _storeContext.GetCurrentStoreAsync(), isCategorypage ? "category" : "listing");

//            var defaultPictureModel = await _staticCacheManager.GetAsync(cacheKey, async () =>
//            {
//                var picture = (await _customPictureService.GetproductListingimage(product.Id, isCategorypage));
//                string fullSizeImageUrl, imageUrl;
//                (imageUrl, picture) = await _customPictureService.GetPictureUrlAsync(picture, pictureSize);
//                (fullSizeImageUrl, picture) = await _customPictureService.GetPictureUrlAsync(picture);

//                var pictureModel = new PictureModel
//                {
//                    ImageUrl = imageUrl,
//                    FullSizeImageUrl = fullSizeImageUrl,
//                    //"title" attribute
//                    Title = (picture != null && !string.IsNullOrEmpty(picture.TitleAttribute))
//                        ? picture.TitleAttribute
//                        : string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat"),
//                            productName),
//                    //"alt" attribute
//                    AlternateText = (picture != null && !string.IsNullOrEmpty(picture.AltAttribute))
//                        ? picture.AltAttribute
//                        : string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat"),
//                            productName)
//                };

//                return pictureModel;
//            });

//            return defaultPictureModel;
//        }

//    }
//}