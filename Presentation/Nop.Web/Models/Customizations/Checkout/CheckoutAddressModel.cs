using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Models.Common;
using Nop.Web.Models.Customer;


namespace Nop.Web.Models.Checkout;

public partial record CheckoutAddressModel : AddressModel
{
    public CheckoutAddressModel()
    {
        AvailableCountries = new List<SelectListItem>();
        AvailableStates = new List<SelectListItem>();
        CustomAddressAttributes = new List<AddressAttributeModel>();
        GdprConsents = new List<GdprConsentModel>();
    }

    public string TimeZoneId { get; set; }

    public bool DisplayVatNumber { get; set; }
    public string VatNumber { get; set; }
    public bool GenderEnabled { get; set; }
    [NopResourceDisplayName("Account.Fields.Gender")]
    public string Gender { get; set; }

    public bool DateOfBirthEnabled { get; set; }
    [NopResourceDisplayName("Account.Fields.DateOfBirth")]

    public int? DateOfBirthDay { get; set; }

    [NopResourceDisplayName("Account.Fields.DateOfBirth")]
    public int? DateOfBirthMonth { get; set; }

    [NopResourceDisplayName("Account.Fields.DateOfBirth")]
    public int? DateOfBirthYear { get; set; }
    public bool DateOfBirthRequired { get; set; }


    public bool Newsletter { get; set; }
    public bool SignatureEnabled { get; set; }
    [NopResourceDisplayName("Account.Fields.Signature")]
    public string Signature { get; set; }


    public bool CheckUsernameAvailabilityEnabled { get; set; }
    public bool AllowUsersToChangeUsernames { get; set; }
    public bool UsernamesEnabled { get; set; }
    [NopResourceDisplayName("Account.Fields.Username")]
    public string Username { get; set; }

    public string Fax { get; set; }
    public bool FaxEnabled { get; set; }
    public DateTime? ParseDateOfBirth()
    {
        if (!DateOfBirthYear.HasValue || !DateOfBirthMonth.HasValue || !DateOfBirthDay.HasValue)
            return null;

        DateTime? dateOfBirth = null;
        try
        {
            dateOfBirth = new DateTime(DateOfBirthYear.Value, DateOfBirthMonth.Value, DateOfBirthDay.Value);
        }
        catch { }
        return dateOfBirth;
    }

    public IList<GdprConsentModel> GdprConsents { get; set; }


    public bool EuVatEnabled { get; set; }
    public bool EuVatEnabledForGuests { get; set; }
}
