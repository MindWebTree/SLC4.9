using MWT.Nop.Core.Domain.Orders;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Services.Orders;
using Nop.Services.Tax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Orders
{
    public partial interface IOrderTotalCalculationExtendedService: IOrderTotalCalculationService
    {
        Task<(decimal, decimal, decimal, decimal, decimal, List<DiscountSummary>)> GetCustomBuyMoreSaveMoreDiscountAndMemberShipDiscountAsync(IList<ShoppingCartItem> cart,
            bool isOrderTotalpassed = false
            , decimal subTotalIncTax = 0,
            decimal subTotalExcTax = 0);
        Task<(decimal? shippingTotal, decimal taxRate, decimal? additionalFee, List<Discount> appliedDiscounts)> GetCustomShoppingCartShippingTotalAsync(IList<ShoppingCartItem> cart, bool includingTax);
        Task<(decimal? shippingTotal, decimal? additionalFee)> GetCustomShoppingCartShippingTotalAsync(IList<ShoppingCartItem> cart);
        Task<decimal> GetCustomShoppingCartSubTotalAsync(IList<ShoppingCartItem> cart);
        Task<(decimal, decimal)> GetMemberShipFee();

        Task<(decimal? shoppingCartTotal, decimal discountAmount, List<Discount> appliedDiscounts, List<AppliedGiftCard> appliedGiftCards, int redeemedRewardPoints, decimal redeemedRewardPointsAmount)> GetCustomShoppingCartTotalAsync(IList<ShoppingCartItem> cart,
        bool? useRewardPoints = null, bool usePaymentMethodAdditionalFee = true);

        Task<(decimal discountAmount, List<Discount> appliedDiscounts, decimal subTotalWithoutDiscount, decimal subTotalWithDiscount,
            SortedDictionary<decimal, decimal> taxRates, List<decimal> discountAmountsApplied)> GetCustomShoppingCartSubTotalAsync(IList<ShoppingCartItem> cart,
          bool includingTax);

        Task<(decimal? shoppingCartTotal, decimal discountAmount, List<Discount> appliedDiscounts, List<AppliedGiftCard> appliedGiftCards, int redeemedRewardPoints, decimal redeemedRewardPointsAmount, List<decimal>)> GetCustomShoppingCartTotalWithDiscountInfosync(IList<ShoppingCartItem> cart,
       bool? useRewardPoints = null, bool usePaymentMethodAdditionalFee = true);

        Task<(decimal orderDiscount, List<Discount> appliedDiscounts, List<decimal>)> GetCustomOrderSubtotalDiscountAsync(Customer customer,
      decimal orderSubTotal);

        Task<(decimal, decimal)> GetCustomDuty(IList<ShoppingCartItem> cart, bool isCustomOrder = false, Customer customOrderCustomer = null, decimal orderTotal = 0);

        Task<(decimal taxTotal, SortedDictionary<decimal, decimal> taxRates, List<TaxInfo> taxes)> CustomGetTaxTotalAsync(IList<ShoppingCartItem> cart, bool usePaymentMethodAdditionalFee = true);
    }
}
