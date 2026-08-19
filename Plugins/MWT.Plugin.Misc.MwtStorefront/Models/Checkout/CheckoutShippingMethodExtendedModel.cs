using Nop.Web.Framework.Models;
using Nop.Web.Models.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Models.Checkout
{
    public record CheckoutShippingMethodExtendedModel: CheckoutShippingMethodModel
    {
        public CheckoutShippingMethodExtendedModel()
        {
            ShippingMethods = new List<ShippingMethodExtendedModel>();
            Warnings = new List<string>();

        }
        public bool WgsRequired { get; set; }
        public new  IList<ShippingMethodExtendedModel> ShippingMethods { get; set; }
        public partial record ShippingMethodExtendedModel : ShippingMethodModel
        {
            public string FeeExcludedAdditionalCharges { get; set; }
            public string AdditionalFee { get; set; }

            public string CustomShippingMethodDescription { get; set; }
        }
    }
}
