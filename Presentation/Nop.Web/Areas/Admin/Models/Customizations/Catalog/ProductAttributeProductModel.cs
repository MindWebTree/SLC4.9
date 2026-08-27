using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Models.Catalog
{
    /// <summary>
    /// Represents a model of products that use the product attribute
    /// </summary>
    public partial record ProductAttributeProductModel : BaseNopEntityModel
    {
        public int ProductId { get; set; }
        public string PictureThumbnailUrl { get; set; }
    }
}
