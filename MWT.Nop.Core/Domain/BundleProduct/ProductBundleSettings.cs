using Nop.Core.Configuration;

namespace MWT.Nop.Core.Domain.ProductBundle
{

    public class ProductBundleSettings : ISettings
    {
        public decimal DiscountPercentage { get; set; }

        public string BundleVariantIds { get; set; }
    }
}
