using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;

namespace MWT.Nop.Core.Services.Catalog
{
    public partial class DiscountExtendedService : DiscountService, IDiscountExtendedService
    {
        #region Ctor
        public DiscountExtendedService(ICustomerService customerService, IDiscountPluginManager discountPluginManager, ILocalizationService localizationService, 
            IProductService productService, IRepository<Discount> discountRepository, IRepository<DiscountRequirement> discountRequirementRepository, 
            IRepository<DiscountUsageHistory> discountUsageHistoryRepository, IRepository<Order> orderRepository, IShortTermCacheManager shortTermCacheManager, 
            IStaticCacheManager staticCacheManager, IStoreContext storeContext) : base(customerService, discountPluginManager, localizationService, productService, discountRepository, discountRequirementRepository, discountUsageHistoryRepository, orderRepository, shortTermCacheManager, staticCacheManager, storeContext)
        {
        }

        #endregion

        #region Methods

        public virtual List<Discount> GetCustomPreferredDiscount(IList<Discount> discounts,
    decimal amount, out decimal discountAmount, out List<decimal> discountAmountsApplied)
        {
            if (discounts == null)
                throw new ArgumentNullException(nameof(discounts));

            var result = new List<Discount>();
            discountAmount = decimal.Zero;
            discountAmountsApplied = new List<decimal>();
            if (!discounts.Any())
                return result;

            //first we check simple discounts
            foreach (var discount in discounts)
            {
                var currentDiscountValue = GetDiscountAmount(discount, amount);
                if (currentDiscountValue <= discountAmount)
                    continue;

                discountAmount = currentDiscountValue;

                result.Clear();
                discountAmountsApplied.Clear();
                discountAmountsApplied.Add(discountAmount);
                result.Add(discount);
            }
            //now let's check cumulative discounts
            //right now we calculate discount values based on the original amount value
            //please keep it in mind if you're going to use discounts with "percentage"
            var cumulativeDiscounts = discounts.Where(x => x.IsCumulative).OrderBy(x => x.Name).ToList();
            if (cumulativeDiscounts.Count <= 1)
                return result;

            var cumulativeDiscountAmount = cumulativeDiscounts.Sum(d => GetDiscountAmount(d, amount));
            if (cumulativeDiscountAmount <= discountAmount)
                return result;

            discountAmount = cumulativeDiscountAmount;

            result.Clear();
            result.AddRange(cumulativeDiscounts);
            discountAmountsApplied.Clear();
            discountAmountsApplied.AddRange(cumulativeDiscounts.Select(d => GetDiscountAmount(d, amount)));

            return result;
        }
        #endregion
    }
}
