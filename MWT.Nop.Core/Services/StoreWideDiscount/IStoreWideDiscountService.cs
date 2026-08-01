using MWT.Nop.Core.Domain.StoreWideDiscount;
using Nop.Core; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Service.StoreWideDiscount
{
    public partial interface IStoreWideDiscountService
    {
        #region StoreWideDiscount 
        
        Task<StoreWideProductDiscountInfo> GetStoreWideProductDiscountInfoByProductIdAsync(int productId);
        Task<MWT.Nop.Core.Domain.StoreWideDiscount.StoreWideDiscount> GetProductSaleInfo(int productId);

        #endregion
    }
}
