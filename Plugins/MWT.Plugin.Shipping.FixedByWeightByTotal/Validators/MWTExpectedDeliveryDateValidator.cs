
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

    namespace MWT.Plugin.Shipping.FixedByWeightByTotal.Validators
    {
        public partial class MWTExpectedDeliveryDateValidator : BaseNopValidator<MWTExpectedDeliveryDateModel>
        {
            public MWTExpectedDeliveryDateValidator(ILocalizationService localizationService)
            {
                RuleFor(x => x.ZoneID).GreaterThan(0).WithMessageAwait(localizationService.GetResourceAsync("MWT.Plugins.Shipping.FixedByWeightByTotal.MWTExpectedDeliveryDate.ZoneId.Required"));
                RuleFor(x => x.ExpectedMinNoOfDays).GreaterThan(0).WithMessageAwait(localizationService.GetResourceAsync("MWT.Plugins.Shipping.FixedByWeightByTotal.MWTExpectedDeliveryDate.ExpectedMinNoOfDays.Required"));
                RuleFor(x => x.ExpectedMinNoOfDays).GreaterThan(0).WithMessageAwait(localizationService.GetResourceAsync("MWT.Plugins.Shipping.FixedByWeightByTotal.MWTExpectedDeliveryDate.ExpectedMinNoOfDays.Required"));
            }
        }
    }
}
