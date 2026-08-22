using Nop.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Misc.ProductBundle.Domain
{

    public class ProductBundleSettings : ISettings
    {
        public decimal DiscountPercentage { get; set; }

        public string BundleVariantIds { get; set; }
    }
}
