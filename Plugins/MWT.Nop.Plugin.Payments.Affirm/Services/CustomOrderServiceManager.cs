using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using MWT.Nop.Core.Domain.CustomOrders;
using MWT.Nop.Core.Domain.PaymentMethod;
using MWT.Nop.Core.Services.Customers;
using MWT.Nop.Core.Services.Customizations.CustomOrders;
using MWT.Nop.Core.Services.Orders;
using MWT.Nop.Plugin.Payments.Affirm.Domain;
using MWT.Nop.Plugin.Payments.Affirm.Infrastructure;
using MWT.Nop.Plugin.Payments.Affirm.Models;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Services.Attributes;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Stores;
using Nop.Services.Tax;
using System.Globalization;


namespace MWT.Nop.Plugin.Payments.Affirm.Services
{
    public class CustomOrderServiceManager
    {
        #region Fields

        private readonly CurrencySettings _currencySettings;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IAddressService _addresService;
        protected readonly IAttributeParser<CheckoutAttribute, CheckoutAttributeValue> _checkoutAttributeParser;
        private readonly ICountryService _countryService;
        private readonly ICurrencyService _currencyService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILogger _logger;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IProductService _productService;
        private readonly IShoppingCartExtendedService _shoppingCartService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly ITaxService _taxService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly ISettingService _settingService;
        private readonly ICustomerExtendedService _customerService;
        private readonly IRepository<PaymentMethodSession> _affirmSessionRepository;
        private readonly IRepository<AffirmLog> _affirmLogRepository;
        private readonly ICustomOrderService _customOrderService;

        #endregion

        #region Ctor

        public CustomOrderServiceManager(CurrencySettings currencySettings,
            IActionContextAccessor actionContextAccessor,
            IAddressService addresService,
             IAttributeParser<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeParser,
            ICountryService countryService,
            ICurrencyService currencyService,
            IGenericAttributeService genericAttributeService,
            ILogger logger,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IProductService productService,
            IShoppingCartExtendedService shoppingCartService,
            IStateProvinceService stateProvinceService,
            IStoreContext storeContext,
            IStoreService storeService,
            ITaxService taxService,
            IUrlHelperFactory urlHelperFactory,
            IWebHelper webHelper,
            IWorkContext workContext,
            ISettingService settingService,
            ICustomerExtendedService customerService,
            IRepository<PaymentMethodSession> affirmSessionRepository,
            IRepository<AffirmLog> affirmLogRepository,
            ICustomOrderService customOrderService
            )
        {
            _currencySettings = currencySettings;
            _actionContextAccessor = actionContextAccessor;
            _addresService = addresService;
            _checkoutAttributeParser = checkoutAttributeParser;
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
            _customOrderService = customOrderService;
        }

        #endregion

        #region Methods

        public async Task<PaymentInfoModel> CheckoutInit(Guid orderGuid, dynamic additionalData)
        {


            decimal subTotal = 0;
            decimal subtotalDiscount = 0;
            decimal shipping = 0;
            decimal shippingDiscount = 0;

            decimal wgs = 0;
            decimal tax = 0;
            decimal payableAmount = 0;
            List<AffirmItem> cartItems = new List<AffirmItem>();
            PaymentInfoModel model = new PaymentInfoModel();
            var storeLocation = _webHelper.GetStoreLocation();
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var affirmCheckoutSettings = await _settingService.LoadSettingAsync<AffirmCheckoutSettings>(storeScope);
            if (!await IsConfigured(affirmCheckoutSettings))
                throw new NopException("Plugin not configured");


            int _customOrderId = 0;

            int.TryParse(Convert.ToString(additionalData.Id), out _customOrderId);
            var customOrder = await _customOrderService.GetById(_customOrderId);


            if (customOrder.Shipping != null && customOrder.Shipping > 0)
                shipping = Convert.ToDecimal(customOrder.Shipping);
            if (customOrder.Wgs != null && customOrder.Wgs > 0)
                wgs = Convert.ToDecimal(customOrder.Wgs);
            if (customOrder.OrderTax != null && customOrder.OrderTax > 0)
                tax = Convert.ToDecimal(customOrder.OrderTax);

            decimal.TryParse(Convert.ToString(additionalData.CustomDuty), NumberStyles.Currency,
     CultureInfo.CurrentCulture.NumberFormat, out decimal customDuty);
            tax += customDuty;


            if (!string.IsNullOrEmpty(Convert.ToString(additionalData.SubTotal)))
            {
                decimal.TryParse(Convert.ToString(additionalData.SubTotal), NumberStyles.Currency,
             CultureInfo.CurrentCulture.NumberFormat, out subTotal);


            }

            if (!string.IsNullOrEmpty(Convert.ToString(additionalData.PayableAmount)))
            {
                decimal.TryParse(Convert.ToString(additionalData.PayableAmount), NumberStyles.Currency,
             CultureInfo.CurrentCulture.NumberFormat, out payableAmount);
            }


            if (additionalData.SubTotalDiscountDetails != null)
            {
                if (!string.IsNullOrEmpty(Convert.ToString(additionalData.SubTotalDiscountDetails.TotalAdjustment)))
                {
                    decimal.TryParse(Convert.ToString(additionalData.SubTotalDiscountDetails.TotalAdjustment), NumberStyles.Currency,
                 CultureInfo.CurrentCulture.NumberFormat, out subtotalDiscount);



                    if (Convert.ToString(additionalData.SubTotalDiscountDetails.ChargeType) == ChargeType.Add.ToString())
                    {
                        subTotal = subTotal + subtotalDiscount;
                        subtotalDiscount = 0;
                    }
                }
            }




            if (additionalData.ShippingDiscountDetails != null)
            {
                if (!string.IsNullOrEmpty(Convert.ToString(additionalData.ShippingDiscountDetails.TotalAdjustment)))
                {
                    decimal.TryParse(Convert.ToString(additionalData.ShippingDiscountDetails.TotalAdjustment), NumberStyles.Currency,
                 CultureInfo.CurrentCulture.NumberFormat, out shippingDiscount);

                    if (Convert.ToString(additionalData.ShippingDiscountDetails.ChargeType) == ChargeType.Add.ToString())
                    {
                        shipping = shipping + shippingDiscount;
                        shippingDiscount = 0;
                    }
                    else
                    {
                        if (shippingDiscount < 0)
                        {
                            shipping = shipping + shippingDiscount;
                            shippingDiscount = 0;
                        }
                        else
                        {

                            shipping = shipping - shippingDiscount;
                            shippingDiscount = 0;
                        }



                    }
                }

            }


            shipping = shipping + wgs;
            int.TryParse(Convert.ToString(customOrder.CustomerId), out int customerId);


            var customer = await _customerService.GetCustomerByIdAsync(customerId);


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
            shippingLastName = string.IsNullOrEmpty(shippingLastName) ? shippingFirstName : shippingLastName;
            string shippingAddress1 = shippingAddress.Address1 ?? string.Empty;
            string shippingAddress2 = shippingAddress.Address2 ?? string.Empty;
            string shippingCity = shippingAddress?.City ?? string.Empty;
            string shippingState = shipStateProvince?.Abbreviation;
            string shippingZipCode = shippingAddress?.ZipPostalCode ?? string.Empty;
            string shippingCountry = shipCountry?.Name ?? string.Empty;
            string shippingEmail = shippingAddress?.Email ?? await _customerService.GetCustomerEmailAsync(customer,false,true) ?? string.Empty;
            string shippingPhonenumber = shippingAddress?.PhoneNumber ?? await _customerService.GetCustomerPhoneAsync(customer,false,true) ?? string.Empty;

            string billingFirstName = billingAddress.FirstName ?? shippingAddress.FirstName ?? customer.FirstName ?? string.Empty;
            string billingLastName = billingAddress.LastName ?? shippingAddress.LastName ?? customer.LastName ?? string.Empty;
            billingLastName = string.IsNullOrEmpty(billingLastName) ? billingFirstName : billingLastName;
            string billingAddress1 = billingAddress.Address1 ?? shippingAddress.Address1 ?? string.Empty;
            string billingAddress2 = billingAddress.Address2 ?? shippingAddress.Address2 ?? string.Empty;
            string billingCity = billingAddress?.City ?? shippingAddress?.City ?? string.Empty;
            string billingState = billStateProvince?.Abbreviation ?? shipStateProvince?.Abbreviation ?? string.Empty;
            string billingZipCode = billingAddress?.ZipPostalCode ?? shippingAddress?.ZipPostalCode ?? string.Empty;
            string billingCountry = billCountry?.Name ?? shipCountry?.Name ?? string.Empty;
            string billingEmail = billingAddress?.Email ?? await _customerService.GetCustomerEmailAsync(customer,true,false) ?? string.Empty;

            string billingPhonenumber = billingAddress?.PhoneNumber ?? await _customerService.GetCustomerPhoneAsync(customer,true,false) ?? string.Empty;

            var itemTotal = decimal.Zero;
            var items = await _customOrderService.GetOrderItems(customOrder.Id);

            foreach (var item in items)
            {
                string notes = "";

                var product = await _productService.GetProductByIdAsync(item.ProductId);
                var _priceAdjustment = await _customOrderService.GetPriceAdjustmentsByCartId(item.Id);
                decimal price = 0;
                decimal totalAdjustment = 0;


                if (_priceAdjustment != null)
                {
                    decimal discountAmount = _priceAdjustment.Discountamount == null ? 0 : Convert.ToDecimal(_priceAdjustment.Discountamount);
                    decimal discountPercentage = _priceAdjustment.DiscountPercentage == null ? 0 : Convert.ToDecimal(_priceAdjustment.DiscountPercentage);
                    price = _priceAdjustment.ShoppingCartProductPrice == null ? 0 : Convert.ToDecimal(_priceAdjustment.ShoppingCartProductPrice);
                    totalAdjustment = (_priceAdjustment.Discounttype == DiscountType.Percentage.ToString() && discountPercentage != 0 ?
                       (_priceAdjustment.ShoppingCartProductPrice == null ? 0 : Convert.ToDecimal(_priceAdjustment.ShoppingCartProductPrice)
                       * Convert.ToDecimal(_priceAdjustment.DiscountPercentage)) / 100
                       : (_priceAdjustment.Discounttype == DiscountType.Fixed.ToString() && discountAmount != 0
                       ? discountAmount : 0));


                    if (_priceAdjustment.Chargestype == ChargeType.Subtract.ToString())
                        totalAdjustment = -totalAdjustment;


                    notes = _priceAdjustment.AdjustmentNotes;

                    cartItems.Add(new AffirmItem()
                    {
                        DisplayName = CommonHelper.EnsureMaximumLength(product.Name, 127),
                        Sku = CommonHelper.EnsureMaximumLength(product.Sku, 127),
                        qty = item.Quantity,
                        unit_price = AffirmHelper.ConvertDecimalToCents(price + totalAdjustment)

                    });
                }
            }


            itemTotal = Math.Round(subTotal, 2);
            decimal.TryParse(Convert.ToString(additionalData.OrderTotal), NumberStyles.Currency,
             CultureInfo.CurrentCulture.NumberFormat, out decimal orderTotal);
            var discountTotal = Math.Round(subtotalDiscount, 2);
            if (orderTotal > 0)
            {
                discountTotal = discountTotal + (orderTotal - payableAmount);
            }
            tax = Math.Round(tax, 2);
            if (Math.Round((itemTotal + tax + shipping) - discountTotal, 2) != payableAmount)
            {
                discountTotal = discountTotal + (((itemTotal + tax + shipping) - discountTotal) - payableAmount);
            }



            #region Take Log of data
            var affirmSession = new PaymentMethodSession()
            {
                CartItems = JsonConvert.SerializeObject(cartItems),
                CartTotal = itemTotal,
                Checkout_Token = string.Empty,
                OrderTotal = payableAmount,
                DiscountTotal = discountTotal,
                LiveOrderNumber = 0,
                IsCustomOrder = true,
                ShipingTotal = shipping,
                Country = shippingCountry,
                CustomDutyTotal = customDuty,
                ShippingMethod = string.Empty,
                CustomerId = customer.Id,
                TaxTotal = tax,
                Status = RequestStatus.Init,
                ZipCode = shippingZipCode,
                Coupon = string.Empty,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                CustomOrderId = _customOrderId
            };


            await _affirmSessionRepository.InsertAsync(affirmSession);

            #endregion
            var payload = new
            {
                merchant = new
                {
                    public_api_key = affirmCheckoutSettings.PublicApiKey,
                    user_confirmation_url = $"{storeLocation}Admin/Affirm/CustomOrderConfirmCallbackHandler?orderid={_customOrderId}&customerId={customer.Id}",
                    user_cancel_url = $"{storeLocation}Admin/Affirm/CustomOrderCancelCallbackHandler?orderid={_customOrderId}",
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
                items = cartItems.Select(item => new
                {
                    display_name = item.DisplayName,
                    sku = item.Sku,
                    unit_price = item.unit_price, // Convert price to cents
                    qty = item.qty
                }),
                discounts = new
                {
                    Discount = new
                    {
                        discount_amount = AffirmHelper.ConvertDecimalToCents(discountTotal < 0 ? -discountTotal : discountTotal),
                        discount_display_name = "Discount"
                    }
                },
                meta = new
                {
                    customerid = customer.Id,
                    tempOrderId = affirmSession.Id,
                    orderGuid = orderGuid.ToString(),
                    customOrderId = _customOrderId
                    // Another custom property
                },
                currency = currency,
                shipping_amount = AffirmHelper.ConvertDecimalToCents(shipping),
                tax_amount = AffirmHelper.ConvertDecimalToCents(tax),
                total = AffirmHelper.ConvertDecimalToCents(payableAmount),
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
        public async Task<bool> IsConfigured(AffirmCheckoutSettings settings, bool canOrderTotalCheck = true)
        {
            var shoppingCart = (await _shoppingCartService
              .GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, _storeContext.GetCurrentStore().Id))
              .ToList();
            return !string.IsNullOrEmpty(settings?.PublicApiKey) && !string.IsNullOrEmpty(settings?.PrivateApiKey);

        }
        #endregion

    }
}
