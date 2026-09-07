namespace MWT.Nop.Core.Services.ElasticSearch
{
    public partial interface IFuzzySearchService
    {
        Task<List<(int categoryId, double score)>> SearchCategoryGenricKeyWords(string searchTerm);

        Task<List<(int categoryId, double score)>> SearchCategories(string searchTerm);
    }
}
