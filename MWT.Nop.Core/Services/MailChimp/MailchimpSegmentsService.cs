using MWT.Nop.Core.Domain.Mailchimp;
using Nop.Data;

namespace MWT.Nop.Core.Services.MailChimp
{
    public partial class MailchimpSegmentsService : IMailchimpSegmentsService
    {
        #region Fields

        private readonly IRepository<MailchimpSegments> _repository;

        #endregion

        #region ctor
        public MailchimpSegmentsService(IRepository<MailchimpSegments> repository)
        {
            this._repository = repository;
        }
        #endregion

        #region Methods
        public async Task<List<MailchimpSegments>> Segments()
        {
            return await this._repository.Table.ToListAsync();
        }
        public async Task Insert(MailchimpSegments segment)
        {
            await this._repository.InsertAsync(segment);
        }
     
   
        #endregion
    }
}
