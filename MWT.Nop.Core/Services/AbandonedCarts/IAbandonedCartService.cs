using MWT.Nop.Core.Domain.AbandonedCarts;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.AbandonedCarts
{
    public interface IAbandonedCartService
    {
        Task<IList<AbandonedCartItem>> SyncAbandonedCartItems();
        Task<IList<AbandonedCartItem>> SyncSalesForceAbandonedCartItems(DateTime startDate);
        Task DeleteItem(int shoppingCartRecid);
        Task MarkItemAsOld(int shoppingCartRecid);
        Task<AbandonedCart> GetAbandonedInvoiceByGuid(Guid guid);
        Task MarkAbandonedInvoiceAsPaid(int customerId, int[] shoppingCartRecIds, int ordernumber, decimal total);
        Task<List<AbandonedCart>> AbandonedCarts();
        Task AbandonedCardForCustomerOnPaymentFail(Customer customer);

        #region V3 Version
        Task SyncAbandonedCarts(DateTime startDate);
        Task<IList<AbandonedCartReminderSchedule>> GetAbandonedCartReminderSchedules();
        Task<IList<AbandonedReminder>> GetAbandonedReminders();
        Task InsertAbandonedReminderHistory(AbandonedReminderHistory reminder, bool trackReminder = true);
        Task SendAbandonedCartReminder(AbandonedReminder reminder, Customer customer, string name, string email, string phone, int messageTemplateId, IList<ShoppingCartItem> cart, string utmSource);
        #endregion
    }
}
