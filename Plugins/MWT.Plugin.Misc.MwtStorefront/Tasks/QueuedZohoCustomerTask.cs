using MWT.Nop.Core.Service.Zoho;
using Nop.Services.ScheduleTasks;

namespace MWT.Plugin.Misc.MwtStorefront.Tasks
{
    public partial class QueuedZohoCustomerTask : IScheduleTask
    {
        #region Fields

        private readonly IZohoService _zohoService;


        #endregion


        #region Ctor

        public QueuedZohoCustomerTask(IZohoService zohoService)
        {
            _zohoService = zohoService;
        }

        #endregion
        public virtual async System.Threading.Tasks.Task ExecuteAsync()
        {
            #region  Customer Sync

            var pendingCustomers = await _zohoService.QueuedZohoCustomerListAsync();

            foreach (var customer in pendingCustomers)
            {
                await _zohoService.SaveContact(customer);
            }

            #endregion
        }
    }
}
