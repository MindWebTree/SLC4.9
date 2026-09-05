using Nop.Web.Areas.Admin.Models.Catalog;

namespace Nop.Web.Areas.Admin.Factories
{
    public partial interface IRelatedSearchModelFactory
    {
        Task<RelatedSearchListModel> PrepareRelatedSearchListModelAsync(RelatedProductSearchModel searchModel, int entityId, string entityType);
    }
}
