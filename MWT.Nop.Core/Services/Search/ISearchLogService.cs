using MWT.Nop.Core.Domain; 

namespace MWT.Nop.Core.Services.Search
{
    public partial interface ISearchLogService
    {
        Task InsertSearchLog(SearchLog log);
        Task<List<SearchLog>> GetLatestSearchTermOFCustomer(int customerId);

        Task ClearRecentSearchOfCustomer(int customerId);
    }
}
