using MWT.Plugin.Shipping.FixedByWeightByTotal.Domain;
using System.Threading.Tasks;

namespace MWT.Plugin.Shipping.FixedByWeightByTotal.Services
{
    public interface IMWTEstimationDeliveryDateNotificationService
    {
        Task Insert(MWTEstimationDeliveryDateNotification obj);
        Task<bool> CheckSpamActivity(string zipcode);
    }
}
