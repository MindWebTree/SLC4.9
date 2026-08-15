using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Service.Catalog;
using MWT.Plugin.Misc.MwtStorefront.Factories;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;

namespace Nop.Web.Components
{
    public class Custom_RelatedProductsViewComponent : NopViewComponent
    {
        private readonly IAclService _aclService;
        private readonly ICustomProductModelFactory _productModelFactory;
        private readonly IProductExtendedService _productService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ISettingService _settingService;

        public Custom_RelatedProductsViewComponent(IAclService aclService,
            ICustomProductModelFactory productModelFactory,
            IProductExtendedService productService,
            IStoreMappingService storeMappingService,
            ISettingService settingService)
        {
            _aclService = aclService;
            _productModelFactory = productModelFactory;
            _productService = productService;
            _storeMappingService = storeMappingService;
            _settingService = settingService;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync(int productId, int? productThumbPictureSize)
        {
            int noOfRelatedProducts = 10;

            try
            {
                noOfRelatedProducts = await _settingService.GetSettingByKeyAsync<int>("CatalogSettings.NoOfRelatedProducts");
            }
            catch
            {
            }
            //load and cache report
            var productIds = (await _productService.GetCustomRelatedProductsByProductId1Async(productId, noOfRelatedProducts));

            //load products
            var products = await (await _productService.GetProductsByIdsAsync(productIds.ToArray()))
            //ACL and store mapping
            .WhereAwait(async p => await _aclService.AuthorizeAsync(p) && await _storeMappingService.AuthorizeAsync(p))
            //availability dates
            .Where(p => _productService.ProductIsAvailable(p))
            //visible individually
            .Where(p => p.VisibleIndividually).ToListAsync();

            if (!products.Any())
                return Content(string.Empty);

            var model = (await _productModelFactory.PrepareCustomProductOverviewModelsAsync(products, true, true, productThumbPictureSize)).ToList();
            return View(model);
        }
    }
}