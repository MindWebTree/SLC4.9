using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Payments.Affirm.Models
{
    public record PaymentLibraryModel : BaseNopModel
    {
        public string LibraryAPIUrl { get; set; }
        public string PublicApiKey { get; set; }
    }
}
