using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;

namespace MWT.Nop.Core.Services.Catalog
{
    public partial interface ICustomProductAttributeService : IProductAttributeService
    {
        Task UpdateProductAttributeValueWithoutEventAsync(ProductAttributeValue productAttributeValue);
        Task<IList<ProductAttributeCombination>> CustomGetAllProductAttributeCombinationsAsync(int productId);
        Task<IList<ProductAttributeMapping>> CustomGetProductAttributeMappingsByProductIdAsync(int productId);
        Task<IList<ProductAttributeValue>> CustomGetProductAttributeValuesAsync(int productAttributeMappingId);
        Task<IList<string>> Stains(int productAttributeId, string search);
        Task CustomUpdateProductAttributeCombinationAsync(ProductAttributeCombination combination);
    }
}
