using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Nop.Web.Areas.Admin.Models.Catalog
{
    /// <summary>
    /// Represents a product attribute mapping model
    /// </summary>
    public partial record ProductAttributeMappingModel : BaseNopEntityModel, ILocalizedModel<ProductAttributeMappingLocalizedModel>
    {
        [NopResourceDisplayName("Admin.Catalog.Products.ProductAttributes.Attributes.Fields.IsExpanded")]
        public bool IsExpanded { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.ProductAttributes.Attributes.Fields.EnableHoverImpact")]
        public bool EnableHoverImpact { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.ProductAttributes.Attributes.Fields.Published")]
        public bool Published { get; set; }
    }
}
