using MWT.Nop.Core.Domain.KW;
using MWT.Nop.Core.Service.Catalog;
using Nop.Services.Caching;

namespace MWT.Nop.Core.Services.Catalog.Caching
{
    public partial class CategoryKwTermCacheEventConsumer : CacheEventConsumer<CategoryKwTerm>
    {
        /// <summary>
        /// Clear cache data
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <param name="entityEventType">Entity event type</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task ClearCacheAsync(CategoryKwTerm entity, EntityEventType entityEventType)
        {
            await RemoveByPrefixAsync(CustomNopCatalogDefaults.KwTermsCategoriesCachePrefix);
            await base.ClearCacheAsync(entity, entityEventType);
        }
    }
}
