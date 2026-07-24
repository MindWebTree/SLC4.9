using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.Catalog
{
    public partial class ExportProductSpecificationAttributeFormat
    {
        public int SpecificationAttributeOptionId { get; set; }
        public int ProductId { get; set; }
        public int DisplayOrder { get; set; }
        public string CategoryIds { get; set; }
        public int MobileDisplayOrder { get; set; }
    }
}
