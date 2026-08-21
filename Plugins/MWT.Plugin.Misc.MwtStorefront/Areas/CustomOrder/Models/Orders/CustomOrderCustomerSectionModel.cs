using Nop.Core.Domain.Common;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Common;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Customers;
using Nop.Web.Framework.Models;
using System.Collections.Generic;
using MWT.Plugin.Misc.MwtStorefront.Models.Common;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Orders
{
    public partial record CustomOrderCustomerSectionModel : BaseCustomOrderEntityModel
    {
        public CustomOrderCustomerSectionModel()
        {
        }
        public bool EnableCustomerSearch { get; set; }
        public AddressModel ShippingAddress { get; set; }
        public AddressModel BillingAddress { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }
}
