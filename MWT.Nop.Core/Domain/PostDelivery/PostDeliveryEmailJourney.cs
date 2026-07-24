using Nop.Core;
using System;
namespace MWT.Nop.Core.Domain.PostDelivery
{
    public class PostDeliveryEmailJourney : BaseEntity
    {
        public int OrderId { get; set; }
        public DateTime OrderDateTime { get; set; }
        public int LastReminderId { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsActive { get; set; }
        public DateTime? DeactivatedOn { get; set; }
        public string DeactivatedRemarks { get; set; }

    }
}
