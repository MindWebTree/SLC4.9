using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product picture mapping
    /// </summary>
    public partial class LogProductPicture : BaseEntity
    {
        /// <summary>
        /// Gets or sets the product identifier
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the picture identifier
        /// </summary>
        public int PictureId { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }

        public bool? DisplayOnListingModules { get; set; }
        public bool? DisplayOnCategoryPage { get; set; }
        public bool? HideOnProductPage { get; set; }

        public int CustomerId { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Action { get; set; }

        public int ReferenceId { get; set; }
    }
}
