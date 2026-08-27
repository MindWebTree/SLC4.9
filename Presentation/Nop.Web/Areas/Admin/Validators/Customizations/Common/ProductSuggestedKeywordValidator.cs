using FluentValidation;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Models.Customization.Custom;
using Nop.Web.Framework.Validators;

namespace Nop.Web.Areas.Admin.Validators.Customizations.Common
{
    public partial class ProductSuggestedKeywordValidator : BaseNopValidator<ProductSuggestedKeywordModel>
    {
        public ProductSuggestedKeywordValidator(ILocalizationService localizationService, INopDataProvider dataProvider)
        {
            RuleFor(x => x.KeyWord).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Category.SuggestedKeyword.Fields.Title.Required"));
        }
    }
}