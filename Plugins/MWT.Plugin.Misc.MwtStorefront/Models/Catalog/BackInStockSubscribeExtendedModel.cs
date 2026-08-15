using Nop.Web.Models.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Models.Catalog
{
    public record BackInStockSubscribeExtendedModel : BackInStockSubscribeModel
    {
        public string Email { get; set; }
        public int CategoryId { get; set; }
        public string SeName { get; set; }
    }
}
