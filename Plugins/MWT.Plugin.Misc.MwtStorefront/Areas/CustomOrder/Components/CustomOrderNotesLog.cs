using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Services.Customers;
using MWT.Nop.Core.Services.Customizations.CustomOrders;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Orders;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Web.Framework.Components;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Components
{
    public class CustomOrderNotesLogViewComponent : NopViewComponent
    {
        #region Fields
        private readonly ICustomOrderService _customOrderService;
        private readonly ICustomerExtendedService _customerService;
        private readonly IAddressService _addressService;
        private readonly ILocalizationService _localizationService;
        #endregion

        #region Ctor

        public CustomOrderNotesLogViewComponent(ICustomOrderService customOrderService, ICustomerExtendedService customerService,
            IAddressService addressService, ILocalizationService localizationService)
        {
            this._customOrderService = customOrderService;
            this._customerService = customerService;
            this._addressService = addressService;
            this._localizationService = localizationService;
        }

        #endregion


        #region Methods

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync(int orderId)
        {
            List<CustomOrderNotesLogModel> lstLogsModel = new List<CustomOrderNotesLogModel>();
            var order = await _customOrderService.GetById(orderId);
            if (order != null)
            {
                try
                {
                    Dictionary<int, string> customers = new Dictionary<int, string>();
                    var logs = await _customOrderService.GetOrderNotesLogs(orderId);
                    foreach (var log in logs)
                    {
                        CustomOrderNotesLogModel logModel = new CustomOrderNotesLogModel();
                        logModel.CreatedOn = log.CreatedOn;
                        logModel.Notes = log.SpecialInstructions;

                        if (log.UserId != 0)
                        {
                            var customerObject = customers.Where(c => c.Key == log.UserId).FirstOrDefault();
                            if (customerObject.Key == null || customerObject.Key == 0)
                            {
                                var customer = await _customerService.GetCustomerByIdAsync(log.UserId);
                                if (customer != null)
                                {
                                    string fullName= await _customerService.GetExtendedCustomerFullNameAsync(customer);
                                    customers.Add(log.UserId, fullName);
                                    logModel.CustomerName = fullName;
                                }

                            }
                            else
                            {
                                logModel.CustomerName = customerObject.Value;
                            }
                        }
                        lstLogsModel.Add(logModel);
                    }

                }
                catch
                {

                }
            }
            return View(lstLogsModel);
        }

        #endregion
    }
}
