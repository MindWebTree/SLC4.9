using MWT.Nop.Plugin.Payments.Affirm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Payments.Affirm.Services
{
    public partial interface IAffirmService
    {
        Task<AffirmResponseModel> CheckoutDetails(string token);
        Task<(string, string,string,string, HttpStatusCode)> CaptureTransaction(string checkouttoken, Guid orderId, int orderTotal);
    }
}
