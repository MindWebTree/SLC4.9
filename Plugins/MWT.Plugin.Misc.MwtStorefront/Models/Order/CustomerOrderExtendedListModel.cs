using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Models.Order
{
    

    public partial record CustomerOrderListModel : BaseNopModel
    {
        public partial record OrderDetailsModel : BaseNopEntityModel
        {
            public string CustomerEmail { get; set; }
        }
    }
}
