using MWT.Nop.Core.Services.Integrity_Report;
using MWT.Plugin.Misc.MwtStorefront.Factories;
using Nop.Services.Catalog;
using Nop.Services.ScheduleTasks;

namespace MWT.Plugin.Misc.MwtStorefront.Tasks
{
    public class ProductIntegrityReportTask : IScheduleTask
    {
        #region fields

        private readonly IProductService _productService;
        private readonly IProductIntegrityReportModelFactory _productIntegrityReportModelFactory;
        private readonly IProductIntegrityReportService _productIntegrityReportService;

        #endregion

        #region Ctor

        public ProductIntegrityReportTask(IProductService productService,
            IProductIntegrityReportModelFactory productIntegrityReportModelFactory,
            IProductIntegrityReportService productIntegrityReportService)
        {
            this._productService = productService;
            this._productIntegrityReportModelFactory = productIntegrityReportModelFactory;
            this._productIntegrityReportService = productIntegrityReportService;


        }

        #endregion
        public async System.Threading.Tasks.Task ExecuteAsync()
        {
            int pageIndex = 0;
            int pageSize = 50;
            bool hasNextPage = false;
            List<int> totalProductIds = new List<int>();
            do
            {
                var result = await this._productService.SearchProductsAsync(pageIndex: pageIndex, pageSize: pageSize, showHidden: false);

                hasNextPage = result.HasNextPage;
                var products = result.ToList();

                totalProductIds.AddRange(await this._productIntegrityReportModelFactory.GenerateProductIntegrityReportAsync(products));

                pageIndex++;
            } while (hasNextPage);

            await _productIntegrityReportService.DeleteAllExceptIdsAsync(totalProductIds);

        }
    }
}