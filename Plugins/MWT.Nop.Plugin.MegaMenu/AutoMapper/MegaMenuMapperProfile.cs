using AutoMapper;
using MWT.Nop.Plugin.MegaMenu.Areas.Admin.Models;
using MWT.Nop.Plugin.MegaMenu.Domain;
using Nop.Core.Infrastructure.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.MegaMenu.AutoMapper
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        public MapperConfiguration()
        {
            CreateMap<MegaMenuSettings, MegaMenuSettingsModel>().ReverseMap();
            CreateMap<Menu, MenuModel>().ReverseMap();
            CreateMap<MenuItem, MenuItemModel>().ReverseMap(); 
        } 
        // Defines the order in which this mapper is initialized
        public int Order => 1;
    }
}
