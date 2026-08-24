using Microsoft.AspNetCore.Mvc;
using Nop.Core.Caching;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Components
{
    public class NewArrivalProductsViewComponent : NopViewComponent
    {
        private readonly IAclService _aclService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductService _productService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly ISettingService _settingService;

        public NewArrivalProductsViewComponent(IAclService aclService,
            IProductModelFactory productModelFactory,
            IProductService productService,
            IStoreMappingService storeMappingService,
            IStaticCacheManager staticCacheManager,
            ISettingService settingService)
        {
            _aclService = aclService;
            _productModelFactory = productModelFactory;
            _productService = productService;
            _storeMappingService = storeMappingService;
            _staticCacheManager = staticCacheManager;
            _settingService = settingService;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync(int? productThumbPictureSize, string heading)
        {
            int numberOfNewArrival = 10;
            try
            {
                numberOfNewArrival = await _settingService.GetSettingByKeyAsync<int>("CatalogSettings.NumberOfNewArrival");
            }
            catch
            {

            }
            //load and cache report
            var newArrivals = await (await _staticCacheManager.GetAsync(NopModelCacheDefaults.NewArrivalCacheKey,
                async () => await (await _productService.GetNewArrivalProductsAsync(0,
                    pageSize: numberOfNewArrival)).ToListAsync())).WhereAwait(async p => await _aclService.AuthorizeAsync(p)
                    && await _storeMappingService.AuthorizeAsync(p))
            //availability dates
            .Where(p => _productService.ProductIsAvailable(p))
            //visible individually
            .Where(p => p.VisibleIndividually).ToListAsync();

            if (!newArrivals.Any())
                return Content("");

            var model = (await _productModelFactory.PrepareCustomProductOverviewModelsAsync(newArrivals, true, true, productThumbPictureSize)).ToList();
            ViewBag.Heading = heading;
            return View(model);
        }
    }
}
