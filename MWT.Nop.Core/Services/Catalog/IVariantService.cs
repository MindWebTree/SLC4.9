using MWTNop.Core.Domain.Catalog;
using Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Catalog
{
    public partial interface IVariantService
    {
        Task<int> GetProductVariantId(int productId);
        Task<List<VariantCombination>> GetPublishedProductVariants(int productId);
        Task<List<VariantCombination>> GetProductVariants(int productId);
        Task<(bool, decimal, decimal, decimal, decimal, decimal, decimal)> GetVariantPriceRange(Product product, bool createCombination = true, bool updateProduct = true
    , bool updateProductWithEvent = true, bool isVariantTask = true);

        Task<int> GetVariantId(int productId, string attributes);
        Task<int> GetProductVariantIdBySize(int productId, string size);


        Task<bool> GenerateVariantCombinations(int productId);
        Task<VariantCombination> GetVariantById(int id);
        Task UpdateVariant(VariantCombination variant, bool updateCombinations = true);
        Task<int> GetVariantIdFromAttributeDescription(int productId, string attributeDescription);
        Task<VariantCombination> GetVariantByVariantId(int variantId);
        Task<VariantCombination> GetVariantFromAttributeValues(int productId, List<int> attributeValueIds);
        Task GenerateProductVariantSename(Product product);
        Task<VariantCombination> GetItemVariantInfo(int productId, string attributesXml);

        Task<List<ProductAttributeCombination>> GetVariantCombinationAsync(
VariantCombination variant, IList<ProductAttributeCombination> productAttributeCombinations);
        Task<bool> UpdateGroupProductPrice(int productID);
        Task<bool> SyncGroupedProductsPrice();
    }
}
