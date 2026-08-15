
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Service.Catalog;
using MWT.Plugin.Misc.MwtStorefront.Factories;
using MWT.Plugin.Misc.MwtStorefront.Models.Catalog;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using Nop.Web.Infrastructure.Cache;

namespace Nop.Web.Components
{
    public class Custom_CrossSellProductsBlockViewComponent : NopViewComponent
    {
        private readonly IAclService _aclService;
        private readonly ICustomProductModelFactory _productModelFactory;
        private readonly IProductExtendedService _productService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IWorkContext _workContext;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly IStaticCacheManager _staticCacheManager;

        public Custom_CrossSellProductsBlockViewComponent(IAclService aclService,
            ICustomProductModelFactory productModelFactory,
            IProductExtendedService productService,
            IShoppingCartService shoppingCartService,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            IWorkContext workContext,
            ShoppingCartSettings shoppingCartSettings,
            IStaticCacheManager staticCacheManager)
        {
            _aclService = aclService;
            _productModelFactory = productModelFactory;
            _productService = productService;
            _shoppingCartService = shoppingCartService;
            _storeContext = storeContext;
            _storeMappingService = storeMappingService;
            _workContext = workContext;
            _shoppingCartSettings = shoppingCartSettings;
            this._staticCacheManager = staticCacheManager;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync(int productId, int? productThumbPictureSize)
        {
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.CrossSellProductsCacheKey,
    productId);

            var model = await this._staticCacheManager.GetAsync(cacheKey, async () =>
            {
                List<CustomProductOverviewModel> model = new List<CustomProductOverviewModel>();
                var crossProducts = await _productService.GetCrossSellProductsByProductId1Async(productId);
                if (crossProducts.Any())

                {

                    var products = (await _productService.GetProductsByIdsAsync(crossProducts.Select(m => m.ProductId2).ToArray())).Where(p=>p.TotalInventory>0);
                    model = (await _productModelFactory.PrepareCustomProductOverviewModelsAsync(products,
                           productThumbPictureSize: productThumbPictureSize, forceRedirectionAfterAddingToCart: true))
                       .ToList();
                }
                return model;
            });
            if (model.Count == 0)
                return Content("");
            return View(model);
        }
    }
}