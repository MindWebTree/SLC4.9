using Nop.Core.Domain.Discounts;
using Nop.Services.Discounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Catalog
{
    public partial interface  IDiscountExtendedService: IDiscountService
    {
        List<Discount> GetCustomPreferredDiscount(IList<Discount> discounts,
        decimal amount, out decimal discountAmount, out List<decimal> discountAmountsApplied);
    }
}
