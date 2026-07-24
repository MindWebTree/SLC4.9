using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain
{
    public class GroupedProductConfiguration : BaseEntity
    {
        public int ProductId { get; set; }
        public string Raw { get; set; }
        public int ProductAttributeOptionId { get; set; }
    }
}
