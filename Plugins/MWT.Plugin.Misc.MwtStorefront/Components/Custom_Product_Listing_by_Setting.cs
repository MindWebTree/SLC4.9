
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Catalog;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using System.Linq;
using System.Collections.Generic;
using Nop.Core.Caching;
using Nop.Core;
using Nop.Web.Infrastructure.Cache;
using Nop.Core.Domain.Catalog;
using MWT.Nop.Core.Service.Catalog;
using MWT.Plugin.Misc.MwtStorefront.Factories;

namespace MWT.Plugin.Misc.MwtStorefront.Components
{
    public class Custom_Product_Listing_by_SettingViewComponent : NopViewComponent
    {
        private readonly ICustomProductModelFactory _customproductModelFactory;
        private IProductExtendedService _customproductService;
        private readonly IStaticCacheManager _staticCacheManager;
        public Custom_Product_Listing_by_SettingViewComponent(ICustomProductModelFactory customproductModelFactory,
                                          IProductExtendedService customproductService, IStaticCacheManager staticCacheManager)
        {
            _customproductModelFactory = customproductModelFactory;
            _customproductService = customproductService;
            _staticCacheManager = staticCacheManager;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync(string productIds, string heading,string sectionId= "featuredproducts-home", int noOfProductsToDisplay = 4, ProductType type = ProductType.SimpleProduct)
        {
            if (string.IsNullOrEmpty(productIds) || productIds.Trim()=="")
                return Content("");

            List<int> _lstProductIds = new List<int>();

            foreach (var _productID in productIds.Split(','))
            {
                int.TryParse(_productID, out int productID);
                if (productID != 0)
                    _lstProductIds.Add(productID);
            }

            if (!_lstProductIds.Any())
                return Content("");



            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.ProductListingByProductIds,
                                 string.Join(',', _lstProductIds.ToArray()));

            var model = await this._staticCacheManager.GetAsync(cacheKey, async () =>
                 {
                     var products = await _customproductService.GetProductsByIdsAsync(_lstProductIds.ToArray());
                     products = products.Where(p => p.Published && !p.Deleted
                     && p.TotalInventory > 0 && p.ProductType == type).Take(noOfProductsToDisplay).ToList();
                     var model = (await _customproductModelFactory.PrepareCustomProductOverviewModelsAsync(products, true, true, null)).ToList();
                     return model;
                 });

            ViewBag.Heading = heading;
            ViewBag.SectionId = sectionId;
            ViewBag.Type = type;
            return View(model);
        }
    }
}
