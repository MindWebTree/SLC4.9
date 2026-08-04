using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Services.Custom;
using MWT.Plugin.Misc.MwtStorefront.Models.Custom;
using Nop.Web.Framework.Components;

namespace MWT.Plugin.Misc.MwtStorefront.Components
{
    public class CustomFormViewComponent : NopViewComponent
    {
        private readonly ICustomFormService _customFormService;
        public CustomFormViewComponent(ICustomFormService customFormService)
        {
            this._customFormService = customFormService;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync(string formname, string heading)
        {
            if (string.IsNullOrEmpty(formname.Trim()))
                return Content("");
            var customForm = await _customFormService.GetCustomFormByName(formname);
            if (customForm == null)
                return Content("");
            CustomFormModel model = new CustomFormModel();
            model.Id = customForm.Id;
            model.FormHtml = customForm.FormHtml;
            model.FormName = customForm.FormName;
            model.Heading = heading;
            model.RenderActions = customForm.RenderActions;
            return View(model);
        }
    }
}
