using Nop.Web.Framework.Models;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.Integrity_Report
{
    /// <summary>
    /// Represents a category list model
    /// </summary>
    public partial record ProductIntegrityReportModel : BaseNopModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PictureThumbnailUrl { get; set; }
        public string ErrorReason { get; set; }
    }
}