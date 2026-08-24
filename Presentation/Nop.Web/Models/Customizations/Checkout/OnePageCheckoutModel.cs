using Nop.Web.Framework.Models;

namespace Nop.Web.Models.Checkout
{
    public partial record OnePageCheckoutModel : BaseNopModel
    {
        public CheckoutAddressModel AddressModel { get; set; }
        public string StepActive { get; set; }
        public CheckoutShippingMethodModel CheckoutShippingMethodModel { get; set; }
        public CheckoutPaymentMethodModel CheckoutPaymentMethodModel { get; set; }
    }
}
