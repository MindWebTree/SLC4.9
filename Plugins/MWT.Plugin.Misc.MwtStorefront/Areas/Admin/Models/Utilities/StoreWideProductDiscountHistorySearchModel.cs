using Nop.Web.Framework.Models;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.Utilities
{
    public partial record StoreWideProductDiscountHistorySearchModel : BaseSearchModel
    {
        public int SearchProductId { get; set; }
    }
}
