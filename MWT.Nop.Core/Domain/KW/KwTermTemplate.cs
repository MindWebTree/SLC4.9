using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.KW
{

    public partial class KwTermTemplate : BaseEntity
    {
        public string Name { get; set; }
        public string ViewPath { get; set; }
        public int DisplayOrder { get; set; }
        public string GridLineViewPath { get; set; }
        public string FilterViewPath { get; set; }
        public string FilterViewPathForMobile { get; set; }
        public int PageSize_Extension_Third_Banner { get; set; }
        public int PictureSize { get; set; }
        public bool IsHorizontal { get; set; }

    }
}
