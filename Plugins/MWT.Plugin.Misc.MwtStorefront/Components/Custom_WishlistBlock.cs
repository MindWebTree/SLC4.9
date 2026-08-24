using Microsoft.AspNetCore.Mvc;
using MWT.Plugin.Misc.MwtStorefront.Factories;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Orders;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Components
{
    public class Custom_WishlistBlockViewComponent : NopViewComponent
    {

        private readonly IShoppingCartService _shoppingCartService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IProductService _productService;
        private readonly ICustomProductModelFactory _productModelFactory;


        public Custom_WishlistBlockViewComponent(
            IShoppingCartService shoppingCartService,
            IWorkContext workContext,
             IStoreContext storeContext,
             IProductService productService,
             ICustomProductModelFactory productModelFactory
           )
        {
            _shoppingCartService = shoppingCartService;
            _workContext = workContext;
            _storeContext = storeContext;
            _productService = productService;
            _productModelFactory = productModelFactory;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.Wishlist, (await _storeContext.GetCurrentStoreAsync()).Id);

            if (!cart.Any())
            {
                return Content("");
            }
            var productIDs = cart.Select(c => c.ProductId).Distinct().ToArray();
            var products = await _productService.GetProductsByIdsAsync(productIDs);
            var model = (await _productModelFactory.PrepareCustomProductOverviewModelsAsync(products, true, true, null)).ToList();

            return View(model);

        }
    }
}
