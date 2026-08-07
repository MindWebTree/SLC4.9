
using MWT.Plugin.Misc.MwtStorefront.Models.RelatedSearch;
using Nop.Web.Models.Catalog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Factories
{
    public partial interface IRelatedSearchModelFactory
    {
        Task<List<RelatedSearchModel>> PrepareRelatedSearchListModelAsync( int entityId, string entityType);
        Task<List<RelatedSearchModel>> PrepareRelatedSearchListForSearchPageModelAsync(int[] productIds, string seacrhTerm);
    }
}
