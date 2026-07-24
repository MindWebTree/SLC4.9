using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.Mailchimp
{
    public partial class MailchimpSegments : BaseEntity
    {
        public string NewsLetterForm { get; set; }
        public int SegmentID { get; set; }
        public string Listid { get; set; }
        public string SegmentName { get; set; }
        public string ListName { get; set; }

        public string CreatedBy { get; set; }
        public string UpdatedBY { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedON { get; set; }
    }
}
