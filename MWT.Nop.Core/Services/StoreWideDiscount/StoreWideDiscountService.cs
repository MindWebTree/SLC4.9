using LinqToDB.Data;
using MWT.Nop.Core.Domain.StoreWideDiscount;
using MWT.Nop.Core.Service.Catalog;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Services.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MWT.Nop.Core.Domain.StoreWideDiscount;

namespace MWT.Nop.Core.Service.StoreWideDiscount
{
    public partial class StoreWideDiscountService : IStoreWideDiscountService
    {
        #region Fields 
        private readonly IRepository<StoreWideDiscountSetting> _storeWideDiscountSettingRepository;
        private readonly IRepository<StoreWideProductDiscountInfo> _storeWideProductDiscountInfoRepository;
        private readonly IRepository<StoreWideProductDiscountHistory> _storeWideProductDiscountHistoryRepository;
        protected readonly IRepository<ProductCategory> _productCategoryRepository;
        protected readonly IStaticCacheManager _staticCacheManager;
        #endregion

        #region Ctor
        public StoreWideDiscountService( 
            IRepository<StoreWideDiscountSetting> storeWideDiscountSettingRepository,
             IRepository<StoreWideProductDiscountInfo> storeWideProductDiscountInfoRepository,
             IRepository<StoreWideProductDiscountHistory> storeWideProductDiscountHistoryRepository,
             IRepository<ProductCategory> productCategoryRepository,
             IStaticCacheManager staticCacheManager
             )
        {   
            _storeWideDiscountSettingRepository = storeWideDiscountSettingRepository;
            _storeWideProductDiscountInfoRepository = storeWideProductDiscountInfoRepository;
            _storeWideProductDiscountHistoryRepository = storeWideProductDiscountHistoryRepository;
            _productCategoryRepository = productCategoryRepository;
            _staticCacheManager = staticCacheManager;
        }

        #endregion
        #region Methods

        public async Task<StoreWideProductDiscountInfo> GetStoreWideProductDiscountInfoByProductIdAsync(int productId)
        {
            var query = _storeWideProductDiscountInfoRepository.Table;

            query =
            from o in query
            where o.ProductId == productId
            select o;

            return await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(CustomNopCatalogDefaults.StoreWideProductDiscountInfoCacheKey, productId), async () => await query.FirstOrDefaultAsync());

        }


        #endregion
    }
}
