using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Shipping
{
    /// <summary>
    /// Represents a request for getting shipping rate options
    /// </summary>
    public partial class GetShippingOptionRequest
    {
        public decimal CustomSubtotal { get; set; }
        public bool UseCustomSubtotal { get; set; }
        public bool IsSurchargeApplicable { get; set; }
    }
}
