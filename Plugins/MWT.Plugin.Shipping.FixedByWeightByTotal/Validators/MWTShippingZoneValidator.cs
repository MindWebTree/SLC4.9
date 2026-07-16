using FluentValidation;
using MWT.Plugin.Shipping.FixedByWeightByTotal.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Shipping.FixedByWeightByTotal.Validators
{
    public partial class MWTShippingZoneValidator : BaseNopValidator<MWTShippingZoneModel>
    {
        public MWTShippingZoneValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("MWT.Plugins.Shipping.FixedByWeightByTotal.Name.Required"));
            RuleFor(x => x.ZipCodes).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("MWT.Plugins.Shipping.FixedByWeightByTotal.ZipCodes.Required"));
        }
    }
}
