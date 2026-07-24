using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain
{
    public partial class ConfigurationProductPriceImport : BaseEntity
    {
        public int ProductId { get; set; }
        public int VariantId { get; set; }
        public decimal OlPrice { get; set; }
        public decimal Price { get; set; }

        public bool IsUpdated { get; set; }
    }
}
