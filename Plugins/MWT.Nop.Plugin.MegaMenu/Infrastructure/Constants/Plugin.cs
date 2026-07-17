

using MWT.Nop.Plugin.MegaMenu.Domain.Enums;
using System.Collections.Generic;

namespace MWT.Nop.Plugin.MegaMenu.Infrastructure.Constants
{
    public static class Plugin
    {
        public const string SystemName = "MWT.Nop.Plugin.MegaMenu";
        public const string FolderName = "MWT.Nop.Plugin.MegaMenu";
        public const string ControllerName = "MWT.Nop.Plugin.MegaMenu.Controllers";
        public const string Name = "Nop Mega Menu";
        public const string ResourceName = "MWT.Plugin.MegaMenu.Admin.Menu.MenuName";
        public const string UrlInStore = "";
        public const string ContextName = "7spikes_mega_menu_object_context";
        public const string MegaMenuTableName = "SS_MM_Menu";
        public const string MegaMenuItemTableName = "SS_MM_MenuItem";
        public static EntityType EntityType = EntityType.Menu;

        public static IList<MenuItemType> PredefinedPageTypes = (IList<MenuItemType>)new List<MenuItemType>()
    {
      MenuItemType.HomePage,
      MenuItemType.CustomerInfo,
      MenuItemType.ContactUs,
      MenuItemType.NewProducts,
      MenuItemType.Blog,
      MenuItemType.NewsArchive,
      MenuItemType.Boards,
      MenuItemType.ManufacturerList,
      MenuItemType.VendorList,
      MenuItemType.ProductTagsAll,
      MenuItemType.ProductSearch,
      MenuItemType.ShoppingCart,
      MenuItemType.Wishlist,
      MenuItemType.CompareProducts,
      MenuItemType.Checkout,
      MenuItemType.Login,
      MenuItemType.Register,
      MenuItemType.Sitemap,
      MenuItemType.RecentlyViewedProducts
    };
    }
}
