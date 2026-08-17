using MWT.Nop.Core.Domain;
using MWT.Nop.Core.Domain.Marketing;
using MWT.Nop.Core.Service;
using MWT.Nop.Core.Service.Discounts;
using MWT.Nop.Core.Services.Customers;
using MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.Utilities;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Topics;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Infrastructure.Cache;

namespace MWT.Plugin.Misc.MwtStorefront.Area.Admin.Factories.Customization
{
    public partial class UtilitiesModelFactory : IUtilitiesModelFactory
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IProductService _productService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly ICategoryService _categoryService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly ISettingService _settingService;
        private readonly IRepository<RelatedProduct> _relatedProductRepository;
        private readonly IRepository<FBTProduct> _fbtProductRepository;
        private readonly IRepository<CrossSellProduct> _crossSellProductRepository;
        private readonly IRepository<CollectionProduct> _collectionProductRepository;
        private readonly IRepository<PairWithProduct> _pairWithProductRepository;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly MarketingSettings _marketingSettings;
        private readonly IStoreContext _storeContext;
        private readonly ITopicService _topicService;
        private readonly IWorkContext _workContext;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IStoreWideDiscountService _storeWideDiscountService;
        private readonly ICustomerExtendedService _customerService;
        private readonly ITestimonialService _testimonialService;
        private readonly IOrderService _orderService;

        #endregion

        #region Ctor 
        public UtilitiesModelFactory(ILocalizationService localizationService,
             IProductService productService, IProductAttributeService productAttributeService,
             ICategoryService categoryService, ISpecificationAttributeService specificationAttributeService,
             ISettingService settingService, IRepository<RelatedProduct> relatedProductRepository,
             IRepository<FBTProduct> fbtProductRepository,
            IRepository<CrossSellProduct> crossSellProductRepository,
            IRepository<CollectionProduct> collectionProductRepository,
            IRepository<PairWithProduct> pairWithProductRepository,
            IStaticCacheManager staticCacheManager,
            MarketingSettings marketingSettings,
            IStoreContext storeContext,
            ITopicService topicService,
            IWorkContext workContext,
            IBaseAdminModelFactory baseAdminModelFactory,
            IStoreWideDiscountService storeWideDiscountService,
            ICustomerExtendedService customerService,
            ITestimonialService testimonialService,
             IOrderService orderService
            )
        {
            this._localizationService = localizationService;
            this._productService = productService;
            this._productAttributeService = productAttributeService;
            this._categoryService = categoryService;
            this._specificationAttributeService = specificationAttributeService;
            this._settingService = settingService;
            this._relatedProductRepository = relatedProductRepository;
            this._fbtProductRepository = fbtProductRepository;
            this._crossSellProductRepository = crossSellProductRepository;
            this._collectionProductRepository = collectionProductRepository;
            this._pairWithProductRepository = pairWithProductRepository;
            this._staticCacheManager = staticCacheManager;
            this._marketingSettings = marketingSettings;
            this._storeContext = storeContext;
            this._topicService = topicService;
            this._workContext = workContext;
            this._baseAdminModelFactory = baseAdminModelFactory;
            this._storeWideDiscountService = storeWideDiscountService;
            this._customerService = customerService;
            this._testimonialService = testimonialService;
            this._orderService = orderService;
        }

        #endregion

        #region Methods
        //public async Task<ProductNotesManagementModel> PrepareProductNotesManagementModel(ProductNotesManagementModel model)
        //{
        //    model.EntityTypes.Add(
        //        new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem()
        //        {
        //            Text = await _localizationService.GetResourceAsync("admin.common.select"),
        //            Value = await _localizationService.GetResourceAsync("")
        //        }
        //       );
        //    model.EntityTypes.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem()
        //    {
        //        Text = "Category",
        //        Value = "Category"
        //    });

        //    model.EntityTypes.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem()
        //    {
        //        Text = "Product",
        //        Value = "Product"
        //    });
        //    return model;
        //}

        //#region Marketing

        //public async Task<MarketingModel> PrepareMarketingModel()
        //{
        //    var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        //    var marketingSettings = await _settingService.LoadSettingAsync<MarketingSettings>(storeScope);
        //    MarketingModel model = new MarketingModel();
        //    model.BuyMoreSaveMoreDiscountConfiguration = marketingSettings.BuyMoreSaveMoreDiscountConfiguration;
        //    model.EnableBuyMoreSaveMoreDiscount = marketingSettings.EnableBuyMoreSaveMoreDiscount;
        //    model.EnableEmailExclusiveOffer = marketingSettings.EnableEmailExclusiveOffer;
        //    model.ShowBuyMoreSaveMoreBanner = marketingSettings.ShowBuyMoreSaveMoreBanner;
        //    model.LimitedOfferContent = await _localizationService.GetResourceAsync(marketingSettings.LimitedOffer);
        //    model.BuyMoreSaveMoreContent = await _localizationService.GetResourceAsync(marketingSettings.BuyMoreSaveMoreContent);
        //    model.HeaderStripContent = await _localizationService.GetResourceAsync(marketingSettings.BuyMoreSaveMoreContent);
        //    var topic = await _topicService.GetTopicBySystemNameAsync(marketingSettings.HeaderStrip);
        //    if (topic != null)
        //        model.HeaderStripContent = topic.Body;
        //    return model;
        //}
        //public async Task SaveMarketingSettings(MarketingModel model)
        //{
        //    var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        //    var marketingSettings = await _settingService.LoadSettingAsync<MarketingSettings>(storeScope);
        //    marketingSettings.BuyMoreSaveMoreDiscountConfiguration = model.BuyMoreSaveMoreDiscountConfiguration;
        //    marketingSettings.EnableBuyMoreSaveMoreDiscount = model.EnableBuyMoreSaveMoreDiscount;
        //    marketingSettings.EnableEmailExclusiveOffer = model.EnableEmailExclusiveOffer;
        //    marketingSettings.ShowBuyMoreSaveMoreBanner = model.ShowBuyMoreSaveMoreBanner;

        //    await _settingService.SaveSettingOverridablePerStoreAsync(marketingSettings, x => x.BuyMoreSaveMoreDiscountConfiguration, true, storeScope, false);

        //    await _settingService.SaveSettingOverridablePerStoreAsync(marketingSettings, x => x.EnableBuyMoreSaveMoreDiscount, true, storeScope, false);
        //    await _settingService.SaveSettingOverridablePerStoreAsync(marketingSettings, x => x.EnableEmailExclusiveOffer, true, storeScope, false);
        //    await _settingService.SaveSettingOverridablePerStoreAsync(marketingSettings, x => x.ShowBuyMoreSaveMoreBanner, true, storeScope, false);


        //    var limitedTimeOfferResource = await _localizationService.GetLocaleStringResourceByNameAsync(marketingSettings.LimitedOffer, (await _workContext.GetWorkingLanguageAsync()).Id);
        //    if (limitedTimeOfferResource != null)
        //    {
        //        limitedTimeOfferResource.ResourceValue = model.LimitedOfferContent;
        //        await _localizationService.UpdateLocaleStringResourceAsync(limitedTimeOfferResource);
        //    }

        //    var buyMoreSaveMoreContentResource = await _localizationService.GetLocaleStringResourceByNameAsync(marketingSettings.BuyMoreSaveMoreContent, (await _workContext.GetWorkingLanguageAsync()).Id);
        //    if (buyMoreSaveMoreContentResource != null)
        //    {
        //        buyMoreSaveMoreContentResource.ResourceValue = model.BuyMoreSaveMoreContent;
        //        await _localizationService.UpdateLocaleStringResourceAsync(buyMoreSaveMoreContentResource);
        //    }

        //    var topic = await _topicService.GetTopicBySystemNameAsync(marketingSettings.HeaderStrip);
        //    if (topic != null)
        //    {
        //        topic.Body = model.HeaderStripContent;
        //        await _topicService.UpdateTopicAsync(topic);

        //    }
        //}

        //public async Task<StoreWideDiscountSettingModel> PrepareOfferModel()
        //{
        //    StoreWideDiscountSettingModel model = new StoreWideDiscountSettingModel();
        //    await _baseAdminModelFactory.PrepareCategoriesAsync(model.AvailableCategories, false);
        //    return model;
        //}

        //public async Task BulkUpdateCustomizationFormTemplates(string productids, string categoryIds, int customizationFormTemplateId)
        //{
        //    if (!string.IsNullOrEmpty(categoryIds))
        //    {
        //        var categories = await this._categoryService.GetCategoriesByIdsAsync(categoryIds.Split(new string[] { "," }, System.StringSplitOptions.RemoveEmptyEntries)
        //            .Select(Int32.Parse).ToArray());
        //        foreach (var category in categories)
        //        {
        //            category.CustomizationFormTemplateId = customizationFormTemplateId;
        //            await _categoryService.UpdateCategoryAsync(category);
        //        }
        //        await this._staticCacheManager.ClearAsync();
        //    }

        //    if (!string.IsNullOrEmpty(productids))
        //    {
        //        var products = await this._productService.GetProductsByIdsAsync(productids.Split(new string[] { "," }, System.StringSplitOptions.RemoveEmptyEntries)
        //            .Select(Int32.Parse).ToArray());
        //        foreach (var product in products)
        //        {
        //            product.CustomizationFormTemplateId = customizationFormTemplateId;
        //            await _productService.UpdateProductWithoutEvent(product);
        //        }
        //        await this._staticCacheManager.ClearAsync();
        //    }
        //}
        //#endregion


        //#region Bulk Update

        ///********************Product Listing Section***********************/
        //public async Task BulkUpdateProductListing(string productids, int productId, string productListingType)
        //{

        //    // VERIFY 
        //    var product = await _productService.GetProductByIdAsync(productId);

        //    if (product != null)
        //    {
        //        switch (productListingType)
        //        {
        //            case "People Also Viewed":
        //                var relatedProducts = from rp in _relatedProductRepository.Table
        //                                      where rp.ProductId1 == productId
        //                                      select rp;

        //                foreach (var relateproduct in relatedProducts)
        //                {
        //                    await _relatedProductRepository.DeleteAsync(relateproduct);
        //                }


        //                var products =
        //                productids.Split(new string[] { "," }, System.StringSplitOptions.RemoveEmptyEntries)
        //                    .Select(Int32.Parse).ToArray();

        //                for (int i = 0; i < products.Length; i++)

        //                {
        //                    if ((await _productService.GetProductByIdAsync(products[i])) != null)
        //                    {
        //                        await _relatedProductRepository.InsertAsync(new RelatedProduct()
        //                        {
        //                            ProductId1 = productId,
        //                            ProductId2 = products[i],
        //                            DisplayOrder = i + 1
        //                        });
        //                    }
        //                }

        //                break;
        //            case "FbtProducts":

        //                var fbtProduct = from fbtp in _fbtProductRepository.Table
        //                                 where fbtp.ProductId1 == productId
        //                                 select fbtp;

        //                foreach (var fbtProductItem in fbtProduct)
        //                {
        //                    await _fbtProductRepository.DeleteAsync(fbtProductItem);
        //                }


        //                var fbtProductItems =
        //                productids.Split(new string[] { "," }, System.StringSplitOptions.RemoveEmptyEntries)
        //                    .Select(Int32.Parse).ToArray();

        //                for (int i = 0; i < fbtProductItems.Length; i++)

        //                {
        //                    if ((await _productService.GetProductByIdAsync(fbtProductItems[i])) != null)
        //                    {


        //                        await _fbtProductRepository.InsertAsync(new FBTProduct()
        //                        {
        //                            ProductId1 = productId,
        //                            ProductId2 = fbtProductItems[i],
        //                            DisplayOrder = i + 1,
        //                            DefaultQuantity = 0
        //                        });
        //                    }
        //                }

        //                break;
        //            case "You May Also Like":
        //                var crosssellProduct = from crsslpro in _crossSellProductRepository.Table
        //                                       where crsslpro.ProductId1 == productId
        //                                       select crsslpro;

        //                foreach (var crosssellProductItem in crosssellProduct)
        //                {
        //                    await _crossSellProductRepository.DeleteAsync(crosssellProductItem);
        //                }


        //                var crosssellProductItems =
        //                productids.Split(new string[] { "," }, System.StringSplitOptions.RemoveEmptyEntries)
        //                    .Select(Int32.Parse).ToArray();

        //                for (int i = 0; i < crosssellProductItems.Length; i++)

        //                {
        //                    if ((await _productService.GetProductByIdAsync(crosssellProductItems[i])) != null)
        //                    {


        //                        await _crossSellProductRepository.InsertAsync(new CrossSellProduct()
        //                        {
        //                            ProductId1 = productId,
        //                            ProductId2 = crosssellProductItems[i],

        //                        });

        //                    }
        //                }
        //                break;
        //            case "Pick Products You Like":
        //                var collectionProduct = from clpro in _collectionProductRepository.Table
        //                                        where clpro.ProductId1 == productId
        //                                        select clpro;

        //                foreach (var collectionProductItem in collectionProduct)
        //                {
        //                    await _collectionProductRepository.DeleteAsync(collectionProductItem);
        //                }


        //                var collectionProductItems =
        //                productids.Split(new string[] { "," }, System.StringSplitOptions.RemoveEmptyEntries)
        //                    .Select(Int32.Parse).ToArray();

        //                for (int i = 0; i < collectionProductItems.Length; i++)

        //                {

        //                    if ((await _productService.GetProductByIdAsync(collectionProductItems[i])) != null)
        //                    {
        //                        await _collectionProductRepository.InsertAsync(new CollectionProduct()
        //                        {
        //                            ProductId1 = productId,
        //                            ProductId2 = collectionProductItems[i],
        //                            DisplayOrder = i + 1

        //                        });
        //                    }
        //                }
        //                break;

        //            case "Pair With Products":
        //                var pairWithProduct = from pwp in _pairWithProductRepository.Table
        //                                      where pwp.ProductId1 == productId
        //                                      select pwp;

        //                foreach (var pairWithProductItem in pairWithProduct)
        //                {
        //                    await _pairWithProductRepository.DeleteAsync(pairWithProductItem);
        //                }


        //                var pairWithProductItems =
        //                productids.Split(new string[] { "," }, System.StringSplitOptions.RemoveEmptyEntries)
        //                    .Select(Int32.Parse).ToArray();

        //                for (int i = 0; i < pairWithProductItems.Length; i++)

        //                {
        //                    if ((await _productService.GetProductByIdAsync(pairWithProductItems[i])) != null)
        //                    {

        //                        await _pairWithProductRepository.InsertAsync(new PairWithProduct()
        //                        {
        //                            ProductId1 = productId,
        //                            ProductId2 = pairWithProductItems[i],
        //                            DisplayOrder = i + 1

        //                        });
        //                    }
        //                }
        //                break;

        //        }

        //    }

        //}
        ///***********************End*****************************************************/
        //public async Task BulkUpdateInventory(string productids, int inventory)
        //{
        //    var products = await this._productService.GetProductsByIdsAsync(productids.Split(new string[] { "," }, System.StringSplitOptions.RemoveEmptyEntries)
        //           .Select(Int32.Parse).ToArray());

        //    foreach (var product in products)
        //    {
        //        var combinations = await _productAttributeService.CustomGetAllProductAttributeCombinationsAsync(product.Id);
        //        foreach (var combination in combinations)
        //        {
        //            combination.StockQuantity = inventory;
        //            await _productAttributeService.CustomUpdateProductAttributeCombinationAsync(combination);
        //        }


        //        product.StockQuantity = inventory;
        //        product.TotalInventory = inventory;
        //        await _productService.UpdateProductAsync(product);
        //    }

        //}
        //public async Task BulkUpdateTemplate(string categoryIds, int productTemplateId)
        //{
        //    if (!string.IsNullOrEmpty(categoryIds))
        //    {
        //        var categories = await this._categoryService.GetCategoriesByIdsAsync(categoryIds.Split(new string[] { "," }, System.StringSplitOptions.RemoveEmptyEntries)
        //            .Select(Int32.Parse).ToArray());
        //        foreach (var category in categories)
        //        {

        //            var products = await _productService.SearchProductsAsync(pageIndex: 0, pageSize: int.MaxValue, categoryIds: new List<int>() { category.Id }, showHidden: true);
        //            foreach (var product in products)
        //            {
        //                product.ProductTemplateId = productTemplateId;
        //                await _productService.UpdateProductWithoutEvent(product);
        //            }

        //        }
        //        await this._staticCacheManager.ClearAsync();
        //    }
        //}
        //public async Task BulkUpdateProductCategorySpecificationAttributeMapping(string productids, string categoryIds, string specificationAttributeIds, bool isRemove)
        //{
        //    if (!string.IsNullOrEmpty(categoryIds))
        //    {
        //        var categories = await this._categoryService.GetCategoriesByIdsAsync(categoryIds.Split(new string[] { "," }, System.StringSplitOptions.RemoveEmptyEntries)
        //           .Select(Int32.Parse).ToArray());

        //        foreach (var category in categories)
        //        {
        //            foreach (int productId in productids.Split(new string[] { "," }, System.StringSplitOptions.RemoveEmptyEntries)
        //           .Select(Int32.Parse).ToArray())
        //            {
        //                var productCategories = await _categoryService.GetProductCategoriesByProductIdAsync(productId);
        //                if (isRemove)
        //                {
        //                    var productCategory = productCategories.Where(pc => pc.CategoryId == category.Id).FirstOrDefault();
        //                    if (productCategory != null)
        //                        await _categoryService.DeleteProductCategoryAsync(productCategory);
        //                }
        //                else
        //                {
        //                    var productCategory = productCategories.Where(pc => pc.CategoryId == category.Id).FirstOrDefault();
        //                    if (productCategory == null)
        //                    {
        //                        var product = await _productService.GetProductByIdAsync(productId);
        //                        if (product != null)
        //                            await _categoryService.InsertProductCategoryAsync(new Core.Domain.Catalog.ProductCategory()
        //                            {
        //                                CategoryId = category.Id,
        //                                ProductId = productId,
        //                                IsFeaturedProduct = false,
        //                                DisplayOrder = 10001
        //                            });
        //                    }
        //                }
        //            }


        //        }

        //    }

        //    if (!string.IsNullOrEmpty(specificationAttributeIds))
        //    {
        //        var specificationAttributesOptions = await this._specificationAttributeService.GetSpecificationAttributeOptionsByIdsAsync(specificationAttributeIds.Split(new string[] { "," }, System.StringSplitOptions.RemoveEmptyEntries)
        //           .Select(Int32.Parse).ToArray());

        //        foreach (var specificationAttributesOption in specificationAttributesOptions)
        //        {
        //            var specificationAttribute = await _specificationAttributeService.GetSpecificationAttributeByIdAsync(specificationAttributesOption.SpecificationAttributeId);
        //            if (specificationAttribute == null || specificationAttribute.SpecificationAttributeGroupId == null)
        //                continue;
        //            var specificationAttributeGroup = await _specificationAttributeService.GetSpecificationAttributeGroupByIdAsync(Convert.ToInt32(specificationAttribute.SpecificationAttributeGroupId));
        //            if (specificationAttributeGroup == null)
        //                continue;
        //            if (specificationAttributeGroup.Id != await _settingService.GetSettingByKeyAsync<int>("specification.product.filters"))
        //                continue;
        //            foreach (int productId in productids.Split(new string[] { "," }, System.StringSplitOptions.RemoveEmptyEntries)
        //           .Select(Int32.Parse).ToArray())
        //            {
        //                var mappings = await _specificationAttributeService.GetProductSpecificationAttributeBySpecificationAttributeOptionIdAndProductIdAsync(specificationAttributesOption.Id, productId);
        //                if (isRemove)
        //                {

        //                    foreach (var mapping in mappings)
        //                        await _specificationAttributeService.DeleteProductSpecificationAttributeAsync(mapping);
        //                }
        //                else
        //                {

        //                    if (!mappings.Any())
        //                    {
        //                        var product = await _productService.GetProductByIdAsync(productId);
        //                        if (product != null)
        //                            await _specificationAttributeService.InsertProductSpecificationAttributeAsync(new Core.Domain.Catalog.ProductSpecificationAttribute()
        //                            {
        //                                AllowFiltering = true,
        //                                AttributeType = SpecificationAttributeType.Option,
        //                                DisplayOrder = 10001,
        //                                ProductId = productId,
        //                                AttributeTypeId = 0,
        //                                ShowOnProductPage = false,
        //                                SpecificationAttributeOptionId = specificationAttributesOption.Id,

        //                            });
        //                    }
        //                }
        //            }


        //        }

        //    }
        //}

        //public async Task BulkPublishUnpublishProductCategories(string productids, string categoryIds, string specificationAttributeIds, bool publish)
        //{
        //    if (!string.IsNullOrEmpty(productids))
        //    {
        //        var products = await this._productService.GetProductsByIdsAsync(productids.Split(new string[] { "," }, System.StringSplitOptions.RemoveEmptyEntries)
        //            .Select(Int32.Parse).ToArray());
        //        foreach (var product in products)
        //        {
        //            product.Published = publish;
        //            await _productService.UpdateProductAsync(product);
        //        }
        //        #region Clear Cache of Child Section

        //        await _staticCacheManager.RemoveByPrefixAsync(NopCatalogDefaults.CollectionPrefix);
        //        await _staticCacheManager.RemoveByPrefixAsync(NopCatalogDefaults.PairWithPrefix);
        //        await _staticCacheManager.RemoveByPrefixAsync(NopCatalogDefaults.FBTPrefix);
        //        await _staticCacheManager.RemoveByPrefixAsync(NopModelCacheDefaults.FBTPrefix);
        //        await _staticCacheManager.RemoveByPrefixAsync(NopCatalogDefaults.RelatedPrefix);


        //        #endregion
        //    }
        //    if (!string.IsNullOrEmpty(categoryIds))
        //    {
        //        var categories = await this._categoryService.GetCategoriesByIdsAsync(categoryIds.Split(new string[] { "," }, System.StringSplitOptions.RemoveEmptyEntries)
        //            .Select(Int32.Parse).ToArray());
        //        foreach (var category in categories)
        //        {
        //            category.Published = publish;
        //            await _categoryService.UpdateCategoryAsync(category);
        //        }
        //    }
        //}

        //#endregion


        //#region  StoreWideDiscount
        //public async Task<StoreWideDiscountSearchModel> PrepareStoreWideDiscountSearchModel(StoreWideDiscountSearchModel searchModel)
        //{
        //    if (searchModel == null)
        //        throw new ArgumentNullException(nameof(searchModel));

        //    //prepare page parameters
        //    searchModel.SetGridPageSize();
        //    return searchModel;
        //}
        //public async Task<StoreWideDiscountListModel> PrepareStoreWideDiscountListModelAsync(StoreWideDiscountSearchModel searchModel)
        //{
        //    var storeWideDiscountList = await _storeWideDiscountService.GetAllStoreWideDiscountAsync(searchModel.Page - 1, pageSize: searchModel.PageSize);

        //    var model = await new StoreWideDiscountListModel().PrepareToGridAsync(searchModel, storeWideDiscountList, () =>
        //    {
        //        return storeWideDiscountList.SelectAwait(async storeWideDiscount =>
        //        {
        //            var storeWideDiscountModel = storeWideDiscount.ToModel<StoreWideDiscountModel>();
        //            if (storeWideDiscount.CreatedBy != 0)
        //            {
        //                var customer = await _customerService.GetCustomerByIdAsync(storeWideDiscount.CreatedBy);
        //                if (customer != null)
        //                {
        //                    storeWideDiscountModel.Created_By = await _customerService.GetCustomerFullNameAsync(customer);
        //                }
        //            }
        //            return storeWideDiscountModel;
        //        });
        //    });
        //    return model;
        //}

        //public async Task<StoreWideDiscountModel> PrepareStoreWideDiscountModel(StoreWideDiscountModel model, StoreWideDiscount storeWideDiscount)
        //{
        //    if (model == null)
        //    {
        //        model = new StoreWideDiscountModel();
        //    }
        //    if (storeWideDiscount.Id != 0)
        //    {
        //        model = storeWideDiscount.ToModel<StoreWideDiscountModel>();
        //    }
        //    model.StoreWideDiscountSettingSearchModel = new StoreWideDiscountSettingSearchModel();
        //    model.StoreWideDiscountSettingSearchModel.SetGridPageSize();

        //    return model;
        //}
        //#endregion

        //#region  StoreWideDiscountSetting

        //public async Task<StoreWideDiscountSettingListModel> PrepareStoreWideDiscountSettingListModelAsync(StoreWideDiscountSettingSearchModel searchModel)
        //{
        //    var storeWiseDiscountSettings = await _storeWideDiscountService.GetAllStoreWideDiscountSettingAsync(searchModel.StoreWideDiscountId, searchModel.Page - 1, pageSize: searchModel.PageSize);

        //    var model = await new StoreWideDiscountSettingListModel().PrepareToGridAsync(searchModel, storeWiseDiscountSettings, () =>
        //    {
        //        return storeWiseDiscountSettings.SelectAwait(async storeWiseDiscountSetting =>
        //        {
        //            var storeWidesettingModel = storeWiseDiscountSetting.ToModel<StoreWideDiscountSettingModel>();
        //            return storeWidesettingModel;
        //        });
        //    });
        //    return model;
        //}

        //public async Task<StoreWideDiscountSettingModel> PrepareStoreWideDiscountSettingModel(StoreWideDiscountSettingModel model, StoreWideDiscountSetting storeWideDiscountSetting)
        //{
        //    if (model == null)
        //    {
        //        model = new StoreWideDiscountSettingModel();
        //    }
        //    if (storeWideDiscountSetting?.Id != 0)
        //    {
        //        model = storeWideDiscountSetting.ToModel<StoreWideDiscountSettingModel>();
        //        foreach (var category in model.CategoryIds?.Split(',', StringSplitOptions.RemoveEmptyEntries))
        //        {
        //            model.SelectedCategoryIds.Add(Convert.ToInt32(category));
        //        }
        //    }
        //    await _baseAdminModelFactory.PrepareCategoriesAsync(model.AvailableCategories);
        //    return model;
        //}
        //#endregion

        //#region  StoreWideProductDiscountInfo
        //public async Task<StoreWideProductDiscountInfoSearchModel> PrepareOfferDiscountLogSearchModel(StoreWideProductDiscountInfoSearchModel searchModel)
        //{
        //    if (searchModel == null)
        //        throw new ArgumentNullException(nameof(searchModel));

        //    await _baseAdminModelFactory.PrepareCategoriesAsync(searchModel.AvailableCategories);
        //    //prepare page parameters
        //    searchModel.SetGridPageSize();
        //    return searchModel;
        //}

        //public async Task<StoreWideProductDiscountInfoListModel> PrepareOfferDiscountListModelAsync(StoreWideProductDiscountInfoSearchModel searchModel)
        //{
        //    #region Validate productids

        //    List<int> lstProductIds = new List<int>();
        //    if (!string.IsNullOrEmpty(searchModel.SearchProductIds))
        //    {
        //        foreach (var productid in searchModel.SearchProductIds.Split(','))
        //        {
        //            int.TryParse(productid.Trim(), out int _productId);
        //            if (_productId != 0)
        //            {
        //                lstProductIds.Add(_productId);
        //            }
        //        }
        //    }
        //    #endregion
        //    var offerDiscountLogList = await _storeWideDiscountService.SearchStoreWideProductDiscountInfo(searchModel.SearchCategoryId, lstProductIds,
        //  searchModel.Page - 1, pageSize: searchModel.PageSize);

        //    var model = await new StoreWideProductDiscountInfoListModel().PrepareToGridAsync(searchModel, offerDiscountLogList, () =>
        //    {
        //        return offerDiscountLogList.SelectAwait(async offerDiscountLog =>
        //        {
        //            StoreWideProductDiscountInfoModel offerDiscountLogModel = new StoreWideProductDiscountInfoModel();
        //            offerDiscountLogModel.ProductId = offerDiscountLog.ProductId;
        //            offerDiscountLogModel.Discount = offerDiscountLog.Discount;
        //            offerDiscountLogModel.InfoText = offerDiscountLog.InfoText;
        //            offerDiscountLogModel.InfoHelpText = offerDiscountLog.InfoHelpText;
        //            offerDiscountLogModel.CreatedOn = offerDiscountLog.CreatedOn;
        //            offerDiscountLogModel.UpdatedOn = offerDiscountLog.UpdatedOn;
        //            //fill in additional values (not existing in the entity)

        //            return offerDiscountLogModel;
        //        });
        //    });
        //    return model;
        //}


        //#endregion

        //#region  StoreWideProductDiscountHistory
        //public async Task<StoreWideProductDiscountHistorySearchModel> PrepareProductDiscountHistorySearchModel(StoreWideProductDiscountHistorySearchModel searchModel)
        //{
        //    if (searchModel == null)
        //        throw new ArgumentNullException(nameof(searchModel));

        //    //prepare page parameters
        //    searchModel.SetGridPageSize();
        //    return searchModel;
        //}
        //public async Task<StoreWideProductDiscountHistoryListModel> PrepareOfferProductDiscountHistoryListModelAsync(StoreWideProductDiscountHistorySearchModel searchModel)
        //{
        //    #region Validate productids

        //    #endregion
        //    var offerProductDiscountLogList = await _storeWideDiscountService.SearchProductDiscountLog(searchModel.SearchProductId,
        //  searchModel.Page - 1, pageSize: searchModel.PageSize);

        //    var model = await new StoreWideProductDiscountHistoryListModel().PrepareToGridAsync(searchModel, offerProductDiscountLogList, () =>
        //    {
        //        return offerProductDiscountLogList.SelectAwait(async offerDiscountLog =>
        //        {
        //            StoreWideProductDiscountHistoryModel offerDiscountLogModel = new StoreWideProductDiscountHistoryModel();
        //            offerDiscountLogModel.ProductId = offerDiscountLog.ProductId;
        //            offerDiscountLogModel.Discount = offerDiscountLog.Discount;
        //            offerDiscountLogModel.InfoText = offerDiscountLog.InfoText;
        //            offerDiscountLogModel.InfoHelpText = offerDiscountLog.InfoHelpText;
        //            offerDiscountLogModel.CreatedOn = offerDiscountLog.CreatedOn;
        //            offerDiscountLogModel.UpdatedBy = await this._customerService.GetCustomerEmail(await _customerService.GetCustomerByIdAsync(offerDiscountLog.CustomerId));
        //            var customer = new Customer();

        //            if (offerDiscountLog.CustomerId != 0)
        //            {
        //                customer = await _customerService.GetCustomerByIdAsync(offerDiscountLog.CustomerId);
        //            }

        //            if (customer != null)
        //            {
        //                offerDiscountLogModel.UpdatedBy = await this._customerService.GetCustomerEmail(customer);
        //            }
        //            offerDiscountLogModel.Name = offerDiscountLog.StoreWideDiscountId == 0 ? "" : (await _storeWideDiscountService.GetStoreWideDiscountByIdAsync(offerDiscountLog.StoreWideDiscountId))?.Name ?? "";
        //            return offerDiscountLogModel;
        //        });
        //    });
        //    return model;
        //}

        //#endregion


        #region Testimonials
        public virtual async Task<TestimonialListModel> SearchTestimonials(TestimonialSearchModel searchModel)
        {
            try
            {
                var testimonials = await _testimonialService.SearchTestimonials(searchModel.OrderId, searchModel.Page, searchModel.PageSize);
                return await new TestimonialListModel().PrepareToGridAsync(searchModel, testimonials, () =>
                  {
                      return testimonials.SelectAwait(async x =>
                      {
                          string customerName = string.Empty;
                          string customerEmail = string.Empty;
                          var order = await this._orderService.GetOrderByIdAsync(x.OrderId);
                          if (order != null)
                          {
                              var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
                              if (customer != null)
                              {
                                  customerName = await _customerService.GetCustomerFullNameAsync(customer);
                                  customerEmail = await _customerService.GetCustomerEmail(customer);
                              }


                          }

                          var model = new TestimonialModel
                          {
                              Id = x.Id,
                              OrderId = x.OrderId,
                              CustomerEmail = customerEmail,
                              CustomerName = customerName,
                              CreatedOn = x.CreatedOn,
                              VideoUrl = x.VideoUrl,
                              IsConsent = x.IsConsent
                          };
                          return model;
                      });
                  });

            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion


        #endregion
    }
}

