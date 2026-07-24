using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.AbandonedCart
{
    public class AbandonedCart : BaseEntity
    {
        public int Id { get; set; }
        public string OrderNotes { get; set; }
        public bool? IsOrderConfirmed { get; set; }
        public int? OrderNumber { get; set; }
        public string Privatenotes { get; set; }
        public DateTime? CreatedOn { get; set; }

        public int CustomerId { get; set; }
        public Guid Guid { get; set; }

        public bool IsCompleted { get; set; }
        public string SalesForceReferenceId { get; set; }
    }
}
