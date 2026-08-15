using Microsoft.AspNetCore.Mvc;
using MWT.Plugin.Misc.MwtStorefront.Factories;
using Nop.Core.Domain.Orders;
using Nop.Services.Security;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Components
{
    public class FlyoutRightWishlistViewComponent : NopViewComponent
    {
        private readonly IPermissionService _permissionService;
        private readonly IShoppingCartExtendedModelFactory _shoppingCartModelFactory;
        private readonly ShoppingCartSettings _shoppingCartSettings;

        public FlyoutRightWishlistViewComponent(IPermissionService permissionService,
            IShoppingCartExtendedModelFactory shoppingCartModelFactory,
            ShoppingCartSettings shoppingCartSettings)
        {
            _permissionService = permissionService;
            _shoppingCartModelFactory = shoppingCartModelFactory;
            _shoppingCartSettings = shoppingCartSettings;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync(string actionType = "")
        {
            if (!_shoppingCartSettings.MiniShoppingCartEnabled)
                return Content("");

            if (!await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.ENABLE_SHOPPING_CART))
                return Content("");

            var model = await _shoppingCartModelFactory.PrepareCustomMiniShoppingCartModelAsync(ShoppingCartType.Wishlist);
            model.Heading = actionType;
            return View(model);
        }
    }
}
