using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain
{
    public partial class ProductSuggestedKeyword:BaseEntity
    {
        public int SuggestedKeyWordID { get; set; }
        public int ProductId { get; set; }

        public bool IsCustom { get; set; }
    }
}
