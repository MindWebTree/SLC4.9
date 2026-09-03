using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Payments;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Orders;
using Nop.Web.Framework.Components;
using MWT.Nop.Core.Domain.CustomOrders;
using MWT.Nop.Core.Services.Customizations.CustomOrders;
using MWT.Nop.Core.Services.Customers;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Components
{
    public class CustomOrderStatusLogViewComponent : NopViewComponent
    {
        #region Fields
        
        private readonly ICustomOrderService _customOrderService;
        private readonly ICustomerExtendedService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly IQueuedEmailService _queuedEmailService;

        #endregion

        #region Ctor

        public CustomOrderStatusLogViewComponent(ICustomOrderService customOrderService, ICustomerExtendedService customerService, ILocalizationService localizationService,
             IQueuedEmailService queuedEmailService)
        {
            this._customOrderService = customOrderService;
            this._customerService = customerService;
            this._localizationService = localizationService;
            this._queuedEmailService = queuedEmailService;
        }

        #endregion

        #region Methods
        public async Task<IViewComponentResult> InvokeAsync(int orderId)
        {
            OrderDetailSummarryModel model = new OrderDetailSummarryModel();
            List<CustomOrderStatusLogModel> lstLogsModel = new List<CustomOrderStatusLogModel>();
            var orderStauses = await _customOrderService.GetOrderStatuses();
            var types = await _customOrderService.GetOrderTypes();
            var order = await _customOrderService.GetById(orderId);
           if (order != null)
            {
                string orderTypePrefix = "";
                var orderType = types.Where(t => t.Id == order.OrderTypeId).FirstOrDefault();
                if (orderType != null && orderType.Name != "AlreadyPaid" && !string.IsNullOrEmpty(orderType.Name))
                    orderTypePrefix = orderType.Prefix;
                else
                {
                    orderType = types.Where(t => t.Id == order.SubOrderTypeId).FirstOrDefault();
                    if (orderType != null)
                        orderTypePrefix = orderType.Prefix;
                }

                model.Orderid = orderTypePrefix + orderId;
                model.LiveOrderNumber = order.LiveOrderNumber ?? 0;
                string orderStatus = orderStauses.Where(s => s.Id == order.StatusId).FirstOrDefault()?.Name;
                model.OrderStatus = string.IsNullOrEmpty(orderStatus) ?
                    "" : await _localizationService.GetResourceAsync("customorder.status" + orderStatus);

                var paidStatus = orderStauses.Where(m => m.Name == OrderStatus.Paid.ToString()).FirstOrDefault();
                try
                {
                    Dictionary<int, string> customers = new Dictionary<int, string>();
                    var logs = await _customOrderService.GetOrderStatusLogs(orderId);
                    foreach (var log in logs)
                    {
                        CustomOrderStatusLogModel logModel = new CustomOrderStatusLogModel();
                        logModel.CreatedOn = log.CreatedOn;
                        orderStatus = orderStauses.Where(s => s.Id == log.StatusId).FirstOrDefault()?.Name;
                        logModel.Status = order.OrderTotal <= 0 && paidStatus.Id == log.StatusId ? await _localizationService.GetResourceAsync("customorder.status.Complementory") : string.IsNullOrEmpty(orderStatus) ?
                    string.Empty : await _localizationService.GetResourceAsync("customorder.status" + orderStatus);
                        logModel.Orderid = orderTypePrefix + log.OrderId;
                        logModel.InvoiceSendTo = log.InvoiceSendTo;
                        logModel.Comments = log.Comments;
                        if (log.NotificationId != 0)
                        {
                            var queuedEmail = await _queuedEmailService.GetQueuedEmailByIdAsync(log.NotificationId);
                            logModel.EmailStatus = queuedEmail.SentOnUtc == null ? (queuedEmail.SentTries >= 3 ? await _localizationService.GetResourceAsync("Admin.Email.Status.Failed")
                                : await _localizationService.GetResourceAsync("Admin.Email.Status.Queued")) : await _localizationService.GetResourceAsync("Admin.Email.Status.Sent");
                        }

                        if (log.UserId != 0)
                        {
                            var customerObject = customers.Where(c => c.Key == log.UserId).FirstOrDefault();
                            if (customerObject.Key == null || customerObject.Key == 0)
                            {
                                var customer = await _customerService.GetCustomerByIdAsync(log.UserId);
                                if (customer != null)
                                {
                                    string fullName = await _customerService.GetExtendedCustomerFullNameAsync(customer);
                                    customers.Add(log.UserId, fullName);
                                    logModel.CustomerName = fullName;
                                }

                            }
                            else
                            {
                                logModel.CustomerName = customerObject.Value;
                            }
                        }
                        if (!string.IsNullOrEmpty(log.PaymentResponse))
                        {
                            try
                            {
                                var result = JsonConvert.DeserializeObject<ProcessPaymentResult>(log.PaymentResponse);
                                logModel.TransactionInfo = $"Capture TransactionId:{result.CaptureTransactionId} Authorization TransactionId{result.AuthorizationTransactionId}";

                            }
                            catch
                            {

                            }
                        }

                        lstLogsModel.Add(logModel);
                    }

                }
                catch
                {

                }
                model.LastUpdatedBy = lstLogsModel.FirstOrDefault()?.CustomerName ?? "";
                model.LastUpdatedOn = lstLogsModel.FirstOrDefault()?.CreatedOn ?? null;
                model.Logs = lstLogsModel;
            }
            return View(model);
        }

        #endregion
    }
}
