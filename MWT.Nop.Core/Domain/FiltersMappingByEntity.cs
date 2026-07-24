using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain
{
    public partial class FiltersMappingByEntity : BaseEntity
    {
        public string Filtertype { get; set; }
        public string EntityType { get; set; }
        public int EntityId { get; set; }
        public int FilterId { get; set; }
        public int DisplayOrder { get; set; }
        public bool Disabled { get; set; }
        public bool DisplayOnTop { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public int CreatedBy { get; set; }
        public int UpdatedBy { get; set; }

    }
}
