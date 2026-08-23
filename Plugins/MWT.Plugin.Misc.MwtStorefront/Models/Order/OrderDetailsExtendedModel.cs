using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Orders;
using MWT.Plugin.Misc.MwtStorefront.Models.Common;
using Nop.Services.Tax;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Common;
using Nop.Web.Models.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Models.Order
{
    public partial record OrderDetailsExtendedModel : OrderDetailsModel
    {
        public OrderDetailsExtendedModel()
        {
            Items = new List<OrderItemExtendedModel>();
            BillingAddress = new AddressModel();
            ShippingAddress = new AddressModel();
        }
        public new AddressModel ShippingAddress { get; set; }
        public new AddressModel BillingAddress { get; set; }
        public new IList<OrderItemExtendedModel> Items { get; set; }
        public string MembershipFee { get; set; }
        public string MembershipFeeDiscount { get; set; }
        public string MembershipDiscountIncTax { get; set; }
        public string OfferDiscountIncTax { get; set; }
        public string BuyMoreSaveMoreDiscountIncTax { get; set; }
        public string SpecialInstructions { get; set; }
        public string CreditCardNumber { get; set; }
        public string AdditonalShippingCharges { get; set; }

        public CustomOrderSummaryModel CustomOrderSummaryModel { get; set; }
        public string CustomerCurrencyCode { get; set; }
        public string CustomDuty { get; set; }
        public decimal CustomDutyPercentage { get; set; }
        public bool OrderConfirmed { get; set; }
        public List<TaxInfo> Taxes { get; set; }

        public Guid OrderGuid { get; set; }
        public string Email { get; set; }
        public partial record OrderItemExtendedModel : OrderItemModel
        {
            public string ItemPriceIncTax { get; set; }
            public string MembershipDiscountIncTax { get; set; }
            public string OfferDiscountIncTax { get; set; }
            public string BuyMoreSaveMoreDiscountIncTax { get; set; }
            public string SpecialInstructions { get; set; }
            public string ImageUrl { get; set; }
            public string TotalDiscount { get; set; }
            public string ItemTotal { get; set; }
            public int VariantId { get; set; }
        }
    }
}
