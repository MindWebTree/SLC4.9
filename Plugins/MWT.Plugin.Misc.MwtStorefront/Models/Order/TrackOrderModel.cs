using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Models.Order
{
    public partial record TrackOrderModel : BaseNopEntityModel
    {
        public string Email { get; set; }
    }
}
