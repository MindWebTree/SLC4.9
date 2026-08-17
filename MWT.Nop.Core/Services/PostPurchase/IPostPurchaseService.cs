using MWT.Nop.Core.Domain.PostPurchase;
using Nop.Core.Domain.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.PostPurchase
{
    public partial interface IPostPurchaseService
    {
        Task InsertPostPurchaseEmailJourney(Order order);
        Task<IList<PostPurchaseEmailJourneyReminder>> GetAllEmailReminders();
        Task<IList<PostPurchaseEmailJourney>> GetRecordsForFirstPurchaseReminder(int reminderId, int days);
        Task<IList<PostPurchaseEmailJourney>> GetPurchaseJournalRecordsForReminder(int reminderId, int days);
        Task SendReminderEmail(PostPurchaseEmailJourney emailjourney, PostPurchaseEmailJourneyReminder reminders);

    }
}
