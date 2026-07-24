using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.AbandonedCart
{
    public partial class AbandonedCartReminderSchedule : BaseEntity
    {
        public int Number { get; set; }
        public decimal Hours { get; set; }
        public bool Published { get; set; }
        public int MessageTemplateId { get; set; }

        public string UtmSource { get; set; }
    }
}
