
using Microsoft.AspNetCore.Mvc;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Factories;
using Nop.Web.Framework.Components;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Components
{
    public class CustomOrder_CustomerSection : NopViewComponent
    {

        #region Fields

        private readonly ICustomOrderModelFactory _customOrderModelFactory;

        #endregion

        #region Ctor

        public CustomOrder_CustomerSection(ICustomOrderModelFactory customOrderModelFactory)
        {
            this._customOrderModelFactory = customOrderModelFactory;
        }

        #endregion

        #region Methods

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync(int customerId)
        {
            try
            {
                var section = await _customOrderModelFactory.PrepareCustomerSection(0, customerId,true);
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
