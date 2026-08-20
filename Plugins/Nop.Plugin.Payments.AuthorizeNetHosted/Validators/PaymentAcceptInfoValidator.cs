using FluentValidation;
using Nop.Plugin.Payments.AuthorizeNetHosted.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.AuthorizeNetHosted.Validators
{
    public class PaymentAcceptInfoValidator : BaseNopValidator<PaymentInfoModel>
    {
        #region Ctor

        public PaymentAcceptInfoValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.AuthorizeNetHostedPaymentDataValue)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Payment.AuthorizenetHostedaymentDataValue.Required"));
            RuleFor(x => x.AuthorizeNetHostedPaymentDataDescriptor)
              .NotEmpty()
              .WithMessageAwait(localizationService.GetResourceAsync("Payment.AuthorizenetHostedPaymentdataDescriptor.Required"));
        }

        #endregion
    }
}
