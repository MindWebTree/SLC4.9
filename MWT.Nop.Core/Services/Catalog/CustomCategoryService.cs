using MWT.Nop.Core.Domain.Security;
using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Services.Customers;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;


namespace MWT.Nop.Core.Services.Catalog
{
    public partial class CustomCategoryService : CategoryService , ICustomCategoryService
    {
        #region Fields
        private readonly ISettingService _settingService;
        #endregion
        public CustomCategoryService(IAclService aclService, ICustomerService customerService,
            ILocalizationService localizationService, IRepository<Category> categoryRepository,
            IRepository<DiscountCategoryMapping> discountCategoryMappingRepository, IRepository<Product> productRepository,
            IRepository<ProductCategory> productCategoryRepository, IStaticCacheManager staticCacheManager, IStoreContext storeContext,
            IStoreMappingService storeMappingService, IWorkContext workContext, ISettingService settingService) : base(aclService, customerService, localizationService, categoryRepository, 
                discountCategoryMappingRepository, productRepository, productCategoryRepository, staticCacheManager, storeContext, storeMappingService, workContext)
        {
            _settingService=settingService;
        }


        #region Methods

        public virtual async Task<IPagedList<Category>> GetAccessibleCategoriesAsync(string categoryName, int customerId, int storeId = 0,
         int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null)
        {
            var _categoryUserMappingRepository = EngineContext.Current.Resolve<IRepository<CategoryUserMapping>>();
            var unsortedCategories = await _categoryRepository.GetAllAsync(async query =>
            {
                if (!showHidden)
                    query = query.Where(c => c.Published);
                else if (overridePublished.HasValue)
                    query = query.Where(c => c.Published == overridePublished.Value);

                //apply store mapping constraints
                query = await _storeMappingService.ApplyStoreMapping(query, storeId);

                //apply ACL constraints
                if (!showHidden)
                {
                    var customer = await _workContext.GetCurrentCustomerAsync();
                    query = await _aclService.ApplyAcl(query, customer);
                }

                if (!string.IsNullOrWhiteSpace(categoryName))
                    query = query.Where(c => c.Name.Contains(categoryName));

                query = from entity in query
                        join catUserMapping in _categoryUserMappingRepository.Table
                        on entity.Id equals catUserMapping.CategoryId
                        where catUserMapping.UserId == customerId
                        select entity;



                query = query.Where(c => !c.Deleted);

                return query.OrderBy(c => c.ParentCategoryId).ThenBy(c => c.DisplayOrder).ThenBy(c => c.Id);
            });

            //sort categories
            var sortedCategories =  SortCategoriesForTree(unsortedCategories.ToLookup(c => c.ParentCategoryId)).ToList();

            //paging
            return new PagedList<Category>(sortedCategories, pageIndex, pageSize);
        }
        public virtual async Task<IList<ProductCategory>> GetProductCategoriesByProductIdExcludingRootLevelCategoriesAsync(int productId, bool showHidden = false)
        {
            return await GetProductCategoriesByProductIdExcludingRootLevelCategoriesAsync(productId, (await _storeContext.GetCurrentStoreAsync()).Id, showHidden);
        }

        public async ValueTask<bool> IsMwtWidgetApplied(string widget, int entityID, string entityType, bool isMobileDevice)
        {
            var _settingService = EngineContext.Current.Resolve<ISettingService>();
            string keyValue = await _settingService.GetSettingByKeyAsync<string>($"MWTPluginWidgetsCatalogSetting{(isMobileDevice ? ".Mobile" : "")}.{entityType}." + widget);
            if (!string.IsNullOrEmpty(keyValue))
            {
                return keyValue.Split(',').Where(m => m == entityID.ToString()).Any();
            }
            return false;
        }

        #endregion

        #region Utilities

        protected virtual async Task<IList<ProductCategory>> GetProductCategoriesByProductIdExcludingRootLevelCategoriesAsync(int productId, int storeId,
     bool showHidden = false)
        {
            if (productId == 0)
                return new List<ProductCategory>();

            var customer = await _workContext.GetCurrentCustomerAsync();

            return await _productCategoryRepository.GetAllAsync(async query =>
            {
                if (!showHidden)
                {
                    var categoriesQuery = _categoryRepository.Table.Where(c => c.Published && c.ParentCategoryId > 0);

                    //apply store mapping constraints
                    categoriesQuery = await _storeMappingService.ApplyStoreMapping(categoriesQuery, storeId);

                    //apply ACL constraints
                    categoriesQuery = await _aclService.ApplyAcl(categoriesQuery, customer);

                    query = query.Where(pc => categoriesQuery.Any(c => !c.Deleted && c.Id == pc.CategoryId));
                }

                return query
                    .Where(pc => pc.ProductId == productId)
                    .OrderBy(pc => pc.DisplayOrder)
                    .ThenBy(pc => pc.Id);

            }, cache => _staticCacheManager.PrepareKeyForDefaultCache(CustomNopCatalogDefaults.ProductCategoriesByProductExcludingRootLevelCategoriesCacheKey,
                productId, showHidden, customer, storeId, true));
        }
        #endregion

    }
}
