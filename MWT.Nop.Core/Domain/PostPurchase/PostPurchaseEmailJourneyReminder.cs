using Nop.Core;

namespace MWT.Nop.Core.Domain.PostPurchase
{
    public class PostPurchaseEmailJourneyReminder : BaseEntity
    {
        public int ReminderDays { get; set; }
        public int ReminderNo { get; set; }
        public int MessageTemplateId { get; set; }
        public bool IsDefault { get; set; }
        public string EmailType { get; set; }
        public string UtmParameters { get; set; }
    }
}