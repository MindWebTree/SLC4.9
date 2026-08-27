
using FluentValidation;
using MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Web.Areas.Admin.Validators.Customizations
{
    public partial class CustomFormValidator : BaseNopValidator<CustomFormModel>
    {
        public CustomFormValidator(ILocalizationService localizationService, INopDataProvider dataProvider)
        {
            RuleFor(x => x.FormName).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Product.CustomForm.Fields.FormName.Required"));
            RuleFor(x => x.FormHtml).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Product.CustomForm.Fields.FormHtml.Required"));
            RuleFor(x => x.Body).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Product.CustomForm.Fields.Body.Required"));
            RuleFor(x => x.Subject).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Product.CustomForm.Fields.Subject.Required"));

        }
    }
}