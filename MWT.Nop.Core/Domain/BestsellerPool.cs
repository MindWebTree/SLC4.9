using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain
{
    public class BestsellerPool : BaseEntity
    {
        public int ProductId { get; set; }
        public int CategoryId { get; set; }
        public int DisplayOrder { get; set; }
    }
}
