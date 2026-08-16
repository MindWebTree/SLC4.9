using Nop.Services.ExportImport;

namespace MWT.Nop.Core.Services.ExportImport
{
    public partial interface IImportExtendedManager: IImportManager
    {
        Task ImportCategoryProductsFromXlsxAsync(Stream stream, int categoryid);
        Task ImportSpecificationAttributeOptionProductsFromXlsxAsync(Stream stream, int specificationAttributeOptionId);
        Task ImportProductAttributeCombinationFromXlsxAsync(Stream stream);
    }
}
