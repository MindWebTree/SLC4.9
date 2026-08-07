using MWT.Nop.Core.Domain;
using Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
