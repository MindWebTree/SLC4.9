using AutoMapper;
using MWT.Nop.Core.Domain.KW;
using Nop.Core.Infrastructure.Mapper;
using Nop.Web.Areas.Admin.Models.Customization.Custom.KW;

namespace Nop.Web.Areas.Admin.Infrastructure.Mapper;

/// <summary>
/// AutoMapper configuration for admin area models
/// </summary>
public partial class CustomMapperConfiguration : Profile, IOrderedMapperProfile
{
    public CustomMapperConfiguration()
    {
        CreateKwtermMap();
    }
    public virtual void CreateKwtermMap()
    {
        CreateMap<ProductKwTerm, KwTermProductModel>();
        CreateMap<KwTermProductModel, ProductKwTerm>();
        CreateMap<KwTermCategoryModel, CategoryKwTerm>();
        CreateMap<CategoryKwTerm, KwTermCategoryModel>();
    }
    public int Order => 1;
}