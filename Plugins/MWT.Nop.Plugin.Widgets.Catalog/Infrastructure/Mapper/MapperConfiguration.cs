using AutoMapper;
using MWT.Nop.Plugin.Widgets.Catalog.Domain;
using MWT.Nop.Plugin.Widgets.Catalog.Models;
using Nop.Core.Infrastructure.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Widgets.Catalog.Infrastructure.Mapper
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        #region Ctor

        public MapperConfiguration()
        {
            CreateMap<MWTEntityBannerModel, MWTEntityBanner>(); 
        }

        #endregion
        public int Order => 1;
    }
}