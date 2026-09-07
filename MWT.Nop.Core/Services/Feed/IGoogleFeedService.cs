using MWT.Nop.Core.Domain.Feed;

namespace MWT.Nop.Core.Services.Feed
{
    public partial interface IGoogleFeedService
    {
        Task<GoogleCategory> GetFeedCategoryAsync(int categoryId, string productType);
    }
}
