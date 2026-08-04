using MWT.Nop.Core.Domain;
using Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
