using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.Custom.Campaign_Management
{
    public partial class MWT_CampaignEvent : BaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description
        {
            get; set;
        }
    }
}
