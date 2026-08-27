using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Catalog
{
    /// <summary>
    /// Represents a related product search model to add to the product
    /// </summary>
    public partial record CategorySpecificationOptionProductSearchModel : BaseSearchModel
    {
        public CategorySpecificationOptionProductSearchModel()
        {
            SelectedProductIds = new List<int>();
            ProductIds = new List<int>();
        }
        public int SpecificationAttributeOptionId { get; set; }
        public IList<int> SelectedProductIds { get; set; }

        public IList<int> ProductIds { get; set; }
        public int CategoryId { get; set; }
    }
}