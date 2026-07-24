using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.Custom.Campaign_Management
{
    public partial class CampaignOverView:BaseEntity
    {
        public string Title { get; set; }
        public bool IsActive { get; set; }
        public int TemplateId { get; set; }
        public DateTime createdOn { get; set; }
        public string Html { get; set; }
        public int Impressions { get; set; }
        public int Conversions { get; set; }
        public int NoOfMonthCreated { get; set; }
        public int NoOfYearCreated { get; set; }
        public int NoOfDayCreated { get; set; }
    }
}
