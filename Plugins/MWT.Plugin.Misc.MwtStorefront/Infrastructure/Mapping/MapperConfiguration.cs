using AutoMapper;
using MWT.Nop.Core.Domain;
using MWT.Nop.Core.Domain.Catalog;
using MWT.Nop.Core.Domain.Custom.Campaign_Management;
using MWT.Nop.Core.Domain.Custom.LandingPage_Management;
using MWT.Nop.Core.Domain.QA;
using MWT.Plugin.Misc.MwtStorefront.Area.Admin.Models.Catalog.LandingPage_Management;
using MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models;
using MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.Campaign_Management;
using MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.QA;
using MWT.Plugin.Misc.MwtStorefront.Models.Catalog;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure.Mapper;

namespace MWT.Plugin.Misc.MwtStorefront.Infrastructure
{
  
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        public MapperConfiguration()
        {
            CreateProductTemplateSectionMap();
            CreateProductTemplateMap();
        }
        #region Utilities

        public virtual void CreateProductTemplateSectionMap()
        {
            CreateMap<ProductTemplateSection, ProductTemplateSectionModel>();
            CreateMap<ProductTemplateSectionModel, ProductTemplateSection>();
        }
        public virtual void CreateProductTemplateMap()
        {
            CreateMap<ProductTemplate, ProductTemplateModel>();
            CreateMap<ProductTemplateModel, ProductTemplate>();
        }
        #endregion

        #region Properties

        /// <summary>
        /// Order of this mapper implementation
        /// </summary>
        public int Order => 0;

        #endregion
    }
}