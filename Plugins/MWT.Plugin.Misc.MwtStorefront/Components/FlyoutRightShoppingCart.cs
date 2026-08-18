using Microsoft.AspNetCore.Mvc;
using MWT.Plugin.Misc.MwtStorefront.Factories;
using MWT.Plugin.Misc.MwtStorefront.Models.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Services.Security;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;

namespace MWT.Plugin.Misc.MwtStorefront.Components
{
    public class FlyoutRightShoppingCartViewComponent : NopViewComponent
    {
        private readonly IPermissionService _permissionService;
        private readonly IShoppingCartExtendedModelFactory _shoppingCartModelFactory;
        private readonly ShoppingCartSettings _shoppingCartSettings;

        public FlyoutRightShoppingCartViewComponent(IPermissionService permissionService,
            IShoppingCartExtendedModelFactory shoppingCartModelFactory,
            ShoppingCartSettings shoppingCartSettings)
        {
            _permissionService = permissionService;
            _shoppingCartModelFactory = shoppingCartModelFactory;
            _shoppingCartSettings = shoppingCartSettings;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync(string actionType = "", List<int> cartItems = null)
        {
            if (!_shoppingCartSettings.MiniShoppingCartEnabled)
                return Content("");

            if (!await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.ENABLE_SHOPPING_CART))
                return Content("");

            var model = await _shoppingCartModelFactory.PrepareCustomMiniShoppingCartModelAsync(ShoppingCartType.ShoppingCart);
            model.Heading = actionType;
            if (cartItems == null)
                model.cartItems = model.Items.Count == 0 ? new List<int>() : new List<int>() { model.Items.FirstOrDefault().Id };
            else
                model.cartItems = cartItems; ;
            return View(model);
        }
    }
}
