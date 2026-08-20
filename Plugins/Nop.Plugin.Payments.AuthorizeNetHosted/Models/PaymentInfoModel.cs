using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.AuthorizeNetHosted.Models
{
    public record PaymentInfoModel : BaseNopModel
    {
        public PaymentInfoModel()
        {
            Warnings = new List<string>();
        }
        public string LoginId { get; set; }
        public string PublicClientKey { get; set; }
        public bool UseSandbox { get; set; }
        public bool EnableBankAccount { get; set; }
        public string AuthorizeNetHostedPaymentErrors { get; set; }
        public IList<string> Warnings { get; set; }
        public bool OnePageCheckoutPlugin { get; set; }
        public string Token { get; set; }
        public string FormActionUrl { get; set; }
        public Guid OrderId { get; set; }
        public string TransactionId { get; set; }
        public string AuthorizeNetHostedPaymentDataValue { get; set; }
        public string AuthorizeNetHostedPaymentDataDescriptor { get; set; }

 
    }
}
