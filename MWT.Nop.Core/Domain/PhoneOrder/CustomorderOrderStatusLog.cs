using Nop.Core;
using System;

namespace MWT.Nop.Core.Domain.PhoneOrder
{
    public partial class CustomorderOrderStatusLog : BaseEntity
    {
        public int OrderId { get; set; }
        public int StatusId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedOn { get; set; }
        public string InvoiceSendTo { get; set; }
        public decimal AmountPaid { get; set; }
        public int NotificationId { get; set; }
        public string PaymentResponse { get; set; }

        public string Comments { get; set; }
    }
}
