using MWT.Nop.Core.Domain.RewardClaim;

namespace MWT.Nop.Core.Services.Search.RewardClaim
{
    public partial interface IRewardClaimService
    {
        public Task CreateRewardClaimRequest(RewardClaimRequest request);
    }
}
