using Nop.Web.Areas.Admin.Models.Catalog;

namespace Nop.Web.Areas.Admin.Factories
{
    public partial interface ICategoryCollectionLinkModelFactory
    {
        Task<CategoryCollectionLinkListModel> PrepareCategoryCollectionLinkListModelAsync(CollectionLinkSearchModel searchModel, int entityId);
    }
}
