using Microsoft.AspNetCore.Mvc;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Factories;
using Nop.Web.Framework.Components;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Components
{
    public class CustomOrder_OrderSummary : NopViewComponent
    {

        #region Fields

        private readonly ICustomOrderModelFactory _customOrderModelFactory;

        #endregion

        #region Ctor

        public CustomOrder_OrderSummary(ICustomOrderModelFactory customOrderModelFactory)
        {
            this._customOrderModelFactory = customOrderModelFactory;
        }

        #endregion

        #region Methods

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync(int orderId)
        {
            try
            {
                var section = await _customOrderModelFactory.PrepareOderSummaryModel(orderId);
                if(section==null)
                    return Content("");
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
