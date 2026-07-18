using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Plugin.Payments.Affirm.Models;
using MWT.Nop.Plugin.Payments.Affirm.Services;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Payments;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Payments.Affirm.Components
{
    public class AffirmPaymentInfoViewComponent : NopViewComponent
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPaymentService _paymentService;
        private readonly OrderSettings _orderSettings;
        private readonly IWorkContext _workContext;
        private readonly ServiceManager _serviceManager;
        private readonly IAddressService _addresService;
        private readonly AffirmCheckoutSettings _affirmCheckoutSettings;
        #endregion

        #region Ctor

        public AffirmPaymentInfoViewComponent(ILocalizationService localizationService,
            INotificationService notificationService,
            IPaymentService paymentService,
            OrderSettings orderSettings,
            ServiceManager serviceManager,
            IWorkContext workContext,
            IAddressService addresService,
            AffirmCheckoutSettings affirmCheckoutSettings)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _paymentService = paymentService;
            _orderSettings = orderSettings;
            _serviceManager = serviceManager;
            _workContext = workContext;
            _addresService = addresService;
            _affirmCheckoutSettings= affirmCheckoutSettings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Invoke view component
        /// </summary>
        /// <param name="widgetZone">Widget zone name</param>
        /// <param name="additionalData">Additional data</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the view component result
        /// </returns>
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {

          
            var model = new PaymentInfoModel();
            //prepare order GUID
            var paymentRequest = new ProcessPaymentRequest();
          //  _paymentService.GenerateOrderGuid(paymentRequest);
            //try to create an order
            model = await _serviceManager.CheckoutInit(paymentRequest.OrderGuid);


            return View("~/Plugins/MWT.Nop.Plugin.Payments.Affirm/Views/PaymentInfo.cshtml", model);
        }

        #endregion
    }
}
