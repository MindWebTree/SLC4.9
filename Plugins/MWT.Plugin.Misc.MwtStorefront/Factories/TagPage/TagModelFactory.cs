using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using MWT.Nop.Core.Domain.TagPage;
using MWT.Nop.Core.Domain.TagPage.Cache;
using MWT.Nop.Core.Infrastructure;
using MWT.Nop.Core.Service;
using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Services.TagPage;
using MWT.Plugin.Misc.MwtStorefront.Models.Catalog;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Seo;
using Nop.Web.Factories;
using Nop.Web.Models.Catalog; 

namespace MWT.Plugin.Misc.MwtStorefront.Factories.TagPage
{
    public partial class TagModelFactory : ITagModelFactory
    {
        #region Props

        private readonly ILocalizationService _localizationService;
        private readonly ICatalogModelFactory _catalogModelFactory;
        private readonly IProductExtendedService _productService;
        private readonly IStoreContext _storeContext;
        private readonly CatalogSettings _catalogSettings;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly ITagSpecificationAttributeService _tagSpecificationAttributeService;
        private readonly ICustomWorkContext _customWorkContext;
        private readonly IWorkContext _workContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICategoryService _categoryService;
        private readonly ITagProductService _tagProductService;
        private readonly ICustomProductModelFactory _productModelFactory;
        private readonly IUrlRecordService _urlRecordService;
        private readonly ICustomerService _customerService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IFiltersMappingByEntityService _filtersMappingByEntityService;



        #endregion

        #region Ctor

        public TagModelFactory(ILocalizationService localizationService, ICatalogModelFactory catalogModelFactory,
            IProductExtendedService productService, IStoreContext storeContext, CatalogSettings catalogSettings,
            ISpecificationAttributeService specificationAttributeService, ITagSpecificationAttributeService tagSpecificationAttributeService,
            IWorkContext workContext, ICustomWorkContext customWorkContext, IHttpContextAccessor httpContextAccessor, ICategoryService categoryService,
            ITagProductService tagProductService, ICustomProductModelFactory productModelFactory, IUrlRecordService urlRecordService, IStaticCacheManager staticCacheManager,
            ICustomerService customerService, IFiltersMappingByEntityService filtersMappingByEntityService)
        {
            _localizationService = localizationService;
            _catalogModelFactory = catalogModelFactory;
            _productService = productService;
            _storeContext = storeContext;
            _catalogSettings = catalogSettings;
            _specificationAttributeService = specificationAttributeService;
            _tagSpecificationAttributeService = tagSpecificationAttributeService;
            _workContext = workContext;
            _customWorkContext = customWorkContext;
            _httpContextAccessor = httpContextAccessor;
            _categoryService = categoryService;
            _tagProductService = tagProductService;
            _productModelFactory = productModelFactory;
            _urlRecordService = urlRecordService;
            _staticCacheManager = staticCacheManager;
            _customerService = customerService; 
            _filtersMappingByEntityService = filtersMappingByEntityService;
        }

        #endregion

        #region Method
        public async Task<CustomCategoryModel> PrepareTagModelAsync(TagSlugMapping tag, Category category, string categorySlug, CustomCatalogProductsCommand command, string queryString, int pictureSize)
        {

            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var model = new CustomCategoryModel
            {
                Id = tag.Id,
                Name = $"{category?.Name ?? tag.Label}",
                Description = tag.Description,
                AdditionalDescription = tag.AdditionalDescription,
                MetaKeywords = tag.MetaKeywords,
                MetaDescription = tag.MetaDescription,
                MetaTitle = tag.MetaTitle,
                SeName = $"{tag.Slug}/{categorySlug}",
                CatalogProductsModel = await PrepareTagProductsModelAsync(tag, category, command, queryString, pictureSize: pictureSize),
                QuickFilterHeading = string.Empty,
                EnableInfiniteScroll = tag.EnableInfiniteScroll,
                DisplayGridListOption = false,
                FeaturedListingDisplaySimilarOnTop = false
            };

            if (_catalogSettings.CategoryBreadcrumbEnabled)
            {
                model.DisplayCategoryBreadcrumb = true;

                model.CategoryBreadcrumb.Add(new CategoryModel()
                {
                    Id = tag.Id,
                    Name = tag.Label,
                    SeName = tag.Slug
                });

                if (!string.Equals(categorySlug, "all", StringComparison.InvariantCultureIgnoreCase))
                {
                    model.CategoryBreadcrumb.Add(new CategoryModel()
                    {
                        Id = tag.Id,
                        Name = category.Name,
                        SeName = categorySlug
                    });
                }
            }
            return model;
        }

        //public virtual async Task<string> PrepareTagSitemapXmlAsync(int? id)
        //{
        //    var language = await _workContext.GetWorkingLanguageAsync();
        //    var customer = await _workContext.GetCurrentCustomerAsync();
        //    var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
        //    var store = await _storeContext.GetCurrentStoreAsync();
        //    var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopTagCatalogDefaults.TagsSitemapSeoModelKey,
        //        id, language, customerRoleIds, store);
        //    var siteMap = await _staticCacheManager.GetAsync(cacheKey, async () => await _sitemapGenerator.TagsGenerateAsync(id));
        //    return siteMap;
        //}

        #endregion

        #region Utilities

        public virtual async Task<CustomCatalogProductsModel> PrepareTagProductsModelAsync(TagSlugMapping tag, Category category, CustomCatalogProductsCommand command,
          string queryString = "", int categoryId = 0, bool forSections = false, int pictureSize = 0)
        {

            if (command == null)
                throw new ArgumentNullException(nameof(command));
            command.OrderBy = await PrepareSortByToEnum(command.SortBy);
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
            await PreparePageSizeOptionsAsync(model, command, false,
                "10,30,40,50", 30);

            //var categoryIds = new List<int>();
            //if ((category?.Id ?? 0) != 0 && _catalogSettings.ShowProductsFromSubcategories)
            //{
            //    categoryIds.Add((int)category.Id);
            //    categoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(category.Id, currentStore.Id));
            //}

            //filterable options
            var filterableOptions = await _tagSpecificationAttributeService
                .GetFilterableCategoriesByTagSegmentAsync(tag.TagId, 0);

            model.SpecificationFilter = await PrepareSpecificationFilterModel(command.Specs, filterableOptions);



            var filteredSpecs = command.Specs is null ? null : filterableOptions.Where(fo => command.Specs.Contains(fo.Id)).ToList();


            #region Products


            var allCategories = await _categoryService.GetAllCategoriesAsync(showHidden: false);

            var specificationAttrs = allCategories.Where(pc => pc.ParentCategoryId == 0).Select(pc => new SpecificationAttribute()
            {
                DisplayOrder = pc.DisplayOrder,
                Id = pc.Id,
                Name = pc.Name,
                SpecificationAttributeGroupId = 0
            }).ToList();
            var specificationAttrOptions = allCategories.Where(pc => pc.ParentCategoryId > 0).Select(pc => new SpecificationAttributeOption()
            {
                DisplayOrder = pc.DisplayOrder,
                Id = pc.Id,
                Name = pc.Name,
                SpecificationAttributeId = pc.ParentCategoryId,
            }).ToList();




            string featuredIdQuery = _httpContextAccessor.HttpContext.Request.Query["featuredid"];
            int featuredId = 0;
            if (featuredIdQuery != null)
            {
                if (featuredIdQuery.IndexOf("-") > 0)
                    featuredIdQuery = featuredIdQuery.Split('-')[0];
                int.TryParse(featuredIdQuery, out featuredId);
            }
            // Get All Products Ids with specification Attributes

            var productSpecificationAttribute = await _tagProductService.CustomSearchGetProductSpecificationAttributeAsync(
                categoryIds: new List<int>(),
                storeId: currentStore.Id,
                visibleIndividuallyOnly: true,
                  excludeFeaturedProducts: false,
                   featuredId: featuredId, isMobileDevice: _customWorkContext.IsMobileDevice(),
                  productTagId: tag.TagId
                 );


            // end

            var filters = new Dictionary<int, List<int>>();

            if (category != null)
            {
                filters.Add(category.ParentCategoryId, new List<int> { category.Id });
            }
            await PrepareFiltersAndProducts(command,
                tag, category, model, specificationAttrs, specificationAttrOptions, filters, productSpecificationAttribute, pictureSize, featuredId);




            #endregion

            model.SpecificationFilter.CustomProperties.Add("Categoryid", category.Id.ToString());
            if ((category?.Id ?? 0) > 0)
                model.SpecificationFilter.CustomProperties.Add("Indexable", false.ToString());
            model.SpecificationFilter.CustomProperties.Add("TagName", tag.Label);
            model.SpecificationFilter.CustomProperties.Add("TagSlug", tag.Slug);
            model.SpecificationFilter.CustomProperties.Add("ExploreMoreLinks", tag.ExploreMoreLinks);
            return model;




        }

        public virtual async Task PrepareViewModesAsync(CatalogProductsModel model, CatalogProductsCommand command)
        {
            model.AllowProductViewModeChanging = _catalogSettings.AllowProductViewModeChanging;

            var viewMode = !string.IsNullOrEmpty(command.ViewMode)
                ? command.ViewMode
                : _catalogSettings.DefaultViewMode;
            model.ViewMode = viewMode;
            if (model.AllowProductViewModeChanging)
            {
                //grid
                model.AvailableViewModes.Add(new SelectListItem
                {
                    Text = await _localizationService.GetResourceAsync("Catalog.ViewMode.Grid"),
                    Value = "grid",
                    Selected = viewMode == "grid"
                });
                //list
                model.AvailableViewModes.Add(new SelectListItem
                {
                    Text = await _localizationService.GetResourceAsync("Catalog.ViewMode.List"),
                    Value = "list",
                    Selected = viewMode == "list"
                });
            }
        }
        public virtual async Task PrepareSortingOptionsAsync(CatalogProductsModel model, CatalogProductsCommand command)
        {
            //set the order by position by default
            model.OrderBy = command.OrderBy;
            command.OrderBy = (int)ProductSortingEnum.Position;

            //ensure that product sorting is enabled
            if (!_catalogSettings.AllowProductSorting)
                return;

            //get active sorting options
            var activeSortingOptionsIds = Enum.GetValues(typeof(ProductSortingEnum)).Cast<int>()
                .Except(_catalogSettings.ProductSortingEnumDisabled).ToList();
            if (!activeSortingOptionsIds.Any())
                return;

            //order sorting options
            var orderedActiveSortingOptions = activeSortingOptionsIds
                .Select(id => new { Id = id, Order = _catalogSettings.ProductSortingEnumDisplayOrder.TryGetValue(id, out var order) ? order : id })
                .OrderBy(option => option.Order).ToList();

            model.AllowProductSorting = true;
            command.OrderBy = model.OrderBy ?? orderedActiveSortingOptions.FirstOrDefault().Id;

            //prepare available model sorting options
            foreach (var option in orderedActiveSortingOptions)
            {
                model.AvailableSortOptions.Add(new SelectListItem
                {
                    Text = await _localizationService.GetLocalizedEnumAsync((ProductSortingEnum)option.Id),
                    Value = option.Id.ToString(),
                    Selected = option.Id == command.OrderBy
                });
            }
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
        public virtual Task PreparePageSizeOptionsAsync(CatalogProductsModel model, CatalogProductsCommand command, bool allowCustomersToSelectPageSize, string pageSizeOptions, int fixedPageSize)
        {
            if (command.PageNumber <= 0)
                command.PageNumber = 1;

            model.AllowCustomersToSelectPageSize = false;
            if (allowCustomersToSelectPageSize && pageSizeOptions != null)
            {
                var pageSizes = pageSizeOptions.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (pageSizes.Any())
                {
                    // get the first page size entry to use as the default (category page load) or if customer enters invalid value via query string
                    if (command.PageSize <= 0 || !pageSizes.Contains(command.PageSize.ToString()))
                    {
                        if (int.TryParse(pageSizes.FirstOrDefault(), out var temp))
                        {
                            if (temp > 0)
                                command.PageSize = temp;
                        }
                    }

                    foreach (var pageSize in pageSizes)
                    {
                        if (!int.TryParse(pageSize, out var temp))
                            continue;

                        if (temp <= 0)
                            continue;

                        model.PageSizeOptions.Add(new SelectListItem
                        {
                            Text = pageSize,
                            Value = pageSize,
                            Selected = pageSize.Equals(command.PageSize.ToString(), StringComparison.InvariantCultureIgnoreCase)
                        });
                    }

                    if (model.PageSizeOptions.Any())
                    {
                        model.PageSizeOptions = model.PageSizeOptions.OrderBy(x => int.Parse(x.Value)).ToList();
                        model.AllowCustomersToSelectPageSize = true;

                        if (command.PageSize <= 0)
                            command.PageSize = int.Parse(model.PageSizeOptions.First().Value);
                    }
                }
            }
            else
            {
                //customer is not allowed to select a page size
                command.PageSize = fixedPageSize;
            }

            //ensure pge size is specified
            if (command.PageSize <= 0)
            {
                command.PageSize = fixedPageSize;
            }

            return Task.CompletedTask;
        }
        protected virtual async Task<CustomSpecificationFilterModel> PrepareSpecificationFilterModel(IList<int> selectedOptions, IList<SpecificationAttributeOption> availableOptions)
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
                        var attribute = await _categoryService
                            .GetCategoryByIdAsync(option.SpecificationAttributeId);
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
                        Selected = selectedOptions?.Any(optionId => optionId == option.Id) == true

                    });
                }
            }

            return model;
        }




        private async Task PrepareFiltersAndProducts(CustomCatalogProductsCommand command, TagSlugMapping tag, Category category, CustomCatalogProductsModel model,
            List<SpecificationAttribute> specsAttrs, List<SpecificationAttributeOption> specsAttrOptions,
       Dictionary<int, List<int>> selectedOptions, List<ProductSpecificationAttribute> productSpecificationAttribute, int pictureSize, int featuredId = 0)
        {

          

            bool isSelected = false;
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
                                   where specAttrOption.SpecificationAttributeId == attrId
                                   orderby specAttrOption.DisplayOrder, specAttrOption.Name
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
                                    Sename = await _urlRecordService.GetSeNameAsync(attribute.Id, "Category")
                                };
                                specificationFilterModel.Attributes.Add(attributeFilter);
                            }
                        }


                        if (selectedFilterObj.Count() == 0)
                            isSelected = false;
                        else

                            isSelected = selectedFilterObj.FirstOrDefault().Value.Exists(m => m == _optionObj.Id);
                        if (isSelected)
                        {
                            _options.Add(_optionObj.Id);
                            if (_catalogSettings.ShowProductsFromSubcategories)
                            {
                                _options.AddRange(await _categoryService.GetChildCategoryIdsAsync(_optionObj.Id, (await _storeContext.GetCurrentStoreAsync()).Id));
                            }
                        }

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
                                Sename = await _urlRecordService.GetSeNameAsync(_optionObj.Id, "Category")
                            });
                    }




                    // end


                }
            }


            specificationFilterModel.Attributes = (from attribute in specificationFilterModel.Attributes
                                                   orderby attribute.DisplayOrder, attribute.Name
                                                   select attribute).Distinct().ToList();



            model.SpecificationFilter = specificationFilterModel;

            #endregion


            //#endregion


            #region Products

            int pageSize = command.PageSize;

            #region Sorting

            List<int> productIds = new List<int>();


            productIds = productSpecificationAttribute.Select(m => m.ProductId).Distinct().ToList();




            #endregion

            if (_options.Count == 0 && category?.Id > 0)
            {
                var currentStore = await _storeContext.GetCurrentStoreAsync();
                _options.Add(category.Id);
                _options.AddRange(await _categoryService.GetChildCategoryIdsAsync(category.Id, currentStore.Id));

            }

            var products = await _tagProductService.SearchProductsWithVariablePageSizeAsync(productIds.ToArray(), tag.TagId, _options, (ProductSortingEnum)command.OrderBy,
                 command.ViewAll == 1 ? 0 : command.PageNumber - 1,
                     command.ViewAll == 1 ? int.MaxValue : pageSize,
                      isMobileDevice: _customWorkContext.IsMobileDevice());



            var isFiltering = productSpecificationAttribute.Any();
            await PrepareCatalogProductsAsync(model, products, isFiltering, pictureSize: pictureSize);

            if ((ProductSortingEnum)command.OrderBy == ProductSortingEnum.Position && featuredId != 0 && products.Count > 0)
            {
                var relatedProducts = await _productService.GetRelatedProductsByProductId1Async(featuredId);
                foreach (var prd in model.Products)
                {
                    prd.IsSimilarProduct = relatedProducts.Where(rp => rp.ProductId2 == prd.Id).Any() ? true : false;
                }
            }


            model.defaultPageSize = 30;

            #endregion

        }


        protected virtual async Task PrepareCatalogProductsAsync(CustomCatalogProductsModel model, IPagedList<Product> products, bool isFiltering = false, int pictureSize = 0)
        {
            if (!string.IsNullOrEmpty(model.WarningMessage))
                return;

            if (products.Count == 0 && isFiltering)
                model.NoResultMessage = await _localizationService.GetResourceAsync("Catalog.Products.NoResult");
            else
            {
                var _settingService = EngineContext.Current.Resolve<ISettingService>();
                var prepareSizeShadeAggregation = await _settingService.GetSettingByKeyAsync<bool>("CategoryPage.Display.Attributes.Aggregation");
                var prepareShades = await _settingService.GetSettingByKeyAsync<bool>("CategoryPage.Display.Shades");
                model.Products = (await _productModelFactory.PrepareCustomProductOverviewModelsAsync(products: products, prepareShades: prepareShades, prepareAlternatePictureModel: false, productThumbPictureSize: pictureSize == 0 ? null : pictureSize,
                    isCategorypage: true, prepareSizeShadeAggregation: prepareSizeShadeAggregation)).ToList();
                model.LoadPagedList(products);
            }
        }


        #endregion
    }
}
