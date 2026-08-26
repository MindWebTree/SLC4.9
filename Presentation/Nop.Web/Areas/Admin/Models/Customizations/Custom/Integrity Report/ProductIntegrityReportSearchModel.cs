using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Customization.Custom.Integrity_Report
{
    /// <summary>
    /// Represents a category search model
    /// </summary>
    public partial record ProductIntegrityReportSearchModel : BaseSearchModel
    {
        #region Properties

        [NopResourceDisplayName("Admin.Integrity.Report.List.Name")]
        public string Name { get; set; }

        #endregion
    }
}