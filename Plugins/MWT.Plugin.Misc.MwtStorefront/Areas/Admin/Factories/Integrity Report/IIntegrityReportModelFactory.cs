using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customization.Custom;
using Nop.Web.Areas.Admin.Models.Customization.Custom;
using Nop.Web.Areas.Admin.Models.Customization.Custom.Integrity_Report;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Factories.Customization.Integrity_Report
{
    public partial interface IIntegrityReportModelFactory
    {

        Task<ProductIntegrityReportListModel> PrepareProductIntegrityReportSearchListModelAsync(ProductIntegrityReportSearchModel searchModel);

    }
}