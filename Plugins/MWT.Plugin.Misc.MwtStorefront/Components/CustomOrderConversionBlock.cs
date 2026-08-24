using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Services.Customizations.CustomOrders;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Orders;
using MWT.Plugin.Misc.MwtStorefront.Factories;
using Nop.Services.Orders;
using Nop.Web.Framework.Components;

namespace MWT.Plugin.Misc.MwtStorefront.Components
{
    public class CustomOrderConversionBlockViewComponent : NopViewComponent
    {

        #region Fields

        private readonly ICustomOrderService _customOrderService;
        private readonly IOrderService _orderService;
        private IOrderExtendedModelFactory _orderModelFactory;

        #endregion

        #region Ctor

        public CustomOrderConversionBlockViewComponent(ICustomOrderService customOrderService,
            IOrderService orderService, IOrderExtendedModelFactory orderModelFactory)
        {
            this._customOrderService = customOrderService;
            this._orderService = orderService;
            this._orderModelFactory = orderModelFactory;
        }

        #endregion

        #region Methods

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync(int id, List<ItemModel> Items)
        {
            try
            {
                var customOrder = await _customOrderService.GetById(id);
                if (customOrder == null || (customOrder.LiveOrderNumber ?? 0) == 0)
                {
                    return Content("");
                }

                var order = await _orderService.GetOrderByIdAsync(Convert.ToInt32(customOrder.LiveOrderNumber));
                if (order == null || order.OrderConfirmed)
                {
                    return Content("");
                }

                var conversionModel = await _orderModelFactory.PrepareCustomOrderConversionModel(order);
                conversionModel.Items = Items;
                conversionModel.order = order;
                order.OrderConfirmed = true;
                await _orderService.UpdateOrderAsync(order);

                return View(conversionModel);
            }
            catch (Exception exp)
            {
                return Content("");
            }
        }

        #endregion
    }
}
