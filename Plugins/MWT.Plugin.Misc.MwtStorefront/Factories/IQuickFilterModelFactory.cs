
using MWT.Plugin.Misc.MwtStorefront.Models.QuickFilter;
using Nop.Web.Models.Catalog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Factories
{
    public partial interface IQuickFilterModelFactory
    {
        Task<List<QuickFilterModel>> PrepareQuickFilterListModelAsync( int entityId, string entityType);
    }
}
