using MWT.Nop.Core.Domain;
using MWT.Nop.Core.Service.Catalog;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Services.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.CategoryCollection
{
    public partial class CategoryCollectionLinkService : ICategoryCollectionLinkService
    {
        #region Fields

        IRepository<CategoryCollectionLink> _categoryCollectionLinkRepository;
        IStaticCacheManager _staticCacheManager;

        #endregion

        public CategoryCollectionLinkService(IRepository<CategoryCollectionLink> categoryCollectionLinkRepository,
            IStaticCacheManager staticCacheManager)
        {
            _categoryCollectionLinkRepository = categoryCollectionLinkRepository;
            _staticCacheManager = staticCacheManager;
        }

        #region Methods

        public async Task DeleteAsync(CategoryCollectionLink quickFilter)
        {
            await _categoryCollectionLinkRepository.DeleteAsync(quickFilter);
        }

        public async Task<CategoryCollectionLink> GetById(int Id)
        {
            return await _categoryCollectionLinkRepository.GetByIdAsync(Id, cache => default);
        }

        public async Task<IList<CategoryCollectionLink>> GetCategoryCollectionLinkByEntityId(int entityId)
        {
            var query = from rs in _categoryCollectionLinkRepository.Table
                        where rs.EntityId == entityId 
                       orderby rs.DisplayOrder, rs.Id
                        select rs;

            var quickFilters = await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(CustomNopCatalogDefaults.CategoryCollectionLinkCacheKey, entityId), async () => await query.ToListAsync());

            return quickFilters;
        }

        public async Task InsertAsync(CategoryCollectionLink quickFilter)
        {
            await _categoryCollectionLinkRepository.InsertAsync(quickFilter);
        }

        public async Task UpdateAsync(CategoryCollectionLink quickFilter)
        {
            await _categoryCollectionLinkRepository.UpdateAsync(quickFilter);
        }

        #endregion
    }
}
