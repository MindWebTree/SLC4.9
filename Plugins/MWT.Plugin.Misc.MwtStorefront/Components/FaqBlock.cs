using Microsoft.AspNetCore.Mvc;
using Nop.Core.Caching;
using Nop.Web.Framework.Components;
using Nop.Web.Infrastructure.Cache;
using MWT.Plugin.Misc.MwtStorefront.Factories;



namespace MWT.Plugin.Misc.MwtStorefront.Components
{

    public class FaqBlockViewComponent : NopViewComponent
    {
        #region Fields

        private readonly IFaqModelFactory _faqModelFactory;
        private readonly IStaticCacheManager _staticCacheManager;

        #endregion

        #region Ctor

        public FaqBlockViewComponent(IFaqModelFactory faqModelFactory,
            IStaticCacheManager staticCacheManager
            )
        {
            _faqModelFactory = faqModelFactory;
            _staticCacheManager = staticCacheManager;
        }

        #endregion

        #region Methods
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync(int entityId, string entityType)
        {
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.FaqCacheKey,
                entityId, entityType);

            var terms = await this._staticCacheManager.GetAsync(cacheKey, async () =>
            {
                return await _faqModelFactory.PrepareFaqListModelAsync(entityId, entityType);
            });

            return View(terms);

        }

        #endregion
    }
}