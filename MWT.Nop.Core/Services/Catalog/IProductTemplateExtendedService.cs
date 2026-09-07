using MWT.Nop.Core.Domain.Catalog;

namespace MWT.Nop.Core.Services.Catalog
{
    public interface IProductTemplateSectionService
    {
        Task<IList<ProductTemplateSection>> GetProductTemplateSectionsByTemplateIdAsync(int templateId);
    }
}
