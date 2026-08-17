using MWT.Nop.Core.Domain.Custom.Integrity_Report;
using Nop.Core.Domain.Catalog;

namespace MWT.Nop.Core.Services.Integrity_Report
{
    public partial interface IProductIntegrityReportService
    {
        Task<IList<(Product Product, string issue)>> GetUnpublishedOrAttributeMissingProductsAsync(List<Product> productList);
        Task DeleteAllExceptIdsAsync(List<int> Ids);
        Task Upsert(List<(Product Product, string ErrorReason)> combinedlist);
        Task<List<ProductIntegrityReport>> GetAllAsync();
    }
}
