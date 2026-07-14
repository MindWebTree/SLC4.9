
using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Core.Events;
using Nop.Plugin.Misc.Redirect.Domain;
using Nop.Services.Caching;
using Nop.Services.Events;

namespace Nop.Plugin.Misc.Redirect.Infrastructure.Cache
{


    /// <summary>
    /// Model cache event consumer (used for caching of presentation layer models)
    /// </summary>
    public class ModelCacheEventConsumer :
        //tax rates
        IConsumer<EntityInsertedEvent<RedirectionRule>>,
        IConsumer<EntityUpdatedEvent<RedirectionRule>>,
        IConsumer<EntityDeletedEvent<RedirectionRule>>
    {
        #region Fields

        private readonly IStaticCacheManager _staticCacheManager;

        #endregion

        #region Ctor

        public ModelCacheEventConsumer(IStaticCacheManager staticCacheManager)
        {
            _staticCacheManager = staticCacheManager;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handle tax rate inserted event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityInsertedEvent<RedirectionRule> eventMessage)
        {
            await ClearCache();
        }

        /// <summary>
        /// Handle tax rate updated event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityUpdatedEvent<RedirectionRule> eventMessage)
        {
            await ClearCache();
        }

        /// <summary>
        /// Handle tax rate deleted event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityDeletedEvent<RedirectionRule> eventMessage)
        {
            await ClearCache();
        }

        private async Task ClearCache()
        {
            await _staticCacheManager.RemoveAsync(RedirectDefaults.RedirectionsRulesCacheKey);
        }



        #endregion
    }
}