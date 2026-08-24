using Microsoft.AspNetCore.Mvc;
using MWT.Plugin.Misc.MwtStorefront.Factories;
using MWT.Plugin.Misc.MwtStorefront.Infrastructure.Cache;
using Nop.Core.Caching;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Components
{
    public class CustomRelatedSearchTermsForSearchViewComponent : NopViewComponent
    {
        #region Fields
        private readonly IStaticCacheManager _staticCacheManager;


        private readonly IRelatedSearchModelFactory _relatedSearchModelFactory;

        #endregion

        #region Ctor

        public CustomRelatedSearchTermsForSearchViewComponent(IRelatedSearchModelFactory relatedSearchModelFactory,
            IStaticCacheManager staticCacheManager)
        {
            _relatedSearchModelFactory = relatedSearchModelFactory;
            _staticCacheManager = staticCacheManager;
        }

        #endregion

        #region Methods
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return Content("");

            var key = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.SearchResultProductsDefaultCacheKey,
                 searchTerm);
            var productIds = _staticCacheManager.Get(key, () =>
            {
                return new int[] { };
            });
            if (productIds.Length == 0)
            {
                key = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.SearchResultProductsAlternateCacheKey,
                 searchTerm);
                productIds = _staticCacheManager.Get(key, () =>
                {
                    return new int[] { };
                });
                if (productIds.Length == 0)
                {
                    key = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.SearchResultProductsSuggestedCacheKey,
                 searchTerm);
                    productIds = _staticCacheManager.Get(key, () =>
                    {
                        return new int[] { };
                    });
                }
            }
            return View(await _relatedSearchModelFactory.PrepareRelatedSearchListForSearchPageModelAsync(productIds, searchTerm));

        }

        #endregion
    }
}
