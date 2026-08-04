using FluentValidation;
using Nop.Core.Domain.Customers;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using MWT.Plugin.Misc.MwtStorefront.Models;

namespace MWT.Plugin.Misc.MwtStorefront.Validators
{
    public partial class TestimonialModelValidator : BaseNopValidator<TestimonialModel>
    {
        public TestimonialModelValidator(ILocalizationService localizationService, CustomerSettings customerSettings)
        {
            //login by email
            RuleFor(x => x.Email).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Testimonial .Fields.Email.Required"));
            RuleFor(x => x.Email).EmailAddress().WithMessageAwait(localizationService.GetResourceAsync("Common.WrongEmail"));
            RuleFor(x => x.OrderId).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Testimonial.Fields.OrderId.Required"));
            RuleFor(x => x.IsConsent).Equal(true).WithMessageAwait(localizationService.GetResourceAsync("Testimonial.Fields.IsConsent.Required"));
            RuleFor(x => x)
          .Must(x => (x.Video != null) || !string.IsNullOrWhiteSpace(x.VideoUrl))
          .WithMessageAwait(localizationService.GetResourceAsync("Testimonial.Fields.VideoOrVideoUrl.Required"));
        }
    }
}
