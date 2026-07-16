using MWT.Plugin.Shipping.FixedByWeightByTotal.Domain;
using Nop.Core;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace MWT.Plugin.Shipping.FixedByWeightByTotal.Services
{
    public partial interface IMWTExpectedDeliveryDateService
    {
        Task<MWTExpectedDeliveryDate> GetByIdAsync(int id);

        Task Insert(MWTExpectedDeliveryDate mwtExpectedDeliveryDate);

        Task Update(MWTExpectedDeliveryDate mwtExpectedDeliveryDate);

        Task Delete(MWTExpectedDeliveryDate mwtExpectedDeliveryDate);

        Task<IPagedList<MWTExpectedDeliveryDate>> GetMWTExpectedDeliveryDateList(int pageIndex, int pageSize);

        Task<MWTExpectedDeliveryDate_Entity_Mapping> GetEntityMappingByIDAsync(int id);

        Task InsertEntityMapping(MWTExpectedDeliveryDate_Entity_Mapping mwtExpectedDeliveryDate);

        Task UpdateEntityMapping(MWTExpectedDeliveryDate_Entity_Mapping mwtExpectedDeliveryDate);

        Task DeleteEntityMapping(MWTExpectedDeliveryDate_Entity_Mapping mwtExpectedDeliveryDate);

        Task<List<MWTExpectedDeliveryDate_Entity_Mapping>> GetEntityMappingsByExpectedDeliveryDateIDAndTypeAsync(int id, string type);
        Task<String> GetShippingEstimateDate(int productID, string postalCode);

    }
}
