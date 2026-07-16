using MWT.Plugin.Shipping.FixedByWeightByTotal.Domain;
//using Nop.Core.Domain.Customization.Custom;
using Nop.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Shipping.FixedByWeightByTotal.Services
{
    public class MWTEstimationDeliveryDateNotificationService : IMWTEstimationDeliveryDateNotificationService
    {
        #region Fields
        private IRepository<MWTEstimationDeliveryDateNotification> _mwtEstimationDeliveryDateRepository;
        #endregion
        public MWTEstimationDeliveryDateNotificationService(
            IRepository<MWTEstimationDeliveryDateNotification> mwtEstimationDeliveryDateRepository)
        {
            this._mwtEstimationDeliveryDateRepository = mwtEstimationDeliveryDateRepository;
        }
        #region Methods
        public async Task<bool> CheckSpamActivity(string zipcode)
        {
            return await this._mwtEstimationDeliveryDateRepository.Table.Where(es => es.ZipCode == zipcode && es.CreatedOn >= DateTime.UtcNow.AddHours(-24))
                .AnyAsync();
        }

        public async Task Insert(MWTEstimationDeliveryDateNotification obj)
        {
            await this._mwtEstimationDeliveryDateRepository.InsertAsync(obj);
        }
        #endregion
    }
}
