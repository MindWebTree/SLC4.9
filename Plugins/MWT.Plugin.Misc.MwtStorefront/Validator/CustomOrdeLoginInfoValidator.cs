using FluentValidation;
using MWT.Plugin.Misc.MwtStorefront.Models.CustomOrder;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace MWT.Plugin.Misc.MwtStorefront.Validators
{
    public partial class CustomOrdeLoginInfoValidator : BaseNopValidator<CustomOrderLoginInfo>
    {
        public CustomOrdeLoginInfoValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.EmailAddress).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("CustomizationForm.Fields.Email.Required"));

            RuleFor(x => x.EmailAddress).EmailAddress().WithMessageAwait(localizationService.GetResourceAsync("Common.WrongEmail"));

            RuleFor(x => x.ZipCode).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("CustomizationForm.Fields.ZipCode.Required"));
        }
    }
}