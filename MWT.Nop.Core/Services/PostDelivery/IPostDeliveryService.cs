using MWT.Nop.Core.Domain.PostDelivery;
using Nop.Core.Domain.Orders;

namespace MWT.Nop.Core.Services.PostDelivery
{
    public partial interface IPostDeliveryService
    {
        Task InsertPostDeliveryEmailJourney(Order order);
        Task<IList<PostDeliveryEmailJourneyReminder>> GetAllEmailReminders();
        Task<IList<PostDeliveryEmailJourney>> GetRecordsForFirstPurchaseReminder(int reminderId, int days);
        Task<IList<PostDeliveryEmailJourney>> GetPurchaseJournalRecordsForReminder(int reminderId, int days);
        Task SendReminderEmail(PostDeliveryEmailJourney emailjourney, PostDeliveryEmailJourneyReminder reminders);
        Task<List<PostDeliveryQueueEmail>> GetPostDeliveryQueueEmailList();
        Task<PostDeliveryEmailJourney> GetPostPurchaseEmailJourneyByOrderId(int orderId);
        Task UpdatePostPurchaseEmailJourney(PostDeliveryEmailJourney postDelivery);

    }
}
