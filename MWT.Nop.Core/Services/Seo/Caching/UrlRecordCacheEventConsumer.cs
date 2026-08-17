using Nop.Core.Domain.Seo;
using Nop.Services.Caching;

namespace MWT.Nop.Core.Services.Seo.Caching
{
    /// <summary>
    /// Represents a related product cache event consumer
    /// </summary>
    public partial class UrlRecordCacheEventConsumer : CacheEventConsumer<UrlRecord>
    {
        /// <summary>
        /// Clear cache data
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task ClearCacheAsync(UrlRecord entity)
        {
            await RemoveAsync(NopSeoExtendedDefaults.UrlRecordByTermSlugCacheKey, entity.Slug);
            await RemoveAsync(NopSeoExtendedDefaults.UrlRecordByQuestionAnswerSlugCacheKey, entity.Slug);
        }
    }
}
