using FluentValidation;
using MWT.Nop.Plugin.Widgets.Catalog.Domain;
using MWT.Nop.Plugin.Widgets.Catalog.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Widgets.Catalog.Validator
{
    public partial class MWTEntityBannerValidator : BaseNopValidator<MWTEntityBannerModel>
    {
        public MWTEntityBannerValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.EntityType)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("MWT.Plugins.Widgets.Catalog.Fields.EntityType.Required"));

            RuleFor(x => x.WidgetZone)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("MWT.Plugins.Widgets.Catalog.Fields.WidgetZone.Required"));

            SetDatabaseValidationRules<MWTEntityBanner>();
        }
    }
}
