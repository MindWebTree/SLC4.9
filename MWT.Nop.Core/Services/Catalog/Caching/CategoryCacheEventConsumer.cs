using MWT.Nop.Core.Service.Catalog;
using Nop.Core.Domain.Catalog;
using Nop.Services.Caching;

namespace MWT.Nop.Core.Services.Catalog.Caching
{
    public partial class CategoryCacheEventConsumer : CacheEventConsumer<Category>
    {
        /// <summary>
        /// Clear cache data
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <param name="entityEventType">Entity event type</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task ClearCacheAsync(Category entity, EntityEventType entityEventType)
        {
            await RemoveByPrefixAsync(CustomNopCatalogDefaults.KwTermsCategoriesCachePrefix);

        }
    }
}

