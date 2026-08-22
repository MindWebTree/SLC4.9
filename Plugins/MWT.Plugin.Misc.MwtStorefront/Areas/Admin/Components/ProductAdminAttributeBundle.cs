
using Microsoft.AspNetCore.Mvc;
using MWT.Plugin.Misc.MwtStorefront.Models.Catalog;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Models;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Components
{
    [ViewComponent(Name = "ProductAdminAttributeBundle")]
    public class ProductAdminAttributeBundleViewComponent : NopViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (additionalData is not ProductModel productDetailsModel)
                return Content("");
            return View((ProductModel)additionalData);
        }
    }
}
