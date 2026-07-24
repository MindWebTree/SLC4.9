using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.Custom.Campaign_Management
{
    public partial class MWT_Popup_Position:BaseEntity
    {
        public string PositionName { get; set; }
        public string PositionStyle { get; set; }
        public string IconPositionName { get; set; }
        public string IconPositionStyle { get; set; }
    }
}
