using MWT.Nop.Core.Services.AbandonedCarts;
using MWT.Nop.Core.Services.Customers;
using MWT.Plugin.Misc.MwtStorefront.Models.AbandonedCarts;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Orders;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Web.Models.ShoppingCart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Factories
{
    public partial class AbandonedCartModelFactory : IAbandonedCartModelFactory
    {

        #region Fields

        private readonly IAbandonedCartService _abandonedCartService;
        private readonly IAddressExtendedModelFactory _addressModelFactory;
        private readonly IAddressService _addressService;
        private readonly ICustomerExtendedService _customerService;
        private readonly AddressSettings _addressSettings;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IShoppingCartExtendedModelFactory _shoppingCartModelFactory;
        private IStoreContext _storeContext;

        #endregion

        #region Ctor

        public AbandonedCartModelFactory(IAbandonedCartService abandonedCartService,
            IAddressExtendedModelFactory addressModelFactory, IAddressService addressService,
            ICustomerExtendedService customerService,
            AddressSettings addressSettings, IShoppingCartService shoppingCartService,
            IShoppingCartExtendedModelFactory shoppingCartModelFactory, IStoreContext storeContext)
        {
            _abandonedCartService = abandonedCartService;
            _addressModelFactory = addressModelFactory;
            _addressService = addressService;
            _customerService = customerService;
            _addressSettings = addressSettings;
            _shoppingCartService = shoppingCartService;
            _shoppingCartModelFactory = shoppingCartModelFactory;
            _storeContext = storeContext;

        }


        #endregion

        #region Methods
        public async Task<AbandonedCartModel> PrepareAbandonedCartModel(string invoiceId)
        {
            AbandonedCartModel model = new AbandonedCartModel();

            bool isValid = Guid.TryParse(invoiceId, out Guid invId);
            if (!isValid)
            {
                model.ErrorType = AbandonedCartErrorType.InValidInvoiceId;
                return model;
            }

            var invoice = await _abandonedCartService.GetAbandonedInvoiceByGuid(invId);
            if (invoice == null)
            {
                model.ErrorType = AbandonedCartErrorType.InValidInvoiceId;
                return model;
            }
            if (invoice.OrderNumber > 0)
            {
                model.ErrorType = AbandonedCartErrorType.Processed;
                return model;
            }
            var customer = await _customerService.GetCustomerByIdAsync(invoice.CustomerId);
            if (customer == null)
            {
                model.ErrorType = AbandonedCartErrorType.InValidInvoiceId;
                return model;
            }
            model.ImpersonateUser = true;
            model.Customer = customer;
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
            if (cart.Count == 0)
            {
                model.ErrorType = AbandonedCartErrorType.Empty;
                return model;
            }
            var cartModel = new ShoppingCartModel();

            model.Cart = await this._shoppingCartModelFactory.ModifyCartItemModelForCustomUpdates(await _shoppingCartModelFactory.PrepareShoppingCartModelAsync(cartModel, cart), customer);

            var shippingAddress = await _addressService.GetAddressByIdAsync(customer.ShippingAddressId ?? 0);
            if (shippingAddress != null)
            {
                await _addressModelFactory.PrepareCustomAddressModelAsync(model.ShippingAddress,
                    address: shippingAddress,
                    excludeProperties: false,
                    addressSettings: _addressSettings);
            }
            var billingAddress = await _addressService.GetAddressByIdAsync(customer.BillingAddressId ?? 0);
            if (billingAddress != null)
            {
                await _addressModelFactory.PrepareCustomAddressModelAsync(model.BillingAddress,
                address: shippingAddress,
                excludeProperties: false,
                addressSettings: _addressSettings);
            }


            return model;
        }


        public async Task<List<AbandonedCartModel>> AbandonedCardList()
        {
            var abandCarts = await _abandonedCartService.AbandonedCarts();
            List<AbandonedCartModel> abandCartModels = new List<AbandonedCartModel>();
            foreach (var abandonedCart in abandCarts)
            {
                var customer = await _customerService.GetCustomerByIdAsync(abandonedCart.CustomerId);
                if (customer != null)
                {

                    var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
                    if (cart.Count > 0)
                    {
                        customer.Email = await this._customerService.GetCustomerEmailAsync(customer);
                        if (!string.IsNullOrEmpty(customer.Email))
                        {
                            abandCartModels.Add(new AbandonedCartModel()
                            {
                                CreatedOn = abandonedCart.CreatedOn.Value,
                                Customer = customer,
                                InvoiceId = abandonedCart.Guid,
                                NoOfCartItem = cart.Count
                            });
                        }
                    }
                }
            }
            return abandCartModels;
        }


        #endregion
    }
}
