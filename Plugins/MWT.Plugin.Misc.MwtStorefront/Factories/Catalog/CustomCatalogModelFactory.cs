using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using MWT.Nop.Core.Infrastructure;
using MWT.Nop.Core.Service;
using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Services.Catalog;
using MWT.Nop.Core.Services.ElasticSearch;
using MWT.Plugin.Misc.MwtStorefront.Models.Catalog;
using MWT.Plugin.Misc.MwtStorefront.Models.Media;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.FilterLevels;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;
using System.Net;

namespace MWT.Plugin.Misc.MwtStorefront.Factories.Catalog
{
    public class CustomCatalogModelFactory : CatalogModelFactory, ICustomCatalogModelFactory
    {
        #region Fields
        private readonly ICustomProductModelFactory _customProductModelFactory;
        private readonly IProductExtendedService _customProductService;
        private readonly ICustomSpecificationAttributeService _customSpecificationAttributeService;
        private readonly ICustomWorkContext _customWorkContext;
        private readonly ISettingService _settingService;
        private readonly IElasticSearchHelpService _elasticSearchHelpService;
        private readonly ILogger _logger;
        private readonly ICustomCategoryService _customCategoryService;
        private readonly IFiltersMappingByEntityService _filtersMappingByEntityService;

        #endregion
        public CustomCatalogModelFactory(CatalogSettings catalogSettings, CustomerSettings customerSettings,
            ForumSettings forumSettings, ICategoryService categoryService,
            ICategoryTemplateService categoryTemplateService, ICurrencyService currencyService,
            ICustomerService customerService, IEventPublisher eventPublisher, IFilterLevelValueService filterLevelValueService,
            IGenericAttributeService genericAttributeService, IHttpContextAccessor httpContextAccessor, IJsonLdModelFactory jsonLdModelFactory,
            ILocalizationService localizationService, IManufacturerService manufacturerService, IManufacturerTemplateService manufacturerTemplateService,
            INopUrlHelper nopUrlHelper, IPictureService pictureService, IProductModelFactory productModelFactory, IProductReviewService productReviewService,
            IProductService productService, IProductTagService productTagService, ISearchTermService searchTermService, ISpecificationAttributeService specificationAttributeService,
            IStaticCacheManager staticCacheManager, IStoreContext storeContext, IUrlRecordService urlRecordService, IVendorService vendorService, IWebHelper webHelper,
            IWorkContext workContext, MediaSettings mediaSettings, SeoSettings seoSettings, VendorSettings vendorSettings,
            ICustomProductModelFactory customProductModelFactory, IProductExtendedService customProductService, ICustomSpecificationAttributeService customSpecificationAttributeService,
            ICustomWorkContext customWorkContext, ISettingService settingService, IElasticSearchHelpService elasticSearchHelpService,
            ILogger logger, ICustomCategoryService customCategoryService, IFiltersMappingByEntityService filtersMappingByEntityService
            ) : base(catalogSettings, customerSettings,
                forumSettings, categoryService, categoryTemplateService, currencyService, customerService, eventPublisher, filterLevelValueService, genericAttributeService,
                httpContextAccessor, jsonLdModelFactory, localizationService, manufacturerService, manufacturerTemplateService, nopUrlHelper, pictureService,
                productModelFactory, productReviewService, productService, productTagService, searchTermService, specificationAttributeService, staticCacheManager,
                storeContext, urlRecordService, vendorService, webHelper, workContext, mediaSettings, seoSettings, vendorSettings)
        {
            _customProductModelFactory = customProductModelFactory;
            _customProductService = customProductService;
            _customSpecificationAttributeService = customSpecificationAttributeService;
            _customWorkContext = customWorkContext;
            _settingService = settingService;
            _elasticSearchHelpService = elasticSearchHelpService;
            _logger = logger;
            _customCategoryService = customCategoryService;
            _filtersMappingByEntityService = filtersMappingByEntityService;
        }


        #region Categories

        public virtual async Task<List<CategoryModel>> CustomPrepareHomepageCategoryModelsAsync()
        {
            var language = await _workContext.GetWorkingLanguageAsync();
            var customer = await _workContext.GetCurrentCustomerAsync();
            var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
            var store = await _storeContext.GetCurrentStoreAsync();
            var pictureSize = _mediaSettings.CategoryThumbPictureSize;
            var categoriesCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.CustomCategoryHomepageKey,
                store, customerRoleIds, pictureSize, language, _webHelper.IsCurrentConnectionSecured());

            var model = await _staticCacheManager.GetAsync(categoriesCacheKey, async () =>
            {
                var homepageCategories = (await _categoryService.GetAllCategoriesDisplayedOnHomepageAsync()).OrderBy(c => c.AlternateDisplayOrder).ThenBy(c => c.Id);
                return await homepageCategories.SelectAwait(async category =>
                {
                    var catModel = new CategoryModel
                    {
                        Id = category.Id,
                        Name = await _localizationService.GetLocalizedAsync(category, x => x.Name),
                        Description = await _localizationService.GetLocalizedAsync(category, x => x.Description),
                        MetaKeywords = await _localizationService.GetLocalizedAsync(category, x => x.MetaKeywords),
                        MetaDescription = await _localizationService.GetLocalizedAsync(category, x => x.MetaDescription),
                        MetaTitle = await _localizationService.GetLocalizedAsync(category, x => x.MetaTitle),
                        SeName = await _urlRecordService.GetSeNameAsync(category),
                    };

                    //prepare picture model
                    var secured = _webHelper.IsCurrentConnectionSecured();
                    var categoryPictureCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.CategoryPictureModelKey,
                        category, pictureSize, true, language, secured, store);
                    catModel.PictureModel = await _staticCacheManager.GetAsync(categoryPictureCacheKey, async () =>
                    {
                        var picture = await _pictureService.GetPictureByIdAsync(category.PictureId);
                        string fullSizeImageUrl, imageUrl;

                        (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                        (imageUrl, _) = await _pictureService.GetPictureUrlAsync(picture, pictureSize);

                        var titleLocale = await _localizationService.GetResourceAsync("Media.Category.ImageLinkTitleFormat");
                        var altLocale = await _localizationService.GetResourceAsync("Media.Category.ImageAlternateTextFormat");
                        return new PictureModel
                        {
                            FullSizeImageUrl = fullSizeImageUrl,
                            ImageUrl = imageUrl,
                            Title = string.Format(titleLocale, catModel.Name),
                            AlternateText = string.Format(altLocale, catModel.Name)
                        };
                    });

                    return catModel;
                }).ToListAsync();
            });

            return model;
        }
        #endregion
        public virtual async Task<CustomCategoryModel> PrepareCustomCategoryModelAsync(Category category, CustomCatalogProductsCommand command, string queryString,
                   int prdPictureSize)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            if (command == null)
                throw new ArgumentNullException(nameof(command));


            var model = new CustomCategoryModel
            {
                Id = category.Id,
                Name = await _localizationService.GetLocalizedAsync(category, x => x.Name),
                Description = await _localizationService.GetLocalizedAsync(category, x => x.Description),
                AdditionalDescription = await _localizationService.GetLocalizedAsync(category, x => x.AdditionalDescription),
                MetaKeywords = await _localizationService.GetLocalizedAsync(category, x => x.MetaKeywords),
                MetaDescription = await _localizationService.GetLocalizedAsync(category, x => x.MetaDescription),
                MetaTitle = await _localizationService.GetLocalizedAsync(category, x => x.MetaTitle),
                SeName = await _urlRecordService.GetSeNameAsync(category),
                CatalogProductsModel = await PrepareCustomCategoryProductsModelAsync(category, command, queryString, pictureSize: prdPictureSize),
                QuickFilterHeading = category.QuickFilterHeading,
                EnableInfiniteScroll = category.EnableInfiniteScroll,
                DisplayGridListOption = category.DisplayGridListOption,
                FeaturedListingDisplaySimilarOnTop = category.FeaturedListingDisplaySimilarOnTop
            };

            //category breadcrumb
            if (_catalogSettings.CategoryBreadcrumbEnabled)
            {
                model.DisplayCategoryBreadcrumb = true;

                model.CategoryBreadcrumb = await (await _categoryService.GetCategoryBreadCrumbAsync(category)).SelectAwait(async catBr =>
                    new CategoryModel
                    {
                        Id = catBr.Id,
                        Name = await _localizationService.GetLocalizedAsync(catBr, x => x.Name),
                        SeName = await _urlRecordService.GetSeNameAsync(catBr)
                    }).ToListAsync();
            }

            var currentStore = await _storeContext.GetCurrentStoreAsync();
            var pictureSize = _mediaSettings.CategoryThumbPictureSize;

            //subcategories
            model.SubCategories = await (await _categoryService.GetAllCategoriesByParentCategoryIdAsync(category.Id))
                .SelectAwait(async curCategory =>
                {
                    var subCatModel = new CategoryModel.SubCategoryModel
                    {
                        Id = curCategory.Id,
                        Name = await _localizationService.GetLocalizedAsync(curCategory, y => y.Name),
                        SeName = await _urlRecordService.GetSeNameAsync(curCategory),
                        Description = await _localizationService.GetLocalizedAsync(curCategory, y => y.Description)
                    };

                    //prepare picture model
                    var categoryPictureCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.CategoryPictureModelKey, curCategory,
                        pictureSize, true, await _workContext.GetWorkingLanguageAsync(), _webHelper.IsCurrentConnectionSecured(),
                        currentStore);

                    subCatModel.PictureModel = await _staticCacheManager.GetAsync(categoryPictureCacheKey, async () =>
                    {
                        var picture = await _pictureService.GetPictureByIdAsync(curCategory.PictureId);
                        string fullSizeImageUrl, imageUrl;

                        (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                        (imageUrl, _) = await _pictureService.GetPictureUrlAsync(picture, pictureSize);

                        var pictureModel = new PictureModel
                        {
                            FullSizeImageUrl = fullSizeImageUrl,
                            ImageUrl = imageUrl,
                            Title = string.Format(await _localizationService
                                .GetResourceAsync("Media.Category.ImageLinkTitleFormat"), subCatModel.Name),
                            AlternateText = string.Format(await _localizationService
                                .GetResourceAsync("Media.Category.ImageAlternateTextFormat"), subCatModel.Name)
                        };

                        return pictureModel;
                    });

                    return subCatModel;
                }).ToListAsync();

            //featured products
            if (!_catalogSettings.IgnoreFeaturedProducts)
            {
                var featuredProducts = await _productService.GetCategoryFeaturedProductsAsync(category.Id, currentStore.Id);
                if (featuredProducts != null)
                    model.FeaturedProducts = (await _customProductModelFactory.PrepareCustomProductOverviewModelsAsync(featuredProducts)).ToList();
            }

            return model;
        }

        public virtual async Task<CustomCatalogProductsModel> PrepareCustomCategoryProductsModelAsync(Category category, CustomCatalogProductsCommand command,
            string queryString = "", int categoryId = 0, bool forSections = false, int pictureSize = 0)
        {
            command.OrderBy = await this.PrepareSortByToEnum(command.SortBy);
            if (forSections)
            {
                var model = new CustomCatalogProductsModel();
                var products = await _customProductService.CustomSearchProductsAsync(
                 command.PageNumber - 1,
                 command.PageSize,
                 categoryIds: categoryId == 0 ? null : new List<int>() { categoryId },
                 storeId: (await _storeContext.GetCurrentStoreAsync()).Id,
                 visibleIndividuallyOnly: true,
                 excludeFeaturedProducts: !_catalogSettings.IgnoreFeaturedProducts && !_catalogSettings.IncludeFeaturedProductsInNormalLists,
                  orderBy: (ProductSortingEnum)command.OrderBy);
                await PrepareCustomCatalogProductsAsync(model, category, products, false);
                return model;
            }
            else
            {
                if (category == null)
                    throw new ArgumentNullException(nameof(category));

                if (command == null)
                    throw new ArgumentNullException(nameof(command));

                var model = new CustomCatalogProductsModel
                {
                    UseAjaxLoading = _catalogSettings.UseAjaxCatalogProductsLoading
                };

                var currentStore = await _storeContext.GetCurrentStoreAsync();

                //sorting
                await PrepareSortingOptionsAsync(model, command);
                //view mode
                await PrepareViewModesAsync(model, command);
                //page size
                await PreparePageSizeOptionsAsync(model, command, category.AllowCustomersToSelectPageSize,
                    category.PageSizeOptions, category.PageSize);

                var categoryIds = new List<int> { category.Id };

                //include subcategories
                if (_catalogSettings.ShowProductsFromSubcategories)
                    categoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(category.Id, currentStore.Id));

                //price range
                PriceRangeModel selectedPriceRange = null;
                if (_catalogSettings.EnablePriceRangeFiltering && category.PriceRangeFiltering)
                {
                    selectedPriceRange = await GetConvertedPriceRangeAsync(command);

                    PriceRangeModel availablePriceRange = null;
                    if (!category.ManuallyPriceRange)
                    {
                        async Task<decimal?> getProductPriceAsync(ProductSortingEnum orderBy)
                        {
                            var products = await _productService.SearchProductsAsync(0, 1,
                                categoryIds: categoryIds,
                                storeId: currentStore.Id,
                                visibleIndividuallyOnly: true,
                                excludeFeaturedProducts: !_catalogSettings.IgnoreFeaturedProducts && !_catalogSettings.IncludeFeaturedProductsInNormalLists,
                                orderBy: orderBy);

                            return products?.FirstOrDefault()?.Price ?? 0;
                        }

                        availablePriceRange = new PriceRangeModel
                        {
                            From = await getProductPriceAsync(ProductSortingEnum.PriceAsc),
                            To = await getProductPriceAsync(ProductSortingEnum.PriceDesc)
                        };
                    }
                    else
                    {
                        availablePriceRange = new PriceRangeModel
                        {
                            From = category.PriceFrom,
                            To = category.PriceTo
                        };
                    }

                    model.PriceRangeFilter = await PreparePriceRangeFilterAsync(selectedPriceRange, availablePriceRange);
                }

                //filterable options
                var filterableOptions = await _specificationAttributeService
                    .GetFiltrableSpecificationAttributeOptionsByCategoryIdAsync(category.Id);

                model.SpecificationFilter = await CustomPrepareSpecificationFilterModel(command.Specs, filterableOptions);

                //filterable manufacturers
                if (_catalogSettings.EnableManufacturerFiltering)
                {
                    var manufacturers = await _manufacturerService.GetManufacturersByCategoryIdAsync(category.Id);

                    model.ManufacturerFilter = await PrepareManufacturerFilterModel(command.Ms, manufacturers);
                }

                var filteredSpecs = command.Specs is null ? null : filterableOptions.Where(fo => command.Specs.Contains(fo.Id)).ToList();


                #region Products

                // get All specification Attributes
                var specificationAttrs = await _customSpecificationAttributeService.GetAllSpecificationAttributesAsync();

                // get All specification Options
                var specificationAttrOptions = await _customSpecificationAttributeService.GetAllSpecificationOptionsAsync();

                // Get Selected Filters
                var filters = await this.PrepareSelectedFiltersAttributes(queryString, specificationAttrs, specificationAttrOptions);
                string featuredIdQuery = _httpContextAccessor.HttpContext.Request.Query["featuredid"];
                int featuredId = 0;
                if (featuredIdQuery != null)
                {
                    if (featuredIdQuery.IndexOf("-") > 0)
                        featuredIdQuery = featuredIdQuery.Split('-')[0];
                    int.TryParse(featuredIdQuery, out featuredId);
                }
                // Get All Products Ids with specification Attributes

                var productSpecificationAttribute = await _customProductService.CustomSearchGetProductSpecificationAttributeAsync(
                   command.ViewAll == 1 ? 0 : command.PageNumber - 1,
                     command.ViewAll == 1 ? int.MaxValue : command.PageSize,
                      categoryIds: categoryIds,
                      storeId: currentStore.Id,
                      visibleIndividuallyOnly: true,
                      excludeFeaturedProducts: !_catalogSettings.IgnoreFeaturedProducts && !_catalogSettings.IncludeFeaturedProductsInNormalLists,
                      priceMin: selectedPriceRange?.From,
                      priceMax: selectedPriceRange?.To,
                      manufacturerIds: command.Ms,
                      featuredId: featuredId, isMobileDevice: _customWorkContext.IsMobileDevice()
                     );


                // end


                await PrepareCustomFiltersAndProducts(command,
                    category, model, selectedPriceRange, specificationAttrs, specificationAttrOptions, filters, productSpecificationAttribute, pictureSize, featuredId);




                #endregion



                //products
                //var products = await _productService.CustomSearchProductsAsync(
                // command.ViewAll == 1 ? 0 : command.PageNumber - 1,
                //   command.ViewAll == 1 ? int.MaxValue : command.PageSize,
                //    categoryIds: categoryIds,
                //    storeId: currentStore.Id,
                //    visibleIndividuallyOnly: true,
                //    excludeFeaturedProducts: !_catalogSettings.IgnoreFeaturedProducts && !_catalogSettings.IncludeFeaturedProductsInNormalLists,
                //    priceMin: selectedPriceRange?.From,
                //    priceMax: selectedPriceRange?.To,
                //    manufacturerIds: command.ManufacturerIds,
                //    filteredSpecOptions: filteredSpecs,
                //    orderBy: (ProductSortingEnum)command.OrderBy);

                //var isFiltering = filterableOptions.Any() || selectedPriceRange?.From is not null;
                //await PrepareCustomCatalogProductsAsync(model, products, isFiltering);

                //model.defaultPageSize = category.PageSize;

                return model;



            }
        }

        protected virtual async Task PrepareCustomCatalogProductsAsync(CustomCatalogProductsModel model, Category category, IPagedList<Product> products, bool isFiltering = false, int pictureSize = 0)
        {
            if (!string.IsNullOrEmpty(model.WarningMessage))
                return;

            if (products.Count == 0 && isFiltering)
                model.NoResultMessage = await _localizationService.GetResourceAsync("Catalog.Products.NoResult");
            else
            {

                var prepareSizeShadeAggregation = await _settingService.GetSettingByKeyAsync<bool>("CategoryPage.Display.Attributes.Aggregation");
                var prepareShades = await _settingService.GetSettingByKeyAsync<bool>("CategoryPage.Display.Shades");
                model.Products = (await _customProductModelFactory.PrepareCustomProductOverviewModelsAsync(products: products, prepareShades: prepareShades, prepareAlternatePictureModel: category.EnableHoverImage, productThumbPictureSize: pictureSize == 0 ? null : pictureSize,
                    isCategorypage: true, prepareSizeShadeAggregation: prepareSizeShadeAggregation)).ToList();
                model.LoadPagedList(products);
            }
        }

        public virtual async Task<CustomSearchModel> PrepareCustomSearchModelAsync(CustomSearchModel model, CustomCatalogProductsCommand command, string queryString)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (command == null)
                throw new ArgumentNullException(nameof(command));
            model.CatalogProductsModel = await PrepareCustomSearchProductsModelAsync(model, command, queryString);

            return model;
        }

        public virtual async Task<(string templateViewPath, string listingViewPath, string filterViewPath, string FilterViewPathForMobile, int pictureSize, bool isHorizontal)> CustomPrepareCategoryTemplateViewPathAsync(int templateId)
        {
            var template = await _categoryTemplateService.GetCategoryTemplateByIdAsync(templateId) ??
                           (await _categoryTemplateService.GetAllCategoryTemplatesAsync()).FirstOrDefault();

            if (template == null)
                throw new Exception("No default template could be loaded");

            return (template.ViewPath, template.GridLineViewPath, template.FilterViewPath, template.FilterViewPathForMobile, template.PictureSize <= 0 ? _mediaSettings.CategoryThumbPictureSize : template.PictureSize, template.IsHorizontal);
        }
        public virtual async Task<CategoryTemplate> GetCategoryTemplate(int templateId)
        {
            var template = await _categoryTemplateService.GetCategoryTemplateByIdAsync(templateId) ??
                      (await _categoryTemplateService.GetAllCategoryTemplatesAsync()).FirstOrDefault();

            if (template == null)
                throw new Exception("No default template could be loaded");

            return template;
        }
        public virtual async Task<CustomCatalogProductsModel> PrepareCustomSearchProductsModelAsync(CustomSearchModel searchModel, CustomCatalogProductsCommand command, string queryString)
        {
            command.OrderBy = await this.PrepareSortByToEnum(command.SortBy);
            var model = new CustomCatalogProductsModel
            {
                UseAjaxLoading = _catalogSettings.UseAjaxCatalogProductsLoading
            };


            var workingLanguage = await _workContext.GetWorkingLanguageAsync();
            //sorting
            await PrepareSortingOptionsAsync(model, command);

            var removeSortOption = await _localizationService.GetResourceAsync("enums.nop.core.domain.catalog.productsortingenum.position");
            var availableSortOptions = model.AvailableSortOptions.Where(s => s.Text != removeSortOption).ToList();

            availableSortOptions.Insert(0, new SelectListItem()
            {
                Value = "-1",
                Text = await _localizationService.GetResourceAsync("Common.Select"),
                Selected = availableSortOptions.Where(s => s.Selected).Any() ? false : true
            });

            model.AvailableSortOptions = availableSortOptions;
            //view mode
            await PrepareViewModesAsync(model, command);

            await PreparePageSizeOptionsAsync(model, command, _catalogSettings.SearchPageAllowCustomersToSelectPageSize,
          _catalogSettings.SearchPagePageSizeOptions, _catalogSettings.SearchPageProductsPerPage);


            var searchTerms = searchModel.SearchTerm == null
                 ? string.Empty
                 : searchModel.SearchTerm.Trim();
            var isSearchTermSpecified = _httpContextAccessor.HttpContext.Request.Query.ContainsKey("searchterm");
            if (isSearchTermSpecified)
            {
                var currentStore = await _storeContext.GetCurrentStoreAsync();

                if (searchTerms.Length < _catalogSettings.ProductSearchTermMinimumLength)
                {
                    model.WarningMessage =
                        string.Format(await _localizationService.GetResourceAsync("Search.SearchTermMinimumLengthIsNCharacters"),
                            _catalogSettings.ProductSearchTermMinimumLength);
                }
                else
                {
                    var specificationAttrs = await _customSpecificationAttributeService.GetAllSpecificationAttributesAsync();
                    var categories = await _categoryService.GetAllCategoriesAsync(_storeContext.GetCurrentStore().Id, false);
                    // get All specification Options
                    var specificationAttrOptions = await _customSpecificationAttributeService.GetAllSpecificationOptionsAsync();

                    var filters = await this.PrepareSelectedFiltersAttributes(queryString, specificationAttrs, specificationAttrOptions);

                    command.OrderBy = await this.PrepareSortByToEnum(command.SortBy);
                    Dictionary<int, string> _filters = new Dictionary<int, string>();
                    foreach (var filter in filters)
                    {
                        string specificationSelectedOptions = "";
                        foreach (var option in filter.Value)
                        {
                            specificationSelectedOptions += "sec-" + option + "^";
                        }
                        _filters.Add(filter.Key, specificationSelectedOptions.Substring(0, specificationSelectedOptions.Length - 1));
                    }
                    int catId = await this.PrepareCatagoryAttributes(queryString, categories);


                    searchTerms = searchTerms == null ? "" : searchTerms.Replace("\"", "");

                    string json = await _elasticSearchHelpService.GetListing(searchTerms, command.PageNumber, command.PageSize, catId,
                          ((ProductSortingEnum)command.OrderBy).ToString(), _filters);
                    (List<CustomProductOverviewModel> products, CustomSpecificationFilterModel specificationModel, int Total) =
                        await this.DesearlizeElasticSearchResponse(json, specificationAttrs, specificationAttrOptions,
                        categories, _filters, catId);


                    model.Products = products;
                    model.SpecificationFilter = specificationModel;



                    model.LoadPagedList(new PagedList<CustomProductOverviewModel>(products, command.PageNumber - 1, command.PageSize, Total));
                    if (!string.IsNullOrEmpty(searchTerms))
                    {
                        var searchTerm =
                            await _searchTermService.GetSearchTermByKeywordAsync(searchTerms, currentStore.Id);
                        if (searchTerm != null)
                        {
                            searchTerm.Count++;
                            await _searchTermService.UpdateSearchTermAsync(searchTerm);
                        }
                        else
                        {
                            searchTerm = new SearchTerm
                            {
                                Keyword = searchTerms,
                                StoreId = currentStore.Id,
                                Count = 1
                            };
                            await _searchTermService.InsertSearchTermAsync(searchTerm);
                        }
                    }

                    //event
                    await _eventPublisher.PublishAsync(new ProductSearchEvent
                    {
                        SearchTerm = searchTerms,
                        SearchInDescriptions = true,
                        CategoryIds = new List<int>(),
                        ManufacturerId = 0,
                        WorkingLanguageId = workingLanguage.Id,
                        VendorId = 0
                    });
                }
            }
            return model;
        }

        #region Utilities

        protected async Task<CustomSpecificationFilterModel> CustomPrepareSpecificationFilterModel(IList<int> selectedOptions, IList<SpecificationAttributeOption> availableOptions)
        {
            var model = new CustomSpecificationFilterModel();

            if (availableOptions?.Any() == true)
            {
                model.Enabled = true;

                var workingLanguage = await _workContext.GetWorkingLanguageAsync();

                foreach (var option in availableOptions)
                {
                    var attributeFilter = model.Attributes.FirstOrDefault(model => model.Id == option.SpecificationAttributeId);
                    if (attributeFilter == null)
                    {
                        var attribute = await _specificationAttributeService
                            .GetSpecificationAttributeByIdAsync(option.SpecificationAttributeId);
                        attributeFilter = new CustomSpecificationAttributeFilterModel
                        {
                            Id = attribute.Id,
                            Name = await _localizationService
                                .GetLocalizedAsync(attribute, x => x.Name, workingLanguage.Id)
                        };
                        model.Attributes.Add(attributeFilter);
                    }

                    attributeFilter.Values.Add(new CustomSpecificationAttributeValueFilterModel
                    {
                        Id = option.Id,
                        Name = await _localizationService
                            .GetLocalizedAsync(option, x => x.Name, workingLanguage.Id),
                        Selected = selectedOptions?.Any(optionId => optionId == option.Id) == true,
                        ColorSquaresRgb = option.ColorSquaresRgb
                    });
                }
            }

            return model;
        }

        #region Elastic search functions
        private async Task BindingFilters(string levelName, string type, int filterID, int prdCount, Dictionary<int, string> filters, int catID,
            List<SpecificationAttribute> specificationAttributes, List<SpecificationAttributeOption> specificationAttributeOptions,
             CustomSpecificationFilterModel specificationFilterModel, IList<Category> categories,
             Language workingLanguage)// Optimize all sections at once
        {
            try
            {
                int levelID = 0;
                if (type == "Category")
                {
                    var category = categories.AsEnumerable().Where(c => c.Id == filterID && c.Published == true).FirstOrDefault();// Optimize
                    if (category != null)
                    {
                        var catAttribute = specificationFilterModel.Attributes.Where(a => a.Name == "Category").FirstOrDefault();
                        if (catAttribute == null)
                        {
                            var attributeFilter = new CustomSpecificationAttributeFilterModel
                            {
                                Id = -1,
                                Name = "Category",
                                DisplayOrder = -1000,
                                Sename = "categoryid"
                            };
                            specificationFilterModel.Attributes.Add(attributeFilter);
                            catAttribute = attributeFilter;
                        }
                        if (catAttribute != null)
                            catAttribute.Values.Add(new CustomSpecificationAttributeValueFilterModel
                            {
                                Id = category.Id,
                                Name = await _localizationService
                               .GetLocalizedAsync(category, x => x.Name, workingLanguage.Id),
                                Selected = category.Id == catID ? true : false,
                                Count = prdCount,
                                DisplayOrder = category.DisplayOrder + 1000,
                                Sename = GenrateFilterSename(await _localizationService
                               .GetLocalizedAsync(category, x => x.Name, workingLanguage.Id))
                            });

                    }
                }
                else
                {
                    int.TryParse(levelName.Split('-')[1], out levelID);
                    var _optionObj = specificationAttributeOptions.Where(o => o.Id == filterID).FirstOrDefault();// Optimize
                    if (_optionObj != null)
                    {
                        try
                        {
                            if (filters.Count() == 0 /*&& row["removeFromCategory"].ToString().Split(',').Where(m => m == catID.ToString()).Count() == 0*/)
                            {
                                var filter = specificationFilterModel.Attributes.Where(a => a.Id == _optionObj.SpecificationAttributeId).FirstOrDefault();
                                if (filter == null)
                                {
                                    var attribute = specificationAttributes.Where(a => a.Id == _optionObj.SpecificationAttributeId).FirstOrDefault();
                                    if (attribute != null)
                                    {
                                        var attributeFilter = new CustomSpecificationAttributeFilterModel
                                        {
                                            Id = attribute.Id,
                                            Name = await _localizationService
                                               .GetLocalizedAsync(attribute, x => x.Name, workingLanguage.Id),
                                            DisplayOrder = attribute.DisplayOrder + 1000,
                                            Sename = GenrateFilterSename(await _localizationService
                                           .GetLocalizedAsync(_optionObj, x => x.Name, workingLanguage.Id))
                                        };
                                        filter = attributeFilter;
                                        specificationFilterModel.Attributes.Add(filter);
                                    }
                                }
                                if (filter != null)
                                    filter.Values.Add(new CustomSpecificationAttributeValueFilterModel
                                    {
                                        Id = _optionObj.Id,
                                        Name = await _localizationService
                                       .GetLocalizedAsync(_optionObj, x => x.Name, workingLanguage.Id),
                                        Selected = false,
                                        ColorSquaresRgb = _optionObj.ColorSquaresRgb,
                                        Count = prdCount,
                                        DisplayOrder = _optionObj.DisplayOrder + 1000,
                                        Sename = GenrateFilterSename(await _localizationService
                                       .GetLocalizedAsync(_optionObj, x => x.Name, workingLanguage.Id))
                                    });


                            }
                            else if (filters.Count() <= levelID && filters.Where(m => m.Key == _optionObj.SpecificationAttributeId).Count() == 0
                                /*&& row["removeFromCategory"].ToString().Split(',').Where(m => m == catID.ToString()).Count() == 0*/)
                            {
                                var filter = specificationFilterModel.Attributes.Where(a => a.Id == _optionObj.SpecificationAttributeId).FirstOrDefault();
                                if (filter == null)
                                {
                                    var attribute = specificationAttributes.Where(a => a.Id == _optionObj.SpecificationAttributeId).FirstOrDefault();
                                    if (attribute != null)
                                    {
                                        var attributeFilter = new CustomSpecificationAttributeFilterModel
                                        {
                                            Id = attribute.Id,
                                            Name = await _localizationService
                                               .GetLocalizedAsync(attribute, x => x.Name, workingLanguage.Id),
                                            DisplayOrder = attribute.DisplayOrder + 1000,
                                            Sename = GenrateFilterSename(await _localizationService
                                           .GetLocalizedAsync(_optionObj, x => x.Name, workingLanguage.Id)),
                                        };
                                        filter = attributeFilter;
                                        specificationFilterModel.Attributes.Add(filter);
                                    }
                                }
                                if (filter != null)
                                    filter.Values.Add(new CustomSpecificationAttributeValueFilterModel
                                    {
                                        Id = _optionObj.Id,
                                        Name = await _localizationService
                                   .GetLocalizedAsync(_optionObj, x => x.Name, workingLanguage.Id),
                                        Selected = levelID + 1 > filters.Count ? false : filters.Take(levelID + 1).Skip(levelID).FirstOrDefault().Value.Split('^').
                                        Where(m => m.Replace("sec-", "") == filterID.ToString()).Count() > 0 ? true : false
                                      ,
                                        ColorSquaresRgb = _optionObj.ColorSquaresRgb,
                                        Count = prdCount,
                                        DisplayOrder = _optionObj.DisplayOrder + 1000,
                                        Sename = GenrateFilterSename(await _localizationService
                                   .GetLocalizedAsync(_optionObj, x => x.Name, workingLanguage.Id))
                                    });

                            }
                            else if (_optionObj.SpecificationAttributeId == filters.Take(levelID + 1).Skip(levelID).FirstOrDefault().Key && filters.Count() > levelID
                                /*&& row["removeFromCategory"].ToString().Split(',').Where(m => m == catID.ToString()).Count() == 0*/)
                            {
                                var filter = specificationFilterModel.Attributes.Where(a => a.Id == _optionObj.SpecificationAttributeId).FirstOrDefault();
                                if (filter == null)
                                {
                                    var attribute = specificationAttributes.Where(a => a.Id == _optionObj.SpecificationAttributeId).FirstOrDefault();
                                    if (attribute != null)
                                    {
                                        var attributeFilter = new CustomSpecificationAttributeFilterModel
                                        {
                                            Id = attribute.Id,
                                            Name = await _localizationService
                                               .GetLocalizedAsync(attribute, x => x.Name, workingLanguage.Id),
                                            DisplayOrder = attribute.DisplayOrder + 1000,
                                            Sename = GenrateFilterSename(await _localizationService
                                           .GetLocalizedAsync(_optionObj, x => x.Name, workingLanguage.Id))
                                        };
                                        filter = attributeFilter;
                                        specificationFilterModel.Attributes.Add(filter);
                                    }
                                }
                                if (filter != null)
                                    filter.Values.Add(new CustomSpecificationAttributeValueFilterModel
                                    {
                                        Id = _optionObj.Id,
                                        Name = await _localizationService
                                .GetLocalizedAsync(_optionObj, x => x.Name, workingLanguage.Id),
                                        Selected = filters.Take(levelID + 1).Skip(levelID).FirstOrDefault().Value.Split('^').Where(m => m.Replace("sec-", "") == filterID.ToString()).Count() > 0 ? true : false,

                                        ColorSquaresRgb = _optionObj.ColorSquaresRgb,
                                        Count = prdCount,
                                        DisplayOrder = _optionObj.DisplayOrder + 1000,
                                        Sename = GenrateFilterSename(await _localizationService
                                .GetLocalizedAsync(_optionObj, x => x.Name, workingLanguage.Id))
                                    });
                            }
                        }
                        catch { }
                    }
                    //  }
                }
            }

            catch (Exception ex)
            {

                await _logger.InsertLogAsync(LogLevel.Error, "Elastic Search - Search Page - Function BindingFilters", ex.Message);
            }
        }

        public async Task<(List<CustomProductOverviewModel>, CustomSpecificationFilterModel, int total)> DesearlizeElasticSearchResponse(string json, List<SpecificationAttribute> specificationAttrs,
            List<SpecificationAttributeOption> specificationAttrOptions, IList<Category> categories, Dictionary<int, string> filters, int catID)
        {
            var data = JsonConvert.DeserializeObject<dynamic>(json);
            int total = 0;

            string levelName;
            CustomProductOverviewModel response = new CustomProductOverviewModel();
            List<CustomProductOverviewModel> products = new List<CustomProductOverviewModel>();
            var workingLanguage = await _workContext.GetWorkingLanguageAsync();
            var specificationFilterModel = new CustomSpecificationFilterModel();
            specificationFilterModel.Enabled = true;
   

            string cataLogName = await _settingService.GetSettingByKeyAsync<string>("Es_ProductCatalogIndexName");

            try
            {
                foreach (var obj in data)
                {
                    if (obj.Name == "responses")
                    {
                        foreach (var query in obj)
                        {
                            foreach (var subquery in query)
                            {
                                foreach (var content in subquery)
                                {
                                    if (content.Name == "hits")
                                    {
                                        foreach (var subcontent in content)
                                        {
                                            foreach (var index in subcontent)
                                            {
                                                if (index.Name == "total")
                                                {
                                                    foreach (var node in index)
                                                    {
                                                        foreach (var innerNode in node)
                                                        {
                                                            if (innerNode.Name == "value")
                                                                total = (int)innerNode;
                                                        }
                                                    }
                                                }
                                                else if (index.Name == "hits")
                                                {
                                                    foreach (var subindex in index)
                                                    {
                                                        foreach (var item in subindex)
                                                        {
                                                            if (item._index == cataLogName.Replace("/_doc", ""))
                                                            {
                                                                try
                                                                {
                                                                    var _product = await _productService.GetProductByIdAsync((int)item._source.Id);
                                                                    CustomProductOverviewModel product = new CustomProductOverviewModel()
                                                                    {
                                                                        Id = item._source.Id,
                                                                        Name = item._source.Name,
                                                                        FullDescription = item._source.FullDescription,
                                                                        SeName = item._source.SeName,
                                                                        Sku = item._source.Sku,
                                                                        Inventory = item._source.Inventory,
                                                                        EnableCustomizationModule = item._source.EnableCustomizationModule == null ? false : item._source.EnableCustomizationModule,
                                                                        MetaKeywords = item._source.MetaKeywords,
                                                                        CollectionMessage = item._source.CollectionMessage == null ? "" : item._source.CollectionMessage,
                                                                        ProductRelation = item._source.ProductRelation == null ? "" : item._source.ProductRelation,


                                                                    };

                                                                    //Price binding
                                                                    if (item._source.ProductPrice != null)
                                                                    {
                                                                        CustomProductOverviewModel.ProductPriceModel priceModel = new CustomProductOverviewModel.ProductPriceModel();
                                                                        priceModel.BasePricePAngV = item._source.ProductPrice.BasePricePAngV == null ? "" : item._source.ProductPrice.BasePricePAngV;
                                                                        priceModel.PriceValue = item._source.ProductPrice.PriceValue == null ? 0 : item._source.ProductPrice.PriceValue;
                                                                        priceModel.CallForPrice = item._source.ProductPrice.CallForPrice == null ? false : item._source.ProductPrice.CallForPrice;
                                                                        priceModel.OldPrice = item._source.ProductPrice.OldPrice;
                                                                        priceModel.RentalPrice = item._source.ProductPrice.RentalPrice == null ? "" : item._source.ProductPrice.RentalPrice;
                                                                        priceModel.Price = item._source.ProductPrice.Price;
                                                                        priceModel.DisableBuyButton = item._source.ProductPrice.DisableBuyButton == null ? false : item._source.ProductPrice.DisableBuyButton;
                                                                        priceModel.DisableWishlistButton = item._source.ProductPrice.DisableWishlistButton == null ? false : item._source.ProductPrice.DisableWishlistButton;
                                                                        priceModel.AvailableForPreOrder = item._source.ProductPrice.AvailableForPreOrder == null ? false : item._source.ProductPrice.AvailableForPreOrder;
                                                                        priceModel.DisableAddToCompareListButton = item._source.ProductPrice.DisableAddToCompareListButton == null ? false : item._source.ProductPrice.DisableAddToCompareListButton;
                                                                        // remove   priceModel.PreOrderAvailabilityStartDateTimeUtc = item._source.ProductPrice.PreOrderAvailabilityStartDateTimeUtc == null ? "" : item._source.ProductPrice.PreOrderAvailabilityStartDateTimeUtc;
                                                                        priceModel.IsRental = item._source.ProductPrice.IsRental == null ? false : item._source.ProductPrice.IsRental;
                                                                        priceModel.ForceRedirectionAfterAddingToCart = item._source.ProductPrice.ForceRedirectionAfterAddingToCart == null ? false : item._source.ProductPrice.ForceRedirectionAfterAddingToCart;
                                                                        priceModel.DisplayTaxShippingInfo = item._source.ProductPrice.DisplayTaxShippingInfo == null ? false : item._source.ProductPrice.DisplayTaxShippingInfo;
                                                                        priceModel.MembershipPrice = item._source.ProductPrice.MembershipPrice;
                                                                        priceModel.MembershipPriceValue = item._source.ProductPrice.MembershipPriceValue == null ? 0 : item._source.ProductPrice.MembershipPriceValue;
                                                                        priceModel.Msrp = item._source.ProductPrice.Msrp;
                                                                        priceModel.MsrpValue = item._source.ProductPrice.MsrpValue == null ? 0 : item._source.ProductPrice.MsrpValue;
                                                                        priceModel.OldPriceValue = item._source.ProductPrice.OldPriceValue == null ? 0 : item._source.ProductPrice.OldPriceValue;
                                                                        priceModel.CurrencyCode = item._source.ProductPrice.CurrencyCode == null ? "" : item._source.ProductPrice.CurrencyCode;
                                                                        priceModel.PriceWithDiscount = item._source.ProductPrice.PriceWithDiscount == null ? "" : item._source.ProductPrice.PriceWithDiscount;
                                                                        priceModel.CustomerEntersPrice = item._source.ProductPrice.CustomerEntersPrice == null ? false : item._source.ProductPrice.CustomerEntersPrice;
                                                                        priceModel.ProductId = item._source.ProductPrice.ProductId == null ? 0 : item._source.ProductPrice.ProductId;
                                                                        priceModel.HidePrices = item._source.ProductPrice.HidePrices == null ? false : item._source.ProductPrice.HidePrices;
                                                                        priceModel.IsVariantProduct = item._source.ProductPrice.IsVariantProduct == null ? false :
                                                                                                                                                   Convert.ToBoolean(item._source.ProductPrice.IsVariantProduct);
                                                                        priceModel.MinOldPriceValue = item._source.ProductPrice.MinOldPriceValue == null ? 0 :
                                                                                                                                                   Convert.ToDecimal(item._source.ProductPrice.MinOldPriceValue);
                                                                        priceModel.MaxOldPriceValue = item._source.ProductPrice.MaxOldPriceValue == null ? 0 :
                                                                                                                                                   Convert.ToDecimal(item._source.ProductPrice.MaxOldPriceValue);
                                                                        priceModel.MinPriceValue = item._source.ProductPrice.MinPriceValue == null ? 0 :
                                                                                                                                                   Convert.ToDecimal(item._source.ProductPrice.MinPriceValue);
                                                                        priceModel.MaxPriceValue = item._source.ProductPrice.MaxPriceValue == null ? 0 :
                                                                                                                                                   Convert.ToDecimal(item._source.ProductPrice.MaxPriceValue);
                                                                        priceModel.MinMsrpValue = item._source.ProductPrice.MinMsrpValue == null ? 0 :
                                                                                                                                                   Convert.ToDecimal(item._source.ProductPrice.MinMsrpValue);
                                                                        priceModel.MaxMsrpValue = item._source.ProductPrice.MaxMsrpValue == null ? 0 :
                                                                                                                                                   Convert.ToDecimal(item._source.ProductPrice.MaxMsrpValue);
                                                                        priceModel.MinMembershipPriceValue = item._source.ProductPrice.MinMembershipPriceValue == null ? 0 :
                                                                                                                                                   Convert.ToDecimal(item._source.ProductPrice.MinMembershipPriceValue);
                                                                        priceModel.MaxMembershipPriceValue = item._source.ProductPrice.MaxMembershipPriceValue == null ? 0 :
                                                                                                                                                   Convert.ToDecimal(item._source.ProductPrice.MaxMembershipPriceValue);

                                                                        priceModel.MinOldPrice = item._source.ProductPrice.MinOldPrice == null ? "" :
                                                                                                                                                (string)item._source.ProductPrice.MinOldPrice;
                                                                        priceModel.MaxOldPrice = item._source.ProductPrice.MaxOldPrice == null ? "" :
                                                                                                                                              (string)item._source.ProductPrice.MaxOldPrice;
                                                                        priceModel.MinPrice = item._source.ProductPrice.MinPrice == null ? "" :
                                                                                                                                              (string)item._source.ProductPrice.MinPrice;
                                                                        priceModel.MaxPrice = item._source.ProductPrice.MaxPrice == null ? "" :
                                                                                                                                              (string)item._source.ProductPrice.MaxPrice;
                                                                        priceModel.MinMsrp = item._source.ProductPrice.MinMsrp == null ? "" :
                                                                                                                                              (string)item._source.ProductPrice.MinMsrp;
                                                                        priceModel.MaxMsrp = item._source.ProductPrice.MaxMsrp == null ? "" :
                                                                                                                                              (string)item._source.ProductPrice.MaxMsrp;
                                                                        priceModel.MinMembershipPrice = item._source.ProductPrice.MinMembershipPrice == null ? "" :
                                                                                                                                              (string)item._source.ProductPrice.MinMembershipPrice;
                                                                        priceModel.MaxMembershipPrice = item._source.ProductPrice.MaxMembershipPrice == null ? "" :
                                                                                                                                              (string)item._source.ProductPrice.MaxMembershipPrice;


                                                                        #region Price Offer Module

                                                                        (priceModel.OfferText, priceModel.OfferPlaceHolder, priceModel.DiscountAmount, priceModel.DiscountPercentage, priceModel.SaleStartDate, priceModel.SaleEndDate)
                                                                            = await _customProductService.GetProductSaleOfferInfo(_product, priceModel.OldPriceValue, priceModel.PriceValue);



                                                                        #endregion
                                                                        priceModel.Price = CustomCommonHelper.FormatCurrencyPriceWithoutDecimal(priceModel.Price);
                                                                        priceModel.PriceValue = CustomCommonHelper.FormatPriceWithoutDecimal(priceModel.PriceValue);
                                                                        priceModel.OldPrice = CustomCommonHelper.FormatCurrencyPriceWithoutDecimal(priceModel.OldPrice);
                                                                        priceModel.OldPriceValue = CustomCommonHelper.FormatPriceWithoutDecimal(priceModel.OldPriceValue);
                                                                        priceModel.MinPrice = CustomCommonHelper.FormatCurrencyPriceWithoutDecimal(priceModel.MinPrice);
                                                                        priceModel.MinPriceValue = CustomCommonHelper.FormatPriceWithoutDecimal(priceModel.MinPriceValue);
                                                                        priceModel.MinOldPrice = CustomCommonHelper.FormatCurrencyPriceWithoutDecimal(priceModel.MinOldPrice);
                                                                        priceModel.MinOldPriceValue = CustomCommonHelper.FormatPriceWithoutDecimal(priceModel.MinOldPriceValue);
                                                                        priceModel.MaxPrice = CustomCommonHelper.FormatCurrencyPriceWithoutDecimal(priceModel.MaxPrice);
                                                                        priceModel.MaxPriceValue = CustomCommonHelper.FormatPriceWithoutDecimal(priceModel.MaxPriceValue);
                                                                        priceModel.MaxOldPrice = CustomCommonHelper.FormatCurrencyPriceWithoutDecimal(priceModel.MaxOldPrice);
                                                                        priceModel.MaxOldPriceValue = CustomCommonHelper.FormatPriceWithoutDecimal(priceModel.MaxOldPriceValue);
                                                                        product.ProductPrice = priceModel;

                                                                    }

                                                                    // end

                                                                    /*BindingVariantInfo(item._source.variants, ref listing);
                                                                    var cartItem = cart.CartItems.Where(m => m.VariantID == listing.variantID).FirstOrDefault();
                                                                    listing.ShoppingcartRecid = cartItem == null ? 0 : cartItem.ShoppingCartRecordID;
                                                                    listing.IsWish = cartItem == null ? false : true;
                                                                    */


                                                                    // DefaultPictureModel

                                                                    if (item._source.DefaultPictureModel != null)
                                                                    {
                                                                        CustomPictureModel pictureModel = new CustomPictureModel();
                                                                        pictureModel.ImageUrl = item._source.DefaultPictureModel.ImageUrl;
                                                                        pictureModel.ThumbImageUrl = item._source.DefaultPictureModel.ThumbImageUrl == null ? "" : item._source.DefaultPictureModel.ThumbImageUrl;
                                                                        pictureModel.FullSizeImageUrl = item._source.DefaultPictureModel.FullSizeImageUrl;
                                                                        pictureModel.Title = item._source.DefaultPictureModel.Title;
                                                                        pictureModel.AlternateText = item._source.DefaultPictureModel.AlternateText;
                                                                        product.DefaultPictureModel = pictureModel;

                                                                    }

                                                                    if (item._source.AlternatePictureModel != null)
                                                                    {

                                                                        PictureModel pictureModel = new PictureModel();
                                                                        pictureModel.ImageUrl = item._source.AlternatePictureModel.ImageUrl;
                                                                        pictureModel.ThumbImageUrl = item._source.AlternatePictureModel.ThumbImageUrl == null ? "" : item._source.AlternatePictureModel.ThumbImageUrl;
                                                                        pictureModel.FullSizeImageUrl = item._source.AlternatePictureModel.FullSizeImageUrl;
                                                                        pictureModel.Title = item._source.AlternatePictureModel.Title;
                                                                        pictureModel.AlternateText = item._source.AlternatePictureModel.AlternateText;
                                                                        product.AlternatePictureModel = pictureModel;

                                                                    }

                                                                    if (item._source.Tags != null)
                                                                    {
                                                                        List<ProductTag> tags = new List<ProductTag>();
                                                                        foreach (var _tag in item._source.Tags)
                                                                        {
                                                                            ProductTag tag = new ProductTag();
                                                                            tag.Name = _tag.Name;
                                                                            tag.Id = _tag.Id;
                                                                            tags.Add(tag);

                                                                        }
                                                                        product.Tags = tags;
                                                                    }

                                                                    products.Add(product);

                                                                }
                                                                catch (Exception exp)
                                                                {
                                                                }


                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else if (content.Name == "aggregations")
                                    {
                                        foreach (var subcontent in content)
                                        {
                                            foreach (var index in subcontent)
                                            {
                                                levelName = index.Name;
                                                foreach (var subindex in index)
                                                {
                                                    foreach (var innerIndex in subindex)
                                                    {
                                                        if (innerIndex.Name == "buckets")
                                                        {
                                                            foreach (var entities in innerIndex)
                                                            {
                                                                foreach (var entity in entities)
                                                                {
                                                                    if (((string)entity.key).Contains("cat-") && levelName.Contains("categoryFilters")
                                                                   //&& specificationFilterModel.Attributes.Where(m => m.Name == "Category").FirstOrDefault().Values.Count < 11// setting manage
                                                                   // setting Category es_category_filter_type
                                                                   )
                                                                        await BindingFilters(levelName, "Category", Convert.ToInt32(((string)entity.key).Split('-')[1]), (int)entity.doc_count, filters, catID, specificationAttrs,
                                                                                   specificationAttrOptions, specificationFilterModel, categories, workingLanguage);
                                                                    else if (((string)entity.key).Contains("sec-") && levelName.Contains("filterLevel"))
                                                                        await BindingFilters(levelName, "Section", Convert.ToInt32(((string)entity.key).Split('-')[1]), (int)entity.doc_count, filters, 0, specificationAttrs,
                                                                                    specificationAttrOptions, specificationFilterModel, categories, workingLanguage);

                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                /* response.Listings = esListings;
                 response.filters = esFilters.Where(m => m.ChilFilters.Count() > 0).ToList();
                 response.pageNumber = pageNumber;
                 response.pageSize = pageSize;
                */
            }
            catch (Exception ex)
            {
                await _logger.InsertLogAsync(LogLevel.Error, "Elastic Search - Search Page - Function DesearlizeElasticSearchResponse", ex.Message);
            }


            var attributes = specificationFilterModel.Attributes.OrderBy(a => a.DisplayOrder).ThenBy(a => a.Name);

            foreach (var attribute in attributes)
            {
                attribute.Values = attribute.Values.OrderBy(v => v.DisplayOrder).ToList();
            }

            specificationFilterModel.Attributes = attributes.ToList();

            return (products, specificationFilterModel, total);
        }



        #endregion



        private async Task<int> PrepareCatagoryAttributes(string queryString, IList<Category> categories)
        {
            int categoryId = 0;
            Dictionary<int, List<int>> selectedCategory = new Dictionary<int, List<int>>();
            if (queryString != "")
            {
                queryString = WebUtility.UrlDecode(queryString);
                var ignoreAttrs = (await _localizationService.GetResourceAsync("QueryString.Ignore.Filter.Attrs")).Split('|');
                string[] queryStringArr = queryString.Split('&', StringSplitOptions.RemoveEmptyEntries);
                foreach (string query in queryStringArr)
                {
                    var arr = query.Split('=');
                    if (arr.Length == 2 && arr[1] != "")
                    {
                        if (ignoreAttrs.Where(m => m.Trim().Equals(arr[0].Trim(), StringComparison.CurrentCultureIgnoreCase)).Any())
                            continue;
                        else
                        {
                            if (arr[0].Trim().Equals("categoryid", StringComparison.CurrentCultureIgnoreCase))
                            {
                                string options = arr[1].Replace("[", "").Replace("]", "");
                                arr = options.Split(',', StringSplitOptions.RemoveEmptyEntries);
                                foreach (var option in arr)
                                {
                                    int.TryParse(option, out int _catId);
                                    if (_catId != 0)
                                        categoryId = categories.Where(c => c.Id == _catId).Select(C => C.Id).FirstOrDefault();
                                }
                            }
                        }
                    }
                }

            }

            return categoryId;

        }
        private async Task<Dictionary<int, List<int>>> PrepareSelectedFiltersAttributes(string queryString, List<SpecificationAttribute> specsAttrs,
            List<SpecificationAttributeOption> specsAttrOptions)
        {

            Dictionary<int, List<int>> selectedFilters = new Dictionary<int, List<int>>();
            if (queryString != "")
            {
                queryString = WebUtility.UrlDecode(queryString);
                var ignoreAttrs = (await _localizationService.GetResourceAsync("QueryString.Ignore.Filter.Attrs")).Split('|');
                string[] queryStringArr = queryString.Split('&', StringSplitOptions.RemoveEmptyEntries);
                foreach (string query in queryStringArr)
                {
                    var arr = query.Split('=');
                    if (arr.Length == 2 && arr[1] != "")
                    {
                        if (ignoreAttrs.Where(m => m.Trim().Equals(arr[0].Trim(), StringComparison.CurrentCultureIgnoreCase)).Any())
                            continue;
                        else
                        {
                            var specAttr = specsAttrs.Where(m => m.Name.Equals(arr[0].Trim().Replace("?", ""), StringComparison.CurrentCultureIgnoreCase)).OrderBy(m => m.DisplayOrder).FirstOrDefault();
                            if (specAttr != null)
                            {
                                if (!selectedFilters.Keys.Where(m => m == specAttr.Id).Any())
                                {
                                    List<int> lstSpecificationAttrOptionId = new List<int>();
                                    // Selected options
                                    string options = arr[1].Replace("[", "").Replace("]", "");
                                    arr = options.Split(',', StringSplitOptions.RemoveEmptyEntries);
                                    foreach (var option in arr)
                                    {
                                        var selectedOptions = specsAttrOptions.Where(m => m.SpecificationAttributeId == specAttr.Id
                                         && GenrateFilterSename(m.Name).Trim().Equals(option.Trim(), StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                                        if (selectedOptions != null)
                                            lstSpecificationAttrOptionId.Add(selectedOptions.Id);

                                    }

                                    if (lstSpecificationAttrOptionId.Count > 0)
                                        selectedFilters.Add(specAttr.Id, lstSpecificationAttrOptionId);
                                    // end

                                }
                            }
                        }
                    }
                }

            }

            return selectedFilters;

        }
        private async Task PrepareCustomFiltersAndProducts(CustomCatalogProductsCommand command, Category category, CustomCatalogProductsModel model,
            PriceRangeModel selectedPriceRange, List<SpecificationAttribute> specsAttrs, List<SpecificationAttributeOption> specsAttrOptions,
            Dictionary<int, List<int>> selectedOptions, List<ProductSpecificationAttribute> productSpecificationAttribute, int pictureSize, int featuredId = 0)
        {



            var entityWiseParentFilters = await _filtersMappingByEntityService.GetFiltersMappingByEntityByFilterType(category.Id, "Category", "SpecificationAttribute");
            var entityWiseChildFilters = await _filtersMappingByEntityService.GetFiltersMappingByEntityByFilterType(category.Id, "Category", "SpecificationAttributeOption");

            bool isSelected = false;
            #region 

            // Order and Filter Filters by Entity -- coming soon

            #endregion





            #region Prepare model for Specification Attributes

            var specificationFilterModel = new CustomSpecificationFilterModel();
            List<int> _options = new List<int>();

            if (productSpecificationAttribute?.Any() == true)
            {
                specificationFilterModel.Enabled = true;

                var workingLanguage = await _workContext.GetWorkingLanguageAsync();

                var lstSpecificationAttrs = (from psattr in productSpecificationAttribute
                                             join specAttrOption in specsAttrOptions
                                             on psattr.SpecificationAttributeOptionId equals specAttrOption.Id
                                             join _parentFilter in entityWiseParentFilters
                                             on specAttrOption.SpecificationAttributeId equals _parentFilter.FilterId
                                             into parentFilterGroup
                                             from parentFilter in parentFilterGroup.DefaultIfEmpty()
                                             where parentFilter == null || parentFilter.Disabled == false
                                             select specAttrOption.SpecificationAttributeId).Distinct().ToList();

                var lstSelectedSpecificationAttrs = (from selectedAttr in selectedOptions.Keys.ToList()
                                                     join specAttrOption in lstSpecificationAttrs on
                                                     selectedAttr equals specAttrOption
                                                     select selectedAttr).ToList();

                var mergedList = lstSelectedSpecificationAttrs.Concat(lstSpecificationAttrs.Where(m => !lstSelectedSpecificationAttrs.Exists(lss => lss == m)));


                foreach (var attrId in mergedList)
                {
                    var options = (from psattr in productSpecificationAttribute
                                   join specAttrOption in specsAttrOptions
                                   on psattr.SpecificationAttributeOptionId equals specAttrOption.Id
                                   join _childFilter in entityWiseChildFilters
                                   on specAttrOption.Id equals _childFilter.FilterId
                                  into childFilterGroup
                                   from childFilter in childFilterGroup.DefaultIfEmpty()
                                   where specAttrOption.SpecificationAttributeId == attrId
                                   && (childFilter == null || childFilter.Disabled == false)
                                   orderby (childFilter == null ? specAttrOption.DisplayOrder : childFilter.DisplayOrder), specAttrOption.Name
                                   select specAttrOption).Distinct();
                    var selectedFilterObj = selectedOptions.Where(m => m.Key == attrId);
                    foreach (var _optionObj in options)
                    {

                        var attributeFilter = specificationFilterModel.Attributes.FirstOrDefault(model => model.Id == _optionObj.SpecificationAttributeId);
                        if (attributeFilter == null)
                        {
                            var attribute = specsAttrs.Where(m => m.Id == _optionObj.SpecificationAttributeId).FirstOrDefault();
                            if (attribute != null)
                            {
                                attributeFilter = new CustomSpecificationAttributeFilterModel
                                {
                                    Id = attribute.Id,
                                    Name = await _localizationService
                                        .GetLocalizedAsync(attribute, x => x.Name, workingLanguage.Id),
                                    DisplayOrder = attribute.DisplayOrder + 1000,
                                    Sename = GenrateFilterSename(await _localizationService
                                    .GetLocalizedAsync(_optionObj, x => x.Name, workingLanguage.Id))
                                };
                                specificationFilterModel.Attributes.Add(attributeFilter);
                            }
                        }


                        if (selectedFilterObj.Count() == 0)
                            isSelected = false;
                        else

                            isSelected = selectedFilterObj.FirstOrDefault().Value.Exists(m => m == _optionObj.Id);
                        if (isSelected)
                            _options.Add(_optionObj.Id);

                        if (attributeFilter != null)
                            attributeFilter.Values.Add(new CustomSpecificationAttributeValueFilterModel
                            {
                                Id = _optionObj.Id,
                                Name = await _localizationService
                                    .GetLocalizedAsync(_optionObj, x => x.Name, workingLanguage.Id),
                                Selected = isSelected,
                                ColorSquaresRgb = _optionObj.ColorSquaresRgb,
                                Count = productSpecificationAttribute.Where(m => m.SpecificationAttributeOptionId == _optionObj.Id).Count(),
                                DisplayOrder = _optionObj.DisplayOrder + 1000,
                                Sename = GenrateFilterSename(await _localizationService
                                    .GetLocalizedAsync(_optionObj, x => x.Name, workingLanguage.Id))
                            });
                    }

                    // filterProducts
                    if (selectedFilterObj.Count() > 0)
                    {
                        var prdIds = productSpecificationAttribute.Where(ps => selectedFilterObj.FirstOrDefault().Value.Exists(f => f == ps.SpecificationAttributeOptionId)).ToList();
                        productSpecificationAttribute = productSpecificationAttribute.Where(ps =>
                         prdIds.
                         Exists(cmb => cmb.ProductId == ps.ProductId)
                        ).ToList();
                    }


                    // end


                }
            }
            if ((await _categoryService.GetChildCategoryIdsAsync(category.Id)).Any())
            {
                specificationFilterModel.Attributes = (from attribute in specificationFilterModel.Attributes
                                                       join _parentFilter in entityWiseParentFilters
                                                       on attribute.Id equals _parentFilter.FilterId
                                                       into parentFilterGroup
                                                       from parentFilter in parentFilterGroup.DefaultIfEmpty()
                                                       orderby (parentFilter == null ? attribute.DisplayOrder : parentFilter.DisplayOrder), attribute.Name
                                                       select new CustomSpecificationAttributeFilterModel()
                                                       {
                                                           Id = attribute.Id,
                                                           DisplayOrder = attribute.DisplayOrder,
                                                           Name = attribute.Name,
                                                           Sename = attribute.Sename,
                                                           DisplayOnTop = parentFilter == null ? false : parentFilter.DisplayOnTop,
                                                           Values = attribute.Values

                                                       }).Distinct().ToList();
            }
            else
            {
                specificationFilterModel.Attributes = (from attribute in specificationFilterModel.Attributes
                                                       join _parentFilter in entityWiseParentFilters
                                                       on attribute.Id equals _parentFilter.FilterId
                                                       into parentFilterGroup
                                                       from parentFilter in parentFilterGroup.DefaultIfEmpty()
                                                       orderby (parentFilter == null ? attribute.DisplayOrder : parentFilter.DisplayOrder), attribute.Name
                                                       select attribute).Distinct().ToList();
            }


            model.SpecificationFilter = specificationFilterModel;

            #endregion


            //#endregion


            #region Products

            bool isThirdPositionWidgetApplied = await _customCategoryService.IsMwtWidgetApplied(CustomPublicWidgetZones.CategoryDetailsProductListThirdPosition, category.Id, "Category", _customWorkContext.IsMobileDevice());
            bool isNinthPositionWidgetApplied = await _customCategoryService.IsMwtWidgetApplied(CustomPublicWidgetZones.CategoryDetailsProductListNinthPosition, category.Id, "Category", _customWorkContext.IsMobileDevice());
            bool isSixthPositionWidgetApplied = await _customCategoryService.IsMwtWidgetApplied(CustomPublicWidgetZones.CategoryDetailsProductListSixthPosition, category.Id, "Category", _customWorkContext.IsMobileDevice());
            var template = await this.GetCategoryTemplate(category.CategoryTemplateId);
            int pageSize = command.PageSize, firstPageSize = command.PageSize, subsequentPageSize = command.PageSize;
            if (command.PageNumber > 1 && template.SubsequentPageSize > 0)
                subsequentPageSize = pageSize = template.SubsequentPageSize;
            if (!template.ShowBannersOnSubsequentPages && (command.PageNumber > 1))
                model.ShowBanner = false;

            if (!_customWorkContext.IsMobileDevice())
            {
                if (isThirdPositionWidgetApplied || isNinthPositionWidgetApplied || isSixthPositionWidgetApplied)
                {

                    if (template.ShowBannersOnSubsequentPages || (command.PageNumber == 1))
                        pageSize = pageSize + template.PageSize_Extension_Third_Banner + (isThirdPositionWidgetApplied && isNinthPositionWidgetApplied ? -1 : 0);
                    if (template.ShowBannersOnSubsequentPages)
                        subsequentPageSize = subsequentPageSize + template.PageSize_Extension_Third_Banner + (isThirdPositionWidgetApplied && isNinthPositionWidgetApplied ? -1 : 0);
                    firstPageSize = firstPageSize + template.PageSize_Extension_Third_Banner + (isThirdPositionWidgetApplied && isNinthPositionWidgetApplied ? -1 : 0);
                }
            }
            else
            {
                if (isThirdPositionWidgetApplied || isNinthPositionWidgetApplied || isSixthPositionWidgetApplied)
                {
                    if (template.ShowBannersOnSubsequentPages || (command.PageNumber == 1))
                        pageSize = pageSize + 1;
                    if (template.ShowBannersOnSubsequentPages)
                        subsequentPageSize = subsequentPageSize + 1;
                    firstPageSize = firstPageSize + +1;
                }
            }

            #region Sorting

            List<int> productIds = new List<int>();

            //Dictionary<int, int> _dicSortedProducts = new Dictionary<int, int>();
            //List<ProductSpecificationAttribute> attrs = new List<ProductSpecificationAttribute>();
            //if (selectedOptions.Count > 0 && hasFilters)
            //{
            //    List<int> _options = new List<int>();
            //    foreach (var option in selectedOptions)
            //    {
            //        foreach (var _option in option.Value)
            //        {
            //            _options.Add(_option);
            //        }
            //    }
            //    attrs = (from specOptionAttr in productSpecificationAttribute
            //             join _entityWiseChildFilters in entityWiseChildFilters
            //             on specOptionAttr.SpecificationAttributeOptionId equals _entityWiseChildFilters.FilterId
            //             join _option in _options
            //              on specOptionAttr.SpecificationAttributeOptionId equals _option
            //             select specOptionAttr).ToList();

            //    List<FilterProductSort> _products = new List<FilterProductSort>();
            //    int row_num = 0;
            //    foreach (var _option in _options)
            //    {
            //        var sortPrds = (from attr in attrs
            //                        where attr.SpecificationAttributeOptionId == _option
            //                        select new FilterProductSort
            //                        {
            //                            DisplayOrder = attr.DisplayOrder,
            //                            ProductID = attr.ProductId,
            //                            RowNumb = row_num
            //                        }).ToList();
            //        if (sortPrds.Count > 0)
            //        {
            //            row_num++;
            //            _products = _products.Concat(sortPrds).ToList();
            //        }
            //    }
            //    var _productIds = productSpecificationAttribute.Select(m => m.ProductId).Distinct();
            //    foreach (var prdId in _productIds)
            //    {
            //        var prd = _products.Where(m => m.ProductID == prdId).OrderBy(m => m.RowNumb).ThenBy(m => m.DisplayOrder).First();
            //        _dicSortedProducts.Add(prd.ProductID, prd.DisplayOrder);
            //    }
            //    productIds = _dicSortedProducts.OrderBy(m => m.Value).Select(m => m.Key).ToList();
            //}
            //else
            productIds = productSpecificationAttribute.Select(m => m.ProductId).Distinct().ToList();




            #endregion


            var products = await _customProductService.CustomSearchProductsWithVariablePageSizeAsync(productIds.ToArray(), _options, (ProductSortingEnum)command.OrderBy,
                 command.ViewAll == 1 ? 0 : command.PageNumber - 1,
                     command.ViewAll == 1 ? int.MaxValue : pageSize,
                     command.ViewAll == 1 ? int.MaxValue : firstPageSize, command.ViewAll == 1 ? int.MaxValue : subsequentPageSize,
                     categoryId: category.Id, featuredId: featuredId, featuredListingDisplaySimilarOnTop: category.FeaturedListingDisplaySimilarOnTop,
                     isMobileDevice: _customWorkContext.IsMobileDevice());



            var isFiltering = productSpecificationAttribute.Any() || selectedPriceRange?.From is not null;
            await PrepareCustomCatalogProductsAsync(model, category, products, isFiltering, pictureSize: pictureSize);

            if ((ProductSortingEnum)command.OrderBy == ProductSortingEnum.Position && featuredId != 0 && products.Count > 0)
            {
                var relatedProducts = await _productService.GetRelatedProductsByProductId1Async(featuredId);
                foreach (var prd in model.Products)
                {
                    prd.IsSimilarProduct = relatedProducts.Where(rp => rp.ProductId2 == prd.Id).Any() ? true : false;
                }
            }


            model.defaultPageSize = category.PageSize;

            #endregion

        }

        private async Task<int> PrepareSortByToEnum(string sortBy)
        {
            var orderBy = (int)ProductSortingEnum.Position;
            if (!string.IsNullOrEmpty(sortBy))
            {
                foreach (int i in Enum.GetValues(typeof(ProductSortingEnum)))
                {
                    var value = await _localizationService.GetLocalizedEnumAsync((ProductSortingEnum)i);
                    if (value.Replace(" ", "_").Equals(sortBy.Replace("[", "").Replace("]", ""), StringComparison.CurrentCultureIgnoreCase))
                    {
                        orderBy = i;
                        break;
                    }

                }
            }
            return orderBy;
        }

        private string GenrateFilterSename(string name)
        {
            return name.Replace(" ", "-").Replace("&", "-");
        }

        #endregion


    }

    public class FilterProductSort
    {
        public int ProductID { get; set; }
        public int RowNumb { get; set; }
        public int DisplayOrder { get; set; }
    }



}

