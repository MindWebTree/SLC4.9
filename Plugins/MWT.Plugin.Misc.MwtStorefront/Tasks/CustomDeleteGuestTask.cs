
using MWT.Nop.Core.Domain.Configuration;
using MWT.Nop.Core.Services.Customers;
using MWT.Nop.Core.Services.Message;
using Nop.Core;
using Nop.Core.Domain.Messages;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.ScheduleTasks;

namespace MWT.Plugin.Misc.MwtStorefront.Tasks
{
    public partial class CustomDeleteGuestTask : IScheduleTask
    {
        #region Fields

        protected readonly CustomSettings _customCustomerSettings;
        protected readonly ICustomerExtendedService _customerService;
        protected readonly IScheduleTaskService _scheduleTaskService;
        protected readonly ILogger _logger;
        protected readonly ICustomWorkflowMessageService _customWorkflowMessageService;
        protected readonly IWorkContext _workContext;
        #endregion

        #region Ctor

        public CustomDeleteGuestTask(CustomSettings customcustomerSettings,
            ICustomerExtendedService customerService, IScheduleTaskService scheduleTaskService,
            ILogger logger,
            ICustomWorkflowMessageService customWorkflowMessageService , IWorkContext workContext)
        {
            _customCustomerSettings = customcustomerSettings;
            _customerService = customerService;
            _scheduleTaskService = scheduleTaskService;
            _logger = logger; 
            _customWorkflowMessageService = customWorkflowMessageService;
            _workContext = workContext; 
        }

        #endregion

        #region Methods

        /// <summary>
        /// Executes a task
        /// </summary>
        public virtual async Task ExecuteAsync()
        {
            try
            {
                var currentTime = DateTime.Now.TimeOfDay;
                var startTime = TimeSpan.Zero;
                var endTime = new TimeSpan(4, 0, 0);

                if (currentTime >= startTime && currentTime <= endTime)
                {
                    var noOfCustomerTodelete = _customCustomerSettings.NumberOfGuestCustomersToDelete;
                    await _customerService.DeleteGuestCustomersAsync(true, noOfCustomerTodelete);
                }
            }
            catch (Exception ex)
            {
                // Logging the error 
                var message = $"Delete Guest Task failed and is auto-disabling. Error: {ex.Message}";
                await _logger.ErrorAsync(message, ex);

               
                // 3. Shoot email using the Custom Workflow Message Service and your new SQL template
                var emailErrorMessage = $"<b>Message:</b> {message}<br /><br /><b>Stack Trace:</b><br />{ex.StackTrace}";
                await _customWorkflowMessageService.SendScheduledTaskFailedNotificationAsync("Delete Guest Customers", emailErrorMessage, (await this._workContext.GetWorkingLanguageAsync()).Id);
            }
        }

        #endregion
    }
}
