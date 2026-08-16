using MWT.Plugin.Misc.MwtStorefront.Models.Catalog;
using Nop.Core.Domain.Catalog;


namespace MWT.Plugin.Misc.MwtStorefront.Factories
{
    public partial interface IElasticSearchModelFactory
    {
        Task<CustomProductOverviewModel> PrepareProductModelForElasticSearch(int productId);

        Task<CustomProductOverviewModel> PrepareProductModelForElasticSearch(Product product);
    }
}
