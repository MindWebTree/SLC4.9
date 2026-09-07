using MWT.Nop.Core.Domain.Catalog;

namespace MWT.Nop.Core.Services.Catalog
{
    public partial interface ICustomizationFormSerivce
    {
        Task<IList<ProductCustomizationFormTemplate>> GetAllProductCustomizationFormTemplatesAsync();
        Task<ProductCustomizationFormTemplate> GetProductCustomizationFormTemplateByIdAsync(int productCustomizationFormTemplateId);

    }
}
