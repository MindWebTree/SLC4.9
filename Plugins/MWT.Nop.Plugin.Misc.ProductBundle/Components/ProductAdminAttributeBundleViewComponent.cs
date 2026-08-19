
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Components;

namespace MWT.Nop.Plugin.Misc.ProductBundle.Components
{
    [ViewComponent(Name = "ProductAdminAttributeBundle")]
    public class ProductAdminAttributeBundle : NopViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(ProductModel model)
        { 
            return View("~/Plugins/MWT.Nop.Plugin.Misc.ProductBundle/Views/_ProductVariantBundle.cshtml", model);
        }
    }
}
