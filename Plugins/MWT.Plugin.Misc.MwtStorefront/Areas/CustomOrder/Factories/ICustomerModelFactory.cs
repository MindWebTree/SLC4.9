using Nop.Core.Domain.Customers;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Customers;
using MWT.Nop.Core.Domain.Address;


namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Factories
{
    public partial interface ICustomerModelFactory
    {
        Task<CustomerSearchModel> PrepareCustomerSearchModelAsync(CustomerSearchModel searchModel);
        Task<CustomerListModel> PrepareCustomerListModelAsync(CustomerSearchModel searchModel);
        Task<CustomerModel> PrepareCustomerModel(Customer customer, bool bindShippingAddress = false, bool bindBillingAddress = false);
        Task<CustomerInfoModel> PrepareCustomerInfoModelAsync(Customer customer);
         Task<AddressModel> PrepareCustomAddressModelAsync(
            Customer customer, AddressType addressType, int addressId,
            AddressModel address, 
            int? selectedCountryId = null, bool prePopulateNewAddressWithCustomerFields = false,
            string overrideAttributesXml = "", bool isEdit = false);
        Task<CustomerAddressListModel> PrepareCustomerAddressListModelAsync(Customer customer);
      
    }
}
