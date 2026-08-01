//using MWT.Nop.Core.Infrastructure;
//using MWT.Nop.Core.Service.Catalog;
//using MWT.Nop.Core.Service.StoreWideDiscount;
//using MWT.Nop.Core.Services.Catalog;
//using MWT.Plugin.Misc.MwtStorefront.Models.ProductApi;
//using Nop.Core.Domain.Catalog;
//using Nop.Services.Catalog;
//using Nop.Services.Configuration;
//using Nop.Services.Localization;
//using Nop.Services.Seo;
//using Nop.Web.Areas.Admin.Factories;
//using Nop.Web.Areas.Admin.Models.Catalog;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace MWT.Plugin.Misc.MwtStorefront.Factories.Catalog
//{
//    public class FeedModelFactory : IFeedModelFactory
//    {
//        private readonly ISettingService _settingService;
//        private readonly IStoreWideDiscountService _storeWideDiscountService;
//        private readonly IFeedService _feedService;
//        private readonly ILocalizationService _localizationService;
//        private readonly IUrlRecordService _urlRecordService;
//        private readonly ICustomProductService _productService;
//        private readonly IManufacturerService _manufacturerService;
//        private readonly ICategoryService _categoryService;
//        private readonly IProductModelFactory _productmodelfactory;
//        public FeedModelFactory(ISettingService settingService, IFeedService feedService, ILocalizationService localizationService
//            , IStoreWideDiscountService storeWideDiscountService, IUrlRecordService urlRecordService, ICustomProductService productService,
//            IManufacturerService manufacturerService,
//            ICategoryService categoryService, IProductModelFactory productmodelfactory)
//        {
//            _settingService = settingService;
//            _feedService = feedService;
//            _localizationService = localizationService;
//            _storeWideDiscountService = storeWideDiscountService;
//            _urlRecordService = urlRecordService;
//            _productService = productService;
//            _manufacturerService = manufacturerService;
//            _categoryService = categoryService;
//            _productmodelfactory = productmodelfactory;
//        }
//        public async Task<List<Models.ProductApi.ProductModel>> PrepareProductFeed(int pageNumber, int pageSize)
//        {

//            var products = await _feedService.ProductsFeedAsync(pageNumber - 1, pageSize);
//            var categorySeprator = await _localizationService.GetResourceAsync("Product.Feed.Category.Seprator");
//            var excludedCategories = await _settingService.GetSettingByKeyAsync<string>("Product.Feed.Category.Excluded");

//            List<int> lstexcludedCategories = new List<int>();
//            foreach (var category in excludedCategories.Split(','))
//            {
//                int.TryParse(category, out int categoryId);
//                if (categoryId != 0)
//                    lstexcludedCategories.Add(categoryId);
//            }


//            List<Models.ProductApi.ProductModel> lstModel = new List<Models.ProductApi.ProductModel>();
//            foreach (var product in products)
//            {

//                Models.ProductApi.ProductModel model = new Models.ProductApi.ProductModel();

//                var categories = await _categoryService.GetProductCategoriesByProductIdAsync(product.Id);
//                List<Models.ProductApi.ProductModel.CategoryModel> lstCategoriesModel = new List<Models.ProductApi.ProductModel.CategoryModel>();
//                foreach (var category in categories)
//                {
//                    Models.ProductApi.ProductModel.CategoryModel categoryModel = new
//                         Models.ProductApi.ProductModel.CategoryModel();
//                    var categoryObject = await _categoryService.GetCategoryByIdAsync(category.CategoryId);
//                    if (categoryObject != null)
//                    {

//                        categoryModel.Id = categoryObject.Id;
//                        categoryModel.Name = categoryObject.Name;
//                        categoryModel.Sename = await _urlRecordService.GetSeNameAsync(categoryObject);
//                        categoryModel.Path = await this.CategoryPath(categorySeprator, categoryObject.Id,
//                            categoryObject.Name, categoryObject.ParentCategoryId, lstexcludedCategories);
//                        lstCategoriesModel.Add(categoryModel);
//                    }

//                }

//                #region Individual Products

//                model.IndividualProducts = string.Join(',', await _productService.GetPairWithProductsByProductId1Async(product.Id));

//                #endregion

//                var prdManufacturers = await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id, true);
//                if (prdManufacturers.Count > 0)
//                {
//                    var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(prdManufacturers.FirstOrDefault().ManufacturerId);
//                    model.Manufacturer = manufacturer.Name;
//                }

//                model.Categories = lstCategoriesModel;

//                var Attributes = await PrepareProductSpecificationAttributeModelAsync(product, null);
//                foreach (var attr in Attributes)
//                {
//                    if (attr.Name.Equals("Froggle_Description", StringComparison.CurrentCultureIgnoreCase))
//                        model.FullDescription = attr.Values.FirstOrDefault()?.ValueRaw;
//                    else if (attr.Name.Equals("DIMENSIONS", StringComparison.CurrentCultureIgnoreCase))
//                        model.Dimensions = attr.Values.FirstOrDefault()?.ValueRaw;
//                }

//                var groups = await _specificationAttributeService.GetSpecificationAttributeGroupsAsync(0, int.MaxValue);
//                var specifications = new List<Models.ProductApi.ProductModel.CustomProductSpecificationModel>();
//                foreach (var group in groups)
//                {
//                    if (group.Id == 4)
//                    {
//                        Attributes = await CustomfeedPrepareProductSpecificationAttributeModelAsync(product, group);
//                        foreach (var attr in Attributes)
//                        {
//                            specifications.Add(new Models.ProductApi.ProductModel.CustomProductSpecificationModel()
//                            {
//                                Name = attr.Name,
//                                Value = attr.Values.FirstOrDefault()?.ValueRaw
//                            });
//                            if (attr.Name == "Froggle_Description")
//                                model.FullDescription = attr.Values.FirstOrDefault()?.ValueRaw;
//                            else if (attr.Name == "Main Category Id")
//                            {
//                                int.TryParse(attr.Values.FirstOrDefault()?.ValueRaw, out int mainCategoryId);
//                                if (mainCategoryId != 0)
//                                {
//                                    var mainCategory = await _categoryService.GetCategoryByIdAsync(mainCategoryId);
//                                    if (mainCategory != null)
//                                    {
//                                        model.MainCategory = new Models.Customizations.Custom.ProductApi.ProductModel.CategoryModel()
//                                        {
//                                            Id = mainCategory.Id,
//                                            Name = mainCategory.Name,
//                                            Sename = await _urlRecordService.GetSeNameAsync(mainCategory),
//                                            Path = await this.CategoryPath(categorySeprator, mainCategory.Id, mainCategory.Name, mainCategory.ParentCategoryId, lstexcludedCategories)
//                                        };
//                                    }
//                                }
//                            }
//                        }
//                    }
//                }

//                model.Gtin = product.Gtin;
//                model.Id = product.Id;
//                model.ManufacturerPartNumber = product.ManufacturerPartNumber;
//                model.MetaDescription = product.MetaDescription;
//                model.MetaKeywords = product.MetaKeywords;
//                model.MetaTitle = product.MetaTitle;
//                model.Name = product.Name;
//                IList<ProductPictureModel> allPictureModels;
//                (_, allPictureModels) = await CustomPfeedrepareProductDetailsPictureModelAsync(product, false);
//                model.PictureModels = allPictureModels;
//                var productPrice = await PrepareCustomProductPriceModelAsync(product);
//                model.ProductPrice = new Models.ProductApi.ProductModel.ProductPriceModel()
//                {
//                    MsrpValue = productPrice.MsrpValue,
//                    OldPriceValue = productPrice.OldPriceValue,
//                    PriceValue = productPrice.PriceValue
//                };
//                model.Specifications = specifications;
//                int[] lastRelatedIds = (await _productService.GetRelatedProductsByProductId1Async(product.Id)).Select(r => r.ProductId2).ToArray();

//                List<string> relatedProducts = new List<string>();
//                foreach (var productid in lastRelatedIds)
//                {
//                    var variantId = await this.GetProductVariantId(productid);
//                    if (variantId != 0)
//                        relatedProducts.Add(productid + "-" + variantId + "--");
//                }


//                model.RelatedProducts = relatedProducts;
//                model.SeName = await _urlRecordService.GetSeNameAsync(product);
//                model.ShippingCost = product.ShippingPrice;
//                model.ShortDescription = product.ShortDescription;
//                model.Sku = await GetProductSku(product.Sku, product.Id);
//                model.TotaLinventory = product.TotalInventory;



//                #region Variant
//                List<Models.Customizations.Custom.ProductApi.ProductModel.Variant> Variants = new List<Models.Customizations.Custom.ProductApi.ProductModel.Variant>();


//                var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
//                foreach (var attribute in productAttributeMapping)
//                {
//                    var productAttrubute = (await _productAttributeService.GetProductAttributeByIdAsync(attribute.ProductAttributeId));

//                    if (attribute.ShouldHaveValues() && productAttrubute.Name.Equals(await _localizationService.GetResourceAsync("Product.Attr.Size"), StringComparison.InvariantCultureIgnoreCase))
//                    {
//                        List<CustomProductAttributeCombination> lstCombinations = new List<CustomProductAttributeCombination>();

//                        var combinations = await _productAttributeService.GetAllProductAttributeCombinationsAsync(product.Id);

//                        foreach (var combination in combinations)
//                        {
//                            var values = await _productAttributeParser
//                        .ParseProductAttributeValuesAsync(combination.AttributesXml, attribute.Id);
//                            if (values == null || values.Count == 0)
//                                continue;

//                            lstCombinations.Add(new CustomProductAttributeCombination()
//                            {
//                                Combination = combination,
//                                ValueId = values.FirstOrDefault()?.Id ?? 0

//                            });
//                        }

//                        var attributeValues = (await _productAttributeService.GetProductAttributeValuesAsync(attribute.Id)).OrderBy(o => o.DisplayOrder);
//                        //values
//                        foreach (var attrValue in attributeValues)
//                        {
//                            if (attrValue.VariantId > 0 && attrValue.Published)
//                            {
//                                Models.Customizations.Custom.ProductApi.ProductModel.Variant variant = new Models.Customizations.Custom.ProductApi.ProductModel.Variant();
//                                variant.Name = attrValue.Name;
//                                variant.Id = attrValue.VariantId;
//                                variant.IsDefault = attrValue.IsPreSelected ? true : false;
//                                variant.ManufacturerPartNumber = attrValue.ManufacturerPartNumber;
//                                variant.Weight = attrValue.WeightAdjustment;
//                                variant.VariantTitle = attrValue.VariantTitle;
//                                variant.Dimension = attrValue.Dimension;

//                                variant.QueryParameter = attrValue.QueryParameter;
//                                var combination = lstCombinations.Where(c => c.ValueId == attrValue.Id).FirstOrDefault();
//                                if (combination != null)
//                                {
//                                    variant.MsrpValue = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(combination.Combination.OverriddenMsrp ?? productPrice.MsrpValue, await _workContext.GetWorkingCurrencyAsync());
//                                    variant.OldPriceValue = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(combination.Combination.OverriddenOldPrice ?? productPrice.OldPriceValue, await _workContext.GetWorkingCurrencyAsync());
//                                    variant.PriceValue = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(combination.Combination.OverriddenPrice ?? productPrice.PriceValue, await _workContext.GetWorkingCurrencyAsync());
//                                }
//                                else
//                                {
//                                    variant.MsrpValue = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(productPrice.MsrpValue, await _workContext.GetWorkingCurrencyAsync());

//                                    variant.OldPriceValue = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(productPrice.OldPriceValue, await _workContext.GetWorkingCurrencyAsync());

//                                    variant.PriceValue = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(productPrice.PriceValue, await _workContext.GetWorkingCurrencyAsync());


//                                }

//                                Variants.Add(variant);

//                            }
//                        }
//                        break;
//                    }
//                }
//                model.Variants = Variants;
//                #endregion

//                #region Sale Info

//                var discountInfo = await _storeWideDiscountService.GetProductSaleInfo(model.Id);
//                if (discountInfo != null)
//                {
//                    model.SaleStartDate = discountInfo.StartDate;
//                    model.SaleEndDate = discountInfo.EndDate;
//                }

//                #endregion

//                lstModel.Add(model);
//            }
//            return lstModel;
//        }
//        public async Task<List<ProductModel>> PrepareProductFeedVersion2(int pageNumber, int pageSize)
//        {

//            var products = await _productService.ProductsFeedAsync(pageNumber - 1, pageSize);
//            var categorySeprator = await _localizationService.GetResourceAsync("Product.Feed.Category.Seprator");
//            var excludedCategories = await _settingService.GetSettingByKeyAsync<string>("Product.Feed.Category.Excluded");
//            string sizeAttributeName = await _localizationService.GetResourceAsync("Product.Attr.Size");

//            List<int> lstexcludedCategories = new List<int>();
//            foreach (var category in excludedCategories.Split(','))
//            {
//                int.TryParse(category, out int categoryId);
//                if (categoryId != 0)
//                    lstexcludedCategories.Add(categoryId);
//            }


//            List<ProductModel> lstModel = new List<ProductModel>();
//            foreach (var product in products)
//            {

//                Models.ProductApi.ProductModel model = new ProductModel();

//                var categories = await _categoryService.GetProductCategoriesByProductIdAsync(product.Id);
//                List<ProductModel.CategoryModel> lstCategoriesModel = new List<ProductModel.CategoryModel>();
//                foreach (var category in categories)
//                {
//                    ProductModel.CategoryModel categoryModel = new
//                        ProductModel.CategoryModel();
//                    var categoryObject = await _categoryService.GetCategoryByIdAsync(category.CategoryId);
//                    if (categoryObject != null)
//                    {

//                        categoryModel.Id = categoryObject.Id;
//                        categoryModel.Name = categoryObject.Name;
//                        categoryModel.Sename = await _urlRecordService.GetSeNameAsync(categoryObject);
//                        categoryModel.Path = await this.CategoryPath(categorySeprator, categoryObject.Id,
//                            categoryObject.Name, categoryObject.ParentCategoryId, lstexcludedCategories);
//                        lstCategoriesModel.Add(categoryModel);
//                    }

//                }

//                #region Individual Products

//                model.IndividualProducts = string.Join(',', await _productService.GetPairWithProductsByProductId1Async(product.Id));

//                #endregion

//                var prdManufacturers = await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id, true);
//                if (prdManufacturers.Count > 0)
//                {
//                    var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(prdManufacturers.FirstOrDefault().ManufacturerId);
//                    model.Manufacturer = manufacturer.Name;
//                }

//                model.Categories = lstCategoriesModel;

//                var Attributes = await PrepareProductSpecificationAttributeModelAsync(product, null);
//                foreach (var attr in Attributes)
//                {
//                    if (attr.Name.Equals("Froggle_Description", StringComparison.CurrentCultureIgnoreCase))
//                        model.FroogleDescription = attr.Values.FirstOrDefault()?.ValueRaw;
//                    else if (attr.Name.Equals("DIMENSIONS", StringComparison.CurrentCultureIgnoreCase))
//                        model.Dimensions = attr.Values.FirstOrDefault()?.ValueRaw;
//                }

//                var groups = await _specificationAttributeService.GetSpecificationAttributeGroupsAsync(0, int.MaxValue);
//                var specifications = new List<Models.Customizations.Custom.ProductApi.ProductModel.CustomProductSpecificationModel>();
//                foreach (var group in groups)
//                {
//                    if (group.Id == 4)
//                    {
//                        Attributes = await CustomfeedPrepareProductSpecificationAttributeModelAsync(product, group);
//                        foreach (var attr in Attributes)
//                        {
//                            specifications.Add(new Models.Customizations.Custom.ProductApi.ProductModel.CustomProductSpecificationModel()
//                            {
//                                Name = attr.Name,
//                                Value = attr.Values.FirstOrDefault()?.ValueRaw
//                            });
//                            if (attr.Name == "Froggle_Description")
//                                model.FroogleDescription = attr.Values.FirstOrDefault()?.ValueRaw;
//                            else if (attr.Name == "Main Category Id")
//                            {
//                                int.TryParse(attr.Values.FirstOrDefault()?.ValueRaw, out int mainCategoryId);
//                                if (mainCategoryId != 0)
//                                {
//                                    var mainCategory = await _categoryService.GetCategoryByIdAsync(mainCategoryId);
//                                    if (mainCategory != null)
//                                    {
//                                        model.MainCategory = new Models.Customizations.Custom.ProductApi.ProductModel.CategoryModel()
//                                        {
//                                            Id = mainCategory.Id,
//                                            Name = mainCategory.Name,
//                                            Sename = await _urlRecordService.GetSeNameAsync(mainCategory),
//                                            Path = await this.CategoryPath(categorySeprator, mainCategory.Id, mainCategory.Name, mainCategory.ParentCategoryId, lstexcludedCategories)
//                                        };
//                                    }
//                                }
//                            }
//                        }
//                    }
//                }

//                model.Gtin = product.Gtin;
//                model.FullDescription = product.FullDescription;
//                model.Id = product.Id;
//                model.ManufacturerPartNumber = product.ManufacturerPartNumber;
//                model.MetaDescription = product.MetaDescription;
//                model.MetaKeywords = product.MetaKeywords;
//                model.MetaTitle = product.MetaTitle;
//                model.Name = product.Name;
//                IList<ProductPictureModel> allPictureModels;
//                (_, allPictureModels) = await CustomPfeedrepareProductDetailsPictureModelAsync(product, false);
//                model.PictureModels = allPictureModels;
//                var productPrice = await PrepareCustomProductPriceModelAsync(product);
//                model.ProductPrice = new Models.Customizations.Custom.ProductApi.ProductModel.ProductPriceModel()
//                {
//                    MsrpValue = productPrice.MsrpValue,
//                    OldPriceValue = productPrice.OldPriceValue,
//                    PriceValue = productPrice.PriceValue
//                };
//                model.Specifications = specifications;
//                int[] lastRelatedIds = (await _productService.GetRelatedProductsByProductId1Async(product.Id)).Select(r => r.ProductId2).ToArray();

//                List<string> relatedProducts = new List<string>();
//                foreach (var productid in lastRelatedIds)
//                {
//                    var variantId = await this.GetProductVariantId(productid);
//                    if (variantId != 0)
//                        relatedProducts.Add(productid + "-" + variantId + "--");
//                }


//                model.RelatedProducts = relatedProducts;
//                model.SeName = await _urlRecordService.GetSeNameAsync(product);
//                model.ShippingCost = product.ShippingPrice;
//                model.ShortDescription = product.ShortDescription;
//                model.Sku = await GetProductSku(product.Sku, product.Id);
//                model.TotaLinventory = product.TotalInventory;



//                #region Variant
//                var combinations = await _productAttributeService.GetAllProductAttributeCombinationsAsync(product.Id);
//                var validFullCombinationSets = new List<HashSet<int>>();

//                foreach (var combo in combinations)
//                {
//                    var comboValues = await _productAttributeParser.ParseProductAttributeValuesAsync(combo.AttributesXml);
//                    // Only consider published combinations
//                    if (!comboValues.Any(av => !av.Published))
//                    {
//                        validFullCombinationSets.Add(new HashSet<int>(comboValues.Select(v => v.Id)));
//                    }
//                }

//                List<Models.Customizations.Custom.ProductApi.ProductModel.Variant> Variants = new List<Models.Customizations.Custom.ProductApi.ProductModel.Variant>();
//                foreach (var _variant in await _productService.GetProductVariants(product.Id))
//                {

//                    if (product.EnableConditionalAttributes)
//                    {
//                        var variantValueIds = (_variant.ProductAttributeValueIds ?? string.Empty)
//                      .Split('-', StringSplitOptions.RemoveEmptyEntries)
//                      .Select(int.Parse)
//                      .ToHashSet();


//                        bool isValid = validFullCombinationSets.Any(comboSet => variantValueIds.IsSubsetOf(comboSet));

//                        if (!isValid)
//                            continue;
//                    }
//                    bool isDefault = true;
//                    bool published = true;

//                    string manufacturerPartNumber = string.Empty;
//                    decimal defaultWeight = 0;
//                    ProductAttribute sizeAttribute = null;
//                    ProductAttributeValue sizeAttributeValue = null;
//                    Dictionary<string, string> attributes = new Dictionary<string, string>();
//                    foreach (var attrValueId in (_variant.ProductAttributeValueIds ?? string.Empty).Split('-'))
//                    {
//                        int.TryParse(attrValueId, out int _attrValueId);
//                        var productAttributeValue = await _productAttributeService.GetProductAttributeValueByIdAsync(_attrValueId);
//                        if (productAttributeValue != null)
//                        {
//                            var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(productAttributeValue.ProductAttributeMappingId);
//                            if (productAttributeMapping != null)
//                            {
//                                var productAttribute = await _productAttributeService.GetProductAttributeByIdAsync(productAttributeMapping.ProductAttributeId);
//                                if (productAttribute != null && productAttribute.Name.Equals(sizeAttributeName, StringComparison.InvariantCultureIgnoreCase))
//                                {
//                                    sizeAttribute = productAttribute;
//                                    sizeAttributeValue = productAttributeValue;
//                                }
//                                if (productAttribute != null && !attributes.Keys.Where(K => K == CustomCommonHelper.SanitizeToLower(productAttribute.Name)).Any()) { }
//                                {
//                                    attributes.Add(CustomCommonHelper.SanitizeToLower(productAttribute.Name), productAttributeValue.Name);
//                                }
//                            }
//                        }
//                        if (productAttributeValue == null || !productAttributeValue.Published)
//                        {
//                            published = false;
//                        }
//                        isDefault = !isDefault ? false : productAttributeValue?.IsPreSelected ?? false;

//                    }

//                    if (_variant.VariantId > 0 && published)
//                    {
//                        Models.ProductApi.ProductModel.Variant variant = new Models.ProductApi.ProductModel.Variant();
//                        variant.Name = CustomCommonHelper.FirstOrEmpty(sizeAttributeValue?.Name, _variant.Title);
//                        variant.Id = _variant.VariantId;
//                        variant.IsDefault = isDefault;
//                        variant.ManufacturerPartNumber = CustomCommonHelper.FirstOrEmpty(_variant?.ManufacturerPartNumber, sizeAttributeValue?.ManufacturerPartNumber ?? string.Empty);
//                        variant.Weight = _variant.Weight == 0 ? sizeAttributeValue?.WeightAdjustment ?? 0 : _variant.Weight;
//                        variant.VariantTitle = CustomCommonHelper.FirstOrEmpty(_variant?.Title, sizeAttributeValue?.VariantTitle ?? string.Empty);
//                        variant.Dimension = CustomCommonHelper.FirstOrEmpty(_variant?.Dimension, sizeAttributeValue?.Dimension ?? String.Empty);
//                        variant.QueryParameter = CustomCommonHelper.FirstOrEmpty(_variant?.QueryParameter, sizeAttributeValue?.QueryParameter ?? string.Empty);
//                        variant.Attributes = attributes;
//                        if ((_variant?.OldPrice ?? 0) == 0 && (_variant?.Price ?? 0) == 0)
//                        {
//                            variant.MsrpValue = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(productPrice.MsrpValue, await _workContext.GetWorkingCurrencyAsync());
//                            variant.OldPriceValue = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(productPrice.OldPriceValue, await _workContext.GetWorkingCurrencyAsync());
//                            variant.PriceValue = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(productPrice.PriceValue, await _workContext.GetWorkingCurrencyAsync());

//                        }
//                        else
//                        {
//                            variant.MsrpValue = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(_variant.Msrp ?? 0, await _workContext.GetWorkingCurrencyAsync());
//                            variant.OldPriceValue = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(_variant.OldPrice ?? 0, await _workContext.GetWorkingCurrencyAsync());
//                            variant.PriceValue = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(_variant.Price, await _workContext.GetWorkingCurrencyAsync());
//                        }

//                        Variants.Add(variant);
//                    }
//                }
//                model.Variants = Variants;
//                #endregion

//                #region Sale Info

//                var discountInfo = await _storeWideDiscountService.GetProductSaleInfo(model.Id);
//                if (discountInfo != null)
//                {
//                    model.SaleStartDate = discountInfo.StartDate;
//                    model.SaleEndDate = discountInfo.EndDate;
//                }

//                #endregion

//                lstModel.Add(model);
//            }
//            return lstModel;
//        }

//        #region Utilties
//        private async Task<string> CategoryPath(string seprator, int categodyId, string categoryName, int parentCategoryId, List<int> excludedCategories)
//        {
//            string path = "";
//            if (excludedCategories.Contains(categodyId))
//            {
//                path = categoryName;
//                while (parentCategoryId != 0)
//                {
//                    var category = await _categoryService.GetCategoryByIdAsync(parentCategoryId);
//                    if (category != null)
//                    {
//                        path = category.Name + seprator + path;
//                        parentCategoryId = category.ParentCategoryId;
//                    }
//                    else
//                        parentCategoryId = 0;
//                }
//            }
//            return path;
//        }

//        #endregion
//    }
//}
