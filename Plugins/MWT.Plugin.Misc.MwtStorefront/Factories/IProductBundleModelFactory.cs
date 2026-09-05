using MWT.Nop.Core.Domain.ProductBundle;
using MWT.Plugin.Misc.MwtStorefront.Area.Admin.Models.Variant;
using MWTNop.Core.Domain.Catalog;
using Nop.Core;
using Nop.Core.Domain.Catalog;

namespace MWT.Plugin.Misc.MwtStorefront.Factories
{
    public interface IProductBundleModelFactory
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
        Task<BundleConfiguration> GetBundleByVariantIdAsync(int variantId);
        Task<BundleItem> GetBundleItemByProductIdAsync(int bundleId, int productId);
        Task<ProductAttributeCombination> GetBestMatchingCombinationAsync(
 VariantCombination variant);
        Task<List<BundleConfiguration>> GetAffectedBundlesByProductId(int productId);
        Task<VariantBundleSearchListModel> PrepareVariantListModelAsync(VariantBundleSearchModel searchModel, Product product);
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