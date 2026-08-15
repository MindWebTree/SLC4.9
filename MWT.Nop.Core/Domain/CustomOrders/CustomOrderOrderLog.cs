using System;


namespace MWT.Nop.Core.Domain.CustomOrders
{
    public partial class CustomOrderOrderLog
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public int RevertOption { get; set; }
        public DateTime? CreatedOn { get; set; }
    }
}
