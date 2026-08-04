
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models
{
    public partial record CustomFormEntryModel : BaseNopEntityModel
    {
        #region Ctor

        public CustomFormEntryModel()
        {

        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Admin.Catalog.CustomForm.Fields.Name")]
        public string FormName { get; set; }

        [NopResourceDisplayName("Admin.Catalog.CustomFormsEntry.Fields.CreatedOnUtc")]
        public string CreatedOnUtc { get; set; }

        [NopResourceDisplayName("Admin.Catalog.CustomFormsEntry.Fields.UpdatedOnUtc")]
        public string UpdatedOnUtc { get; set; }

        [NopResourceDisplayName("Admin.Catalog.CustomFormsEntry.Fields.Email")]
        public string Email { get; set; }

        [NopResourceDisplayName("Admin.Catalog.CustomFormsEntry.Fields.CustomerName")]
        public string CustomerName { get; set; }


        #endregion
    }
}
