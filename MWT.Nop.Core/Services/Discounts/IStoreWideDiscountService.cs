
using MWT.Nop.Core.Domain.StoreWideDiscount;
using Nop.Core;

namespace MWT.Nop.Core.Service.Discounts
{
    public partial interface IStoreWideDiscountService
    {
        #region StoreWideDiscount 
        
 #region StoreWideDiscount 
        Task<StoreWideDiscount> GetStoreWideDiscountByIdAsync(int storeWideDiscountId);
        Task DeleteStoreWideDiscountAsync(StoreWideDiscount  storeWideDiscount);
        Task InsertStoreWideDiscountAsync(StoreWideDiscount storeWideDiscount);
        Task UpdateStoreWideDiscountAsync(StoreWideDiscount storeWideDiscount);
        Task<IPagedList<StoreWideDiscount>> GetAllStoreWideDiscountAsync(int pageIndex, int pageSize);
        Task<bool> IsStoreWideDiscountExistInDateRange(DateTime startDate, DateTime endDate, int storeDiscountId);
        Task<List<StoreWideDiscount>> GetStoreWideDiscountByDate(DateTime date);

        Task<StoreWideDiscount> GetProductSaleInfo(int productId);
        #endregion

        #region StoreWideDiscountSetting
        Task<StoreWideDiscountSetting> GetStoreWideDiscountSettingByIdAsync(int storeWideDiscountSettingId);
        Task DeleteStoreWideDiscountSettingAsync(StoreWideDiscountSetting storeWideDiscountSetting);
        Task InsertStoreWideDiscountSettingAsync(StoreWideDiscountSetting storeWideDiscountSetting);
        Task UpdateStoreWideDiscountSettingAsync(StoreWideDiscountSetting storeWideDiscountSetting);
        Task<IPagedList<StoreWideDiscountSetting>> GetAllStoreWideDiscountSettingAsync(int storeWideDiscountId, int pageIndex, int pageSize, bool showHidden = false);
        Task<bool> ApplyDiscount(StoreWideDiscountSetting discount);
        #endregion

        #region StoreWideProductDiscountInfo
        Task InsertStoreWideProductDiscountInfoAsync(StoreWideProductDiscountInfo storeWideProductDiscountInfo);
        Task UpdateStoreWideProductDiscountInfoAsync(StoreWideProductDiscountInfo storeWideProductDiscountInfo);
        Task<StoreWideProductDiscountInfo> GetStoreWideProductDiscountInfoByProductIdAsync(int productId);
        Task<IPagedList<StoreWideProductDiscountInfo>> SearchStoreWideProductDiscountInfo(int categoryId, List<int> productIds, int pageIndex, int pageSize);
        Task<List<StoreWideProductDiscountInfo>> GetStoreWideProductDiscountInfoHaveDiscountAppliedAsync();

        #endregion

        #region  StoreWideProductDiscountHistory
        Task<IPagedList<StoreWideProductDiscountHistory>> SearchProductDiscountLog(int productId, int pageIndex, int pageSize);
        Task InsertStoreWideProductDiscountHistoryAsync(StoreWideProductDiscountHistory storeWideProductDiscountHistory);
        #endregion

        #endregion
    }
}
