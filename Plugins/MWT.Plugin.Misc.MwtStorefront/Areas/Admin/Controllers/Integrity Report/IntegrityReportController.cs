using Microsoft.AspNetCore.Mvc;
using MWT.Plugin.Misc.MwtStorefront.Area.Admin.Factories.Integrity_Report;
using MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.Integrity_Report;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Framework.Mvc.Filters;


namespace MWT.Plugin.Misc.MwtStorefront.Area.Admin.Controllers.Integrity_Report
{
    public class IntegrityReportController : BaseAdminController
    {
        #region Fields 

        private readonly IIntegrityReportModelFactory _productIntegrityReportModelFactory;

        #endregion

        public IntegrityReportController(
          IIntegrityReportModelFactory productIntegrityReportModelFactory)
        {
            _productIntegrityReportModelFactory = productIntegrityReportModelFactory;
        }

        [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> Product()
        {
            var searchModel = new ProductIntegrityReportSearchModel();
            searchModel.SetGridPageSize();
            return View(searchModel);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> Product(ProductIntegrityReportSearchModel searchModel)
        {
            var model = await _productIntegrityReportModelFactory.PrepareProductIntegrityReportSearchListModelAsync(searchModel);

            return Json(model);
        }
    }
}