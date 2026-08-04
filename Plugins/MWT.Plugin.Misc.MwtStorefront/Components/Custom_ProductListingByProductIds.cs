using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Service.Catalog;
using MWT.Plugin.Misc.MwtStorefront.Factories;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using Nop.Web.Infrastructure.Cache;

namespace MWT.Plugin.Misc.MwtStorefront.Components
{
    public class Custom_ProductListingByProductIdsViewComponent : NopViewComponent
    {
        private readonly ICustomProductModelFactory _productModelFactory;
        private ICustomProductService _productService;
        private readonly IStaticCacheManager _staticCacheManager;
        public Custom_ProductListingByProductIdsViewComponent(ICustomProductModelFactory productModelFactory,
                                            ICustomProductService productService, IStaticCacheManager staticCacheManager)
        {
            _productModelFactory = productModelFactory;
            _productService = productService;
            _staticCacheManager = staticCacheManager;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync(string productIds, string type)
        {
            if (string.IsNullOrEmpty(productIds) || productIds.Trim() == "")
                return Content("");

            string sectionId = "";
            List<int> _lstProductIds = new List<int>();

            foreach (var _productID in productIds.Split(','))
            {
                int.TryParse(_productID, out int productID);
                if (productID != 0)
                {
                    _lstProductIds.Add(productID);
                    sectionId = sectionId + productID.ToString();
                }
            }

            if (!_lstProductIds.Any())
                return Content("");



            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.ProductListingByProductIds,
                                 string.Join(',', _lstProductIds.ToArray()));

            var model = await this._staticCacheManager.GetAsync(cacheKey, async () =>
            {
                var products = await _productService.GetProductsByIdsAsync(_lstProductIds.ToArray());
                products = products.Where(p => p.Published && !p.Deleted
                && p.TotalInventory > 0 && p.ProductType == ProductType.SimpleProduct).Take(20).ToList();
                var model = (await _productModelFactory.PrepareCustomProductOverviewModelsAsync(products, true, true, null)).ToList();
                return model;
            });
            ViewBag.Type = type;
            ViewBag.sectionId = sectionId;
            return View(model);
        }
    }
}
