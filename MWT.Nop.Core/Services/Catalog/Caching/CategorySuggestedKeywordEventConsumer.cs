using MWT.Nop.Core.Domain;
using Nop.Services.Caching;

namespace MWT.Nop.Core.Services.Catalog.Caching
{
    public partial class CategorySuggestedKeywordEventConsumer : CacheEventConsumer<CategorySuggestedKeyword>
    {
        protected override async Task ClearCacheAsync(CategorySuggestedKeyword entity, EntityEventType entityEventType)
        {
            await base.ClearCacheAsync(entity, entityEventType);
        }
    }
}
