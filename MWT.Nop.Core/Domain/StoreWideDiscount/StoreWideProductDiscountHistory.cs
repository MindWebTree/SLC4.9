using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.StoreWideDiscount
{
    public partial class StoreWideProductDiscountHistory : BaseEntity
    {
        public int StoreWideDiscountId { get; set; }
        public int ProductId { get; set; }
        public decimal Discount { get; set; }
        public string InfoText { get; set; }
        public string InfoHelpText { get; set; }
        public int CustomerId { get; set; }
        public DateTime? CreatedOn { get; set; }
    }
}
