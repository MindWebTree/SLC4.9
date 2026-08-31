using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Service.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
 
namespace Nop.Web.Areas.Admin.Customizations.Components
{
    public class Custom_Upsell_MailerViewComponent : NopViewComponent
    {
        private readonly IAclService _aclService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductExtendedService _productService;
        private readonly IStoreMappingService _storeMappingService;
        public Custom_Upsell_MailerViewComponent(IAclService aclService,
                  IProductModelFactory productModelFactory,
                  IProductExtendedService productService,
                  IStoreMappingService storeMappingService)
        {
            _aclService = aclService;
            _productModelFactory = productModelFactory;
            _productService = productService;
            _storeMappingService = storeMappingService;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync(int productId, int noOfRelatedProducts, int? productThumbPictureSize)
        {
     
            //load and cache report
            var productIds = (await _productService.GetCustomRelatedProductsByProductId1Async(productId, noOfRelatedProducts)).ToArray();

            //load products
            var products = await (await _productService.GetProductsByIdsAsync(productIds))
            //ACL and store mapping
            .WhereAwait(async p => await _aclService.AuthorizeAsync(p) && await _storeMappingService.AuthorizeAsync(p))
            //availability dates
            .Where(p => _productService.ProductIsAvailable(p))
            //visible individually
            .Where(p => p.VisibleIndividually).ToListAsync();

            if (!products.Any())
                return Content(string.Empty);

            var model = (await _productModelFactory.PrepareProductOverviewModelsAsync(products, true, true, productThumbPictureSize)).ToList();
            return View(model);
        }
    }
}