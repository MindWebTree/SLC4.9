using System;
using Nop.Core;
namespace MWT.Plugin.Shipping.FixedByWeightByTotal.Domain
{
    public class MWTEstimationDeliveryDateNotification : BaseEntity
    {
        public string ZipCode { get; set; }
        public string IpAddress { get; set; }
        public DateTime CreatedOn { get; set; }

    }
}
