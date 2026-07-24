using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MWT.Nop.Core.Domain
{
    /// <summary>
    /// Represents a related product
    /// </summary>
    public partial class CategoryCollectionLink : BaseEntity
    {
        /// <summary>
        /// Gets or sets the first product identifier
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the second product identifier
        /// </summary>
        public string Link { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        /// 
        public int EntityId { get; set; }
        public int DisplayOrder { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public DateTime UpdatedOnUtc { get; set; }
    }
}
