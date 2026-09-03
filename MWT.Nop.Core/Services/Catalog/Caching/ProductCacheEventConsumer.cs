using MWT.Nop.Core.Service.Catalog;
using Nop.Core.Domain.Catalog;
using Nop.Services.Caching;
using Nop.Services.Catalog;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Catalog.Caching
{
    public partial class ProductCacheEventConsumer : CacheEventConsumer<Product>
    {
        protected override async Task ClearCacheAsync(Product entity, EntityEventType entityEventType)
        {
            await RemoveAsync(CustomNopCatalogDefaults.ProductsBasicInfoCacheKey);
            await RemoveByPrefixAsync(NopCatalogDefaults.RelatedProductsPrefix, entity);
        }
    }
}
