using MWT.Nop.Core.Domain.PaymentMethod;
using Nop.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Payments
{
    public partial class PaymentSessionService : IPaymentSessionService
    {
        #region Fields

        private readonly IRepository<PaymentMethodSession> _paymentMethodSessionrepository;

        #endregion


        #region  Ctor

        public PaymentSessionService(IRepository<PaymentMethodSession> paymentMethodSessionrepository)
        {

            _paymentMethodSessionrepository = paymentMethodSessionrepository;
        }
        #endregion
        public async Task<PaymentMethodSession> GetOrderSession(int orderId)
        {
            return await (from _paymentMethodSession in _paymentMethodSessionrepository.Table
                          where _paymentMethodSession.LiveOrderNumber == orderId
                          select _paymentMethodSession
                                                    ).FirstOrDefaultAsync();
        }
    }
}
