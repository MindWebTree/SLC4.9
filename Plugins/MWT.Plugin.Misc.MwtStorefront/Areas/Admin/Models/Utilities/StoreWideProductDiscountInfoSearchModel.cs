using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Models.Customization.Utilities
{
    public partial record StoreWideProductDiscountInfoSearchModel : BaseSearchModel
    {
        public StoreWideProductDiscountInfoSearchModel()
        {
            AvailableCategories = new List<SelectListItem>();
        }
        #region Properties

        public string SearchProductIds { get; set; }

        [NopResourceDisplayName("Admin.Utilities.Marketing.Offer.SearchCategoryId")]
        public int SearchCategoryId { get; set; }

        public IList<SelectListItem> AvailableCategories { get; set; }

        #endregion
    }
}
