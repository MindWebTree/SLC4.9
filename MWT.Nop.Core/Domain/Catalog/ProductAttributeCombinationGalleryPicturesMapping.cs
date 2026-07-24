using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.Catalog
{
    public partial class ProductAttributeCombinationGalleryPicturesMapping : BaseEntity
    {
        public int PictureId { get; set; }
        public int ProductAttributeCombinationId { get; set; }
        public int DisplayOrder { get; set; }
    }
}