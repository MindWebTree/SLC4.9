using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using MWT.Nop.Core.Domain;
using MWT.Nop.Core.Domain.QA;
using MWT.Nop.Core.Infrastructure;
using MWT.Nop.Core.Service;
using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Services.Catalog;
using MWT.Nop.Core.Services.QA;
using MWT.Plugin.Misc.MwtStorefront.Models.Catalog;
using MWT.Plugin.Misc.MwtStorefront.Models.QA;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;
using System.Net;

namespace MWT.Plugin.Misc.MwtStorefront.Factories.QA
{
    public partial class QuestionAnswerModelFactory : IQuestionAnswerModelFactory
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IQuestionAnswerService _questionAnswerService;
        private readonly IQuestionAnswerTemplateService _questionAnswerTemplateService;
        private readonly ICustomerService _customerService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly IPictureService _pictureService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ICustomProductModelFactory _customproductModelFactory;
        private readonly ICustomProductService _customproductService;
        private readonly IProductTagService _productTagService;
        private readonly ICustomSpecificationAttributeService _customspecificationAttributeService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreContext _storeContext;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly ICustomWorkContext _customWorkContext;
        private readonly MediaSettings _mediaSettings;
        private readonly ICategoryService _categoryService;
        private readonly IFiltersMappingByEntityService  _filtersMappingByEntityService;
        private readonly ISettingService _settingService;


        #endregion

        #region Ctor
        public QuestionAnswerModelFactory(
            CatalogSettings catalogSettings,
            IActionContextAccessor actionContextAccessor,
            IQuestionAnswerService QuestionAnswerService,
            IQuestionAnswerTemplateService questionAnswerTemplateService,
           ICustomerService customerService,
            IHttpContextAccessor httpContextAccessor,
            ILocalizationService localizationService,
            IPictureService pictureService,
            IPriceFormatter priceFormatter,
            ICustomProductModelFactory customproductModelFactory,
            ICustomProductService customproductService,
            IProductTagService productTagService,
            ICustomSpecificationAttributeService customspecificationAttributeService,
            IStaticCacheManager staticCacheManager,
            IStoreContext storeContext,
            IUrlHelperFactory urlHelperFactory,
            IUrlRecordService urlRecordService,
            IWebHelper webHelper,
            IWorkContext workContext,
            MediaSettings mediaSettings,
            ICategoryService categoryService,
           ICustomWorkContext customWorkContext,
            IFiltersMappingByEntityService filtersMappingByEntityService,
            ISettingService settingService)
        {
            _catalogSettings = catalogSettings;
            _actionContextAccessor = actionContextAccessor;
            _questionAnswerService = QuestionAnswerService;
            _questionAnswerTemplateService = questionAnswerTemplateService;
            _customerService = customerService;
            _httpContextAccessor = httpContextAccessor;
            _localizationService = localizationService;
            _pictureService = pictureService;
            _priceFormatter = priceFormatter;
            _customproductModelFactory = customproductModelFactory;
            _customproductService = customproductService;
            _productTagService = productTagService;
            _customspecificationAttributeService = customspecificationAttributeService;
            _staticCacheManager = staticCacheManager;
            _storeContext = storeContext;
            _urlHelperFactory = urlHelperFactory;
            _urlRecordService = urlRecordService;
            _webHelper = webHelper;
            _workContext = workContext;
            _mediaSettings = mediaSettings;
            _categoryService = categoryService;
            _customWorkContext = customWorkContext;
            _filtersMappingByEntityService = filtersMappingByEntityService;
            _settingService=settingService;

        }

        #endregion


        #region Methods

        public async Task<QuestionAnswerModel> PrepareQuestionAnswerModelAsync(QuestionAnswer questionAnswer, 
            CustomCatalogProductsCommand command, string queryString, int prdPictureSize
            )
        {
            if (questionAnswer == null)
                throw new ArgumentNullException(nameof(QuestionAnswer));

            if (command == null)
                throw new ArgumentNullException(nameof(command));


            var model = new QuestionAnswerModel
            {
                Id = questionAnswer.Id,
                Name = await _localizationService.GetLocalizedAsync(questionAnswer, x => x.Name),
                Description = await _localizationService.GetLocalizedAsync(questionAnswer, x => x.Description),
                MetaKeywords = await _localizationService.GetLocalizedAsync(questionAnswer, x => x.MetaKeywords),
                MetaDescription = await _localizationService.GetLocalizedAsync(questionAnswer, x => x.MetaDescription),
                MetaTitle = await _localizationService.GetLocalizedAsync(questionAnswer, x => x.MetaTitle),
                SeName = await _urlRecordService.GetSeNameAsync(questionAnswer),
                CatalogProductsModel = await PrepareQuestionAnswerProductsModelAsync(questionAnswer, command, queryString, pictureSize: prdPictureSize),
                EnableInfiniteScroll = questionAnswer.EnableInfiniteScroll,
                ListingLink = questionAnswer.ListingLink,
                ListingTitle = questionAnswer.ListingTitle,
                PublishedOn = questionAnswer.PublishedOn,
                BackgroundColor = questionAnswer.BackgroundColor,
                AuthorName = questionAnswer.AuthorName,
                AuthorPictureId = questionAnswer.AuthorPictureId,
                CreatedOn = questionAnswer.CreatedOnUtc,
                AuthorPictureUrl = questionAnswer.AuthorPictureId == 0 ? string.Empty : await GetPictureUrlAsync(questionAnswer.AuthorPictureId)
            };

            //category breadcrumb
            if (_catalogSettings.CategoryBreadcrumbEnabled)
            {
                model.DisplayCategoryBreadcrumb = true;

                model.QuestionAnswerBreadcrumb = await (await _questionAnswerService.GetQuestionAnswerBreadCrumbAsync(questionAnswer)).SelectAwait(async catBr =>
                    new QuestionAnswerModel
                    {
                        Id = catBr.Id,
                        Name = await _localizationService.GetLocalizedAsync(catBr, x => x.Name),
                        SeName = await _urlRecordService.GetSeNameAsync(catBr)
                    }).ToListAsync();
            }

            var currentStore = await _storeContext.GetCurrentStoreAsync();
            var pictureSize = _mediaSettings.CategoryThumbPictureSize;

            var questionAnswerPictureCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.QuestionAnswerPictureModelKey, questionAnswer,
                       pictureSize, true, await _workContext.GetWorkingLanguageAsync(), _webHelper.IsCurrentConnectionSecured(),
                       currentStore);

            model.PictureId = questionAnswer.PictureId;
            model.PictureModel = await _staticCacheManager.GetAsync(questionAnswerPictureCacheKey, async () =>
            {
                var picture = await _pictureService.GetPictureByIdAsync(questionAnswer.PictureId);
                string fullSizeImageUrl, imageUrl;

                (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                (imageUrl, _) = await _pictureService.GetPictureUrlAsync(picture, pictureSize);

                var pictureModel = new PictureModel
                {
                    FullSizeImageUrl = fullSizeImageUrl,
                    ImageUrl = imageUrl,
                    Title = string.Format(await _localizationService
                        .GetResourceAsync("Media.Category.ImageLinkTitleFormat"), questionAnswer.Name),
                    AlternateText = string.Format(await _localizationService
                        .GetResourceAsync("Media.Category.ImageAlternateTextFormat"), questionAnswer.Name)
                };

                return pictureModel;
            });


            return model;
        }

        public virtual async Task<CustomCatalogProductsModel> PrepareQuestionAnswerProductsModelAsync(QuestionAnswer QuestionAnswer, CustomCatalogProductsCommand command,
    string queryString = "", int categoryId = 0, bool forSections = false, int pictureSize = 0)
        {

            command.OrderBy = await this.PrepareSortByToEnum(command.SortBy);


            if (QuestionAnswer == null)
                throw new ArgumentNullException(nameof(QuestionAnswer));

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
            await PreparePageSizeOptionsAsync(model, command, QuestionAnswer.AllowCustomersToSelectPageSize,
                QuestionAnswer.PageSizeOptions, QuestionAnswer.PageSize);



            //include subcategories



            #region Products



            // get All specification Attributes
            var specificationAttrs = await _customspecificationAttributeService.GetAllSpecificationAttributesAsync();

            // get All specification Options
            var specificationAttrOptions = await _customspecificationAttributeService.GetAllSpecificationOptionsAsync();

            // Get Selected Filters
            var filters = await this.PrepareSelectedFiltersAttributes(queryString, specificationAttrs, specificationAttrOptions);


            // Get All Products Ids with specification Attributes

            var productSpecificationAttribute = await _customproductService.CustomSearchGetProductSpecificationAttributeAsync(
               command.ViewAll == 1 ? 0 : command.PageNumber - 1,
                 command.ViewAll == 1 ? int.MaxValue : command.PageSize,
                  questionAnswerId: QuestionAnswer.Id,
                  storeId: currentStore.Id,
                  visibleIndividuallyOnly: true,
                  excludeFeaturedProducts: !_catalogSettings.IgnoreFeaturedProducts && !_catalogSettings.IncludeFeaturedProductsInNormalLists,
                   manufacturerIds: command.Ms,
                  featuredId: 0
                  , isMobileDevice: _customWorkContext.IsMobileDevice()
                 );


            // end


            await PrepareCustomFiltersAndProducts(command,
                QuestionAnswer, model, specificationAttrs, specificationAttrOptions, filters, productSpecificationAttribute, pictureSize, 0);




            #endregion

            return model;




        }

        public async Task<(string templateViewPath, string listingViewPath, string filterViewPath, string FilterViewPathForMobile, int pictureSize, bool isHorizontal)> PrepareQuestionAnswerTemplateViewPathAsync(int templateId)
        {
            var template = await _questionAnswerTemplateService.GetQuestionAnswerTemplateByIdAsync(templateId) ??
                                    (await _questionAnswerTemplateService.GetAllQuestionAnswerTemplatesAsync()).FirstOrDefault();

            if (template == null)
                throw new Exception("No default template could be loaded");

            return (template.ViewPath, template.GridLineViewPath, template.FilterViewPath, template.FilterViewPathForMobile, template.PictureSize <= 0 ? _mediaSettings.CategoryThumbPictureSize : template.PictureSize, template.IsHorizontal);
        }


        public async Task<QuestionAnswerListModel> PrepareQuestionAnswerList(QuestionAnswerSearchModel searchModel)
        {
            var questionAnswers = await _questionAnswerService.GetAllQuestionAnswersAsync(
         showHidden: false,
         storeId: (await _storeContext.GetCurrentStoreAsync()).Id,
         pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);
            var model = new QuestionAnswerListModel
            {
                UseAjaxLoading = false
            };
            if (questionAnswers.Count == 0)
                model.NoResultMessage = await _localizationService.GetResourceAsync("Catalog.Products.NoResult");
            else
            {
                model.QuestionAnswers = await questionAnswers.SelectAwait(async QuestionAnswer =>
                   {
                       var QuestionAnswerModel = new QuestionAnswerModel();
                       QuestionAnswerModel.SeName = await _urlRecordService.GetSeNameAsync(QuestionAnswer, 0, true, false);
                       QuestionAnswerModel.Name = QuestionAnswer.Name;
                       QuestionAnswerModel.Id = QuestionAnswer.Id;
                       QuestionAnswerModel.Description = QuestionAnswer.Description;
                       QuestionAnswerModel.MetaDescription = QuestionAnswer.MetaDescription;
                       QuestionAnswerModel.MetaKeywords = QuestionAnswer.MetaKeywords;
                       QuestionAnswerModel.MetaKeywords = QuestionAnswer.MetaTitle;
                       return QuestionAnswerModel;
                   }).ToListAsync();


                model.LoadPagedList(questionAnswers);
            }
            return model;

        }


        #endregion

        #region Utilities

        protected async Task<string> GetPictureUrlAsync(int pictureId)
        {
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.Picture_Url_Key,
                pictureId, _webHelper.IsCurrentConnectionSecured() ? Uri.UriSchemeHttps : Uri.UriSchemeHttp);

            return await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                //little hack here. nulls aren't cacheable so set it to ""
                var url = await _pictureService.GetPictureUrlAsync(pictureId, showDefaultPicture: false) ?? "";
                return url;
            });
        }
        private async Task PrepareCustomFiltersAndProducts(CustomCatalogProductsCommand command, QuestionAnswer QuestionAnswer, CustomCatalogProductsModel model, List<SpecificationAttribute> specsAttrs, List<SpecificationAttributeOption> specsAttrOptions,
        Dictionary<int, List<int>> selectedOptions, List<ProductSpecificationAttribute> productSpecificationAttribute, int pictureSize, int featuredId = 0)
        {



            var entityWiseParentFilters = new List<FiltersMappingByEntity>();// await _filtersMappingByEntityService.GetFiltersMappingByEntityByFilterType(QuestionAnswer.Id, "QuestionAnswer", "SpecificationAttribute");
            var entityWiseChildFilters = new List<FiltersMappingByEntity>(); //await _filtersMappingByEntityService.GetFiltersMappingByEntityByFilterType(QuestionAnswer.Id, "QuestionAnswer", "SpecificationAttributeOption");

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
            specificationFilterModel.Attributes = (from attribute in specificationFilterModel.Attributes
                                                   join _parentFilter in entityWiseParentFilters
                                                   on attribute.Id equals _parentFilter.FilterId
                                                   into parentFilterGroup
                                                   from parentFilter in parentFilterGroup.DefaultIfEmpty()
                                                   orderby (parentFilter == null ? attribute.DisplayOrder : parentFilter.DisplayOrder), attribute.Name
                                                   select attribute).Distinct().ToList();
            model.SpecificationFilter = specificationFilterModel;

            #endregion


            //#endregion


            #region Sorting

            List<int> productIds = new List<int>();

            productIds = productSpecificationAttribute.Select(m => m.ProductId).Distinct().ToList();




            #endregion


                var products = await _customproductService.CustomSearchProductsAsync(productIds.ToArray(), _options, (ProductSortingEnum)command.OrderBy,
                 command.ViewAll == 1 ? 0 : command.PageNumber - 1,
                     command.ViewAll == 1 ? int.MaxValue : command.PageSize,
                     categoryId: 0, featuredId: featuredId,
                     isMobileDevice: _customWorkContext.IsMobileDevice(), questionAnswerId: QuestionAnswer.Id);



            var isFiltering = productSpecificationAttribute.Any();
            await PrepareCustomCatalogProductsAsync(model, products, isFiltering, pictureSize: pictureSize);

            model.defaultPageSize = QuestionAnswer.PageSize;


        }

        protected virtual async Task PrepareCustomCatalogProductsAsync(CustomCatalogProductsModel model, IPagedList<Product> products, bool isFiltering = false, int pictureSize = 0)
        {
            if (!string.IsNullOrEmpty(model.WarningMessage))
                return;

            if (products.Count == 0 && isFiltering)
                model.NoResultMessage = await _localizationService.GetResourceAsync("Catalog.Products.NoResult");
            else
            {
          
                var prepareSizeShadeAggregation = await _settingService.GetSettingByKeyAsync<bool>("CategoryPage.Display.Attributes.Aggregation");
                var prepareShades = await _settingService.GetSettingByKeyAsync<bool>("CategoryPage.Display.Shades");
                model.Products = (await _customproductModelFactory.PrepareCustomProductOverviewModelsAsync(products: products, prepareShades: prepareShades, prepareAlternatePictureModel: true, productThumbPictureSize: pictureSize == 0 ? null : pictureSize,
                    isCategorypage: true, prepareSizeShadeAggregation: prepareSizeShadeAggregation)).ToList();
                model.LoadPagedList(products);
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

        public virtual async Task PrepareSortingOptionsAsync(CatalogProductsModel model, CustomCatalogProductsCommand command)
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



        public virtual async Task PrepareViewModesAsync(CatalogProductsModel model, CustomCatalogProductsCommand command)
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

        public virtual Task PreparePageSizeOptionsAsync(CatalogProductsModel model, CustomCatalogProductsCommand command,
     bool allowCustomersToSelectPageSize, string pageSizeOptions, int fixedPageSize)
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

        private string GenrateFilterSename(string name)
        {
            return name.Replace(" ", "-").Replace("&", "-");
        }


        #endregion
    }
}
