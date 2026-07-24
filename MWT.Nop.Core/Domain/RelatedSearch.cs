using Nop.Core;
using System;

namespace MWT.Nop.Core.Domain
{
    public partial class RelatedSearch : BaseEntity
    {
        public string EntityType { get; set; }

        public string TermName { get; set; }

        public string Link { get; set; }

        public int EntityId { get; set; }

        public int DisplayOrder { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public DateTime UpdatedOnUtc { get; set; }
    }
}
