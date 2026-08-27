using MWT.Nop.Core.Service;
using MWT.Nop.Core.Services.Catalog;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Media;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Web.Areas.Admin.Factories
{
    public partial class SpecificationAttributeModelFactory : ISpecificationAttributeModelFactory
    {
        #region Methods
        public virtual async Task<SpecificationAttributeOptionProductListModel> PrepareSpecificationAttributeOptionProductListModelAsync(
        SpecificationAttributeOptionProductSearchModel searchModel, SpecificationAttributeOption specificationAttributeOption)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (specificationAttributeOption == null)
                throw new ArgumentNullException(nameof(specificationAttributeOption));

            //get products
            var _customSpecificationAttributeService = EngineContext.Current.Resolve<ICustomSpecificationAttributeService>();
            var products = await _customSpecificationAttributeService.GetProductsSearchBySpecificationAttributeOptionIdAsync(
                specificationAttributeOptionId: specificationAttributeOption.Id,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize, search: searchModel.Search,
                categoryId: searchModel.SearchCategoryId);

            var _productService = EngineContext.Current.Resolve<IProductService>();
            var _pictureService = EngineContext.Current.Resolve<IPictureService>();
            //prepare list model
            var model = await new SpecificationAttributeOptionProductListModel().PrepareToGridAsync(searchModel, products, () =>
            {
                //fill in model values from the entity
                return products.SelectAwait(async product =>
                {
                    var _product = await _productService.GetProductByIdAsync(product.ProductId);
                    var specificationAttributeOptionProductModel = new SpecificationAttributeOptionProductModel();
                    specificationAttributeOptionProductModel.ProductId = product.ProductId;
                    specificationAttributeOptionProductModel.DisplayOrder = product.DisplayOrder;
                    specificationAttributeOptionProductModel.MobileDisplayOrder = product.MobileDisplayOrder;
                    specificationAttributeOptionProductModel.ProductName = _product.Name;
                    specificationAttributeOptionProductModel.SpecificationAttributeOptionId = product.SpecificationAttributeOptionId;
                    specificationAttributeOptionProductModel.Id = product.Id;

                    var defaultProductPicture = (await _pictureService.GetPicturesByProductIdAsync(product.ProductId, 1)).FirstOrDefault();
                    (specificationAttributeOptionProductModel.PictureThumbnailUrl, _) = await _pictureService.GetPictureUrlAsync(defaultProductPicture, 75);
                    return specificationAttributeOptionProductModel;
                });
            });

            return model;
        }
        public virtual async Task<SpecificationAttributeOptionModel> CustomPrepareSpecificationAttributeOptionModelAsync(SpecificationAttributeOptionModel model,
       SpecificationAttribute specificationAttribute,
    SpecificationAttributeOption specificationAttributeOption,
     bool excludeProperties = false)
        {


            Func<SpecificationAttributeOptionLocalizedModel, int, Task> localizedModelConfiguration = null;

            if (specificationAttributeOption != null)
            {
                //fill in model values from the entity
                model ??= specificationAttributeOption.ToModel<SpecificationAttributeOptionModel>();

                model.EnableColorSquaresRgb = !string.IsNullOrEmpty(specificationAttributeOption.ColorSquaresRgb);

                //define localized model configuration action
                localizedModelConfiguration = async (locale, languageId) =>
                {
                    locale.Name = await _localizationService.GetLocalizedAsync(specificationAttributeOption, entity => entity.Name, languageId, false, false);
                };
            }

            model.SpecificationAttributeId = specificationAttribute.Id;

            //prepare localized models
            if (!excludeProperties)
                model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);
            SpecificationAttributeOptionProductSearchModel searchModel = new SpecificationAttributeOptionProductSearchModel();
            searchModel.SetGridPageSize();
            await _baseAdminModelFactory.PrepareCategoriesAsync(searchModel.AvailableCategories);
            model.SpecificationAttributeOptionProductSearchModel = searchModel;
            return model;
        }


        public virtual async Task<SpecificationAttributeModel> CustomPrepareSpecificationAttributeModelAsync(SpecificationAttributeModel model,
    SpecificationAttribute specificationAttribute, bool excludeProperties = false)
        { 
            Func<SpecificationAttributeLocalizedModel, int, Task> localizedModelConfiguration = null;
            if (specificationAttribute != null)
            {
                //fill in model values from the entity
                model ??= specificationAttribute.ToModel<SpecificationAttributeModel>();

                //prepare nested search models
                PrepareSpecificationAttributeOptionSearchModel(model.SpecificationAttributeOptionSearchModel, specificationAttribute);
                await CustomPrepareSpecificationAttributeProductSearchModel(model.SpecificationAttributeProductSearchModel, specificationAttribute);

                //define localized model configuration action
                localizedModelConfiguration = async (locale, languageId) =>
                {
                    locale.Name = await _localizationService.GetLocalizedAsync(specificationAttribute, entity => entity.Name, languageId, false, false);
                };
            }

            //prepare localized models
            if (!excludeProperties)
            {
                model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);

                await _baseAdminModelFactory.PrepareSpecificationAttributeGroupsAsync(model.AvailableGroups,
                    defaultItemText: await _localizationService.GetResourceAsync("Admin.Catalog.Attributes.SpecificationAttributes.SpecificationAttribute.Fields.SpecificationAttributeGroup.None"));
            }

            return model;
        }



        public virtual async Task<SpecificationAttributeProductListModel> CustomPrepareSpecificationAttributeProductListModelAsync(
       SpecificationAttributeProductSearchModel searchModel, SpecificationAttribute specificationAttribute)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (specificationAttribute == null)
                throw new ArgumentNullException(nameof(specificationAttribute));

            //get products
            var _customSpecificationAttributeService = EngineContext.Current.Resolve<ICustomSpecificationAttributeService>();
            var products = await _customSpecificationAttributeService.CustomGetProductsBySpecificationAttributeIdAsync(
                specificationAttributeId: specificationAttribute.Id,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize, search: searchModel.Search, categoryId: searchModel.SearchCategoryId);
            var _pictureService = EngineContext.Current.Resolve<IPictureService>();
            //prepare list model
            var model = await new SpecificationAttributeProductListModel().PrepareToGridAsync(searchModel, products, () =>
             {
                 //fill in model values from the entity
                 return products.SelectAwait(async product =>
                  {
                      var specificationAttributeProductModel = product.ToModel<SpecificationAttributeProductModel>();
                      specificationAttributeProductModel.ProductId = product.Id;
                      specificationAttributeProductModel.ProductName = product.Name;
                      specificationAttributeProductModel.OptionName = product.Sku;
                      specificationAttributeProductModel.SpecificationOptionId = product.ProductTemplateId;
                      specificationAttributeProductModel.SpecificationAttributeId = specificationAttribute.Id;
                      var defaultProductPicture = (await _pictureService.GetPicturesByProductIdAsync(product.Id, 1)).FirstOrDefault();
                      (specificationAttributeProductModel.PictureThumbnailUrl, _) = await _pictureService.GetPictureUrlAsync(defaultProductPicture, 75);
                      return specificationAttributeProductModel;
                  });
             });

            return model;
        }
        public virtual async Task<SpecificationAttributeListModel> CustomPrepareCategorySpecificationAttributeListModelAsync(string entityType, int categoryId, SpecificationAttributeSearchModel searchModel, SpecificationAttributeGroup group)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));
            var specificationAttributes = (await _specificationAttributeService.GetSpecificationAttributesByGroupIdAsync(group?.Id)).ToPagedList(searchModel);
            var _filtersMappingByEntityService = EngineContext.Current.Resolve<IFiltersMappingByEntityService>();
            var model = await new SpecificationAttributeListModel().PrepareToGridAsync(searchModel, specificationAttributes, () =>
            {
                return specificationAttributes.SelectAwait(async attribute =>
                {
                    var specificationAttributeModel = attribute.ToModel<SpecificationAttributeModel>();
                    var filterMapping = await _filtersMappingByEntityService.GetFilterMapping(entityType, categoryId, attribute.Id, "SpecificationAttribute");
                    specificationAttributeModel.Disabled = !filterMapping?.Disabled ?? true;
                    specificationAttributeModel.DisplayOnTop = filterMapping?.DisplayOnTop ?? false;
                    specificationAttributeModel.DisplayOrder = filterMapping?.DisplayOrder ?? 0;
                    specificationAttributeModel.CategoryId = categoryId;
                    return specificationAttributeModel;
                });
            });
            return model;
        }

        public virtual async Task<SpecificationAttributeOptionListModel> CustomPrepareCategorySpecificationAttributeOptionListModelAsync(string entityType,
           SpecificationAttributeOptionSearchModel searchModel, SpecificationAttribute specificationAttribute)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));
            if (specificationAttribute == null)
                throw new ArgumentNullException(nameof(specificationAttribute));
            var options = (await _specificationAttributeService
                .GetSpecificationAttributeOptionsBySpecificationAttributeAsync(specificationAttribute.Id)).ToPagedList(searchModel);
            var _filtersMappingByEntityService = EngineContext.Current.Resolve<IFiltersMappingByEntityService>();
            var model = await new SpecificationAttributeOptionListModel().PrepareToGridAsync(searchModel, options, () =>
            {
                return options.SelectAwait(async option =>
                {
                    var optionModel = option.ToModel<SpecificationAttributeOptionModel>();
                    var GetFiltersMappingExist = await _filtersMappingByEntityService.GetFilterMapping(entityType, searchModel.CategoryId, option.Id, "SpecificationAttributeOption");
                    optionModel.Disabled = !GetFiltersMappingExist?.Disabled ?? true;
                    optionModel.DisplayOrder = GetFiltersMappingExist?.DisplayOrder ?? 0;

                    var _customSpecificationAttributeService = EngineContext.Current.Resolve<ICustomSpecificationAttributeService>();
                    var ProductsCount = await _customSpecificationAttributeService.CustomGetProductsSpecificationAttributeOptionByCategoryWiseAsync(searchModel.CategoryId, specificationAttributeOptionId: option.Id);
                    optionModel.NumberOfAssociatedProducts = ProductsCount.Count;
                    optionModel.CategoryId = searchModel.CategoryId;
                    return optionModel;
                });
            });
            return model;
        }

        public virtual async Task<SpecificationAttributeOptionProductListModel> CustomPrepareCategorySpecificationOptionUsedByProductsListModelAsync(
        SpecificationAttributeOptionProductSearchModel searchModel, SpecificationAttributeOption specificationAttributeOption)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));
            if (specificationAttributeOption == null)
                throw new ArgumentNullException(nameof(specificationAttributeOption));
            var _customSpecificationAttributeService = EngineContext.Current.Resolve<ICustomSpecificationAttributeService>();
            var products = (await _customSpecificationAttributeService.CustomGetProductsSpecificationAttributeOptionByCategoryWiseAsync(searchModel.SearchCategoryId, specificationAttributeOption.Id
                )).ToPagedList(searchModel);
            var pagedProducts = new PagedList<ProductSpecificationAttribute>(
               products, searchModel.Page - 1, searchModel.PageSize);
            var _productService = EngineContext.Current.Resolve<IProductService>();
            var _pictureService = EngineContext.Current.Resolve<IPictureService>();
            var model = await new SpecificationAttributeOptionProductListModel().PrepareToGridAsync(searchModel, products, () =>
            {
                return products.SelectAwait(async product =>
                {
                    var _product = await _productService.GetProductByIdAsync(product.ProductId);
                    var specificationAttributeOptionProductModel = new SpecificationAttributeOptionProductModel();
                    specificationAttributeOptionProductModel.ProductId = product.ProductId;
                    specificationAttributeOptionProductModel.DisplayOrder = product.DisplayOrder;
                    specificationAttributeOptionProductModel.MobileDisplayOrder = product.MobileDisplayOrder;
                    specificationAttributeOptionProductModel.ProductName = _product.Name;
                    specificationAttributeOptionProductModel.SpecificationAttributeOptionId = product.SpecificationAttributeOptionId;
                    specificationAttributeOptionProductModel.Id = product.Id;

                    var defaultProductPicture = (await _pictureService.GetPicturesByProductIdAsync(product.ProductId, 1)).FirstOrDefault();
                    (specificationAttributeOptionProductModel.PictureThumbnailUrl, _) = await _pictureService.GetPictureUrlAsync(defaultProductPicture, 75);
                    return specificationAttributeOptionProductModel;
                });
            });
            return model;
        }
        #endregion

        #region Utilities
        protected virtual async Task<SpecificationAttributeProductSearchModel> CustomPrepareSpecificationAttributeProductSearchModel(
    SpecificationAttributeProductSearchModel searchModel, SpecificationAttribute specificationAttribute)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (specificationAttribute == null)
                throw new ArgumentNullException(nameof(specificationAttribute));

            searchModel.SpecificationAttributeId = specificationAttribute.Id;
            await _baseAdminModelFactory.PrepareCategoriesAsync(searchModel.AvailableCategories);
            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }
        #endregion



    }
}
