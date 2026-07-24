using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product picture mapping
    /// </summary>
    public partial class ProductPicture : BaseEntity
    {
        public bool? DisplayOnListingModules { get; set; }
        public bool? DisplayOnCategoryPage { get; set; }
        public bool? HideOnProductPage { get; set; }
    }
}
