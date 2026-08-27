using Nop.Web.Framework.Models;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.Utilities
{
    public partial record StoreWideDiscountSettingSearchModel : BaseSearchModel
    {
        public int StoreWideDiscountId { get; set; }
    }
}
