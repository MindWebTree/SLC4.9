using MWT.Nop.Core.Services.MailChimp;
using Nop.Core.Domain.Orders;
using Nop.Services.ScheduleTasks;

namespace MWT.Plugin.Misc.MwtStorefront.Tasks
{


    public partial class QueuedMailChimpCartSignUpProcessTask : IScheduleTask
    {

        #region Fields

        private readonly IQueuedMailChimpCartSignUpService _queuedMailChimpCartSignUpService;
        private readonly IMailchimpService _mailchimpService;

        #endregion

        #region ctor

        public QueuedMailChimpCartSignUpProcessTask(IQueuedMailChimpCartSignUpService queuedMailChimpCartSignUpService,
            IMailchimpService mailchimpService)
        {
            this._queuedMailChimpCartSignUpService = queuedMailChimpCartSignUpService;
            this._mailchimpService = mailchimpService;
        }

        #endregion

        public virtual async System.Threading.Tasks.Task ExecuteAsync()
        {
            var items = await this._queuedMailChimpCartSignUpService.ListAsync();
            foreach (var item in items)
            {
                List<string> infoTags = new List<string>();
                infoTags.Add(item.ShoppingCartType == ShoppingCartType.ShoppingCart ? "Add to cart" : "Wishlist");
                infoTags.Add("Product ID " + item.ProductId);
                var isProcessed = await this._mailchimpService.CartOperation(item.Email, item.FirstName, "", 
                    infoTags, item.Url, item.UserAgent, item.ProductId.ToString(), item.IpAddress);
                item.IsProcessed = isProcessed;
                item.NoOfTries = item.NoOfTries + 1;
                item.ProcessedOn = isProcessed ? DateTime.UtcNow : null;
                await _queuedMailChimpCartSignUpService.UpdateAsync(item);
            }

        }


    }
}

