

using MWT.Nop.Core.Domain.Mailchimp;

namespace MWT.Nop.Core.Services.MailChimp
{
    public partial interface IQueuedMailChimpCartSignUpService
    {
        Task InsertAsync(QueuedMailChimpCartSignUp obj);
        Task UpdateAsync(QueuedMailChimpCartSignUp obj);
        Task<List<QueuedMailChimpCartSignUp>> ListAsync();
    }
}
