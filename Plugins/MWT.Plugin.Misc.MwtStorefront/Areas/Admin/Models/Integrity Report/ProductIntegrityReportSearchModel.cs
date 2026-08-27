using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.Integrity_Report
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