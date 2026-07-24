using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.Custom.Campaign_Management
{
    public partial class MWT_CampaignAnalytics : BaseEntity
    {
        public string EmailId { get; set; }
        public string IpAddress { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? EventId { get; set; }
        public string Url { get; set; }
        public int? CampaignId { get; set; }
        public string MailchimpListId { get; set; }
        public int? DeviceType { get; set; }
        public string UserAgent { get; set; }
        public int? CountryId { get; set; }

    }
}
