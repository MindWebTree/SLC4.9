using MWT.Nop.Core.Domain;
using MWT.Nop.Core.Service.Catalog;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services
{
    public partial class RelatedSearchService : IRelatedSearchService
    {
        #region Fields

        IRepository<RelatedSearch> _relatedSearchRepository;
        IStaticCacheManager _staticCacheManager;

        #endregion

        public RelatedSearchService(IRepository<RelatedSearch> relatedSearchRepository,
            IStaticCacheManager staticCacheManager)
        {
            _relatedSearchRepository = relatedSearchRepository;
            _staticCacheManager = staticCacheManager;
        }

        #region Methods

        public async Task DeleteAsync(RelatedSearch relatedSearch)
        {
            await _relatedSearchRepository.DeleteAsync(relatedSearch);
        }

        public async Task<RelatedSearch> GetById(int Id)
        {
            return await _relatedSearchRepository.GetByIdAsync(Id, cache => default);
        }

        public async Task<IList<RelatedSearch>> GetRelatedSearchTermsByEntity(int entityId, string entityType)
        {
            var query = from rs in _relatedSearchRepository.Table
                        where rs.EntityId == entityId &&
                        rs.EntityType == entityType
                        orderby rs.DisplayOrder, rs.Id
                        select rs;

            var relatedSearches = await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(CustomNopCatalogDefaults.ReleatedSearchCacheKey, entityId, entityType), async () => await query.ToListAsync());

            return relatedSearches;
        }

        public async Task InsertAsync(RelatedSearch relatedSearch)
        {
            await _relatedSearchRepository.InsertAsync(relatedSearch);
        }

        public async Task UpdateAsync(RelatedSearch relatedSearch)
        {
            await _relatedSearchRepository.UpdateAsync(relatedSearch);
        }


        public async Task<IList<RelatedSearch>> GetRelatedSearchTermsByProductIds(int[] productIds)
        {
            if (productIds.Length == 0)
            {
                var query = _relatedSearchRepository.Table.Where(r => r.EntityType == "Product").GroupBy(q => q.TermName)
                .OrderByDescending(gp => gp.Count())
               .Take(5)
               .Select(g => new RelatedSearch { TermName = g.Key });


                var relatedSearches = await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(CustomNopCatalogDefaults.ReleatedSearchDefaultForSearchpageCacheKey)
                    , async () => await query.ToListAsync());
                return relatedSearches;
            }
            else
            {
                var query = _relatedSearchRepository.Table.Where(r => productIds.Contains(r.EntityId) && r.EntityType == "Product").GroupBy(q => q.TermName)
                  .OrderByDescending(gp => gp.Count())
                 .Take(5)
                 .Select(g => new RelatedSearch { TermName = g.Key });


                var relatedSearches = await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(CustomNopCatalogDefaults.ReleatedSearchByProductIdsCacheKey,string.Join('-', productIds)), async () => await query.ToListAsync());
                return relatedSearches;
            }


         
        }

        #endregion
    }
}
