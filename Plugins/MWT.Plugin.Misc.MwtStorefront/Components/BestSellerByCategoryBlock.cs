using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MWT.Plugin.Misc.MwtStorefront.Factories;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Orders;
using Nop.Web.Areas.Admin.Models.Reports;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using Nop.Web.Infrastructure.Cache;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;

namespace MWT.Plugin.Misc.MwtStorefront.Components
{
    public class BestSellerByCategoryBlockViewComponent : NopViewComponent
    {
        private readonly IProductService _productService;
        private readonly ICustomProductModelFactory _productModelFactory;
        private readonly IStoreContext _storeContext;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IOrderReportService _orderReportService;
        private readonly CatalogSettings _catalogSettings;
        public BestSellerByCategoryBlockViewComponent(IProductService productService,
        ICustomProductModelFactory productModelFactory,
        IStoreContext storeContext,
        IStaticCacheManager staticCacheManager,
        IOrderReportService orderReportService,
        CatalogSettings catalogSettings)
        {
            _productService = productService;
            _productModelFactory = productModelFactory;
            _storeContext = storeContext;
            _staticCacheManager = staticCacheManager;
            _orderReportService = orderReportService;
            _catalogSettings = catalogSettings;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync(int categoryId)
        {
            if (categoryId == 0)
                return Content("");
            var model = (await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.BestSellerByCategoryCacheKey,



                await _storeContext.GetCurrentStoreAsync(), categoryId), async () =>
                {
                    var bestsellers = await _orderReportService.BestSellersReportAsync(categoryId, pageIndex: 0, pageSize: _catalogSettings.NumberOfBestsellersOnHomepage,
                    storeId: _storeContext.GetCurrentStore().Id);

                    var productIds = await bestsellers.SelectAwait(async bestseller =>
                    {
                        //fill in model values from the entity
                        return bestseller.ProductId;
                    }).ToArrayAsync();


                    var model = (await _productModelFactory.PrepareCustomProductOverviewModelsAsync(await _productService.GetProductsByIdsAsync(productIds),
                     productThumbPictureSize: null, forceRedirectionAfterAddingToCart: true))
                    .ToList();
                    return model;

                }));




            return View(model);
        }
    }
}
