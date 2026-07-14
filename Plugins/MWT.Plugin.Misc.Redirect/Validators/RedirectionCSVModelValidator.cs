using FluentValidation; 
using Nop.Plugin.Misc.Redirect.Models.Redirections;
using Nop.Web.Framework.Validators;

namespace MWT.Plugin.Misc.Redirect.Validators
{
    public class RedirectionCSVModelValidator : BaseNopValidator<RedirectionCSVModel>
    {
        public RedirectionCSVModelValidator()
        {
            RuleFor(x => x.Pattern)
                .NotEmpty().WithMessage("The CSV import pattern field cannot be empty.");

            RuleFor(x => x.RedirectUrl)
                .NotEmpty().WithMessage("The CSV import target destination field cannot be empty.");

            RuleFor(x => x.Type)
                .IsInEnum().WithMessage("The provided item type in the file is invalid.");
        }
    }
}