using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Services.Message;
using Nop.Core;
using Nop.Services.Customizations.CustomOrders;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Web.Framework.Components;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Components
{
    public class OrderReceipt : NopViewComponent
    {
        #region Fields

        private readonly IOrderService _orderService;
        private readonly ICustomWorkflowMessageService _workflowMessageService;
        private readonly ICustomOrderService _customOrderService;
        private readonly IWorkContext _workContext;

        #endregion

        public OrderReceipt(IOrderService orderService, ICustomWorkflowMessageService workflowMessageService,
            ICustomOrderService customOrderService, IWorkContext workContext)
        {
            this._orderService = orderService;
            this._workflowMessageService = workflowMessageService;
            this._customOrderService = customOrderService;
            this._workContext = workContext;
        }
        public async Task<IViewComponentResult> InvokeAsync(int orderId = 0, int liveOrderNumber = 0)
        {
            string content = "";
            if (liveOrderNumber != 0 || orderId != 0)
            {
                if (liveOrderNumber == 0)
                {
                    var customOrder = await _customOrderService.GetById(orderId);
                    content = await this._workflowMessageService.CustomOrderInvoiceContentNotificationAsync(customOrder, (await this._workContext.GetWorkingLanguageAsync()).Id);
                    content = "<div data-orderid=\"" + (orderId == 0 ? liveOrderNumber : orderId) + "\">" + content + "</div>";
                }
                else
                {
                    var order = await _orderService.GetOrderByIdAsync(liveOrderNumber);
                    if (order != null)
                    {
                        if (order.IsCustomOrder)
                        {
                            var customOrder = await _customOrderService.GetByOrderNumber(orderId);
                            content = await this._workflowMessageService.CustomOrderReceiptContentAsync(customOrder, (await this._workContext.GetWorkingLanguageAsync()).Id);
                            content = "<div data-orderid=\"" + (orderId == 0 ? liveOrderNumber : orderId) + "\">" + content + "</div>";
                        }
                        else
                        {
                            content = await this._workflowMessageService.OrderReceiptContentAsync(order, (await this._workContext.GetWorkingLanguageAsync()).Id);
                            content = "<div data-orderid=\"" + (orderId == 0 ? liveOrderNumber : orderId) + "\">" + content + "</div>";
                        }

                    }
                }
            }
            return Content(content);
        }
    }
}
