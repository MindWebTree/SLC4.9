using Nop.Web.Areas.Admin.Models.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Factories
{
    public partial interface ICategoryCollectionLinkModelFactory
    {
        Task<CategoryCollectionLinkListModel> PrepareCategoryCollectionLinkListModelAsync(CollectionLinkSearchModel searchModel, int entityId);
    }
}
