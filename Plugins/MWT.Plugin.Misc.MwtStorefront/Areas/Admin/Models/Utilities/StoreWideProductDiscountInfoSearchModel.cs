using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.Utilities
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
