using Nop.Services.Catalog;

namespace MWT.Nop.Core.Services.Catalog
{
    public partial interface ICustomRecentlyViewedProductsService:IRecentlyViewedProductsService
    {
        Task CustomAddProductToRecentlyViewedListAsync(int productId);
    }
}
