using MWT.Plugin.Misc.MwtStorefront.Models.Common;
using Nop.Web.Models.Common;
using Nop.Web.Models.ShoppingCart;
using NopCustomer = Nop.Core.Domain.Customers.Customer;

namespace MWT.Plugin.Misc.MwtStorefront.Models.AbandonedCarts
{
    public class AbandonedCartModel
    {
        public AbandonedCartModel()
        {
            ShippingAddress = new AddressModel();
            BillingAddress = new AddressModel();
        }
        public Guid InvoiceId { get; set; }
        public AddressModel ShippingAddress { get; set; }
        public AddressModel BillingAddress { get; set; }
        public ShoppingCartModel Cart { get; set; }
        public bool IsCartValid { get; set; }
        public bool ImpersonateUser { get; set; }
        public AbandonedCartErrorType ErrorType { get; set; }
        public DateTime CreatedOn { get; set; }
        public NopCustomer Customer { get; set; }
        public int NoOfCartItem { get; set; }
    }

    public enum AbandonedCartErrorType
    {
        Empty = 1,
        InValid = 2,
        InValidInvoiceId = 3,
        Processed = 4
    }
}
