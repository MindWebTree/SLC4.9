using Nop.Core;
using System;


namespace MWT.Nop.Core.Domain.FAQModule
{
    public class Faq : BaseEntity
    {
        public string EntityType { get; set; }
        public int EntityId { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public bool Deleted { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
    }

}
