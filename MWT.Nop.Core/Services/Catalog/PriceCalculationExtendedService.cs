using MWT.Nop.Core.Service.Catalog;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;

namespace MWT.Nop.Core.Services.Catalog
{
    public partial class PriceCalculationExtendedService : PriceCalculationService, IPriceCalculationExtendedService
    {
        #region Fields

        private readonly IStoreContext _storeContext;
        private readonly IDiscountExtendedService _discountExtendedService;
        
        #endregion

        #region Ctor
        public PriceCalculationExtendedService(CatalogSettings catalogSettings, CurrencySettings currencySettings, ICategoryService categoryService,
            ICurrencyService currencyService, ICustomerService customerService, IDiscountService discountService, IManufacturerService manufacturerService, 
            IProductAttributeParser productAttributeParser, IProductService productService, IStaticCacheManager staticCacheManager, IStoreContext storeContext,
            IDiscountExtendedService discountExtendedService) 
            : base(catalogSettings, currencySettings, categoryService, currencyService, customerService, discountService, manufacturerService, productAttributeParser, productService, staticCacheManager)
        {
            _storeContext = storeContext;
            _discountExtendedService = discountExtendedService;

        }

        #endregion

        #region Methods
        public async Task<(decimal rezPrice, decimal appliedDiscountAmount, List<Discount> appliedDiscounts, List<decimal> discounts)> GetItemDiscount(Product product,
           Customer customer,
           decimal price,
           decimal additionalCharge,
          int quantity,
           DateTime? rentalStartDate,
           DateTime? rentalEndDate)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopCatalogDefaults.ProductDiscountCacheKey,
       product,
       price,
       additionalCharge,
       true,
       quantity,
       await _customerService.GetCustomerRoleIdsAsync(customer),
       await _storeContext.GetCurrentStoreAsync(),
       "Discount");
            decimal rezPrice;
            decimal discountAmount;
            List<Discount> appliedDiscounts;
            if (!_catalogSettings.CacheProductPrices || product.IsRental)
                cacheKey.CacheTime = 0;
            List<decimal> discountAmountsApplied = new List<decimal>();
            (rezPrice, discountAmount, appliedDiscounts, discountAmountsApplied) = await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                var discounts = new List<Discount>();
                var appliedDiscountAmount = decimal.Zero;
                //discount
                var (tmpDiscountAmount, tmpAppliedDiscounts, discountAmounts) = await GetCustomDiscountAmountAsync(product, customer, price);
                price -= tmpDiscountAmount;

                if (tmpAppliedDiscounts?.Any() ?? false)
                {
                    discounts.AddRange(tmpAppliedDiscounts);
                    appliedDiscountAmount = tmpDiscountAmount;
                }

                return (price, appliedDiscountAmount, discounts, discountAmounts);
            });
            return (rezPrice, discountAmount, appliedDiscounts, discountAmountsApplied);
        }


        #endregion

        #region Utilities

        protected virtual async Task<(decimal, List<Discount>, List<decimal> discountAmountsApplied)> GetCustomDiscountAmountAsync(Product product,
       Customer customer,
       decimal productPriceWithoutDiscount)
        {
            List<decimal> discounts = new List<decimal>();
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var appliedDiscounts = new List<Discount>();
            var appliedDiscountAmount = decimal.Zero;

            //we don't apply discounts to products with price entered by a customer
            if (product.CustomerEntersPrice)
                return (appliedDiscountAmount, appliedDiscounts, discounts);

            //discounts are disabled
            if (_catalogSettings.IgnoreDiscounts)
                return (appliedDiscountAmount, appliedDiscounts, discounts);

            var allowedDiscounts = await GetAllowedDiscountsAsync(product, customer);

            //no discounts
            if (!allowedDiscounts.Any())
                return (appliedDiscountAmount, appliedDiscounts, discounts);
            List<decimal> discountAmountsApplied = new List<decimal>();
            appliedDiscounts = _discountExtendedService.GetCustomPreferredDiscount(allowedDiscounts, productPriceWithoutDiscount, out appliedDiscountAmount,
                out discountAmountsApplied);

            return (appliedDiscountAmount, appliedDiscounts, discountAmountsApplied);
        }

        #endregion
    }
}
