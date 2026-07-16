using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Shipping.FixedByWeightByTotal.Domain
{
    public partial class MWTShippingZone : BaseEntity
    {
        public int StoreId { get; set; }
        public string Name { get; set; }
        public string ZipCodes { get; set; }
    
    }
}
