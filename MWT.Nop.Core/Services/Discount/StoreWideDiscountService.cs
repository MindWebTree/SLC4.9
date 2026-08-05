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

namespace MWT.Nop.Core.Service.Discount
{
    public partial class StoreWideDiscountService : IStoreWideDiscountService
    {
        #region Fields 
        private readonly IRepository<StoreWideDiscountSetting> _storeWideDiscountSettingRepository;
        private readonly IRepository<StoreWideDiscount> _storeWideDiscountRepository;
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
             IStaticCacheManager staticCacheManager,
             IRepository<StoreWideDiscount> storeWideDiscountRepository
             )
        {   
            _storeWideDiscountSettingRepository = storeWideDiscountSettingRepository;
            _storeWideProductDiscountInfoRepository = storeWideProductDiscountInfoRepository;
            _storeWideProductDiscountHistoryRepository = storeWideProductDiscountHistoryRepository;
            _productCategoryRepository = productCategoryRepository;
            _staticCacheManager = staticCacheManager;
            _storeWideDiscountRepository = storeWideDiscountRepository;
        }

        #endregion
        #region Methods

        #region StoreWideDiscount 
        public async Task<StoreWideDiscount> GetStoreWideDiscountByIdAsync(int storeWideDiscountId)
        {
            return await _storeWideDiscountRepository.GetByIdAsync(storeWideDiscountId, cache => default);
        }
        public async Task DeleteStoreWideDiscountAsync(StoreWideDiscount storeWideDiscount)
        {
            await _storeWideDiscountRepository.DeleteAsync(storeWideDiscount);
        }
        public async Task InsertStoreWideDiscountAsync(StoreWideDiscount storeWideDiscount)
        {
            await _storeWideDiscountRepository.InsertAsync(storeWideDiscount);
        }
        public async Task UpdateStoreWideDiscountAsync(StoreWideDiscount storeWideDiscount)
        {
            await _storeWideDiscountRepository.UpdateAsync(storeWideDiscount);
        }
        public async Task<IPagedList<StoreWideDiscount>> GetAllStoreWideDiscountAsync(int pageIndex, int pageSize)
        {
            var query = _storeWideDiscountRepository.Table;
            query = query.Where(s => s.IsDeleted == false);

            return await query.OrderByDescending(o => o.Id).ToPagedListAsync(pageIndex, pageSize);
        }
        public async Task<bool> IsStoreWideDiscountExistInDateRange(DateTime startDate, DateTime endDate, int storeDiscountId)
        {
            var query = _storeWideDiscountRepository.Table;
            query = query.Where(s => s.Id != storeDiscountId);
            query = query.Where(s => (s.StartDate <= startDate && s.EndDate >= startDate && !s.IsDeleted) || (s.StartDate <= endDate && s.EndDate >= endDate && !s.IsDeleted));
            return await query.AnyAsync();
        }
        public async Task<List<StoreWideDiscount>> GetStoreWideDiscountByDate(DateTime date)
        {
            var query = _storeWideDiscountRepository.Table;
            return await query.Where(s => s.StartDate <= date && s.EndDate >= date).ToListAsync();
        }

        public async Task<StoreWideDiscount> GetProductSaleInfo(int productId)
        {
            var storeWideDiscount = await (from _storeWideDiscount in _storeWideDiscountRepository.Table
                                           join _productDiscountInfo in _storeWideProductDiscountInfoRepository.Table
                                           on _storeWideDiscount.Id equals _productDiscountInfo.StoreWideDiscountId
                                           where _productDiscountInfo.ProductId == productId
                                           && _storeWideDiscount.EndDate >= DateTime.Now
                                           && _productDiscountInfo.Discount > 0
                                           orderby _productDiscountInfo.Id descending
                                           select _storeWideDiscount).FirstOrDefaultAsync();
            return storeWideDiscount;

        }

        #endregion

        #region StoreWideDiscountSetting
        public async Task<StoreWideDiscountSetting> GetStoreWideDiscountSettingByIdAsync(int storeWideDiscountSettingId)
        {
            return await _storeWideDiscountSettingRepository.GetByIdAsync(storeWideDiscountSettingId, cache => default);
        }
        public async Task DeleteStoreWideDiscountSettingAsync(StoreWideDiscountSetting storeWideDiscountSetting)
        {
            await _storeWideDiscountSettingRepository.DeleteAsync(storeWideDiscountSetting);
        }
        public async Task InsertStoreWideDiscountSettingAsync(StoreWideDiscountSetting storeWideDiscountSetting)
        {
            await _storeWideDiscountSettingRepository.InsertAsync(storeWideDiscountSetting);
        }
        public async Task UpdateStoreWideDiscountSettingAsync(StoreWideDiscountSetting storeWideDiscountSetting)
        {
            await _storeWideDiscountSettingRepository.UpdateAsync(storeWideDiscountSetting);
        }
        public async Task<IPagedList<StoreWideDiscountSetting>> GetAllStoreWideDiscountSettingAsync(int storeWideDiscountId, int pageIndex, int pageSize, bool showHidden = false)
        {
            var query = _storeWideDiscountSettingRepository.Table;
            if (!showHidden)
            {
                query = query.Where(d => d.IsDeleted != true);
            }

            query = query.Where(q => q.StoreWideDiscountId == storeWideDiscountId);
            return await query.OrderByDescending(o => o.Id).ToPagedListAsync(pageIndex, pageSize);
        }
        public async Task<bool> ApplyDiscount(StoreWideDiscountSetting discount)
        {
            await _storeWideDiscountSettingRepository.EntityFromSqlAsync("Sp_StoreWideDiscount",
                      new DataParameter()
                      {
                          DataType = LinqToDB.DataType.Boolean,
                          Value = discount.FullStore,
                          Direction = System.Data.ParameterDirection.Input,
                          Name = "@FullStore",

                      },

                      new DataParameter()
                      {
                          DataType = LinqToDB.DataType.Decimal,
                          Value = discount.Discount,
                          Direction = System.Data.ParameterDirection.Input,
                          Name = "@Discount",

                      },
                      new DataParameter()
                      {
                          DataType = LinqToDB.DataType.VarChar,
                          Value = discount.CategoryIds,
                          Direction = System.Data.ParameterDirection.Input,
                          Name = "@CategoryIds",

                      },
                        new DataParameter()
                        {
                            DataType = LinqToDB.DataType.VarChar,
                            Value = discount.ProductIds ?? "",
                            Direction = System.Data.ParameterDirection.Input,
                            Name = "@ProductIds",

                        },

                      new DataParameter()
                      {
                          DataType = LinqToDB.DataType.VarChar,
                          Value = discount.InfoHelpText,
                          Direction = System.Data.ParameterDirection.Input,
                          Name = "@InfoHelpText",

                      },
                      new DataParameter()
                      {
                          DataType = LinqToDB.DataType.VarChar,
                          Value = discount.InfoText,
                          Direction = System.Data.ParameterDirection.Input,
                          Name = "@InfoText",

                      }
                      );

            await _staticCacheManager.RemoveByPrefixAsync(CustomNopCatalogDefaults.StoreWideProductDiscountInfoPrefix);
            return true;
        }
        #endregion

        #region StoreWideProductDiscountInfo
        public async Task InsertStoreWideProductDiscountInfoAsync(StoreWideProductDiscountInfo storeWideProductDiscountInfo)
        {
            await _storeWideProductDiscountInfoRepository.InsertAsync(storeWideProductDiscountInfo);
        }
        public async Task UpdateStoreWideProductDiscountInfoAsync(StoreWideProductDiscountInfo storeWideProductDiscountInfo)
        {
            await _storeWideProductDiscountInfoRepository.UpdateAsync(storeWideProductDiscountInfo);
        }

        public async Task<StoreWideProductDiscountInfo> GetStoreWideProductDiscountInfoByProductIdAsync(int productId)
        {
            var query = _storeWideProductDiscountInfoRepository.Table;

            query =
            from o in query
            where o.ProductId == productId
            select o;

            return await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(CustomNopCatalogDefaults.StoreWideProductDiscountInfoCacheKey, productId), async () => await query.FirstOrDefaultAsync());

        }
        public async Task<IPagedList<StoreWideProductDiscountInfo>> SearchStoreWideProductDiscountInfo(int categoryId, List<int> productIds, int pageIndex, int pageSize)
        {
            var offerLogQuery = _storeWideProductDiscountInfoRepository.Table;
            if (categoryId > 0)
            {
                var productCategoryQuery =
                    from pc in _productCategoryRepository.Table
                    where pc.CategoryId == categoryId
                    group pc by pc.ProductId into pc
                    select new
                    {
                        ProductId = pc.Key,

                    };

                offerLogQuery =
                    from o in offerLogQuery
                    join pc in productCategoryQuery on o.ProductId equals pc.ProductId
                    select o;
            }

            if (productIds.Count > 0)
            {
                offerLogQuery =
                from o in offerLogQuery
                where productIds.Contains(o.ProductId)
                orderby o.UpdatedOn
                select o;
            }
            return await offerLogQuery.OrderByDescending(o => o.UpdatedOn).ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task<List<StoreWideProductDiscountInfo>> GetStoreWideProductDiscountInfoHaveDiscountAppliedAsync()
        {
            var offerLogQuery = _storeWideProductDiscountInfoRepository.Table;
            offerLogQuery = offerLogQuery.Where(o => o.Discount > 0 || (!string.IsNullOrEmpty(o.InfoText) || !string.IsNullOrEmpty(o.InfoHelpText)));
            return await offerLogQuery.ToListAsync();
        }

        #endregion

        #region  StoreWideProductDiscountHistory
        public async Task<IPagedList<StoreWideProductDiscountHistory>> SearchProductDiscountLog(int productId, int pageIndex, int pageSize)
        {
            var offerLogQuery = _storeWideProductDiscountHistoryRepository.Table;

            offerLogQuery =
            from o in offerLogQuery
            where o.ProductId == productId

            select o;
            return await offerLogQuery.OrderByDescending(o => o.CreatedOn).ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task InsertStoreWideProductDiscountHistoryAsync(StoreWideProductDiscountHistory storeWideProductDiscountHistory)
        {
            await _storeWideProductDiscountHistoryRepository.InsertAsync(storeWideProductDiscountHistory);
        }

        #endregion


        #endregion
    }
}
