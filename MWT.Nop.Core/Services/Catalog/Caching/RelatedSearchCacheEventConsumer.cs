using MWT.Nop.Core.Domain;
using MWT.Nop.Core.Service.Catalog;
using Nop.Services.Caching;

namespace MWT.Nop.Core.Services.Catalog.Caching
{
    /// <summary>
    /// Represents a related product cache event consumer
    /// </summary>
    public partial class RelatedSearchCacheEventConsumer : CacheEventConsumer<RelatedSearch>
    {
        /// <summary>
        /// entity
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task ClearCacheAsync(RelatedSearch entity)
        {
            await RemoveByPrefixAsync(CustomNopCatalogDefaults.ReleatedSearchPrefix, entity.EntityId);
        }
    }
}
