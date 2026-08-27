using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Models.Catalog
{
    /// <summary>
    /// Represents a copy product model
    /// </summary>
    public partial record CopyProductModel : BaseNopEntityModel
    {

        [NopResourceDisplayName("Admin.Catalog.Products.Copy.Sku")]
        public string Sku { get; set; }
    }
}
