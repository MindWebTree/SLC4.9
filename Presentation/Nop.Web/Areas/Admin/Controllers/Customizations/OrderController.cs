using Microsoft.AspNetCore.Mvc;
using Nop.Core.Infrastructure;
using Nop.Services.Customizations.Phone_Order;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class OrderController : BaseAdminController
    {
        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> CustomOrderList(OrderSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _orderModelFactory.PrepareCustomOrderListModelAsync(searchModel);

            return Json(model);
        }


        [HttpPost, ActionName("List")]
        [FormValueRequired("go-to-order-by-guid")]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> GoToOrderGuid(OrderSearchModel model)
        {
            var order = await _orderService.GetOrderByGuidAsync(model.GoDirectlyToNumberByGuid);

            if (order == null)
                return await List();

            return RedirectToAction("Edit", "Order", new { id = order.Id });
        }



        [HttpPost, ActionName("Edit")]
        [FormValueRequired("SendReceipt")]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> SendReceipt(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();
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
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> CustomerSendReceiptSendReceipt(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();
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
            var customOrder = await _customOrderService.GetByOrderNumber(order.Id);
            var _workflowMessageService = EngineContext.Current.Resolve<IWorkflowMessageService>();
            if (customOrder == null)
            {
                await _workflowMessageService
                .CustomSendOrderPlacedCustomerNotificationAsync(order, order.CustomerLanguageId,null,null, customerOnly);
            }
            else
            {
                await _workflowMessageService
                .CustomOrder_SendCustomerNotificationAsync(customOrder, order.CustomerLanguageId, customerOnly);
            }
        }
        #endregion
    }
}
