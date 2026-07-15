using MWT.Nop.Plugin.Widgets.Catalog.Domain;
using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Widgets.Catalog.Services
{
    public partial interface IMWTEntityBannerService
    {
        Task<IPagedList<MWTEntityBanner>> GetAllAsync(int entityId, int pageIndex = 0, int pageSize = int.MaxValue);
        Task<MWTEntityBanner> GetByIdAsync(int id);
        Task Insert(MWTEntityBanner obj);
        Task Update(MWTEntityBanner obj);
        Task Delete(MWTEntityBanner obj);
        Task<bool> IsRecordExistByEntityId(int entityId, int Id, string WidgetZone, string entityType = "Category");
        Task<MWTEntityBanner> GetWidgetByEntityId(int entityId, string widgetZone, string entityType = "Category");

        Task<(bool, string)> IsWidgetZoneMappedWithEntityId(IList<int> entityIds, int id, string widgetZone, string entityType = "Category");
        Task<List<int>> GetEntitiesMappedWithBanner(int id);

        Task<string> GetEntityNamesMappedWithBanner(int id, string entityType);
        Task InsertEntityBannerMapping(MWTEntityBannerEntityMapping mwtEntityBannerEntityMapping);
        Task DeleteEntityBannerMapping(MWTEntityBannerEntityMapping mwtEntityBannerEntityMapping);
    }
}
