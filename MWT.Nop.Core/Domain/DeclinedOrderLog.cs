using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain
{
    public partial class DeclinedOrderLog : BaseEntity
    {
        public int CustomerId { get; set; }

        public string PaymentMethod { get;set; }
        public string Email { get; set; }
        public string Message { get; set; }
        public DateTime CreatedOn { get; set; }
        public decimal OrderTotal { get; set; }
        public string ShoppingCartIds { get; set; }
    }
}
