using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Orders
{
    public class CustomOrderStatusLogModel
    {
        public string Orderid { get; set; }
        public string CustomerName { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Status { get; set; }
        public string EmailStatus { get; set; }
        public string InvoiceSendTo { get; set; }
        public string TransactionInfo { get; set; }
        public string Comments { get; set; }
    }
}
