
using Microsoft.AspNetCore.Mvc;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Factories;
using Nop.Web.Framework.Components;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Components
{
    public class CustomOrder_AddressSectionViewComponent : NopViewComponent
    {

        #region Fields

        private readonly ICustomOrderModelFactory _customOrderModelFactory;

        #endregion

        #region Ctor

        public CustomOrder_AddressSectionViewComponent(ICustomOrderModelFactory customOrderModelFactory)
        {
            this._customOrderModelFactory = customOrderModelFactory;
        }

        #endregion

        #region Methods

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync(int orderId,int customerId)
        {
            try
            {
                var section = await _customOrderModelFactory.PrepareCustomerSection(orderId, customerId, customerId>0?true:false);
                return View(section);
            }
            catch
            {
                return Content("");
            }
        }

        #endregion
    }
}
