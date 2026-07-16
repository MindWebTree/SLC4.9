using MWT.Plugin.Shipping.FixedByWeightByTotal.Domain;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Shipping.FixedByWeightByTotal.Services
{
    public partial class MWTShippingZoneService : IMWTShippingZoneService   
    {
        #region Fields
        private readonly CacheKey _shippingByWeightByTotalAllKey = new CacheKey("MWT.shippingbyweightbytotal.Zones.all");
        private const string SHIPPINGBYWEIGHTBYTOTAL_PATTERN_KEY = "MWT.shippingbyweightbytotal.";
        private readonly IRepository<MWTShippingZone> _sbwtRepository;
        private readonly IStaticCacheManager _staticCacheManager;

        #endregion

        #region Ctor

        public MWTShippingZoneService(IRepository<MWTShippingZone> sbwtRepository,
            IStaticCacheManager staticCacheManager)
        {
            _sbwtRepository = sbwtRepository;
            _staticCacheManager = staticCacheManager;
        }

        #endregion

        #region Methods

        public async Task Delete(MWTShippingZone MWTShippingZone)
        {
            await _sbwtRepository.DeleteAsync(MWTShippingZone, false);

            await _staticCacheManager.RemoveByPrefixAsync(SHIPPINGBYWEIGHTBYTOTAL_PATTERN_KEY);
        }

        public async Task<MWTShippingZone> GetByIdAsync(int zoneId)
        {
            return await _sbwtRepository.GetByIdAsync(zoneId);
        }

        public async Task<IPagedList<MWTShippingZone>> GetZones(int pageIndex, int pageSize)
        {
            var records = new PagedList<MWTShippingZone>((await GetAllAsync()).ToList(), pageIndex, pageSize);

            return records;
        }

        public async Task Insert(MWTShippingZone MWTShippingZone)
        {
            await _sbwtRepository.InsertAsync(MWTShippingZone, false);

            await _staticCacheManager.RemoveByPrefixAsync(SHIPPINGBYWEIGHTBYTOTAL_PATTERN_KEY);
        }

        public async Task Update(MWTShippingZone MWTShippingZone)
        {
            await _sbwtRepository.UpdateAsync(MWTShippingZone, false);

            await _staticCacheManager.RemoveByPrefixAsync(SHIPPINGBYWEIGHTBYTOTAL_PATTERN_KEY);
        }

        #endregion

        #region Utilities

        public virtual async Task<IPagedList<MWTShippingZone>> GetAllAsync(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var rez = await _sbwtRepository.GetAllAsync(query =>
            {
                return from sbw in query
                       orderby sbw.Id
                       select sbw;
            }, cache => cache.PrepareKey(_shippingByWeightByTotalAllKey));

            var records = new PagedList<MWTShippingZone>(rez, pageIndex, pageSize);

            return records;
        }

        #endregion
    }
}
