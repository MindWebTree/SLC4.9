using Nop.Core.Domain.Orders;
using Nop.Services.Orders;
using Nop.Services.Tax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Orders
{
    public partial interface IOrderExtendedService : IOrderService
    {
        Task<(int nofOfOrders, decimal amount)> GetCustomerStats(int customerId);
        Task<Order> GetRecentOrderOfCustomer(int customerId);
        Task<List<int>> GetNewOrdersIds();
        Task<Order> GetOrderByGuidAsync(string orderGuid);
        List<TaxInfo> GetTaxDetails(Order order);
        List<TaxInfo> GetTaxDetails(string taxInfo);
        Task<(decimal wgsCharges, decimal defaultWgsCharges, decimal surchargeAmount)> GetWgsCharges(Order order, decimal subTotal, bool isSurchargeApplicable = false);
        Task<bool> IsSurchargeApplicable(Order order);
        Task<List<int>> GetLatestDeliveredOrderIdByCustomerEmail(string email);
        Task<Order> GetOrderByTransactionId(string transactionId, string paymentMethod);
        #region Post Purchase Journey
        Task<IList<Order>> GetLast10DaysOrders();
        Task<IList<Order>> GetShippedOrdersForLastNDays(int day);
        #endregion
    }
}
