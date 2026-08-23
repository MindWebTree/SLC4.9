using Nop.Web.Framework.Models;

namespace Nop.Web.Models.Checkout
{
    public partial record CheckoutPaymentMethodModel : BaseNopModel
    {
        public CheckoutPaymentInfoModel CheckoutPaymentInfoModel { get; set; }
        public IList<string> Warnings { get; set; }
        public bool TermsOfServiceOnOrderConfirmPage { get; set; }
        public bool TermsOfServicePopup { get; set; }
        public string MinOrderTotalWarning { get; set; }
    }
}
