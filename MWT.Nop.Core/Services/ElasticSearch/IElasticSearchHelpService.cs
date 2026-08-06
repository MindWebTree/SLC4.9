namespace MWT.Nop.Core.Services.ElasticSearch
{
    public partial interface IElasticSearchHelpService
    {
        Task<string> GetAutoCompleResult(string searchKeyWord);
        Task<string> readData(dynamic json);

        Task<string> GetListing(string searchKeyWord, int pageNumber, int pageSize, int catID, string sortBy,
            Dictionary<int, string> filters);
    }
}
