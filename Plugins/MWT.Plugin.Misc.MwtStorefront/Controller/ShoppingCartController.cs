using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using MWT.Nop.Core.Infrastructure;
using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Services.AbandonedCarts;
using MWT.Nop.Core.Services.Catalog;
using MWT.Nop.Core.Services.Customers;
using MWT.Nop.Core.Services.Message;
using MWT.Plugin.Misc.MwtStorefront.Components;
using MWT.Plugin.Misc.MwtStorefront.Factories;
using MWT.Plugin.Misc.MwtStorefront.Models.Customer;
using MWT.Plugin.Misc.MwtStorefront.Models.ShoppingCart;
using MWTNop.Core.Domain.Catalog;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Core.Http;
using Nop.Core.Http.Extensions;
using Nop.Core.Infrastructure;
using Nop.Services.Attributes;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Components;
using Nop.Web.Controllers;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Media;
using Nop.Web.Models.ShoppingCart;
using System.Net;
namespace MWT.Plugin.Misc.MwtStorefront.Controller;

[AutoValidateAntiforgeryToken]
public partial class ShoppingCartController : BasePublicController
{

    #region Fields

    protected readonly CaptchaSettings _captchaSettings;
    protected readonly CustomerSettings _customerSettings;
    protected readonly IAttributeParser<CheckoutAttribute, CheckoutAttributeValue> _checkoutAttributeParser;
    protected readonly IAttributeService<CheckoutAttribute, CheckoutAttributeValue> _checkoutAttributeService;
    protected readonly ICurrencyService _currencyService;
    protected readonly ICustomerActivityService _customerActivityService;
    protected readonly ICustomerExtendedService _customerService;
    protected readonly ICustomWishlistService _customWishlistService;
    protected readonly IDiscountService _discountService;
    protected readonly IDownloadService _downloadService;
    protected readonly IGenericAttributeService _genericAttributeService;
    protected readonly IGiftCardService _giftCardService;
    protected readonly IHtmlFormatter _htmlFormatter;
    protected readonly ILocalizationService _localizationService;
    protected readonly INopFileProvider _fileProvider;
    protected readonly INopUrlHelper _nopUrlHelper;
    protected readonly INotificationService _notificationService;
    protected readonly IPermissionService _permissionService;
    protected readonly IPictureService _pictureService;
    protected readonly IPriceFormatter _priceFormatter;
    protected readonly ICustomProductAttributeParser _productAttributeParser;
    protected readonly IProductAttributeService _productAttributeService;
    protected readonly IProductExtendedService _productService;
    protected readonly IShippingService _shippingService;
    protected readonly IShoppingCartExtendedModelFactory _shoppingCartModelFactory;
    protected readonly IShoppingCartExtendedCartService _shoppingCartService;
    protected readonly IStaticCacheManager _staticCacheManager;
    protected readonly IStoreContext _storeContext;
    protected readonly IStoreMappingService _storeMappingService;
    protected readonly ITaxService _taxService;
    protected readonly IWebHelper _webHelper;
    protected readonly IWorkContext _workContext;
    protected readonly ICustomWorkflowMessageService _workflowMessageService;
    protected readonly MediaSettings _mediaSettings;
    protected readonly OrderSettings _orderSettings;
    protected readonly ShoppingCartSettings _shoppingCartSettings;
    protected readonly ShippingSettings _shippingSettings;
    private readonly IUrlRecordService _urlRecordService;
    private readonly IAbandonedCartService _abandonedCartService;
    private readonly ILogger _logger;
    private static readonly char[] _separator = [','];

    #endregion

    #region Ctor

    public ShoppingCartController(CaptchaSettings captchaSettings,
        CustomerSettings customerSettings,
        IAttributeParser<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeParser,
        IAttributeService<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeService,
        ICurrencyService currencyService,
        ICustomerActivityService customerActivityService,
        ICustomerExtendedService customerService,
        ICustomWishlistService customWishlistService,
        IDiscountService discountService,
        IDownloadService downloadService,
        IGenericAttributeService genericAttributeService,
        IGiftCardService giftCardService,
        IHtmlFormatter htmlFormatter,
        ILocalizationService localizationService,
        INopFileProvider fileProvider,
        INopUrlHelper nopUrlHelper,
        INotificationService notificationService,
        IPermissionService permissionService,
        IPictureService pictureService,
        IPriceFormatter priceFormatter,
        ICustomProductAttributeParser productAttributeParser,
        IProductAttributeService productAttributeService,
        IProductExtendedService productService,
        IShippingService shippingService,
        IShoppingCartExtendedModelFactory shoppingCartModelFactory,
        IShoppingCartExtendedCartService shoppingCartService,
        IStaticCacheManager staticCacheManager,
        IStoreContext storeContext,
        IStoreMappingService storeMappingService,
        ITaxService taxService,
        IWebHelper webHelper,
        IWorkContext workContext,
        ICustomWorkflowMessageService workflowMessageService,
        MediaSettings mediaSettings,
        OrderSettings orderSettings,
        ShoppingCartSettings shoppingCartSettings,
        ShippingSettings shippingSettings, IUrlRecordService urlRecordService, ILogger logger, IAbandonedCartService abandonedCartService)
    {
        _captchaSettings = captchaSettings;
        _customerSettings = customerSettings;
        _checkoutAttributeParser = checkoutAttributeParser;
        _checkoutAttributeService = checkoutAttributeService;
        _currencyService = currencyService;
        _customerActivityService = customerActivityService;
        _customerService = customerService;
        _customWishlistService = customWishlistService;
        _discountService = discountService;
        _downloadService = downloadService;
        _genericAttributeService = genericAttributeService;
        _giftCardService = giftCardService;
        _htmlFormatter = htmlFormatter;
        _localizationService = localizationService;
        _fileProvider = fileProvider;
        _nopUrlHelper = nopUrlHelper;
        _notificationService = notificationService;
        _permissionService = permissionService;
        _pictureService = pictureService;
        _priceFormatter = priceFormatter;
        _productAttributeParser = productAttributeParser;
        _productAttributeService = productAttributeService;
        _productService = productService;
        _shippingService = shippingService;
        _shoppingCartModelFactory = shoppingCartModelFactory;
        _shoppingCartService = shoppingCartService;
        _staticCacheManager = staticCacheManager;
        _storeContext = storeContext;
        _storeMappingService = storeMappingService;
        _taxService = taxService;
        _webHelper = webHelper;
        _workContext = workContext;
        _workflowMessageService = workflowMessageService;
        _mediaSettings = mediaSettings;
        _orderSettings = orderSettings;
        _shoppingCartSettings = shoppingCartSettings;
        _shippingSettings = shippingSettings;
        _urlRecordService = urlRecordService;
        _logger = logger;
        _abandonedCartService = abandonedCartService;
    }

    #endregion

    [HttpPost]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> CustomAddProductToCart_Catalog(int productId, int shoppingCartTypeId,
     int quantity, bool forceredirection = false)
    {
        var cartType = (ShoppingCartType)shoppingCartTypeId;
        if (cartType == ShoppingCartType.Wishlist)
        {
            var wishlistid = Request.Query["wishlistid"];
            int.TryParse(wishlistid, out int _wishlistid);
            if (_wishlistid > 0)
            {
                var cartItems = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), cartType, (await _storeContext.GetCurrentStoreAsync()).Id);
                var item = cartItems.Where(m => m.Id == _wishlistid).FirstOrDefault();
                if (item != null)
                    await _shoppingCartService.DeleteShoppingCartItemAsync(item);

                var updateTopWishlistSectionHtml = string.Format(
        await _localizationService.GetResourceAsync("Wishlist.HeaderQuantity"),
        cartItems.Where(c => c.Id != _wishlistid).Sum(item => item.Quantity));

                var updateFlyoutWishlistPopupSectionHtml = _shoppingCartSettings.MiniShoppingCartEnabled
               ? await RenderViewComponentToStringAsync(typeof(FlyoutWishlistViewComponent), new { ActionType = ".Item.Delete" })
               : string.Empty;

                return Json(new
                {
                    Id = -1,
                    success = true,
                    message = string.Format(await _localizationService.GetResourceAsync("Products.ProductHasBeenDeletedFromWishlist.Link"), Url.RouteUrl("Wishlist")),
                    updatetopwishlistsectionhtml = updateTopWishlistSectionHtml,
                    updateFlyoutWishlistPopupSectionHtml = updateFlyoutWishlistPopupSectionHtml
                });
            }
        }
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null)
            //no product found
            return Json(new
            {
                success = false,
                message = "No product found with the specified ID"
            });

        //we can add only simple products
        if (product.ProductType != ProductType.SimpleProduct)
        {
            return Json(new
            {
                redirect = Url.RouteUrl("Product", new { id = product.Id, SeName = await _urlRecordService.GetSeNameAsync(product) })
            });
        }

        //products with "minimum order quantity" more than a specified qty
        if (product.OrderMinimumQuantity > quantity)
        {
            //we cannot add to the cart such products from category pages
            //it can confuse customers. That's why we redirect customers to the product details page
            return Json(new
            {
                redirect = Url.RouteUrl("Product", new { id = product.Id, SeName = await _urlRecordService.GetSeNameAsync(product) })
            });
        }

        if (product.CustomerEntersPrice)
        {
            //cannot be added to the cart (requires a customer to enter price)
            return Json(new
            {
                redirect = Url.RouteUrl("Product", new { id = product.Id, SeName = await _urlRecordService.GetSeNameAsync(product) })
            });
        }

        if (product.IsRental)
        {
            //rental products require start/end dates to be entered
            return Json(new
            {
                redirect = Url.RouteUrl("Product", new { id = product.Id, SeName = await _urlRecordService.GetSeNameAsync(product) })
            });
        }

        //var allowedQuantities = _productService.ParseAllowedQuantities(product);
        //if (allowedQuantities.Length > 0 && shoppingCartTypeId == (int)ShoppingCartType.ShoppingCart)
        //{
        //    //cannot be added to the cart (requires a customer to select a quantity from dropdownlist)
        //    return Json(new
        //    {
        //        redirect = Url.RouteUrl("Product", new { id = product.Id, SeName = await _urlRecordService.GetSeNameAsync(product) })
        //    });
        //}

        //allow a product to be added to the cart when all attributes are with "read-only checkboxes" type
        var productAttributes = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
        if (productAttributes.Any(pam => pam.AttributeControlType != AttributeControlType.ReadonlyCheckboxes) && shoppingCartTypeId == (int)ShoppingCartType.ShoppingCart)
        {
            //product has some attributes. let a customer see them
            return Json(new
            {
                redirect = Url.RouteUrl("Product", new { id = product.Id, SeName = await _urlRecordService.GetSeNameAsync(product) })
            });
        }

        //creating XML for "read-only checkboxes" attributes
        var attXml = await productAttributes.AggregateAwaitAsync(string.Empty, async (attributesXml, attribute) =>
        {
            var attributeValues = await _productAttributeService.GetProductAttributeValuesAsync(attribute.Id);
            foreach (var selectedAttributeId in attributeValues
                .Where(v => v.IsPreSelected)
                .Select(v => v.Id)
                .ToList())
            {
                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                    attribute, selectedAttributeId.ToString());
            }

            return attributesXml;
        });


        if (string.IsNullOrEmpty(attXml))
        {
            var attrCombination = await _productAttributeService.GetAllProductAttributeCombinationsAsync(productId);
            attXml = attrCombination.FirstOrDefault()?.AttributesXml;
        }

        //get standard warnings without attribute validations
        //first, try to find existing shopping cart item
        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), cartType, (await _storeContext.GetCurrentStoreAsync()).Id);
        var shoppingCartItem = await _shoppingCartService.FindShoppingCartItemInTheCartAsync(cart, cartType, product);
        //if we already have the same product in the cart, then use the total quantity to validate
        var quantityToValidate = shoppingCartItem != null ? shoppingCartItem.Quantity + quantity : quantity;
        var addToCartWarnings = await _shoppingCartService
            .GetShoppingCartItemWarningsAsync(await _workContext.GetCurrentCustomerAsync(), cartType,
            product, (await _storeContext.GetCurrentStoreAsync()).Id, string.Empty,
            decimal.Zero, null, null, quantityToValidate, false, shoppingCartItem?.Id ?? 0, true, false, false, false);
        if (addToCartWarnings.Any())
        {
            //cannot be added to the cart
            //let's display standard warnings
            return Json(new
            {
                success = false,
                message = addToCartWarnings.ToArray()
            });
        }

        //now let's try adding product to the cart (now including product attribute validation, etc)
        addToCartWarnings = await _shoppingCartService.AddToCartAsync(customer: await _workContext.GetCurrentCustomerAsync(),
            product: product,
            shoppingCartType: cartType,
            storeId: (await _storeContext.GetCurrentStoreAsync()).Id,
            attributesXml: attXml,
            quantity: quantity);
        if (addToCartWarnings.Any())
        {
            //cannot be added to the cart
            //but we do not display attribute and gift card warnings here. let's do it on the product details page
            return Json(new
            {
                redirect = Url.RouteUrl("Product", new { id = product.Id, SeName = await _urlRecordService.GetSeNameAsync(product) })
            });
        }

        //added to the cart/wishlist
        switch (cartType)
        {
            case ShoppingCartType.Wishlist:
                {
                    //activity log
                    await _customerActivityService.InsertActivityAsync("PublicStore.AddToWishlist",
                        string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddToWishlist"), product.Name), product);

                    if (_shoppingCartSettings.DisplayWishlistAfterAddingProduct || forceredirection)
                    {
                        //redirect to the wishlist page
                        return Json(new
                        {
                            redirect = Url.RouteUrl("Wishlist")
                        });
                    }

                    //display notification message and update appropriate blocks
                    var shoppingCarts = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.Wishlist, (await _storeContext.GetCurrentStoreAsync()).Id);

                    var updatetopwishlistsectionhtml = string.Format(await _localizationService.GetResourceAsync("Wishlist.HeaderQuantity"),
                        shoppingCarts.Sum(item => item.Quantity));
                    var item = await this._shoppingCartService.FindShoppingCartItemInTheCartAsync(shoppingCarts,
                                            cartType, product, attXml
                                          );
                    var updateFlyoutWishlistSectionHtml = _shoppingCartSettings.MiniShoppingCartEnabled
             ? await RenderViewComponentToStringAsync(typeof(FlyoutRightWishlistViewComponent), new { ActionType = ".Item.Add" })
             : string.Empty;

                    var updateFlyoutWishlistPopupSectionHtml = _shoppingCartSettings.MiniShoppingCartEnabled
          ? await RenderViewComponentToStringAsync(typeof(FlyoutWishlistViewComponent), new { ActionType = ".Item.Delete" })
          : string.Empty;
                    return Json(new
                    {
                        Id = item?.Id,
                        success = true,
                        message = string.Format(await _localizationService.GetResourceAsync("Products.ProductHasBeenAddedToTheWishlist.Link"), Url.RouteUrl("Wishlist")),
                        updatetopwishlistsectionhtml = updatetopwishlistsectionhtml,
                        updateFlyoutWishlistSectionHtml = updateFlyoutWishlistSectionHtml,
                        updateFlyoutWishlistPopupSectionHtml = updateFlyoutWishlistPopupSectionHtml
                    });
                }

            case ShoppingCartType.ShoppingCart:
            default:
                {
                    //activity log
                    await _customerActivityService.InsertActivityAsync("PublicStore.AddToShoppingCart",
                        string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddToShoppingCart"), product.Name), product);

                    if (_shoppingCartSettings.DisplayCartAfterAddingProduct || forceredirection)
                    {
                        //redirect to the shopping cart page
                        return Json(new
                        {
                            redirect = Url.RouteUrl("ShoppingCart")
                        });
                    }

                    //display notification message and update appropriate blocks
                    var shoppingCarts = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

                    var updateTopCartSectionHtml = string.Format(await _localizationService.GetResourceAsync("ShoppingCart.HeaderQuantity"),
                        shoppingCarts.Sum(item => item.Quantity));

                    var updateFlyoutCartPopupSectionHtml = _shoppingCartSettings.MiniShoppingCartEnabled
                        ? await RenderViewComponentToStringAsync(typeof(Custom_FlyoutShoppingCartViewComponent))
                        : string.Empty;
                    var updateFlyoutCartSectionHtml = _shoppingCartSettings.MiniShoppingCartEnabled
                   ? await RenderViewComponentToStringAsync(typeof(FlyoutRightShoppingCartViewComponent), new { ActionType = ".Item.Add" })
                   : string.Empty;

                    return Json(new
                    {
                        success = true,
                        message = string.Format(await _localizationService.GetResourceAsync("Products.ProductHasBeenAddedToTheCart.Link"),
                            Url.RouteUrl("ShoppingCart")),
                        updatetopcartsectionhtml = updateTopCartSectionHtml,
                        updateflyoutcartsectionhtml = updateFlyoutCartSectionHtml,
                        updateFlyoutCartPopupSectionHtml = updateFlyoutCartPopupSectionHtml,
                        actionType = "Add",
                    });

                }
        }
    }


    [HttpPost, ActionName("Cart")]
    [FormValueRequired("customcheckout")]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> CustomStartCheckout(IFormCollection form)
    {
        return await ProcessCheckoutRequest(form, "checkout");
    }


    [HttpPost, ActionName("Cart")]
    [FormValueRequired("checkout-as-guest")]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> CustomStartCheckoutGuest(IFormCollection form)
    {
        return await ProcessCheckoutRequest(form, "checkout-as-guest");
    }





    [HttpPost, ActionName("Cart")]
    [FormValueRequired("checkout-signin")]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> CustomStartCheckoutSignin(IFormCollection form)
    {
        return await ProcessCheckoutRequest(form, "checkout-signin");
    }



    private async Task<IActionResult> ProcessCheckoutRequest(IFormCollection form, string buttonType)
    {
        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

        //parse and save checkout attributes
        await ParseAndSaveCheckoutAttributesAsync(cart, form);




        //validate attributes
        var checkoutAttributes = await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(),
            NopCustomerDefaults.CheckoutAttributes, (await _storeContext.GetCurrentStoreAsync()).Id);
        var checkoutAttributeWarnings = await _shoppingCartService.GetShoppingCartWarningsAsync(cart, checkoutAttributes, true);
        if (checkoutAttributeWarnings.Any())
        {
            //something wrong, redisplay the page with warnings
            var model = new ShoppingCartModel();
            model = await _shoppingCartModelFactory.PrepareShoppingCartModelAsync(model, cart, validateCheckoutAttributes: true);
            return View(model);
        }

        #region save Special Instructions

        foreach (var cartItem in cart)
        {
            cartItem.SpecialInstructions = form[$"itemspecialinstructions{cartItem.Id}"].ToString();
            await _shoppingCartService.UpdateShoppingCartItemAsync(await _workContext.GetCurrentCustomerAsync(),
                cartItem.Id, cartItem.AttributesXml, cartItem.CustomerEnteredPrice, cartItem.SpecialInstructions,
                cartItem.RentalStartDateUtc, cartItem.RentalEndDateUtc, cartItem.Quantity, true);
        }

        #endregion


        var _isMemberShipAddedInCart = await _customerService.IsMemberShipAddedInCart(await _workContext.GetCurrentCustomerAsync());
        var anonymousPermissed = _orderSettings.AnonymousCheckoutAllowed
                                 && _customerSettings.UserRegistrationType == UserRegistrationType.Disabled
                                 && !_isMemberShipAddedInCart;

        if ((anonymousPermissed || !await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) || (buttonType == "checkout-as-guest" && !_isMemberShipAddedInCart)) && buttonType != "checkout-signin")
        {
            if (form.ContainsKey("Email"))
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                await _genericAttributeService.SaveAttributeAsync(customer, "Email", form["Email"]);
                if (_customerSettings.UsernamesEnabled && _customerSettings.AllowUsersToChangeUsernames)
                    await _genericAttributeService.SaveAttributeAsync(customer, "UserName", form["Email"]);
            }
            return Redirect("/onepagecheckout");
        }

        var cartProductIds = cart.Select(ci => ci.ProductId).ToArray();
        var downloadableProductsRequireRegistration =
            _customerSettings.RequireRegistrationForDownloadableProducts && await _productService.HasAnyDownloadableProductAsync(cartProductIds);

        if (!_orderSettings.AnonymousCheckoutAllowed || downloadableProductsRequireRegistration)
        {
            //verify user identity (it may be facebook login page, or google, or local)
            return Challenge();
        }

        return RedirectToRoute("LoginCheckoutAsGuest", new { returnUrl = Url.RouteUrl("ShoppingCart") });
    }

    [HttpPost]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> CustomCheckoutAttributeChange(IFormCollection form, bool isEditable)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

        //save selected attributes
        await ParseAndSaveCheckoutAttributesAsync(cart, form);
        var attributeXml = await _genericAttributeService.GetAttributeAsync<string>(customer,
            NopCustomerDefaults.CheckoutAttributes, store.Id);

        //conditions
        var enabledAttributeIds = new List<int>();
        var disabledAttributeIds = new List<int>();
        var excludeShippableAttributes = !await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart);
        var attributes = await _checkoutAttributeService.GetAllAttributesAsync(_staticCacheManager, _storeMappingService, store.Id, excludeShippableAttributes);
        foreach (var attribute in attributes)
        {
            var conditionMet = await _checkoutAttributeParser.IsConditionMetAsync(attribute.ConditionAttributeXml, attributeXml);
            if (conditionMet.HasValue)
            {
                if (conditionMet.Value)
                    enabledAttributeIds.Add(attribute.Id);
                else
                    disabledAttributeIds.Add(attribute.Id);
            }
        }

        //update blocks
        var ordetotalssectionhtml = await RenderViewComponentToStringAsync(typeof(CustomOrderTotalsViewComponent), new { isEditable });
        var selectedcheckoutattributesssectionhtml = await RenderViewComponentToStringAsync(typeof(SelectedCheckoutAttributesViewComponent));

        return Json(new
        {
            ordetotalssectionhtml,
            selectedcheckoutattributesssectionhtml,
            enabledattributeids = enabledAttributeIds.ToArray(),
            disabledattributeids = disabledAttributeIds.ToArray()
        });
    }

    [HttpPost]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> CustomAddProductToCart_Details(int productId, int shoppingCartTypeId,
        IFormCollection form, string formId = "")
    {
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null)
        {
            return Json(new
            {
                redirect = Url.RouteUrl("Homepage")
            });
        }
        string attributes = "";
        if ((ShoppingCartType)shoppingCartTypeId == ShoppingCartType.Wishlist)
        {
            var wishlistid = Request.Query["wishlistid"];
            int.TryParse(wishlistid, out int _wishlistid);
            if (_wishlistid > 0)
            {
                var cartItems = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.Wishlist, (await _storeContext.GetCurrentStoreAsync()).Id);
                var item = cartItems.Where(m => m.Id == _wishlistid).FirstOrDefault();
                if (item != null)
                    await _shoppingCartService.DeleteShoppingCartItemAsync(item);
                var updateTopWishlistSectionHtml = string.Format(
      await _localizationService.GetResourceAsync("Wishlist.HeaderQuantity"),
      cartItems.Where(c => c.Id != _wishlistid).Sum(item => item.Quantity));

                var updateFlyoutWishlistPopupSectionHtml = _shoppingCartSettings.MiniShoppingCartEnabled
      ? await RenderViewComponentToStringAsync(typeof(FlyoutWishlistViewComponent), new { ActionType = ".Item.Delete" })
      : string.Empty;
                return Json(new
                {
                    Id = -1,
                    success = true,
                    message = string.Format(await _localizationService.GetResourceAsync("Products.ProductHasBeenDeletedFromWishlist.Link"), Url.RouteUrl("Wishlist")),
                    updatetopwishlistsectionhtml = updateTopWishlistSectionHtml,
                    updateFlyoutWishlistPopupSectionHtml = updateFlyoutWishlistPopupSectionHtml
                });
            }
        }
        //we can add only simple products
        if (product.ProductType != ProductType.SimpleProduct)
        {
            return Json(new
            {
                success = false,
                message = "Only simple products could be added to the cart"
            });
        }

        //update existing shopping cart item
        var updatecartitemid = 0;
        foreach (var formKey in form.Keys)
            if (formKey.Equals($"addtocart_{productId}.UpdatedShoppingCartItemId", StringComparison.InvariantCultureIgnoreCase))
            {
                int.TryParse(form[formKey], out updatecartitemid);
                break;
            }

        ShoppingCartItem updatecartitem = null;
        if (_shoppingCartSettings.AllowCartItemEditing && updatecartitemid > 0)
        {
            //search with the same cart type as specified
            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), (ShoppingCartType)shoppingCartTypeId, (await _storeContext.GetCurrentStoreAsync()).Id);

            updatecartitem = cart.FirstOrDefault(x => x.Id == updatecartitemid);
            //not found? let's ignore it. in this case we'll add a new item
            //if (updatecartitem == null)
            //{
            //    return Json(new
            //    {
            //        success = false,
            //        message = "No shopping cart item found to update"
            //    });
            //}
            //is it this product?
            if (updatecartitem != null && product.Id != updatecartitem.ProductId)
            {
                return Json(new
                {
                    success = false,
                    message = "This product does not match a passed shopping cart item identifier"
                });
            }
        }

        var addToCartWarnings = new List<string>();

        //customer entered price
        var customerEnteredPriceConverted = await _productAttributeParser.ParseCustomerEnteredPriceAsync(product, form);

        //entered quantity
        var quantity = _productAttributeParser.ParseEnteredQuantity(product, form);

        //product and gift card attributes
        attributes = await _productAttributeParser.CustomParseProductAttributesAsync(product, form, addToCartWarnings, formId);

        //rental attributes
        _productAttributeParser.ParseRentalDates(product, form, out var rentalStartDate, out var rentalEndDate);

        var cartType = updatecartitem == null ? (ShoppingCartType)shoppingCartTypeId :
            //if the item to update is found, then we ignore the specified "shoppingCartTypeId" parameter
            updatecartitem.ShoppingCartType;

        await SaveItemAsync(updatecartitem, addToCartWarnings, product, cartType, attributes, customerEnteredPriceConverted, rentalStartDate, rentalEndDate, quantity);

        //return result
        return await CustomGetProductToCartDetailsAsync(addToCartWarnings, cartType, product, attributes);
    }



    [HttpPost]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> CustomAddProductToCart_Collection(string productIds, int shoppingCartTypeId, IFormCollection form,
        bool isFbt = false, string formId = "")
    {
        List<int> shoppingCartItems = new List<int>();
        string[] _productIds = productIds.Split('-', StringSplitOptions.RemoveEmptyEntries);
        var addToCartWarnings = new List<string>();
        Product product = new Product();
        var cartType = ShoppingCartType.ShoppingCart;
        string attributes = "";
        foreach (var _productid in _productIds)
        {
            addToCartWarnings = new List<string>();
            int.TryParse(_productid, out int productId);
            if (productId == 0)
                continue;

            #region For FBT
            if (isFbt)
            {
                if (string.IsNullOrEmpty(form[$"fbq_item_{productId}"].ToString()))
                    continue;
            }
            #endregion



            product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
            {
                return Json(new
                {
                    redirect = Url.RouteUrl("Homepage")
                });
            }

            //we can add only simple products
            if (product.ProductType != ProductType.SimpleProduct)
            {
                return Json(new
                {
                    success = false,
                    message = "Only simple products could be added to the cart"
                });
            }

            //update existing shopping cart item
            var updatecartitemid = 0;
            foreach (var formKey in form.Keys)
                if (formKey.Equals($"addtocart_{productId}.UpdatedShoppingCartItemId", StringComparison.InvariantCultureIgnoreCase))
                {
                    int.TryParse(form[formKey], out updatecartitemid);
                    break;
                }

            ShoppingCartItem updatecartitem = null;
            if (_shoppingCartSettings.AllowCartItemEditing && updatecartitemid > 0)
            {
                //search with the same cart type as specified
                var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), (ShoppingCartType)shoppingCartTypeId, (await _storeContext.GetCurrentStoreAsync()).Id);

                updatecartitem = cart.FirstOrDefault(x => x.Id == updatecartitemid);
                //not found? let's ignore it. in this case we'll add a new item
                //if (updatecartitem == null)
                //{
                //    return Json(new
                //    {
                //        success = false,
                //        message = "No shopping cart item found to update"
                //    });
                //}
                //is it this product?
                if (updatecartitem != null && product.Id != updatecartitem.ProductId)
                {
                    return Json(new
                    {
                        success = false,
                        message = "This product does not match a passed shopping cart item identifier"
                    });
                }
            }



            //customer entered price
            var customerEnteredPriceConverted = await _productAttributeParser.ParseCustomerEnteredPriceAsync(product, form);

            //entered quantity
            var quantity = _productAttributeParser.ParseEnteredQuantity(product, form);

            //product and gift card attributes
            attributes = await _productAttributeParser.CustomParseProductAttributesAsync(product, form, addToCartWarnings, formId);

            //rental attributes
            _productAttributeParser.ParseRentalDates(product, form, out var rentalStartDate, out var rentalEndDate);

            cartType = updatecartitem == null ? (ShoppingCartType)shoppingCartTypeId :
                    //if the item to update is found, then we ignore the specified "shoppingCartTypeId" parameter
                    updatecartitem.ShoppingCartType;

            shoppingCartItems.Add(await CustomSaveItemAsync(updatecartitem, addToCartWarnings, product, cartType, attributes, customerEnteredPriceConverted, rentalStartDate, rentalEndDate, quantity));
        }
        return await CustomGetProductToCartDetailsAsync(addToCartWarnings, cartType, product, attributes, shoppingCartItems);
        //return result

    }

    protected virtual async Task<IActionResult> CustomGetProductToCartDetailsAsync(List<string> addToCartWarnings, ShoppingCartType cartType,
      Product product, string attributes = "", List<int> shoppingCartItems = null)
    {
        if (addToCartWarnings.Any())
        {
            //cannot be added to the cart/wishlist
            //let's display warnings
            return Json(new
            {
                success = false,
                message = addToCartWarnings.ToArray()
            });
        }

        //added to the cart/wishlist
        switch (cartType)
        {
            case ShoppingCartType.Wishlist:
                {
                    //activity log
                    await _customerActivityService.InsertActivityAsync("PublicStore.AddToWishlist",
                        string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddToWishlist"), product.Name), product);

                    if (_shoppingCartSettings.DisplayWishlistAfterAddingProduct)
                    {
                        //redirect to the wishlist page
                        return Json(new
                        {
                            redirect = Url.RouteUrl("Wishlist")
                        });
                    }

                    //display notification message and update appropriate blocks
                    var shoppingCarts = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.Wishlist, (await _storeContext.GetCurrentStoreAsync()).Id);

                    var updateTopWishlistSectionHtml = string.Format(
                        await _localizationService.GetResourceAsync("Wishlist.HeaderQuantity"),
                        shoppingCarts.Sum(item => item.Quantity));

                    var updateFlyoutWishlistPopupSectionHtml = _shoppingCartSettings.MiniShoppingCartEnabled
   ? await RenderViewComponentToStringAsync(typeof(FlyoutWishlistViewComponent), new { ActionType = ".Item.Delete" })
   : string.Empty;
                    var updateFlyoutWishlistSectionHtml = _shoppingCartSettings.MiniShoppingCartEnabled
                     ? await RenderViewComponentToStringAsync(typeof(FlyoutRightWishlistViewComponent), new { ActionType = ".Item.Add" })
                     : string.Empty;
                    var item = await this._shoppingCartService.FindShoppingCartItemInTheCartAsync(shoppingCarts,
                                                                      cartType, product, attributes
                                                                    );
                    return Json(new
                    {
                        Id = item?.Id,
                        success = true,
                        message = string.Format(
                            await _localizationService.GetResourceAsync("Products.ProductHasBeenAddedToTheWishlist.Link"),
                            Url.RouteUrl("Wishlist")),
                        updateFlyoutWishlistSectionHtml = updateFlyoutWishlistSectionHtml,
                        updatetopwishlistsectionhtml = updateTopWishlistSectionHtml,
                        updateFlyoutWishlistPopupSectionHtml = updateFlyoutWishlistPopupSectionHtml
                    });
                }

            case ShoppingCartType.ShoppingCart:
            default:
                {
                    //activity log
                    await _customerActivityService.InsertActivityAsync("PublicStore.AddToShoppingCart",
                        string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddToShoppingCart"), product.Name), product);

                    if (_shoppingCartSettings.DisplayCartAfterAddingProduct)
                    {
                        //redirect to the shopping cart page
                        return Json(new
                        {
                            redirect = Url.RouteUrl("ShoppingCart")
                        });
                    }

                    //display notification message and update appropriate blocks
                    var shoppingCarts = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

                    var updateTopCartSectionHtml = string.Format(
                        await _localizationService.GetResourceAsync("ShoppingCart.HeaderQuantity"),
                        shoppingCarts.Sum(item => item.Quantity));

                    var updateFlyoutCartSectionHtml = _shoppingCartSettings.MiniShoppingCartEnabled
                        ? await RenderViewComponentToStringAsync(typeof(FlyoutRightShoppingCartViewComponent), new { ActionType = ".Item.Add", cartItems = shoppingCartItems })
                        : string.Empty;
                    var updateFlyoutCartPopupSectionHtml = _shoppingCartSettings.MiniShoppingCartEnabled
                 ? await RenderViewComponentToStringAsync(typeof(Custom_FlyoutShoppingCartViewComponent), new { ActionType = ".Item.Delete" })
                 : string.Empty;
                    return Json(new
                    {

                        success = true,
                        message = string.Format(await _localizationService.GetResourceAsync("Products.ProductHasBeenAddedToTheCart.Link"),
                            Url.RouteUrl("ShoppingCart")),
                        updatetopcartsectionhtml = updateTopCartSectionHtml,
                        updateflyoutcartsectionhtml = updateFlyoutCartSectionHtml,
                        updateFlyoutCartPopupSectionHtml = updateFlyoutCartPopupSectionHtml
                    });
                }
        }
    }

    [HttpPost, ActionName("Cart")]
    [FormValueRequired("addmembership")]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> AddMembership(IFormCollection form)
    {
        var _customer = await _workContext.GetCurrentCustomerAsync();
        int storeId = (await _storeContext.GetCurrentStoreAsync()).Id;
        await this._genericAttributeService.SaveAttributeAsync(_customer, NopCustomerDefaults.MemberShipLabelAttribute, true, storeId);
        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
        var model = new ShoppingCartModel();
        model = await _shoppingCartModelFactory.PrepareCustomShoppingCartModelAsync(model, cart);

        return View(model);
    }

    [HttpPost, ActionName("Cart")]
    [FormValueRequired("removemembership")]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> RemoveMembership(IFormCollection form)
    {
        var _customer = await _workContext.GetCurrentCustomerAsync();
        int storeId = (await _storeContext.GetCurrentStoreAsync()).Id;
        await this._genericAttributeService.SaveAttributeAsync(_customer, NopCustomerDefaults.MemberShipLabelAttribute, false, storeId);
        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
        var model = new ShoppingCartModel();
        model = await _shoppingCartModelFactory.PrepareCustomShoppingCartModelAsync(model, cart);

        return View(model);
    }



    [HttpPost]
    public async virtual Task<IActionResult> DeleteCartItem(int shoppingCartRecId, int shoppingCartTypeId)
    {
        int cartQuantity = 0;
        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), (ShoppingCartType)shoppingCartTypeId, (await _storeContext.GetCurrentStoreAsync()).Id);
        cartQuantity = cart.Sum(item => item.Quantity);
        var cartItem = cart.Where(m => m.Id == shoppingCartRecId).FirstOrDefault();
        cartQuantity = cartQuantity - (cartItem != null ? cartItem.Quantity : 0);
        if (cartItem != null)
            await _shoppingCartService.DeleteShoppingCartItemAsync(cartItem, (ShoppingCartType)shoppingCartTypeId == ShoppingCartType.Wishlist ? false : true);
        var updateFlyoutCartSectionHtml = "";
        var updateFlyoutCartPopupSectionHtml = "";
        if ((ShoppingCartType)shoppingCartTypeId == ShoppingCartType.Wishlist)
        {

            var updateTopWishlistSectionHtml = string.Format(
                await _localizationService.GetResourceAsync("Wishlist.HeaderQuantity"),
                cart.Where(c => c.Id != shoppingCartRecId).Sum(item => item.Quantity));
            updateFlyoutCartSectionHtml = _shoppingCartSettings.MiniShoppingCartEnabled
                         ? await RenderViewComponentToStringAsync(typeof(FlyoutRightWishlistViewComponent), new { ActionType = ".Item.Delete" })
                         : string.Empty;
            var updateFlyoutWishlistPopupSectionHtml = _shoppingCartSettings.MiniShoppingCartEnabled
? await RenderViewComponentToStringAsync(typeof(FlyoutWishlistViewComponent), new { ActionType = ".Item.Delete" })
: string.Empty;
            return Json(new
            {
                success = true,
                message = string.Format(
                               await _localizationService.GetResourceAsync("Products.ProductHasBeenDeletedToTheWishlist.Link"),
                               Url.RouteUrl("Wishlist")),
                updateFlyoutWishlistSectionHtml = updateFlyoutCartSectionHtml,
                updatetopwishlistsectionhtml = updateTopWishlistSectionHtml,
                updateFlyoutWishlistPopupSectionHtml = updateFlyoutWishlistPopupSectionHtml
            });
        }
        else
        {
            var updateTopCartSectionHtml = string.Format(
                       await _localizationService.GetResourceAsync("ShoppingCart.HeaderQuantity"),
                       cartQuantity);
            updateFlyoutCartSectionHtml = _shoppingCartSettings.MiniShoppingCartEnabled
                       ? await RenderViewComponentToStringAsync(typeof(FlyoutRightShoppingCartViewComponent), new { ActionType = ".Item.Delete" })
                       : string.Empty;
            updateFlyoutCartPopupSectionHtml = _shoppingCartSettings.MiniShoppingCartEnabled
                   ? await RenderViewComponentToStringAsync(typeof(Custom_FlyoutShoppingCartViewComponent), new { ActionType = ".Item.Delete" })
                   : string.Empty;
            return Json(new
            {
                success = true,

                message = string.Format(await _localizationService.GetResourceAsync("Products.ProductHasBeenDeletedToTheCart.Link"),
                Url.RouteUrl("ShoppingCart")),
                actionType = "delete",
                updatetopcartsectionhtml = updateTopCartSectionHtml,
                updateflyoutcartsectionhtml = updateFlyoutCartSectionHtml,
                updateFlyoutCartPopupSectionHtml = updateFlyoutCartPopupSectionHtml
            });
        }

    }
    public virtual async Task<IActionResult> CustomWishlist(Guid? customerGuid, int? list)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.ENABLE_WISHLIST))
            return RedirectToRoute("Homepage");

        var customer = customerGuid.HasValue ?
            await _customerService.GetCustomerByGuidAsync(customerGuid.Value)
            : await _workContext.GetCurrentCustomerAsync();
        if (customer == null)
            return RedirectToRoute("Homepage");

        var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.Wishlist, (await _storeContext.GetCurrentStoreAsync()).Id);

        var model = new WishlistExtendedModel();
        model = await _shoppingCartModelFactory.PrepareCustomWishlistModelAsync(model, cart, !customerGuid.HasValue, list);
        return View("Wishlist", model);
    }

    [HttpPost, ActionName("CustomWishlist")]
    [FormValueRequired("updatecart")]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> CustomUpdateWishlist(IFormCollection form)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.ENABLE_WISHLIST))
            return RedirectToRoute("Homepage");

        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.Wishlist, (await _storeContext.GetCurrentStoreAsync()).Id);

        var allIdsToRemove = form.ContainsKey("removefromcart")
            ? form["removefromcart"].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse)
            .ToList()
            : new List<int>();

        //current warnings <cart item identifier, warnings>
        var innerWarnings = new Dictionary<int, IList<string>>();
        foreach (var sci in cart)
        {
            var remove = allIdsToRemove.Contains(sci.Id);
            if (remove)
                await _shoppingCartService.DeleteShoppingCartItemAsync(sci);
            else
            {
                foreach (var formKey in form.Keys)
                    if (formKey.Equals($"itemquantity{sci.Id}", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (int.TryParse(form[formKey], out var newQuantity))
                        {
                            var currSciWarnings = await _shoppingCartService.UpdateShoppingCartItemAsync(await _workContext.GetCurrentCustomerAsync(),
                                sci.Id, sci.AttributesXml, sci.CustomerEnteredPrice,
                                sci.RentalStartDateUtc, sci.RentalEndDateUtc,
                                newQuantity, true);
                            innerWarnings.Add(sci.Id, currSciWarnings);
                        }

                        break;
                    }
            }
        }

        //updated wishlist
        cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.Wishlist, (await _storeContext.GetCurrentStoreAsync()).Id);
        var model = new WishlistExtendedModel();
        model = await _shoppingCartModelFactory.PrepareCustomWishlistModelAsync(model, cart);
        //update current warnings
        foreach (var kvp in innerWarnings)
        {
            //kvp = <cart item identifier, warnings>
            var sciId = kvp.Key;
            var warnings = kvp.Value;
            //find model
            var sciModel = model.Items.FirstOrDefault(x => x.Id == sciId);
            if (sciModel != null)
                foreach (var w in warnings)
                    if (!sciModel.Warnings.Contains(w))
                        sciModel.Warnings.Add(w);
        }

        return View("Wishlist", model);
    }

    [HttpPost, ActionName("CustomWishlist")]
    [FormValueRequired("addtocartbutton")]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> CustomMoveWishlistToCart(Guid? customerGuid, IFormCollection form)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.ENABLE_SHOPPING_CART))
            return RedirectToRoute("Homepage");

        if (!await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.ENABLE_WISHLIST))
            return RedirectToRoute("Homepage");

        var pageCustomer = customerGuid.HasValue
            ? await _customerService.GetCustomerByGuidAsync(customerGuid.Value)
            : await _workContext.GetCurrentCustomerAsync();
        if (pageCustomer == null)
            return RedirectToRoute("Homepage");

        var pageCart = await _shoppingCartService.GetShoppingCartAsync(pageCustomer, ShoppingCartType.Wishlist, (await _storeContext.GetCurrentStoreAsync()).Id);

        var allWarnings = new List<string>();
        var countOfAddedItems = 0;
        var allIdsToAdd = form.ContainsKey("addtocart")
            ? form["addtocart"].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList()
            : new List<int>();
        foreach (var sci in pageCart)
        {
            if (allIdsToAdd.Contains(sci.Id))
            {
                var product = await _productService.GetProductByIdAsync(sci.ProductId);

                var warnings = await _shoppingCartService.AddToCartAsync(await _workContext.GetCurrentCustomerAsync(),
                    product, ShoppingCartType.ShoppingCart,
                    (await _storeContext.GetCurrentStoreAsync()).Id,
                    sci.AttributesXml, sci.CustomerEnteredPrice,
                    sci.RentalStartDateUtc, sci.RentalEndDateUtc, sci.Quantity, true);
                if (!warnings.Any())
                    countOfAddedItems++;
                if (_shoppingCartSettings.MoveItemsFromWishlistToCart && //settings enabled
                    !customerGuid.HasValue && //own wishlist
                    !warnings.Any()) //no warnings ( already in the cart)
                {
                    //let's remove the item from wishlist
                    await _shoppingCartService.DeleteShoppingCartItemAsync(sci);
                }

                allWarnings.AddRange(warnings);
            }
        }

        if (countOfAddedItems > 0)
        {
            //redirect to the shopping cart page

            if (allWarnings.Any())
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Wishlist.AddToCart.Error"));
            }

            return RedirectToRoute("ShoppingCart");
        }
        else
        {
            _notificationService.WarningNotification(await _localizationService.GetResourceAsync("Wishlist.AddToCart.NoAddedItems"));
        }
        //no items added. redisplay the wishlist page

        if (allWarnings.Any())
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Wishlist.AddToCart.Error"));
        }

        var cart = await _shoppingCartService.GetShoppingCartAsync(pageCustomer, ShoppingCartType.Wishlist, (await _storeContext.GetCurrentStoreAsync()).Id);

        var model = new WishlistExtendedModel();
        model = await _shoppingCartModelFactory.PrepareCustomWishlistModelAsync(model, cart, !customerGuid.HasValue);
        return View("Wishlist", model);
    }

    [CheckAccessPublicStore(true)]
    [HttpPost]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> CustomAddItemsToCartFromWishlist(IFormCollection form)
    {

        if (!await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.ENABLE_WISHLIST))
            return Json(new
            {
                success = false,
                error = "Wishlist  is not enabled.."
            });



        var pageCart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.Wishlist, (await _storeContext.GetCurrentStoreAsync()).Id);

        var allWarnings = new List<string>();
        var countOfAddedItems = 0;
        var allIdsToAdd = form.ContainsKey("addtocart")
            ? form["addtocart"].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList()
            : new List<int>();
        foreach (var sci in pageCart)
        {
            if (allIdsToAdd.Contains(sci.Id))
            {
                var product = await _productService.GetProductByIdAsync(sci.ProductId);

                var warnings = await _shoppingCartService.AddToCartAsync(await _workContext.GetCurrentCustomerAsync(),
                    product, ShoppingCartType.ShoppingCart,
                    (await _storeContext.GetCurrentStoreAsync()).Id,
                    sci.AttributesXml, sci.CustomerEnteredPrice,
                    sci.RentalStartDateUtc, sci.RentalEndDateUtc, sci.Quantity, true);
                if (!warnings.Any())
                    countOfAddedItems++;
                if (_shoppingCartSettings.MoveItemsFromWishlistToCart && //settings enabled
                    !warnings.Any()) //no warnings ( already in the cart)
                {
                    //let's remove the item from wishlist
                    await _shoppingCartService.DeleteShoppingCartItemAsync(sci);
                }

                allWarnings.AddRange(warnings);
            }
        }
        var updateFlyoutCartSectionHtml = _shoppingCartSettings.MiniShoppingCartEnabled
                       ? await RenderViewComponentToStringAsync(typeof(FlyoutRightShoppingCartViewComponent), new { ActionType = ".Item.Add" })
                       : string.Empty;

        var updateFlyoutCartPopupSectionHtml = _shoppingCartSettings.MiniShoppingCartEnabled
              ? await RenderViewComponentToStringAsync(typeof(Custom_FlyoutShoppingCartViewComponent), new { ActionType = ".Item.Delete" })
              : string.Empty;

        int cartQuantity = 0;
        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
        cartQuantity = cart.Sum(item => item.Quantity);
        var updateTopCartSectionHtml = string.Format(
                       await _localizationService.GetResourceAsync("ShoppingCart.HeaderQuantity"),
                       cartQuantity);
        var updateFlyoutWishlistPopupSectionHtml = _shoppingCartSettings.MiniShoppingCartEnabled
           ? await RenderViewComponentToStringAsync(typeof(FlyoutWishlistViewComponent), new { ActionType = ".Item.Delete" })
           : string.Empty;

        var updateTopWishlistSectionHtml = string.Format(
  await _localizationService.GetResourceAsync("Wishlist.HeaderQuantity"),
  (await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.Wishlist, (await _storeContext.GetCurrentStoreAsync()).Id)
).Sum(item => item.Quantity));
        return Json(new
        {
            success = true,
            updatetopcartsectionhtml = updateTopCartSectionHtml,
            updateFlyoutCartSectionHtml = updateFlyoutCartSectionHtml,
            updateFlyoutCartPopupSectionHtml = updateFlyoutCartPopupSectionHtml,
            updatetopwishlistsectionhtml = updateTopWishlistSectionHtml,
            updateFlyoutWishlistPopupSectionHtml = updateFlyoutWishlistPopupSectionHtml
        });
    }

    [HttpPost]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> CustomProductDetails_AttributeChange(int productId, bool validateAttributeConditions,
       bool loadPicture, IFormCollection form, string formId, int attrid = 0)
    {
        int variantId = 0;

        string title = string.Empty;
        var product = await _productService.GetProductByIdAsync(productId);
        var attrName = "";
        var defaultPictureSize = _mediaSettings.ProductDetailsPictureSize;
        var slug = string.Empty;
        var prdSlug = string.Empty;
        if (product == null)
            return new NullJsonResult();
        slug = prdSlug = await _urlRecordService.GetSeNameAsync(product);
        var errors = new List<string>();
        var attributeXml = await _productAttributeParser.CustomParseProductAttributesAsync(product, form, errors, formId);
        var combination = await _productAttributeParser.FindProductAttributeCombinationAsync(product, attributeXml);
        //rental attributes
        DateTime? rentalStartDate = null;
        DateTime? rentalEndDate = null;
        if (product.IsRental)
        {
            _productAttributeParser.ParseRentalDates(product, form, out rentalStartDate, out rentalEndDate);
        }

        //sku, mpn, gtin
        var sku = await _productService.FormatSkuAsync(product, attributeXml);
        var mpn = await _productService.FormatMpnAsync(product, attributeXml);
        var gtin = await _productService.FormatGtinAsync(product, attributeXml);

        // calculating weight adjustment
        var attributeValues = await _productAttributeParser.ParseProductAttributeValuesAsync(attributeXml);

        // Variant
        var dimensionPictureFullSizeUrl = string.Empty;
        var dimensionPictureDefaultSizeUrl = string.Empty;
        var dimensionPictureThumbImageUrl = string.Empty;
        VariantCombination variant = await _productService.GetVariantFromAttributeValues(productId, attributeValues.Select(attrValues => attrValues.Id).ToList());
        if (variant != null)
        {
            slug = string.IsNullOrEmpty(variant.SeName) ? prdSlug : variant.SeName;
        }

        variantId = variant?.VariantId ?? 0;
        title = variant?.Title ?? product.Name;
        if ((variant?.DimensionPictureId ?? 0) > 0)
        {
            var productAttributePictureCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.ProductAttributePictureModelKey,
                variant.DimensionPictureId, _webHelper.IsCurrentConnectionSecured(), await _storeContext.GetCurrentStoreAsync());
            var pictureModel = await _staticCacheManager.GetAsync(productAttributePictureCacheKey, async () =>
            {
                var picture = await _pictureService.GetPictureByIdAsync(variant.DimensionPictureId);
                string fullSizeImageUrl, imageUrl, thumbImageUrl;

                (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.ProductDetailsPictureSize);


                (thumbImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage);

                return picture == null ? new PictureModel() : new PictureModel
                {
                    FullSizeImageUrl = fullSizeImageUrl,
                    ImageUrl = imageUrl,
                    ThumbImageUrl = thumbImageUrl
                };
            });
            dimensionPictureFullSizeUrl = pictureModel.FullSizeImageUrl;
            dimensionPictureDefaultSizeUrl = pictureModel.ImageUrl;
            dimensionPictureThumbImageUrl = "/images/product/thumb-dimension-images.png";
        }

        // end 

        var totalWeight = product.BasepriceAmount;

        foreach (var attributeValue in attributeValues)
        {
            switch (attributeValue.AttributeValueType)
            {
                case AttributeValueType.Simple:
                    //simple attribute
                    totalWeight += attributeValue.WeightAdjustment;
                    break;
                case AttributeValueType.AssociatedToProduct:
                    //bundled product
                    var associatedProduct = await _productService.GetProductByIdAsync(attributeValue.AssociatedProductId);
                    if (associatedProduct != null)
                        totalWeight += associatedProduct.BasepriceAmount * attributeValue.Quantity;
                    break;
            }
        }

        //price
        var price = string.Empty;
        var oldPrice = string.Empty;
        var msrp = string.Empty;
        var membershipPrice = string.Empty;
        decimal membershipPriceValue = 0;
        string offerText = string.Empty;
        string discount = string.Empty;
        decimal discountPercentage = 0;
        string offerPlaceHolder = string.Empty;
        DateTime? offerStartDate = null;
        DateTime? offerEndDate = null;



        //base price
        var basepricepangv = string.Empty;
        var baseoldpricepangv = string.Empty;
        var baseMsrppangv = string.Empty;
        if (await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.DISPLAY_PRICES) && !product.CustomerEntersPrice)
        {
            //we do not calculate price of "customer enters price" option is enabled
            var (finalPrice, _oldprice, _msrp, _, _) = await _shoppingCartService.GetCustomUnitPriceForAttributeAsync(product,
                await _workContext.GetCurrentCustomerAsync(),
                ShoppingCartType.ShoppingCart,
                1, attributeXml, 0,
                rentalStartDate, rentalEndDate, true);
            #region MemberShipPrice



            var (finalPriceWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, finalPrice);
            (var memberShipPrice, _) = await _shoppingCartService.MemberShipPriceOfProduct(productId, _msrp, _oldprice, finalPriceWithDiscountBase);
            if (memberShipPrice > decimal.Zero)
            {
                memberShipPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(memberShipPrice, await _workContext.GetWorkingCurrencyAsync());
                membershipPrice = await _priceFormatter.FormatPriceAsync(memberShipPrice);
                membershipPriceValue = memberShipPrice;
            }

            #endregion


            var finalPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(finalPriceWithDiscountBase, await _workContext.GetWorkingCurrencyAsync());
            price = await _priceFormatter.FormatPriceAsync(finalPriceWithDiscount);
            baseoldpricepangv = await _priceFormatter.FormatBasePriceAsync(product, finalPriceWithDiscountBase, totalWeight);
            if (_oldprice > 0)
            {
                oldPrice = await _priceFormatter.FormatPriceAsync(_oldprice);
                baseoldpricepangv = await _priceFormatter.FormatBasePriceAsync(product, _oldprice, totalWeight);
            }
            if (_msrp > 0)
            {
                _msrp = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(_msrp, await _workContext.GetWorkingCurrencyAsync());
                msrp = await _priceFormatter.FormatPriceAsync(_msrp);
                baseMsrppangv = await _priceFormatter.FormatBasePriceAsync(product, _msrp, totalWeight);
            }
            (offerText, offerPlaceHolder, discount, discountPercentage, offerStartDate, offerEndDate) = await _productService.GetProductSaleOfferInfo(product, _oldprice, finalPriceWithDiscount);
            offerText = CustomCommonHelper.GetProductOfferText(product.Id, offerText, await _localizationService.GetResourceAsync("label.limitedoffer.v2.placeholder"), discount, discountPercentage, offerEndDate, true);
        }

        //stock
        var stockAvailability = await _productService.FormatStockMessageAsync(product, attributeXml);

        //conditional attributes
        var enabledAttributeMappingIds = new List<int>();
        var disabledAttributeMappingIds = new List<int>();
        if (validateAttributeConditions)
        {
            var attributes = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
            foreach (var attribute in attributes)
            {
                var conditionMet = await _productAttributeParser.IsConditionMetAsync(attribute, attributeXml);
                if (conditionMet.HasValue)
                {
                    if (conditionMet.Value)
                        enabledAttributeMappingIds.Add(attribute.Id);
                    else
                        disabledAttributeMappingIds.Add(attribute.Id);
                }
            }
        }

        //picture. used when we want to override a default product picture when some attribute is selected
        var pictureFullSizeUrl = string.Empty;
        var pictureDefaultSizeUrl = string.Empty;
        var pictureThumbImageUrl = string.Empty;
        ProductAttributeValue attrValue = new ProductAttributeValue();
        if (loadPicture)
        {
            var pictureId = 0;
            if (attrid > 0)
            {
                attrValue = await _productAttributeService.GetProductAttributeValueByIdAsync(attrid);
                var attrValuepictureId = (await _productAttributeService.GetProductAttributeValuePicturesAsync(attrValue.Id)).FirstOrDefault()?.PictureId ?? 0;
                if (attrValuepictureId > 0)
                {
                    var attrMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(attrValue.ProductAttributeMappingId);
                    if (attrMapping.ProductId == productId)
                    {
                        var attr = await _productAttributeService.GetProductAttributeByIdAsync(attrMapping.ProductAttributeId);
                        attrName = attr.Name;
                        //if (attr.Name == await _localizationService.GetResourceAsync("Product.Attr.Size"))
                        pictureId = attrValuepictureId;
                    }
                }
            }
            //first, try to get product attribute combination picture
            if (pictureId == 0)
            {
                if (combination != null)
                {
                    pictureId = (await _productAttributeService.GetProductAttributeCombinationPicturesAsync(combination.Id)).FirstOrDefault()?.PictureId ?? 0;
                }
            }
   

            //then, let's see whether we have attribute values with pictures
            if (pictureId == 0)
            {
                foreach (var _attrValue in attributeValues)
                {
                    pictureId = (await _productAttributeService.GetProductAttributeValuePicturesAsync(_attrValue.Id)).FirstOrDefault()?.PictureId ?? 0;
                    if (pictureId > 0)
                        break;

                }
            }

            if (pictureId > 0)
            {
                var productAttributePictureCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.ProductAttributePictureModelKey,
                    pictureId, _webHelper.IsCurrentConnectionSecured(), await _storeContext.GetCurrentStoreAsync());
                var pictureModel = await _staticCacheManager.GetAsync(productAttributePictureCacheKey, async () =>
                {
                    var picture = await _pictureService.GetPictureByIdAsync(pictureId);
                    string fullSizeImageUrl, imageUrl, thumbImageUrl;

                    (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                    (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.ProductDetailsPictureSize);


                    (thumbImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage);

                    return picture == null ? new PictureModel() : new PictureModel
                    {
                        FullSizeImageUrl = fullSizeImageUrl,
                        ImageUrl = imageUrl,
                        ThumbImageUrl = thumbImageUrl
                    };
                });
                pictureFullSizeUrl = pictureModel.FullSizeImageUrl;
                pictureDefaultSizeUrl = pictureModel.ImageUrl;
                pictureThumbImageUrl = pictureModel.ThumbImageUrl;
            }
        }

        var isFreeShipping = product.IsFreeShipping;
        if (isFreeShipping && !string.IsNullOrEmpty(attributeXml))
        {
            isFreeShipping = await (await _productAttributeParser.ParseProductAttributeValuesAsync(attributeXml))
                .Where(attributeValue => attributeValue.AttributeValueType == AttributeValueType.AssociatedToProduct)
                .SelectAwait(async attributeValue => await _productService.GetProductByIdAsync(attributeValue.AssociatedProductId))
                .AllAsync(associatedProduct => associatedProduct == null || !associatedProduct.IsShipEnabled || associatedProduct.IsFreeShipping);
        }



        #region Gallery Pictures

        List<PictureModel> gallery = new List<PictureModel>();

        if ((combination?.Id ?? 0) > 0)
        {

            var productPicturesCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.ProductAttributeCombinationPictureGalleryModelKey
           , combination.Id, _webHelper.IsCurrentConnectionSecured(), await _storeContext.GetCurrentStoreAsync());

            gallery = await _staticCacheManager.GetAsync(productPicturesCacheKey, async () =>
            {
                var productName = await _localizationService.GetLocalizedAsync(product, x => x.Name);

                string fullSizeImageUrl, imageUrl, thumbImageUrl;

                var combimationPictures = (await _productAttributeService.GetProductAttributeCombinationPicturesAsync(combination.Id)).ToList().Skip(0);

                List<Picture> pictures = new List<Picture>();
                foreach (var combinationPicture in combimationPictures)
                {
                    var picture = await _pictureService.GetPictureByIdAsync(combinationPicture.PictureId);
                    if (picture != null)
                    {
                        pictures.Add(picture);
                    }

                }

                //all pictures
                var pictureModels = new List<PictureModel>();
                for (var i = 0; i < pictures.Count(); i++)
                {
                    var picture = pictures[i];

                    (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, defaultPictureSize, true);
                    (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                    (thumbImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage);

                    var pictureModel = new PictureModel
                    {
                        ImageUrl = imageUrl,
                        ThumbImageUrl = thumbImageUrl,
                        FullSizeImageUrl = fullSizeImageUrl,
                        Title = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat.Details"), productName),
                        AlternateText = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat.Details"), productName),
                    };
                    //"title" attribute
                    pictureModel.Title = !string.IsNullOrEmpty(picture.TitleAttribute) ?
                            picture.TitleAttribute :
                            string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat.Details"), productName);
                    //"alt" attribute
                    pictureModel.AlternateText = !string.IsNullOrEmpty(picture.AltAttribute) ?
                            picture.AltAttribute :
                            string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat.Details"), productName);

                    pictureModels.Add(pictureModel);
                }

                return pictureModels;
            });


        }
        if (attrValue.Id > 0 && gallery.Count == 0)
        {

            gallery = new List<PictureModel>();
            var attrMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(attrValue.ProductAttributeMappingId);
            var attr = await _productAttributeService.GetProductAttributeByIdAsync(attrMapping.ProductAttributeId);

            if (attr != null && (string.Equals(attr.Name, (await _localizationService.GetResourceAsync("product.attr.shades"))) ||
                string.Equals(attr.Name, (await _localizationService.GetResourceAsync("product.attr.configuration")))
                || string.Equals(attr.Name, (await _localizationService.GetResourceAsync("product.attr.size")))
                ))
            {
                var productPicturesCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.ProductAttributePictureGalleryModelKey
           , attrid, _webHelper.IsCurrentConnectionSecured(), await _storeContext.GetCurrentStoreAsync());

                gallery = await _staticCacheManager.GetAsync(productPicturesCacheKey, async () =>
                {
                    var productName = await _localizationService.GetLocalizedAsync(product, x => x.Name);

                    string fullSizeImageUrl, imageUrl, thumbImageUrl;

                    var attrValuePictures = (await _productAttributeService.GetProductAttributeValuePicturesAsync(attrid)).ToList().Skip(0);

                    List<Picture> pictures = new List<Picture>();
                    foreach (var attrValuePicture in attrValuePictures)
                    {

                        var picture = await _pictureService.GetPictureByIdAsync(attrValuePicture.PictureId);
                        if (picture != null)
                        {
                            pictures.Add(picture);
                        }

                    }

                    //all pictures
                    var pictureModels = new List<PictureModel>();
                    for (var i = 0; i < pictures.Count(); i++)
                    {
                        var picture = pictures[i];

                        (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, defaultPictureSize, true);
                        (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                        (thumbImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage);

                        var pictureModel = new PictureModel
                        {
                            ImageUrl = imageUrl,
                            ThumbImageUrl = thumbImageUrl,
                            FullSizeImageUrl = fullSizeImageUrl,
                            Title = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat.Details"), productName),
                            AlternateText = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat.Details"), productName),
                        };
                        //"title" attribute
                        pictureModel.Title = !string.IsNullOrEmpty(picture.TitleAttribute) ?
                                picture.TitleAttribute :
                                string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat.Details"), productName);
                        //"alt" attribute
                        pictureModel.AlternateText = !string.IsNullOrEmpty(picture.AltAttribute) ?
                                picture.AltAttribute :
                                string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat.Details"), productName);

                        pictureModels.Add(pictureModel);
                    }

                    return pictureModels;
                });
            }

        }
        #endregion
        price = CustomCommonHelper.FormatCurrencyPriceWithoutDecimal(price);
        oldPrice = CustomCommonHelper.FormatCurrencyPriceWithoutDecimal(oldPrice);
        msrp = CustomCommonHelper.FormatCurrencyPriceWithoutDecimal(msrp);
        membershipPrice = CustomCommonHelper.FormatCurrencyPriceWithoutDecimal(membershipPrice);
        membershipPriceValue = CustomCommonHelper.FormatPriceWithoutDecimal(membershipPriceValue);
        return Json(new
        {
            productId,
            gtin,
            mpn,
            sku,
            price,
            oldPrice,
            baseoldpricepangv,
            msrp,
            membershipPrice,
            membershipPriceValue,
            baseMsrppangv,
            basepricepangv,
            stockAvailability,
            enabledattributemappingids = enabledAttributeMappingIds.ToArray(),
            disabledattributemappingids = disabledAttributeMappingIds.ToArray(),
            pictureFullSizeUrl,
            pictureDefaultSizeUrl,
            pictureThumbImageUrl,
            isFreeShipping,
            message = errors.Any() ? errors.ToArray() : null,
            offerText,
            attrName,
            gallery,
            variantId,
            dimensionPictureFullSizeUrl,
            dimensionPictureDefaultSizeUrl,
            dimensionPictureThumbImageUrl,
            title,
            slug
        });

    }

    [HttpPost]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> CustomFeaturedProductDetails_AttributeChange(int productId, bool validateAttributeConditions,
       bool loadPicture, IFormCollection form, string formId, int attrid = 0)
    {
        var product = await _productService.GetProductByIdAsync(productId);
        var attrName = "";
        if (product == null)
            return new NullJsonResult();

        var errors = new List<string>();
        var attributeXml = await _productAttributeParser.CustomParseProductAttributesAsync(product, form, errors, formId);
        var combination = await _productAttributeParser.FindProductAttributeCombinationAsync(product, attributeXml);
        var defaultPictureSize = _mediaSettings.ProductDetailsPictureSize;
        //rental attributes
        DateTime? rentalStartDate = null;
        DateTime? rentalEndDate = null;
        if (product.IsRental)
        {
            _productAttributeParser.ParseRentalDates(product, form, out rentalStartDate, out rentalEndDate);
        }

        //sku, mpn, gtin
        var sku = await _productService.FormatSkuAsync(product, attributeXml);
        var mpn = await _productService.FormatMpnAsync(product, attributeXml);
        var gtin = await _productService.FormatGtinAsync(product, attributeXml);

        // calculating weight adjustment
        var attributeValues = await _productAttributeParser.ParseProductAttributeValuesAsync(attributeXml);
        var totalWeight = product.BasepriceAmount;

        foreach (var attributeValue in attributeValues)
        {
            switch (attributeValue.AttributeValueType)
            {
                case AttributeValueType.Simple:
                    //simple attribute
                    totalWeight += attributeValue.WeightAdjustment;
                    break;
                case AttributeValueType.AssociatedToProduct:
                    //bundled product
                    var associatedProduct = await _productService.GetProductByIdAsync(attributeValue.AssociatedProductId);
                    if (associatedProduct != null)
                        totalWeight += associatedProduct.BasepriceAmount * attributeValue.Quantity;
                    break;
            }
        }

        //price
        var price = string.Empty;
        var oldPrice = string.Empty;
        var msrp = string.Empty;
        var membershipPrice = string.Empty;
        decimal membershipPriceValue = 0;
        string offerText = string.Empty;
        string discount = string.Empty;
        decimal discountPercentage = 0;
        string offerPlaceHolder = string.Empty;
        DateTime? offerStartDate = null;
        DateTime? offerEndDate = null;

        //base price
        var basepricepangv = string.Empty;
        var baseoldpricepangv = string.Empty;
        var baseMsrppangv = string.Empty;
        if (await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.DISPLAY_PRICES) && !product.CustomerEntersPrice)
        {
            //we do not calculate price of "customer enters price" option is enabled
            var (finalPrice, _oldprice, _msrp, _, _) = await _shoppingCartService.GetCustomUnitPriceForAttributeAsync(product,
                await _workContext.GetCurrentCustomerAsync(),
                ShoppingCartType.ShoppingCart,
                1, attributeXml, 0,
                rentalStartDate, rentalEndDate, true);
            #region MemberShipPrice



            var (finalPriceWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, finalPrice);
            (var memberShipPrice, _) = await _shoppingCartService.MemberShipPriceOfProduct(productId, _msrp, _oldprice, finalPriceWithDiscountBase);
            if (memberShipPrice > decimal.Zero)
            {
                memberShipPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(memberShipPrice, await _workContext.GetWorkingCurrencyAsync());
                membershipPrice = await _priceFormatter.FormatPriceAsync(memberShipPrice);
                membershipPriceValue = memberShipPrice;
            }

            #endregion


            var finalPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(finalPriceWithDiscountBase, await _workContext.GetWorkingCurrencyAsync());
            price = await _priceFormatter.FormatPriceAsync(finalPriceWithDiscount);
            baseoldpricepangv = await _priceFormatter.FormatBasePriceAsync(product, finalPriceWithDiscountBase, totalWeight);
            if (_oldprice > 0)
            {
                oldPrice = await _priceFormatter.FormatPriceAsync(_oldprice);
                baseoldpricepangv = await _priceFormatter.FormatBasePriceAsync(product, _oldprice, totalWeight);
            }
            if (_msrp > 0)
            {
                _msrp = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(_msrp, await _workContext.GetWorkingCurrencyAsync());
                msrp = await _priceFormatter.FormatPriceAsync(_msrp);
                baseMsrppangv = await _priceFormatter.FormatBasePriceAsync(product, _msrp, totalWeight);
            }
            (offerText, offerPlaceHolder, discount, discountPercentage, offerStartDate, offerEndDate) = await _productService.GetProductSaleOfferInfo(product, _oldprice, finalPriceWithDiscount);
            offerText = CustomCommonHelper.GetProductOfferText(product.Id, offerText, await _localizationService.GetResourceAsync("label.limitedoffer.v2.placeholder"), discount, discountPercentage, offerEndDate, true);

        }

        //stock
        var stockAvailability = await _productService.FormatStockMessageAsync(product, attributeXml);

        //conditional attributes
        var enabledAttributeMappingIds = new List<int>();
        var disabledAttributeMappingIds = new List<int>();
        if (validateAttributeConditions)
        {
            var attributes = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
            foreach (var attribute in attributes)
            {
                var conditionMet = await _productAttributeParser.IsConditionMetAsync(attribute, attributeXml);
                if (conditionMet.HasValue)
                {
                    if (conditionMet.Value)
                        enabledAttributeMappingIds.Add(attribute.Id);
                    else
                        disabledAttributeMappingIds.Add(attribute.Id);
                }
            }
        }

        //picture. used when we want to override a default product picture when some attribute is selected
        var pictureFullSizeUrl = string.Empty;
        var pictureDefaultSizeUrl = string.Empty;
        var pictureThumbImageUrl = string.Empty;
        ProductAttributeValue attrValue = new ProductAttributeValue();
        if (loadPicture)
        {
            var pictureId = 0;
            if (attrid > 0)
            {
                attrValue = await _productAttributeService.GetProductAttributeValueByIdAsync(attrid);
                var attrValuepictureId = (await _productAttributeService.GetProductAttributeValuePicturesAsync(attrValue.Id)).FirstOrDefault()?.PictureId ?? 0;
                if (attrValuepictureId > 0 || attrValue.FeaturedPictureId > 0)
                {
                    var attrMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(attrValue.ProductAttributeMappingId);
                    if (attrMapping.ProductId == productId)
                    {
                        var attr = await _productAttributeService.GetProductAttributeByIdAsync(attrMapping.ProductAttributeId);
                        attrName = attr.Name;
                        //if (attr.Name == await _localizationService.GetResourceAsync("Product.Attr.Size"))
                        pictureId = attrValue.FeaturedPictureId == 0 ? attrValuepictureId : attrValue.FeaturedPictureId;
                    }
                }
            }
            //first, try to get product attribute combination picture
            if (pictureId == 0)
            {
                if (combination != null)
                {
                    pictureId = (await _productAttributeService.GetProductAttributeCombinationPicturesAsync(combination.Id)).FirstOrDefault()?.PictureId ?? 0;
                }
            }
            //then, let's see whether we have attribute values with pictures
            if (pictureId == 0)
            {
                var defaultAttrWithFeaturePicture = (await _productAttributeParser.ParseProductAttributeValuesAsync(attributeXml))
                    .FirstOrDefault(attributeValue => attributeValue.FeaturedPictureId > 0);
                if (defaultAttrWithFeaturePicture != null)
                {
                    pictureId = attrValue.FeaturedPictureId;
                }
                else
                {
                    foreach (var _attrValue in attributeValues)
                    {
                        pictureId = (await _productAttributeService.GetProductAttributeValuePicturesAsync(_attrValue.Id)).FirstOrDefault()?.PictureId ?? 0;
                        if (pictureId > 0)
                            break;

                    }


                }

            }

            if (pictureId > 0)
            {
                var productAttributePictureCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.ProductAttributePictureModelKey,
                    pictureId, _webHelper.IsCurrentConnectionSecured(), await _storeContext.GetCurrentStoreAsync());
                var pictureModel = await _staticCacheManager.GetAsync(productAttributePictureCacheKey, async () =>
                {
                    var picture = await _pictureService.GetPictureByIdAsync(pictureId);
                    string fullSizeImageUrl, imageUrl, thumbImageUrl;

                    (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                    (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.ProductDetailsPictureSize);


                    (thumbImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage);

                    return picture == null ? new PictureModel() : new PictureModel
                    {
                        FullSizeImageUrl = fullSizeImageUrl,
                        ImageUrl = imageUrl,
                        ThumbImageUrl = thumbImageUrl
                    };
                });
                pictureFullSizeUrl = pictureModel.FullSizeImageUrl;
                pictureDefaultSizeUrl = pictureModel.ImageUrl;
                pictureThumbImageUrl = pictureModel.ThumbImageUrl;
            }
        }

        var isFreeShipping = product.IsFreeShipping;
        if (isFreeShipping && !string.IsNullOrEmpty(attributeXml))
        {
            isFreeShipping = await (await _productAttributeParser.ParseProductAttributeValuesAsync(attributeXml))
                .Where(attributeValue => attributeValue.AttributeValueType == AttributeValueType.AssociatedToProduct)
                .SelectAwait(async attributeValue => await _productService.GetProductByIdAsync(attributeValue.AssociatedProductId))
                .AllAsync(associatedProduct => associatedProduct == null || !associatedProduct.IsShipEnabled || associatedProduct.IsFreeShipping);
        }



        #region Gallery Pictures

        List<PictureModel> gallery = new List<PictureModel>();

        if ((combination?.Id ?? 0) > 0)
        {

            var productPicturesCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.ProductAttributeCombinationPictureGalleryModelKey
           , combination.Id, _webHelper.IsCurrentConnectionSecured(), await _storeContext.GetCurrentStoreAsync());

            gallery = await _staticCacheManager.GetAsync(productPicturesCacheKey, async () =>
            {
                var productName = await _localizationService.GetLocalizedAsync(product, x => x.Name);

                string fullSizeImageUrl, imageUrl, thumbImageUrl;

                var combinationPictures = (await _productAttributeService.GetProductAttributeCombinationPicturesAsync(combination.Id)).ToList().Skip(0);

                List<Picture> pictures = new List<Picture>();
                foreach (var combinationPicture in combinationPictures)
                {
                    var picture = await _pictureService.GetPictureByIdAsync(combinationPicture.Id);
                    if (picture != null)
                    {
                        pictures.Add(picture);
                    }
                }

                //all pictures
                var pictureModels = new List<PictureModel>();
                for (var i = 0; i < pictures.Count(); i++)
                {
                    var picture = pictures[i];

                    (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, defaultPictureSize, true);
                    (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                    (thumbImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage);

                    var pictureModel = new PictureModel
                    {
                        ImageUrl = imageUrl,
                        ThumbImageUrl = thumbImageUrl,
                        FullSizeImageUrl = fullSizeImageUrl,
                        Title = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat.Details"), productName),
                        AlternateText = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat.Details"), productName),
                    };
                    //"title" attribute
                    pictureModel.Title = !string.IsNullOrEmpty(picture.TitleAttribute) ?
                            picture.TitleAttribute :
                            string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat.Details"), productName);
                    //"alt" attribute
                    pictureModel.AlternateText = !string.IsNullOrEmpty(picture.AltAttribute) ?
                            picture.AltAttribute :
                            string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat.Details"), productName);

                    pictureModels.Add(pictureModel);
                }

                return pictureModels;
            });


        }
        if (attrValue.Id > 0 && gallery.Count == 0)
        {

            gallery = new List<PictureModel>();
            var attrMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(attrValue.ProductAttributeMappingId);
            var attr = await _productAttributeService.GetProductAttributeByIdAsync(attrMapping.ProductAttributeId);

            if (attr != null && (string.Equals(attr.Name, (await _localizationService.GetResourceAsync("product.attr.shades"))) ||
                string.Equals(attr.Name, (await _localizationService.GetResourceAsync("product.attr.configuration")))
                || string.Equals(attr.Name, (await _localizationService.GetResourceAsync("product.attr.size")))
                ))
            {
                var productPicturesCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.ProductAttributePictureGalleryModelKey
           , attrid, _webHelper.IsCurrentConnectionSecured(), await _storeContext.GetCurrentStoreAsync());

                gallery = await _staticCacheManager.GetAsync(productPicturesCacheKey, async () =>
                {
                    var productName = await _localizationService.GetLocalizedAsync(product, x => x.Name);

                    string fullSizeImageUrl, imageUrl, thumbImageUrl;

                    var attrValuePictures = (await _productAttributeService.GetProductAttributeValuePicturesAsync(attrid)).ToList().Skip(0);

                    List<Picture> pictures = new List<Picture>();
                    foreach (var attrValuePicture in attrValuePictures)
                    {
                        var picture = await _pictureService.GetPictureByIdAsync(attrValuePicture.PictureId);
                        if (picture != null)
                        {
                            pictures.Add(picture);
                        }
                    }

                    //all pictures
                    var pictureModels = new List<PictureModel>();
                    for (var i = 0; i < pictures.Count(); i++)
                    {
                        var picture = pictures[i];

                        (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, defaultPictureSize, true);
                        (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                        (thumbImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage);

                        var pictureModel = new PictureModel
                        {
                            ImageUrl = imageUrl,
                            ThumbImageUrl = thumbImageUrl,
                            FullSizeImageUrl = fullSizeImageUrl,
                            Title = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat.Details"), productName),
                            AlternateText = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat.Details"), productName),
                        };
                        //"title" attribute
                        pictureModel.Title = !string.IsNullOrEmpty(picture.TitleAttribute) ?
                                picture.TitleAttribute :
                                string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat.Details"), productName);
                        //"alt" attribute
                        pictureModel.AlternateText = !string.IsNullOrEmpty(picture.AltAttribute) ?
                                picture.AltAttribute :
                                string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat.Details"), productName);

                        pictureModels.Add(pictureModel);
                    }

                    return pictureModels;
                });
            }

        }
        #endregion
        price = CustomCommonHelper.FormatCurrencyPriceWithoutDecimal(price);
        oldPrice = CustomCommonHelper.FormatCurrencyPriceWithoutDecimal(oldPrice);
        msrp = CustomCommonHelper.FormatCurrencyPriceWithoutDecimal(msrp);
        membershipPrice = CustomCommonHelper.FormatCurrencyPriceWithoutDecimal(membershipPrice);
        membershipPriceValue = CustomCommonHelper.FormatPriceWithoutDecimal(membershipPriceValue);
        return Json(new
        {
            productId,
            gtin,
            mpn,
            sku,
            price,
            oldPrice,
            baseoldpricepangv,
            msrp,
            membershipPrice,
            membershipPriceValue,
            baseMsrppangv,
            basepricepangv,
            stockAvailability,
            enabledattributemappingids = enabledAttributeMappingIds.ToArray(),
            disabledattributemappingids = disabledAttributeMappingIds.ToArray(),
            pictureFullSizeUrl,
            pictureDefaultSizeUrl,
            pictureThumbImageUrl,
            isFreeShipping,
            message = errors.Any() ? errors.ToArray() : null,
            offerText,
            attrName,
            gallery,
        });
    }

    [Route("/managewishlist/saveforlater")]
    public virtual async Task<IActionResult> SaveForLater(int cartId = 0, string returnUrl = "")
    {
        if (cartId == 0)
            return RedirectToRoute("ShoppingCart");
        else
        {
            var cartItems = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
            if (cartItems.Where(m => m.Id == cartId).Any())
            {
                var item = cartItems.Where(m => m.Id == cartId).FirstOrDefault();
                item.ShoppingCartType = ShoppingCartType.Wishlist;
                await _shoppingCartService.UpdateShoppingCartItemAsync(item);
            }
            if (returnUrl != "")
                return Redirect(returnUrl);
            else
                return RedirectToRoute("ShoppingCart");
        }

    }

    [HttpPost, ActionName("Cart")]
    [FormValueRequired("customupdatecart")]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> customupdatecart(IFormCollection form, bool isAjaxCart = false)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.ENABLE_SHOPPING_CART))
            return RedirectToRoute("Homepage");

        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

        //get identifiers of items to remove
        var itemIdsToRemove = form["removefromcart"]
            .SelectMany(value => value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            .Select(idString => int.TryParse(idString, out var id) ? id : 0)
            .Distinct().ToList();


        var products = (await _productService.GetProductsByIdsAsync(cart.Select(item => item.ProductId).Distinct().ToArray()))
            .ToDictionary(item => item.Id, item => item);

        //get order items with changed quantity
        var itemsWithNewQuantity = cart.Select(item => new
        {
            //try to get a new quantity for the item, set 0 for items to remove
            NewQuantity = itemIdsToRemove.Contains(item.Id) ? 0 : int.TryParse(form[$"itemquantity{item.Id}"], out var quantity) ? quantity : item.Quantity,
            Item = item,
            Product = products.ContainsKey(item.ProductId) ? products[item.ProductId] : null,
            NewSpecialInstructions = itemIdsToRemove.Contains(item.Id) ? "" : form[$"itemspecialinstructions{item.Id}"].ToString()
        }).Where(item => item.NewQuantity != item.Item.Quantity || item.NewSpecialInstructions != item.Item.SpecialInstructions);

        //order cart items
        //first should be items with a reduced quantity and that require other products;
        //or items with an increased quantity and are required for other products
        var orderedCart = await itemsWithNewQuantity
            .OrderByDescendingAwait(async cartItem =>
                (cartItem.NewQuantity < cartItem.Item.Quantity &&
                 (cartItem.Product?.RequireOtherProducts ?? false)) ||
                (cartItem.NewQuantity > cartItem.Item.Quantity && cartItem.Product != null && (await _shoppingCartService
                     .GetProductsRequiringProductAsync(cart, cartItem.Product)).Any()))
            .ToListAsync();

        #region Abandoned card

        if (itemIdsToRemove.Count > 0)
        {
            var _abandonedCartService = EngineContext.Current.Resolve<IAbandonedCartService>();
            foreach (var recid in itemIdsToRemove)
            {
                await _abandonedCartService.DeleteItem(recid);
            }
        }

        #endregion
        //try to update cart items with new quantities and get warnings
        var warnings = await orderedCart.SelectAwait(async cartItem => new
        {
            ItemId = cartItem.Item.Id,
            Warnings = await _shoppingCartService.UpdateShoppingCartItemAsync(await _workContext.GetCurrentCustomerAsync(),
                cartItem.Item.Id, cartItem.Item.AttributesXml, cartItem.Item.CustomerEnteredPrice, cartItem.NewSpecialInstructions,
                cartItem.Item.RentalStartDateUtc, cartItem.Item.RentalEndDateUtc, cartItem.NewQuantity, true)
        }).ToListAsync();

        //updated cart
        cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

        //parse and save checkout attributes
        await ParseAndSaveCheckoutAttributesAsync(cart, form);

        //prepare model
        var model = new ShoppingCartModel();
        model = await _shoppingCartModelFactory.PrepareShoppingCartModelAsync(model, cart);

        //update current warnings
        foreach (var warningItem in warnings.Where(warningItem => warningItem.Warnings.Any()))
        {
            //find shopping cart item model to display appropriate warnings
            var itemModel = model.Items.FirstOrDefault(item => item.Id == warningItem.ItemId);
            if (itemModel != null)
                itemModel.Warnings = warningItem.Warnings.Concat(itemModel.Warnings).Distinct().ToList();
        }
        if (!isAjaxCart)
            return View(model);
        else
            return Content(await RenderPartialViewToStringAsync("Ajax_Cart", model));
    }

    [HttpPost, ActionName("Cart")]
    [FormValueRequired("SaveCustomAttributes")]
    public virtual async Task<IActionResult> SaveCustomAttributes(IFormCollection form, bool isAjaxCart = false)
    {
        try
        {
            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

            await ParseAndSaveCheckoutAttributesAsync(cart, form);
            return Ok();
        }
        catch (Exception exp)
        {
            await _logger.InsertLogAsync(LogLevel.Error, "Saving Order Notes", exp.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, await _localizationService.GetResourceAsync("Common.Error.Message"));
        }

    }

    #region Wishlist

    [HttpPost, ActionName("Wishlist")]
    [FormValueRequired("customaddtocartbutton")]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> CustomAddItemsToCartFromWishlist(Guid? customerGuid, IFormCollection form)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.ENABLE_SHOPPING_CART))
            return RedirectToRoute("Homepage");

        if (!await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.ENABLE_WISHLIST))
            return RedirectToRoute("Homepage");

        var pageCustomer = customerGuid.HasValue
            ? await _customerService.GetCustomerByGuidAsync(customerGuid.Value)
            : await _workContext.GetCurrentCustomerAsync();
        if (pageCustomer == null)
            return RedirectToRoute("Homepage");

        var pageCart = await _shoppingCartService.GetShoppingCartAsync(pageCustomer, ShoppingCartType.Wishlist, (await _storeContext.GetCurrentStoreAsync()).Id);

        var allWarnings = new List<string>();
        var countOfAddedItems = 0;
        var allIdsToAdd = form.ContainsKey("addtocart")
            ? form["addtocart"].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList()
            : new List<int>();
        foreach (var sci in pageCart)
        {
            if (allIdsToAdd.Contains(sci.Id))
            {
                var product = await _productService.GetProductByIdAsync(sci.ProductId);

                var warnings = await _shoppingCartService.AddToCartAsync(await _workContext.GetCurrentCustomerAsync(),
                    product, ShoppingCartType.ShoppingCart,
                    (await _storeContext.GetCurrentStoreAsync()).Id,
                    sci.AttributesXml, sci.CustomerEnteredPrice,
                    sci.RentalStartDateUtc, sci.RentalEndDateUtc, sci.Quantity, true);
                if (!warnings.Any())
                    countOfAddedItems++;
                if (_shoppingCartSettings.MoveItemsFromWishlistToCart && //settings enabled
                    !customerGuid.HasValue && //own wishlist
                    !warnings.Any()) //no warnings ( already in the cart)
                {
                    //let's remove the item from wishlist
                    await _shoppingCartService.DeleteShoppingCartItemAsync(sci);
                }

                allWarnings.AddRange(warnings);
            }
        }

        if (countOfAddedItems > 0)
        {
            //redirect to the shopping cart page

            if (allWarnings.Any())
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Wishlist.AddToCart.Error"));
            }

            return RedirectToRoute("ShoppingCart");
        }
        else
            ViewBag.ErrorMessage = await _localizationService.GetResourceAsync("Wishlist.AddToCart.NoAddedItems");

        //no items added. redisplay the wishlist page

        if (allWarnings.Any())
            ViewBag.ErrorMessage = await _localizationService.GetResourceAsync("Wishlist.AddToCart.Error");


        var cart = await _shoppingCartService.GetShoppingCartAsync(pageCustomer, ShoppingCartType.Wishlist, (await _storeContext.GetCurrentStoreAsync()).Id);

        var model = new WishlistModel();
        model = await _shoppingCartModelFactory.PrepareWishlistModelAsync(model, cart, !customerGuid.HasValue);
        return View(model);
    }

    [CheckAccessPublicStore(true)]
    [HttpPost]
    public virtual async Task<IActionResult> EmailWishlistPopup(LoginRegisterModel model, bool captchaValid)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.ENABLE_WISHLIST) || !_shoppingCartSettings.EmailWishlistEnabled)
            return RedirectToRoute(NopRouteNames.General.HOMEPAGE);
        var customer = await _workContext.GetCurrentCustomerAsync();
        string result = "";
        bool success = false;
        result = await _localizationService.GetResourceAsync("Wishlist.EmailAFriend.Failed");
        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.Wishlist, (await _storeContext.GetCurrentStoreAsync()).Id);

        if (cart.Any())
        {
            if (ModelState.IsValid)
            {
                var wishlistUrl = Url.RouteUrl(NopRouteNames.General.WISHLIST, new { customerGuid = customer.CustomerGuid }, _webHelper.GetCurrentRequestProtocol());

                //email
                await _workflowMessageService.CustomSendWishlistEmailAFriendMessageAsync(await _workContext.GetCurrentCustomerAsync(),
                        (await _workContext.GetWorkingLanguageAsync()).Id, model.Email,
                        model.Email, _htmlFormatter.FormatText(await _localizationService.GetResourceAsync("Wishlist.Message"), false, true, false, false, false, false), wishlistUrl);


                result = await _localizationService.GetResourceAsync("Wishlist.EmailAFriend.Popup.SuccessfullySent");
                success = true;

            }
        }
        return Json(new
        {
            statusCode = success ? 200 : 400,
            message = result
        });
    }


    [HttpPost]
    public async Task<IActionResult> UpdateCartItem(int shoppingCartId, int shoppingCartTypeId, [FromQuery] int quantity)
    {
        if (quantity <= 0)
            return Json(new { success = false, message = "Quantity must be greater than zero." });

        var customer = await _workContext.GetCurrentCustomerAsync();



        var shoppingCartType = (ShoppingCartType)shoppingCartTypeId;
        var cart = await _shoppingCartService.GetShoppingCartAsync(customer, shoppingCartType);
        var cartItem = cart.FirstOrDefault(x => x.Id == shoppingCartId);

        if (cartItem != null)
        {
            // Update existing cart item quantity
            var updateWarnings = await _shoppingCartService.UpdateShoppingCartItemAsync(
                customer,
                cartItem.Id,
                cartItem.AttributesXml,
                cartItem.CustomerEnteredPrice,
                cartItem.RentalStartDateUtc,
                cartItem.RentalEndDateUtc,
                quantity, true);

            if (updateWarnings.Any())
                return Json(new { success = false, message = updateWarnings.FirstOrDefault() });
        }

        // After update/add, get the updated cart quantity count
        var updatedCart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart);
        var cartQuantity = updatedCart.Sum(x => x.Quantity);

        // Prepare updated HTML snippets for client
        var updateTopCartSectionHtml = string.Format(
            await _localizationService.GetResourceAsync("ShoppingCart.HeaderQuantity"), cartQuantity);

        var updateFlyoutCartSectionHtml = _shoppingCartSettings.MiniShoppingCartEnabled
                     ? await RenderViewComponentToStringAsync(typeof(FlyoutRightShoppingCartViewComponent), new { ActionType = ".Item.Add", cartItems = (object)null })
                     : string.Empty;
        return Json(new
        {
            success = true,
            message = await _localizationService.GetResourceAsync("Products.ProductQuantityUpdated"),
            updatetopcartsectionhtml = updateTopCartSectionHtml,
            updateflyoutcartsectionhtml = updateFlyoutCartSectionHtml,
            actionType = "Add"
        });
    }
    [HttpGet]

    public async Task<IActionResult> Share(Guid customerGuid)
    {
        if (customerGuid == Guid.Empty)
            return RedirectToRoute("ShoppingCart");

        var currentCustomer = await _workContext.GetCurrentCustomerAsync();

        var sharedCustomer = await _customerService.GetCustomerByGuidAsync(customerGuid);
        if (sharedCustomer == null)
            return RedirectToRoute("ShoppingCart");

        var cartItems = await _shoppingCartService.GetShoppingCartAsync(sharedCustomer, ShoppingCartType.ShoppingCart);

        foreach (var item in cartItems)
        {
            var product = await _productService.GetProductByIdAsync(item.ProductId);
            await _shoppingCartService.AddToCartAsync(
                currentCustomer,
                product,
                ShoppingCartType.ShoppingCart,
                item.StoreId,
                item.AttributesXml,
                item.CustomerEnteredPrice,
                item.RentalStartDateUtc,
                item.RentalEndDateUtc,
                item.Quantity
            );
        }
        return RedirectToRoute("ShoppingCart");
    }
    #endregion

    #region Utilities

    protected virtual async Task<int> CustomSaveItemAsync(ShoppingCartItem updatecartitem, List<string> addToCartWarnings, Product product,
ShoppingCartType cartType, string attributes, decimal customerEnteredPriceConverted, DateTime? rentalStartDate,
DateTime? rentalEndDate, int quantity)
    {
        int shoppingCartItemId = 0;
        if (updatecartitem == null)
        {
            //add to the cart
            var result = await _shoppingCartService.CustomAddToCartCollectionAsync(await _workContext.GetCurrentCustomerAsync(),
                        product, cartType, (await _storeContext.GetCurrentStoreAsync()).Id,
                        attributes, customerEnteredPriceConverted,
                        rentalStartDate, rentalEndDate, quantity, true);
            shoppingCartItemId = result.Item2;
            addToCartWarnings.AddRange(result.Item1);
        }
        else
        {
            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), updatecartitem.ShoppingCartType, (await _storeContext.GetCurrentStoreAsync()).Id);

            var otherCartItemWithSameParameters = await _shoppingCartService.FindShoppingCartItemInTheCartAsync(
                cart, updatecartitem.ShoppingCartType, product, attributes, customerEnteredPriceConverted,
                rentalStartDate, rentalEndDate);
            if (otherCartItemWithSameParameters != null &&
                otherCartItemWithSameParameters.Id == updatecartitem.Id)
            {
                //ensure it's some other shopping cart item
                otherCartItemWithSameParameters = null;
            }
            //update existing item
            addToCartWarnings.AddRange(await _shoppingCartService.UpdateShoppingCartItemAsync(await _workContext.GetCurrentCustomerAsync(),
                updatecartitem.Id, attributes, customerEnteredPriceConverted,
                rentalStartDate, rentalEndDate, quantity + (otherCartItemWithSameParameters?.Quantity ?? 0), true));
            if (otherCartItemWithSameParameters != null && !addToCartWarnings.Any())
            {
                //delete the same shopping cart item (the other one)
                await _shoppingCartService.DeleteShoppingCartItemAsync(otherCartItemWithSameParameters);
            }
        }
        return shoppingCartItemId;
    }


    [HttpPost, ActionName("Cart")]
    [FormValueRequired("customapplydiscountcouponcode")]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> CustomApplyDiscountCoupon(string discountcouponcode, IFormCollection form)
    {
        //trim
        if (discountcouponcode != null)
            discountcouponcode = discountcouponcode.Trim();

        //cart
        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

        //parse and save checkout attributes
        await ParseAndSaveCheckoutAttributesAsync(cart, form);

        var model = new ShoppingCartModel();
        if (!string.IsNullOrWhiteSpace(discountcouponcode))
        {
            //we find even hidden records here. this way we can display a user-friendly message if it's expired
            var discounts = (await _discountService.GetAllDiscountsAsync(couponCode: discountcouponcode, showHidden: true))
                .Where(d => d.RequiresCouponCode)
                .ToList();
            if (discounts.Any())
            {
                var userErrors = new List<string>();
                bool anyValidDiscount = true;
                var allCategoryIds = new List<int>();
                var _categoryService = EngineContext.Current.Resolve<ICategoryService>();

                if (discounts.FirstOrDefault().DiscountType == DiscountType.AssignedToCategories)
                {

                    var requiredCategories = await _categoryService.GetCategoriesByAppliedDiscountAsync(discounts.FirstOrDefault().Id);
                    foreach (var category in requiredCategories)
                    {
                        allCategoryIds.Add(category.Id);
                    }
                    if (discounts.FirstOrDefault().AppliedToSubCategories)
                    {

                        var categoriesToProcess = new Queue<int>();

                        foreach (var category in requiredCategories)
                        {
                            categoriesToProcess.Enqueue(category.Id);
                        }

                        while (categoriesToProcess.Any())
                        {
                            var parentId = categoriesToProcess.Dequeue();
                            var children = await _categoryService.GetAllCategoriesByParentCategoryIdAsync(parentId, showHidden: true);
                            foreach (var child in children)
                            {
                                if (!allCategoryIds.Contains(child.Id))
                                {
                                    allCategoryIds.Add(child.Id);
                                    categoriesToProcess.Enqueue(child.Id);
                                }
                            }
                        }

                    }
                    if (requiredCategories.Any())
                    {
                        anyValidDiscount = false;
                    }
                    foreach (var item in cart)
                    {
                        var productCategories = await _categoryService.GetProductCategoriesByProductIdAsync(item.ProductId);
                        if (productCategories.Any(pc => allCategoryIds.Contains(pc.CategoryId)))
                        {
                            anyValidDiscount = true;
                            break;
                        }
                    }
                }

                if (anyValidDiscount)
                {
                    anyValidDiscount = await discounts.AnyAwaitAsync(async discount =>
                    {
                        var validationResult = await _discountService.ValidateDiscountAsync(discount, await _workContext.GetCurrentCustomerAsync(), new[] { discountcouponcode });
                        userErrors.AddRange(validationResult.Errors);

                        return validationResult.IsValid;
                    });
                }
                else
                {
                    userErrors.Add("Coupon cannot be applied. Your cart does not contain eligible products.");
                }

                if (anyValidDiscount)
                {


                    //valid
                    await _customerService.ApplyDiscountCouponCodeAsync(await _workContext.GetCurrentCustomerAsync(), discountcouponcode);
                    model.DiscountBox.Messages.Add(await _localizationService.GetResourceAsync("ShoppingCart.DiscountCouponCode.Applied"));
                    model.DiscountBox.IsApplied = true;
                }
                else
                {
                    if (userErrors.Any())
                        //some user errors
                        model.DiscountBox.Messages = userErrors;
                    else
                        //general error text
                        model.DiscountBox.Messages.Add(await _localizationService.GetResourceAsync("ShoppingCart.DiscountCouponCode.WrongDiscount"));
                }
            }
            else
                //discount cannot be found
                model.DiscountBox.Messages.Add(await _localizationService.GetResourceAsync("ShoppingCart.DiscountCouponCode.CannotBeFound"));
        }
        else
            //empty coupon code
            model.DiscountBox.Messages.Add(await _localizationService.GetResourceAsync("ShoppingCart.DiscountCouponCode.Empty"));

        model = await _shoppingCartModelFactory.PrepareShoppingCartModelAsync(model, cart);

        return View(model);
    }




    protected virtual async Task ParseAndSaveCheckoutAttributesAsync(IList<ShoppingCartItem> cart, IFormCollection form)
    {
        ArgumentNullException.ThrowIfNull(cart);

        ArgumentNullException.ThrowIfNull(form);

        var attributesXml = string.Empty;
        var excludeShippableAttributes = !await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart);
        var store = await _storeContext.GetCurrentStoreAsync();
        var checkoutAttributes = await _checkoutAttributeService.GetAllAttributesAsync(_staticCacheManager, _storeMappingService, store.Id, excludeShippableAttributes);
        foreach (var attribute in checkoutAttributes)
        {
            var controlId = $"checkout_attribute_{attribute.Id}";
            switch (attribute.AttributeControlType)
            {
                case AttributeControlType.DropdownList:
                case AttributeControlType.RadioList:
                case AttributeControlType.ColorSquares:
                case AttributeControlType.ImageSquares:
                    {
                        var ctrlAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                        {
                            var selectedAttributeId = int.Parse(ctrlAttributes);
                            if (selectedAttributeId > 0)
                                attributesXml = _checkoutAttributeParser.AddAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString());
                        }
                    }

                    break;
                case AttributeControlType.Checkboxes:
                    {
                        var cblAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(cblAttributes))
                        {
                            foreach (var item in cblAttributes.ToString().Split(_separator, StringSplitOptions.RemoveEmptyEntries))
                            {
                                var selectedAttributeId = int.Parse(item);
                                if (selectedAttributeId > 0)
                                    attributesXml = _checkoutAttributeParser.AddAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                            }
                        }
                    }

                    break;
                case AttributeControlType.ReadonlyCheckboxes:
                    {
                        //load read-only (already server-side selected) values
                        var attributeValues = await _checkoutAttributeService.GetAttributeValuesAsync(attribute.Id);
                        foreach (var selectedAttributeId in attributeValues
                                     .Where(v => v.IsPreSelected)
                                     .Select(v => v.Id)
                                     .ToList())
                        {
                            attributesXml = _checkoutAttributeParser.AddAttribute(attributesXml,
                                attribute, selectedAttributeId.ToString());
                        }
                    }

                    break;
                case AttributeControlType.TextBox:
                case AttributeControlType.MultilineTextbox:
                    {
                        var ctrlAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                        {
                            var enteredText = ctrlAttributes.ToString().Trim();
                            attributesXml = _checkoutAttributeParser.AddAttribute(attributesXml,
                                attribute, enteredText);
                        }
                    }

                    break;
                case AttributeControlType.Datepicker:
                    {
                        var date = form[controlId + "_day"];
                        var month = form[controlId + "_month"];
                        var year = form[controlId + "_year"];
                        DateTime? selectedDate = null;
                        try
                        {
                            selectedDate = new DateTime(int.Parse(year), int.Parse(month), int.Parse(date));
                        }
                        catch
                        {
                            // ignored
                        }

                        if (selectedDate.HasValue)
                            attributesXml = _checkoutAttributeParser.AddAttribute(attributesXml,
                                attribute, selectedDate.Value.ToString("D"));
                    }

                    break;
                case AttributeControlType.FileUpload:
                    {
                        _ = Guid.TryParse(form[controlId], out var downloadGuid);
                        var download = await _downloadService.GetDownloadByGuidAsync(downloadGuid);
                        if (download != null)
                        {
                            attributesXml = _checkoutAttributeParser.AddAttribute(attributesXml,
                                attribute, download.DownloadGuid.ToString());
                        }
                    }

                    break;
                default:
                    break;
            }
        }

        //validate conditional attributes (if specified)
        foreach (var attribute in checkoutAttributes)
        {
            var conditionMet = await _checkoutAttributeParser.IsConditionMetAsync(attribute.ConditionAttributeXml, attributesXml);
            if (conditionMet.HasValue && !conditionMet.Value)
                attributesXml = _checkoutAttributeParser.RemoveAttribute(attributesXml, attribute.Id);
        }

        //save checkout attributes
        await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.CheckoutAttributes, attributesXml, store.Id);
    }

    protected virtual async Task SaveItemAsync(ShoppingCartItem updatecartitem, List<string> addToCartWarnings, Product product,
    ShoppingCartType cartType, string attributes, decimal customerEnteredPriceConverted, DateTime? rentalStartDate,
    DateTime? rentalEndDate, int quantity)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();
        if (updatecartitem == null)
        {
            //add to the cart
            addToCartWarnings.AddRange(await _shoppingCartService.AddToCartAsync(customer,
                product, cartType, store.Id,
                attributes, customerEnteredPriceConverted,
                rentalStartDate, rentalEndDate, quantity, true));
        }
        else
        {
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, updatecartitem.ShoppingCartType, store.Id);

            var otherCartItemWithSameParameters = await _shoppingCartService.FindShoppingCartItemInTheCartAsync(
                cart, updatecartitem.ShoppingCartType, product, attributes, customerEnteredPriceConverted,
                rentalStartDate, rentalEndDate);
            if (otherCartItemWithSameParameters != null &&
                otherCartItemWithSameParameters.Id == updatecartitem.Id)
            {
                //ensure it's some other shopping cart item
                otherCartItemWithSameParameters = null;
            }
            //update existing item
            addToCartWarnings.AddRange(await _shoppingCartService.UpdateShoppingCartItemAsync(customer,
                updatecartitem.Id, attributes, customerEnteredPriceConverted,
                rentalStartDate, rentalEndDate, quantity + (otherCartItemWithSameParameters?.Quantity ?? 0), true));
            if (otherCartItemWithSameParameters != null && !addToCartWarnings.Any())
            {
                //delete the same shopping cart item (the other one)
                await _shoppingCartService.DeleteShoppingCartItemAsync(otherCartItemWithSameParameters);
            }
        }
    }

    #endregion
}