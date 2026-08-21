    using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using MWT.Nop.Core.Data.Discounts;
using MWT.Nop.Core.Domain.Orders;
using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Services.Catalog;
using MWT.Nop.Core.Services.Customers;
using MWT.Nop.Core.Services.Media;
using MWT.Nop.Core.Services.Orders;
using MWT.Plugin.Misc.MwtStorefront.Models.ShoppingCart;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Core.Http.Extensions;
using Nop.Core.Infrastructure;
using Nop.Services.Attributes;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Common;
using Nop.Web.Models.Media;
using Nop.Web.Models.ShoppingCart;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DiscountType = MWT.Nop.Core.Domain.Orders.DiscountType;

namespace MWT.Plugin.Misc.MwtStorefront.Factories;

public partial class ShoppingCartExtendedModelFactory : ShoppingCartModelFactory, IShoppingCartExtendedModelFactory
{
    #region Fields
    private readonly ISettingService _settingService;
    private readonly IShoppingCartExtendedService _shoppingCartExtendedCartService;
    private readonly IProductExtendedService _productExtendedService;
    private readonly ICustomerExtendedService _customerExtendedService;
    private readonly ICustomProductAttributeFormatter _customProductAttributeFormatter;
    private readonly IPriceCalculationExtendedService _priceCalculationService;
    private readonly IOrderTotalCalculationExtendedService _orderTotalCalculationExtendedService;
    private readonly IPictureExtendedService _pictureExtendedService;
    private readonly IProductAttributeParser _productAttributeParser;



    #endregion



    #region Ctor
    public ShoppingCartExtendedModelFactory(AddressSettings addressSettings, CaptchaSettings captchaSettings,
        CatalogSettings catalogSettings, CommonSettings commonSettings, CustomerSettings customerSettings,
        IAddressModelFactory addressModelFactory, IAddressService addressService,
        IAttributeParser<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeParser,
        IAttributeService<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeService,
        ICheckoutAttributeFormatter checkoutAttributeFormatter, ICountryService countryService,
        ICurrencyService currencyService, ICustomerService customerService, ICustomWishlistService customWishlistService,
        IDateTimeHelper dateTimeHelper, IDiscountService discountService, IDownloadService downloadService,
        IGenericAttributeService genericAttributeService, IGiftCardService giftCardService, IHttpContextAccessor httpContextAccessor,
        ILocalizationService localizationService, IOrderProcessingService orderProcessingService,
        IOrderTotalCalculationService orderTotalCalculationService, IPaymentPluginManager paymentPluginManager,
        IPaymentService paymentService, IPermissionService permissionService, IPictureService pictureService,
        IPriceFormatter priceFormatter, IProductAttributeFormatter productAttributeFormatter, IProductService productService,
        IShippingService shippingService, IShoppingCartService shoppingCartService, IShortTermCacheManager shortTermCacheManager,
        IStateProvinceService stateProvinceService, IStaticCacheManager staticCacheManager, IStoreContext storeContext,
        IStoreMappingService storeMappingService, ITaxService taxService, IUrlRecordService urlRecordService, IVendorService vendorService,
        IWebHelper webHelper, IWorkContext workContext, MediaSettings mediaSettings, OrderSettings orderSettings,
        RewardPointsSettings rewardPointsSettings, ShippingSettings shippingSettings, ShoppingCartSettings shoppingCartSettings,
        TaxSettings taxSettings, VendorSettings vendorSettings, ISettingService settingService,
        IShoppingCartExtendedService shoppingCartExtendedCartService, IProductExtendedService productExtendedService,
        ICustomerExtendedService customerExtendedService, ICustomProductAttributeFormatter customProductAttributeFormatter,
        IPriceCalculationExtendedService priceCalculationService, IOrderTotalCalculationExtendedService orderTotalCalculationExtendedService,
        IPictureExtendedService pictureExtendedService, IProductAttributeParser productAttributeParser) :
        base(addressSettings, captchaSettings, catalogSettings, commonSettings, customerSettings, addressModelFactory,
            addressService, checkoutAttributeParser, checkoutAttributeService, checkoutAttributeFormatter, countryService,
            currencyService, customerService, customWishlistService, dateTimeHelper, discountService, downloadService,
            genericAttributeService, giftCardService, httpContextAccessor, localizationService, orderProcessingService,
            orderTotalCalculationService, paymentPluginManager, paymentService, permissionService, pictureService,
            priceFormatter, productAttributeFormatter, productService, shippingService, shoppingCartService,
            shortTermCacheManager, stateProvinceService, staticCacheManager, storeContext, storeMappingService,
            taxService, urlRecordService, vendorService, webHelper, workContext, mediaSettings, orderSettings,
            rewardPointsSettings, shippingSettings, shoppingCartSettings, taxSettings, vendorSettings)
    {
        _settingService = settingService;
        _shoppingCartExtendedCartService = shoppingCartExtendedCartService;
        _productExtendedService = productExtendedService;
        _customerExtendedService = customerExtendedService;
        _customProductAttributeFormatter = customProductAttributeFormatter;
        _priceCalculationService = priceCalculationService;
        _orderTotalCalculationExtendedService = orderTotalCalculationExtendedService;
        _pictureExtendedService = pictureExtendedService;
        _productAttributeParser = productAttributeParser;
    }
    #endregion




    #region     Methods
    public virtual async Task<ShoppingCartModel> PrepareCustomShoppingCartModelAsync(ShoppingCartModel model,
         IList<ShoppingCartItem> cart, bool isEditable = true,
         bool validateCheckoutAttributes = false,
         bool prepareAndDisplayOrderReviewData = false)
    {
        ArgumentNullException.ThrowIfNull(cart);

        ArgumentNullException.ThrowIfNull(model);

        //simple properties
        model.OnePageCheckoutEnabled = _orderSettings.OnePageCheckoutEnabled;

        if (!cart.Any())
            return model;

        model.IsEditable = isEditable;
        model.ShowProductImages = _shoppingCartSettings.ShowProductImagesOnShoppingCart;
        model.ShowSku = _catalogSettings.ShowSkuOnProductDetailsPage;
        model.ShowVendorName = _vendorSettings.ShowVendorOnOrderDetailsPage;
        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();
        var checkoutAttributesXml = await _genericAttributeService.GetAttributeAsync<string>(customer,
            NopCustomerDefaults.CheckoutAttributes, store.Id);
        var minOrderSubtotalAmountOk = await _orderProcessingService.ValidateMinOrderSubtotalAmountAsync(cart);
        if (!minOrderSubtotalAmountOk)
        {
            var minOrderSubtotalAmount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(_orderSettings.MinOrderSubtotalAmount, await _workContext.GetWorkingCurrencyAsync());
            model.MinOrderSubtotalWarning = string.Format(await _localizationService.GetResourceAsync("Checkout.MinOrderSubtotalAmount"), await _priceFormatter.FormatPriceAsync(minOrderSubtotalAmount, true, false));
        }

        model.TermsOfServiceOnShoppingCartPage = _orderSettings.TermsOfServiceOnShoppingCartPage;
        model.TermsOfServiceOnOrderConfirmPage = _orderSettings.TermsOfServiceOnOrderConfirmPage;
        model.TermsOfServicePopup = _commonSettings.PopupForTermsOfServiceLinks;
        model.DisplayTaxShippingInfo = _catalogSettings.DisplayTaxShippingInfoShoppingCart;

        //discount and gift card boxes
        model.DiscountBox.Display = _shoppingCartSettings.ShowDiscountBox;
        var discountCouponCodes = await _customerService.ParseAppliedDiscountCouponCodesAsync(customer);
        foreach (var couponCode in discountCouponCodes)
        {
            var discount = await (await _discountService.GetAllDiscountsAsync(couponCode: couponCode))
               .FirstOrDefaultAwaitAsync(async d => d.RequiresCouponCode && (await _discountService.ValidateDiscountAsync(d, customer, discountCouponCodes)).IsValid);

            if (discount != null)
            {
                model.DiscountBox.AppliedDiscountsWithCodes.Add(new ShoppingCartModel.DiscountBoxModel.DiscountInfoModel
                {
                    Id = discount.Id,
                    CouponCode = discount.CouponCode
                });
            }
        }

        model.GiftCardBox.Display = _shoppingCartSettings.ShowGiftCardBox;

        //cart warnings
        var cartWarnings = await _shoppingCartService.GetShoppingCartWarningsAsync(cart, checkoutAttributesXml, validateCheckoutAttributes);
        foreach (var warning in cartWarnings)
            model.Warnings.Add(warning);

        //checkout attributes
        model.CheckoutAttributes = await PrepareCheckoutAttributeModelsAsync(cart);

        var subTotalIncludingTax = await _workContext.GetTaxDisplayTypeAsync() == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal;
        var (orderSubTotalDiscountAmountBase, _, subTotal, _, _) = await _orderTotalCalculationService.GetShoppingCartSubTotalAsync(cart, subTotalIncludingTax);

        (CustomDiscountType discountType, decimal buyMoreDiscount, int notElgibleForSavemoreDiscountCartId) = await _shoppingCartExtendedCartService.GetBuyMoreSaveMoreDiscountDetailsAsync(cart, subTotal);

        // end
        var isCustomerElgibleForMembershipPrice = await _customerExtendedService.IsCustomerEligibleForMemberShipDiscount(customer);

        decimal memberShipDiscount = 0;

        //cart items
        foreach (var sci in cart)
        {
            (var cartItemModel, decimal _memberShipDiscount) = await PrepareCustomShoppingCartItemModelAsync(cart, sci,
                           sci.Id == notElgibleForSavemoreDiscountCartId ? 0 : buyMoreDiscount,
                           discountType, isCustomerElgibleForMembershipPrice);

            cartItemModel.Variant = await _productExtendedService.GetItemVariantInfo(sci.ProductId, sci.AttributesXml);
            cartItemModel.ProductName = string.IsNullOrWhiteSpace(cartItemModel.Variant?.Title) ? cartItemModel.ProductName : cartItemModel.Variant.Title;
            model.Items.Add(cartItemModel);
            memberShipDiscount += _memberShipDiscount;
        }

        //payment methods
        //all payment methods (do not filter by country here as it could be not specified yet)
        var paymentMethods = await (await _paymentPluginManager
            .LoadActivePluginsAsync(customer, store.Id))
            .WhereAwait(async pm => !await pm.HidePaymentMethodAsync(cart)).ToListAsync();
        //payment methods displayed during checkout (not with "Button" type)
        var nonButtonPaymentMethods = paymentMethods
            .Where(pm => pm.PaymentMethodType != PaymentMethodType.Button)
            .ToList();
        //"button" payment methods(*displayed on the shopping cart page)
        var buttonPaymentMethods = paymentMethods
            .Where(pm => pm.PaymentMethodType == PaymentMethodType.Button)
            .ToList();
        foreach (var pm in buttonPaymentMethods)
        {
            if (await _shoppingCartService.ShoppingCartIsRecurringAsync(cart) && pm.RecurringPaymentType == RecurringPaymentType.NotSupported)
                continue;

            var viewComponentName = pm.GetPublicViewComponent();
            model.ButtonPaymentMethodViewComponents.Add(viewComponentName);
        }
        //hide "Checkout" button if we have only "Button" payment methods
        model.HideCheckoutButton = !nonButtonPaymentMethods.Any() && model.ButtonPaymentMethodViewComponents.Any();

        //order review data
        if (prepareAndDisplayOrderReviewData)
        {
            model.OrderReviewData = await PrepareOrderReviewDataModelAsync(cart);
        }
        model.MemberShipDiscountValue = memberShipDiscount;
        model.MemberShipDiscount = "-" + await _priceFormatter.FormatPriceAsync(memberShipDiscount);
        return model;
    }

    protected virtual async Task<(ShoppingCartModel.ShoppingCartItemModel, decimal)> PrepareCustomShoppingCartItemModelAsync(IList<ShoppingCartItem> cart, ShoppingCartItem sci, decimal buyMoreSaveMoreDiscount, CustomDiscountType discountType, bool isCustomerElgibleForMembershipPrice)
    {
        ArgumentNullException.ThrowIfNull(cart);

        ArgumentNullException.ThrowIfNull(sci);

        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();
        var workingCurrency = await _workContext.GetWorkingCurrencyAsync();
        decimal memberShipDiscount = 0;
        decimal buyMoreSaveMoreDiscountBase = 0;


        var product = await _productService.GetProductByIdAsync(sci.ProductId);

        var cartItemModel = new ShoppingCartModel.ShoppingCartItemModel
        {
            Id = sci.Id,
            Sku = await _productService.FormatSkuAsync(product, sci.AttributesXml),
            VendorName = _vendorSettings.ShowVendorOnOrderDetailsPage ? (await _vendorService.GetVendorByProductIdAsync(product.Id))?.Name : string.Empty,
            ProductId = sci.ProductId,
            ProductName = await _localizationService.GetLocalizedAsync(product, x => x.Name),
            ProductSeName = await _urlRecordService.GetSeNameAsync(product),
            Quantity = sci.Quantity,
            AttributeInfo = await _customProductAttributeFormatter.CustomFormatAttributesAsync(product, sci.AttributesXml),
        };

        //allow editing?
        //1. setting enabled?
        //2. simple product?
        //3. has attribute or gift card?
        //4. visible individually?
        cartItemModel.AllowItemEditing = _shoppingCartSettings.AllowCartItemEditing &&
                                         product.ProductType == ProductType.SimpleProduct &&
                                         (!string.IsNullOrEmpty(cartItemModel.AttributeInfo) ||
                                          product.IsGiftCard) &&
                                         product.VisibleIndividually;

        //disable removal?
        //1. do other items require this one?
        cartItemModel.DisableRemoval = (await _shoppingCartService.GetProductsRequiringProductAsync(cart, product)).Any();

        //allowed quantities
        var allowedQuantities = _productService.ParseAllowedQuantities(product);
        foreach (var qty in allowedQuantities)
        {
            cartItemModel.AllowedQuantities.Add(new SelectListItem
            {
                Text = qty.ToString(),
                Value = qty.ToString(),
                Selected = sci.Quantity == qty
            });
        }

        //recurring info
        if (product.IsRecurring)
            cartItemModel.RecurringInfo = string.Format(await _localizationService.GetResourceAsync("ShoppingCart.RecurringPeriod"),
                    product.RecurringCycleLength, await _localizationService.GetLocalizedEnumAsync(product.RecurringCyclePeriod));

        //rental info
        if (product.IsRental)
        {
            var rentalStartDate = sci.RentalStartDateUtc.HasValue
                ? _productService.FormatRentalDate(product, sci.RentalStartDateUtc.Value)
                : string.Empty;
            var rentalEndDate = sci.RentalEndDateUtc.HasValue
                ? _productService.FormatRentalDate(product, sci.RentalEndDateUtc.Value)
                : string.Empty;
            cartItemModel.RentalInfo =
                string.Format(await _localizationService.GetResourceAsync("ShoppingCart.Rental.FormattedDate"),
                    rentalStartDate, rentalEndDate);
        }

        //unit prices
        if (product.CallForPrice &&
            //also check whether the current user is impersonated
            (!_orderSettings.AllowAdminsToBuyCallForPriceProducts || _workContext.OriginalCustomerIfImpersonated == null))
        {
            cartItemModel.UnitPrice = await _localizationService.GetResourceAsync("Products.CallForPrice");
            cartItemModel.UnitPriceValue = 0;
        }
        else
        {
            decimal itemPrice = 0;
            var (_unitPrice, _oldprice, _msrp, _, _) = await _shoppingCartExtendedCartService.GetCustomUnitPriceAsync(product,
                customer, store,
                sci.ShoppingCartType,
                1, sci.AttributesXml, 0,
                sci.RentalStartDateUtc, sci.RentalEndDateUtc, false);

            (_unitPrice, _) = await _taxService.GetProductPriceAsync(product, _unitPrice);
            if (_oldprice > 0)
                (_oldprice, _) = await _taxService.GetProductPriceAsync(product, _oldprice);

            itemPrice = _unitPrice;

            #region Membership Price Section

            (var _memberShipPrice, _) = await _shoppingCartExtendedCartService.MemberShipPriceOfProduct(product.Id, _msrp, _oldprice, _unitPrice);

            if (_memberShipPrice > 0)
            {
                if (_oldprice > 0 && _oldprice > _unitPrice && isCustomerElgibleForMembershipPrice)

                    itemPrice =
                     _memberShipPrice - ((_memberShipPrice * (
                     ((_oldprice - _unitPrice) / _oldprice) * 100)) / 100);
                else if (isCustomerElgibleForMembershipPrice)
                    itemPrice = _memberShipPrice;

                #region MembershipDiscount

                memberShipDiscount = (_oldprice > 0 && _oldprice > _unitPrice ? _oldprice : _unitPrice) - _memberShipPrice;
                memberShipDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(memberShipDiscount, await _workContext.GetWorkingCurrencyAsync());
                if (isCustomerElgibleForMembershipPrice)
                {
                    cartItemModel.MemberShipDiscountValue = memberShipDiscount * sci.Quantity;
                    cartItemModel.MemberShipDiscount = "-" + await _priceFormatter.FormatPriceAsync(memberShipDiscount * sci.Quantity);
                }


                #endregion

                _memberShipPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(_memberShipPrice, await _workContext.GetWorkingCurrencyAsync());

                cartItemModel.MemberShipPriceValue = _memberShipPrice;

                cartItemModel.MemberShipPrice = await _priceFormatter.FormatPriceAsync(_memberShipPrice);

            }

            #endregion

            var shoppingCartUnitPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(_unitPrice, await _workContext.GetWorkingCurrencyAsync());
            if (_oldprice > 0)
                _oldprice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(_oldprice, await _workContext.GetWorkingCurrencyAsync());

            cartItemModel.UnitPriceValue = shoppingCartUnitPriceWithDiscount;
            cartItemModel.OldPriceValue = _oldprice;

            var shoppingCartItemPriceWithDiscount =
                await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(itemPrice, await _workContext.GetWorkingCurrencyAsync());
            if (_oldprice > await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(_unitPrice, await _workContext.GetWorkingCurrencyAsync()))
            {
                cartItemModel.UnitPriceValue = _oldprice;
                cartItemModel.UnitPrice = await _priceFormatter.FormatPriceAsync(_oldprice);
                if (isCustomerElgibleForMembershipPrice && cartItemModel.MemberShipPriceValue > 0)
                    cartItemModel.OfferDiscountValue = sci.Quantity * (cartItemModel.MemberShipPriceValue - shoppingCartItemPriceWithDiscount);
                else
                    cartItemModel.OfferDiscountValue = sci.Quantity * (_oldprice - shoppingCartUnitPriceWithDiscount);

                cartItemModel.OfferDiscount = "-" + await _priceFormatter.FormatPriceAsync(cartItemModel.OfferDiscountValue);
            }
            else
                cartItemModel.UnitPrice = await _priceFormatter.FormatPriceAsync(shoppingCartUnitPriceWithDiscount);

            #region Buy More Save More 

            if (buyMoreSaveMoreDiscount > 0)
            {
                if (cart.Count > 1)
                {
                    buyMoreSaveMoreDiscountBase =
                        discountType == CustomDiscountType.Percent ?
                           await _priceCalculationService.RoundPriceAsync((((shoppingCartItemPriceWithDiscount * sci.Quantity) * buyMoreSaveMoreDiscount) / 100), workingCurrency) :
                                                  (buyMoreSaveMoreDiscount > shoppingCartItemPriceWithDiscount ? shoppingCartItemPriceWithDiscount * sci.Quantity :
                                                  buyMoreSaveMoreDiscount * sci.Quantity);


                    cartItemModel.BuyMoreSaveMoreDiscount = "-" + await _priceFormatter.FormatPriceAsync(buyMoreSaveMoreDiscountBase);
                }
                else
                {
                    buyMoreSaveMoreDiscountBase =
                      discountType == CustomDiscountType.Percent ?
                         await _priceCalculationService.RoundPriceAsync((((shoppingCartItemPriceWithDiscount * (sci.Quantity - 1)) * buyMoreSaveMoreDiscount) / 100), workingCurrency) :
                                                (buyMoreSaveMoreDiscount > shoppingCartItemPriceWithDiscount ? shoppingCartItemPriceWithDiscount * (sci.Quantity - 1) :
                                                buyMoreSaveMoreDiscount * (sci.Quantity - 1));


                    cartItemModel.BuyMoreSaveMoreDiscount = "-" + await _priceFormatter.FormatPriceAsync(buyMoreSaveMoreDiscountBase);
                }
            }

            #endregion
        }
        //subtotal, discount
        if (product.CallForPrice &&
            //also check whether the current user is impersonated
            (!_orderSettings.AllowAdminsToBuyCallForPriceProducts || _workContext.OriginalCustomerIfImpersonated == null))
        {
            cartItemModel.SubTotal = await _localizationService.GetResourceAsync("Products.CallForPrice");
            cartItemModel.SubTotalValue = 0;
        }
        else
        {
            //sub total
            var (subTotal, shoppingCartItemDiscountBase, _, maximumDiscountQty) = await _shoppingCartService.GetSubTotalAsync(sci, true);
            var (shoppingCartItemSubTotalWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, subTotal);
            var shoppingCartItemSubTotalWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartItemSubTotalWithDiscountBase, await _workContext.GetWorkingCurrencyAsync());

            cartItemModel.SubTotal = await _priceFormatter.FormatPriceAsync(shoppingCartItemSubTotalWithDiscount);
            cartItemModel.MaximumDiscountedQty = maximumDiscountQty;

            cartItemModel.SubTotal = await _priceFormatter.FormatPriceAsync(cartItemModel.UnitPriceValue * sci.Quantity);

            decimal subTotalIncludeDiscount = (
      await _priceCalculationService.RoundPriceAsync(cartItemModel.UnitPriceValue, workingCurrency) * sci.Quantity)
      - (await _priceCalculationService.RoundPriceAsync
      (buyMoreSaveMoreDiscountBase, workingCurrency)
       + await _priceCalculationService.RoundPriceAsync(cartItemModel.OfferDiscountValue, workingCurrency)
       + await _priceCalculationService.RoundPriceAsync(cartItemModel.MemberShipDiscountValue, workingCurrency));


            decimal appliedDiscountAmount = decimal.Zero;
            List<Discount> appliedDiscounts = new List<Discount>();
            (subTotalIncludeDiscount, appliedDiscountAmount, appliedDiscounts, _) = await _priceCalculationService.GetItemDiscount
                (product, await _workContext.GetCurrentCustomerAsync(), subTotalIncludeDiscount, decimal.Zero,
                     sci.Quantity, sci.RentalStartDateUtc, sci.RentalEndDateUtc);

            if (appliedDiscountAmount > 0)
                cartItemModel.Discount = "-" + await _priceFormatter.FormatPriceAsync(appliedDiscountAmount);

            cartItemModel.SubTotalIncludeDiscount = await _priceFormatter.FormatPriceAsync(subTotalIncludeDiscount);
            //display an applied discount amount

        }

        //picture
        if (_shoppingCartSettings.ShowProductImagesOnShoppingCart)
        {
            cartItemModel.Picture = await PrepareCartItemPictureModelAsync(sci,
                _mediaSettings.CartThumbPictureSize, true, cartItemModel.ProductName);
        }

        //item warnings
        var itemWarnings = await _shoppingCartService.GetShoppingCartItemWarningsAsync(
            await _workContext.GetCurrentCustomerAsync(),
            sci.ShoppingCartType,
            product,
            sci.StoreId,
            sci.AttributesXml,
            sci.CustomerEnteredPrice,
            sci.RentalStartDateUtc,
            sci.RentalEndDateUtc,
            sci.Quantity,
            false,
            sci.Id);
        foreach (var warning in itemWarnings)
            cartItemModel.Warnings.Add(warning);

        return (cartItemModel, memberShipDiscount * sci.Quantity);
    }
    public virtual async Task<OrderTotalsModel> PrepareCustomOrderTotalsModelAsync(IList<ShoppingCartItem> cart, bool isEditable)
    {
        var model = new OrderTotalsModel
        {
            IsEditable = isEditable
        };

        if (cart.Any())
        {
            //subtotal
            var subTotalIncludingTax = await _workContext.GetTaxDisplayTypeAsync() == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal;
            var (orderSubTotalDiscountAmountBase, appliedDiscounts, subTotalWithoutDiscountBase, _, _, discountAmountsApplied) =
              await _orderTotalCalculationExtendedService.GetCustomShoppingCartSubTotalAsync(cart, subTotalIncludingTax);
            var subtotalBase = subTotalWithoutDiscountBase;
            var subtotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(subtotalBase, await _workContext.GetWorkingCurrencyAsync());
            decimal orderSubTotalDiscountAmount = 0;


            #region calculate BuyMoreSaveMoreDiscount

            (decimal buyMoreSaveMoreDiscount, decimal membershipdiscount, decimal offerDiscount, decimal offerDiscountDefault, decimal productItemsDiscount, List<DiscountSummary> discountSummary)
                = await _orderTotalCalculationExtendedService.GetCustomBuyMoreSaveMoreDiscountAndMemberShipDiscountAsync(cart);

            model.DiscountSummary = discountSummary;
            if (buyMoreSaveMoreDiscount > 0)
                model.BuyMoreSaveMoreDiscount = "-" + await _priceFormatter.FormatPriceAsync(buyMoreSaveMoreDiscount);
            if (membershipdiscount > 0)
                model.MemberShipDiscount = "-" + await _priceFormatter.FormatPriceAsync(membershipdiscount);
            if (orderSubTotalDiscountAmountBase > decimal.Zero || productItemsDiscount > 0)
            {

                DiscountSummary subtotalDiscountSummary = new DiscountSummary();
                subtotalDiscountSummary.Heading = await _localizationService.GetResourceAsync("OrderSummary.Coupon.SubtotalDiscount.heading");
                subtotalDiscountSummary.Type = DiscountType.SubTotalCouponDiscount;
                var counter = 0;
                foreach (var discount in appliedDiscounts)
                {
                    subtotalDiscountSummary.Summary += string.Format(
       await _localizationService.GetResourceAsync(
        discount.UsePercentage ?
           "OrderSummary.Coupon.SubtotalDiscount.Summary.percent"
           : "OrderSummary.Coupon.SubtotalDiscount.Summary.fixed"
           ),
            discount.CouponCode,
        (discount.UsePercentage ? discount.DiscountPercentage : discount.DiscountAmount).ToString("F2"),
        "-" + await _priceFormatter.FormatPriceAsync(discountAmountsApplied[counter])
       );
                    counter++;
                }

                discountSummary.Add(subtotalDiscountSummary);

                orderSubTotalDiscountAmount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(orderSubTotalDiscountAmountBase + productItemsDiscount, await _workContext.GetWorkingCurrencyAsync());
                model.SubTotalDiscount = await _priceFormatter.FormatPriceAsync(-orderSubTotalDiscountAmount, true, await _workContext.GetWorkingCurrencyAsync(), (await _workContext.GetWorkingLanguageAsync()).Id, subTotalIncludingTax);
                model.SubTotalDiscount = "-" + model.SubTotalDiscount.Replace("(", "").Replace(")", "");
            }
            #region Membership fees

            decimal memberShipFee = 0;
            decimal memberShipFeeDiscount = 0;
            if (await _customerExtendedService.IsMemberShipAddedInCart(await _workContext.GetCurrentCustomerAsync()))
            {
                (memberShipFee, memberShipFeeDiscount) = await _orderTotalCalculationExtendedService.GetMemberShipFee();
                model.MemberShipFee = await _priceFormatter.FormatPriceAsync(memberShipFee);
                if (memberShipFeeDiscount > 0)
                {
                    model.MemberShipFeeDiscount = "-" + await _priceFormatter.FormatPriceAsync(memberShipFeeDiscount);
                    discountSummary.Add(new DiscountSummary()
                    {
                        Heading = await _localizationService.GetResourceAsync("OrderSummary.MembershipFeeDiscount.heading"),
                        Type = DiscountType.MemberShipFeeDiscount,
                        Summary = string.Format(
                                          await _localizationService.GetResourceAsync("OrderSummary.MembershipFeeDiscount.Summary"), model.MemberShipFeeDiscount)
                    });
                }

            }
            #endregion



            var customSubTotal = await _orderTotalCalculationExtendedService.GetCustomShoppingCartSubTotalAsync(cart);
            customSubTotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(customSubTotal, await _workContext.GetWorkingCurrencyAsync());

            if (offerDiscount > 0)
                model.offerDiscount = "-" + await _priceFormatter.FormatPriceAsync(offerDiscount
                , true, await _workContext.GetWorkingCurrencyAsync(), (await _workContext.GetWorkingLanguageAsync()).Id, false);

            model.subTotalWithOutDiscount = await _priceFormatter.FormatPriceAsync(customSubTotal
                , true, await _workContext.GetWorkingCurrencyAsync(), (await _workContext.GetWorkingLanguageAsync()).Id, false);


            #endregion


            #region CustomOrderTotal

            #endregion

            //shipping info
            decimal shoppingCartShipping = 0;
            model.RequiresShipping = await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart);
            if (model.RequiresShipping)
            {
                var (shoppingCartShippingBase, additionalFee) = await _orderTotalCalculationExtendedService.GetCustomShoppingCartShippingTotalAsync(cart);
                if (shoppingCartShippingBase.HasValue)
                {
                    shoppingCartShipping = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartShippingBase.Value, await _workContext.GetWorkingCurrencyAsync());
                    model.Shipping = await _priceFormatter.FormatShippingPriceAsync(shoppingCartShipping, true);
                    if (additionalFee.HasValue && additionalFee.Value > 0)
                        model.AdditionalFee = await _priceFormatter.FormatShippingPriceAsync(additionalFee.Value, true);

                    //selected shipping method
                    var shippingOption = await _genericAttributeService.GetAttributeAsync<ShippingOption>(await _workContext.GetCurrentCustomerAsync(),
                        NopCustomerDefaults.SelectedShippingOptionAttribute, (await _storeContext.GetCurrentStoreAsync()).Id);
                    if (shippingOption != null)
                        model.SelectedShippingMethod = shippingOption.Name;
                }
            }
            else
            {
                model.HideShippingTotal = _shippingSettings.HideShippingTotal;
            }

            //payment method fee
            var paymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.SelectedPaymentMethodAttribute, (await _storeContext.GetCurrentStoreAsync()).Id);
            var paymentMethodAdditionalFee = await _paymentService.GetAdditionalHandlingFeeAsync(cart, paymentMethodSystemName);
            var (paymentMethodAdditionalFeeWithTaxBase, _) = await _taxService.GetPaymentMethodAdditionalFeeAsync(paymentMethodAdditionalFee, await _workContext.GetCurrentCustomerAsync());
            if (paymentMethodAdditionalFeeWithTaxBase > decimal.Zero)
            {
                var paymentMethodAdditionalFeeWithTax = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(paymentMethodAdditionalFeeWithTaxBase, await _workContext.GetWorkingCurrencyAsync());
                model.PaymentMethodAdditionalFee = await _priceFormatter.FormatPaymentMethodAdditionalFeeAsync(paymentMethodAdditionalFeeWithTax, true);
            }

            //tax
            bool displayTax;
            bool displayTaxRates;
            if (_taxSettings.HideTaxInOrderSummary && await _workContext.GetTaxDisplayTypeAsync() == TaxDisplayType.IncludingTax || (((await _workContext.GetCurrentCustomerAsync()).ShippingAddressId ?? 0) == 0))
            {
                displayTax = false;
                displayTaxRates = false;
            }
            else
            {
                var (shoppingCartTaxBase, taxRates, taxes) = await _orderTotalCalculationExtendedService.CustomGetTaxTotalAsync(cart);
                var shoppingCartTax = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartTaxBase, await _workContext.GetWorkingCurrencyAsync());

                if (shoppingCartTaxBase == 0 && _taxSettings.HideZeroTax)
                {
                    displayTax = false;
                    displayTaxRates = false;
                }
                else
                {
                    displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Any();
                    displayTax = !displayTaxRates;

                    model.Tax = await _priceFormatter.FormatPriceAsync(shoppingCartTax, true, false);
                    foreach (var tax in taxes)
                    {
                        tax.Amount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(tax.Amount, await _workContext.GetWorkingCurrencyAsync());
                    }
                    model.Taxes = taxes;

                    foreach (var tr in taxRates)
                    {
                        model.TaxRates.Add(new OrderTotalsModel.TaxRate
                        {
                            Rate = _priceFormatter.FormatTaxRate(tr.Key),
                            Value = await _priceFormatter.FormatPriceAsync(await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(tr.Value, await _workContext.GetWorkingCurrencyAsync()), true, false),
                        });
                    }
                }
            }

            model.DisplayTaxRates = displayTaxRates;
            model.DisplayTax = displayTax;

            //total

            var (shoppingCartTotalBase, orderTotalDiscountAmountBase, orderAppliedDiscounts, appliedGiftCards, redeemedRewardPoints, redeemedRewardPointsAmount, orderDiscountAmountsApplied) =
                await _orderTotalCalculationExtendedService.GetCustomShoppingCartTotalWithDiscountInfosync(cart);

            if (shoppingCartTotalBase.HasValue)
            {
                var shoppingCartTotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartTotalBase.Value, await _workContext.GetWorkingCurrencyAsync());
                model.OrderTotal = await _priceFormatter.FormatPriceAsync(shoppingCartTotal, true, false);
            }
            (decimal customDutyPercentage, decimal customDuty) = await _orderTotalCalculationExtendedService.GetCustomDuty(cart);
            customDuty = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(customDuty, await _workContext.GetWorkingCurrencyAsync());
            if (customDuty > 0)
            {
                model.CustomDuty = await _priceFormatter.FormatPriceAsync(customDuty);
                model.CustomDutyPercentage = customDutyPercentage;
            }

            model.SubTotal = await _priceFormatter.FormatPriceAsync((subtotal +
          memberShipFee + (offerDiscountDefault - offerDiscount))
          - memberShipFeeDiscount
          - orderSubTotalDiscountAmount
          - buyMoreSaveMoreDiscount
          - membershipdiscount - (shoppingCartTotalBase.HasValue ? 0 : orderTotalDiscountAmountBase),
          true,
          await _workContext.GetWorkingCurrencyAsync(), (await _workContext.GetWorkingLanguageAsync()).Id, subTotalIncludingTax);
            //discount
            if (orderTotalDiscountAmountBase > decimal.Zero)
            {
                DiscountSummary orderlDiscountSummary = new DiscountSummary();
                orderlDiscountSummary.Heading = await _localizationService.GetResourceAsync("OrderSummary.Coupon.OrderDiscount.heading");
                orderlDiscountSummary.Type = DiscountType.OrderDiscount;
                var counter = 0;
                foreach (var discount in orderAppliedDiscounts)
                {
                    orderlDiscountSummary.Summary += string.Format(
       await _localizationService.GetResourceAsync(
        discount.UsePercentage ?
           "OrderSummary.Coupon.OrderDiscount.Summary.percent"
           : "OrderSummary.Coupon.OrderDiscount.Summary.fixed"
           ),
            discount.CouponCode,
        (discount.UsePercentage ? discount.DiscountPercentage : discount.DiscountAmount).ToString("F2"),
        "-" + await _priceFormatter.FormatPriceAsync(orderDiscountAmountsApplied[counter])
       );
                }
                discountSummary.Add(orderlDiscountSummary);
                var orderTotalDiscountAmount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(orderTotalDiscountAmountBase, await _workContext.GetWorkingCurrencyAsync());
                model.OrderTotalDiscount = "-" + (await _priceFormatter.FormatPriceAsync(-orderTotalDiscountAmount, true, false)).Replace("(", "").Replace(")", "");
            }

            //gift cards
            if (appliedGiftCards != null && appliedGiftCards.Any())
            {
                foreach (var appliedGiftCard in appliedGiftCards)
                {
                    var gcModel = new OrderTotalsModel.GiftCard
                    {
                        Id = appliedGiftCard.GiftCard.Id,
                        CouponCode = appliedGiftCard.GiftCard.GiftCardCouponCode,
                    };
                    var amountCanBeUsed = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(appliedGiftCard.AmountCanBeUsed, await _workContext.GetWorkingCurrencyAsync());
                    gcModel.Amount = await _priceFormatter.FormatPriceAsync(-amountCanBeUsed, true, false);

                    var remainingAmountBase = await _giftCardService.GetGiftCardRemainingAmountAsync(appliedGiftCard.GiftCard) - appliedGiftCard.AmountCanBeUsed;
                    var remainingAmount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(remainingAmountBase, await _workContext.GetWorkingCurrencyAsync());
                    gcModel.Remaining = await _priceFormatter.FormatPriceAsync(remainingAmount, true, false);

                    model.GiftCards.Add(gcModel);
                }
            }

            //reward points to be spent (redeemed)
            if (redeemedRewardPointsAmount > decimal.Zero)
            {
                var redeemedRewardPointsAmountInCustomerCurrency = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(redeemedRewardPointsAmount, await _workContext.GetWorkingCurrencyAsync());
                model.RedeemedRewardPoints = redeemedRewardPoints;
                model.RedeemedRewardPointsAmount = await _priceFormatter.FormatPriceAsync(-redeemedRewardPointsAmountInCustomerCurrency, true, false);
            }

            //reward points to be earned
            if (_rewardPointsSettings.Enabled && _rewardPointsSettings.DisplayHowMuchWillBeEarned && shoppingCartTotalBase.HasValue)
            {
                //get shipping total
                var shippingBaseInclTax = !model.RequiresShipping ? 0 : (await _orderTotalCalculationService.GetShoppingCartShippingTotalAsync(cart, true)).shippingTotal ?? 0;

                //get total for reward points
                var totalForRewardPoints = _orderTotalCalculationService
                    .CalculateApplicableOrderTotalForRewardPoints(shippingBaseInclTax, shoppingCartTotalBase.Value);
                if (totalForRewardPoints > decimal.Zero)
                    model.WillEarnRewardPoints = await _orderTotalCalculationService.CalculateRewardPointsAsync(await _workContext.GetCurrentCustomerAsync(), totalForRewardPoints);
            }
        }

        return model;
    }
    public async Task<ShoppingCartModel> ModifyCartItemModelForCustomUpdates(ShoppingCartModel model)
    {
        #region Buy More Save More 

        var workingCurrency = await _workContext.GetWorkingCurrencyAsync();
        decimal memberShipDiscount = 0;
        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();
        // Buy moreSaveMore
        decimal buyMoreDiscount = 0;
        int notElgibleForSavemoreDiscountCartId = 0;
        CustomDiscountType discountType = CustomDiscountType.Fixed;

        bool isBuyMoreSaveMoreEnabled = await _settingService.GetSettingByKeyAsync<bool>("MarketingSettings.EnableBuyMoreSaveMoreDiscount");
        decimal buyMoreSaveMoreSingleItemThreshold = await _settingService.GetSettingByKeyAsync<decimal>("MarketingSettings.ApplyBuyMoreSaveMoreOnSingleItemOverThreshold");

        var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

        foreach (var item in model.Items)
        {
            var sci = cart.FirstOrDefault(sc => sc.Id == item.Id);
            if (sci != null)
            {
                item.Variant = await _productExtendedService.GetItemVariantInfo(sci.ProductId, sci.AttributesXml);
                item.ProductName = string.IsNullOrWhiteSpace(item.Variant?.Title) ? item.ProductName : item.Variant.Title;
            }
        }
        if (isBuyMoreSaveMoreEnabled && (model.Items.Count > 1 || model.Items.Sum(c => c.Quantity) > 1))
        {
            decimal minTotal = 0;
            decimal maxTotal = 0;
            var subTotalIncludingTax = await _workContext.GetTaxDisplayTypeAsync() == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal;
            var (orderSubTotalDiscountAmountBase, _, subTotal, _, _) = await _orderTotalCalculationService.GetShoppingCartSubTotalAsync(cart, subTotalIncludingTax);
            string buyMoreSaveMoreDiscounts = await _settingService.GetSettingByKeyAsync<string>("MarketingSettings.BuyMoreSaveMoreDiscountConfiguration");
            if (!string.IsNullOrEmpty(buyMoreSaveMoreDiscounts))
            {
                string[] discountLevels = buyMoreSaveMoreDiscounts.Split('|', StringSplitOptions.RemoveEmptyEntries);
                foreach (var level in discountLevels)
                {
                    string[] levelConfig = level.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                    if (levelConfig.Length == 4)
                    {
                        decimal.TryParse(levelConfig[3], out maxTotal);
                        decimal.TryParse(levelConfig[2], out minTotal);
                        if (levelConfig[0].Trim() == "%")
                            discountType = CustomDiscountType.Percent;
                        if (subTotal > minTotal && subTotal <= maxTotal)
                        {
                            decimal.TryParse(levelConfig[1], out buyMoreDiscount);
                            break;
                        }

                    }
                }
            }

            decimal costlyItemPrice = 0;
            decimal itemPrice = 0;
            foreach (var item in model.Items)
            {
                itemPrice = Decimal.Parse(item.UnitPrice.Replace("\"", ""), NumberStyles.Currency);

                if (((itemPrice * item.Quantity) > costlyItemPrice && model.Items.Count > 1) ||
               (model.Items.Count == 1 && (model.Items.Sum(c => c.Quantity) == 1 || itemPrice < buyMoreSaveMoreSingleItemThreshold)))
                {
                    costlyItemPrice = itemPrice * item.Quantity;
                    notElgibleForSavemoreDiscountCartId = item.Id;
                }
            }

        }

        var isCustomerElgibleForMembershipPrice = await _customerExtendedService.IsCustomerEligibleForMemberShipDiscount(await _workContext.GetCurrentCustomerAsync());
        foreach (var item in model.Items)
        {
            var sci = cart.Where(m => m.Id == item.Id).FirstOrDefault();
            if (sci == null)
                continue;
            item.SpecialInstructions = sci.SpecialInstructions;
            var product = await _productService.GetProductByIdAsync(item.ProductId);
            item.AttributeInfo = await _customProductAttributeFormatter.CustomFormatAttributesAsync(product, sci.AttributesXml);
            decimal itemPrice = 0;
            var (_unitPrice, _oldprice, _msrp, _, _) = await _shoppingCartExtendedCartService.GetCustomUnitPriceAsync(product, customer, store
        , sci.ShoppingCartType,
        1, sci.AttributesXml, 0,
        sci.RentalStartDateUtc, sci.RentalEndDateUtc, false);


            if (_oldprice > 0)
                (_oldprice, _) = await _taxService.GetProductPriceAsync(product, _oldprice);
            (_unitPrice, _) = await _taxService.GetProductPriceAsync(product, _unitPrice);
            itemPrice = _unitPrice;
            #region Membership Price Section



            (var _memberShipPrice, _) = await _shoppingCartExtendedCartService.MemberShipPriceOfProduct(item.ProductId, _msrp, _oldprice, _unitPrice);

            if (_memberShipPrice > 0)
            {
                if (_oldprice > 0 && _oldprice > _unitPrice && isCustomerElgibleForMembershipPrice)
                    itemPrice =
                   _memberShipPrice - ((_memberShipPrice * (
                   ((_oldprice - _unitPrice) / _oldprice) * 100)) / 100);


                else if (isCustomerElgibleForMembershipPrice)
                    itemPrice = _memberShipPrice;

                #region MembershipDiscount

                decimal _memberShipDiscount = (_oldprice > 0 && _oldprice > _unitPrice ? _oldprice : _unitPrice) - _memberShipPrice;
                _memberShipDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(_memberShipDiscount, await _workContext.GetWorkingCurrencyAsync());
                if (isCustomerElgibleForMembershipPrice)
                {
                    item.MemberShipDiscountValue = _memberShipDiscount * sci.Quantity;
                    item.MemberShipDiscount = "-" + await _priceFormatter.FormatPriceAsync(_memberShipDiscount * sci.Quantity);
                }

                memberShipDiscount += _memberShipDiscount * sci.Quantity;
                #endregion

                _memberShipPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(_memberShipPrice, await _workContext.GetWorkingCurrencyAsync());

                item.MemberShipPriceValue = _memberShipPrice;

                item.MemberShipPrice = await _priceFormatter.FormatPriceAsync(_memberShipPrice);

            }

            #endregion

            var shoppingCartUnitPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(_unitPrice, await _workContext.GetWorkingCurrencyAsync());
            if (_oldprice > 0)
                _oldprice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(_oldprice, await _workContext.GetWorkingCurrencyAsync());

            item.UnitPriceValue = shoppingCartUnitPriceWithDiscount;
            item.OldPriceValue = _oldprice;
            var (shoppingCartItemPriceWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, itemPrice);
            var shoppingCartItemPriceWithDiscount =
                await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartItemPriceWithDiscountBase, await _workContext.GetWorkingCurrencyAsync());
            decimal buyMoreSaveMoreDiscountBase = 0;
            if (item.Id != notElgibleForSavemoreDiscountCartId && buyMoreDiscount > 0)
            {

                if (model.Items.Count > 1)
                {
                    buyMoreSaveMoreDiscountBase = discountType == CustomDiscountType.Percent ?
                 await _priceCalculationService.RoundPriceAsync((((shoppingCartItemPriceWithDiscount * sci.Quantity) * buyMoreDiscount) / 100), workingCurrency) :
                       (buyMoreDiscount > shoppingCartItemPriceWithDiscount ? shoppingCartItemPriceWithDiscount * sci.Quantity : buyMoreDiscount * sci.Quantity);
                    item.BuyMoreSaveMoreDiscount = "-" + await _priceFormatter.FormatPriceAsync(await _priceCalculationService.RoundPriceAsync(
                        buyMoreSaveMoreDiscountBase, workingCurrency));
                }
                else
                {
                    buyMoreSaveMoreDiscountBase = discountType == CustomDiscountType.Percent ?
                 await _priceCalculationService.RoundPriceAsync((((shoppingCartItemPriceWithDiscount * (sci.Quantity - 1)) * buyMoreDiscount) / 100), workingCurrency) :
                       (buyMoreDiscount > shoppingCartItemPriceWithDiscount ? shoppingCartItemPriceWithDiscount * (sci.Quantity - 1) : buyMoreDiscount * (sci.Quantity - 1));
                    item.BuyMoreSaveMoreDiscount = "-" + await _priceFormatter.FormatPriceAsync(await _priceCalculationService.RoundPriceAsync(
                        buyMoreSaveMoreDiscountBase, workingCurrency));
                }
            }
            if (_oldprice > await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(_unitPrice, await _workContext.GetWorkingCurrencyAsync()))
            {

                item.UnitPrice = await _priceFormatter.FormatPriceAsync(_oldprice);
                item.UnitPriceValue = _oldprice;
                if (isCustomerElgibleForMembershipPrice && item.MemberShipPriceValue > 0)
                    item.OfferDiscountValue = await _priceCalculationService.RoundPriceAsync(sci.Quantity *
                        (item.MemberShipPriceValue - shoppingCartItemPriceWithDiscount), workingCurrency);
                else
                    item.OfferDiscountValue = await _priceCalculationService.RoundPriceAsync(sci.Quantity * (_oldprice - shoppingCartUnitPriceWithDiscount), workingCurrency);
                item.OfferDiscount = "-" + await _priceFormatter.FormatPriceAsync(item.OfferDiscountValue);
            }
            else
                item.UnitPrice = await _priceFormatter.FormatPriceAsync(shoppingCartUnitPriceWithDiscount);
            item.SubTotal = await _priceFormatter.FormatPriceAsync(item.UnitPriceValue * sci.Quantity);
            decimal discount = 0;
            if (!string.IsNullOrEmpty(item.Discount))
            {
                decimal.TryParse(item.Discount, NumberStyles.Currency,
             CultureInfo.CurrentCulture.NumberFormat, out discount);
            }


            decimal subTotalIncludeDiscount = (
       await _priceCalculationService.RoundPriceAsync(item.UnitPriceValue, workingCurrency) * sci.Quantity)
       - (await _priceCalculationService.RoundPriceAsync
       (buyMoreSaveMoreDiscountBase, workingCurrency)
        + await _priceCalculationService.RoundPriceAsync(item.OfferDiscountValue, workingCurrency)
        + await _priceCalculationService.RoundPriceAsync(item.MemberShipDiscountValue, workingCurrency));
            decimal appliedDiscountAmount = decimal.Zero;
            List<Discount> appliedDiscounts = new List<Discount>();
            (subTotalIncludeDiscount, appliedDiscountAmount, appliedDiscounts, _) = await _priceCalculationService.GetItemDiscount(product, customer, subTotalIncludeDiscount, decimal.Zero,
                     sci.Quantity, sci.RentalStartDateUtc, sci.RentalEndDateUtc);


            if (appliedDiscountAmount > 0)
                item.Discount = "-" + await _priceFormatter.FormatPriceAsync(appliedDiscountAmount);
            item.SubTotalIncludeDiscount = await _priceFormatter.FormatPriceAsync(subTotalIncludeDiscount);
        }

        model.MemberShipDiscountValue = await _priceCalculationService.RoundPriceAsync(memberShipDiscount, workingCurrency);
        model.MemberShipDiscount = "-" + await _priceFormatter.FormatPriceAsync(await _priceCalculationService.RoundPriceAsync(memberShipDiscount,
            workingCurrency));

        model.SubTotal = await _priceFormatter.FormatPriceAsync(await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(await _orderTotalCalculationExtendedService.GetCustomShoppingCartSubTotalAsync(cart), await _workContext.GetWorkingCurrencyAsync()),
             true, await _workContext.GetWorkingCurrencyAsync(), (await _workContext.GetWorkingLanguageAsync()).Id, false);



        return model;

        #endregion
    }
    public async Task<ShoppingCartModel> ModifyCartItemModelForCustomUpdates(ShoppingCartModel model, Customer customer)
    {
        #region Buy More Save More 

        var workingCurrency = await _workContext.GetWorkingCurrencyAsync();
        decimal memberShipDiscount = 0;
        var store = await _storeContext.GetCurrentStoreAsync();
        // Buy moreSaveMore

        var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
        var subTotalIncludingTax = await _workContext.GetTaxDisplayTypeAsync() == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal;
        var (orderSubTotalDiscountAmountBase, _, subTotal, _, _) = await _orderTotalCalculationService.GetShoppingCartSubTotalAsync(cart, subTotalIncludingTax);

        (CustomDiscountType discountType, decimal buyMoreDiscount, int notElgibleForSavemoreDiscountCartId) =
            await _shoppingCartExtendedCartService.GetBuyMoreSaveMoreDiscountDetailsAsync(cart, subTotal);





        var isCustomerElgibleForMembershipPrice = await _customerExtendedService.IsCustomerEligibleForMemberShipDiscount(customer);
        foreach (var item in model.Items)
        {
            var sci = cart.Where(m => m.Id == item.Id).FirstOrDefault();
            if (sci == null)
                continue;
            item.SpecialInstructions = sci.SpecialInstructions;
            var product = await _productService.GetProductByIdAsync(item.ProductId);
            decimal itemPrice = 0;
            var (_unitPrice, _oldprice, _msrp, _, _) = await _shoppingCartExtendedCartService.GetCustomUnitPriceAsync(product,
        customer, store,
        sci.ShoppingCartType,
        1, sci.AttributesXml, 0,
        sci.RentalStartDateUtc, sci.RentalEndDateUtc, false);


            if (_oldprice > 0)
                (_oldprice, _) = await _taxService.GetProductPriceAsync(product, _oldprice);
            (_unitPrice, _) = await _taxService.GetProductPriceAsync(product, _unitPrice);
            itemPrice = _unitPrice;
            #region Membership Price Section



            (var _memberShipPrice, _) = await _shoppingCartExtendedCartService.MemberShipPriceOfProduct(item.ProductId, _msrp, _oldprice, _unitPrice);

            if (_memberShipPrice > 0)
            {
                if (_oldprice > 0 && _oldprice > _unitPrice && isCustomerElgibleForMembershipPrice)
                    itemPrice =
                   _memberShipPrice - ((_memberShipPrice * (
                   ((_oldprice - _unitPrice) / _oldprice) * 100)) / 100);


                else if (isCustomerElgibleForMembershipPrice)
                    itemPrice = _memberShipPrice;

                #region MembershipDiscount

                decimal _memberShipDiscount = (_oldprice > 0 && _oldprice > _unitPrice ? _oldprice : _unitPrice) - _memberShipPrice;
                _memberShipDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(_memberShipDiscount, await _workContext.GetWorkingCurrencyAsync());
                if (isCustomerElgibleForMembershipPrice)
                {
                    item.MemberShipDiscountValue = _memberShipDiscount * sci.Quantity;
                    item.MemberShipDiscount = "-" + await _priceFormatter.FormatPriceAsync(_memberShipDiscount * sci.Quantity);
                }

                memberShipDiscount += _memberShipDiscount * sci.Quantity;
                #endregion

                _memberShipPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(_memberShipPrice, await _workContext.GetWorkingCurrencyAsync());

                item.MemberShipPriceValue = _memberShipPrice;

                item.MemberShipPrice = await _priceFormatter.FormatPriceAsync(_memberShipPrice);

            }

            #endregion

            var shoppingCartUnitPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(_unitPrice, await _workContext.GetWorkingCurrencyAsync());
            if (_oldprice > 0)
                _oldprice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(_oldprice, await _workContext.GetWorkingCurrencyAsync());

            item.UnitPriceValue = shoppingCartUnitPriceWithDiscount;
            item.OldPriceValue = _oldprice;
            var (shoppingCartItemPriceWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, itemPrice);
            var shoppingCartItemPriceWithDiscount =
                await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartItemPriceWithDiscountBase, await _workContext.GetWorkingCurrencyAsync());
            decimal buyMoreSaveMoreDiscountBase = 0;
            if (item.Id != notElgibleForSavemoreDiscountCartId && buyMoreDiscount > 0)
            {

                if (model.Items.Count > 1)
                {
                    buyMoreSaveMoreDiscountBase = discountType == CustomDiscountType.Percent ?
                     await _priceCalculationService.RoundPriceAsync((((shoppingCartItemPriceWithDiscount * sci.Quantity) * buyMoreDiscount) / 100), workingCurrency) :
                           (buyMoreDiscount > shoppingCartItemPriceWithDiscount ? shoppingCartItemPriceWithDiscount * sci.Quantity : buyMoreDiscount * sci.Quantity);
                    item.BuyMoreSaveMoreDiscount = "-" + await _priceFormatter.FormatPriceAsync(await _priceCalculationService.RoundPriceAsync(
                        buyMoreSaveMoreDiscountBase, workingCurrency));
                }
                else
                {
                    buyMoreSaveMoreDiscountBase = discountType == CustomDiscountType.Percent ?
                await _priceCalculationService.RoundPriceAsync((((shoppingCartItemPriceWithDiscount * (sci.Quantity - 1)) * buyMoreDiscount) / 100), workingCurrency) :
                      (buyMoreDiscount > shoppingCartItemPriceWithDiscount ? shoppingCartItemPriceWithDiscount * (sci.Quantity - 1) : buyMoreDiscount * (sci.Quantity - 1));
                    item.BuyMoreSaveMoreDiscount = "-" + await _priceFormatter.FormatPriceAsync(await _priceCalculationService.RoundPriceAsync(
                        buyMoreSaveMoreDiscountBase, workingCurrency));
                }
            }
            if (_oldprice > await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(_unitPrice, await _workContext.GetWorkingCurrencyAsync()))
            {

                item.UnitPrice = await _priceFormatter.FormatPriceAsync(_oldprice);
                item.UnitPriceValue = _oldprice;
                if (isCustomerElgibleForMembershipPrice && item.MemberShipPriceValue > 0)
                    item.OfferDiscountValue = await _priceCalculationService.RoundPriceAsync(sci.Quantity *
                        (item.MemberShipPriceValue - shoppingCartItemPriceWithDiscount), workingCurrency);
                else
                    item.OfferDiscountValue = await _priceCalculationService.RoundPriceAsync(sci.Quantity * (_oldprice - shoppingCartUnitPriceWithDiscount), workingCurrency);
                item.OfferDiscount = "-" + await _priceFormatter.FormatPriceAsync(item.OfferDiscountValue);
            }
            else
                item.UnitPrice = await _priceFormatter.FormatPriceAsync(shoppingCartUnitPriceWithDiscount);
            item.SubTotal = await _priceFormatter.FormatPriceAsync(item.UnitPriceValue * sci.Quantity);
            decimal discount = 0;
            if (!string.IsNullOrEmpty(item.Discount))
            {
                decimal.TryParse(item.Discount, NumberStyles.Currency,
             CultureInfo.CurrentCulture.NumberFormat, out discount);
            }


            decimal subTotalIncludeDiscount = (
       await _priceCalculationService.RoundPriceAsync(item.UnitPriceValue, workingCurrency) * sci.Quantity)
       - (await _priceCalculationService.RoundPriceAsync
       (buyMoreSaveMoreDiscountBase, workingCurrency)
        + await _priceCalculationService.RoundPriceAsync(item.OfferDiscountValue, workingCurrency)
        + await _priceCalculationService.RoundPriceAsync(item.MemberShipDiscountValue, workingCurrency));
            decimal appliedDiscountAmount = decimal.Zero;
            List<Discount> appliedDiscounts = new List<Discount>();
            (subTotalIncludeDiscount, appliedDiscountAmount, appliedDiscounts, _) = await _priceCalculationService.GetItemDiscount(product, customer, subTotalIncludeDiscount, decimal.Zero,
                     sci.Quantity, sci.RentalStartDateUtc, sci.RentalEndDateUtc);


            if (appliedDiscountAmount > 0)
                item.Discount = "-" + await _priceFormatter.FormatPriceAsync(appliedDiscountAmount);
            item.SubTotalIncludeDiscount = await _priceFormatter.FormatPriceAsync(subTotalIncludeDiscount);
        }

        model.MemberShipDiscountValue = await _priceCalculationService.RoundPriceAsync(memberShipDiscount, workingCurrency);
        model.MemberShipDiscount = "-" + await _priceFormatter.FormatPriceAsync(await _priceCalculationService.RoundPriceAsync(memberShipDiscount,
            workingCurrency));

        model.SubTotal = await _priceFormatter.FormatPriceAsync(await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(await _orderTotalCalculationExtendedService.GetCustomShoppingCartSubTotalAsync(cart), await _workContext.GetWorkingCurrencyAsync()),
             true, await _workContext.GetWorkingCurrencyAsync(), (await _workContext.GetWorkingLanguageAsync()).Id, false);



        return model;

        #endregion
    }
    public virtual async Task<MiniShoppingCartExtendedModel> PrepareCustomMiniShoppingCartModelAsync(ShoppingCartType cartType)
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var model = new MiniShoppingCartExtendedModel
        {
            ShowProductImages = _shoppingCartSettings.ShowProductImagesInMiniShoppingCart,
            //let's always display it
            DisplayShoppingCartButton = true,
            CurrentCustomerIsGuest = await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()),
            AnonymousCheckoutAllowed = _orderSettings.AnonymousCheckoutAllowed,
        };

        //performance optimization (use "HasShoppingCartItems" property)
        if ((await _workContext.GetCurrentCustomerAsync()).HasShoppingCartItems)
        {
            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), cartType, (await _storeContext.GetCurrentStoreAsync()).Id);

            if (cart.Any())
            {
                model.TotalProducts = cart.Sum(item => item.Quantity);
                if (cartType == ShoppingCartType.ShoppingCart)
                {
                    //subtotal
                    var subTotalIncludingTax = await _workContext.GetTaxDisplayTypeAsync() == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal;
                    var (_, _, _, subTotalWithoutDiscountBase, _) = await _orderTotalCalculationService.GetShoppingCartSubTotalAsync(cart, subTotalIncludingTax);
                    var subtotalBase = subTotalWithoutDiscountBase;
                    var subtotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(subtotalBase, await _workContext.GetWorkingCurrencyAsync());
                    model.SubTotal = await _priceFormatter.FormatPriceAsync(subtotal, false, await _workContext.GetWorkingCurrencyAsync(), (await _workContext.GetWorkingLanguageAsync()).Id, subTotalIncludingTax);

                    var requiresShipping = await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart);
                    //a customer should visit the shopping cart page (hide checkout button) before going to checkout if:
                    //1. "terms of service" are enabled
                    //2. min order sub-total is OK
                    //3. we have at least one checkout attribute
                    var checkoutAttributes =
             await _checkoutAttributeService.GetAllAttributesAsync(_staticCacheManager, _storeMappingService, store.Id, requiresShipping);
                    var checkoutAttributesExist = checkoutAttributes
                .Any();
                    var minOrderSubtotalAmountOk = await _orderProcessingService.ValidateMinOrderSubtotalAmountAsync(cart);

                    var cartProductIds = cart.Select(ci => ci.ProductId).ToArray();

                    var downloadableProductsRequireRegistration =
                        _customerSettings.RequireRegistrationForDownloadableProducts && await _productService.HasAnyDownloadableProductAsync(cartProductIds);

                    model.DisplayCheckoutButton = !_orderSettings.TermsOfServiceOnShoppingCartPage &&
                        minOrderSubtotalAmountOk &&
                        !checkoutAttributesExist &&
                        !(downloadableProductsRequireRegistration
                            && await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()));
                }

                //products. sort descending (recently added products)
                foreach (var sci in cart
                    .OrderByDescending(x => x.UpdatedOnUtc)
                    .Take(_shoppingCartSettings.MiniShoppingCartProductNumber)
                    .ToList())
                {
                    var product = await _productService.GetProductByIdAsync(sci.ProductId);

                    var cartItemModel = new MiniShoppingCartExtendedModel.ShoppingCartItemModel
                    {
                        Id = sci.Id,
                        ProductId = sci.ProductId,
                        ProductName = await _localizationService.GetLocalizedAsync(product, x => x.Name),
                        ProductSeName = await _urlRecordService.GetSeNameAsync(product),
                        Quantity = sci.Quantity,
                        AttributeInfo = await _customProductAttributeFormatter.CustomFormatAttributesAsync(product, sci.AttributesXml),
                        ParentGroupedProductId = product.ParentGroupedProductId
                    };
                    cartItemModel.Variant = await _productExtendedService.GetItemVariantInfo(sci.ProductId, sci.AttributesXml);
                    cartItemModel.ProductName = string.IsNullOrWhiteSpace(cartItemModel.Variant?.Title) ? cartItemModel.ProductName : cartItemModel.Variant.Title;
                    var allowedQuantities = _productService.ParseAllowedQuantities(product);

                    foreach (var qty in allowedQuantities)
                    {
                        cartItemModel.AllowedQuantities.Add(new SelectListItem
                        {
                            Text = qty.ToString(),
                            Value = qty.ToString(),
                            Selected = sci.Quantity == qty
                        });

                    }
                    //unit prices
                    if (product.CallForPrice &&
                        //also check whether the current user is impersonated
                        (!_orderSettings.AllowAdminsToBuyCallForPriceProducts || _workContext.OriginalCustomerIfImpersonated == null))
                    {
                        cartItemModel.UnitPrice = await _localizationService.GetResourceAsync("Products.CallForPrice");
                    }
                    else
                    {
                        var (shoppingCartUnitPriceWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, (await _shoppingCartService.GetUnitPriceAsync(sci, true)).unitPrice);
                        var shoppingCartUnitPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartUnitPriceWithDiscountBase, await _workContext.GetWorkingCurrencyAsync());
                        cartItemModel.UnitPrice = await _priceFormatter.FormatPriceAsync(shoppingCartUnitPriceWithDiscount);
                    }

                    //picture
                    if (_shoppingCartSettings.ShowProductImagesInMiniShoppingCart)
                    {
                        cartItemModel.Picture = await PrepareCartItemPictureModelAsync(sci,
                            _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage, true, cartItemModel.ProductName);
                    }

                    model.Items.Add(cartItemModel);
                }
            }
        }

        return model;
    }
    public virtual async Task<MiniShoppingCartExtendedModel> PrepareCustomFlyoutShoppingCartModelAsync(ShoppingCartType cartType)
    {
        var model = new MiniShoppingCartExtendedModel
        {
            ShowProductImages = _shoppingCartSettings.ShowProductImagesInMiniShoppingCart,
            //let's always display it
            DisplayShoppingCartButton = true,
            CurrentCustomerIsGuest = await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()),
            AnonymousCheckoutAllowed = _orderSettings.AnonymousCheckoutAllowed,
        };
        var store = await _storeContext.GetCurrentStoreAsync();

        //performance optimization (use "HasShoppingCartItems" property)
        if ((await _workContext.GetCurrentCustomerAsync()).HasShoppingCartItems)
        {
            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), cartType, (await _storeContext.GetCurrentStoreAsync()).Id);

            if (cart.Any())
            {
                model.TotalProducts = cart.Sum(item => item.Quantity);
                if (cartType == ShoppingCartType.ShoppingCart)
                {
                    //subtotal
                    var subTotalIncludingTax = await _workContext.GetTaxDisplayTypeAsync() == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal;
                    var (_, _, _, subTotalWithoutDiscountBase, _) = await _orderTotalCalculationService.GetShoppingCartSubTotalAsync(cart, subTotalIncludingTax);
                    var subtotalBase = subTotalWithoutDiscountBase;
                    var subtotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(subtotalBase, await _workContext.GetWorkingCurrencyAsync());
                    model.SubTotal = await _priceFormatter.FormatPriceAsync(subtotal, false, await _workContext.GetWorkingCurrencyAsync(), (await _workContext.GetWorkingLanguageAsync()).Id, subTotalIncludingTax);

                    var requiresShipping = await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart);

                    //a customer should visit the shopping cart page (hide checkout button) before going to checkout if:
                    //1. "terms of service" are enabled
                    //2. min order sub-total is OK
                    //3. we have at least one checkout attribute
                    var checkoutAttributesExist = (
             await _checkoutAttributeService.GetAllAttributesAsync(_staticCacheManager, _storeMappingService, store.Id, requiresShipping))
                        .Any();

                    var minOrderSubtotalAmountOk = await _orderProcessingService.ValidateMinOrderSubtotalAmountAsync(cart);

                    var cartProductIds = cart.Select(ci => ci.ProductId).ToArray();

                    var downloadableProductsRequireRegistration =
                        _customerSettings.RequireRegistrationForDownloadableProducts && await _productService.HasAnyDownloadableProductAsync(cartProductIds);

                    model.DisplayCheckoutButton = !_orderSettings.TermsOfServiceOnShoppingCartPage &&
                        minOrderSubtotalAmountOk &&
                        !checkoutAttributesExist &&
                        !(downloadableProductsRequireRegistration
                            && await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()));
                }

                //products. sort descending (recently added products)
                foreach (var sci in cart
                    .OrderByDescending(x => x.UpdatedOnUtc)
                    .Take(_shoppingCartSettings.MiniShoppingCartProductNumber)
                    .ToList())
                {
                    var product = await _productService.GetProductByIdAsync(sci.ProductId);

                    var cartItemModel = new MiniShoppingCartExtendedModel.ShoppingCartItemModel
                    {
                        Id = sci.Id,
                        ProductId = sci.ProductId,
                        ProductName = await _localizationService.GetLocalizedAsync(product, x => x.Name),
                        ProductSeName = await _urlRecordService.GetSeNameAsync(product),
                        Quantity = sci.Quantity,
                        AttributeInfo = await _customProductAttributeFormatter.CustomFormatAttributesAsync(product, sci.AttributesXml),
                        ParentGroupedProductId = product.ParentGroupedProductId
                    };
                    cartItemModel.Variant = await _productExtendedService.GetItemVariantInfo(sci.ProductId, sci.AttributesXml);
                    cartItemModel.ProductName = string.IsNullOrWhiteSpace(cartItemModel.Variant?.Title) ? cartItemModel.ProductName : cartItemModel.Variant.Title;
                    var allowedQuantities = _productService.ParseAllowedQuantities(product);

                    foreach (var qty in allowedQuantities)
                    {
                        cartItemModel.AllowedQuantities.Add(new SelectListItem
                        {
                            Text = qty.ToString(),
                            Value = qty.ToString(),
                            Selected = sci.Quantity == qty
                        });

                    }
                    //unit prices
                    if (product.CallForPrice &&
                        //also check whether the current user is impersonated
                        (!_orderSettings.AllowAdminsToBuyCallForPriceProducts || _workContext.OriginalCustomerIfImpersonated == null))
                    {
                        cartItemModel.UnitPrice = await _localizationService.GetResourceAsync("Products.CallForPrice");
                    }
                    else
                    {
                        var (shoppingCartUnitPriceWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, (await _shoppingCartService.GetUnitPriceAsync(sci, true)).unitPrice);
                        var shoppingCartUnitPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartUnitPriceWithDiscountBase, await _workContext.GetWorkingCurrencyAsync());
                        cartItemModel.UnitPrice = await _priceFormatter.FormatPriceAsync(shoppingCartUnitPriceWithDiscount);
                    }

                    //picture
                    if (_shoppingCartSettings.ShowProductImagesInMiniShoppingCart)
                    {
                        cartItemModel.Picture = await PrepareCartItemPictureModelAsync(sci,
                            _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage, true, cartItemModel.ProductName);
                    }
                    cartItemModel.PictureModels = await this.PrepareCartItemAttributePictureModelAsync(sci, cartItemModel.Variant?.VariantId ?? 0, _mediaSettings.ProductThumbPictureSize, cartItemModel.ProductName);
                    cartItemModel.EnableNewATCLayout = await this._productExtendedService.IsNewAtcLayoutActive(product);
                    model.Items.Add(cartItemModel);
                }
            }
        }

        return model;
    }
    public virtual async Task<MiniShoppingCartExtendedModel> PrepareWishlistModelAsync(int? lst)
    {
        var model = new MiniShoppingCartExtendedModel
        {
            ShowProductImages = _shoppingCartSettings.ShowProductImagesInMiniShoppingCart,
            //let's always display it
            DisplayShoppingCartButton = true,
            CurrentCustomerIsGuest = await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()),
            AnonymousCheckoutAllowed = _orderSettings.AnonymousCheckoutAllowed,
        };

        var store = await _storeContext.GetCurrentStoreAsync();
        var currency = await _workContext.GetWorkingCurrencyAsync();
        var customer = await _workContext.GetCurrentCustomerAsync();
        var language = await _workContext.GetWorkingLanguageAsync();
        //performance optimization (use "HasShoppingCartItems" property)
        if ((await _workContext.GetCurrentCustomerAsync()).HasShoppingCartItems)
        {
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.Wishlist, store.Id, customWishlistId: lst);

            if (cart.Any())
            {
                model.TotalProducts = cart.Sum(item => item.Quantity);

                //subtotal
                var subTotalIncludingTax = await _workContext.GetTaxDisplayTypeAsync() == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal;
                var (_, _, _, subTotalWithoutDiscountBase, _) = await _orderTotalCalculationService.GetShoppingCartSubTotalAsync(cart, subTotalIncludingTax);
                var subtotalBase = subTotalWithoutDiscountBase;
                var subtotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(subtotalBase, currency);
                model.SubTotal = await _priceFormatter.FormatPriceAsync(subtotal, false, currency, language.Id, subTotalIncludingTax);

                var requiresShipping = await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart);
                //a customer should visit the shopping cart page (hide checkout button) before going to checkout if:
                //1. "terms of service" are enabled
                //2. min order sub-total is OK
                //3. we have at least one checkout attribute
                var checkoutAttributesExist = (await _checkoutAttributeService.GetAllAttributesAsync(_staticCacheManager, _storeMappingService, store.Id, requiresShipping)).Any();


                var minOrderSubtotalAmountOk = await _orderProcessingService.ValidateMinOrderSubtotalAmountAsync(cart);

                var cartProductIds = cart.Select(ci => ci.ProductId).ToArray();

                var downloadableProductsRequireRegistration =
                    _customerSettings.RequireRegistrationForDownloadableProducts && await _productService.HasAnyDownloadableProductAsync(cartProductIds);

                model.DisplayCheckoutButton = !_orderSettings.TermsOfServiceOnShoppingCartPage &&
                    minOrderSubtotalAmountOk &&
                    !checkoutAttributesExist &&
                    !(downloadableProductsRequireRegistration
                        && await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()));

                //products. sort descending (recently added products)
                foreach (var sci in cart
                    .OrderByDescending(x => x.Id)
                    .Take(_shoppingCartSettings.MiniShoppingCartProductNumber)
                    .ToList())
                {
                    var product = await _productService.GetProductByIdAsync(sci.ProductId);

                    var cartItemModel = new MiniShoppingCartExtendedModel.ShoppingCartItemModel
                    {
                        Id = sci.Id,
                        ProductId = sci.ProductId,
                        ProductName = await _localizationService.GetLocalizedAsync(product, x => x.Name),
                        ProductSeName = await _urlRecordService.GetSeNameAsync(product),
                        Quantity = sci.Quantity,
                        AttributeInfo = await _customProductAttributeFormatter.CustomFormatAttributesAsync(product, sci.AttributesXml)
                    };
                    cartItemModel.Variant = await _productExtendedService.GetItemVariantInfo(sci.ProductId, sci.AttributesXml);
                    cartItemModel.ProductName = string.IsNullOrWhiteSpace(cartItemModel.Variant?.Title) ? cartItemModel.ProductName : cartItemModel.Variant.Title;
                    //unit prices
                    if (product.CallForPrice &&
                        //also check whether the current user is impersonated
                        (!_orderSettings.AllowAdminsToBuyCallForPriceProducts || _workContext.OriginalCustomerIfImpersonated == null))
                    {
                        cartItemModel.UnitPrice = await _localizationService.GetResourceAsync("Products.CallForPrice");
                    }
                    else
                    {
                        var (shoppingCartUnitPriceWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, (await _shoppingCartService.GetUnitPriceAsync(sci, true)).unitPrice);
                        var shoppingCartUnitPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartUnitPriceWithDiscountBase, await _workContext.GetWorkingCurrencyAsync());
                        cartItemModel.UnitPrice = await _priceFormatter.FormatPriceAsync(shoppingCartUnitPriceWithDiscount);
                    }

                    //picture
                    if (_shoppingCartSettings.ShowProductImagesInMiniShoppingCart)
                    {
                        cartItemModel.Picture = await PrepareCartItemPictureModelAsync(sci,
                            _mediaSettings.MiniCartThumbPictureSize, true, cartItemModel.ProductName);
                    }

                    model.Items.Add(cartItemModel);
                }
            }
        }

        return model;
    }
    public async Task<bool> IsWgsShippingMethodRequired(IList<ShoppingCartItem> cart)
    {

        bool wgsRequired = false;
        foreach (var item in cart)
        {
            string attributeDescription = await _customProductAttributeFormatter.CustomFormatAttributesAsync(await _productService.GetProductByIdAsync(item.ProductId), item.AttributesXml);
            int variantId = await _productExtendedService.GetVariantId(item.ProductId, attributeDescription);
            if (variantId > 0)
            {
                var variantCombination = await _productExtendedService.GetProductVariants(item.ProductId);
                if ((variantCombination.Where(v => v.VariantId == variantId).FirstOrDefault()?.WgsRequired ?? false))
                {
                    wgsRequired = true;
                    break;
                }
            }
        }
        return wgsRequired;
    }

    #region WishList

    public virtual async Task<WishlistExtendedModel> PrepareCustomWishlistModelAsync(WishlistExtendedModel model, IList<ShoppingCartItem> cart, bool isEditable = true, int? list = null)
    {
        ArgumentNullException.ThrowIfNull(cart);
        ArgumentNullException.ThrowIfNull(model);

        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        var isGuest = await _customerService.IsGuestAsync(currentCustomer);

        model.EmailWishlistEnabled = _shoppingCartSettings.EmailWishlistEnabled;
        model.ListId = list;
        model.AllowMultipleWishlist = _shoppingCartSettings.AllowMultipleWishlist && !isGuest;
        model.IsEditable = isEditable;
        model.DisplayAddToCart = await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.ENABLE_SHOPPING_CART);
        model.DisplayTaxShippingInfo = _catalogSettings.DisplayTaxShippingInfoWishlist;

        //custom wishlist items
        var currentWishlists = await _customWishlistService.GetAllCustomWishlistsAsync(currentCustomer.Id);
        foreach (var wishlist in currentWishlists)
        {
            var customWishlistModel = new CustomWishlistModel
            {
                Id = wishlist.Id,
                Name = wishlist.Name
            };
            model.CustomWishlistItems.Add(customWishlistModel);
        }

        if (list != null)
        {
            var customWishlist = await _customWishlistService.GetCustomWishlistByIdAsync(list.Value);
            if (customWishlist != null)
                model.CustomWishlistName = customWishlist.Name;
        }

        if (!cart.Any())
            return model;

        //simple properties
        var customer = await _customerService.GetShoppingCartCustomerAsync(cart);

        model.CustomerGuid = customer.CustomerGuid;
        model.CustomerFullname = await _customerService.GetCustomerFullNameAsync(customer);
        model.ShowProductImages = _shoppingCartSettings.ShowProductImagesOnWishList;
        model.ShowSku = _catalogSettings.ShowSkuOnProductDetailsPage;

        //cart warnings
        var cartWarnings = await _shoppingCartService.GetShoppingCartWarningsAsync(cart, string.Empty, false);
        foreach (var warning in cartWarnings)
            model.Warnings.Add(warning);

        //cart items
        foreach (var sci in cart)
        {
            var cartItemModel = await PrepareCustomWishlistItemModelAsync(sci);
            model.Items.Add(cartItemModel);
        }

        return model;
    }

    #endregion

    #endregion


    #region Utilities

    protected virtual async Task<IList<PictureModel>> PrepareCartItemAttributePictureModelAsync(ShoppingCartItem sci, int variantId, int pictureSize, string productName)
    {
        string paramVariantId = variantId.ToString();
        if (!string.IsNullOrEmpty(sci.AttributesXml))
        {
          
            paramVariantId += string.Join('-', (await _productAttributeParser
             .ParseProductAttributeValuesAsync(sci.AttributesXml)).Select(av => av.Id));

        }
        var pictureCacheKey = _shortTermCacheManager.PrepareKey(CustomNopModelCacheDefaults.CartGalleryPictureModelKey
            , sci.ProductId, paramVariantId, pictureSize, true, await _workContext.GetWorkingLanguageAsync(), _webHelper.IsCurrentConnectionSecured(), await _storeContext.GetCurrentStoreAsync());


        var model = await _staticCacheManager.GetAsync(pictureCacheKey, async () =>
        {
            var product = await _productService.GetProductByIdAsync(sci.ProductId);

            //shopping cart item picture
            var pictures = await _pictureExtendedService.GetProductAttributePicturesAsync(product, sci.AttributesXml);
            string fullSizeImageUrl, imageUrl, thumbImageUrl;
            var pictureModels = new List<PictureModel>();
            for (var i = 0; i < pictures.Count(); i++)
            {
                var picture = pictures[i];

                (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, pictureSize);
                (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                (thumbImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage);

                var pictureModel = new PictureModel
                {
                    ImageUrl = imageUrl,
                    ThumbImageUrl = thumbImageUrl,
                    FullSizeImageUrl = fullSizeImageUrl,
                    Title = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat.Details"), productName),
                    AlternateText = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat.Details"), productName),
                };
                //"title" attribute
                pictureModel.Title = !string.IsNullOrEmpty(picture.TitleAttribute) ?
                    picture.TitleAttribute :
                    string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat.Details"), productName);
                //"alt" attribute
                pictureModel.AlternateText = !string.IsNullOrEmpty(picture.AltAttribute) ?
                    picture.AltAttribute :
                    string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat.Details"), productName);

                pictureModels.Add(pictureModel);
            }
            return pictureModels;
        });

        return model;
    }
    protected virtual async Task<WishlistExtendedModel.ShoppingCartItemModel> PrepareCustomWishlistItemModelAsync(ShoppingCartItem sci)
    {
        if (sci == null)
            throw new ArgumentNullException(nameof(sci));

        var product = await _productService.GetProductByIdAsync(sci.ProductId);

        var cartItemModel = new WishlistExtendedModel.ShoppingCartItemModel
        {
            Id = sci.Id,
            Sku = await _productService.FormatSkuAsync(product, sci.AttributesXml),
            ProductId = product.Id,
            ProductName = await _localizationService.GetLocalizedAsync(product, x => x.Name),
            ProductSeName = await _urlRecordService.GetSeNameAsync(product),
            Quantity = sci.Quantity,
            AttributeInfo = await _productAttributeFormatter.FormatAttributesAsync(product, sci.AttributesXml),
        };
        cartItemModel.Variant = await _productExtendedService.GetItemVariantInfo(sci.ProductId, sci.AttributesXml);
        cartItemModel.ProductName = string.IsNullOrWhiteSpace(cartItemModel.Variant?.Title) ? cartItemModel.ProductName : cartItemModel.Variant.Title;

        //allow editing?
        //1. setting enabled?
        //2. simple product?
        //3. has attribute or gift card?
        //4. visible individually?
        cartItemModel.AllowItemEditing = _shoppingCartSettings.AllowCartItemEditing &&
                                         product.ProductType == ProductType.SimpleProduct &&
                                         (!string.IsNullOrEmpty(cartItemModel.AttributeInfo) ||
                                          product.IsGiftCard) &&
                                         product.VisibleIndividually;

        //allowed quantities
        var allowedQuantities = _productService.ParseAllowedQuantities(product);
        foreach (var qty in allowedQuantities)
        {
            cartItemModel.AllowedQuantities.Add(new SelectListItem
            {
                Text = qty.ToString(),
                Value = qty.ToString(),
                Selected = sci.Quantity == qty
            });
        }

        //recurring info
        if (product.IsRecurring)
            cartItemModel.RecurringInfo = string.Format(await _localizationService.GetResourceAsync("ShoppingCart.RecurringPeriod"),
                    product.RecurringCycleLength, await _localizationService.GetLocalizedEnumAsync(product.RecurringCyclePeriod));

        //rental info
        if (product.IsRental)
        {
            var rentalStartDate = sci.RentalStartDateUtc.HasValue
                ? _productService.FormatRentalDate(product, sci.RentalStartDateUtc.Value)
                : string.Empty;
            var rentalEndDate = sci.RentalEndDateUtc.HasValue
                ? _productService.FormatRentalDate(product, sci.RentalEndDateUtc.Value)
                : string.Empty;
            cartItemModel.RentalInfo =
                string.Format(await _localizationService.GetResourceAsync("ShoppingCart.Rental.FormattedDate"),
                    rentalStartDate, rentalEndDate);
        }

        //unit prices
        if (product.CallForPrice &&
            //also check whether the current user is impersonated
            (!_orderSettings.AllowAdminsToBuyCallForPriceProducts || _workContext.OriginalCustomerIfImpersonated == null))
        {
            cartItemModel.UnitPrice = await _localizationService.GetResourceAsync("Products.CallForPrice");
        }
        else
        {
            var (shoppingCartUnitPriceWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, (await _shoppingCartService.GetUnitPriceAsync(sci, true)).unitPrice);
            var shoppingCartUnitPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartUnitPriceWithDiscountBase, await _workContext.GetWorkingCurrencyAsync());
            cartItemModel.UnitPrice = await _priceFormatter.FormatPriceAsync(shoppingCartUnitPriceWithDiscount);
        }
        //subtotal, discount
        if (product.CallForPrice &&
            //also check whether the current user is impersonated
            (!_orderSettings.AllowAdminsToBuyCallForPriceProducts || _workContext.OriginalCustomerIfImpersonated == null))
        {
            cartItemModel.SubTotal = await _localizationService.GetResourceAsync("Products.CallForPrice");
        }
        else
        {
            //sub total
            var (subTotal, shoppingCartItemDiscountBase, _, maximumDiscountQty) = await _shoppingCartService.GetSubTotalAsync(sci, true);
            var (shoppingCartItemSubTotalWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, subTotal);
            var shoppingCartItemSubTotalWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartItemSubTotalWithDiscountBase, await _workContext.GetWorkingCurrencyAsync());
            cartItemModel.SubTotal = await _priceFormatter.FormatPriceAsync(shoppingCartItemSubTotalWithDiscount);
            cartItemModel.MaximumDiscountedQty = maximumDiscountQty;

            //display an applied discount amount
            if (shoppingCartItemDiscountBase > decimal.Zero)
            {
                (shoppingCartItemDiscountBase, _) = await _taxService.GetProductPriceAsync(product, shoppingCartItemDiscountBase);
                if (shoppingCartItemDiscountBase > decimal.Zero)
                {
                    var shoppingCartItemDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartItemDiscountBase, await _workContext.GetWorkingCurrencyAsync());
                    cartItemModel.Discount = await _priceFormatter.FormatPriceAsync(shoppingCartItemDiscount);
                }
            }
        }

        //picture
        if (_shoppingCartSettings.ShowProductImagesOnWishList)
        {
            cartItemModel.Picture = await PrepareCartItemPictureModelAsync(sci,
                _mediaSettings.CategoryThumbPictureSize, true, cartItemModel.ProductName);
        }
        var attrName = await _settingService.GetSettingAsync("Catalog.Product.Attribute.Shade.Name");


        //item warnings
        var itemWarnings = await _shoppingCartService.GetShoppingCartItemWarningsAsync(
            await _workContext.GetCurrentCustomerAsync(),
            sci.ShoppingCartType,
            product,
            sci.StoreId,
            sci.AttributesXml,
            sci.CustomerEnteredPrice,
            sci.RentalStartDateUtc,
            sci.RentalEndDateUtc,
            sci.Quantity,
            false,
            sci.Id);
        foreach (var warning in itemWarnings)
            cartItemModel.Warnings.Add(warning);

        return cartItemModel;
    }
    #endregion
}
