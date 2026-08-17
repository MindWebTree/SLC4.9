using MWT.Nop.Core.Service.Catalog;
using Nop.Services.Catalog;
using Nop.Services.ScheduleTasks;

namespace MWT.Plugin.Misc.MwtStorefront.Tasks
{
    public partial class VariantIdRegenrateTask: IScheduleTask
    {
        #region Fields

        private readonly IProductExtendedService _productService;

        #endregion

        #region Ctor

        public VariantIdRegenrateTask(IProductExtendedService productService)
        {
            _productService = productService;
        }


        #endregion

        #region Methods

        public async System.Threading.Tasks.Task ExecuteAsync()
        {
            int pageIndex = 0;
            int pageSize = 50;
            bool hasNextPage = false;
            do
            {
                var result = await this._productService.SearchProductsAsync(pageIndex: pageIndex, pageSize: pageSize, showHidden: true);
                hasNextPage = result.HasNextPage;

                foreach (var product in result)
                {
                    await this._productService.GenerateVariantCombinations(product.Id);
                }
                pageIndex++;
            } while (hasNextPage);
        }

        #endregion
    }
}
