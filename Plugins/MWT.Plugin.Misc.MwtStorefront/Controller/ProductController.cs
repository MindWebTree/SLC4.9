using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Services.Catalog;
using MWT.Nop.Core.Services.MailChimp;
using MWT.Plugin.Misc.MwtStorefront.Factories;
using MWT.Plugin.Misc.MwtStorefront.Models.Catalog;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Core.Rss;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Components;
using Nop.Web.Controllers;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Controller
{

    [AutoValidateAntiforgeryToken]
    public partial class ProductController : BasePublicController
    {
        private readonly IProductExtendedService _productService;
        private readonly CatalogSettings _catalogSettings;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IPermissionService _permissionService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        protected readonly ICustomRecentlyViewedProductsService _recentlyViewedProductsService;
        protected readonly ICustomerActivityService _customerActivityService;
        protected readonly ILocalizationService _localizationService;
        private readonly ICustomProductModelFactory _productModelFactory;
        private readonly IWebHelper _webHelper;
        private readonly ICustomSpecificationAttributeService _specificationAttributeService;
        private readonly ICustomCategoryService _categoryService;
        private readonly ICustomizationFormSerivce _customizationFormSerivce;
        private readonly ICustomBackInStockSubscriptionService _backInStockSubscriptionService;
        private readonly IMailchimpService _mailchimpService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ProductController(IProductExtendedService productService,CatalogSettings catalogSettings,IAclService aclService,IStoreMappingService storeMappingService,
            IPermissionService permissionService, IUrlRecordService urlRecordService, ShoppingCartSettings shoppingCartSettings,
            IShoppingCartService shoppingCartService, IWorkContext workContext, IStoreContext storeContext, ICustomRecentlyViewedProductsService recentlyViewedProductsService,
            ICustomerActivityService customerActivityService, ILocalizationService localizationService, ICustomProductModelFactory productModelFactory, IWebHelper webHelper,
            ICustomSpecificationAttributeService specificationAttributeService, ICustomCategoryService categoryService, ICustomizationFormSerivce customizationFormSerivce,
            ICustomBackInStockSubscriptionService backInStockSubscriptionService, IMailchimpService mailchimpService, IHttpContextAccessor httpContextAccessor
            )
        {
            _productService = productService;
            _catalogSettings = catalogSettings;
            _aclService = aclService;
            _storeMappingService = storeMappingService;
            _permissionService = permissionService;
            _urlRecordService = urlRecordService;
            _shoppingCartSettings = shoppingCartSettings;
            _shoppingCartService = shoppingCartService;
            _workContext = workContext;
            _storeContext = storeContext;
            _recentlyViewedProductsService = recentlyViewedProductsService;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _productModelFactory = productModelFactory;
            _webHelper = webHelper;
            _specificationAttributeService = specificationAttributeService;
            _categoryService = categoryService;
            _customizationFormSerivce = customizationFormSerivce;
            _backInStockSubscriptionService = backInStockSubscriptionService;
            _mailchimpService = mailchimpService;
            _httpContextAccessor=httpContextAccessor;
        }


        #region methods
        public virtual async Task<IActionResult> CustomProductDetails(int id, string SeName, int updatecartitemid = 0,
            int variantId = 0, string size = "")
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null || product.Deleted)
                return InvokeHttp404();

            var notAvailable =
                //published?
                (!product.Published && !_catalogSettings.AllowViewUnpublishedProductPage) ||
                //ACL (access control list) 
                !await _aclService.AuthorizeAsync(product) ||
                //Store mapping
                !await _storeMappingService.AuthorizeAsync(product) ||
                //availability dates
                !_productService.ProductIsAvailable(product);
            //Check whether the current user has a "Manage products" permission (usually a store owner)
            //We should allows him (her) to use "Preview" functionality
            var hasAdminAccess = await _permissionService.AuthorizeAsync(StandardPermission.Security.ACCESS_ADMIN_PANEL) && await _permissionService.AuthorizeAsync(StandardPermission.Catalog.PRODUCTS_VIEW);
            if (notAvailable && !hasAdminAccess)
                return InvokeHttp404();

            //visible individually?
            if (!product.VisibleIndividually)
            {
                //is this one an associated products?
                var parentGroupedProduct = await _productService.GetProductByIdAsync(product.ParentGroupedProductId);
                if (parentGroupedProduct == null)
                    return RedirectToRoute("Homepage");
                return RedirectToRoutePermanent("CustomProduct", new { id = id, SeName = await _urlRecordService.GetSeNameAsync(parentGroupedProduct) });
            }

            var _SeName = await _urlRecordService.GetSeNameAsync(product);
            // 301 Redirect to the correct search engine name in the url if it is wrong
            if (!string.IsNullOrEmpty(_SeName)
                && !StringComparer.OrdinalIgnoreCase.Equals(SeName, _SeName))
            {
                //await EngineContext.Current.Resolve<ILogger>().InsertLogAsync(
                //    logLevel: Core.Domain.Logging.LogLevel.Information,
                //      shortMessage: "Invalid product se name.",
                //      fullMessage: string.Format("Product {0} does not have an SE Name set.", product.Id)
                //    );
                return RedirectToRoutePermanent("Product", new { id = id, SeName = _SeName });

            }

            //update existing shopping cart or wishlist  item?
            ShoppingCartItem updatecartitem = null;
            if (_shoppingCartSettings.AllowCartItemEditing && updatecartitemid > 0)
            {
                var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), storeId: (await _storeContext.GetCurrentStoreAsync()).Id);
                updatecartitem = cart.FirstOrDefault(x => x.Id == updatecartitemid);
                //not found?
                if (updatecartitem == null)
                {
                    return RedirectToRoutePermanent("Product", new { id = id, SeName = _SeName });
                }
                //is it this product?
                if (product.Id != updatecartitem.ProductId)
                {
                    return RedirectToRoutePermanent("Product", new { id = id, SeName = _SeName });
                }
            }

            //save as recently viewed
            await _recentlyViewedProductsService.CustomAddProductToRecentlyViewedListAsync(product.Id);

            //display "edit" (manage) link
            if (await _permissionService.AuthorizeAsync(StandardPermission.Security.ACCESS_ADMIN_PANEL) &&
                await _permissionService.AuthorizeAsync(StandardPermission.Catalog.PRODUCTS_VIEW))
            {
                //a vendor should have access only to his products
                if (await _workContext.GetCurrentVendorAsync() == null || (await _workContext.GetCurrentVendorAsync()).Id == product.VendorId)
                {
                    DisplayEditLink(Url.Action("Edit", "Product", new { id = product.Id, area =   AreaNames.ADMIN }));
                }
            }



            //activity log
            await _customerActivityService.InsertActivityAsync("PublicStore.ViewProduct",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.ViewProduct"), product.Name), product);

            if (variantId == 0 && !string.IsNullOrEmpty(size) && !string.IsNullOrEmpty(size.Trim()))
                variantId = await _productModelFactory.GetVariantIdBySize(product.Id, size);
            //model
            var model = await _productModelFactory.PrepareCustomProductDetailsModelAsync(product, updatecartitem, false, variantId);




            //template
            var productTemplateViewPath = await _productModelFactory.PrepareProductTemplateViewPathAsync(product);

            if (model.UseNewVersionOfTemplate)
            {
                productTemplateViewPath = "New." + productTemplateViewPath;
            }
            model.IsMainProduct = true;

            return View(productTemplateViewPath, model);
        }
        public virtual async Task<IActionResult> CustomProductDetailsWithVariantId(string productId, string SeName, int updatecartitemid = 0)
        {
            int id = 0;
            int variantId = 0;
            if (!string.IsNullOrEmpty(productId))
            {
                string[] parts = productId.Split('_');

                int.TryParse(parts[0], out id);
                if (parts.Length > 1)
                {
                    int.TryParse(parts[1], out variantId);
                }
            }

            var product = await _productService.GetProductByIdAsync(id);
            if (product == null || product.Deleted)
                return InvokeHttp404();

            var variant = await _productModelFactory.ValidateVariantID(id, variantId);
            if (variant == null)
                return InvokeHttp404();
            var notAvailable =
                //published?
                (!product.Published && !_catalogSettings.AllowViewUnpublishedProductPage) ||
                //ACL (access control list) 
                !await _aclService.AuthorizeAsync(product) ||
                //Store mapping
                !await _storeMappingService.AuthorizeAsync(product) ||
                //availability dates
                !_productService.ProductIsAvailable(product);
            //Check whether the current user has a "Manage products" permission (usually a store owner)
            //We should allows him (her) to use "Preview" functionality
            var hasAdminAccess = await _permissionService.AuthorizeAsync(StandardPermission.Security.ACCESS_ADMIN_PANEL) && await _permissionService.AuthorizeAsync(StandardPermission.Catalog.PRODUCTS_VIEW);
            if (notAvailable && !hasAdminAccess)
                return InvokeHttp404();

            //visible individually?
            if (!product.VisibleIndividually)
            {
                //is this one an associated products?
                var parentGroupedProduct = await _productService.GetProductByIdAsync(product.ParentGroupedProductId);
                if (parentGroupedProduct == null)
                    return RedirectToRoute("Homepage");
                return RedirectToRoutePermanent("CustomProduct", new { id = id, SeName = await _urlRecordService.GetSeNameAsync(parentGroupedProduct) });
            }

            var _SeName = await _urlRecordService.GetSeNameAsync(product);
            // 301 Redirect to the correct search engine name in the url if it is wrong
            if (!string.IsNullOrEmpty(_SeName)
                && !StringComparer.OrdinalIgnoreCase.Equals(SeName, string.IsNullOrEmpty(variant.SeName) ? _SeName : variant.SeName))
            {
                //await EngineContext.Current.Resolve<ILogger>().InsertLogAsync(
                //    logLevel: Core.Domain.Logging.LogLevel.Information,
                //      shortMessage: "Invalid product se name.",
                //      fullMessage: string.Format("Product {0} does not have an SE Name set.", product.Id)
                //    );
                return RedirectToRoutePermanent("ProductwithVariantId", new { productId = productId, SeName = string.IsNullOrEmpty(variant.SeName) ? _SeName : variant.SeName });

            }

            //update existing shopping cart or wishlist  item?
            ShoppingCartItem updatecartitem = null;
            if (_shoppingCartSettings.AllowCartItemEditing && updatecartitemid > 0)
            {
                var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), storeId: (await _storeContext.GetCurrentStoreAsync()).Id);
                updatecartitem = cart.FirstOrDefault(x => x.Id == updatecartitemid);
                //not found?
                if (updatecartitem == null)
                {
                    return RedirectToRoutePermanent("Product", new { id = id, SeName = _SeName });
                }
                //is it this product?
                if (product.Id != updatecartitem.ProductId)
                {
                    return RedirectToRoutePermanent("Product", new { id = id, SeName = _SeName });
                }
            }

            //save as recently viewed
            await _recentlyViewedProductsService.CustomAddProductToRecentlyViewedListAsync(product.Id);

            //display "edit" (manage) link
            if (await _permissionService.AuthorizeAsync(StandardPermission.Security.ACCESS_ADMIN_PANEL) &&
                await _permissionService.AuthorizeAsync(StandardPermission.Catalog.PRODUCTS_VIEW))
            {
                //a vendor should have access only to his products
                if (await _workContext.GetCurrentVendorAsync() == null || (await _workContext.GetCurrentVendorAsync()).Id == product.VendorId)
                {
                    DisplayEditLink(Url.Action("Edit", "Product", new { id = product.Id, area =   AreaNames.ADMIN }));
                }
            }

            //activity log
            await _customerActivityService.InsertActivityAsync("PublicStore.ViewProduct",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.ViewProduct"), product.Name), product);

            //model
            var model = await _productModelFactory.PrepareCustomProductDetailsModelAsync(product, updatecartitem, false, variantId);




            //template
            var productTemplateViewPath = await _productModelFactory.PrepareProductTemplateViewPathAsync(product);
            if (model.UseNewVersionOfTemplate)
            {
                productTemplateViewPath = "New." + productTemplateViewPath;
            }
            model.IsMainProduct = true;
            return View(productTemplateViewPath, model);
        }
        public virtual async Task<IActionResult> CustomProductDetailsModern(int productId, int updatecartitemid = 0)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null || product.Deleted)
                return InvokeHttp404();

            var notAvailable =
                //published?
                (!product.Published && !_catalogSettings.AllowViewUnpublishedProductPage) ||
                //ACL (access control list) 
                !await _aclService.AuthorizeAsync(product) ||
                //Store mapping
                !await _storeMappingService.AuthorizeAsync(product) ||
                //availability dates
                !_productService.ProductIsAvailable(product);
            //Check whether the current user has a "Manage products" permission (usually a store owner)
            //We should allows him (her) to use "Preview" functionality
            var hasAdminAccess = await _permissionService.AuthorizeAsync(StandardPermission.Security.ACCESS_ADMIN_PANEL) && await _permissionService.AuthorizeAsync(StandardPermission.Catalog.PRODUCTS_VIEW);
            if (notAvailable && !hasAdminAccess)
                return InvokeHttp404();

            //visible individually?
            if (!product.VisibleIndividually)
            {
                //is this one an associated products?
                var parentGroupedProduct = await _productService.GetProductByIdAsync(product.ParentGroupedProductId);
                if (parentGroupedProduct == null)
                    return RedirectToRoute("Homepage");
                return RedirectToRoutePermanent("CustomProduct", new { SeName = await _urlRecordService.GetSeNameAsync(parentGroupedProduct) });
            }


            //update existing shopping cart or wishlist  item?
            ShoppingCartItem updatecartitem = null;
            if (_shoppingCartSettings.AllowCartItemEditing && updatecartitemid > 0)
            {
                var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), storeId: (await _storeContext.GetCurrentStoreAsync()).Id);
                updatecartitem = cart.FirstOrDefault(x => x.Id == updatecartitemid);
                //not found?
                if (updatecartitem == null)
                {
                    return RedirectToRoutePermanent("Product", new { SeName = await _urlRecordService.GetSeNameAsync(product) });

                }
                //is it this product?
                if (product.Id != updatecartitem.ProductId)
                {
                    return RedirectToRoutePermanent("Product", new { SeName = await _urlRecordService.GetSeNameAsync(product) });

                }
            }

            //save as recently viewed
            await _recentlyViewedProductsService.CustomAddProductToRecentlyViewedListAsync(product.Id);

            //display "edit" (manage) link
            if (await _permissionService.AuthorizeAsync(StandardPermission.Security.ACCESS_ADMIN_PANEL) &&
                await _permissionService.AuthorizeAsync(StandardPermission.Catalog.PRODUCTS_VIEW))
            {
                //a vendor should have access only to his products
                if (await _workContext.GetCurrentVendorAsync() == null || (await _workContext.GetCurrentVendorAsync()).Id == product.VendorId)
                {
                    DisplayEditLink(Url.Action("Edit", "Product", new
                    {
                        id = product.Id,
                        area =   AreaNames.ADMIN
                    }));
                }
            }

            //activity log
            await _customerActivityService.InsertActivityAsync("PublicStore.ViewProduct",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.ViewProduct"), product.Name), product);

            //model
            var model = await _productModelFactory.PrepareCustomProductDetailsModelAsync(product, updatecartitem, false);
            //template
            var productTemplateViewPath = await _productModelFactory.PrepareProductTemplateViewPathAsync(product);

            return View(productTemplateViewPath, model);
        }
        [CheckLanguageSeoCode(true)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> CustomNewProductsRss()
        {
            var feed = new RssFeed(
                $"{await _localizationService.GetLocalizedAsync(await _storeContext.GetCurrentStoreAsync(), x => x.Name)}: New products",
                "Information about products",
                new Uri(_webHelper.GetStoreLocation()),
                DateTime.UtcNow);

            if (!_catalogSettings.NewProductsEnabled)
                return new RssActionResult(feed, _webHelper.GetThisPageUrl(false));

            var items = new List<RssItem>();

            var storeId = (await _storeContext.GetCurrentStoreAsync()).Id;
            var products = await _productService.GetProductsMarkedAsNewAsync(storeId);

            foreach (var product in products)
            {
                var productUrl = Url.RouteUrl("Product", new { id = product.Id, SeName = await _urlRecordService.GetSeNameAsync(product) }, _webHelper.GetCurrentRequestProtocol());
                var productName = await _localizationService.GetLocalizedAsync(product, x => x.Name);
                var productDescription = await _localizationService.GetLocalizedAsync(product, x => x.ShortDescription);
                var item = new RssItem(productName, productDescription, new Uri(productUrl), $"urn:store:{(await _storeContext.GetCurrentStoreAsync()).Id}:newProducts:product:{product.Id}", product.CreatedOnUtc);
                items.Add(item);
                //uncomment below if you want to add RSS enclosure for pictures
                //var picture = _pictureService.GetPicturesByProductId(product.Id, 1).FirstOrDefault();
                //if (picture != null)
                //{
                //    var imageUrl = _pictureService.GetPictureUrl(picture, _mediaSettings.ProductDetailsPictureSize);
                //    item.ElementExtensions.Add(new XElement("enclosure", new XAttribute("type", "image/jpeg"), new XAttribute("url", imageUrl), new XAttribute("length", picture.PictureBinary.Length)));
                //}

            }
            feed.Items = items;
            return new RssActionResult(feed, _webHelper.GetThisPageUrl(false));
        }


        [IgnoreAntiforgeryToken]
        [HttpPost]
        public virtual async Task<IActionResult> BackInStockSubscription(IFormCollection form)
        {
            string message = await _localizationService.GetResourceAsync("Common.Success.Message");
            string email = form["email"];
            int.TryParse(form["productid"], out int productId);
            if (!string.IsNullOrEmpty(email) && productId != 0)
            {
                try
                {
               
                    var subscription = await _backInStockSubscriptionService
                       .FindSubscriptionAsync((await _workContext.GetCurrentCustomerAsync()).Id, productId, (await _storeContext.GetCurrentStoreAsync()).Id, email);
                    if (subscription == null)
                    {
                        if ((await _backInStockSubscriptionService
                       .GetAllSubscriptionsByCustomerIdAsync((await _workContext.GetCurrentCustomerAsync()).Id, (await _storeContext.GetCurrentStoreAsync()).Id, 0, 1))
                       .TotalCount >= _catalogSettings.MaximumBackInStockSubscriptions)
                        {
                            return Json(new
                            {
                                result = string.Format(await _localizationService.GetResourceAsync("BackInStockSubscriptions.MaxSubscriptions"), _catalogSettings.MaximumBackInStockSubscriptions)
                            });
                        }
                        subscription = new BackInStockSubscription
                        {
                            CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                            ProductId = productId,
                            StoreId = (await _storeContext.GetCurrentStoreAsync()).Id,
                            CreatedOnUtc = DateTime.UtcNow,
                            Email = email
                        };

                      
                
                        string url = _httpContextAccessor.HttpContext?.Request.Headers["Referer"];
                        string absoluteUrl = _httpContextAccessor.HttpContext?.Request.Headers["Referer"];
                        string userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"];
                        await _mailchimpService.NewsLetterSignup(email, "OutOfStock", url, absoluteUrl, userAgent, "OutOfStock",
                            productId, 0, "");
                        await _backInStockSubscriptionService.InsertSubscriptionAsync(subscription);
                        return Ok(message);
                    }
                    else
                    {
                        return Conflict(await _localizationService.GetResourceAsync("Common.Subscribe.Email.Exist"));
                    }

                }
                catch (Exception exp)
                {
                    return BadRequest(message);
                }
            }
            return BadRequest(await _localizationService.GetResourceAsync("Common.Email.Required"));
        }
        public virtual async Task<IActionResult> Reviews(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null || product.Deleted || !product.Published || !product.AllowCustomerReviews)
                return RedirectToRoute("Homepage");

            return View(product);
        }
        public IActionResult ProductOverview(int id)
        {
            return ViewComponent("Custom_ProductOverView", new { productId = id });
        }

        #endregion


    
    }
}
