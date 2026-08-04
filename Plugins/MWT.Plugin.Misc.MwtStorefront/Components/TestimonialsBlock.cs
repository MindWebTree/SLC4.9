using Microsoft.AspNetCore.Mvc;
using MWT.Plugin.Misc.MwtStorefront.Models;
using Nop.Web.Framework.Components;

namespace MWT.Plugin.Misc.MwtStorefront.Components
{
    public class TestimonialsBlockViewComponent : NopViewComponent
    {


        public TestimonialsBlockViewComponent()
        {

        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            TestimonialModel model = new TestimonialModel();
            return View(model);
        }
    }
}

