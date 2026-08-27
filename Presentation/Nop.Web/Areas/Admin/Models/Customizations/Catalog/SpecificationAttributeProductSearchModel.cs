using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Models.Catalog
{
    public partial record SpecificationAttributeProductSearchModel : BaseSearchModel
    {
        #region ctor
        public SpecificationAttributeProductSearchModel()
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
