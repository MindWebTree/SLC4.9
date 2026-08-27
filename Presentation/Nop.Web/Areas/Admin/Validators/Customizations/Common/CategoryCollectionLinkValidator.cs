using FluentValidation;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Validators;

namespace Nop.Web.Areas.Admin.Validators.Customizations.Common
{
    public partial class CategoryCollectionLinkValidator : BaseNopValidator<CategoryCollectionLinkModel>
    {
        public CategoryCollectionLinkValidator(ILocalizationService localizationService, INopDataProvider dataProvider)
        {
            RuleFor(x => x.Title).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Product.CategoryCollectionLink.Fields.Title.Required"));
            RuleFor(x => x.Link).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Product.CategoryCollectionLink.Fields.Link.Required"));

        }
    }
}