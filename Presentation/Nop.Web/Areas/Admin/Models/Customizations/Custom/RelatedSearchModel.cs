

using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Catalog
{
    /// <summary>
    /// Represents a related product model
    /// </summary>
    public partial record RelatedSearchModel : BaseNopEntityModel
    {
        #region Properties

        [NopResourceDisplayName("Admin.Catalog.Products.RelatedSearch.Fields.Product.Link")]
        public string Link { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.RelatedSearch.Fields.Product.TermName")]
        public string TermName { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.RelatedSearch.Fields.DisplayOrder.DisplayOrder")]
        public int DisplayOrder { get; set; }

        public string EntityType { get; set; }

        public int EntityId { get; set; }


        #endregion
    }
}