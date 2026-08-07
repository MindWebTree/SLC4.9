
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MWT.Plugin.Misc.MwtStorefront.Factories;
using Nop.Core.Caching;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using Nop.Web.Infrastructure.Cache;

namespace MWT.Plugin.Misc.MwtStorefront.Components
{
    public class RelatedSearchTermsViewComponent : NopViewComponent
    {
        #region Fields

        private readonly IRelatedSearchModelFactory _relatedSearchModelFactory;
        private readonly IStaticCacheManager _staticCacheManager;

        #endregion

        #region Ctor

        public RelatedSearchTermsViewComponent(IRelatedSearchModelFactory relatedSearchModelFactory,
            IStaticCacheManager staticCacheManager
            )
        {
            _relatedSearchModelFactory = relatedSearchModelFactory;
            _staticCacheManager = staticCacheManager;
        }

        #endregion

        #region Methods
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync(int entityId, string entityType)
        {
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.RelatedSearchTermCacheKey,
                entityId, entityType);

            var terms = await this._staticCacheManager.GetAsync(cacheKey, async () =>
            {
                return await _relatedSearchModelFactory.PrepareRelatedSearchListModelAsync(entityId, entityType);
            });
            
            return View(terms);

        }

        #endregion
    }
}

