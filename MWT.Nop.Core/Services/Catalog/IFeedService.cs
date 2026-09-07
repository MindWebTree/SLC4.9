using Nop.Core;
using Nop.Core.Domain.Catalog;

namespace MWT.Nop.Core.Services.Catalog
{
    public partial interface IFeedService
    {
        Task<IPagedList<Product>> ProductsFeedAsync(
int pageIndex = 0,
int pageSize = int.MaxValue,
int storeId = 0);
    }
}
