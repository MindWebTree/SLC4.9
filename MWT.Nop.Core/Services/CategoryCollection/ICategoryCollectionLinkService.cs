using MWT.Nop.Core.Domain;

namespace MWT.Nop.Core.Services.CategoryCollection
{
    public partial interface ICategoryCollectionLinkService
    {
        Task UpdateAsync(CategoryCollectionLink collectionlink);
        Task DeleteAsync(CategoryCollectionLink collectionlink);
        Task InsertAsync(CategoryCollectionLink collectionlink);
        Task<CategoryCollectionLink> GetById(int collectionlink);
        Task<IList<CategoryCollectionLink>> GetCategoryCollectionLinkByEntityId(int entityId);
      
    }
}
