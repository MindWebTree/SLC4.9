using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Payments.Affirm.Models
{
    public record PaymentInfoModel : BaseNopModel
    {
        public PaymentInfoModel()
        {
            Warnings = new List<string>();
        }
        public string AffirmJSON { get; set; }
        public IList<string> Warnings { get; set; }
    }
}
