using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Models.Customization.Custom
{
    public partial record FiltersMappingByEntitySearchModel: BaseSearchModel
    {
        public int EntityId { get; set; }
        public string EntityType { get; set; }

        public string FilterType { get; set; }
    }
}
