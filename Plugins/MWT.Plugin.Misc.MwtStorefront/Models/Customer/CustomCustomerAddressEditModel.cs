using MWT.Plugin.Misc.MwtStorefront.Models.Common;
using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Models.Customer
{
  
    public partial record CustomCustomerAddressEditModel : BaseNopModel
    {
        public CustomCustomerAddressEditModel()
        {
            Address = new CustomAddressModel();
        }

        public CustomAddressModel Address { get; set; }
    }
}
