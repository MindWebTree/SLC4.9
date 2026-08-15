using Nop.Core;
using System;
namespace MWT.Nop.Core.Domain.CustomOrders
{
    public partial class CustomOrderNotesLog : BaseEntity
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string SpecialInstructions { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
