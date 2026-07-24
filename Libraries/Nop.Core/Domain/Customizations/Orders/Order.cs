using Nop.Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Orders
{
    /// <summary>
    /// Represents an order
    /// </summary>
    public partial class Order : BaseEntity, ISoftDeletedEntity
    {
        public string CustomerEmail { get; set; }
        public decimal MembershipFeeInclTax { get; set; }
        public decimal MembershipFeeDiscountInclTax { get; set; }
        public decimal MembershipDiscountIncTax { get; set; }
        public decimal OfferDiscountIncTax { get; set; }
        public decimal BuyMoreSaveMoreDiscountIncTax { get; set; }
        public decimal ShippingDiscountInclTax { get; set; }
        public bool IsCustomOrder { get; set; }
        public decimal WgsChargesInclTax { get; set; }
        public decimal AdditonalShippingChargesInclTax { get; set; }

        public int ParentOrderID { get; set; }
        public bool? IsImported { get; set; }
        public decimal CustomDutyInclTax { get; set; }
        public decimal CustomDutyPercentage { get; set; }
        public decimal CustomDutyExclTax { get; set; }
        public bool OrderConfirmed { get; set; }
        public string TaxInfo { get; set; }
    }
}
