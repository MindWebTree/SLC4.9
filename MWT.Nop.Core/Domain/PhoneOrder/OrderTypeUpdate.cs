using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.PhoneOrder
{
    public enum OrderTypeUpdate
    {
        Discount = 1,
        Wgs = 2,
        Shipping = 3,
        Notes = 4,
        StatusUpdate=5,
        PartialOrderPaymentLink=6,
        TaxUpdate=7
    }
}
