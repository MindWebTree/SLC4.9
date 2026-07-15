using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace MWT.Tax.FixedOrByCountryStateZip.Models
{
    public record ConfigurationModel : BaseSearchModel
    {
        public ConfigurationModel()
        {
            AvailableStores = new List<SelectListItem>();
            AvailableCountries = new List<SelectListItem>();
            AvailableStates = new List<SelectListItem>();
            AvailableTaxCategories = new List<SelectListItem>();
            TaxProviderTypes = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.Store")]
        public int AddStoreId { get; set; }
        [NopResourceDisplayName("Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.Country")]
        public int AddCountryId { get; set; }
        [NopResourceDisplayName("Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.StateProvince")]
        public int AddStateProvinceId { get; set; }
        [NopResourceDisplayName("Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.Zip")]
        public string AddZip { get; set; }

        [NopResourceDisplayName("Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.Percentage")]
        public decimal AddPercentage { get; set; }

        public bool CountryStateZipEnabled { get; set; }

        public string TaxCategoriesCanNotLoadedError { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }
        public IList<SelectListItem> AvailableCountries { get; set; }
        public IList<SelectListItem> AvailableStates { get; set; }
        public IList<SelectListItem> AvailableTaxCategories { get; set; }

        public IList<SelectListItem> TaxProviderTypes { get; set; }

        [NopResourceDisplayName("Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.ApiLink")]
        public string ApiLink { get; set; }


        [NopResourceDisplayName("Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.Token")]
        public string Token { get; set; }

        [NopResourceDisplayName("Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.IncludeWgs")]
        public bool IncludeWgs { get; set; }

        [NopResourceDisplayName("Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.ZipTaxApiLink")]
        public string ZipTaxApiLink { get; set; }

        [NopResourceDisplayName("Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.ZipTaxKey")]
        public string ZipTaxKey { get; set; }

        [NopResourceDisplayName("Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.TaxProvider")]
        public int TaxProvider { get; set; }

        [NopResourceDisplayName("Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.OrderId")]
        public int OrderId { get; set; }

        [NopResourceDisplayName("Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.GSTPercentage")]
        public decimal GSTPercentage { get; set; }

        [NopResourceDisplayName("Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.PSTPercentage")]
        public decimal PSTPercentage { get; set; }

        [NopResourceDisplayName("Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.HSTPercentage")]
        public decimal HSTPercentage { get; set; }

        [NopResourceDisplayName("Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.QSTPercentage")]
        public decimal QSTPercentage { get; set; }

        [NopResourceDisplayName("Plugins.MWT.Tax.FixedOrByCountryStateZip.Fields.IncludedCountries")]
        public string IncludedCountries { get; set; }


    }
}