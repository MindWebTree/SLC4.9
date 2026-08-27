


using iTextSharp.text.rtf.parser;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Catalog
{
    /// <summary>
    /// Represents a model of products that use the specification attribute
    /// </summary>
    public partial record SpecificationAttributeOptionProductModel : BaseNopEntityModel
    {
        #region Properties

        public int SpecificationAttributeOptionId { get; set; }

        public int ProductId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.SpecificationAttribute.UsedByProducts.Product")]
        public string ProductName { get; set; }
        public int DisplayOrder { get; set; }

       public string PictureThumbnailUrl { get; set; }

        public int MobileDisplayOrder { get; set; }

        #endregion
    }
}