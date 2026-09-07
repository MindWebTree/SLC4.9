
using MWT.Plugin.Misc.MwtStorefront.Models.QuickFilter;

namespace MWT.Plugin.Misc.MwtStorefront.Factories
{
    public partial interface IQuickFilterModelFactory
    {
        Task<List<QuickFilterModel>> PrepareQuickFilterListModelAsync( int entityId, string entityType);
    }
}
