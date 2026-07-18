using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Payments.Affirm.Models
{
    public record ErrorMessageModel : BaseNopModel
    {
        public ErrorMessageModel()
        {
            this.Warnings = new List<string>();
        }
        public IList<string> Warnings { get; set; }
    }
}
