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
        public partial class MWTShippingByWeightByTotalValidator : BaseNopValidator<MWTShippingByWeightByTotalModel>
        {
            public MWTShippingByWeightByTotalValidator(ILocalizationService localizationService)
            {
                RuleFor(x => x.ZoneId).NotEqual(0).WithMessageAwait(localizationService.GetResourceAsync("MWT.Plugins.Shipping.FixedByWeightByTotal.Name.ZoneId"));
            }
        }
    }
}
