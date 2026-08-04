using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using MWT.Nop.Core.Data.Discounts;
using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Services.Customers;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Attributes;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Date;
using Nop.Services.Stores;

namespace MWT.Nop.Core.Services.Catalog
{
    public partial class CustomShoppingCartService : ShoppingCartService, ICustomShoppingCartService
    {
        private readonly ICustomSpecificationAttributeService _customSpecificationAttributeService;
        private readonly ICustomProductAttributeFormatter _customProductAttributeFormatter;
        private readonly IVariantService _variantService;

        public CustomShoppingCartService(CatalogSettings catalogSettings, IAclService aclService, IActionContextAccessor actionContextAccessor, IAttributeParser<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeParser, IAttributeService<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeService, ICurrencyService currencyService, ICustomerService customerService, IDateRangeService dateRangeService, IDateTimeHelper dateTimeHelper, IEventPublisher eventPublisher, IGenericAttributeService genericAttributeService, IGiftCardService giftCardService, ILocalizationService localizationService, IPermissionService permissionService, IPriceCalculationService priceCalculationService, IPriceFormatter priceFormatter, IProductAttributeParser productAttributeParser, IProductAttributeService productAttributeService, IProductService productService, IRepository<ShoppingCartItem> sciRepository, IShippingService shippingService, IShortTermCacheManager shortTermCacheManager, IStaticCacheManager staticCacheManager, IStoreContext storeContext, IStoreService storeService, IStoreMappingService storeMappingService, IUrlHelperFactory urlHelperFactory, IUrlRecordService urlRecordService, IWorkContext workContext, OrderSettings orderSettings, ShoppingCartSettings shoppingCartSettings, ICustomSpecificationAttributeService customSpecificationAttributeService, ICustomProductAttributeFormatter customProductAttributeFormatter, IVariantService variantService) : base(catalogSettings, aclService, actionContextAccessor, checkoutAttributeParser, checkoutAttributeService, currencyService, customerService, dateRangeService, dateTimeHelper, eventPublisher, genericAttributeService, giftCardService, localizationService, permissionService, priceCalculationService, priceFormatter, productAttributeParser, productAttributeService, productService, sciRepository, shippingService, shortTermCacheManager, staticCacheManager, storeContext, storeService, storeMappingService, urlHelperFactory, urlRecordService, workContext, orderSettings, shoppingCartSettings)
        {
            _customSpecificationAttributeService = customSpecificationAttributeService;
            _customProductAttributeFormatter = customProductAttributeFormatter;
            _variantService = variantService;
        }

        public virtual async Task<(decimal unitPrice, decimal oldPrice, decimal msrp, decimal discountAmount, List<Discount> appliedDiscounts)> GetCustomUnitPriceAsync(Product product,
Customer customer,
ShoppingCartType shoppingCartType,
int quantity,
string attributesXml,
decimal customerEnteredPrice,
DateTime? rentalStartDate, DateTime? rentalEndDate,
bool includeDiscounts)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var discountAmount = decimal.Zero;
            var appliedDiscounts = new List<Discount>();

            decimal finalPrice;
            decimal oldPrice = product.OldPrice;
            decimal msrp = product.Msrp;
            var store = await _storeContext.GetCurrentStoreAsync();

            var combination = await _productAttributeParser.FindProductAttributeCombinationAsync(product, attributesXml);
            if (combination?.OverriddenPrice.HasValue ?? false)
            {
                (_, finalPrice, discountAmount, appliedDiscounts) = await _priceCalculationService.GetFinalPriceAsync(product,
                        customer,
                        store,
                        combination.OverriddenPrice.Value,
                        decimal.Zero,
                        includeDiscounts,
                        quantity,
                        product.IsRental ? rentalStartDate : null,
                        product.IsRental ? rentalEndDate : null);
                oldPrice = combination.OverriddenOldPrice == null ? 0 : combination.OverriddenOldPrice.Value;
                msrp = combination.OverriddenMsrp == null ? 0 : combination.OverriddenMsrp.Value;
            }
            else
            {
                //summarize price of all attributes
                var attributesTotalPrice = decimal.Zero;
                var attributeValues = await _productAttributeParser.ParseProductAttributeValuesAsync(attributesXml);
                if (attributeValues != null)
                {
                    foreach (var attributeValue in attributeValues)
                    {
                        attributesTotalPrice += await _priceCalculationService.GetProductAttributeValuePriceAdjustmentAsync(product, attributeValue, customer, store, product.CustomerEntersPrice ? (decimal?)customerEnteredPrice : null);
                    }
                }

                //get price of a product (with previously calculated price of all attributes)
                if (product.CustomerEntersPrice)
                {
                    finalPrice = customerEnteredPrice;
                }
                else
                {
                    int qty;
                    if (_shoppingCartSettings.GroupTierPricesForDistinctShoppingCartItems)
                    {
                        //the same products with distinct product attributes could be stored as distinct "ShoppingCartItem" records
                        //so let's find how many of the current products are in the cart                        
                        qty = (await GetShoppingCartAsync(customer, shoppingCartType: shoppingCartType, productId: product.Id))
                            .Sum(x => x.Quantity);

                        if (qty == 0)
                        {
                            qty = quantity;
                        }
                    }
                    else
                    {
                        qty = quantity;
                    }

                    (_, finalPrice, discountAmount, appliedDiscounts) = await _priceCalculationService.GetFinalPriceAsync(product,
                        customer,
                        store,
                        attributesTotalPrice,
                        includeDiscounts,
                        qty,
                        product.IsRental ? rentalStartDate : null,
                        product.IsRental ? rentalEndDate : null);
                }
            }

            //rounding
            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                finalPrice = await _priceCalculationService.RoundPriceAsync(finalPrice);

            oldPrice = oldPrice == 0 ? finalPrice : oldPrice;

            return (finalPrice, oldPrice, msrp, discountAmount, appliedDiscounts);
        }


        public async Task<(decimal, decimal)> MemberShipPriceOfProduct(int productId, decimal msrp, decimal price, decimal salePrice)
        {
            decimal memberShipPrice = 0;
            decimal discountPercent = 0;
            try
            {
                var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopCatalogDefaults.MemberShipPriceOfProduct, productId);


                var _settingService = EngineContext.Current.Resolve<ISettingService>();
                var _specificationAttributeService = EngineContext.Current.Resolve<ISpecificationAttributeService>();
                var _categoryService = EngineContext.Current.Resolve<ICategoryService>();
                discountPercent = await _staticCacheManager.GetAsync(cacheKey, async () =>
                {
                    decimal _discountPercent = 0;
                    var isMembershipEnabled = await _settingService.GetSettingByKeyAsync<bool>("ismembershipenabled");
                    if (isMembershipEnabled)
                    {
                        var mainCategoryId = await _customSpecificationAttributeService.GetMainCategoryOfProduct(productId);
                        if (mainCategoryId != 0)
                        {
                            var category = await _categoryService.GetCategoryByIdAsync(mainCategoryId);
                            if (category != null)
                                _discountPercent = category.MembershipDiscount;
                        }
                        if (_discountPercent <= 0)
                        {
                            var productCategories = await _categoryService.GetProductCategoriesByProductIdAsync(productId);
                            if (productCategories.Any())
                            {
                                var categories = await _categoryService.GetCategoriesByIdsAsync(productCategories.Select(m => m.CategoryId).ToArray());
                                if (categories.Any())
                                    _discountPercent = categories.OrderByDescending(m => m.MembershipDiscount).FirstOrDefault().MembershipDiscount;
                            }
                        }

                        if (_discountPercent <= 0)
                        {
                            try
                            {
                                _discountPercent = await _settingService.GetSettingByKeyAsync<int>("default.Memembership.Discount.Percent");
                            }
                            catch { }
                        }
                        return _discountPercent;
                    }
                    else
                        return _discountPercent;

                });

                if (discountPercent > 0 && price > 0)
                    memberShipPrice = price - ((price * discountPercent) / 100);
                else if (discountPercent > 0 && salePrice > 0)
                    memberShipPrice = salePrice - ((salePrice * discountPercent) / 100);

            }
            catch (Exception exp)
            {
                var _loggerService = EngineContext.Current.Resolve<ILogger>();
                await _loggerService.InsertLogAsync(LogLevel.Error, "MemberShipPrice function:MemberShipPriceOfProduct", "Failed to get Membership Price, Exception: " + exp.Message);
            }
            return (memberShipPrice, discountPercent);
        }

        public virtual async Task<IList<string>> CustomAddToCartAsync(Customer customer, Product product,
        ShoppingCartType shoppingCartType, int storeId, string attributesXml = null,
        decimal customerEnteredPrice = decimal.Zero,
        DateTime? rentalStartDate = null, DateTime? rentalEndDate = null,
        int quantity = 1, bool addRequiredProducts = true)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var warnings = new List<string>();
            if (shoppingCartType == ShoppingCartType.ShoppingCart && !await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.ENABLE_SHOPPING_CART, customer))
            {
                warnings.Add("Shopping cart is disabled");
                return warnings;
            }

            if (shoppingCartType == ShoppingCartType.Wishlist && !await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.ENABLE_WISHLIST, customer))
            {
                warnings.Add("Wishlist is disabled");
                return warnings;
            }

            if (customer.IsSearchEngineAccount())
            {
                warnings.Add("Search engine can't add to cart");
                return warnings;
            }

            if (quantity <= 0)
            {
                warnings.Add(await _localizationService.GetResourceAsync("ShoppingCart.QuantityShouldPositive"));
                return warnings;
            }

            //reset checkout info
            await _customerService.ResetCheckoutDataAsync(customer, storeId);

            var cart = await GetShoppingCartAsync(customer, shoppingCartType, storeId);

            var shoppingCartItem = await FindShoppingCartItemInTheCartAsync(cart,
                shoppingCartType, product, attributesXml, customerEnteredPrice,
                rentalStartDate, rentalEndDate);

            if (shoppingCartItem != null)
            {
                //update existing shopping cart item
                var newQuantity = shoppingCartItem.Quantity + quantity;
                warnings.AddRange(await GetShoppingCartItemWarningsAsync(customer, shoppingCartType, product,
                    storeId, attributesXml,
                    customerEnteredPrice, rentalStartDate, rentalEndDate,
                    newQuantity, addRequiredProducts, shoppingCartItem.Id));

                shoppingCartItem.AttributesXml = attributesXml;
                shoppingCartItem.Quantity = newQuantity;
                shoppingCartItem.UpdatedOnUtc = DateTime.UtcNow;

                await _sciRepository.UpdateAsync(shoppingCartItem);
            }
            else
            {
                //new shopping cart item
                warnings.AddRange(await GetShoppingCartItemWarningsAsync(customer, shoppingCartType, product,
                    storeId, attributesXml, customerEnteredPrice,
                    rentalStartDate, rentalEndDate,
                    quantity, addRequiredProducts));

                //if (warnings.Any())
                //    return warnings;

                //maximum items validation
                switch (shoppingCartType)
                {
                    case ShoppingCartType.ShoppingCart:
                        if (cart.Count >= _shoppingCartSettings.MaximumShoppingCartItems)
                        {
                            warnings.Add(string.Format(await _localizationService.GetResourceAsync("ShoppingCart.MaximumShoppingCartItems"), _shoppingCartSettings.MaximumShoppingCartItems));
                            return warnings;
                        }

                        break;
                    case ShoppingCartType.Wishlist:
                        if (cart.Count >= _shoppingCartSettings.MaximumWishlistItems)
                        {
                            warnings.Add(string.Format(await _localizationService.GetResourceAsync("ShoppingCart.MaximumWishlistItems"), _shoppingCartSettings.MaximumWishlistItems));
                            return warnings;
                        }

                        break;
                    default:
                        break;
                }

                var now = DateTime.UtcNow;
                shoppingCartItem = new ShoppingCartItem
                {
                    ShoppingCartType = shoppingCartType,
                    StoreId = storeId,
                    ProductId = product.Id,
                    AttributesXml = attributesXml,
                    CustomerEnteredPrice = customerEnteredPrice,
                    Quantity = quantity,
                    RentalStartDateUtc = rentalStartDate,
                    RentalEndDateUtc = rentalEndDate,
                    CreatedOnUtc = now,
                    UpdatedOnUtc = now,
                    CustomerId = customer.Id
                };

                await _sciRepository.InsertAsync(shoppingCartItem);

                //updated "HasShoppingCartItems" property used for performance optimization
                customer.HasShoppingCartItems = !await IsCustomerShoppingCartEmptyAsync(customer);

                await _customerService.UpdateCustomerAsync(customer);
            }

            return warnings;
        }

        public virtual async Task UpdateShoppingCartItemAsync(ShoppingCartItem item)
        {
            await _sciRepository.UpdateAsync(item);
        }

        public virtual async Task<IList<string>> UpdateShoppingCartItemAsync(Customer customer,
    int shoppingCartItemId, string attributesXml,
    decimal customerEnteredPrice, string specialInstructions,
    DateTime? rentalStartDate = null, DateTime? rentalEndDate = null,
    int quantity = 1, bool resetCheckoutData = true)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var warnings = new List<string>();

            var shoppingCartItem = await _sciRepository.GetByIdAsync(shoppingCartItemId, cache => default);

            if (shoppingCartItem == null || shoppingCartItem.CustomerId != customer.Id)
                return warnings;

            if (resetCheckoutData)
            {
                //reset checkout data
                await _customerService.ResetCheckoutDataAsync(customer, shoppingCartItem.StoreId);
            }

            var product = await _productService.GetProductByIdAsync(shoppingCartItem.ProductId);

            if (quantity > 0)
            {
                //check warnings
                warnings.AddRange(await GetShoppingCartItemWarningsAsync(customer, shoppingCartItem.ShoppingCartType,
                    product, shoppingCartItem.StoreId,
                    attributesXml, customerEnteredPrice,
                    rentalStartDate, rentalEndDate, quantity, false, shoppingCartItemId));
                if (warnings.Any())
                    return warnings;

                //if everything is OK, then update a shopping cart item
                shoppingCartItem.Quantity = quantity;
                shoppingCartItem.AttributesXml = attributesXml;
                shoppingCartItem.CustomerEnteredPrice = customerEnteredPrice;
                shoppingCartItem.RentalStartDateUtc = rentalStartDate;
                shoppingCartItem.RentalEndDateUtc = rentalEndDate;
                shoppingCartItem.UpdatedOnUtc = DateTime.UtcNow;
                shoppingCartItem.SpecialInstructions = specialInstructions;
                await _sciRepository.UpdateAsync(shoppingCartItem);
                await _customerService.UpdateCustomerAsync(customer);
            }
            else
            {
                //check warnings for required products
                warnings.AddRange(await GetRequiredProductWarningsAsync(customer, shoppingCartItem.ShoppingCartType,
                    product, shoppingCartItem.StoreId, quantity, false, shoppingCartItemId));
                if (warnings.Any())
                    return warnings;

                //delete a shopping cart item
                await DeleteShoppingCartItemAsync(shoppingCartItem, resetCheckoutData, true);
            }

            return warnings;
        }


        public virtual async Task<(IList<string>, int)> CustomAddToCartCollectionAsync(Customer customer, Product product,
            ShoppingCartType shoppingCartType, int storeId, string attributesXml = null,
            decimal customerEnteredPrice = decimal.Zero,
            DateTime? rentalStartDate = null, DateTime? rentalEndDate = null,
            int quantity = 1, bool addRequiredProducts = true)
        {
            int shoppingcartItemId = 0;
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var warnings = new List<string>();
            if (shoppingCartType == ShoppingCartType.ShoppingCart && !await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.ENABLE_SHOPPING_CART, customer))
            {
                warnings.Add("Shopping cart is disabled");
                return (warnings, shoppingcartItemId);
            }

            if (shoppingCartType == ShoppingCartType.Wishlist && !await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.ENABLE_WISHLIST, customer))
            {
                warnings.Add("Wishlist is disabled");
                return (warnings, shoppingcartItemId);
            }

            if (customer.IsSearchEngineAccount())
            {
                warnings.Add("Search engine can't add to cart");
                return (warnings, shoppingcartItemId);
            }

            if (quantity <= 0)
            {
                warnings.Add(await _localizationService.GetResourceAsync("ShoppingCart.QuantityShouldPositive"));
                return (warnings, shoppingcartItemId);
            }

            //reset checkout info
            await _customerService.ResetCheckoutDataAsync(customer, storeId);

            var cart = await GetShoppingCartAsync(customer, shoppingCartType, storeId);

            var shoppingCartItem = await FindShoppingCartItemInTheCartAsync(cart,
                shoppingCartType, product, attributesXml, customerEnteredPrice,
                rentalStartDate, rentalEndDate);

            if (shoppingCartItem != null)
            {
                //update existing shopping cart item
                var newQuantity = shoppingCartItem.Quantity + quantity;
                warnings.AddRange(await GetShoppingCartItemWarningsAsync(customer, shoppingCartType, product,
                    storeId, attributesXml,
                    customerEnteredPrice, rentalStartDate, rentalEndDate,
                    newQuantity, addRequiredProducts, shoppingCartItem.Id));

                if (warnings.Any())
                    return (warnings, shoppingcartItemId);

                shoppingCartItem.AttributesXml = attributesXml;
                shoppingCartItem.Quantity = newQuantity;
                shoppingCartItem.UpdatedOnUtc = DateTime.UtcNow;

                await _sciRepository.UpdateAsync(shoppingCartItem);
                shoppingcartItemId = shoppingCartItem.Id;
            }
            else
            {
                //new shopping cart item
                warnings.AddRange(await GetShoppingCartItemWarningsAsync(customer, shoppingCartType, product,
                    storeId, attributesXml, customerEnteredPrice,
                    rentalStartDate, rentalEndDate,
                    quantity, addRequiredProducts));

                if (warnings.Any())
                    return (warnings, shoppingcartItemId);

                //maximum items validation
                switch (shoppingCartType)
                {
                    case ShoppingCartType.ShoppingCart:
                        if (cart.Count >= _shoppingCartSettings.MaximumShoppingCartItems)
                        {
                            warnings.Add(string.Format(await _localizationService.GetResourceAsync("ShoppingCart.MaximumShoppingCartItems"), _shoppingCartSettings.MaximumShoppingCartItems));
                            return (warnings, shoppingcartItemId);
                        }

                        break;
                    case ShoppingCartType.Wishlist:
                        if (cart.Count >= _shoppingCartSettings.MaximumWishlistItems)
                        {
                            warnings.Add(string.Format(await _localizationService.GetResourceAsync("ShoppingCart.MaximumWishlistItems"), _shoppingCartSettings.MaximumWishlistItems));
                            return (warnings, shoppingcartItemId);
                        }

                        break;
                    default:
                        break;
                }

                var now = DateTime.UtcNow;
                shoppingCartItem = new ShoppingCartItem
                {
                    ShoppingCartType = shoppingCartType,
                    StoreId = storeId,
                    ProductId = product.Id,
                    AttributesXml = attributesXml,
                    CustomerEnteredPrice = customerEnteredPrice,
                    Quantity = quantity,
                    RentalStartDateUtc = rentalStartDate,
                    RentalEndDateUtc = rentalEndDate,
                    CreatedOnUtc = now,
                    UpdatedOnUtc = now,
                    CustomerId = customer.Id
                };

                await _sciRepository.InsertAsync(shoppingCartItem);
                shoppingcartItemId = shoppingCartItem.Id;

                //updated "HasShoppingCartItems" property used for performance optimization
                customer.HasShoppingCartItems = !await IsCustomerShoppingCartEmptyAsync(customer);

                await _customerService.UpdateCustomerAsync(customer);
            }

            return (warnings, shoppingcartItemId);
        }


        public virtual async Task<(decimal unitPrice, decimal oldPrice, decimal msrp, decimal discountAmount, List<Discount> appliedDiscounts)> GetCustomUnitPriceForAttributeAsync(Product product,
         Customer customer,
         ShoppingCartType shoppingCartType,
         int quantity,
         string attributesXml,
 decimal customerEnteredPrice,
DateTime? rentalStartDate, DateTime? rentalEndDate,
bool includeDiscounts)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var discountAmount = decimal.Zero;
            var appliedDiscounts = new List<Discount>();

            decimal finalPrice = product.Price;
            decimal oldPrice = product.OldPrice;
            decimal msrp = product.Msrp;

            var combination = await _productAttributeParser.FindProductAttributeCombinationAsync(product, attributesXml);
            if (combination?.OverriddenPrice.HasValue ?? false)
            {

                oldPrice = combination.OverriddenOldPrice == null ? 0 : combination.OverriddenOldPrice.Value;
                msrp = combination.OverriddenMsrp == null ? 0 : combination.OverriddenMsrp.Value;
                finalPrice = combination.OverriddenPrice == null ? finalPrice : combination.OverriddenPrice.Value;
            }


            //rounding
            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                finalPrice = await _priceCalculationService.RoundPriceAsync(finalPrice);

            oldPrice = oldPrice == 0 ? finalPrice : oldPrice;

            return (finalPrice, oldPrice, msrp, discountAmount, appliedDiscounts);
        }



        public async Task<bool> IsSurchargeApplicable(IList<ShoppingCartItem> cart)
        {
            var _productAttributeFormatter = EngineContext.Current.Resolve<IProductAttributeFormatter>();
            bool wgsSurchargeApplicable = false;
            try
            {
                foreach (var item in cart)
                {
                    string attributeDescription = await _customProductAttributeFormatter.CustomFormatAttributesAsync(await _productService.GetProductByIdAsync(item.ProductId), item.AttributesXml);
                    int variantId = await _variantService.GetVariantId(item.ProductId, attributeDescription);
                    if (variantId > 0)
                    {
                        var variantCombination = await _variantService.GetProductVariants(item.ProductId);
                        if ((variantCombination.Where(v => v.VariantId == variantId).FirstOrDefault()?.EnableSurcharge ?? false))
                        {
                            wgsSurchargeApplicable = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                var _logger = EngineContext.Current.Resolve<ILogger>();
                await _logger.InsertLogAsync(LogLevel.Error, "Cart surcharge check request failed.", $"Failed to process the request to check if surcharge is applicable on cart items. CartInfo :{string.Join(',', cart.Select(c => c.Id))}");
            }
            finally
            {

            }
            return wgsSurchargeApplicable;
        }


        public async Task<string> GetBuyMoreSaveMoreDiscountConfiguration()
        {
            decimal discount = 0;
            CustomDiscountType discountType = CustomDiscountType.Fixed;
            var _settingService = EngineContext.Current.Resolve<ISettingService>();
            bool isBuyMoreSaveMoreEnabled = await _settingService.GetSettingByKeyAsync<bool>("MarketingSettings.EnableBuyMoreSaveMoreDiscount");
            if (isBuyMoreSaveMoreEnabled)
            {
                string buyMoreSaveMoreDiscounts = await _settingService.GetSettingByKeyAsync<string>("MarketingSettings.BuyMoreSaveMoreDiscountConfiguration");
                string[] discountLevels = buyMoreSaveMoreDiscounts.Split('|', StringSplitOptions.RemoveEmptyEntries);
                foreach (var level in discountLevels)
                {
                    string[] levelConfig = level.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                    if (levelConfig.Length == 4)
                    {
                        if (levelConfig[0].Trim() == "%")
                            discountType = CustomDiscountType.Percent;

                        decimal.TryParse(levelConfig[1], out discount);
                        break;


                    }
                }
            }
            return discount == 0 ? string.Empty : discountType == CustomDiscountType.Percent ? $"{discount}%" : await _priceFormatter.FormatPriceAsync(discount);
        }
    }
}
