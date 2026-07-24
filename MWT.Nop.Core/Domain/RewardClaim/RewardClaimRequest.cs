using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.RewardClaim
{
    public partial class RewardClaimRequest : BaseEntity
    {
        public int OrderId { get; set; }
        public string EmailAddress { get; set; }
        public string SelectedGift { get; set; }
        public bool IsGoogleReview { get; set; }
        public bool IsInstagramReview { get; set; }
        public bool IsPreferVideo { get; set; }
        public string Comments { get; set; }
        public DateTime CreatedOnUtc { get; set; }

    }
}
