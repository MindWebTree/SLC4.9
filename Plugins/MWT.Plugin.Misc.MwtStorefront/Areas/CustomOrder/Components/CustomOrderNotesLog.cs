using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Customers;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Orders;
using Nop.Web.Framework.Components;
using Nop.Services.Customizations.CustomOrders;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Components
{
    public class CustomOrderNotesLogViewComponent : NopViewComponent
    {
        #region Fields
        private readonly ICustomOrderService _customOrderService;
        private readonly ICustomerService _customerService;
        private readonly IAddressService _addressService;
        private readonly ILocalizationService _localizationService;
        #endregion

        #region Ctor

        public CustomOrderNotesLogViewComponent(ICustomOrderService customOrderService, ICustomerService customerService,
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
            string firstName = "";
            string lastName = "";
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
                                    firstName = customer.FirstName;
                                    lastName = customer.LastName;
                                    customers.Add(log.UserId, firstName ?? "" + " " + lastName ?? "");
                                    logModel.CustomerName = firstName ?? "" + " " + lastName ?? "";
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
