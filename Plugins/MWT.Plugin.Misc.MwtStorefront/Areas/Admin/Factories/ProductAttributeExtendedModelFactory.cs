using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Services.Catalog;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the product attribute model factory implementation
    /// </summary>
    public partial class ProductAttributeExtendedModelFactory  : ProductAttributeModelFactory, IProductAttributeExtendedModelFactory
    {
        private readonly ICustomProductAttributeService _customProductAttributeService;
        private readonly IProductExtendedService  _customProductService;

        public ProductAttributeExtendedModelFactory(IBaseAdminModelFactory baseAdminModelFactory, ILocalizationService localizationService, ILocalizedModelFactory localizedModelFactory, IProductAttributeService productAttributeService, IProductService productService
            , ICustomProductAttributeService customProductAttributeService , IProductExtendedService customProductService) : base(baseAdminModelFactory, localizationService, localizedModelFactory, productAttributeService, productService)
        {
            _customProductAttributeService = customProductAttributeService;
            _customProductService = customProductService;
        }


        #region Methods
        public virtual async Task<PredefinedProductAttributeValueListModel> Stains(
         PredefinedProductAttributeValueSearchModel searchModel, ProductAttribute productAttribute)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (productAttribute == null)
                throw new ArgumentNullException(nameof(productAttribute));

            //get predefined product attribute values
            var values = (await _customProductAttributeService.Stains(productAttribute.Id, searchModel.SearchTerm)).ToPagedList(searchModel);

            //prepare list model
            var _fileProvider = EngineContext.Current.Resolve<INopFileProvider>();
            var model = new PredefinedProductAttributeValueListModel().PrepareToGrid(searchModel, values, () =>
            {
                return values.Select(value =>
                {
                    //fill in model values from the entity

                    var predefinedProductAttributeValueModel = new PredefinedProductAttributeValueModel();

                    predefinedProductAttributeValueModel.Name = value;


                    //Image code
                    var imagePath = Path.Combine("wwwroot", "images", "shades", "large", $"{value}.jpg");
                    var fileInfo = _fileProvider.GetFileInfo(imagePath);

                    if (fileInfo.Exists)
                    {
                        DateTime date = DateTime.Now;
                        predefinedProductAttributeValueModel.Image = $"~/images/shades/large/{value}.jpg?Date={date.ToString("yyyy-MM-ddTHH:mm:ss")}";
                    }
                    else
                    {
                        predefinedProductAttributeValueModel.Image = "NA";
                    }

                    return predefinedProductAttributeValueModel;
                });
            });

            return model;
        }


        public virtual async Task<ProductAttributeModel> CustomPrepareProductAttributeModelAsync(ProductAttributeModel model,
          ProductAttribute productAttribute, bool excludeProperties = false)
        {
            Func<ProductAttributeLocalizedModel, int , Task> localizedModelConfiguration = null;

            if (productAttribute != null)
            {
                //fill in model values from the entity
                model ??= productAttribute.ToModel<ProductAttributeModel>();

                //prepare nested search models
                PreparePredefinedProductAttributeValueSearchModel(model.PredefinedProductAttributeValueSearchModel, productAttribute);
                await CustomPrepareProductAttributeProductSearchModel(model.ProductAttributeProductSearchModel, productAttribute);

                //define localized model configuration action
                localizedModelConfiguration = async (locale, languageId) =>
                {
                    locale.Name = await _localizationService.GetLocalizedAsync(productAttribute, entity => entity.Name, languageId, false, false);
                    locale.Description = await _localizationService.GetLocalizedAsync(productAttribute, entity => entity.Description, languageId, false, false);
                };
            }

            //prepare localized models
            if (!excludeProperties)
                model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);

            return model;
        }



        public virtual async Task<ProductAttributeProductListModel> CustomPrepareProductAttributeProductListModelAsync(ProductAttributeProductSearchModel searchModel,
        ProductAttribute productAttribute)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (productAttribute == null)
                throw new ArgumentNullException(nameof(productAttribute));

            //get products
            var products = await _customProductService.CustomGetProductsByProductAtributeIdAsync(productAttributeId: productAttribute.Id,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize, search: searchModel.Search, categoryId: searchModel.SearchCategoryId);
            var _pictureService = EngineContext.Current.Resolve<IPictureService>();
            //prepare list model
            var model = await new ProductAttributeProductListModel().PrepareToGridAsync(searchModel, products, () =>
            {
                //fill in model values from the entity
                return products.SelectAwait(async product =>
                {
                    var productAttributeProductModel = product.ToModel<ProductAttributeProductModel>();
                    productAttributeProductModel.ProductName = product.Name;
                    productAttributeProductModel.ProductId = product.Id;
                    var defaultProductPicture = (await _pictureService.GetPicturesByProductIdAsync(product.Id, 1)).FirstOrDefault();
                    (productAttributeProductModel.PictureThumbnailUrl, _) = await _pictureService.GetPictureUrlAsync(defaultProductPicture, 75);
                    return productAttributeProductModel;
                });
            });

            return model;
        }

        #endregion

        #region Utilities
        protected virtual async Task<ProductAttributeProductSearchModel> CustomPrepareProductAttributeProductSearchModel(ProductAttributeProductSearchModel searchModel,
    ProductAttribute productAttribute)
        {
            var _baseAdminModelFactory = EngineContext.Current.Resolve<IBaseAdminModelFactory>();
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (productAttribute == null)
                throw new ArgumentNullException(nameof(productAttribute));

            searchModel.ProductAttributeId = productAttribute.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();
            await _baseAdminModelFactory.PrepareCategoriesAsync(searchModel.AvailableCategories);
            return searchModel;
        }

        #endregion
    }
}
