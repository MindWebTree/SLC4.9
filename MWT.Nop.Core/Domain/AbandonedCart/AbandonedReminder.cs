using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Customization.AbandonedCart
{
    public partial  class AbandonedReminder:BaseEntity
    {
        public int CustomerId { get; set; }
        public int ReminderNumber { get; set; }

        public Guid Guid { get; set; }
        public DateTime LastProcessingDate { get; set; }
    }
}
