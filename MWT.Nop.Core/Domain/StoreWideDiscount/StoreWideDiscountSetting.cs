using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.StoreWideDiscount
{
    public partial class StoreWideDiscountSetting : BaseEntity
    {
        public int StoreWideDiscountId { get; set; }
        public decimal Discount { get; set; }
        public decimal Threshold { get; set; }
        public string CategoryIds { get; set; }
        public bool FullStore { get; set; }
        public string ProductIds { get; set; }
        public string InfoText { get; set; }
        public string InfoHelpText { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }

    }
}
