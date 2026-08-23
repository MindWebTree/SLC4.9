using MWT.Plugin.Misc.MwtStorefront.Models.Common;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Models.Customer
{
    
    public partial record CustomerAddressEditExtendedModel : BaseNopModel
    {
        public CustomerAddressEditExtendedModel()
        {
            Address = new AddressModel();
        }

        public AddressModel Address { get; set; }
    }
}
