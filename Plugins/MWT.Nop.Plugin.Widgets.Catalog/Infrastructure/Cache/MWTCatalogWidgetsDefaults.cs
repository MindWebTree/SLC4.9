using Nop.Core.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Widgets.Catalog.Infrastructure.Cache
{
    public static partial class MWTCatalogWidgetsDefaults
    {
        public static string MWTCatalogWidgetPrefix => "MWT.MWTEntityBanner.";
        public static CacheKey MWTCatalogWidgetCacheKey => new CacheKey("MWT.MWTEntityBanner.{0}-{1}-{2}");
    }
}
