using MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.Integrity_Report;

namespace MWT.Plugin.Misc.MwtStorefront.Area.Admin.Factories.Integrity_Report
{
    public partial interface IIntegrityReportModelFactory
    {

        Task<ProductIntegrityReportListModel> PrepareProductIntegrityReportSearchListModelAsync(ProductIntegrityReportSearchModel searchModel);

    }
}