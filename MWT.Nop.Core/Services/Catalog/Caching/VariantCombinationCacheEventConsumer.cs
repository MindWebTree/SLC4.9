using MWT.Nop.Core.Service.Catalog;
using MWTNop.Core.Domain.Catalog;
using Nop.Services.Caching;
using Nop.Services.Catalog;

namespace MWT.Nop.Core.Services.Catalog.Caching
{
    public partial class VariantCombinationCacheEventConsumer : CacheEventConsumer<VariantCombination>
    {
        protected override async Task ClearCacheAsync(VariantCombination entity, EntityEventType entityEventType)
        {
            await RemoveAsync(CustomNopCatalogDefaults.ProductVariantsCacheKey, entity.ProductId);
            await RemoveAsync(CustomNopCatalogDefaults.ProductVariantsPublishedCacheKey, entity.ProductId);
            await RemoveAsync(CustomNopCatalogDefaults.ProductVariantByVariantIdCacheKey, entity.VariantId);
        }
    }
}
