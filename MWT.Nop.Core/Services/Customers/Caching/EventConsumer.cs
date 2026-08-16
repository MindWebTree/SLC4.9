using Nop.Core.Domain.Customers;
using Nop.Core.Events;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Customers.Caching
{
    public class EventConsumer :
         IConsumer<EntityInsertedEvent<Customer>>

    {
        #region Fields

        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;

        #endregion

        #region Ctor

        public EventConsumer(ICustomerActivityService customerActivityService,
                            ILocalizationService localizationService,
                            ILanguageService languageService)
        {
            this._customerActivityService = customerActivityService;
            this._localizationService = localizationService;
            this._languageService = languageService;
        }

        #endregion

        #region Methods

        public async Task HandleEventAsync(EntityInsertedEvent<Customer> eventMessage)
        {
            var languageId = (await this._languageService.GetAllLanguagesAsync()).OrderBy(o => o.DisplayOrder).FirstOrDefault()?.Id;
            if (languageId != null)
            {
                var customerInsertActivityFormat = await _localizationService.GetResourceAsync("CustomOrder.Activity.Customer.Register", Convert.ToInt32(languageId));
                await this._customerActivityService.InsertActivityAsync(eventMessage.Entity, "CustomerRelatedActivity", String.Format(customerInsertActivityFormat, eventMessage.Entity.CreatedOnUtc.ToString("dd MMM yyyy")), null);
            }
        }

        #endregion
    }
}
