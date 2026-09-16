using MWT.Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Catalog;
using Nop.Services.ExportImport;

namespace MWT.Nop.Core.Services.ExportImport
{
    public partial interface IExportExtendedManager: IExportManager
    {
        Task<byte[]> ExportCategoryProductsToXlsxAsync(IList<ProductCategory> products);
        Task<byte[]> ExportSpecificationAttributeOptionProductsToXlsxAsync(IList<ExportProductSpecFormat> products);
        Task ExportProductsToCsvAsync(List<object> records, string folder, string fileName);
        Task<byte[]> ExportProductAttributeCombinationToXlsxAsync(IList<ExportAttrCombination> attributeCombinations);

    }

}
