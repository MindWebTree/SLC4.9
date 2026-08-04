using MWT.Nop.Core.Domain.Custom.LandingPage_Management;
using Nop.Core;

namespace MWT.Nop.Core.Services.LandingPage_Management
{
    public partial interface ILandingPageService
    {
        /// <summary>
        /// Delete QuestionAnswers
        /// </summary>
        /// <param name="LandingPages">LandingPages</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteLandingPageAsync(LandingPage landingPage);

        /// <summary>
        /// Gets all categories
        /// </summary>
        /// <param name="storeId">Store identifier; 0 if you want to get all records</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the categories
        /// </returns>
        Task<IPagedList<LandingPage>> GetAllLandingPagesAsync(string landingPagesName = null, int storeId = 0, bool showHidden = false, int pageIndex = 0, int pageSize = 0, bool? overridePublished = null);

        /// <summary>
        /// Gets a LandingPages
        /// </summary>
        /// <param name="LandingPagesId">LandingPages identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the LandingPages
        /// </returns>
        Task<LandingPage> GetLandingPageByIdAsync(int landingPageId);

        /// <summary>
        /// Inserts LandingPages
        /// </summary>
        /// <param name="LandingPages">LandingPages</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertLandingPageAsync(LandingPage landingPage);

        /// <summary>
        /// Updates the LandingPages
        /// </summary>
        /// <param name="LandingPages">LandingPages</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateLandingPageAsync(LandingPage landingPages);

        /// <summary>
        /// Delete a list of LandingPages
        /// </summary>
        /// <param name="categories">LandingPages</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteLandingPageAsync(IList<LandingPage> landingPages);
        /// <summary>
        /// Delete a list of LandingPages
        /// </summary>
        /// <param name="categories">LandingPages</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// 
        Task<IList<LandingPage>> GetLandingPagesByIdsAsync(int[] landingPagesIds);

    }
}
