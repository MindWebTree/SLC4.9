namespace MWT.Nop.Core.Services.PostDelivery
{
    public partial class PostDeliveryQueueEmail
    {
        public int OrderId { get; set; }
        public DateTime ReminderDate { get; set; }
        public int ReminderNumber { get; set; }
        public string DeactivatedRemarks { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
    }
}
