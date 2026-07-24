using Nop.Core;
using System;


namespace MWT.Nop.Core.Domain
{

    /// <summary>
    /// Represents a related product
    /// </summary>
    public partial class CustomFormEntry : BaseEntity
    {
        public int FormId { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
    }
}
