using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.Custom.Campaign_Management
{
    public partial class CampaignTargetingSave : BaseEntity
    {
        public int CampaignId { get; set; }
        public bool IsActive { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsExitPopup { get; set; }

        public int? TriggerOn { get; set; }
        public int? FrequencyId { get; set; }
        public int? DeviceType { get; set; }
        public bool IsGlobal { get; set; }
        public string Conditions { get; set; }
        public int? Position { get; set; }
        public int? IconPosition { get; set; }
        public string IconImageSrc { get; set; }
        public string IconHtml { get; set; }
        public string Name { get; set; }

        public bool TriggerOnExit { get; set; }
        public int ResponseTemplateId { get; set; }
        public int TemplateId { get; set; }
    }
}
