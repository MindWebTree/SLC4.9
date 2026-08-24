using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Service.Catalog;
using MWT.Plugin.Misc.MwtStorefront.Factories;
using MWT.Plugin.Misc.MwtStorefront.Models.Catalog;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Catalog;

namespace Nop.Web.Components
{
    public class Custom_CollectionProducts_Vertical_DisplayBlockViewComponent : NopViewComponent
    {
        private readonly ICustomProductModelFactory _productModelFactory;
        private readonly IProductExtendedService _productService;
        private readonly IStoreContext _storeContext;
        private readonly MediaSettings _mediaSettings;
        private readonly ISettingService _settingService;

        public Custom_CollectionProducts_Vertical_DisplayBlockViewComponent(CatalogSettings catalogSettings,
   ICustomProductModelFactory productModelFactory,
            IProductExtendedService productService,
             IStoreContext storeContext,
             MediaSettings mediaSettings,
             ISettingService settingService)
        {
            _productModelFactory = productModelFactory;
            _productService = productService;
            _storeContext = storeContext;
            _mediaSettings = mediaSettings;
            _settingService = settingService;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync(int productid, int? productThumbPictureSize, bool? preparePriceModel)
        {
            int pageSize = 10;
            try
            {
                pageSize = await _settingService.GetSettingByKeyAsync<int>("CatalogSettings.NoOfPickProductYouLike");
            }
            catch
            {
            }
            var (products, parentGroupedProductId) = await _productService.CustomGetCollectionProductsAsync(productid, pageSize, (await _storeContext.GetCurrentStoreAsync()).Id);
            if (!products.Any())
                return Content("");

            //prepare model
            CollectionDetailProductModel model = new CollectionDetailProductModel();
            var lstProductDetailsModel = new List<CustomProductDetailsModel>();
            var lstMainCollectionProducts = new List<Product>();

            if (parentGroupedProductId == productid)
            {
                parentGroupedProductId = 0;
                model.IsCollectionPage = true;
            }
            if (products.Where(m => m.Id == parentGroupedProductId).Any())
            {
                lstMainCollectionProducts = products.Where(m => m.Id == parentGroupedProductId).ToList();
                products = products.Where(m => m.Id != parentGroupedProductId).ToList();
            }
            products = products.Where(m => m.Id != productid).ToList();

            foreach (var product in products)
            {
                lstProductDetailsModel.Add(await _productModelFactory.PrepareCustomProductDetailsModelAsync(product,
                    null, true));
            }

            if (lstMainCollectionProducts.Count > 0)
            {
                model.MainCollection = (await _productModelFactory.PrepareCustomProductOverviewModelsAsync(lstMainCollectionProducts, true, true, _mediaSettings.ProductDetailsPictureSize, false, false, false, true, true, false)).FirstOrDefault();


            }
            model.Products = lstProductDetailsModel;
            return View(model);
        }
    }
}