using MWT.Nop.Core.Domain;
using MWT.Nop.Core.Service.Catalog;
using Nop.Core.Domain.Catalog;
using Nop.Services.Caching;
using System;
using System.Threading.Tasks;


namespace MWT.Nop.Core.Services.Catalog.Caching
{
    /// <summary>
    /// Represents a related product cache event consumer
    /// </summary>
    public partial class FBTProductCacheEventConsumer : CacheEventConsumer<FBTProduct>
    {
        /// <summary>
        /// entity
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task ClearCacheAsync(FBTProduct entity)
        {
            await RemoveByPrefixAsync(CustomNopCatalogDefaults.FBTProductsPrefix, entity.ProductId1);
        }
    }
}
