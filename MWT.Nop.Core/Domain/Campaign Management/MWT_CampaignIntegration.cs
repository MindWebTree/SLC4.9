using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.Custom.Campaign_Management
{
    public partial class MWT_CampaignIntegration:BaseEntity
    {
        public string ServiceName { get; set; }
        public string Api { get; set; }
        public string DefaultlistingId { get; set; }
        public string Host { get; set; }
        public string ListName { get; set; }
        public string StoreUrl { get; set; }
    }
}
