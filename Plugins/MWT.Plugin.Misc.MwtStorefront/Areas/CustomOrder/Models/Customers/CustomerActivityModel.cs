using Nop.Web.Framework.Models;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Customers
{
    public partial record CustomerActivityModel : BaseNopEntityModel
    {
        public string Details { get; set; }
    }
}
