using FluentValidation;
using MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.Utilities;
using MWT.Plugin.Misc.MwtStorefront.Infrastructure.Extensions;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Web.Areas.Admin.Validators.Customizations.Utilities
{
    public class ProductMappingModelValidator : BaseNopValidator<ProductMappingModel>
    {
        public ProductMappingModelValidator(ILocalizationService localizationService, INopDataProvider dataProvider)
        {
            RuleFor(x => x.Action).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Utilities.ProductMapping.Field.Action.Required"));
            RuleFor(x => x.ProductIds).IsValidCommaSplitNumbers();
            RuleFor(x => x.CategoryIds).IsValidCommaSplitNumbers();
            RuleFor(x => x.SpecificationAttributeIds).IsValidCommaSplitNumbers();

            RuleFor(x => x.ProductIds).NotEmpty().When(x => x.Action != "Category Products Publish/Unpublish" && x.Action!= "Product Template Mapping" && x.Action != "Customization Form Template").WithMessageAwait(localizationService.GetResourceAsync("Admin.Utilities.ProductMapping.Field.ProductIds.Required"));


            RuleFor(x => x.Inventory).NotNull().When(x => x.Action == "Product Quantity Update").WithMessageAwait(localizationService.GetResourceAsync("Admin.Utilities.ProductMapping.Field.Inventory.Required"));

            RuleFor(x => x.CategoryIds).NotEmpty().When(x => string.IsNullOrEmpty(x.SpecificationAttributeIds))
                .When(x => x.Action == "Product Category Department Mapping").WithMessageAwait(localizationService.GetResourceAsync("Admin.Utilities.ProductMapping.Field.SpecificationAttributeIds.CategoryIds.Required"));

            RuleFor(x => x.SpecificationAttributeIds).NotEmpty().When(x => string.IsNullOrEmpty(x.CategoryIds))
           .When(x => x.Action == "Product Category Department Mapping").WithMessageAwait(localizationService.GetResourceAsync("Admin.Utilities.ProductMapping.Field.SpecificationAttributeIds.CategoryIds.Required"));


            RuleFor(x => x.ProductIds).NotEmpty().When(x => string.IsNullOrEmpty(x.CategoryIds))
                .When(x => x.Action == "Category Products Publish/Unpublish" || x.Action == "Customization Form Template").WithMessageAwait(localizationService.GetResourceAsync("Admin.Utilities.ProductMapping.Field.ProductIds.CategoryIds.Required"));


            RuleFor(x => x.CategoryIds).NotEmpty().When(x => string.IsNullOrEmpty(x.ProductIds))
          .When(x => x.Action == "Category Products Publish/Unpublish" || x.Action == "Customization Form Template").WithMessageAwait(localizationService.GetResourceAsync("Admin.Utilities.ProductMapping.Field.ProductIds.CategoryIds.Required"));

            RuleFor(x => x.ProductIds).NotEmpty()
         .When(x => x.Action == "Product Listing Section").WithMessageAwait(localizationService.GetResourceAsync("Admin.Utilities.ProductMapping.Field.ProductIds.Required"));

            RuleFor(x => x.ProductId).GreaterThan(0)
        .When(x => x.Action == "Product Listing Section").WithMessageAwait(localizationService.GetResourceAsync("Admin.Utilities.ProductMapping.Field.ProductId.GreaterThanZero"));

            RuleFor(x => x.ProductId).NotEmpty()
        .When(x => x.Action == "Product Listing Section").WithMessageAwait(localizationService.GetResourceAsync("Admin.Utilities.ProductMapping.Field.ProductId.Required"));


            RuleFor(x => x.CategoryIds).NotEmpty()
               .When(x => x.Action == "Product Template Mapping").WithMessageAwait(localizationService.GetResourceAsync("Admin.Utilities.ProductMapping.Field.CategoryIds.Required"));




        }
    }
}
