using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.AbandonedCarts
{
    public class AbandonedShoppingCart: BaseEntity
    {
        
        public int ShoppingCartRecID { get; set; }
        public int StoreID { get; set; }
        public int CustomerID { get; set; }
        public int ProductID { get; set; }
        public string AttributeXml { get; set; }
        public int Quantity { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedOn { get; set; }
        public int CartType { get; set; }
        public int CartInvoiceID { get; set; }
        public bool IsNew { get; set; }
   
    }
}
