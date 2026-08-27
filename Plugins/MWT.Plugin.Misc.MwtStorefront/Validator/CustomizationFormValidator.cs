using FluentValidation;
using MWT.Plugin.Misc.MwtStorefront.Models.Catalog;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace MWT.Plugin.Misc.MwtStorefront.Validators
{
    public partial class CustomizationFormValidator : BaseNopValidator<CustomizationFormModel>
    {
        public CustomizationFormValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Email).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("CustomizationForm.Fields.Email.Required"));

            RuleFor(x => x.Email).EmailAddress().WithMessageAwait(localizationService.GetResourceAsync("Common.WrongEmail"));

            RuleFor(x => x.FullName).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("CustomizationForm.Fields.FullName.Required"));

            RuleFor(x => x.Phone).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("CustomizationForm.Fields.Phone.Required"));
        }
    }
}