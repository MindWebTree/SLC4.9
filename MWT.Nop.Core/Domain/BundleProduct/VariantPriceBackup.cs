using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Misc.ProductBundle.Domain
{
    public class VariantPriceBackup: BaseEntity
    { 
        public int VariantId { get; set; }
        public int ProductId { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public decimal? Msrp { get; set; }
        public DateTime BackupDateUtc { get; set; } 
    }
}
