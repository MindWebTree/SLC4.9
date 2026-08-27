using FluentValidation;
using MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.Utilities;
using MWT.Plugin.Misc.MwtStorefront.Infrastructure.Extensions;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Web.Areas.Admin.Validators.Customizations.Utilities
{
    public class MarketingModelValidator : BaseNopValidator<MarketingModel>
    {
        public MarketingModelValidator(ILocalizationService localizationService, INopDataProvider dataProvider)
        {
            RuleFor(x => x.BuyMoreSaveMoreContent).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Utilities.Marketing.Field.BuyMoreSaveMoreContent.Required"));
            RuleFor(x => x.BuyMoreSaveMoreDiscountConfiguration).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Utilities.Marketing.Field.BuyMoreSaveMoreDiscountConfiguration.Required"));
            RuleFor(x => x.HeaderStripContent).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Utilities.Marketing.Field.HeaderStripContent.Required"));
            RuleFor(x => x.LimitedOfferContent).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Utilities.Marketing.Field.LimitedOfferContent.Required"));
            RuleFor(x => x.BuyMoreSaveMoreDiscountConfiguration).ValidatetBuyMoreSaveMore();
        }
    }
}
