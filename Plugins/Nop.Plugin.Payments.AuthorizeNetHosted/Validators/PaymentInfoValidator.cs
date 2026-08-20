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
    public class PaymentInfoValidator : BaseNopValidator<PaymentInfoModel>
    {
        #region Ctor

        public PaymentInfoValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.TransactionId)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Payment.TransactionId.Required"));
        }

        #endregion
    }
}
