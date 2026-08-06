using MWT.Plugin.Misc.MwtStorefront.Models.FAQModule;

namespace MWT.Plugin.Misc.MwtStorefront.Factories
{
    public partial interface IFaqModelFactory
    {
        Task<List<FaqModel>> PrepareFaqListModelAsync(int entityId, string entityType);
    }
}
