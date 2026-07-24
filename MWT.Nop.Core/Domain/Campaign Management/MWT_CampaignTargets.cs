using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.Custom.Campaign_Management
{
    public partial class MWT_CampaignTargets:BaseEntity
    {
        public int? CampaignId { get; set; }
        public string Url { get; set; }
        public int? ConditionId { get; set; }
    }
}
