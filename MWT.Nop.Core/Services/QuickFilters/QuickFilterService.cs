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

namespace MWT.Nop.Core.Services.QuickFilters
{
    public partial class QuickFilterService : IQuickFilterService
    {
        #region Fields

        IRepository<QuickFilter> _quickFilterRepository;
        IStaticCacheManager _staticCacheManager;

        #endregion

        public QuickFilterService(IRepository<QuickFilter> quickFilterRepository,
            IStaticCacheManager staticCacheManager)
        {
            _quickFilterRepository = quickFilterRepository;
            _staticCacheManager = staticCacheManager;
        }

        #region Methods

        public async Task DeleteAsync(QuickFilter quickFilter)
        {
            await _quickFilterRepository.DeleteAsync(quickFilter);
        }

        public async Task<QuickFilter> GetById(int Id)
        {
            return await _quickFilterRepository.GetByIdAsync(Id, cache => default);
        }

        public async Task<IList<QuickFilter>> GetQuickFilterByEntity(int entityId, string entityType)
        {
            var query = from rs in _quickFilterRepository.Table
                        where rs.EntityId == entityId &&
                        rs.EntityType == entityType
                        orderby rs.DisplayOrder, rs.Id
                        select rs;

            var quickFilters = await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(CustomNopCatalogDefaults.QuickFilterCacheKey, entityId, entityType), async () => await query.ToListAsync());

            return quickFilters;
        }

        public async Task InsertAsync(QuickFilter quickFilter)
        {
            await _quickFilterRepository.InsertAsync(quickFilter);
        }

        public async Task UpdateAsync(QuickFilter quickFilter)
        {
            await _quickFilterRepository.UpdateAsync(quickFilter);
        }

        #endregion
    }
}
