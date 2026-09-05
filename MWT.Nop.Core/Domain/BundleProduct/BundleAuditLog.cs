using Nop.Core;
namespace MWT.Nop.Core.Domain.ProductBundle 
{

    public class BundleAuditLog : BaseEntity
    {
        public int BundleId { get; set; }

        public int ProductId { get; set; }

        public int? VariantId { get; set; } 
        
        public string ActionType { get; set; }

        public int UserIdentifier { get; set; }

        public decimal? OldPrice { get; set; }

        public decimal? NewPrice { get; set; }

        public bool IsValid { get; set; }

        public int RequiredPieces { get; set; }

        public int ActualQuantity { get; set; }

        public string LogMessage { get; set; }

        public DateTime CreatedOnUtc { get; set; }
    }
}
