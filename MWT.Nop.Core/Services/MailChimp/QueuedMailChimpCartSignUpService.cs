using MWT.Nop.Core.Domain.Mailchimp;
using Nop.Data;

namespace MWT.Nop.Core.Services.MailChimp
{
    public partial class QueuedMailChimpCartSignUpService : IQueuedMailChimpCartSignUpService
    {
        #region Ctor

        private readonly IRepository<QueuedMailChimpCartSignUp> _queuedMailChimpCartSignUpRepository;
        public QueuedMailChimpCartSignUpService(IRepository<QueuedMailChimpCartSignUp> queuedMailChimpCartSignUpRepository)
        {
            _queuedMailChimpCartSignUpRepository = queuedMailChimpCartSignUpRepository;
        }

        #endregion

        #region Methods

        public async Task InsertAsync(QueuedMailChimpCartSignUp obj)
        {
            await _queuedMailChimpCartSignUpRepository.InsertAsync(obj);
        }
        public async Task UpdateAsync(QueuedMailChimpCartSignUp obj)
        {
            await _queuedMailChimpCartSignUpRepository.UpdateAsync(obj);
        }
        public async Task<List<QueuedMailChimpCartSignUp>> ListAsync()
        {
            return await (from queuedCartSignUp in _queuedMailChimpCartSignUpRepository.Table
                    where queuedCartSignUp.IsProcessed == false
                    && queuedCartSignUp.NoOfTries < 4
                    orderby queuedCartSignUp.Id select queuedCartSignUp).ToListAsync();

        }

        #endregion
    }
}
