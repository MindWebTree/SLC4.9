using ExCSS;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Domain.AbandonedCarts;
using MWT.Plugin.Misc.MwtStorefront.Factories;
using MWT.Plugin.Misc.MwtStorefront.Models.AbandonedCarts;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Authentication;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MWT.Plugin.Misc.MwtStorefront.Controllers
{
    public class RebuildCartController : BasePublicController
    {
        #region Fields

        private readonly IAbandonedCartModelFactory _abandonedCartModelFactory;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreContext _storeContext;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly IProductService _productService;
        private readonly ILocalizationService _localizationService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IDiscountService _discountService;
        private IHttpContextAccessor _httpContextAccessor;

        #endregion

        #region Ctor

        public RebuildCartController(IAbandonedCartModelFactory abandonedCartModelFactory,
            IWorkContext workContext, ICustomerService customerService,
            IAuthenticationService authenticationService,
            IShoppingCartService shoppingCartService,
            IStoreContext storeContext,
            ICustomerRegistrationService customerRegistrationService,
            IProductService productService,
            ILocalizationService localizationService,
            IGenericAttributeService genericAttributeService,
            IDiscountService discountService,
            IHttpContextAccessor httpContextAccessor)
        {
            _abandonedCartModelFactory = abandonedCartModelFactory;
            _workContext = workContext;
            _authenticationService = authenticationService;
            _customerService = customerService;
            _shoppingCartService = shoppingCartService;
            _storeContext = storeContext;
            _customerRegistrationService = customerRegistrationService;
            _productService = productService;
            _localizationService = localizationService;
            _genericAttributeService = genericAttributeService;
            _discountService = discountService;
            _httpContextAccessor = httpContextAccessor;
        }

        #endregion

        public async Task<IActionResult> Index(string invoiceId, string discountCode)
        {
            var routeValues = new Dictionary<string, object>();
            foreach (var query in this._httpContextAccessor.HttpContext.Request.Query)
            {
                if (!routeValues.ContainsKey(query.Key))
                {
                    routeValues.Add(query.Key, query.Value);
                }
            }


            var model = await this._abandonedCartModelFactory.PrepareAbandonedCartModel(invoiceId);
            if (model.ErrorType ==AbandonedCartErrorType.InValidInvoiceId)
            {
                return RedirectToRoute("PageNotFound", routeValues);
            }
            if (model.ImpersonateUser)
            {
                if (model.Cart == null)
                {
                    return RedirectToRoute("ShoppingCart", routeValues);
                }
                // end 
                var currentCustomer = await _workContext.GetCurrentCustomerAsync();
                if (await _customerService.IsRegisteredAsync(currentCustomer) && currentCustomer.Id != model.Customer.Id)
                {


                    if (model.Cart.Items.Count == 0)
                        return RedirectToRoute("ShoppingCart", routeValues);

                }
                else
                {


                    #region Apply Coupon
                    if (!string.IsNullOrEmpty(discountCode))
                    {
                        var couponCodes = new List<string>();
                        var existingCouponCodes = await _genericAttributeService.GetAttributeAsync<string>(model.Customer, NopCustomerDefaults.DiscountCouponCodeAttribute);
                        if (string.IsNullOrEmpty(existingCouponCodes))
                        {
                            try
                            {
                                var xmlDoc = new XmlDocument();
                                xmlDoc.LoadXml(existingCouponCodes);

                                var nodeList1 = xmlDoc.SelectNodes(@"//DiscountCouponCodes/CouponCode");
                                foreach (XmlNode node1 in nodeList1)
                                {
                                    if (node1.Attributes?["Code"] == null)
                                        continue;
                                    var code = node1.Attributes["Code"].InnerText.Trim();
                                    couponCodes.Add(code.ToLower().Trim());
                                }
                            }
                            catch
                            {
                                // ignored
                            }
                        }
                        if (!String.IsNullOrWhiteSpace(discountCode) && (String.IsNullOrWhiteSpace(existingCouponCodes) || !couponCodes.Contains(discountCode.ToLower().Trim())))
                        {
                            var discounts = (await _discountService.GetAllDiscountsAsync(couponCode: discountCode, showHidden: true))
                     .Where(d => d.RequiresCouponCode)
                     .ToList();

                            if (discounts.Any())
                            {
                                var userErrors = new List<string>();
                                var anyValidDiscount = await discounts.AnyAwaitAsync(async discount =>
                                {
                                    var validationResult = await _discountService.ValidateDiscountAsync(discount, model.Customer, new[] { discountCode });
                                    userErrors.AddRange(validationResult.Errors);

                                    return validationResult.IsValid;
                                });

                                if (anyValidDiscount)
                                {
                                    //valid
                                    await _customerService.ApplyDiscountCouponCodeAsync(model.Customer, discountCode);

                                }
                            }
                        }
                    }

                    #endregion
                    if (await _customerService.IsRegisteredAsync(model.Customer))
                    {
                        return await _customerRegistrationService.SignInCustomerAsync(model.Customer, Url.RouteUrl("ShoppingCart", routeValues), true);
                    }
                    else
                    {
                        await _workContext.SetCurrentCustomerAsync(model.Customer);
                    }
                    return RedirectToRoute("ShoppingCart", routeValues);
                }



            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(string invoiceId, ICollection<int> selectedIds)
        {
            var routeValues = new Dictionary<string, object>();
            foreach (var query in this._httpContextAccessor.HttpContext.Request.Query)
            {
                if (!routeValues.ContainsKey(query.Key))
                {
                    routeValues.Add(query.Key, query.Value);
                }
            }
            var model = await this._abandonedCartModelFactory.PrepareAbandonedCartModel(invoiceId);
            if (model.ErrorType == AbandonedCartErrorType.InValidInvoiceId)
            {

                return RedirectToRoute("PageNotFound", routeValues);
            }
            if (model.ImpersonateUser)
            {
                var currentCustomer = await _workContext.GetCurrentCustomerAsync();
                if (await _customerService.IsRegisteredAsync(currentCustomer) && currentCustomer.Id != model.Customer.Id)
                {


                    if (model.Cart.Items.Count == 0)
                        return RedirectToRoute("ShoppingCart", routeValues);
                    else
                    {
                        if (selectedIds.Count > 0)
                        {
                            var cart = await _shoppingCartService.GetShoppingCartAsync(model.Customer, ShoppingCartType.ShoppingCart);
                            for (var i = 0; i < cart.Count; i++)
                            {
                                if (selectedIds.Contains(cart[i].Id))
                                {
                                    var sci = cart[i];
                                    var product = await _productService.GetProductByIdAsync(sci.ProductId);

                                    await _shoppingCartService.AddToCartAsync(currentCustomer, product, sci.ShoppingCartType, sci.StoreId,
                                             sci.AttributesXml, sci.CustomerEnteredPrice,
                                             sci.RentalStartDateUtc, sci.RentalEndDateUtc, sci.Quantity, false);
                                }
                            }
                            return RedirectToRoute("ShoppingCart", routeValues);
                        }
                        else
                        {
                            ModelState.AddModelError("", await _localizationService.GetResourceAsync("AbdandonedCart.Error.Select.Cart"));
                        }
                    }
                }
                else
                {
                    if (await _customerService.IsRegisteredAsync(model.Customer))
                    {
                        return await _customerRegistrationService.SignInCustomerAsync(model.Customer, Url.RouteUrl("ShoppingCart", routeValues), true);
                    }
                    else
                    {
                        await _workContext.SetCurrentCustomerAsync(model.Customer);
                    }
                    return RedirectToRoute("ShoppingCart", routeValues);
                }



            }
            return View(model);






        }


        public async Task<IActionResult> AbandonedCardList()
        {
            return View(await this._abandonedCartModelFactory.AbandonedCardList());
        }
    }
}
