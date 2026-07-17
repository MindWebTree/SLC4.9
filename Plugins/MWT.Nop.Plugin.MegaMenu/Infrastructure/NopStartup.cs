
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MWT.Nop.Plugin.MegaMenu.ActionFilters;
using MWT.Nop.Plugin.MegaMenu.Areas.Admin.Models;
using MWT.Nop.Plugin.MegaMenu.AutoMapper;
using MWT.Nop.Plugin.MegaMenu.Domain;
using MWT.Nop.Plugin.MegaMenu.Helpers;
using MWT.Nop.Plugin.MegaMenu.Services;
using MWT.Nop.Plugin.MegaMenu.ViewLocations;
using Nop.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MWT.Nop.Plugin.MegaMenu.Infrastructure
{
    public class NopStartup : INopStartup
    {
        public int Order => 2000;

        public void Configure(IApplicationBuilder application)
        {
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IViewLocationsManager, ViewLocationsManager>();
            services.AddTransient<IThemeService, ThemeService>();
            services.AddTransient<IMappingInstallerService, MappingsInstallerService>();
            services.AddTransient<IEntityWidgetMappingService, EntityWidgetMappingService>();
            services.AddTransient<IEntityMappingService, EntityMappingService>();
            services.AddTransient<IMenuService, MenuService>();
            services.AddTransient<IMenuItemService, MenuItemService>();
            services.AddScoped<IMegaMenuCategoryCounterHelper, MegaMenuCategoryCounterHelper>();
            services.AddScoped<IInstallHelper, InstallHelper>();
            services.AddScoped<IAccessControlHelper, AccessControlHelper>();
            services.Configure<RazorViewEngineOptions>((Action<RazorViewEngineOptions>)(options => options.ViewLocationExpanders.Add((IViewLocationExpander)new ThemeablePluginViewLocationExpander())));
            GlobalActionFiltersProvider implementationInstance = new GlobalActionFiltersProvider();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IFilterProvider>((IFilterProvider)implementationInstance));
            services.Add(ServiceDescriptor.Singleton(typeof(GlobalActionFiltersProvider), (object)implementationInstance));
            services.AddAutoMapper(typeof(NopStartup).Assembly);
        }
    }
}
