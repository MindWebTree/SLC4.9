using MWT.Nop.Core.Domain.QA;
using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Services.Customers;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.QA
{
    public partial class QuestionAnswerService : IQuestionAnswerService
    {
        #region Fields

        private readonly IAclService _aclService;
        private readonly ICustomerExtendedService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly IRepository<QuestionAnswer> _QuestionAnswerRepository;
        private readonly IRepository<RelatedQuestionAnswer> _relatedQuestionAnswerRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductQuestionAnswer> _productQuestionAnswerRepository;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreContext _storeContext;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public QuestionAnswerService(
            IAclService aclService,
            ICustomerExtendedService customerService,
            ILocalizationService localizationService,
            IRepository<QuestionAnswer> QuestionAnswerRepository,
            IRepository<Product> productRepository,
            IRepository<ProductQuestionAnswer> productQuestionAnswerRepository,
            IStaticCacheManager staticCacheManager,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            IRepository<RelatedQuestionAnswer> relatedQuestionAnswerRepository,
            IWorkContext workContext)
        {
            _aclService = aclService;
            _customerService = customerService;
            _localizationService = localizationService;
            _QuestionAnswerRepository = QuestionAnswerRepository;
            _productRepository = productRepository;
            _productQuestionAnswerRepository = productQuestionAnswerRepository;
            _staticCacheManager = staticCacheManager;
            _storeContext = storeContext;
            _storeMappingService = storeMappingService;
            _workContext = workContext;
            _relatedQuestionAnswerRepository = relatedQuestionAnswerRepository;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Sort categories for tree representation
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="parentId">Parent QuestionAnswers identifier</param>
        /// <param name="ignoreQuestionAnswersWithoutExistingParent">A value indicating whether categories without parent QuestionAnswers in provided QuestionAnswers list (source) should be ignored</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the sorted categories
        /// </returns>
        protected virtual async Task<IList<QuestionAnswer>> SortQuestionAnswerForTreeAsync(IList<QuestionAnswer> source, int parentId = 0,
            bool ignoreQuestionAnswersWithoutExistingParent = false)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var result = new List<QuestionAnswer>();

            if (ignoreQuestionAnswersWithoutExistingParent || result.Count == source.Count)
                return result;

            //find categories without parent in provided QuestionAnswers source and insert them into result
            foreach (var cat in source)
                if (result.FirstOrDefault(x => x.Id == cat.Id) == null)
                    result.Add(cat);

            return result;
        }

        #endregion

        #region Methods


        /// <summary>
        /// Delete QuestionAnswers
        /// </summary>
        /// <param name="QuestionAnswers">QuestionAnswers</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteQuestionAnswerAsync(QuestionAnswer QuestionAnswers)
        {
            await _QuestionAnswerRepository.DeleteAsync(QuestionAnswers);
        }

        /// <summary>
        /// Delete QuestionAnswers
        /// </summary>
        /// <param name="categories">QuestionAnswers</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task DeleteQuestionAnswerAsync(IList<QuestionAnswer> QuestionAnswers)
        {
            if (QuestionAnswers == null)
                throw new ArgumentNullException(nameof(QuestionAnswers));

            foreach (var QuestionAnswer in QuestionAnswers)
                await DeleteQuestionAnswerAsync(QuestionAnswer);
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
        public virtual async Task<IList<QuestionAnswer>> GetAllQuestionAnswersAsync(int storeId = 0, bool showHidden = false)
        {
            var key = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopCatalogDefaults.QuestionAnswersAllCacheKey,
                  storeId,
                  await _customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync()),
                  showHidden);

            var QuestionAnswers = await _staticCacheManager
             .GetAsync(key, async () => (await GetAllQuestionAnswersAsync(string.Empty, storeId, showHidden: showHidden)).ToList());
            return QuestionAnswers;
        }

        /// <summary>
        /// Gets all categories
        /// </summary>
        /// <param name="QuestionAnswersName">QuestionAnswers name</param>
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
        public async Task<IPagedList<QuestionAnswer>> GetAllQuestionAnswersAsync(string QuestionAnswersName = null, int storeId = 0, bool showHidden = false, int pageIndex = 0, int pageSize = 0, bool? overridePublished = null)
        {
            var unsortedQuestionAnswers = await _QuestionAnswerRepository.GetAllAsync(async query =>
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

                if (!string.IsNullOrWhiteSpace(QuestionAnswersName))
                    query = query.Where(c => c.Name.Contains(QuestionAnswersName));

                query = query.Where(c => !c.Deleted);

                return query.OrderBy(c => c.DisplayOrder).ThenBy(c => c.Id);
            });

            //sort categories
            var sortedQuestionAnswers = await SortQuestionAnswerForTreeAsync(unsortedQuestionAnswers);

            //paging
            return new PagedList<QuestionAnswer>(sortedQuestionAnswers, pageIndex, pageSize);

        }


        /// <summary>
        /// Gets a QuestionAnswers
        /// </summary>
        /// <param name="QuestionAnswersId">QuestionAnswers identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the QuestionAnswers
        /// </returns>
        public virtual async Task<QuestionAnswer> GetQuestionAnswerByIdAsync(int QuestionAnswersId)
        {
            return await _QuestionAnswerRepository.GetByIdAsync(QuestionAnswersId, cache => default);
        }


        /// <summary>
        /// Inserts QuestionAnswers
        /// </summary>
        /// <param name="QuestionAnswers">QuestionAnswers</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertQuestionAnswerAsync(QuestionAnswer QuestionAnswers)
        {
            await _QuestionAnswerRepository.InsertAsync(QuestionAnswers);
        }



        /// <summary>
        /// Updates the QuestionAnswers
        /// </summary>
        /// <param name="QuestionAnswers">QuestionAnswers</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateQuestionAnswerAsync(QuestionAnswer QuestionAnswers)
        {
            if (QuestionAnswers == null)
                throw new ArgumentNullException(nameof(QuestionAnswers));


            await _QuestionAnswerRepository.UpdateAsync(QuestionAnswers);
        }

        /// <summary>
        /// Deletes a product QuestionAnswers mapping
        /// </summary>
        /// <param name="productQuestionAnswers">Product QuestionAnswers</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteProductQuestionAnswerAsync(ProductQuestionAnswer productQuestionAnswer)
        {
            await _productQuestionAnswerRepository.DeleteAsync(productQuestionAnswer);
        }

        /*        /// <summary>
                /// Gets a product QuestionAnswers mapping collection
                /// </summary>
                /// <param name="productId">Product identifier</param>
                /// <param name="showHidden"> A value indicating whether to show hidden records</param>
                /// <returns>
                /// A task that represents the asynchronous operation
                /// The task result contains the product QuestionAnswers mapping collection
                /// </returns>
                public virtual async Task<IList<ProductQuestionAnswers>> GetProductQuestionAnswersByQuestionAnswerIdAsync(int productId, int storeId, bool showHidden = false)
                {
                    if (productId == 0)
                        return new List<ProductQuestionAnswers>();

                    var customer = await _workContext.GetCurrentCustomerAsync();

                    Func<IQueryable<ProductQuestionAnswers>, Task<IQueryable<ProductQuestionAnswers>>> p = async query =>
                    {
                        if (!showHidden)
                        {
                            var categoriesQuery = _QuestionAnswersRepository.Table.Where(c => c.Published);

        //apply store mapping constraints
                            categoriesQuery = await _storeMappingService.ApplyStoreMapping(categoriesQuery, storeId);

                            //apply ACL constraints
                            categoriesQuery = await _aclService.ApplyAcl(categoriesQuery, customer);

                            query = query.Where(pc => categoriesQuery.Any(c => !c.Deleted && c.Id == pc.QuestionAnswersId));
                        }

                        return query
                            .Where(pc => pc.ProductId == productId)
                            .OrderBy(pc => pc.DisplayOrder)
                            .ThenBy(pc => pc.Id);

                    };
                    return await _productQuestionAnswersRepository.GetAllAsync(p, cache => _staticCacheManager.PrepareKeyForDefaultCache(NopCatalogDefaults.ProductQuestionAnswersByProductCacheKey));
                }*/

        /// <summary>
        /// Gets a product QuestionAnswers mapping 
        /// </summary>
        /// <param name="productQuestionAnswersId">Product QuestionAnswers mapping identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product QuestionAnswers mapping
        /// </returns>
        public virtual async Task<ProductQuestionAnswer> GetProductQuestionAnswerByIdAsync(int productQuestionAnswersId)
        {
            return await _productQuestionAnswerRepository.GetByIdAsync(productQuestionAnswersId, cache => default);
        }

        /// <summary>
        /// Inserts a product QuestionAnswers mapping
        /// </summary>
        /// <param name="productQuestionAnswers">>Product QuestionAnswers mapping</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertProductQuestionAnswerAsync(ProductQuestionAnswer productQuestionAnswers)
        {
            await _productQuestionAnswerRepository.InsertAsync(productQuestionAnswers);
        }

        /// <summary>
        /// Updates the product QuestionAnswers mapping 
        /// </summary>
        /// <param name="productQuestionAnswers">>Product QuestionAnswers mapping</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateProductQuestionAnswerAsync(ProductQuestionAnswer productQuestionAnswers)
        {
            await _productQuestionAnswerRepository.UpdateAsync(productQuestionAnswers);
        }

        /// <summary>
        /// Gets categories by identifier
        /// </summary>
        /// <param name="QuestionAnswersIds">QuestionAnswers identifiers</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the categories
        /// </returns>
        public virtual async Task<IList<QuestionAnswer>> GetQuestionAnswersByIdsAsync(int[] QuestionAnswersIds)
        {
            return await _QuestionAnswerRepository.GetByIdsAsync(QuestionAnswersIds, includeDeleted: false);
        }

        /// <summary>
        /// Returns a ProductQuestionAnswers that has the specified values
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="productId">Product identifier</param>
        /// <param name="QuestionAnswersId">QuestionAnswers identifier</param>
        /// <returns>A ProductQuestionAnswers that has the specified values; otherwise null</returns>
        public virtual ProductQuestionAnswer FindProductQuestionAnswer(IList<ProductQuestionAnswer> source, int productId, int QuestionAnswersId)
        {
            foreach (var productQuestionAnswer in source)
                if (productQuestionAnswer.ProductId == productId && productQuestionAnswer.QuestionAnswerId == QuestionAnswersId)
                    return productQuestionAnswer;

            return null;
        }

        /// <summary>
        /// Get formatted QuestionAnswers breadcrumb 
        /// Note: ACL and store mapping is ignored
        /// </summary>
        /// <param name="QuestionAnswers">QuestionAnswers</param>
        /// <param name="allQuestionAnswers">All categories</param>
        /// <param name="separator">Separator</param>
        /// <param name="languageId">Language identifier for localization</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the formatted breadcrumb
        /// </returns>
        public virtual async Task<string> GetFormattedBreadCrumbAsync(QuestionAnswer QuestionAnswer, IList<QuestionAnswer> allQuestionAnswers = null,
            string separator = ">>", int languageId = 0)
        {
            var result = string.Empty;

            var breadcrumb = await GetQuestionAnswerBreadCrumbAsync(QuestionAnswer, allQuestionAnswers, true);
            for (var i = 0; i <= breadcrumb.Count - 1; i++)
            {
                var QuestionAnswersName = await _localizationService.GetLocalizedAsync(breadcrumb[i], x => x.Name, languageId);
                result = string.IsNullOrEmpty(result) ? QuestionAnswersName : $"{result} {separator} {QuestionAnswersName}";
            }

            return result;
        }

        /// <summary>
        /// Get QuestionAnswers breadcrumb 
        /// </summary>
        /// <param name="QuestionAnswers">QuestionAnswers</param>
        /// <param name="allQuestionAnswers">All categories</param>
        /// <param name="showHidden">A value indicating whether to load hidden records</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the QuestionAnswers breadcrumb 
        /// </returns>

        public async Task<IPagedList<ProductQuestionAnswer>> GetProductQuestionAnswersByQuestionAnswersIdAsync(int QuestionAnswerId, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            {
                if (QuestionAnswerId == 0)
                    return null;

                var query = from pc in _productQuestionAnswerRepository.Table
                            join p in _productRepository.Table on pc.ProductId equals p.Id
                            where pc.QuestionAnswerId == QuestionAnswerId && !p.Deleted
                            orderby pc.DisplayOrder, pc.Id
                            select pc;

                if (!showHidden)
                {
                    var QuestionAnswerQuery = _QuestionAnswerRepository.Table.Where(c => c.Published);

                    //apply store mapping constraints
                    var store = await _storeContext.GetCurrentStoreAsync();
                    QuestionAnswerQuery = await _storeMappingService.ApplyStoreMapping(QuestionAnswerQuery, store.Id);

                    //apply ACL constraints
                    var customer = await _workContext.GetCurrentCustomerAsync();
                    QuestionAnswerQuery = await _aclService.ApplyAcl(QuestionAnswerQuery, customer);

                    query = query.Where(pc => QuestionAnswerQuery.Any(c => c.Id == pc.QuestionAnswerId));
                }

                return await query.ToPagedListAsync(pageIndex, pageSize);

            }
        }

        public async Task<IPagedList<ProductQuestionAnswer>> GetProductQuestionAnswersByQuestionAnswerIdAsync(int QuestionAnswerId, int pageIndex, int pageSize, bool showHidden)
        {
            if (QuestionAnswerId == 0)
                return null;

            var query = from pc in _productQuestionAnswerRepository.Table
                        join p in _productRepository.Table on pc.ProductId equals p.Id
                        where pc.QuestionAnswerId == QuestionAnswerId && !p.Deleted
                        orderby pc.DisplayOrder, pc.Id
                        select pc;

            if (!showHidden)
            {
                var QuestionAnswerQuery = _QuestionAnswerRepository.Table.Where(c => c.Published);

                //apply store mapping constraints
                var store = await _storeContext.GetCurrentStoreAsync();
                QuestionAnswerQuery = await _storeMappingService.ApplyStoreMapping(QuestionAnswerQuery, store.Id);

                //apply ACL constraints
                var customer = await _workContext.GetCurrentCustomerAsync();
                QuestionAnswerQuery = await _aclService.ApplyAcl(QuestionAnswerQuery, customer);

                query = query.Where(pc => QuestionAnswerQuery.Any(c => c.Id == pc.QuestionAnswerId));
            }

            return await query.ToPagedListAsync(pageIndex, pageSize);

        }

        public async Task<IList<QuestionAnswer>> GetQuestionAnswerBreadCrumbAsync(QuestionAnswer QuestionAnswer, IList<QuestionAnswer> allQuestionAnswers = null, bool showHidden = false)
        {

            if (QuestionAnswer == null)
                throw new ArgumentNullException(nameof(QuestionAnswer));

            var breadcrumbCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopCatalogDefaults.QuestionAnswersBreadcrumbCacheKey,
                QuestionAnswer,
                await _customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync()),
                await _storeContext.GetCurrentStoreAsync(),
                await _workContext.GetWorkingLanguageAsync());

            return await _staticCacheManager.GetAsync(breadcrumbCacheKey, async () =>
            {
                var result = new List<QuestionAnswer>();

                //used to prevent circular references
                var alreadyProcessedCategoryIds = new List<int>();

                while (QuestionAnswer != null && //not null
                       !QuestionAnswer.Deleted && //not deleted
                       (showHidden || QuestionAnswer.Published) && //published
                       (showHidden || await _aclService.AuthorizeAsync(QuestionAnswer)) && //ACL
                       (showHidden || await _storeMappingService.AuthorizeAsync(QuestionAnswer)) && //Store mapping
                       !alreadyProcessedCategoryIds.Contains(QuestionAnswer.Id)) //prevent circular references
                {
                    result.Add(QuestionAnswer);

                    alreadyProcessedCategoryIds.Add(QuestionAnswer.Id);

                    QuestionAnswer = allQuestionAnswers != null
                        ? allQuestionAnswers.FirstOrDefault(c => c.Id == QuestionAnswer.Id)
                        : await GetQuestionAnswerByIdAsync(QuestionAnswer.Id);
                }

                result.Reverse();

                return result;
            });
        }
        #region related question answers

        /// <summary>
        /// Deletes a related question answer
        /// </summary>
        /// <param name="related">related question answer</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteRelatedQuestionAnswerAsync(RelatedQuestionAnswer relatedQuestionAnswer)
        {
            await _relatedQuestionAnswerRepository.DeleteAsync(relatedQuestionAnswer);
        }

        /// <summary>
        /// Gets related question answers by product identifier
        /// </summary>
        /// <param name="productId">The first product identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the related question answers
        /// </returns>
        public virtual async Task<IList<RelatedQuestionAnswer>> GetRelatedQuestionAnswersByQuestionAnswerId1Async(int questionAnswerId, bool showHidden = false)
        {
            var query = from rq in _relatedQuestionAnswerRepository.Table
                        join q in _QuestionAnswerRepository.Table on rq.QuestionAnswerId2 equals q.Id
                        where rq.QuestionAnswerId1 == questionAnswerId &&
                        !q.Deleted &&
                        (showHidden || q.Published)
                        orderby rq.DisplayOrder, rq.Id
                        select rq;

            var RelatedQuestionAnswers = await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(CustomNopCatalogDefaults.RelatedQuestionAnswersCacheKey, questionAnswerId, showHidden), async () => await query.ToListAsync());

            return RelatedQuestionAnswers;
        }

        /// <summary>
        /// Gets a related question answer
        /// </summary>
        /// <param name="RelatedQuestionAnswerId">related question answer identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the related question answer
        /// </returns>
        public virtual async Task<RelatedQuestionAnswer> GetRelatedQuestionAnswerByIdAsync(int relatedQuestionAnswerId)
        {
            return await _relatedQuestionAnswerRepository.GetByIdAsync(relatedQuestionAnswerId, cache => default);
        }

        /// <summary>
        /// Inserts a related question answer
        /// </summary>
        /// <param name="RelatedQuestionAnswer">related question answer</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertRelatedQuestionAnswerAsync(RelatedQuestionAnswer relatedQuestionAnswer)
        {
            await _relatedQuestionAnswerRepository.InsertAsync(relatedQuestionAnswer);
        }

        /// <summary>
        /// Updates a related question answer
        /// </summary>
        /// <param name="RelatedQuestionAnswer">related question answer</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateRelatedQuestionAnswerAsync(RelatedQuestionAnswer relatedQuestionAnswer)
        {
            await _relatedQuestionAnswerRepository.UpdateAsync(relatedQuestionAnswer);
        }

        /// <summary>
        /// Finds a related question answer item by specified identifiers
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="productId1">The first product identifier</param>
        /// <param name="productId2">The second product identifier</param>
        /// <returns>related question answer</returns>
        public virtual RelatedQuestionAnswer FindRelatedQuestionAnswer(IList<RelatedQuestionAnswer> source, int questionAnswerId1, int questionAnswerId2)
        {
            foreach (var relatedQuestionAnswer in source)
                if (relatedQuestionAnswer.QuestionAnswerId1 == questionAnswerId1 && relatedQuestionAnswer.QuestionAnswerId2 == questionAnswerId2)
                    return relatedQuestionAnswer;
            return null;
        }

        #endregion
        #endregion





    }
}


