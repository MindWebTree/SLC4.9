using MWT.Plugin.Shipping.FixedByWeightByTotal.Domain;
using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Shipping.FixedByWeightByTotal.Services
{
    public partial interface IMWTShippingZoneService
    {
        Task<MWTShippingZone> GetByIdAsync(int zoneId);

        Task Insert(MWTShippingZone MWTShippingZone);

        Task Update(MWTShippingZone MWTShippingZone);

        Task Delete(MWTShippingZone MWTShippingZone);

        Task<IPagedList<MWTShippingZone>> GetZones(int pageIndex, int pageSize);
    }
}
