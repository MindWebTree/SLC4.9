using Nop.Core.Caching;

namespace MWT.Nop.Core.Service.Catalog
{
    /// <summary>
    /// Represents default values related to catalog services
    /// </summary>
    public static class CustomNopCatalogDefaults
    {
        #region Caching defaults


        #region Search
        public static CacheKey SearchDefaultAutoCompleteCacheKey => new CacheKey("Nop.pres.sitemap.DefaultSearch.AutoComplete-{0}-{1}");
        public static string SearchDefaultAutoCompletePrefix => "Nop.pres.sitemap.DefaultSearch.AutoComplete-{0}";
        #endregion

        #region Offer

        public static string StoreWideProductDiscountInfoPrefix => "Nop.Product.StoreWideProductDiscountInfo";
        public static CacheKey StoreWideProductDiscountInfoCacheKey => new CacheKey("Nop.Product.StoreWideProductDiscountInfo.{0}");

        #endregion
        #region Variant


        public static CacheKey ProductVariantsCacheKey => new CacheKey("Nop.product.variants.{0}");
        public static CacheKey ProductVariantsPublishedCacheKey => new CacheKey("Nop.product.variants.Published.{0}");
        public static CacheKey ProductVariantByVariantIdCacheKey => new CacheKey("Nop.product.variant.by.variantid.{0}");

        #endregion
        #endregion
    }
}
