using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Catalog
{
    public partial record SpecificationAttributeOptionModel : BaseNopEntityModel, ILocalizedModel<SpecificationAttributeOptionLocalizedModel>
    {
        [NopResourceDisplayName("Admin.Catalog.Entity.FiltersMappingByEntity.Fields.Disabled")]
        public bool Disabled { get; set; }
        public int CategoryId { get; set; }
        public SpecificationAttributeOptionProductSearchModel SpecificationAttributeOptionProductSearchModel { get; set; }
    }
}
