using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Tax
{
    public partial class TaxTotalRequest
    {
        public bool IsCustomorder { get; set; }
        public decimal Total { get; set; }
        public string ZipCode { get; set; }
        public decimal ShippingCharges { get; set; }
    }
}
