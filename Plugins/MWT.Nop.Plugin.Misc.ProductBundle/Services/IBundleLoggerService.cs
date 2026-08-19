using MWT.Nop.Plugin.Misc.ProductBundle.Domain;
using Nop.Core;
using Nop.Core.Domain.Customization.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Misc.ProductBundle.Services
{
    public interface IBundleLoggerService
    {
        Task LogBundleCreatedAsync(BundleConfiguration bundle);
    //    Task LogBundleCreatedAsync(BundleConfiguration bundle, List<BundleItem> items , int price);
        Task LogBundleUpdatedAsync(BundleConfiguration oldBundle,bool isValid, List<BundleItem> items
            , decimal? oldPrice, decimal? newPrice);
        Task LogBundleDeletedAsync(BundleConfiguration bundle, VariantCombination variant);
      
        Task<IPagedList<BundleAuditLog>> GetAllLogsAsync(
            int productId = 0,
            int variantId = 0,
            string searchKeyword = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue); 
    }
}
