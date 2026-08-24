using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Services.Orders;
using MWT.Plugin.Misc.MwtStorefront.Factories;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Orders;
using Nop.Web.Framework.Components;
using Nop.Web.Models.ShoppingCart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Components
{
    public class CheckoutOrderSummaryViewComponent : NopViewComponent
    {
        private readonly IShoppingCartExtendedModelFactory _shoppingCartModelFactory;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly IOrderTotalCalculationExtendedService _orderTotalCalculationService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        public CheckoutOrderSummaryViewComponent(IShoppingCartExtendedModelFactory  shoppingCartModelFactory,
            IShoppingCartService shoppingCartService,
            IStoreContext storeContext,
            IWorkContext workContext,
            IOrderTotalCalculationExtendedService orderTotalCalculationService,
            ICurrencyService currencyService,
            IPriceFormatter priceFormatter
            )
        {
            _shoppingCartModelFactory = shoppingCartModelFactory;
            _shoppingCartService = shoppingCartService;
            _storeContext = storeContext;
            _workContext = workContext;
            _orderTotalCalculationService = orderTotalCalculationService;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync(bool? prepareAndDisplayOrderReviewData, ShoppingCartModel overriddenModel, bool? isConfirmationPage = false)
        {
            //use already prepared (shared) model
            if (overriddenModel != null)
                return View(overriddenModel);


            //if not passed, then create a new model
            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

            var model = new ShoppingCartModel();
            model = await _shoppingCartModelFactory.PrepareCustomShoppingCartModelAsync(model, cart,
                isEditable: false,
                prepareAndDisplayOrderReviewData: prepareAndDisplayOrderReviewData.GetValueOrDefault());
            if (cart.Count > 0)
            {
                var (shoppingCartTotalBase, orderTotalDiscountAmountBase, orderAppliedDiscounts, appliedGiftCards, redeemedRewardPoints, redeemedRewardPointsAmount, orderDiscountAmountsApplied) =
                             await _orderTotalCalculationService.GetCustomShoppingCartTotalWithDiscountInfosync(cart);
                if (shoppingCartTotalBase.HasValue)
                {
                    var shoppingCartTotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartTotalBase.Value, await _workContext.GetWorkingCurrencyAsync());
                    model.OrderTotal = await _priceFormatter.FormatPriceAsync(shoppingCartTotal, true, false);
                }
            }
            model.isConfirmationPage = isConfirmationPage;
            return View(model);
        }
    }
}
