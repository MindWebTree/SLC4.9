using Nop.Core;
using System;

namespace MWT.Nop.Plugin.Misc.ProductBundle.Domain
{
    public class BundleConfiguration : BaseEntity
    {
        public int ProductId { get; set; }
        public string Name { get; set; }  
        public int VariantId { get; set; }
        public DateTime? CreatedOnUtc { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public DateTime? DeletedOnUtc { get; set; }
        public int DeletedBy { get; set; }
        public int NoOfPieces { get; set; }
        public int CreatedBy { get; set; }
        public int UpdatedBy { get; set; }
        public bool IsActive { get; set; }
        public int PictureId { get; set; }
        public bool DisplayOnProductPage { get; set; }
   
    }
}