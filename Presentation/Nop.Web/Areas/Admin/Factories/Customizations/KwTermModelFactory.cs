using AspNetCoreGeneratedDocument;
using Microsoft.AspNetCore.Mvc.Rendering;
using MWT.Nop.Core.Domain.KW;
using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Services.KW;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Areas.Admin.Models.Customization.Custom.KW;
using Nop.Web.Framework.Extensions;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
namespace Nop.Web.Areas.Admin.Factories.Customization
{
    /// <summary>
    /// Represents the category model factory implementation
    /// </summary>
    public partial class KwTermModelFactory : IKwTermModelFactory
    {
        private readonly CatalogSettings _catalogSettings;
        private readonly CurrencySettings _currencySettings;
        private readonly ICurrencyService _currencyService;
        private readonly IAclSupportedModelFactory _aclSupportedModelFactory;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ICategoryService _categoryService;
        private readonly IDiscountService _discountService;
        private readonly IDiscountSupportedModelFactory _discountSupportedModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly IProductService _productService;
        private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IKwTermService _kwTermService;

        #region Ctor

        public KwTermModelFactory(CatalogSettings catalogSettings,
            CurrencySettings currencySettings,
            ICurrencyService currencyService,
            IAclSupportedModelFactory aclSupportedModelFactory,
            IBaseAdminModelFactory baseAdminModelFactory,
            ICategoryService categoryService,
            IDiscountService discountService,
            IDiscountSupportedModelFactory discountSupportedModelFactory,
            ILocalizationService localizationService,
            ILocalizedModelFactory localizedModelFactory,
            IProductService productService,
            IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory,
            IUrlRecordService urlRecordService,
            IKwTermService kwTermsService)
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
            _kwTermService = kwTermsService;
        }

        #endregion

        #region Products
        public async Task<KwTermSearchModel> PrepareKwTermSearchModelAsync(KwTermSearchModel searchModel)
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
        public async Task<KwTermListModel> PrepareKwTermListModelAsync(KwTermSearchModel searchModel)
        {

            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));
            //get categories
            var kwTerms = await _kwTermService.GetAllKwTermsAsync(kwTermName: searchModel.SearchKwTermName,
                showHidden: true,
                storeId: searchModel.SearchStoreId,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize,
                overridePublished: searchModel.SearchPublishedId == 0 ? null : (bool?)(searchModel.SearchPublishedId == 1));

            //prepare grid model
            var model = await new KwTermListModel().PrepareToGridAsync(searchModel, kwTerms, () =>
            {

                return kwTerms.SelectAwait(async kwTerm =>
                {
                    //fill in model values from the entity
                    var kwTermModel = new KwTermModel();

                    //fill in additional values (not existing in the entity)
                    kwTermModel.Breadcrumb = await _kwTermService.GetFormattedBreadCrumbAsync(kwTerm);
                    kwTermModel.SeName = await _urlRecordService.GetSeNameAsync(kwTerm, 0, true, false);
                    kwTermModel.Name = kwTerm.Name;
                    kwTermModel.Published = kwTerm.Published;
                    kwTermModel.DisplayOrder = kwTerm.DisplayOrder;
                    kwTermModel.Id = kwTerm.Id;
                    kwTermModel.KwTermTemplateId = kwTerm.KwTermsTemplateId;
                    kwTermModel.AllowCustomersToSelectPageSize = kwTerm.AllowCustomersToSelectPageSize;
                    kwTermModel.Deleted = kwTerm.Deleted;
                    kwTermModel.Description = kwTerm.Description;
                    kwTermModel.MetaDescription = kwTerm.MetaDescription;
                    kwTermModel.MetaKeywords = kwTerm.MetaKeywords;
                    kwTermModel.MetaKeywords = kwTerm.MetaTitle;
                    kwTermModel.PictureId = kwTerm.PictureId;
                    kwTermModel.AllowCustomersToSelectPageSize = kwTerm.AllowCustomersToSelectPageSize;
                    return kwTermModel;
                });
            });

            return model;

        }
        public async Task<KwTermModel> PrepareKwTermModelAsync(KwTermModel model, KwTerm kwTerm, bool excludeProperties = false)
        {

            Func<KwTermLocalizedModel, int , Task> localizedModelConfiguration = null;

            if (kwTerm != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = new KwTermModel();
                    model.SeName = await _urlRecordService.GetSeNameAsync(kwTerm, 0, true, false);
                    model.AllowCustomersToSelectPageSize = kwTerm.AllowCustomersToSelectPageSize;
                    model.Published = kwTerm.Published;
                    model.PictureId = kwTerm.PictureId;
                    model.PageSizeOptions = kwTerm.PageSizeOptions;
                    model.PageSize = kwTerm.PageSize;
                    model.Name = kwTerm.Name;
                    model.MetaTitle = kwTerm.MetaTitle;
                    model.Description = kwTerm.Description;
                    model.MetaDescription = kwTerm.MetaDescription;
                    model.MetaKeywords = kwTerm.MetaKeywords;
                    model.KwTermTemplateId = kwTerm.KwTermsTemplateId;
                    model.DisplayOrder = kwTerm.DisplayOrder;
                    model.Id = kwTerm.Id;
                    model.EnableInfiniteScroll = kwTerm.EnableInfiniteScroll;
                }
                PrepareKwTermCategorySearchModel(model.KwTermCategorySearchModel, kwTerm);
                //prepare nested search model
                PrepareKwTermProductSearchModel(model.KwTermProductSearchModel, kwTerm);

                //define localized model configuration action
                localizedModelConfiguration = async (locale, languageId) =>
                {
                    locale.Name = await _localizationService.GetLocalizedAsync(kwTerm, entity => entity.Name, languageId, false, false);
                    locale.Description = await _localizationService.GetLocalizedAsync(kwTerm, entity => entity.Description, languageId, false, false);
                    locale.MetaKeywords = await _localizationService.GetLocalizedAsync(kwTerm, entity => entity.MetaKeywords, languageId, false, false);
                    locale.MetaDescription = await _localizationService.GetLocalizedAsync(kwTerm, entity => entity.MetaDescription, languageId, false, false);
                    locale.MetaTitle = await _localizationService.GetLocalizedAsync(kwTerm, entity => entity.MetaTitle, languageId, false, false);
                    locale.SeName = await _urlRecordService.GetSeNameAsync(kwTerm, languageId, false, false);
                };
            }

            //set default values for the new model
            if (kwTerm == null)
            {
                model.PageSize = _catalogSettings.DefaultCategoryPageSize;
                model.PageSizeOptions = _catalogSettings.DefaultCategoryPageSizeOptions;
                model.Published = true;
                model.AllowCustomersToSelectPageSize = true;
            }

            model.PrimaryStoreCurrencyCode = (await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId)).CurrencyCode;

            //prepare localized models
            if (!excludeProperties)
                model.Locales = (IList<KwTermLocalizedModel>)await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);

            //prepare available category templates
            await _baseAdminModelFactory.PrepareKWTemplatesAsync(model.AvailableKwTermTemplates, false);

            //prepare available parent categories
            await _baseAdminModelFactory.PrepareCategoriesAsync(model.AvailableKwTerm,
                defaultItemText: await _localizationService.GetResourceAsync("Admin.Catalog.Categories.Fields.Parent.None"));

            /*          //prepare model discounts
                      var availableDiscounts = await _discountService.GetAllDiscountsAsync(DiscountType., showHidden: true);
                      await _discountSupportedModelFactory.PrepareModelDiscountsAsync(model, category, availableDiscounts, excludeProperties);*/

            //prepare model customer roles
            // need to confirm
            //   await _aclSupportedModelFactory.PrepareModelCustomerRolesAsync(model, kwTerm, excludeProperties);
            await _aclSupportedModelFactory.PrepareModelCustomerRolesAsync(model);
            //prepare model stores
            await _storeMappingSupportedModelFactory.PrepareModelStoresAsync(model, kwTerm, excludeProperties);

            return model;

        }
        public async Task<AddProductToKwTermListModel> CustomPrepareAddProductToKwTermListModelAsync(AddProductToKwTermSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get products
            var _productService = EngineContext.Current.Resolve<IProductExtendedService>();
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
            var model = await new AddProductToKwTermListModel().PrepareToGridAsync(searchModel, products, () =>
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
        public async Task<KwTermProductListModel> CustomPrepareKwTermProductListModelAsync(KwTermProductSearchModel searchModel, KwTerm kwTerm)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (kwTerm == null)
                throw new ArgumentNullException(nameof(kwTerm));

            //get product categories
            var productTerms = await _kwTermService.GetProductKwTermsByKwTermIdAsync(kwTerm.Id,
                showHidden: true,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);
            var _pictureService = EngineContext.Current.Resolve<IPictureService>();
            //prepare grid model
            var model = await new KwTermProductListModel().PrepareToGridAsync(searchModel, productTerms, () =>
            {
                return productTerms.SelectAwait(async productKwTerm =>
                {
                    //fill in model values from the entity
                    var KwTermProductModel = productKwTerm.ToModel<KwTermProductModel>();

                    //fill in additional values (not existing in the entity)
                    KwTermProductModel.ProductName = (await _productService.GetProductByIdAsync(productKwTerm.ProductId))?.Name;
                    var defaultProductPicture = (await _pictureService.GetPicturesByProductIdAsync(productKwTerm.ProductId, 1)).FirstOrDefault();
                    (KwTermProductModel.PictureThumbnailUrl, _) = await _pictureService.GetPictureUrlAsync(defaultProductPicture, 75);
                    return KwTermProductModel;
                });
            });

            return model;
        }
        public async Task<AddProductToKwTermSearchModel> PrepareAddProductToKwTermSearchModelAsync(AddProductToKwTermSearchModel searchModel)
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
        protected virtual KwTermProductSearchModel PrepareKwTermProductSearchModel(KwTermProductSearchModel searchModel, KwTerm kwTerm)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (kwTerm == null)
                throw new ArgumentNullException(nameof(kwTerm));

            searchModel.KwTermId = kwTerm.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }


        #endregion

        #region Categories

        protected virtual KwTermCategorySearchModel PrepareKwTermCategorySearchModel(KwTermCategorySearchModel searchModel, KwTerm kwTerm)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (kwTerm == null)
                throw new ArgumentNullException(nameof(kwTerm));

            searchModel.KwTermId = kwTerm.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        public async Task<KwTermCategoryListModel> CustomPrepareKwTermCategoryListModelAsync(KwTermCategorySearchModel searchModel, KwTerm kwTerm)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (kwTerm == null)
                throw new ArgumentNullException(nameof(kwTerm));

            //get product categories
            var categoryTerms = await _kwTermService.GetCategoryKwTermsByKwTermIdAsync(kwTerm.Id,
                showHidden: true,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);
            var _pictureService = EngineContext.Current.Resolve<IPictureService>();
            //prepare grid model
            var model = await new KwTermCategoryListModel().PrepareToGridAsync(searchModel, categoryTerms, () =>
            {
                return categoryTerms.SelectAwait(async categoryKwTerm =>
                {
                    //fill in model values from the entity
                    try
                    {
                        var KwTermCategoryModel = categoryKwTerm.ToModel<KwTermCategoryModel>();

                        //fill in additional values (not existing in the entity)
                        KwTermCategoryModel.CategoryName = (await _categoryService.GetCategoryByIdAsync(categoryKwTerm.CategoryId))?.Name;
                        return KwTermCategoryModel;
                    }
                    catch(Exception exp)
                    {
                        return null;
                    }
                 
                });
            });

            return model;
        }

        public async Task<AddCategoryToKwTermListModel> CustomPrepareAddCategoryToKwTermListModelAsync(AddCategoryToKwTermSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get Categories
            var categories = await _categoryService.GetAllCategoriesAsync(categoryName: searchModel.SearchCategoryName,
                showHidden: true,
                storeId: searchModel.SearchStoreId,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize,
                overridePublished: searchModel.SearchPublishedId == 0 ? null : (bool?)(searchModel.SearchPublishedId == 1));

            //prepare grid model
            var model = await new AddCategoryToKwTermListModel().PrepareToGridAsync(searchModel, categories, () =>
            {
                return categories.SelectAwait(async category =>
                {
                    var categoryModel = category.ToModel<CategoryModel>();
                    categoryModel.SeName = await _urlRecordService.GetSeNameAsync(category, 0, true, false);

                    return categoryModel;
                });
            });

            return model;
        }

        public async Task<AddCategoryToKwTermSearchModel> PrepareAddCategoryToKwTermSearchModelAsync(AddCategoryToKwTermSearchModel searchModel)
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
            searchModel.SetPopupGridPageSize();

            return searchModel;


        }

        #endregion
    }
}
