using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.Zoho
{
    public partial class QueuedZohoCustomer:BaseEntity
    {
        public int CustomerId { get; set; }
        public int NoOfTries { get; set; }
        public bool IsProcessed { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public DateTime? ProcessedOn { get; set; }
        public string ZohoReferenceId { get; set; }
    }
}
