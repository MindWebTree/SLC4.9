using MWT.Plugin.Misc.MwtStorefront.Models.ShoppingCart;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Web.Factories;
using Nop.Web.Models.ShoppingCart;

namespace MWT.Plugin.Misc.MwtStorefront.Factories;
public partial interface IShoppingCartExtendedModelFactory:IShoppingCartModelFactory
{
    Task<ShoppingCartModel> PrepareCustomShoppingCartModelAsync(ShoppingCartModel model,
       IList<ShoppingCartItem> cart, bool isEditable = true,
       bool validateCheckoutAttributes = false,
       bool prepareAndDisplayOrderReviewData = false);
    Task<OrderTotalsModel> PrepareCustomOrderTotalsModelAsync(IList<ShoppingCartItem> cart, bool isEditable);
    Task<ShoppingCartModel> ModifyCartItemModelForCustomUpdates(ShoppingCartModel model);
    Task<ShoppingCartModel> ModifyCartItemModelForCustomUpdates(ShoppingCartModel model, Customer customer);
    Task<MiniShoppingCartExtendedModel> PrepareCustomMiniShoppingCartModelAsync(ShoppingCartType cartType);

    Task<MiniShoppingCartExtendedModel> PrepareCustomFlyoutShoppingCartModelAsync(ShoppingCartType cartType);
    Task<MiniShoppingCartExtendedModel> PrepareWishlistModelAsync(int? list = null);
    Task<WishlistExtendedModel> PrepareCustomWishlistModelAsync(WishlistExtendedModel model, IList<ShoppingCartItem> cart, bool isEditable = true, int? list = null);
    Task<bool> IsWgsShippingMethodRequired(IList<ShoppingCartItem> cart);
}


