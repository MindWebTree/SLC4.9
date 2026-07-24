using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.Security
{
    public partial class CategoryUserMapping:BaseEntity
    {
        public int UserId { get; set; }
        public int CategoryId { get; set; }

    }
}
