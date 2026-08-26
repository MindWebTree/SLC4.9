using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Models.Catalog
{
    /// <summary>
    /// Represents a specification attribute model
    /// </summary>
    public partial record SpecificationAttributeModel : BaseNopEntityModel, ILocalizedModel<SpecificationAttributeLocalizedModel>
    {
        #region Properties

        [NopResourceDisplayName("Admin.Catalog.Entity.FiltersMappingByEntity.Fields.Disabled")]
        public bool Disabled { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Entity.FiltersMappingByEntity.Fields.DisplayOnTop")]
        public bool DisplayOnTop { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Entity.FiltersMappingByEntity.Fields.CategoryId")]
        public int CategoryId { get; set; }



        #endregion
    }
}