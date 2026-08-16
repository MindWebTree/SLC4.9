using MWT.Nop.Core.Domain;
using MWT.Nop.Core.Service.Catalog;
using Nop.Services.Caching;
using Nop.Services.Catalog;

namespace MWT.Nop.Core.Services.Catalog.Caching
{
    public partial class QuickFilterCacheEventConsumer : CacheEventConsumer<QuickFilter>
    {
        /// <summary>
        /// entity
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task ClearCacheAsync(QuickFilter entity)
        {
            await RemoveByPrefixAsync(CustomNopCatalogDefaults.QuickFilterPrefix, entity.EntityId);
        }
    }
}
