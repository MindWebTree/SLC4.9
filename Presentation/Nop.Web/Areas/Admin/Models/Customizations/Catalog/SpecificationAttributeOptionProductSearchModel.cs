using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

namespace Nop.Web.Areas.Admin.Models.Catalog
{
    public partial record SpecificationAttributeOptionProductSearchModel : BaseSearchModel
    {

        #region ctor
        public SpecificationAttributeOptionProductSearchModel()
        {
            AvailableCategories = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        public int SpecificationAttributeOptionId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.List.Search")]
        public string Search { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.List.SearchCategory")]
        public int SearchCategoryId { get; set; }

        public IList<SelectListItem> AvailableCategories { get; set; }


        #endregion


    }
}
