using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product attribute combination
    /// </summary>
    public partial class ProductAttributeCombination : BaseEntity
    {
        public decimal? OverriddenOldPrice { get; set; }
        public decimal? OverriddenMsrp { get; set; }
    }
}
