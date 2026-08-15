using Nop.Core.Domain.Configuration;
using Nop.Services.Caching;
using Nop.Services.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Configuration.Consumer
{
    public partial class SettingCacheEventConsumer : CacheEventConsumer<Setting>
    {
        private readonly ICustomerActivityService _customerActivityService;
        public SettingCacheEventConsumer(ICustomerActivityService customerActivityService)
        {
            _customerActivityService = customerActivityService;
        }
        /// <summary>
        /// Clear cache by entity event type
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <param name="entityEventType">Entity event type</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task ClearCacheAsync(Setting entity, EntityEventType entityEventType)
        {
            await _customerActivityService.InsertActivityAsync(entityEventType == EntityEventType.Insert ? "Insert Settings" :
                (entityEventType == EntityEventType.Delete ? "Deleted Setting" : "EditSettings"), entity.Name + " " + entity.Value);
        }
    }
}
