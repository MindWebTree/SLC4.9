using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.Custom.Campaign_Management
{
    public class CampaignPopupResult : BaseEntity
    {
        public int CampaignId { get; set; }
        public string Title { get; set; }
        public bool showOnExit { get; set; }
        public int triggerOn_Secs { get; set; }
        public string htmlContent { get; set; }
        public int duration_Days { get; set; }
        public string message { get; set; }
        public bool static_message { get; set; }
        public bool isError { get; set; }
        public string positionStyle { get; set; }
        public string iconPositionStyle { get; set; }
        public string iconImage { get; set; }
        public string IconHtml { get; set; }
        public bool IsSideBarDisplayed { get; set; }
        public bool IsUserSubscribed { get; set; }
  public bool IsPopupDisplayed { get; set; }
    }
}
