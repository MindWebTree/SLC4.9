using FluentValidation;
using MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.Utilities;
using MWT.Plugin.Misc.MwtStorefront.Infrastructure.Extensions;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Web.Areas.Admin.Validators.Customizations.Utilities
{
    public class StoreWideDiscountSettingModelValidator : BaseNopValidator<StoreWideDiscountSettingModel>
    {
        public StoreWideDiscountSettingModelValidator(ILocalizationService localizationService, INopDataProvider dataProvider)
        {
            RuleFor(x => x.ProductIds).IsValidCommaSplitNumbers();
            RuleFor(x => x.Discount).GreaterThan(-1).WithMessageAwait(localizationService.GetResourceAsync("Admin.Utilities.Marketing.Offer.Field.DiscountPercentage.Required"));
            RuleFor(x => x.SelectedCategoryIds).NotEmpty().When(x => !x.FullStore && string.IsNullOrEmpty(x.ProductIds)).WithMessageAwait(localizationService.GetResourceAsync("Admin.Utilities.Marketing.Offer.Field.CategoryIds.Required"));

            RuleFor(x => x.ProductIds).NotEmpty().When(x => !x.FullStore && x.SelectedCategoryIds.Count == 0).WithMessageAwait(localizationService.GetResourceAsync("Admin.Utilities.Marketing.Offer.Field.ProductIds.Required"));

            RuleFor(x => x.InfoHelpText).NotEmpty().When(x => x.Discount > 0).WithMessageAwait(localizationService.GetResourceAsync("Admin.Utilities.Marketing.Offer.Field.OfferHelpText.Required"));
            RuleFor(x => x.InfoText).NotEmpty().When(x => x.Discount > 0).WithMessageAwait(localizationService.GetResourceAsync("Admin.Utilities.Marketing.Offer.Field.OfferText.Required"));
        }
    }
}
