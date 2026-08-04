using MWT.Nop.Core.Domain.RewardClaim;
using Nop.Data;

namespace MWT.Nop.Core.Services.Search.RewardClaim
{
    public partial class RewardClaimService : IRewardClaimService
    {
        #region Fields

        private readonly IRepository<RewardClaimRequest> _rewardClaimRepository;

        #endregion

        #region Ctor

        public RewardClaimService(IRepository<RewardClaimRequest> rewardClaimRepository)
        {
            _rewardClaimRepository = rewardClaimRepository;
        }

        #endregion

        #region Methods
        public async Task CreateRewardClaimRequest(RewardClaimRequest request)
        {
            await _rewardClaimRepository.InsertAsync(request);
        }

        #endregion
    }
}
