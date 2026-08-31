using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Areas.Models.Customizations.Mailer;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;

namespace Nop.Web.Areas.Admin.Customizations.Components
{
    public class Custom_Product_Info_MailerViewComponent : NopViewComponent
    {
        private readonly IAclService _aclService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductService _productService;
        private readonly IStoreMappingService _storeMappingService;
        public Custom_Product_Info_MailerViewComponent(IAclService aclService,
                  IProductModelFactory productModelFactory,
                  IProductService productService,
                  IStoreMappingService storeMappingService)
        {
            _aclService = aclService;
            _productModelFactory = productModelFactory;
            _productService = productService;
            _storeMappingService = storeMappingService;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync(int productId, string moduleType, int? productThumbPictureSize)
        {

            var product = await _productService.GetProductByIdAsync(productId);
            if (product != null)
            {
                List<Product> products = new List<Product>();
                products.Add(product);
                ProductInfoModel model = new ProductInfoModel();
                model.Products = (await _productModelFactory.PrepareProductOverviewModelsAsync(products, true, true, productThumbPictureSize)).ToList();
                model.ModuleType = moduleType;
                return View(model);
            }
            return Content("");
        }
    }
}