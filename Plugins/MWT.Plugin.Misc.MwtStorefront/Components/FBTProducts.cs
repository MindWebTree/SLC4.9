
using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Service.Catalog;
using MWT.Plugin.Misc.MwtStorefront.Factories;
using MWT.Plugin.Misc.MwtStorefront.Models.Catalog;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using Nop.Web.Infrastructure.Cache;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Nop.Web.Components
{
    public class FBTProductsViewComponent : NopViewComponent
    {
        private readonly ICustomProductModelFactory _productModelFactory;
        private readonly IProductExtendedService _productService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly IStaticCacheManager _staticCacheManager;
        public FBTProductsViewComponent(ICustomProductModelFactory productModelFactory,
                                                  IProductExtendedService productService,
                                                  IStoreContext storeContext,
                                                  ISettingService settingService,
                                                  IStaticCacheManager staticCacheManager)
        {
            _productService = productService;
            _productModelFactory = productModelFactory;
            _storeContext = storeContext;
            _settingService = settingService;
            _staticCacheManager = staticCacheManager;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync(int productId, int? productThumbPictureSize)
        {
            var noOfProductsToShow = await _settingService.GetSettingByKeyAsync<int>("FBTViewedProductsNumber");
            if (noOfProductsToShow == 0)
                return Content("");
            var model = await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache
                (CustomNopModelCacheDefaults.FBTProductsCacheKey, productId), async () =>
                {
                    List<CustomProductDetailsModel> model = new List<CustomProductDetailsModel>();
                    var FBTProducts = await _productService.GetFBTProductsByProductId1Async(productId, false, false);
                    if (FBTProducts.Count == 0)
                        return model;


                    var productsToShow = (noOfProductsToShow < FBTProducts.Count) ? FBTProducts.Take(noOfProductsToShow).ToList() : FBTProducts.ToList(); // make a copy to avoid modifying cached list
                    var productIds = productsToShow.Select(m => m.ProductId2).ToList();
                    productIds.Insert(0, productId);

                    var products = await _productService.GetProductsByIdsAsync(productIds.Select(p => p).Distinct().ToArray());

                    var mainproduct = products.Where(p => p.Id == productId).FirstOrDefault();
                    if (mainproduct != null)
                    {
                        products.Remove(mainproduct);
                        products.Insert(0, mainproduct);
                    }
                    foreach (var _product in products)
                    {
                        var product = await _productModelFactory.PrepareCustomProductDetailsModelAsync(_product, null, true);
                        product.AddToCart.EnteredQuantity = productsToShow.Where(m => m.ProductId2 == product.Id).FirstOrDefault()?.DefaultQuantity ?? 1;
                        model.Add(product);
                    }
                    return model;
                });
            return View(model);
        }
    }
}