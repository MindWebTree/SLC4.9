using Nop.Core.Domain.Catalog;
using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.TagPage
{
    public partial interface  ITagProductService
    {
        Task<IPagedList<Product>> SearchProductsWithVariablePageSizeAsync(int[] ids, int tagId, List<int> _specificationAttributes, ProductSortingEnum orderBy = ProductSortingEnum.Position,
                int pageIndex = 0, int pageSize = int.MaxValue, bool isMobileDevice = false);

        Task<List<ProductSpecificationAttribute>> CustomSearchGetProductSpecificationAttributeAsync(
    IList<int> categoryIds = null,
   int storeId = 0,
    int vendorId = 0,
    int warehouseId = 0,
    ProductType? productType = null,
    bool visibleIndividuallyOnly = false,
    bool excludeFeaturedProducts = false,
   int productTagId = 0,
    int languageId = 0,
    bool? overridePublished = null,
    bool showOutOfStock = false, int featuredId = 0, bool isMobileDevice = false, bool showHidden = false);

    }
}
