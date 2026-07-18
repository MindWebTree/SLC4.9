using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using MWT.Nop.Plugin.Payments.Affirm.Data.Domain;
using MWT.Nop.Plugin.Payments.Affirm.Domain;
using MWT.Nop.Plugin.Payments.Affirm.Infrastructure;
using MWT.Nop.Plugin.Payments.Affirm.Models;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Framework.Infrastructure;


namespace MWT.Nop.Plugin.Payments.Affirm.Services
{
    public class ServiceManager
    {
        #region Fields

        private readonly CurrencySettings _currencySettings;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IAddressService _addresService;
     //   private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly ICountryService _countryService;
        private readonly ICurrencyService _currencyService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILogger _logger;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IProductService _productService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly ITaxService _taxService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly ISettingService _settingService;
        private readonly ICustomerService _customerService;
        private readonly IRepository<PaymentMethodSession> _affirmSessionRepository;
        private readonly IRepository<AffirmLog> _affirmLogRepository;

        #endregion

        #region Ctor

        public ServiceManager(CurrencySettings currencySettings,
            IActionContextAccessor actionContextAccessor,
            IAddressService addresService,
    //        ICheckoutAttributeParser checkoutAttributeParser,
            ICountryService countryService,
            ICurrencyService currencyService,
            IGenericAttributeService genericAttributeService,
            ILogger logger,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IProductService productService,
            IShoppingCartService shoppingCartService,
            IStateProvinceService stateProvinceService,
            IStoreContext storeContext,
            IStoreService storeService,
            ITaxService taxService,
            IUrlHelperFactory urlHelperFactory,
            IWebHelper webHelper,
            IWorkContext workContext,
            ISettingService settingService,
            ICustomerService customerService,
            IRepository<PaymentMethodSession> affirmSessionRepository,
            IRepository<AffirmLog> affirmLogRepository
            )
        {
            _currencySettings = currencySettings;
            _actionContextAccessor = actionContextAccessor;
            _addresService = addresService;
         //   _checkoutAttributeParser = checkoutAttributeParser;
            _countryService = countryService;
            _currencyService = currencyService;
            _genericAttributeService = genericAttributeService;
            _logger = logger;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _productService = productService;
            _shoppingCartService = shoppingCartService;
            _stateProvinceService = stateProvinceService;
            _storeContext = storeContext;
            _storeService = storeService;
            _taxService = taxService;
            _urlHelperFactory = urlHelperFactory;
            _webHelper = webHelper;
            _workContext = workContext;
            _settingService = settingService;
            _customerService = customerService;
            _affirmSessionRepository = affirmSessionRepository;
            _affirmLogRepository = affirmLogRepository;
        }

        #endregion

        #region Methods

        public async Task<PaymentInfoModel> CheckoutInit(Guid orderGuid)
        {
            PaymentInfoModel model = new PaymentInfoModel();
            var storeLocation = _webHelper.GetStoreLocation();
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var affirmCheckoutSettings = await _settingService.LoadSettingAsync<AffirmCheckoutSettings>(storeScope);
            if (!await IsConfigured(affirmCheckoutSettings))
                throw new NopException("Plugin not configured");

            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();

            var currency = (await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId))?.CurrencyCode;
            if (string.IsNullOrEmpty(currency))
                throw new NopException("Primary store currency not set");
            var billingAddress = await _addresService.GetAddressByIdAsync(customer.BillingAddressId ?? 0)
                  ?? throw new NopException("Customer billing address not set");

            var shippingAddress = await _addresService.GetAddressByIdAsync(customer.ShippingAddressId ?? 0);

            var billStateProvince = await _stateProvinceService.GetStateProvinceByAddressAsync(billingAddress);
            var shipStateProvince = await _stateProvinceService.GetStateProvinceByAddressAsync(shippingAddress);
            var shipCountry = await _countryService.GetCountryByAddressAsync(shippingAddress);
            var billCountry = await _countryService.GetCountryByAddressAsync(billingAddress);


            string shippingFirstName = shippingAddress?.FirstName ?? customer.FirstName ?? string.Empty;
            string shippingLastName = shippingAddress?.LastName ?? customer.LastName ?? string.Empty;
            shippingLastName = string.IsNullOrEmpty(shippingLastName) ? shippingFirstName : shippingLastName; string shippingAddress1 = shippingAddress.Address1 ?? string.Empty;
            string shippingAddress2 = shippingAddress.Address2 ?? string.Empty;
            string shippingCity = shippingAddress?.City ?? string.Empty;
            string shippingState = shipStateProvince?.Abbreviation;
            string shippingZipCode = shippingAddress?.ZipPostalCode ?? string.Empty;
            string shippingCountry = shipCountry?.Name ?? string.Empty;
            string shippingEmail = shippingAddress?.Email ??   customer.Email ?? string.Empty;
            string shippingPhonenumber = shippingAddress?.PhoneNumber ?? customer.Phone ?? string.Empty;


            string billingFirstName = billingAddress?.FirstName ?? customer.FirstName ?? string.Empty;
            string billingLastName = billingAddress?.LastName ?? customer.LastName ?? string.Empty;
            billingLastName = string.IsNullOrEmpty(billingLastName) ? billingFirstName : billingLastName; string billingAddress1 = billingAddress.Address1 ?? shippingAddress.Address1 ?? string.Empty;
            string billingAddress2 = billingAddress.Address2 ?? shippingAddress.Address2 ?? string.Empty;
            string billingCity = billingAddress?.City ?? shippingAddress?.City ?? string.Empty;
            string billingState = billStateProvince?.Abbreviation ?? shipStateProvince?.Abbreviation ?? string.Empty;
            string billingZipCode = billingAddress?.ZipPostalCode ?? shippingAddress?.ZipPostalCode ?? string.Empty;
            string billingCountry = billCountry?.Name ?? shipCountry?.Name ?? string.Empty;
            string billingEmail = billingAddress?.Email ?? customer.Email ?? string.Empty;

            string billingPhonenumber = billingAddress?.PhoneNumber ?? customer.Phone ?? string.Empty;
            var shoppingCart = (await _shoppingCartService
               .GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id))
               .ToList();

            //if (discountTotal > 0)
            //{
            //    items.Add(new AffirmItem()
            //    {
            //        DisplayName = "Discount",
            //        qty = 1,
            //        Sku="Discount",
            //        unit_price=-AffirmHelper.ConvertDecimalToCents(discountTotal)
            //     });
            //}
            ShippingOption shippingOption = await _genericAttributeService.GetAttributeAsync<ShippingOption>(customer, NopCustomerDefaults.SelectedShippingOptionAttribute, (await _storeContext.GetCurrentStoreAsync()).Id);

            var existingCouponCodes = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.DiscountCouponCodeAttribute);


            (List<AffirmItem> items, decimal? shoppingCartTotal, decimal discountTotal, decimal customDuty, decimal shippingTotal, decimal taxTotal, decimal orderTotal) = await GetCartSummarry(shoppingCart, customer);
            #region Take Log of data
            var affirmSession = new PaymentMethodSession()
            {
                CartItems = JsonConvert.SerializeObject(shoppingCart),
                CartTotal = shoppingCartTotal,
                Checkout_Token = string.Empty,
                OrderTotal = orderTotal,
                DiscountTotal = discountTotal,
                LiveOrderNumber = 0,
                IsCustomOrder = false,
                ShipingTotal = shippingTotal,
                Country = shippingCountry,
                CustomDutyTotal = customDuty,
                ShippingMethod = JsonConvert.SerializeObject(shippingOption),
                CustomerId = customer.Id,
                TaxTotal = taxTotal,
                Status = RequestStatus.Init,
                ZipCode = shippingZipCode,
                Coupon = existingCouponCodes,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };


            await _affirmSessionRepository.InsertAsync(affirmSession);

            #endregion
            var payload = new
            {
                merchant = new
                {
                    public_api_key = affirmCheckoutSettings.PublicApiKey,
                    user_confirmation_url = $"{storeLocation}Admin/Affirm/ConfirmCallbackHandler",
                    user_cancel_url = $"{storeLocation}Admin/Affirm/CancelCallbackHandler",
                    user_confirmation_url_action = "GET"
                },
                shipping = new
                {
                    name = new
                    {
                        first = shippingFirstName,
                        last = shippingLastName
                    },
                    address = new
                    {
                        line1 = CommonHelper.EnsureMaximumLength(shippingAddress1, 300),
                        line2 = CommonHelper.EnsureMaximumLength(shippingAddress2, 300),
                        city = shippingCity,
                        state = shippingState,
                        zipcode = shippingZipCode,
                        country = shippingCountry
                    },
                    phone_number = shippingPhonenumber,
                    email = shippingEmail
                },
                billing = new
                {
                    name = new
                    {
                        first = billingFirstName,
                        last = billingLastName
                    },
                    address = new
                    {
                        line1 = CommonHelper.EnsureMaximumLength(billingAddress1, 300),
                        line2 = CommonHelper.EnsureMaximumLength(billingAddress2, 300),
                        city = billingCity,
                        state = billingState,
                        zipcode = billingZipCode,
                        country = billingCountry
                    },
                    phone_number = billingPhonenumber,
                    email = billingEmail
                },
                items = items.Select(item => new
                {
                    display_name = item.DisplayName,
                    sku = item.Sku,
                    unit_price = item.unit_price, // Convert price to cents
                    qty = item.qty
                }),
                meta = new
                {
                    promo_code = existingCouponCodes, // Example custom property
                    customerid = customer.Id,
                    tempOrderId = affirmSession.Id,
                    orderGuid = orderGuid.ToString()
                    // Another custom property
                },
                discounts = new
                {
                    Discount = new
                    {
                        discount_amount = AffirmHelper.ConvertDecimalToCents(discountTotal < 0 ? -discountTotal : discountTotal),
                        discount_display_name = "Discount"
                    }
                },
                currency = (await _workContext.GetWorkingCurrencyAsync()).CurrencyCode,
                shipping_amount = AffirmHelper.ConvertDecimalToCents(shippingTotal),
                tax_amount = AffirmHelper.ConvertDecimalToCents(taxTotal),
                total = AffirmHelper.ConvertDecimalToCents(orderTotal),
                webhook_session_id = (await _workContext.GetCurrentCustomerAsync())?.CustomerGuid.ToString() ?? string.Empty,// ,
                order_id = orderGuid.ToString()
            };

            model.AffirmJSON = "affirm.checkout(" + Newtonsoft.Json.JsonConvert.SerializeObject(payload) + ");";
            return model;
        }


        public async Task<(string Script, string Error)> GetScriptAsync(AffirmCheckoutSettings settings, string widgetZone)
        {
            return await HandleFunctionAsync(async () =>
            {
                //ensure that plugin is configured
                if (!await IsConfigured(settings))
                    throw new NopException("Plugin not configured");
                string scriptUrl = settings.UseSandbox ? "https://sandbox.affirm.com/js/v2/affirm.js" : "https://cdn1.affirm.com/js/v2/affirm.js";
                return $@"<script defer src=""{scriptUrl}""></script>";
            });
        }

        #endregion

        #region Utilities

        private async Task<(TResult Result, string Error)> HandleFunctionAsync<TResult>(Func<Task<TResult>> function)
        {
            try
            {
                //invoke function
                return (await function(), default);
            }
            catch (Exception exception)
            {

                var message = exception.Message;
                var logMessage = $"{AffirmCheckoutDefaults.SystemName} error: {System.Environment.NewLine}{message}";
                await _logger.ErrorAsync(logMessage, exception, await _workContext.GetCurrentCustomerAsync());

                return (default, message);
            }
        }
        public async Task<bool> IsConfigured(AffirmCheckoutSettings settings, bool canOrderTotalCheck = true, List<ShoppingCartItem> shoppingCart = null)
        {
            //    var customer = await _workContext.GetCurrentCustomerAsync();
            //    if (shoppingCart == null)
            //    {
            //        shoppingCart = (await _shoppingCartService
            //             .GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, _storeContext.GetCurrentStore().Id))
            //             .ToList();
            //    }
            //    else if(shoppingCart.Count>0)
            //    {
            //        customer = await _customerService.GetCustomerByIdAsync(shoppingCart[0].CustomerId);
            //    }


            //    if (canOrderTotalCheck)
            //    {
            //        (List<AffirmItem> affirmItems, decimal? shoppingCartTotal, decimal discountTotal, decimal customDuty, decimal shippingTotal, decimal taxTotal, decimal orderTotal) = await GetCartSummarry(shoppingCart, customer);
            //        //client id and secret are required to request services
            //        return !string.IsNullOrEmpty(settings?.PublicApiKey) && !string.IsNullOrEmpty(settings?.PrivateApiKey) && (
            //            orderTotal > (settings.MinimumSubTotalAmount ?? 0));
            //    }
            //    else
            //    {
            return !string.IsNullOrEmpty(settings?.PublicApiKey) && !string.IsNullOrEmpty(settings?.PrivateApiKey);
            // }
        }
        #endregion

        public async Task<(List<AffirmItem> affirmItems, decimal? shoppingCartTotal, decimal discountTotal, decimal customDuty, decimal shippingTotal, decimal taxTotal, decimal orderTotal)> GetCartSummarry(List<ShoppingCartItem> shoppingCart, Customer customer)
        {

            var itemTotal = decimal.Zero;
            var taxTotal = decimal.Zero;
            var customDuty = decimal.Zero;
            var shippingTotal = decimal.Zero;
            decimal? shoppingCartTotal = decimal.Zero;
            decimal orderTotal = decimal.Zero;
            decimal discountTotal = decimal.Zero;
            var items = await shoppingCart.SelectAwait(async item =>
            {
                var product = await _productService.GetProductByIdAsync(item.ProductId);

              //  var (_unitPrice, _oldprice, _msrp, _, _) = await _shoppingCartService.GetCustomUnitPriceAsync(product,
              //customer,
              // item.ShoppingCartType,
              //  1, item.AttributesXml, 0,
              // item.RentalStartDateUtc, item.RentalEndDateUtc, false);

                //(_unitPrice, _) = await _taxService.GetProductPriceAsync(product, _unitPrice);
                //if (_oldprice > 0)
                //    (_oldprice, _) = await _taxService.GetProductPriceAsync(product, _oldprice);
                //decimal itemPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(_unitPrice, await _workContext.GetWorkingCurrencyAsync());
                //if (_oldprice > 0)
                //    _oldprice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(_oldprice, await _workContext.GetWorkingCurrencyAsync());

                //if (_oldprice > itemPrice)
                //    itemPrice = _oldprice;


                // itemTotal += itemPrice * item.Quantity;
                itemTotal += product.Price * item.Quantity;

                return new AffirmItem
                {
                    DisplayName = CommonHelper.EnsureMaximumLength(product.Name, 127),
                    Sku = CommonHelper.EnsureMaximumLength(product.Sku, 127),
                    qty = item.Quantity,
                   // unit_price = AffirmHelper.ConvertDecimalToCents(itemPrice)
                    unit_price = AffirmHelper.ConvertDecimalToCents(product.Price)
                };
            }).ToListAsync();

            if (shoppingCart.Count > 0)
            {
                taxTotal = Math.Round((await _orderTotalCalculationService.GetTaxTotalAsync(shoppingCart, false)).taxTotal, 2);
               // customDuty = Math.Round((await _orderTotalCalculationService.GetCustomDuty(shoppingCart)).Item2, 2);
                customDuty = 0;
                shippingTotal = Math.Round(await _orderTotalCalculationService.GetShoppingCartShippingTotalAsync(shoppingCart) ?? decimal.Zero, 2);
                (shoppingCartTotal, _, _, _, _, _) = await _orderTotalCalculationService
                  .GetShoppingCartTotalAsync(shoppingCart, usePaymentMethodAdditionalFee: false);
                orderTotal = Math.Round(shoppingCartTotal ?? decimal.Zero, 2);
                itemTotal = Math.Round(itemTotal, 2);
                discountTotal = Math.Round(itemTotal + taxTotal + shippingTotal + customDuty - orderTotal, 2);
            }
            return (items, shoppingCartTotal, discountTotal, customDuty, shippingTotal, taxTotal, orderTotal + customDuty);

        }
    }
}
