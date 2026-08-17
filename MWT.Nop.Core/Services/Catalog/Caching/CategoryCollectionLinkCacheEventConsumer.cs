using MWT.Nop.Core.Domain;
using MWT.Nop.Core.Service.Catalog;
using Nop.Services.Caching;

namespace MWT.Nop.Core.Services.Catalog.Caching
{
    public partial class CategoryCollectionLinkCacheEventConsumer : CacheEventConsumer<CategoryCollectionLink>
    {
        /// <summary>
        /// entity
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task ClearCacheAsync(CategoryCollectionLink entity)
        {
            await RemoveAsync(CustomNopCatalogDefaults.CategoryCollectionLinkCacheKey, entity.EntityId);
        }
    }
}
