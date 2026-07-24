using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.Campaign_Management
{
    public partial class MailchimpCampaignIntegration : BaseEntity
    {
        public string ApiKey { get; set; }
        public string ListId { get; set; }
        public string UrlCondition { get; set; }
        public string Host { get; set; }
        public string SegmentID { get; set; }
    }
}
