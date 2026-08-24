using System.Linq;
using FluentValidation;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Common;
using MWT.Plugin.Misc.MwtStorefront.Models.Common;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.Validators.Common
{
    public partial class AddressValidator : BaseNopValidator<AddressModel>
    {
        public AddressValidator(ILocalizationService localizationService,
            IStateProvinceService stateProvinceService,
            AddressSettings addressSettings,
            CustomerSettings customerSettings)
        {


            if (addressSettings.CountryEnabled)
            {
                RuleFor(x => x.CountryId)
                    .NotNull()
                    .WithMessageAwait(localizationService.GetResourceAsync("Address.Fields.Country.Required"));
                RuleFor(x => x.CountryId)
                    .NotEqual(0)
                    .WithMessageAwait(localizationService.GetResourceAsync("Address.Fields.Country.Required"));
            }
            if (addressSettings.CountryEnabled && addressSettings.StateProvinceEnabled)
            {
                RuleFor(x => x.StateProvinceId).MustAwait(async (x, context) =>
                {
                    //does selected country has states?
                    var countryId = x.CountryId ?? 0;
                    var hasStates = (await stateProvinceService.GetStateProvincesByCountryIdAsync(countryId)).Any();

                    if (hasStates)
                    {
                        //if yes, then ensure that state is selected
                        if (!x.StateProvinceId.HasValue || x.StateProvinceId.Value == 0)
                            return false;
                    }

                    return true;
                }).WithMessageAwait(localizationService.GetResourceAsync("Address.Fields.StateProvince.Required"));
            }

            if (addressSettings.StreetAddressRequired && addressSettings.StreetAddressEnabled)
            {
                RuleFor(x => x.Address1).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.StreetAddress.Required"));
            }
            if (addressSettings.StreetAddress2Required && addressSettings.StreetAddress2Enabled)
            {
                RuleFor(x => x.Address2).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.StreetAddress2.Required"));
            }
            if (addressSettings.ZipPostalCodeRequired && addressSettings.ZipPostalCodeEnabled)
            {
                RuleFor(x => x.ZipPostalCode).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.ZipPostalCode.Required"));
            }
            if (addressSettings.CityRequired && addressSettings.CityEnabled)
            {
                RuleFor(x => x.City).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.City.Required"));
            }
            if (addressSettings.PhoneRequired && addressSettings.PhoneEnabled)
            {
                RuleFor(x => x.PhoneNumber).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.Phone.Required"));
            }
            if (addressSettings.PhoneEnabled)
            {
                RuleFor(x => x.PhoneNumber).IsPhoneNumber(customerSettings).WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.Phone.NotValid"));
            }

        }
    }
}