using Nop.Core.Domain.Discounts;
using Nop.Services.Discounts;

namespace MWT.Nop.Core.Services.Discounts
{
    public partial interface  IDiscountExtendedService: IDiscountService
    {
        List<Discount> GetCustomPreferredDiscount(IList<Discount> discounts,
        decimal amount, out decimal discountAmount, out List<decimal> discountAmountsApplied);
    }
}
