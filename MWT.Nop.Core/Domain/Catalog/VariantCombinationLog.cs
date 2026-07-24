using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.Catalog
{
    public class VariantCombinationLog : BaseEntity
    {
        public int VariantId { get; set; }
        public int ProductId { get; set; }
        public string Combination { get; set; }
        public decimal? OldPrice { get; set; }
        public decimal Price { get; set; }
        public decimal? Msrp { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string ProductAttributeValueIds { get; set; }
    }
}
