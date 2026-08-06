using FluentValidation;
using MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.FAQModule;
using Nop.Data;
using Nop.Services.Localization; 
using Nop.Web.Framework.Validators;

namespace MWT.Nop.Web.Areas.Admin.Validators.FAQModule
{
    public partial class FaqValidator : BaseNopValidator<AddFaqModel>
    {
        public FaqValidator(ILocalizationService localizationService, INopDataProvider dataProvider)
        {
            RuleFor(x => x.Question).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Product.Faq.Fields.Question.Required"));
            RuleFor(x => x.Answer).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Product.Faq.Fields.Answer.Required"));
        }
    }
}