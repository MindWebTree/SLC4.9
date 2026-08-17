using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.CustomOrders
{
    public partial class CustomOrderPriceAdjustment : BaseEntity
    {
        public int ShoppingCartRecID { get; set; }
        public decimal? ShoppingCartProductPrice { get; set; }
        public decimal? Discountamount { get; set; }
        public string Chargestype { get; set; }
        public string AdjustmentNotes { get; set; }
        public string Discounttype { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public int? OrderId { get; set; }
    }
}
