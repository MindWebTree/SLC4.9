using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.Custom.Campaign_Management
{
    public partial class CampaignTargetOverView:BaseEntity
    {
        public string Url { get; set; }
        public int ConditionId { get; set; }
        public string Name { get; set; }
    }
}
