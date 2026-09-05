using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Models.Catalog
{
    /// <summary>
    /// Represents a predefined product attribute value model
    /// </summary>
    public partial record ProductAttributeValueModel : BaseNopEntityModel, ILocalizedModel<ProductAttributeValueLocalizedModel>
    {
        public int VariantId { get; set; }
        public string VariantTitle { get; set; }

        public string Dimension { get; set; }

        public string VariantDimension { get; set; }
        public string ManufacturerPartNumber { get; set; }

        public bool Published { get; set; }
       
        public string QueryParameter { get; set; } 

        public int FeaturedPictureId { get; set; }   
    }
}
