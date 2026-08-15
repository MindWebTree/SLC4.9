using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.Orders
{
    public partial class DiscountSummary
    {
        public DiscountSummary()
        {
            DiscountsSummary = new List<DiscountSummary>();
        }
        public DiscountType Type { get; set; }
        public string Heading { get; set; }
        public string Description { get; set; }
        public string Summary { get; set; }
        public List<DiscountSummary> DiscountsSummary { get; set; }
    }

    public enum DiscountType
    {
        MemberShipFeeDiscount = 1,
        MemberShipDiscount = 2,
        OfferDiscount = 3,
        BuyMoreSaveMoreDiscount = 4,
        SubTotalCouponDiscount = 5,
        ProductCouponDiscount = 6,
        OrderDiscount = 7
    }
}
