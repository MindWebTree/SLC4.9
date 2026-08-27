using Microsoft.AspNetCore.Mvc.Rendering;
using MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.Security;

namespace MWT.Plugin.Misc.MwtStorefront.Area.Admin.Factories
{
    public partial interface ISecurityModelExtendedFactory
    {
        #region Category Permission

        Task<CategoryPermissionListModel> PrepareCategoryPermissionListModelAsync(CategoryPermissionSearchModel searchModel);
        Task PrepareCustomersAsync(IList<SelectListItem> items);

        #endregion
    }
}