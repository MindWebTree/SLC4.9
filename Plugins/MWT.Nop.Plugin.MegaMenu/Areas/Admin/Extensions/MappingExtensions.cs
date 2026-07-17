
//using MWT.Nop.Plugin.MegaMenu.AutoMapper;
//using MWT.Nop.Plugin.MegaMenu.Areas.Admin.Models;
//using MWT.Nop.Plugin.MegaMenu.Domain;

//namespace MWT.Nop.Plugin.MegaMenu.Areas.Admin.Extensions
//{
//    public static class MappingExtensions
//    {
//        public static MegaMenuSettingsModel ToModel(this MegaMenuSettings entity) => entity.MapTo<MegaMenuSettings, MegaMenuSettingsModel>();

//        public static MegaMenuSettings ToEntity(this MegaMenuSettingsModel model) => model.MapTo<MegaMenuSettingsModel, MegaMenuSettings>();

//        public static MegaMenuSettings ToEntity(
//          this MegaMenuSettingsModel model,
//          MegaMenuSettings destination)
//        {
//            return model.MapTo<MegaMenuSettingsModel, MegaMenuSettings>(destination);
//        }

//        public static MenuModel ToModel(this Menu entity) => entity.MapTo<Menu, MenuModel>();

//        public static Menu ToEntity(this MenuModel model) => model.MapTo<MenuModel, Menu>();

//        public static Menu ToEntity(this MenuModel model, Menu destination) => model.MapTo<MenuModel, Menu>(destination);

//        public static MenuItemModel ToModel(this MenuItem entity) => entity.MapTo<MenuItem, MenuItemModel>();

//        public static MenuItem ToEntity(this MenuItemModel model) => model.MapTo<MenuItemModel, MenuItem>();

//        public static MenuItem ToEntity(this MenuItemModel model, MenuItem destination) => model.MapTo<MenuItemModel, MenuItem>(destination);
//    }
//}
