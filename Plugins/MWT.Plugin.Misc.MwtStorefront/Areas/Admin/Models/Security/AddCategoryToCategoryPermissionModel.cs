using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.Security
{
    public partial record AddCategoryToCategoryPermissionModel : BaseNopModel
    {
        #region Ctor

        public AddCategoryToCategoryPermissionModel()
        {
            AvailableCategories = new List<SelectListItem>();
            AvailableCustomerIds = new List<SelectListItem>();
        }
        #endregion

        #region Properties

        [NopResourceDisplayName("Admin.Catalog.CategoryPermission.Fields.Categories")]
        public int SelectedCategoryId { get; set; }
        public IList<SelectListItem> AvailableCategories { get; set; }

        [NopResourceDisplayName("Admin.Catalog.CategoryPermission.Fields.Customers")]
        public int SelectedCustomerId { get; set; }
        public IList<SelectListItem> AvailableCustomerIds { get; set; }

        #endregion
    }

}
