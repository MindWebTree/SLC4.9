using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain
{
    public partial class CategorySuggestedKeyword : BaseEntity
    {
        public int CategoryId { get; set; }
        public string KeyWord { get; set; }
   
    }
}
