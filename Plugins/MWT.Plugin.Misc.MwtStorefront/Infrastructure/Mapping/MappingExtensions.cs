using MWT.Plugin.Misc.MwtStorefront.Models.Common;
using MWT.Plugin.Misc.MwtStorefront.Models.Customer;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Models.Checkout;
using Nop.Web.Models.Common;
using Nop.Web.Models.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Infrastructure.Mapping
{
    public static class MappingExtensions
    {
        //public static AddressModel CheckoutAddressModelToAddressModel(this CheckoutAddressModel model, bool trimFields = true)
        //{
        //    if (model == null)
        //        return null;
        //    var destination = new AddressModel();


        //    if (trimFields)
        //    {
        //        if (model.FirstName != null)
        //            model.FirstName = model.FirstName.Trim();
        //        if (model.LastName != null)
        //            model.LastName = model.LastName.Trim();
        //        if (model.Email != null)
        //            model.Email = model.Email.Trim();
        //        if (model.Company != null)
        //            model.Company = model.Company.Trim();
        //        if (model.County != null)
        //            model.County = model.County.Trim();
        //        if (model.City != null)
        //            model.City = model.City.Trim();
        //        if (model.Address1 != null)
        //            model.Address1 = model.Address1.Trim();
        //        if (model.Address2 != null)
        //            model.Address2 = model.Address2.Trim();
        //        if (model.ZipPostalCode != null)
        //            model.ZipPostalCode = model.ZipPostalCode.Trim();
        //        if (model.PhoneNumber != null)
        //            model.PhoneNumber = model.PhoneNumber.Trim();
        //        if (model.FaxNumber != null)
        //            model.FaxNumber = model.FaxNumber.Trim();
        //    }
        //    destination.Id = model.Id;
        //    destination.FirstName = model.FirstName;
        //    destination.LastName = model.LastName;
        //    destination.Email = model.Email;
        //    destination.Company = model.Company;
        //    destination.CountryId = model.CountryId == 0 ? null : model.CountryId;
        //    destination.StateProvinceId = model.StateProvinceId == 0 ? null : model.StateProvinceId;
        //    destination.County = model.County;
        //    destination.City = model.City;
        //    destination.Address1 = model.Address1;
        //    destination.Address2 = model.Address2;
        //    destination.ZipPostalCode = model.ZipPostalCode;
        //    destination.PhoneNumber = model.PhoneNumber;
        //    destination.FaxNumber = model.FaxNumber;

        //    return destination;
        //}

        //public static CheckoutAddressModel AddressModelToCheckoutAddressModel(this AddressModel model, bool trimFields = true)
        //{
        //    if (model == null)
        //        return null;
        //    var destination = new CheckoutAddressModel();


        //    if (trimFields)
        //    {
        //        if (model.FirstName != null)
        //            model.FirstName = model.FirstName.Trim();
        //        if (model.LastName != null)
        //            model.LastName = model.LastName.Trim();
        //        if (model.Email != null)
        //            model.Email = model.Email.Trim();
        //        if (model.Company != null)
        //            model.Company = model.Company.Trim();
        //        if (model.County != null)
        //            model.County = model.County.Trim();
        //        if (model.City != null)
        //            model.City = model.City.Trim();
        //        if (model.Address1 != null)
        //            model.Address1 = model.Address1.Trim();
        //        if (model.Address2 != null)
        //            model.Address2 = model.Address2.Trim();
        //        if (model.ZipPostalCode != null)
        //            model.ZipPostalCode = model.ZipPostalCode.Trim();
        //        if (model.PhoneNumber != null)
        //            model.PhoneNumber = model.PhoneNumber.Trim();
        //        if (model.FaxNumber != null)
        //            model.FaxNumber = model.FaxNumber.Trim();
        //    }
        //    destination.Id = model.Id;
        //    destination.FirstName = model.FirstName;
        //    destination.LastName = model.LastName;
        //    destination.FullName = model.FullName;
        //    destination.Email = model.Email;
        //    destination.CompanyEnabled = model.CompanyEnabled;
        //    destination.Company = model.Company;
        //    destination.CountryEnabled = model.CountryEnabled;
        //    destination.CountryId = model.CountryId == 0 ? null : model.CountryId;
        //    destination.CountryName = model.CountryName;
        //    destination.StateProvinceEnabled = model.StateProvinceEnabled;
        //    destination.StateProvinceId = model.StateProvinceId == 0 ? null : model.StateProvinceId;
        //    destination.StateProvinceName = model.StateProvinceName;
        //    destination.CountyEnabled = model.CountyEnabled;
        //    destination.CountyRequired = model.CountyRequired;
        //    destination.County = model.County;
        //    destination.CityEnabled = model.CityEnabled;
        //    destination.CityRequired = model.CityRequired;
        //    destination.City = model.City;
        //    destination.StreetAddressEnabled = model.StreetAddressEnabled;
        //    destination.StreetAddressRequired = model.StreetAddressRequired;
        //    destination.Address1 = model.Address1;
        //    destination.StreetAddress2Enabled = model.StreetAddress2Enabled;
        //    destination.StreetAddress2Required = model.StreetAddress2Required;
        //    destination.Address2 = model.Address2;
        //    destination.ZipPostalCodeEnabled = model.ZipPostalCodeEnabled;
        //    destination.ZipPostalCodeRequired = model.ZipPostalCodeRequired;
        //    destination.ZipPostalCode = model.ZipPostalCode;
        //    destination.PhoneEnabled = model.PhoneEnabled;
        //    destination.PhoneRequired = model.PhoneRequired;
        //    destination.PhoneNumber = model.PhoneNumber;
        //    destination.FaxEnabled = model.FaxEnabled;
        //    destination.FaxRequired = model.FaxRequired;
        //    destination.FaxNumber = model.FaxNumber;
        //    destination.AvailableCountries = model.AvailableCountries;
        //    destination.AvailableStates = model.AvailableStates;
        //    destination.FormattedCustomAddressAttributes = model.FormattedCustomAddressAttributes;
        //    destination.CustomAddressAttributes = model.CustomAddressAttributes;
        //    destination.CountryTwoLetterSeoCode = model.CountryTwoLetterSeoCode;
        //    destination.StateAbbreviation = model.StateAbbreviation;
        //    return destination;
        //}

        public static CustomerInfoModel CustomerInfoExtendedModelToCustomerInfoModel(this CustomerInfoExtendedModel model, bool trimFields = true)
        {
            if (model == null)
                return null;
            var destination = new CustomerInfoModel();


            if (trimFields)
            {
                if (model.FirstName != null)
                    model.FirstName = model.FirstName.Trim();
                if (model.LastName != null)
                    model.LastName = model.LastName.Trim();
                if (model.Email != null)
                    model.Email = model.Email.Trim();
                if (model.Company != null)
                    model.Company = model.Company.Trim();
                if (model.County != null)
                    model.County = model.County.Trim();
                if (model.City != null)
                    model.City = model.City.Trim();
                if (model.StreetAddress != null)
                    model.StreetAddress = model.StreetAddress.Trim();
                if (model.StreetAddress2 != null)
                    model.StreetAddress2 = model.StreetAddress2.Trim();
                if (model.ZipPostalCode != null)
                    model.ZipPostalCode = model.ZipPostalCode.Trim();
                if (model.Phone != null)
                    model.Phone = model.Phone.Trim();
                if (model.Fax != null)
                    model.Fax = model.Fax.Trim();
            }
            destination.Email = model.Email;
            destination.EmailToRevalidate = model.EmailToRevalidate;
            destination.CheckUsernameAvailabilityEnabled = model.CheckUsernameAvailabilityEnabled;
            destination.AllowUsersToChangeUsernames = model.AllowUsersToChangeUsernames;
            destination.UsernamesEnabled = model.UsernamesEnabled;
            destination.Username = model.Username;
            destination.NeutralGenderEnabled = model.NeutralGenderEnabled;
            destination.VatNumberRequired = model.VatNumberRequired;
            destination.GenderEnabled = model.GenderEnabled;
            destination.Gender = model.Gender;
            destination.City = model.City;
            destination.FirstNameEnabled = model.FirstNameEnabled;
            destination.FirstName = model.FirstName;
            destination.FirstNameRequired = model.FirstNameRequired;
            destination.LastNameEnabled = model.LastNameEnabled;
            destination.LastName = model.LastName;
            destination.LastNameRequired = model.LastNameRequired;
            destination.DateOfBirthEnabled = model.DateOfBirthEnabled;
            destination.DateOfBirthDay = model.DateOfBirthDay;
            destination.DateOfBirthMonth = model.DateOfBirthMonth;
            destination.DateOfBirthYear = model.DateOfBirthYear;
            destination.DateOfBirthRequired = model.DateOfBirthRequired;
            destination.CompanyEnabled = model.CompanyEnabled;
            destination.CompanyRequired = model.CompanyRequired;
            destination.Company = model.Company;
            destination.StreetAddressEnabled = model.StreetAddressEnabled;
            destination.StreetAddressRequired = model.StreetAddressRequired;
            destination.StreetAddress = model.StreetAddress;
            destination.StreetAddress2Enabled = model.StreetAddress2Enabled;
            destination.StreetAddress2Required = model.StreetAddress2Required;
            destination.StreetAddress2 = model.StreetAddress2;
            destination.ZipPostalCodeEnabled = model.ZipPostalCodeEnabled;
            destination.ZipPostalCodeRequired = model.ZipPostalCodeRequired;
            destination.ZipPostalCode = model.ZipPostalCode;
            destination.City = model.City;
            destination.CountyEnabled = model.CountyEnabled;
            destination.CountyRequired = model.CountyRequired;
            destination.County = model.County;
            destination.CountryEnabled = model.CountryEnabled;
            destination.CountryRequired = model.CountryRequired;
            destination.CountryId = model.CountryId;
            destination.AvailableCountries = model.AvailableCountries;
            destination.StateProvinceEnabled = model.StateProvinceEnabled;
            destination.StateProvinceRequired = model.StateProvinceRequired;
            destination.StateProvinceId = model.StateProvinceId;
            destination.AvailableStates = model.AvailableStates;
            destination.PhoneEnabled = model.PhoneEnabled;
            destination.PhoneRequired = model.PhoneRequired;
            destination.Phone = model.Phone;
            destination.FaxEnabled = model.FaxEnabled;
            destination.FaxRequired = model.FaxRequired;
            destination.Fax = model.Fax;
            destination.NewsletterEnabled = model.NewsletterEnabled;
            destination.NewsLetterSubscriptions = model.NewsLetterSubscriptions;
            destination.SignatureEnabled = model.SignatureEnabled;
            destination.Signature = model.Signature;
            destination.TimeZoneId = model.TimeZoneId;
            destination.AllowCustomersToSetTimeZone = model.AllowCustomersToSetTimeZone;
            destination.AvailableTimeZones = model.AvailableTimeZones;
            destination.VatNumber = model.VatNumber;
            destination.VatNumberStatusNote = model.VatNumberStatusNote;
            destination.DisplayVatNumber = model.DisplayVatNumber;
            destination.AssociatedExternalAuthRecords = model.AssociatedExternalAuthRecords;
            destination.NumberOfExternalAuthenticationProviders = model.NumberOfExternalAuthenticationProviders;
            destination.AllowCustomersToRemoveAssociations = model.AllowCustomersToRemoveAssociations;
            destination.CustomerAttributes = model.CustomerAttributes;
            destination.GdprConsents = model.GdprConsents;

            return destination;
        }

        public static CustomerInfoExtendedModel CustomerInfoModelToCustomerInfoExtendedModel(this CustomerInfoModel model, bool trimFields = true)
        {
            if (model == null)
                return null;
            var destination = new CustomerInfoExtendedModel();


            if (trimFields)
            {
                if (model.FirstName != null)
                    model.FirstName = model.FirstName.Trim();
                if (model.LastName != null)
                    model.LastName = model.LastName.Trim();
                if (model.Email != null)
                    model.Email = model.Email.Trim();
                if (model.Company != null)
                    model.Company = model.Company.Trim();
                if (model.County != null)
                    model.County = model.County.Trim();
                if (model.City != null)
                    model.City = model.City.Trim();
                if (model.StreetAddress != null)
                    model.StreetAddress = model.StreetAddress.Trim();
                if (model.StreetAddress2 != null)
                    model.StreetAddress2 = model.StreetAddress2.Trim();
                if (model.ZipPostalCode != null)
                    model.ZipPostalCode = model.ZipPostalCode.Trim();
                if (model.Phone != null)
                    model.Phone = model.Phone.Trim();
                if (model.Fax != null)
                    model.Fax = model.Fax.Trim();
            }
            destination.Email = model.Email;
            destination.EmailToRevalidate = model.EmailToRevalidate;
            destination.CheckUsernameAvailabilityEnabled = model.CheckUsernameAvailabilityEnabled;
            destination.AllowUsersToChangeUsernames = model.AllowUsersToChangeUsernames;
            destination.UsernamesEnabled = model.UsernamesEnabled;
            destination.NeutralGenderEnabled = model.NeutralGenderEnabled;
            destination.VatNumberRequired = model.VatNumberRequired;
            destination.Username = model.Username;
            destination.GenderEnabled = model.GenderEnabled;
            destination.Gender = model.Gender;
            destination.City = model.City;
            destination.FirstNameEnabled = model.FirstNameEnabled;
            destination.FirstName = model.FirstName;
            destination.FirstNameRequired = model.FirstNameRequired;
            destination.LastNameEnabled = model.LastNameEnabled;
            destination.LastName = model.LastName;
            destination.LastNameRequired = model.LastNameRequired;
            destination.DateOfBirthEnabled = model.DateOfBirthEnabled;
            destination.DateOfBirthDay = model.DateOfBirthDay;
            destination.DateOfBirthMonth = model.DateOfBirthMonth;
            destination.DateOfBirthYear = model.DateOfBirthYear;
            destination.DateOfBirthRequired = model.DateOfBirthRequired;
            destination.CompanyEnabled = model.CompanyEnabled;
            destination.CompanyRequired = model.CompanyRequired;
            destination.Company = model.Company;
            destination.StreetAddressEnabled = model.StreetAddressEnabled;
            destination.StreetAddressRequired = model.StreetAddressRequired;
            destination.StreetAddress = model.StreetAddress;
            destination.StreetAddress2Enabled = model.StreetAddress2Enabled;
            destination.StreetAddress2Required = model.StreetAddress2Required;
            destination.StreetAddress2 = model.StreetAddress2;
            destination.ZipPostalCodeEnabled = model.ZipPostalCodeEnabled;
            destination.ZipPostalCodeRequired = model.ZipPostalCodeRequired;
            destination.ZipPostalCode = model.ZipPostalCode;
            destination.City = model.City;
            destination.CountyEnabled = model.CountyEnabled;
            destination.CountyRequired = model.CountyRequired;
            destination.County = model.County;
            destination.CountryEnabled = model.CountryEnabled;
            destination.CountryRequired = model.CountryRequired;
            destination.CountryId = model.CountryId;
            destination.AvailableCountries = model.AvailableCountries;
            destination.StateProvinceEnabled = model.StateProvinceEnabled;
            destination.StateProvinceRequired = model.StateProvinceRequired;
            destination.StateProvinceId = model.StateProvinceId;
            destination.AvailableStates = model.AvailableStates;
            destination.PhoneEnabled = model.PhoneEnabled;
            destination.PhoneRequired = model.PhoneRequired;
            destination.Phone = model.Phone;
            destination.FaxEnabled = model.FaxEnabled;
            destination.FaxRequired = model.FaxRequired;
            destination.Fax = model.Fax;
            destination.NewsletterEnabled = model.NewsletterEnabled;
            destination.NewsLetterSubscriptions = model.NewsLetterSubscriptions;
            destination.SignatureEnabled = model.SignatureEnabled;
            destination.Signature = model.Signature;
            destination.TimeZoneId = model.TimeZoneId;
            destination.AllowCustomersToSetTimeZone = model.AllowCustomersToSetTimeZone;
            destination.AvailableTimeZones = model.AvailableTimeZones;
            destination.VatNumber = model.VatNumber;
            destination.VatNumberStatusNote = model.VatNumberStatusNote;
            destination.DisplayVatNumber = model.DisplayVatNumber;
            destination.AssociatedExternalAuthRecords = model.AssociatedExternalAuthRecords;
            destination.NumberOfExternalAuthenticationProviders = model.NumberOfExternalAuthenticationProviders;
            destination.AllowCustomersToRemoveAssociations = model.AllowCustomersToRemoveAssociations;
            destination.CustomerAttributes = model.CustomerAttributes;
            destination.GdprConsents = model.GdprConsents;

            return destination;
        }



        public static AddressModel CustomAddressModelToAddressModel(this CustomAddressModel model, bool trimFields = true)
        {
            if (model == null)
                return null;
            var destination = new AddressModel();


            if (trimFields)
            {
                if (model.FirstName != null)
                    model.FirstName = model.FirstName.Trim();
                if (model.LastName != null)
                    model.LastName = model.LastName.Trim();
                if (model.Email != null)
                    model.Email = model.Email.Trim();
                if (model.Company != null)
                    model.Company = model.Company.Trim();
                if (model.County != null)
                    model.County = model.County.Trim();
                if (model.City != null)
                    model.City = model.City.Trim();
                if (model.Address1 != null)
                    model.Address1 = model.Address1.Trim();
                if (model.Address2 != null)
                    model.Address2 = model.Address2.Trim();
                if (model.ZipPostalCode != null)
                    model.ZipPostalCode = model.ZipPostalCode.Trim();
                if (model.PhoneNumber != null)
                    model.PhoneNumber = model.PhoneNumber.Trim();
                if (model.FaxNumber != null)
                    model.FaxNumber = model.FaxNumber.Trim();
            }
            destination.Id = model.Id;
            destination.FirstName = model.FirstName;
            destination.LastName = model.LastName;
            destination.Email = model.Email;
            destination.Company = model.Company;
            destination.CountryId = model.CountryId == 0 ? null : model.CountryId;
            destination.StateProvinceId = model.StateProvinceId == 0 ? null : model.StateProvinceId;
            destination.County = model.County;
            destination.City = model.City;
            destination.Address1 = model.Address1;
            destination.Address2 = model.Address2;
            destination.ZipPostalCode = model.ZipPostalCode;
            destination.PhoneNumber = model.PhoneNumber;
            destination.FaxNumber = model.FaxNumber;

            return destination;
        }

        public static CustomAddressModel AddressModelToCustomAddressModel(this AddressModel model, bool trimFields = true)
        {
            if (model == null)
                return null;
            var destination = new CustomAddressModel();


            if (trimFields)
            {
                if (model.FirstName != null)
                    model.FirstName = model.FirstName.Trim();
                if (model.LastName != null)
                    model.LastName = model.LastName.Trim();
                if (model.Email != null)
                    model.Email = model.Email.Trim();
                if (model.Company != null)
                    model.Company = model.Company.Trim();
                if (model.County != null)
                    model.County = model.County.Trim();
                if (model.City != null)
                    model.City = model.City.Trim();
                if (model.Address1 != null)
                    model.Address1 = model.Address1.Trim();
                if (model.Address2 != null)
                    model.Address2 = model.Address2.Trim();
                if (model.ZipPostalCode != null)
                    model.ZipPostalCode = model.ZipPostalCode.Trim();
                if (model.PhoneNumber != null)
                    model.PhoneNumber = model.PhoneNumber.Trim();
                if (model.FaxNumber != null)
                    model.FaxNumber = model.FaxNumber.Trim();
            }
            destination.Id = model.Id;
            destination.FirstName = model.FirstName;
            destination.LastName = model.LastName;
            destination.FullName = model.FullName;
            destination.Email = model.Email;
            destination.CompanyEnabled = model.CompanyEnabled;
            destination.Company = model.Company;
            destination.CountryEnabled = model.CountryEnabled;
            destination.CountryId = model.CountryId == 0 ? null : model.CountryId;
            destination.CountryName = model.CountryName;
            destination.StateProvinceEnabled = model.StateProvinceEnabled;
            destination.StateProvinceId = model.StateProvinceId == 0 ? null : model.StateProvinceId;
            destination.StateProvinceName = model.StateProvinceName;
            destination.CountyEnabled = model.CountyEnabled;
            destination.CountyRequired = model.CountyRequired;
            destination.County = model.County;
            destination.CityEnabled = model.CityEnabled;
            destination.CityRequired = model.CityRequired;
            destination.City = model.City;
            destination.StreetAddressEnabled = model.StreetAddressEnabled;
            destination.StreetAddressRequired = model.StreetAddressRequired;
            destination.Address1 = model.Address1;
            destination.StreetAddress2Enabled = model.StreetAddress2Enabled;
            destination.StreetAddress2Required = model.StreetAddress2Required;
            destination.Address2 = model.Address2;
            destination.ZipPostalCodeEnabled = model.ZipPostalCodeEnabled;
            destination.ZipPostalCodeRequired = model.ZipPostalCodeRequired;
            destination.ZipPostalCode = model.ZipPostalCode;
            destination.PhoneEnabled = model.PhoneEnabled;
            destination.PhoneRequired = model.PhoneRequired;
            destination.PhoneNumber = model.PhoneNumber;
            destination.FaxEnabled = model.FaxEnabled;
            destination.FaxRequired = model.FaxRequired;
            destination.FaxNumber = model.FaxNumber;
            destination.AvailableCountries = model.AvailableCountries;
            destination.AvailableStates = model.AvailableStates;
            destination.FormattedCustomAddressAttributes = model.FormattedCustomAddressAttributes;
            destination.CustomAddressAttributes = model.CustomAddressAttributes;
            destination.CountryTwoLetterSeoCode = model.CountryTwoLetterSeoCode;
            destination.StateAbbreviation = model.StateAbbreviation;
            return destination;
        }

        public static AddressModel CheckoutAddressModelToAddressModel(this CheckoutAddressModel model, bool trimFields = true)
        {
            if (model == null)
                return null;
            var destination = new AddressModel();


            if (trimFields)
            {
                if (model.FirstName != null)
                    model.FirstName = model.FirstName.Trim();
                if (model.LastName != null)
                    model.LastName = model.LastName.Trim();
                if (model.Email != null)
                    model.Email = model.Email.Trim();
                if (model.Company != null)
                    model.Company = model.Company.Trim();
                if (model.County != null)
                    model.County = model.County.Trim();
                if (model.City != null)
                    model.City = model.City.Trim();
                if (model.Address1 != null)
                    model.Address1 = model.Address1.Trim();
                if (model.Address2 != null)
                    model.Address2 = model.Address2.Trim();
                if (model.ZipPostalCode != null)
                    model.ZipPostalCode = model.ZipPostalCode.Trim();
                if (model.PhoneNumber != null)
                    model.PhoneNumber = model.PhoneNumber.Trim();
                if (model.FaxNumber != null)
                    model.FaxNumber = model.FaxNumber.Trim();
            }
            destination.Id = model.Id;
            destination.FirstName = model.FirstName;
            destination.LastName = model.LastName;
            destination.Email = model.Email;
            destination.Company = model.Company;
            destination.CountryId = model.CountryId == 0 ? null : model.CountryId;
            destination.StateProvinceId = model.StateProvinceId == 0 ? null : model.StateProvinceId;
            destination.County = model.County;
            destination.City = model.City;
            destination.Address1 = model.Address1;
            destination.Address2 = model.Address2;
            destination.ZipPostalCode = model.ZipPostalCode;
            destination.PhoneNumber = model.PhoneNumber;
            destination.FaxNumber = model.FaxNumber;

            return destination;
        }

        public static CheckoutAddressModel AddressModelToCheckoutAddressModel(this AddressModel model, bool trimFields = true)
        {
            if (model == null)
                return null;
            var destination = new CheckoutAddressModel();


            if (trimFields)
            {
                if (model.FirstName != null)
                    model.FirstName = model.FirstName.Trim();
                if (model.LastName != null)
                    model.LastName = model.LastName.Trim();
                if (model.Email != null)
                    model.Email = model.Email.Trim();
                if (model.Company != null)
                    model.Company = model.Company.Trim();
                if (model.County != null)
                    model.County = model.County.Trim();
                if (model.City != null)
                    model.City = model.City.Trim();
                if (model.Address1 != null)
                    model.Address1 = model.Address1.Trim();
                if (model.Address2 != null)
                    model.Address2 = model.Address2.Trim();
                if (model.ZipPostalCode != null)
                    model.ZipPostalCode = model.ZipPostalCode.Trim();
                if (model.PhoneNumber != null)
                    model.PhoneNumber = model.PhoneNumber.Trim();
                if (model.FaxNumber != null)
                    model.FaxNumber = model.FaxNumber.Trim();
            }
            destination.Id = model.Id;
            destination.FirstName = model.FirstName;
            destination.LastName = model.LastName;
            destination.FullName = model.FullName;
            destination.Email = model.Email;
            destination.CompanyEnabled = model.CompanyEnabled;
            destination.Company = model.Company;
            destination.CountryEnabled = model.CountryEnabled;
            destination.CountryId = model.CountryId == 0 ? null : model.CountryId;
            destination.CountryName = model.CountryName;
            destination.StateProvinceEnabled = model.StateProvinceEnabled;
            destination.StateProvinceId = model.StateProvinceId == 0 ? null : model.StateProvinceId;
            destination.StateProvinceName = model.StateProvinceName;
            destination.CountyEnabled = model.CountyEnabled;
            destination.CountyRequired = model.CountyRequired;
            destination.County = model.County;
            destination.CityEnabled = model.CityEnabled;
            destination.CityRequired = model.CityRequired;
            destination.City = model.City;
            destination.StreetAddressEnabled = model.StreetAddressEnabled;
            destination.StreetAddressRequired = model.StreetAddressRequired;
            destination.Address1 = model.Address1;
            destination.StreetAddress2Enabled = model.StreetAddress2Enabled;
            destination.StreetAddress2Required = model.StreetAddress2Required;
            destination.Address2 = model.Address2;
            destination.ZipPostalCodeEnabled = model.ZipPostalCodeEnabled;
            destination.ZipPostalCodeRequired = model.ZipPostalCodeRequired;
            destination.ZipPostalCode = model.ZipPostalCode;
            destination.PhoneEnabled = model.PhoneEnabled;
            destination.PhoneRequired = model.PhoneRequired;
            destination.PhoneNumber = model.PhoneNumber;
            destination.FaxEnabled = model.FaxEnabled;
            destination.FaxRequired = model.FaxRequired;
            destination.FaxNumber = model.FaxNumber;
            destination.AvailableCountries = model.AvailableCountries;
            destination.AvailableStates = model.AvailableStates;
            destination.FormattedCustomAddressAttributes = model.FormattedCustomAddressAttributes;
            destination.CustomAddressAttributes = model.CustomAddressAttributes;
            destination.CountryTwoLetterSeoCode = model.CountryTwoLetterSeoCode;
            destination.StateAbbreviation = model.StateAbbreviation;
            return destination;
        }


    }
}

