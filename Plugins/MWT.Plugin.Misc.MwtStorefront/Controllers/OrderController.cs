using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Services.Customers;
using MWT.Nop.Core.Services.Customizations.CustomOrders;
using MWT.Nop.Core.Services.Orders;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Factories;
using MWT.Plugin.Misc.MwtStorefront.Factories;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure;
using Nop.Web.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Controllers
{
    public partial class OrderController : BasePublicController
    {
        #region Fields

        private readonly IOrderExtendedModelFactory _orderModelFactory;
        private readonly ICustomerExtendedService _customerService;
        private readonly IWorkContext _workContext;
        private readonly IOrderExtendedService _orderService;


        #endregion

        #region Ctor

        public OrderController(IOrderExtendedModelFactory orderModelFactory,ICustomerExtendedService customerService,IWorkContext workContext,
            IOrderExtendedService orderService)
        {
            _orderModelFactory = orderModelFactory;
            _customerService = customerService;
            _workContext = workContext;
            _orderService = orderService;
        }

        #endregion

        public virtual async Task<IActionResult> CustomOrders(int? pageNumber, OrderHistoryPeriods limit)
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Challenge();

            var model = await _orderModelFactory.CustomPrepareCustomerOrderListModelAsync(pageNumber,limit);
            return View("CustomerOrders", model);
        }
        public virtual async Task<IActionResult> CustomDetails(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null || order.Deleted || (await _workContext.GetCurrentCustomerAsync()).Id != order.CustomerId)
                return Challenge();

            var model = await _orderModelFactory.PrepareCustomOrderDetailsModelAsync(order);
            if (order.IsCustomOrder)
            {
                var _customOrderService = EngineContext.Current.Resolve<ICustomOrderService>();
                var _customOrderModelFactory = EngineContext.Current.Resolve<ICustomOrderModelFactory>();

                var customOrderid = (await _customOrderService.GetByOrderNumber(orderId))?.Id ?? 0;
                model.CustomOrderSummaryModel = await _customOrderModelFactory.PrepareOderSummaryModel(customOrderid);
                return View("CustomOrderDetails", model);
            }
            else
                return View("Details", model);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> CustomPrintOrderDetails(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null || order.Deleted || (await _workContext.GetCurrentCustomerAsync()).Id != order.CustomerId)
                return Challenge();

            var model = await _orderModelFactory.PrepareCustomOrderDetailsModelAsync(order);
            model.PrintMode = true;
            if (order.IsCustomOrder)
            {
                var _customOrderService = EngineContext.Current.Resolve<ICustomOrderService>();
                var _customOrderModelFactory = EngineContext.Current.Resolve<ICustomOrderModelFactory>();

                var customOrderid = (await _customOrderService.GetByOrderNumber(orderId))?.Id ?? 0;
                model.CustomOrderSummaryModel = await _customOrderModelFactory.PrepareOderSummaryModel(customOrderid);

                return View("_CustomOrderDetails", model);
            }
            else
                return View("_Details", model);

        }

        //My account / Order details page / PDF invoice
        [CheckLanguageSeoCode(true)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> CustomGetPdfInvoice(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null || order.Deleted || (await _workContext.GetCurrentCustomerAsync()).Id != order.CustomerId)
                return Challenge();

            List<Order> orders = new List<Order>();
            orders.Add(order);
            byte[] bytes;
            await using (var stream = new MemoryStream())
            {
                await this._orderModelFactory.PrintOrdersToPdfAsync(stream, orders, (await _workContext.GetWorkingLanguageAsync()).Id);
                bytes = stream.ToArray();
            }
            return File(bytes, MimeTypes.ApplicationPdf, $"order_{order.Id}.pdf");
        }

    }
}
