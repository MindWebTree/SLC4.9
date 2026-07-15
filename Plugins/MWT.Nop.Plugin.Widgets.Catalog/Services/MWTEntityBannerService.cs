using MWT.Nop.Plugin.Widgets.Catalog.Domain;
using Nop.Core;
using Nop.Data;
using Nop.Services.Catalog;
// using Nop.Services.Customizations.Custom.KW;

namespace MWT.Nop.Plugin.Widgets.Catalog.Services
{
    public partial class MWTEntityBannerService : IMWTEntityBannerService
    {

        #region Fields

        private readonly IRepository<MWTEntityBanner> _mwtEntityBannerRepository;
        private readonly IRepository<MWTEntityBannerEntityMapping> _mwtEntityBannerMappingRepository;
        private readonly ICategoryService _categoryService;
       // private readonly IKwTermService _kwTermService;

        #endregion

        #region ctor

        public MWTEntityBannerService(IRepository<MWTEntityBanner> mwtEntityBannerRepository, IRepository<MWTEntityBannerEntityMapping> mwtEntityBannerMappingRepository,
            ICategoryService categoryService/*, IKwTermService kwTermService*/)
        {
            _mwtEntityBannerRepository = mwtEntityBannerRepository;
            _mwtEntityBannerMappingRepository = mwtEntityBannerMappingRepository;
            _categoryService = categoryService;
            //_kwTermService = kwTermService;
        }

        #endregion

        #region Methods

        public async Task Delete(MWTEntityBanner obj)
        {
            await _mwtEntityBannerRepository.DeleteAsync(obj, true);

        }

        public async Task<IPagedList<MWTEntityBanner>> GetAllAsync(int entityId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var rez = await _mwtEntityBannerRepository.GetAllAsync(query =>
            {
                return from mbpluginWidgetsCatalog in query
                      
                       orderby mbpluginWidgetsCatalog.Id
                       select mbpluginWidgetsCatalog;
            });

            var records = new PagedList<MWTEntityBanner>(rez, pageIndex, pageSize);

            return records;
        }

        public async Task<bool> IsRecordExistByEntityId(int entityId, int Id, string widgetZone, string entityType = "Category")
        {
            return await _mwtEntityBannerRepository.Table.Where(m => /*m.EntityId == entityId &&*/ m.WidgetZone == widgetZone && m.EntityType == entityType && m.Id != Id).AnyAsync();

        }

        public async Task<MWTEntityBanner> GetByIdAsync(int id)
        {
            return await _mwtEntityBannerRepository.GetByIdAsync(id);
        }

        public async Task Insert(MWTEntityBanner obj)
        {
            await _mwtEntityBannerRepository.InsertAsync(obj);
        }

        public async Task Update(MWTEntityBanner obj)
        {
            await _mwtEntityBannerRepository.UpdateAsync(obj, true);
        }

        public async Task<MWTEntityBanner> GetWidgetByEntityId(int entityId, string widgetZone, string entityType)
        {
            return await (from mwtEntityBanner in _mwtEntityBannerRepository.Table
                          join mwtEntityBannerMapping in _mwtEntityBannerMappingRepository.Table
                          on mwtEntityBanner.Id equals mwtEntityBannerMapping.BannerId
                          where mwtEntityBannerMapping.EntityId == entityId &&
                          mwtEntityBanner.WidgetZone == widgetZone &&
                          mwtEntityBanner.EntityType == entityType
                          orderby mwtEntityBanner.BannerId descending
                          select mwtEntityBanner).FirstOrDefaultAsync();

        }

        public async Task<(bool, string)> IsWidgetZoneMappedWithEntityId(IList<int> entityIds, int id, string widgetZone, string entityType = "Category")
        {
            foreach (var entityId in entityIds)
            {
                if (await (from mwtEntityBanner in _mwtEntityBannerRepository.Table
                           join mwtEntityBannerMapping in _mwtEntityBannerMappingRepository.Table
                           on mwtEntityBanner.Id equals mwtEntityBannerMapping.BannerId
                           where mwtEntityBannerMapping.EntityId == entityId &&
                           mwtEntityBanner.WidgetZone == widgetZone &&
                           mwtEntityBanner.EntityType == entityType &&
                           mwtEntityBannerMapping.BannerId != id
                           select mwtEntityBanner).AnyAsync())
                {
                    return (true, entityType == "Category" ? (await _categoryService.GetCategoryByIdAsync(entityId))?.Name ?? "" : "");
                     //   :
                     //   (await _kwTermService.GetKwTermByIdAsync(entityId))?.Name ?? "");

                }

            }

            return (false, "");

        }

        public async Task<List<int>> GetEntitiesMappedWithBanner(int id)
        {
            var query = from bannerEntityMapping in _mwtEntityBannerMappingRepository.Table
                        where bannerEntityMapping.BannerId == id
                        select bannerEntityMapping.EntityId;
            return await query.ToListAsync();
        }
        public async Task<string> GetEntityNamesMappedWithBanner(int id, string entityType)
        {
            string entityNames = string.Empty;
            var query = from bannerEntityMapping in _mwtEntityBannerMappingRepository.Table
                        where bannerEntityMapping.BannerId == id
                        select bannerEntityMapping.EntityId;
            var ids = await query.ToListAsync();

            if (entityType == "Category")
            {
                entityNames = string.Join(",", (await _categoryService.GetCategoriesByIdsAsync(ids.ToArray())).Select(c => c.Name).ToArray());
            }
            else
            {
               // entityNames = string.Join(",", (await _kwTermService.GetkwTermsByIdsAsync(ids.ToArray())).Select(c => c.Name).ToArray());
            }
            return entityNames;
        }
        public async Task InsertEntityBannerMapping(MWTEntityBannerEntityMapping mwtEntityBannerEntityMapping)
        {
            await _mwtEntityBannerMappingRepository.InsertAsync(mwtEntityBannerEntityMapping);
        }


        public async Task DeleteEntityBannerMapping(MWTEntityBannerEntityMapping mwtEntityBannerEntityMapping)
        {
            foreach (var _mwtEntityBannerEntityMapping in await _mwtEntityBannerMappingRepository.Table.Where(mwtE => mwtE.BannerId == mwtEntityBannerEntityMapping.BannerId && mwtE.EntityId == mwtEntityBannerEntityMapping.EntityId).ToListAsync())
            {
                await _mwtEntityBannerMappingRepository.DeleteAsync(_mwtEntityBannerEntityMapping);
            }
        }
        #endregion
    }
}
