using LinqToDB.Data;
using Microsoft.Extensions.Primitives;
using MWT.Nop.Core.Domain;
using MWT.Nop.Core.Domain.Catalog;
using MWT.Nop.Core.Domain.KW;
using MWT.Nop.Core.Domain.QA;
using MWT.Nop.Core.Domain.Security;
using MWT.Nop.Core.Domain.StoreWideDiscount;
using MWT.Nop.Core.Service.Discount;
using MWT.Nop.Core.Services.Catalog;
using MWTNop.Core.Domain.Catalog;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customization.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Shipping;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Customization.Extensions;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping.Date;
using Nop.Services.Stores;
using Nop.Services.Vendors;


namespace MWT.Nop.Core.Service.Catalog
{

    public partial class CustomProductService : ProductService, ICustomProductService
    {
        #region Fields

        private readonly ICustomProductAttributeParser _customProductAttributeParser;
        private readonly ICustomSpecificationAttributeService _customSpecificationAttributeService;
        private readonly ICustomCategoryService _customCategoryService;
        private readonly ICustomProductAttributeService _customProductAttributeService;
        private readonly IRepository<ProductBasicInfo> _productInfoRepository;
        private readonly IRepository<CategoryUserMapping> _categoryUserMappingRepository;
        private readonly IRepository<ProductKwTerm> _productKwTermRepository;
        private readonly IRepository<ProductQuestionAnswer> _productQuestionAnswerRepository;
        private readonly IRepository<VariantCombination> _variantcombinationRepository;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IStoreWideDiscountService _storeWideDiscountService;
        private readonly IDownloadService _downloadService;
        private readonly IRepository<ProductVariant> _productVariantRepository;
        private readonly IRepository<VariantCombinationLog> _variantCombinationLogRepository;
        private readonly IRepository<CollectionProduct> _collectionRepository;
        private readonly IRepository<PairWithProduct> _pairwithRepository;
        private readonly IRepository<FBTProduct> _fbtProductepository; 
        private readonly IPriceFormatter _priceFormatter;
        private readonly IStoreContext _storeContext;
        private readonly ILogger _loggerService;



        #endregion

        #region Ctor
        public CustomProductService(CatalogSettings catalogSettings, IAclService aclService, ICustomerService customerService, IDateRangeService dateRangeService, ILanguageService languageService, ILocalizationService localizationService, IProductAttributeParser productAttributeParser, IProductAttributeService productAttributeService, IRepository<Category> categoryRepository, IRepository<CrossSellProduct> crossSellProductRepository, IRepository<DiscountProductMapping> discountProductMappingRepository, IRepository<LocalizedProperty> localizedPropertyRepository, IRepository<Manufacturer> manufacturerRepository, IRepository<Product> productRepository, IRepository<ProductAttributeCombination> productAttributeCombinationRepository, IRepository<ProductAttributeMapping> productAttributeMappingRepository, IRepository<ProductCategory> productCategoryRepository, IRepository<ProductManufacturer> productManufacturerRepository, IRepository<ProductPicture> productPictureRepository, IRepository<ProductProductTagMapping> productTagMappingRepository, IRepository<ProductSpecificationAttribute> productSpecificationAttributeRepository, IRepository<ProductTag> productTagRepository, IRepository<ProductVideo> productVideoRepository, IRepository<ProductWarehouseInventory> productWarehouseInventoryRepository, IRepository<RelatedProduct> relatedProductRepository, IRepository<Shipment> shipmentRepository, IRepository<StockQuantityHistory> stockQuantityHistoryRepository, IRepository<TierPrice> tierPriceRepository, ISearchPluginManager searchPluginManager, IStaticCacheManager staticCacheManager, IVendorService vendorService, IStoreMappingService storeMappingService, IWorkContext workContext, LocalizationSettings localizationSettings
            , INopDataProvider dataProvider, ICustomProductAttributeParser customProductAttributeParser, ICustomSpecificationAttributeService customSpecificationAttributeService,
            ICustomCategoryService customCategoryService, ICustomProductAttributeService customProductAttributeService,
            IRepository<ProductBasicInfo> productInfoRepository, IRepository<CategoryUserMapping> categoryUserMappingRepository, IRepository<ProductKwTerm> productKwTermRepository,
            IRepository<ProductQuestionAnswer> productQuestionAnswerRepository, IRepository<VariantCombination> variantcombinationRepository, IUrlRecordService urlRecordService,
            IStoreWideDiscountService storeWideDiscountService, IDownloadService downloadService, IRepository<ProductVariant> productVariantRepository,
             IRepository<VariantCombinationLog> variantCombinationLogRepository, IRepository<CollectionProduct> collectionRepository,
            IRepository<PairWithProduct> pairwithRepository, IRepository<FBTProduct> fbtProductepository, IPriceFormatter priceFormatter, IStoreContext storeContext, ILogger loggerService) :
            base(catalogSettings, aclService, customerService, dateRangeService, languageService, localizationService, productAttributeParser, productAttributeService, categoryRepository, crossSellProductRepository, discountProductMappingRepository, localizedPropertyRepository, manufacturerRepository, productRepository, productAttributeCombinationRepository, productAttributeMappingRepository, productCategoryRepository, productManufacturerRepository, productPictureRepository, productTagMappingRepository, productSpecificationAttributeRepository, productTagRepository, productVideoRepository, productWarehouseInventoryRepository, relatedProductRepository, shipmentRepository, stockQuantityHistoryRepository, tierPriceRepository, searchPluginManager, staticCacheManager, vendorService, storeMappingService, workContext, localizationSettings)
        {

            this._customProductAttributeParser = customProductAttributeParser;
            this._customSpecificationAttributeService = customSpecificationAttributeService;
            this._customCategoryService = customCategoryService;
            this._customProductAttributeService = customProductAttributeService;
            this._productInfoRepository = productInfoRepository;
            this._categoryUserMappingRepository = categoryUserMappingRepository;
            this._productKwTermRepository = productKwTermRepository;
            this._productQuestionAnswerRepository = productQuestionAnswerRepository;
            this._variantcombinationRepository = variantcombinationRepository;
            this._urlRecordService = urlRecordService;
            this._storeWideDiscountService = storeWideDiscountService;
            this._downloadService = downloadService;
            this._productVariantRepository = productVariantRepository;
            this._variantCombinationLogRepository = variantCombinationLogRepository;
            this._collectionRepository = collectionRepository;
            this._pairwithRepository = pairwithRepository;
            this._fbtProductepository = fbtProductepository;
            this._priceFormatter = priceFormatter;
            this._storeContext = storeContext;
            this._loggerService = loggerService;
        }

        #endregion
        private static readonly SemaphoreSlim _semaphoreProductVariantIdGenerate = new SemaphoreSlim(1, 1);

        #region Product
        public async Task<IList<ProductBasicInfo>> GetAllProducts()
        {
            return await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(CustomNopCatalogDefaults.ProductsBasicInfoCacheKey), async () => await _productInfoRepository.EntityFromSqlAsync("Sp_Get_All_Products"));
        }
        public virtual async Task<IPagedList<Product>> OverriddenSearchProductsAsync(
                int pageIndex = 0,
                int pageSize = int.MaxValue,
                IList<int> categoryIds = null,
                IList<int> manufacturerIds = null,
                int storeId = 0,
                int vendorId = 0,
                int warehouseId = 0,
                ProductType? productType = null,
                bool visibleIndividuallyOnly = false,
                bool excludeFeaturedProducts = false,
                decimal? priceMin = null,
                decimal? priceMax = null,
                int productTagId = 0,
                string keywords = null,
                bool searchDescriptions = false,
                bool searchManufacturerPartNumber = true,
                bool searchSku = true,
                bool searchProductTags = false,
                int languageId = 0,
                IList<SpecificationAttributeOption> filteredSpecOptions = null,
                ProductSortingEnum orderBy = ProductSortingEnum.Position,
                bool showHidden = false,
                bool? overridePublished = null)
        {
            //some databases don't support int.MaxValue
            if (pageSize == int.MaxValue)
                pageSize = int.MaxValue - 1;

            var productsQuery = _productRepository.Table;

            if (!showHidden)
                productsQuery = productsQuery.Where(p => p.Published);
            else if (overridePublished.HasValue)
                productsQuery = productsQuery.Where(p => p.Published == overridePublished.Value);

            //apply store mapping constraints
            productsQuery = await _storeMappingService.ApplyStoreMapping(productsQuery, storeId);

            //apply ACL constraints
            if (!showHidden)
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                productsQuery = await _aclService.ApplyAcl(productsQuery, customer);
            }

            productsQuery =
                from p in productsQuery
                where !p.Deleted &&
                    (!visibleIndividuallyOnly || p.VisibleIndividually) &&
                    (vendorId == 0 || p.VendorId == vendorId) &&
                    (
                        warehouseId == 0 ||
                        (
                            !p.UseMultipleWarehouses ? p.WarehouseId == warehouseId :
                                _productWarehouseInventoryRepository.Table.Any(pwi => pwi.Id == warehouseId && pwi.ProductId == p.Id)
                        )
                    ) &&
                    (productType == null || p.ProductTypeId == (int)productType) &&
                    (showHidden || LinqToDB.Sql.Between(DateTime.UtcNow, p.AvailableStartDateTimeUtc ?? DateTime.MinValue, p.AvailableEndDateTimeUtc ?? DateTime.MaxValue)) &&
                    (priceMin == null || p.Price >= priceMin) &&
                    (priceMax == null || p.Price <= priceMax)
                select p;

            if (!string.IsNullOrEmpty(keywords))
            {
                var langs = await _languageService.GetAllLanguagesAsync(showHidden: true);

                //Set a flag which will to points need to search in localized properties. If showHidden doesn't set to true should be at least two published languages.
                var searchLocalizedValue = languageId > 0 && langs.Count >= 2 && (showHidden || langs.Count(l => l.Published) >= 2);

                IQueryable<int> productsByKeywords;

                productsByKeywords =
                        from p in _productRepository.Table
                        where p.Name.Contains(keywords) ||
                            (searchDescriptions &&
                                (p.ShortDescription.Contains(keywords) || p.FullDescription.Contains(keywords))) ||
                            (searchManufacturerPartNumber && p.ManufacturerPartNumber == keywords) ||
                            (searchSku && p.Sku.StartsWith(keywords)) ||
                            (p.Id.ToString() == keywords)
                        select p.Id;

                //search by SKU for ProductAttributeCombination
                if (searchSku)
                {
                    productsByKeywords = productsByKeywords.Union(
                        from pac in _productAttributeCombinationRepository.Table
                        where pac.Sku == keywords
                        select pac.ProductId);
                }

                if (searchProductTags)
                {
                    productsByKeywords = productsByKeywords.Union(
                        from pptm in _productTagMappingRepository.Table
                        join pt in _productTagRepository.Table on pptm.ProductTagId equals pt.Id
                        where pt.Name == keywords
                        select pptm.ProductId
                    );

                    if (searchLocalizedValue)
                    {
                        productsByKeywords = productsByKeywords.Union(
                        from pptm in _productTagMappingRepository.Table
                        join lp in _localizedPropertyRepository.Table on pptm.ProductTagId equals lp.EntityId
                        where lp.LocaleKeyGroup == nameof(ProductTag) &&
                              lp.LocaleKey == nameof(ProductTag.Name) &&
                              lp.LocaleValue.Contains(keywords)
                        select lp.EntityId);
                    }
                }

                if (searchLocalizedValue)
                {
                    productsByKeywords = productsByKeywords.Union(
                                from lp in _localizedPropertyRepository.Table
                                let checkName = lp.LocaleKey == nameof(Product.Name) &&
                                                lp.LocaleValue.Contains(keywords)
                                let checkShortDesc = searchDescriptions &&
                                                lp.LocaleKey == nameof(Product.ShortDescription) &&
                                                lp.LocaleValue.Contains(keywords)
                                let checkProductTags = searchProductTags &&
                                                lp.LocaleKeyGroup == nameof(ProductTag) &&
                                                lp.LocaleKey == nameof(ProductTag.Name) &&
                                                lp.LocaleValue.Contains(keywords)
                                where
                                    lp.LocaleKeyGroup == nameof(Product) && lp.LanguageId == languageId && (checkName || checkShortDesc) ||
                                    checkProductTags

                                select lp.EntityId);
                }

                productsQuery =
                    from p in productsQuery
                    from pbk in LinqToDB.LinqExtensions.InnerJoin(productsByKeywords, pbk => pbk == p.Id)
                    select p;
            }

            if (categoryIds is not null)
            {
                if (categoryIds.Contains(0))
                    categoryIds.Remove(0);

                if (categoryIds.Any())
                {
                    var productCategoryQuery =
                        from pc in _productCategoryRepository.Table
                        where (!excludeFeaturedProducts || !pc.IsFeaturedProduct) &&
                            categoryIds.Contains(pc.CategoryId)
                        group pc by pc.ProductId into pc
                        select new
                        {
                            ProductId = pc.Key,
                            DisplayOrder = pc.First().DisplayOrder
                        };

                    productsQuery =
                        from p in productsQuery
                        join pc in productCategoryQuery on p.Id equals pc.ProductId
                        orderby pc.DisplayOrder, p.Name
                        select p;
                }
            }

            if (manufacturerIds is not null)
            {
                if (manufacturerIds.Contains(0))
                    manufacturerIds.Remove(0);

                if (manufacturerIds.Any())
                {
                    var productManufacturerQuery =
                        from pm in _productManufacturerRepository.Table
                        where (!excludeFeaturedProducts || !pm.IsFeaturedProduct) &&
                            manufacturerIds.Contains(pm.ManufacturerId)
                        group pm by pm.ProductId into pm
                        select new
                        {
                            ProductId = pm.Key,
                            DisplayOrder = pm.First().DisplayOrder
                        };

                    productsQuery =
                        from p in productsQuery
                        join pm in productManufacturerQuery on p.Id equals pm.ProductId
                        orderby pm.DisplayOrder, p.Name
                        select p;
                }
            }

            if (productTagId > 0)
            {
                productsQuery =
                    from p in productsQuery
                    join ptm in _productTagMappingRepository.Table on p.Id equals ptm.ProductId
                    where ptm.ProductTagId == productTagId
                    select p;
            }

            if (filteredSpecOptions?.Count > 0)
            {
                var specificationAttributeIds = filteredSpecOptions
                    .Select(sao => sao.SpecificationAttributeId)
                    .Distinct();

                foreach (var specificationAttributeId in specificationAttributeIds)
                {
                    var optionIdsBySpecificationAttribute = filteredSpecOptions
                        .Where(o => o.SpecificationAttributeId == specificationAttributeId)
                        .Select(o => o.Id);

                    var productSpecificationQuery =
                        from psa in _productSpecificationAttributeRepository.Table
                        where psa.AllowFiltering && optionIdsBySpecificationAttribute.Contains(psa.SpecificationAttributeOptionId)
                        select psa;

                    productsQuery =
                        from p in productsQuery
                        where productSpecificationQuery.Any(pc => pc.ProductId == p.Id)
                        select p;
                }
            }

            return await productsQuery.OrderBy(_localizedPropertyRepository, await _workContext.GetWorkingLanguageAsync(), orderBy).ToPagedListAsync(pageIndex, pageSize);
        }

        public virtual async Task<IPagedList<Product>> GetAccessibleSearchProductsAsync(
      int pageIndex = 0,
      int pageSize = int.MaxValue,
      int customerId = 0,
      IList<int> categoryIds = null,
      IList<int> manufacturerIds = null,
      int storeId = 0,
      int vendorId = 0,
      int warehouseId = 0,
      ProductType? productType = null,
      bool visibleIndividuallyOnly = false,
      bool excludeFeaturedProducts = false,
      decimal? priceMin = null,
      decimal? priceMax = null,
      int productTagId = 0,
      string keywords = null,
      bool searchDescriptions = false,
      bool searchManufacturerPartNumber = true,
      bool searchSku = true,
      bool searchProductTags = false,
      int languageId = 0,
      IList<SpecificationAttributeOption> filteredSpecOptions = null,
      ProductSortingEnum orderBy = ProductSortingEnum.Position,
      bool showHidden = false,
      bool? overridePublished = null)
        {
            //some databases don't support int.MaxValue
            if (pageSize == int.MaxValue)
                pageSize = int.MaxValue - 1;

            var productsQuery = _productRepository.Table;

            if (!showHidden)
                productsQuery = productsQuery.Where(p => p.Published);

            else if (overridePublished.HasValue)
                productsQuery = productsQuery.Where(p => p.Published == overridePublished.Value);

            //apply store mapping constraints
            productsQuery = await _storeMappingService.ApplyStoreMapping(productsQuery, storeId);

            //apply ACL constraints
            if (!showHidden)
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                productsQuery = await _aclService.ApplyAcl(productsQuery, customer);
            }


            var categoryAccessQuery =
                                from pc in _productCategoryRepository.Table
                                join catUserMapping in _categoryUserMappingRepository.Table
                                 on pc.CategoryId equals catUserMapping.CategoryId
                                where catUserMapping.UserId == customerId
                                group pc by pc.ProductId into pc
                                select new
                                {
                                    ProductId = pc.Key,
                                    DisplayOrder = pc.First().DisplayOrder
                                };

            productsQuery =
                        from p in productsQuery
                        join pc in categoryAccessQuery on p.Id equals pc.ProductId
                        orderby pc.DisplayOrder, p.Name
                        select p;
            productsQuery =
                from p in productsQuery
                where !p.Deleted &&
                    (!visibleIndividuallyOnly || p.VisibleIndividually) &&
                    (vendorId == 0 || p.VendorId == vendorId) &&
                    (
                        warehouseId == 0 ||
                        (
                            !p.UseMultipleWarehouses ? p.WarehouseId == warehouseId :
                                _productWarehouseInventoryRepository.Table.Any(pwi => pwi.Id == warehouseId && pwi.ProductId == p.Id)
                        )
                    ) &&
                    (productType == null || p.ProductTypeId == (int)productType) &&
                    (showHidden || LinqToDB.Sql.Between(DateTime.UtcNow, p.AvailableStartDateTimeUtc ?? DateTime.MinValue, p.AvailableEndDateTimeUtc ?? DateTime.MaxValue)) &&
                    (priceMin == null || p.Price >= priceMin) &&
                    (priceMax == null || p.Price <= priceMax)
                select p;

            if (!string.IsNullOrEmpty(keywords))
            {
                var langs = await _languageService.GetAllLanguagesAsync(showHidden: true);

                //Set a flag which will to points need to search in localized properties. If showHidden doesn't set to true should be at least two published languages.
                var searchLocalizedValue = languageId > 0 && langs.Count >= 2 && (showHidden || langs.Count(l => l.Published) >= 2);

                IQueryable<int> productsByKeywords;

                productsByKeywords =
                        from p in _productRepository.Table
                        where p.Name.Contains(keywords) ||
                            (searchDescriptions &&
                                (p.ShortDescription.Contains(keywords) || p.FullDescription.Contains(keywords))) ||
                            (searchManufacturerPartNumber && p.ManufacturerPartNumber == keywords) ||
                            (searchSku && p.Sku.StartsWith(keywords)) ||
                            (p.Id.ToString() == keywords)
                        select p.Id;

                //search by SKU for ProductAttributeCombination
                if (searchSku)
                {
                    productsByKeywords = productsByKeywords.Union(
                        from pac in _productAttributeCombinationRepository.Table
                        where pac.Sku == keywords
                        select pac.ProductId);
                }

                if (searchProductTags)
                {
                    productsByKeywords = productsByKeywords.Union(
                        from pptm in _productTagMappingRepository.Table
                        join pt in _productTagRepository.Table on pptm.ProductTagId equals pt.Id
                        where pt.Name == keywords
                        select pptm.ProductId
                    );

                    if (searchLocalizedValue)
                    {
                        productsByKeywords = productsByKeywords.Union(
                        from pptm in _productTagMappingRepository.Table
                        join lp in _localizedPropertyRepository.Table on pptm.ProductTagId equals lp.EntityId
                        where lp.LocaleKeyGroup == nameof(ProductTag) &&
                              lp.LocaleKey == nameof(ProductTag.Name) &&
                              lp.LocaleValue.Contains(keywords)
                        select lp.EntityId);
                    }
                }

                if (searchLocalizedValue)
                {
                    productsByKeywords = productsByKeywords.Union(
                                from lp in _localizedPropertyRepository.Table
                                let checkName = lp.LocaleKey == nameof(Product.Name) &&
                                                lp.LocaleValue.Contains(keywords)
                                let checkShortDesc = searchDescriptions &&
                                                lp.LocaleKey == nameof(Product.ShortDescription) &&
                                                lp.LocaleValue.Contains(keywords)
                                let checkProductTags = searchProductTags &&
                                                lp.LocaleKeyGroup == nameof(ProductTag) &&
                                                lp.LocaleKey == nameof(ProductTag.Name) &&
                                                lp.LocaleValue.Contains(keywords)
                                where
                                    lp.LocaleKeyGroup == nameof(Product) && lp.LanguageId == languageId && (checkName || checkShortDesc) ||
                                    checkProductTags

                                select lp.EntityId);
                }

                productsQuery =
                    from p in productsQuery
                    from pbk in LinqToDB.LinqExtensions.InnerJoin(productsByKeywords, pbk => pbk == p.Id)
                    select p;
            }

            if (categoryIds is not null)
            {
                if (categoryIds.Contains(0))
                    categoryIds.Remove(0);

                if (categoryIds.Any())
                {
                    var productCategoryQuery =
                        from pc in _productCategoryRepository.Table
                        where (!excludeFeaturedProducts || !pc.IsFeaturedProduct) &&
                            categoryIds.Contains(pc.CategoryId)
                        group pc by pc.ProductId into pc
                        select new
                        {
                            ProductId = pc.Key,
                            DisplayOrder = pc.First().DisplayOrder
                        };

                    productsQuery =
                        from p in productsQuery
                        join pc in productCategoryQuery on p.Id equals pc.ProductId
                        orderby pc.DisplayOrder, p.Name
                        select p;
                }
            }

            if (manufacturerIds is not null)
            {
                if (manufacturerIds.Contains(0))
                    manufacturerIds.Remove(0);

                if (manufacturerIds.Any())
                {
                    var productManufacturerQuery =
                        from pm in _productManufacturerRepository.Table
                        where (!excludeFeaturedProducts || !pm.IsFeaturedProduct) &&
                            manufacturerIds.Contains(pm.ManufacturerId)
                        group pm by pm.ProductId into pm
                        select new
                        {
                            ProductId = pm.Key,
                            DisplayOrder = pm.First().DisplayOrder
                        };

                    productsQuery =
                        from p in productsQuery
                        join pm in productManufacturerQuery on p.Id equals pm.ProductId
                        orderby pm.DisplayOrder, p.Name
                        select p;
                }
            }

            if (productTagId > 0)
            {
                productsQuery =
                    from p in productsQuery
                    join ptm in _productTagMappingRepository.Table on p.Id equals ptm.ProductId
                    where ptm.ProductTagId == productTagId
                    select p;
            }

            if (filteredSpecOptions?.Count > 0)
            {
                var specificationAttributeIds = filteredSpecOptions
                    .Select(sao => sao.SpecificationAttributeId)
                    .Distinct();

                foreach (var specificationAttributeId in specificationAttributeIds)
                {
                    var optionIdsBySpecificationAttribute = filteredSpecOptions
                        .Where(o => o.SpecificationAttributeId == specificationAttributeId)
                        .Select(o => o.Id);

                    var productSpecificationQuery =
                        from psa in _productSpecificationAttributeRepository.Table
                        where psa.AllowFiltering && optionIdsBySpecificationAttribute.Contains(psa.SpecificationAttributeOptionId)
                        select psa;

                    productsQuery =
                        from p in productsQuery
                        where productSpecificationQuery.Any(pc => pc.ProductId == p.Id)
                        select p;
                }
            }

            return await productsQuery.OrderBy(_localizedPropertyRepository, await _workContext.GetWorkingLanguageAsync(), orderBy).ToPagedListAsync(pageIndex, pageSize);
        }
        public async Task UpdateProductWithoutEvent(Product product)
        {
            await _productRepository.UpdateAsync(product, false);
        }
        public async Task<bool> IsNewAtcLayoutActive(Product product)
        {
            if (product.EnableNewATCLayout)
                return true;
            else
            {
                var categoryId = await this._customSpecificationAttributeService.GetMainCategoryOfProduct(product.Id);
                if (categoryId > 0)
                {
                    return (await _categoryRepository.GetByIdAsync(categoryId))?.EnableNewATCLayout ?? false;
                }
                return false;
            }
        }
        public virtual async Task<IPagedList<Product>> CustomSearchProductsAsync(
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            IList<int> categoryIds = null,
            IList<int> manufacturerIds = null,
            int storeId = 0,
            int vendorId = 0,
            int warehouseId = 0,
            ProductType? productType = null,
            bool visibleIndividuallyOnly = false,
            bool excludeFeaturedProducts = false,
            decimal? priceMin = null,
            decimal? priceMax = null,
            int productTagId = 0,
            string keywords = null,
            bool searchDescriptions = false,
            bool searchManufacturerPartNumber = true,
            bool searchSku = true,
            bool searchProductTags = false,
            int languageId = 0,
            IList<SpecificationAttributeOption> filteredSpecOptions = null,
            ProductSortingEnum orderBy = ProductSortingEnum.Position,
            bool showHidden = false,
            bool? overridePublished = null)
        {
            //some databases don't support int.MaxValue
            if (pageSize == int.MaxValue)
                pageSize = int.MaxValue - 1;

            var productsQuery = _productRepository.Table;

            if (!showHidden)
                productsQuery = productsQuery.Where(p => p.Published);
            else if (overridePublished.HasValue)
                productsQuery = productsQuery.Where(p => p.Published == overridePublished.Value);

            //apply store mapping constraints
            productsQuery = await _storeMappingService.ApplyStoreMapping(productsQuery, storeId);
            productsQuery = productsQuery.Where(p => p.TotalInventory > 0);
            productsQuery = productsQuery.Where(p => p.VisiblityOncategoryPage == null || p.VisiblityOncategoryPage == 1);
            //apply ACL constraints
            if (!showHidden)
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                productsQuery = await _aclService.ApplyAcl(productsQuery, customer);
            }

            productsQuery =
                from p in productsQuery
                where !p.Deleted &&
                    (!visibleIndividuallyOnly || p.VisibleIndividually) &&
                    (vendorId == 0 || p.VendorId == vendorId) &&
                    (
                        warehouseId == 0 ||
                        (
                            !p.UseMultipleWarehouses ? p.WarehouseId == warehouseId :
                                _productWarehouseInventoryRepository.Table.Any(pwi => pwi.Id == warehouseId && pwi.ProductId == p.Id)
                        )
                    ) &&
                    (productType == null || p.ProductTypeId == (int)productType) &&
                    (showHidden || LinqToDB.Sql.Between(DateTime.UtcNow, p.AvailableStartDateTimeUtc ?? DateTime.MinValue, p.AvailableEndDateTimeUtc ?? DateTime.MaxValue)) &&
                    (priceMin == null || p.Price >= priceMin) &&
                    (priceMax == null || p.Price <= priceMax)
                select p;

            if (!string.IsNullOrEmpty(keywords))
            {
                var langs = await _languageService.GetAllLanguagesAsync(showHidden: true);

                //Set a flag which will to points need to search in localized properties. If showHidden doesn't set to true should be at least two published languages.
                var searchLocalizedValue = languageId > 0 && langs.Count >= 2 && (showHidden || langs.Count(l => l.Published) >= 2);

                IQueryable<int> productsByKeywords;

                productsByKeywords =
                        from p in _productRepository.Table
                        where p.Name.Contains(keywords) ||
                            (searchDescriptions &&
                                (p.ShortDescription.Contains(keywords) || p.FullDescription.Contains(keywords))) ||
                            (searchManufacturerPartNumber && p.ManufacturerPartNumber == keywords) ||
                            (searchSku && p.Sku == keywords)
                        select p.Id;

                //search by SKU for ProductAttributeCombination
                if (searchSku)
                {
                    productsByKeywords = productsByKeywords.Union(
                        from pac in _productAttributeCombinationRepository.Table
                        where pac.Sku == keywords
                        select pac.ProductId);
                }

                if (searchProductTags)
                {
                    productsByKeywords = productsByKeywords.Union(
                        from pptm in _productTagMappingRepository.Table
                        join pt in _productTagRepository.Table on pptm.ProductTagId equals pt.Id
                        where pt.Name == keywords
                        select pptm.ProductId
                    );

                    if (searchLocalizedValue)
                    {
                        productsByKeywords = productsByKeywords.Union(
                        from pptm in _productTagMappingRepository.Table
                        join lp in _localizedPropertyRepository.Table on pptm.ProductTagId equals lp.EntityId
                        where lp.LocaleKeyGroup == nameof(ProductTag) &&
                              lp.LocaleKey == nameof(ProductTag.Name) &&
                              lp.LocaleValue.Contains(keywords)
                        select lp.EntityId);
                    }
                }

                if (searchLocalizedValue)
                {
                    productsByKeywords = productsByKeywords.Union(
                                from lp in _localizedPropertyRepository.Table
                                let checkName = lp.LocaleKey == nameof(Product.Name) &&
                                                lp.LocaleValue.Contains(keywords)
                                let checkShortDesc = searchDescriptions &&
                                                lp.LocaleKey == nameof(Product.ShortDescription) &&
                                                lp.LocaleValue.Contains(keywords)
                                let checkProductTags = searchProductTags &&
                                                lp.LocaleKeyGroup == nameof(ProductTag) &&
                                                lp.LocaleKey == nameof(ProductTag.Name) &&
                                                lp.LocaleValue.Contains(keywords)
                                where
                                    lp.LocaleKeyGroup == nameof(Product) && lp.LanguageId == languageId && (checkName || checkShortDesc) ||
                                    checkProductTags

                                select lp.EntityId);
                }

                productsQuery =
                    from p in productsQuery
                    from pbk in LinqToDB.LinqExtensions.InnerJoin(productsByKeywords, pbk => pbk == p.Id)
                    select p;
            }

            if (categoryIds is not null)
            {
                if (categoryIds.Contains(0))
                    categoryIds.Remove(0);

                if (categoryIds.Any())
                {
                    var productCategoryQuery =
                        from pc in _productCategoryRepository.Table
                        where (!excludeFeaturedProducts || !pc.IsFeaturedProduct) &&
                            categoryIds.Contains(pc.CategoryId)
                        group pc by pc.ProductId into pc
                        select new
                        {
                            ProductId = pc.Key,
                            DisplayOrder = pc.First().DisplayOrder
                        };

                    productsQuery =
                        from p in productsQuery
                        join pc in productCategoryQuery on p.Id equals pc.ProductId
                        orderby pc.DisplayOrder, p.Name
                        select p;
                }
            }

            if (manufacturerIds is not null)
            {
                if (manufacturerIds.Contains(0))
                    manufacturerIds.Remove(0);

                if (manufacturerIds.Any())
                {
                    var productManufacturerQuery =
                        from pm in _productManufacturerRepository.Table
                        where (!excludeFeaturedProducts || !pm.IsFeaturedProduct) &&
                            manufacturerIds.Contains(pm.ManufacturerId)
                        group pm by pm.ProductId into pm
                        select new
                        {
                            ProductId = pm.Key,
                            DisplayOrder = pm.First().DisplayOrder
                        };

                    productsQuery =
                        from p in productsQuery
                        join pm in productManufacturerQuery on p.Id equals pm.ProductId
                        orderby pm.DisplayOrder, p.Name
                        select p;
                }
            }

            if (productTagId > 0)
            {
                productsQuery =
                    from p in productsQuery
                    join ptm in _productTagMappingRepository.Table on p.Id equals ptm.ProductId
                    where ptm.ProductTagId == productTagId
                    select p;
            }

            if (filteredSpecOptions?.Count > 0)
            {
                var specificationAttributeIds = filteredSpecOptions
                    .Select(sao => sao.SpecificationAttributeId)
                    .Distinct();

                foreach (var specificationAttributeId in specificationAttributeIds)
                {
                    var optionIdsBySpecificationAttribute = filteredSpecOptions
                        .Where(o => o.SpecificationAttributeId == specificationAttributeId)
                        .Select(o => o.Id);

                    var productSpecificationQuery =
                        from psa in _productSpecificationAttributeRepository.Table
                        where psa.AllowFiltering && optionIdsBySpecificationAttribute.Contains(psa.SpecificationAttributeOptionId)
                        select psa;

                    productsQuery =
                        from p in productsQuery
                        where productSpecificationQuery.Any(pc => pc.ProductId == p.Id)
                        select p;
                }
            }

            return await productsQuery.CustomOrderBy(orderBy).ToPagedListAsync(pageIndex, pageSize);
        }

        public virtual async Task<List<ProductSpecificationAttribute>> CustomSearchGetProductSpecificationAttributeAsync(
               int pageIndex = 0,
           int pageSize = int.MaxValue,
           IList<int> categoryIds = null,
           IList<int> manufacturerIds = null,
           int storeId = 0,
           int vendorId = 0,
           int warehouseId = 0,
           ProductType? productType = null,
           bool visibleIndividuallyOnly = false,
           bool excludeFeaturedProducts = false,
           decimal? priceMin = null,
           decimal? priceMax = null,
           int productTagId = 0,
           string keywords = null,
           bool searchDescriptions = false,
           bool searchManufacturerPartNumber = true,
           bool searchSku = true,
           bool searchProductTags = false,
           int languageId = 0,
           IList<SpecificationAttributeOption> filteredSpecOptions = null,
           ProductSortingEnum orderBy = ProductSortingEnum.Position,
           bool showHidden = false,
           bool? overridePublished = null,
           bool showOutOfStock = false, int featuredId = 0, bool isMobileDevice = false, int kwTermId = 0, int questionAnswerId = 0)
        {

            var productsQuery = _productRepository.Table;
            if (featuredId != 0)
                productsQuery = productsQuery.Where(p => p.Id != featuredId);
            if (!showHidden)
                productsQuery = productsQuery.Where(p => p.Published);
            else if (overridePublished.HasValue)
                productsQuery = productsQuery.Where(p => p.Published == overridePublished.Value);

            if (!showOutOfStock)
                productsQuery = productsQuery.Where(p => p.TotalInventory >= 0);

            productsQuery = productsQuery.Where(p => p.VisiblityOncategoryPage == null || p.VisiblityOncategoryPage == 1);
            //apply store mapping constraints
            productsQuery = await _storeMappingService.ApplyStoreMapping(productsQuery, storeId);

            //apply ACL constraints
            if (!showHidden)
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                productsQuery = await _aclService.ApplyAcl(productsQuery, customer);
            }

            productsQuery =
                from p in productsQuery
                where !p.Deleted &&
                    (!visibleIndividuallyOnly || p.VisibleIndividually) &&
                    (vendorId == 0 || p.VendorId == vendorId) &&
                    (
                        warehouseId == 0 ||
                        (
                            !p.UseMultipleWarehouses ? p.WarehouseId == warehouseId :
                                _productWarehouseInventoryRepository.Table.Any(pwi => pwi.Id == warehouseId && pwi.ProductId == p.Id)
                        )
                    ) &&
                    (productType == null || p.ProductTypeId == (int)productType) &&
                    (showHidden || LinqToDB.Sql.Between(DateTime.UtcNow, p.AvailableStartDateTimeUtc ?? DateTime.MinValue, p.AvailableEndDateTimeUtc ?? DateTime.MaxValue)) &&
                    (priceMin == null || p.Price >= priceMin) &&
                    (priceMax == null || p.Price <= priceMax)
                select p;

            if (!string.IsNullOrEmpty(keywords))
            {
                var langs = await _languageService.GetAllLanguagesAsync(showHidden: true);

                //Set a flag which will to points need to search in localized properties. If showHidden doesn't set to true should be at least two published languages.
                var searchLocalizedValue = languageId > 0 && langs.Count >= 2 && (showHidden || langs.Count(l => l.Published) >= 2);

                IQueryable<int> productsByKeywords;

                productsByKeywords =
                        from p in _productRepository.Table
                        where p.Name.Contains(keywords) ||
                            (searchDescriptions &&
                                (p.ShortDescription.Contains(keywords) || p.FullDescription.Contains(keywords))) ||
                            (searchManufacturerPartNumber && p.ManufacturerPartNumber == keywords) ||
                            (searchSku && p.Sku == keywords)
                        select p.Id;

                //search by SKU for ProductAttributeCombination
                if (searchSku)
                {
                    productsByKeywords = productsByKeywords.Union(
                        from pac in _productAttributeCombinationRepository.Table
                        where pac.Sku == keywords
                        select pac.ProductId);
                }

                if (searchProductTags)
                {
                    productsByKeywords = productsByKeywords.Union(
                        from pptm in _productTagMappingRepository.Table
                        join pt in _productTagRepository.Table on pptm.ProductTagId equals pt.Id
                        where pt.Name == keywords
                        select pptm.ProductId
                    );

                    if (searchLocalizedValue)
                    {
                        productsByKeywords = productsByKeywords.Union(
                        from pptm in _productTagMappingRepository.Table
                        join lp in _localizedPropertyRepository.Table on pptm.ProductTagId equals lp.EntityId
                        where lp.LocaleKeyGroup == nameof(ProductTag) &&
                              lp.LocaleKey == nameof(ProductTag.Name) &&
                              lp.LocaleValue.Contains(keywords)
                        select lp.EntityId);
                    }
                }

                if (searchLocalizedValue)
                {
                    productsByKeywords = productsByKeywords.Union(
                                from lp in _localizedPropertyRepository.Table
                                let checkName = lp.LocaleKey == nameof(Product.Name) &&
                                                lp.LocaleValue.Contains(keywords)
                                let checkShortDesc = searchDescriptions &&
                                                lp.LocaleKey == nameof(Product.ShortDescription) &&
                                                lp.LocaleValue.Contains(keywords)
                                let checkProductTags = searchProductTags &&
                                                lp.LocaleKeyGroup == nameof(ProductTag) &&
                                                lp.LocaleKey == nameof(ProductTag.Name) &&
                                                lp.LocaleValue.Contains(keywords)
                                where
                                    lp.LocaleKeyGroup == nameof(Product) && lp.LanguageId == languageId && (checkName || checkShortDesc) ||
                                    checkProductTags

                                select lp.EntityId);
                }

                productsQuery =
                    from p in productsQuery
                    from pbk in LinqToDB.LinqExtensions.InnerJoin(productsByKeywords, pbk => pbk == p.Id)
                    select p;
            }

            if (categoryIds is not null)
            {
                if (categoryIds.Contains(0))
                    categoryIds.Remove(0);

                if (categoryIds.Any())
                {
                    var productCategoryQuery =
                        from pc in _productCategoryRepository.Table
                        where (!excludeFeaturedProducts || !pc.IsFeaturedProduct) &&
                            categoryIds.Contains(pc.CategoryId)
                        group pc by pc.ProductId into pc
                        select new
                        {
                            ProductId = pc.Key,
                            DisplayOrder = isMobileDevice ? pc.First().MobileDisplayOrder : pc.First().DisplayOrder
                        };

                    productsQuery =
                        from p in productsQuery
                        join pc in productCategoryQuery on p.Id equals pc.ProductId
                        orderby pc.DisplayOrder, p.Name
                        select p;
                }
            }
            if (kwTermId != 0)
            {

                var productKwTermQuery =
                       from pk in _productKwTermRepository.Table
                       where
                           pk.KwTermId == kwTermId
                       group pk by pk.ProductId into pk
                       select new
                       {
                           ProductId = pk.Key,
                           DisplayOrder = isMobileDevice ? pk.First().MobileDisplayOrder : pk.First().DisplayOrder
                       };

                productsQuery =
                    from p in productsQuery
                    join pc in productKwTermQuery on p.Id equals pc.ProductId
                    orderby pc.DisplayOrder, p.Name
                    select p;
            }
            else if (questionAnswerId != 0)
            {
                var productQuestionAnswerQuery =
                       from pq in _productQuestionAnswerRepository.Table
                       where
                           pq.QuestionAnswerId == questionAnswerId
                       group pq by pq.ProductId into pk
                       select new
                       {
                           ProductId = pk.Key,
                           DisplayOrder = isMobileDevice ? pk.First().MobileDisplayOrder : pk.First().DisplayOrder
                       };

                productsQuery =
                    from p in productsQuery
                    join pc in productQuestionAnswerQuery on p.Id equals pc.ProductId
                    orderby pc.DisplayOrder, p.Name
                    select p;
            }
            if (manufacturerIds is not null)
            {
                if (manufacturerIds.Contains(0))
                    manufacturerIds.Remove(0);

                if (manufacturerIds.Any())
                {
                    var productManufacturerQuery =
                        from pm in _productManufacturerRepository.Table
                        where (!excludeFeaturedProducts || !pm.IsFeaturedProduct) &&
                            manufacturerIds.Contains(pm.ManufacturerId)
                        group pm by pm.ProductId into pm
                        select new
                        {
                            ProductId = pm.Key,
                            DisplayOrder = pm.First().DisplayOrder
                        };

                    productsQuery =
                        from p in productsQuery
                        join pm in productManufacturerQuery on p.Id equals pm.ProductId
                        orderby pm.DisplayOrder, p.Name
                        select p;
                }
            }

            if (productTagId > 0)
            {
                productsQuery =
                    from p in productsQuery
                    join ptm in _productTagMappingRepository.Table on p.Id equals ptm.ProductId
                    where ptm.ProductTagId == productTagId
                    select p;
            }
            var query = from p in productsQuery
                        join psa in _productSpecificationAttributeRepository.Table.Where(_psa => _psa.AllowFiltering == true) on p.Id equals psa.ProductId into psaj
                        from psaComb in psaj.DefaultIfEmpty()

                        select new ProductSpecificationAttribute
                        {
                            ProductId = p.Id,
                            SpecificationAttributeOptionId = psaComb == null ? 0 : psaComb.SpecificationAttributeOptionId,
                            DisplayOrder = psaComb == null ? 0 : isMobileDevice ? psaComb.MobileDisplayOrder : psaComb.DisplayOrder
                        };

            var products = await (query).ToListAsync();

            return products;

        }

        public virtual async Task<IPagedList<Product>> CustomSearchProductsAsync(int[] ids, List<int> _specificationAttributes, ProductSortingEnum orderBy = ProductSortingEnum.Position,
                       int pageIndex = 0,
                int pageSize = int.MaxValue, int categoryId = 0, int featuredId = 0, bool featuredListingDisplaySimilarOnTop = false, bool isMobileDevice = false, int kwTermId = 0, int questionAnswerId = 0)
        {
            if (categoryId == 0 && kwTermId == 0 && questionAnswerId == 0)
            {
                var productsQuery = _productRepository.Table.Where(entry => ids.Contains(entry.Id));
                return await productsQuery.CustomOrderBy(orderBy).ToPagedListAsync(pageIndex, pageSize);
            }
            else
            {
                var productsQuery = _productRepository.Table.Where(entry => ids.Contains(entry.Id));

                if (_specificationAttributes.Any())
                {
                    var productSpecificationQuery =
                          from psa in _productSpecificationAttributeRepository.Table
                          where _specificationAttributes.Contains(psa.SpecificationAttributeOptionId)
                          orderby isMobileDevice ? psa.MobileDisplayOrder : psa.DisplayOrder
                          group psa by psa.ProductId into pc
                          select new
                          {
                              ProductId = pc.Key,
                              DisplayOrder = isMobileDevice ? pc.FirstOrDefault().MobileDisplayOrder : pc.FirstOrDefault().DisplayOrder
                          };
                    productsQuery =
            from p in productsQuery
            join pc in productSpecificationQuery on p.Id equals pc.ProductId
            orderby pc.DisplayOrder, p.Name
            select p;
                    return await productsQuery.CustomOrderBy(orderBy).ToPagedListAsync(pageIndex, pageSize);
                }
                else
                {

                    if (categoryId != 0 && orderBy == ProductSortingEnum.Position && featuredId != 0 &&
                        (((await GetProductByIdAsync(featuredId))?.FeaturedListingDisplaySimilarOnTop ?? false) || featuredListingDisplaySimilarOnTop))
                    {



                        var productCategoryQuery =
                                 from pc in _productCategoryRepository.Table
                                 where pc.CategoryId == categoryId
                                 group pc by pc.ProductId into pc
                                 select new
                                 {
                                     ProductId = pc.Key,
                                     DisplayOrder = isMobileDevice ? pc.FirstOrDefault().MobileDisplayOrder : pc.FirstOrDefault().DisplayOrder
                                 };


                        var relatedProducts =
                                               from rel in _relatedProductRepository.Table
                                               where rel.ProductId1 == featuredId
                                               group rel by rel.ProductId2 into pc
                                               select new
                                               {
                                                   ProductId = pc.Key,
                                                   DisplayOrder = -((100 - pc.FirstOrDefault().DisplayOrder) * 10000000)
                                               };

                        productCategoryQuery = from pc in productCategoryQuery
                                               join _relatedProduct in relatedProducts
                                               on pc.ProductId equals _relatedProduct.ProductId into relatedProduct
                                               from _relatedProduct in relatedProduct.DefaultIfEmpty()
                                               select new
                                               {
                                                   ProductId = pc.ProductId,
                                                   DisplayOrder = _relatedProduct == null ? pc.DisplayOrder : _relatedProduct.DisplayOrder
                                               };


                        //if (_dicSortedProducts.Count > 0 && orderBy == ProductSortingEnum.Position)
                        //{
                        //    productsQuery =
                        // from p in productsQuery
                        // join pc in productCategoryQuery on p.Id equals pc.ProductId
                        // select p;
                        //    return await productsQuery.ToPagedListAsync(pageIndex, pageSize);
                        //}
                        //else
                        //{
                        productsQuery =
                     from p in productsQuery
                     join pc in productCategoryQuery on p.Id equals pc.ProductId
                     orderby pc.DisplayOrder, p.Name
                     select p;
                        return await productsQuery.CustomOrderBy(orderBy).ToPagedListAsync(pageIndex, pageSize);

                    }
                    else if (categoryId > 0)
                    {
                        var productCategoryQuery =
                               from pc in _productCategoryRepository.Table
                               where pc.CategoryId == categoryId
                               group pc by pc.ProductId into pc
                               select new
                               {
                                   ProductId = pc.Key,
                                   DisplayOrder = isMobileDevice ? pc.FirstOrDefault().MobileDisplayOrder : pc.FirstOrDefault().DisplayOrder
                               };


                        //if (_dicSortedProducts.Count > 0 && orderBy == ProductSortingEnum.Position)
                        //{
                        //    productsQuery =
                        // from p in productsQuery
                        // join pc in productCategoryQuery on p.Id equals pc.ProductId
                        // select p;
                        //    return await productsQuery.ToPagedListAsync(pageIndex, pageSize);
                        //}
                        //else
                        //{
                        productsQuery =
                     from p in productsQuery
                     join pc in productCategoryQuery on p.Id equals pc.ProductId
                     orderby pc.DisplayOrder, p.Name
                     select p;
                        return await productsQuery.CustomOrderBy(orderBy).ToPagedListAsync(pageIndex, pageSize);
                    }
                    else if (kwTermId > 0)
                    {

                        var productKwTermQuery =
                               from pk in _productKwTermRepository.Table
                               where pk.KwTermId == kwTermId
                               group pk by pk.ProductId into pk
                               select new
                               {
                                   ProductId = pk.Key,
                                   DisplayOrder = isMobileDevice ? pk.FirstOrDefault().MobileDisplayOrder : pk.FirstOrDefault().DisplayOrder
                               };


                        //if (_dicSortedProducts.Count > 0 && orderBy == ProductSortingEnum.Position)
                        //{
                        //    productsQuery =
                        // from p in productsQuery
                        // join pc in productCategoryQuery on p.Id equals pc.ProductId
                        // select p;
                        //    return await productsQuery.ToPagedListAsync(pageIndex, pageSize);
                        //}
                        //else
                        //{
                        productsQuery =
                     from p in productsQuery
                     join pc in productKwTermQuery on p.Id equals pc.ProductId
                     orderby pc.DisplayOrder, p.Name
                     select p;
                        return await productsQuery.CustomOrderBy(orderBy).ToPagedListAsync(pageIndex, pageSize);
                    }
                    else
                    {

                        var productQuestionAnswerQuery =
                               from pq in _productQuestionAnswerRepository.Table
                               where pq.QuestionAnswerId == questionAnswerId
                               group pq by pq.ProductId into pk
                               select new
                               {
                                   ProductId = pk.Key,
                                   DisplayOrder = isMobileDevice ? pk.FirstOrDefault().MobileDisplayOrder : pk.FirstOrDefault().DisplayOrder
                               };


                        productsQuery =
                      from p in productsQuery
                      join pc in productQuestionAnswerQuery on p.Id equals pc.ProductId
                      orderby pc.DisplayOrder, p.Name
                      select p;
                        return await productsQuery.CustomOrderBy(orderBy).ToPagedListAsync(pageIndex, pageSize);
                    }
                }

                //   }

            }
        }
        public virtual async Task<IPagedList<Product>> CustomSearchProductsWithVariablePageSizeAsync(int[] ids, List<int> _specificationAttributes, ProductSortingEnum orderBy = ProductSortingEnum.Position,
                              int pageIndex = 0,
                       int pageSize = int.MaxValue, int firstPageSize = int.MaxValue, int subsequentPageSize = int.MaxValue, int categoryId = 0, int featuredId = 0, bool featuredListingDisplaySimilarOnTop = false, bool isMobileDevice = false, int kwTermId = 0, int questionAnswerId = 0)
        {
            if (categoryId == 0 && kwTermId == 0 && questionAnswerId == 0)
            {
                var productsQuery = _productRepository.Table.Where(entry => ids.Contains(entry.Id));
                return await productsQuery.CustomOrderBy(orderBy).ToPagedListAsync(pageIndex, pageSize);
            }
            else
            {
                var productsQuery = _productRepository.Table.Where(entry => ids.Contains(entry.Id));

                if (_specificationAttributes.Any())
                {
                    var productSpecificationQuery =
                          from psa in _productSpecificationAttributeRepository.Table
                          where _specificationAttributes.Contains(psa.SpecificationAttributeOptionId)
                          orderby isMobileDevice ? psa.MobileDisplayOrder : psa.DisplayOrder
                          group psa by psa.ProductId into pc
                          select new
                          {
                              ProductId = pc.Key,
                              DisplayOrder = isMobileDevice ? pc.FirstOrDefault().MobileDisplayOrder : pc.FirstOrDefault().DisplayOrder
                          };
                    productsQuery =
            from p in productsQuery
            join pc in productSpecificationQuery on p.Id equals pc.ProductId
            orderby pc.DisplayOrder, p.Name
            select p;
                    return await productsQuery.CustomOrderBy(orderBy).CustomToPagedListAsync(pageIndex, pageSize, firstPageSize, subsequentPageSize);
                }
                else
                {

                    if (categoryId != 0 && orderBy == ProductSortingEnum.Position && featuredId != 0 &&
                        (((await GetProductByIdAsync(featuredId))?.FeaturedListingDisplaySimilarOnTop ?? false) || featuredListingDisplaySimilarOnTop))
                    {




                        var productCategoryQuery =
                                 from pc in _productCategoryRepository.Table
                                 where pc.CategoryId == categoryId
                                 group pc by pc.ProductId into pc
                                 select new
                                 {
                                     ProductId = pc.Key,
                                     DisplayOrder = isMobileDevice ? pc.FirstOrDefault().MobileDisplayOrder : pc.FirstOrDefault().DisplayOrder
                                 };


                        var relatedProducts =
                                               from rel in _relatedProductRepository.Table
                                               where rel.ProductId1 == featuredId
                                               group rel by rel.ProductId2 into pc
                                               select new
                                               {
                                                   ProductId = pc.Key,
                                                   DisplayOrder = -((100 - pc.FirstOrDefault().DisplayOrder) * 10000000)
                                               };

                        productCategoryQuery = from pc in productCategoryQuery
                                               join _relatedProduct in relatedProducts
                                               on pc.ProductId equals _relatedProduct.ProductId into relatedProduct
                                               from _relatedProduct in relatedProduct.DefaultIfEmpty()
                                               select new
                                               {
                                                   ProductId = pc.ProductId,
                                                   DisplayOrder = _relatedProduct == null ? pc.DisplayOrder : _relatedProduct.DisplayOrder
                                               };


                        //if (_dicSortedProducts.Count > 0 && orderBy == ProductSortingEnum.Position)
                        //{
                        //    productsQuery =
                        // from p in productsQuery
                        // join pc in productCategoryQuery on p.Id equals pc.ProductId
                        // select p;
                        //    return await productsQuery.ToPagedListAsync(pageIndex, pageSize);
                        //}
                        //else
                        //{
                        productsQuery =
                     from p in productsQuery
                     join pc in productCategoryQuery on p.Id equals pc.ProductId
                     orderby pc.DisplayOrder, p.Name
                     select p;
                        return await productsQuery.CustomOrderBy(orderBy).CustomToPagedListAsync(pageIndex, pageSize, firstPageSize, subsequentPageSize);

                    }
                    else if (categoryId > 0)
                    {
                        var productCategoryQuery =
                               from pc in _productCategoryRepository.Table
                               where pc.CategoryId == categoryId
                               group pc by pc.ProductId into pc
                               select new
                               {
                                   ProductId = pc.Key,
                                   DisplayOrder = isMobileDevice ? pc.FirstOrDefault().MobileDisplayOrder : pc.FirstOrDefault().DisplayOrder
                               };


                        //if (_dicSortedProducts.Count > 0 && orderBy == ProductSortingEnum.Position)
                        //{
                        //    productsQuery =
                        // from p in productsQuery
                        // join pc in productCategoryQuery on p.Id equals pc.ProductId
                        // select p;
                        //    return await productsQuery.ToPagedListAsync(pageIndex, pageSize);
                        //}
                        //else
                        //{
                        productsQuery =
                     from p in productsQuery
                     join pc in productCategoryQuery on p.Id equals pc.ProductId
                     orderby pc.DisplayOrder, p.Name
                     select p;
                        return await productsQuery.CustomOrderBy(orderBy).CustomToPagedListAsync(pageIndex, pageSize, firstPageSize, subsequentPageSize);
                    }
                    else if (kwTermId > 0)
                    {

                        var productKwTermQuery =
                               from pk in _productKwTermRepository.Table
                               where pk.KwTermId == kwTermId
                               group pk by pk.ProductId into pk
                               select new
                               {
                                   ProductId = pk.Key,
                                   DisplayOrder = isMobileDevice ? pk.FirstOrDefault().MobileDisplayOrder : pk.FirstOrDefault().DisplayOrder
                               };


                        //if (_dicSortedProducts.Count > 0 && orderBy == ProductSortingEnum.Position)
                        //{
                        //    productsQuery =
                        // from p in productsQuery
                        // join pc in productCategoryQuery on p.Id equals pc.ProductId
                        // select p;
                        //    return await productsQuery.ToPagedListAsync(pageIndex, pageSize);
                        //}
                        //else
                        //{
                        productsQuery =
                     from p in productsQuery
                     join pc in productKwTermQuery on p.Id equals pc.ProductId
                     orderby pc.DisplayOrder, p.Name
                     select p;
                        return await productsQuery.CustomOrderBy(orderBy).CustomToPagedListAsync(pageIndex, pageSize, firstPageSize, subsequentPageSize);
                    }
                    else
                    {

                        var productQuestionAnswerQuery =
                               from pq in _productQuestionAnswerRepository.Table
                               where pq.QuestionAnswerId == questionAnswerId
                               group pq by pq.ProductId into pk
                               select new
                               {
                                   ProductId = pk.Key,
                                   DisplayOrder = isMobileDevice ? pk.FirstOrDefault().MobileDisplayOrder : pk.FirstOrDefault().DisplayOrder
                               };


                        productsQuery =
                      from p in productsQuery
                      join pc in productQuestionAnswerQuery on p.Id equals pc.ProductId
                      orderby pc.DisplayOrder, p.Name
                      select p;
                        return await productsQuery.CustomOrderBy(orderBy).CustomToPagedListAsync(pageIndex, pageSize, firstPageSize, subsequentPageSize);
                    }
                }

                //   }

            }
        }
        public async Task UpdateProductInventory(Product product)
        {
            product.TotalInventory = await GetProductInventory(product);
            await this.UpdateProductWithoutEvent(product);
        }


        public async Task<int> GetProductInventory(Product product)
        {
            var stock = product.StockQuantity;
            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes)
            {
                var combinations = await _customProductAttributeService.CustomGetAllProductAttributeCombinationsAsync(product.Id);
                if (combinations.Count > 0)
                {
                    foreach (var combination in combinations)
                    {
                        stock += combination.StockQuantity < 0 ? 0 : combination.StockQuantity;
                    }
                }
            }
            return stock;
        }

        #endregion

        #region Search

        public async Task<Product> GetProductBySearchTerm(string term)
        {
            return await (from _prd in _productRepository.Table
                          where _prd.Id.ToString() == term
                          || _prd.Sku == term
                          && _prd.Published == true && _prd.Deleted == false
                          select _prd).FirstOrDefaultAsync();
        }

        #endregion


        #region ProductVariant
        public async Task<VariantCombination> ValidateVariantID(int productId, int variantId)
        {
            VariantCombination variantCombination = (await this.GetProductVariants(productId)).Where(v => v.VariantId == variantId).FirstOrDefault();
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
        public async Task<int> GetProductDefaultVariantId(int productId)
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
        public async Task<int> GetVariantId(int productId, string attributes)
        {
            int variantId = await this.GetVariantIdFromAttributeDescription(productId, attributes);
            if (variantId == 0)
            {
                string size = "";

                if (!string.IsNullOrEmpty(attributes))
                {
                    string[] attrs = attributes.Split("<br /", StringSplitOptions.RemoveEmptyEntries);
                    foreach (var attr in attrs)
                    {
                        if (attr.Split(':').Length > 0)
                        {
                            if (attr.Split(':')[0].Equals("size", StringComparison.InvariantCultureIgnoreCase))
                            {
                                size = string.Join(':', attr.Split(":").Skip(1));
                                break;
                            }
                        }
                    }
                }
                variantId = await this.GetProductVariantIdBySize(productId, System.Net.WebUtility.HtmlDecode(size));
            }
            return variantId;
        }
        public async Task<int> GetProductVariantIdBySize(int productId, string size)
        {
            int defaultVariantId = 0;
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
                        defaultVariantId = attributeValues.Where(v => v.IsPreSelected).FirstOrDefault()?.VariantId ?? 0;
                        if (defaultVariantId == 0)
                            defaultVariantId = attributeValues.FirstOrDefault()?.VariantId ?? 0;
                        foreach (var value in attributeValues)
                        {
                            if (value.Name.Trim().Equals(size.Trim(), StringComparison.InvariantCultureIgnoreCase))
                            {
                                variantId = value.VariantId;
                                if (variantId > 0)
                                    break;
                            }
                        }
                    }
                }
            }
            return variantId > 0 ? variantId : defaultVariantId;
        }
        public async Task<VariantCombination> GetVariantById(int id)
        {
            return await _variantcombinationRepository.GetByIdAsync(id);
        }
        public async Task UpdateVariant(VariantCombination variant, bool updateCombinations = true)
        {
            if (updateCombinations)
            {
                await UpdateAttributeCombinationPrices(variant);
            }
            await UpdateVariantCombinationAsync(variant);
        }
        public async Task<List<VariantCombination>> GetProductVariants(int productId)
        {
            return await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(CustomNopCatalogDefaults.ProductVariantsCacheKey, productId), async () => await _variantcombinationRepository.Table.Where(vc => vc.ProductId == productId).ToListAsync());
        }
        public async Task<List<VariantCombination>> GetPublishedProductVariants(int productId)
        {

            return await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(CustomNopCatalogDefaults.ProductVariantsPublishedCacheKey, productId), async () =>
            {

                var variantCombinations = await _variantcombinationRepository.Table.Where(vc => vc.ProductId == productId).ToListAsync();
                var variants = new List<VariantCombination>();
                foreach (var _variant in variantCombinations)
                {
                    bool published = true;
                    foreach (var attrValueId in (_variant.ProductAttributeValueIds ?? string.Empty).Split('-'))
                    {
                        int.TryParse(attrValueId, out int _attrValueId);
                        var productAttributeValue = await _productAttributeService.GetProductAttributeValueByIdAsync(_attrValueId);
                        if (productAttributeValue == null || !productAttributeValue.Published)
                        {
                            published = false;
                            break;
                        }

                    }
                    if (published)
                        variants.Add(_variant);

                }
                return variants;
            });
        }
        public async Task<int> GenerateVariantIdAsync(ProductAttributeMapping mapping, int productAttributeValueId, string productAttributeValue)
        {
            var attribute = await _productAttributeService.GetProductAttributeByIdAsync(mapping.ProductAttributeId);
            ProductVariant productVariant = new ProductVariant();
            if (!string.Equals(attribute.Name, await _localizationService.GetResourceAsync("Product.Attr.Stain"), StringComparison.InvariantCultureIgnoreCase))
            {
                productVariant = await this.GetProductVariantByAttributeValueIdAsync(productAttributeValueId);
                if (productVariant == null)
                {
                    productVariant = new ProductVariant();
                    productVariant = await GetProductVariantByAttributeValueAsync(mapping.ProductId, mapping.ProductAttributeId, productAttributeValue);
                    if (productVariant == null)
                    {
                        productVariant = new ProductVariant();
                        productVariant.ProductId = mapping.ProductId;
                        productVariant.ProductAttributeValueId = productAttributeValueId;
                        productVariant.ProductAttributeId = mapping.ProductAttributeId;
                        productVariant.AttributeValue = productAttributeValue;
                        productVariant.ProductAttributeValueIds = string.Empty;
                        await this.InsertProductVariantAsync(productVariant);
                    }
                    else
                    {
                        productVariant.ProductAttributeValueId = productAttributeValueId;
                        productVariant.ProductAttributeValueIds = string.Empty;
                        await this.UpdateProductVariantAsync(productVariant);
                    }

                }
                else
                {
                    if (!string.Equals(productAttributeValue, productVariant.AttributeValue))
                    {
                        productVariant.AttributeValue = productAttributeValue;
                        await this.UpdateProductVariantAsync(productVariant);
                    }
                }

                await GenerateVariantCombinations(mapping.ProductId);
            }
            return productVariant?.VariantId ?? 0;
        }
        public async Task<bool> GenerateVariantCombinations(int productId)
        {
            var productAttributesCombinations = await _customProductAttributeService.CustomGetAllProductAttributeCombinationsAsync(productId);

            Dictionary<int, List<int>> dicAttrCombinations = new Dictionary<int, List<int>>();

            foreach (var _attrCombination in productAttributesCombinations)
            {
                dicAttrCombinations.Add(_attrCombination.Id, (await _productAttributeParser.ParseProductAttributeValuesAsync(_attrCombination.AttributesXml)).Select(x => x.Id).ToList());
            }
            var existingCombinations = await this.GetProductVariants(productId);
            var exisitngProductVariant = await this.GetAllProductVariantByProductIdAsync(productId);
            var product = await GetProductByIdAsync(productId);

            Dictionary<string, List<int>> combinations =
                await _customProductAttributeParser.CustomGenerateAllCombinationsAsync(product);

            List<int> productAttributeValueIds = combinations.Values.Where(combination => combination.Any()).Select(combination => combination.First()).Distinct().ToList();

            List<VariantCombination> lstCombinations = new List<VariantCombination>();
            foreach (var productAttributeValueId in productAttributeValueIds)
            {
                var _combinations = combinations.Where(kv => kv.Value.Contains(productAttributeValueId)).ToDictionary(kv => kv.Key, kv => kv.Value);
                var productAttributeValue = await _productAttributeService.GetProductAttributeValueByIdAsync(productAttributeValueId);
                foreach (var _combination in _combinations)
                {
                    lstCombinations.Add(new VariantCombination()
                    {
                        Combination = _combination.Key,
                        OldPrice = 0,
                        Price = 0,
                        ProductId = productId,
                        ProductAttributeValueIds = string.Join("-", _combination.Value),
                        Title = productAttributeValue?.VariantTitle ?? string.Empty
                    });
                    foreach (var attributeValueId in _combination.Value)
                    {
                        var productVariant = await GetProductVariantByAttributeValueIdAsync(attributeValueId);
                        if (productVariant == null)
                        {
                            var attributeValue = await _productAttributeService.GetProductAttributeValueByIdAsync(Convert.ToInt32(attributeValueId));
                            var productmapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(attributeValue.ProductAttributeMappingId);
                            productVariant = new ProductVariant();
                            productVariant.AttributeValue = attributeValue.Name;
                            productVariant.ProductAttributeId = productmapping.ProductAttributeId;
                            productVariant.ProductAttributeValueId = Convert.ToInt32(attributeValueId);
                            productVariant.ProductId = productId;
                            productVariant.ProductAttributeValueIds = string.Empty;
                            await InsertProductVariantAsync(productVariant);
                            exisitngProductVariant.Add(productVariant);
                        }
                    }

                }
            }

            #region Delete all Combination that's no longer exist
            List<VariantCombination> itemsToRemove = new List<VariantCombination>();
            foreach (var existingCombination in existingCombinations)
            {
                var combination = GetVariantCombinationByProductAttributeValueIds(lstCombinations, existingCombination.ProductAttributeValueIds);
                if (combination == null)
                {
                    await DeleteVariantCombinationAsync(existingCombination);

                    // nsert Log
                    VariantCombinationLog variantcombinationlog = new VariantCombinationLog();
                    variantcombinationlog.DeletedOn = DateTime.UtcNow;
                    variantcombinationlog.IsDeleted = true;
                    variantcombinationlog.Combination = existingCombination.Combination;
                    variantcombinationlog.VariantId = existingCombination.VariantId;
                    variantcombinationlog.UpdatedOn = DateTime.UtcNow;
                    variantcombinationlog.Price = existingCombination.Price;
                    variantcombinationlog.OldPrice = existingCombination.OldPrice;
                    variantcombinationlog.Msrp = existingCombination.Msrp;
                    variantcombinationlog.ProductId = existingCombination.ProductId;
                    variantcombinationlog.ProductAttributeValueIds = existingCombination.ProductAttributeValueIds;
                    await InsertVariantCombinationLogAsync(variantcombinationlog);
                    itemsToRemove.Add(existingCombination);
                }
            }
            existingCombinations.RemoveAll(itemsToRemove.Contains);
            #endregion


            #region VariantId Assign
            foreach (var combination in lstCombinations)
            {
                var productVariant = GetVariantByProductAttributeValueIds(exisitngProductVariant, combination.ProductAttributeValueIds);
                if (productVariant != null)
                {

                    combination.VariantId = productVariant.VariantId;
                    var existcombination = GetVariantCombinationByProductAttributeValueIds(existingCombinations, combination.ProductAttributeValueIds);
                    if (existcombination != null)
                    {
                        if (combination.VariantId != productVariant.VariantId)
                        {
                            combination.Id = existcombination.Id;
                            combination.Price = existcombination.Price;
                            combination.OldPrice = existcombination.OldPrice;
                            combination.Msrp = existcombination.Msrp;
                            combination.Title = existcombination.Title;
                            await UpdateVariantCombinationAsync(combination);

                            // Log nsert 
                            VariantCombinationLog variantcombinationlog = new VariantCombinationLog();
                            variantcombinationlog.CreatedOn = DateTime.UtcNow;
                            variantcombinationlog.IsDeleted = false;
                            variantcombinationlog.Combination = combination.Combination;
                            variantcombinationlog.VariantId = existcombination.VariantId;
                            variantcombinationlog.UpdatedOn = DateTime.UtcNow;
                            variantcombinationlog.Price = combination.Price;
                            variantcombinationlog.OldPrice = combination.OldPrice;
                            variantcombinationlog.Msrp = combination.Msrp;
                            variantcombinationlog.ProductId = combination.ProductId;
                            variantcombinationlog.ProductAttributeValueIds = existcombination.ProductAttributeValueIds;
                            await InsertVariantCombinationLogAsync(variantcombinationlog);
                        }
                    }
                    else
                    {
                        combination.CreatedOn = DateTime.UtcNow;
                        combination.UpdatedOn = DateTime.UtcNow;
                        AssignPriceToProductAttributeCombination(combination, productAttributesCombinations, dicAttrCombinations, product);
                        await InsertVariantCombinationAsync(combination);
                        existingCombinations.Add(combination);
                    }
                }
                else
                {

                    var valueIdsSet = new HashSet<string>((combination.ProductAttributeValueIds ?? string.Empty).Split('-'));
                    var attributeValue = await _productAttributeService.GetProductAttributeValueByIdAsync(Convert.ToInt32(valueIdsSet.First()));
                    var productmapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(attributeValue.ProductAttributeMappingId);
                    if (valueIdsSet.Count == 1)
                    {
                        productVariant = new ProductVariant();
                        productVariant.AttributeValue = attributeValue.Name;
                        productVariant.ProductAttributeId = productmapping.ProductAttributeId;
                        productVariant.ProductAttributeValueId = Convert.ToInt32(valueIdsSet.First());
                        productVariant.ProductId = productId;
                        productVariant.ProductAttributeValueIds = string.Empty;
                        await InsertProductVariantAsync(productVariant);
                        exisitngProductVariant.Add(productVariant);
                        combination.CreatedOn = DateTime.UtcNow;
                        combination.UpdatedOn = DateTime.UtcNow;
                        combination.VariantId = productVariant.VariantId;
                        AssignPriceToProductAttributeCombination(combination, productAttributesCombinations, dicAttrCombinations, product);
                        await InsertVariantCombinationAsync(combination);
                        existingCombinations.Add(combination);
                    }
                    else
                    {
                        productVariant = await GetProductVariantByAttributeValueIdAsync(Convert.ToInt32(valueIdsSet.First()));
                        if (productVariant == null)
                        {
                            productVariant = new ProductVariant();
                            productVariant.AttributeValue = attributeValue.Name;
                            productVariant.ProductAttributeId = productmapping.ProductAttributeId;
                            productVariant.ProductAttributeValueId = Convert.ToInt32(valueIdsSet.First());
                            productVariant.ProductId = productId;
                            productVariant.ProductAttributeValueIds = string.Empty;
                            await InsertProductVariantAsync(productVariant);
                            exisitngProductVariant.Add(productVariant);
                        }

                        bool isVariantIdAssigned = existingCombinations.Where(V => V.VariantId == productVariant.VariantId && V.ProductAttributeValueIds != combination.ProductAttributeValueIds).Any();

                        if (isVariantIdAssigned)
                        {
                            productVariant = new ProductVariant();
                            productVariant.AttributeValue = string.Empty;
                            productVariant.ProductAttributeId = 0;
                            productVariant.ProductAttributeValueId = 0;
                            productVariant.ProductId = productId;
                            productVariant.ProductAttributeValueIds = combination.ProductAttributeValueIds;
                            await InsertProductVariantAsync(productVariant);
                            exisitngProductVariant.Add(productVariant);

                            combination.VariantId = productVariant.VariantId;
                            combination.CreatedOn = DateTime.UtcNow;
                            combination.UpdatedOn = DateTime.UtcNow;
                            AssignPriceToProductAttributeCombination(combination, productAttributesCombinations, dicAttrCombinations, product);
                            await InsertVariantCombinationAsync(combination);
                            existingCombinations.Add(combination);
                        }
                        else
                        {
                            var existcombination = GetVariantCombinationByProductAttributeValueIds(existingCombinations, combination.ProductAttributeValueIds);
                            if (existcombination == null)
                            {

                                combination.VariantId = productVariant.VariantId;
                                combination.CreatedOn = DateTime.UtcNow;
                                combination.UpdatedOn = DateTime.UtcNow;
                                AssignPriceToProductAttributeCombination(combination, productAttributesCombinations, dicAttrCombinations, product);
                                await InsertVariantCombinationAsync(combination);
                                existingCombinations.Add(combination);
                            }
                        }


                    }
                }
            }
            #endregion

            return true;
        }
        public async Task<int> GetVariantIdFromAttributeDescription(int productId, string attributeDescription)
        {

            List<int> attributeValueIds = new List<int>();
            Dictionary<string, string> attributes = new Dictionary<string, string>();


            if (!string.IsNullOrEmpty(attributeDescription))
            {
                string[] attrs = attributeDescription.Split("<br />", StringSplitOptions.RemoveEmptyEntries);
                foreach (var attr in attrs)
                {
                    if (attr.Split(':').Length > 0)
                    {
                        if (!attributes.Where(a => a.Key.Trim() == string.Join(':', attr.Split(":")[0]).Trim()).Any())
                        {
                            attributes.Add(System.Net.WebUtility.HtmlDecode(System.Net.WebUtility.HtmlDecode(string.Join(':', attr.Split(":")[0])).Trim()), System.Net.WebUtility.HtmlDecode(string.Join(':', attr.Split(":").Skip(1))).Trim());
                        }
                    }
                }
            }

            var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(productId);

            foreach (var mapping in productAttributeMapping)
            {
                var attribute = await _productAttributeService.GetProductAttributeByIdAsync(mapping.ProductAttributeId);
                if (!string.Equals(attribute.Name, await _localizationService.GetResourceAsync("Product.Attr.Stain"), StringComparison.InvariantCultureIgnoreCase))
                {
                    foreach (var attributeValue in await _productAttributeService.GetProductAttributeValuesAsync(mapping.Id))
                    {
                        if (attributes.Where(a => (a.Key.Trim() == attribute.Name || a.Key.Trim() == mapping.TextPrompt) && a.Value.Trim() == attributeValue.Name.Trim()).Any())
                        {
                            attributeValueIds.Add(attributeValue.Id);
                        }
                    }
                }
            }

            return (GetVariantCombinationByProductAttributeValueIds(await GetProductVariants(productId), string.Join("-", attributeValueIds.ToArray())))?.VariantId ?? 0;

        }
        public async Task<VariantCombination> GetVariantByVariantId(int variantId)
        {

            return await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(CustomNopCatalogDefaults.ProductVariantByVariantIdCacheKey, variantId),
                            async () => await _variantcombinationRepository.Table.Where(vc => vc.VariantId == variantId).FirstOrDefaultAsync());

        }
        public async Task<VariantCombination> GetVariantFromAttributeValues(int productId, List<int> attributeValueIds)
        {

            for (int i = attributeValueIds.Count() - 1; i >= 0; i--)
            {
                var pav = await _productAttributeService.GetProductAttributeValueByIdAsync(attributeValueIds[i]);
                var mapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(pav.ProductAttributeMappingId);
                var attribute = await _productAttributeService.GetProductAttributeByIdAsync(mapping.ProductAttributeId);

                if (string.Equals(
                        attribute.Name,
                        await _localizationService.GetResourceAsync("Product.Attr.Stain"),
                        StringComparison.InvariantCultureIgnoreCase))
                {
                    attributeValueIds.RemoveAt(i);
                }
            }

            return (GetVariantCombinationByProductAttributeValueIds(await GetProductVariants(productId), string.Join("-", attributeValueIds)));
        }
        public async Task GenerateProductVariantSename(Product product)
        {
            var productVariantCombinations = await GetProductVariants(product.Id);

            foreach (var variantCombination in productVariantCombinations)
            {
                if (string.IsNullOrWhiteSpace(variantCombination.Title))
                    continue;
                if (variantCombination.Title.Trim().ToLower() == product.Name.Trim().ToLower())
                    continue;

                if (!string.IsNullOrEmpty(variantCombination.SeName))
                    continue;

                variantCombination.SeName = await _urlRecordService.ValidateSeNameAsync(variantCombination.VariantId, "VariantCombination", "", variantCombination.Title, false);
                await this.UpdateVariantCombinationAsync(variantCombination);
            }
        }
        public async Task<VariantCombination> GetItemVariantInfo(int productId, string attributesXml)
        {
            if (string.IsNullOrEmpty(attributesXml))
                return null;
            else
            {
                return await this.GetVariantFromAttributeValues(productId, (await _productAttributeParser.ParseProductAttributeValuesAsync(attributesXml)).Select(attrValues => attrValues.Id).ToList());
            }
        }
        public async Task<List<ProductAttributeCombination>> GetVariantCombinationAsync(
    VariantCombination variant, IList<ProductAttributeCombination> productAttributeCombinations)
        {
            var variantValues = await _productAttributeParser
                .ParseProductAttributeValuesAsync(variant.Combination);

            var variantValueIds = variantValues
                .Select(v => v.Id)
                .ToList();

            if (!productAttributeCombinations.Any())
                return new List<ProductAttributeCombination>();

            List<ProductAttributeCombination> matchedCombinations =
                new List<ProductAttributeCombination>();

            foreach (var combination in productAttributeCombinations)
            {
                var comboValues = await _productAttributeParser
                    .ParseProductAttributeValuesAsync(combination.AttributesXml);

                var comboValueIds = comboValues
                    .Select(v => v.Id)
                    .ToList();

                if (variantValueIds.All(vId => comboValueIds.Contains(vId)) &&
                    comboValueIds.Count <= variantValueIds.Count + 1)
                {
                    matchedCombinations.Add(combination);
                }
            }

            return matchedCombinations
                .OrderByDescending(x => x.OverriddenPrice ?? 0)
                .ToList();
        }

        #endregion

        #region Product Attribute

        public virtual async Task<IPagedList<Product>> CustomGetProductsByProductAtributeIdAsync(int productAttributeId
        , string search, int categoryId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var productsQuery = _productRepository.Table;
            if (categoryId != 0)
            {
                var productCategoryQuery =
                       from pc in _productCategoryRepository.Table
                       where pc.CategoryId == categoryId
                       select pc.ProductId;
                productsQuery =
                 from p in productsQuery
                 join pc in productCategoryQuery on p.Id equals pc
                 select p;
            }


            if (!string.IsNullOrEmpty(search))
            {


                IQueryable<int> productsByKeywords;
                productsByKeywords =
                        from p in _productRepository.Table
                        where p.Name.Contains(search) || p.Sku == search || p.Id.ToString() == search
                        select p.Id;
                productsQuery =
                      from p in productsQuery
                      from pbk in LinqToDB.LinqExtensions.InnerJoin(productsByKeywords, pbk => pbk == p.Id)
                      select p;
            }
            var productAttributeQuery = from pam in _productAttributeMappingRepository.Table
                                        where pam.ProductAttributeId == productAttributeId
                                        select pam;

            var query =
                           from p in productsQuery
                           join pc in productAttributeQuery on p.Id equals pc.ProductId

                           orderby pc.DisplayOrder
                           select p;

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        #endregion



        #region Import UnPubshed Product Variant

        public async Task ImportProductUnpubishedVariant(int productId)
        {
            await _productRepository.EntityFromSqlAsync("SP_Sync_Product_UnPubished_Variants",
                      new DataParameter()
                      {
                          DataType = LinqToDB.DataType.Int32,
                          Value = productId,
                          Direction = System.Data.ParameterDirection.Input,
                          Name = "@Product_Id",

                      });
        }

        #endregion



        #region SaleInfo

        public async Task<StoreWideDiscount> GetProductSaleInfo(int productId)
        {
            return await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(CustomNopCatalogDefaults.ProductSaleInfoCacheKey), async () => await _storeWideDiscountService.GetProductSaleInfo(productId));

        }

        #endregion

        #region Utilities
        protected async Task<string> GetDefaultCombinationAttrXml(IList<ProductAttributeMapping> productAttributeMapping)
        {

            var attributesXml = string.Empty;
            Dictionary<int, int> productDefaultAttrs = new Dictionary<int, int>();
            foreach (var attribute in productAttributeMapping)
            {
                if (attribute.ShouldHaveValues())
                {
                    var attributeValues = await _customProductAttributeService.CustomGetProductAttributeValuesAsync(attribute.Id);

                    foreach (var attributeValue in attributeValues)
                    {
                        if (attributeValue.IsPreSelected)
                            attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                attribute, attributeValue.Id.ToString(), null);

                    }

                }
            }
            foreach (var attribute in productAttributeMapping)
            {
                var conditionMet = await _productAttributeParser.IsConditionMetAsync(attribute, attributesXml);
                if (conditionMet.HasValue && !conditionMet.Value)
                {
                    attributesXml = _productAttributeParser.RemoveProductAttribute(attributesXml, attribute);
                }
            }
            return attributesXml;
        }
        protected async Task<string> GetCustomProductAttributesXmlAsync(IList<ProductAttributeMapping> productAttributes, Dictionary<int, string> attrMappings)
        {

            var attributesXml = string.Empty;
            foreach (var attribute in productAttributes)
            {
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                        {
                            try
                            {
                                string attrValue = attrMappings.Where(m => m.Key == attribute.Id).FirstOrDefault().Value.Split(',')[0];

                                var selectedAttributeId = int.Parse(attrValue);

                                if (selectedAttributeId > 0)
                                {
                                    if (selectedAttributeId > 0)
                                    {
                                        //get quantity entered by customer
                                        var quantity = 1;
                                        attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                            attribute, selectedAttributeId.ToString(), quantity > 1 ? (int?)quantity : null);
                                    }
                                }
                            }
                            catch (Exception exp)
                            {

                            }
                        }
                        break;
                    case AttributeControlType.Checkboxes:
                        {
                            var ctrlAttributes = attrMappings.Where(m => m.Key == attribute.Id).FirstOrDefault().Value;
                            try
                            {
                                if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                                {
                                    foreach (var item in ctrlAttributes.ToString()
                                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                    {
                                        var selectedAttributeId = int.Parse(item);
                                        if (selectedAttributeId > 0)
                                        {
                                            //get quantity entered by customer
                                            var quantity = 1;
                                            attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                                attribute, selectedAttributeId.ToString(), quantity > 1 ? (int?)quantity : null);
                                        }
                                    }
                                }
                            }
                            catch (Exception exp)
                            {

                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //load read-only (already server-side selected) values
                            var attributeValues = await _productAttributeService.GetProductAttributeValuesAsync(attribute.Id);
                            foreach (var selectedAttributeId in attributeValues
                                .Where(v => v.IsPreSelected)
                                .Select(v => v.Id)
                                .ToList())
                            {
                                //get quantity entered by customer
                                var quantity = 1;


                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString(), quantity > 1 ? (int?)quantity : null);
                            }
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            var ctrlAttributes = attrMappings.Where(m => m.Key == attribute.Id).FirstOrDefault().Value;
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                var enteredText = ctrlAttributes.ToString().Trim();
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml, attribute, enteredText);
                            }
                        }
                        break;

                    case AttributeControlType.FileUpload:
                        {
                            Guid.TryParse(attrMappings.Where(m => m.Key == attribute.Id).FirstOrDefault().Value, out var downloadGuid);
                            var download = await _downloadService.GetDownloadByGuidAsync(downloadGuid);
                            if (download != null)
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, download.DownloadGuid.ToString());
                        }
                        break;
                    default:
                        break;
                }
            }
            //validate conditional attributes (if specified)
            foreach (var attribute in productAttributes)
            {
                var conditionMet = await _productAttributeParser.IsConditionMetAsync(attribute, attributesXml);
                if (conditionMet.HasValue && !conditionMet.Value)
                {
                    attributesXml = _productAttributeParser.RemoveProductAttribute(attributesXml, attribute);
                }
            }


            return attributesXml;
        }
        protected async Task<(decimal DiscountAmount, decimal DiscountPercentage)> GetMaxDiscountAsync(Product product, decimal? oldPrice, decimal? price)
        {
            decimal maxDiscountAmount = 0;
            decimal maxDiscountPercentage = 0;

            void TryUpdateMaxDiscount(decimal price, decimal? oldPrice)
            {
                if (!oldPrice.HasValue || oldPrice.Value <= price || price <= 0)
                    return;

                var discountAmount = oldPrice.Value - price;
                var discountPercentage = (discountAmount / oldPrice.Value) * 100;

                if (discountAmount > maxDiscountAmount)
                {
                    maxDiscountAmount = discountAmount;
                    maxDiscountPercentage = Math.Round(discountPercentage, 0);
                }
            }
            if (price.HasValue && price.Value > 0)
            {
                TryUpdateMaxDiscount(price.Value, oldPrice ?? 0);
                return (Math.Round(maxDiscountAmount, 2), Math.Round(maxDiscountPercentage, 2));
            }

            var combinations = await _productAttributeService
     .GetAllProductAttributeCombinationsAsync(product.Id);
            if (combinations != null && combinations.Any())
            {
                foreach (var combination in combinations)
                {

                    var attributes = await _productAttributeParser.ParseProductAttributeMappingsAsync(combination.AttributesXml);
                    if (!attributes.Any())
                        continue;

                    bool isAttributeValid = true;
                    foreach (var attribute in attributes)
                    {
                        if (isAttributeValid)
                        {
                            if (!attribute.ShouldHaveValues())
                            {
                                isAttributeValid = false;
                                break;
                            }

                            foreach (var attributeValue in _customProductAttributeParser.CustomParseValuesWithQuantity(combination.AttributesXml, attribute.Id))
                            {
                                if (string.IsNullOrEmpty(attributeValue.Item1) || !int.TryParse(attributeValue.Item1, out var attributeValueId))
                                {
                                    isAttributeValid = false;
                                    break;
                                }
                                var value = await _productAttributeService.GetProductAttributeValueByIdAsync(attributeValueId);
                                if (value == null || !value.Published)
                                {
                                    isAttributeValid = false;
                                    break;
                                }
                            }
                        }
                    }
                    if (!isAttributeValid)
                        continue;

                    var combinationPrice = combination.OverriddenPrice.HasValue && combination.OverriddenPrice.Value > 0
                        ? combination.OverriddenPrice.Value
                        : product.Price;

                    var combinationOldPrice = combination.OverriddenOldPrice.HasValue && combination.OverriddenPrice.Value > 0
                        ? combination.OverriddenOldPrice
                        : product.OldPrice > 0 ? product.OldPrice : null;

                    TryUpdateMaxDiscount(combinationPrice, combinationOldPrice);
                }
            }


            if (maxDiscountPercentage == 0 && product.OldPrice > 0 && product.OldPrice > product.Price)
                TryUpdateMaxDiscount(product.Price, product.OldPrice);

            return (Math.Round(maxDiscountAmount, 2), Math.Round(maxDiscountPercentage, 2));
        }
        protected async Task InsertProductVariantAsync(ProductVariant variant)
        {
            try
            {
                await _semaphoreProductVariantIdGenerate.WaitAsync();
                variant.VariantId = await GenerateVariantId();
                await _productVariantRepository.InsertAsync(variant);

            }
            finally
            {
                _semaphoreProductVariantIdGenerate.Release();
            }
        }
        protected async Task UpdateProductVariantAsync(ProductVariant variant)
        {
            await _productVariantRepository.UpdateAsync(variant);
        }
        protected async Task UpdateVariantCombinationAsync(VariantCombination variantcombination)
        {
            await _variantcombinationRepository.UpdateAsync(variantcombination);
        }
        protected async Task DeleteVariantCombinationAsync(VariantCombination variantcombination)
        {
            await _variantcombinationRepository.DeleteAsync(variantcombination);
        }
        protected async Task UpdateVariantCombinationLogAsync(VariantCombinationLog variantcombinationlog)
        {
            await _variantCombinationLogRepository.UpdateAsync(variantcombinationlog);
        }
        protected async Task<ProductVariant> GetProductVariantByAttributeValueAsync(int productId, int productAttributeId, string attrValue)
        {
            return await _productVariantRepository.Table.Where(x => x.ProductAttributeId == productAttributeId && x.ProductId == productId && x.AttributeValue.ToLower() == attrValue.ToLower()).FirstOrDefaultAsync();
        }
        protected async Task<List<ProductVariant>> GetAllProductVariantByProductIdAsync(int productId)
        {
            return await _productVariantRepository.Table.Where(x => x.ProductId == productId).ToListAsync();
        }
        protected async Task<ProductVariant> GetProductVariantByAttributeValueIdAsync(int attributevalueid)
        {
            return await _productVariantRepository.Table.Where(x => x.ProductAttributeValueId == attributevalueid).FirstOrDefaultAsync();
        }
        protected async Task InsertVariantCombinationAsync(VariantCombination vc)
        {
            await _variantcombinationRepository.InsertAsync(vc);
        }
        protected async Task<int> GenerateVariantId()
        {
            int variantId = (await _productVariantRepository.Table
     .Select(v => (int?)v.VariantId)
     .DefaultIfEmpty(0)
     .MaxAsync() ?? 0) + 1;

            return variantId;
        }
        protected VariantCombination GetVariantCombinationByProductAttributeValueIds(List<VariantCombination> combinations, string valueIds)
        {
            var valueIdsSet = new HashSet<string>((valueIds ?? string.Empty).Split('-'));

            foreach (var combination in combinations)
            {
                var combinationValueIdSet = new HashSet<string>((combination.ProductAttributeValueIds ?? string.Empty).Split('-'));

                // Check if both sets have exactly the same elements
                if (valueIdsSet.SetEquals(combinationValueIdSet))
                {
                    return combination;
                }
            }
            return null;
        }
        protected ProductVariant GetVariantByProductAttributeValueIds(List<ProductVariant> productVariants, string valueIds)
        {
            var valueIdsSet = new HashSet<string>((valueIds ?? string.Empty).Split('-')); // Convert input to HashSet

            foreach (var productVariant in productVariants)
            {

                if (valueIdsSet.Count == 1)
                {

                    if (productVariant.ProductAttributeValueId == Convert.ToInt32(valueIdsSet.First()))
                    {
                        return productVariant;
                    }
                }
                var combinationValueIdSet = new HashSet<string>((productVariant.ProductAttributeValueIds ?? string.Empty).Split('-')); // Convert combination to HashSet
                                                                                                                                       // Check if both sets have exactly the same elements
                if (valueIdsSet.SetEquals(combinationValueIdSet))
                {
                    return productVariant;
                }
            }
            return null;
        }
        protected void AssignPriceToProductAttributeCombination(VariantCombination variantCombination, IList<ProductAttributeCombination> combinations, Dictionary<int, List<int>> dicAttrCombinations, Product product)
        {
            if (string.IsNullOrWhiteSpace(variantCombination.ProductAttributeValueIds))
                return;

            var attrValueIds = new HashSet<int>(
                (variantCombination.ProductAttributeValueIds ?? string.Empty)
                    .Split('-')
                    .Select(s => int.TryParse(s, out int value) ? value : (int?)null)
                    .Where(v => v.HasValue)
                    .Select(v => v.Value)
            );
            foreach (var dicAttCombination in dicAttrCombinations)
            {
                if (attrValueIds.IsSubsetOf(new HashSet<int>(dicAttCombination.Value)))
                {

                    var combination = combinations.Where(c => c.Id == dicAttCombination.Key).First();
                    variantCombination.OldPrice = (decimal)(combination?.OverriddenOldPrice ?? product.OldPrice);
                    variantCombination.Msrp = (decimal)(combination?.OverriddenMsrp ?? product.Msrp);
                    variantCombination.Price = (decimal)(combination?.OverriddenPrice ?? product.Price);
                    break;
                }
            }
            if (variantCombination.Price == 0)
            {
                variantCombination.Price = product.Price;
                variantCombination.OldPrice = product.OldPrice;
                variantCombination.Msrp = product.Msrp;
            }

        }
        protected async Task InsertVariantCombinationLogAsync(VariantCombinationLog vcLog)
        {
            await _variantCombinationLogRepository.InsertAsync(vcLog);
        }
        protected async Task UpdateAttributeCombinationPrices(VariantCombination variant)
        {
            if (string.IsNullOrWhiteSpace(variant.ProductAttributeValueIds))
                return;

            var attrValueIds = new HashSet<int>(
                (variant.ProductAttributeValueIds ?? string.Empty)
                    .Split('-')
                    .Select(s => int.TryParse(s, out int value) ? value : (int?)null)
                    .Where(v => v.HasValue)
                    .Select(v => v.Value)
            );

            var productAttributesCombinations = await _customProductAttributeService.CustomGetAllProductAttributeCombinationsAsync(variant.ProductId);

            foreach (var _attrCombination in productAttributesCombinations)
            {
                var combinationAttributeValueIds = (await _productAttributeParser.ParseProductAttributeValuesAsync(_attrCombination.AttributesXml)).Select(x => x.Id).ToList();
                if (new HashSet<int>(attrValueIds).IsSubsetOf(combinationAttributeValueIds))
                {
                    _attrCombination.OverriddenOldPrice = variant.OldPrice;
                    _attrCombination.OverriddenPrice = variant.Price;
                    _attrCombination.OverriddenMsrp = variant.Msrp;
                    await _productAttributeService.UpdateProductAttributeCombinationAsync(_attrCombination);
                }
            }

            var product = await GetProductByIdAsync(variant.ProductId);
            await UpdateProductAsync(product);
        }

        #endregion

        #region Feed
        public async Task<IPagedList<Product>> ProductsFeedAsync(int pageIndex = 0, int pageSize = int.MaxValue, int storeId = 0)
        {
            //some databases don't support int.MaxValue
            if (pageSize == int.MaxValue)
                pageSize = int.MaxValue - 1;

            var productsQuery = _productRepository.Table;
            productsQuery = productsQuery.Where(p => p.Published);

            productsQuery = productsQuery.Where(p => !p.Deleted);
            //apply store mapping constraints
            productsQuery = await _storeMappingService.ApplyStoreMapping(productsQuery, storeId);
            productsQuery = productsQuery.Where(p => p.TotalInventory > 0);
            productsQuery = productsQuery.Where(p => p.VisiblityOncategoryPage == null || p.VisiblityOncategoryPage == 1);
            productsQuery = productsQuery.Where(p => !p.Sku.StartsWith("3000000"));
            productsQuery = productsQuery.OrderBy(p => p.Id);

            return await productsQuery.ToPagedListAsync(pageIndex, pageSize);

        }

        #endregion


        #region Collection Products
        public virtual async Task<IList<Product>> CustomGetCollectionProductsBygroupAsync(int productId, int parentGroupedProductId,
         int storeId = 0, int vendorId = 0, bool showHidden = false)
        {

            if (parentGroupedProductId == 0)
                return await this.GetAssociatedProductsAsync(productId, storeId, vendorId, showHidden);
            else
            {
                var query = _productRepository.Table;
                query = query.Where(x => x.ParentGroupedProductId == parentGroupedProductId || x.Id == parentGroupedProductId);
                if (!showHidden)
                {
                    query = query.Where(x => x.Published);

                    //available dates
                    query = query.Where(p =>
                        (!p.AvailableStartDateTimeUtc.HasValue || p.AvailableStartDateTimeUtc.Value < DateTime.UtcNow) &&
                        (!p.AvailableEndDateTimeUtc.HasValue || p.AvailableEndDateTimeUtc.Value > DateTime.UtcNow));
                }
                query = query.Where(m => m.Id != productId);
                //vendor filtering
                if (vendorId > 0)
                {
                    query = query.Where(p => p.VendorId == vendorId);
                }

                query = query.Where(x => !x.Deleted);
                query = query.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Id);

                var products = await query.ToListAsync();

                //ACL mapping
                if (!showHidden)
                    products = await products.WhereAwait(async x => await _aclService.AuthorizeAsync(x)).ToListAsync();

                //Store mapping
                if (!showHidden && storeId > 0)
                    products = await products.WhereAwait(async x => await _storeMappingService.AuthorizeAsync(x, storeId)).ToListAsync();

                return products;
            }

        }


        public virtual async Task<(IList<Product> products, int parentProductId)> CustomGetCollectionProductsAsync(int productId, int pageSize,
       int storeId = 0, int vendorId = 0, bool showHidden = false)
        {

            var product = await this.GetProductByIdAsync(productId);
            List<Product> products = new List<Product>();
            List<int> pids = await this.GetCollectionProductsByProductId1Async(productId, false);
            if (pids.Count > 0)
            {
                if (product.MainCollectionProductId > 0)
                {
                    var parentCollection = await this.GetProductByIdAsync(product.MainCollectionProductId);
                    if (parentCollection != null && !pids.Contains(parentCollection.Id) && parentCollection.Published && parentCollection.TotalInventory > 0)
                        pids.Add(product.MainCollectionProductId);
                }
                products = (List<Product>)await this.GetProductsByIdsAsync(pids.Take(pageSize).ToArray());
            }
            return (products, product.MainCollectionProductId);
        }

        public async Task<List<int>> GetCollectionProductsByProductId1Async(int productId, bool showHidden = false)
        {
            var query = from rp in _collectionRepository.Table
                        join p in _productRepository.Table on rp.ProductId2 equals p.Id
                        where rp.ProductId1 == productId &&
                        !p.Deleted &&
                        (showHidden || p.Published)
                        && p.TotalInventory > 0
                        orderby rp.DisplayOrder, rp.Id
                        select rp.ProductId2;

            var collectionProducts = await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(CustomNopCatalogDefaults.collectionProductsIdsCacheKey, productId, showHidden), async () => await query.ToListAsync());

            return collectionProducts;
        }

        public async Task<List<int>> GetCollectionAssocitedProductsAsync(int productId, bool showHidden = false)
        {

            return await this.GetCollectionProductsByProductId1Async(productId, false);
        }

        public virtual async Task<IList<CollectionProduct>> GetCollectionProductsByProductId1ListAsync(int productId, bool showHidden = false)
        {
            var query = from rp in _collectionRepository.Table
                        join p in _productRepository.Table on rp.ProductId2 equals p.Id
                        where rp.ProductId1 == productId &&
                        !p.Deleted &&
                        (showHidden || p.Published)
                        orderby rp.DisplayOrder, rp.Id
                        select rp;

            var collectionProducts = await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(CustomNopCatalogDefaults.collectionProductsCacheKey, productId, showHidden), async () => await query.ToListAsync());

            return collectionProducts;
        }

        public virtual async Task<CollectionProduct> GetCollectionProductByIdAsync(int relatedProductId)
        {
            return await _collectionRepository.GetByIdAsync(relatedProductId, cache => default);
        }

        public virtual async Task UpdateCollectionProductAsync(CollectionProduct CollectionProduct)
        {
            await _collectionRepository.UpdateAsync(CollectionProduct);
        }

        public virtual async Task DeleteCollectionProductAsync(CollectionProduct CollectionProduct)
        {
            await _collectionRepository.DeleteAsync(CollectionProduct);
        }
        public virtual CollectionProduct FindCollectionProduct(IList<CollectionProduct> source, int productId1, int productId2)
        {
            foreach (var collectionProduct in source)
                if (collectionProduct.ProductId1 == productId1 && collectionProduct.ProductId2 == productId2)
                    return collectionProduct;
            return null;
        }
        public virtual async Task InsertCollectionProductAsync(CollectionProduct collectionProduct)
        {
            await _collectionRepository.InsertAsync(collectionProduct);
        }


        #endregion

        #region PairWith

        public async Task<List<int>> GetPairWithProductsByProductId1Async(int productId, bool showHidden = false)
        {
            var query = from rp in _pairwithRepository.Table
                        join p in _productRepository.Table on rp.ProductId2 equals p.Id
                        where rp.ProductId1 == productId &&
                        !p.Deleted &&
                        (showHidden || p.Published)
                        && p.TotalInventory > 0
                        orderby rp.DisplayOrder, rp.Id
                        select rp.ProductId2;

            var collectionProducts = await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(CustomNopCatalogDefaults.PairWithProductsIdsCacheKey, productId, showHidden), async () => await query.ToListAsync());

            return collectionProducts;
        }

        public virtual async Task<IList<PairWithProduct>> GetPairWithProductsByProductId1ListAsync(int productId, bool showHidden = false)
        {
            var query = from rp in _pairwithRepository.Table
                        join p in _productRepository.Table on rp.ProductId2 equals p.Id
                        where rp.ProductId1 == productId &&
                        !p.Deleted &&
                        (showHidden || p.Published)
                        orderby rp.DisplayOrder, rp.Id
                        select rp;

            var pairwithProducts = await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(CustomNopCatalogDefaults.PairWithProductsCacheKey, productId, showHidden), async () => await query.ToListAsync());

            return pairwithProducts;
        }

        public virtual async Task<PairWithProduct> GetPairWithProductByIdAsync(int relatedProductId)
        {
            return await _pairwithRepository.GetByIdAsync(relatedProductId, cache => default);
        }

        public virtual async Task UpdatePairWithProductAsync(PairWithProduct pairwithProduct)
        {
            await _pairwithRepository.UpdateAsync(pairwithProduct);
        }

        public virtual async Task DeletePairWithProductAsync(PairWithProduct pairwithProduct)
        {
            await _pairwithRepository.DeleteAsync(pairwithProduct);
        }
        public virtual PairWithProduct FindPairWithProduct(IList<PairWithProduct> source, int productId1, int productId2)
        {
            foreach (var PairWithProduct in source)
                if (PairWithProduct.ProductId1 == productId1 && PairWithProduct.ProductId2 == productId2)
                    return PairWithProduct;
            return null;
        }
        public virtual async Task InsertPairWithProductAsync(PairWithProduct pairwithProduct)
        {
            await _pairwithRepository.InsertAsync(pairwithProduct);
        }
        #endregion

        #region FBTproduct
        public virtual async Task DeleteFBTProductAsync(FBTProduct FBTProduct)
        {
            await _fbtProductepository.DeleteAsync(FBTProduct);
        }
        public virtual async Task<IList<FBTProduct>> GetFBTProductsByProductId1Async(int productId, bool showHidden = false, bool showOutOfStock = true)
        {
            var query = from rp in _fbtProductepository.Table
                        join p in _productRepository.Table on rp.ProductId2 equals p.Id
                        where rp.ProductId1 == productId &&
                        !p.Deleted &&
                        (showHidden || p.Published)
                        && (showOutOfStock || p.TotalInventory > 0)
                        orderby rp.DisplayOrder, rp.Id
                        select rp;

            var FBTProducts = await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(CustomNopCatalogDefaults.FBTProductsCacheKey, productId, showHidden), async () => await query.ToListAsync());

            return FBTProducts;
        }
        public virtual async Task<FBTProduct> GetFBTProductByIdAsync(int FBTProductId)
        {
            return await _fbtProductepository.GetByIdAsync(FBTProductId, cache => default);
        }

        public virtual async Task InsertFBTProductAsync(FBTProduct FBTProduct)
        {
            await _fbtProductepository.InsertAsync(FBTProduct);
        }
        public virtual async Task UpdateFBTProductAsync(FBTProduct FBTProduct)
        {
            await _fbtProductepository.UpdateAsync(FBTProduct);
        }
        public virtual FBTProduct FindFBTProduct(IList<FBTProduct> source, int productId1, int productId2)
        {
            foreach (var FBTProduct in source)
                if (FBTProduct.ProductId1 == productId1 && FBTProduct.ProductId2 == productId2)
                    return FBTProduct;
            return null;
        }

        #endregion

        #region Misc
        public virtual async Task<IList<Product>> GetRecommendationProducts(int[] productIds, int numberOfRecommendedProducts)
        {
            List<int> lstRelatedproductIds = new List<int>();
            foreach (var productId in productIds)
            {
                var _relatedProductIds = (await GetRelatedProductsByProductId1Async(productId)).Select(x => x.ProductId2).ToArray();
                foreach (var relatedProductId in _relatedProductIds)
                {
                    if (!lstRelatedproductIds.Contains(relatedProductId))
                    {
                        if (lstRelatedproductIds.Count >= numberOfRecommendedProducts)
                            break;

                        lstRelatedproductIds.Add(relatedProductId);
                    }
                }
                if (lstRelatedproductIds.Count >= numberOfRecommendedProducts)
                    break;

            }

            return await GetProductsByIdsAsync(lstRelatedproductIds.OrderBy(o => o).ToArray());

        }
        public virtual async Task<IList<Product>> GetShoppingCartRecommendationProducts(int[] productIds, int numberOfRecommendedProducts)
        {
            List<int> lstRelatedproductIds = new List<int>();
            foreach (var productId in productIds)
            {
                var _productIds = (await GetCrossSellProductsByProductId1Async(productId)).Select(x => x.ProductId2).ToArray();
                if (_productIds.Count() == 0)
                    _productIds = (await GetRelatedProductsByProductId1Async(productId)).Select(x => x.ProductId2).ToArray();
                foreach (var relatedProductId in _productIds)
                {
                    if (!lstRelatedproductIds.Contains(relatedProductId))
                    {
                        if (lstRelatedproductIds.Count >= numberOfRecommendedProducts)
                            break;

                        lstRelatedproductIds.Add(relatedProductId);
                    }
                }
                if (lstRelatedproductIds.Count >= numberOfRecommendedProducts)
                    break;

            }

            return await GetProductsByIdsAsync(lstRelatedproductIds.ToArray());

        }
        public async Task<(string offerText, string offerPlaceHolder, string discountAmount, decimal discountPercentage, DateTime? saleStartDate, DateTime? saleEndDate)> GetProductSaleOfferInfo(Product product, decimal? oldPrice, decimal? price)
        {
            var productOfferInfo = await _storeWideDiscountService.GetStoreWideProductDiscountInfoByProductIdAsync(product.Id);

            if (productOfferInfo == null)
                return (string.Empty, string.Empty, string.Empty, 0, null, null);

            var offerInfo = await _storeWideDiscountService.GetStoreWideDiscountByIdAsync(productOfferInfo.StoreWideDiscountId);

            if (offerInfo == null)
                return (string.Empty, string.Empty, string.Empty, 0, null, null);


            string offerText = productOfferInfo?.InfoText ?? "";
            (decimal discountAmount, decimal discountPercentage) = await GetMaxDiscountAsync(product, oldPrice, price);

            return (offerText, await _localizationService.GetResourceAsync("label.limitedoffer.v2.placeholder"),
    await _priceFormatter.FormatPriceAsync(discountAmount), discountPercentage,
                    offerInfo.StartDate, offerInfo.EndDate);
        }
        public virtual async Task<IList<Product>> GetNewArrivalProductsAsync(int pageIndex, int pageSize)
        {
            var query = from p in _productRepository.Table
                        orderby p.DisplayOrder, p.Id
                        where p.Published &&
                              !p.Deleted && p.TotalInventory > 0
                        orderby p.Id descending
                        select p;

            var result = await query.ToPagedListAsync(pageIndex, pageSize);

            return result;
        }
        public async Task<List<Product>> GetGroupProductsByEntity(int categoryId, int storeId)
        {
            var productsQuery = _productRepository.Table;
            productsQuery = productsQuery.Where(p => p.Published);
            productsQuery = productsQuery.Where(p => !p.Deleted);
            productsQuery = productsQuery.Where(p => p.ProductTypeId == 10);
            //apply store mapping constraints
            productsQuery = await _storeMappingService.ApplyStoreMapping(productsQuery, storeId);

            var customer = await _workContext.GetCurrentCustomerAsync();
            productsQuery = await _aclService.ApplyAcl(productsQuery, customer);
            //productsQuery = productsQuery.Where(p => p.TotalInventory > 0);
            productsQuery =
                       from p in productsQuery
                       join pc in _productCategoryRepository.Table on p.Id equals pc.ProductId
                       where pc.CategoryId == categoryId
                       orderby pc.DisplayOrder, p.Name
                       select p;

            var query =
                from p in _productRepository.Table
                join q in productsQuery
                on p.ParentGroupedProductId equals q.Id
                where p.Published
                && !p.Deleted
                && p.ProductTypeId != 10
                && p.TotalInventory > 0
                select p
                ;
            var associatedproducts = await query.ToListAsync();
            var groupProducts = await productsQuery.ToListAsync();
            return groupProducts.Concat(associatedproducts).ToList();


        }
        public virtual async Task<IList<int>> GetCustomRelatedProductsByProductId1Async(int productId, int pageSize, bool showHidden = false)
        {

            var relatedProducts = await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(CustomNopCatalogDefaults.CustomRelatedProductsCacheKey, productId, showHidden), async () =>

            {
                var query = from rp in _relatedProductRepository.Table
                            join p in _productRepository.Table on rp.ProductId2 equals p.Id
                            where rp.ProductId1 == productId && rp.ProductId2 != productId &&
                            !p.Deleted &&
                            (showHidden || p.Published)
                            && p.TotalInventory > 0
                            orderby rp.DisplayOrder, rp.Id
                            select rp;
                var relatedProducts = (await query.Take(pageSize).ToListAsync()).Select(rl => rl.ProductId2).ToList();
                if (relatedProducts.Count < pageSize)
                {
                    if ((await this.GetProductByIdAsync(productId)).EnableCategoryWiseSimilarProducts)
                    {

                        var category = (await this._customCategoryService.GetProductCategoriesByProductIdExcludingRootLevelCategoriesAsync(productId)).FirstOrDefault();
                        if (category != null)
                        {
                            var prdCategoryQuery = from pc in _productCategoryRepository.Table
                                                   join p in _productRepository.Table on pc.ProductId equals p.Id
                                                   where pc.CategoryId == category.CategoryId && pc.ProductId != productId &&
                                                   !p.Deleted &&
                                                   (showHidden || p.Published)
                                                   && !relatedProducts.Contains(pc.ProductId)

                                                    && p.TotalInventory > 0

                                                   orderby pc.DisplayOrder
                                                   select pc.ProductId;
                            relatedProducts = (relatedProducts.Concat(await prdCategoryQuery.Take(pageSize - relatedProducts.Count()).ToListAsync())).ToList();

                        }
                    }

                }
                return relatedProducts;
            });


            return relatedProducts;
        }
        public async Task<IList<Product>> GetServiceTypeProducts()
        {
            return await _productRepository.Table.Where(p => p.IsServiceTypeProduct).ToListAsync();
        }


        #endregion

        #region Schedule Task Functions For Grouped Product
        public async Task<bool> SyncGroupedProductsPrice()
        {
            try
            {
                var productIds = await (from prd in _productRepository.Table
                                        where prd.ProductTypeId == (int)ProductType.GroupedProduct
                                        select prd.Id).ToListAsync();

                foreach (var productId in productIds)
                {
                    await this.UpdateGroupProductPrice(productId);
                }
            }
            catch (Exception ex)
            {

            }
            return true;
        }
        public async Task<bool> UpdateGroupProductPrice(int productID)
        {

            var products = await this.GetAssociatedProductsAsync(productID, (await _storeContext.GetCurrentStoreAsync()).Id);
            if (products.Count > 0)
            {

                decimal totalPrice = 0;
                decimal totalOldPrice = 0;
                decimal totalMsrp = 0;
                decimal readMsrp = 0;
                decimal minOldPrice = 0;
                decimal maxOldPrice = 0;
                decimal minPrice = 0;
                decimal maxPrice = 0;
                decimal minMsrp = 0;
                decimal maxMsrp = 0;
                bool isVariantPrice = false;
                bool isAttributeProduct = false;
                foreach (var _product in products)
                {
                    (bool _isVariantPrice, decimal _minOldPrice, decimal _maxOldPrice, decimal _minPrice, decimal _maxPrice,
                       decimal _minMsrp, decimal _maxMsrp)
                       = await GetVariantPriceRange(_product, false, false);
                    isVariantPrice = isVariantPrice == true ? true : _isVariantPrice;


                    minOldPrice = minOldPrice + (_minOldPrice == 0 ? _minPrice : _minOldPrice);
                    maxOldPrice = maxOldPrice + (_maxOldPrice == 0 ? _maxPrice : _maxOldPrice);

                    minPrice = minPrice + _minPrice;
                    maxPrice = maxPrice + _maxPrice;

                    minMsrp = minMsrp + (_minMsrp == 0 ? _minOldPrice == 0 ? _minPrice : _minOldPrice : _minMsrp);
                    maxMsrp = maxMsrp + (_maxMsrp == 0 ? _maxOldPrice == 0 ? _maxPrice : _maxOldPrice : _maxMsrp);

                    isAttributeProduct = false;
                    var result = await GetDefaultAttributePriceOfProduct(_product);
                    decimal msrp = result.Item1;
                    decimal oldPrice = result.Item2;
                    decimal price = result.Item3;
                    isAttributeProduct = result.Item4;

                    if (!isAttributeProduct)
                    {
                        totalOldPrice = totalOldPrice + (_product.OldPrice == 0 ? _product.Price : _product.OldPrice);
                        totalPrice = totalPrice + _product.Price;
                        totalMsrp = totalMsrp + (_product.Msrp == 0 ? _product.Price : _product.Msrp);
                        readMsrp = readMsrp + _product.Msrp;
                    }
                    else
                    {
                        totalOldPrice = totalOldPrice + (oldPrice == 0 ? price : oldPrice);
                        totalPrice = totalPrice + price;
                        totalMsrp = totalMsrp + (msrp == 0 ? price : msrp);
                        readMsrp = readMsrp + msrp;
                    }
                }
                var product = await this.GetProductByIdAsync(productID);
                product.OldPrice = totalOldPrice;
                product.Price = totalPrice;
                product.Msrp = readMsrp == 0 ? 0 : totalMsrp;
                product.IsVariantProduct = isAttributeProduct;
                product.MinMsrp = minMsrp;
                product.MaxMsrp = maxMsrp;
                product.MinPrice = minPrice;
                product.MaxPrice = maxPrice;
                product.MinOldprice = minOldPrice;
                product.MaxOldPrice = maxOldPrice;
                await this.UpdateProductAsync(product);
            }
            return true;
        }
        public async Task<(decimal, decimal, decimal, bool)> GetDefaultAttributePriceOfProduct(Product product)
        {
            bool isAttributeProduct = false;
            decimal price = 0;
            decimal msrp = 0;
            decimal oldPrice = 0;
            var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
            if (productAttributeMapping.Count > 0)
            {
                isAttributeProduct = true;
                Dictionary<int, string> attrMappings = new Dictionary<int, string>();
                foreach (var mapping in productAttributeMapping)
                {

                    var attributeValues = await _productAttributeService.GetProductAttributeValuesAsync(mapping.Id);
                    if (!attributeValues.Where(a => a.IsPreSelected).Any())
                    {
                        isAttributeProduct = false;
                        break;
                    }
                    else
                        attrMappings.Add(mapping.Id, string.Join(',', attributeValues.Where(a => a.IsPreSelected).OrderByDescending(s => s.DisplayOrder).Select(s => s.Id).ToArray()));

                }
                if (isAttributeProduct)
                {
                    string attributeXml = await this.GetCustomProductAttributesXmlAsync(productAttributeMapping, attrMappings);

                    var _attributeValues = await _productAttributeParser.ParseProductAttributeValuesAsync(attributeXml);

                    var combination = await _productAttributeParser.FindProductAttributeCombinationAsync(product, attributeXml);
                    if (combination?.OverriddenPrice.HasValue ?? false)
                    {
                        oldPrice = combination.OverriddenOldPrice.Value;
                        price = combination.OverriddenPrice.Value;
                        msrp = combination.OverriddenMsrp ?? 0;
                    }
                    else
                        isAttributeProduct = false;
                }
            }
            return (msrp, oldPrice, price, isAttributeProduct);
        }

        #endregion

        #region Schedule Task Functions for Variant Price


        public async Task<(bool, decimal, decimal, decimal, decimal, decimal, decimal)> GetVariantPriceRange(Product product, bool createCombination = true, bool updateProduct = false, bool updateProductWithEvent = true, bool isVariantTask = true)
        {
            bool isVariantPrice = false;
            decimal minOldPrice = 0;
            decimal maxOldPrice = 0;
            decimal minPrice = 0;
            decimal maxPrice = 0;
            decimal minMsrp = 0;
            decimal maxMsrp = 0;
            if (product.ProductType != ProductType.GroupedProduct)
            {
                var productAttributeMapping = await _customProductAttributeService.CustomGetProductAttributeMappingsByProductIdAsync(product.Id);
                if (productAttributeMapping.Count() > 0)
                {
                    isVariantPrice = true;
                    #region Genrate Combination
                    //if (product.UpdatedOnUtc > DateTime.UtcNow.AddDays(-2) && createCombination)
                    //{
                    //    var allAttributesXml = await _productAttributeParser.GenerateAllCombinationsAsync(product, true, null);
                    //    foreach (var attributesXml in allAttributesXml)
                    //    {
                    //        var existingCombination = await _productAttributeParser.FindProductAttributeCombinationAsync(product, attributesXml);

                    //        //already exists?
                    //        if (existingCombination != null)
                    //            continue;

                    //        //new one
                    //        var warnings = new List<string>();
                    //        warnings.AddRange(await _shoppingCartService.GetShoppingCartItemAttributeWarningsAsync(await _workContext.GetCurrentCustomerAsync(),
                    //            ShoppingCartType.ShoppingCart, product, 1, attributesXml, true, true));
                    //        if (warnings.Count != 0)
                    //            continue;

                    //        //save combination
                    //        var combination = new ProductAttributeCombination
                    //        {
                    //            ProductId = product.Id,
                    //            AttributesXml = attributesXml,
                    //            StockQuantity = 0,
                    //            AllowOutOfStockOrders = false,
                    //            Sku = null,
                    //            ManufacturerPartNumber = null,
                    //            Gtin = null,
                    //            OverriddenPrice = product.Price,
                    //            OverriddenOldPrice = product.OldPrice,
                    //            OverriddenMsrp = product.Msrp,
                    //            NotifyAdminForQuantityBelow = 1,
                    //            PictureId = 0
                    //        };
                    //        await _productAttributeService.InsertProductAttributeCombinationAsync(combination);
                    //    }
                    //}
                    var combinations = await _customProductAttributeService.CustomGetAllProductAttributeCombinationsAsync(product.Id);

                    decimal oldPrice = 0;
                    decimal price = 0;
                    decimal msrp = 0;
                    decimal defaulCombinationOldPrice = product.OldPrice;
                    decimal defaultCombinationPrice = product.Price;
                    decimal defaultCombinationMsrp = product.Msrp;
                    string defaultCombinationAttrXml = await this.GetDefaultCombinationAttrXml(productAttributeMapping);
                    foreach (var combination in combinations)
                    {
                        var attributes = await _productAttributeParser.ParseProductAttributeMappingsAsync(combination.AttributesXml);
                        if (!attributes.Any())
                            continue;

                        bool isAttributeValid = true;
                        foreach (var attribute in attributes)
                        {
                            if (isAttributeValid)
                            {
                                if (!attribute.ShouldHaveValues())
                                {
                                    isAttributeValid = false;
                                    break;
                                }

                                foreach (var attributeValue in _customProductAttributeParser.CustomParseValuesWithQuantity(combination.AttributesXml, attribute.Id))
                                {
                                    if (string.IsNullOrEmpty(attributeValue.Item1) || !int.TryParse(attributeValue.Item1, out var attributeValueId))
                                    {
                                        isAttributeValid = false;
                                        break;
                                    }
                                    var value = await _productAttributeService.GetProductAttributeValueByIdAsync(attributeValueId);
                                    if (value == null || !value.Published)
                                    {
                                        isAttributeValid = false;
                                        break;
                                    }
                                }
                            }
                        }
                        if (!isAttributeValid)
                            continue;
                        if (combination.AttributesXml == defaultCombinationAttrXml)
                        {
                            defaulCombinationOldPrice = (combination.OverriddenOldPrice ?? 0) == 0 ?
                                ((combination.OverriddenPrice ?? 0) == 0 ? defaulCombinationOldPrice : 0) : Convert.ToDecimal(combination.OverriddenOldPrice);
                            defaultCombinationPrice = combination.OverriddenPrice ?? defaultCombinationPrice;
                            defaultCombinationMsrp = (combination.OverriddenMsrp ?? 0) == 0 ?
                                ((combination.OverriddenPrice ?? 0) == 0 ? defaultCombinationMsrp : 0) : Convert.ToDecimal(combination.OverriddenMsrp);

                        }
                        oldPrice = (combination.OverriddenOldPrice ?? 0) == 0 ? product.OldPrice : combination.OverriddenOldPrice ?? 0;
                        price = (combination.OverriddenPrice ?? 0) == 0 ? product.Price : combination.OverriddenPrice ?? 0;
                        msrp = (combination.OverriddenMsrp ?? 0) == 0 ? product.Msrp : combination.OverriddenMsrp ?? 0;

                        minOldPrice = minOldPrice == 0 ? oldPrice :
                            (minOldPrice < oldPrice || oldPrice == 0
                            ? minOldPrice : oldPrice);

                        maxOldPrice = maxOldPrice == 0 ? oldPrice :
                           (maxOldPrice > oldPrice ? maxOldPrice : oldPrice);

                        minPrice = minPrice == 0 ? price :
                       (minPrice < price || price == 0 ? minPrice : price);


                        maxPrice = maxPrice == 0 ? price :
                           (maxPrice > price ? maxPrice : price);

                        minMsrp = minMsrp == 0 ? msrp :
                    (minMsrp < msrp || msrp == 0 ? minMsrp : msrp);
                        maxMsrp = maxMsrp == 0 ? msrp :
                           (maxMsrp > msrp ? maxMsrp : msrp);
                    }

                    #endregion

                    minMsrp = minMsrp == 0 ? product.Msrp : minMsrp;
                    maxMsrp = maxMsrp == 0 ? product.Msrp : maxMsrp;
                    minOldPrice = minOldPrice == 0 ? product.OldPrice : minOldPrice;
                    maxOldPrice = maxOldPrice == 0 ? product.OldPrice : maxOldPrice;
                    minPrice = minPrice == 0 ? product.Price : minPrice;
                    maxPrice = maxPrice == 0 ? product.Price : maxPrice;

                    #region Update Product
                    if (updateProduct && (
                        product.MinMsrp != minMsrp
                        || product.MaxMsrp != maxMsrp
                        || product.MinPrice != minPrice
                        || product.MaxPrice != maxPrice
                        || product.MinOldprice != minOldPrice
                        || product.MaxOldPrice != maxOldPrice))
                    {
                        product.MinMsrp = minMsrp == 0 ? product.Msrp : minMsrp;
                        product.MaxMsrp = maxMsrp == 0 ? product.Msrp : maxMsrp;
                        product.MinOldprice = minOldPrice == 0 ? product.OldPrice : minOldPrice;
                        product.MaxOldPrice = maxOldPrice == 0 ? product.OldPrice : maxOldPrice;
                        product.MinPrice = minPrice == 0 ? product.Price : minPrice;
                        product.MaxPrice = maxPrice == 0 ? product.Price : maxPrice;
                        product.Price = defaultCombinationPrice <= 0 ? product.Price : defaultCombinationPrice;
                        product.OldPrice = defaulCombinationOldPrice <= 0 ? product.OldPrice : defaulCombinationOldPrice;
                        product.Msrp = defaultCombinationMsrp <= 0 ? product.Msrp : defaultCombinationMsrp;
                        product.IsVariantProduct = true;
                        if (updateProductWithEvent)
                        {
                            await this.UpdateProductAsync(product);
                        }
                        else
                        {
                            await this.UpdateProductWithoutEvent(product);
                        }
                        if (isVariantTask)
                        {
                            await _loggerService.InsertLogAsync(LogLevel.Information, "Variant product Task ",
                                $"Product marked as variant product and Variant Price Update for Product info: {minPrice} - {maxPrice} -{minOldPrice}-{maxOldPrice}- {minMsrp}-{maxMsrp} " + product.Id);
                        }
                    }
                    else if (defaultCombinationPrice != product.Price || defaulCombinationOldPrice != product.OldPrice
                        || defaultCombinationMsrp != product.Msrp)
                    {

                        product.Price = defaultCombinationPrice <= 0 ? product.Price : defaultCombinationPrice;
                        product.OldPrice = defaulCombinationOldPrice <= 0 ? product.OldPrice : defaulCombinationOldPrice;
                        product.Msrp = defaultCombinationMsrp <= 0 ? product.Msrp : defaultCombinationMsrp;
                        if (updateProductWithEvent)
                        {
                            await this.UpdateProductAsync(product);
                        }
                        else
                        {
                            await this.UpdateProductWithoutEvent(product);
                        }
                    }

                    #endregion
                }
                else
                {
                    if (updateProduct && product.IsVariantProduct)
                    {
                        product.MinMsrp = minMsrp;
                        product.MaxMsrp = maxMsrp;
                        product.MinOldprice = minOldPrice;
                        product.MaxOldPrice = maxOldPrice;
                        product.MinPrice = minPrice;
                        product.MaxPrice = maxPrice;
                        product.IsVariantProduct = false;
                        if (updateProductWithEvent)
                        {
                            await this.UpdateProductAsync(product);
                        }
                        else
                        {
                            await this.UpdateProductWithoutEvent(product);

                        }
                        if (isVariantTask)
                        {
                            await _loggerService.InsertLogAsync(LogLevel.Information, "Variant product Task ",
                   $"Product marked as simple product" + product.Id);
                        }
                    }
                }
            }
            else if (updateProduct && product.IsVariantProduct)
            {
                await this.UpdateGroupProductPrice(product.Id);
                //           product.MinMsrp = minMsrp;
                //           product.MaxMsrp = maxMsrp;
                //           product.MinOldprice = minOldPrice;
                //           product.MaxOldPrice = maxOldPrice;
                //           product.MinPrice = minPrice;
                //           product.MaxPrice = maxPrice;
                //           product.IsVariantProduct = false;
                //           await this.UpdateProductAsync(product);
                //           await _loggerService.InsertLogAsync(Core.Domain.Logging.LogLevel.Information, "Variant product Task ",
                //$"Product marked as simple product" + product.Id);
            }
            return (isVariantPrice, minOldPrice, maxOldPrice, minPrice, maxPrice, minMsrp, maxMsrp);
        }


        #endregion
       

    }
}
