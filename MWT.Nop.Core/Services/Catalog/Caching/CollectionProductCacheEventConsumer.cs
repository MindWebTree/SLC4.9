
using System.Threading.Tasks;
using MWT.Nop.Core.Domain;
using MWT.Nop.Core.Service.Catalog;
using Nop.Core.Domain.Catalog;
using Nop.Services.Caching;

namespace MWT.Nop.Core.Services.Catalog.Caching
{
    /// <summary>
    /// Represents a related product cache event consumer
    /// </summary>
    public partial class CollectionProductCacheEventConsumer : CacheEventConsumer<CollectionProduct>
    {
        /// <summary>
        /// entity
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task ClearCacheAsync(CollectionProduct entity)
        {
            await RemoveByPrefixAsync(CustomNopCatalogDefaults.CollectionProductsPrefix, entity.ProductId1);
        }
    }
}
