

using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

namespace Nop.Web.Areas.Admin.Models.Customization.Custom
{
    /// <summary>
    /// Represents a related product model
    /// </summary>
    public partial record FiltersMappingByEntityModel : BaseNopEntityModel
    {
        public FiltersMappingByEntityModel()
        {
            filters = new List<SelectListItem>();
        }

        public List<SelectListItem> filters;

        [NopResourceDisplayName("Admin.Catalog.Entity.FiltersMappingByEntity.Fields.Disabled")]
        public bool Disabled { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Entity.FiltersMappingByEntity.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        public string EntityType { get; set; }

        public string Filtertype { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Entity.FiltersMappingByEntity.Fields.FilterId")]
        public int FilterId { get; set; }

        public int EntityId { get; set; }

        public string filterName { get; set; }

        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
    }
}