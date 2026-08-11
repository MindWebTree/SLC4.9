using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Service.Catalog;
using MWT.Plugin.Misc.MwtStorefront.Factories;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;

namespace Nop.Web.Components
{
    public class Custom_RelatedProductBlockViewComponent : NopViewComponent
    {
        private readonly IAclService _aclService;
        private readonly ICustomProductModelFactory _productModelFactory;
        private readonly ICustomProductService _productService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ISettingService _settingService;

        public Custom_RelatedProductBlockViewComponent(IAclService aclService,
            ICustomProductModelFactory productModelFactory,
            ICustomProductService productService,
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

            var product = await _productService.GetProductByIdAsync(productId);
            if(product==null)
                return Content(string.Empty);
    
            return View (
                (await _productModelFactory.PrepareCustomProductOverviewModelsAsync(new List<Product> { product }, true, true, productThumbPictureSize)).First());
            
        }
    }
}