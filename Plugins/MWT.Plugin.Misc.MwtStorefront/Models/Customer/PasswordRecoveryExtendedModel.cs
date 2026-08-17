using Nop.Web.Models.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Models.Customer
{
    public partial record PasswordRecoveryExtendedModel: PasswordRecoveryModel
    {
        public string Result { get; set; }
        public int StatusCode { get; set; }
    }
}
