using System.Threading.Tasks;
using MWT.Nop.Core.Service.Catalog;
using Nop.Core.Domain.Catalog;
using Nop.Services.Caching;

namespace MWT.Nop.Core.Services.Catalog.Caching
{
    /// <summary>
    /// Represents a specification attribute cache event consumer
    /// </summary>
    public partial class CustomSpecificationAttributeCacheEventConsumer : CacheEventConsumer<SpecificationAttribute>
    {
        /// <summary>
        /// Clear cache data
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <param name="entityEventType">Entity event type</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task ClearCacheAsync(SpecificationAttribute entity, EntityEventType entityEventType)
        {
            await RemoveByPrefixAsync(CustomNopCatalogDefaults.SpecificationAttributesByGroupPrefix);
            await base.ClearCacheAsync(entity, entityEventType);
        }
    }
}
