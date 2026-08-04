using Nop.Services.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Message
{
    public partial interface ICustomNewsLetterSubscriptionService : INewsLetterSubscriptionService
    {
        Task<bool> CheckEmailSubscriber(string Email, string ListID="");
    }
}
