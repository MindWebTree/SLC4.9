using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Customizations.Phone_Order;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Web.Controllers;
using SkiaSharp;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Controllers
{
    public class ReceiptController : BaseCustomOrderController
    {
        #region Fields

        private readonly IOrderService _orderService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly ICustomOrderService _customOrderService;
        private readonly IWorkContext _workContext;

        #endregion
        public ReceiptController(IOrderService orderService, IWorkflowMessageService workflowMessageService,
               ICustomOrderService customOrderService, IWorkContext workContext)
        {
            this._orderService = orderService;
            this._workflowMessageService = workflowMessageService;
            this._customOrderService = customOrderService;
            this._workContext = workContext;
        }
        public async Task<IActionResult> Index(int orderId=0,int liveOrderNumber=0)
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
                            var customOrder = await _customOrderService.GetByOrderNumber(order.Id);
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
