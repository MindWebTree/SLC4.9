using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Nop.Web.Areas.Admin.Models.Catalog
{
    /// <summary>
    /// Represents a specification attribute search model
    /// </summary>
    public partial record SpecificationAttributeSearchModel : BaseSearchModel
    {
        public int CategoryId { get; set; }
    }
}   