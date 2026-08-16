using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Services.Catalog;
using MWT.Plugin.Misc.MwtStorefront.Models.Catalog;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Seo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Factories
{
    public partial class ElasticSearchModelFactory : IElasticSearchModelFactory
    {
        #region Fields


        private readonly IProductExtendedService _productService;
        private readonly ICustomProductModelFactory _productModelFactory;
        private readonly ICustomSpecificationAttributeService _specificationAttributeService;
        private readonly ISuggestedKeywordsService _suggestedwordsService;
        private readonly ICategoryService _categoryService;
        private readonly IUrlRecordService _urlRecordService;
        #endregion

        #region Ctor

        public ElasticSearchModelFactory(IProductExtendedService productService, ICustomProductModelFactory productModelFactory,
                                         ICustomSpecificationAttributeService specificationAttributeService, ISuggestedKeywordsService suggestedwordsService,
                                         ICategoryService categoryService, IUrlRecordService urlRecordService)
        {
            this._productService = productService;
            this._productModelFactory = productModelFactory;
            this._specificationAttributeService = specificationAttributeService;
            this._suggestedwordsService = suggestedwordsService;
            this._categoryService = categoryService;
            this._urlRecordService = urlRecordService;
        }

        #endregion

        #region Methods
        public virtual async Task<CustomProductOverviewModel> PrepareProductModelForElasticSearch(int productId)
        {
            CustomProductOverviewModel model = new CustomProductOverviewModel();
            var product = await _productService.GetProductByIdAsync(productId);
            if (!product.Published || product.Deleted)
                return null;
            List<Product> products = new List<Product>();
            products.Add(product);
            model = (await _productModelFactory.PrepareCustomProductOverviewModelsAsync(products: products, prepareShades: true, prepareAlternatePictureModel: true, productThumbPictureSize: null, isCategorypage: true)).FirstOrDefault();
            if (model == null)
                return null;
            await PrepareSpecificationAttributeForProduct(productId, model);
            model.suggestedwords = await PrepareSuggestedWordsModelForProduct(productId);

            return model;
        }

        public virtual async Task<CustomProductOverviewModel> PrepareProductModelForElasticSearch(Product product)
        {
            CustomProductOverviewModel model = new CustomProductOverviewModel();
            if (product.Deleted)
                return null;
            List<Product> products = new List<Product>();
            products.Add(product);
            model = (await _productModelFactory.PrepareCustomProductOverviewModelsAsync(products: products, prepareShades: true, prepareAlternatePictureModel: true, productThumbPictureSize: null, isCategorypage: true)).FirstOrDefault();
            if (model == null)
                return null;
            await PrepareSpecificationAttributeForProduct(product.Id, model);
            model.suggestedwords = await PrepareSuggestedWordsModelForProduct(product.Id);
            model.entities = await PrepareCategoriesModelForProduct(product.Id);
            return model;
        }

        #endregion

        #region Utilities
        private async Task<List<EsEntitytModel>> PrepareCategoriesModelForProduct(int productId)
        {
            List<EsEntitytModel> _entities = new List<EsEntitytModel>();
            var prdCategories = await _categoryService.GetProductCategoriesByProductIdAsync(productId);
            foreach (var prdCategory in prdCategories)
            {
                var category = await _categoryService.GetCategoryByIdAsync(prdCategory.CategoryId);
                if (category != null && category.Published && !category.Deleted)
                {
                    var parentCategory = new Category();
                    if (category.ParentCategoryId != 0)
                        parentCategory = await _categoryService.GetCategoryByIdAsync(category.ParentCategoryId);
                    EsEntitytModel esEntitytModel = new EsEntitytModel();
                    esEntitytModel.Published = category.Published;
                    esEntitytModel.DisplayOrder = category.DisplayOrder;
                    esEntitytModel.ParentEntityID = "cat-" + category.ParentCategoryId.ToString();
                    esEntitytModel.ParentEntityName = parentCategory == null ? "" : parentCategory.Name;
                    esEntitytModel.EntityID = "cat-" + category.Id.ToString();
                    esEntitytModel.EntityName = category.Name;
                    esEntitytModel.SEKeywords = category.MetaKeywords;
                    esEntitytModel.EntityType = "Category";
                    esEntitytModel.ShowOutOfStock = true;
                    esEntitytModel.SEName = await _urlRecordService.GetSeNameAsync(category.Id, "Category", languageId: null, true);
                    _entities.Add(esEntitytModel);
                }
            }
            return _entities;
        }
        private async Task PrepareSpecificationAttributeForProduct(int productId, CustomProductOverviewModel model)
        {
            var _attrs = await _specificationAttributeService.GetAllSpecificationAttributesAsync();
            var _specificationAttrOptions = await _specificationAttributeService.GetAllSpecificationOptionsAsync();
            var _prdSpecificationOptions = await _specificationAttributeService.GetProductSpecificationAttributesAsync(productId: productId, allowFiltering: true);

            List<EsProductSpecificationAttributesModel> _filters =
                new List<EsProductSpecificationAttributesModel>();

            foreach (var _option in _prdSpecificationOptions)
            {
                var _optionObj = _specificationAttrOptions.Where(o => o.Id == _option.SpecificationAttributeOptionId).FirstOrDefault();
                if (_optionObj == null)
                    continue;
                var _attrObj = _attrs.Where(a => a.Id == _optionObj.SpecificationAttributeId).FirstOrDefault();
                if (_attrObj == null)
                    continue;

                EsProductSpecificationAttributesModel _filter =
                    new EsProductSpecificationAttributesModel();
                _filter.keyword = _optionObj.Name;
                _filter.ParentDisplayOrder = _attrObj.DisplayOrder;
                _filter.DisplayOrder = _optionObj.DisplayOrder;
                _filter.Published = true;
                _filter.SpecificationAttributeOptionID = "sec-" + _optionObj.Id;
                _filter.ColorSquaresRgb = _optionObj.ColorSquaresRgb;
                _filter.Name = _optionObj.Name;
                _filter.ShowOutOfStock = true;
                _filter.ParentName = _attrObj.Name;
                _filter.SpecificationAttributeID = "sec-" + _attrObj.Id;
                _filters.Add(_filter);
            }

            model.filters = _filters;
        }
        private async Task<List<EsProductSuggestedKeyWordModel>> PrepareSuggestedWordsModelForProduct(int productId)
        {
            var _words = await _suggestedwordsService.GetSuggestedKeywordsForProduct(productId);
            List<EsProductSuggestedKeyWordModel>
                    _lstSuggestedWords = new List<EsProductSuggestedKeyWordModel>();
            foreach (var _word in _words)
            {
                _lstSuggestedWords.Add(new EsProductSuggestedKeyWordModel()
                {
                    ID = _word.Id,
                    SuggestedKeywords = _word.SuggestedKeywords
                });
            }
            return _lstSuggestedWords;
        }

        #endregion
    }
}
