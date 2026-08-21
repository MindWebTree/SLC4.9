using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Orders
{
    public class CustomOrderNotesLogModel
    {
        public string Orderid { get; set; }
        public string CustomerName { get; set; }

        public DateTime CreatedOn { get; set; }
        public string Notes { get; set; }
    }
}
