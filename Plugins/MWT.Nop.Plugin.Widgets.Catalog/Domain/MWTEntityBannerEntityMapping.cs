using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Widgets.Catalog.Domain
{
    public partial class MWTEntityBannerEntityMapping:BaseEntity
    {
        public int EntityId { get; set; }
        public int BannerId { get; set; }
    }
}
