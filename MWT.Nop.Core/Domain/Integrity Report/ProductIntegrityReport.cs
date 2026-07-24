using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.Custom.Integrity_Report
{
    public partial class ProductIntegrityReport : BaseEntity
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ErrorReason { get; set; }

    }
}
