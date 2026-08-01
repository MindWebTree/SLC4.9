using Microsoft.AspNetCore.Mvc.Rendering;
using MWT.Nop.Core.Domain.QA;
using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Services.QA;
using MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.QA;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog; 
using Nop.Web.Framework.Extensions;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
namespace MWT.Plugin.Misc.MwtStorefront.Area.Admin.Factories.QA
{
    /// <summary>
    /// Represents the category model factory implementation
    /// </summary>
    public partial class QuestionAnswerModelFactory : IQuestionAnswerModelFactory
    {
        private readonly CatalogSettings _catalogSettings;
        private readonly CurrencySettings _currencySettings;
        private readonly ICurrencyService _currencyService;
        private readonly IAclSupportedModelFactory _aclSupportedModelFactory;
        private readonly ICustomBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ICategoryService _categoryService;
        private readonly IDiscountService _discountService;
        private readonly IDiscountSupportedModelFactory _discountSupportedModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly ICustomProductService _productService;
        private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IQuestionAnswerService _questionAnswerService;
        private readonly IStoreContext _storeContext;
        #region Ctor

        public QuestionAnswerModelFactory(CatalogSettings catalogSettings,
            CurrencySettings currencySettings,
            ICurrencyService currencyService,
            IAclSupportedModelFactory aclSupportedModelFactory,
            ICustomBaseAdminModelFactory baseAdminModelFactory,
            ICategoryService categoryService,
            IDiscountService discountService,
            IDiscountSupportedModelFactory discountSupportedModelFactory,
            ILocalizationService localizationService,
            ILocalizedModelFactory localizedModelFactory,
            ICustomProductService productService,
            IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory,
            IUrlRecordService urlRecordService,
            IQuestionAnswerService questionAnswersService,
            IStoreContext storeContext)
        {
            _catalogSettings = catalogSettings;
            _currencySettings = currencySettings;
            _currencyService = currencyService;
            _aclSupportedModelFactory = aclSupportedModelFactory;
            _baseAdminModelFactory = baseAdminModelFactory;
            _categoryService = categoryService;
            _discountService = discountService;
            _discountSupportedModelFactory = discountSupportedModelFactory;
            _localizationService = localizationService;
            _localizedModelFactory = localizedModelFactory;
            _productService = productService;
            _storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
            _urlRecordService = urlRecordService;
            _questionAnswerService = questionAnswersService;
            _storeContext = storeContext;
        }

        #endregion



        public async Task<QuestionAnswerListModel> PrepareQuestionAnswerListModelAsync(QuestionAnswerSearchModel searchModel)
        {

            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));
            //get categories
            var QuestionAnswers = await _questionAnswerService.GetAllQuestionAnswersAsync(QuestionAnswerName: searchModel.SearchQuestionAnswerName,
                showHidden: true,
                storeId: searchModel.SearchStoreId,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize,
                overridePublished: searchModel.SearchPublishedId == 0 ? null : (bool?)(searchModel.SearchPublishedId == 1));

            //prepare grid model
            var model = await new QuestionAnswerListModel().PrepareToGridAsync(searchModel, QuestionAnswers, () =>
            {

                return QuestionAnswers.SelectAwait(async QuestionAnswer =>
                {
                    //fill in model values from the entity
                    var QuestionAnswerModel = new QuestionAnswerModel();

                    QuestionAnswerModel.Breadcrumb = await _questionAnswerService.GetFormattedBreadCrumbAsync(QuestionAnswer);
                    QuestionAnswerModel.SeName = await _urlRecordService.GetSeNameAsync(QuestionAnswer, 0, true, false);
                    QuestionAnswerModel.Name = QuestionAnswer.Name;
                    QuestionAnswerModel.Published = QuestionAnswer.Published;
                    QuestionAnswerModel.DisplayOrder = QuestionAnswer.DisplayOrder;
                    QuestionAnswerModel.Id = QuestionAnswer.Id;
                    QuestionAnswerModel.QuestionAnswerTemplateId = QuestionAnswer.QuestionAnswerTemplateId;
                    QuestionAnswerModel.AllowCustomersToSelectPageSize = QuestionAnswer.AllowCustomersToSelectPageSize;
                    QuestionAnswerModel.Deleted = QuestionAnswer.Deleted;
                    QuestionAnswerModel.Description = QuestionAnswer.Description;
                    QuestionAnswerModel.MetaDescription = QuestionAnswer.MetaDescription;
                    QuestionAnswerModel.MetaKeywords = QuestionAnswer.MetaKeywords;
                    QuestionAnswerModel.MetaKeywords = QuestionAnswer.MetaTitle;
                    QuestionAnswerModel.PictureId = QuestionAnswer.PictureId;
                    QuestionAnswerModel.AllowCustomersToSelectPageSize = QuestionAnswer.AllowCustomersToSelectPageSize;
                    return QuestionAnswerModel;
                });
            });

            return model;

        }

        public async Task<QuestionAnswerModel> PrepareQuestionAnswerModelAsync(QuestionAnswerModel model, QuestionAnswer QuestionAnswers, bool excludeProperties = false)
        {

            Func<QuestionAnswerLocalizedModel, int, Task> localizedModelConfiguration = null;

            if (QuestionAnswers != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = new QuestionAnswerModel();
                    model.SeName = await _urlRecordService.GetSeNameAsync(QuestionAnswers, 0, true, false);
                    model.AllowCustomersToSelectPageSize = QuestionAnswers.AllowCustomersToSelectPageSize;
                    model.Published = QuestionAnswers.Published;
                    model.PictureId = QuestionAnswers.PictureId;
                    model.PageSizeOptions = QuestionAnswers.PageSizeOptions;
                    model.PageSize = QuestionAnswers.PageSize;
                    model.Name = QuestionAnswers.Name;
                    model.MetaTitle = QuestionAnswers.MetaTitle;
                    model.Description = QuestionAnswers.Description;
                    model.MetaDescription = QuestionAnswers.MetaDescription;
                    model.MetaKeywords = QuestionAnswers.MetaKeywords;
                    model.QuestionAnswerTemplateId = QuestionAnswers.QuestionAnswerTemplateId;
                    model.DisplayOrder = QuestionAnswers.DisplayOrder;
                    model.Id = QuestionAnswers.Id;
                    model.EnableInfiniteScroll = QuestionAnswers.EnableInfiniteScroll;
                    model.PublishedOn = QuestionAnswers.PublishedOn;
                    model.BackgroundColor = QuestionAnswers.BackgroundColor;
                    model.AuthorName = QuestionAnswers.AuthorName;
                    model.AuthorPictureId = QuestionAnswers.AuthorPictureId;
                    model.ListingTitle = QuestionAnswers.ListingTitle;
                    model.ListingLink = QuestionAnswers.ListingLink;

                }

                //prepare nested search model
                PrepareQuestionAnswerProductSearchModel(model.QuestionAnswerProductSearchModel, QuestionAnswers);

                //define localized model configuration action
                localizedModelConfiguration = async (locale, languageId) =>
                {
                    locale.Name = await _localizationService.GetLocalizedAsync(QuestionAnswers, entity => entity.Name, languageId, false, false);
                    locale.Description = await _localizationService.GetLocalizedAsync(QuestionAnswers, entity => entity.Description, languageId, false, false);
                    locale.MetaKeywords = await _localizationService.GetLocalizedAsync(QuestionAnswers, entity => entity.MetaKeywords, languageId, false, false);
                    locale.MetaDescription = await _localizationService.GetLocalizedAsync(QuestionAnswers, entity => entity.MetaDescription, languageId, false, false);
                    locale.MetaTitle = await _localizationService.GetLocalizedAsync(QuestionAnswers, entity => entity.MetaTitle, languageId, false, false);
                    locale.SeName = await _urlRecordService.GetSeNameAsync(QuestionAnswers, languageId, false, false);
                };
            }

            //set default values for the new model
            if (QuestionAnswers == null)
            {
                model.PageSize = _catalogSettings.DefaultCategoryPageSize;
                model.PageSizeOptions = _catalogSettings.DefaultCategoryPageSizeOptions;
                model.Published = true;
                model.AllowCustomersToSelectPageSize = true;
            }

            model.PrimaryStoreCurrencyCode = (await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId)).CurrencyCode;

            //prepare localized models
            if (!excludeProperties)
                model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);
            //prepare available category templates
            await _baseAdminModelFactory.PrepareQuestionAnswerTemplatesAsync(model.AvailableQuestionAnswerTemplates, false);

            //prepare available parent categories
            await _baseAdminModelFactory.PrepareCategoriesAsync(model.AvailableQuestionAnswer,
                defaultItemText: await _localizationService.GetResourceAsync("Admin.Catalog.Categories.Fields.Parent.None"));

            /*          //prepare model discounts
                      var availableDiscounts = await _discountService.GetAllDiscountsAsync(DiscountType., showHidden: true);
                      await _discountSupportedModelFactory.PrepareModelDiscountsAsync(model, category, availableDiscounts, excludeProperties);*/

            //prepare model customer roles
            // need to verify
            await _aclSupportedModelFactory.PrepareModelCustomerRolesAsync(model);

            //prepare model stores
            await _storeMappingSupportedModelFactory.PrepareModelStoresAsync(model, QuestionAnswers, excludeProperties);

            return model;

        }

        #region Products
        public async Task<QuestionAnswerSearchModel> PrepareQuestionAnswerSearchModelAsync(QuestionAnswerSearchModel searchModel)
        {

            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare available stores
            await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

            searchModel.HideStoresList = _catalogSettings.IgnoreStoreLimitations || searchModel.AvailableStores.SelectionIsNotPossible();

            //prepare "published" filter (0 - all; 1 - published only; 2 - unpublished only)
            searchModel.AvailablePublishedOptions.Add(new SelectListItem
            {
                Value = "0",
                Text = await _localizationService.GetResourceAsync("Admin.Catalog.Categories.List.SearchPublished.All")
            });
            searchModel.AvailablePublishedOptions.Add(new SelectListItem
            {
                Value = "1",
                Text = await _localizationService.GetResourceAsync("Admin.Catalog.Categories.List.SearchPublished.PublishedOnly")
            });
            searchModel.AvailablePublishedOptions.Add(new SelectListItem
            {
                Value = "2",
                Text = await _localizationService.GetResourceAsync("Admin.Catalog.Categories.List.SearchPublished.UnpublishedOnly")
            });

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }
        public async Task<AddProductToQuestionAnswerListModel> PrepareAddProductToQuestionAnswerListModelAsync(AddProductToQuestionAnswerSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get products
            var products = await _productService.OverriddenSearchProductsAsync(showHidden: true,
                categoryIds: new List<int> { searchModel.SearchCategoryId },
                manufacturerIds: new List<int> { searchModel.SearchManufacturerId },
                storeId: searchModel.SearchStoreId,
                vendorId: searchModel.SearchVendorId,
                productType: searchModel.SearchProductTypeId > 0 ? (ProductType?)searchModel.SearchProductTypeId : null,
                keywords: searchModel.SearchProductName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);
            var _pictureService = EngineContext.Current.Resolve<IPictureService>();
            //prepare grid model
            var model = await new AddProductToQuestionAnswerListModel().PrepareToGridAsync(searchModel, products, () =>
            {
                return products.SelectAwait(async product =>
                {
                    var productModel = product.ToModel<ProductModel>();
                    var defaultProductPicture = (await _pictureService.GetPicturesByProductIdAsync(product.Id, 1)).FirstOrDefault();
                    (productModel.PictureThumbnailUrl, _) = await _pictureService.GetPictureUrlAsync(defaultProductPicture, 75);
                    productModel.SeName = await _urlRecordService.GetSeNameAsync(product, 0, true, false);

                    return productModel;
                });
            });

            return model;
        }
        public async Task<QuestionAnswerProductListModel> PrepareQuestionAnswerProductListModelAsync(QuestionAnswerProductSearchModel searchModel, QuestionAnswer QuestionAnswer)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (QuestionAnswer == null)
                throw new ArgumentNullException(nameof(QuestionAnswer));

            //get product categories
            var productTerms = await _questionAnswerService.GetProductQuestionAnswersByQuestionAnswerIdAsync(QuestionAnswer.Id,
                showHidden: true,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);
            var _pictureService = EngineContext.Current.Resolve<IPictureService>();
            //prepare grid model
            var model = await new QuestionAnswerProductListModel().PrepareToGridAsync(searchModel, productTerms, () =>
            {
                return productTerms.SelectAwait(async productQuestionAnswer =>
                {
                    //fill in model values from the entity
                    var QuestionAnswerProductModel = productQuestionAnswer.ToModel<QuestionAnswerProductModel>();

                    //fill in additional values (not existing in the entity)
                    QuestionAnswerProductModel.ProductName = (await _productService.GetProductByIdAsync(productQuestionAnswer.ProductId))?.Name;
                    var defaultProductPicture = (await _pictureService.GetPicturesByProductIdAsync(productQuestionAnswer.ProductId, 1)).FirstOrDefault();
                    (QuestionAnswerProductModel.PictureThumbnailUrl, _) = await _pictureService.GetPictureUrlAsync(defaultProductPicture, 75);
                    return QuestionAnswerProductModel;
                });
            });

            return model;
        }
        public async Task<AddProductToQuestionAnswerSearchModel> PrepareAddProductToQuestionAnswerSearchModelAsync(AddProductToQuestionAnswerSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare available categories
            await _baseAdminModelFactory.PrepareCategoriesAsync(searchModel.AvailableCategories);

            //prepare available manufacturers
            await _baseAdminModelFactory.PrepareManufacturersAsync(searchModel.AvailableManufacturers);

            //prepare available stores
            await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

            //prepare available vendors
            await _baseAdminModelFactory.PrepareVendorsAsync(searchModel.AvailableVendors);

            //prepare available product types
            await _baseAdminModelFactory.PrepareProductTypesAsync(searchModel.AvailableProductTypes);

            //prepare page parameters
            searchModel.SetPopupGridPageSize();

            return searchModel;
        }
        protected virtual QuestionAnswerProductSearchModel PrepareQuestionAnswerProductSearchModel(QuestionAnswerProductSearchModel searchModel, QuestionAnswer QuestionAnswer)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (QuestionAnswer == null)
                throw new ArgumentNullException(nameof(QuestionAnswer));

            searchModel.QuestionAnswerId = QuestionAnswer.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }


        #endregion

        #region Related

        public virtual async Task<RelatedQuestionAnswerListModel> PrepareRelatedQuestionAnswerListModelAsync(RelatedQuestionAnswerSearchModel searchModel, QuestionAnswer questionAnswer)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (questionAnswer == null)
                throw new ArgumentNullException(nameof(questionAnswer));

            //get collection products
            var relatedQuestionAnswers = (await _questionAnswerService
                .GetRelatedQuestionAnswersByQuestionAnswerId1Async(questionAnswerId1: questionAnswer.Id, showHidden: true)).ToPagedList(searchModel);

            //prepare grid model
            var model = await new RelatedQuestionAnswerListModel().PrepareToGridAsync(searchModel, relatedQuestionAnswers, () =>
            {
                return relatedQuestionAnswers.SelectAwait(async questionAnswer =>
                {
                    RelatedQuestionAnswerModel relatedQuestionAnswerModel = new RelatedQuestionAnswerModel();
                    relatedQuestionAnswerModel.DisplayOrder = questionAnswer.DisplayOrder;
                    relatedQuestionAnswerModel.Id = questionAnswer.Id;
                    relatedQuestionAnswerModel.QuestionAnswerId2 = questionAnswer.QuestionAnswerId2;
                    //fill in additional values (not existing in the entity)
                    relatedQuestionAnswerModel.QuestionAnswer2Name = (await _questionAnswerService.GetQuestionAnswerByIdAsync(questionAnswer.QuestionAnswerId2))?.Name;

                    return relatedQuestionAnswerModel;
                });
            });
            return model;
        }
        public virtual async Task<AddRelatedQuestionAnswerSearchModel> PrepareAddRelatedQuestionAnswerSearchModelAsync(AddRelatedQuestionAnswerSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));
            await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);
            searchModel.SetPopupGridPageSize();

            return searchModel;
        }

        public virtual async Task<AddRelatedQuestionAnswerListModel> PrepareAddRelatedQuestionAnswerListModelAsync(AddRelatedQuestionAnswerSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

           

            //get products
            var questionAnswers = await _questionAnswerService.GetAllQuestionAnswersAsync(searchModel.SearchName,
                (await _storeContext.GetCurrentStoreAsync()).Id,false,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = await new AddRelatedQuestionAnswerListModel().PrepareToGridAsync(searchModel, questionAnswers, () =>
            {
                return questionAnswers.SelectAwait(async questionAnswer =>
                {
                    QuestionAnswerModel model = new QuestionAnswerModel();
                    model.Id = questionAnswer.Id;
                    model.Name = questionAnswer.Name;
                    model.Published = questionAnswer.Published;
                    model.PublishedOn = questionAnswer.PublishedOn;
                    model.SeName = await _urlRecordService.GetSeNameAsync(questionAnswer, 0, true, false);

                    return model;
                });
            });

            return model;
        }

        #endregion
    }
}
