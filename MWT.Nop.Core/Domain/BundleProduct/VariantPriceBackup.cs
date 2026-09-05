using Nop.Core;

namespace MWT.Nop.Core.Domain.ProductBundle
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
