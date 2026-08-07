
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Caching;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Catalog;

namespace Nop.Web.Components
{
    public class QuickFilterViewComponent : NopViewComponent
    {
        #region Fields

        private readonly IQuickFilterModelFactory _quickFilterModelFactory;
        private readonly IStaticCacheManager _staticCacheManager;

        #endregion

        #region Ctor

        public QuickFilterViewComponent(IQuickFilterModelFactory quickFilterModelFactory, IStaticCacheManager staticCacheManager)
        {
            _quickFilterModelFactory = quickFilterModelFactory;
            _staticCacheManager = staticCacheManager;
        }

        #endregion

        #region Methods
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync(int entityId, string entityType, string heading)
        {
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.QuickFilterCacheKey,
                entityId, entityType);

            var filters = await this._staticCacheManager.GetAsync(cacheKey, async () =>
            {
                return await _quickFilterModelFactory.PrepareQuickFilterListModelAsync(entityId, entityType);
            });
            QuickFilterListModel model = new QuickFilterListModel();
            model.Name = heading;
            model.Filters = filters;
            return View(model);

        }

        #endregion
    }
}

