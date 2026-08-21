using AutoMapper;
using Nop.Core.Domain.Common;
using Nop.Core.Infrastructure.Mapper;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Customers;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Orders;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Mapper
{
    /// <summary>
    /// AutoMapper configuration for admin area models
    /// </summary>
    public class CustomOrderMapperConfiguration : Profile, IOrderedMapperProfile
    {
        #region Ctor

        public CustomOrderMapperConfiguration()
        {
            CreateCustomerMaps();
            CreateCustomOrderMaps();
        }

        #endregion

        #region Methods

        protected virtual void CreateCustomerMaps()
        {
            CreateMap<Address, AddressModel>()
                 .ForMember(model => model.AvailableCountries, options => options.Ignore())
                .ForMember(model => model.AvailableStates, options => options.Ignore())
                .ForMember(model => model.CountryName, options => options.Ignore())
                .ForMember(model => model.FormattedCustomAddressAttributes, options => options.Ignore())
                .ForMember(model => model.StateProvinceName, options => options.Ignore())
                .ForMember(model => model.CityRequired, options => options.Ignore())
                .ForMember(model => model.CompanyRequired, options => options.Ignore())
                .ForMember(model => model.CountyRequired, options => options.Ignore())
                .ForMember(model => model.FaxRequired, options => options.Ignore())
               .ForMember(model => model.PhoneRequired, options => options.Ignore())
                .ForMember(model => model.StateProvinceName, options => options.Ignore())
                .ForMember(model => model.StreetAddress2Required, options => options.Ignore())
                .ForMember(model => model.StreetAddressRequired, options => options.Ignore())
                .ForMember(model => model.ZipPostalCodeRequired, options => options.Ignore());
            CreateMap<AddressModel, Address>()
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.CustomAttributes, options => options.Ignore());
        }
        protected virtual void CreateCustomOrderMaps()
        {
            CreateMap<MWT.Nop.Core.Domain.CustomOrders.CustomOrder, CustomOrderModel>();
            CreateMap<CustomOrderModel, MWT.Nop.Core.Domain.CustomOrders.CustomOrder>();

            CreateMap<MWT.Nop.Core.Domain.CustomOrders.CustomOrderOrderType, CustomOrderOrderTypeModel>();
            CreateMap<CustomOrderOrderTypeModel, MWT.Nop.Core.Domain.CustomOrders.CustomOrderOrderType>();
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

