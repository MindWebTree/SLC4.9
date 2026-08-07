using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Service.Catalog;
using MWT.Plugin.Misc.MwtStorefront.Factories;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Orders;
using Nop.Web.Areas.Admin.Models.Reports;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using Nop.Web.Infrastructure.Cache;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System.Linq;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Components
{
    public class RecommendationProductsBlockViewComponent : NopViewComponent
    {
        private readonly ICustomProductService _productService;
        private readonly ICustomProductModelFactory _productModelFactory;
        private readonly IStoreContext _storeContext;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly CatalogSettings _catalogSettings;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IWorkContext _workContext;
        private readonly ISettingService _settingService;
        public RecommendationProductsBlockViewComponent(ICustomProductService productService,
        ICustomProductModelFactory productModelFactory,
        IStoreContext storeContext,
        IStaticCacheManager staticCacheManager,
        CatalogSettings catalogSettings,
        IShoppingCartService shoppingCartService,
        IWorkContext workContext,
        ISettingService settingService)
        {
            _productService = productService;
            _productModelFactory = productModelFactory;
            _storeContext = storeContext;
            _staticCacheManager = staticCacheManager;
            _catalogSettings = catalogSettings;
            _shoppingCartService = shoppingCartService;
            _workContext = workContext;
            _settingService = settingService;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.Wishlist, (await _storeContext.GetCurrentStoreAsync()).Id);

            if (cart.Count == 0)
                return Content("");
            int numberOfRecommendedProducts = 10;
            try
            {
                numberOfRecommendedProducts = await _settingService.GetSettingByKeyAsync<int>("CatalogSettings.NumberOfRecommendedProducts");
            }
            catch
            {
            }
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.RecommendationProductsCacheKey,
             (await _workContext.GetCurrentCustomerAsync())?.Id, numberOfRecommendedProducts);

            var model = await this._staticCacheManager.GetAsync(cacheKey, async () =>
            {
                var products = await _productService.GetRecommendationProducts(cart.OrderByDescending(o => o.UpdatedOnUtc).Select(m => m.ProductId).ToArray(), numberOfRecommendedProducts);

                var _model = (await _productModelFactory.PrepareCustomProductOverviewModelsAsync(products,
                           productThumbPictureSize: null, forceRedirectionAfterAddingToCart: true))
                          .ToList();
                return _model;
            });

            return View(model);
        }
    }
}
