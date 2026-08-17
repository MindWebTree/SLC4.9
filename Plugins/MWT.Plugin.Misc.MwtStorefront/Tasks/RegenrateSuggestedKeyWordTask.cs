using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Services.Catalog;
using Nop.Core.Caching;
using Nop.Services.Catalog;
using Nop.Services.ScheduleTasks;

namespace MWT.Plugin.Misc.MwtStorefront.Tasks
{
    public partial class RegenrateSuggestedKeyWordTask : IScheduleTask
    {
        #region Fields

        private readonly ISuggestedKeywordsService _suggestedKeywordsService;
        private readonly IProductExtendedService _productService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IScheduleTaskService _scheduleTaskService;
        #endregion

        #region Ctor

        public RegenrateSuggestedKeyWordTask(ISuggestedKeywordsService suggestedKeywordsService,
            IProductExtendedService productService, IStaticCacheManager staticCacheManager, IScheduleTaskService scheduleTaskService)
        {
            _suggestedKeywordsService = suggestedKeywordsService;
            _productService = productService;
            _staticCacheManager = staticCacheManager;
            _scheduleTaskService = scheduleTaskService;
        }

        #endregion
        public async System.Threading.Tasks.Task ExecuteAsync()
        {
            var products = await _productService.GetAllProducts();
            foreach (var prd in products)
            {
                await _suggestedKeywordsService.RegenrateSuggestedKeyWords(prd.Id);
            }
            #region  reset cache
            await _staticCacheManager.ClearAsync();

            #endregion

            var scheduleTask = await _scheduleTaskService.GetTaskByTypeAsync("Nop.Web.Customizations.Tasks.ElasticSearchTask");
            if (scheduleTask != null)
            {
                scheduleTask.LastSuccessUtc = scheduleTask.LastSuccessUtc.HasValue ?
                System.DateTime.Now.AddHours(-12).AddMinutes(10) : null;
                scheduleTask.LastStartUtc = scheduleTask.LastStartUtc.HasValue ? System.DateTime.Now.AddHours(-12).AddMinutes(10) : null;
                scheduleTask.LastEndUtc = scheduleTask.LastEndUtc.HasValue ? System.DateTime.Now.AddHours(-12).AddMinutes(10) : null;
                await _scheduleTaskService.UpdateTaskAsync(scheduleTask);
            }

        }
    }
}
