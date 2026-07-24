using Nop.Core.Domain.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Catalog
{
    public partial class ProductSpecificationAttribute : BaseEntity, ILocalizedEntity
    {
        /// <summary>
        /// Gets or sets the Mobile display order
        /// </summary>
        public int MobileDisplayOrder { get; set; }
    }
}
