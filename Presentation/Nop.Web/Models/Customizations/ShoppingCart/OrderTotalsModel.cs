using MWT.Nop.Core.Domain.Orders;
using Nop.Services.Tax;
using Nop.Web.Framework.Models;

namespace Nop.Web.Models.ShoppingCart
{
    public partial record OrderTotalsModel : BaseNopModel
    {
        public string BuyMoreSaveMoreDiscount { get; set; }
        public string MemberShipDiscount { get; set; }
        public string MemberShipFee { get; set; }
        public string MemberShipFeeDiscount { get; set; }
        public string CustomOrderTotal { get; set; }
        public string AdditionalFee { get; set; }
        public string subTotalWithOutDiscount { get; set; }
        public string offerDiscount { get; set; }

        public string CustomDuty { get; set; }
        public decimal CustomDutyPercentage { get; set; }
        public List<DiscountSummary> DiscountSummary { get; set; }
        public List<TaxInfo> Taxes { get; set; } = new List<TaxInfo>();
    }
}