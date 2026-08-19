using AutoMapper;
using MWT.Nop.Plugin.Misc.ProductBundle.Models;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Misc.ProductBundle.Infrastructure
{
    public class PluginMapperConfiguration : Profile, IOrderedMapperProfile
    {
        public PluginMapperConfiguration()
        {
            // Tells AutoMapper how to convert the core entity into your custom model
            CreateMap<ProductAttributeMapping, ProductAttributeMappingBundleModule>()
                // Ignore the properties that are handled manually in your service or don't map directly
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableProductAttributes, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.Ignore())
                .ForMember(dest => dest.ConditionModel, mo => mo.Ignore())
                .ForMember(dest => dest.ProductAttributeValueSearchModel, mo => mo.Ignore());
        }

        // The order parameter dictates when this profile is registered. 
        // 0 is usually reserved for NopCommerce core profiles.
        public int Order => 1;
    }
}
