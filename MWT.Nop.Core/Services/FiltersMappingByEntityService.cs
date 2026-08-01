using MWT.Nop.Core.Domain;
using MWT.Nop.Core.Service.Catalog;
using Nop.Core;
using Nop.Core.Caching; 
using Nop.Data;
using Nop.Services.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Customizations.Custom
{
    public partial class FiltersMappingByEntityService : IFiltersMappingByEntityService
    {

        #region Fields

        private readonly IRepository<FiltersMappingByEntity> _filterMappingByEntityRepository;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IWorkContext _workContext;

        #endregion

        public FiltersMappingByEntityService(IRepository<FiltersMappingByEntity> filterMappingByEntityRepository,
            IStaticCacheManager staticCacheManager, IWorkContext workContext)
        {
            this._filterMappingByEntityRepository = filterMappingByEntityRepository;
            this._staticCacheManager = staticCacheManager;
            this._workContext = workContext;
        }

        #region Methods

        public async Task DeleteAsync(FiltersMappingByEntity obj)
        {
            await _filterMappingByEntityRepository.DeleteAsync(obj);
        }

        public async Task<FiltersMappingByEntity> GetById(int Id)
        {
            return await _filterMappingByEntityRepository.GetByIdAsync(Id, cache => default);
        }

        public async Task<IList<FiltersMappingByEntity>> GetFiltersMappingByEntityByFilterType(int entityId, string entityType, string filterType)
        {
            var query = from fm in _filterMappingByEntityRepository.Table
                        where fm.EntityId == entityId &&
                        fm.EntityType == entityType
                        && fm.Filtertype == filterType
                        orderby fm.DisplayOrder, fm.Id
                        select fm;

            var filtersMappingByEntity = await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(CustomNopCatalogDefaults.FilterMappingByEntitybyfilterTypeCacheKey, entityId, entityType, filterType), async () => await query.ToListAsync());

            return filtersMappingByEntity;
        }

        public async Task<IList<FiltersMappingByEntity>> GetFiltersMappingByEntity(int entityId, string entityType)
        {
            var query = from fm in _filterMappingByEntityRepository.Table
                        where fm.EntityId == entityId &&
                        fm.EntityType == entityType
                        orderby fm.DisplayOrder, fm.Id
                        select fm;

            var filtersMappingByEntity = await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(CustomNopCatalogDefaults.FilterMappingByEntityCacheKey, entityId, entityType), async () => await query.ToListAsync());

            return filtersMappingByEntity;
        }

        public async Task InsertAsync(FiltersMappingByEntity obj)
        {
            var customer = await this._workContext.GetCurrentCustomerAsync();
            obj.CreatedOnUtc = DateTime.UtcNow;
            obj.UpdatedOnUtc = DateTime.UtcNow;
            obj.CreatedBy = customer.Id;
            obj.UpdatedBy = customer.Id;
            await _filterMappingByEntityRepository.InsertAsync(obj);
        }

        public async Task UpdateAsync(FiltersMappingByEntity obj)
        {
            var customer = await this._workContext.GetCurrentCustomerAsync();
            obj.UpdatedOnUtc = DateTime.UtcNow;
            obj.UpdatedBy = customer.Id;
            await _filterMappingByEntityRepository.UpdateAsync(obj);
        }

        public async Task<bool> IsMappingExist(int entityId, string entityType, int filterId, string filterType, int id)
        {
            var query = from fm in _filterMappingByEntityRepository.Table
                        where fm.EntityId == entityId &&
                        fm.EntityType == entityType
                        && fm.Filtertype == filterType
                        && fm.Id != id
                        && fm.FilterId == filterId
                        orderby fm.DisplayOrder, fm.Id
                        select fm;
            return await query.AnyAsync() ? true : false;
        }

        public async Task<FiltersMappingByEntity> GetFilterMapping(string entityType,int entityId, int filterId, string filterType)
        {
            var query = from fm in _filterMappingByEntityRepository.Table
                        where fm.EntityId == entityId
                        && fm.FilterId == filterId
                        && fm.Filtertype == filterType
                        && fm.EntityType== entityType
                        select fm;
            return await query.FirstOrDefaultAsync();
        }

        #endregion
    }
}
