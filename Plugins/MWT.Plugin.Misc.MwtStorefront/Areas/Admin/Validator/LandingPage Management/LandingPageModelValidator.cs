using FluentValidation;
using MWT.Plugin.Misc.MwtStorefront.Area.Admin.Models.Catalog.LandingPage_Management;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Web.Areas.Admin.Validators.Customizations.Custom.LandingPages
{
    public partial class LandingPageValidator : BaseNopValidator<LandingPageModel>
    {
        public LandingPageValidator(ILocalizationService localizationService, INopDataProvider dataProvider)
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Admin.LandingPage.Fields.Name.Required"));
          
            RuleFor(x => x.PageContent)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Admin.LandingPage.Fields.PageContent.Required"));


        }
    }
}
