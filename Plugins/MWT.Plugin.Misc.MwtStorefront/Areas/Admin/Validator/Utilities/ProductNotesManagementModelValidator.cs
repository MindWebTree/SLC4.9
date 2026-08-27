
using FluentValidation;
using MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.Utilities;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Web.Areas.Admin.Validators.Customizations.Utilities
{
    public partial class ProductNotesManagementModelValidator : BaseNopValidator<ProductNotesManagementModel>
    {
        public ProductNotesManagementModelValidator(ILocalizationService localizationService, INopDataProvider dataProvider)
        {
            RuleFor(x => x.Notes).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Utilities.Notes.Fields.Notes.Required"));
            RuleFor(x => x.EntityType).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Product.Notes.Fields.EntityType.Required"));
            RuleFor(x => x.ProductIds).NotEmpty().When(x=>x.EntityType=="Product" )
                .WithMessageAwait(localizationService.GetResourceAsync("Admin.Product.Notes.Fields.ProductIds.Required"));
            RuleFor(x => x.CategoryIds).NotEmpty().When(x => x.EntityType == "Category")
                .WithMessageAwait(localizationService.GetResourceAsync("Admin.Product.Notes.Fields.CategoryIds.Required"));
        }
    }
}