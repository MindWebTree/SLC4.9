using FluentValidation;
using MWT.Plugin.Misc.MwtStorefront.Models.RewardClaim;
using Nop.Core.Domain.Common;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators; 
namespace MWT.Plugin.Misc.MwtStorefront.Validators.RewardClaim
{
    public partial class RewardClaimRequestValidator : BaseNopValidator<RewardClaimRequestModel>
    {
        public RewardClaimRequestValidator(ILocalizationService localizationService, CommonSettings commonSettings)
        {
            RuleFor(x => x.EmailAddress).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("RewardClaim.Email.Required"));
            RuleFor(x => x.EmailAddress).EmailAddress().WithMessageAwait(localizationService.GetResourceAsync("RewardClaim.WrongEmail"));
            RuleFor(x => x.OrderId).GreaterThan(0).WithMessageAwait(localizationService.GetResourceAsync("RewardClaim.Invalid.OrderId"));
            RuleFor(x => x).Must(x => x.IsGoogleReview || x.IsPreferVideo || x.IsInstagramReview).WithMessageAwait(localizationService.GetResourceAsync("RewardClaim.Review.Required"));
            RuleFor(x => x.SelectedGift)
                .NotNull()
                .WithMessageAwait(localizationService.GetResourceAsync("RewardClaim.Gift.Required"));
        }
    }
}
