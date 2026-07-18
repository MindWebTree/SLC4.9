using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Payments.Affirm.Models
{
    
    public class Event
    {
        public String id;
        public String type;
        public String currency;
        public DateTime created;
        public int amount;
    }

    public class AuthorizeCaptureModel
    {
        public DateTime authorization_expiration;
        public int amount_refunded;
        public String status;
        public String order_id;
        public String id;
        public int provider_id;
        public String currency;
        public DateTime created;
        public List<Event> events;
        public bool remove_tax;
        public String checkout_id;
        public int amount;
    }


}


