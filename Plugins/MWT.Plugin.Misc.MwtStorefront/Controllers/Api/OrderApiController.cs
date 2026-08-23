using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Services.Orders;
using MWT.Plugin.Misc.MwtStorefront.Factories;
using MWT.Plugin.Misc.MwtStorefront.Infrastructure.Filters;
using MWT.Plugin.Misc.MwtStorefront.Models.Order;
using Nop.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Controllers.Api
{
    public class OrderApiController : BasePublicController
    {
        #region fields
        private readonly IOrderExtendedModelFactory _orderModelFactory;
        private readonly IOrderExtendedService _orderService;

        #endregion

        #region Ctor
        public OrderApiController(IOrderExtendedModelFactory orderModelFactory, IOrderExtendedService orderService)
        {
            this._orderModelFactory = orderModelFactory;
            _orderService = orderService;
        }

        #endregion

        [ValidateApiKey]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            dynamic obj = new ExpandoObject();
            obj.Orders = await this._orderModelFactory.GetNewOrders();
            return new JsonResult(obj);
        }

        [ValidateApiKey]
        [HttpGet]
        public async Task<IActionResult> GetOrdDetails(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null)
                return NotFound();
            if (order.IsCustomOrder)
                return new JsonResult(await _orderModelFactory.PrepareApiCustomOrder_OrderDetailsModelAsync(order));
            else
                return new JsonResult(await _orderModelFactory.PrepareApiCustomOrderDetailsModelAsync(order));
        }

        [ValidateApiKey]
        [HttpPost]
        public async Task<IActionResult> Update(int orderId, bool isImported)
        {
            await this._orderModelFactory.UpdateStatusOfOrder(orderId, isImported);
            return Ok();
        }
        [ValidateApiKey]
        [HttpPost]
        public async Task<IActionResult> Deliver([FromBody] DeliverOrderRequestModel model)
        {
            await this._orderModelFactory.MarkOrderAsDelivered(model);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetLatestDeliveredOrderIdByCustomerEmail(string email)
        {
            if (email == null)
                return NotFound();
            int orderId = await this._orderModelFactory.GetLatestDeliveredOrderIdByCustomerEmail(email);
            return Ok(new { orderId });
        }

        [ValidateApiKey]
        [HttpGet]
        public async Task<IActionResult> GetShippingMethods(int variantId, decimal total, string zipcode, int countryId)
        {
            return Ok(await _orderModelFactory.GetShippingMethods(variantId, total, zipcode, countryId));
        }

        #region Wgs


        [HttpPost]
        public async Task<IActionResult> SendAdditionalWgsServiceInvoice(int orderNumber, string pairedOrderIds = "")
        {
            return Ok(await _orderModelFactory.ApiSendAdditionalWgsServiceInvoice(orderNumber, pairedOrderIds));
        }

        [HttpGet]
        public async Task<IActionResult> GetStatusOfAdditionalWgsService(int orderNumber)
        {
            return Ok(await _orderModelFactory.ApiGetStatusOfAdditionalWgsService(orderNumber));
        }
        #endregion

    }
}
