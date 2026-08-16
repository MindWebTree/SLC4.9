using MWT.Nop.Core.Domain.Orders;
using Nop.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Payments
{
    public partial class PaymentProfileService : IPaymentProfileService
    {
        #region Fields

        private readonly IRepository<PaymentProfile> _paymentProfileRepository;

        #endregion

        #region Ctor

        public PaymentProfileService(IRepository<PaymentProfile> paymentProfileRepository)
        {
            this._paymentProfileRepository = paymentProfileRepository;
        }

        #endregion

        #region Methods
        public async Task<PaymentProfile> GetPaymentPrpfileOfOrder(int customOrderNo)
        {
            return await this._paymentProfileRepository.Table.Where(p => p.CustomOrderNo == customOrderNo).FirstOrDefaultAsync();
        }

        public async Task SavePaymentProfile(PaymentProfile profile)
        {
            await this._paymentProfileRepository.InsertAsync(profile);
        }

        #endregion
    }
}
