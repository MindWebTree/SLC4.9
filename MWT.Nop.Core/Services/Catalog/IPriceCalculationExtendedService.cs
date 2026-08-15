using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Services.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Catalog
{
    public partial interface IPriceCalculationExtendedService: IPriceCalculationService
    {
        Task<(decimal rezPrice, decimal appliedDiscountAmount, List<Discount> appliedDiscounts, List<decimal> discounts)> GetItemDiscount(Product product,
          Customer customer,
          decimal price,
          decimal additionalCharge,
         int quantity,
          DateTime? rentalStartDate,
          DateTime? rentalEndDate);
    }
}
