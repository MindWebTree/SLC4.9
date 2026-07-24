using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.PhoneOrder
{
    public enum OrderStatus
    {
        Paid,
        InvoiceSent,
        OrderSaved,
        SavedDraft,
        NotInterested
    }
}
