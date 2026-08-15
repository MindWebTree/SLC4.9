using Microsoft.AspNetCore.Mvc;
using MWT.Plugin.Misc.MwtStorefront.Factories;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
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
    public class Version2CustomCategoryFeaturedProductBlockViewComponent : NopViewComponent
    {

        private readonly ICustomProductModelFactory _productModelFactory;
        private readonly IProductService _productService;
        private readonly CatalogSettings _catalogSettings;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;
        public Version2CustomCategoryFeaturedProductBlockViewComponent(CatalogSettings catalogSettings,
            ICustomProductModelFactory productModelFactory,
            IProductService productService,
           IAclService aclService,
           IStoreMappingService storeMappingService)
        {
            _productModelFactory = productModelFactory;
            _productService = productService;
            _catalogSettings = catalogSettings;
            _aclService = aclService;
            _storeMappingService = storeMappingService;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync(int productId, int variantId = 0)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null || product.Deleted)
                return Content("");
            var notAvailable =
          //published?
          (!product.Published && !_catalogSettings.AllowViewUnpublishedProductPage) ||
          //ACL (access control list) 
          !await _aclService.AuthorizeAsync(product) ||
          //Store mapping
          !await _storeMappingService.AuthorizeAsync(product) ||
          //availability dates
          !_productService.ProductIsAvailable(product);
            if (notAvailable)
                return Content("");
            List<Product> products = new List<Product>();
            products.Add(product);
            var model = await _productModelFactory.PrepareCustomProductOverviewDetailInfoModelAsync(products, true, true, null, false, false, false, false, false, false, variantId);
            return View(model.FirstOrDefault());
        }
    }
}
