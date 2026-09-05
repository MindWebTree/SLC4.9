using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Models.Catalog
{
    /// <summary>
    /// Represents a product attribute combination model
    /// </summary>
    public partial record ProductAttributeCombinationModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.Catalog.Products.ProductAttributes.AttributeCombinations.Fields.OverriddenOldPrice")]
        [UIHint("DecimalNullable")]
        public decimal? OverriddenOldPrice { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.ProductAttributes.AttributeCombinations.Fields.OverriddenMsrp")]
        [UIHint("DecimalNullable")]
        public decimal? OverriddenMsrp { get; set; } 
    }
}
