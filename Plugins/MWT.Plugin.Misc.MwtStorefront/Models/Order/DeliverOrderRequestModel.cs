using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Models.Order
{
    public class DeliverOrderRequestModel
    {
        public int OrderId { get; set; }
        public DateTime DeliveredDate { get; set; }
        public string TrackingNumber { get; set; }
    }
}
