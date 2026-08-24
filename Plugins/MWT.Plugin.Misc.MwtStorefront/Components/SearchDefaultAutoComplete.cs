using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Services.Search;
using MWT.Plugin.Misc.MwtStorefront.Catalog;
using MWT.Plugin.Misc.MwtStorefront.Factories;
using MWT.Plugin.Misc.MwtStorefront.Models.Catalog;
using MWT.Plugin.Misc.MwtStorefront.SearchBox;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Http;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework.Components;

namespace MWT.Plugin.Misc.MwtStorefront.Components
{
    public class SearchDefaultAutoCompleteViewComponent : NopViewComponent
    {
        private readonly CatalogSettings _catalogSettings;
        private readonly IAclService _aclService;
        private readonly ICustomProductModelFactory _productModelFactory;
        private readonly IProductService _productService;
        private readonly IRecentlyViewedProductsService _recentlyViewedProductsService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ISettingService _settingService;
        private readonly ISearchLogService _searchLogService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public SearchDefaultAutoCompleteViewComponent(CatalogSettings catalogSettings,
            IAclService aclService,
            ICustomProductModelFactory productModelFactory,
            IProductService productService,
            IRecentlyViewedProductsService recentlyViewedProductsService,
            IStoreMappingService storeMappingService,
            ISettingService settingService,
            ISearchLogService searchLogService,
            IStaticCacheManager staticCacheManager,
            IWorkContext workContext,
            ILocalizationService localizationService,
            IHttpContextAccessor httpContextAccessor)
        {
            _catalogSettings = catalogSettings;
            _aclService = aclService;
            _productModelFactory = productModelFactory;
            _productService = productService;
            _recentlyViewedProductsService = recentlyViewedProductsService;
            _storeMappingService = storeMappingService;
            _settingService = settingService;
            _searchLogService = searchLogService;
            _staticCacheManager = staticCacheManager;
            _workContext = workContext;
            _localizationService = localizationService;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync(int? productThumbPictureSize, bool? preparePriceModel)
        {
            var customer = await this._workContext.GetCurrentCustomerAsync();
            if (customer == null)
                return Content("");
            var httpContext = _httpContextAccessor.HttpContext;
            var cookieName = $"{NopCookieDefaults.Prefix}{NopCookieDefaults.RecentlyViewedProductsCookie}";
            httpContext.Request.Cookies.TryGetValue(cookieName, out var productIdsCookie);
            SearchDefaultAutoCompleteModel model = new SearchDefaultAutoCompleteModel();
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopCatalogDefaults.SearchDefaultAutoCompleteCacheKey,
             customer.Id, productIdsCookie ?? "");

            model = await this._staticCacheManager.GetAsync(cacheKey, async () =>
            {
                SearchDefaultAutoCompleteModel _model = new SearchDefaultAutoCompleteModel();
                #region Recently Viewed Products
                var recentlyViewedProducts = new List<CustomProductOverviewModel>();
                int recentlyViewedProductsNumber = await _settingService.GetSettingByKeyAsync<int>("catalogsettings.AutoCompleteSearch.NoOfRecentlyViewedProductsNumber");
                if (_catalogSettings.RecentlyViewedProductsEnabled)
                {
                    var products = await (await _recentlyViewedProductsService.GetRecentlyViewedProductsAsync(recentlyViewedProductsNumber))
                           //ACL and store mapping
                           .WhereAwait(async p => await _aclService.AuthorizeAsync(p) && await _storeMappingService.AuthorizeAsync(p))
                           //availability dates
                           .Where(p => _productService.ProductIsAvailable(p)).ToListAsync();

                    if (products.Any())
                    {
                        //prepare model

                        recentlyViewedProducts.AddRange(await _productModelFactory.PrepareCustomProductOverviewModelsAsync(products,
                            true,
                            true,
                            productThumbPictureSize));
                    }
                }
                _model.RecentlyViewedProducts = recentlyViewedProducts;
                #endregion

                var searchlog = await _searchLogService.GetLatestSearchTermOFCustomer(customer.Id);
                _model.RecentSearchTerms = searchlog.Select(s => s.Keyword).ToList();
                return _model;

            });

            List<ItemAutoCompleteSearchModel> items = new List<ItemAutoCompleteSearchModel>();
            if (model.RecentlyViewedProducts.Count > 0)
            {
                List<SearchRecentlyViewedProduct> products = new List<SearchRecentlyViewedProduct>();
                foreach (var product in model.RecentlyViewedProducts)
                {
                    products.Add(new SearchRecentlyViewedProduct()
                    {
                        Name = product.Name.Replace("\"", "double-quot"),
                        Url = Url.RouteUrl("Product", new { id = product.Id, SeName = product.SeName }),
                        ImageUrl = product.DefaultPictureModel.ImageUrl
                    });
                }

                items.Add(new ItemAutoCompleteSearchModel()
                {
                    Header = await _localizationService.GetResourceAsync("Search.AutoComplete.RecentlyViewed.Heading")
                });

                items.Add(new ItemAutoCompleteSearchModel()
                {
                    Products = products
                });

            }

            if (model.RecentSearchTerms.Count > 0)
            {

                items.Add(new ItemAutoCompleteSearchModel()
                {
                    Header = await _localizationService.GetResourceAsync("Search.AutoComplete.RecentSearch.Heading"),
                    IsRecentSearch = true
                });

                foreach (var searchTerm in model.RecentSearchTerms)
                {
                    items.Add(new ItemAutoCompleteSearchModel()
                    {
                        Name = searchTerm.Replace("\"", "double-quot"),
                        Url = Url.RouteUrl("ProductSearch", new { SearchTerm = searchTerm }),
                        IsRecentSearch = true
                    });
                }
                items.Add(new ItemAutoCompleteSearchModel()
                {
                    Name = (await _localizationService.GetResourceAsync("Search.AutoComplete.ClearRecentSearch")).Replace("\"", "double-quot"),
                    Url = "",
                    IsRecentSearch = true

                });
            }
            return new ContentViewComponentResult(JsonConvert.SerializeObject(items));
        }
    }
}