using Nop.Core.Configuration;

namespace MWT.Tax.FixedOrByCountryStateZip
{
    /// <summary>
    /// Represents settings of the "Fixed or by country & state & zip" tax plugin
    /// </summary>
    public class TaxZarSettings : ISettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether the "tax calculation by country & state & zip" method is selected
        /// </summary>
        public bool CountryStateZipEnabled { get; set; }
        public string ApiLink { get; set; }
        public string Token { get; set; }
        public bool IncludeWgs { get; set; }
        public string ZipTaxApiLink { get; set; }
        public string ZipTaxKey { get; set; }
        public int TaxProvider { get; set; }
        public string IncludedCountries { get; set; }
    }
}