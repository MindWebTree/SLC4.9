using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Area.Admin.Models.Variant
{
    public partial record VariantBundleSearchModel : BaseSearchModel
    {
        public int ProductId { get; set; }
    }
}
