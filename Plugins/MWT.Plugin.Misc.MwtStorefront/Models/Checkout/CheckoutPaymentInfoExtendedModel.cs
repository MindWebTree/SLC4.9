using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Orders;
using Nop.Web.Models.Checkout;

namespace MWT.Plugin.Misc.MwtStorefront.Models.Checkout
{
    public partial record CheckoutPaymentInfoExtendedModel : CheckoutPaymentInfoModel
    {
        public CustomOrderSummaryModel AdditionaData { get; set; }
    }
}
