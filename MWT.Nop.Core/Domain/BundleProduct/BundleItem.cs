using Nop.Core;
using System;

namespace MWT.Nop.Plugin.Misc.ProductBundle.Domain
{
    public class BundleItem : BaseEntity
    {
        public int BundleId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public int VariantId { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedOnUtc { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public DateTime? DeletedOnUtc { get; set; }
        public int DeletedBy { get; set; }
        public int CreatedBy { get; set; }
        public int UpdatedBy { get; set; }
    }
}