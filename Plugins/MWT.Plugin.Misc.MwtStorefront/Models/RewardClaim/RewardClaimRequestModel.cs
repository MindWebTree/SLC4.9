using MWT.Nop.Core.Domain.RewardClaim;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace MWT.Plugin.Misc.MwtStorefront.Models.RewardClaim
{
    public partial record RewardClaimRequestModel : BaseNopEntityModel
    {

        [NopResourceDisplayName("RewardClaim.Fields.OrderId")]
        public int OrderId { get; set; }

        [DataType(DataType.EmailAddress)]
        [NopResourceDisplayName("RewardClaim.Fields.Email")]
        public string EmailAddress { get; set; }
        [NopResourceDisplayName("RewardClaim.Fields.SelectedGift")]
        public RewardClaimGift SelectedGift { get; set; }
        public bool IsGoogleReview { get; set; }
        public bool IsInstagramReview { get; set; }
        public bool IsPreferVideo { get; set; }
        public string ReviewComment { get; set; }
        public string ErrorMessage { get; set; }
        public bool ShowModal { get; set; }
    }
}