using MWT.Nop.Core.Domain;

namespace MWT.Nop.Core.Services
{
    public partial interface IRelatedSearchService
    {
        Task UpdateAsync(RelatedSearch relatedSearch);
        Task DeleteAsync(RelatedSearch relatedSearch);
        Task InsertAsync(RelatedSearch relatedSearch);
        Task<RelatedSearch> GetById(int Id);
        Task<IList<RelatedSearch>> GetRelatedSearchTermsByEntity(int entityId, string entityType);

        Task<IList<RelatedSearch>> GetRelatedSearchTermsByProductIds(int[] productIds);


    }
}
