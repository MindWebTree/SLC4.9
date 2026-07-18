using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Payments.Affirm.Models
{
    public class AffirmItem
    {
        public string DisplayName { get; set; }
        public string Sku { get; set; }
        public double unit_price { get; set; }
        public int qty { get; set; }
    }
}
