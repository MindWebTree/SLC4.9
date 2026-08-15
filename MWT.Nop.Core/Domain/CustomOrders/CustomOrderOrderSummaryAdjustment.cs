using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.CustomOrders
{
    public partial class CustomOrderOrderSummaryAdjustment : BaseEntity
    {
        public decimal? SubtotalDiscount { get; set; }
        public string SubtotalChargeType { get; set; }
        public string SubTotalDiscountType { get; set; }
        public decimal? ShippingDiscount { get; set; }
        public string ShippingChargeType { get; set; }
        public string ShippingDiscountType { get; set; }
        public decimal? TaxDiscount { get; set; }
        public string TaxDiscountType { get; set; }
        public int CustomerID { get; set; }
        public int OrderId { get; set; }
        
        public string SubTotalAdjustmentNotes { get; set; }
        public string ShippingAdjustmentNotes { get; set; }
    }
}
