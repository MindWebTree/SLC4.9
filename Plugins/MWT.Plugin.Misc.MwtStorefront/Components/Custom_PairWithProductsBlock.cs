using Nop.Web.Factories;
using Nop.Web.Models.Catalog;
using System.Linq;
using Nop.Services.Catalog;
using Nop.Core;
using System.Threading.Tasks;
using Nop.Web.Framework.Components;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Catalog;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using MWT.Nop.Core.Service.Catalog;
using MWT.Plugin.Misc.MwtStorefront.Factories;
using MWT.Plugin.Misc.MwtStorefront.Models.Catalog;

namespace Nop.Web.Components
{
    public class Custom_PairWithProductsBlockViewComponent : NopViewComponent
    {
        private readonly ICustomProductModelFactory _productModelFactory;
        private readonly ICustomProductService _productService;
        private readonly IStoreContext _storeContext;
        private readonly MediaSettings _mediaSettings;

        public Custom_PairWithProductsBlockViewComponent(CatalogSettings catalogSettings,
   ICustomProductModelFactory productModelFactory,
            ICustomProductService productService,
             IStoreContext storeContext,
             MediaSettings mediaSettings)
        {
            _productModelFactory = productModelFactory;
            _productService = productService;
            _storeContext = storeContext;
            _mediaSettings = mediaSettings;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync(int productid, int? productThumbPictureSize, bool? preparePriceModel)
        {
            List<int> prdIds = await _productService.GetPairWithProductsByProductId1Async(productid);
            if (!prdIds.Any())
                return Content("");

            var products = await _productService.GetProductsByIdsAsync(prdIds.ToArray());
            List<CustomProductDetailsModel> model = new List<CustomProductDetailsModel>();
            foreach (var product in products)
            {
                model.Add(await _productModelFactory.PrepareCustomProductDetailsModelAsync(product, null, true));
            }

            //prepare model

            return View(model);
        }
    }
}