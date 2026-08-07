
using Microsoft.AspNetCore.Mvc;
using MWT.Plugin.Misc.MwtStorefront.Factories.Catalog;
using MWT.Plugin.Misc.MwtStorefront.Models.Catalog;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Orders;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using Nop.Web.Infrastructure.Cache;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Components
{
    public class TopSellingProductsByCategoryViewComponent : NopViewComponent
    {
        private readonly ICustomCatalogModelFactory _catalogModelFactory;
        private readonly ICategoryService _categoryService;
        private readonly ISettingService _settingService;
        private readonly IStaticCacheManager _staticCacheManager;
        public TopSellingProductsByCategoryViewComponent(ICustomCatalogModelFactory catalogModelFactory,
                                            ICategoryService categoryService,
                                            ISettingService settingService,
                                            IStaticCacheManager staticCacheManager)
        {
            _catalogModelFactory = catalogModelFactory;
            _categoryService = categoryService;
            _settingService= settingService;
            _staticCacheManager = staticCacheManager;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync(int categoryId,string categoryName, string sename)
        {
            int noOfTopSellerProductsByCategory = 10;
            try
            {
                noOfTopSellerProductsByCategory = await _settingService.GetSettingByKeyAsync<int>("CatalogSettings.NoOfTopSellerProductsByCategory");
            }
            catch
            {
            }
            var category = await _categoryService.GetCategoryByIdAsync(categoryId);
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.CategoryTopSellingProductsCacheKey,
          categoryId, noOfTopSellerProductsByCategory);
           CustomCatalogProductsCommand command = new CustomCatalogProductsCommand();
            command.PageNumber = 1;
            command.PageSize = noOfTopSellerProductsByCategory;
             command.OrderBy = (int)ProductSortingEnum.BestSeller;

            var model = await this._staticCacheManager.GetAsync(cacheKey, async () =>
            {
                return await _catalogModelFactory.PrepareCustomCategoryProductsModelAsync(category, command, "", categoryId, true);
            });


        
            model.CategoryName = categoryName;
            model.Sename = sename;
            model.CategoryID = categoryId;
            return View(model);
        }
    }
}
