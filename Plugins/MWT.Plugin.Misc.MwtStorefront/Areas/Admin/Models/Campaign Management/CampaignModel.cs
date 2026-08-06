using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Models;
using System;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.Campaign_Management
{
    public partial record CampaignModel : BaseNopEntityModel
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

        public string TimeSpan { get; set; }
    }
}
