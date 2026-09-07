using Nop.Services.Messages;

namespace MWT.Nop.Core.Services.Message
{
    public partial interface ICustomNewsLetterSubscriptionService : INewsLetterSubscriptionService
    {
        Task<bool> CheckEmailSubscriber(string Email, string ListID = "");
    }
}
