using MWT.Nop.Core.Domain.KW;
using MWT.Nop.Core.Service.Catalog;
using Nop.Services.Caching;
using Nop.Services.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Catalog.Caching
{
    public partial class KwTermCacheEventConsumer : CacheEventConsumer<KwTerm>
    {
        /// <summary>
        /// Clear cache data
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <param name="entityEventType">Entity event type</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task ClearCacheAsync(KwTerm entity, EntityEventType entityEventType)
        {

            await RemoveByPrefixAsync(CustomNopCatalogDefaults.KwTermsBreadcrumbPrefix);
            await RemoveByPrefixAsync(CustomNopCatalogDefaults.KwTermsCategoriesCachePrefix);
            await base.ClearCacheAsync(entity, entityEventType);
        }
    }
}
