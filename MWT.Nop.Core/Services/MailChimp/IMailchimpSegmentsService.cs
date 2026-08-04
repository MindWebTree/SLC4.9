using MWT.Nop.Core.Domain.Mailchimp;

namespace MWT.Nop.Core.Services.MailChimp
{
    public  partial interface IMailchimpSegmentsService
    {
        Task<List<MailchimpSegments>> Segments();
        Task Insert(MailchimpSegments segment);
    }
}
