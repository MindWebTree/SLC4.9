using MWT.Nop.Core.Data.Discounts;
using MWT.Nop.Core.Domain.Orders;
using MWT.Nop.Core.Services.Catalog;
using MWT.Nop.Core.Services.Customers;
using MWT.Nop.Core.Services.Discounts;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Tax;
using Nop.Services.Attributes;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscountType = Nop.Core.Domain.Discounts.DiscountType;

namespace MWT.Nop.Core.Services.Orders
{
    public partial class OrderTotalCalculationExtendedService : OrderTotalCalculationService, IOrderTotalCalculationExtendedService
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly ICustomerExtendedService _customerExtendedService;
        private readonly ILogger _logger;
        private readonly ILocalizationService _localizationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IShoppingCartExtendedService _shoppingCartExtendedCartService;
        private readonly ICurrencyService _currencyService;
        private readonly IDiscountExtendedService _discountExtendedService;
        private readonly IPriceCalculationExtendedService _priceCalculationExtendedService;

        #endregion

        #region Ctor
        public OrderTotalCalculationExtendedService(CatalogSettings catalogSettings, IAddressService addressService, IAttributeParser<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeParser,
            ICustomerService customerService, IDiscountService discountService, IGenericAttributeService genericAttributeService, IGiftCardService giftCardService, IOrderService orderService,
            IPaymentService paymentService, IPriceCalculationService priceCalculationService, IProductService productService, IRewardPointService rewardPointService, IShippingPluginManager shippingPluginManager,
            IShippingService shippingService, IShoppingCartService shoppingCartService, IStoreContext storeContext, ITaxService taxService, IWorkContext workContext, RewardPointsSettings rewardPointsSettings,
            ShippingSettings shippingSettings, ShoppingCartSettings shoppingCartSettings, TaxSettings taxSettings, ISettingService settingService,
            ICustomerExtendedService customerExtendedService, ILogger logger, ILocalizationService localizationService, IPriceFormatter priceFormatter,
            IShoppingCartExtendedService shoppingCartExtendedCartService, ICurrencyService currencyService, IDiscountExtendedService discountExtendedService, IPriceCalculationExtendedService priceCalculationExtendedService)
            : base(catalogSettings, addressService, checkoutAttributeParser, customerService, discountService, genericAttributeService, giftCardService, orderService, paymentService, priceCalculationService, productService, rewardPointService, shippingPluginManager, shippingService, shoppingCartService, storeContext, taxService, workContext, rewardPointsSettings, shippingSettings, shoppingCartSettings, taxSettings)
        {
            _settingService = settingService;
            _customerExtendedService = customerExtendedService;
            _logger = logger;
            _localizationService = localizationService;
            _priceFormatter = priceFormatter;
            _shoppingCartExtendedCartService = shoppingCartExtendedCartService;
            _currencyService = currencyService;
            _discountExtendedService = discountExtendedService;
            _priceCalculationExtendedService = priceCalculationExtendedService;
        }

        #endregion

        #region Methods

        public virtual async Task<(decimal, decimal)> GetCustomDuty(IList<ShoppingCartItem> cart, bool isCustomOrder = false, Customer customOrderCustomer = null, decimal orderTotal = 0)
        {
            decimal customDuty = 0;
            decimal customDutyPercentage = 0;
            if (!isCustomOrder)
            {
                var customer = await _customerService.GetShoppingCartCustomerAsync(cart);
                if (customer != null && customer.ShippingAddressId.HasValue && customer.ShippingAddressId.Value > 0)
                {
                    var shippingAddress = await _addressService.GetAddressByIdAsync(customer.ShippingAddressId.Value);
                    if (shippingAddress.CountryId.HasValue)
                    {
                        customDutyPercentage = await _settingService.GetSettingByKeyAsync<decimal>("Custom-Duty-Percentage-" + shippingAddress.CountryId);
                        if (customDutyPercentage > 0)
                        {
                            #region Custom updates Need to shift with Upgrade
                            var (_, _, _, subTotalWithDiscountBase, _, _) = await this.GetCustomShoppingCartSubTotalAsync(cart, false);
                            #endregion
                            //subtotal with discount
                            decimal subTotal = subTotalWithDiscountBase;

                            #region Custom updates Need to shift with Upgrade

                            decimal membershipfee = 0;
                            decimal membershipfeeDiscount = 0;
                            (decimal buyMoreDiscount, decimal membershipDiscount, decimal offerDiscount, decimal offerDiscountDefault, decimal productItemsDiscount, _) = await this.GetCustomBuyMoreSaveMoreDiscountAndMemberShipDiscountAsync(cart);
                            if (await _customerExtendedService.IsMemberShipAddedInCart(customer))
                                (membershipfee, membershipfeeDiscount) = await this.GetMemberShipFee();

                            subTotal = (subTotal + membershipfee + (offerDiscountDefault - offerDiscount)) - buyMoreDiscount - membershipDiscount - membershipfeeDiscount - productItemsDiscount;

                            #endregion
                            customDuty = await _priceCalculationService.RoundPriceAsync((subTotal * customDutyPercentage / 100));
                        }

                    }
                }
            }
            else
            {
                if (customOrderCustomer != null && customOrderCustomer.ShippingAddressId.HasValue && customOrderCustomer.ShippingAddressId.Value > 0)
                {

                    var shippingAddress = await _addressService.GetAddressByIdAsync(customOrderCustomer.ShippingAddressId.Value);
                    if (shippingAddress.CountryId.HasValue)
                    {
                        customDutyPercentage = await _settingService.GetSettingByKeyAsync<decimal>("Custom-Duty-Percentage-" + shippingAddress.CountryId);
                        if (customDutyPercentage > 0)
                        {
                            customDuty = await _priceCalculationService.RoundPriceAsync((orderTotal * customDutyPercentage / 100));
                        }
                    }
                }
            }
            return (customDutyPercentage, customDuty);
        }

        public virtual async Task<(decimal, decimal)> GetMemberShipFee()
        {

            decimal memberShipFee = 0;
            decimal memberShipFeeDiscount = 0;
            var isMembershipEnabled = await _settingService.GetSettingByKeyAsync<bool>("ismembershipenabled");
            if (isMembershipEnabled)
            {

                memberShipFee = await _settingService.GetSettingByKeyAsync<decimal>("MemberShip.Fee");
                if (memberShipFee == 0)
                {
                    await _logger.InsertLogAsync(LogLevel.Error, "Membership Program: Setting missed  \"MemberShip.Fee\" for Membership Program", "Used 0 as Membership fees");
                }
                memberShipFeeDiscount = await _settingService.GetSettingByKeyAsync<decimal>("MemberShip.Discount");
                if (memberShipFeeDiscount == 0)
                {
                    await _logger.InsertLogAsync(LogLevel.Error, "Membership Program: Setting missed  \"MemberShip.Discount\" for Membership Program", "Used 0 as Membership Discount");
                }

                if (memberShipFeeDiscount > 0)
                    memberShipFeeDiscount = ((memberShipFee * (memberShipFeeDiscount > 100 ? 100 : memberShipFeeDiscount)) / 100);
            }
            return (memberShipFee, memberShipFeeDiscount);
        }

        public virtual async Task<(decimal, decimal, decimal, decimal, decimal, List<DiscountSummary>)>
            GetCustomBuyMoreSaveMoreDiscountAndMemberShipDiscountAsync(IList<ShoppingCartItem> cart, bool isOrderTotalpassed = false
            , decimal subTotalIncTax = 0,
            decimal subTotalExcTax = 0)
        {

            List<DiscountSummary> discountSummary = new List<DiscountSummary>();
            DiscountSummary memberShipDiscountSummary = new DiscountSummary();
            DiscountSummary OfferDiscountSummary = new DiscountSummary();
            DiscountSummary buyMoreSaveMoreDiscountSummary = new DiscountSummary();
            DiscountSummary productCouponDiscountSummary = new DiscountSummary();
            buyMoreSaveMoreDiscountSummary.Heading = await _localizationService.GetResourceAsync("OrderSummary.BuyMoreSaveMoreDiscount.heading");
            buyMoreSaveMoreDiscountSummary.Type = MWT.Nop.Core.Domain.Orders.DiscountType.BuyMoreSaveMoreDiscount;
            memberShipDiscountSummary.Heading = await _localizationService.GetResourceAsync("OrderSummary.MemberShipDiscount.heading");
            memberShipDiscountSummary.Type = MWT.Nop.Core.Domain.Orders.DiscountType.MemberShipDiscount;
            OfferDiscountSummary.Heading = await _localizationService.GetResourceAsync("OrderSummary.OfferDiscount.heading");
            OfferDiscountSummary.Type = MWT.Nop.Core.Domain.Orders.DiscountType.OfferDiscount;
            productCouponDiscountSummary.Heading = await _localizationService.GetResourceAsync("OrderSummary.Coupon.Discount.heading");
            productCouponDiscountSummary.Type = MWT.Nop.Core.Domain.Orders.DiscountType.ProductCouponDiscount;

            var customer = await _customerService.GetShoppingCartCustomerAsync(cart);
            var store = await _storeContext.GetCurrentStoreAsync();
            decimal buyMoreSaveMoreDiscount = 0;
            decimal membershipDiscountTotal = 0;
            decimal itemMembershipDiscountTotal = 0;
            decimal buyMoreDiscount = 0;
            decimal itemBuyMoreDiscount = 0;
            decimal offerDiscount = 0;
            decimal itemOfferDiscount = 0;
            decimal offerDiscountDefault = 0;
            decimal productItemsDiscount = 0;
            int notElgibleForSavemoreDiscountCartId = 0;
            CustomDiscountType discountType = CustomDiscountType.Fixed;

            bool isBuyMoreSaveMoreEnabled = await _settingService.GetSettingByKeyAsync<bool>("MarketingSettings.EnableBuyMoreSaveMoreDiscount");
            decimal buyMoreSaveMoreSingleItemThreshold = await _settingService.GetSettingByKeyAsync<decimal>("MarketingSettings.ApplyBuyMoreSaveMoreOnSingleItemOverThreshold");

            var currency = await _workContext.GetWorkingCurrencyAsync();
            if (isBuyMoreSaveMoreEnabled && (cart.Count > 1 || cart.Sum(c => c.Quantity) > 1))
            {
                decimal minTotal = 0;
                decimal maxTotal = 0;
                decimal orderSubTotalDiscountAmountBase = 0;
                decimal subTotal = 0;
                var subTotalIncludingTax = await _workContext.GetTaxDisplayTypeAsync() == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal;
                if (isOrderTotalpassed)
                    subTotal = subTotalIncludingTax ? subTotalIncTax : subTotalExcTax;
                else
                    (orderSubTotalDiscountAmountBase, _, subTotal, _, _)
                       =
                       await GetShoppingCartSubTotalAsync(cart, subTotalIncludingTax);

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
                foreach (var sci in cart)
                {
                    itemPrice = (await _shoppingCartService.GetUnitPriceAsync(sci, false)).unitPrice;
                    if (((itemPrice * sci.Quantity) > costlyItemPrice && cart.Count > 1) ||
                        (cart.Count == 1 && (cart.Sum(c => c.Quantity) == 1 || itemPrice < buyMoreSaveMoreSingleItemThreshold)))
                    {

                        costlyItemPrice = itemPrice * sci.Quantity;
                        notElgibleForSavemoreDiscountCartId = sci.Id;
                    }
                }
            }
            var isCustomerElgibleForMembershipPrice = await _customerExtendedService.IsCustomerEligibleForMemberShipDiscount(customer);
            foreach (var sci in cart)
            {
                itemBuyMoreDiscount = 0;
                itemOfferDiscount = 0;
                itemMembershipDiscountTotal = 0;
                var product = await _productService.GetProductByIdAsync(sci.ProductId);
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
                decimal memberShipDiscount = 0;
                decimal _memberShipPrice = 0;
                if (isCustomerElgibleForMembershipPrice && !product.CallForPrice)
                {
                    decimal discountPercent = 0;
                    (_memberShipPrice, discountPercent) = await _shoppingCartExtendedCartService.MemberShipPriceOfProduct(product.Id, _msrp, _oldprice, _unitPrice);
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
                        memberShipDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(memberShipDiscount, currency);
                        memberShipDiscountSummary.Summary += string.Format(
                                              await _localizationService.GetResourceAsync("OrderSummary.MemberShipDiscount.Summary"),
                                              product.Sku,
                                             "-" + await _priceFormatter.FormatPriceAsync(
                                                 await _priceCalculationService.RoundPriceAsync(memberShipDiscount * sci.Quantity, await _workContext.GetWorkingCurrencyAsync())), discountPercent.ToString("F2")
                                              );

                        #endregion
                    }
                    _memberShipPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(_memberShipPrice, currency);
                }

                #endregion

                decimal shoppingCartItemPriceWithDiscount = 0;
                decimal shoppingCartUnitPriceWithDiscount = 0;
                if (!product.CallForPrice)
                {
                    shoppingCartItemPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(itemPrice, currency);
                    shoppingCartUnitPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(_unitPrice, currency);

                }
                if (!product.CallForPrice && sci.Id != notElgibleForSavemoreDiscountCartId && buyMoreDiscount > 0)
                {
                    if (string.IsNullOrEmpty(buyMoreSaveMoreDiscountSummary.Description))
                        buyMoreSaveMoreDiscountSummary.Description =
                           string.Format(
                            await _localizationService.GetResourceAsync(discountType == CustomDiscountType.Percent ? "OrderSummary.BuyMoreSaveMoreDiscount.Subheading.Percent.Discount" :
                            "OrderSummary.BuyMoreSaveMoreDiscount.Subheading.Fixed.Discount"
                            ), Math.Round(buyMoreDiscount, 2));

                    if (cart.Count > 1)
                    {
                        itemBuyMoreDiscount = await _priceCalculationService.RoundPriceAsync(
                            discountType == CustomDiscountType.Percent ?
                            Math.Round(((shoppingCartItemPriceWithDiscount * sci.Quantity) * buyMoreDiscount / 100), 2) :
                          (buyMoreDiscount > shoppingCartItemPriceWithDiscount ? shoppingCartItemPriceWithDiscount * sci.Quantity : buyMoreDiscount * sci.Quantity),
                            currency);
                        buyMoreSaveMoreDiscount += itemBuyMoreDiscount;
                        buyMoreSaveMoreDiscountSummary.Summary += string.Format(
                            await _localizationService.GetResourceAsync("OrderSummary.BuyMoreSaveMoreDiscount.Summary"),
                            product.Sku,
                           "-" + await _priceFormatter.FormatPriceAsync(
                               itemBuyMoreDiscount)
                            );
                    }
                    else
                    {
                        itemBuyMoreDiscount = await _priceCalculationService.RoundPriceAsync(
                          discountType == CustomDiscountType.Percent ?
                          Math.Round(((shoppingCartItemPriceWithDiscount * (sci.Quantity - 1)) * buyMoreDiscount / 100), 2) :
                        (buyMoreDiscount > shoppingCartItemPriceWithDiscount ? shoppingCartItemPriceWithDiscount * (sci.Quantity - 1) : buyMoreDiscount * (sci.Quantity - 1)),
                          currency);
                        buyMoreSaveMoreDiscount += itemBuyMoreDiscount;
                        buyMoreSaveMoreDiscountSummary.Summary += string.Format(
                            await _localizationService.GetResourceAsync("OrderSummary.BuyMoreSaveMoreDiscount.Summary"),
                            product.Sku,
                           "-" + await _priceFormatter.FormatPriceAsync(
                               itemBuyMoreDiscount)
                            );
                    }
                }

                #region Offer Discount

                if (!product.CallForPrice)
                {
                    if (_oldprice > _unitPrice)
                    {
                        _oldprice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(_oldprice, await _workContext.GetWorkingCurrencyAsync());
                        if (isCustomerElgibleForMembershipPrice && _memberShipPrice > 0)
                        {

                            itemOfferDiscount = await _priceCalculationService.RoundPriceAsync(
                                sci.Quantity * (_memberShipPrice - shoppingCartItemPriceWithDiscount), currency);
                            offerDiscount += itemOfferDiscount;
                        }
                        else
                        {
                            itemOfferDiscount = await _priceCalculationService.RoundPriceAsync(
                                sci.Quantity * (_oldprice - shoppingCartItemPriceWithDiscount), currency);
                            offerDiscount += itemOfferDiscount;
                        }

                        OfferDiscountSummary.Summary += string.Format(
                       await _localizationService.GetResourceAsync("OrderSummary.OfferDiscount.Summary"),
                       product.Sku,
                      "-" + await _priceFormatter.FormatPriceAsync(itemOfferDiscount),
                      (100 - ((shoppingCartUnitPriceWithDiscount / _oldprice)
                      * 100)).ToString("F2")
                       );

                        offerDiscountDefault += sci.Quantity * (_oldprice - shoppingCartUnitPriceWithDiscount);
                    }
                }

                #endregion


                itemMembershipDiscountTotal = await _priceCalculationService.RoundPriceAsync(memberShipDiscount * sci.Quantity, currency);
                membershipDiscountTotal += itemMembershipDiscountTotal;

                var UnitPriceValue =
                     await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(_unitPrice, currency);
                if (_oldprice > await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(_unitPrice, currency))
                    UnitPriceValue = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(_oldprice, currency);

                decimal subTotalIncludeDiscount = (
      await _priceCalculationService.RoundPriceAsync(UnitPriceValue, currency) * sci.Quantity)
      - (await _priceCalculationService.RoundPriceAsync
      (itemBuyMoreDiscount, currency)
       + await _priceCalculationService.RoundPriceAsync(itemOfferDiscount, currency)
       + await _priceCalculationService.RoundPriceAsync(itemMembershipDiscountTotal, currency));

                (_, decimal appliedDiscountAmount, List<Discount> appliedDiscounts, List<decimal> discountAmountsApplied) = await _priceCalculationExtendedService.GetItemDiscount(product, customer, subTotalIncludeDiscount, decimal.Zero,
                       sci.Quantity, sci.RentalStartDateUtc, sci.RentalEndDateUtc);
                productItemsDiscount += appliedDiscountAmount;
                if (appliedDiscountAmount > 0)
                {
                    var counter = 0;
                    foreach (var discount in appliedDiscounts)
                    {
                        productCouponDiscountSummary.Summary += string.Format(
                     await _localizationService.GetResourceAsync(
                      discount.UsePercentage ?
                         "OrderSummary.Coupon.Discount.Summary.percent"
                         : "OrderSummary.Coupon.Discount.Summary.fixed"
                         ),
                     discount.CouponCode,
                     product.Sku,
                           Convert.ToDouble(discount.UsePercentage ? discount.DiscountPercentage : discount.DiscountAmount).ToString("F2"),
                      "-" + await _priceFormatter.FormatPriceAsync(discountAmountsApplied[counter])
                     );
                    }

                    counter++;
                }
            }

            if (!string.IsNullOrEmpty(memberShipDiscountSummary.Summary))
                discountSummary.Add(memberShipDiscountSummary);
            if (!string.IsNullOrEmpty(OfferDiscountSummary.Summary))
                discountSummary.Add(OfferDiscountSummary);
            if (!string.IsNullOrEmpty(buyMoreSaveMoreDiscountSummary.Summary))
                discountSummary.Add(buyMoreSaveMoreDiscountSummary);
            if (!string.IsNullOrEmpty(productCouponDiscountSummary.Summary))
                discountSummary.Add(productCouponDiscountSummary);
            return (await _priceCalculationService.RoundPriceAsync(buyMoreSaveMoreDiscount, await _workContext.GetWorkingCurrencyAsync()),
               await _priceCalculationService.RoundPriceAsync(membershipDiscountTotal, await _workContext.GetWorkingCurrencyAsync()),
               await _priceCalculationService.RoundPriceAsync(offerDiscount, await _workContext.GetWorkingCurrencyAsync()),
               await _priceCalculationService.RoundPriceAsync(offerDiscountDefault), await _priceCalculationService.RoundPriceAsync(productItemsDiscount), discountSummary);
        }

        public virtual async Task<(decimal? shippingTotal, decimal? additionalFee)> GetCustomShoppingCartShippingTotalAsync(IList<ShoppingCartItem> cart)
        {
            var includingTax = await _workContext.GetTaxDisplayTypeAsync() == TaxDisplayType.IncludingTax;
            var shippingInfo = (await GetCustomShoppingCartShippingTotalAsync(cart, includingTax));
            return (shippingInfo.shippingTotal, shippingInfo.additionalFee);
        }
        public virtual async Task<(decimal? shippingTotal, decimal taxRate, decimal? additionalFee, List<Discount> appliedDiscounts)> GetCustomShoppingCartShippingTotalAsync(IList<ShoppingCartItem> cart, bool includingTax)
        {
            decimal? shippingTotal = null;
            decimal additionalFee = 0;
            var appliedDiscounts = new List<Discount>();
            var taxRate = decimal.Zero;

            var customer = await _customerService.GetShoppingCartCustomerAsync(cart);
            var store = await _storeContext.GetCurrentStoreAsync();
            var isFreeShipping = await IsFreeShippingAsync(cart);
            if (isFreeShipping)
                return (decimal.Zero, taxRate, decimal.Zero, appliedDiscounts);

            ShippingOption shippingOption = null;
            if (customer != null)
                shippingOption = await _genericAttributeService.GetAttributeAsync<ShippingOption>(customer, NopCustomerDefaults.SelectedShippingOptionAttribute, store.Id);

            if (shippingOption != null)
            {
                //use last shipping option (get from cache)
                (shippingTotal, appliedDiscounts) = await AdjustShippingRateAsync(shippingOption.Rate, cart, shippingOption.IsPickupInStore);
                if (Math.Round((shippingTotal == null ? 0 : Convert.ToDecimal(shippingTotal)), 2) == Math.Round(shippingOption.Rate, 2) && shippingOption.AdditionalFee > 0)
                {
                    shippingTotal = shippingTotal - shippingOption.AdditionalFee;
                    additionalFee = shippingOption.AdditionalFee;
                }
                else
                {
                    decimal _shippingTotal = Math.Round((shippingTotal == null ? 0 : Convert.ToDecimal(shippingTotal)), 2);
                    if (shippingOption.AdditionalFee > 0 && _shippingTotal > 0)
                        if (_shippingTotal > Math.Round(shippingOption.Rate, 2))
                        {
                            shippingTotal = shippingTotal - shippingOption.AdditionalFee;
                            additionalFee = shippingOption.AdditionalFee;
                        }
                        else
                        {
                            decimal shippingDiscountPercentage = Math.Round((100 - ((_shippingTotal / (shippingOption.Rate) * 100))), 2);
                            _shippingTotal = (shippingOption.Rate - shippingOption.AdditionalFee);
                            _shippingTotal = Math.Round(_shippingTotal - ((_shippingTotal * shippingDiscountPercentage) / 100), 2);
                            shippingTotal = _shippingTotal;
                            additionalFee = Math.Round(shippingOption.AdditionalFee - ((shippingOption.AdditionalFee * shippingDiscountPercentage) / 100), 2);
                        }
                }
            }
            else
            {
                //use fixed rate (if possible)
                Address shippingAddress = null;
                if (customer != null)
                    shippingAddress = await _customerService.GetCustomerShippingAddressAsync(customer);

                var shippingRateComputationMethods = await _shippingPluginManager.LoadActivePluginsAsync(customer, store.Id);
                if (!shippingRateComputationMethods.Any() && !_shippingSettings.AllowPickupInStore)
                    throw new NopException("Shipping rate computation method could not be loaded");

                if (shippingRateComputationMethods.Count == 1)
                {
                    var shippingRateComputationMethod = shippingRateComputationMethods[0];

                    var shippingOptionRequests = (await _shippingService.CreateShippingOptionRequestsAsync(cart,
                        shippingAddress,
                        store.Id)).shipmentPackages;

                    decimal? fixedRate = null;
                    foreach (var shippingOptionRequest in shippingOptionRequests)
                    {
                        //calculate fixed rates for each request-package
                        var fixedRateTmp = await shippingRateComputationMethod.GetFixedRateAsync(shippingOptionRequest);
                        if (!fixedRateTmp.HasValue)
                            continue;

                        if (!fixedRate.HasValue)
                            fixedRate = decimal.Zero;

                        fixedRate += fixedRateTmp.Value;
                    }

                    if (fixedRate.HasValue)
                    {
                        //adjust shipping rate
                        (shippingTotal, appliedDiscounts) = await AdjustShippingRateAsync(fixedRate.Value, cart);
                    }
                }
            }

            if (!shippingTotal.HasValue)
                return (null, taxRate, null, appliedDiscounts);

            if (shippingTotal.Value < decimal.Zero)
                shippingTotal = decimal.Zero;

            //round
            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                shippingTotal = await _priceCalculationService.RoundPriceAsync(shippingTotal.Value);

            decimal? shippingTotalTaxed;

            (shippingTotalTaxed, taxRate) = await _taxService.GetShippingPriceAsync(shippingTotal.Value,
                includingTax,
                customer);

            //round
            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                shippingTotalTaxed = await _priceCalculationService.RoundPriceAsync(shippingTotalTaxed.Value);

            return (shippingTotalTaxed, taxRate, additionalFee, appliedDiscounts);
        }
        public virtual async Task<decimal> GetCustomShoppingCartSubTotalAsync(IList<ShoppingCartItem> cart)
        {
            var customer = await _customerService.GetShoppingCartCustomerAsync(cart);
            var store = await _storeContext.GetCurrentStoreAsync();
            var subTotalWithoutDiscount = decimal.Zero;
            if (!cart.Any())
                return subTotalWithoutDiscount;

            foreach (var sci in cart)
            {
                var product = await _productService.GetProductByIdAsync(sci.ProductId);
                var (_unitPrice, _oldprice, _msrp, _, _) = await _shoppingCartExtendedCartService.GetCustomUnitPriceAsync(product,
              customer, store,
              sci.ShoppingCartType,
              1, sci.AttributesXml, 0,
              sci.RentalStartDateUtc, sci.RentalEndDateUtc, false);
                var (shoppingCartUnitPriceWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, _unitPrice);
                var shoppingCartUnitPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartUnitPriceWithDiscountBase, await _workContext.GetWorkingCurrencyAsync());
                if (_oldprice > 0)
                {
                    (_oldprice, _) = await _taxService.GetProductPriceAsync(product, _oldprice);
                    _oldprice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(_oldprice, await _workContext.GetWorkingCurrencyAsync());
                }
                if (_oldprice > shoppingCartUnitPriceWithDiscount)
                    subTotalWithoutDiscount = subTotalWithoutDiscount + (_oldprice * sci.Quantity);
                else
                    subTotalWithoutDiscount = subTotalWithoutDiscount + (shoppingCartUnitPriceWithDiscount * sci.Quantity);

            }
            return subTotalWithoutDiscount;
        }

        public virtual async Task<(decimal? shoppingCartTotal, decimal discountAmount, List<Discount> appliedDiscounts, List<AppliedGiftCard> appliedGiftCards, int redeemedRewardPoints,
            decimal redeemedRewardPointsAmount)> GetCustomShoppingCartTotalAsync(IList<ShoppingCartItem> cart,
        bool? useRewardPoints = null, bool usePaymentMethodAdditionalFee = true)
        {
            var redeemedRewardPoints = 0;
            var redeemedRewardPointsAmount = decimal.Zero;

            var customer = await _customerService.GetShoppingCartCustomerAsync(cart);
            var store = await _storeContext.GetCurrentStoreAsync();
            var paymentMethodSystemName = string.Empty;
            if (customer != null)
            {
                paymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(customer,
                    NopCustomerDefaults.SelectedPaymentMethodAttribute, store.Id);
            }

            //subtotal without tax
            var (_, _, _, subTotalWithDiscountBase, _) = await GetShoppingCartSubTotalAsync(cart, false);
            //subtotal with discount
            var subtotalBase = subTotalWithDiscountBase;

            //shipping without tax
            var shoppingCartShipping = (await GetShoppingCartShippingTotalAsync(cart, false)).shippingTotal;

            //payment method additional fee without tax
            var paymentMethodAdditionalFeeWithoutTax = decimal.Zero;
            if (usePaymentMethodAdditionalFee && !string.IsNullOrEmpty(paymentMethodSystemName))
            {
                var paymentMethodAdditionalFee = await _paymentService.GetAdditionalHandlingFeeAsync(cart,
                    paymentMethodSystemName);
                paymentMethodAdditionalFeeWithoutTax =
                    (await _taxService.GetPaymentMethodAdditionalFeeAsync(paymentMethodAdditionalFee,
                        false, customer)).price;
            }

            //tax
            var shoppingCartTax = (await GetTaxTotalAsync(cart, usePaymentMethodAdditionalFee)).taxTotal;

            //order total
            var resultTemp = decimal.Zero;
            resultTemp += subtotalBase;
            if (shoppingCartShipping.HasValue)
            {
                resultTemp += shoppingCartShipping.Value;
            }

            resultTemp += paymentMethodAdditionalFeeWithoutTax;
            resultTemp += shoppingCartTax;
            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                resultTemp = await _priceCalculationService.RoundPriceAsync(resultTemp);

            //order total discount
            var (discountAmount, appliedDiscounts) = await GetOrderTotalDiscountAsync(customer, resultTemp);

            //sub totals with discount        
            if (resultTemp < discountAmount)
                discountAmount = resultTemp;

            //reduce subtotal
            resultTemp -= discountAmount;

            if (resultTemp < decimal.Zero)
                resultTemp = decimal.Zero;
            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                resultTemp = await _priceCalculationService.RoundPriceAsync(resultTemp);

            //let's apply gift cards now (gift cards that can be used)
            var appliedGiftCards = new List<AppliedGiftCard>();
            resultTemp = await AppliedGiftCardsAsync(cart, appliedGiftCards, customer, resultTemp);

            if (resultTemp < decimal.Zero)
                resultTemp = decimal.Zero;
            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                resultTemp = await _priceCalculationService.RoundPriceAsync(resultTemp);

            if (!shoppingCartShipping.HasValue)
            {
                //we have errors
                return (null, discountAmount, appliedDiscounts, appliedGiftCards, redeemedRewardPoints, redeemedRewardPointsAmount);
            }


            var orderTotal = resultTemp;

            //reward points
            (redeemedRewardPoints, redeemedRewardPointsAmount) = await SetRewardPointsAsync(redeemedRewardPoints, redeemedRewardPointsAmount, useRewardPoints, customer, orderTotal);

            orderTotal -= redeemedRewardPointsAmount;

            if (_shoppingCartSettings.RoundPricesDuringCalculation)
            {
                #region Custom updates Need to shift with Upgrade
                decimal membershipfee = 0;
                decimal membershipfeeDiscount = 0;
                (decimal buyMoreDiscount, decimal membershipDiscount, decimal offerDiscount, decimal offerDiscountDefault, decimal productItemsDiscount, _) = await GetCustomBuyMoreSaveMoreDiscountAndMemberShipDiscountAsync(cart);
                if (await _customerExtendedService.IsMemberShipAddedInCart(customer))
                    (membershipfee, membershipfeeDiscount) = await GetMemberShipFee();

                orderTotal = (orderTotal + membershipfee + (offerDiscountDefault - offerDiscount)) - buyMoreDiscount - membershipDiscount - membershipfeeDiscount;

                #endregion

                orderTotal = await _priceCalculationService.RoundPriceAsync(orderTotal);
            }
            return (orderTotal, discountAmount, appliedDiscounts, appliedGiftCards, redeemedRewardPoints, redeemedRewardPointsAmount);
        }

        public virtual async Task<(decimal discountAmount, List<Discount> appliedDiscounts, decimal subTotalWithoutDiscount, decimal subTotalWithDiscount, SortedDictionary<decimal, decimal> taxRates, List<decimal> discountAmountsApplied)> GetCustomShoppingCartSubTotalAsync(IList<ShoppingCartItem> cart,
          bool includingTax)
        {
            List<decimal> discountAmountsApplied = new List<decimal>();
            var discountAmount = decimal.Zero;
            var appliedDiscounts = new List<Discount>();
            var subTotalWithoutDiscount = decimal.Zero;
            var subTotalWithDiscount = decimal.Zero;
            var taxRates = new SortedDictionary<decimal, decimal>();

            if (!cart.Any())
                return (discountAmount, appliedDiscounts, subTotalWithoutDiscount, subTotalWithDiscount, taxRates, discountAmountsApplied);

            //get the customer 
            var customer = await _customerService.GetShoppingCartCustomerAsync(cart);
            var store = await _storeContext.GetCurrentStoreAsync();

            //sub totals
            var subTotalExclTaxWithoutDiscount = decimal.Zero;
            var subTotalInclTaxWithoutDiscount = decimal.Zero;
            foreach (var shoppingCartItem in cart)
            {
                var sciSubTotal = (await _shoppingCartService.GetSubTotalAsync(shoppingCartItem, false)).subTotal;
                var product = await _productService.GetProductByIdAsync(shoppingCartItem.ProductId);

                var (sciExclTax, taxRate) = await _taxService.GetProductPriceAsync(product, sciSubTotal, false, customer);
                var (sciInclTax, _) = await _taxService.GetProductPriceAsync(product, sciSubTotal, true, customer);
                subTotalExclTaxWithoutDiscount += sciExclTax;
                subTotalInclTaxWithoutDiscount += sciInclTax;

                //tax rates
                var sciTax = sciInclTax - sciExclTax;
                if (taxRate <= decimal.Zero || sciTax <= decimal.Zero)
                    continue;

                if (!taxRates.ContainsKey(taxRate))
                {
                    taxRates.Add(taxRate, sciTax);
                }
                else
                {
                    taxRates[taxRate] = taxRates[taxRate] + sciTax;
                }
            }

            //checkout attributes
            if (customer != null)
            {
                var checkoutAttributesXml = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.CheckoutAttributes, store.Id);
                var attributeValues = _checkoutAttributeParser.ParseAttributeValues(checkoutAttributesXml);
                if (attributeValues != null)
                {
                    await foreach (var (attribute, values) in attributeValues)
                    {
                        await foreach (var attributeValue in values)
                        {
                            var (caExclTax, taxRate) = await _taxService.GetCheckoutAttributePriceAsync(attribute, attributeValue, false, customer);
                            var (caInclTax, _) = await _taxService.GetCheckoutAttributePriceAsync(attribute, attributeValue, true, customer);

                            subTotalExclTaxWithoutDiscount += caExclTax;
                            subTotalInclTaxWithoutDiscount += caInclTax;

                            //tax rates
                            var caTax = caInclTax - caExclTax;
                            if (taxRate <= decimal.Zero || caTax <= decimal.Zero)
                                continue;

                            if (!taxRates.ContainsKey(taxRate))
                            {
                                taxRates.Add(taxRate, caTax);
                            }
                            else
                            {
                                taxRates[taxRate] = taxRates[taxRate] + caTax;
                            }
                        }
                    }
                }
            }

            //subtotal without discount
            subTotalWithoutDiscount = includingTax ? subTotalInclTaxWithoutDiscount : subTotalExclTaxWithoutDiscount;


            if (subTotalWithoutDiscount < decimal.Zero)
                subTotalWithoutDiscount = decimal.Zero;

            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                subTotalWithoutDiscount = await _priceCalculationService.RoundPriceAsync(subTotalWithoutDiscount);

            //We calculate discount amount on order subtotal excl tax (discount first)
            //calculate discount amount ('Applied to order subtotal' discount)
            decimal discountAmountExclTax;

            #region Custom updates Need to shift with Upgrade

            (decimal buyMoreSaveMoreDiscount, decimal membershipdiscount, decimal offerDiscount, decimal offerDiscountDefault, decimal productItemsDiscount, _) =
                await this.GetCustomBuyMoreSaveMoreDiscountAndMemberShipDiscountAsync(cart, true, subTotalInclTaxWithoutDiscount, subTotalExclTaxWithoutDiscount);
            #region Membership fees

            decimal memberShipFee = 0;
            decimal memberShipFeeDiscount = 0;
            if (await _customerExtendedService.IsMemberShipAddedInCart(customer))
                (memberShipFee, memberShipFeeDiscount) = await this.GetMemberShipFee();

            #endregion

            #endregion

            (discountAmountExclTax, appliedDiscounts, discountAmountsApplied) = await GetCustomOrderSubtotalDiscountAsync(customer,
                     (subTotalExclTaxWithoutDiscount +
                     memberShipFee + (offerDiscountDefault - offerDiscount))
                     - memberShipFeeDiscount
                     - buyMoreSaveMoreDiscount
                     - membershipdiscount
                     - productItemsDiscount
                     );
            if (subTotalExclTaxWithoutDiscount < discountAmountExclTax)
                discountAmountExclTax = subTotalExclTaxWithoutDiscount;
            var discountAmountInclTax = discountAmountExclTax;
            //subtotal with discount (excl tax)
            var subTotalExclTaxWithDiscount = subTotalExclTaxWithoutDiscount - discountAmountExclTax;
            var subTotalInclTaxWithDiscount = subTotalExclTaxWithDiscount;

            //add tax for shopping items & checkout attributes
            var tempTaxRates = new Dictionary<decimal, decimal>(taxRates);
            foreach (var kvp in tempTaxRates)
            {
                var taxRate = kvp.Key;
                var taxValue = kvp.Value;

                if (taxValue == decimal.Zero)
                    continue;

                //discount the tax amount that applies to subtotal items
                if (subTotalExclTaxWithoutDiscount > decimal.Zero)
                {
                    var discountTax = taxRates[taxRate] * (discountAmountExclTax / subTotalExclTaxWithoutDiscount);
                    discountAmountInclTax += discountTax;
                    taxValue = taxRates[taxRate] - discountTax;
                    if (_shoppingCartSettings.RoundPricesDuringCalculation)
                        taxValue = await _priceCalculationService.RoundPriceAsync(taxValue);
                    taxRates[taxRate] = taxValue;
                }

                //subtotal with discount (incl tax)
                subTotalInclTaxWithDiscount += taxValue;
            }

            if (_shoppingCartSettings.RoundPricesDuringCalculation)
            {
                discountAmountInclTax = await _priceCalculationService.RoundPriceAsync(discountAmountInclTax);
                discountAmountExclTax = await _priceCalculationService.RoundPriceAsync(discountAmountExclTax);
            }

            if (includingTax)
            {
                subTotalWithDiscount = subTotalInclTaxWithDiscount;
                discountAmount = discountAmountInclTax;
            }
            else
            {
                subTotalWithDiscount = subTotalExclTaxWithDiscount;
                discountAmount = discountAmountExclTax;
            }

            if (subTotalWithDiscount < decimal.Zero)
                subTotalWithDiscount = decimal.Zero;

            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                subTotalWithDiscount = await _priceCalculationService.RoundPriceAsync(subTotalWithDiscount);

            return (discountAmount, appliedDiscounts, subTotalWithoutDiscount, subTotalWithDiscount, taxRates, discountAmountsApplied);
        }

        public async Task<(decimal orderDiscount, List<Discount> appliedDiscounts, List<decimal>)> GetCustomOrderSubtotalDiscountAsync(Customer customer,
          decimal orderSubTotal)
        {
            List<decimal> discountAmountsApplied = new List<decimal>();
            var appliedDiscounts = new List<Discount>();
            var discountAmount = decimal.Zero;
            if (_catalogSettings.IgnoreDiscounts)
                return (discountAmount, appliedDiscounts, discountAmountsApplied);

            var allDiscounts = await _discountService.GetAllDiscountsAsync(DiscountType.AssignedToOrderSubTotal);
            var allowedDiscounts = new List<Discount>();
            if (allDiscounts != null)
            {
                var couponCodesToValidate = await _customerService.ParseAppliedDiscountCouponCodesAsync(customer);
                foreach (var discount in allDiscounts)
                    if (!_discountService.ContainsDiscount(allowedDiscounts, discount) &&
                        (await _discountService.ValidateDiscountAsync(discount, customer, couponCodesToValidate)).IsValid)
                    {
                        allowedDiscounts.Add(discount);
                    }
            }

            appliedDiscounts = _discountExtendedService.GetCustomPreferredDiscount(allowedDiscounts, orderSubTotal, out discountAmount, out discountAmountsApplied);

            if (discountAmount < decimal.Zero)
                discountAmount = decimal.Zero;

            return (discountAmount, appliedDiscounts, discountAmountsApplied);
        }



        public virtual async Task<(decimal? shoppingCartTotal, decimal discountAmount, List<Discount> appliedDiscounts, List<AppliedGiftCard> appliedGiftCards, int redeemedRewardPoints, decimal redeemedRewardPointsAmount, List<decimal>)> GetCustomShoppingCartTotalWithDiscountInfosync(IList<ShoppingCartItem> cart,
       bool? useRewardPoints = null, bool usePaymentMethodAdditionalFee = true)
        {

            var redeemedRewardPoints = 0;
            var redeemedRewardPointsAmount = decimal.Zero;

            var customer = await _customerService.GetShoppingCartCustomerAsync(cart);
            var store = await _storeContext.GetCurrentStoreAsync();
            var paymentMethodSystemName = string.Empty;
            if (customer != null)
            {
                paymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(customer,
                    NopCustomerDefaults.SelectedPaymentMethodAttribute, store.Id);
            }

            //subtotal without tax
            #region Custom updates Need to shift with Upgrade
            var (_, _, _, subTotalWithDiscountBase, _, _) = await GetCustomShoppingCartSubTotalAsync(cart, false);
            #endregion

            #region Custom Duty
            subTotalWithDiscountBase = subTotalWithDiscountBase + (await this.GetCustomDuty(cart)).Item2;
            #endregion
            //subtotal with discount
            var subtotalBase = subTotalWithDiscountBase;

            if (_shoppingCartSettings.RoundPricesDuringCalculation)
            {
                #region Custom updates Need to shift with Upgrade

                decimal membershipfee = 0;
                decimal membershipfeeDiscount = 0;
                (decimal buyMoreDiscount, decimal membershipDiscount, decimal offerDiscount, decimal offerDiscountDefault, decimal productItemsDiscount, _) = await GetCustomBuyMoreSaveMoreDiscountAndMemberShipDiscountAsync(cart);


                if (await _customerExtendedService.IsMemberShipAddedInCart(customer))
                    (membershipfee, membershipfeeDiscount) = await GetMemberShipFee();

                subtotalBase = (subtotalBase + membershipfee + (offerDiscountDefault - offerDiscount)) - buyMoreDiscount - membershipDiscount - membershipfeeDiscount - productItemsDiscount;

                #endregion
            }

            //shipping without tax
            var shoppingCartShipping = (await GetShoppingCartShippingTotalAsync(cart, false)).shippingTotal;

            //payment method additional fee without tax
            var paymentMethodAdditionalFeeWithoutTax = decimal.Zero;
            if (usePaymentMethodAdditionalFee && !string.IsNullOrEmpty(paymentMethodSystemName))
            {
                var paymentMethodAdditionalFee = await _paymentService.GetAdditionalHandlingFeeAsync(cart,
                    paymentMethodSystemName);
                paymentMethodAdditionalFeeWithoutTax =
                    (await _taxService.GetPaymentMethodAdditionalFeeAsync(paymentMethodAdditionalFee,
                        false, customer)).price;
            }

            //tax
            var shoppingCartTax = (await GetTaxTotalAsync(cart, usePaymentMethodAdditionalFee)).taxTotal;

            //order total
            var resultTemp = decimal.Zero;
            resultTemp += subtotalBase;
            if (shoppingCartShipping.HasValue)
            {
                resultTemp += shoppingCartShipping.Value;
            }

            resultTemp += paymentMethodAdditionalFeeWithoutTax;
            resultTemp += shoppingCartTax;
            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                resultTemp = await _priceCalculationService.RoundPriceAsync(resultTemp);

            //order total discount
            var (discountAmount, appliedDiscounts, discountAmountsApplied) = await GetCustomOrderTotalDiscountAsync(customer, resultTemp);

            //sub totals with discount        
            if (resultTemp < discountAmount)
                discountAmount = resultTemp;

            //reduce subtotal
            resultTemp -= discountAmount;

            if (resultTemp < decimal.Zero)
                resultTemp = decimal.Zero;
            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                resultTemp = await _priceCalculationService.RoundPriceAsync(resultTemp);

            //let's apply gift cards now (gift cards that can be used)
            var appliedGiftCards = new List<AppliedGiftCard>();
            resultTemp = await AppliedGiftCardsAsync(cart, appliedGiftCards, customer, resultTemp);

            if (resultTemp < decimal.Zero)
                resultTemp = decimal.Zero;
            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                resultTemp = await _priceCalculationService.RoundPriceAsync(resultTemp);

            var orderTotal = resultTemp;

            //reward points
            (redeemedRewardPoints, redeemedRewardPointsAmount) = await SetRewardPointsAsync(redeemedRewardPoints, redeemedRewardPointsAmount, useRewardPoints, customer, orderTotal);

            orderTotal -= redeemedRewardPointsAmount;
            orderTotal = await _priceCalculationService.RoundPriceAsync(orderTotal);

            if (!shoppingCartShipping.HasValue)
            {
                //we have errors
                return (orderTotal, discountAmount, appliedDiscounts, appliedGiftCards, redeemedRewardPoints, redeemedRewardPointsAmount, discountAmountsApplied);
            }



            return (orderTotal, discountAmount, appliedDiscounts, appliedGiftCards, redeemedRewardPoints, redeemedRewardPointsAmount, discountAmountsApplied);
        }

        public virtual async Task<(decimal taxTotal, SortedDictionary<decimal, decimal> taxRates, List<TaxInfo> taxes)> CustomGetTaxTotalAsync(IList<ShoppingCartItem> cart, bool usePaymentMethodAdditionalFee = true)
        {
            if (cart == null)
                throw new ArgumentNullException(nameof(cart));

            var taxTotalResult = await _taxService.GetTaxTotalAsync(cart, usePaymentMethodAdditionalFee);
            var taxRates = taxTotalResult?.TaxRates ?? new SortedDictionary<decimal, decimal>();
            var taxTotal = taxTotalResult?.TaxTotal ?? decimal.Zero;

            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                taxTotal = await _priceCalculationService.RoundPriceAsync(taxTotal);

            return (taxTotal, taxRates, taxTotalResult?.Taxes ?? new List<TaxInfo>());
        }
        #endregion

        #region Utilities

        protected virtual async Task<(decimal orderDiscount, List<Discount> appliedDiscounts, List<decimal>)> GetCustomOrderTotalDiscountAsync(Customer customer, decimal orderTotal)
        {
            List<decimal> discountAmountsApplied = new List<decimal>();
            var appliedDiscounts = new List<Discount>();
            var discountAmount = decimal.Zero;
            if (_catalogSettings.IgnoreDiscounts)
                return (discountAmount, appliedDiscounts, discountAmountsApplied);

            var allDiscounts = await _discountService.GetAllDiscountsAsync(DiscountType.AssignedToOrderTotal);
            var allowedDiscounts = new List<Discount>();
            if (allDiscounts != null)
            {
                var couponCodesToValidate = await _customerService.ParseAppliedDiscountCouponCodesAsync(customer);
                foreach (var discount in allDiscounts)
                    if (!_discountService.ContainsDiscount(allowedDiscounts, discount) &&
                        (await _discountService.ValidateDiscountAsync(discount, customer, couponCodesToValidate)).IsValid)
                    {
                        allowedDiscounts.Add(discount);
                    }
            }

            appliedDiscounts = _discountExtendedService.GetCustomPreferredDiscount(allowedDiscounts, orderTotal, out discountAmount, out discountAmountsApplied);

            if (discountAmount < decimal.Zero)
                discountAmount = decimal.Zero;

            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                discountAmount = await _priceCalculationService.RoundPriceAsync(discountAmount);

            return (discountAmount, appliedDiscounts, discountAmountsApplied);
        }


        #endregion
    }
}
