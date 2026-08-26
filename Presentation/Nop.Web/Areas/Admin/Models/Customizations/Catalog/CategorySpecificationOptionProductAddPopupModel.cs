using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.Catalog
{
    /// <summary>
    /// Represents a related product model to add to the product
    /// </summary>
    public partial record CategorySpecificationOptionProductAddPopupModel : BaseNopEntityModel
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Sku { get; set; }
        public bool Published { get; set; }
        public int DisplayOrder { get; set; }
        public string PictureThumbnailUrl { get; set; }
        public bool Selected { get; set; }
    }
}