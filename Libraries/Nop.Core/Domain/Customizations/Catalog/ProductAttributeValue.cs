using Nop.Core.Domain.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Catalog
{
    public partial class ProductAttributeValue : BaseEntity, ILocalizedEntity
    {
        public int VariantId { get; set; }
        public string VariantTitle { get; set; }
        public string ManufacturerPartNumber { get; set; }
        public string QueryParameter { get; set; }
        public string Dimension { get; set; }
        public string VariantDimension { get; set; }
        public bool Published { get; set; }
        public int FeaturedPictureId { get; set; }

    }
}
