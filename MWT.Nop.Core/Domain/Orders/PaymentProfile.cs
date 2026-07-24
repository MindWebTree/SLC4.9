using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.Orders
{
    public partial class PaymentProfile : BaseEntity
    {
        public int AddressId { get; set; }
        public int CustomerId { get; set; }
        public Int64 AuthorizeNetProfileId { get; set; }
        public string ExpirationMonth { get; set; }
        public string ExpirationYear { get; set; }
        public string CardType { get; set; }
        public DateTime UpdatedOn { get; set; }
        public DateTime CreatedOn { get; set; }
        public int CustomOrderNo { get; set; }
    }

}
