using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain
{
    public partial class Testimonial : BaseEntity
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public DateTime CreatedOn { get; set; }
        public string VideoUrl { get; set; }
        public bool IsConsent { get; set; }
    }
}
