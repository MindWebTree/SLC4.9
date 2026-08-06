using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Messages;

namespace MWT.Nop.Core.Services.Catalog
{
   
    public partial interface ICustomBackInStockSubscriptionService: IBackInStockSubscriptionService
    {
        Task<BackInStockSubscription> FindSubscriptionAsync(int customerId, int productId, int storeId, string email);
        Task<int> CustomSendNotificationsToSubscribersAsync(Product product, List<Token> tokens);
    }
}
