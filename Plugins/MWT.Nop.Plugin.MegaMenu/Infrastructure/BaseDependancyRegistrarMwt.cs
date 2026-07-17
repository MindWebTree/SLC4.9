
//using AutoMapper;
//using Microsoft.AspNetCore.Builder;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using MWT.Nop.Plugin.MegaMenu.Areas.Admin.Models;
//using MWT.Nop.Plugin.MegaMenu.AutoMapper;
//using MWT.Nop.Plugin.MegaMenu.Domain;
//using Nop.Core.Configuration;
//using Nop.Core.Infrastructure; 

//namespace MWT.Nop.Plugin.MegaMenu.Infrastructure
//{
//    public abstract class BaseDependancyRegistrarMwt 
//    {
//        //public virtual void CreateModelMappings()
//        //{

//        //    this.CreateMvcModelMap<MegaMenuSettingsModel, MegaMenuSettings>();
//        //    this.CreateMvcModelMap<Menu, MWT.Nop.Plugin.MegaMenu.Areas.Admin.Models.MenuModel>();
//        //    this.CreateMvcModelMap<MenuItem, MWT.Nop.Plugin.MegaMenu.Areas.Admin.Models.MenuItemModel>();
//        //    this.CreateMvcModelMap<Menu, MWT.Nop.Plugin.MegaMenu.Areas.Admin.Models.MenuModel>();
//        //    this.CreateMvcModelMap<MenuItem, MWT.Nop.Plugin.MegaMenu.Areas.Admin.Models.MenuItemModel>();
//        //}

//        //protected void CreateMvcModelMap<TModel, TEntity>()
//        //{
//        //    ((Profile)AutoMapperConfigurationMwt.MapperConfigurationExpression).CreateMap<TModel, TEntity>();
//        //    ((Profile)AutoMapperConfigurationMwt.MapperConfigurationExpression).CreateMap<TEntity, TModel>();
//        //    ((Profile)AutoMapperConfigurationMwt.MapperConfigurationExpression).CreateMap<TEntity, TModel>();
//        //}

//        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
//        {

//        }

//        public void Configure(IApplicationBuilder application)
//        {

//        }
//        //protected virtual void RegisterPluginServices(IServiceCollection services)
//        //{
//        //}
//        public virtual int Order => (int)short.MaxValue;
//    }
//}
