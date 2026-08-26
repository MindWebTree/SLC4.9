

using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.ComponentModel.DataAnnotations;

namespace Nop.Web.Areas.Admin.Models.Catalog
{
    /// <summary>
    /// Represents a related product model
    /// </summary>
    public partial record CategoryCollectionLinkModel : BaseNopEntityModel
    {
        #region Properties


        [NopResourceDisplayName("Admin.Catalog.Products.CategoryCollectionLink.Fields.Product.Title")]
        public string Title { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.CategoryCollectionLink.Fields.Product.Link")]
        public string Link { get; set; }

       [NopResourceDisplayName("Admin.Catalog.Products.CategoryCollectionLink.Fields.DisplayOrder.DisplayOrder")]
        public int DisplayOrder { get; set; }

        public int EntityId { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public DateTime UpdatedOnUtc { get; set; }


        #endregion
    }
}