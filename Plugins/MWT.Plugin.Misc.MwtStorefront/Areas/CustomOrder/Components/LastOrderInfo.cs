using Microsoft.AspNetCore.Mvc;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Factories;
using Nop.Web.Framework.Components;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Components
{
    public class CustomerLastOrderViewComponent : NopViewComponent
    {
        #region Fields

        private readonly ICustomOrderModelFactory _customOrderModelFactory;

        #endregion

        #region Ctor

        public CustomerLastOrderViewComponent(ICustomOrderModelFactory customOrderModelFactory)
        {
            this._customOrderModelFactory = customOrderModelFactory;
        }

        #endregion

        #region Methods

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync(int customerId)
        {
            return View(await this._customOrderModelFactory.PrepareCustomerStatsModel(customerId));
        }
        #endregion
    }

}

