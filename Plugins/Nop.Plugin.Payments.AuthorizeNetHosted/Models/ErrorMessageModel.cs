using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.AuthorizeNetHosted.Models
{
    public record ErrorMessageModel : BaseNopModel
    {
        public IList<string> Warnings { get; set; }
    }
}
