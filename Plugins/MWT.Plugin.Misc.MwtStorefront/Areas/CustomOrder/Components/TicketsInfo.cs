using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Components
{
    public class TicketsInfoViewComponent:NopViewComponent
    {

        #region Ctor

        public TicketsInfoViewComponent()
        {

        }

        #endregion

        #region Methods

        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View();
        }

        #endregion
    }

}
