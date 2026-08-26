using Microsoft.AspNetCore.Mvc;
using Nop.Services.Catalog;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using System.Threading.Tasks;
using Nop.Web.Areas.Admin.Factories.Customization.Integrity_Report;
using Nop.Web.Areas.Admin.Models.Customization.Custom.Integrity_Report;


namespace Nop.Web.Areas.Admin.Controllers.Customizations
{
    public class IntegrityReportController : BaseAdminController
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly IIntegrityReportModelFactory _productIntegrityReportModelFactory;
        private readonly IPermissionService _permissionService;

        #endregion
        public IntegrityReportController(IProductService productService,
          IPermissionService permissionService,
          IIntegrityReportModelFactory productIntegrityReportModelFactory)
        {
            this._productService = productService;
            this._permissionService = permissionService;
            this._productIntegrityReportModelFactory = productIntegrityReportModelFactory;
        }
        public virtual async Task<IActionResult> Product()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return await AccessDeniedDataTablesJson();
            var searchModel = new ProductIntegrityReportSearchModel();
            searchModel.SetGridPageSize();
            return View(searchModel);
        }
        [HttpPost]
        public virtual async Task<IActionResult> Product(ProductIntegrityReportSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return await AccessDeniedDataTablesJson();

            var model = await _productIntegrityReportModelFactory.PrepareProductIntegrityReportSearchListModelAsync(searchModel);

            return Json(model);
        }
    }
}