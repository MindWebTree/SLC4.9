using iTextSharp.text;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Areas.Admin.Factories.Customization;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

namespace Nop.Web.Areas.Admin.Models.Customization.Utilities
{
    public partial record ProductNotesManagementModel : BaseNopEntityModel
    {

        public ProductNotesManagementModel()
        {
            EntityTypes = new List<SelectListItem>();

        }

        public List<SelectListItem> EntityTypes { get; set; }


        [NopResourceDisplayName("Admin.Utilities.Notes.EntityType")]
        public string EntityType { get; set; }

        [NopResourceDisplayName("Admin.Utilities.Notes.Productids")]
        public string ProductIds { get; set; }
        [NopResourceDisplayName("Admin.Utilities.Notes.Notes")]
        public string Notes { get; set; }

        [NopResourceDisplayName("Admin.Utilities.Notes.CategoryIds")]
        public string CategoryIds { get; set; }
    }
}
