using MWT.Nop.Core.Services.Catalog;
using Nop.Services.ScheduleTasks;

namespace MWT.Plugin.Misc.MwtStorefront.Tasks
{
    /// <summary>
    /// Scheduled task for automated product tag management
    /// Runs daily via NopCommerce task scheduler
    /// Delegates all logic to NewArrivalTagService and BestsellerTagService
    /// </summary>
    public class ProductTagAutomationTask : IScheduleTask
    {
        #region Fields

        private readonly INewArrivalTagService _newArrivalTagService;
        private readonly IBestsellerTagService _bestsellerTagService;
        private readonly IBestsellerPoolService _bestsellerPoolService;
        #endregion

        #region Ctor

        public ProductTagAutomationTask(
            INewArrivalTagService newArrivalTagService,
            IBestsellerTagService bestsellerTagService, IBestsellerPoolService bestsellerPoolService)
        {
            _newArrivalTagService = newArrivalTagService;
            _bestsellerTagService = bestsellerTagService;
            _bestsellerPoolService = bestsellerPoolService;
        }

        #endregion

        #region IScheduleTask

        /// <summary>
        /// Execute the scheduled task
        /// </summary>
        public async System.Threading.Tasks.Task ExecuteAsync()
        {   
            // Process New Arrival tags then recalculate queue
            await _newArrivalTagService.ProcessTagsAsync();
    

            // Process Bestseller tags then recalculate queue
            await _bestsellerTagService.ProcessTagsAsync();
            await _newArrivalTagService.RecalculateQueueAsync();
            await _bestsellerTagService.RecalculateQueueAsync();
            await _bestsellerPoolService.RebuildPoolAsync();
        }

        #endregion
    }
}
