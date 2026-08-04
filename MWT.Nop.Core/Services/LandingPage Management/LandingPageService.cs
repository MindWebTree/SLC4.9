using MWT.Nop.Core.Domain.Custom.LandingPage_Management;
using Nop.Core;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Security;
using Nop.Services.Stores;

namespace MWT.Nop.Core.Services.LandingPage_Management
{
    public partial class LandingPageService : ILandingPageService
    {
        #region Fields

        private readonly IAclService _aclService;
        private readonly IRepository<LandingPage> _landingPageRepository;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public LandingPageService(
            IAclService aclService,
            IRepository<LandingPage> landingPageRepository,
            IStoreMappingService storeMappingService,
            IWorkContext workContext)
        {
            _aclService = aclService;
            _landingPageRepository = landingPageRepository;
            _storeMappingService = storeMappingService;
            _workContext = workContext;
        }

        #endregion

   

        #region Methods


        /// <summary>
        /// Delete LandingPages
        /// </summary>
        /// <param name="LandingPages">LandingPages</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteLandingPageAsync(LandingPage landingPages)
        {
            await _landingPageRepository.DeleteAsync(landingPages);
        }

        /// <summary>
        /// Delete LandingPages
        /// </summary>
        /// <param name="LandingPages">LandingPages</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task DeleteLandingPageAsync(IList<LandingPage> landingPages)
        {
            if (landingPages == null)
                throw new ArgumentNullException(nameof(landingPages));

            foreach (var landingPage in landingPages)
                await DeleteLandingPageAsync(landingPage);
        }

        /// <summary>
        /// Gets all LandingPages
        /// </summary>
        /// <param name="LandingPagesName">LandingPages name</param>
        /// <param name="storeId">Store identifier; 0 if you want to get all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="overridePublished">
        /// null - process "Published" property according to "showHidden" parameter
        /// true - load only "Published" products
        /// false - load only "Unpublished" products
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the LandingPages
        /// </returns>

        public async Task<IPagedList<LandingPage>> GetAllLandingPagesAsync(string landingPagesName = null, int storeId = 0, bool showHidden = false, int pageIndex = 0, int pageSize = 0, bool? overridePublished = null)
        {
            var unsortedlandingPages = await _landingPageRepository.GetAllAsync(async query =>
            {
                if (!showHidden)
                    query = query.Where(c => c.Published);
                else if (overridePublished.HasValue)
                    query = query.Where(c => c.Published == overridePublished.Value);

                //apply store mapping constraints
                query = await _storeMappingService.ApplyStoreMapping(query, storeId);

                //apply ACL constraints
                if (!showHidden)
                {
                    var customer = await _workContext.GetCurrentCustomerAsync();
                    query = await _aclService.ApplyAcl(query, customer);
                }

                if (!string.IsNullOrWhiteSpace(landingPagesName))
                    query = query.Where(c => c.Name.Contains(landingPagesName));

                query = query.Where(c => !c.Deleted);

                return query.OrderBy(c => c.DisplayOrder).ThenBy(c => c.Id);
            });

           
            var sortedlandingPages = await SortLandingPageForTreeAsync(unsortedlandingPages);

            //paging
            return new PagedList<LandingPage>(sortedlandingPages, pageIndex, pageSize);

        }

        protected virtual async Task<IList<LandingPage>> SortLandingPageForTreeAsync(IList<LandingPage> source, int parentId = 0,
            bool ignoreLandingPageWithoutExistingParent = false)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var result = new List<LandingPage>();

            if (ignoreLandingPageWithoutExistingParent || result.Count == source.Count)
                return result;

            foreach (var lanfingPage in source)
                if (result.FirstOrDefault(x => x.Id == lanfingPage.Id) == null)
                    result.Add(lanfingPage);

            return result;
        }

        /// <summary>
        /// Gets a LandingPages
        /// </summary>
        /// <param name="LandingPagesId">LandingPages identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the LandingPages
        /// </returns>
        public virtual async Task<LandingPage> GetLandingPageByIdAsync(int LandingPagesId)
        {
            return await _landingPageRepository.GetByIdAsync(LandingPagesId, cache => default);
        }


        /// <summary>
        /// Inserts LandingPages
        /// </summary>
        /// <param name="LandingPages">LandingPages</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertLandingPageAsync(LandingPage LandingPages)
        {
            await _landingPageRepository.InsertAsync(LandingPages);
        }

        /// <summary>
        /// Updates the LandingPages
        /// </summary>
        /// <param name="LandingPages">LandingPages</param>
        /// <returns>A task that represents the asynchronous operation</returns>

        public virtual async Task<IList<LandingPage>> GetLandingPagesByIdsAsync(int[] landingPagesIds)
        {
            return await _landingPageRepository.GetByIdsAsync(landingPagesIds, includeDeleted: false);
        }

        /// <summary>
        /// Updates the LandingPages
        /// </summary>
        /// <param name="LandingPages">LandingPages</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateLandingPageAsync(LandingPage LandingPages)
        {
            if (LandingPages == null)
                throw new ArgumentNullException(nameof(LandingPages));


            await _landingPageRepository.UpdateAsync(LandingPages);
        }

        #endregion

    }
}


