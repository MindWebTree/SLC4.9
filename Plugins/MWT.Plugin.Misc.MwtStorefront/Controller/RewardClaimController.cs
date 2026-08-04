using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Domain.RewardClaim;
using MWT.Nop.Core.Services.Manage;
using MWT.Nop.Core.Services.Search.RewardClaim;
using MWT.Plugin.Misc.MwtStorefront.Models.RewardClaim;

namespace Nop.Web.Controllers
{
    public partial class RewardClaimController : BasePublicController
    {

        private readonly IRewardClaimService _rewardClaimService;
        private readonly IManageService _manageService;
        public RewardClaimController(IRewardClaimService rewardClaimService, IManageService manageService)
        {
            _rewardClaimService = rewardClaimService;
            _manageService = manageService;
        }

        #region Methods


        public async Task<IActionResult> Index(int? orderId, string email)
        {
            var model = new RewardClaimRequestModel();

            if (orderId.HasValue && !string.IsNullOrEmpty(email))
            {
                // call external API
                (string comment, string errorMessage) = await _manageService.GetOrderFeedback(orderId.Value, email);
                //model.ErrorMessage = errorMessage;
                model.ReviewComment = comment;
                model.OrderId = orderId.Value;
                model.EmailAddress = email;
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(RewardClaimRequestModel model)
        {
            if (ModelState.IsValid)
            {
                (bool IsSaved, string ErrorMessage) = await _manageService.SaveOrderFeedbackRewardClaim(model.OrderId, model.EmailAddress
                , model.ReviewComment, model.SelectedGift.ToString(), model.IsGoogleReview, model.IsInstagramReview, model.IsPreferVideo);


                if (IsSaved)
                {
                    var rewardClaimRequest = new RewardClaimRequest
                    {
                        OrderId = model.OrderId,
                        EmailAddress = model.EmailAddress,
                        SelectedGift = model.SelectedGift.ToString(),
                        IsGoogleReview = model.IsGoogleReview,
                        IsInstagramReview = model.IsInstagramReview,
                        IsPreferVideo = model.IsPreferVideo,
                        CreatedOnUtc = DateTime.UtcNow
                    };
                    await _rewardClaimService.CreateRewardClaimRequest(rewardClaimRequest);
                    model = new RewardClaimRequestModel();
                }
                else
                {

                    model.ErrorMessage = ErrorMessage;
                }
                model.ShowModal = true;
            }
            return View(model);
        }


        #endregion
    }
}