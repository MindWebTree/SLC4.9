
using MWT.Nop.Core.Services.Message;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Stores;
using Nop.Core.Events;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Customizations.Orders.Consumer
{
    public class EventConsumer :
         IConsumer<EntityInsertedEvent<Order>>

    {
        #region Fields

        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomWorkflowMessageService _workflowMessageService;
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public EventConsumer(ICustomerActivityService customerActivityService,
                            ILocalizationService localizationService, ICustomWorkflowMessageService workflowMessageService,
                            ICustomerService customerService, IWorkContext workContext)
        {
            this._customerActivityService = customerActivityService;
            this._localizationService = localizationService;
            this._workflowMessageService = workflowMessageService;
            this._customerService = customerService;
            this._workContext = workContext;
        }

        #endregion

        #region Methods

        public async Task HandleEventAsync(EntityInsertedEvent<Order> eventMessage)
        {
            var customerInsertActivityFormat = await _localizationService.GetResourceAsync("CustomOrder.Activity.Order.Purchased");
            await this._customerActivityService.InsertActivityAsync("CustomerRelatedActivity", String.Format(customerInsertActivityFormat, eventMessage.Entity.Id, eventMessage.Entity.CreatedOnUtc.ToString("dd MMM yyyy")), null);
            if (eventMessage.Entity.PaymentStatus == Core.Domain.Payments.PaymentStatus.Pending)
            {
                if (!eventMessage.Entity.PaymentMethodSystemName.Contains("CashOnDelivery", StringComparison.InvariantCultureIgnoreCase))
                {
                    var customer = await _customerService.GetCustomerByIdAsync(eventMessage.Entity.CustomerId);
                    await _workflowMessageService.SendSupportPendingOrderEmailMessage(customer, (await this._workContext.GetWorkingLanguageAsync()).Id, eventMessage.Entity.CaptureTransactionId ?? string.Empty,
                        eventMessage.Entity.Id.ToString(), eventMessage.Entity.IsCustomOrder, eventMessage.Entity.Id, eventMessage.Entity.PaymentMethodSystemName);
                }
            }

        }

        #endregion
    }
}
