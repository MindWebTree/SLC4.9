using Microsoft.AspNetCore.Mvc;
using MWT.Plugin.Misc.MwtStorefront.Factories;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Components
{
    
    public class CustomKwTermCategoriesBlockViewComponent : NopViewComponent
    {
        private readonly IKwTermModelFactory _kwTermModelFactory;

        public CustomKwTermCategoriesBlockViewComponent(
        IKwTermModelFactory kwTermModelFactory)
        {
            _kwTermModelFactory = kwTermModelFactory;

        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync(int kwTermId)
        {
            return View(await _kwTermModelFactory.PrepareKwTermCategoriesModelAsync(kwTermId));
        }
    }
}
