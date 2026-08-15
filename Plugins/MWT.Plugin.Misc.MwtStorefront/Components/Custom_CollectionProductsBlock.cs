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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Components
{
    public class Custom_CollectionProductsBlockViewComponent : NopViewComponent
    {
        private readonly ICustomProductModelFactory _productModelFactory;
        private readonly ICustomProductService _productService;
        private readonly IStoreContext _storeContext;
        private readonly MediaSettings _mediaSettings;
        private readonly ISettingService _settingService;

        public Custom_CollectionProductsBlockViewComponent(CatalogSettings catalogSettings,
   ICustomProductModelFactory productModelFactory,
            ICustomProductService productService,
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
            CollectionProductModel model = new CollectionProductModel();
            var lstProductOverViewModel = new List<CustomProductOverviewModel>();
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

            lstProductOverViewModel.AddRange(await _productModelFactory.PrepareCustomProductOverviewModelsAsync(products,
                true,
                true,
                productThumbPictureSize,
                false,
                false
                ));

            if (lstMainCollectionProducts.Count > 0)
            {
                var mainCollectionProduct = (await _productModelFactory.PrepareCustomProductOverviewModelsAsync(lstMainCollectionProducts, true, true, _mediaSettings.ProductDetailsPictureSize, false, false, false, true, true, false)).FirstOrDefault();
                mainCollectionProduct.IsMainCollection = true;
                lstProductOverViewModel.Add(mainCollectionProduct);
            }
            model.Products = lstProductOverViewModel;
            return View(model);
        }
    }
}