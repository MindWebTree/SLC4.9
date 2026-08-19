using MWT.Nop.Plugin.Misc.ProductBundle.Domain;
using MWT.Nop.Plugin.Misc.ProductBundle.Models;
using MWT.Nop.Plugin.Misc.ProductBundle.Models.Variant;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customization.Catalog;
using Nop.Web.Areas.Admin.Models.Catalog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Misc.ProductBundle.Services
{
    public interface IBundleService
    {
        Task<IList<BundleConfiguration>> GetBundlesByProductIdAsync(int productId);
        Task<IList<BundleConfiguration>> GetAllBundlesByProductIdAsync(int productId);
        Task<IPagedList<Product>> GetBundleProductsAsync(int pageIndex, int pageSize);
        Task<BundleConfiguration> GetBundleByIdAsync(int bundleId);
        Task InsertBundleAsync(BundleConfiguration bundle);

        Task UpdateBundleAsync(BundleConfiguration bundle);

        Task DeleteBundleAsync(BundleConfiguration bundle);
        Task DisableBundle(int variantId);

        Task<IList<BundleItem>> GetBundleItemsAsync(int bundleId);
        Task<BundleItem> GetBundleItemsexistAsync(int productId, int bundleId, int variantId);

        Task InsertBundleItemAsync(BundleItem item);

        Task DeleteBundleItemAsync(BundleItem item);
        Task UpdateBundleItemAsync(BundleItem bundle, bool publishEvent = true);

        Task<BundleItem> GetBundleItemByIdAsync(int Id);
        Task<List<BundleConfiguration>> GetActiveBundleByProductIdAsync(int productId);
        Task UpdateBundleVariantIdsAsync(int variantId, bool isBundleEnabled);
        Task<bool> IsBundleValid(BundleConfiguration bundle);
        Task CalculateBundlePrice(Product product);
        Task ValidateAndRestoreIncompleteBundlesAsync();
        #region Variant
        Task<VariantBundleSearchListModel> PrepareVariantListModelAsync(VariantBundleSearchModel searchModel, Product product);
        Task<BundleConfiguration> GetBundleByVariantIdAsync(int variantId);
        Task<BundleItem> GetBundleItemByProductIdAsync(int bundleId, int productId);
        Task<ProductAttributeCombination> GetBestMatchingCombinationAsync(
 VariantCombination variant);
        Task<List<BundleConfiguration>> GetAffectedBundlesByProductId(int productId);
        #endregion
        #region Variantbackup
        Task<VariantPriceBackup> GetVariantPriceBackupAsync(int variantId);
        Task DeleteVariantPriceBackupAsync(int variantId);
        Task RestoreVariantPriceFromBackupAsync(int variantId);
        Task ValidateAndRestoreBundleVariantAsync(BundleConfiguration bundle);
        Task CalculateSingleBundlePrice(BundleConfiguration bundle);
        #endregion


    }
}