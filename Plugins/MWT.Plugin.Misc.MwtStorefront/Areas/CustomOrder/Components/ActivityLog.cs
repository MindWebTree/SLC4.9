using Microsoft.AspNetCore.Mvc;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Factories;
using Nop.Web.Framework.Components;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Components
{
    public class ActivityLogViewComponent : NopViewComponent
    {

        #region Fields

        private readonly ICustomerActivityModelFactory _customerActivityModelFactory;

        #endregion

        #region Ctor

        public ActivityLogViewComponent(ICustomerActivityModelFactory customerActivityModelFactor)
        {
            _customerActivityModelFactory = customerActivityModelFactor;
        }

        #endregion

        #region Methods

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync(int customerId)
        {
            try
            {
                var activities = await _customerActivityModelFactory.PrepareCustomerActivityListModel(customerId);
                return View(activities);
            }
            catch
            {
                return Content("");
            }
        }

        #endregion
    }
}
