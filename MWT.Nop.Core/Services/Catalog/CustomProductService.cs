using Microsoft.Extensions.Primitives;
using MWT.Nop.Core.Domain;
using MWT.Nop.Core.Domain.Catalog;
using MWT.Nop.Core.Domain.KW;
using MWT.Nop.Core.Domain.QA;
using MWT.Nop.Core.Domain.Security;
using MWT.Nop.Core.Service.StoreWideDiscount;
using MWT.Nop.Core.Services.Catalog; 
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
        protected readonly IStaticCacheManager _staticCacheManager; 
        protected readonly INopDataProvider _dataProvider;
        private readonly ICustomProductAttributeParser _customProductAttributeParser;
     
        public CustomProductService(CatalogSettings catalogSettings, IAclService aclService, ICustomerService customerService, IDateRangeService dateRangeService, ILanguageService languageService, ILocalizationService localizationService, IProductAttributeParser productAttributeParser, IProductAttributeService productAttributeService, IRepository<Category> categoryRepository, IRepository<CrossSellProduct> crossSellProductRepository, IRepository<DiscountProductMapping> discountProductMappingRepository, IRepository<LocalizedProperty> localizedPropertyRepository, IRepository<Manufacturer> manufacturerRepository, IRepository<Product> productRepository, IRepository<ProductAttributeCombination> productAttributeCombinationRepository, IRepository<ProductAttributeMapping> productAttributeMappingRepository, IRepository<ProductCategory> productCategoryRepository, IRepository<ProductManufacturer> productManufacturerRepository, IRepository<ProductPicture> productPictureRepository, IRepository<ProductProductTagMapping> productTagMappingRepository, IRepository<ProductSpecificationAttribute> productSpecificationAttributeRepository, IRepository<ProductTag> productTagRepository, IRepository<ProductVideo> productVideoRepository, IRepository<ProductWarehouseInventory> productWarehouseInventoryRepository, IRepository<RelatedProduct> relatedProductRepository, IRepository<Shipment> shipmentRepository, IRepository<StockQuantityHistory> stockQuantityHistoryRepository, IRepository<TierPrice> tierPriceRepository, ISearchPluginManager searchPluginManager, IStaticCacheManager staticCacheManager, IVendorService vendorService, IStoreMappingService storeMappingService, IWorkContext workContext, LocalizationSettings localizationSettings
            , INopDataProvider dataProvider, ICustomProductAttributeParser customProductAttributeParser) : base(catalogSettings, aclService, customerService, dateRangeService, languageService, localizationService, productAttributeParser, productAttributeService, categoryRepository, crossSellProductRepository, discountProductMappingRepository, localizedPropertyRepository, manufacturerRepository, productRepository, productAttributeCombinationRepository, productAttributeMappingRepository, productCategoryRepository, productManufacturerRepository, productPictureRepository, productTagMappingRepository, productSpecificationAttributeRepository, productTagRepository, productVideoRepository, productWarehouseInventoryRepository, relatedProductRepository, shipmentRepository, stockQuantityHistoryRepository, tierPriceRepository, searchPluginManager, staticCacheManager, vendorService, storeMappingService, workContext, localizationSettings)
        {
            _staticCacheManager = staticCacheManager;
            _dataProvider = dataProvider;
            _customProductAttributeParser = customProductAttributeParser;
      
        }


        private static readonly SemaphoreSlim _semaphoreProductVariantIdGenerate = new SemaphoreSlim(1, 1);
        #region Product Basic Info
        public async Task<IList<ProductBasicInfo>> GetAllProducts()
        {
            return await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(CustomNopCatalogDefaults.ProductsBasicInfoCacheKey), async () =>
            {
                var queryResult = await _dataProvider.QueryProcAsync<ProductBasicInfo>("Sp_Get_All_Products");
                return queryResult.ToList();
            });
        }

        #endregion
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


                        var _relatedProductRepository = EngineContext.Current.Resolve<IRepository<RelatedProduct>>();

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
                        var _productKwTermRepository = EngineContext.Current.Resolve<IRepository<ProductKwTerm>>();
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
                        var _productQuestionAnswerRepository = EngineContext.Current.Resolve<IRepository<ProductQuestionAnswer>>();
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


        public virtual async Task<IList<Product>> GetCollectionProductsBygroupAsync(int productId, int parentGroupedProductId,
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

        public async Task<List<int>> GetCollectionProductsByProductId1Async(int productId, bool showHidden = false)
        {
            var _collectionRepository = EngineContext.Current.Resolve<IRepository<CollectionProduct>>();
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
            var _collectionRepository = EngineContext.Current.Resolve<IRepository<CollectionProduct>>();
            return await this.GetCollectionProductsByProductId1Async(productId, false);
        }
        public virtual async Task<IList<CollectionProduct>> GetCollectionProductsByProductId1ListAsync(int productId, bool showHidden = false)
        {
            var _collectionRepository = EngineContext.Current.Resolve<IRepository<CollectionProduct>>();
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
            var _collectionRepository = EngineContext.Current.Resolve<IRepository<CollectionProduct>>();
            return await _collectionRepository.GetByIdAsync(relatedProductId, cache => default);
        }
        public virtual async Task UpdateCollectionProductAsync(CollectionProduct CollectionProduct)
        {
            var _collectionRepository = EngineContext.Current.Resolve<IRepository<CollectionProduct>>();
            await _collectionRepository.UpdateAsync(CollectionProduct);
        }
        public virtual async Task DeleteCollectionProductAsync(CollectionProduct CollectionProduct)
        {
            var _collectionRepository = EngineContext.Current.Resolve<IRepository<CollectionProduct>>();
            await _collectionRepository.DeleteAsync(CollectionProduct);
        }
        public virtual async Task InsertCollectionProductAsync(CollectionProduct collectionProduct)
        {
            var _collectionRepository = EngineContext.Current.Resolve<IRepository<CollectionProduct>>();
            await _collectionRepository.InsertAsync(collectionProduct);
        }

        public virtual async Task<(IList<Product> products, int parentProductId)> GetCollectionProductsAsync(int productId, int pageSize,
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
                var fetchedProducts = await this.GetProductsByIdsAsync(pids.Take(pageSize).ToArray());
                products = fetchedProducts.ToList();
            }
            return (products, product.MainCollectionProductId);
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


        public virtual async Task<List<ProductSpecificationAttribute>> SearchGetProductSpecificationAttributeAsync(
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
                var _productKwTermRepository = EngineContext.Current.Resolve<IRepository<ProductKwTerm>>();
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
                var _productQuestionAnswerRepository = EngineContext.Current.Resolve<IRepository<ProductQuestionAnswer>>();
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


        public virtual async Task<IPagedList<Product>> SearchProductsAsync(int[] ids, List<int> _specificationAttributes, ProductSortingEnum orderBy = ProductSortingEnum.Position,
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


                        var _relatedProductRepository = EngineContext.Current.Resolve<IRepository<RelatedProduct>>();

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
                        var _productKwTermRepository = EngineContext.Current.Resolve<IRepository<ProductKwTerm>>();
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
                        var _productQuestionAnswerRepository = EngineContext.Current.Resolve<IRepository<ProductQuestionAnswer>>();
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

        public virtual async Task<IPagedList<Product>> SearchProductsWithVariablePageSizeAsync(int[] ids, List<int> _specificationAttributes, ProductSortingEnum orderBy = ProductSortingEnum.Position,
                      int pageIndex = 0,
               int pageSize = int.MaxValue, int firstPageSize = int.MaxValue, int subsequentPageSize =
          int.MaxValue, int categoryId = 0, int featuredId = 0, bool featuredListingDisplaySimilarOnTop =
          false, bool isMobileDevice = false, int kwTermId = 0, int questionAnswerId = 0)
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


                        var _relatedProductRepository = EngineContext.Current.Resolve<IRepository<RelatedProduct>>();

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
                        var _productKwTermRepository = EngineContext.Current.Resolve<IRepository<ProductKwTerm>>();
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
                        var _productQuestionAnswerRepository = EngineContext.Current.Resolve<IRepository<ProductQuestionAnswer>>();
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


        #region PairWith
        public async Task<List<int>> GetPairWithProductsByProductId1Async(int productId, bool showHidden = false)
        {
            var _pairwithRepository = EngineContext.Current.Resolve<IRepository<PairWithProduct>>();
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
            var _pairwithRepository = EngineContext.Current.Resolve<IRepository<PairWithProduct>>();
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
            var _pairwithRepository = EngineContext.Current.Resolve<IRepository<PairWithProduct>>();
            return await _pairwithRepository.GetByIdAsync(relatedProductId, cache => default);
        }

        public virtual async Task UpdatePairWithProductAsync(PairWithProduct pairwithProduct)
        {
            var _pairwithRepository = EngineContext.Current.Resolve<IRepository<PairWithProduct>>();
            await _pairwithRepository.UpdateAsync(pairwithProduct);
        }
        public virtual async Task DeletePairWithProductAsync(PairWithProduct pairwithProduct)
        {
            var _pairwithRepository = EngineContext.Current.Resolve<IRepository<PairWithProduct>>();
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
            var _pairwithRepository = EngineContext.Current.Resolve<IRepository<PairWithProduct>>();
            await _pairwithRepository.InsertAsync(pairwithProduct);
        }
        #endregion

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

        public virtual async Task<IList<Product>> GetRecommendationProducts(int[]
     productIds, int numberOfRecommendedProducts)
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
        public virtual async Task<IList<Product>> GetShoppingCartRecommendationProducts(int[]
         productIds, int numberOfRecommendedProducts)
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

        #region FBT
        public virtual async Task DeleteFBTProductAsync(FBTProduct FBTProduct)
        {
            var _FBTProductepository = EngineContext.Current.Resolve<IRepository<FBTProduct>>();
            await _FBTProductepository.DeleteAsync(FBTProduct);
        }
        public virtual async Task<IList<FBTProduct>> GetFBTProductsByProductId1Async(int productId,
    bool showHidden = false, bool showOutOfStock = true)
        {
            var _FBTProductepository = EngineContext.Current.Resolve<IRepository<FBTProduct>>();
            var query = from rp in _FBTProductepository.Table
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
            var _FBTProductepository = EngineContext.Current.Resolve<IRepository<FBTProduct>>();
            return await _FBTProductepository.GetByIdAsync(FBTProductId, cache => default);
        }

        public virtual async Task UpdateFBTProductAsync(FBTProduct FBTProduct)
        {
            var _FBTProductepository = EngineContext.Current.Resolve<IRepository<FBTProduct>>();
            await _FBTProductepository.UpdateAsync(FBTProduct);
        }

        public virtual async Task InsertFBTProductAsync(FBTProduct FBTProduct)
        {
            var _FBTProductepository = EngineContext.Current.Resolve<IRepository<FBTProduct>>();
            await _FBTProductepository.InsertAsync(FBTProduct);
        }
        public virtual FBTProduct FindFBTProduct(IList<FBTProduct> source, int productId1, int productId2)
        {
            foreach (var FBTProduct in source)
                if (FBTProduct.ProductId1 == productId1 && FBTProduct.ProductId2 == productId2)
                    return FBTProduct;
            return null;
        }

        #endregion

        public async Task<IList<Product>> GetServiceTypeProducts()
        {
            return await _productRepository.Table.Where(p => p.IsServiceTypeProduct).ToListAsync();
        }
    
        public async Task UpdateProductWithoutEvent(Product product)
        {
            await _productRepository.UpdateAsync(product, false);
        }
        public async Task<Product> GetProductBySearchTerm(string term)
        {
            return await (from _prd in _productRepository.Table
                          where _prd.Id.ToString() == term
                          || _prd.Sku == term
                          && _prd.Published == true && _prd.Deleted == false
                          select _prd).FirstOrDefaultAsync();
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
                var combinations = await _productAttributeService.GetAllProductAttributeCombinationsAsync(product.Id);
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

            return await productsQuery.CustomOrderBy(orderBy).ToPagedListAsync(pageIndex, pageSize);
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

            var _categoryUserMappingRepository = EngineContext.Current.Resolve<IRepository<CategoryUserMapping>>();
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

            return await productsQuery.CustomOrderBy(orderBy).ToPagedListAsync(pageIndex, pageSize);
        }

         

        #region SaleInfo

     

        #endregion
 
        protected virtual async Task<string> GetCustomProductAttributesXmlAsync(IList<ProductAttributeMapping> productAttributes, Dictionary<int, string> attrMappings)
        {
            var _productAttributeParser = EngineContext.Current.Resolve<IProductAttributeParser>();
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
                            var _downloadService = EngineContext.Current.Resolve<IDownloadService>();
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


        public async Task<string> GetDefaultCombinationAttrXml(IList<ProductAttributeMapping> productAttributeMapping)
        {
            var _productAttributeParser = EngineContext.Current.Resolve<IProductAttributeParser>();
            var attributesXml = string.Empty;
            Dictionary<int, int> productDefaultAttrs = new Dictionary<int, int>();
            foreach (var attribute in productAttributeMapping)
            {
                if (attribute.ShouldHaveValues())
                {
                    var attributeValues = await _productAttributeService.GetProductAttributeValuesAsync(attribute.Id);

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
   
        public virtual async Task<IPagedList<Product>> GetProductsByProductAtributeIdAsync(int productAttributeId
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

        public async Task<MWT.Nop.Core.Domain.StoreWideDiscount.StoreWideDiscount> GetProductSaleInfo(int productId)
        {
            var _storeWideDiscountService = EngineContext.Current.Resolve<IStoreWideDiscountService>();
            return await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(CustomNopCatalogDefaults.ProductSaleInfoCacheKey), async () => await _storeWideDiscountService.GetProductSaleInfo(productId));

        }


     

    }
}
