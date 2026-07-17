
using Nop.Core.Caching;

namespace MWT.Nop.Plugin.MegaMenu.Infrastructure.Constants
{
    public static class CacheKeys
    {
        public const string MenuCacheKey = "nop.pres.mwt.megamenu";

        public static CacheKey MenuModelCacheKey => new CacheKey("nop.pres.mwt.megamenu-{0}-{1}-{2}-{3}-{4}" );

        public static CacheKey MenuWidgetMappingsCacheKey => new CacheKey("nop.pres.mwt.megamenu.widgetmappings" );

        public static CacheKey MenusAllCacheKey => new CacheKey("nop.pres.mwt.megamenu.menusall" );

        public static CacheKey MenusByWidgetZonesCacheKey => new CacheKey("nop.pres.mwt.megamenu.menussbywidgetzones-{0}-{1}" );

        public static CacheKey MenuItems => new CacheKey("nop.pres.mwt.megamenu.menuitems" );

        public static CacheKey MenuTopicCacheKey => new CacheKey("nop.pres.mwt.megamenu.topic.bysystemname-{0}-{1}-{2}-{3}" );

        public static CacheKey MegaMenuCategoriesKey => new CacheKey("nop.pres.mwt.megamenu-categories-{0}-{1}-{2}-{3}-{4}-{5}" );
    }
}
