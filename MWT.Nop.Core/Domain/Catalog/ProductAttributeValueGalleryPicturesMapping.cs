using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.Catalog
{
  public partial  class ProductAttributeValueGalleryPicturesMapping:BaseEntity
    {
        public int PictureId { get; set; }
        public int ProductAttributeValueId { get; set; }
        public int DisplayOrder { get; set; }
    }
}
