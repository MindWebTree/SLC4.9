using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Plugin.Payments.Affirm.Infrastructure;
using MWT.Nop.Plugin.Payments.Affirm.Models;
using MWT.Nop.Plugin.Payments.Affirm.Services;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.ShoppingCart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Payments.Affirm.Components
{
    public class AffirmPromotionMessageWidgetViewComponent : NopViewComponent
    {
        #region Fields

        private readonly AffirmCheckoutSettings _affirmSettings;

        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ServiceManager _serviceManager;
        #endregion

        #region Ctor


        public AffirmPromotionMessageWidgetViewComponent(IPaymentPluginManager paymentPluginManager,
        IStoreContext storeContext,
        IWorkContext workContext,
AffirmCheckoutSettings affirmSettings,
IOrderTotalCalculationService orderTotalCalculationService,
IShoppingCartService shoppingCartService,
ServiceManager serviceManager)
        {
            _paymentPluginManager = paymentPluginManager;
            _storeContext = storeContext;
            _workContext = workContext;
            _affirmSettings = affirmSettings;
            _orderTotalCalculationService = orderTotalCalculationService;
            _shoppingCartService = shoppingCartService;
            _serviceManager = serviceManager;
        }


        #endregion
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            try
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                var store = await _storeContext.GetCurrentStoreAsync();
                if (!await _paymentPluginManager.IsPluginActiveAsync(AffirmCheckoutDefaults.SystemName, customer, store?.Id ?? 0))
                    return Content(string.Empty);

                if (!await _serviceManager.IsConfigured(_affirmSettings, false))
                    return Content(string.Empty);


                if (!widgetZone.Equals(_affirmSettings.WidgetProductDetailsPage) && !widgetZone.Equals(_affirmSettings.WidgetZoneShoppingCart) || ((widgetZone.Equals(_affirmSettings.WidgetProductDetailsPage) && !_affirmSettings.EnableOnProductDetailsPage) || (widgetZone.Equals(_affirmSettings.WidgetZoneShoppingCart) && !_affirmSettings.EnableOnShoppingCart)))
                    return Content(string.Empty);
                decimal total = 0;
                int productId = 0;

                if (additionalData is ShoppingCartModel || additionalData is null)`
                {

                    var shoppingCart = (await _shoppingCartService
                .GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id))
                .ToList();
                    var (shoppingCartTotal, _, _, _, _, _) = await _orderTotalCalculationService
                     .GetShoppingCartTotalAsync(shoppingCart, usePaymentMethodAdditionalFee: false);
                    total = Math.Round(shoppingCartTotal ?? decimal.Zero, 2);
                }
                else
                {
                    productId = additionalData is BaseNopEntityModel model ? model.Id : 0;
                }
                PromotionalMessagingInfoModel promotionModel = new PromotionalMessagingInfoModel();
                promotionModel.PrivateKey = _affirmSettings.PrivateApiKey;
                promotionModel.PublicKey = _affirmSettings.PublicApiKey;
                promotionModel.PageType = productId == 0 ? "Cart" : "Product";
                promotionModel.Amount = AffirmHelper.ConvertDecimalToCents(total);
                promotionModel.MessageType = _affirmSettings.PromotionalMessageType.ToString();
                promotionModel.ProductId = productId;
                promotionModel.MessageColor = _affirmSettings.PromotionalMessageColor.ToString();
                promotionModel.UseSandBox = _affirmSettings.UseSandbox;
                return View("~/Plugins/MWT.Nop.Plugin.Payments.Affirm/Views/Promotion.cshtml", promotionModel);
            }
            catch
            {

            }

            return Content(string.Empty);



        }
    }
}
