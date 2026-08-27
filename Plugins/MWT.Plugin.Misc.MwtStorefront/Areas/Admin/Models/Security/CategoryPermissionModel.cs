
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.Security
{
    public partial record CategoryPermissionModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.Catalog.CategoryPermission.Fields.UserEmail")]
        public string UserEmail { get; set; }
        [NopResourceDisplayName("Admin.Catalog.CategoryPermission.Fields.CategoryName")]
        public string CategoryName { get; set; }
    }
}
