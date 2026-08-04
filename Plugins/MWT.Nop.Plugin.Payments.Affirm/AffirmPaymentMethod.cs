using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Primitives;
using MWT.Nop.Core.Services.Customers;
using MWT.Nop.Plugin.Payments.Affirm.Components;
using MWT.Nop.Plugin.Payments.Affirm.Domain;
using MWT.Nop.Plugin.Payments.Affirm.Models;
using MWT.Nop.Plugin.Payments.Affirm.Services;
using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Tax;
using Nop.Core.Http.Extensions;
using Nop.Services.Catalog;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Framework.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Payments.Affirm
{

    public class AffirmPaymentMethod : BasePlugin, IPaymentMethod, IWidgetPlugin
    {
        #region Fields
        private readonly ServiceManager _serviceManager;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IPaymentService _paymentService;
        private readonly IPictureService _pictureService;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly ITaxService _taxService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IOrderService _orderService;
        private readonly IDiscountService _discountService;
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
      //  private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly IWebHelper _webHelper;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IStoreContext _storeContext;
        private readonly PaymentSettings _paymentSettings;
        private readonly TaxSettings _taxSettings;
        private readonly AffirmCheckoutSettings _affirmCheckoutSettings;
        private readonly WidgetSettings _widgetSettings;
        private readonly IWorkContext _workContext;
        private readonly IAddressService _addresService;

        #endregion

        #region Ctor


        public AffirmPaymentMethod(IActionContextAccessor actionContextAccessor,
               IPaymentService paymentService,
               IPictureService pictureService,
               ILocalizationService localizationService,
               ISettingService settingService,
               ITaxService taxService,
               IUrlRecordService urlRecordService,
               IOrderService orderService,
               IDiscountService discountService,
               IProductService productService,
               ICustomerService customerService,
            // ICheckoutAttributeParser checkoutAttributeParser,
               IWebHelper webHelper,
               IUrlHelperFactory urlHelperFactory,
               IStoreContext storeContext,
               PaymentSettings paymentSettings,
               TaxSettings taxSettings,
               AffirmCheckoutSettings affirmCheckoutSettings,
               WidgetSettings widgetSettings,
                ServiceManager serviceManager,
                IWorkContext workContext,
                IAddressService addresService
               )
        {
            _actionContextAccessor = actionContextAccessor;
            _paymentService = paymentService;
            _pictureService = pictureService;
            _localizationService = localizationService;
            _settingService = settingService;
            _taxService = taxService;
            _urlRecordService = urlRecordService;
            _orderService = orderService;
            _discountService = discountService;
            _productService = productService;
            _customerService = customerService;
            //_checkoutAttributeParser = checkoutAttributeParser;
            _webHelper = webHelper;
            _urlHelperFactory = urlHelperFactory;
            _storeContext = storeContext;
            _paymentSettings = paymentSettings;
            _taxSettings = taxSettings;
            _affirmCheckoutSettings = affirmCheckoutSettings;
            _widgetSettings = widgetSettings;
            _serviceManager = serviceManager;
            _workContext = workContext;
            _addresService = addresService;
        }

        #endregion

        #region Methods

        public async Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            //try to get an order id from custom values
            ProcessPaymentResult processPaymentResult = new ProcessPaymentResult();
            processPaymentResult.NewPaymentStatus = _affirmCheckoutSettings.TransactMode == TransactMode.AuthorizeAndCapture ? PaymentStatus.Paid : PaymentStatus.Authorized;
            //processPaymentResult.SubscriptionTransactionId = processPaymentRequest.CustomValues.Where(c => c.Key == "TransactionId").First().Value.ToString();
            //processPaymentResult.AuthorizationTransactionId = processPaymentRequest.CustomValues.Where(c => c.Key == "AuthTransactionId").First().Value.ToString();
            //processPaymentResult.AuthorizationTransactionResult = processPaymentRequest.CustomValues.Where(c => c.Key == "AuthTransactionResult").First().Value.ToString();
            //processPaymentResult.CaptureTransactionId = (string)processPaymentRequest.CustomValues.Where(c => c.Key == "CaptureTransactionId").FirstOrDefault().Value ?? string.Empty;
            //processPaymentResult.CaptureTransactionResult = (string)processPaymentRequest.CustomValues.Where(c => c.Key == "CaptureTransactionResult").FirstOrDefault().Value ?? string.Empty;

            if (processPaymentRequest.CustomValues.TryGetValue("TransactionId", out var transactionId))
                processPaymentResult.SubscriptionTransactionId = transactionId?.ToString();

            if (processPaymentRequest.CustomValues.TryGetValue("AuthTransactionId", out var authId))
                processPaymentResult.AuthorizationTransactionId = authId?.ToString();

            if (processPaymentRequest.CustomValues.TryGetValue("AuthTransactionResult", out var authResult))
                processPaymentResult.AuthorizationTransactionResult = authResult?.ToString();

            if (processPaymentRequest.CustomValues.TryGetValue("CaptureTransactionId", out var captureId))
                processPaymentResult.CaptureTransactionId = captureId?.ToString() ?? string.Empty;

            if (processPaymentRequest.CustomValues.TryGetValue("CaptureTransactionResult", out var captureResult))
                processPaymentResult.CaptureTransactionResult = captureResult?.ToString() ?? string.Empty;

            return processPaymentResult;
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the capture payment result
        /// </returns>
        public async Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
        {
            return null;
        }
        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
        {
            return null;

        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            return null;
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the process payment result
        /// </returns>
        public Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return Task.FromResult(new ProcessPaymentResult { Errors = new[] { "Recurring payment not supported" } });
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            return Task.FromResult(new CancelRecurringPaymentResult { Errors = new[] { "Recurring payment not supported" } });
        }

        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the rue - hide; false - display.
        /// </returns>
        public async Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            var notConfigured = !await _serviceManager.IsConfigured(_affirmCheckoutSettings, true, cart.ToList());
            if (!notConfigured)
            {
                var customer = new Customer();
                if (cart.Count() > 0)
                {
                    customer = await _customerService.GetCustomerByIdAsync(cart.FirstOrDefault().CustomerId);
                }
                else
                {
                    customer = await _workContext.GetCurrentCustomerAsync();
                }
                var shippingAddress = await _addresService.GetAddressByIdAsync(customer.ShippingAddressId ?? 0);
                if (shippingAddress == null || !(_affirmCheckoutSettings.SupportedCountryIds ?? string.Empty).Split(',').Where(countryId => countryId == ((shippingAddress.CountryId ?? 0).ToString())).Any())
                {
                    notConfigured = true;

                }
            }


            return notConfigured;
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the additional handling fee
        /// </returns>
        public Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            return Task.FromResult(decimal.Zero);
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public Task<bool> CanRePostProcessPaymentAsync(Order order)
        {
            return Task.FromResult(false);
        }

        /// <summary>
        /// Validate payment form
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of validating errors
        /// </returns>
        public Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            return Task.FromResult<IList<string>>((IList<string>)new List<string>());
        }

        /// <summary>
        /// Get payment information
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the payment info holder
        /// </returns>
        public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            return Task.FromResult<ProcessPaymentRequest>(new ProcessPaymentRequest());
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/AffirmSetting/Configure"; ;
        }

        /// <summary>
        /// Gets a view component for displaying plugin in public store ("payment info" checkout step)
        /// </summary>
       

        /// <summary>
        /// Gets widget zones where this widget should be rendered
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the widget zones
        /// </returns>
        public Task<IList<string>> GetWidgetZonesAsync()
        {
            var zones = new List<string>();
            zones.Add(PublicWidgetZones.CheckoutPaymentInfoTop);
            zones.Add(PublicWidgetZones.OpcContentBefore);
            zones.Add(PublicWidgetZones.OrderSummaryContentBefore);
            if (_affirmCheckoutSettings.EnableOnShoppingCart)
            {
                zones.Add(_affirmCheckoutSettings.WidgetZoneShoppingCart);
            }
            if (_affirmCheckoutSettings.EnableOnProductDetailsPage)
            {
                zones.Add(_affirmCheckoutSettings.WidgetProductDetailsPage);
            }

            return Task.FromResult((IList<string>)zones);
        }


        /// <summary>
        /// Gets a name of a view component for displaying widget
        /// </summary>
        /// <param name="widgetZone">Name of the widget zone</param>
        /// <returns>View component name</returns>
        public string GetWidgetViewComponentName(string widgetZone)
        {
            if (PublicWidgetZones.OrderSummaryContentBefore.ToString() == widgetZone)
            {
                return AffirmCheckoutDefaults.WIDGET_COMPONENT_NAME;
            }
            else
            {
                return AffirmCheckoutDefaults.WIDGET_PROMOTION_MESSAGE_COMPONENT_NAME;
            }
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task InstallAsync()
        {

            if (!_paymentSettings.ActivePaymentMethodSystemNames.Contains(AffirmCheckoutDefaults.SystemName))
            {
                _paymentSettings.ActivePaymentMethodSystemNames.Add(AffirmCheckoutDefaults.SystemName);
                await _settingService.SaveSettingAsync(_paymentSettings);
            }

            if (!_widgetSettings.ActiveWidgetSystemNames.Contains(AffirmCheckoutDefaults.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Add(AffirmCheckoutDefaults.SystemName);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }

            var affirmCheckoutSettings = new AffirmCheckoutSettings
            {
                CountryAPIMode = CountryAPIMode.USA,
                CreateOrderMode = CreateOrderMode.AfterPayment,
                TransactMode = TransactMode.AuthorizeAndCapture,
                SerialNumber = "",
                showDebugInfo = false,
                SkipPaymentInfo = false
            };
            await _settingService.SaveSettingAsync(affirmCheckoutSettings);

            //locales
            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["enums.MWT.Plugin.Payments.Affirm.Domain.CountryAPIMode.Canada"] = "Canada",
                ["enums.MWT.Plugin.Payments.Affirm.Domain.CountryAPIMode.USA"] = "USA",
                ["MWT.Plugin.Payments.Affirm.Error.EmptyOrderTotal"] = "Error. Empty order total.",
                ["MWT.Plugin.Payments.Affirm.Error.Exception"] = "Error. Exception: '{0}'.",
                ["MWT.Plugin.Payments.Affirm.Error.CreatedOrder"] = "Error. Can't create order.",
                ["MWT.Plugin.Payments.Affirm.Error.WrongOrderTotal"] = "Error. Wrong order total '{0}' and '{1}'.",
                ["MWT.Plugin.Payments.Affirm.Error.EmptyShoppingCart"] = "Error. Empty shopping cart.",
                ["MWT.Plugin.Payments.Affirm.Error.WrongOrderId"] = "Error. Wrong order Id = '{0}'.",
                ["MWT.Plugin.Payments.Affirm.Error.EmptyPaymentToken"] = "Error. Empty payment token 'checkout_token'.",
                ["MWT.Plugin.Payments.Affirm.DescriptionError"] = "You have an error with this payment, please write to support and we'll try to help you.",
                ["MWT.Plugin.Payments.Affirm.Title"] = "Affirm Monthly Payments",
                ["MWT.Plugin.Payments.Affirm.ReceiptLinkText"] = "Return to Merchant website",
                ["MWT.Plugin.Payments.Affirm.RedirectionTip"] = "You will be redirected to Affirm Hosted Checkout Page.",
                ["MWT.Plugin.Payments.Affirm.PaymentMethodDescription"] = "Check out with Affirm",
                ["Admin.MWT.Plugin.Payments.Affirm.CreateOrderMode.Hint"] = "Select whether to create the nopCommerce order before Affirm checkout initiation takes place or after a successful Affirm purchase. If unsure, select After Payment.",
                ["Admin.MWT.Plugin.Payments.Affirm.CreateOrderMode"] = "Create order",
                ["Admin.MWT.Plugin.Payments.Affirm.WidgetProductBox.Hint"] = "Select placement of promotional messaging for product boxes.",
                ["Admin.MWT.Plugin.Payments.Affirm.WidgetProductBox"] = "Widget Zone",
                ["Admin.MWT.Plugin.Payments.Affirm.EnableOnProductBox.Hint"] = "Enable Affirm promotional messaging on the product info boxes seen on the category page and other pages where there is a list of multiple products with picture, description, price (vendor, manufacturer, carousel, etc.).",
                ["Admin.MWT.Plugin.Payments.Affirm.EnableOnProductBox"] = "Enable on Product Box",
                ["Admin.MWT.Plugin.Payments.Affirm.MinimumSubTotalAmount.Hint"] = "Set the minimum subtotal amount. The plugin will display the promo message if the price or subtotal amount more then xxx.xx.",
                ["Admin.MWT.Plugin.Payments.Affirm.MinimumSubTotalAmount"] = "Minimum subtotal amount",
                ["Admin.MWT.Plugin.Payments.Affirm.WidgetProductDetailsPage.Hint"] = "Select placement of promotional messaging on product details page.",
                ["Admin.MWT.Plugin.Payments.Affirm.WidgetProductDetailsPage"] = "Widget Zone",
                ["Admin.MWT.Plugin.Payments.Affirm.EnableOnProductDetailsPage.Hint"] = "Enable Affirm promotional messaging on the product page.",
                ["Admin.MWT.Plugin.Payments.Affirm.EnableOnProductDetailsPage"] = "Enable on the product page",
                ["Admin.MWT.Plugin.Payments.Affirm.WidgetZoneShoppingCart.Hint"] = "Select placement of promotional messaging on shopping cart page.",
                ["Admin.MWT.Plugin.Payments.Affirm.WidgetZoneShoppingCart"] = "Widget Zone",
                ["Admin.MWT.Plugin.Payments.Affirm.EnableOnShoppingCart.Hint"] = "Enable Affirm promotional messaging on the cart page.",
                ["Admin.MWT.Plugin.Payments.Affirm.EnableOnShoppingCart"] = "Enable on Shopping Cart",
                ["Admin.MWT.Plugin.Payments.Affirm.PromotionalMessaging"] = "Promotional messaging",
                ["Admin.MWT.Plugin.Payments.Affirm.SkipPaymentInfo.Hint"] = "Skip payment info page.",
                ["Admin.MWT.Plugin.Payments.Affirm.SkipPaymentInfo"] = "Skip payment info page",
                ["Admin.MWT.Plugin.Payments.Affirm.AdditionalFeePercentage.Hint"] = "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.",
                ["Admin.MWT.Plugin.Payments.Affirm.AdditionalFeePercentage"] = "Additional fee. Use percentage",
                ["Admin.MWT.Plugin.Payments.Affirm.AdditionalFee.Hint"] = "Enter additional fee to charge your customers.",
                ["Admin.MWT.Plugin.Payments.Affirm.AdditionalFee"] = "Additional fee",
                ["Admin.MWT.Plugin.Payments.Affirm.PromotionalMessageColor.Hint"] = "Choose the promotional messaging color. This option does not apply to the text message type. Text will always be black.",
                ["Admin.MWT.Plugin.Payments.Affirm.PromotionalMessageColor"] = "Message color",
                ["Admin.MWT.Plugin.Payments.Affirm.PromotionalMessageType.Hint"] = "The type of display to use for the product page promotional messaging. Logo will show the full Affirm logo, Text will show text only, and Symbol will use a smaller/partial Affirm logo.",
                ["Admin.MWT.Plugin.Payments.Affirm.PromotionalMessageType"] = "Message type",
                ["Admin.MWT.Plugin.Payments.Affirm.TransactMode.Hint"] = "When a customer completes payment when placing their order, do you want to only Authorize the payment or do you want to immediately Capture it as well?",
                ["Admin.MWT.Plugin.Payments.Affirm.TransactMode"] = "Transaction mode",
                ["Admin.MWT.Plugin.Payments.Affirm.CountryAPIMode.Hint"] = "Select US unless instructed by Affirm.",
                ["Admin.MWT.Plugin.Payments.Affirm.CountryAPIMode"] = "Country API",
                ["Admin.MWT.Plugin.Payments.Affirm.UseSandbox.Hint"] = "Check this if sandbox testing.",
                ["Admin.MWT.Plugin.Payments.Affirm.UseSandbox"] = "Use Sandbox",
                ["Admin.MWT.Plugin.Payments.Affirm.FacingMerchantName.Hint"] = "Optional. If you have multiple sites operating under a single Affirm account, you can override the external company/brand name that the customer sees. This affects all references to your company name in the Affirm UI.",
                ["Admin.MWT.Plugin.Payments.Affirm.FacingMerchantName"] = "Facing Merchant Name",
                ["Admin.MWT.Plugin.Payments.Affirm.PrivateApiKey.Hint"] = "API keys can be found in your Affirm dashboard.",
                ["Admin.MWT.Plugin.Payments.Affirm.PrivateApiKey"] = "Private Api Key",
                ["Admin.MWT.Plugin.Payments.Affirm.PublicApiKey.Hint"] = "API keys can be found in your Affirm dashboard.",
                ["Admin.MWT.Plugin.Payments.Affirm.PublicApiKey"] = "Public Api Key",
                ["Admin.MWT.Plugin.Payments.Affirm.GetLogFile"] = "Download log-file",
                ["Admin.MWT.Plugin.Payments.Affirm.ClearLogFile"] = "Clear log-file",
                ["Admin.MWT.Plugin.Payments.Affirm.StoreUrl.Hint"] = "Website address (for registration).",
                ["Admin.MWT.Plugin.Payments.Affirm.StoreUrl"] = "Website address (for registration)",
                ["Admin.MWT.Plugin.Payments.Affirm.IsExpired"] = "Your license has expired, you should renew the license with a 50% discount.",
                ["Admin.MWT.Plugin.Payments.Affirm.IsNoRegisted"] = "Unregistered version is fully operational. There is only one limitation – only 100 payments.",
                ["Admin.MWT.Plugin.Payments.Affirm.IsRegisted"] = "Registered version.",
                ["Admin.MWT.Plugin.Payments.Affirm.SerialNumber.Hint"] = "Specify serial number.",
                ["Admin.MWT.Plugin.Payments.Affirm.SerialNumber"] = "Serial Number",
                ["Admin.MWT.Plugin.Payments.Affirm.showDebugInfo.Hint"] = "Write debugging information into file. In ~/App_Data/ folder.",
                ["Admin.MWT.Plugin.Payments.Affirm.showDebugInfo"] = "Enable Debugging",

            });

            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task UninstallAsync()
        {


            //settings
            if (_paymentSettings.ActivePaymentMethodSystemNames.Contains(AffirmCheckoutDefaults.SystemName))
            {
                _paymentSettings.ActivePaymentMethodSystemNames.Remove(AffirmCheckoutDefaults.SystemName);
                await _settingService.SaveSettingAsync(_paymentSettings);
            }

            if (_widgetSettings.ActiveWidgetSystemNames.Contains(AffirmCheckoutDefaults.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Remove(AffirmCheckoutDefaults.SystemName);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }

            await _settingService.DeleteSettingAsync<AffirmCheckoutSettings>();

            //locales
            await _localizationService.DeleteLocaleResourcesAsync("enums.MWT.Plugin.Payments.Affirm.Domain.CountryAPIMode");
            await _localizationService.DeleteLocaleResourcesAsync("Admin.MWT.Plugin.Payments.Affirm");
            await _localizationService.DeleteLocaleResourcesAsync("MWT.Plugin.Payments.Affirm");
            await base.UninstallAsync();
        }

        /// <summary>
        /// Gets a payment method description that will be displayed on checkout pages in the public store
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            return await _localizationService.GetResourceAsync("Plugins.Payments.Affirm.PaymentMethodDescription");
        }
        //public string GetPublicViewComponentName()
        //{
        //    return AffirmCheckoutDefaults.PAYMENT_INFO_VIEW_COMPONENT_NAME;
        //}
        public Type GetPublicViewComponent()
        {
            return typeof(AffirmPaymentInfoViewComponent);
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            if (PublicWidgetZones.OrderSummaryContentBefore.ToString() == widgetZone)
            {
                return typeof(AffirmPaymentWidgetViewComponent);
            }
            else
            {
                return typeof(AffirmPromotionMessageWidgetViewComponent);
            }
        }

        #endregion

        #region Properies

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture => true;

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid => true;

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund => true;

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund => true;

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType => PaymentMethodType.Standard;

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo => false;

        /// <summary>
        /// Gets a value indicating whether to hide this plugin on the widget list page in the admin area
        /// </summary>
        public bool HideInWidgetList => true;

        #endregion


    }
}
