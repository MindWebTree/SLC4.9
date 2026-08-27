using FluentValidation;
using MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.Utilities;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Web.Areas.Admin.Validators.Customizations.Utilities
{
   public class ImportProductUnpubishedVariantModelValidator : BaseNopValidator<ImportProductUnpubishedVariantModel>
    {
        public ImportProductUnpubishedVariantModelValidator(ILocalizationService localizationService, INopDataProvider dataProvider)
        {
            RuleFor(x => x.ProductId).GreaterThan(0);
          
        }
    }
}
