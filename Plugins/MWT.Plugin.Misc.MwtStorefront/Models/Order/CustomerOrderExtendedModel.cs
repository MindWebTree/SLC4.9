

using Nop.Web.Models.Order;

namespace MWT.Plugin.Misc.MwtStorefront.Models.Order
{
    public partial record CustomerOrderExtendedModel: CustomerOrderModel
    {
        public string CustomerEmail { get; set; }
    }
}
