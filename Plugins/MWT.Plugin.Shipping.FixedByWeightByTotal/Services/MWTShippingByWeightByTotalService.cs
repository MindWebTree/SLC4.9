using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using MWT.Plugin.Shipping.FixedByWeightByTotal.Domain;
using System.Collections.Generic;
using Nop.Services.Configuration;

namespace MWT.Plugin.Shipping.FixedByWeightByTotal.Services
{
    /// <summary>
    /// Represents service shipping by weight service implementation
    /// </summary>
    public partial class MWTShippingByWeightByTotalService : IMWTShippingByWeightByTotalService
    {
        #region Constants

        /// <summary>
        /// Key for caching all records
        /// </summary>
        private readonly CacheKey _shippingByWeightByTotalAllKey = new CacheKey("MWT.shippingbyweightbytotal.all");
        private const string SHIPPINGBYWEIGHTBYTOTAL_PATTERN_KEY = "MWT.shippingbyweightbytotal.";

        #endregion

        #region Fields

        private readonly IRepository<MWTShippingByWeightByTotalRecord> _sbwtRepository;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IMWTShippingZoneService _mwtShippingZoneService;
        private readonly ISettingService _settingService;

        #endregion

        #region Ctor

        public MWTShippingByWeightByTotalService(IRepository<MWTShippingByWeightByTotalRecord> sbwtRepository,
            IStaticCacheManager staticCacheManager, IMWTShippingZoneService mwtShippingZoneService,
            ISettingService settingService)
        {
            _sbwtRepository = sbwtRepository;
            _staticCacheManager = staticCacheManager;
            _mwtShippingZoneService = mwtShippingZoneService;
            _settingService = settingService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get all shipping by weight records
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of the shipping by weight record
        /// </returns>
        public virtual async Task<IPagedList<MWTShippingByWeightByTotalRecord>> GetAllAsync(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var rez = await _sbwtRepository.GetAllAsync(query =>
            {
                return from sbw in query
                       orderby sbw.StoreId, sbw.CountryId, sbw.StateProvinceId, sbw.ZoneId, sbw.ShippingMethodId,
                           sbw.WeightFrom, sbw.OrderSubtotalFrom
                       select sbw;
            }, cache => cache.PrepareKey(_shippingByWeightByTotalAllKey));

            var records = new PagedList<MWTShippingByWeightByTotalRecord>(rez, pageIndex, pageSize);

            return records;
        }

        /// <summary>
        /// Filter Shipping Weight Records
        /// </summary>
        /// <param name="shippingMethodId">Shipping method identifier</param>
        /// <param name="storeId">Store identifier</param>
        /// <param name="warehouseId">Warehouse identifier</param>
        /// <param name="countryId">Country identifier</param>
        /// <param name="stateProvinceId">State identifier</param>
        /// <param name="zip">Zip postal code</param>
        /// <param name="weight">Weight</param>
        /// <param name="orderSubtotal">Order subtotal</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of the shipping by weight record
        /// </returns>
        public virtual async Task<IPagedList<MWTShippingByWeightByTotalRecord>> FindRecordsAsync(int shippingMethodId, int storeId, int warehouseId,
            int countryId, int stateProvinceId, string zip, decimal? weight, decimal? orderSubtotal, int pageIndex, int pageSize)
        {
            zip = zip?.Trim() ?? string.Empty;

            //filter by weight and shipping method
            var existingRates = (await GetAllAsync())
                .Where(sbw => sbw.ShippingMethodId == shippingMethodId)
                .ToList();

            //filter by order subtotal
            var matchedBySubtotal = !orderSubtotal.HasValue ? existingRates :
                existingRates.Where(sbw => orderSubtotal >= sbw.OrderSubtotalFrom && orderSubtotal <= sbw.OrderSubtotalTo);

            //filter by store
            var matchedByStore = storeId == 0
                ? matchedBySubtotal
                : matchedBySubtotal.Where(r => r.StoreId == storeId || r.StoreId == 0);

            //filter by warehouse
            var matchedByWarehouse = warehouseId == 0
                ? matchedByStore
                : matchedByStore.Where(r => r.WarehouseId == warehouseId || r.WarehouseId == 0);

            //filter by country
            var matchedByCountry = countryId == 0
                ? matchedByWarehouse
                : matchedByWarehouse.Where(r => r.CountryId == countryId || r.CountryId == 0);

            //filter by state/province
            var matchedByStateProvince = stateProvinceId == 0
                ? matchedByCountry
                : matchedByCountry.Where(r => r.StateProvinceId == stateProvinceId || r.StateProvinceId == 0);

            //filter by zip
            var zipWiseShipping = await _settingService.GetSettingByKeyAsync<bool>($"ShippingZipWise_{countryId}");
            if (zipWiseShipping)
            {
                List<MWTShippingByWeightByTotalRecord> matchedByZip = new List<MWTShippingByWeightByTotalRecord>();


                if (!string.IsNullOrEmpty(zip))
                    foreach (var _matchedByStateProvince in matchedByStateProvince.Where(M => M.ZoneId > 0))
                    {
                        var zipCodes = (await _mwtShippingZoneService.GetByIdAsync(_matchedByStateProvince.ZoneId))?.ZipCodes;
                        if (!string.IsNullOrEmpty(zipCodes))
                        {
                            if ((zipCodes.Replace("\n", "").Replace("\r", "").Replace("\t", "").ToLower()).Split(',').Contains(zip.ToLower()))
                                matchedByZip.Add(_matchedByStateProvince);
                        }
                    }
                //sort from particular to general, more particular cases will be the first
                var foundRecords = matchedByZip.OrderBy(r => r.StoreId == 0).ThenBy(r => r.WarehouseId == 0)
                    .ThenBy(r => r.CountryId == 0).ThenBy(r => r.StateProvinceId == 0);

                var records = new PagedList<MWTShippingByWeightByTotalRecord>(foundRecords.ToList(), pageIndex, pageSize);
                return records;
            }
            else
            {
                var foundRecords = matchedByStateProvince.OrderBy(r => r.StoreId == 0).ThenBy(r => r.WarehouseId == 0)
              .ThenBy(r => r.CountryId == 0).ThenBy(r => r.StateProvinceId == 0);

                var records = new PagedList<MWTShippingByWeightByTotalRecord>(foundRecords.ToList(), pageIndex, pageSize);
                return records;
            }


        }



        public virtual async Task<IPagedList<MWTShippingByWeightByTotalRecord>> FindRecordsBackendAsync(int shippingMethodId, int storeId, int warehouseId,
           int countryId, int stateProvinceId, string zip, decimal? weight, decimal? orderSubtotal, int pageIndex, int pageSize)
        {
            zip = zip?.Trim() ?? string.Empty;

            //filter by weight and shipping method
            var existingRates = (await GetAllAsync())
                .Where(sbw => sbw.ShippingMethodId == shippingMethodId)
                .ToList();

            //filter by order subtotal
            var matchedBySubtotal = !orderSubtotal.HasValue ? existingRates :
                existingRates.Where(sbw => orderSubtotal >= sbw.OrderSubtotalFrom && orderSubtotal <= sbw.OrderSubtotalTo);

            //filter by store
            var matchedByStore = storeId == 0
                ? matchedBySubtotal
                : matchedBySubtotal.Where(r => r.StoreId == storeId || r.StoreId == 0);

            //filter by warehouse
            var matchedByWarehouse = warehouseId == 0
                ? matchedByStore
                : matchedByStore.Where(r => r.WarehouseId == warehouseId || r.WarehouseId == 0);

            //filter by country
            var matchedByCountry = countryId == 0
                ? matchedByWarehouse
                : matchedByWarehouse.Where(r => r.CountryId == countryId || r.CountryId == 0);

            //filter by state/province
            var matchedByStateProvince = stateProvinceId == 0
                ? matchedByCountry
                : matchedByCountry.Where(r => r.StateProvinceId == stateProvinceId || r.StateProvinceId == 0);

            //filter by zip

            //sort from particular to general, more particular cases will be the first
            var foundRecords = matchedByStateProvince.OrderBy(r => r.StoreId == 0).ThenBy(r => r.WarehouseId == 0)
                .ThenBy(r => r.CountryId == 0).ThenBy(r => r.StateProvinceId == 0);

            var records = new PagedList<MWTShippingByWeightByTotalRecord>(foundRecords.ToList(), pageIndex, pageSize);

            return records;
        }
        /// <summary>
        /// Get a shipping by weight record by passed parameters
        /// </summary>
        /// <param name="shippingMethodId">Shipping method identifier</param>
        /// <param name="storeId">Store identifier</param>
        /// <param name="warehouseId">Warehouse identifier</param>
        /// <param name="countryId">Country identifier</param>
        /// <param name="stateProvinceId">State identifier</param>
        /// <param name="zip">Zip postal code</param>
        /// <param name="weight">Weight</param>
        /// <param name="orderSubtotal">Order subtotal</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the shipping by weight record
        /// </returns>
        public virtual async Task<MWTShippingByWeightByTotalRecord> FindRecordsAsync(int shippingMethodId, int storeId, int warehouseId,
            int countryId, int stateProvinceId, string zip, decimal weight, decimal orderSubtotal)
        {
            var foundRecords = await FindRecordsAsync(shippingMethodId, storeId, warehouseId, countryId, stateProvinceId, zip, weight, orderSubtotal, 0, int.MaxValue);

            return foundRecords.FirstOrDefault();
        }



        /// <summary>
        /// Get a shipping by weight record by identifier
        /// </summary>
        /// <param name="shippingByWeightRecordId">Record identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the shipping by weight record
        /// </returns>
        public virtual async Task<MWTShippingByWeightByTotalRecord> GetByIdAsync(int shippingByWeightRecordId)
        {
            return await _sbwtRepository.GetByIdAsync(shippingByWeightRecordId);
        }

        /// <summary>
        /// Insert the shipping by weight record
        /// </summary>
        /// <param name="shippingByWeightRecord">Shipping by weight record</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertShippingByWeightRecordAsync(MWTShippingByWeightByTotalRecord shippingByWeightRecord)
        {
            await _sbwtRepository.InsertAsync(shippingByWeightRecord, false);

            await _staticCacheManager.RemoveByPrefixAsync(SHIPPINGBYWEIGHTBYTOTAL_PATTERN_KEY);
        }

        /// <summary>
        /// Update the shipping by weight record
        /// </summary>
        /// <param name="shippingByWeightRecord">Shipping by weight record</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateShippingByWeightRecordAsync(MWTShippingByWeightByTotalRecord shippingByWeightRecord)
        {
            await _sbwtRepository.UpdateAsync(shippingByWeightRecord, false);

            await _staticCacheManager.RemoveByPrefixAsync(SHIPPINGBYWEIGHTBYTOTAL_PATTERN_KEY);
        }

        /// <summary>
        /// Delete the shipping by weight record
        /// </summary>
        /// <param name="shippingByWeightRecord">Shipping by weight record</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteShippingByWeightRecordAsync(MWTShippingByWeightByTotalRecord shippingByWeightRecord)
        {
            await _sbwtRepository.DeleteAsync(shippingByWeightRecord, false);

            await _staticCacheManager.RemoveByPrefixAsync(SHIPPINGBYWEIGHTBYTOTAL_PATTERN_KEY);
        }




        #endregion
    }
}
