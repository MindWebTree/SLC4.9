
using MWT.Nop.Core.Domain;
using MWT.Nop.Core.Service;
using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Services.Catalog;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Media;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Areas.Admin.Models.Customization.Custom;
using Nop.Web.Framework.Models.Extensions;
namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the category model factory implementation
    /// </summary>
    public partial class CategoryModelFactory : ICategoryModelFactory
    {


        #region Methods

        public virtual async Task<CategoryListModel> PrepareCustomCategoryListModelAsync(CategorySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var _workContext = EngineContext.Current.Resolve<IWorkContext>();
            var _customerService = EngineContext.Current.Resolve<ICustomerService>();
            var customer = await _workContext.GetCurrentCustomerAsync();
            IPagedList<Category> categories;
            if (await _customerService.IsInCustomerRoleAsync(customer, "CategoryManager"))
            {
                var _categoryService = EngineContext.Current.Resolve<ICustomCategoryService>();
                categories = await _categoryService.GetAccessibleCategoriesAsync(categoryName: searchModel.SearchCategoryName, customerId: customer.Id,
           showHidden: true,
           storeId: searchModel.SearchStoreId,
           pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize,
           overridePublished: searchModel.SearchPublishedId == 0 ? null : (bool?)(searchModel.SearchPublishedId == 1));
            }
            else
            {
                //get categories
                categories = await _categoryService.GetAllCategoriesAsync(categoryName: searchModel.SearchCategoryName,
                   showHidden: true,
                   storeId: searchModel.SearchStoreId,
                   pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize,
                   overridePublished: searchModel.SearchPublishedId == 0 ? null : (bool?)(searchModel.SearchPublishedId == 1));
            }

            //prepare grid model
            var model = await new CategoryListModel().PrepareToGridAsync(searchModel, categories, () =>
            {
                return categories.SelectAwait(async category =>
                {
                    //fill in model values from the entity
                    var categoryModel = category.ToModel<CategoryModel>();

                    //fill in additional values (not existing in the entity)
                    categoryModel.Breadcrumb = await _categoryService.GetFormattedBreadCrumbAsync(category);
                    categoryModel.SeName = await _urlRecordService.GetSeNameAsync(category, 0, true, false);

                    return categoryModel;
                });
            });

            return model;
        }

        public async Task<FiltersMappingByEntityListModel> PrepareFiltersMappingByEntityListModelAsync(FiltersMappingByEntitySearchModel searchModel, int entityId, string entityType, string filterType)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));
            var _filtersMappingByEntityService = EngineContext.Current.Resolve<IFiltersMappingByEntityService>();
            var _customerService = EngineContext.Current.Resolve<ICustomerService>();
            var _specificationAttributeService = EngineContext.Current.Resolve<ISpecificationAttributeService>();

            //get collection products
            var searchList = (await _filtersMappingByEntityService
                .GetFiltersMappingByEntityByFilterType(entityId: entityId, entityType: entityType, filterType: filterType)).ToPagedList(searchModel);

            //prepare grid model
            var model = await new FiltersMappingByEntityListModel().PrepareToGridAsync(searchModel, searchList, () =>
            {
                return searchList.SelectAwait(async filterMappingByEntity =>
                {
                    FiltersMappingByEntityModel filtersMappingByEntityModel = new FiltersMappingByEntityModel();
                    filtersMappingByEntityModel.DisplayOrder = filterMappingByEntity.DisplayOrder;
                    filtersMappingByEntityModel.EntityId = filterMappingByEntity.EntityId;
                    filtersMappingByEntityModel.EntityType = filterMappingByEntity.EntityType;
                    filtersMappingByEntityModel.Disabled = filterMappingByEntity.Disabled;
                    filtersMappingByEntityModel.FilterId = filterMappingByEntity.FilterId;
                    filtersMappingByEntityModel.Filtertype = filterMappingByEntity.Filtertype;
                    filtersMappingByEntityModel.Id = filterMappingByEntity.Id;
                    filtersMappingByEntityModel.filterName = filterType == "SpecificationAttribute" ?
                    (await _specificationAttributeService.GetSpecificationAttributeByIdAsync(filterMappingByEntity.FilterId))?.Name
                    : (await _specificationAttributeService.GetSpecificationAttributeOptionByIdAsync(filterMappingByEntity.FilterId))?.Name;
                    filtersMappingByEntityModel.CreatedBy = (await _customerService.GetCustomerByIdAsync(filterMappingByEntity.CreatedBy))?.Username;
                    filtersMappingByEntityModel.UpdatedBy = (await _customerService.GetCustomerByIdAsync(filterMappingByEntity.CreatedBy))?.Username;
                    return filtersMappingByEntityModel;
                });
            });
            return model;
        }
        public virtual async Task<CategorySpecificationOptionProductListModel> CustomPrepareCategorySpecificationOptionMappingAddPopupListModelAsync(CategorySpecificationOptionProductSearchModel searchModel, Category category)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (category == null)
                throw new ArgumentNullException(nameof(category));

            var productCategories = await _categoryService.GetProductCategoriesByCategoryIdAsync(
                category.Id,
                showHidden: true,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);

            var pictureService = EngineContext.Current.Resolve<IPictureService>();
            var _specificationAttributeService = EngineContext.Current.Resolve<ISpecificationAttributeService>();


            return await new CategorySpecificationOptionProductListModel().PrepareToGridAsync(searchModel, productCategories, () =>
            {
                return productCategories.SelectAwait(async productCategory =>
                {
                    var product = await _productService.GetProductByIdAsync(productCategory.ProductId);
                    if (product == null)
                        return null;

                    var existingAttributes = await _specificationAttributeService
                       .GetProductSpecificationAttributesAsync(product.Id, searchModel.SpecificationAttributeOptionId);


                    var defaultProductPicture = (await pictureService.GetPicturesByProductIdAsync(product.Id, 1)).FirstOrDefault();
                    var (pictureUrl, _) = await pictureService.GetPictureUrlAsync(defaultProductPicture, 75);

                    return new CategorySpecificationOptionProductAddPopupModel
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Sku = product.Sku,
                        Published = product.Published,
                        DisplayOrder = productCategory.DisplayOrder,
                        PictureThumbnailUrl = pictureUrl,
                        Selected = existingAttributes != null && existingAttributes.Any()
                    };
                });
            });


        }


        #endregion

        #region Products
        public virtual async Task<AddProductToCategoryListModel> CustomPrepareAddProductToCategoryListModelAsync(AddProductToCategorySearchModel searchModel)
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
            var model = await new AddProductToCategoryListModel().PrepareToGridAsync(searchModel, products, () =>
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

        public virtual async Task<CategoryProductListModel> CustomPrepareCategoryProductListModelAsync(CategoryProductSearchModel searchModel, Category category)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (category == null)
                throw new ArgumentNullException(nameof(category));

            //get product categories
            var productCategories = await _categoryService.GetProductCategoriesByCategoryIdAsync(category.Id,
                showHidden: true,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);
            var _pictureService = EngineContext.Current.Resolve<IPictureService>();
            //prepare grid model
            var model = await new CategoryProductListModel().PrepareToGridAsync(searchModel, productCategories, () =>
            {
                return productCategories.SelectAwait(async productCategory =>
                {
                    //fill in model values from the entity
                    var categoryProductModel = productCategory.ToModel<CategoryProductModel>();

                    //fill in additional values (not existing in the entity)
                    categoryProductModel.ProductName = (await _productService.GetProductByIdAsync(productCategory.ProductId))?.Name;
                    var defaultProductPicture = (await _pictureService.GetPicturesByProductIdAsync(productCategory.ProductId, 1)).FirstOrDefault();
                    (categoryProductModel.PictureThumbnailUrl, _) = await _pictureService.GetPictureUrlAsync(defaultProductPicture, 75);
                    return categoryProductModel;
                });
            });

            return model;
        }
        #endregion

        #region Suggested Keyword
        public virtual async Task<CategorySuggestedKeywordListModel> CustomPrepareSuggestedKeywordListModelAsync(CategorySuggestedKeywordSearchModel searchModel, int categoryid)
        {
            var _suggestedKeywordsService = EngineContext.Current.Resolve<ISuggestedKeywordsService>();
            try
            {


                if (searchModel == null)
                    throw new ArgumentNullException(nameof(searchModel));

                if (categoryid == 0)
                    throw new ArgumentNullException("Please Provide CategoryId");

                //get product categories
                var suggestedKeyWords = await _suggestedKeywordsService.GetCategorySuggestedKeywords(categoryid,
                   pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

                //prepare grid model
                var model = await new CategorySuggestedKeywordListModel().PrepareToGridAsync(searchModel, suggestedKeyWords, () =>
                {
                    return suggestedKeyWords.SelectAwait(async suggestedKeyWord =>
                    {
                        CategorySuggestedKeywordModel model = new CategorySuggestedKeywordModel();
                        model.CategoryId = suggestedKeyWord.CategoryId;
                        model.KeyWord = suggestedKeyWord.KeyWord;
                        model.Id = suggestedKeyWord.Id;
                        return model;
                    });
                });

                return model;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<CategorySuggestedKeywordModel> GetCategorySuggestedKeywordById(int id)
        {
            var _suggestedKeywordsService = EngineContext.Current.Resolve<ISuggestedKeywordsService>();

            var suggestedKeyWord = await _suggestedKeywordsService.GetCategorySuggestedKeywordById(id);
            CategorySuggestedKeywordModel model = new CategorySuggestedKeywordModel();
            model.CategoryId = suggestedKeyWord.CategoryId;
            model.KeyWord = suggestedKeyWord.KeyWord;
            model.Id = suggestedKeyWord.Id;
            return model;
        }

        public async Task CreateCategorySuggestedKeyword(CategorySuggestedKeywordModel categorySuggestedKeywordModel)
        {
            var _suggestedKeywordsService = EngineContext.Current.Resolve<ISuggestedKeywordsService>();

            CategorySuggestedKeyword categorySuggestedKeyword = new CategorySuggestedKeyword()
            {
                Id = categorySuggestedKeywordModel.Id,
                CategoryId = categorySuggestedKeywordModel.CategoryId,
                KeyWord = categorySuggestedKeywordModel.KeyWord,
            };

            await _suggestedKeywordsService.CreateCategorySuggestedKeyword(categorySuggestedKeyword);

        }

        public async Task UpdateCategorySuggestedKeyword(CategorySuggestedKeywordModel categorySuggestedKeywordModel)
        {
            var _suggestedKeywordsService = EngineContext.Current.Resolve<ISuggestedKeywordsService>();

            CategorySuggestedKeyword categorySuggestedKeyword = new CategorySuggestedKeyword()
            {
                Id = categorySuggestedKeywordModel.Id,
                CategoryId = categorySuggestedKeywordModel.CategoryId,
                KeyWord = categorySuggestedKeywordModel.KeyWord,
            };
            await _suggestedKeywordsService.UpdateCategorySuggestedKeyword(categorySuggestedKeyword);

        }

        public async Task DeleteCategorySuggestedKeyword(CategorySuggestedKeywordModel categorySuggestedKeywordModel)
        {
            var _suggestedKeywordsService = EngineContext.Current.Resolve<ISuggestedKeywordsService>();

            CategorySuggestedKeyword categorySuggestedKeyword = new CategorySuggestedKeyword()
            {
                Id = categorySuggestedKeywordModel.Id,
                CategoryId = categorySuggestedKeywordModel.CategoryId,
                KeyWord = categorySuggestedKeywordModel.KeyWord,
            };
            await _suggestedKeywordsService.DeleteCategorySuggestedKeyword(categorySuggestedKeyword);
        }

        public async Task<bool> IsCategoryKeyWordExist(int keywordId, string keyWord)
        {
            var _suggestedKeywordsService = EngineContext.Current.Resolve<ISuggestedKeywordsService>();
            return await _suggestedKeywordsService.IsCategoryKeywordExist(keywordId, keyWord);
        }
        #endregion
    }


}
