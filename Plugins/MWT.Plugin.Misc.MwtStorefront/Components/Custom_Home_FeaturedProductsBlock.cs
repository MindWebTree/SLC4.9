using Microsoft.AspNetCore.Mvc;
using MWT.Plugin.Misc.MwtStorefront.Factories;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Services.Catalog;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework.Components;
using Nop.Web.Infrastructure.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Components
{
    public class Custom_Home_FeaturedProductsBlockViewComponent : NopViewComponent
    {
        private readonly IAclService _aclService;
        private readonly ICustomProductModelFactory _productModelFactory;
        private readonly IProductService _productService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreContext _storeWorkContext;

        public Custom_Home_FeaturedProductsBlockViewComponent(IAclService aclService,
            ICustomProductModelFactory productModelFactory,
            IProductService productService,
            IStoreMappingService storeMappingService,
            IStaticCacheManager staticCacheManager,
            IStoreContext storeWorkContext)
        {
            _aclService = aclService;
            _productModelFactory = productModelFactory;
            _productService = productService;
            _storeMappingService = storeMappingService;
            _staticCacheManager = staticCacheManager;
            _storeWorkContext = storeWorkContext;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync(int? productThumbPictureSize)
        {

            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.HomePageFeaturedProductsCacheKey,
                  (await _storeWorkContext.GetCurrentStoreAsync())?.Id);
            var model = await this._staticCacheManager.GetAsync(cacheKey, async () =>
            {
                var products = await (await _productService.GetAllProductsDisplayedOnHomepageAsync())
            //ACL and store mapping
            .WhereAwait(async p => await _aclService.AuthorizeAsync(p) && await _storeMappingService.AuthorizeAsync(p))
            //availability dates
            .Where(p => _productService.ProductIsAvailable(p))
            //visible individually
            .Where(p => p.VisibleIndividually).ToListAsync();
                var model = (await _productModelFactory.PrepareCustomProductOverviewModelsAsync(products,
                productThumbPictureSize: productThumbPictureSize, forceRedirectionAfterAddingToCart: true))
                .ToList();
                return model;
            });
            if (model.Count == 0)
                return Content("");
            return View(model);
        }
    }
}
