using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Messages
{
    /// <summary>
    /// Represents NewsLetterSubscription entity
    /// </summary>
    public partial class NewsLetterSubscription : BaseEntity
    {
        public NewsLetterSubscription()
        {
            UpdatedOnUtc = DateTime.UtcNow;
        }
        private DateTime _updatedOnUtc;
        public DateTime UpdatedOnUtc { get; set; }

        public string MailChimpListId { get; set; }
        public string MailChimpAppId { get; set; }
        public DateTime? UnsubscribedOn { get; set; }
        public string IpAddress { get; set; }

        public string Section { get; set; }
        public string Url { get; set; }

        public string UserAgent { get; set; }
    }
}
