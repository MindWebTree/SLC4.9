using System.Threading.Tasks;
using MWT.Nop.Core.Domain.QA;
using MWT.Nop.Core.Service.Catalog;
using Nop.Core.Domain.Catalog; 
using Nop.Services.Caching;
using Nop.Services.Catalog;

namespace MWT.Nop.Core.Services.Seo.Caching
{
    /// <summary>
    /// Represents a related product cache event consumer
    /// </summary>
    public partial class RelatedQuestionAnswerCacheEventConsumer : CacheEventConsumer<RelatedQuestionAnswer>
    {
        /// <summary>
        /// entity
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task ClearCacheAsync(RelatedQuestionAnswer entity)
        {
            await RemoveByPrefixAsync(CustomNopCatalogDefaults.RelatedQuestionAnswersPrefix, entity.QuestionAnswerId1);
        }
    }
}
