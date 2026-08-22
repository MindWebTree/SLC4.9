using Nop.Web.Framework.Models;
namespace Nop.Web.Models.Checkout
{
    public partial record CheckoutPaymentInfoModel : BaseNopModel
    {
        public dynamic AdditionaData { get; set; }
    }
}

