

using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Nop.Web.Areas.Admin.Models.Catalog
{
    /// <summary>
    /// Represents a related product model
    /// </summary>
    public partial record QuickFilterModel : BaseNopEntityModel
    {
        #region Properties

        [NopResourceDisplayName("Admin.Catalog.Products.QuickFilter.Fields.Product.Link")]
        public string Link { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.QuickFilter.Fields.Product.TermName")]
        public string TermName { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.QuickFilter.Fields.DisplayOrder.DisplayOrder")]
        public int DisplayOrder { get; set; }

        public string EntityType { get; set; }

        public int EntityId { get; set; }
        [NopResourceDisplayName("Admin.Catalog.Products.QuickFilter.Fields.DisplayOrder.Picture")]
        [UIHint("Picture")]
        public int PictureId { get; set; }

        #endregion
    }
}