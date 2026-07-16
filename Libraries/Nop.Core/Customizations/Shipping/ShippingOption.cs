using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Shipping
{
    /// <summary>
    /// Represents a shipping option
    /// </summary>
    public partial class ShippingOption
    {
        public decimal AdditionalFee { get; set; }
        public string CustomShippingMethodDescription { get; set; }
        public decimal SurchargeAmount { get; set; }
        public decimal DefaultAmount { get; set; }
    }
}
