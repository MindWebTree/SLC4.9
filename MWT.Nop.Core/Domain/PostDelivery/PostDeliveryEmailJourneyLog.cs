using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.PostDelivery
{
    public class PostDeliveryEmailJourneyLog : BaseEntity
    {
        public int PostDeliveryEmailJourneyId { get; set; }
        public int ReminderId { get; set; }
        public string ReminderHtml { get; set; }
        public DateTime SentOn { get; set; }
        public string Comments { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
