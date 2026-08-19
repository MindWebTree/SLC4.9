using Nop.Core.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Misc.ProductBundle.Infrastructure.Cache
{
    public static partial class NopBundleCatalogDefaults
    {
        public static CacheKey BundlePictureModelKey => new CacheKey("Nop.pres.bundle.picture-{0}-{1}-{2}-{3}-{4}-{5}", BundlePicturePrefixCacheKey, BundlePicturePrefixCacheKeyById);
        public static string BundlePicturePrefixCacheKey => "Nop.pres.bundle.picture";
        public static string BundlePicturePrefixCacheKeyById => "Nop.pres.bundle.picture-{0}-";

        public static CacheKey BundleWidgetModelKey => new CacheKey("Nop.pres.bundle.widget-{0}-{1}-{2}-{3}", BundleWidgetPrefixCacheKey, BundleWidgetPrefixCacheKeyById);
        public static string BundleWidgetPrefixCacheKey => "Nop.pres.bundle.widget";
        public static string BundleWidgetPrefixCacheKeyById => "Nop.pres.bundle.widget-{0}-";

    }
}
