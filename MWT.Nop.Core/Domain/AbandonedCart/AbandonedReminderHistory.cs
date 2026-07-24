using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.AbandonedCart
{
    public partial class AbandonedReminderHistory : BaseEntity
    {
        public int AbandonedCartInvoiceID { get; set; }
        public int ReminderNumber { get; set; }
        public DateTime ReminderDate { get; set; }
        public bool Isdeleted { get; set; }
    }
}
