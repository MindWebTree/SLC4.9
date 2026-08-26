using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Services.Customizations.CustomOrders;
using MWT.Nop.Core.Services.Message;
using MWT.Nop.Core.Services.Orders;
using Nop.Core.Infrastructure;
using Nop.Services.Customizations.Phone_Order;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class OrderController : BaseAdminController
    {
      
        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> CustomOrderList(OrderSearchModel searchModel)
        {
            //prepare model
            var model = await _orderModelFactory.PrepareCustomOrderListModelAsync(searchModel);

            return Json(model);
        }


        [HttpPost, ActionName("List")]
        [FormValueRequired("go-to-order-by-guid")]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> GoToOrderGuid(OrderSearchModel model)
        {
           
            var order = await _orderService.GetOrderByGuidAsync(Guid.Parse(model.GoDirectlyToNumberByGuid));

            if (order == null)
                return await List();

            return RedirectToAction("Edit", "Order", new { id = order.Id });
        }



        [HttpPost, ActionName("Edit")]
        [FormValueRequired("SendReceipt")]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> SendReceipt(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return RedirectToAction("List");
            await SendReceipt(order, false);
            var model = await _orderModelFactory.PrepareOrderModelAsync(null, order);
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Order.Receipt.Successfully"));
            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("CustomerSendReceipt")]
        [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> CustomerSendReceiptSendReceipt(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return RedirectToAction("List");

            await SendReceipt(order, true);
            var model = await _orderModelFactory.PrepareOrderModelAsync(null, order);
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Order.Receipt.Successfully"));
            return View(model);
        }


        #region Utilities

        public async Task SendReceipt(Core.Domain.Orders.Order order, bool customerOnly = false)
        {
            var _customOrderService = EngineContext.Current.Resolve<ICustomOrderService>();
            var _customWorkflowMessageService = EngineContext.Current.Resolve<ICustomWorkflowMessageService>();
            var customOrder = await _customOrderService.GetByOrderNumber(order.Id);
            if (customOrder == null)
            {
                await _customWorkflowMessageService
                .CustomSendOrderPlacedCustomerNotificationAsync(order, order.CustomerLanguageId, null, null, customerOnly);
            }
            else
            {
                await _customWorkflowMessageService
                .CustomOrder_SendCustomerNotificationAsync(customOrder, order.CustomerLanguageId, customerOnly);
            }
        }
        #endregion
    }
}
