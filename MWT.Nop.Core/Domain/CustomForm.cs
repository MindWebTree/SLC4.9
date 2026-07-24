using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain
{

    /// <summary>
    /// Represents a related product
    /// </summary>
    public partial class CustomForm : BaseEntity
    {
        public string FormName { get; set; }
        public string FormHtml { get; set; }
        public bool ShowInPopup { get; set; }
        public string ThankYouHtml { get; set; }
        public string ThankYouPageLink { get; set; }
        public bool SendCustomerNotification { get; set; }
        public string BccEmailAddresses { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string CustomerNotificationEmailBody { get; set; }
        public string CustomerNotificationSubject { get; set; }
        public bool Published { get; set; }
        public bool Deleted { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool RenderActions { get; set; }
    }
}
