using Nop.Core.Domain.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Catalog
{
    public partial class SpecificationAttributeOption : BaseEntity, ILocalizedEntity
    {
        public decimal? LowerLimit { get; set; }
        public decimal? HigherLimit { get; set; }
    }
}
