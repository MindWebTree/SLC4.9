using MWT.Nop.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Customizations.Custom
{
    public partial interface IFiltersMappingByEntityService
    {
        Task UpdateAsync(FiltersMappingByEntity obj);
        Task DeleteAsync(FiltersMappingByEntity obj);
        Task InsertAsync(FiltersMappingByEntity obj);
        Task<FiltersMappingByEntity> GetById(int Id);
        Task<IList<FiltersMappingByEntity>> GetFiltersMappingByEntityByFilterType(int entityId, string entityType, string filterType);
        Task<IList<FiltersMappingByEntity>> GetFiltersMappingByEntity(int entityId, string entityType);
        Task<bool> IsMappingExist(int entityId, string entityType, int filterId, string filterType, int id);
        Task<FiltersMappingByEntity> GetFilterMapping(string entityType,int entityId, int filterId, string filterType);
    }
}
