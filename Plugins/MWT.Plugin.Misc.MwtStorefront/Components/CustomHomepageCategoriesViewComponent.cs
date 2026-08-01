using Microsoft.AspNetCore.Mvc;
using MWT.Plugin.Misc.MwtStorefront.Factories.Catalog;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace MWT.Plugin.Misc.MwtStorefront.Components
{
    public class CustomHomepageCategoriesViewComponent : NopViewComponent
    {
        private readonly ICustomCatalogModelFactory _catalogModelFactory;

        public CustomHomepageCategoriesViewComponent(ICustomCatalogModelFactory catalogModelFactory)
        {
            _catalogModelFactory = catalogModelFactory;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await _catalogModelFactory.CustomPrepareHomepageCategoryModelsAsync();
            if (!model.Any())
                return Content("");

            return View(model);
        }
    }
}
