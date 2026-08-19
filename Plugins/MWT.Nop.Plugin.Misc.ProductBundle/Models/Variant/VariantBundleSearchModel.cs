using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Misc.ProductBundle.Models.Variant
{
    public partial record VariantBundleSearchModel : BaseSearchModel
    {
        public int ProductId { get; set; }
    }
}
