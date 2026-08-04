using MWT.Nop.Core.Domain.KW;
using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Services.Customers;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.KW
{
    public partial class KwTermService : IKwTermService
    {
        #region Fields

        private readonly IAclService _aclService;
        private readonly ICustomCustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly IRepository<KwTerm> _kwTermRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductKwTerm> _productKwTermRepository;
        private readonly IRepository<CategoryKwTerm> _categoryKwTermRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreContext _storeContext;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public KwTermService(
            IAclService aclService,
            ICustomCustomerService customerService,
            ILocalizationService localizationService,
            IRepository<KwTerm> kwTermRepository,
            IRepository<Product> productRepository,
            IRepository<ProductKwTerm> productKwTermRepository,
            IStaticCacheManager staticCacheManager,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            IWorkContext workContext,
            IRepository<CategoryKwTerm> categoryKwTermRepository,
            IRepository<Category> categoryRepository)
        {
            _aclService = aclService;
            _customerService = customerService;
            _localizationService = localizationService;
            _kwTermRepository = kwTermRepository;
            _productRepository = productRepository;
            _productKwTermRepository = productKwTermRepository;
            _staticCacheManager = staticCacheManager;
            _storeContext = storeContext;
            _storeMappingService = storeMappingService;
            _workContext = workContext;
            _categoryKwTermRepository = categoryKwTermRepository;
            _categoryRepository = categoryRepository;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Sort categories for tree representation
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="parentId">Parent kwTerms identifier</param>
        /// <param name="ignorekwTermsWithoutExistingParent">A value indicating whether categories without parent kwTerms in provided kwTerms list (source) should be ignored</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the sorted categories
        /// </returns>
        protected virtual async Task<IList<KwTerm>> SortkwTermForTreeAsync(IList<KwTerm> source, int parentId = 0,
            bool ignorekwTermsWithoutExistingParent = false)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var result = new List<KwTerm>();

            if (ignorekwTermsWithoutExistingParent || result.Count == source.Count)
                return result;

            //find categories without parent in provided kwTerms source and insert them into result
            foreach (var cat in source)
                if (result.FirstOrDefault(x => x.Id == cat.Id) == null)
                    result.Add(cat);

            return result;
        }

        #endregion

        #region Methods


        /// <summary>
        /// Delete kwTerms
        /// </summary>
        /// <param name="kwTerms">KwTerms</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteKwTermAsync(KwTerm kwTerms)
        {
            await _kwTermRepository.DeleteAsync(kwTerms);
        }

        /// <summary>
        /// Delete kwTerms
        /// </summary>
        /// <param name="categories">kwTerms</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task DeleteKwTermAsync(IList<KwTerm> kwTerms)
        {
            if (kwTerms == null)
                throw new ArgumentNullException(nameof(kwTerms));

            foreach (var kwTerm in kwTerms)
                await DeleteKwTermAsync(kwTerm);
        }

        /// <summary>
        /// Gets all categories
        /// </summary>
        /// <param name="storeId">Store identifier; 0 if you want to get all records</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the categories
        /// </returns>
        public virtual async Task<IList<KwTerm>> GetAllKwTermsAsync(int storeId = 0, bool showHidden = false)
        {
            var key = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopCatalogDefaults.KwTermsAllCacheKey,
                  storeId,
                  await _customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync()),
                  showHidden);

            var kwTerms = await _staticCacheManager
             .GetAsync(key, async () => (await GetAllKwTermsAsync(string.Empty, storeId, showHidden: showHidden)).ToList());
            return kwTerms;
        }

        /// <summary>
        /// Gets all categories
        /// </summary>
        /// <param name="kwTermsName">KwTerms name</param>
        /// <param name="storeId">Store identifier; 0 if you want to get all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="overridePublished">
        /// null - process "Published" property according to "showHidden" parameter
        /// true - load only "Published" products
        /// false - load only "Unpublished" products
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the categories
        /// </returns>
        public async Task<IPagedList<KwTerm>> GetAllKwTermsAsync(string kwTermsName = null, int storeId = 0, bool showHidden = false, int pageIndex = 0, int pageSize = 0, bool? overridePublished = null)
        {
            var unsortedkwTerms = await _kwTermRepository.GetAllAsync(async query =>
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

                if (!string.IsNullOrWhiteSpace(kwTermsName))
                    query = query.Where(c => c.Name.Contains(kwTermsName));

                query = query.Where(c => !c.Deleted);

                return query.OrderBy(c => c.DisplayOrder).ThenBy(c => c.Id);
            });

            //sort categories
            var sortedkwTerms = await SortkwTermForTreeAsync(unsortedkwTerms);

            //paging
            return new PagedList<KwTerm>(sortedkwTerms, pageIndex, pageSize);

        }


        /// <summary>
        /// Gets a kwTerms
        /// </summary>
        /// <param name="kwTermsId">KwTerms identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the kwTerms
        /// </returns>
        public virtual async Task<KwTerm> GetKwTermByIdAsync(int kwTermsId)
        {
            return await _kwTermRepository.GetByIdAsync(kwTermsId, cache => default);
        }


        /// <summary>
        /// Inserts kwTerms
        /// </summary>
        /// <param name="kwTerms">KwTerms</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertKwTermAsync(KwTerm kwTerms)
        {
            await _kwTermRepository.InsertAsync(kwTerms);
        }



        /// <summary>
        /// Updates the kwTerms
        /// </summary>
        /// <param name="kwTerms">KwTerms</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateKwTermAsync(KwTerm kwTerms)
        {
            if (kwTerms == null)
                throw new ArgumentNullException(nameof(kwTerms));


            await _kwTermRepository.UpdateAsync(kwTerms);
        }

        /// <summary>
        /// Deletes a product kwTerms mapping
        /// </summary>
        /// <param name="productKwTerms">Product kwTerms</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteProductKwTermAsync(ProductKwTerm productKwTerm)
        {
            await _productKwTermRepository.DeleteAsync(productKwTerm);
        }

        /*        /// <summary>
                /// Gets a product kwTerms mapping collection
                /// </summary>
                /// <param name="productId">Product identifier</param>
                /// <param name="showHidden"> A value indicating whether to show hidden records</param>
                /// <returns>
                /// A task that represents the asynchronous operation
                /// The task result contains the product kwTerms mapping collection
                /// </returns>
                public virtual async Task<IList<ProductKwTerms>> GetProductKwTermsByKwTermIdAsync(int productId, int storeId, bool showHidden = false)
                {
                    if (productId == 0)
                        return new List<ProductKwTerms>();

                    var customer = await _workContext.GetCurrentCustomerAsync();

                    Func<IQueryable<ProductKwTerms>, Task<IQueryable<ProductKwTerms>>> p = async query =>
                    {
                        if (!showHidden)
                        {
                            var categoriesQuery = _kwTermsRepository.Table.Where(c => c.Published);

        //apply store mapping constraints
                            categoriesQuery = await _storeMappingService.ApplyStoreMapping(categoriesQuery, storeId);

                            //apply ACL constraints
                            categoriesQuery = await _aclService.ApplyAcl(categoriesQuery, customer);

                            query = query.Where(pc => categoriesQuery.Any(c => !c.Deleted && c.Id == pc.KwTermsId));
                        }

                        return query
                            .Where(pc => pc.ProductId == productId)
                            .OrderBy(pc => pc.DisplayOrder)
                            .ThenBy(pc => pc.Id);

                    };
                    return await _productKwTermsRepository.GetAllAsync(p, cache => _staticCacheManager.PrepareKeyForDefaultCache(NopCatalogDefaults.ProductKwTermsByProductCacheKey));
                }*/

        /// <summary>
        /// Gets a product kwTerms mapping 
        /// </summary>
        /// <param name="productKwTermsId">Product kwTerms mapping identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product kwTerms mapping
        /// </returns>
        public virtual async Task<ProductKwTerm> GetProductKwTermByIdAsync(int productKwTermsId)
        {
            return await _productKwTermRepository.GetByIdAsync(productKwTermsId, cache => default);
        }

        /// <summary>
        /// Inserts a product kwTerms mapping
        /// </summary>
        /// <param name="productKwTerms">>Product kwTerms mapping</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertProductKwTermAsync(ProductKwTerm productKwTerms)
        {
            await _productKwTermRepository.InsertAsync(productKwTerms);
        }

        /// <summary>
        /// Updates the product kwTerms mapping 
        /// </summary>
        /// <param name="productKwTerms">>Product kwTerms mapping</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateProductKwTermAsync(ProductKwTerm productKwTerms)
        {
            await _productKwTermRepository.UpdateAsync(productKwTerms);
        }

        /// <summary>
        /// Gets categories by identifier
        /// </summary>
        /// <param name="kwTermsIds">KwTerms identifiers</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the categories
        /// </returns>
        public virtual async Task<IList<KwTerm>> GetkwTermsByIdsAsync(int[] kwTermsIds)
        {
            return await _kwTermRepository.GetByIdsAsync(kwTermsIds, includeDeleted: false);
        }

        /// <summary>
        /// Returns a ProductKwTerms that has the specified values
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="productId">Product identifier</param>
        /// <param name="kwTermsId">KwTerms identifier</param>
        /// <returns>A ProductKwTerms that has the specified values; otherwise null</returns>
        public virtual ProductKwTerm FindProductKwTerm(IList<ProductKwTerm> source, int productId, int kwTermsId)
        {
            foreach (var productKwTerm in source)
                if (productKwTerm.ProductId == productId && productKwTerm.KwTermId == kwTermsId)
                    return productKwTerm;

            return null;
        }

        /// <summary>
        /// Get formatted kwTerms breadcrumb 
        /// Note: ACL and store mapping is ignored
        /// </summary>
        /// <param name="kwTerms">KwTerms</param>
        /// <param name="allkwTerms">All categories</param>
        /// <param name="separator">Separator</param>
        /// <param name="languageId">Language identifier for localization</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the formatted breadcrumb
        /// </returns>
        public virtual async Task<string> GetFormattedBreadCrumbAsync(KwTerm kwTerm, IList<KwTerm> allkwTerms = null,
            string separator = ">>", int languageId = 0)
        {
            var result = string.Empty;

            var breadcrumb = await GetKwTermBreadCrumbAsync(kwTerm, allkwTerms, true);
            for (var i = 0; i <= breadcrumb.Count - 1; i++)
            {
                var kwTermsName = await _localizationService.GetLocalizedAsync(breadcrumb[i], x => x.Name, languageId);
                result = string.IsNullOrEmpty(result) ? kwTermsName : $"{result} {separator} {kwTermsName}";
            }

            return result;
        }

        /// <summary>
        /// Get kwTerms breadcrumb 
        /// </summary>
        /// <param name="kwTerms">KwTerms</param>
        /// <param name="allkwTerms">All categories</param>
        /// <param name="showHidden">A value indicating whether to load hidden records</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the kwTerms breadcrumb 
        /// </returns>

        public async Task<IPagedList<ProductKwTerm>> GetProductKwTermsByKwTermsIdAsync(int kwTermId, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            {
                if (kwTermId == 0)
                    return null;

                var query = from pc in _productKwTermRepository.Table
                            join p in _productRepository.Table on pc.ProductId equals p.Id
                            where pc.KwTermId == kwTermId && !p.Deleted
                            orderby pc.DisplayOrder, pc.Id
                            select pc;

                if (!showHidden)
                {
                    var kwTermQuery = _kwTermRepository.Table.Where(c => c.Published);

                    //apply store mapping constraints
                    var store = await _storeContext.GetCurrentStoreAsync();
                    kwTermQuery = await _storeMappingService.ApplyStoreMapping(kwTermQuery, store.Id);

                    //apply ACL constraints
                    var customer = await _workContext.GetCurrentCustomerAsync();
                    kwTermQuery = await _aclService.ApplyAcl(kwTermQuery, customer);

                    query = query.Where(pc => kwTermQuery.Any(c => c.Id == pc.KwTermId));
                }

                return await query.ToPagedListAsync(pageIndex, pageSize);

            }
        }

        public async Task<IPagedList<ProductKwTerm>> GetProductKwTermsByKwTermIdAsync(int kwTermId, int pageIndex, int pageSize, bool showHidden)
        {
            if (kwTermId == 0)
                return null;

            var query = from pc in _productKwTermRepository.Table
                        join p in _productRepository.Table on pc.ProductId equals p.Id
                        where pc.KwTermId == kwTermId && !p.Deleted
                        orderby pc.DisplayOrder, pc.Id
                        select pc;

            if (!showHidden)
            {
                var kwTermQuery = _kwTermRepository.Table.Where(c => c.Published);

                //apply store mapping constraints
                var store = await _storeContext.GetCurrentStoreAsync();
                kwTermQuery = await _storeMappingService.ApplyStoreMapping(kwTermQuery, store.Id);

                //apply ACL constraints
                var customer = await _workContext.GetCurrentCustomerAsync();
                kwTermQuery = await _aclService.ApplyAcl(kwTermQuery, customer);

                query = query.Where(pc => kwTermQuery.Any(c => c.Id == pc.KwTermId));
            }

            return await query.ToPagedListAsync(pageIndex, pageSize);

        }

        public async Task<IList<KwTerm>> GetKwTermBreadCrumbAsync(KwTerm kwTerm, IList<KwTerm> allKwTerms = null, bool showHidden = false)
        {

            if (kwTerm == null)
                throw new ArgumentNullException(nameof(kwTerm));

            var breadcrumbCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopCatalogDefaults.KwTermsBreadcrumbCacheKey,
                kwTerm,
                await _customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync()),
                await _storeContext.GetCurrentStoreAsync(),
                await _workContext.GetWorkingLanguageAsync());

            return await _staticCacheManager.GetAsync(breadcrumbCacheKey, async () =>
            {
                var result = new List<KwTerm>();

                //used to prevent circular references
                var alreadyProcessedCategoryIds = new List<int>();

                while (kwTerm != null && //not null
                       !kwTerm.Deleted && //not deleted
                       (showHidden || kwTerm.Published) && //published
                       (showHidden || await _aclService.AuthorizeAsync(kwTerm)) && //ACL
                       (showHidden || await _storeMappingService.AuthorizeAsync(kwTerm)) && //Store mapping
                       !alreadyProcessedCategoryIds.Contains(kwTerm.Id)) //prevent circular references
                {
                    result.Add(kwTerm);

                    alreadyProcessedCategoryIds.Add(kwTerm.Id);

                    kwTerm = allKwTerms != null
                        ? allKwTerms.FirstOrDefault(c => c.Id == kwTerm.Id)
                        : await GetKwTermByIdAsync(kwTerm.Id);
                }

                result.Reverse();

                return result;
            });
        }
        #region Categories

        public async Task<IPagedList<CategoryKwTerm>> GetCategoryKwTermsByKwTermIdAsync(int kwTermId, int pageIndex, int pageSize, bool showHidden)
        {
            if (kwTermId == 0)
                return null;

            var query = from pc in _categoryKwTermRepository.Table
                        join p in _categoryRepository.Table on pc.CategoryId equals p.Id
                        where pc.KwTermId == kwTermId && !p.Deleted
                        orderby pc.DisplayOrder, pc.Id
                        select pc;

            if (!showHidden)
            {
                var kwTermQuery = _kwTermRepository.Table.Where(c => c.Published);

                //apply store mapping constraints
                var store = await _storeContext.GetCurrentStoreAsync();
                kwTermQuery = await _storeMappingService.ApplyStoreMapping(kwTermQuery, store.Id);

                //apply ACL constraints
                var customer = await _workContext.GetCurrentCustomerAsync();
                kwTermQuery = await _aclService.ApplyAcl(kwTermQuery, customer);

                query = query.Where(pc => kwTermQuery.Any(c => c.Id == pc.KwTermId));
            }

            return await query.ToPagedListAsync(pageIndex, pageSize);

        }

        public virtual async Task<CategoryKwTerm> GetCategoryKwTermByIdAsync(int categoryKwTermsId)
        {
            return await _categoryKwTermRepository.GetByIdAsync(categoryKwTermsId, cache => default);
        }

        public virtual async Task DeleteCategoryKwTermAsync(CategoryKwTerm categoryKwTerm)
        {
            await _categoryKwTermRepository.DeleteAsync(categoryKwTerm);
        }

        public virtual async Task UpdateCategoryKwTermAsync(CategoryKwTerm categoryKwTerms)
        {
            await _categoryKwTermRepository.UpdateAsync(categoryKwTerms);
        }



        public virtual async Task InsertCategoryKwTermAsync(CategoryKwTerm categoryKwTerms)
        {
            await _categoryKwTermRepository.InsertAsync(categoryKwTerms);
        }

        public virtual async Task<IList<Category>> GetCategoriesAsync(int kwTermId)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var customer = await _workContext.GetCurrentCustomerAsync();
            var key = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopCatalogDefaults.KwTermsCategoriesCacheKey,
                             kwTermId,
                             store);


            return await _staticCacheManager.GetAsync(key, async () =>
            {
                return await (from kc in _categoryKwTermRepository.Table
                              join c in _categoryRepository.Table on kc.CategoryId equals c.Id
                              where kc.KwTermId == kwTermId && !c.Deleted && c.Published
                              orderby kc.DisplayOrder, kc.Id
                              select c).ToListAsync();
            });
        }

        #endregion

        #endregion
    }
}
