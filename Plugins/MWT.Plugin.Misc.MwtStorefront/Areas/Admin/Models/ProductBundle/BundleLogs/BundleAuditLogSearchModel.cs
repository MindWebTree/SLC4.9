using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Area.Admin.BundleLogs
{
    public record BundleAuditLogSearchModel : BaseSearchModel
    {
        public int ProductId { get; set; }
        public int VariantId { get; set; }
        public string SearchKeyword { get; set; }
    }

}
