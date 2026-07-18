using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Payments.Affirm.Models
{
    public class AffirmWebhookPayloadModel
    {
        public string OrderId { get; set; }
        public string Status { get; set; }
        public string TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string CustomerEmail { get; set; }

        public string webhook_session_id { get; set; }
        public int Amount_Financed { get; set; }
        public string Checkout_Token { get; set; }

        public string Event{ get;set; }
    }
}
