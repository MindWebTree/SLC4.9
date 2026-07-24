using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.AbandonedCart
{
    public class AbandonedCartItem : BaseEntity
    {
        public int CustomerID { get; set; }
        public string ProductName { get; set; }
        public string Sku { get; set; }
        public int Quantity { get; set; }
        public string Notes { get; set; }
        public string AttributesXml { get; set; }
        public int ProductID { get; set; }
        public DateTime AbandonedOn { get; set; }
        public int ShoppingCartRecID { get; set; }
        public int cartInvoiceId { get; set; }
        public Guid InvoiceGuid { get; set; }
        public string SalesForceReferenceId { get; set; }
    }
}
