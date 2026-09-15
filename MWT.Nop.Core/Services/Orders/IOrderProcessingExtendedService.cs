using MWT.Nop.Core.Domain.CustomOrders;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Orders;
using Nop.Services.Payments;

namespace MWT.Nop.Core.Services.Orders
{
    public partial interface IOrderProcessingExtendedService : IOrderProcessingService
    {
        Task CustomSendNotificationsAndSaveNotesAsync(Order order);
        Task<(PlaceOrderResult result, ProcessPaymentResult request)> CustomPlaceOrderAsync(ProcessPaymentRequest processPaymentRequest, CustomOrder customOrder,int paidBy, dynamic orderSummary, bool saveOrderDetails = true, int refOrderno = 0, bool chargeFromInitialaOrder = false);
        Task SetProcessPaymentRequestAsync(ProcessPaymentRequest processPaymentRequest, Customer customer, bool useNewOrderGuid = false);
        Task<ProcessPaymentRequest> GetProcessPaymentRequestAsync(Customer customer);

    }
}
