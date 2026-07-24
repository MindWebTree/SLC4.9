using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.Custom.Campaign_Management
{

    public partial class MWT_Campaign : BaseEntity
    {
        public string Title { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? ShowOnExit { get; set; }
        public int? TemplateId { get; set; }
        public bool? IsGlobal { get; set; }
        public int? TriggerOn { get; set; }
        public int? FrequencyId { get; set; }
        public int? DeviceType { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string MailchimpListId { get; set; }
        public bool IsDeleted { get; set; }
        public int? ResponseTemplateId { get; set; }
        public string IconImage { get; set; }
        public int? PositionId { get; set; }
        public bool? IsExitPopUp { get; set; }
        public int? IconPositionId { get; set; }

        public int? SubscribedUserResponseTemplateId { get; set; }
        public string IconHtml { get; set; }

    }
}
