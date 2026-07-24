using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.Catalog
{
    public partial class ExportProductAttributeCombinationFormat
    {
        public int ProductId { get; set; }
        public string AttributeXml { get; set; }
        public string AttributeDescription { get; set; }
        public decimal? Msrp { get; set; }
        public decimal? Price { get; set; }
        public decimal? SalePrice { get; set; }
    }
}
