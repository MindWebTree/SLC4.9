using FluentValidation;
using MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.Campaign_Management;
using Nop.Data;
using Nop.Services.Localization; 
using Nop.Web.Framework.Validators;

namespace MWT.Nop.Web.Areas.Admin.Validators.Campaign_Management
{
   

public partial class DesignModelValidator : BaseNopValidator<DesignModel>
{
    public DesignModelValidator(ILocalizationService localizationService, INopDataProvider dataProvider)
    {
        RuleFor(x => x.Name).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("CampaignManagement.Template.Fields.Name.Required"));
        RuleFor(x => x.HtmlContent).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("CampaignManagement.Template.Fields.HtmlContent.Required"));

    }
}
}