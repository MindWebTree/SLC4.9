
using MWT.Nop.Core.Domain;
using Nop.Data;
using Nop.Services.Configuration; 

namespace MWT.Nop.Core.Services.Search
{
    public partial class SearchLogService : ISearchLogService
    {
        #region Fields

        private readonly IRepository<SearchLog> _searchLogRepository;
        private readonly ISettingService _settingService;

        #endregion

        #region Ctor

        public SearchLogService(IRepository<SearchLog> searchLogRepository,
            ISettingService settingService)
        {
            this._searchLogRepository = searchLogRepository;
            this._settingService = settingService;
        }
        #endregion

        #region Methods
        public async Task<List<SearchLog>> GetLatestSearchTermOFCustomer(int customerId)
        {
            int noOfSearchTerms = await _settingService.GetSettingByKeyAsync<int>("catalogsettings.AutoCompleteSearch.NoOfSearchTerms");
            if (noOfSearchTerms <= 0)
                return new List<SearchLog>();
            var query = from st in _searchLogRepository.Table
                        where st.CustomerId == customerId
                        orderby st.Id descending
                        select st;
            return await query.Take(noOfSearchTerms).ToListAsync();
        }

        public async Task InsertSearchLog(SearchLog log)
        {
            if (log.Keyword.Length > 2000)
                log.Keyword = log.Keyword.Substring(0, 2000);
            var query = from st in _searchLogRepository.Table
                        where st.Keyword.Trim() == log.Keyword.Trim() && st.CustomerId == log.CustomerId
                        orderby st.Id descending
                        select st;
            var searchTerm = await query.FirstOrDefaultAsync();
            if (searchTerm != null)
            {
                searchTerm.UpdatedOn = DateTime.Now;
                await _searchLogRepository.UpdateAsync(log);
            }
            else
            {
                await _searchLogRepository.InsertAsync(log);
            }
        }
        public async Task ClearRecentSearchOfCustomer(int customerId)
        {
            var logs = await (from st in _searchLogRepository.Table
                              where st.CustomerId == customerId
                              orderby st.Id descending
                              select st).ToListAsync();
            foreach (var log in logs)
            {
                await _searchLogRepository.DeleteAsync(log);
            }
        }

        #endregion
    }
}
