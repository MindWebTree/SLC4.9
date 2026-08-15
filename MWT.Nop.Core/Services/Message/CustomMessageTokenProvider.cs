using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using MWT.Nop.Core.Domain.PhoneOrder;
using MWT.Nop.Core.Infrastructure;
using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Services.Catalog;
using MWT.Nop.Core.Services.Customers;
using MWT.Nop.Core.Services.Media;
using MWTNop.Core.Domain.Catalog;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
using Nop.Core.Infrastructure;
using Nop.Services.Attributes;
using Nop.Services.Blogs;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.News;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Tax;
using System.Net;
using System.Text;


namespace MWT.Nop.Core.Services.Message
{
    /// <summary>
    /// Message token provider
    /// </summary>
    public partial class CustomMessageTokenProvider : MessageTokenProvider, ICustomMessageTokenProvider
    {
        private readonly ICustomerExtendedService _customCustomerService;
        private readonly IPictureExtendedService _pictureService;
        private readonly MediaSettings _mediaSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHelper _webHelper;
        private readonly ICustomProductAttributeFormatter _productAttributeFormatter;
        private readonly IShoppingCartExtendedCartService _shoppingCartService;
        private readonly IProductExtendedService _customProductService;
        private readonly ITaxService _taxService;
        private readonly ICategoryService _categoryService;
        private readonly IEncryptionService _encryptionService;
        public CustomMessageTokenProvider(CatalogSettings catalogSettings, CurrencySettings currencySettings, IActionContextAccessor actionContextAccessor, IAddressService addressService, IAttributeFormatter<AddressAttribute,
            AddressAttributeValue> addressAttributeFormatter, IAttributeFormatter<CustomerAttribute, CustomerAttributeValue> customerAttributeFormatter, IAttributeFormatter<VendorAttribute, VendorAttributeValue> vendorAttributeFormatter, IBlogService blogService, ICountryService countryService, ICurrencyService currencyService, ICustomerService customerService, IDateTimeHelper dateTimeHelper, IEventPublisher eventPublisher, IGenericAttributeService genericAttributeService, IGiftCardService giftCardService, IHtmlFormatter htmlFormatter, ILanguageService languageService, ILocalizationService localizationService, ILogger logger, INewsService newsService, IOrderService orderService, IPaymentPluginManager paymentPluginManager, IPaymentService paymentService, IPriceFormatter priceFormatter, IProductService productService, IRewardPointService rewardPointService, IShipmentService shipmentService, IStateProvinceService stateProvinceService, IStoreContext storeContext, IStoreService storeService, IUrlHelperFactory urlHelperFactory, IUrlRecordService urlRecordService, IWorkContext workContext, MessageTemplatesSettings templatesSettings, PaymentSettings paymentSettings, StoreInformationSettings storeInformationSettings,
            TaxSettings taxSettings, ICustomerExtendedService customCustomerService,
            IPictureExtendedService pictureService, MediaSettings mediaSettings,
            IHttpContextAccessor httpContextAccessor, IWebHelper webHelper,
            ICustomProductAttributeFormatter productAttributeFormatter, IShoppingCartExtendedCartService shoppingCartService,
            ITaxService taxService, ICategoryService categoryService, IEncryptionService encryptionService, IProductExtendedService customProductService) : base(catalogSettings, currencySettings, actionContextAccessor, addressService, addressAttributeFormatter, customerAttributeFormatter, vendorAttributeFormatter, blogService, countryService, currencyService, customerService, dateTimeHelper, eventPublisher, genericAttributeService, giftCardService, htmlFormatter, languageService, localizationService, logger, newsService, orderService, paymentPluginManager, paymentService, priceFormatter, productService, rewardPointService, shipmentService, stateProvinceService, storeContext, storeService, urlHelperFactory, urlRecordService, workContext, templatesSettings, paymentSettings, storeInformationSettings, taxSettings)
        {
            _customCustomerService = customCustomerService;
            _pictureService = pictureService;
            _mediaSettings = mediaSettings;
            _httpContextAccessor = httpContextAccessor;
            _webHelper = webHelper;
            _productAttributeFormatter = productAttributeFormatter;
            _shoppingCartService = shoppingCartService;
            _taxService = taxService;
            _categoryService = categoryService;
            _encryptionService = encryptionService;
            _customProductService = customProductService;
        }

        #region Methods

        public virtual async Task CustomAddOrderTokensAsync(IList<Token> tokens, Order order, int languageId, int vendorId = 0)
        {
            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
            //lambda expression for choosing correct order address
            async Task<Address> orderAddress(Order o) => await _addressService.GetAddressByIdAsync((o.PickupInStore ? o.PickupAddressId : o.ShippingAddressId) ?? 0);

            var taxRates = _orderService.ParseTaxRates(order, order.TaxRates);

            var taxPercentage = _priceFormatter.FormatTaxRate(taxRates.Count > 0 ?
                taxRates.FirstOrDefault().Key : 0) + "%";
            tokens.Add(new Token("Order.TaxPercentage", taxPercentage));
            var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);

            tokens.Add(new Token("Order.OrderId", order.Id));
            tokens.Add(new Token("Order.OrderNumber", order.CustomOrderNumber));
            tokens.Add(new Token("Order.OrderGuid", order.OrderGuid.ToString()));
            tokens.Add(new Token("Order.TransactionId", order.CaptureTransactionId ?? ""));
            tokens.Add(new Token("Order.CustomerFullName", $"{billingAddress.FirstName} {billingAddress.LastName}"));
            tokens.Add(new Token("Order.CustomerEmail", billingAddress.Email));

            tokens.Add(new Token("Order.BillingFirstName", billingAddress.FirstName));
            tokens.Add(new Token("Order.BillingLastName", billingAddress.LastName));
            tokens.Add(new Token("Order.BillingPhoneNumber", billingAddress.PhoneNumber));
            tokens.Add(new Token("Order.BillingEmail", billingAddress.Email ?? await _customCustomerService.GetCustomerEmail(customer)));
            tokens.Add(new Token("Order.BillingFaxNumber", billingAddress.FaxNumber));
            tokens.Add(new Token("Order.BillingCompany", billingAddress.Company));
            tokens.Add(new Token("Order.BillingAddress1", billingAddress.Address1));
            tokens.Add(new Token("Order.BillingAddress2", billingAddress.Address2));
            tokens.Add(new Token("Order.BillingCity", billingAddress.City));
            tokens.Add(new Token("Order.HasCreditCardNumber", order != null ? string.IsNullOrEmpty(order.MaskedCreditCardNumber) ? false : true : false));
            tokens.Add(new Token("Order.CreditCardNumber", string.IsNullOrEmpty(order.MaskedCreditCardNumber) ? "" : _encryptionService.DecryptText(order.MaskedCreditCardNumber)));
            tokens.Add(new Token("Order.BillingCounty", billingAddress.County));
            tokens.Add(new Token("Order.BillingStateProvince", await _stateProvinceService.GetStateProvinceByAddressAsync(billingAddress) is StateProvince billingStateProvince ? await _localizationService.GetLocalizedAsync(billingStateProvince, x => x.Name) : string.Empty));
            tokens.Add(new Token("Order.BillingZipPostalCode", billingAddress.ZipPostalCode));
            tokens.Add(new Token("Order.BillingCountry", await _countryService.GetCountryByAddressAsync(billingAddress) is Country billingCountry ? await _localizationService.GetLocalizedAsync(billingCountry, x => x.Name) : string.Empty));
            tokens.Add(new Token("Order.BillingCustomAttributes", await _addressAttributeFormatter.FormatAttributesAsync(billingAddress.CustomAttributes), true));

            tokens.Add(new Token("Order.Shippable", !string.IsNullOrEmpty(order.ShippingMethod)));
            tokens.Add(new Token("Order.ShippingMethod", order.ShippingMethod));
            tokens.Add(new Token("Order.PickupInStore", order.PickupInStore));
            tokens.Add(new Token("Order.ShippingFirstName", (await orderAddress(order))?.FirstName ?? string.Empty));
            tokens.Add(new Token("Order.ShippingLastName", (await orderAddress(order))?.LastName ?? string.Empty));
            tokens.Add(new Token("Order.ShippingPhoneNumber", (await orderAddress(order))?.PhoneNumber ?? string.Empty));
            tokens.Add(new Token("Order.ShippingEmail", (await orderAddress(order))?.Email ?? await _customCustomerService.GetCustomerEmail(customer)));
            tokens.Add(new Token("Order.ShippingFaxNumber", (await orderAddress(order))?.FaxNumber ?? string.Empty));
            tokens.Add(new Token("Order.ShippingCompany", (await orderAddress(order))?.Company ?? string.Empty));
            tokens.Add(new Token("Order.ShippingAddress1", (await orderAddress(order))?.Address1 ?? string.Empty));
            tokens.Add(new Token("Order.ShippingAddress2", (await orderAddress(order))?.Address2 ?? string.Empty));
            tokens.Add(new Token("Order.ShippingCity", (await orderAddress(order))?.City ?? string.Empty));
            tokens.Add(new Token("Order.ShippingCounty", (await orderAddress(order))?.County ?? string.Empty));
            tokens.Add(new Token("Order.ShippingStateProvince", await _stateProvinceService.GetStateProvinceByAddressAsync(await orderAddress(order)) is StateProvince shippingStateProvince ? await _localizationService.GetLocalizedAsync(shippingStateProvince, x => x.Name) : string.Empty));
            tokens.Add(new Token("Order.ShippingZipPostalCode", (await orderAddress(order))?.ZipPostalCode ?? string.Empty));
            tokens.Add(new Token("Order.ShippingCountry", await _countryService.GetCountryByAddressAsync(await orderAddress(order)) is Country orderCountry ? await _localizationService.GetLocalizedAsync(orderCountry, x => x.Name) : string.Empty));
            tokens.Add(new Token("Order.ShippingCustomAttributes", await _addressAttributeFormatter.FormatAttributesAsync((await orderAddress(order))?.CustomAttributes ?? string.Empty), true));

            var paymentMethod = await _paymentPluginManager.LoadPluginBySystemNameAsync(order.PaymentMethodSystemName);
            var paymentMethodName = paymentMethod != null ? await _localizationService.GetLocalizedFriendlyNameAsync(paymentMethod, (await _workContext.GetWorkingLanguageAsync()).Id) : await this._localizationService.GetResourceAsync(order.PaymentMethodSystemName);
            tokens.Add(new Token("Order.PaymentMethod", await _localizationService.GetResourceAsync(paymentMethodName)));
            tokens.Add(new Token("Order.VatNumber", order.VatNumber));
            var sbCustomValues = new StringBuilder();

            // need to confirm
            //var customValues = _paymentService.DeserializeCustomValues(order);
            //if (customValues != null)
            //{
            //    foreach (var item in customValues)
            //    {
            //        sbCustomValues.AppendFormat("{0}: {1}", WebUtility.HtmlEncode(item.Key), WebUtility.HtmlEncode(item.Value != null ? item.Value.ToString() : string.Empty));
            //        sbCustomValues.Append("<br />");
            //    }
            //}

            tokens.Add(new Token("Order.CustomValues", sbCustomValues.ToString(), true));

            tokens.Add(new Token("Order.Product(s)", await CustomProductListToHtmlTableAsync(order, languageId, vendorId), true));

            await this.AddOrderTotalTokens(tokens, order, (await orderAddress(order))?.ZipPostalCode ?? string.Empty, taxPercentage, vendorId, languageId);


            var language = await _languageService.GetLanguageByIdAsync(languageId);
            if (language != null && !string.IsNullOrEmpty(language.LanguageCulture))
            {

                var createdOn = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, TimeZoneInfo.Utc, await _dateTimeHelper.GetCustomerTimeZoneAsync(customer));
                tokens.Add(new Token("Order.CreatedOn", createdOn.ToString("MM/dd/yyyy")));
            }
            else
            {
                tokens.Add(new Token("Order.CreatedOn", order.CreatedOnUtc.ToString("MM/dd/yyyy")));
            }

            var orderUrl = await RouteUrlAsync(order.StoreId, "OrderDetails", new { orderId = order.Id });
            tokens.Add(new Token("Order.OrderURLForCustomer", orderUrl, true));



            //event notification
            await _eventPublisher.EntityTokensAddedAsync(order, tokens);
        }

        public async Task AddStoreLogoToken(IList<Token> tokens)
        {



            var logo = string.Empty;
            var logoPictureId = _storeInformationSettings.LogoPictureId;

            if (logoPictureId > 0)
                logo = await _pictureService.GetPictureUrlAsync(logoPictureId, showDefaultPicture: false);

            if (string.IsNullOrEmpty(logo))
            {
                //use default logo
                var pathBase = _httpContextAccessor.HttpContext.Request.PathBase.Value ?? string.Empty;
                var storeLocation = _mediaSettings.UseAbsoluteImagePath ? _webHelper.GetStoreLocation() : $"{pathBase}/";
                logo = $"{storeLocation}Themes/{_storeInformationSettings.DefaultStoreTheme}/Content/images/logo.png";
            }

            tokens.Add(new Token("Store.Logo", logo, true));

        }


        public virtual async Task CustomAddCustomerTokensAsync(IList<Token> tokens, int customerId)
        {
            if (customerId <= 0)
                throw new ArgumentOutOfRangeException(nameof(customerId));

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            await CustomAddCustomerTokensAsync(tokens, customer);
        }

        /// <summary>
        /// Add customer tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="customer">Customer</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task CustomAddCustomerTokensAsync(IList<Token> tokens, Customer customer)
        {
            tokens.Add(new Token("Customer.CustomerId", customer.Id));
            tokens.Add(new Token("Customer.Email", customer.Email));
            tokens.Add(new Token("Customer.Username", customer.Username));
            tokens.Add(new Token("Customer.FullName", await _customerService.GetCustomerFullNameAsync(customer)));
            tokens.Add(new Token("Customer.FirstName", customer.FirstName));
            tokens.Add(new Token("Customer.LastName", customer.LastName));
            tokens.Add(new Token("Customer.VatNumber", customer.VatNumber));
            tokens.Add(new Token("Customer.VatNumberStatus", customer.VatNumberStatus));
            tokens.Add(new Token("Customer.CustomAttributes", await _customerAttributeFormatter.FormatAttributesAsync(customer.CustomCustomerAttributesXML), true));

            //note: we do not use SEO friendly URLS for these links because we can get errors caused by having .(dot) in the URL (from the email address)
            var passwordRecoveryUrl = await RouteUrlAsync(routeName: "PasswordRecoveryConfirm", routeValues: new { token = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.PasswordRecoveryTokenAttribute), guid = customer.CustomerGuid });
            var accountActivationUrl = await RouteUrlAsync(routeName: "AccountActivation", routeValues: new { token = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.AccountActivationTokenAttribute), guid = customer.CustomerGuid });
            var emailRevalidationUrl = await RouteUrlAsync(routeName: "EmailRevalidation", routeValues: new { token = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.EmailRevalidationTokenAttribute), guid = customer.CustomerGuid });
            var wishlistUrl = await RouteUrlAsync(routeName: "Wishlist", routeValues: new { customerGuid = customer.CustomerGuid });
            tokens.Add(new Token("Customer.PasswordRecoveryURL", passwordRecoveryUrl, true));
            tokens.Add(new Token("Customer.AccountActivationURL", accountActivationUrl, true));
            tokens.Add(new Token("Customer.EmailRevalidationURL", emailRevalidationUrl, true));
            tokens.Add(new Token("Wishlist.URLForCustomer", wishlistUrl, true));

            //event notification
            await _eventPublisher.EntityTokensAddedAsync(customer, tokens);
        }

        #region CustomOrder

        public async Task WgsAdditionalServiceAddTokenAsync(IList<Token> tokens, CustomOrder customOrder, int languageId, int vendorId = 0)
        {
            //var _customOrderService = EngineContext.Current.Resolve<ICustomOrderService>();
            //List<CustomOrderShoppingCartItem> cartItems = await _customOrderService.GetOrderItems(Convert.ToInt32(customOrder.Id));
            //bool hasDiscount = false;
            //decimal totalWithoutDiscount = customOrder.OrderTotal ?? 0;
            //foreach (var item in cartItems)
            //{
            //    var priceAdjustment = await _customOrderService.GetPriceAdjustmentsByCartId(Convert.ToInt32(item.Id));
            //    if (priceAdjustment != null)
            //    {
            //        if (priceAdjustment.ShoppingCartProductPrice > customOrder.OrderTotal)
            //        {
            //            hasDiscount = true;
            //            totalWithoutDiscount = priceAdjustment.ShoppingCartProductPrice ?? 0;
            //        }
            //    }
            //    break;
            //}
            //var pairedOrderIds = string.IsNullOrEmpty(customOrder.PairedOrderIds) ? customOrder.ParentOrderID.ToString() : string.Join(", ",
            //    (await _orderService.GetOrdersByIdsAsync(customOrder.PairedOrderIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
            //    .Select(id => id.Trim()).Where(id => int.TryParse(id, out _)).Select(int.Parse).Distinct().ToArray())).Select(o => o.Id).Prepend(customOrder.ParentOrderID));


            //tokens.Add(new Token("Order.Subject", pairedOrderIds, true));
            //tokens.Add(new Token("Order.Total.WithoutDiscount", totalWithoutDiscount, true));
            //tokens.Add(new Token("OrderTotalWithoutDiscount", hasDiscount, true));
        }
        public async Task CustomOrderAddTokensAsync(IList<Token> tokens, CustomOrder customOrder, int languageId, int vendorId = 0)
        {

            var _order = new Order();

            int billingAddressId = 0;
            int shippingAddressId = 0;
            var customer = new Customer();
            if (customOrder.LiveOrderNumber != null && customOrder.LiveOrderNumber > 0)
            {

                _order = await _orderService.GetOrderByIdAsync(Convert.ToInt32(customOrder.LiveOrderNumber));

                tokens.Add(new Token("Order.OrderGuid", _order?.OrderGuid.ToString() ?? ""));
                tokens.Add(new Token("Order.TransactionId", _order?.CaptureTransactionId ?? ""));
                tokens.Add(new Token("Order.HasCreditCardNumber", _order != null ? string.IsNullOrEmpty(_order.MaskedCreditCardNumber) ? false : true : false));

                tokens.Add(new Token("Order.CreditCardNumber", _order != null ? string.IsNullOrEmpty(_order.MaskedCreditCardNumber) ? "" : _encryptionService.DecryptText(_order.MaskedCreditCardNumber) : ""));
                billingAddressId = _order.BillingAddressId;
                shippingAddressId = _order.ShippingAddressId ?? 0;
                customer = await _customerService.GetCustomerByIdAsync(Convert.ToInt32(_order.CustomerId));
            }
            else
            {

                if (customOrder.CustomerId != null && customOrder.CustomerId > 0)
                {
                    customer = await _customerService.GetCustomerByIdAsync(Convert.ToInt32(customOrder.CustomerId));
                    billingAddressId = customer.BillingAddressId ?? 0;
                    shippingAddressId = customer.ShippingAddressId ?? 0;
                }
            }


            var billingAddress = await _addressService.GetAddressByIdAsync(billingAddressId);

            var shippingAddress = await _addressService.GetAddressByIdAsync(shippingAddressId);
            if (billingAddress == null)
                billingAddress = shippingAddress;
            tokens.Add(new Token("Checkout.Link", $"checkoutCustomOrder?orderid={customOrder.Id}&customerid={customOrder.CustomerId}"));
            tokens.Add(new Token("Order.OrderId", customOrder.LiveOrderNumber));
            tokens.Add(new Token("Order.ParentOrderID", customOrder.ParentOrderID));
            tokens.Add(new Token("Order.OrderNumber", customOrder.Id));

            tokens.Add(new Token("Order.CustomerFullName", $"{billingAddress?.FirstName} {billingAddress?.LastName}"));
            tokens.Add(new Token("Order.CustomerEmail", billingAddress?.Email));

            tokens.Add(new Token("Order.BillingFirstName", billingAddress?.FirstName));
            tokens.Add(new Token("Order.BillingLastName", billingAddress?.LastName));
            tokens.Add(new Token("Order.BillingPhoneNumber", billingAddress?.PhoneNumber));
            tokens.Add(new Token("Order.BillingEmail", billingAddress?.Email ?? await _customCustomerService.GetCustomerEmail(customer)));
            tokens.Add(new Token("Order.BillingFaxNumber", billingAddress?.FaxNumber));
            tokens.Add(new Token("Order.BillingCompany", billingAddress?.Company));
            tokens.Add(new Token("Order.BillingAddress1", billingAddress?.Address1));
            tokens.Add(new Token("Order.BillingAddress2", billingAddress?.Address2));
            tokens.Add(new Token("Order.BillingCity", billingAddress?.City));
            tokens.Add(new Token("Order.BillingCounty", billingAddress?.County));
            tokens.Add(new Token("Order.BillingStateProvince", billingAddress?.StateProvinceId == null ? "" :
                await _stateProvinceService.GetStateProvinceByAddressAsync(billingAddress) is StateProvince billingStateProvince ? await _localizationService.GetLocalizedAsync(billingStateProvince, x => x.Name) : string.Empty));
            tokens.Add(new Token("Order.BillingZipPostalCode", billingAddress?.ZipPostalCode));
            tokens.Add(new Token("Order.BillingCountry", billingAddress?.CountryId == null ? "" : await _countryService.GetCountryByAddressAsync(billingAddress) is Country billingCountry ? await _localizationService.GetLocalizedAsync(billingCountry, x => x.Name) : string.Empty));
            tokens.Add(new Token("Order.BillingCustomAttributes", await _addressAttributeFormatter.FormatAttributesAsync(billingAddress?.CustomAttributes), true));

            tokens.Add(new Token("Order.Shippable", true));
            tokens.Add(new Token("Order.ShippingMethod", customOrder.Wgs > 0 || customOrder.ComplementryWgsFree ? "White Glove Service" : "Curb Side Delivery"));
            tokens.Add(new Token("Order.TaxPercentage", _priceFormatter.FormatTaxRate(customOrder.TaxRate) + "%"));
            tokens.Add(new Token("Order.PickupInStore", ""));
            tokens.Add(new Token("Order.ShippingFirstName", shippingAddress?.FirstName));
            tokens.Add(new Token("Order.ShippingLastName", shippingAddress?.LastName));
            tokens.Add(new Token("Order.ShippingPhoneNumber", shippingAddress?.PhoneNumber));
            tokens.Add(new Token("Order.ShippingEmail", shippingAddress?.Email ?? await _customCustomerService.GetCustomerEmail(customer)));
            tokens.Add(new Token("Order.ShippingFaxNumber", shippingAddress?.FaxNumber));
            tokens.Add(new Token("Order.ShippingCompany", shippingAddress?.Company));
            tokens.Add(new Token("Order.ShippingAddress1", shippingAddress?.Address1));
            tokens.Add(new Token("Order.ShippingAddress2", shippingAddress?.Address2));
            tokens.Add(new Token("Order.ShippingCity", shippingAddress?.City));
            tokens.Add(new Token("Order.ShippingCounty", shippingAddress?.County));
            tokens.Add(new Token("Order.ShippingStateProvince", shippingAddress?.StateProvinceId == null ? "" :
                await _stateProvinceService.GetStateProvinceByAddressAsync(shippingAddress) is StateProvince shippingStateProvince ? await _localizationService.GetLocalizedAsync(shippingStateProvince, x => x.Name) : string.Empty));
            tokens.Add(new Token("Order.ShippingZipPostalCode", shippingAddress?.ZipPostalCode));
            tokens.Add(new Token("Order.ShippingCountry", shippingAddress?.CountryId == null ? "" : await _countryService.GetCountryByAddressAsync(shippingAddress) is Country orderCountry ? await _localizationService.GetLocalizedAsync(orderCountry, x => x.Name) : string.Empty));
            tokens.Add(new Token("Order.ShippingCustomAttributes", await _addressAttributeFormatter.FormatAttributesAsync(shippingAddress?.CustomAttributes ?? string.Empty), true));

            string paymentMethodName = "";
            if (_order.Id != 0)
            {
                var paymentMethod = await _paymentPluginManager.LoadPluginBySystemNameAsync(_order.PaymentMethodSystemName);
                paymentMethodName = paymentMethod != null ? await _localizationService.GetLocalizedFriendlyNameAsync(paymentMethod, (await _workContext.GetWorkingLanguageAsync()).Id) : _order.PaymentMethodSystemName;
            }
            if (customOrder.LiveOrderNumber != null && customOrder.LiveOrderNumber > 0)
            {
                //var _customOrderService = EngineContext.Current.Resolve<Nop.Services.Customizations.Phone_Order.ICustomOrderService>();
                //var orderTypes = await _customOrderService.GetOrderTypes();
                //string OrderType = (orderTypes.Where(o => o.Id == customOrder.OrderTypeId)).FirstOrDefault()?.Name;

                //if (OrderType == OrderTypes.AlreadyPaid.ToString())
                //{
                //    paymentMethodName = "OrderType." + OrderTypes.AlreadyPaid.ToString();
                //}
            }
            tokens.Add(new Token("Order.PaymentMethod", await _localizationService.GetResourceAsync(paymentMethodName)));
            tokens.Add(new Token("Order.VatNumber", _order.Id == 0 ? "" : _order.VatNumber));
            //if (_order.Id != 0)
            //{
            //    var sbCustomValues = new StringBuilder();
            //    var customValues = _paymentService.DeserializeCustomValues(_order);
            //    if (customValues != null)
            //    {
            //        foreach (var item in customValues)
            //        {
            //            sbCustomValues.AppendFormat("{0}: {1}", WebUtility.HtmlEncode(item.Key), WebUtility.HtmlEncode(item.Value != null ? item.Value.ToString() : string.Empty));
            //            sbCustomValues.Append("<br />");
            //        }
            //    }
            //    tokens.Add(new Token("Order.CustomValues", sbCustomValues.ToString(), true));
            //}



            tokens.Add(new Token("Order.Product(s)", await CustomOrderProductListToHtmlTableAsync(customOrder, languageId, vendorId), true));

            await this.AddCustomOrderTotalTokens(tokens, customOrder, shippingAddress?.ZipPostalCode ?? string.Empty, vendorId, languageId);


            var language = await _languageService.GetLanguageByIdAsync(languageId);
            if (language != null && !string.IsNullOrEmpty(language.LanguageCulture) && customOrder.CustomerId != null)
            {
                if (_order.Id != 0)
                {
                    var createdOn = _dateTimeHelper.ConvertToUserTime(_order.CreatedOnUtc, TimeZoneInfo.Utc, await _dateTimeHelper.GetCustomerTimeZoneAsync(customer));
                    tokens.Add(new Token("Order.CreatedOn", createdOn.ToString("MM/dd/yyyy")));
                }
                else
                {
                    if (customOrder.CreatedOn != null)
                    {
                        var createdOn = _dateTimeHelper.ConvertToUserTime(DateTime.UtcNow, TimeZoneInfo.Utc, await _dateTimeHelper.GetCustomerTimeZoneAsync(customer));
                        tokens.Add(new Token("Order.CreatedOn", createdOn.ToString("MM/dd/yyyy")));
                    }
                    else
                        tokens.Add(new Token("Order.CreatedOn", ""));

                }
            }
            else
            {
                if (customOrder.CreatedOn != null)
                {
                    var createdOn = _dateTimeHelper.ConvertToUserTime(Convert.ToDateTime(DateTime.UtcNow), TimeZoneInfo.Utc, await _dateTimeHelper.GetCustomerTimeZoneAsync(customer));
                    tokens.Add(new Token("Order.CreatedOn", createdOn.ToString("MM/dd/yyyy")));
                }
                else
                    tokens.Add(new Token("Order.CreatedOn", ""));
            }

            if (_order.Id != 0)
            {
                var orderUrl = await RouteUrlAsync(_order.StoreId, "OrderDetails", new { orderId = _order.Id });
                tokens.Add(new Token("Order.OrderURLForCustomer", orderUrl, true));
            }
            else
                tokens.Add(new Token("Order.OrderURLForCustomer", "/", true));


            if (_order.Id != 0)
                await _eventPublisher.EntityTokensAddedAsync(_order, tokens);
        }
        #endregion
        #endregion


        #region Estimated Delivery Date Module

        public void CustomAddShippingToken(IList<Token> tokens, int languageId, string zipCode, string ipAddress, string customerName,
          string customerEmail)
        {
            tokens.Add(new Token("Shipping.ZipCode", zipCode));
            tokens.Add(new Token("Shipping.IpAddress", ipAddress));
            tokens.Add(new Token("Shipping.Name", customerName));
            tokens.Add(new Token("Shipping.Email", customerEmail));
        }

        #endregion

        #region

        public async Task CustomAddOrderDeclineTokensAsync(IList<Token> tokens, Customer customer, IList<ShoppingCartItem> cart, string error, int orderId, int languageId)
        {
            tokens.Add(new Token("Customer.FullName", await _customerService.GetCustomerFullNameAsync(customer)));
            tokens.Add(new Token("Error", error));
            string email = customer.Email;
            if (!string.IsNullOrEmpty(email))
                tokens.Add(new Token("Email", customer.Email));


            string phone = "";
            if (customer.BillingAddressId.HasValue)
            {
                var address = await _addressService.GetAddressByIdAsync(Convert.ToInt32(customer.BillingAddressId));
                if (string.IsNullOrEmpty(email))
                    tokens.Add(new Token("Email", address.Email));
                tokens.Add(new Token("Phone", address.PhoneNumber));
            }
            else if (customer.ShippingAddressId.HasValue)
            {
                var address = await _addressService.GetAddressByIdAsync(Convert.ToInt32(customer.ShippingAddressId));
                if (string.IsNullOrEmpty(email))
                    tokens.Add(new Token("Email", address.Email));
                tokens.Add(new Token("Phone", address.PhoneNumber));
            }


            if (orderId == 0 && cart.Count > 0)
            {
                tokens.Add(new Token("cartProducts", await this.CustomCartProductListToHtmlTableAsync(cart, languageId), true));
            }
            else if (orderId > 0)
            {
                tokens.Add(new Token("cartProducts", await this.CustomCustomOrderCartProductListToHtmlTableAsync(orderId, languageId), true));
            }
        }

        public async Task CustomAddCustomerOrderDeclineTokensAsync(IList<Token> tokens, Customer customer, IList<ShoppingCartItem> cart, int orderId, int languageId)
        {
            //var _customOrderService = EngineContext.Current.Resolve<Nop.Services.Customizations.Phone_Order.ICustomOrderService>();
            //tokens.Add(new Token("Customer.FullName", await _customerService.GetCustomerFullNameAsync(customer)));
            //string email = customer.Email;
            //if (!string.IsNullOrEmpty(email))
            //    tokens.Add(new Token("Email", customer.Email));


            //string phone = "";
            //if (customer.BillingAddressId.HasValue)
            //{
            //    var address = await _addressService.GetAddressByIdAsync(Convert.ToInt32(customer.BillingAddressId));
            //    if (string.IsNullOrEmpty(email))
            //        tokens.Add(new Token("Email", address.Email));
            //    tokens.Add(new Token("Phone", address.PhoneNumber));
            //}
            //else if (customer.ShippingAddressId.HasValue)
            //{
            //    var address = await _addressService.GetAddressByIdAsync(Convert.ToInt32(customer.ShippingAddressId));
            //    if (string.IsNullOrEmpty(email))
            //        tokens.Add(new Token("Email", address.Email));
            //    tokens.Add(new Token("Phone", address.PhoneNumber));
            //}
            //tokens.Add(new Token("OrderDate", DateTime.Now.ToString("MMMM d, yyyy hh:mm tt")));

            //if (orderId == 0)
            //{
            //    var _orderTotalCalculationService = EngineContext.Current.Resolve<IOrderTotalCalculationService>();
            //    var (shoppingCartTotalBase, orderTotalDiscountAmountBase, _, appliedGiftCards, redeemedRewardPoints, redeemedRewardPointsAmount) =
            //                 await _orderTotalCalculationService.GetShoppingCartTotalAsync(cart);
            //    var shoppingCartTotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartTotalBase.Value, await _workContext.GetWorkingCurrencyAsync());

            //    tokens.Add(new Token("OrderTotal", await _priceFormatter.FormatPriceAsync(shoppingCartTotal, true, false)));
            //    tokens.Add(new Token("Checkout.Link", $"ReInitiateCheckout/{customer.Id}"));

            //    if (cart.Count > 0)
            //    {

            //        var product = await _productService.GetProductByIdAsync(cart[0].ProductId);
            //        if (product != null)
            //        {
            //            var prdurl = (await RouteUrlAsync(_storeContext.GetCurrentStore().Id, "product", new
            //            {
            //                id = product.Id,
            //                SeName = await _urlRecordService.GetSeNameAsync(product)
            //            }));
            //            var variant = await _variantService.GetItemVariantInfo(product.Id, cart[0].AttributesXml);

            //            tokens.Add(new Token("Cart.Product.Link", prdurl));
            //            tokens.Add(new Token("Cart.Product.Name", string.IsNullOrWhiteSpace(variant?.Title) ? product.Name : variant.Title));


            //            var picture = (await _pictureService.CustomGetPicturesOfProducAsync(product.Id, 1)).FirstOrDefault();
            //            string fullSizeImageUrl, imageUrl;
            //            (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.CategoryThumbPictureSize);
            //            (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
            //            tokens.Add(new Token("Cart.Product.Image", fullSizeImageUrl));
            //        }
            //    }
            //}
            //else
            //{
            //    var customOrder = await _customOrderService.GetById(orderId);
            //    tokens.Add(new Token("OrderTotal", await _priceFormatter.FormatPriceAsync(customOrder.OrderTotal ?? 0, true, false)));
            //    tokens.Add(new Token("Checkout.Link", $"checkoutCustomOrder?orderid={orderId}&customerid={customer.Id}"));
            //    var items = await _customOrderService.GetOrderItems(orderId);
            //    if (items.Count > 0)
            //    {
            //        var product = await _productService.GetProductByIdAsync(items[0].ProductId);
            //        if (product != null)
            //        {
            //            var prdurl = (await RouteUrlAsync(_storeContext.GetCurrentStore().Id, "product", new
            //            {
            //                id = product.Id,
            //                SeName = await _urlRecordService.GetSeNameAsync(product)
            //            }));
            //            tokens.Add(new Token("Cart.Product.Link", prdurl));
            //            tokens.Add(new Token("Cart.Product.Name", product.Name));

            //            var picture = (await _pictureService.CustomGetPicturesOfProducAsync(product.Id, 1)).FirstOrDefault();
            //            string fullSizeImageUrl, imageUrl;
            //            (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.CategoryThumbPictureSize);
            //            (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
            //            tokens.Add(new Token("Cart.Product.Image", fullSizeImageUrl));
            //        }
            //    }
            //}

        }

        protected virtual async Task<string> CustomCustomOrderCartProductListToHtmlTableAsync(int orderId, int languageId)
        {
            return string.Empty;
            //var sb = new StringBuilder();
            //sb.AppendLine("<br><br><br>----------------Cart Details--------------<br><br><table style=\"border: 1px solid black\">");
            //sb.AppendLine("<tbody><tr>");
            //sb.AppendLine("<th style=\"border: 1px solid black\">SKU</th>");
            //sb.AppendLine("<th style=\"border: 1px solid black\">Quantity</th>");
            //sb.AppendLine("<th style=\"border: 1px solid black\">Price</th>");
            //sb.AppendLine("</tr>");
            //var _customOrderService = EngineContext.Current.Resolve<Nop.Services.Customizations.Phone_Order.ICustomOrderService>();
            //var cart = await _customOrderService.GetOrderItems(orderId);
            //var _settingService = EngineContext.Current.Resolve<Nop.Services.Configuration.ISettingService>();
            //foreach (var item in cart)
            //{
            //    var _priceAdjustment = await _customOrderService.GetPriceAdjustmentsByCartId(item.Id);
            //    sb.AppendLine("<tr>");
            //    sb.Append("<td style=\"border: 1px solid black\">");
            //    var product = await _productService.GetProductByIdAsync(item.ProductId);
            //    sb.Append(product.Sku);
            //    sb.Append("</br>");
            //    Dictionary<string, string> attrs = new Dictionary<string, string>();
            //    string attributesDescription = !string.IsNullOrEmpty(item.AttributesDescription) ?
            //        item.AttributesDescription : (!string.IsNullOrEmpty(item.CustomAttributesDescription) ?
            //         item.CustomAttributesDescription : "");
            //    var shadeAttr = await _settingService.GetSettingByKeyAsync<string>("catalog.product.attribute.shade.name");
            //    if (!string.IsNullOrEmpty(attributesDescription))
            //    {
            //        foreach (var attribute in attributesDescription.Split(new String[] { "<br />" }, StringSplitOptions.None))
            //        {
            //            if (attribute.Split(":").Length > 1)
            //                try
            //                {

            //                    if (string.Equals(attribute.Split(":")[0], shadeAttr, StringComparison.CurrentCultureIgnoreCase))
            //                    {

            //                        attrs.Add(attribute.Split(":")[0], System.Net.WebUtility.HtmlDecode(CustomCommonHelper.StripUnwantedPrefixFromShade(
            //                            string.Join(':', attribute.Split(":").Skip(1)))));
            //                    }
            //                    else
            //                    {

            //                        attrs.Add(attribute.Split(":")[0], System.Net.WebUtility.HtmlDecode(string.Join(':', attribute.Split(":").Skip(1))));
            //                    }
            //                }
            //                catch { }
            //        }

            //    }
            //    string attributes = string.Empty;
            //    foreach (var attr in attrs)
            //    {

            //        attributes += $"<b>{attr.Key}:</b><br/>  {attr.Value}";
            //        attributes += "<br/>";
            //    }
            //    sb.Append(attributes);
            //    sb.Append("</td>");
            //    sb.Append($"<td style=\"border: 1px solid black\">{item.Quantity}</td>");
            //    sb.Append($"<td style=\"border: 1px solid black\">{await _priceFormatter.FormatPriceAsync((_priceAdjustment?.ShoppingCartProductPrice ?? 0) == 0 ? 0 : Convert.ToDecimal(_priceAdjustment.ShoppingCartProductPrice))}</td>");
            //    sb.AppendLine("</tr>");
            //}
            //sb.Append("</tbody></table>");
            //var result = sb.ToString();
            //return result;
        }
        protected virtual async Task<string> CustomCartProductListToHtmlTableAsync(IList<ShoppingCartItem> cart, int languageId)
        {

            var sb = new StringBuilder();
            sb.AppendLine("<br><br><br>----------------Cart Details--------------<br><br><table style=\"border: 1px solid black\">");
            sb.AppendLine("<tbody><tr>");
            sb.AppendLine("<th style=\"border: 1px solid black\">SKU</th>");
            sb.AppendLine("<th style=\"border: 1px solid black\">Quantity</th>");
            sb.AppendLine("<th style=\"border: 1px solid black\">Price</th>");
            sb.AppendLine("</tr>");
            foreach (var item in cart)
            {
                sb.AppendLine("<tr>");
                sb.Append("<td style=\"border: 1px solid black\">");
                var product = await _productService.GetProductByIdAsync(item.ProductId);
                sb.Append(product.Sku);
                sb.Append("</br>");
                sb.Append(await _productAttributeFormatter.CustomFormatAttributesAsync(product, item.AttributesXml));
                sb.Append("</td>");
                sb.Append($"<td style=\"border: 1px solid black\">{item.Quantity}</td>");
                sb.Append($"<td style=\"border: 1px solid black\">{await _taxService.GetProductPriceAsync(product,
                    (await _shoppingCartService.GetUnitPriceAsync(item, true)).unitPrice)}</td>");
                sb.AppendLine("</tr>");
            }
            sb.Append("</tbody></table>");
            var result = sb.ToString();
            return result;
        }

        #endregion

        #region Abandoned Card

        public async Task CustomSupportAddAbandonedCartTokensAsync(IList<Token> tokens, Customer customer, IList<ShoppingCartItem> cart, int languageId)
        {
            tokens.Add(new Token("Customer.FullName", await _customerService.GetCustomerFullNameAsync(customer)));
            string email = customer.Email;
            if (!string.IsNullOrEmpty(email))
                tokens.Add(new Token("Email", customer.Email));


            string phone = "";
            if (customer.BillingAddressId.HasValue)
            {
                var address = await _addressService.GetAddressByIdAsync(Convert.ToInt32(customer.BillingAddressId));
                if (string.IsNullOrEmpty(email))
                    tokens.Add(new Token("Email", address.Email));
                tokens.Add(new Token("Phone", address.PhoneNumber));
            }
            else if (customer.ShippingAddressId.HasValue)
            {
                var address = await _addressService.GetAddressByIdAsync(Convert.ToInt32(customer.ShippingAddressId));
                if (string.IsNullOrEmpty(email))
                    tokens.Add(new Token("Email", address.Email));
                tokens.Add(new Token("Phone", address.PhoneNumber));
            }
            else
            {
                tokens.Add(new Token("Email", await _genericAttributeService.GetAttributeAsync<string>(customer, "Email", 0)));
                tokens.Add(new Token("Phone", await _genericAttributeService.GetAttributeAsync<string>(customer, "Phone", 0)));
            }


            tokens.Add(new Token("cartProducts", await this.CustomCartProductListToHtmlTableAsync(cart, languageId), true));
        }


        public async Task CustomAddAbandonedCartTokensAsync(IList<Token> tokens, Customer customer, string cartLink, Product product,
            List<Product> relatedProducts, int languageId, string utmSource)
        {
            tokens.Add(new Token("Customer.FullName", await _customerService.GetCustomerFullNameAsync(customer)));
            string email = customer.Email;
            if (!string.IsNullOrEmpty(email))
                tokens.Add(new Token("Email", customer.Email));
            tokens.Add(new Token("cart.link", cartLink));
            tokens.Add(new Token("Cart.Latest.Item", await CustomLatestCartItemHtml(product, cartLink, utmSource), true));
            tokens.Add(new Token("Cart.Related.Products", await CustomAddRelatedProductHtml(relatedProducts, utmSource), true));
        }
        #endregion

        #region Purchase Journey


        public async Task CustomAddPurchaseJourneyTokenAsync(IList<Token> tokens, Customer customer, List<Product> products,
            int productId, int categoryId, string templateType, string utm_params)
        {

            if (templateType == "complete your collection")
                tokens.Add(new Token("products", await this.CustomProductAddHTMLEmailB(products, utm_params), true));
            else if (templateType == "you might also like")
                tokens.Add(new Token("products", await this.CustomProductAddHTMLEmailC(products, utm_params), true));

            else if (templateType == "complementary categories")
                tokens.Add(new Token("products", await this.CustomProductAddHTMLEmailD(products, utm_params), true));
            if (productId > 0)
            {
                var product = await _productService.GetProductByIdAsync(productId);

                if (product != null && categoryId == 0)
                {
                    tokens.Add(new Token("shop.Url", (await RouteUrlAsync(_storeContext.GetCurrentStore().Id, "product", new
                    {
                        id = product.Id,
                        SeName = await _urlRecordService.GetSeNameAsync(product)
                    })) + $"?{utm_params}"));
                }
                if (product != null)
                {
                    tokens.Add(new Token("Product.Name", product.Name));
                }

                if ((product?.MainCollectionProductId ?? 0) > 0)
                {
                    var collectionProduct = await _productService.GetProductByIdAsync(product.MainCollectionProductId);

                    if (collectionProduct != null)
                    {
                        tokens.Add(new Token("Collection.Name", collectionProduct.Name));
                    }
                    else
                    {
                        if (product != null)
                        {
                            tokens.Add(new Token("Collection.Name", product.Name));
                        }
                    }
                }
                else
                {
                    if (product != null)
                    {
                        tokens.Add(new Token("Collection.Name", product.Name));
                    }
                }
            }
            if (categoryId > 0)
            {

                var category = await _categoryService.GetCategoryByIdAsync(categoryId);
                if (category != null)
                {
                    tokens.Add(new Token("shop.Url", await RouteUrlAsync(_storeContext.GetCurrentStore().Id, "category", new
                    {
                        id = category.Id,
                        SeName = await _urlRecordService.GetSeNameAsync(category)
                    }) + $"?sortby=[Best_Seller]&{utm_params}"));

                }
            }


        }


        #endregion

        #region utilities

        private async Task<string> CustomProductAddHTMLEmailB(List<Product> products, string utm_params)
        {
            if (products.Count > 3)
            {
                products = await products.Take(3).ToListAsync();
            }
            string html = "";


            StringBuilder productRows = new StringBuilder();

            for (int i = 0; i < products.Count; i++)
            {
                var product = products[i];
                bool imageFirst = i % 2 == 0;

                var prdurl = (await RouteUrlAsync(_storeContext.GetCurrentStore().Id, "product", new
                {
                    id = product.Id,
                    SeName = await _urlRecordService.GetSeNameAsync(product)
                })) + $"?{utm_params}";
                var picture = (await _pictureService.CustomGetPicturesOfProducAsync(product.Id, 1)).FirstOrDefault();
                string fullSizeImageUrl, imageUrl;
                (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.CategoryThumbPictureSize);
                (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);

                productRows.AppendLine($@"
           <tr>
               {(imageFirst
                            ? $@"<td style='width: 50%; padding: 0;'>
                           <a href='{prdurl}'><img src='{fullSizeImageUrl}' style='width: 100%;display: block;'></a>
                       </td>
                       <td style='font-size: 22px; text-align: center; width: 50%; padding: 30px;'>
                           <em style='color: #000;'><a href='{prdurl}' style='text-decoration:none;color:#363636;'>{product.Name}</a></em><br>
                           <div style='padding-top: 20px;'>
                               <a href='{prdurl}' style='text-decoration:none;color:#363636;'><img src='https://www.sierralivingconcepts.com/images/uploaded/journey-img13.jpg' alt=''></a>
                           </div>
                       </td>"
                            : $@"<td style='font-size: 22px; text-align: center; width: 50%; padding: 30px;'>
                           <em style='color: #000;'><a href='{prdurl}' style='text-decoration:none;color:#363636;'>{product.Name}</a></em><br>
                           <div style='padding-top: 20px;'>
                               <a href='{prdurl}' style='text-decoration:none;color:#363636;'><img src='https://www.sierralivingconcepts.com/images/uploaded/journey-img13.jpg' alt=''></a>
                           </div>
                       </td>
                       <td style='width: 50%; padding: 0;'>
                           <a href='{prdurl}'><img src='{fullSizeImageUrl}' style='width: 100%;  display: block;'></a>
                       </td>")}
           </tr>");
            }
            html = productRows.ToString();
            return html;
        }
        private async Task<string> CustomProductAddHTMLEmailC(List<Product> products, string utm_params)
        {

            if (products.Count > 3)
            {
                products = await products.Take(3).ToListAsync();
            }

            string html = "";


            StringBuilder productRows = new StringBuilder();



            for (int i = 0; i < products.Count; i++)
            {
                var product = products[i];
                bool imageFirst = i % 2 == 0;

                var prdurl = (await RouteUrlAsync(_storeContext.GetCurrentStore().Id, "product", new
                {
                    id = product.Id,
                    SeName = await _urlRecordService.GetSeNameAsync(product)
                })) + $"?{utm_params}";
                var picture = (await _pictureService.CustomGetPicturesOfProducAsync(product.Id, 1)).FirstOrDefault();
                string fullSizeImageUrl, imageUrl;
                (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.CategoryThumbPictureSize);
                (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);

                productRows.AppendLine($@"
<tr>
    {(imageFirst
            ? $@"<td style='width: 50%; padding: 0;'>
            <a href='{prdurl}'><img src='{fullSizeImageUrl}' style='width: 100%;  display: block;'></a>
        </td>
        <td style='font-size: 22px; text-align: center; width: 50%; padding: 30px; font-family: Playfair Display, serif;'>
            <em style='color: #3c3c3c;'><a href='{prdurl}' style='text-decoration:none;color:#363636;' >{product.Name}</a></em><br>
            <div style='padding-top: 20px;'>
                <a href='{prdurl}'><img src='https://www.sierralivingconcepts.com/images/uploaded/journey-img10.jpg' alt=''></a>
            </div>
        </td>"
            : $@"<td style='font-size: 22px; text-align: center; width: 50%; padding: 30px; font-family: Playfair Display, serif;'>
            <em style='color: #3c3c3c;'><a href='{prdurl}' style='text-decoration:none;color:#363636;' >{product.Name}</a></em><br>
            <div style='padding-top: 20px;'>
                <a href='{prdurl}'><img src='https://www.sierralivingconcepts.com/images/uploaded/journey-img10.jpg' alt=''></a>
            </div>
        </td>
        <td style='width: 50%; padding: 0;'>
            <a href='{prdurl}'><img src='{fullSizeImageUrl}' style='width: 100%;  display: block;'></a>
        </td>")}
</tr>");
            }
            html = productRows.ToString();
            return html;
        }
        private async Task<string> CustomProductAddHTMLEmailD(List<Product> products, string utm_params)
        {
            if (products.Count > 6)
            {
                products = await products.Take(6).ToListAsync();
            }
            string html = "";


            StringBuilder productRows = new StringBuilder();
            for (int i = 0; i < products.Count; i += 3)
            {
                productRows.AppendLine("<tr>");

                for (int j = i; j < i + 3 && j < products.Count; j++)
                {
                    var product = products[j];
                    var prdurl = (await RouteUrlAsync(_storeContext.GetCurrentStore().Id, "product", new
                    {
                        id = product.Id,
                        SeName = await _urlRecordService.GetSeNameAsync(product)
                    })) + $"?{utm_params}";
                    var picture = (await _pictureService.CustomGetPicturesOfProducAsync(product.Id, 1)).FirstOrDefault();
                    string fullSizeImageUrl, imageUrl;
                    (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.CategoryThumbPictureSize);
                    (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);

                    productRows.AppendLine($@"
<td style='width:33.333%'>
    <a href='{prdurl}' ><img src='{fullSizeImageUrl}' alt='Image' style='width: 100%; max-width: 180px; height: auto; display: block;height: 180px;
    object-fit: contain;
    margin: 0 auto;'></a>
    <p style='font-size: 12px; padding: 10px 15px 40px 15px; color: #535353;text-decoration:none;'>{product.Name}</p>
</td>");
                }

                productRows.AppendLine("</tr>");
            }
            html = productRows.ToString();
            return html;
        }
        protected virtual async Task<string> CustomOrderProductListToHtmlTableAsync(CustomOrder customOrder, int languageId, int vendorId)
        {
            return string.Empty;
            //var store = (await _storeService.GetAllStoresAsync()).OrderByDescending(s => s.DisplayOrder).FirstOrDefault();
            //int storeId = store?.Id ?? 0;
            //var _customOrderService = EngineContext.Current.Resolve<Nop.Services.Customizations.Phone_Order.ICustomOrderService>();

            //var table = await _customOrderService.GetOrderItems(customOrder.Id);

            //var language = await _languageService.GetLanguageByIdAsync(languageId);

            //var sb = new StringBuilder();

            //var _pictureService = EngineContext.Current.Resolve<IPictureService>();
            //var _mediaSettings = EngineContext.Current.Resolve<MediaSettings>();
            ////var _pictureService = EngineContext.Current.Resolve<INopF>();

            //for (var i = 0; i <= table.Count - 1; i++)
            //{
            //    var orderItem = table[i];

            //    var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
            //    var prdurl = await RouteUrlAsync(storeId, "product", new
            //    {
            //        id = product.Id,
            //        SeName = await _urlRecordService.GetSeNameAsync(product)
            //    });
            //    if (product == null)
            //        continue;


            //    sb.AppendLine("<div style=\" float: left;width: 100%;margin-bottom: 15px;display: flex;justify-content: space-between;\">");
            //    sb.AppendLine("<div class=\"col-12 col-md-7 col-lg-6\" style=\"width: 50%;\">");
            //    sb.AppendLine("<div class=\"section-3-contant-1\">");
            //    sb.AppendLine("<div style=\"display: flex;gap: 15px;padding: 0 15px;\">");
            //    sb.AppendLine($"<div class=\" section-3-order-details col-12 col-md-6 col-lg-4 \">");

            //    var picture = (await _pictureService.GetPicturesByProductIdAsync(product.Id, 1)).FirstOrDefault();
            //    string fullSizeImageUrl, imageUrl;
            //    (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.CategoryThumbPictureSize);
            //    (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);

            //    sb.AppendLine($"<img src=\"{fullSizeImageUrl}\" style=\"width:90px;height:90px;\"/>");

            //    sb.AppendLine("</div>");


            //    //product name

            //    sb.AppendLine("<div style=\"padding-left: 10px;\">");



            //    var productName = string.Empty;
            //    if (customOrder.ParentOrderID == 0)
            //    {
            //        productName = "<p style=\"margin-bottom:0;\"><a href=\"" + prdurl + "\">" + await _localizationService.GetLocalizedAsync(product, x => x.Name, languageId) + "</a>";
            //        productName += $"<br/><b>SKU</b> : ({product.Sku})</p>";
            //    }
            //    else
            //    {
            //        productName = "<p style=\"margin-bottom:0;\">" + await _localizationService.GetLocalizedAsync(product, x => x.Name, languageId) + "</p>";
            //    }


            //    //attributes
            //    var _settingService = EngineContext.Current.Resolve<Nop.Services.Configuration.ISettingService>();
            //    var shadeAttr = await _settingService.GetSettingByKeyAsync<string>("catalog.product.attribute.shade.name");

            //    Dictionary<string, string> attrs = new Dictionary<string, string>();
            //    string attributesDescription = !string.IsNullOrEmpty(orderItem.AttributesDescription) ?
            //        orderItem.AttributesDescription : (!string.IsNullOrEmpty(orderItem.CustomAttributesDescription) ?
            //         orderItem.CustomAttributesDescription : "");

            //    if (!string.IsNullOrEmpty(attributesDescription))
            //    {
            //        foreach (var attribute in attributesDescription.Split(new String[] { "<br />" }, StringSplitOptions.None))
            //        {
            //            if (attribute.Split(":").Length > 1)
            //                try
            //                {

            //                    if (string.Equals(attribute.Split(":")[0], shadeAttr, StringComparison.CurrentCultureIgnoreCase))
            //                    {

            //                        attrs.Add(attribute.Split(":")[0], System.Net.WebUtility.HtmlDecode(CommonHelper.StripUnwantedPrefixFromShade(
            //                            string.Join(':', attribute.Split(":").Skip(1)))));
            //                    }
            //                    else
            //                    {

            //                        attrs.Add(attribute.Split(":")[0], System.Net.WebUtility.HtmlDecode(string.Join(':', attribute.Split(":").Skip(1))));
            //                    }
            //                }
            //                catch { }
            //        }

            //    }
            //    productName += attrs.Count > 0 ? "<p>" : "";
            //    foreach (var attr in attrs)
            //    {

            //        productName += $"<b>{attr.Key}:</b><br/>  {attr.Value}";
            //        productName += "<br/>";
            //    }

            //    productName += attrs.Count > 0 ? "</p>" : "";
            //    sb.AppendLine(productName);

            //    // end

            //    // quantity



            //    // end



            //    if (!string.IsNullOrEmpty(table[i].Notes))
            //    {
            //        string notes = table[i].Notes.Replace("images/customorder", store?.Hosts ?? "" + "images/customorder");
            //        sb.AppendLine($"<p><b>{await _localizationService.GetResourceAsync("Messages.Order.Product(s).SpecialInstructions", languageId)}</b> {notes}</ p >");
            //    }

            //    sb.AppendLine("</div></div></div></div>");

            //    sb.AppendLine("<div class=\"col-md-2 d-flex align-items-lg-center align-items-md-start\" style=\"width:20%\"><div class=\"section-3-order-details-price\">");







            //    // Price



            //    string ItemPriceIncTax = "";
            //    string adjustmentAmount = "";
            //    decimal discountPercentage = 0;
            //    decimal discountAmount = 0;
            //    string itemTotal = "";
            //    string cssClass = "";
            //    var _priceAdjustment = await _customOrderService.GetPriceAdjustmentsByCartId(table[i].Id);
            //    string resourceName = "Receipt.Item.OfferDiscount";
            //    if (_priceAdjustment != null)
            //    {

            //        ItemPriceIncTax = await _priceFormatter.FormatPriceAsync(
            //            _priceAdjustment.ShoppingCartProductPrice == null ? 0 :
            //            Convert.ToDecimal(_priceAdjustment.ShoppingCartProductPrice) * table[i].Quantity);

            //        discountAmount = _priceAdjustment.Discountamount == null ? 0 : Convert.ToDecimal(_priceAdjustment.Discountamount);
            //        discountPercentage = _priceAdjustment.DiscountPercentage == null ? 0 : Convert.ToDecimal(_priceAdjustment.DiscountPercentage);
            //        decimal _totalAdjustment = (_priceAdjustment.Discounttype == DiscountType.Percentage.ToString() && discountPercentage != 0 ?
            //             (_priceAdjustment.ShoppingCartProductPrice == null ? 0 : Convert.ToDecimal(_priceAdjustment.ShoppingCartProductPrice)
            //             * Convert.ToDecimal(_priceAdjustment.DiscountPercentage)) / 100
            //             : (_priceAdjustment.Discounttype == DiscountType.Fixed.ToString() && discountAmount != 0
            //             ? discountAmount : 0)) * table[i].Quantity;
            //        resourceName = _priceAdjustment.Chargestype == ChargeType.Add.ToString() ? "Receipt.Item.AdditionalAdjustment" : resourceName;
            //        adjustmentAmount = _totalAdjustment != 0 ? (_priceAdjustment.Chargestype == ChargeType.Subtract.ToString() ? "-" : "") + await _priceFormatter.FormatPriceAsync(_totalAdjustment) : "";
            //        cssClass = _priceAdjustment.Chargestype == ChargeType.Subtract.ToString() ? "danger" : "success";
            //    }

            //    sb.AppendLine($"<p style=\"line-height: 20px;\">Price:{await _priceFormatter.FormatPriceAsync(_priceAdjustment.ShoppingCartProductPrice == null ? 0 : Convert.ToDecimal(_priceAdjustment.ShoppingCartProductPrice))}  <br>Qty :{orderItem.Quantity} </p>");
            //    sb.AppendLine("</div></div>");

            //    sb.AppendLine("<div class=\"col-md-3 d-flex align-items-lg-center align-items-md-start justify-content-lg-center justify-content-md-center\" style=\"padding-right: 15px; width: 30%; text-align: right;\">");


            //    sb.AppendLine("<div class=\" text-md-right text-lg-right \">");

            //    sb.AppendLine($"<p class=\"m-0\">{ItemPriceIncTax}</p>");

            //    if (!string.IsNullOrEmpty(adjustmentAmount))
            //    {
            //        sb.AppendLine("<p class=\"text-danger m-0\" style=\"font-style:italic;\">" + string.Format(await _localizationService.GetResourceAsync(resourceName)
            //            , adjustmentAmount) + "</p>");
            //    }

            //    sb.AppendLine("</div>");


            //    // end

            //    sb.AppendLine("</div>");

            //    sb.AppendLine("</div>");
            //    // end
            //}
            //var result = sb.ToString();
            //return result;
        }
        protected virtual async Task<string> CustomProductListToHtmlTableAsync(Order order, int languageId, int vendorId)
        {

            var table = await _orderService.GetOrderItemsAsync(order.Id, vendorId: vendorId);
            decimal customSubTotal = 0;
            foreach (var item in table)
            {
                if (item.ItemPriceIncTax == 0)
                {
                    customSubTotal = 0;
                    break;
                }
                else
                    customSubTotal += item.ItemPriceIncTax * item.Quantity;
            }


            var language = await _languageService.GetLanguageByIdAsync(languageId);

            var sb = new StringBuilder();






            for (var i = 0; i <= table.Count - 1; i++)
            {
                var orderItem = table[i];

                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
                var prdurl = await RouteUrlAsync(order.StoreId, "product", new
                {
                    id = product.Id,
                    SeName = await _urlRecordService.GetSeNameAsync(product)
                });
                if (product == null)
                    continue;
                sb.AppendLine("<div style=\" float: left;width: 100%;margin-bottom: 15px;display: flex;justify-content: space-between\">");
                sb.AppendLine("<div class=\"col-12 col-md-7 col-lg-6\" style=\"width: 50%;\">");
                sb.AppendLine("<div class=\"section-3-contant-1\">");
                sb.AppendLine("<div style=\"display: flex; gap: 15px;padding: 0 15px;\">");
                sb.AppendLine($"<div class=\" section-3-order-details col-12 col-md-6 col-lg-4 \">");

                var picture = (await _pictureService.CustomGetPicturesOfProducAsync(product.Id, 1)).FirstOrDefault();
                string fullSizeImageUrl, imageUrl;
                (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.CategoryThumbPictureSize);


                sb.AppendLine($"<img src=\"{imageUrl}\" style=\"width:90px;height:90px;\"/>");

                sb.AppendLine("</div>");


                //product name

                sb.AppendLine("<div style=\"padding-left: 10px;\">");


                VariantCombination variant = new VariantCombination();
                variant = await _customProductService.GetItemVariantInfo(product.Id, orderItem.AttributesXml);
                string prdName = string.IsNullOrWhiteSpace(variant?.Title) ? product.Name : variant.Title;

                var productName = "<p style=\"margin-bottom:0;\"><a href=\"" + prdurl + "\">" +
                    prdName + "</a>";

                productName += $"<br/><b>SKU</b> : ({product.Sku})</p>";

                //attributes

                var attributeDescription = string.IsNullOrEmpty(orderItem.AttributesXml) ? orderItem.AttributeDescription :
                    await _productAttributeFormatter.CustomFormatAttributesAsync(product, orderItem.AttributesXml);
                Dictionary<string, string> attrs = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(attributeDescription))
                {
                    foreach (var attribute in attributeDescription.Split(new String[] { "<br />" }, StringSplitOptions.None))
                    {
                        if (attribute.Split(":").Length > 1)
                            try
                            {
                                attrs.Add(attribute.Split(":")[0], System.Net.WebUtility.HtmlDecode(string.Join(':', attribute.Split(":").Skip(1))));
                            }
                            catch { }
                    }

                }
                productName += attrs.Count > 0 ? "<p>" : "";
                foreach (var attr in attrs)
                {

                    productName += $"<b>{attr.Key}:</b><br/> {attr.Value}";
                    productName += "<br/>";
                }
                productName += attrs.Count > 0 ? "</p>" : "";
                sb.AppendLine(productName);

                if (!string.IsNullOrEmpty(orderItem.SpecialInstructions))
                {
                    //Special Instructions

                    sb.AppendLine($"<p><b>{await _localizationService.GetResourceAsync("Messages.Order.Product(s).SpecialInstructions",
                        languageId)}</b> {orderItem.SpecialInstructions}</p>");
                }
                sb.AppendLine("</div></div></div></div>");



                sb.AppendLine("<div class=\"col-md-2 d-flex align-items-lg-center align-items-md-start\" style=\"width:20%\"><div class=\"section-3-order-details-price\">");
                sb.AppendLine($"<p style=\"  line-height: 20px;\">Price:{await _priceFormatter.FormatPriceAsync(
                    _currencyService.ConvertCurrency(orderItem.ItemPriceIncTax, order.CurrencyRate),
                    true, order.CustomerCurrencyCode, languageId, false)}  <br>Qty :{orderItem.Quantity} </p>");


                sb.AppendLine("</div></div>");

                sb.AppendLine("<div class=\"col-md-3 d-flex align-items-lg-center align-items-md-start justify-content-lg-center justify-content-md-center\" style=\"padding-right: 15px; width: 30%; text-align: right;\">");

                sb.AppendLine("<div class=\" text-md-right text-lg-right \">");
                string itemSubtotal;
                itemSubtotal = await _priceFormatter.FormatPriceAsync(_currencyService.ConvertCurrency(orderItem.ItemPriceIncTax, order.CurrencyRate) * orderItem.Quantity, true, order.CustomerCurrencyCode, languageId, false);
                sb.AppendLine($"<p class=\"m-0\">{itemSubtotal}</p>");







                string membershipDiscountIncTaxStr = "";
                string offerDiscountIncTaxStr = "";
                string buyMoreSaveMoreDiscountIncTaxStr = "";
                if (customSubTotal > 0)
                {
                    if (orderItem.MembershipDiscountIncTax != 0)
                        membershipDiscountIncTaxStr = "-" + await _priceFormatter.FormatPriceAsync(_currencyService.ConvertCurrency(orderItem.MembershipDiscountIncTax, order.CurrencyRate), true, order.CustomerCurrencyCode, languageId, true);
                    if (orderItem.OfferDiscountIncTax != 0)
                        offerDiscountIncTaxStr = "-" + await _priceFormatter.FormatPriceAsync(_currencyService.ConvertCurrency(orderItem.OfferDiscountIncTax, order.CurrencyRate), true, order.CustomerCurrencyCode, languageId, true);
                    if (orderItem.BuyMoreSaveMoreDiscountIncTax != 0)
                        buyMoreSaveMoreDiscountIncTaxStr = "-" + await _priceFormatter.FormatPriceAsync(orderItem.BuyMoreSaveMoreDiscountIncTax, true, order.CustomerCurrencyCode, languageId, true);
                }

                if (!string.IsNullOrEmpty(membershipDiscountIncTaxStr))
                {

                    sb.AppendLine("<p class=\"text-danger m-0\" style=\"font-style:italic;\">" + string.Format(await _localizationService.GetResourceAsync("Receipt.Item.MembershipDiscount")
                        , membershipDiscountIncTaxStr) + "</p>");
                }

                if (!string.IsNullOrEmpty(offerDiscountIncTaxStr))

                    sb.AppendLine($"<p class=\"text-danger m-0\" style=\"font-style:italic;\">" +
                        $"{string.Format(await _localizationService.GetResourceAsync("Receipt.Item.OfferDiscount"), offerDiscountIncTaxStr)}</p>");

                if (!string.IsNullOrEmpty(buyMoreSaveMoreDiscountIncTaxStr))
                {
                    sb.AppendLine($"<p class=\"text-danger m-0\" style=\"font-style:italic;\">" +
                         $"{string.Format(await _localizationService.GetResourceAsync("Receipt.Item.BuyMoreSaveMoreDiscount"), buyMoreSaveMoreDiscountIncTaxStr)}</p>");
                }
                if (orderItem.TotalDiscount > 0)
                {
                    sb.AppendLine($"<p class=\"text-danger m-0\" style=\"font-style:italic;\">" +
                       $"{string.Format(await _localizationService.GetResourceAsync("Receipt.Item.discount"), await _priceFormatter.FormatPriceAsync(_currencyService.ConvertCurrency(orderItem.TotalDiscount > 0 ? -orderItem.TotalDiscount : orderItem.TotalDiscount, order.CurrencyRate), true, order.CustomerCurrencyCode, languageId, true))}</p>");

                }
                sb.AppendLine("</div>");


                // end

                sb.AppendLine("</div>");

                sb.AppendLine("</div>");

                // end
            }
            var result = sb.ToString();
            return result;
        }
        protected virtual async Task<string> GetTaxDetails(Order order, string shippingZipPostalCode, string taxPercentage, string cusTaxTotal, int languageId)
        {
            string html = "";
            //var taxes = _orderService.GetTaxDetails(order);
            //if (!taxes.Where(t => t.TaxType != Nop.Services.Tax.TaxType.Tax && t.TaxRate > 0).Any())
            //{
            //    html = $"<p style=\"margin-bottom:10px;text-align:right;\">{shippingZipPostalCode} - Tax({taxPercentage}): {cusTaxTotal}</p>";
            //}
            //else
            //{
            //    html = $"<p style=\"margin-bottom:10px;text-align:right;\">{await _localizationService.GetResourceAsync("ShoppingCart.Totals.DisplayTax.Info")}: &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</p>";
            //    foreach (var taxInfo in taxes.Where(t => t.TaxRate > 0))
            //    {
            //        html += $"<p style=\"margin-bottom:10px;text-align:right;text-transform: uppercase;\">{string.Format(await _localizationService.GetResourceAsync("Canada.Tax.Label"), taxInfo.TaxType.ToString(), taxInfo.TaxRate.ToString("F2"))}: {await _priceFormatter.FormatPriceAsync(taxInfo.Amount, true, order.CustomerCurrencyCode, languageId, true)}</p>";
            //    }
            //}

            return html;
        }

        protected virtual async Task<string> CustomOrderGetTaxDetails(string taxInfo, string cusTaxTotal, string shippingZipPostalCode, int languageId)
        {
            string html = "";
            //var taxes = _orderService.GetTaxDetails(taxInfo);
            //if (!taxes.Where(t => t.TaxType != Nop.Services.Tax.TaxType.Tax && t.TaxRate > 0).Any())
            //{
            //    if (taxes.Count > 0)
            //    {
            //        html = $"<p style=\"margin-bottom:10px;text-align:right;\">{shippingZipPostalCode} - Tax({taxes.FirstOrDefault()?.TaxRate}): {cusTaxTotal}</p>";
            //    }
            //    else
            //    {
            //        html = $"<p style=\"margin-bottom:10px;text-align:right;\">Tax: {cusTaxTotal}</p>";
            //    }
            //}
            //else
            //{
            //    html = $"<p style=\"margin-bottom:10px;text-align:right;\">{await _localizationService.GetResourceAsync("ShoppingCart.Totals.DisplayTax.Info")}: &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</p>";
            //    foreach (var _taxInfo in taxes.Where(t => t.TaxRate > 0))
            //    {
            //        html += $"<p style=\"margin-bottom:10px;text-align:right;text-transform: uppercase;\">{string.Format(await _localizationService.GetResourceAsync("Canada.Tax.Label"), _taxInfo.TaxType.ToString(), _taxInfo.TaxRate.ToString("F2"))}: {await _priceFormatter.FormatPriceAsync(_taxInfo.Amount)}</p>";
            //    }
            //}

            return html;
        }
        protected virtual async Task AddOrderTotalTokens(IList<Token> tokens, Order order, string shippingZipCode, string taxPercentage, int vendorId, int languageId)
        {
            var table = await _orderService.GetOrderItemsAsync(order.Id, vendorId: vendorId);
            decimal customSubTotal = 0;
            foreach (var item in table)
            {
                if (item.ItemPriceIncTax == 0)
                {
                    customSubTotal = 0;
                    break;
                }
                else
                    customSubTotal += item.ItemPriceIncTax * item.Quantity;
            }


            var language = await _languageService.GetLanguageByIdAsync(languageId);
            //subtotal
            string cusSubTotal;
            var displaySubTotalDiscount = false;
            var cusSubTotalDiscount = string.Empty;
            decimal orderItemsDiscount = 0;
            decimal netSubTotal = 0;
            foreach (var orderItem in table)
            {
                orderItemsDiscount += _currencyService.ConvertCurrency(orderItem.TotalDiscount, order.CurrencyRate);
            }
            if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal)
            {
                //including tax

                //subtotal
                var orderSubtotalInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubtotalInclTax, order.CurrencyRate);
                netSubTotal = orderSubtotalInclTaxInCustomerCurrency;
                cusSubTotal = await _priceFormatter.FormatPriceAsync(orderSubtotalInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);
                //discount (applied to order subtotal)
                var orderSubTotalDiscountInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubTotalDiscountInclTax, order.CurrencyRate);
                if (orderSubTotalDiscountInclTaxInCustomerCurrency > decimal.Zero || orderItemsDiscount > decimal.Zero)

                {
                    netSubTotal = netSubTotal - (orderSubTotalDiscountInclTaxInCustomerCurrency + orderItemsDiscount);
                    cusSubTotalDiscount = "-" + await _priceFormatter.FormatPriceAsync(orderSubTotalDiscountInclTaxInCustomerCurrency + orderItemsDiscount, true, order.CustomerCurrencyCode, languageId, true);
                    displaySubTotalDiscount = true;
                }
            }
            else
            {
                //excluding tax

                //subtotal
                var orderSubtotalExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubtotalExclTax, order.CurrencyRate);
                netSubTotal = orderSubtotalExclTaxInCustomerCurrency;
                cusSubTotal = await _priceFormatter.FormatPriceAsync(orderSubtotalExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);
                //discount (applied to order subtotal)
                var orderSubTotalDiscountExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubTotalDiscountExclTax, order.CurrencyRate);
                if (orderSubTotalDiscountExclTaxInCustomerCurrency > decimal.Zero || orderItemsDiscount > decimal.Zero)

                {
                    netSubTotal = netSubTotal - (orderSubTotalDiscountExclTaxInCustomerCurrency + orderItemsDiscount);
                    cusSubTotalDiscount = "-" + await _priceFormatter.FormatPriceAsync(
                        orderSubTotalDiscountExclTaxInCustomerCurrency + orderItemsDiscount, true, order.CustomerCurrencyCode, languageId, false);
                    displaySubTotalDiscount = true;
                }
            }

            //shipping, payment method fee
            string cusShipTotal;
            string wgsCharges = "";
            string addtionalShippingCharges = "";
            string cusPaymentMethodAdditionalFee;
            var taxRates = new SortedDictionary<decimal, decimal>();
            var cusTaxTotal = string.Empty;
            var cusDiscount = string.Empty;
            if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
            {
                //including tax

                //shipping
                var orderShippingInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingInclTax, order.CurrencyRate);
                cusShipTotal = await _priceFormatter.FormatShippingPriceAsync(orderShippingInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);
                //payment method additional fee
                var paymentMethodAdditionalFeeInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeInclTax, order.CurrencyRate);
                cusPaymentMethodAdditionalFee = await _priceFormatter.FormatPaymentMethodAdditionalFeeAsync(paymentMethodAdditionalFeeInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);
            }
            else
            {
                //excluding tax

                //shipping
                var orderShippingExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingExclTax, order.CurrencyRate);
                cusShipTotal = await _priceFormatter.FormatShippingPriceAsync(orderShippingExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);
                //payment method additional fee
                var paymentMethodAdditionalFeeExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeExclTax, order.CurrencyRate);
                cusPaymentMethodAdditionalFee = await _priceFormatter.FormatPaymentMethodAdditionalFeeAsync(paymentMethodAdditionalFeeExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);
            }

            //shipping
            var displayShipping = order.ShippingStatus != ShippingStatus.ShippingNotRequired;

            //payment method fee
            var displayPaymentMethodFee = order.PaymentMethodAdditionalFeeExclTax > decimal.Zero;

            //tax
            bool displayTax;
            bool displayTaxRates;
            if (_taxSettings.HideTaxInOrderSummary && order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
            {
                displayTax = false;
                displayTaxRates = false;
            }
            else
            {
                if (order.OrderTax == 0 && _taxSettings.HideZeroTax)
                {
                    displayTax = false;
                    displayTaxRates = false;
                }
                else
                {
                    taxRates = new SortedDictionary<decimal, decimal>();
                    foreach (var tr in _orderService.ParseTaxRates(order, order.TaxRates))
                        taxRates.Add(tr.Key, _currencyService.ConvertCurrency(tr.Value, order.CurrencyRate));

                    displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Any();
                    displayTax = !displayTaxRates;

                    var orderTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTax, order.CurrencyRate);
                    var taxStr = await _priceFormatter.FormatPriceAsync(orderTaxInCustomerCurrency, true, order.CustomerCurrencyCode,
                        false, languageId);
                    cusTaxTotal = taxStr;
                }
            }

            //discount
            var displayDiscount = false;
            if (order.OrderDiscount > decimal.Zero)
            {
                var orderDiscountInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderDiscount, order.CurrencyRate);
                cusDiscount = await _priceFormatter.FormatPriceAsync(-orderDiscountInCustomerCurrency, true, order.CustomerCurrencyCode, false, languageId);
                displayDiscount = true;
            }

            //total
            var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
            var cusTotal = await _priceFormatter.FormatPriceAsync(orderTotalInCustomerCurrency, true, order.CustomerCurrencyCode, false, languageId);
            string membershipFee = "";
            string membershipFeeDiscount = "";
            string membershipDiscountIncTax = "";
            string offerDiscountIncTax = "";
            string buyMoreSaveMoreDiscountIncTax = "";
            if (customSubTotal > 0)
            {
                cusSubTotal = await _priceFormatter.FormatPriceAsync(customSubTotal, true, order.CustomerCurrencyCode, languageId, true);
                if (order.MembershipFeeInclTax > 0)
                    membershipFee = await _priceFormatter.FormatPriceAsync(_currencyService.ConvertCurrency(order.MembershipFeeInclTax, order.CurrencyRate), true, order.CustomerCurrencyCode, languageId, true);
                if (order.MembershipFeeDiscountInclTax != 0)
                    membershipFeeDiscount = "-" + await _priceFormatter.FormatPriceAsync(_currencyService.ConvertCurrency(order.MembershipFeeDiscountInclTax, order.CurrencyRate), true, order.CustomerCurrencyCode, languageId, true);
                if (order.MembershipDiscountIncTax != 0)
                    membershipDiscountIncTax = "-" + await _priceFormatter.FormatPriceAsync(_currencyService.ConvertCurrency(order.MembershipDiscountIncTax, order.CurrencyRate), true, order.CustomerCurrencyCode, languageId, true);
                if (order.OfferDiscountIncTax != 0)
                    offerDiscountIncTax = "-" + await _priceFormatter.FormatPriceAsync(_currencyService.ConvertCurrency(order.OfferDiscountIncTax, order.CurrencyRate), true, order.CustomerCurrencyCode, languageId, true);
                if (order.BuyMoreSaveMoreDiscountIncTax != 0)
                    buyMoreSaveMoreDiscountIncTax = "-" + await _priceFormatter.FormatPriceAsync(_currencyService.ConvertCurrency(order.BuyMoreSaveMoreDiscountIncTax, order.CurrencyRate), true, order.CustomerCurrencyCode, languageId, true);
                if (order.WgsChargesInclTax > 0)
                    wgsCharges = await _priceFormatter.FormatPriceAsync(_currencyService.ConvertCurrency(order.WgsChargesInclTax, order.CurrencyRate), true, order.CustomerCurrencyCode, languageId, true);
                if (order.AdditonalShippingChargesInclTax > 0)
                    addtionalShippingCharges = await _priceFormatter.FormatPriceAsync(_currencyService.ConvertCurrency(order.AdditonalShippingChargesInclTax, order.CurrencyRate), true, order.CustomerCurrencyCode, languageId, true);

            }
            if (!order.IsCustomOrder)
            {
                if (order.ShippingMethod.ToLower().IndexOf("white glove service") >= 0
                                || order.ShippingMethod.ToLower().IndexOf("wgs") >= 0)
                {
                    wgsCharges = cusShipTotal;
                    cusShipTotal = await _priceFormatter.FormatShippingPriceAsync(0, true, order.CustomerCurrencyCode, languageId, true);


                }
            }

            //notes
            tokens.Add(new Token("Order.Notes", order.CheckoutAttributeDescription == null ? "" : order.CheckoutAttributeDescription.Replace("Order Notes:", "")));

            //subtotal
            tokens.Add(new Token("Order.Subtotal", cusSubTotal));

            //discount (applied to order subtotal)
            if (displaySubTotalDiscount)
            {
                tokens.Add(new Token("Order.HasSubtotalDiscount", true));
                tokens.Add(new Token("Order.SubtotalDiscount", cusSubTotalDiscount));
            }
            else
                tokens.Add(new Token("Order.HasSubtotalDiscount", false));

            if (order.CustomDutyInclTax > 0)
            {
                tokens.Add(new Token("Order.HasCustomDuty", true));
                tokens.Add(new Token("Order.CustomDutyPercentage", Math.Round(order.CustomDutyPercentage, 2) + "%"));
                tokens.Add(new Token("Order.CustomDuty", await _priceFormatter.FormatPriceAsync
                    (_currencyService.ConvertCurrency(order.CustomDutyInclTax, order.CurrencyRate), true, order.CustomerCurrencyCode, languageId, true), false));
            }
            else
                tokens.Add(new Token("Order.HasCustomDuty", false));

            if (!string.IsNullOrEmpty(membershipFee))
            {
                tokens.Add(new Token("Order.MemberProgram", true));
                tokens.Add(new Token("Order.MembershipFee", membershipFee));
                netSubTotal = netSubTotal + order.MembershipFeeInclTax;
            }
            else
                tokens.Add(new Token("Order.MemberProgram", false));

            if (!string.IsNullOrEmpty(membershipFeeDiscount))
            {
                tokens.Add(new Token("Order.HasMemberProgramFeeDiscount", true));
                tokens.Add(new Token("Order.MembershipFeeDiscount", membershipFeeDiscount));
                netSubTotal = netSubTotal - order.MembershipFeeDiscountInclTax;
            }
            else
                tokens.Add(new Token("Order.HasMemberProgramFeeDiscount", false));


            if (!string.IsNullOrEmpty(membershipDiscountIncTax))
            {
                tokens.Add(new Token("Order.HasMemberProgramDiscount", true));
                tokens.Add(new Token("Order.MembershipDiscountIncTax", membershipDiscountIncTax));
                netSubTotal = netSubTotal - order.MembershipDiscountIncTax;
            }
            else
                tokens.Add(new Token("Order.HasMemberProgramDiscount", false));

            if (!string.IsNullOrEmpty(offerDiscountIncTax))
            {
                tokens.Add(new Token("Order.HasOfferDiscount", true));
                tokens.Add(new Token("Order.OfferDiscount", offerDiscountIncTax));
                netSubTotal = netSubTotal - order.OfferDiscountIncTax;
            }
            else
                tokens.Add(new Token("Order.HasOfferDiscount", false));

            if (!string.IsNullOrEmpty(buyMoreSaveMoreDiscountIncTax))
            {
                tokens.Add(new Token("Order.HasBuyMoreSaveMoreDiscount", true));
                tokens.Add(new Token("Order.BuyMoreSaveMoreDiscount", buyMoreSaveMoreDiscountIncTax));
                netSubTotal = netSubTotal - order.BuyMoreSaveMoreDiscountIncTax;
            }
            else
                tokens.Add(new Token("Order.HasBuyMoreSaveMoreDiscount", false));
            tokens.Add(new Token("Order.NetSubTotal", await _priceFormatter.FormatPriceAsync(_currencyService.ConvertCurrency(netSubTotal, order.CurrencyRate), true, order.CustomerCurrencyCode, languageId, true)));

            if (order.ShippingMethod != await _localizationService.GetResourceAsync("freeshipping.method.name")
                && order.ShippingMethod != await _localizationService.GetResourceAsync("WhiteGloveService.method.name"))
            {
                wgsCharges = "";
            }

            tokens.Add(new Token("Order.Shipping", order.OrderShippingExclTax <= 0 || !string.IsNullOrEmpty(wgsCharges) ? "Free" :
                await _priceFormatter.FormatPriceAsync(_currencyService.ConvertCurrency(order.OrderShippingExclTax, order.CurrencyRate), true, order.CustomerCurrencyCode, languageId, true)
                ));


            if (!string.IsNullOrEmpty(wgsCharges))
            {
                tokens.Add(new Token("Order.HasWgsService", true));
                tokens.Add(new Token("Order.WgsCharges", wgsCharges));
                tokens.Add(new Token("Order.Shipping.Label", await _localizationService.GetResourceAsync("order.shipping.CurbSide")));
            }
            else
            {
                tokens.Add(new Token("Order.HasWgsService", false));
                tokens.Add(new Token("Order.Shipping.Label", string.Format(
                    await _localizationService.GetResourceAsync("order.shipping.with.method"), order.ShippingMethod)));
            }

            if (!string.IsNullOrEmpty(addtionalShippingCharges))
            {
                tokens.Add(new Token("Order.HasAdditionalShippingServiceCharges", true));
                tokens.Add(new Token("Order.AdditionalShippingServiceCharges", addtionalShippingCharges));
            }
            else
                tokens.Add(new Token("Order.HasAdditionalShippingServiceCharges", false));

            if (displayDiscount)
            {
                tokens.Add(new Token("Order.HasTotalDiscount", true));
                tokens.Add(new Token("Order.TotalDiscount", cusDiscount));
            }
            else
                tokens.Add(new Token("Order.HasTotalDiscount", false));
            if (displayTax)
            {
                tokens.Add(new Token("Order.HasTax", true));
                tokens.Add(new Token("Order.Tax", cusTaxTotal));
                tokens.Add(new Token("Order.TaxInfo", await GetTaxDetails(order, shippingZipCode, taxPercentage, cusTaxTotal, languageId), true));
            }
            else
                tokens.Add(new Token("Order.HasTax", false));

            tokens.Add(new Token("Order.Total", cusTotal));


        }
        protected virtual async Task AddCustomOrderTotalTokens(IList<Token> tokens, CustomOrder customOrder, string shippingZipPostalCode, int vendorId, int languageId)
        {
            //decimal subTotal = 0;
            //string subTotalAdjustment = "";
            //string shippingAdjustment = "";
            ////notes


            ////subtotal

            //subTotal = customOrder.SubTotal == null ? 0 : Convert.ToDecimal(customOrder.SubTotal);
            //decimal orderTotal = customOrder.OrderTotal == null ? 0 : (Convert.ToDecimal(customOrder.OrderTotal));

            //tokens.Add(new Token("Order.Notes", customOrder.InvoiceNote));
            //tokens.Add(new Token("Order.SpecialInstructionsfromBuyer", customOrder.SpecialInstructionsfromBuyer));
            //tokens.Add(new Token("Order.Subtotal", await _priceFormatter.FormatPriceAsync(subTotal)));

            //if (customOrder.CustomDuty > 0)
            //{
            //    tokens.Add(new Token("Order.HasCustomDuty", true));
            //    tokens.Add(new Token("Order.CustomDutyPercentage", Math.Round(customOrder.CustomDutyPercentage, 2) + "%"));
            //    tokens.Add(new Token("Order.CustomDuty", await _priceFormatter.FormatPriceAsync
            //        (customOrder.CustomDuty)));
            //}
            //else
            //    tokens.Add(new Token("Order.HasCustomDuty", false));

            //var _customOrderService = EngineContext.Current.Resolve<Nop.Services.Customizations.Phone_Order.ICustomOrderService>();
            //var orderSummaryAdj = await _customOrderService.GetOrderSummaryAdjustment(customOrder.Id);
            //decimal netAmount = subTotal;
            //if (orderSummaryAdj != null)
            //{
            //    #region SubTotalAdj

            //    string subTotalDiscountType = orderSummaryAdj.SubTotalDiscountType == null ? "" :
            //       (orderSummaryAdj.SubTotalDiscountType == DiscountType.Percentage.ToString() ? DiscountType.Percentage.ToString() : DiscountType.Fixed.ToString());

            //    decimal discountAmount = orderSummaryAdj.SubtotalDiscount == null ? 0 :
            //        Convert.ToDecimal(orderSummaryAdj.SubtotalDiscount);
            //    if (discountAmount != 0)
            //    {
            //        subTotalAdjustment = orderSummaryAdj.SubtotalChargeType == ChargeType.Add.ToString() ? "+" : "-" + await _priceFormatter.FormatPriceAsync(
            //            subTotalDiscountType == DiscountType.Percentage.ToString() ?
            //            (subTotal * discountAmount) / 100
            //            : discountAmount);
            //        netAmount +=
            //            (orderSummaryAdj.SubtotalChargeType == ChargeType.Add.ToString() ? 1 : -1) *
            //            (
            //                subTotalDiscountType == DiscountType.Percentage.ToString()
            //                    ? (subTotal * discountAmount) / 100
            //                    : discountAmount
            //            );
            //    }




            //    #endregion

            //    #region ShippingAdj

            //    string shippingDiscountType = orderSummaryAdj.ShippingDiscountType == null ? "" :
            //       (orderSummaryAdj.ShippingDiscountType == DiscountType.Percentage.ToString() ? DiscountType.Percentage.ToString() : DiscountType.Fixed.ToString());

            //    discountAmount = orderSummaryAdj.ShippingDiscount == null ? 0 : Convert.ToDecimal(orderSummaryAdj.ShippingDiscount);
            //    if (discountAmount != 0)
            //    {
            //        shippingAdjustment = (orderSummaryAdj.ShippingChargeType == ChargeType.Add.ToString() ? "+" : "-") + await _priceFormatter.FormatPriceAsync(shippingDiscountType == DiscountType.Percentage.ToString() ?
            //        ((customOrder.Shipping == null ? 0 : Convert.ToDecimal(customOrder.Shipping)) * discountAmount) / 100
            //            : discountAmount);
            //    }
            //    #endregion
            //}

            //tokens.Add(new Token("Order.NetSubTotal", await _priceFormatter.FormatPriceAsync
            //       (netAmount)));

            ////discount (applied to order subtotal)
            //if (subTotalAdjustment != "")
            //{
            //    tokens.Add(new Token("Order.HasSubtotalDiscount", true));
            //    tokens.Add(new Token("Order.SubtotalDiscount", subTotalAdjustment));
            //}
            //else
            //    tokens.Add(new Token("Order.HasSubtotalDiscount", false));

            //if (customOrder.Shipping != null)
            //{
            //    tokens.Add(new Token("Order.Shipping.label", await _localizationService.GetResourceAsync("order.shipping.CurbSide")));
            //    tokens.Add(new Token("Order.HasShipping", true));
            //    tokens.Add(new Token("Order.Shipping", customOrder.Shipping > 0 ? await _priceFormatter.FormatPriceAsync(Convert.ToDecimal(customOrder.Shipping)) : "Free"));
            //}
            //else
            //    tokens.Add(new Token("Order.HasShipping", false));

            //if (!string.IsNullOrEmpty(shippingAdjustment))
            //{
            //    tokens.Add(new Token("Order.HasShippingDiscount", true));
            //    tokens.Add(new Token("Order.ShippingDiscount", shippingAdjustment));
            //}
            //else
            //    tokens.Add(new Token("Order.HasShippingDiscount", false));

            //if ((customOrder.Wgs != null && customOrder.Wgs > 0) || customOrder.ComplementryWgsFree)
            //{
            //    tokens.Add(new Token("Order.HasWgs", true));
            //    tokens.Add(new Token("Order.Wgs", await _priceFormatter.FormatPriceAsync(Convert.ToDecimal(customOrder.Wgs))));
            //}
            //else
            //    tokens.Add(new Token("Order.HasWgs", false));

            //if (customOrder.OrderTax != null)
            //{
            //    tokens.Add(new Token("Order.HasTax", true));
            //    tokens.Add(new Token("Order.TaxInfo", await CustomOrderGetTaxDetails(customOrder.TaxInfo, await _priceFormatter.FormatPriceAsync(Convert.ToDecimal(customOrder.OrderTax)), shippingZipPostalCode, languageId), true));

            //}
            //else
            //    tokens.Add(new Token("Order.HasTax", false));

            //var orderTypes = await _customOrderService.GetOrderTypes();
            //string OrderType = (orderTypes.Where(o => o.Id == customOrder.OrderTypeId)).FirstOrDefault()?.Name;

            //if (OrderType == OrderTypes.HouzzOrder.ToString() && customOrder.HouzzFee != null && customOrder.HouzzFee > 0)
            //{
            //    decimal houzzFee = customOrder.HouzzFeeType == DiscountType.Percentage.ToString() ?
            //        (orderTotal * Convert.ToDecimal(customOrder.HouzzFee)) / 100 : Convert.ToDecimal(customOrder.HouzzFee);
            //    orderTotal = orderTotal - houzzFee;
            //    tokens.Add(new Token("Order.HasHouzzFee", true));
            //    tokens.Add(new Token("Order.HouzzFee", "-" + await _priceFormatter.FormatPriceAsync(houzzFee)));
            //}
            //else
            //    tokens.Add(new Token("Order.HasHouzzFee", false));

            //if ((OrderType == OrderTypes.AlreadyPaid.ToString() || OrderType == OrderTypes.CustomOrder.ToString())
            //      && customOrder.AlreadyFee != null && customOrder.AlreadyFee > 0 && !customOrder.FullPaid)
            //{

            //    if (OrderType != OrderTypes.AlreadyPaid.ToString())
            //        tokens.Add(new Token("Order.OrderType", OrderType));
            //    else
            //    {
            //        string subOrderType = (orderTypes.Where(o => o.Id == customOrder.SubOrderTypeId)).FirstOrDefault()?.Name;
            //        tokens.Add(new Token("Order.OrderType", subOrderType ?? OrderType));
            //    }

            //    decimal initialPayment = (orderTotal * Convert.ToDecimal(customOrder.AlreadyFee)) / 100;
            //    decimal pendingPayment = orderTotal - initialPayment;
            //    if (OrderType == OrderTypes.CustomOrder.ToString())
            //    {
            //        tokens.Add(new Token("Order.HasInitialPayment", true));
            //        tokens.Add(new Token("Order.InitialPayment", await _priceFormatter.FormatPriceAsync(initialPayment)));
            //        tokens.Add(new Token("Order.HasPendingPayment", true));
            //        tokens.Add(new Token("Order.PendingPayment", await _priceFormatter.FormatPriceAsync(pendingPayment)));
            //        tokens.Add(new Token("Order.HasAlreadyPaid", false));
            //        tokens.Add(new Token("Order.HasPayableAmount", true));
            //        decimal payableAmount = 0;
            //        if (customOrder.LiveOrderNumber == null || customOrder.LiveOrderNumber == 0)
            //            payableAmount = initialPayment;
            //        else if (!customOrder.FullPaid)
            //            payableAmount = pendingPayment;
            //        else
            //            payableAmount = 0;
            //        tokens.Add(new Token("Order.PayableAmount", await _priceFormatter.FormatPriceAsync(Convert.ToDecimal(payableAmount))));
            //    }
            //    else
            //    {

            //        tokens.Add(new Token("Order.HasAlreadyPaid", true));
            //        tokens.Add(new Token("Order.AlreadyPaid", await _priceFormatter.FormatPriceAsync(initialPayment)));
            //        tokens.Add(new Token("Order.HasPendingPayment", false));
            //        tokens.Add(new Token("Order.HasInitialPayment", false));
            //        tokens.Add(new Token("Order.HasPayableAmount", false));
            //    }

            //}
            //else
            //{
            //    if (OrderType != OrderTypes.AlreadyPaid.ToString())
            //        tokens.Add(new Token("Order.OrderType", OrderType));
            //    else
            //    {
            //        string subOrderType = (orderTypes.Where(o => o.Id == customOrder.SubOrderTypeId)).FirstOrDefault()?.Name;
            //        tokens.Add(new Token("Order.OrderType", subOrderType ?? OrderType));
            //    }
            //    tokens.Add(new Token("Order.HasPendingPayment", false));
            //    tokens.Add(new Token("Order.HasInitialPayment", false));
            //    tokens.Add(new Token("Order.HasAlreadyPaid", false));
            //    tokens.Add(new Token("Order.HasPayableAmount", false));
            //}

            //tokens.Add(new Token("Order.Total", await _priceFormatter.FormatPriceAsync(orderTotal)));


        }
        protected virtual async Task CustomWriteTotalsAsync(Order order, decimal customSubTotal, Language language, StringBuilder sb)
        {

            //subtotal
            string cusSubTotal;
            var displaySubTotalDiscount = false;
            var cusSubTotalDiscount = string.Empty;
            var languageId = language.Id;
            if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal)
            {
                //including tax

                //subtotal
                var orderSubtotalInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubtotalInclTax, order.CurrencyRate);
                cusSubTotal = await _priceFormatter.FormatPriceAsync(orderSubtotalInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);
                //discount (applied to order subtotal)
                var orderSubTotalDiscountInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubTotalDiscountInclTax, order.CurrencyRate);
                if (orderSubTotalDiscountInclTaxInCustomerCurrency > decimal.Zero)
                {
                    cusSubTotalDiscount = await _priceFormatter.FormatPriceAsync(-orderSubTotalDiscountInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);
                    displaySubTotalDiscount = true;
                }
            }
            else
            {
                //excluding tax

                //subtotal
                var orderSubtotalExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubtotalExclTax, order.CurrencyRate);
                cusSubTotal = await _priceFormatter.FormatPriceAsync(orderSubtotalExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);
                //discount (applied to order subtotal)
                var orderSubTotalDiscountExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubTotalDiscountExclTax, order.CurrencyRate);
                if (orderSubTotalDiscountExclTaxInCustomerCurrency > decimal.Zero)
                {
                    cusSubTotalDiscount = await _priceFormatter.FormatPriceAsync(-orderSubTotalDiscountExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);
                    displaySubTotalDiscount = true;
                }
            }

            //shipping, payment method fee
            string cusShipTotal;
            string cusPaymentMethodAdditionalFee;
            var taxRates = new SortedDictionary<decimal, decimal>();
            var cusTaxTotal = string.Empty;
            var cusDiscount = string.Empty;
            if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
            {
                //including tax

                //shipping
                var orderShippingInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingInclTax, order.CurrencyRate);
                cusShipTotal = await _priceFormatter.FormatShippingPriceAsync(orderShippingInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);
                //payment method additional fee
                var paymentMethodAdditionalFeeInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeInclTax, order.CurrencyRate);
                cusPaymentMethodAdditionalFee = await _priceFormatter.FormatPaymentMethodAdditionalFeeAsync(paymentMethodAdditionalFeeInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);
            }
            else
            {
                //excluding tax

                //shipping
                var orderShippingExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingExclTax, order.CurrencyRate);
                cusShipTotal = await _priceFormatter.FormatShippingPriceAsync(orderShippingExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);
                //payment method additional fee
                var paymentMethodAdditionalFeeExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeExclTax, order.CurrencyRate);
                cusPaymentMethodAdditionalFee = await _priceFormatter.FormatPaymentMethodAdditionalFeeAsync(paymentMethodAdditionalFeeExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);
            }

            //shipping
            var displayShipping = order.ShippingStatus != ShippingStatus.ShippingNotRequired;

            //payment method fee
            var displayPaymentMethodFee = order.PaymentMethodAdditionalFeeExclTax > decimal.Zero;

            //tax
            bool displayTax;
            bool displayTaxRates;
            if (_taxSettings.HideTaxInOrderSummary && order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
            {
                displayTax = false;
                displayTaxRates = false;
            }
            else
            {
                if (order.OrderTax == 0 && _taxSettings.HideZeroTax)
                {
                    displayTax = false;
                    displayTaxRates = false;
                }
                else
                {
                    taxRates = new SortedDictionary<decimal, decimal>();
                    foreach (var tr in _orderService.ParseTaxRates(order, order.TaxRates))
                        taxRates.Add(tr.Key, _currencyService.ConvertCurrency(tr.Value, order.CurrencyRate));

                    displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Any();
                    displayTax = !displayTaxRates;

                    var orderTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTax, order.CurrencyRate);
                    var taxStr = await _priceFormatter.FormatPriceAsync(orderTaxInCustomerCurrency, true, order.CustomerCurrencyCode,
                        false, languageId);
                    cusTaxTotal = taxStr;
                }
            }

            //discount
            var displayDiscount = false;
            if (order.OrderDiscount > decimal.Zero)
            {
                var orderDiscountInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderDiscount, order.CurrencyRate);
                cusDiscount = await _priceFormatter.FormatPriceAsync(orderDiscountInCustomerCurrency < 0 ?
                    -orderDiscountInCustomerCurrency : orderDiscountInCustomerCurrency, true, order.CustomerCurrencyCode, false, languageId);
                displayDiscount = true;
            }

            //total
            var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
            var cusTotal = await _priceFormatter.FormatPriceAsync(orderTotalInCustomerCurrency, true, order.CustomerCurrencyCode, false, languageId);
            string membershipFee = "";
            string membershipFeeDiscount = "";
            string membershipDiscountIncTax = "";
            string offerDiscountIncTax = "";
            string buyMoreSaveMoreDiscountIncTax = "";
            if (customSubTotal > 0)
            {
                cusSubTotal = await _priceFormatter.FormatPriceAsync(customSubTotal, true, order.CustomerCurrencyCode, languageId, true);
                if (order.MembershipFeeInclTax > 0)
                    membershipFee = await _priceFormatter.FormatPriceAsync(_currencyService.ConvertCurrency(order.MembershipFeeInclTax, order.CurrencyRate), true, order.CustomerCurrencyCode, languageId, true);
                if (order.MembershipFeeDiscountInclTax != 0)
                    membershipFeeDiscount = "-" + await _priceFormatter.FormatPriceAsync(_currencyService.ConvertCurrency(order.MembershipFeeDiscountInclTax, order.CurrencyRate), true, order.CustomerCurrencyCode, languageId, true);
                if (order.MembershipDiscountIncTax != 0)
                    membershipDiscountIncTax = "-" + await _priceFormatter.FormatPriceAsync(_currencyService.ConvertCurrency(order.MembershipDiscountIncTax, order.CurrencyRate), true, order.CustomerCurrencyCode, languageId, true);
                if (order.OfferDiscountIncTax != 0)
                    offerDiscountIncTax = "-" + await _priceFormatter.FormatPriceAsync(_currencyService.ConvertCurrency(order.OfferDiscountIncTax, order.CurrencyRate), true, order.CustomerCurrencyCode, languageId, true);
                if (order.BuyMoreSaveMoreDiscountIncTax != 0)
                    buyMoreSaveMoreDiscountIncTax = "-" + await _priceFormatter.FormatPriceAsync(_currencyService.ConvertCurrency(order.BuyMoreSaveMoreDiscountIncTax, order.CurrencyRate), true, order.CustomerCurrencyCode, languageId, true);
            }
            //subtotal
            sb.AppendLine($"<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{await _localizationService.GetResourceAsync("Messages.Order.SubTotal", languageId)}</strong></td> <td style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{cusSubTotal}</strong></td></tr>");

            //discount (applied to order subtotal)
            if (displaySubTotalDiscount)
                sb.AppendLine($"<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{await _localizationService.GetResourceAsync("Messages.Order.SubTotalDiscount", languageId)}</strong></td> <td style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{cusSubTotalDiscount}</strong></td></tr>");

            if (!string.IsNullOrEmpty(membershipFee))
                sb.AppendLine($"<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{await _localizationService.GetResourceAsync("ShoppingCart.Totals.MemberShipFee", languageId)}</strong></td> <td style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{membershipFee}</strong></td></tr>");


            if (!string.IsNullOrEmpty(membershipFeeDiscount))
                sb.AppendLine($"<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{await _localizationService.GetResourceAsync("ShoppingCart.Totals.MemberShipFee.Discount", languageId)}</strong></td> <td style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong  class=\"color-red\">{membershipFeeDiscount}</strong></td></tr>");


            if (!string.IsNullOrEmpty(membershipDiscountIncTax))
                sb.AppendLine($"<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{await _localizationService.GetResourceAsync("ShoppingCart.Totals.MemberShipDiscount", languageId)}</strong></td> <td style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong  class=\"color-red\">{membershipDiscountIncTax}</strong></td></tr>");

            if (!string.IsNullOrEmpty(offerDiscountIncTax))
                sb.AppendLine($"<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{await _localizationService.GetResourceAsync("ShoppingCart.Totals.offerDiscount", languageId)}</strong></td> <td style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong  class=\"color-red\">{offerDiscountIncTax}</strong></td></tr>");

            if (!string.IsNullOrEmpty(buyMoreSaveMoreDiscountIncTax))
                sb.AppendLine($"<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{await _localizationService.GetResourceAsync("ShoppingCart.Totals.BuyMoreSaveMoreDiscount", languageId)}</strong></td> <td style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong  class=\"color-red\">{buyMoreSaveMoreDiscountIncTax}</strong></td></tr>");



            //shipping
            if (displayShipping)
            {
                sb.AppendLine($"<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{await _localizationService.GetResourceAsync("Messages.Order.Shipping", languageId)}</strong></td> <td style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{cusShipTotal}</strong></td></tr>");
            }

            //payment method fee
            if (displayPaymentMethodFee)
            {
                var paymentMethodFeeTitle = await _localizationService.GetResourceAsync("Messages.Order.PaymentMethodAdditionalFee", languageId);
                sb.AppendLine($"<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{paymentMethodFeeTitle}</strong></td> <td style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{cusPaymentMethodAdditionalFee}</strong></td></tr>");
            }

            //tax
            if (displayTax)
            {
                sb.AppendLine($"<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{await _localizationService.GetResourceAsync("Messages.Order.Tax", languageId)}</strong></td> <td style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{cusTaxTotal}</strong></td></tr>");
            }

            if (displayTaxRates)
            {
                foreach (var item in taxRates)
                {
                    var taxRate = string.Format(await _localizationService.GetResourceAsync("Messages.Order.TaxRateLine"),
                        _priceFormatter.FormatTaxRate(item.Key));
                    var taxValue = await _priceFormatter.FormatPriceAsync(item.Value, true, order.CustomerCurrencyCode, false, languageId);
                    sb.AppendLine($"<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{taxRate}</strong></td> <td style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{taxValue}</strong></td></tr>");
                }
            }

            //discount
            if (displayDiscount)
            {
                sb.AppendLine($"<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{await _localizationService.GetResourceAsync("Messages.Order.TotalDiscount", languageId)}</strong></td> <td style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{cusDiscount}</strong></td></tr>");
            }

            //gift cards
            foreach (var gcuh in await _giftCardService.GetGiftCardUsageHistoryAsync(order))
            {
                var giftCardText = string.Format(await _localizationService.GetResourceAsync("Messages.Order.GiftCardInfo", languageId),
                    WebUtility.HtmlEncode((await _giftCardService.GetGiftCardByIdAsync(gcuh.GiftCardId))?.GiftCardCouponCode));
                var giftCardAmount = await _priceFormatter.FormatPriceAsync(-_currencyService.ConvertCurrency(gcuh.UsedValue, order.CurrencyRate), true, order.CustomerCurrencyCode,
                    false, languageId);
                sb.AppendLine($"<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{giftCardText}</strong></td> <td style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{giftCardAmount}</strong></td></tr>");
            }

            //reward points
            if (order.RedeemedRewardPointsEntryId.HasValue && await _rewardPointService.GetRewardPointsHistoryEntryByIdAsync(order.RedeemedRewardPointsEntryId.Value) is RewardPointsHistory redeemedRewardPointsEntry)
            {
                var rpTitle = string.Format(await _localizationService.GetResourceAsync("Messages.Order.RewardPoints", languageId),
                    -redeemedRewardPointsEntry.Points);
                var rpAmount = await _priceFormatter.FormatPriceAsync(-_currencyService.ConvertCurrency(redeemedRewardPointsEntry.UsedAmount, order.CurrencyRate), true,
                    order.CustomerCurrencyCode, false, languageId);
                sb.AppendLine($"<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{rpTitle}</strong></td> <td style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{rpAmount}</strong></td></tr>");
            }

            //total
            sb.AppendLine($"<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{await _localizationService.GetResourceAsync("Messages.Order.OrderTotal", languageId)}</strong></td> <td style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{cusTotal}</strong></td></tr>");
        }
        public async Task<string> CustomLatestCartItemHtml(Product product, string cartLink, string utmSource)
        {

            var picture = (await _pictureService.CustomGetPicturesOfProducAsync(product.Id, 1)).FirstOrDefault();
            string fullSizeImageUrl, imageUrl;
            (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.CategoryThumbPictureSize);
            (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);

            return $"<a href=\"{cartLink}?{utmSource}\" target=\"_blank\"><img src = \"{fullSizeImageUrl}\" style = \"width:100%; max-width: 549px;max-height:550px; height: auto;\" class=\"CToWUd\" data-bit=\"iit\"></a> <a href=\"{cartLink}?{utmSource}\" style=\"width:100%;text-align:center;margin-top:21px;margin-bottom:21px;float:left;display:block;font-size:21px;color: #00012d;text-decoration:none;max-width:100%;text-align:center;\" >{product.Name}</a> ";
        }


        public async Task<string> CustomAddRelatedProductHtml(List<Product> relatedProducts, string utmSource)
        {
            string html = "";


            foreach (var product in relatedProducts)
            {
                var prdurl = await RouteUrlAsync(_storeContext.GetCurrentStore().Id, "product", new
                {
                    id = product.Id,
                    SeName = await _urlRecordService.GetSeNameAsync(product)
                });
                var picture = (await _pictureService.CustomGetPicturesOfProducAsync(product.Id, 1)).FirstOrDefault();
                string fullSizeImageUrl, imageUrl;
                (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.CategoryThumbPictureSize);
                (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                html += $"<td style=\"    padding: 0 10px;\">        <div style=\"float:left;width:100%\">            <a href=\"{prdurl}?{utmSource}\" target=\"_blank\">                <img src=\"{fullSizeImageUrl}\" style=\"width:100%;max-width:190px;max-height:190px; height: auto;\" class=\"CToWUd\" data-bit=\"iit\">            </a>            <a href=\"{prdurl}?{utmSource}\" target=\"_blank\" style=\"float: left; display: block;font-size: 15px;color: #00012d;text-decoration: none;  margin-top:13px;margin-bottom:13px;          max-width: 100%;margin: 0 auto;text-align: center;padding: 0 0px;\">                      {product.Name}            </a>        </div>    </td>";
            }
            return html;
        }

        #endregion
    }
}
