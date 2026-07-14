using FluentValidation;
using Nop.Plugin.Misc.Redirect.Models.Redirections;
using Nop.Web.Framework.Validators;
 

namespace MWT.Plugin.Misc.Redirect.Validators
{
    public class RedirectionModelValidator : BaseNopValidator<RedirectionModel>
    {
        public RedirectionModelValidator()
        {
            RuleFor(x => x.Pattern)
                .NotEmpty().WithMessage("The pattern path/URL is required.");

            RuleFor(x => x.RedirectUrl)
                .NotEmpty().WithMessage("The destination redirect URL is required.");

            RuleFor(x => x.Type)
                .IsInEnum().WithMessage("Please select a valid redirection configuration type.");
        }
    }
}
