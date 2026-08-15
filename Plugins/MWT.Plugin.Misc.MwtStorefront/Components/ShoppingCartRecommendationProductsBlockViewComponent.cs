using Microsoft.AspNetCore.Mvc;
using MWT.Plugin.Misc.MwtStorefront.Factories;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Orders;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Components
{
    public class ShoppingCartRecommendationProductsBlockViewComponent : NopViewComponent
    {
        private readonly IProductService _productService;
        private readonly ICustomProductModelFactory _productModelFactory;
        private readonly IStoreContext _storeContext;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly CatalogSettings _catalogSettings;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IWorkContext _workContext;
        private readonly ISettingService _settingService;
        private readonly IRecentlyViewedProductsService _recentlyViewedProductsService;
        public ShoppingCartRecommendationProductsBlockViewComponent(IProductService productService,
        ICustomProductModelFactory productModelFactory,
        IStoreContext storeContext,
        IStaticCacheManager staticCacheManager,
        CatalogSettings catalogSettings,
        IShoppingCartService shoppingCartService,
        IWorkContext workContext,
        ISettingService settingService,
        IRecentlyViewedProductsService recentlyViewedProductsService
        )
        {
            _productService = productService;
            _productModelFactory = productModelFactory;
            _storeContext = storeContext;
            _staticCacheManager = staticCacheManager;
            _catalogSettings = catalogSettings;
            _shoppingCartService = shoppingCartService;
            _workContext = workContext;
            _settingService = settingService;
            _recentlyViewedProductsService = recentlyViewedProductsService;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

            if (cart.Count == 0)
                return Content("");
            int numberOfRecommendedProducts = 10;
            try
            {
                numberOfRecommendedProducts = await _settingService.GetSettingByKeyAsync<int>("CatalogSettings.NumberOfShoppingCartRecommendedProducts");
            }
            catch
            {
            }
            if (numberOfRecommendedProducts == 0)
            {
                return Content("");
            }
            ViewBag.Heading = "ShoppingCartRecommendedProducts.CrossSell";
            List<Product> recommendedProducts = new List<Product>();
            var productIds = cart.OrderByDescending(o => o.UpdatedOnUtc).Select(m => m.ProductId).ToArray();
            foreach (var productId in productIds)
            {
                var crossSellProductIds = (await this._productService.GetCrossSellProductsByProductId1Async(productId)).Select(x => x.ProductId2).ToArray().Distinct().Take(4);

                foreach (var crossSellProductId in crossSellProductIds)
                {
                    if (!recommendedProducts.Where(p => p.Id == crossSellProductId).Any())
                    {
                        if (recommendedProducts.Count >= numberOfRecommendedProducts)
                            break;

                        recommendedProducts.Add(await _productService.GetProductByIdAsync(crossSellProductId));
                    }
                }
                if (recommendedProducts.Count >= numberOfRecommendedProducts)
                    break;
            }
            if (recommendedProducts.Count == 0)
            {
                ViewBag.Heading = "ShoppingCartRecommendedProducts.Wishlist";
                cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.Wishlist, (await _storeContext.GetCurrentStoreAsync()).Id);
                recommendedProducts.AddRange(await _productService.GetProductsByIdsAsync(cart.OrderByDescending(o => o.UpdatedOnUtc).Select(m => m.ProductId).Distinct().ToArray()));
            }

            if (recommendedProducts.Count == 0)
            {
                ViewBag.Heading = "ShoppingCartRecommendedProducts.RecentlyViewed";
                recommendedProducts = (await _recentlyViewedProductsService.GetRecentlyViewedProductsAsync(numberOfRecommendedProducts)).ToList();
            }


            var model = (await _productModelFactory.PrepareCustomProductOverviewModelsAsync(recommendedProducts,
                   productThumbPictureSize: null, forceRedirectionAfterAddingToCart: true))
                  .ToList();

            return View(model);
        }
    }
}
