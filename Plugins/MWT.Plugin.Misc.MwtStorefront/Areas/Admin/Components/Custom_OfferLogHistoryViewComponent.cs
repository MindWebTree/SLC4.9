using Microsoft.AspNetCore.Mvc;
using MWT.Plugin.Misc.MwtStorefront.Area.Admin.Factories.Utilities;
using MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.Utilities;
using Nop.Web.Framework.Components;

namespace Nop.Web.Components
{
    public class Custom_OfferLogHistoryViewComponent : NopViewComponent
    {
        private readonly IUtilitiesModelFactory _utilitiesModelFactory;

        public Custom_OfferLogHistoryViewComponent(
            IUtilitiesModelFactory utilitiesModelFactory)
        {
            _utilitiesModelFactory = utilitiesModelFactory;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View(await _utilitiesModelFactory.PrepareOfferDiscountLogSearchModel(new StoreWideProductDiscountInfoSearchModel()));
        }
    }
}