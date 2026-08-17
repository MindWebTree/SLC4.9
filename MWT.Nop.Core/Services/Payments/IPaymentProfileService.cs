using MWT.Nop.Core.Domain.Orders;

namespace MWT.Nop.Core.Services.Payments
{
    public partial interface IPaymentProfileService
    {
        Task SavePaymentProfile(PaymentProfile profile);
        Task<PaymentProfile> GetPaymentPrpfileOfOrder(int customOrderNo);
    }
}
