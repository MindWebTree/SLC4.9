using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Payments
{

    public partial class ProcessPaymentRequest
    {
        public bool CreateCim { get; set; }
        public int CustomOrderNumber { get; set; }
        public int? ShippingAddressId { get; set; }
        public int? BillingAddressId { get; set; }
    }
}