using MWT.Nop.Core.Services.Message;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Catalog
{
    public partial class CustomBackInStockSubscriptionService : BackInStockSubscriptionService, ICustomBackInStockSubscriptionService
    {
        #region Fields
        private readonly ICustomWorkflowMessageService _customWorkflowMessageService;

        #endregion
        public CustomBackInStockSubscriptionService(IRepository<BackInStockSubscription> backInStockSubscriptionRepository, IRepository<Customer> customerRepository, IRepository<Product> productRepository, IWorkflowMessageService workflowMessageService,
            ICustomWorkflowMessageService customWorkflowMessageService) : base(backInStockSubscriptionRepository, customerRepository, productRepository, workflowMessageService)
        {
            _customWorkflowMessageService = customWorkflowMessageService;
        }

        #region Methods
        public virtual async Task<BackInStockSubscription> FindSubscriptionAsync(int customerId, int productId, int storeId, string email)
        {
            var query = from biss in _backInStockSubscriptionRepository.Table
                        orderby biss.CreatedOnUtc descending
                        where biss.CustomerId == customerId &&
                              biss.ProductId == productId &&
                              biss.StoreId == storeId &&
                              biss.Email == email
                        select biss;

            var subscription = await query.FirstOrDefaultAsync();

            return subscription;
        }


        public virtual async Task<int> CustomSendNotificationsToSubscribersAsync(Product product, List<Token> tokens)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var result = 0;
            var subscriptions = await GetAllSubscriptionsByProductIdAsync(product.Id);

            foreach (var subscription in subscriptions)
            {
                var customer = await _customerRepository.GetByIdAsync(subscription.CustomerId);
                result += (await _customWorkflowMessageService.CustomSendBackInStockNotificationAsync(subscription, customer?.LanguageId ?? 0, tokens)).Count;
            }

            for (var i = 0; i <= subscriptions.Count - 1; i++)
                await DeleteSubscriptionAsync(subscriptions[i]);

            return result;
        }
        #endregion
    }
}
