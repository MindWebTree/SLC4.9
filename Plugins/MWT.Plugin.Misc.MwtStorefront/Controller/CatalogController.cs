using ExCSS;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Domain;
using MWT.Nop.Core.Infrastructure;
using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Services.ElasticSearch;
using MWT.Nop.Core.Services.Search;
using MWT.Plugin.Misc.MwtStorefront.Factories.Catalog;
using MWT.Plugin.Misc.MwtStorefront.Models.Catalog;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.FilterLevels;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.FilterLevels;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Nop.Web.Controllers;
using Nop.Web.Factories;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Catalog;

namespace MWT.Plugin.Misc.MwtStorefront.Controller
{
    [AutoValidateAntiforgeryToken]
    public partial class CatalogController : BasePublicController
    {

        #region Fields

        protected readonly CatalogSettings _catalogSettings;
        protected readonly IAclService _aclService;
        protected readonly ICustomCatalogModelFactory _catalogModelFactory;
        protected readonly ICategoryService _categoryService;
        protected readonly ICustomerActivityService _customerActivityService;
        protected readonly IFilterLevelValueModelFactory _filterLevelValueModelFactory;
        protected readonly IFilterLevelValueService _filterLevelValueService;
        protected readonly IGenericAttributeService _genericAttributeService;
        protected readonly ILocalizationService _localizationService;
        protected readonly IManufacturerService _manufacturerService;
        protected readonly INopUrlHelper _nopUrlHelper;
        protected readonly IPermissionService _permissionService;
        protected readonly IProductModelFactory _productModelFactory;
        protected readonly ICustomProductService _productService;
        protected readonly IProductTagService _productTagService;
        protected readonly IStoreContext _storeContext;
        protected readonly IStoreMappingService _storeMappingService;
        protected readonly IVendorService _vendorService;
        protected readonly IWebHelper _webHelper;
        protected readonly IWorkContext _workContext;
        protected readonly FilterLevelSettings _filterLevelSettings;
        protected readonly MediaSettings _mediaSettings;
        protected readonly VendorSettings _vendorSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICustomWorkContext _customWorkContext;
        private readonly IFuzzySearchService _fuzzySearchService;
        private readonly IUrlRecordService  _urlRecordService;
        #endregion

        #region Ctor

        public CatalogController(CatalogSettings catalogSettings,
            IAclService aclService,
            ICustomCatalogModelFactory catalogModelFactory,
            ICategoryService categoryService,
            ICustomerActivityService customerActivityService,
            IFilterLevelValueModelFactory filterLevelValueModelFactory,
            IFilterLevelValueService filterLevelValueService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            IManufacturerService manufacturerService,
            INopUrlHelper nopUrlHelper,
            IPermissionService permissionService,
            IProductModelFactory productModelFactory,
            ICustomProductService productService,
            IProductTagService productTagService,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            IVendorService vendorService,
            IWebHelper webHelper,
            IWorkContext workContext,
            FilterLevelSettings filterLevelSettings,
            MediaSettings mediaSettings,
            VendorSettings vendorSettings,
            IHttpContextAccessor httpContextAccessor,
            ICustomWorkContext customWorkContext,
            IFuzzySearchService fuzzySearchService,
            IUrlRecordService urlRecordService)
        {
            _catalogSettings = catalogSettings;
            _aclService = aclService;
            _catalogModelFactory = catalogModelFactory;
            _categoryService = categoryService;
            _customerActivityService = customerActivityService;
            _filterLevelValueModelFactory = filterLevelValueModelFactory;
            _filterLevelValueService = filterLevelValueService;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _manufacturerService = manufacturerService;
            _nopUrlHelper = nopUrlHelper;
            _permissionService = permissionService;
            _productModelFactory = productModelFactory;
            _productService = productService;
            _productTagService = productTagService;
            _storeContext = storeContext;
            _storeMappingService = storeMappingService;
            _vendorService = vendorService;
            _webHelper = webHelper;
            _workContext = workContext;
            _filterLevelSettings = filterLevelSettings;
            _mediaSettings = mediaSettings;
            _vendorSettings = vendorSettings;
            _httpContextAccessor = httpContextAccessor;
            _customWorkContext = customWorkContext;
            _fuzzySearchService = fuzzySearchService;
            _urlRecordService = urlRecordService;
        }

        #endregion
        public virtual async Task<IActionResult> CustomManufacturer(int id, string SeName, CustomCatalogProductsCommand command)
        {
            return NotFound();
        }
        #region Categories

        [SaveLastContinueShoppingPage]
        public virtual async Task<IActionResult> CustomCategory(int id, string SeName, CustomCatalogProductsCommand command)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);

            if (category == null || !await CheckCategoryAvailabilityAsync(category))
                return InvokeHttp404();

     

            //display "edit" (manage) link
            if (await _permissionService.AuthorizeAsync(StandardPermission.Security.ACCESS_ADMIN_PANEL) && await _permissionService.AuthorizeAsync(StandardPermission.Catalog.CATEGORIES_VIEW))
                DisplayEditLink(Url.Action("Edit", "Category", new { id = category.Id, area = AreaNames.ADMIN  }));

            var _SeName = await EngineContext.Current.Resolve<IUrlRecordService>().GetSeNameAsync(category);
            // 301 Redirect to the correct search engine name in the url if it is wrong
            if (!string.IsNullOrEmpty(_SeName)
                && !StringComparer.OrdinalIgnoreCase.Equals(SeName, _SeName))
            {
                //await EngineContext.Current.Resolve<ILogger>().InsertLogAsync(
                //    logLevel: Core.Domain.Logging.LogLevel.Information,
                //      shortMessage: "Invalid category se name.",
                //      fullMessage: string.Format("Category {0} does not have an SE Name set.", category.Id)
                //    );
                return RedirectToRoutePermanent("Category", new { id = id, SeName = _SeName });

            }

            //activity log
            await _customerActivityService.InsertActivityAsync("PublicStore.ViewCategory",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.ViewCategory"), category.Name), category);

            //model

            var _httpContextAccessor = EngineContext.Current.Resolve<IHttpContextAccessor>();

            (string templateViewPath, string listingViewPath, string filterViewPath, string FilterViewPathForMobile, int pictureSize, bool isHorizontal) =
               await _catalogModelFactory.CustomPrepareCategoryTemplateViewPathAsync(category.CategoryTemplateId);

            var model = await _catalogModelFactory.PrepareCustomCategoryModelAsync(category, command, _httpContextAccessor.HttpContext.Request.QueryString.Value, pictureSize);

            if (model.CatalogProductsModel != null)
            {
                model.CatalogProductsModel.CategoryID = model.Id;
                model.CatalogProductsModel.CategoryName = model.Name;
                model.CatalogProductsModel.Description = model.Description;
                model.CatalogProductsModel.Sename = model.SeName;
            }

            //template

            model.GridLineViewPath = listingViewPath;
            model.FilterViewPath = filterViewPath;
            model.FilterViewPathForMobile = FilterViewPathForMobile;
            return View(templateViewPath, model);
        }

        [SaveLastContinueShoppingPage]
        public virtual async Task<IActionResult> CustomCategoryModern(int categoryId, CustomCatalogProductsCommand command)
        {
            var category = await _categoryService.GetCategoryByIdAsync(categoryId);

            if (!await CheckCategoryAvailabilityAsync(category))
                return InvokeHttp404();

   
        

            //display "edit" (manage) link
            if (await _permissionService.AuthorizeAsync(StandardPermission.Security.ACCESS_ADMIN_PANEL) && await _permissionService.AuthorizeAsync(StandardPermission.Catalog.CATEGORIES_VIEW))
                DisplayEditLink(Url.Action("Edit", "Category", new { id = category.Id, area = AreaNames.ADMIN  }));



            //activity log
            await _customerActivityService.InsertActivityAsync("PublicStore.ViewCategory",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.ViewCategory"), category.Name), category);

        
            //model
            var model = await _catalogModelFactory.PrepareCustomCategoryModelAsync(category, command, _httpContextAccessor.HttpContext.Request.QueryString.Value, 0);

            //template
            var templateViewPath = await _catalogModelFactory.PrepareCategoryTemplateViewPathAsync(category.CategoryTemplateId);
            return View(templateViewPath, model);
        }
        [CheckLanguageSeoCode(true)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> GetCustomCategoryProducts(int categoryId, CustomCatalogProductsCommand command, string queryString)
        {
            var category = await _categoryService.GetCategoryByIdAsync(categoryId);

            if (!await CheckCategoryAvailabilityAsync(category))
                return NotFound();
            var _httpContextAccessor = EngineContext.Current.Resolve<IHttpContextAccessor>();

            (string templateViewPath, string listingViewPath, string filterViewPath, string FilterViewPathForMobile, int pictureSize, bool IsHorizontal) =
               await _catalogModelFactory.CustomPrepareCategoryTemplateViewPathAsync(category.CategoryTemplateId);

            var model = await _catalogModelFactory.PrepareCustomCategoryProductsModelAsync(category, command,
                _httpContextAccessor.HttpContext.Request.QueryString.Value, pictureSize: pictureSize);

     
            model.CategoryID = categoryId;
            model.CategoryName = category.Name;
            model.Description = category.Description;
            model.Sename = await _urlRecordService.GetSeNameAsync(category);
            ViewData["openTabs"] = command.openTabs;
            ViewData["topOpenTabs"] = command.topOpenTabs;
            ViewData["popupOpenTabs"] = command.popupOpenTabs;
            if (_customWorkContext.IsMobileDevice())
            {
                return Json(new
                {

                    products = await RenderPartialViewToStringAsync(listingViewPath, model),
                    mobileFilters = await RenderPartialViewToStringAsync(FilterViewPathForMobile, model.SpecificationFilter),
                    topFilters = await RenderPartialViewToStringAsync("_AjaxFilterSpecsBoxTop", model),
                    filters = string.Join(",", model.SpecificationFilter.Attributes.Select(m => m.Name.ToLower().Trim()).ToArray())
                }); ;
            }
            else
            {
                if (IsHorizontal)
                {
                    return Json(new
                    {
                        products = await RenderPartialViewToStringAsync(listingViewPath, model),
                        horizontalFilters = await RenderPartialViewToStringAsync(filterViewPath, model.SpecificationFilter),
                        popupFilters = await RenderPartialViewToStringAsync("_AjaxFilterSpecsBoxPopup", model.SpecificationFilter),
                        filters = string.Join(",", model.SpecificationFilter.Attributes.Select(m => m.Name.ToLower().Trim()).ToArray()),

                    });
                }
                else
                {
                    return Json(new
                    {
                        products = await RenderPartialViewToStringAsync(listingViewPath, model),
                        sideFilters = await RenderPartialViewToStringAsync(filterViewPath, model.SpecificationFilter),
                        topFilters = await RenderPartialViewToStringAsync("_AjaxFilterSpecsBoxTop", model),
                        filters = string.Join(",", model.SpecificationFilter.Attributes.Select(m => m.Name.ToLower().Trim()).ToArray()),
                    });
                }
            }

        }


        public virtual async Task<IActionResult> CustomSearchTermAutoComplete(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Content("");

            term = term.Trim();

            if (string.IsNullOrWhiteSpace(term) || term.Length < _catalogSettings.ProductSearchTermMinimumLength)
                return Content("");


            //products
            var productNumber = _catalogSettings.ProductSearchAutoCompleteNumberOfProducts > 0 ?
                _catalogSettings.ProductSearchAutoCompleteNumberOfProducts : 10;

            var _elasticSearchHelpService = EngineContext.Current.Resolve<IElasticSearchHelpService>();
            return Content(await _elasticSearchHelpService.GetAutoCompleResult(term));
            //var products = await _productService.SearchProductsAsync(0,
            //storeId: (await _storeContext.GetCurrentStoreAsync()).Id,
            //keywords: term,
            //languageId: (await _workContext.GetWorkingLanguageAsync()).Id,
            //visibleIndividuallyOnly: true,
            //pageSize: productNumber);

            //var showLinkToResultSearch = _catalogSettings.ShowLinkToAllResultInSearchAutoComplete && (products.TotalCount > productNumber);

            //var models = (await _productModelFactory.PrepareProductOverviewModelsAsync(products, false, _catalogSettings.ShowProductImagesInSearchAutoComplete, _mediaSettings.AutoCompleteSearchThumbPictureSize)).ToList();
            //var result = (from p in models
            //              select new
            //              {
            //                  label = p.Name,
            //                  producturl = Url.RouteUrl("Product", new { id = p.Id, SeName = p.SeName }),
            //                  productpictureurl = p.DefaultPictureModel.ImageUrl,
            //                  showlinktoresultsearch = showLinkToResultSearch
            //              })
            //    .ToList();
            //return Json(result);
        }

        public virtual async Task<IActionResult> CustomSearch(CustomSearchModel model, CustomCatalogProductsCommand command)
        {
            var _httpContextAccessor = EngineContext.Current.Resolve<IHttpContextAccessor>();
            //'Continue shopping' URL
            await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
                NopCustomerDefaults.LastContinueShoppingPageAttribute,
                _webHelper.GetThisPageUrl(true),
                (await _storeContext.GetCurrentStoreAsync()).Id);

            if (model == null)
                model = new CustomSearchModel();
            else
            {
                #region Save Search Terms

                if ((model.SearchTerm ?? "").Trim() != "")
                {
                    try
                    {
                        var customer = await _workContext.GetCurrentCustomerAsync();
                        if (customer != null)
                        {
                            var _searchLogService = EngineContext.Current.Resolve<ISearchLogService>();
                            await _searchLogService.InsertSearchLog(new SearchLog()
                            {
                                CreatedOn = DateTime.Now,
                                UpdatedOn = DateTime.Now,
                                CustomerId = customer.Id,
                                Keyword = (model.SearchTerm ?? "").Trim()
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        var _logger = EngineContext.Current.Resolve<ILogger>();
                        await _logger.InsertLogAsync(LogLevel.Error, "Failed to insert search keyword",
                            $"search key word {model.SearchTerm} Exception {ex.Message}"
                            );
                    }
                }

                #endregion
                if ((model.SearchTerm ?? "").Trim() != "")
                {
           
                    #region Product Search
                    var product = await _productService.GetProductBySearchTerm(CustomCommonHelper.RemoveSpecialCharacters(model.SearchTerm));
                    if (product != null)
                    {

                        return Redirect(Url.RouteUrl("Product", new { id = product.Id, SeName = await _urlRecordService.GetSeNameAsync(product) }));

                    }

                    #endregion
       
                    #region Categories Search

                    var searchedTerms = await _fuzzySearchService.SearchCategories(model.SearchTerm);
                    if (searchedTerms.Any())
                    {
                        var category = await _categoryService.GetCategoryByIdAsync(searchedTerms.FirstOrDefault().categoryId);
                        return Redirect(Url.RouteUrl("Category", new { id = category.Id, SeName = await _urlRecordService.GetSeNameAsync(category) }));
                    }

                    #endregion


                    #region Categories Search

                    searchedTerms = await _fuzzySearchService.SearchCategoryGenricKeyWords(model.SearchTerm);
                    if (searchedTerms.Any())
                    {
                        var category = await _categoryService.GetCategoryByIdAsync(searchedTerms.FirstOrDefault().categoryId);
                        return Redirect(Url.RouteUrl("Category", new { id = category.Id, SeName = await _urlRecordService.GetSeNameAsync(category) }));
                    }

                    #endregion
                }
            }
            model = await _catalogModelFactory.PrepareCustomSearchModelAsync(model, command, _httpContextAccessor.HttpContext.Request.QueryString.Value);

            if (model.CatalogProductsModel.TotalItems > 0)
            {
                var _staticCacheManager = EngineContext.Current.Resolve<IStaticCacheManager>();
                var key = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.SearchResultProductsDefaultCacheKey,
                  model.SearchTerm);
                var data = _staticCacheManager.Get(key, () =>
                {
                    return new int[] { };
                });



                if (data.Length == 0)
                {
                    if ((model.CatalogProductsModel.SpecificationFilter == null ||
                        (model.CatalogProductsModel.SpecificationFilter.Attributes.Count == 0
                        ||

                        (model.CatalogProductsModel.SpecificationFilter.Attributes.Count == 1
                        && !model.CatalogProductsModel.SpecificationFilter.Attributes.FirstOrDefault().Values.Where(v => v.Selected).Any()))
                        )
                  )
                    {
                        if (model.CatalogProductsModel.OrderBy == 0 && model.CatalogProductsModel.PageIndex == 0)
                            await _staticCacheManager.SetAsync(key, model.CatalogProductsModel.Products.Select(prd => prd.Id).ToArray());

                        else
                        {
                            key = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.SearchResultProductsAlternateCacheKey,
                                          model.SearchTerm);
                            data = _staticCacheManager.Get(key, () =>
                            {
                                return new int[] { };
                            });
                            if (data.Length == 0)
                                await _staticCacheManager.SetAsync(key, model.CatalogProductsModel.Products.Select(prd => prd.Id).ToArray());
                        }

                    }
                    else
                    {
                        key = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.SearchResultProductsSuggestedCacheKey,
                                                           model.SearchTerm);
                        data = _staticCacheManager.Get(key, () =>
                        {
                            return new int[] { };
                        });
                        if (data.Length == 0)
                            await _staticCacheManager.SetAsync(key, model.CatalogProductsModel.Products.Select(prd => prd.Id).ToArray());
                    }
                }

            }

            return View("search", model);
        }

        public virtual async Task<IActionResult> GetSearchCategoryProducts(string searchTerm, CustomCatalogProductsCommand command, string queryString)
        {
            if (command == null)
                return NotFound();
            var _settingService = EngineContext.Current.Resolve<ISettingService>();
            string listingViewPath = await _settingService.GetSettingByKeyAsync<string>("Search.listingView");
            string filterViewPathForMobile = await _settingService.GetSettingByKeyAsync<string>("Search.filterViewForMobile");
            string filterViewPath = await _settingService.GetSettingByKeyAsync<string>("Search.filterViewPath");
            bool IsHorizontal = await _settingService.GetSettingByKeyAsync<bool>("Search.IsHorizontal");
            var _httpContextAccessor = EngineContext.Current.Resolve<IHttpContextAccessor>();

            CustomSearchModel searchModel = new CustomSearchModel();
            searchModel.SearchTerm = searchTerm;

            var model = await _catalogModelFactory.PrepareCustomSearchProductsModelAsync(searchModel, command, _httpContextAccessor.HttpContext.Request.QueryString.Value);

            ViewData["openTabs"] = command.openTabs;
            ViewData["popupOpenTabs"] = command.popupOpenTabs;
            ViewData["topOpenTabs"] = command.topOpenTabs;
            if (_customWorkContext.IsMobileDevice())
            {
                return Json(new
                {

                    products = await RenderPartialViewToStringAsync(listingViewPath, model),
                    mobileFilters = await RenderPartialViewToStringAsync(filterViewPathForMobile, model.SpecificationFilter),
                    filters = string.Join(",", model.SpecificationFilter.Attributes.Select(m => m.Name.ToLower().Trim()).ToArray())
                }); ;
            }
            else

            {
                if (IsHorizontal)
                {
                    return Json(new
                    {
                        products = await RenderPartialViewToStringAsync(listingViewPath, model),
                        horiZontalFilters = await RenderPartialViewToStringAsync(filterViewPath, model.SpecificationFilter),
                        popupFilters = await RenderPartialViewToStringAsync("_AjaxFilterSpecsBoxPopup", model.SpecificationFilter),
                        filters = string.Join(",", model.SpecificationFilter.Attributes.Select(m => m.Name.ToLower().Trim()).ToArray()),

                    });
                }
                else
                {
                    return Json(new
                    {
                        products = await RenderPartialViewToStringAsync(listingViewPath, model),
                        sideFilters = await RenderPartialViewToStringAsync(filterViewPathForMobile, model.SpecificationFilter),
                        filters = string.Join(",", model.SpecificationFilter.Attributes.Select(m => m.Name.ToLower().Trim()).ToArray()),
                    });
                }
            }

        }

        [HttpPost]
        public virtual async Task<IActionResult> ClearRecentSearch()
        {
            var _searchLogService = EngineContext.Current.Resolve<ISearchLogService>();
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (customer != null)
            {
                await _searchLogService.ClearRecentSearchOfCustomer(customer.Id);
            }
            return new NullJsonResult();
        }

        //public virtual async Task<IActionResult> CustomSearchTest(SearchModel model, CatalogProductsCommand command)
        //{
        //    //'Continue shopping' URL
        //    await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
        //        NopCustomerDefaults.LastContinueShoppingPageAttribute,
        //        _webHelper.GetThisPageUrl(true),
        //        (await _storeContext.GetCurrentStoreAsync()).Id);

        //    if (model == null)
        //        model = new SearchModel();
        //        return Content(await _catalogModelFactory.PrepareCustomSearchTestModelAsync(model, command));

        //}

        #endregion

        #region Utilities
        protected virtual async Task<bool> CheckCategoryAvailabilityAsync(Category category)
        {
            if (category is null)
                return false;

            var isAvailable = true;

            if (category.Deleted)
                isAvailable = false;

            var notAvailable =
                //published?
                !category.Published ||
                //ACL (access control list) 
                !await _aclService.AuthorizeAsync(category) ||
                //Store mapping
                !await _storeMappingService.AuthorizeAsync(category);
            //Check whether the current user has a "Manage categories" permission (usually a store owner)
            //We should allows him (her) to use "Preview" functionality
            var hasAdminAccess = await _permissionService.AuthorizeAsync(StandardPermission.Security.ACCESS_ADMIN_PANEL) && await _permissionService.AuthorizeAsync(StandardPermission.Catalog.CATEGORIES_VIEW);
            if (notAvailable && !hasAdminAccess)
                isAvailable = false;

            return isAvailable;
        }
        #endregion
    }
}
