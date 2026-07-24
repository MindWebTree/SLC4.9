using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Orders
{
    /// <summary>
    /// Represents an order item
    /// </summary>
    public partial class OrderItem : BaseEntity
    {
        public decimal ItemPriceIncTax { get; set; }
        public decimal MembershipDiscountIncTax { get; set; }
        public decimal OfferDiscountIncTax { get; set; }
        public decimal BuyMoreSaveMoreDiscountIncTax { get; set; }
        public string CustomAttributesDescription { get; set; }
        public string Notes { get; set; }
        public string SpecialInstructions { get; set; }

        public decimal TotalDiscount { get; set; }
    }
}
