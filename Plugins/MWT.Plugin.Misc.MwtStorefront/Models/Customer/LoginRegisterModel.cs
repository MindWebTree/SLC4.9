using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Models.Customer
{
    public partial record LoginRegisterModel : BaseNopModel
    {
        public string Email { get; set; }

        public string Password { get; set; }

        public string ProcessType { get; set; }

        public string ReturnUrl { get; set; }
    }
}
