using ExCSS;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Authentication;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Controllers
{
    public class ReInitiateCheckoutController : BasePublicController
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreContext _storeContext;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private IHttpContextAccessor _httpContextAccessor;

        #endregion

        #region Ctor

        public ReInitiateCheckoutController(IWorkContext workContext, ICustomerService customerService,
                                            IAuthenticationService authenticationService, IShoppingCartService shoppingCartService,
                                            IStoreContext storeContext, ICustomerRegistrationService customerRegistrationService,
                                            IHttpContextAccessor httpContextAccessor)
        {

            _workContext = workContext;
            _authenticationService = authenticationService;
            _customerService = customerService;
            _shoppingCartService = shoppingCartService;
            _storeContext = storeContext;
            _customerRegistrationService = customerRegistrationService;
            _httpContextAccessor = httpContextAccessor;
        }


        #endregion

        #region Methods
        public async Task<IActionResult> Index(int customerId)
        {
            var routeValues = new Dictionary<string, object>();
            foreach (var query in this._httpContextAccessor.HttpContext.Request.Query)
            {
                if (!routeValues.ContainsKey(query.Key))
                {
                    routeValues.Add(query.Key, query.Value);
                }
            }
            var customer = await _customerService.GetCustomerByIdAsync(customerId);
            if (customer == null)
            {
                return RedirectToRoute("PageNotFound", routeValues);
            }

            if (await _customerService.IsRegisteredAsync(customer))
            {
                return await _customerRegistrationService.SignInCustomerAsync(customer, Url.RouteUrl("Checkout", routeValues), true);
            }
            else
            {
                await _workContext.SetCurrentCustomerAsync(customer);
            }
            return RedirectToRoute("Checkout", routeValues);

        }

        #endregion
    }
}
