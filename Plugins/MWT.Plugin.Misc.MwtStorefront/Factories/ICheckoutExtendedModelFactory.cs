using MWT.Plugin.Misc.MwtStorefront.Models.Checkout;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Web.Factories;
using Nop.Web.Models.Checkout;
using Nop.Web.Models.Common;
namespace MWT.Plugin.Misc.MwtStorefront.Factories;
public partial interface ICheckoutExtendedModelFactory : ICheckoutModelFactory
{
    Task<OnePageCheckoutModel> PrepareCustomOnePageCheckoutModelAsync(IList<ShoppingCartItem> cart);
    Task<CheckoutAddressModel> PrepareCustomShippingAddressModelAsync(Customer customer, AddressModel address, IList<ShoppingCartItem> cart,
        int? selectedCountryId = null, bool prePopulateNewAddressWithCustomerFields = false, string overrideAttributesXml = "", bool isEdit = false);
    Task<CheckoutAddressModel> PrepareCustomBillingAddressModelAsync(AddressModel address, IList<ShoppingCartItem> cart,
        int? selectedCountryId = null, bool prePopulateNewAddressWithCustomerFields = false, string overrideAttributesXml = "", bool isEdit = false);
    Task<CheckoutShippingMethodModel> PrepareCustomShippingMethodModelAsync(IList<ShoppingCartItem> cart, Address shippingAddress);

    Task<(bool, ZipCodeTaxRateModel model)> GetTaxByZipCode(string zipCode);
}
