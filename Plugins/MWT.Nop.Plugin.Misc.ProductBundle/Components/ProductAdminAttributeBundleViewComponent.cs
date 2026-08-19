
using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Plugin.Misc.ProductBundle.Models;
using MWT.Nop.Plugin.Misc.ProductBundle.Services;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
