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
    public partial interface ICustomCategoryService : ICategoryService
    {

        Task<IList<ProductCategory>> GetProductCategoriesByProductIdExcludingRootLevelCategoriesAsync(int productId, bool showHidden = false);

        ValueTask<bool> IsMwtWidgetApplied(string widget, int entityId, string entityType, bool isMobileDevice);

        Task<IPagedList<Category>> GetAccessibleCategoriesAsync(string categoryName, int customerId, int storeId = 0,
         int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null);

    }
}
