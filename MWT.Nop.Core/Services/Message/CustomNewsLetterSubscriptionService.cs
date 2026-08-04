using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Messages;
using Nop.Core.Events;
using Nop.Data;
using Nop.Services.Customers;
using Nop.Services.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Message
{
    public partial class CustomNewsLetterSubscriptionService : NewsLetterSubscriptionService , ICustomNewsLetterSubscriptionService
    {
        public CustomNewsLetterSubscriptionService(ICustomerService customerService, IEventPublisher eventPublisher, IRepository<Customer> customerRepository, IRepository<CustomerCustomerRoleMapping> customerCustomerRoleMappingRepository, IRepository<NewsLetterSubscription> subscriptionRepository) : base(customerService, eventPublisher, customerRepository, customerCustomerRoleMappingRepository, subscriptionRepository)
        {
        }

        #region Methods
        public async Task<bool> CheckEmailSubscriber(string Email, string ListID = "")
        {
            return await _subscriptionRepository.Table.Where(s => s.Email == Email && s.MailChimpListId == (ListID == "" ? s.MailChimpListId :
            ListID)
            && s.UnsubscribedOn == null).AnyAsync();


        }
        #endregion
    }
}
