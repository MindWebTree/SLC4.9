using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

namespace Nop.Web.Areas.Admin.Models.Catalog
{
    /// <summary>
    /// Represents a search model of products that use the product attribute
    /// </summary>
    public partial record ProductAttributeProductSearchModel : BaseSearchModel
    {
        #region ctor
        public ProductAttributeProductSearchModel()
        {
            AvailableCategories = new List<SelectListItem>();
        }

        #endregion

        #region Properties


        [NopResourceDisplayName("Admin.Catalog.Products.List.Search")]
        public string Search { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.List.SearchCategory")]
        public int SearchCategoryId { get; set; }

        public IList<SelectListItem> AvailableCategories { get; set; }

        #endregion
    }
}
