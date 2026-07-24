using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product category mapping
    /// </summary>
    public partial class ProductCategory : BaseEntity
    {
        public int MobileDisplayOrder { get; set; }
    }
}
