using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Payments.Affirm.Models
{
    public record PromotionalMessagingInfoModel : BaseNopModel
    {
        public decimal Amount { get; set; }
        public string MessageType { get; set; }

        public bool UseSandBox { get; set; }
        public string MessageColor { get; set; }
        public string PageType { get; set; }
        public int ProductId { get; set; }
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
    }
}
