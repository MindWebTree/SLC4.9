using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;

namespace MWT.Nop.Core.Services.Catalog
{
    public partial interface ICopyProductExtendedService: ICopyProductService
    {
        Task<Product> CustomCopyProductAsync(Product product, string newName, string newSku,
                  bool isPublished = true, bool copyImages = true, bool copyAssociatedProducts = true);


    }
}
