using System.Threading.Tasks;
using Nop.Core;
using MWT.Tax.FixedOrByCountryStateZip.Domain;

namespace MWT.Tax.FixedOrByCountryStateZip.Services
{
    /// <summary>
    /// Tax rate service interface
    /// </summary>
    public partial interface IMWTCountryStateZipService
    {
        /// <summary>
        /// Deletes a tax rate
        /// </summary>
        /// <param name="taxRate">Tax rate</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteTaxRateAsync(MWTTaxRate taxRate);

        /// <summary>
        /// Gets all tax rates
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ax rates
        /// </returns>
        Task<IPagedList<MWTTaxRate>> GetAllTaxRatesAsync(int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Gets a tax rate
        /// </summary>
        /// <param name="taxRateId">Tax rate identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ax rate
        /// </returns>
        Task<MWTTaxRate> GetTaxRateByIdAsync(int taxRateId);

        /// <summary>
        /// Inserts a tax rate
        /// </summary>
        /// <param name="taxRate">Tax rate</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertTaxRateAsync(MWTTaxRate taxRate);

        /// <summary>
        /// Updates the tax rate
        /// </summary>
        /// <param name="taxRate">Tax rate</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateTaxRateAsync(MWTTaxRate taxRate);
    }
}