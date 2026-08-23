using Nop.Web.Framework.Models;

namespace Nop.Web.Models.Checkout
{
    public partial record CheckoutShippingMethodModel : BaseNopModel
    {
        public bool WgsRequired { get; set; }
        public partial record ShippingMethodModel : BaseNopModel
        {
            public string FeeExcludedAdditionalCharges { get; set; }
            public string AdditionalFee { get; set; }

            public string CustomShippingMethodDescription { get; set; }
        }
    }
}
