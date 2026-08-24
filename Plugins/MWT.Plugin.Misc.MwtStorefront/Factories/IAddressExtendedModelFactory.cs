using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Web.Factories;
using Nop.Web.Models.Common;


namespace MWT.Plugin.Misc.MwtStorefront.Factories
{
    public partial interface IAddressExtendedModelFactory:IAddressModelFactory
    {
        Task PrepareCustomAddressModelAsync(AddressModel model,
       Address address, bool excludeProperties,
       AddressSettings addressSettings,
       Func<Task<IList<Country>>> loadCountries = null,
       bool prePopulateWithCustomerFields = false,
       Customer customer = null,
       string overrideAttributesXml = "");
    }
}
