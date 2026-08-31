
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc.Rendering;
using MWT.Nop.Core.Domain.CustomOrders;
using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Services.Catalog;
using MWT.Nop.Core.Services.Customizations.CustomOrders;
using MWT.Nop.Core.Services.Media;
using MWT.Nop.Core.Services.Orders;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Factories;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Orders;
using MWT.Plugin.Misc.MwtStorefront.Models.Api;
using MWT.Plugin.Misc.MwtStorefront.Models.Order;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Common.Pdf;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Models.Common;
using Nop.Web.Models.Media;
using Nop.Web.Models.Order;
using System.Dynamic;
using System.Text.RegularExpressions;
using static MWT.Plugin.Misc.MwtStorefront.Models.Api.ProductModel;
using NopHttp = Nop.Core.Http;
using NopOrderStatus = Nop.Core.Domain.Orders.OrderStatus;

namespace MWT.Plugin.Misc.MwtStorefront.Factories
{
    public partial class OrderExtendedModelFactory : OrderModelFactory, IOrderExtendedModelFactory
    {
        #region Fields
        private readonly IEncryptionService _encryptionService;
        private readonly IAddressExtendedModelFactory _addressExtendedModelFactory;
        private readonly IOrderExtendedService _orderExtendedService;
        private readonly ICustomProductAttributeFormatter _productAttributeFormatter;
        private readonly IProductExtendedService _productExtendedService;
        private readonly IPictureExtendedService _pictureExtendedService;
        private readonly ISettingService _settingService;
        private readonly ILanguageService _languageService;
        private readonly ICustomOrderService _customOrderService;
        private readonly ICustomOrderModelFactory _customOrderModelFactory;
        private readonly IGdprService _gdprService;
        private readonly GdprSettings _gdprSettings;
        private readonly ICustomProductModelFactory _customProductModelFactory;
        private readonly ICustomSpecificationAttributeService _specificationAttributeService;
        private readonly IProductTagService _productTagService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IShippingPluginManager _shippingPluginManager;
        private readonly ICustomProductAttributeService _productAttributeService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly ILogger _logger;




        #endregion

        #region Ctor
        public OrderExtendedModelFactory(AddressSettings addressSettings, CatalogSettings catalogSettings, IAddressModelFactory addressModelFactory, IAddressService addressService,
            ICountryService countryService, ICurrencyService currencyService, ICustomerService customerService, IDateTimeHelper dateTimeHelper, IGiftCardService giftCardService,
            ILocalizationService localizationService, IOrderProcessingService orderProcessingService, IOrderService orderService, IOrderTotalCalculationService orderTotalCalculationService,
            IPaymentPluginManager paymentPluginManager, IPaymentService paymentService, IPictureService pictureService, IPriceFormatter priceFormatter, IProductService productService,
            IRewardPointService rewardPointService, IShipmentService shipmentService, IShortTermCacheManager shortTermCacheManager, IStateProvinceService stateProvinceService, IStaticCacheManager staticCacheManager,
            IStoreContext storeContext, IUrlRecordService urlRecordService, IVendorService vendorService, IWebHelper webHelper, IWorkContext workContext,
            MediaSettings mediaSettings, OrderSettings orderSettings, PdfSettings pdfSettings, RewardPointsSettings rewardPointsSettings, ShippingSettings shippingSettings,
            TaxSettings taxSettings, VendorSettings vendorSettings, IEncryptionService encryptionService, IAddressExtendedModelFactory addressExtendedModelFactory,
            IOrderExtendedService orderExtendedService, ICustomProductAttributeFormatter productAttributeFormatter, IProductExtendedService productExtendedService,
            IPictureExtendedService pictureExtendedService, ISettingService settingService, ILanguageService languageService, ICustomOrderService customOrderService,
            ICustomOrderModelFactory customOrderModelFactory, IGdprService gdprService, GdprSettings gdprSettings,
            ICustomSpecificationAttributeService specificationAttributeService, IProductTagService productTagService,
            IShoppingCartService shoppingCartService, IShippingPluginManager shippingPluginManager, ICustomProductAttributeService productAttributeService,
            IProductAttributeParser productAttributeParser, ILogger logger) :
            base(addressSettings, catalogSettings, addressModelFactory, addressService, countryService, currencyService, customerService, dateTimeHelper, giftCardService, localizationService, orderProcessingService, orderService, orderTotalCalculationService, paymentPluginManager, paymentService, pictureService, priceFormatter, productService, rewardPointService, shipmentService, shortTermCacheManager, stateProvinceService, staticCacheManager, storeContext, urlRecordService, vendorService, webHelper, workContext, mediaSettings, orderSettings, pdfSettings, rewardPointsSettings, shippingSettings, taxSettings, vendorSettings)
        {
            _encryptionService = encryptionService;
            _addressExtendedModelFactory = addressExtendedModelFactory;
            _orderExtendedService = orderExtendedService;
            _productAttributeFormatter = productAttributeFormatter;
            _productExtendedService = productExtendedService;
            _pictureExtendedService = pictureExtendedService;
            _settingService = settingService;
            _languageService = languageService;
            _customOrderService = customOrderService;
            _customOrderModelFactory = customOrderModelFactory;
            _gdprService = gdprService;
            _gdprSettings = gdprSettings;
            _specificationAttributeService = specificationAttributeService;
            _productTagService = productTagService;
            _shippingPluginManager = shippingPluginManager;
            _shoppingCartService = shoppingCartService;
            _productAttributeService = productAttributeService;
            _productAttributeParser = productAttributeParser;
            _logger = logger;
        }
        #endregion

        #region Methods

        public virtual async Task<CustomerOrderExtendedListModel> CustomPrepareCustomerOrderListModelAsync(int? page, OrderHistoryPeriods limit)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var pageSize = _orderSettings.CustomerOrdersPageSize;
            var pageIndex = Math.Max((page ?? 0) - 1, 0);
            var orders = await _orderService.SearchOrdersAsync(storeId: store.Id,
              customerId: customer.Id,
              createdFromUtc: limit == OrderHistoryPeriods.All ? null : DateTime.UtcNow.AddDays((int)limit * -1),
              createdToUtc: limit > 0 ? DateTime.UtcNow : null,
              pageIndex: pageIndex,
              pageSize: pageSize);
            var periods = await Enum.GetValues<OrderHistoryPeriods>()
         .SelectAwait(async enumValue => new
         {
             ID = enumValue.ToString().ToLower(),
             Name = await _localizationService.GetLocalizedEnumAsync(enumValue)
         }).ToListAsync();

            var model = new CustomerOrderExtendedListModel
            {
                AvailableLimits = new SelectList(periods, "ID", "Name", limit.ToString()).ToList(),
                PagerModel = new PagerModel(_localizationService)
                {
                    PageSize = orders.PageSize,
                    TotalRecords = orders.TotalCount,
                    PageIndex = orders.PageIndex,
                    ShowTotalSummary = true,
                    RouteActionName = NopHttp.NopRouteNames.Standard.CUSTOMER_ORDERS_PAGED,
                    UseRouteLinks = true,
                    RouteValues = new CustomerOrdersRouteValues { PageNumber = orders.PageIndex, Limit = limit.ToString().ToLower() }
                }
            };


            foreach (var order in orders)
            {
                var orderModel = new CustomerOrderExtendedModel
                {
                    Id = order.Id,
                    CustomerEmail = order.CustomerEmail,
                    CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(order.CreatedOnUtc, DateTimeKind.Utc),
                    OrderStatusEnum = order.OrderStatus,
                    OrderStatus = await _localizationService.GetLocalizedEnumAsync(order.OrderStatus),
                    PaymentStatus = await _localizationService.GetLocalizedEnumAsync(order.PaymentStatus),
                    ShippingStatus = await _localizationService.GetLocalizedEnumAsync(order.ShippingStatus),
                    IsReturnRequestAllowed = await _orderProcessingService.IsReturnRequestAllowedAsync(order),
                    CustomOrderNumber = order.CustomOrderNumber
                };
                var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
                orderModel.OrderTotal = await _priceFormatter.FormatPriceAsync(orderTotalInCustomerCurrency, true, order.CustomerCurrencyCode, false, (await _workContext.GetWorkingLanguageAsync()).Id);

                model.Orders.Add(orderModel);
            }



            return model;
        }
        public virtual async Task<OrderDetailsExtendedModel> PrepareCustomOrderDetailsModelAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));


            var model = new OrderDetailsExtendedModel
            {
                Id = order.Id,
                CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(order.CreatedOnUtc, DateTimeKind.Utc),
                OrderStatus = order.OrderStatus,
                OrderStatusText = await _localizationService.GetLocalizedEnumAsync(order.OrderStatus),
                IsReOrderAllowed = _orderSettings.IsReOrderAllowed,
                IsReturnRequestAllowed = await _orderProcessingService.IsReturnRequestAllowedAsync(order),
                PdfInvoiceDisabled = _pdfSettings.DisablePdfInvoicesForPendingOrders && order.OrderStatus == NopOrderStatus.Pending,
                CustomOrderNumber = order.CustomOrderNumber,
                OrderGuid = order.OrderGuid,
                //shipping info
                ShippingStatus = await _localizationService.GetLocalizedEnumAsync(order.ShippingStatus),
                CustomerCurrencyCode = order.CustomerCurrencyCode,
                CreditCardNumber = string.IsNullOrEmpty(order.MaskedCreditCardNumber) ? "" : _encryptionService.DecryptText(order.MaskedCreditCardNumber)
            };
            if (order.ShippingStatus != ShippingStatus.ShippingNotRequired)
            {
                model.IsShippable = true;
                model.PickupInStore = order.PickupInStore;
                if (!order.PickupInStore)
                {
                    var shippingAddress = await _addressService.GetAddressByIdAsync(order.ShippingAddressId ?? 0);

                    await _addressExtendedModelFactory.PrepareCustomAddressModelAsync(model.ShippingAddress,
                        address: shippingAddress,
                        excludeProperties: false,
                        addressSettings: _addressSettings);
                }
                else if (order.PickupAddressId.HasValue && await _addressService.GetAddressByIdAsync(order.PickupAddressId.Value) is Address pickupAddress)
                {
                    model.PickupAddress = new AddressModel
                    {
                        Address1 = pickupAddress.Address1,
                        City = pickupAddress.City,
                        County = pickupAddress.County,
                        StateProvinceName = await _stateProvinceService.GetStateProvinceByAddressAsync(pickupAddress) is StateProvince stateProvince
                            ? await _localizationService.GetLocalizedAsync(stateProvince, entity => entity.Name)
                            : string.Empty,
                        CountryName = await _countryService.GetCountryByAddressAsync(pickupAddress) is Country country
                            ? await _localizationService.GetLocalizedAsync(country, entity => entity.Name)
                            : string.Empty,
                        ZipPostalCode = pickupAddress.ZipPostalCode
                    };
                }

                model.ShippingMethod = order.ShippingMethod;

                //shipments (only already shipped)
                var shipments = (await _shipmentService.GetShipmentsByOrderIdAsync(order.Id, true)).OrderBy(x => x.CreatedOnUtc).ToList();
                foreach (var shipment in shipments)
                {
                    var shipmentModel = new OrderDetailsModel.ShipmentBriefModel
                    {
                        Id = shipment.Id,
                        TrackingNumber = shipment.TrackingNumber,
                    };
                    if (shipment.ShippedDateUtc.HasValue)
                        shipmentModel.ShippedDate = await _dateTimeHelper.ConvertToUserTimeAsync(shipment.ShippedDateUtc.Value, DateTimeKind.Utc);
                    if (shipment.DeliveryDateUtc.HasValue)
                        shipmentModel.DeliveryDate = await _dateTimeHelper.ConvertToUserTimeAsync(shipment.DeliveryDateUtc.Value, DateTimeKind.Utc);
                    model.Shipments.Add(shipmentModel);
                }
            }

            var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);

            //billing info
            await _addressExtendedModelFactory.PrepareCustomAddressModelAsync(model.BillingAddress,
                address: billingAddress,
                excludeProperties: false,
                addressSettings: _addressSettings);

            //VAT number
            model.VatNumber = order.VatNumber;

            var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;

            //payment method
            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
            model.Email = order.CustomerEmail;
            if (string.IsNullOrEmpty(model.Email))
                model.Email = model.BillingAddress?.Email;

            var paymentMethod = await _paymentPluginManager
                .LoadPluginBySystemNameAsync(order.PaymentMethodSystemName, customer, order.StoreId);
            model.PaymentMethod = paymentMethod != null ? await _localizationService.GetLocalizedFriendlyNameAsync(paymentMethod, languageId) : order.PaymentMethodSystemName;
            model.PaymentMethodStatus = await _localizationService.GetLocalizedEnumAsync(order.PaymentStatus);
            model.CanRePostProcessPayment = await _paymentService.CanRePostProcessPaymentAsync(order);
            //custom values

            var customValues = new CustomValues();
            customValues.FillByXml(order.CustomValuesXml, true);
            model.CustomValues = customValues;

            //order subtotal


            if (order.AdditonalShippingChargesInclTax > 0)
                model.AdditonalShippingCharges = await _priceFormatter.FormatPriceAsync
                   (_currencyService.ConvertCurrency(order.AdditonalShippingChargesInclTax, order.CurrencyRate), true, order.CustomerCurrencyCode, languageId, true);
            if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
            {
                //including tax

                //order shipping
                var orderShippingInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingInclTax, order.CurrencyRate);
                model.OrderShipping =
                    orderShippingInclTaxInCustomerCurrency == 0 && order.ShippingMethod == await _localizationService.GetResourceAsync("freeshipping.method.name") ? "Free" :
                    await _priceFormatter.FormatShippingPriceAsync(orderShippingInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);
                //payment method additional fee
                var paymentMethodAdditionalFeeInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeInclTax, order.CurrencyRate);
                if (paymentMethodAdditionalFeeInclTaxInCustomerCurrency > decimal.Zero)
                    model.PaymentMethodAdditionalFee = await _priceFormatter.FormatPaymentMethodAdditionalFeeAsync(paymentMethodAdditionalFeeInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);
            }
            else
            {
                //excluding tax

                //order shipping
                var orderShippingExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingExclTax, order.CurrencyRate);
                model.OrderShipping =
           orderShippingExclTaxInCustomerCurrency == 0 &&
           order.ShippingMethod == await _localizationService.GetResourceAsync("freeshipping.method.name") ? "Free" :
           await _priceFormatter.FormatShippingPriceAsync(orderShippingExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);
                //payment method additional fee
                var paymentMethodAdditionalFeeExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeExclTax, order.CurrencyRate);
                if (paymentMethodAdditionalFeeExclTaxInCustomerCurrency > decimal.Zero)
                    model.PaymentMethodAdditionalFee = await _priceFormatter.FormatPaymentMethodAdditionalFeeAsync(paymentMethodAdditionalFeeExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);
            }

            // customduty
            if (order.CustomDutyInclTax > 0)
            {
                model.CustomDutyPercentage = order.CustomDutyPercentage;
                model.CustomDuty = await _priceFormatter.FormatPriceAsync(_currencyService.ConvertCurrency(order.CustomDutyInclTax, order.CurrencyRate), true, order.CustomerCurrencyCode, false, languageId);
            }
            //tax
            var displayTax = true;
            var displayTaxRates = true;
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
                    var taxRates = _orderService.ParseTaxRates(order, order.TaxRates);
                    displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Any();
                    displayTax = !displayTaxRates;

                    var orderTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTax, order.CurrencyRate);
                    model.Tax = await _priceFormatter.FormatPriceAsync(orderTaxInCustomerCurrency, true, order.CustomerCurrencyCode, false, languageId);

                    foreach (var tr in taxRates)
                    {
                        model.TaxRates.Add(new OrderDetailsModel.TaxRate
                        {
                            Rate = _priceFormatter.FormatTaxRate(tr.Key),
                            Value = await _priceFormatter.FormatPriceAsync(_currencyService.ConvertCurrency(tr.Value, order.CurrencyRate), true, order.CustomerCurrencyCode, false, languageId),
                        });
                    }
                }
            }
            model.DisplayTaxRates = displayTaxRates;
            model.DisplayTax = displayTax;
            model.Taxes = _orderExtendedService.GetTaxDetails(order);
            model.DisplayTaxShippingInfo = _catalogSettings.DisplayTaxShippingInfoOrderDetailsPage;
            model.PricesIncludeTax = order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax;

            //discount (applied to order total)
            var orderDiscountInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderDiscount, order.CurrencyRate);
            if (orderDiscountInCustomerCurrency > decimal.Zero)
                model.OrderTotalDiscount = "-" + await _priceFormatter.FormatPriceAsync(orderDiscountInCustomerCurrency, true, order.CustomerCurrencyCode, false, languageId);

            //gift cards
            foreach (var gcuh in await _giftCardService.GetGiftCardUsageHistoryAsync(order))
            {
                model.GiftCards.Add(new OrderDetailsModel.GiftCard
                {
                    CouponCode = (await _giftCardService.GetGiftCardByIdAsync(gcuh.GiftCardId)).GiftCardCouponCode,
                    Amount = await _priceFormatter.FormatPriceAsync(-(_currencyService.ConvertCurrency(gcuh.UsedValue, order.CurrencyRate)), true, order.CustomerCurrencyCode, false, languageId),
                });
            }

            //reward points           
            if (order.RedeemedRewardPointsEntryId.HasValue && await _rewardPointService.GetRewardPointsHistoryEntryByIdAsync(order.RedeemedRewardPointsEntryId.Value) is RewardPointsHistory redeemedRewardPointsEntry)
            {
                model.RedeemedRewardPoints = -redeemedRewardPointsEntry.Points;
                model.RedeemedRewardPointsAmount = await _priceFormatter.FormatPriceAsync(-(_currencyService.ConvertCurrency(redeemedRewardPointsEntry.UsedAmount, order.CurrencyRate)), true, order.CustomerCurrencyCode, false, languageId);
            }

            //total
            var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
            model.OrderTotal = await _priceFormatter.FormatPriceAsync(orderTotalInCustomerCurrency, true, order.CustomerCurrencyCode, false, languageId);

            //checkout attributes
            model.CheckoutAttributeInfo = order.CheckoutAttributeDescription;

            //order notes
            foreach (var orderNote in (await _orderService.GetOrderNotesByOrderIdAsync(order.Id, true))
                .OrderByDescending(on => on.CreatedOnUtc)
                .ToList())
            {
                model.OrderNotes.Add(new OrderDetailsModel.OrderNote
                {
                    Id = orderNote.Id,
                    HasDownload = orderNote.DownloadId > 0,
                    Note = _orderService.FormatOrderNoteText(orderNote),
                    CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(orderNote.CreatedOnUtc, DateTimeKind.Utc)
                });
            }

            //purchased products
            model.ShowSku = _catalogSettings.ShowSkuOnProductDetailsPage;
            model.ShowVendorName = _vendorSettings.ShowVendorOnOrderDetailsPage;





            var orderItems = await _orderService.GetOrderItemsAsync(order.Id);


            #region Custom Discounts

            string subTotal = model.OrderSubtotal;

            decimal customSubTotal = 0;
            foreach (var item in orderItems)
            {
                if (item.ItemPriceIncTax == 0)
                {
                    customSubTotal = 0;
                    break;
                }
                else
                    customSubTotal += item.ItemPriceIncTax * item.Quantity;
            }

            if (customSubTotal > 0)
            {
                customSubTotal = _currencyService.ConvertCurrency(customSubTotal, order.CurrencyRate);
                model.OrderSubtotal = await _priceFormatter.FormatPriceAsync(customSubTotal, true, order.CustomerCurrencyCode, languageId, true);
                if (order.MembershipFeeInclTax > 0)
                    model.MembershipFee = await _priceFormatter.FormatPriceAsync(_currencyService.ConvertCurrency(order.MembershipFeeInclTax, order.CurrencyRate), true, order.CustomerCurrencyCode, languageId, true);
                if (order.MembershipFeeDiscountInclTax != 0)
                    model.MembershipFeeDiscount = "-" + await _priceFormatter.FormatPriceAsync(_currencyService.ConvertCurrency(order.MembershipFeeDiscountInclTax, order.CurrencyRate), true, order.CustomerCurrencyCode, languageId, true);
                if (order.MembershipDiscountIncTax != 0)
                    model.MembershipDiscountIncTax = "-" + await _priceFormatter.FormatPriceAsync(_currencyService.ConvertCurrency(order.MembershipDiscountIncTax, order.CurrencyRate), true, order.CustomerCurrencyCode, languageId, true);
                if (order.OfferDiscountIncTax != 0)
                    model.OfferDiscountIncTax = "-" + await _priceFormatter.FormatPriceAsync(_currencyService.ConvertCurrency(order.OfferDiscountIncTax, order.CurrencyRate), true, order.CustomerCurrencyCode, languageId, true);
                if (order.BuyMoreSaveMoreDiscountIncTax != 0)
                    model.BuyMoreSaveMoreDiscountIncTax = "-" + await _priceFormatter.FormatPriceAsync(_currencyService.ConvertCurrency(order.BuyMoreSaveMoreDiscountIncTax, order.CurrencyRate), true, order.CustomerCurrencyCode, languageId, true);
            }

            #endregion


            decimal orderItemsDiscount = 0;
            decimal orderTotal = 0;

            foreach (var orderItem in orderItems)
            {
                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
                decimal itemTotal = 0;
                var variant = await _productExtendedService.GetItemVariantInfo(orderItem.ProductId, orderItem.AttributesXml);
                var orderItemModel = new OrderDetailsExtendedModel.OrderItemExtendedModel
                {
                    Id = orderItem.Id,
                    OrderItemGuid = orderItem.OrderItemGuid,
                    Sku = await _productService.FormatSkuAsync(product, orderItem.AttributesXml),
                    VendorName = (await _vendorService.GetVendorByIdAsync(product.VendorId))?.Name ?? string.Empty,
                    ProductId = product.Id,
                    ProductName = string.IsNullOrWhiteSpace(variant?.Title) ? await _localizationService.GetLocalizedAsync(product, x => x.Name) : variant.Title,
                    ProductSeName = await _urlRecordService.GetSeNameAsync(product),
                    Quantity = orderItem.Quantity,
                    AttributeInfo = string.IsNullOrEmpty(orderItem.AttributesXml) ? orderItem.AttributeDescription : await _productAttributeFormatter.CustomFormatAttributesAsync(product, orderItem.AttributesXml),
                    Picture = (await PrepareCartItemPictureModelAsync(orderItem.AttributesXml, product, _mediaSettings.CategoryThumbPictureSize,
                    true, product.Name)),
                    VariantId = variant?.VariantId ?? 0


                };
                if (orderItem.TotalDiscount > 0)
                {
                    itemTotal = -_currencyService.ConvertCurrency(orderItem.TotalDiscount < 0 ? -orderItem.TotalDiscount : orderItem.TotalDiscount, order.CurrencyRate);
                    orderItemModel.TotalDiscount = "-" + await _priceFormatter.FormatPriceAsync(
                        _currencyService.ConvertCurrency(orderItem.TotalDiscount < 0 ? -orderItem.TotalDiscount : orderItem.TotalDiscount, order.CurrencyRate),
                         true, order.CustomerCurrencyCode, languageId, true);
                    orderItemsDiscount += _currencyService.ConvertCurrency(orderItem.TotalDiscount, order.CurrencyRate);

                }



                //rental info
                if (product.IsRental)
                {
                    var rentalStartDate = orderItem.RentalStartDateUtc.HasValue
                        ? _productService.FormatRentalDate(product, orderItem.RentalStartDateUtc.Value) : "";
                    var rentalEndDate = orderItem.RentalEndDateUtc.HasValue
                        ? _productService.FormatRentalDate(product, orderItem.RentalEndDateUtc.Value) : "";
                    orderItemModel.RentalInfo = string.Format(await _localizationService.GetResourceAsync("Order.Rental.FormattedDate"),
                        rentalStartDate, rentalEndDate);
                }


                //unit price, subtotal
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var unitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceInclTax, order.CurrencyRate);
                    orderItemModel.UnitPrice = await _priceFormatter.FormatPriceAsync(unitPriceInclTaxInCustomerCurrency,
                        true, order.CustomerCurrencyCode, languageId, true);

                    var priceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.PriceInclTax, order.CurrencyRate);
                    orderItemModel.SubTotal = await _priceFormatter.FormatPriceAsync(priceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);
                    if (customSubTotal <= 0)
                        itemTotal += priceInclTaxInCustomerCurrency;

                }
                else
                {
                    //excluding tax
                    var unitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceExclTax, order.CurrencyRate);
                    orderItemModel.UnitPrice = await _priceFormatter.FormatPriceAsync(unitPriceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);

                    var priceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.PriceExclTax, order.CurrencyRate);
                    orderItemModel.SubTotal = await _priceFormatter.FormatPriceAsync(priceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);
                    if (customSubTotal <= 0)
                        itemTotal += priceExclTaxInCustomerCurrency;
                }
                #region Custom DiscountS

                if (customSubTotal > 0)
                {

                    orderItemModel.UnitPrice = await _priceFormatter.FormatPriceAsync(_currencyService.ConvertCurrency(orderItem.ItemPriceIncTax, order.CurrencyRate), true, order.CustomerCurrencyCode, languageId, false);

                    orderItemModel.SubTotal = await _priceFormatter.FormatPriceAsync(_currencyService.ConvertCurrency(orderItem.ItemPriceIncTax, order.CurrencyRate) * orderItem.Quantity, true, order.CustomerCurrencyCode, languageId, false);
                    itemTotal += _currencyService.ConvertCurrency(orderItem.ItemPriceIncTax, order.CurrencyRate) * orderItem.Quantity;
                    if (orderItem.MembershipDiscountIncTax != 0)
                    {
                        orderItemModel.MembershipDiscountIncTax = "-" + await _priceFormatter.FormatPriceAsync(_currencyService.ConvertCurrency(orderItem.MembershipDiscountIncTax < 0 ? -orderItem.MembershipDiscountIncTax : orderItem.MembershipDiscountIncTax, order.CurrencyRate), true, order.CustomerCurrencyCode, languageId, true);

                        itemTotal += _currencyService.ConvertCurrency(orderItem.MembershipDiscountIncTax > 0 ? -orderItem.MembershipDiscountIncTax : orderItem.MembershipDiscountIncTax, order.CurrencyRate);
                    }
                    if (orderItem.OfferDiscountIncTax != 0)
                    {
                        orderItemModel.OfferDiscountIncTax = "-" + await _priceFormatter.FormatPriceAsync(_currencyService.ConvertCurrency(orderItem.OfferDiscountIncTax < 0 ?
                            -orderItem.OfferDiscountIncTax : orderItem.OfferDiscountIncTax, order.CurrencyRate), true, order.CustomerCurrencyCode, languageId, true);

                        itemTotal += _currencyService.ConvertCurrency(orderItem.OfferDiscountIncTax > 0 ? -orderItem.OfferDiscountIncTax : orderItem.OfferDiscountIncTax, order.CurrencyRate);
                    }
                    if (orderItem.BuyMoreSaveMoreDiscountIncTax != 0)
                    {
                        orderItemModel.BuyMoreSaveMoreDiscountIncTax = "-" + await _priceFormatter.FormatPriceAsync(orderItem.BuyMoreSaveMoreDiscountIncTax < 0 ? -orderItem.BuyMoreSaveMoreDiscountIncTax : orderItem.BuyMoreSaveMoreDiscountIncTax, true, order.CustomerCurrencyCode, languageId, true);
                        itemTotal += _currencyService.ConvertCurrency(orderItem.BuyMoreSaveMoreDiscountIncTax > 0 ? -orderItem.BuyMoreSaveMoreDiscountIncTax : orderItem.BuyMoreSaveMoreDiscountIncTax, order.CurrencyRate);
                    }

                    orderItemModel.ItemTotal = await _priceFormatter.FormatPriceAsync(itemTotal, true, order.CustomerCurrencyCode, languageId, true);


                }

                #endregion

                //downloadable products
                if (await _orderService.IsDownloadAllowedAsync(orderItem))
                    orderItemModel.DownloadId = product.DownloadId;
                if (await _orderService.IsLicenseDownloadAllowedAsync(orderItem))
                    orderItemModel.LicenseId = orderItem.LicenseDownloadId ?? 0;

                orderItemModel.SpecialInstructions = orderItem.SpecialInstructions;

                var picture = (await _pictureExtendedService.CustomGetPicturesOfProducAsync(product.Id, 1)).FirstOrDefault();
                string imageUrl;
                (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.CategoryThumbPictureSize);
                orderItemModel.ImageUrl = imageUrl;
                model.Items.Add(orderItemModel);
            }
            if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal)
            {
                //including tax

                //order subtotal
                if (customSubTotal == 0)
                {
                    var orderSubtotalInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubtotalInclTax, order.CurrencyRate);
                    model.OrderSubtotal = await _priceFormatter.FormatPriceAsync(orderSubtotalInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);
                }
                //discount (applied to order subtotal)
                var orderSubTotalDiscountInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubTotalDiscountInclTax, order.CurrencyRate);
                if (orderSubTotalDiscountInclTaxInCustomerCurrency > decimal.Zero || orderItemsDiscount > decimal.Zero)
                    model.OrderSubTotalDiscount = await _priceFormatter.FormatPriceAsync(-
                        (orderSubTotalDiscountInclTaxInCustomerCurrency + orderItemsDiscount), true, order.CustomerCurrencyCode, languageId, true);
            }
            else
            {
                //excluding tax

                //order subtotal
                if (customSubTotal == 0)
                {
                    var orderSubtotalExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubtotalExclTax, order.CurrencyRate);
                    model.OrderSubtotal = await _priceFormatter.FormatPriceAsync(orderSubtotalExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);
                }
                //discount (applied to order subtotal)
                var orderSubTotalDiscountExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubTotalDiscountExclTax, order.CurrencyRate);
                if (orderSubTotalDiscountExclTaxInCustomerCurrency > decimal.Zero || orderItemsDiscount > decimal.Zero)
                    model.OrderSubTotalDiscount = "-" + await _priceFormatter.FormatPriceAsync(
                        (orderSubTotalDiscountExclTaxInCustomerCurrency + orderItemsDiscount)
                        , true, order.CustomerCurrencyCode, languageId, false);
            }
            return model;
        }

        public virtual async Task PrintOrdersToPdfAsync(Stream stream, IList<Order> orders, int languageId = 0, int vendorId = 0)
        {

            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (orders == null)
                throw new ArgumentNullException(nameof(orders));

            var pageSize = PageSize.A4;

            if (_pdfSettings.LetterPageSizeEnabled)
                pageSize = PageSize.Letter;


            var doc = new Document(pageSize);
            var pdfWriter = PdfWriter.GetInstance(doc, stream);
            doc.Open();


            var ordCount = orders.Count;
            var ordNum = 0;
            foreach (var order in orders)
            {
                var pdfSettingsByStore = await _settingService.LoadSettingAsync<PdfSettings>(order.StoreId);
                var font = GetFont(await _workContext.GetWorkingLanguageAsync(), pdfSettingsByStore);
                var titleFont = font;
                titleFont.SetStyle(Font.BOLD);
                titleFont.Color = BaseColor.Black;

                var attributesFont = font;
                attributesFont.SetStyle(Font.ITALIC);



                var lang = await _languageService.GetLanguageByIdAsync(languageId == 0 ? order.CustomerLanguageId : languageId);
                if (lang == null || !lang.Published)
                    lang = await _workContext.GetWorkingLanguageAsync();

                var model = await PrepareCustomOrderDetailsModelAsync(order);

                if (order.IsCustomOrder)
                {
                    var customOrderid = (await _customOrderService.GetByOrderNumber(order.Id))?.Id ?? 0;
                    model.CustomOrderSummaryModel = await _customOrderModelFactory.PrepareOderSummaryModel(customOrderid);
                }
                await this.PrintHeaderAsync(pdfSettingsByStore, lang, model, font, titleFont, doc);


            }
            doc.Close();
        }


        #region Api Factory Methods

        public async Task<List<int>> GetNewOrders()
        {
            return await _orderExtendedService.GetNewOrdersIds();
        }
        public async Task UpdateStatusOfOrder(int orderId, bool isImported)
        {
            var order = await this._orderService.GetOrderByIdAsync(orderId);
            if (order != null)
            {
                order.IsImported = isImported;
                await this._orderService.UpdateOrderAsync(order);
            }
        }

        public async Task MarkOrderAsDelivered(DeliverOrderRequestModel model)
        {

            var order = await this._orderService.GetOrderByIdAsync(model.OrderId);
            if (order != null && order.ShippingStatus != ShippingStatus.Delivered)
            {
                if (!(await _shipmentService.GetShipmentsByOrderIdAsync(model.OrderId)).Any())
                {
                    var shipment = new Shipment
                    {
                        OrderId = order.Id,
                        TrackingNumber = model.TrackingNumber,
                        TotalWeight = null,
                        AdminComment = string.Empty,
                        CreatedOnUtc = DateTime.UtcNow
                    };
                    var shipmentItems = new List<ShipmentItem>();

                    decimal? totalWeight = null;
                    var orderItems = await _orderService.GetOrderItemsAsync(order.Id, isShipEnabled: true);
                    foreach (var orderItem in orderItems)
                    {
                        var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

                        //ensure that this product can be shipped (have at least one item to ship)
                        var maxQtyToAdd = await _orderService.GetTotalNumberOfItemsCanBeAddedToShipmentAsync(orderItem);
                        if (maxQtyToAdd <= 0)
                            continue;

                        var qtyToAdd = orderItem.Quantity; //parse quantity


                        var warehouseId = product.WarehouseId;

                        //validate quantity
                        if (qtyToAdd <= 0)
                            continue;
                        if (qtyToAdd > maxQtyToAdd)
                            qtyToAdd = maxQtyToAdd;

                        //ok. we have at least one item. let's create a shipment (if it does not exist)

                        var orderItemTotalWeight = orderItem.ItemWeight * qtyToAdd;
                        if (orderItemTotalWeight.HasValue)
                        {
                            if (!totalWeight.HasValue)
                                totalWeight = 0;
                            totalWeight += orderItemTotalWeight.Value;
                        }

                        //create a shipment item
                        shipmentItems.Add(new ShipmentItem
                        {
                            OrderItemId = orderItem.Id,
                            Quantity = qtyToAdd,
                            WarehouseId = warehouseId
                        });
                    }
                    if (shipmentItems.Any())
                    {
                        shipment.TotalWeight = totalWeight;
                        shipment.ShippedDateUtc = model.DeliveredDate;
                        shipment.DeliveryDateUtc = model.DeliveredDate;
                        await _shipmentService.InsertShipmentAsync(shipment);

                        foreach (var shipmentItem in shipmentItems)
                        {
                            shipmentItem.ShipmentId = shipment.Id;
                            await _shipmentService.InsertShipmentItemAsync(shipmentItem);
                        }
                        await _orderService.InsertOrderNoteAsync(new OrderNote
                        {
                            OrderId = order.Id,
                            Note = "A shipment has been added",
                            DisplayToCustomer = false,
                            CreatedOnUtc = DateTime.UtcNow
                        });

                        order.ShippingStatusId = (int)ShippingStatus.Delivered;
                        order.OrderStatusId = (int)NopOrderStatus.Complete;
                        await _orderService.UpdateOrderAsync(order);

                    }
                }
            }
        }
        public async Task<ApiOrderDetailModel> PrepareApiCustomOrderDetailsModelAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var model = new ApiOrderDetailModel
            {
                Id = order.Id,
                CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(order.CreatedOnUtc, DateTimeKind.Utc),
                OrderStatus = await _localizationService.GetLocalizedEnumAsync(order.OrderStatus),
                CustomOrderNumber = order.CustomOrderNumber,
                ParentOrderID = 0,
                OrderGuid = order.OrderGuid,
                //shipping info
                ShippingStatus = await _localizationService.GetLocalizedEnumAsync(order.ShippingStatus),
                CustomerCurrencyCode = order.CustomerCurrencyCode,
                CreditCardNumber = string.IsNullOrEmpty(order.MaskedCreditCardNumber) ? "" : _encryptionService.DecryptText(order.MaskedCreditCardNumber),
                CustomerId = order.CustomerId,
                TransactionId = string.IsNullOrEmpty(order.AuthorizationTransactionId) ? order.CaptureTransactionId : order.AuthorizationTransactionId
            };


            if (_gdprSettings.GdprEnabled)
            {
                var consents = (await _gdprService.GetAllConsentsAsync()).Where(consent => consent.DisplayOnCustomerInfoPage).ToList();
                foreach (var consent in consents)
                {
                    var accepted = await _gdprService.IsConsentAcceptedAsync(consent.Id, order.CustomerId);
                    model.SmsOptionSelected = accepted != null ? Convert.ToBoolean(accepted) : false;
                }
            }
            if (order.ShippingStatus != ShippingStatus.ShippingNotRequired)
            {
                model.IsShippable = true;
                model.PickupInStore = order.PickupInStore;
                if (!order.PickupInStore)
                {
                    var shippingAddress = await _addressService.GetAddressByIdAsync(order.ShippingAddressId ?? 0);

                    await _addressModelFactory.PrepareAddressModelAsync(model.ShippingAddress,
                        address: shippingAddress,
                        excludeProperties: false,
                        addressSettings: _addressSettings);

                    #region Shipping State Abbreviation

                    if (shippingAddress?.StateProvinceId > 0)
                        if (model.ShippingAddress != null)
                            model.ShippingAddress.Abbreviation = (await _stateProvinceService.GetStateProvinceByIdAsync(Convert.ToInt32(shippingAddress.StateProvinceId)))?.Abbreviation;

                    #endregion
                }
                else if (order.PickupAddressId.HasValue && await _addressService.GetAddressByIdAsync(order.PickupAddressId.Value) is Address pickupAddress)
                {
                    model.PickupAddress = new AddressModel
                    {
                        Address1 = pickupAddress.Address1,
                        City = pickupAddress.City,
                        County = pickupAddress.County,
                        StateProvinceName = await _stateProvinceService.GetStateProvinceByAddressAsync(pickupAddress) is StateProvince stateProvince
                            ? await _localizationService.GetLocalizedAsync(stateProvince, entity => entity.Name)
                            : string.Empty,
                        CountryName = await _countryService.GetCountryByAddressAsync(pickupAddress) is Country country
                            ? await _localizationService.GetLocalizedAsync(country, entity => entity.Name)
                            : string.Empty,
                        ZipPostalCode = pickupAddress.ZipPostalCode
                    };
                }

                model.ShippingMethod = order.ShippingMethod;

                //shipments (only already shipped)
                var shipments = (await _shipmentService.GetShipmentsByOrderIdAsync(order.Id, true)).OrderBy(x => x.CreatedOnUtc).ToList();
                foreach (var shipment in shipments)
                {
                    var shipmentModel = new ApiOrderDetailModel.ApiShipmentBriefModel
                    {
                        Id = shipment.Id,
                        TrackingNumber = shipment.TrackingNumber,
                    };
                    if (shipment.ShippedDateUtc.HasValue)
                        shipmentModel.ShippedDate = await _dateTimeHelper.ConvertToUserTimeAsync(shipment.ShippedDateUtc.Value, DateTimeKind.Utc);
                    if (shipment.DeliveryDateUtc.HasValue)
                        shipmentModel.DeliveryDate = await _dateTimeHelper.ConvertToUserTimeAsync(shipment.DeliveryDateUtc.Value, DateTimeKind.Utc);
                    model.Shipments.Add(shipmentModel);
                }
            }

            var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);

            //billing info
            await _addressModelFactory.PrepareAddressModelAsync(model.BillingAddress,
                address: billingAddress,
                excludeProperties: false,
                addressSettings: _addressSettings);

            #region Billing State Abbreviation

            if (billingAddress?.StateProvinceId > 0)
                if (model.BillingAddress != null)
                    model.BillingAddress.Abbreviation = (await _stateProvinceService.GetStateProvinceByIdAsync(Convert.ToInt32(billingAddress.StateProvinceId)))?.Abbreviation;

            #endregion
            //VAT number
            model.VatNumber = order.VatNumber;

            var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;

            //payment method
            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
            model.Email = order.CustomerEmail;
            if (string.IsNullOrEmpty(model.Email))
                model.Email = model.BillingAddress?.Email;

            var paymentMethod = await _paymentPluginManager
                .LoadPluginBySystemNameAsync(order.PaymentMethodSystemName, customer, order.StoreId);
            model.PaymentMethod = paymentMethod != null ? await _localizationService.GetLocalizedFriendlyNameAsync(paymentMethod, languageId) : order.PaymentMethodSystemName;
            model.PaymentMethodStatus = await _localizationService.GetLocalizedEnumAsync(order.PaymentStatus);
            model.CanRePostProcessPayment = await _paymentService.CanRePostProcessPaymentAsync(order);

            var customValues = new CustomValues();
            customValues.FillByXml(order.CustomValuesXml, true);
            model.CustomValues = customValues;


            //order subtotal


            if (order.AdditonalShippingChargesInclTax > 0)
                model.AdditonalShippingCharges = (_currencyService.ConvertCurrency(order.AdditonalShippingChargesInclTax, order.CurrencyRate));
            if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
            {
                //including tax

                //order shipping
                var orderShippingInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingInclTax, order.CurrencyRate);
                model.OrderShipping = orderShippingInclTaxInCustomerCurrency;
                //payment method additional fee
                var paymentMethodAdditionalFeeInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeInclTax, order.CurrencyRate);
                if (paymentMethodAdditionalFeeInclTaxInCustomerCurrency > decimal.Zero)
                    model.PaymentMethodAdditionalFee = paymentMethodAdditionalFeeInclTaxInCustomerCurrency;
            }
            else
            {
                //excluding tax

                //order shipping
                var orderShippingExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingExclTax, order.CurrencyRate);
                model.OrderShipping = orderShippingExclTaxInCustomerCurrency;
                //payment method additional fee
                var paymentMethodAdditionalFeeExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeExclTax, order.CurrencyRate);
                if (paymentMethodAdditionalFeeExclTaxInCustomerCurrency > decimal.Zero)
                    model.PaymentMethodAdditionalFee = paymentMethodAdditionalFeeExclTaxInCustomerCurrency;
            }
            // customduty
            if (order.CustomDutyInclTax > 0)
            {
                model.CustomDutyPercentage = order.CustomDutyPercentage;
                model.CustomDuty = await _priceFormatter.FormatPriceAsync(_currencyService.ConvertCurrency(order.CustomDutyInclTax, order.CurrencyRate), true, order.CustomerCurrencyCode, false, languageId);
            }
            //tax
            var displayTax = true;
            var displayTaxRates = true;
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
                    var taxRates = _orderService.ParseTaxRates(order, order.TaxRates);
                    displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Any();
                    displayTax = !displayTaxRates;

                    var orderTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTax, order.CurrencyRate);
                    model.Tax = orderTaxInCustomerCurrency;

                    foreach (var tr in taxRates)
                    {
                        model.TaxRates.Add(new ApiOrderDetailModel.ApiTaxRate
                        {
                            Rate = tr.Key,
                            Value = _currencyService.ConvertCurrency(tr.Value, order.CurrencyRate),
                        });
                    }
                }
            }
            model.DisplayTaxRates = displayTaxRates;
            model.DisplayTax = displayTax;
            var taxes = _orderExtendedService.GetTaxDetails(order);
            if (taxes.Where(t => t.TaxType != TaxType.Tax && t.TaxRate > 0).Any())
            {
                foreach (var taxInfo in taxes.Where(t => t.TaxRate > 0))
                {
                    model.TaxInfo.Add(new TaxInfoModel()
                    {
                        Amount = taxInfo.Amount,
                        TaxType = taxInfo.TaxType.ToString(),
                        TaxRate = taxInfo.TaxRate
                    });
                }
            }
            model.DisplayTaxShippingInfo = _catalogSettings.DisplayTaxShippingInfoOrderDetailsPage;
            model.PricesIncludeTax = order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax;

            //discount (applied to order total)
            var orderDiscountInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderDiscount, order.CurrencyRate);
            if (orderDiscountInCustomerCurrency > decimal.Zero)
                model.OrderTotalDiscount = -orderDiscountInCustomerCurrency;

            //gift cards
            foreach (var gcuh in await _giftCardService.GetGiftCardUsageHistoryAsync(order))
            {
                model.GiftCards.Add(new ApiOrderDetailModel.ApiGiftCard
                {
                    CouponCode = (await _giftCardService.GetGiftCardByIdAsync(gcuh.GiftCardId)).GiftCardCouponCode,
                    Amount = -(_currencyService.ConvertCurrency(gcuh.UsedValue, order.CurrencyRate)),
                });
            }

            //reward points           
            if (order.RedeemedRewardPointsEntryId.HasValue && await _rewardPointService.GetRewardPointsHistoryEntryByIdAsync(order.RedeemedRewardPointsEntryId.Value) is RewardPointsHistory redeemedRewardPointsEntry)
            {
                model.RedeemedRewardPoints = -redeemedRewardPointsEntry.Points;
                model.RedeemedRewardPointsAmount = -(_currencyService.ConvertCurrency(redeemedRewardPointsEntry.UsedAmount, order.CurrencyRate));
            }

            //total
            var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
            model.OrderTotal = orderTotalInCustomerCurrency;

            //checkout attributes
            model.CheckoutAttributeInfo = order.CheckoutAttributeDescription;

            //order notes
            foreach (var orderNote in (await _orderService.GetOrderNotesByOrderIdAsync(order.Id, true))
                .OrderByDescending(on => on.CreatedOnUtc)
                .ToList())
            {
                model.OrderNotes.Add(new ApiOrderDetailModel.ApiOrderNote
                {
                    Id = orderNote.Id,
                    HasDownload = orderNote.DownloadId > 0,
                    Note = _orderService.FormatOrderNoteText(orderNote),
                    CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(orderNote.CreatedOnUtc, DateTimeKind.Utc)
                });
            }

            //purchased products
            model.ShowSku = _catalogSettings.ShowSkuOnProductDetailsPage;
            model.ShowVendorName = _vendorSettings.ShowVendorOnOrderDetailsPage;





            var orderItems = await _orderService.GetOrderItemsAsync(order.Id);


            #region Custom Discounts



            decimal customSubTotal = 0;
            foreach (var item in orderItems)
            {
                if (item.ItemPriceIncTax == 0)
                {
                    customSubTotal = 0;
                    break;
                }
                else
                    customSubTotal += item.ItemPriceIncTax * item.Quantity;
            }

            if (customSubTotal > 0)
            {
                customSubTotal = _currencyService.ConvertCurrency(customSubTotal, order.CurrencyRate);
                model.OrderSubtotal = customSubTotal;
                if (order.MembershipFeeInclTax > 0)
                    model.MembershipFee = _currencyService.ConvertCurrency(order.MembershipFeeInclTax, order.CurrencyRate);
                if (order.MembershipFeeDiscountInclTax != 0)
                    model.MembershipFeeDiscount = -_currencyService.ConvertCurrency(order.MembershipFeeDiscountInclTax, order.CurrencyRate);
                if (order.MembershipDiscountIncTax != 0)
                    model.MembershipDiscountIncTax = -_currencyService.ConvertCurrency(order.MembershipDiscountIncTax, order.CurrencyRate);
                if (order.OfferDiscountIncTax != 0)
                    model.OfferDiscountIncTax = -_currencyService.ConvertCurrency(order.OfferDiscountIncTax, order.CurrencyRate);
                if (order.BuyMoreSaveMoreDiscountIncTax != 0)
                    model.BuyMoreSaveMoreDiscountIncTax = -_currencyService.ConvertCurrency(order.BuyMoreSaveMoreDiscountIncTax, order.CurrencyRate);
            }

            #endregion


            decimal orderItemsDiscount = 0;
            decimal orderTotal = 0;


            foreach (var orderItem in orderItems)
            {
                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
                decimal itemTotal = 0;

                #region Specification Model



                var groups = await _specificationAttributeService.GetSpecificationAttributeGroupsAsync(0, int.MaxValue);
                var specifications = new List<CustomProductSpecificationModel>();

                var Attributes = await _customProductModelFactory.PrepareCustomProductSpecificationAttributeModelAsync(product, null);
                foreach (var attr in Attributes)
                {
                    if (attr.Name.Equals("DIMENSIONS", StringComparison.CurrentCultureIgnoreCase))
                        specifications.Add(new CustomProductSpecificationModel()
                        {
                            Name = attr.Name,
                            Value = attr.Values.FirstOrDefault()?.ValueRaw
                        });

                }
                foreach (var group in groups)
                {
                    if (group.Id == 4)
                    {
                        Attributes = await _customProductModelFactory.CustomfeedPrepareProductSpecificationAttributeModelAsync(product, group);
                        foreach (var attr in Attributes)
                        {
                            if (attr.Name != "Froggle_Description")
                                specifications.Add(new CustomProductSpecificationModel()
                                {
                                    Name = attr.Name,
                                    Value = attr.Values.FirstOrDefault()?.ValueRaw
                                });

                        }
                    }
                }
                #endregion

                var variant = await _productExtendedService.GetItemVariantInfo(orderItem.ProductId, orderItem.AttributesXml);
                var orderItemModel = new ApiOrderDetailModel.ApiOrderItemModel
                {
                    Id = orderItem.Id,
                    OrderItemGuid = orderItem.OrderItemGuid,
                    Sku = await _productService.FormatSkuAsync(product, orderItem.AttributesXml),
                    VendorName = (await _vendorService.GetVendorByIdAsync(product.VendorId))?.Name ?? string.Empty,
                    ProductId = product.Id,
                    Name = string.IsNullOrWhiteSpace(variant?.Title) ? await _localizationService.GetLocalizedAsync(product, x => x.Name) : variant.Title,
                    ProductSeName = await _urlRecordService.GetSeNameAsync(product),
                    Quantity = orderItem.Quantity,
                    AttributesDescription = orderItem.AttributeDescription,
                    AttributeXml = orderItem.AttributesXml,
                    Picture = (await PrepareCartItemPictureModelAsync(orderItem.AttributesXml, product, _mediaSettings.AssociatedProductPictureSize,
                    true, product.Name))?.ImageUrl,
                    Specifications = specifications,
                    VariantId = variant?.VariantId ?? 0,
                    DimensionImages = GetProductDimensionImages(orderItem.ProductId)
                };

                if (orderItemModel.VariantId > 0)
                {
                    orderItemModel.EstimatedDeliveryDate = (await _productExtendedService.GetProductVariants(orderItem.ProductId)).Where(v => v.VariantId == orderItemModel.VariantId).FirstOrDefault()?.EstimatedDeliveryDate ??
                        product.EstimatedDeliveryDate ?? string.Empty;
                }
                else
                {
                    orderItemModel.EstimatedDeliveryDate = product.EstimatedDeliveryDate ?? string.Empty;
                }
                if (orderItem.TotalDiscount > 0)
                {
                    itemTotal = -_currencyService.ConvertCurrency(orderItem.TotalDiscount < 0 ? -orderItem.TotalDiscount : orderItem.TotalDiscount, order.CurrencyRate);
                    orderItemModel.TotalDiscount = -(
                        _currencyService.ConvertCurrency(orderItem.TotalDiscount < 0 ? -orderItem.TotalDiscount : orderItem.TotalDiscount, order.CurrencyRate));
                    orderItemsDiscount += _currencyService.ConvertCurrency(orderItem.TotalDiscount, order.CurrencyRate);

                }



                //rental info
                if (product.IsRental)
                {
                    var rentalStartDate = orderItem.RentalStartDateUtc.HasValue
                        ? _productService.FormatRentalDate(product, orderItem.RentalStartDateUtc.Value) : "";
                    var rentalEndDate = orderItem.RentalEndDateUtc.HasValue
                        ? _productService.FormatRentalDate(product, orderItem.RentalEndDateUtc.Value) : "";
                    orderItemModel.RentalInfo = string.Format(await _localizationService.GetResourceAsync("Order.Rental.FormattedDate"),
                        rentalStartDate, rentalEndDate);
                }


                //unit price, subtotal
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var unitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceInclTax, order.CurrencyRate);
                    orderItemModel.UnitPrice = unitPriceInclTaxInCustomerCurrency;

                    var priceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.PriceInclTax, order.CurrencyRate);
                    orderItemModel.SubTotal = priceInclTaxInCustomerCurrency;
                    if (customSubTotal <= 0)
                        itemTotal += priceInclTaxInCustomerCurrency;

                }
                else
                {
                    //excluding tax
                    var unitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceExclTax, order.CurrencyRate);
                    orderItemModel.UnitPrice = unitPriceExclTaxInCustomerCurrency;

                    var priceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.PriceExclTax, order.CurrencyRate);
                    orderItemModel.SubTotal = priceExclTaxInCustomerCurrency;
                    if (customSubTotal <= 0)
                        itemTotal += priceExclTaxInCustomerCurrency;
                }
                #region Custom DiscountS

                if (customSubTotal > 0)
                {

                    orderItemModel.UnitPrice = _currencyService.ConvertCurrency(orderItem.ItemPriceIncTax, order.CurrencyRate);

                    orderItemModel.SubTotal = _currencyService.ConvertCurrency(orderItem.ItemPriceIncTax, order.CurrencyRate) * orderItem.Quantity;
                    itemTotal += _currencyService.ConvertCurrency(orderItem.ItemPriceIncTax, order.CurrencyRate) * orderItem.Quantity;
                    if (orderItem.MembershipDiscountIncTax != 0)
                    {
                        orderItemModel.MembershipDiscountIncTax = -(_currencyService.ConvertCurrency(orderItem.MembershipDiscountIncTax < 0 ? -orderItem.MembershipDiscountIncTax : orderItem.MembershipDiscountIncTax, order.CurrencyRate));

                        itemTotal += _currencyService.ConvertCurrency(orderItem.MembershipDiscountIncTax > 0 ? -orderItem.MembershipDiscountIncTax : orderItem.MembershipDiscountIncTax, order.CurrencyRate);
                    }
                    if (orderItem.OfferDiscountIncTax != 0)
                    {
                        orderItemModel.OfferDiscountIncTax = -_currencyService.ConvertCurrency(orderItem.OfferDiscountIncTax < 0 ?
                            -orderItem.OfferDiscountIncTax : orderItem.OfferDiscountIncTax, order.CurrencyRate);

                        itemTotal += _currencyService.ConvertCurrency(orderItem.OfferDiscountIncTax > 0 ? -orderItem.OfferDiscountIncTax : orderItem.OfferDiscountIncTax, order.CurrencyRate);
                    }
                    if (orderItem.BuyMoreSaveMoreDiscountIncTax != 0)
                    {
                        orderItemModel.BuyMoreSaveMoreDiscountIncTax = -(orderItem.BuyMoreSaveMoreDiscountIncTax < 0 ? -orderItem.BuyMoreSaveMoreDiscountIncTax : orderItem.BuyMoreSaveMoreDiscountIncTax);
                        itemTotal += _currencyService.ConvertCurrency(orderItem.BuyMoreSaveMoreDiscountIncTax > 0 ? -orderItem.BuyMoreSaveMoreDiscountIncTax : orderItem.BuyMoreSaveMoreDiscountIncTax, order.CurrencyRate);
                    }

                    orderItemModel.ItemTotal = itemTotal;


                }

                #endregion

                //downloadable products
                if (await _orderService.IsDownloadAllowedAsync(orderItem))
                    orderItemModel.DownloadId = product.DownloadId;
                if (await _orderService.IsLicenseDownloadAllowedAsync(orderItem))
                    orderItemModel.LicenseId = orderItem.LicenseDownloadId ?? 0;

                orderItemModel.Notes = orderItem.SpecialInstructions;


                var picture = (await _pictureExtendedService.CustomGetPicturesOfProducAsync(product.Id, 1)).FirstOrDefault();
                string imageUrl;
                (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.CategoryThumbPictureSize);
                orderItemModel.Picture = imageUrl;

                #region ThankS giving
                if (!model.ShippingAddress.StateProvinceName.Contains("alaska", StringComparison.InvariantCultureIgnoreCase)
                    && !model.ShippingAddress.StateProvinceName.Contains("hawaii", StringComparison.InvariantCultureIgnoreCase)
                    )
                {


                    var tags = await _productTagService.GetAllProductTagsByProductIdAsync(orderItem.ProductId);
                    var thankGivingTag = await _settingService.GetSettingByKeyAsync<string>("ThanksGiving.ProductTag.Name");
                    if (tags.Where(t => t.Name.Equals(thankGivingTag, StringComparison.InvariantCultureIgnoreCase)).Any())
                    {
                        orderItemModel.IsDeliveryGuaranteed = true;
                    }
                }
                #endregion
                model.Items.Add(orderItemModel);
            }
            if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal)
            {
                //including tax

                //order subtotal
                if (customSubTotal == 0)
                {
                    var orderSubtotalInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubtotalInclTax, order.CurrencyRate);
                    model.OrderSubtotal = orderSubtotalInclTaxInCustomerCurrency;
                }
                //discount (applied to order subtotal)
                var orderSubTotalDiscountInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubTotalDiscountInclTax, order.CurrencyRate);
                if (orderSubTotalDiscountInclTaxInCustomerCurrency > decimal.Zero || orderItemsDiscount > decimal.Zero)
                    model.OrderSubTotalDiscount = -
                        (orderSubTotalDiscountInclTaxInCustomerCurrency + orderItemsDiscount);
            }
            else
            {
                //excluding tax

                //order subtotal
                if (customSubTotal == 0)
                {
                    var orderSubtotalExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubtotalExclTax, order.CurrencyRate);
                    model.OrderSubtotal = orderSubtotalExclTaxInCustomerCurrency;
                }
                //discount (applied to order subtotal)
                var orderSubTotalDiscountExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubTotalDiscountExclTax, order.CurrencyRate);
                if (orderSubTotalDiscountExclTaxInCustomerCurrency > decimal.Zero || orderItemsDiscount > decimal.Zero)
                    model.OrderSubTotalDiscount = -
                        (orderSubTotalDiscountExclTaxInCustomerCurrency + orderItemsDiscount)
                        ;
            }

            return model;
        }

        #endregion

        #region Custom Order

        public async Task<CustomOrderApiDetailModel> PrepareApiCustomOrder_OrderDetailsModelAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var model = new CustomOrderApiDetailModel
            {
                Id = order.Id,
                CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(order.CreatedOnUtc, DateTimeKind.Utc),
                OrderStatus = await _localizationService.GetLocalizedEnumAsync(order.OrderStatus),
                CustomOrderNumber = order.CustomOrderNumber,
                OrderGuid = order.OrderGuid,
                ParentOrderID = order.ParentOrderID,
                //shipping info
                ShippingStatus = await _localizationService.GetLocalizedEnumAsync(order.ShippingStatus),
                CustomerCurrencyCode = order.CustomerCurrencyCode,
                CreditCardNumber = string.IsNullOrEmpty(order.MaskedCreditCardNumber) ? "" : _encryptionService.DecryptText(order.MaskedCreditCardNumber),
                CustomerId = order.CustomerId,
                TransactionId = string.IsNullOrEmpty(order.AuthorizationTransactionId) ? order.CaptureTransactionId : order.AuthorizationTransactionId,

            };
            if (order.ShippingStatus != ShippingStatus.ShippingNotRequired)
            {


                if (!order.PickupInStore)
                {
                    var shippingAddress = await _addressService.GetAddressByIdAsync(order.ShippingAddressId ?? 0);

                    await _addressModelFactory.PrepareAddressModelAsync(model.ShippingAddress,
                        address: shippingAddress,
                        excludeProperties: false,
                        addressSettings: _addressSettings);

                    #region Shipping State Abbreviation

                    if (shippingAddress?.StateProvinceId > 0)
                        if (model.ShippingAddress != null)
                            model.ShippingAddress.Abbreviation = (await _stateProvinceService.GetStateProvinceByIdAsync(Convert.ToInt32(shippingAddress.StateProvinceId)))?.Abbreviation;

                    #endregion
                }
                else if (order.PickupAddressId.HasValue && await _addressService.GetAddressByIdAsync(order.PickupAddressId.Value) is Address pickupAddress)
                {
                    model.PickupAddress = new AddressModel  
                    {
                        Address1 = pickupAddress.Address1,
                        City = pickupAddress.City,
                        County = pickupAddress.County,
                        StateProvinceName = await _stateProvinceService.GetStateProvinceByAddressAsync(pickupAddress) is StateProvince stateProvince
                            ? await _localizationService.GetLocalizedAsync(stateProvince, entity => entity.Name)
                            : string.Empty,
                        CountryName = await _countryService.GetCountryByAddressAsync(pickupAddress) is Country country
                            ? await _localizationService.GetLocalizedAsync(country, entity => entity.Name)
                            : string.Empty,
                        ZipPostalCode = pickupAddress.ZipPostalCode
                    };
                }

                model.ShippingMethod = order.ShippingMethod;

                //shipments (only already shipped)
                var shipments = (await _shipmentService.GetShipmentsByOrderIdAsync(order.Id, true)).OrderBy(x => x.CreatedOnUtc).ToList();
                foreach (var shipment in shipments)
                {
                    var shipmentModel = new ApiOrderDetailModel.ApiShipmentBriefModel
                    {
                        Id = shipment.Id,
                        TrackingNumber = shipment.TrackingNumber,
                    };
                    if (shipment.ShippedDateUtc.HasValue)
                        shipmentModel.ShippedDate = await _dateTimeHelper.ConvertToUserTimeAsync(shipment.ShippedDateUtc.Value, DateTimeKind.Utc);
                    if (shipment.DeliveryDateUtc.HasValue)
                        shipmentModel.DeliveryDate = await _dateTimeHelper.ConvertToUserTimeAsync(shipment.DeliveryDateUtc.Value, DateTimeKind.Utc);
                    model.Shipments.Add(shipmentModel);
                }
            }

            var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);

            //billing info
            await _addressModelFactory.PrepareAddressModelAsync(model.BillingAddress,
                address: billingAddress,
                excludeProperties: false,
                addressSettings: _addressSettings);
            #region Billing State Abbreviation

            if (billingAddress?.StateProvinceId > 0)
                if (model.BillingAddress != null)
                    model.BillingAddress.Abbreviation = (await _stateProvinceService.GetStateProvinceByIdAsync(Convert.ToInt32(billingAddress.StateProvinceId)))?.Abbreviation;

            #endregion
            //VAT number
            model.VatNumber = order.VatNumber;

            var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;

            //payment method
            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
            model.Email = customer?.Email;
            if (string.IsNullOrEmpty(model.Email))
                model.Email = model.BillingAddress?.Email;

            var paymentMethod = await _paymentPluginManager
                .LoadPluginBySystemNameAsync(order.PaymentMethodSystemName, customer, order.StoreId);
            model.PaymentMethod = paymentMethod != null ? await _localizationService.GetLocalizedFriendlyNameAsync(paymentMethod, languageId) : order.PaymentMethodSystemName;
            model.PaymentMethodStatus = await _localizationService.GetLocalizedEnumAsync(order.PaymentStatus);
            model.CanRePostProcessPayment = await _paymentService.CanRePostProcessPaymentAsync(order);
            //custom values
            var customValues = new CustomValues();
            customValues.FillByXml(order.CustomValuesXml, true);
            model.CustomValues = customValues;

            //order subtotal



            //tax
            var displayTax = true;
            var displayTaxRates = true;
            if (_taxSettings.HideTaxInOrderSummary && order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
            {
                displayTax = false;
                displayTaxRates = false;
            }
            else
            {
                var taxRates = _orderService.ParseTaxRates(order, order.TaxRates);
                displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Any();
                displayTax = !displayTaxRates;

                var orderTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTax, order.CurrencyRate);


                foreach (var tr in taxRates)
                {
                    model.TaxRates.Add(new ApiOrderDetailModel.ApiTaxRate
                    {
                        Rate = tr.Key,
                        Value = _currencyService.ConvertCurrency(tr.Value, order.CurrencyRate),
                    });
                }

            }
            //checkout attributes
            model.CheckoutAttributeInfo = order.CheckoutAttributeDescription;





            model.IsCustomorder = true;

            #region Custom Discounts


            #endregion


            var customOrder = (await _customOrderService.GetByOrderNumber(order.Id));
            model.TaxExempted = customOrder == null ? false : !customOrder.ApplyTax;
            model.PairedOrderIds = customOrder?.PairedOrderIds ?? string.Empty;
            if (customOrder != null)
            {
                model.PurchaseOrderNumber = customOrder.PurchaseOrderNumber;
                model.CustomOrderSummaryModel = await _customOrderModelFactory.PrepareApiOderSummaryModel(customOrder.Id);
                model.PromiseDate = customOrder.PromiseDayDate;
                var orderTypes = await _customOrderService.GetOrderTypes();
                string OrderType = (orderTypes.Where(o => o.Id == customOrder.OrderTypeId)).FirstOrDefault()?.Name;
                model.OrderType = OrderType;
                model.SubOrderType = customOrder.SubOrderTypeId == 0 ? "" : (orderTypes.Where(o => o.Id == customOrder.SubOrderTypeId)).FirstOrDefault()?.Name;

                model.Items = (await PrepareCartModel(customOrder.Id))?.Items;

            }


            return model;
        }


        public async Task<dynamic> ApiGetStatusOfAdditionalWgsService(int orderNumber)
        {

            CustomOrder customOrder = await _customOrderService.GetByOrderNumber(orderNumber);
            if (customOrder != null || (await _orderService.GetOrderByIdAsync(orderNumber)) != null)
            {
                CustomOrder additionalServiceOrder = await _customOrderService.GetByParentLiveOrderNumber(orderNumber);
                if (additionalServiceOrder != null)
                {

                    var orderStatuses = await _customOrderService.GetOrderStatuses();
                    if (orderStatuses.Where(m => m.Id == additionalServiceOrder.StatusId).FirstOrDefault()?.Name == MWT.Nop.Core.Domain.CustomOrders.OrderStatus.Paid.ToString())
                    {
                        string serviceName = string.Empty;

                        var itemProductId = (await _customOrderService.GetOrderItems(additionalServiceOrder.Id)).FirstOrDefault()?.ProductId ?? 0;
                        if (itemProductId != 0)
                        {
                            serviceName = (await _productService.GetProductByIdAsync(itemProductId))?.Name ?? string.Empty;
                        }
                        dynamic obj = new ExpandoObject();
                        obj.StatusCode = 200;
                        obj.Status = true;
                        obj.OrderStatus = MWT.Nop.Core.Domain.CustomOrders.OrderStatus.Paid.ToString();
                        obj.Comments = "";
                        obj.OrderNumber = additionalServiceOrder.ParentOrderID;
                        obj.Total = additionalServiceOrder.OrderTotal;
                        obj.ServiceName = serviceName;
                        return obj;
                    }
                    else
                    {
                        dynamic obj = new ExpandoObject();
                        obj.StatusCode = 200;
                        obj.Status = true;
                        obj.OrderStatus = additionalServiceOrder.NotInterested ? "Not interested" : orderStatuses.Where(m => m.Id == additionalServiceOrder.StatusId).FirstOrDefault()?.Name;
                        obj.Comments = "";
                        return obj;
                    }
                }
                else
                {

                    dynamic obj = new ExpandoObject();
                    obj.StatusCode = 404;
                    obj.Status = false;
                    obj.Comments = "Order not Found";
                    return obj;
                }
            }
            else
            {
                dynamic obj = new ExpandoObject();
                obj.StatusCode = 404;
                obj.Status = false;
                obj.Comments = "Order not Found";
                return obj;
            }
        }
        public async Task<dynamic> ApiSendAdditionalWgsServiceInvoice(int orderNumber, string pairedOrderIds)
        {
            CustomOrder customOrder = await _customOrderService.GetByOrderNumber(orderNumber);

            if (customOrder != null || (await _orderService.GetOrderByIdAsync(orderNumber)) != null)
            {
                CustomOrder additionalServiceOrder = await _customOrderService.GetByParentLiveOrderNumber(orderNumber);

                if (additionalServiceOrder != null)
                {
                    if (additionalServiceOrder.NotInterested)
                    {
                        dynamic obj = new ExpandoObject();
                        obj.StatusCode = 200;
                        obj.Status = false;
                        obj.PairedOrderIds = additionalServiceOrder.PairedOrderIds ?? string.Empty;
                        obj.Comments = "Customer Not Interested.";
                        return obj;
                    }
                    var orderStatuses = await _customOrderService.GetOrderStatuses();
                    var status = orderStatuses.Where(m => m.Id == additionalServiceOrder.StatusId).FirstOrDefault()?.Name;
                    if (status == MWT.Nop.Core.Domain.CustomOrders.OrderStatus.Paid.ToString())
                    {
                        dynamic obj = new ExpandoObject();
                        obj.StatusCode = 200;
                        obj.Status = false;
                        obj.PairedOrderIds = additionalServiceOrder.PairedOrderIds ?? string.Empty;
                        obj.Comments = "WGS Additional Service has already been purchased by the customer";
                        return obj;
                    }
                    else
                    {

                        await _customOrderModelFactory.AdditionalServiceSendInvoice(additionalServiceOrder, additionalServiceOrder.OrderTotal ?? 0, 0, additionalServiceOrder.OrderTotal ?? 0, 0);
                        dynamic obj = new ExpandoObject();
                        obj.StatusCode = 200;
                        obj.Status = true;
                        obj.PairedOrderIds = additionalServiceOrder.PairedOrderIds ?? string.Empty;
                        obj.Comments = "Invoice Sent";
                        return obj;
                    }

                }

                else
                {
                    bool isSurchargeApplicable = false;
                    string validOrderIds = string.Empty;
                    if (customOrder != null)
                    {
                        dynamic obj = new ExpandoObject();
                        decimal subTotal = customOrder.SubTotal == null ? 0 : Convert.ToDecimal(customOrder.SubTotal);
                        isSurchargeApplicable = await _customOrderService.IsSurchargeApplicable(customOrder);
                        if (!string.IsNullOrEmpty(pairedOrderIds))
                        {
                            int[] _pairedOrderIds = pairedOrderIds.Split(',').Where(s => int.TryParse(s, out int _out)).Select(s => int.Parse(s)).Where(s => s != orderNumber).Distinct().ToArray();

                            foreach (var orderId in _pairedOrderIds)
                            {
                                var order = await _orderService.GetOrderByIdAsync(orderId);
                                if (order != null)
                                {
                                    var _customOrder = await _customOrderService.GetByOrderNumber(orderId);
                                    if (!isSurchargeApplicable)
                                    {
                                        if (_customOrder != null)
                                        {
                                            isSurchargeApplicable = await _customOrderService.IsSurchargeApplicable(_customOrder);
                                        }
                                        else
                                        {
                                            isSurchargeApplicable = await _orderExtendedService.IsSurchargeApplicable(order);
                                        }
                                    }
                                    subTotal = subTotal + (_customOrder == null ? order.OrderSubtotalInclTax : (_customOrder.SubTotal == null ? 0 : Convert.ToDecimal(_customOrder.SubTotal)));
                                    validOrderIds = validOrderIds + orderId.ToString() + ",";
                                }
                            }
                        }
                        (decimal wgsCharges, decimal defaultWgsCharges, decimal surchargeAmount) = await _customOrderModelFactory.GetWgsCharges(customOrder, subTotal, isSurchargeApplicable);
                        if (wgsCharges <= 0)
                        {
                            obj.StatusCode = 200;
                            obj.Status = false;
                            obj.PairedOrderIds = pairedOrderIds ?? string.Empty;
                            obj.Comments = "Order is not eligible for WGS.";
                            return obj;
                        }
                        var customer = await _customerService.GetCustomerByIdAsync(Convert.ToInt32(customOrder.CustomerId));
                        var orderStatuses = await _customOrderService.GetOrderStatuses();
                        additionalServiceOrder = new CustomOrder()
                        {
                            ParentOrderID = customOrder.LiveOrderNumber ?? 0,
                            TotalDiscount = 0,
                            CreatedBy = 0,
                            SubTotal = wgsCharges,
                            PairedOrderIds = validOrderIds,
                            OrderTax = 0,
                            OrderTotal = wgsCharges,
                            PrivateOrderNotes = "Order created by Automation process.",
                            ApplyTax = false,
                            StatusId = orderStatuses.Where(s => s.Name == MWT.Nop.Core.Domain.CustomOrders.OrderStatus.InvoiceSent.ToString()).Any() ?
                          orderStatuses.Where(s => s.Name == MWT.Nop.Core.Domain.CustomOrders.OrderStatus.InvoiceSent.ToString()).FirstOrDefault().Id : 4,
                            CustomerId = customer.Id,
                            OrderTypeId = (int)OrderTypes.CustomOrder

                        };

                        if (customer.Email != null)
                            additionalServiceOrder.CustomerCCEmail = customer.Email;
                        await _customOrderService.InsertAsync(additionalServiceOrder);

                        CustomOrderShoppingCartItem item = new CustomOrderShoppingCartItem();
                        item.CustomerId = customOrder.CustomerId;
                        item.StoreId = 0;
                        item.ShoppingCartTypeId = 1;
                        item.CustomerEnteredPrice = 0;
                        item.Quantity = 1;
                        item.ProductId = await _settingService.GetSettingByKeyAsync<int>("Wgs.ProductId");
                        item.OrderId = additionalServiceOrder.Id;
                        await _customOrderService.InsertOrderItemAsync(item);

                        CustomOrderPriceAdjustment items = new CustomOrderPriceAdjustment();
                        items.ShoppingCartRecID = item.Id;
                        items.ShoppingCartProductPrice = wgsCharges;
                        items.Discountamount = 0;
                        items.Chargestype = "Subtract";
                        items.Discounttype = "Fixed";
                        items.OrderId = additionalServiceOrder.Id;
                        await _customOrderService.InsertPriceAdjustmentAsync(items);
                        await _customOrderModelFactory.AdditionalServiceSendInvoice(additionalServiceOrder, wgsCharges, 0, 0, 0, defaultWgsCharges, surchargeAmount);
                        obj = new ExpandoObject();
                        obj.StatusCode = 200;
                        obj.Status = true;
                        obj.Comments = "Invoice Sent";
                        return obj;
                    }
                    else
                    {
                        var order = await _orderService.GetOrderByIdAsync(orderNumber);
                        isSurchargeApplicable = await _orderExtendedService.IsSurchargeApplicable(order);
                        decimal subTotal = order.OrderSubtotalInclTax;
                        if (!string.IsNullOrEmpty(pairedOrderIds))
                        {
                            int[] _pairedOrderIds = pairedOrderIds.Split(',').Where(s => int.TryParse(s, out int _out)).Select(s => int.Parse(s)).Where(s => s != orderNumber).Distinct().ToArray();

                            foreach (var orderId in _pairedOrderIds)
                            {
                                var _order = await _orderService.GetOrderByIdAsync(orderId);
                                if (_order != null)
                                {
                                    var _customOrder = await _customOrderService.GetByOrderNumber(orderId);
                                    if (!isSurchargeApplicable)
                                    {
                                        if (_customOrder != null)
                                        {
                                            isSurchargeApplicable = await _customOrderService.IsSurchargeApplicable(_customOrder);
                                        }
                                        else
                                        {
                                            isSurchargeApplicable = await _orderExtendedService.IsSurchargeApplicable(order);
                                        }
                                    }
                                    subTotal = subTotal + (_customOrder == null ? _order.OrderSubtotalInclTax : (_customOrder.SubTotal == null ? 0 : Convert.ToDecimal(_customOrder.SubTotal)));
                                    validOrderIds = validOrderIds + orderId.ToString() + ",";
                                }
                            }
                        }

                        dynamic obj = new ExpandoObject();
                        (decimal wgsCharges, decimal defaultWgsCharges, decimal surchargeAmount) = await this._orderExtendedService.GetWgsCharges(order, subTotal, isSurchargeApplicable);
                        if (wgsCharges <= 0)
                        {
                            obj.StatusCode = 200;
                            obj.Status = false;
                            obj.Comments = "Order is not eligible for WGS.";
                            return obj;
                        }
                        var customer = await _customerService.GetCustomerByIdAsync(Convert.ToInt32(order.CustomerId));
                        var orderStatuses = await _customOrderService.GetOrderStatuses();
                        additionalServiceOrder = new CustomOrder()
                        {
                            ParentOrderID = order.Id,
                            TotalDiscount = 0,
                            SubTotal = wgsCharges,
                            OrderTax = 0,
                            CreatedBy = 0,
                            OrderTotal = wgsCharges,
                            PairedOrderIds = validOrderIds,
                            PrivateOrderNotes = "Order created by Automation process.",
                            ApplyTax = false,
                            StatusId = orderStatuses.Where(s => s.Name == MWT.Nop.Core.Domain.CustomOrders.OrderStatus.InvoiceSent.ToString()).Any() ?
                          orderStatuses.Where(s => s.Name == MWT.Nop.Core.Domain.CustomOrders.OrderStatus.InvoiceSent.ToString()).FirstOrDefault().Id : 4,
                            CustomerId = customer.Id,
                            OrderTypeId = (int)OrderTypes.CustomOrder

                        };

                        if (customer.Email != null)
                            additionalServiceOrder.CustomerCCEmail = customer.Email;
                        await _customOrderService.InsertAsync(additionalServiceOrder);

                        CustomOrderShoppingCartItem item = new CustomOrderShoppingCartItem();
                        item.CustomerId = order.CustomerId;
                        item.StoreId = 0;
                        item.ShoppingCartTypeId = 1;
                        item.CustomerEnteredPrice = 0;
                        item.Quantity = 1;
                        item.ProductId = await _settingService.GetSettingByKeyAsync<int>("Wgs.ProductId");
                        item.OrderId = additionalServiceOrder.Id;
                        await _customOrderService.InsertOrderItemAsync(item);

                        CustomOrderPriceAdjustment items = new CustomOrderPriceAdjustment();
                        items.ShoppingCartRecID = item.Id;
                        items.ShoppingCartProductPrice = wgsCharges;
                        items.Discountamount = 0;
                        items.Chargestype = "Subtract";
                        items.Discounttype = "Fixed";
                        items.OrderId = additionalServiceOrder.Id;
                        await _customOrderService.InsertPriceAdjustmentAsync(items);
                        await _customOrderModelFactory.AdditionalServiceSendInvoice(additionalServiceOrder, wgsCharges, 0, 0, 0, defaultWgsCharges, surchargeAmount);
                        obj = new ExpandoObject();
                        obj.StatusCode = 200;
                        obj.Status = true;
                        obj.Comments = "Invoice Sent";
                        return obj;
                    }
                }
            }
            else
            {
                dynamic obj = new ExpandoObject();
                obj.StatusCode = 404;
                obj.Status = false;
                obj.Comments = "Order not Found";
                return obj;
            }

        }




        public async Task<TrackOrderModel> GetRecentOrderOfCustomer()
        {
            var order = await _orderExtendedService.GetRecentOrderOfCustomer((await _workContext.GetCurrentCustomerAsync())?.Id ?? 0);
            return new TrackOrderModel()
            {
                Id = order.Id,
                Email = order.CustomerEmail
            };
        }

        public async Task<int> GetLatestDeliveredOrderIdByCustomerEmail(string email)
        {
            var orderIds = await _orderExtendedService.GetLatestDeliveredOrderIdByCustomerEmail(email);
            foreach (var orderId in orderIds)
            {
                var customOrder = await _customOrderService.GetByOrderNumber(orderId);
                if (customOrder == null) return orderId;
                if (customOrder.OrderTypeId == 1 && customOrder.ParentOrderID == 0)
                {
                    return orderId;
                }
                else
                {
                    continue;
                }
            }
            return 0;
        }


        public async Task<Dictionary<string, decimal>> GetShippingMethods(int variantId, decimal total, string zipcode, int countryId)
        {
            bool isSurchargeApplicable = false;
            var shippingDict = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (customer.Id != 0)
            {
                var storeId = (await this._storeContext.GetCurrentStoreAsync()).Id;
                var shippingRateComputationMethods = await _shippingPluginManager
                  .LoadActivePluginsAsync(customer, storeId, "");
                if (shippingRateComputationMethods.Any())
                {
                    var address = new Address
                    {
                        ZipPostalCode = zipcode,
                        CountryId = countryId
                    };

                    var variant = await _productExtendedService.GetVariantByVariantId(variantId);
                    if (variant == null)
                        return shippingDict;

                    var product = await _productService.GetProductByIdAsync(variant.ProductId);
                    if (product == null)
                        return shippingDict;

                    string attributeCombination = string.Empty;

                    var attrValueIds = new HashSet<int>(
                        (variant.ProductAttributeValueIds ?? string.Empty)
                            .Split('-')
                            .Select(s => int.TryParse(s, out int value) ? value : (int?)null)
                            .Where(v => v.HasValue)
                            .Select(v => v.Value)
                    );

                    var productAttributesCombinations = await _productAttributeService.CustomGetAllProductAttributeCombinationsAsync(variant.ProductId);

                    foreach (var _attrCombination in productAttributesCombinations)
                    {
                        var combinationAttributeValueIds = (await _productAttributeParser.ParseProductAttributeValuesAsync(_attrCombination.AttributesXml)).Select(x => x.Id).ToList();
                        if (new HashSet<int>(attrValueIds).IsSubsetOf(combinationAttributeValueIds))
                        {
                            attributeCombination = _attrCombination.AttributesXml;
                            if (variant.EnableSurcharge)
                            {
                                isSurchargeApplicable = true;
                            }
                            break;
                        }
                    }

                    await _shoppingCartService.AddToCartAsync(customer, product,
                       ShoppingCartType.ShoppingCart, storeId, attributeCombination);
                    var result = new GetShippingOptionResponse();

                    var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, storeId);
                    //create a package
                    var _shippingService = EngineContext.Current.Resolve<IShippingService>();
                    var (shippingOptionRequests, shippingFromMultipleLocations) = await _shippingService.CreateShippingOptionRequestsAsync(cart, address, storeId);
                    result.ShippingFromMultipleLocations = shippingFromMultipleLocations;
                    if (isSurchargeApplicable)
                    {
                        foreach (var shippingOptionRequest in shippingOptionRequests)
                        {
                            shippingOptionRequest.IsSurchargeApplicable = isSurchargeApplicable;
                        }
                    }
                    foreach (var srcm in shippingRateComputationMethods)
                    {
                        //request shipping options (separately for each package-request)
                        IList<ShippingOption> srcmShippingOptions = null;
                        foreach (var shippingOptionRequest in shippingOptionRequests)
                        {
                            shippingOptionRequest.UseCustomSubtotal = true;
                            shippingOptionRequest.CustomSubtotal = total;
                            var getShippingOptionResponse = await srcm.GetShippingOptionsAsync(shippingOptionRequest);

                            if (getShippingOptionResponse.Success)
                            {
                                //success
                                if (srcmShippingOptions == null)
                                {
                                    //first shipping option request
                                    srcmShippingOptions = getShippingOptionResponse.ShippingOptions;
                                }
                                else
                                {
                                    //get shipping options which already exist for prior requested packages for this scrm (i.e. common options)
                                    srcmShippingOptions = srcmShippingOptions
                                        .Where(existingso => getShippingOptionResponse.ShippingOptions.Any(newso => newso.Name == existingso.Name))
                                        .ToList();

                                    //and sum the rates
                                    foreach (var existingso in srcmShippingOptions)
                                    {
                                        existingso.Rate += getShippingOptionResponse
                                            .ShippingOptions
                                            .First(newso => newso.Name == existingso.Name)
                                            .Rate;
                                    }
                                }
                            }
                            else
                            {
                                //errors
                                foreach (var error in getShippingOptionResponse.Errors)
                                {

                                    result.AddError(error);
                                    await _logger.WarningAsync($"Shipping ({srcm.PluginDescriptor.FriendlyName}). {error}");
                                }
                                //clear the shipping options in this case
                                srcmShippingOptions = new List<ShippingOption>();
                                break;
                            }
                        }

                        //add this scrm's options to the result
                        if (srcmShippingOptions == null)
                            continue;
                        var _priceCalculationService = EngineContext.Current.Resolve<IPriceCalculationService>();
                        var _shoppingCartSettings = EngineContext.Current.Resolve<ShoppingCartSettings>();
                        foreach (var so in srcmShippingOptions)
                        {
                            //set system name if not set yet
                            if (string.IsNullOrEmpty(so.ShippingRateComputationMethodSystemName))
                                so.ShippingRateComputationMethodSystemName = srcm.PluginDescriptor.SystemName;

                            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                                so.Rate = await _priceCalculationService.RoundPriceAsync(so.Rate);
                            result.ShippingOptions.Add(so);
                        }
                    }


                    if (result.ShippingOptions.Any())
                    {
                        foreach (var option in result.ShippingOptions)
                        {
                            if (!shippingDict.ContainsKey(option.Name))
                            {
                                shippingDict.Add(option.Name, option.Rate);
                            }
                        }
                    }



                }
                var cartItems = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
                foreach (var cartItem in cartItems)
                {
                    await _shoppingCartService.DeleteShoppingCartItemAsync(cartItem);
                }
            }

            return shippingDict;
        }

        #endregion

        #endregion
        #region Utilities






        #region GetVariantId

        private async Task<int> GetVariantId(int productId, string attributes)
        {
            int variantId = await _productExtendedService.GetVariantIdFromAttributeDescription(productId, attributes);
            if (variantId == 0)
            {
                string size = "";

                if (!string.IsNullOrEmpty(attributes))
                {
                    string[] attrs = attributes.Split("<br /", StringSplitOptions.RemoveEmptyEntries);
                    foreach (var attr in attrs)
                    {
                        if (attr.Split(':').Length > 0)
                        {
                            if (attr.Split(':')[0].Equals("size", StringComparison.InvariantCultureIgnoreCase))
                            {
                                size = string.Join(':', attr.Split(":").Skip(1));
                                break;
                            }
                        }
                    }
                }

                var _productModelFactory = EngineContext.Current.Resolve<IProductModelFactory>();

                variantId = await _productExtendedService.GetProductVariantIdBySize(productId, System.Net.WebUtility.HtmlDecode(size));
            }
            return variantId;
        }

        #endregion


        #region Custom Order

        private async Task<CustomOrderShoppingCartItemModel> PrepareCartModel(int orderId)
        {

            CustomOrderShoppingCartItemModel model = new CustomOrderShoppingCartItemModel();
            decimal total = 0;
            var items = await _customOrderService.GetOrderItems(orderId);
            var order = await _customOrderService.GetById(orderId);
            model.isEditable = false;
            foreach (var _item in items)
            {
                var product = await _productService.GetProductByIdAsync(_item.ProductId);
                ItemModel itemModel = new ItemModel();
                itemModel.AttributesDescription = _item.AttributesDescription;
                itemModel.AttributesXml = _item.AttributesXml;
                itemModel.CreatedOnUtc = _item.CreatedOnUtc;
                itemModel.CustomAttributesDescription = _item.CustomAttributesDescription;
                itemModel.Id = _item.Id;
                itemModel.ProductId = _item.ProductId;
                itemModel.Notes = _item.Notes;
                itemModel.Quantity = _item.Quantity;
                itemModel.VariantId = await this.GetVariantId(_item.ProductId, _item.AttributesDescription);
                itemModel.DimensionImages = GetProductDimensionImages(_item.ProductId);
                string estimatedDimesionDate = string.Empty;
                if (itemModel.VariantId > 0)
                {
                    itemModel.EstimatedDeliveryDate = (await _productExtendedService.GetProductVariants(_item.ProductId)).Where(v => v.VariantId == itemModel.VariantId).FirstOrDefault()?.EstimatedDeliveryDate ?? product.EstimatedDeliveryDate ?? string.Empty;
                }
                else
                {
                    itemModel.EstimatedDeliveryDate = product.EstimatedDeliveryDate ?? string.Empty;
                }

                // adjustment
                try
                {
                    var _priceAdjustment = await _customOrderService.GetPriceAdjustmentsByCartId(_item.Id);
                    if (_priceAdjustment != null)
                    {
                        itemModel.Price = await _priceFormatter.FormatPriceAsync(
                            _priceAdjustment.ShoppingCartProductPrice == null ? 0 : Convert.ToDecimal(_priceAdjustment.ShoppingCartProductPrice));
                        itemModel.ItemTotal = await _priceFormatter.FormatPriceAsync(
                (_priceAdjustment.ShoppingCartProductPrice == null ? 0 : Convert.ToDecimal(_priceAdjustment.ShoppingCartProductPrice)) * _item.Quantity);

                        itemModel.ChargeType = _priceAdjustment.Chargestype;
                        itemModel.DiscountAmount = _priceAdjustment.Discountamount == null ? 0 : Convert.ToDecimal(_priceAdjustment.Discountamount);
                        itemModel.DiscountPercentage = _priceAdjustment.DiscountPercentage == null ? 0 : Convert.ToDecimal(_priceAdjustment.DiscountPercentage);
                        itemModel.DiscountType = _priceAdjustment.Discounttype;
                        decimal totalAdjustment = (itemModel.DiscountType == DiscountType.Percentage.ToString() && itemModel.DiscountPercentage != 0 ?
                             (_priceAdjustment.ShoppingCartProductPrice == null ? 0 : Convert.ToDecimal(_priceAdjustment.ShoppingCartProductPrice)
                             * Convert.ToDecimal(_priceAdjustment.DiscountPercentage)) / 100
                             : (itemModel.DiscountType == DiscountType.Fixed.ToString() && itemModel.DiscountAmount != 0
                             ? itemModel.DiscountAmount : 0)) * _item.Quantity;

                        itemModel.TotalAdjustment = (totalAdjustment < 0 ? "-" : "") + await _priceFormatter.FormatPriceAsync(totalAdjustment);
                        decimal itemTotal = 0;
                        if (_priceAdjustment.Chargestype == ChargeType.Subtract.ToString())
                            itemTotal = ((
            _priceAdjustment.ShoppingCartProductPrice == null ? 0 : Convert.ToDecimal(_priceAdjustment.ShoppingCartProductPrice)) * _item.Quantity)
            - totalAdjustment;
                        else if (_priceAdjustment.Chargestype == ChargeType.Add.ToString())
                            itemTotal = ((
         _priceAdjustment.ShoppingCartProductPrice == null ? 0 : Convert.ToDecimal(_priceAdjustment.ShoppingCartProductPrice)) * _item.Quantity)
         + totalAdjustment;

                        else
                            itemTotal = ((
      _priceAdjustment.ShoppingCartProductPrice == null ? 0 : Convert.ToDecimal(_priceAdjustment.ShoppingCartProductPrice)) * _item.Quantity);

                        itemModel.Total = (itemTotal < 0 ? "-" : "") + await _priceFormatter.FormatPriceAsync(itemTotal);
                        total += itemTotal;

                    }

                    // end

                    itemModel.Name = product.Name;
                    itemModel.Sku = product.Sku;


                    var picture = (await _pictureExtendedService.CustomGetPicturesOfProducAsync(product.Id, 1)).FirstOrDefault();
                    string imageUrl;
                    (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.CategoryThumbPictureSize);
                    itemModel.Picture = imageUrl;
                    itemModel.UpdatedOnUtc = _item.UpdatedOnUtc;

                    #region Specifications




                    #region Specification Model



                    var groups = await _specificationAttributeService.GetSpecificationAttributeGroupsAsync(0, int.MaxValue);
                    var specifications = new List<CustomProductSpecificationModel>();

                    var attributes = await _customProductModelFactory.PrepareCustomProductSpecificationAttributeModelAsync(product, null);
                    foreach (var attr in attributes)
                    {
                        if (attr.Name.Equals("DIMENSIONS", StringComparison.CurrentCultureIgnoreCase))
                            specifications.Add(new CustomProductSpecificationModel()
                            {
                                Name = attr.Name,
                                Value = attr.Values.FirstOrDefault()?.ValueRaw
                            });

                    }
                    foreach (var group in groups)
                    {
                        if (group.Id == 4)
                        {
                            attributes = await _customProductModelFactory.CustomfeedPrepareProductSpecificationAttributeModelAsync(product, group);
                            foreach (var attr in attributes)
                            {
                                if (attr.Name != "Froggle_Description")
                                    specifications.Add(new CustomProductSpecificationModel()
                                    {
                                        Name = attr.Name,
                                        Value = attr.Values.FirstOrDefault()?.ValueRaw
                                    });

                            }
                        }
                    }

                    itemModel.Specifications = specifications;
                    #endregion
                    #endregion
                    model.Items.Add(itemModel);
                }
                catch (Exception ex)
                {

                }
            }
            model.Total = await _priceFormatter.FormatPriceAsync(total);
            model.Id = orderId;

            return model;
        }



        #endregion
        #region Pdf
        protected virtual async Task PrintHeaderAsync(PdfSettings pdfSettingsByStore, Language lang, OrderDetailsExtendedModel order, Font font, Font titleFont, Document doc)
        {

            var _settingService = EngineContext.Current.Resolve<ISettingService>();
            var storeEmail = await _settingService.GetSettingByKeyAsync<string>("store.email");
            //header
            var headerTable = new PdfPTable(2)
            {
                RunDirection = GetDirection(lang)
            };
            headerTable.DefaultCell.Border = Rectangle.NO_BORDER;

            //store info

            var cellHeader = new PdfPCell(new Phrase(" ")) { Border = Rectangle.NO_BORDER };
            cellHeader.BackgroundColor = new BaseColor(246, 246, 246);

            titleFont.Size = 20;
            cellHeader.PaddingLeft = 5;
            cellHeader.PaddingTop = 20;
            cellHeader.PaddingBottom = 20;
            cellHeader.Phrase.Add(new Phrase(await _localizationService.GetResourceAsync("Order.ThankYou.Message"), titleFont));

            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(new Phrase(await _localizationService.GetResourceAsync("Order.Order.No")));
            font.SetStyle(Font.BOLD);

            cellHeader.Phrase.Add(new Phrase(order.CustomOrderNumber.ToString(), font));
            font.SetStyle(Font.NORMAL);
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(new Phrase(StripHTML(string.Format(await _localizationService.GetResourceAsync("Order.Email.Confirmation.Message"), order.Email))));


            cellHeader.HorizontalAlignment = Element.ALIGN_LEFT;
            cellHeader.Border = Rectangle.NO_BORDER;

            headerTable.AddCell(cellHeader);

            headerTable.SetWidths(lang.Rtl ? new[] { 0.3f, 0.7f } : new[] { 0.7f, 0.3f });
            headerTable.WidthPercentage = 100f;

            //logo               


            var anchorPhone = new Anchor(string.Format(string.Format(await _localizationService.GetResourceAsync("common.callus"), (await _storeContext.GetCurrentStoreAsync())?.CompanyPhoneNumber)), font)
            {
                Reference = "tel:" + (await _storeContext.GetCurrentStoreAsync())?.CompanyPhoneNumber
            };

            var anchorEmail = new Anchor(await _localizationService.GetResourceAsync("Common.Email.Us"), font)
            {
                Reference = "mailto:" + storeEmail
            };
            var cellHelp = new PdfPCell(new Phrase(" ")) { Border = Rectangle.NO_BORDER };
            cellHelp.BackgroundColor = new BaseColor(246, 246, 246);
            cellHelp.PaddingRight = 5;
            cellHelp.PaddingTop = 20;
            cellHelp.PaddingBottom = 20;
            cellHelp.Phrase.Add(new Phrase(await _localizationService.GetResourceAsync("Order.help.Label")));
            cellHelp.Phrase.Add(new Phrase(Environment.NewLine));

            cellHelp.Phrase.Add(anchorPhone);
            cellHelp.Phrase.Add(new Chunk(" or "));
            cellHelp.Phrase.Add(anchorEmail);
            cellHelp.HorizontalAlignment = Element.ALIGN_RIGHT;
            headerTable.AddCell(cellHelp);


            doc.Add(headerTable);
        }
        public string StripHTML(string input)
        {
            return Regex.Replace(input, "<.*?>", String.Empty);
        }
        protected virtual PdfPCell GetPdfCell(object text, Font font)
        {
            return new PdfPCell(new Phrase(text.ToString(), font));
        }
        protected virtual int GetDirection(Language lang)
        {
            return lang.Rtl ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR;
        }
        protected virtual Font GetFont(Language language, PdfSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            var fontName = language?.Rtl == true
                ? !string.IsNullOrEmpty(settings.RtlFontName) ? settings.RtlFontName : NopCommonDefaults.PdfRtlFontName
                : !string.IsNullOrEmpty(settings.LtrFontName) ? settings.LtrFontName : NopCommonDefaults.PdfLtrFontName;

            var fontSize = settings.BaseFontSize >= 0 ? settings.BaseFontSize : 10;

            return PdfDocumentHelper.GetFont(fontName, fontSize);
        }

        /// <summary>
        /// Get font
        /// </summary>
        /// <param name="fontFileName">Font file name</param>
        /// <returns>Font</returns>


        #endregion

        #region DimensionImages

        public List<string> GetProductDimensionImages(int productId)
        {
            List<string> images = new List<string>();
            var _fileProvider = EngineContext.Current.Resolve<INopFileProvider>();
            string imagesPath = Path.Combine(_fileProvider.MapPath("/wwwroot/images/product/dimensionimages"));

            DirectoryInfo info = new DirectoryInfo(imagesPath);
            FileInfo[] files = info.GetFiles().Where(p => p.Name.StartsWith(productId + "_")).OrderByDescending(p => p.CreationTime).ToArray();
            foreach (FileInfo file in files)
            {
                images.Add(_storeContext.GetCurrentStore().Url + "images/product/dimensionimages/" + file.Name);
            }
            return images;
        }

        #endregion
        protected async Task<PictureModel> PrepareCartItemPictureModelAsync(string attributesXml, Product product, int pictureSize, bool showDefaultPicture, string productName)
        {
            var _pictureService = EngineContext.Current.Resolve<IPictureService>();
            //shopping cart item picture
            var sciPicture = await _pictureService.GetProductPictureAsync(product, attributesXml);

            return new PictureModel
            {
                ImageUrl = (await _pictureService.GetPictureUrlAsync(sciPicture, pictureSize, showDefaultPicture)).Url,
                Title = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat"), productName),
                AlternateText = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat"), productName),
            };
        }

        #endregion

        #region Custom Order Conversion

        public async Task<CustomOrderConversionModel> PrepareCustomOrderConversionModel(Order order)
        {
            CustomOrderConversionModel model = new CustomOrderConversionModel();
            var _customOrderService = EngineContext.Current.Resolve<ICustomOrderService>();
            var address = await _addressService.GetAddressByIdAsync(order.ShippingAddressId ?? 0);
            if (address != null)
            {
                await _addressExtendedModelFactory.PrepareCustomAddressModelAsync(model.ShippingAddress,
                address: address,
                excludeProperties: false,
                addressSettings: _addressSettings);
            }

            return model;
        }

        #endregion
    }
}
