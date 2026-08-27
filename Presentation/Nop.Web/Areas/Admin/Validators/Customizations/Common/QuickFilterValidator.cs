using FluentValidation;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Validators;

namespace Nop.Web.Areas.Admin.Validators.Customizations.Common
{
    public partial class QuickFilterValidator : BaseNopValidator<QuickFilterModel>
    {
        public QuickFilterValidator(ILocalizationService localizationService, INopDataProvider dataProvider)
        {
            RuleFor(x => x.TermName).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Product.RelatedSearch.Fields.TermName.Required"));
            RuleFor(x => x.Link).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Product.RelatedSearch.Fields.Link.Required"));
         
        }
    }
}