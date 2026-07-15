using Nop.Core;

namespace MWT.Tax.FixedOrByCountryStateZip.Domain
{
    /// <summary>
    /// Represents a tax rate
    /// </summary>
    public partial class MWTTaxRate : BaseEntity
    {
        /// <summary>
        /// Gets or sets the store identifier
        /// </summary>
        public int StoreId { get; set; }



        /// <summary>
        /// Gets or sets the country identifier
        /// </summary>
        public int CountryId { get; set; }

        /// <summary>
        /// Gets or sets the state/province identifier
        /// </summary>
        public int StateProvinceId { get; set; }

        /// <summary>
        /// Gets or sets the zip
        /// </summary>
        public string Zip { get; set; }

        /// <summary>
        /// Gets or sets the percentage
        /// </summary>
        public decimal Percentage { get; set; }

        public decimal GSTPercentage { get; set; }
        public decimal PSTPercentage { get; set; }
        public decimal HSTPercentage { get; set; }
        public decimal QSTPercentage { get; set; }
    }
}