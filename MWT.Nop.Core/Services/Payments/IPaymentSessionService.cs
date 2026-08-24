using MWT.Nop.Core.Domain.PaymentMethod;

namespace MWT.Nop.Core.Services.Payments
{
    public partial interface  IPaymentSessionService
    {
        Task<PaymentMethodSession> GetOrderSession(int orderId); 
    }
}
