
//using Microsoft.AspNetCore.Mvc.Filters;
//using Microsoft.Extensions.DependencyInjection;
//using MWT.Nop.Plugin.MegaMenu.ActionFilters;
//using MWT.Nop.Plugin.MegaMenu.Areas.Admin.Models;
//using MWT.Nop.Plugin.MegaMenu.Domain;
//using MWT.Nop.Plugin.MegaMenu.Helpers;
//using MWT.Nop.Plugin.MegaMenu.Services;
//using Microsoft.Extensions.DependencyInjection.Extensions;

//namespace MWT.Nop.Plugin.MegaMenu.Infrastructure
//{
//    public class DependancyRegistrar : BaseDependancyRegistrarMwt
//    {
//        public override void CreateModelMappings()
//        {
//            this.CreateMvcModelMap<MegaMenuSettingsModel, MegaMenuSettings>();
//            this.CreateMvcModelMap<Menu, MWT.Nop.Plugin.MegaMenu.Areas.Admin.Models.MenuModel>();
//            this.CreateMvcModelMap<MenuItem, MWT.Nop.Plugin.MegaMenu.Areas.Admin.Models.MenuItemModel>();
//            this.CreateMvcModelMap<Menu, MWT.Nop.Plugin.MegaMenu.Areas.Admin.Models.MenuModel>();
//            this.CreateMvcModelMap<MenuItem, MWT.Nop.Plugin.MegaMenu.Areas.Admin.Models.MenuItemModel>();
//        }


//        //protected override void RegisterPluginServices(IServiceCollection services)
//        //{

//        //    services.AddSingleton<IViewLocationsManager, ViewLocationsManager>();
//        //    services.AddTransient<IThemeService, ThemeService>();
//        //    services.AddTransient<IMappingInstallerService, MappingsInstallerService>();
//        //    services.AddTransient<IEntityWidgetMappingService, EntityWidgetMappingService>();
//        //    services.AddTransient<IEntityMappingService, EntityMappingService>();
//        //    services.AddTransient<IMenuService, MenuService>();
//        //    services.AddTransient<IMenuItemService, MenuItemService>();
//        //    services.AddScoped<IMegaMenuCategoryCounterHelper, MegaMenuCategoryCounterHelper>();
//        //    services.AddScoped<IInstallHelper, InstallHelper>();
//        //    services.AddScoped<IAccessControlHelper, AccessControlHelper>();
//        //}
//    }
//}
