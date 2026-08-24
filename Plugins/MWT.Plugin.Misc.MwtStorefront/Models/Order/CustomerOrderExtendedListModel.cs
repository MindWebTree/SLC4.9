
using Nop.Web.Framework.Models;
using Nop.Web.Models.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Models.Order
{
    public partial record CustomerOrderExtendedListModel : CustomerOrderListModel
    {
        public new List<CustomerOrderExtendedModel> Orders { get; set; } = new();
    }
}
