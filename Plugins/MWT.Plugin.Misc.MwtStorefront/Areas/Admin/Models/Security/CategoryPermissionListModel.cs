using Nop.Web.Framework.Models;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.Security
{
    /// <summary>
    /// Represents a category list model
    /// </summary>
    public partial record CategoryPermissionListModel : BasePagedListModel<CategoryPermissionModel>
    {
    }
}