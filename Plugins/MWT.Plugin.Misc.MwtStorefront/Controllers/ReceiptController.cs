using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Services.Customizations.CustomOrders;
using MWT.Nop.Core.Services.Message;
using MWT.Plugin.Misc.MwtStorefront.Models.Common;
using Nop.Core;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Controllers
{
    public class ReceiptController : BasePublicController
    {
        #region Fields

        private readonly IOrderService _orderService;
        private readonly ICustomWorkflowMessageService _workflowMessageService;
        private readonly ICustomOrderService _customOrderService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public ReceiptController(IOrderService orderService, ICustomWorkflowMessageService workflowMessageService,
            ICustomOrderService customOrderService, IWorkContext workContext)
        {
            this._orderService = orderService;
            this._workflowMessageService = workflowMessageService;
            this._customOrderService = customOrderService;
            this._workContext = workContext;
        }

        #endregion
        public async Task<IActionResult> Index(int orderId)
        {

            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null || order.Deleted)
                return Content("");
            else
            {
                ReceiptModel model = new ReceiptModel();
                string content = "";

                if (order.IsCustomOrder)
                {
                    var customOrder = await _customOrderService.GetByOrderNumber(order.Id);
                    model.Content = await this._workflowMessageService.CustomOrderReceiptContentAsync(customOrder, (await this._workContext.GetWorkingLanguageAsync()).Id);
                }
                else
                    model.Content = await this._workflowMessageService.OrderReceiptContentAsync(order, (await this._workContext.GetWorkingLanguageAsync()).Id);

                return View(model);

            }
        }
    }
}
