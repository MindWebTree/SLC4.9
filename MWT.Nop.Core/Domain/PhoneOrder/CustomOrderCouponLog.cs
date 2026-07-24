using Nop.Core;

namespace MWT.Nop.Core.Domain.PhoneOrder
{
    public partial class CustomOrderCouponLog : BaseEntity
     {
        public int CouponId { get; set; }
        public string CouponCode { get; set; }
        public int CustomerId { get; set; }
        public int OrderId { get; set; }
        public int CustomOrderOrderLogId { get; set; }
    }
}
