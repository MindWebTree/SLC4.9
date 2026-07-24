using Nop.Core.Domain.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product attribute mapping
    /// </summary>
    public partial class ProductAttributeMapping : BaseEntity, ILocalizedEntity
    {
        public bool IsExpanded { get; set; }

        public bool EnableHoverImpact { get; set; }
    }
}
