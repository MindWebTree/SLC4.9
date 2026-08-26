using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Nop.Web.Areas.Admin.Models.Catalog
{
    /// <summary>
    /// Represents a model of products that use the specification attribute
    /// </summary>
    public partial record SpecificationAttributeProductModel : BaseNopEntityModel
    {
        public string PictureThumbnailUrl { get; set; }
        public string OptionName { get; set; }

        public int SpecificationOptionId { get; set; }
    }
}
