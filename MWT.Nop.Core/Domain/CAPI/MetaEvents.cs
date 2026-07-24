using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.CAPI
{
    public partial class MetaEvents : BaseEntity
    {
        public string EventId { get; set; }
        public int EventType { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string Fbp { get; set; }
        public string Fbc { get; set; }
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
        public int VariantId { get; set; }
        public int OrderId { get; set; }
        public string ReferralUrl { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ProcessedOn { get; set; }
        public int NoOfTries { get; set; }
        public MetaEventType EvenType
        {
            get => (MetaEventType)EventType;
            set => EventType = (int)value;
        }
    }
}
