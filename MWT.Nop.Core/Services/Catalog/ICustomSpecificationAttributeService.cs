using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Catalog
{
    public partial interface ICustomSpecificationAttributeService:ISpecificationAttributeService
    {
        Task<List<ProductSpecificationAttribute>> GetProductSpecificationAttributesByAttributeIdAsync(
                int productId, int specificationAttributeId = 0);
        Task<List<SpecificationAttribute>> GetAllSpecificationAttributesAsync();
        Task<List<SpecificationAttributeOption>> GetAllSpecificationOptionsAsync();
        Task<int> GetMainCategoryOfProduct(int productId);
        Task<IPagedList<ProductSpecificationAttribute>> GetProductsBySpecificationAttributeOptionIdAsync(int specificationAttributeOptionId, int pageIndex, int pageSize);

        Task<List<ProductSpecificationAttribute>> GetProductSpecificationAttributeBySpecificationAttributeOptionIdAndProductIdAsync(int specificationAttributeOptionId, int productiD);
        Task<IPagedList<ProductSpecificationAttribute>> GetProductsSearchBySpecificationAttributeOptionIdAsync(int specificationAttributeOptionId, string search, int categoryId, int pageIndex, int pageSize);



        Task<IPagedList<Product>> CustomGetProductsBySpecificationAttributeIdAsync
            (int specificationAttributeId, string search, int categoryId, int pageIndex, int pageSize);

        Task<List<ProductSpecificationAttribute>> CustomGetProductsSpecificationAttributeOptionByCategoryWiseAsync(int categoryId, int specificationAttributeOptionId = 0);
    }
}
