using Microsoft.AspNetCore.Mvc;
using MWT.Plugin.Misc.MwtStorefront.Factories.Catalog;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using Nop.Web.Infrastructure.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Components
{
    public class Custom_TopSellerViewComponent : NopViewComponent
    {
        private readonly ICustomCatalogModelFactory _catalogModelFactory;
        private readonly ICategoryService _categoryService;
        private readonly ISettingService _settingService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreContext _storeContext;
        public Custom_TopSellerViewComponent(ICustomCatalogModelFactory catalogModelFactory,
                                            ICategoryService categoryService,
                                            ISettingService settingService, IStaticCacheManager staticCacheManager
            , IStoreContext storeContext)
        {
            _catalogModelFactory = catalogModelFactory;
            _categoryService = categoryService;
            _settingService = settingService;
            _staticCacheManager = staticCacheManager;
            _storeContext = storeContext;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            int noOfTopSellerProductsByCategory = 10;
            try
            {
                noOfTopSellerProductsByCategory = await _settingService.GetSettingByKeyAsync<int>("CatalogSettings.NoOfTopSellerProducts");
            }
            catch
            {
            }
            Models.Catalog.CustomCatalogProductsCommand command = new Models.Catalog.CustomCatalogProductsCommand();
            command.PageNumber = 1;
            command.PageSize = noOfTopSellerProductsByCategory;
            command.OrderBy = (int)ProductSortingEnum.BestSeller;

            var topsellers = (await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKey(CustomNopModelCacheDefaults.TopsellerCacheKey,
            await _storeContext.GetCurrentStoreAsync()),
               async () => await _catalogModelFactory.PrepareCustomCategoryProductsModelAsync(null, command, "", 0, true)
               ));
            return View(topsellers);
        }
    }
}
