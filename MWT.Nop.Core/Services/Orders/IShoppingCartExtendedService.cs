using MWT.Nop.Core.Data.Discounts;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Stores;
using Nop.Services.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Catalog
{
    public partial interface IShoppingCartExtendedService : IShoppingCartService
    {
        Task<(decimal unitPrice, decimal oldPrice, decimal msrp, decimal discountAmount, List<Discount> appliedDiscounts)> GetCustomUnitPriceAsync(Product product,
         Customer customer,
         Store store,
         ShoppingCartType shoppingCartType,
         int quantity,
         string attributesXml,
         decimal customerEnteredPrice,
         DateTime? rentalStartDate, DateTime? rentalEndDate,
         bool includeDiscounts);

        Task<(decimal, decimal)> MemberShipPriceOfProduct(int productId, decimal msrp, decimal price, decimal salePrice);

        Task<IList<string>> CustomAddToCartAsync(Customer customer, Product product,
        ShoppingCartType shoppingCartType, int storeId, string attributesXml = null,
        decimal customerEnteredPrice = decimal.Zero,
        DateTime? rentalStartDate = null, DateTime? rentalEndDate = null,
        int quantity = 1, bool addRequiredProducts = true, int? wishlistId = null);

        Task UpdateShoppingCartItemAsync(ShoppingCartItem item);
        Task<IList<string>> UpdateShoppingCartItemAsync(Customer customer,
    int shoppingCartItemId, string attributesXml,
    decimal customerEnteredPrice, string specialInstructions,
    DateTime? rentalStartDate = null, DateTime? rentalEndDate = null,
    int quantity = 1, bool resetCheckoutData = true);

        Task<(IList<string>, int)> CustomAddToCartCollectionAsync(Customer customer, Product product,
            ShoppingCartType shoppingCartType, int storeId, string attributesXml = null,
            decimal customerEnteredPrice = decimal.Zero,
            DateTime? rentalStartDate = null, DateTime? rentalEndDate = null,
            int quantity = 1, bool addRequiredProducts = true);

        Task<(decimal unitPrice, decimal oldPrice, decimal msrp, decimal discountAmount, List<Discount> appliedDiscounts)> GetCustomUnitPriceForAttributeAsync(Product product,
Customer customer,
ShoppingCartType shoppingCartType,
int quantity,
string attributesXml,
decimal customerEnteredPrice,
DateTime? rentalStartDate, DateTime? rentalEndDate,
bool includeDiscounts);

        Task<bool> IsSurchargeApplicable(IList<ShoppingCartItem> cart);

        Task<string> GetBuyMoreSaveMoreDiscountConfiguration();
        Task<(CustomDiscountType discountType, decimal buyMoreDiscount, int notEligibleCartItemId)> 
            GetBuyMoreSaveMoreDiscountDetailsAsync(IList<ShoppingCartItem> cart, decimal subTotal);

    }
}
