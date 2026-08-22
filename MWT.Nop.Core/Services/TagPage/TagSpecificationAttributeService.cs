using MWT.Nop.Core.Domain.TagPage.Cache;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Security;
using Nop.Services.Stores;
namespace MWT.Nop.Core.Services.TagPage
{
    /// <summary>
    /// Specification attribute service interface
    /// </summary>
    public partial class TagSpecificationAttributeService : ITagSpecificationAttributeService
    {

        #region Props

        private readonly CatalogSettings _catalogSettings;
        private readonly IStoreContext _storeContext;
        private readonly ICategoryService _categoryService;
        private readonly IRepository<ProductCategory> _productCategoryRepository;
        private readonly IRepository<SpecificationAttribute> _specificationAttributeRepository;
        private readonly IRepository<SpecificationAttributeOption> _specificationAttributeOptionRepository;
        private readonly IRepository<ProductSpecificationAttribute> _productSpecificationAttributeRepository;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IRepository<Product> _productRepository;
        private readonly IWorkContext _workContext;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IAclService _aclService;
        private readonly IRepository<Category> _categoryRepository;
        #endregion

        #region Ctor
        public TagSpecificationAttributeService(CatalogSettings catalogSettings, IStoreContext storeContext, ICategoryService categoryService, IRepository<ProductCategory> productCategoryRepository,
            IRepository<SpecificationAttribute> specificationAttributeRepository, IRepository<SpecificationAttributeOption> specificationAttributeOptionRepository,
            IRepository<ProductSpecificationAttribute> productSpecificationAttributeRepository, IStaticCacheManager staticCacheManager, IRepository<Product> productRepository, IWorkContext workContext,
            IStoreMappingService storeMappingService, IAclService aclService, IRepository<Category> categoryRepository)
        {
            _catalogSettings = catalogSettings;
            _storeContext = storeContext;
            _categoryService = categoryService;
            _productCategoryRepository = productCategoryRepository;
            _specificationAttributeRepository = specificationAttributeRepository;
            _specificationAttributeOptionRepository = specificationAttributeOptionRepository;
            _productSpecificationAttributeRepository = productSpecificationAttributeRepository;
            _staticCacheManager = staticCacheManager;
            _workContext = workContext;
            _storeMappingService = storeMappingService;
            _aclService = aclService;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        #endregion

        #region  Methods
        public virtual async Task<IList<SpecificationAttributeOption>> GetFilterableCategoriesByTagSegmentAsync(int tagId, int categoryId)
        {
            var _productTagRepository = EngineContext.Current.Resolve<IRepository<ProductProductTagMapping>>();

            var productsQuery = await GetAvailableProductsQueryAsync();

            IList<int> subCategoryIds = null;

            if (_catalogSettings.ShowProductsFromSubcategories && categoryId > 0)
            {
                var store = await _storeContext.GetCurrentStoreAsync();
                subCategoryIds = await _categoryService.GetChildCategoryIdsAsync(categoryId, store.Id);
            }

            var productTagQuery =
                from pc in _productTagRepository.Table
                where pc.ProductTagId == tagId
                select pc;
            if (categoryId != 0)
            {
                var store = await _storeContext.GetCurrentStoreAsync();
                subCategoryIds = await _categoryService.GetChildCategoryIdsAsync(categoryId, store.Id);

                productTagQuery = from pt in productTagQuery
                                  join pc in _productCategoryRepository.Table
                                  on pt.ProductId equals pc.ProductId
                                  where pc.CategoryId == categoryId || subCategoryIds.Contains(pc.CategoryId)
                                  select pt;
            }

            var result =
                from catParent in _categoryRepository.Table
                join catChild in _categoryRepository.Table
                on catParent.Id equals catChild.ParentCategoryId
                join pc in _productCategoryRepository.Table on catChild.Id equals pc.CategoryId
                join p in productsQuery on pc.ProductId equals p.Id
                join pt in productTagQuery on p.Id equals pt.ProductId
                where catParent.ParentCategoryId == 0 && catParent.Published == true && catParent.Deleted == false
                 && catChild.Published == true && catChild.Deleted == false && catChild.ParentCategoryId > 0
                orderby
                    catParent.DisplayOrder, catChild.DisplayOrder, catParent.Name, catChild.Name
                select new
                {
                
                    sao = new SpecificationAttributeOption()
                    {
                        Name = catChild.Name,
                        DisplayOrder = catChild.DisplayOrder,
                        Id = catChild.Id,
                        SpecificationAttributeId = catParent.Id,
                    }
                };

            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(
                NopTagCatalogDefaults.FilterableCategoriesByTagSegmentCacheKey, tagId, categoryId.ToString());

            return await _staticCacheManager.GetAsync(cacheKey, async () => (await result.Distinct().ToListAsync()).Select(query => query.sao).ToList());
        }

        #endregion

        #region Utilities

        protected virtual async Task<IQueryable<Product>> GetAvailableProductsQueryAsync()
        {
            var productsQuery =
                from p in _productRepository.Table
                where !p.Deleted && p.Published &&
                      (p.ParentGroupedProductId == 0 || p.VisibleIndividually) &&
                      (!p.AvailableStartDateTimeUtc.HasValue || p.AvailableStartDateTimeUtc <= DateTime.UtcNow) &&
                      (!p.AvailableEndDateTimeUtc.HasValue || p.AvailableEndDateTimeUtc >= DateTime.UtcNow)
                select p;

            var store = await _storeContext.GetCurrentStoreAsync();
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();

            //apply store mapping constraints
            productsQuery = await _storeMappingService.ApplyStoreMapping(productsQuery, store.Id);

            //apply ACL constraints
            productsQuery = await _aclService.ApplyAcl(productsQuery, currentCustomer);

            return productsQuery;
        }

        #endregion
    }
}
