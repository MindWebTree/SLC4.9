using Nop.Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Customers
{
    /// <summary>
    /// Represents a customer
    /// </summary>
    public partial class Customer : BaseEntity, ISoftDeletedEntity
    {
        public bool IsImported { get; set; }
        public Int64 CIM_ProfileId { get; set; }
    }
}

