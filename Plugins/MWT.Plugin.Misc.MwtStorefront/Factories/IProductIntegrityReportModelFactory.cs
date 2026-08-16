using Nop.Core.Domain.Catalog;

namespace MWT.Plugin.Misc.MwtStorefront.Factories
{
    public partial interface IProductIntegrityReportModelFactory
    {
        Task<List<int>> GenerateProductIntegrityReportAsync(List<Product> prouctList);
    }
}
