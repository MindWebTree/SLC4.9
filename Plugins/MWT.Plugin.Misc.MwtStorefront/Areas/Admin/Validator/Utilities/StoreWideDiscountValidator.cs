using FluentValidation;
using MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.Utilities;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Web.Areas.Admin.Validators.Customizations.Utilities
{
    public class StoreWideDiscountValidator:BaseNopValidator<StoreWideDiscountModel>
    {
        public StoreWideDiscountValidator(ILocalizationService localizationService, INopDataProvider dataProvider)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Utilities.Marketing.Offer.Field.Name.Required"));
            RuleFor(x => x.StartDate).NotNull();
            RuleFor(x => x.EndDate).NotNull();
            RuleFor(x => x.StartDate).LessThan(x=>x.EndDate).WithMessageAwait(localizationService.GetResourceAsync("Admin.Utilities.Marketing.Offer.Field.StartDate.Less.Then.EndDate"));
            RuleFor(x => x.StartDate).GreaterThan(new DateTime()).WithMessageAwait(localizationService.GetResourceAsync("Admin.Utilities.Marketing.Offer.Field.StartDate.Greater.Then.CurrentDateTime"));
            RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate).WithMessageAwait(localizationService.GetResourceAsync("Admin.Utilities.Marketing.Offer.Field.EndDate.Greater.Then.StartDate"));
        }
    }
}
