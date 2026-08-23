using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Service.Catalog;
using MWT.Plugin.Misc.MwtStorefront.Factories;
using MWT.Plugin.Misc.MwtStorefront.Infrastructure.Filters;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Controllers.Api
{
    public class ProductApiController : BasePublicController
    {
        #region fields
        private readonly ICustomProductModelFactory _productModelFactory;
        private readonly IProductExtendedService _productService;

        #endregion

        #region Ctor
        public ProductApiController(ICustomProductModelFactory productModelFactory, IProductExtendedService productService)
        {
            this._productModelFactory = productModelFactory;
            _productService = productService;
        }

        #endregion

        [ValidateApiKey]
        [HttpGet]
        public async Task<IActionResult> Feed(int pageNumber = 0, int pageSize = 100)
        {
            if (pageSize > 100)
                pageSize = 100;
            return new JsonResult(await this._productModelFactory.PrepareProductFeed(pageNumber, pageSize));
        }


        [ValidateApiKey]
        [HttpGet]
        public async Task<IActionResult> Products(int pageNumber, int productId = 0)
        {
            return new JsonResult(await _productModelFactory.PrepareApiProductListModelAsync(pageNumber, productId));

        }

        [ValidateApiKey]
        [HttpGet]
        public async Task<IActionResult> GetProductImage(int productId)
        {
            return Content(await this._productModelFactory.GetProductMainImage(productId));
        }
        [ValidateApiKey]
        [HttpGet]
        public async Task<IActionResult> IsVariantSurchargeApplicable(int variantId)
        {
            var _workcontext = EngineContext.Current.Resolve<IWorkContext>();
            var customer = await _workcontext.GetCurrentCustomerAsync();
            bool isApplicable = await _productModelFactory.IsVariantSurchargeApplicable(variantId);
            return Ok(new { IsApplicable = isApplicable });
        }

        [ValidateApiKey]
        [HttpGet]
        public async Task<IActionResult> GetProductVariants(string sku)
        {
            return new JsonResult(await _productModelFactory.PrepareProductVariants(sku));
        }
    }
}
