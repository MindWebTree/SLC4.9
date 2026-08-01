using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Customization.Catalog
{
    public class ProductVariant : BaseEntity
    {
        public int ProductId { get; set; }
        public int ProductAttributeId { get; set; }
        public int ProductAttributeValueId { get; set; }
        public string ProductAttributeValueIds { get; set; }
        public string AttributeValue { get; set; }
        public int VariantId { get; set; }
    }
}
