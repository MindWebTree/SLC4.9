using Nop.Core.Caching;
using Nop.Services.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.TagPage.Cache
{
    public static partial class NopTagCatalogDefaults
    {
        public static CacheKey FilterableCategoriesByTagSegmentCacheKey => new CacheKey("Nop.specificationattributeoption.bytagsegment.{0}.{1}");
        public static CacheKey TagsSitemapSeoModelKey => new CacheKey("Nop.pres.sitemap.tags-{0}-{1}-{2}-{3}");

    }
}
