using System.Threading.Tasks;
using MWT.Nop.Core.Service.Catalog;
using Nop.Core.Domain.Catalog;
using Nop.Services.Caching;

namespace MWT.Nop.Core.Services.Catalog.Caching
{
    /// <summary>
    /// Represents a specification attribute option cache event consumer
    /// </summary>
    public partial class CustomSpecificationAttributeOptionCacheEventConsumer : CacheEventConsumer<SpecificationAttributeOption>
    {
        /// <summary>
        /// Clear cache data
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <param name="entityEventType">Entity event type</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task ClearCacheAsync(SpecificationAttributeOption entity, EntityEventType entityEventType)
        {
            await RemoveByPrefixAsync(CustomNopCatalogDefaults.AllSpecificationOptionsPrefix);
            await base.ClearCacheAsync(entity, entityEventType);
        }
    }
}
