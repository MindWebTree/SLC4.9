using Nop.Web.Areas.Admin.Models.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Factories
{
    public partial interface IQuickFilterModelFactory
    {
        Task<QuickFilterListModel> PrepareQuickFilterListModelAsync(RelatedProductSearchModel searchModel, int entityId, string entityType);
    }
}
