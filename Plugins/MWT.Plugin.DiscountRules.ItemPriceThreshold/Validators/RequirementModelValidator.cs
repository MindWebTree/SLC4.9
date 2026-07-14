using FluentValidation;
using Nop.Plugin.DiscountRules.ItemsBelowPrice.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.DiscountRules.ItemsBelowPrice.Validators
{
    /// <summary>
    /// Represents an <see cref="RequirementModel"/> validator.
    /// </summary>
    public class RequirementModelValidator : BaseNopValidator<RequirementModel>
    {
        public RequirementModelValidator(ILocalizationService localizationService)
        {
            RuleFor(model => model.DiscountId)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugins.DiscountRules.ItemsBelowPrice.Fields.DiscountId.Required"));

            RuleFor(model => model.Amount)
                .GreaterThan(0)
                .WithMessageAwait(localizationService.GetResourceAsync("Plugins.DiscountRules.ItemsBelowPrice.Fields.Amount.Required"));
        }
    }
}
