using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using MWT.Plugin.Shipping.FixedByWeightByTotal.Domain;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
//using Nop.Services.Customizations.Custom;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Org.BouncyCastle.Asn1.IsisMtt.X509;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MWT.Plugin.Shipping.FixedByWeightByTotal.Services
{
    public partial class MWTExpectedDeliveryDateService : IMWTExpectedDeliveryDateService
    {
        #region Fields
        private readonly CacheKey _mwtExpectedDeliveryDateAllKey = new CacheKey("MWT.shippingbyweightbytotal.ExpectedDeliveryDate.all");
        private const string MWTExpectedDeliveryDate_PATTERN_KEY = "MWT.shippingbyweightbytotal.ExpectedDeliveryDate";

        private readonly CacheKey _mwtExpectedDeliveryDateMappingAllKey = new CacheKey("MWT.shippingbyweightbytotal.ExpectedDeliveryDateMapping-{0}-{1}");
        private const string MWTExpectedDeliveryDateMapping_PATTERN_KEY = "MWT.shippingbyweightbytotal.ExpectedDeliveryDateMapping";

        private readonly IRepository<MWTExpectedDeliveryDate> _mwtExpectedDeliveryDateRepository;
        private readonly IRepository<MWTExpectedDeliveryDate_Entity_Mapping> _mwtExpectedDeliveryDate_Entity_MappingRepository;
        private readonly IRepository<MWTShippingZone> _mwtShippingZoneeRepository;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly ICategoryService _categoryService;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IMWTEstimationDeliveryDateNotificationService _mwtEstimationDeliveryDateNotificationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor
        public MWTExpectedDeliveryDateService(IRepository<MWTExpectedDeliveryDate> mwtExpectedDeliveryDateRepository,
           IRepository<MWTExpectedDeliveryDate_Entity_Mapping> mwtExpectedDeliveryDate_Entity_MappingRepository,
           IStaticCacheManager staticCacheManager, ICategoryService categoryService,
          IRepository<MWTShippingZone> mwtShippingZoneeRepository, ILocalizationService localizationService,
          ISettingService settingService,
          IMWTEstimationDeliveryDateNotificationService mwtEstimationDeliveryDateNotificationService,
          IHttpContextAccessor httpContextAccessor, IWorkflowMessageService workflowMessageService,
           IWorkContext workContext, IStoreContext storeContext)
        {
            this._mwtExpectedDeliveryDateRepository = mwtExpectedDeliveryDateRepository;
            this._mwtExpectedDeliveryDate_Entity_MappingRepository = mwtExpectedDeliveryDate_Entity_MappingRepository;
            this._staticCacheManager = staticCacheManager;
            this._categoryService = categoryService;
            this._mwtShippingZoneeRepository = mwtShippingZoneeRepository;
            this._localizationService = localizationService;
            this._settingService = settingService;
            this._mwtEstimationDeliveryDateNotificationService = mwtEstimationDeliveryDateNotificationService;
            this._httpContextAccessor = httpContextAccessor;
            this._workflowMessageService = workflowMessageService;
            this._workContext = workContext;
            this._storeContext = storeContext;
        }

        #endregion

        #region Methods
        public async Task Delete(MWTExpectedDeliveryDate mwtExpectedDeliveryDate)
        {
            await _mwtExpectedDeliveryDateRepository.DeleteAsync(mwtExpectedDeliveryDate, false);

            await _staticCacheManager.RemoveByPrefixAsync(MWTExpectedDeliveryDate_PATTERN_KEY);

        }

        public async Task DeleteEntityMapping(MWTExpectedDeliveryDate_Entity_Mapping mwtExpectedDeliveryDate_Entity_Mapping)
        {
            await _mwtExpectedDeliveryDate_Entity_MappingRepository.DeleteAsync(mwtExpectedDeliveryDate_Entity_Mapping, false);

            await _staticCacheManager.RemoveByPrefixAsync(MWTExpectedDeliveryDateMapping_PATTERN_KEY);
        }

        public async Task<MWTExpectedDeliveryDate> GetByIdAsync(int id)
        {
            return await _mwtExpectedDeliveryDateRepository.GetByIdAsync(id);
        }

        public async Task<MWTExpectedDeliveryDate_Entity_Mapping> GetEntityMappingByIDAsync(int id)
        {
            return await _mwtExpectedDeliveryDate_Entity_MappingRepository.GetByIdAsync(id);
        }

        public async Task<List<MWTExpectedDeliveryDate_Entity_Mapping>> GetEntityMappingsByExpectedDeliveryDateIDAndTypeAsync(int MWTExpectedDeliveryDateID, string entityType)
        {
            var cachekey = _staticCacheManager.PrepareKey(_mwtExpectedDeliveryDateMappingAllKey, MWTExpectedDeliveryDateID, entityType);
            var query = from mapp in _mwtExpectedDeliveryDate_Entity_MappingRepository.Table
                        where mapp.MWTExpectedDeliveryDateID == MWTExpectedDeliveryDateID && mapp.EntityType == entityType
                        select mapp;

            return
                 await _staticCacheManager.GetAsync(cachekey,
                 async () => await query.ToListAsync());
        }

        public async Task<IPagedList<MWTExpectedDeliveryDate>> GetMWTExpectedDeliveryDateList(int pageIndex, int pageSize)
        {
            var rez = await _mwtExpectedDeliveryDateRepository.GetAllAsync(query =>
            {
                return from sbw in query
                       orderby sbw.Id
                       select sbw;
            }, cache => cache.PrepareKey(_mwtExpectedDeliveryDateAllKey));

            var records = new PagedList<MWTExpectedDeliveryDate>(rez, pageIndex, pageSize);

            return records;
        }

        public async Task Insert(MWTExpectedDeliveryDate mwtExpectedDeliveryDate)
        {
            mwtExpectedDeliveryDate.CreatedOnUtc = DateTime.UtcNow;
            mwtExpectedDeliveryDate.UpdatedOnUtc = DateTime.UtcNow;
            await _mwtExpectedDeliveryDateRepository.InsertAsync(mwtExpectedDeliveryDate, false);

            await _staticCacheManager.RemoveByPrefixAsync(MWTExpectedDeliveryDate_PATTERN_KEY);
        }

        public async Task InsertEntityMapping(MWTExpectedDeliveryDate_Entity_Mapping mwtExpectedDeliveryDate_Entity_Mapping)
        {
            mwtExpectedDeliveryDate_Entity_Mapping.CreatedOnUtc = DateTime.UtcNow;
            mwtExpectedDeliveryDate_Entity_Mapping.UpdatedOnUtc = DateTime.UtcNow;
            await _mwtExpectedDeliveryDate_Entity_MappingRepository.InsertAsync(mwtExpectedDeliveryDate_Entity_Mapping, false);

            await _staticCacheManager.RemoveByPrefixAsync(MWTExpectedDeliveryDateMapping_PATTERN_KEY);
        }

        public async Task Update(MWTExpectedDeliveryDate mwtExpectedDeliveryDate)
        {
            mwtExpectedDeliveryDate.UpdatedOnUtc = DateTime.UtcNow;
            await _mwtExpectedDeliveryDateRepository.UpdateAsync(mwtExpectedDeliveryDate, false);

            await _staticCacheManager.RemoveByPrefixAsync(MWTExpectedDeliveryDate_PATTERN_KEY);
        }

        public async Task UpdateEntityMapping(MWTExpectedDeliveryDate_Entity_Mapping mwtExpectedDeliveryDate_Entity_Mapping)
        {
            mwtExpectedDeliveryDate_Entity_Mapping.UpdatedOnUtc = DateTime.UtcNow;
            await _mwtExpectedDeliveryDate_Entity_MappingRepository.UpdateAsync(mwtExpectedDeliveryDate_Entity_Mapping, false);

            await _staticCacheManager.RemoveByPrefixAsync(MWTExpectedDeliveryDateMapping_PATTERN_KEY);
        }

        public async Task<String> GetShippingEstimateDate(int productID, string postalCode)
        {

            MWTExpectedDeliveryDate deliveryDate = await (from mapping in _mwtExpectedDeliveryDate_Entity_MappingRepository.Table
                                                          join expectedDeliveryDate in _mwtExpectedDeliveryDateRepository.Table
                                                          on mapping.MWTExpectedDeliveryDateID equals expectedDeliveryDate.Id
                                                          join zone in _mwtShippingZoneeRepository.Table
                                                          on expectedDeliveryDate.ZoneID equals zone.Id
                                                          where mapping.EntityID == productID && mapping.EntityType == "Product"
                                                          && zone.ZipCodes.Contains(postalCode)
                                                          && expectedDeliveryDate.ExpectedMinNoOfDays > 0
                                                          select expectedDeliveryDate
                        ).FirstOrDefaultAsync();

            if (deliveryDate == null)
            {
                var categories = await _categoryService.GetProductCategoriesByProductIdAsync(productID);
                foreach (var category in categories)
                {
                    deliveryDate = await (from mapping in _mwtExpectedDeliveryDate_Entity_MappingRepository.Table
                                          join expectedDeliveryDate in _mwtExpectedDeliveryDateRepository.Table
                                          on mapping.MWTExpectedDeliveryDateID equals expectedDeliveryDate.Id
                                          join zone in _mwtShippingZoneeRepository.Table
                                          on expectedDeliveryDate.ZoneID equals zone.Id
                                          where mapping.EntityID == category.Id && mapping.EntityType == "Category"
                                          && zone.ZipCodes.Contains(postalCode)
                                          && expectedDeliveryDate.ExpectedMinNoOfDays > 0
                                          select expectedDeliveryDate
                                                            ).FirstOrDefaultAsync();
                    if (deliveryDate != null)
                        break;
                }
            }

            if (deliveryDate == null)
            {
                deliveryDate = await (from mapping in _mwtExpectedDeliveryDate_Entity_MappingRepository.Table
                                      join expectedDeliveryDate in _mwtExpectedDeliveryDateRepository.Table
                                      on mapping.MWTExpectedDeliveryDateID equals expectedDeliveryDate.Id
                                      join zone in _mwtShippingZoneeRepository.Table
                                      on expectedDeliveryDate.ZoneID equals zone.Id
                                      where mapping.EntityID == 0 && mapping.EntityType == "Category"
                                      && zone.ZipCodes.Contains(postalCode)
                                      && expectedDeliveryDate.ExpectedMinNoOfDays > 0
                                      select expectedDeliveryDate
                                                           ).FirstOrDefaultAsync();
            }

            if (deliveryDate == null)
            {
                // Implement code there to send Notifications
                // validating spam activity for last 24 hours
                if (!await this._mwtEstimationDeliveryDateNotificationService.CheckSpamActivity(postalCode))
                {
                    // Ip address
                    var Ip = this._httpContextAccessor.HttpContext.Connection.RemoteIpAddress;
                    await _mwtEstimationDeliveryDateNotificationService.Insert(new MWTEstimationDeliveryDateNotification()
                    {
                        CreatedOn = DateTime.UtcNow,
                        IpAddress = Ip == null ? "" : Ip.ToString(),
                        ZipCode = postalCode

                    });

                    // Send Notifications

                    //var customer = await _workContext.GetCurrentCustomerAsync();
                    //await this._workflowMessageService.SendSupportNotificationZipCodeNotFound(
                    //    (await _workContext.GetWorkingLanguageAsync()).Id,
                    //    await _storeContext.GetCurrentStoreAsync(), postalCode, Ip == null ? "" : Ip.ToString(),
                    //    customer == null ? "" : customer.Username,
                    //    customer == null ? "" : customer.Email);

                }
                return String.Format(await _localizationService.GetResourceAsync("MWT.Plugins.Shipping.FixedByWeightByTotal.MWTExpectedDeliveryDate.NotFound"), postalCode);
            }

            if (deliveryDate.ExpectedMinNoOfDays <= 0)
                deliveryDate.ExpectedMinNoOfDays = await _settingService.GetSettingByKeyAsync<int>("MWT.Plugins.Shipping.FixedByWeightByTotal.DefaultMinOfDays");

            if (deliveryDate.ExpectedMaxNoOfDays <= 0 || deliveryDate.ExpectedMaxNoOfDays <= deliveryDate.ExpectedMinNoOfDays)
                deliveryDate.ExpectedMaxNoOfDays = deliveryDate.ExpectedMinNoOfDays
                                                          + await _settingService.GetSettingByKeyAsync<int>("MWT.Plugins.Shipping.FixedByWeightByTotal.Default.Min.Max.Range");


            string expectedMinDate = DateTime.Now.AddDays(deliveryDate.ExpectedMinNoOfDays).ToString("MMMM dd");
            string expectedMaxDate = DateTime.Now.AddDays(deliveryDate.ExpectedMaxNoOfDays).ToString("MMMM dd");



            return
                string.Format(
                await _localizationService.GetResourceAsync("MWT.Plugins.Shipping.FixedByWeightByTotal.MWTExpectedDeliveryDate.Message"),
                expectedMinDate, expectedMaxDate, postalCode
                );


        }

        #endregion
    }
}
