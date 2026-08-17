using MWT.Nop.Core.Service.Catalog;
using Nop.Services.Catalog;
using Nop.Services.ScheduleTasks;

namespace MWT.Plugin.Misc.MwtStorefront.Tasks
{
    /// Represents a task for sending queued message 
    /// </summary>
    public partial class GroupProductTask : IScheduleTask
    {
        #region Fields

        private readonly IProductExtendedService _productService;

        #endregion

        #region Ctor

        public GroupProductTask(IProductExtendedService productService)
        {
            _productService = productService;
        }

        #endregion

        #region Methods
        public virtual async System.Threading.Tasks.Task ExecuteAsync()
        {
            var products = await _productService.SyncGroupedProductsPrice();
        }

        #endregion
    }
}