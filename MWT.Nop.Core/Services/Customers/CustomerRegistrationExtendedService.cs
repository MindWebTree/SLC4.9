using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using MWT.Nop.Core.Services.Message;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Events;
using Nop.Services.Authentication;
using Nop.Services.Authentication.MultiFactor;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Customers
{

    public partial class CustomerRegistrationExtendedService : CustomerRegistrationService, ICustomerRegistrationExtendedService
    {
        public CustomerRegistrationExtendedService(CustomerSettings customerSettings, IActionContextAccessor actionContextAccessor, IAuthenticationService authenticationService, ICustomerActivityService customerActivityService, ICustomerService customerService, IEncryptionService encryptionService, IEventPublisher eventPublisher, IGenericAttributeService genericAttributeService, ILocalizationService localizationService, IMultiFactorAuthenticationPluginManager multiFactorAuthenticationPluginManager, INewsLetterSubscriptionService newsLetterSubscriptionService, INotificationService notificationService, IPermissionService permissionService, IRewardPointService rewardPointService, IShoppingCartService shoppingCartService, IStoreContext storeContext, IUrlHelperFactory urlHelperFactory, IWorkContext workContext, ICustomWorkflowMessageService workflowMessageService, RewardPointsSettings rewardPointsSettings) : base(customerSettings, actionContextAccessor, authenticationService, customerActivityService, customerService, encryptionService, eventPublisher, genericAttributeService, localizationService, multiFactorAuthenticationPluginManager, newsLetterSubscriptionService, notificationService, permissionService, rewardPointService, shoppingCartService, storeContext, urlHelperFactory, workContext, workflowMessageService, rewardPointsSettings)
        {
        }

        public virtual async Task SignInCustomerAsync(Customer customer, bool isPersist = false)
        {
            if ((await _workContext.GetCurrentCustomerAsync())?.Id != customer.Id)
            {
                //migrate shopping cart
                await _shoppingCartService.MigrateShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), customer, true);

                await _workContext.SetCurrentCustomerAsync(customer);
            }

            //sign in new customer
            await _authenticationService.SignInAsync(customer, isPersist);

            //raise event       
            await _eventPublisher.PublishAsync(new CustomerLoggedinEvent(customer));

            //activity log
            await _customerActivityService.InsertActivityAsync(customer, "PublicStore.Login",
                await _localizationService.GetResourceAsync("ActivityLog.PublicStore.Login"), customer);

        }
    }

}
