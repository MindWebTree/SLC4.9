using Microsoft.AspNetCore.Mvc.Rendering;
using MWT.Nop.Core.Domain;
using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Services.Catalog;
using MWT.Nop.Core.Services.Configuration;
using MWT.Nop.Core.Services.Media;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.FilterLevels;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Areas.Admin.Models.Customization.Catalog;
using Nop.Web.Areas.Admin.Models.Customization.Custom;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Extensions;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using System.ComponentModel;
using System.Net;
using System.Text;

namespace Nop.Web.Areas.Admin.Factories;

/// <summary>
/// Represents the product model factory implementation
/// </summary>
public partial class ProductModelFactory : IProductModelFactory
{

    #region Methods 
    /// <summary>
    /// Prepare paged product list model
    /// </summary>
    /// <param name="searchModel">Product search model</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the product list model
    /// </returns>
    public virtual async Task<ProductListModel> CustomPrepareProductListModelAsync(ProductSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        //get parameters to filter comments
        var overridePublished = searchModel.SearchPublishedId == 0 ? null : (bool?)(searchModel.SearchPublishedId == 1);
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null)
            searchModel.SearchVendorId = currentVendor.Id;
        var categoryIds = new List<int> { searchModel.SearchCategoryId };
        if (searchModel.SearchIncludeSubCategories && searchModel.SearchCategoryId > 0)
        {
            var childCategoryIds = await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: searchModel.SearchCategoryId, showHidden: true);
            categoryIds.AddRange(childCategoryIds);
        }
        IPagedList<Product> products;
        var customer = await _workContext.GetCurrentCustomerAsync();
        //get products
        var primaryStoreCurrency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);
        var _customProductService = EngineContext.Current.Resolve<IProductExtendedService>();
        if (await _customerService.IsInCustomerRoleAsync(customer, "CategoryManager"))
        {
            products = await _customProductService.GetAccessibleSearchProductsAsync(showHidden: true,
            categoryIds: categoryIds,
            customerId:customer.Id,
            manufacturerIds: new List<int> { searchModel.SearchManufacturerId },
            storeId: searchModel.SearchStoreId,
            vendorId: searchModel.SearchVendorId,
            warehouseId: searchModel.SearchWarehouseId,
            productType: searchModel.SearchProductTypeId > 0 ? (ProductType?)searchModel.SearchProductTypeId : null,
            keywords: searchModel.SearchProductName,
            pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize,
            overridePublished: overridePublished);
        }
        else
        {
            products = await _customProductService.OverriddenSearchProductsAsync(showHidden: true,
              categoryIds: categoryIds,
              manufacturerIds: new List<int> { searchModel.SearchManufacturerId },
              storeId: searchModel.SearchStoreId,
              vendorId: searchModel.SearchVendorId,
              warehouseId: searchModel.SearchWarehouseId,
              productType: searchModel.SearchProductTypeId > 0 ? (ProductType?)searchModel.SearchProductTypeId : null,
              keywords: searchModel.SearchProductName,
              pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize,
              overridePublished: overridePublished);
        }
       
        //prepare list model
        var model = await new ProductListModel().PrepareToGridAsync(searchModel, products, () =>
        {
            return products.SelectAwait(async product =>
            {
                //fill in model values from the entity
                var productModel = product.ToModel<ProductModel>();

                //little performance optimization: ensure that "FullDescription" is not returned
                productModel.FullDescription = string.Empty;

                //fill formatted price
                productModel.FormattedPrice = product.ProductType == ProductType.GroupedProduct ? null : await _priceFormatter.FormatPriceAsync(product.Price);

                productModel.PrimaryStoreCurrencyCode = primaryStoreCurrency.CurrencyCode;

                //fill in additional values (not existing in the entity)
                productModel.SeName = await _urlRecordService.GetSeNameAsync(product, 0, true, false);
                var defaultProductPicture = (await _pictureService.GetPicturesByProductIdAsync(product.Id, 1)).FirstOrDefault();
                (productModel.PictureThumbnailUrl, _) = await _pictureService.GetPictureUrlAsync(defaultProductPicture, 75);
                productModel.ProductTypeName = await _localizationService.GetLocalizedEnumAsync(product.ProductType);
                if (product.ProductType == ProductType.SimpleProduct && product.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
                    productModel.StockQuantityStr = (await _productService.GetTotalStockQuantityAsync(product)).ToString();

                return productModel;
            });
        });

        return model;
    }

    /// <summary>
    /// Prepare product model
    /// </summary>
    /// <param name="model">Product model</param>
    /// <param name="product">Product</param>
    /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the product model
    /// </returns>
    public virtual async Task<ProductModel> CustomPrepareProductModelAsync(ProductModel model, Product product, bool excludeProperties = false)
    {
        Func<ProductLocalizedModel, int, Task> localizedModelConfiguration = null;

        if (product != null)
        {
            //fill in model values from the entity
            if (model == null)
            {
                model = product.ToModel<ProductModel>();
                model.SeName = await _urlRecordService.GetSeNameAsync(product, 0, true, false);
            }

            var parentGroupedProduct = await _productService.GetProductByIdAsync(product.ParentGroupedProductId);
            if (parentGroupedProduct != null)
            {
                model.AssociatedToProductId = product.ParentGroupedProductId;
                model.AssociatedToProductName = parentGroupedProduct.Name;
            }

            model.LastStockQuantity = product.StockQuantity;

            model.SelectedProductTags = (await _productTagService.GetAllProductTagsByProductIdAsync(product.Id)).Select(tag => tag.Name).ToList();
            model.AvailableProductTags = (await _productTagService.GetAllProductTagsAsync())
                .Select(pt => new SelectListItem { Text = pt.Name, Value = pt.Name }).ToList();

            model.ProductAttributesExist = (await _productAttributeService.GetAllProductAttributesAsync()).Any();

            model.CanCreateCombinations = await (await _productAttributeService
                .GetProductAttributeMappingsByProductIdAsync(product.Id)).AnyAwaitAsync(async pam => (await _productAttributeService.GetProductAttributeValuesAsync(pam.Id)).Any());

            if (!excludeProperties)
            {
                model.SelectedCategoryIds = (await _categoryService.GetProductCategoriesByProductIdAsync(product.Id, true))
                    .Select(productCategory => productCategory.CategoryId).ToList();
                model.SelectedManufacturerIds = (await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id, true))
                    .Select(productManufacturer => productManufacturer.ManufacturerId).ToList();
            }

            //prepare copy product model
            await PrepareCopyProductModelAsync(model.CopyProductModel, product);

            //prepare nested search model
            PrepareRelatedProductSearchModel(model.RelatedProductSearchModel, product);
            PrepareCrossSellProductSearchModel(model.CrossSellProductSearchModel, product);
            PrepareFilterLevelValuesSearchModel(model.FilterLevelValueSearchModel, product);
            PrepareAssociatedProductSearchModel(model.AssociatedProductSearchModel, product);
            PrepareProductPictureSearchModel(model.ProductPictureSearchModel, product);
            PrepareProductVideoSearchModel(model.ProductVideoSearchModel, product);
            PrepareProductSpecificationAttributeSearchModel(model.ProductSpecificationAttributeSearchModel, product);
            PrepareProductOrderSearchModel(model.ProductOrderSearchModel, product);
            PrepareTierPriceSearchModel(model.TierPriceSearchModel, product);
            await PrepareStockQuantityHistorySearchModelAsync(model.StockQuantityHistorySearchModel, product);
            PrepareProductAttributeMappingSearchModel(model.ProductAttributeMappingSearchModel, product);
            PrepareProductAttributeCombinationSearchModel(model.ProductAttributeCombinationSearchModel, product);
            await PrepareSmartInjectionDataAsync(model, product);
            //define localized model configuration action
            localizedModelConfiguration = async (locale, languageId) =>
            {
                locale.Name = await _localizationService.GetLocalizedAsync(product, entity => entity.Name, languageId, false, false);
                locale.FullDescription = await _localizationService.GetLocalizedAsync(product, entity => entity.FullDescription, languageId, false, false);
                locale.ShortDescription = await _localizationService.GetLocalizedAsync(product, entity => entity.ShortDescription, languageId, false, false);
                locale.MetaKeywords = await _localizationService.GetLocalizedAsync(product, entity => entity.MetaKeywords, languageId, false, false);
                locale.MetaDescription = await _localizationService.GetLocalizedAsync(product, entity => entity.MetaDescription, languageId, false, false);
                locale.MetaTitle = await _localizationService.GetLocalizedAsync(product, entity => entity.MetaTitle, languageId, false, false);
                locale.SeName = await _urlRecordService.GetSeNameAsync(product, languageId, false, false);
            };
        }

        //set default values for the new model
        if (product == null)
        {
            model.MaximumCustomerEnteredPrice = 1000;
            model.MaxNumberOfDownloads = 10;
            model.RecurringCycleLength = 100;
            model.RecurringTotalCycles = 10;
            model.RentalPriceLength = 1;
            model.StockQuantity = 10000;
            model.NotifyAdminForQuantityBelow = 1;
            model.OrderMinimumQuantity = 1;
            model.OrderMaximumQuantity = 10000;
            model.TaxCategoryId = _taxSettings.DefaultTaxCategoryId;
            model.UnlimitedDownloads = true;
            model.IsShipEnabled = true;
            model.AllowCustomerReviews = true;
            model.Published = true;
            model.VisibleIndividually = true;
        }

        model.PrimaryStoreCurrencyCode = (await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId)).CurrencyCode;
        model.BaseWeightIn = (await _measureService.GetMeasureWeightByIdAsync(_measureSettings.BaseWeightId)).Name;
        model.BaseDimensionIn = (await _measureService.GetMeasureDimensionByIdAsync(_measureSettings.BaseDimensionId)).Name;
        model.HasAvailableSpecificationAttributes =
            (await _specificationAttributeService.GetSpecificationAttributesWithOptionsAsync()).Any();

        var currentVendor = await _workContext.GetCurrentVendorAsync();
        model.IsLoggedInAsVendor = currentVendor != null;

        //prepare localized models
        if (!excludeProperties)
            model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);

        //prepare editor settings
        model.ProductEditorSettingsModel = await _settingModelFactory.PrepareProductEditorSettingsModelAsync();

        //prepare available product templates
        await _baseAdminModelFactory.PrepareProductTemplatesAsync(model.AvailableProductTemplates, false);

        //prepare available product types
        var productTemplates = await _productTemplateService.GetAllProductTemplatesAsync();
        foreach (var productType in Enum.GetValues(typeof(ProductType)).OfType<ProductType>())
        {
            model.ProductsTypesSupportedByProductTemplates.Add((int)productType, new List<SelectListItem>());
            foreach (var template in productTemplates)
            {
                var list = (IList<int>)TypeDescriptor.GetConverter(typeof(List<int>)).ConvertFrom(template.IgnoredProductTypes) ?? new List<int>();
                if (string.IsNullOrEmpty(template.IgnoredProductTypes) || !list.Contains((int)productType))
                {
                    model.ProductsTypesSupportedByProductTemplates[(int)productType].Add(new SelectListItem
                    {
                        Text = template.Name,
                        Value = template.Id.ToString()
                    });
                }
            }
        }

        //prepare available delivery dates
        await _baseAdminModelFactory.PrepareDeliveryDatesAsync(model.AvailableDeliveryDates,
            defaultItemText: await _localizationService.GetResourceAsync("Admin.Catalog.Products.Fields.DeliveryDate.None"));

        //prepare available product availability ranges
        await _baseAdminModelFactory.PrepareProductAvailabilityRangesAsync(model.AvailableProductAvailabilityRanges,
            defaultItemText: await _localizationService.GetResourceAsync("Admin.Catalog.Products.Fields.ProductAvailabilityRange.None"));

        //prepare available vendors
        await _baseAdminModelFactory.PrepareVendorsAsync(model.AvailableVendors,
            defaultItemText: await _localizationService.GetResourceAsync("Admin.Catalog.Products.Fields.Vendor.None"));

        //prepare available tax categories
        await _baseAdminModelFactory.PrepareTaxCategoriesAsync(model.AvailableTaxCategories);

        //prepare available warehouses
        await _baseAdminModelFactory.PrepareWarehousesAsync(model.AvailableWarehouses,
            defaultItemText: await _localizationService.GetResourceAsync("Admin.Catalog.Products.Fields.Warehouse.None"));
        await PrepareProductWarehouseInventoryModelsAsync(model.ProductWarehouseInventoryModels, product);

        //prepare available base price units
        var availableMeasureWeights = (await _measureService.GetAllMeasureWeightsAsync())
            .Select(weight => new SelectListItem { Text = weight.Name, Value = weight.Id.ToString() }).ToList();
        model.AvailableBasepriceUnits = availableMeasureWeights;
        model.AvailableBasepriceBaseUnits = availableMeasureWeights;

        //prepare model categories
        await _baseAdminModelFactory.PrepareCategoriesAsync(model.AvailableCategories, false);
        foreach (var categoryItem in model.AvailableCategories)
        {
            categoryItem.Selected = int.TryParse(categoryItem.Value, out var categoryId)
                                    && model.SelectedCategoryIds.Contains(categoryId);
        }

        //prepare model manufacturers
        await _baseAdminModelFactory.PrepareManufacturersAsync(model.AvailableManufacturers, false);
        foreach (var manufacturerItem in model.AvailableManufacturers)
        {
            manufacturerItem.Selected = int.TryParse(manufacturerItem.Value, out var manufacturerId)
                                        && model.SelectedManufacturerIds.Contains(manufacturerId);
        }

        //prepare model discounts
        var availableDiscounts = await _discountService.GetAllDiscountsAsync(
            discountType: DiscountType.AssignedToSkus,
            showHidden: true, isActive: null,
            vendorId: currentVendor?.Id ?? 0);

        await _discountSupportedModelFactory.PrepareModelDiscountsAsync(model, product, availableDiscounts, excludeProperties);

        //prepare model stores
        await _storeMappingSupportedModelFactory.PrepareModelStoresAsync(model, product, excludeProperties);

        await _baseAdminModelFactory.PreparePreTranslationSupportModelAsync(model);

        return model;
    }



    public virtual async Task<CollectionProductListModel> PrepareCollectionProductListModelAsync(RelatedProductSearchModel searchModel, Product product)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        if (product == null)
            throw new ArgumentNullException(nameof(product));
        var _customProductService = EngineContext.Current.Resolve<IProductExtendedService>();
        //get collection products
        var collectionProducts = (await _customProductService
            .GetCollectionProductsByProductId1ListAsync(productId: product.Id, showHidden: true)).ToPagedList(searchModel);

        //prepare grid model
        var model = await new CollectionProductListModel().PrepareToGridAsync(searchModel, collectionProducts, () =>
        {
            return collectionProducts.SelectAwait(async collectionProduct =>
            {
                CollectionProductModel collectionProductModel = new CollectionProductModel();
                collectionProductModel.DisplayOrder = collectionProduct.DisplayOrder;
                collectionProductModel.Id = collectionProduct.Id;
                collectionProductModel.ProductId2 = collectionProduct.ProductId2;
                var defaultProductPicture = (await _pictureService.GetPicturesByProductIdAsync(collectionProductModel.ProductId2, 1)).FirstOrDefault();
                (collectionProductModel.PictureThumbnailUrl, _) = await _pictureService.GetPictureUrlAsync(defaultProductPicture, 75);
                //fill in additional values (not existing in the entity)
                collectionProductModel.Product2Name = (await _productService.GetProductByIdAsync(collectionProduct.ProductId2))?.Name;

                return collectionProductModel;
            });
        });
        return model;
    }
    public virtual async Task<AddCollectionProductSearchModel> PrepareAddCollectionProductSearchModelAsync(AddCollectionProductSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        searchModel.IsLoggedInAsVendor = await _workContext.GetCurrentVendorAsync() != null;

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
    public virtual async Task<AddCollectionProductListModel> PrepareAddCollectionProductListModelAsync(AddCollectionProductSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //a vendor should have access only to his products
        if (await _workContext.GetCurrentVendorAsync() != null)
            searchModel.SearchVendorId = (await _workContext.GetCurrentVendorAsync()).Id;
        var _customProductService = EngineContext.Current.Resolve<IProductExtendedService>();
        //get products
        var products = await _customProductService.OverriddenSearchProductsAsync(showHidden: true,
            categoryIds: new List<int> { searchModel.SearchCategoryId },
            manufacturerIds: new List<int> { searchModel.SearchManufacturerId },
            storeId: searchModel.SearchStoreId,
            vendorId: searchModel.SearchVendorId,
            productType: searchModel.SearchProductTypeId > 0 ? (ProductType?)searchModel.SearchProductTypeId : null,
            keywords: searchModel.SearchProductName,
            pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        //prepare grid model
        var model = await new AddCollectionProductListModel().PrepareToGridAsync(searchModel, products, () =>
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
    #region PairWith

    public virtual async Task<PairWithProductListModel> PreparePairWithProductListModelAsync(RelatedProductSearchModel searchModel, Product product)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        if (product == null)
            throw new ArgumentNullException(nameof(product));
        var _customProductService = EngineContext.Current.Resolve<IProductExtendedService>();
        //get collection products
        var pairWithProducts = (await _customProductService
            .GetPairWithProductsByProductId1ListAsync(productId: product.Id, showHidden: true)).ToPagedList(searchModel);

        //prepare grid model
        var model = await new PairWithProductListModel().PrepareToGridAsync(searchModel, pairWithProducts, () =>
        {
            return pairWithProducts.SelectAwait(async pairwithProduct =>
            {
                PairWithProductModel pairWithProductModel = new PairWithProductModel();
                pairWithProductModel.DisplayOrder = pairwithProduct.DisplayOrder;
                pairWithProductModel.Id = pairwithProduct.Id;
                pairWithProductModel.ProductId2 = pairwithProduct.ProductId2;
                var defaultProductPicture = (await _pictureService.GetPicturesByProductIdAsync(pairWithProductModel.ProductId2, 1)).FirstOrDefault();
                (pairWithProductModel.PictureThumbnailUrl, _) = await _pictureService.GetPictureUrlAsync(defaultProductPicture, 75);
                //fill in additional values (not existing in the entity)
                pairWithProductModel.Product2Name = (await _productService.GetProductByIdAsync(pairwithProduct.ProductId2))?.Name;

                return pairWithProductModel;
            });
        });
        return model;
    }

    public virtual async Task<AddPairWithProductSearchModel> PrepareAddPairWithProductSearchModelAsync(AddPairWithProductSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        searchModel.IsLoggedInAsVendor = await _workContext.GetCurrentVendorAsync() != null;

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

    public virtual async Task<AddPairWithProductListModel> PrepareAddPairWithProductListModelAsync(AddPairWithProductSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //a vendor should have access only to his products
        if (await _workContext.GetCurrentVendorAsync() != null)
            searchModel.SearchVendorId = (await _workContext.GetCurrentVendorAsync()).Id;

        //get products
        var _customProductService = EngineContext.Current.Resolve<IProductExtendedService>();
        var products = await _customProductService.OverriddenSearchProductsAsync(showHidden: true,
            categoryIds: new List<int> { searchModel.SearchCategoryId },
            manufacturerIds: new List<int> { searchModel.SearchManufacturerId },
            storeId: searchModel.SearchStoreId,
            vendorId: searchModel.SearchVendorId,
            productType: searchModel.SearchProductTypeId > 0 ? (ProductType?)searchModel.SearchProductTypeId : null,
            keywords: searchModel.SearchProductName,
            pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        //prepare grid model
        var model = await new AddPairWithProductListModel().PrepareToGridAsync(searchModel, products, () =>
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

    #endregion
    #region FBT Product
    public virtual async Task<FBTProductListModel> PrepareFBTProductListModelAsync(FBTProductSearchModel searchModel, Product product)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        if (product == null)
            throw new ArgumentNullException(nameof(product));

        //get FBT products
        var _customProductService = EngineContext.Current.Resolve<IProductExtendedService>();
        var fBTProducts = (await _customProductService
            .GetFBTProductsByProductId1Async(productId1: product.Id, showHidden: true)).ToPagedList(searchModel);

        //prepare grid model
        var model = await new FBTProductListModel().PrepareToGridAsync(searchModel, fBTProducts, () =>
        {
            return fBTProducts.SelectAwait(async fBTProduct =>
            {
                //fill in model values from the entity
                var fBTProductModel = new FBTProductModel();
                fBTProductModel.Id = fBTProduct.Id;
                fBTProductModel.ProductId2 = fBTProduct.ProductId2;
                fBTProductModel.DisplayOrder = fBTProduct.DisplayOrder;
                fBTProductModel.DefaultQuantity = fBTProduct.DefaultQuantity;

                var defaultProductPicture = (await _pictureService.GetPicturesByProductIdAsync(fBTProductModel.ProductId2, 1)).FirstOrDefault();
                (fBTProductModel.PictureThumbnailUrl, _) = await _pictureService.GetPictureUrlAsync(defaultProductPicture, 75);
                //fill in additional values (not existing in the entity)
                fBTProductModel.Product2Name = (await _productService.GetProductByIdAsync(fBTProduct.ProductId2))?.Name;
                return fBTProductModel;
            });
        });
        return model;
    }

    /// <summary>
    /// Prepare FBT product search model to add to the product
    /// </summary>
    /// <param name="searchModel">FBT product search model to add to the product</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the FBT product search model to add to the product
    /// </returns>
    public virtual async Task<AddFBTProductSearchModel> PrepareAddFBTProductSearchModelAsync(AddFBTProductSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        searchModel.IsLoggedInAsVendor = await _workContext.GetCurrentVendorAsync() != null;

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

    /// <summary>
    /// Prepare paged FBT product list model to add to the product
    /// </summary>
    /// <param name="searchModel">FBT product search model to add to the product</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the FBT product list model to add to the product
    /// </returns>
    public virtual async Task<AddFBTProductListModel> PrepareAddFBTProductListModelAsync(AddFBTProductSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //a vendor should have access only to his products
        if (await _workContext.GetCurrentVendorAsync() != null)
            searchModel.SearchVendorId = (await _workContext.GetCurrentVendorAsync()).Id;

        //get products
        var _customProductService = EngineContext.Current.Resolve<IProductExtendedService>();
        var products = await _customProductService.OverriddenSearchProductsAsync(showHidden: true,
            categoryIds: new List<int> { searchModel.SearchCategoryId },
            manufacturerIds: new List<int> { searchModel.SearchManufacturerId },
            storeId: searchModel.SearchStoreId,
            vendorId: searchModel.SearchVendorId,
            productType: searchModel.SearchProductTypeId > 0 ? (ProductType?)searchModel.SearchProductTypeId : null,
            keywords: searchModel.SearchProductName,
            pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        //prepare grid model
        var model = await new AddFBTProductListModel().PrepareToGridAsync(searchModel, products, () =>
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

    #endregion
    #region GroupedProductConfiguration 
    public async Task<List<GroupedProductConfigurationModel>> PrepareGroupedProductConfiguration(int productId)
    {
        Dictionary<int, string> attributes = new Dictionary<int, string>();
        attributes.Add(0, "Default");
        var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(productId);
        foreach (var attribute in productAttributeMapping)
        {
            var productAttrubute = await _productAttributeService.GetProductAttributeByIdAsync(attribute.ProductAttributeId);
            if (productAttrubute.Name == await _localizationService.GetResourceAsync("Product.Attr.Size"))
            {
                var attributeValues = await _productAttributeService.GetProductAttributeValuesAsync(attribute.Id);
                foreach (var attributeValue in attributeValues)
                {
                    attributes.Add(attributeValue.Id, attributeValue.Name);
                }
            }
        }

        var _groupedProductConfigurationService = EngineContext.Current.Resolve<IGroupedProductConfigurationService>();

        var configurations = await _groupedProductConfigurationService.GetConfigurationOfGroupedProduct(productId);

        List<GroupedProductConfigurationModel> configurationsModel = new List<GroupedProductConfigurationModel>();
        var associatedProducts = (await _productService.GetAssociatedProductsAsync(showHidden: true,
           parentGroupedProductId: productId,
           vendorId: (await _workContext.GetCurrentVendorAsync())?.Id ?? 0));

        foreach (var attribute in attributes)
        {
            GroupedProductConfigurationModel model = new GroupedProductConfigurationModel();
            model.ProductId = productId;
            model.ProductAttributeOptionId = attribute.Key;
            model.ProductAttributeOptionName = attribute.Value;

            var configuration = configurations.Where(c => c.ProductAttributeOptionId == attribute.Key).FirstOrDefault();

            List<GrpConfiguration> dbConfiguration = new List<GrpConfiguration>();
            if (!string.IsNullOrEmpty(configuration?.Raw))
            {
                dbConfiguration = JsonConvert.DeserializeObject<List<GrpConfiguration>>(configuration.Raw);
            }
            model.Id = configuration?.Id ?? 0;
            List<GrpConfiguration> lstConfigurations = new List<GrpConfiguration>();
            foreach (var associateProduct in associatedProducts)
            {
                GrpConfiguration _configuration = new GrpConfiguration();
                _configuration.ProductId = associateProduct.Id;
                _configuration.Sku = associateProduct.Sku;
                _configuration.Quantity = dbConfiguration.Where(c => c.ProductId == associateProduct.Id).FirstOrDefault()?.Quantity ?? 1;
                lstConfigurations.Add(_configuration);
            }
            model.Configurations = lstConfigurations;
            configurationsModel.Add(model);
        }
        return configurationsModel;
    }

    #endregion
    #region Product Suggested Keyword

    public virtual async Task<ProductSuggestedKeywordListModel> CustomPrepareSuggestedKeywordListModelAsync(ProductSuggestedKeywordSearchModel searchModel, int categoryid)
    {
        var _suggestedKeywordsService = EngineContext.Current.Resolve<ISuggestedKeywordsService>();

        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        if (categoryid == 0)
            throw new ArgumentNullException("Provide ProductId");

        //get product categories
        var suggestedKeyWords = await _suggestedKeywordsService.GetProductSuggestedKeyword(categoryid,
           pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        //prepare grid model
        var model = await new ProductSuggestedKeywordListModel().PrepareToGridAsync(searchModel, suggestedKeyWords, () =>
        {
            return suggestedKeyWords.SelectAwait(async suggestedKeyWord =>
            {
                ProductSuggestedKeywordModel model = new ProductSuggestedKeywordModel();
                model.ProductId = suggestedKeyWord.ProductId;
                model.SuggestedKeyWordId = suggestedKeyWord.SuggestedKeyWordID;
                model.Id = suggestedKeyWord.Id;
                return model;
            });
        });

        foreach (var item in model.Data)
        {
            item.KeyWord = (await _suggestedKeywordsService.GetSuggestedKeyWordById(item.SuggestedKeyWordId)).SuggestedKeywords;
        }
        return model;
    }

    public async Task<ProductSuggestedKeywordModel> GetProductSuggestedKeywordById(int id)
    {
        var _suggestedKeywordsService = EngineContext.Current.Resolve<ISuggestedKeywordsService>();

        var suggestedKeyWord = await _suggestedKeywordsService.GetProductSuggestedKeywordById(id);
        ProductSuggestedKeywordModel model = new ProductSuggestedKeywordModel();
        model.ProductId = suggestedKeyWord.ProductId;
        model.SuggestedKeyWordId = suggestedKeyWord.SuggestedKeyWordID;
        model.KeyWord = (await _suggestedKeywordsService.GetSuggestedKeyWordById(suggestedKeyWord.SuggestedKeyWordID)).SuggestedKeywords;
        model.Id = suggestedKeyWord.Id;
        return model;
    }

    public async Task CreateProductSuggestedKeyword(ProductSuggestedKeywordModel productSuggestedKeywordModel)
    {
        var _suggestedKeywordsService = EngineContext.Current.Resolve<ISuggestedKeywordsService>();
        SuggestedKeyword suggestedKeyword = new SuggestedKeyword();
        suggestedKeyword = await _suggestedKeywordsService.GetSuggestedKeyWordByKeyword(productSuggestedKeywordModel.KeyWord);
        if (suggestedKeyword == null)
        {
            suggestedKeyword = new SuggestedKeyword();
            suggestedKeyword.SuggestedKeywords = productSuggestedKeywordModel.KeyWord;
            await _suggestedKeywordsService.InsertSuggestedKeyWordAsync(suggestedKeyword);
        }
        ProductSuggestedKeyword productSuggestedKeyword = new ProductSuggestedKeyword()
        {
            ProductId = productSuggestedKeywordModel.ProductId,
            SuggestedKeyWordID = suggestedKeyword.Id,
            IsCustom = true
        };
        await _suggestedKeywordsService.InsertProductSuggestedKeyWordMappingAsync(productSuggestedKeyword);
    }

    public async Task DeleteProductSuggestedKeyword(ProductSuggestedKeywordModel productSuggestedKeywordModel)
    {
        var _suggestedKeywordsService = EngineContext.Current.Resolve<ISuggestedKeywordsService>();

        ProductSuggestedKeyword productSuggestedKeyword = new ProductSuggestedKeyword()
        {
            Id = productSuggestedKeywordModel.Id,
            ProductId = productSuggestedKeywordModel.ProductId,
            SuggestedKeyWordID = productSuggestedKeywordModel.SuggestedKeyWordId,
        };
        await _suggestedKeywordsService.DeleteProductSuggestedKeyWordMappingAsync(productSuggestedKeyword);
    }

    public async Task<bool> IsProductKeywordExist(int keywordId, string keyword, int productId)
    {
        var _suggestedKeywordsService = EngineContext.Current.Resolve<ISuggestedKeywordsService>();

        return await _suggestedKeywordsService.IsProductKeywordExist(keywordId, keyword, productId);

    }

    #endregion


    #region Variants


    public async Task<VariantSearchListModel> PrepareVariantListModelAsync(VariantSearchModel searchModel, Product product)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));
        var _customProductService = EngineContext.Current.Resolve<IProductExtendedService>();
        var variants = (await _customProductService.GetProductVariants(searchModel.ProductId)).ToPagedList(searchModel);


        var productAttributeFormatter = EngineContext.Current.Resolve<IProductAttributeFormatter>();
        var _productAttributeService = EngineContext.Current.Resolve<IProductAttributeService>();

        //prepare grid model
        return await new VariantSearchListModel().PrepareToGridAsync(searchModel, variants, () =>
        {
            return variants.SelectAwait(async variant =>
            {
                VariantModel variantModel = new VariantModel();
                variantModel.Attributes = await productAttributeFormatter.FormatAttributesAsync(product, variant.Combination);
                variantModel.CreatedOn = variant.CreatedOn;
                variantModel.UpdatedOn = variant.UpdatedOn;
                variantModel.VariantId = variant.VariantId;
                variantModel.Id = variant.Id;
                variantModel.OldPrice = variant.OldPrice;
                variantModel.Price = variant.Price;
                variantModel.Msrp = variant.Msrp;
                variantModel.WgsRequired = variant.WgsRequired;
                variantModel.Title = variant.Title;
                variantModel.EnableSurcharge = variant.EnableSurcharge;
                variantModel.EstimatedDeliveryDate = variant.EstimatedDeliveryDate;
                bool publish = true;
                foreach (var attrValueId in (variant.ProductAttributeValueIds ?? string.Empty).Split('-'))
                {
                    int.TryParse(attrValueId, out int _attrValueId);
                    var productAttributeValue = await _productAttributeService.GetProductAttributeValueByIdAsync(_attrValueId);
                    if (productAttributeValue == null || !productAttributeValue.Published)
                    {
                        publish = false;
                        break;
                    }
                }
                variantModel.Published = publish;
                variantModel.ProductId = variant.ProductId;
                variantModel.QueryParameter = variant.QueryParameter;
                variantModel.Weight = variant.Weight;
                variantModel.ManufacturerPartNumber = variant.ManufacturerPartNumber;
                variantModel.DimensionPictureId = variant.DimensionPictureId;
                return variantModel;

            });
        });

    }
    public async Task<VariantModel> PrepareVariantModelAsync(int id)
    {
        var variantModel = new VariantModel();
        var _customProductService = EngineContext.Current.Resolve<IProductExtendedService>();
        var variant = await _customProductService.GetVariantById(id);
        variantModel.ProductId = variant.ProductId;
        variantModel.Msrp = variant.Msrp;
        variantModel.Price = variant.Price;
        variantModel.Title = variant.Title;
        variantModel.OldPrice = variant.OldPrice;
        variantModel.EnableSurcharge = variant.EnableSurcharge;
        variantModel.EstimatedDeliveryDate = variant.EstimatedDeliveryDate;
        variantModel.WgsRequired = variant.WgsRequired;
        variantModel.QueryParameter = variant.QueryParameter;
        variantModel.Weight = variant.Weight;
        variantModel.ManufacturerPartNumber = variant.ManufacturerPartNumber;
        variantModel.DimensionPictureId = variant.DimensionPictureId;
        var productPictures = await _productService.GetProductPicturesByProductIdAsync(variant.ProductId);
        variantModel.ProductPictureModels = await productPictures.SelectAwait(async productPicture => new ProductPictureModel
        {
            Id = productPicture.Id,
            ProductId = productPicture.ProductId,
            PictureId = productPicture.PictureId,
            PictureUrl = await _pictureService.GetPictureUrlAsync(productPicture.PictureId),
            DisplayOrder = productPicture.DisplayOrder
        }).ToListAsync();
        variantModel.SeName = variant.SeName;
        return variantModel;
    }

    #endregion

    /// <summary>
    /// Prepare required product search model to add to the product
    /// </summary>
    /// <param name="searchModel">Required product search model to add to the product</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the required product search model to add to the product
    /// </returns>
    //public virtual async Task<AddRequiredProductSearchModel> PrepareAddRequiredProductSearchModelAsync(AddRequiredProductSearchModel searchModel)
    //{
    //    ArgumentNullException.ThrowIfNull(searchModel);

    //    searchModel.IsLoggedInAsVendor = await _workContext.GetCurrentVendorAsync() != null;

    //    //prepare available categories
    //    await _baseAdminModelFactory.PrepareCategoriesAsync(searchModel.AvailableCategories);

    //    //prepare available manufacturers
    //    await _baseAdminModelFactory.PrepareManufacturersAsync(searchModel.AvailableManufacturers);

    //    //prepare available stores
    //    await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

    //    //prepare available vendors
    //    await _baseAdminModelFactory.PrepareVendorsAsync(searchModel.AvailableVendors);

    //    //prepare available product types
    //    await _baseAdminModelFactory.PrepareProductTypesAsync(searchModel.AvailableProductTypes);

    //    //prepare page parameters
    //    searchModel.SetPopupGridPageSize();

    //    return searchModel;
    //}

    ///// <summary>
    ///// Prepare required product list model to add to the product
    ///// </summary>
    ///// <param name="searchModel">Required product search model to add to the product</param>
    ///// <returns>
    ///// A task that represents the asynchronous operation
    ///// The task result contains the required product list model to add to the product
    ///// </returns>
    //public virtual async Task<AddRequiredProductListModel> PrepareAddRequiredProductListModelAsync(AddRequiredProductSearchModel searchModel)
    //{
    //    ArgumentNullException.ThrowIfNull(searchModel);

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null)
    //        searchModel.SearchVendorId = currentVendor.Id;

    //    //get products
    //    var products = await _productService.SearchProductsAsync(showHidden: true,
    //        categoryIds: new List<int> { searchModel.SearchCategoryId },
    //        manufacturerIds: new List<int> { searchModel.SearchManufacturerId },
    //        storeId: searchModel.SearchStoreId,
    //        vendorId: searchModel.SearchVendorId,
    //        productType: searchModel.SearchProductTypeId > 0 ? (ProductType?)searchModel.SearchProductTypeId : null,
    //        keywords: searchModel.SearchProductName,
    //        pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

    //    //prepare grid model
    //    var model = await new AddRequiredProductListModel().PrepareToGridAsync(searchModel, products, () =>
    //    {
    //        return products.SelectAwait(async product =>
    //        {
    //            var productModel = product.ToModel<ProductModel>();

    //            productModel.SeName = await _urlRecordService.GetSeNameAsync(product, 0, true, false);

    //            return productModel;
    //        });
    //    });

    //    return model;
    //}

    ///// <summary>
    ///// Prepare paged related product list model
    ///// </summary>
    ///// <param name="searchModel">Related product search model</param>
    ///// <param name="product">Product</param>
    ///// <returns>
    ///// A task that represents the asynchronous operation
    ///// The task result contains the related product list model
    ///// </returns>
    public virtual async Task<RelatedProductListModel> CustomPrepareRelatedProductListModelAsync(RelatedProductSearchModel searchModel, Product product)
    {
        ArgumentNullException.ThrowIfNull(searchModel);
        ArgumentNullException.ThrowIfNull(product);

        //get related products
        var relatedProducts = (await _productService
            .GetRelatedProductsByProductId1Async(productId1: product.Id, showHidden: true)).ToPagedList(searchModel);

        //prepare grid model
        var model = await new RelatedProductListModel().PrepareToGridAsync(searchModel, relatedProducts, () =>
        {
            return relatedProducts.SelectAwait(async relatedProduct =>
            {
                //fill in model values from the entity
                var relatedProductModel = relatedProduct.ToModel<RelatedProductModel>();
                var defaultProductPicture = (await _pictureService.GetPicturesByProductIdAsync(relatedProduct.ProductId2, 1)).FirstOrDefault();
                (relatedProductModel.PictureThumbnailUrl, _) = await _pictureService.GetPictureUrlAsync(defaultProductPicture, 75);
                //fill in additional values (not existing in the entity)
                relatedProductModel.Product2Name = (await _productService.GetProductByIdAsync(relatedProduct.ProductId2))?.Name;

                return relatedProductModel;
            });
        });
        return model;
    }

    ///// <summary>
    ///// Prepare related product search model to add to the product
    ///// </summary>
    ///// <param name="searchModel">Related product search model to add to the product</param>
    ///// <returns>
    ///// A task that represents the asynchronous operation
    ///// The task result contains the related product search model to add to the product
    ///// </returns>
    //public virtual async Task<AddRelatedProductSearchModel> PrepareAddRelatedProductSearchModelAsync(AddRelatedProductSearchModel searchModel)
    //{
    //    ArgumentNullException.ThrowIfNull(searchModel);

    //    searchModel.IsLoggedInAsVendor = await _workContext.GetCurrentVendorAsync() != null;

    //    //prepare available categories
    //    await _baseAdminModelFactory.PrepareCategoriesAsync(searchModel.AvailableCategories);

    //    //prepare available manufacturers
    //    await _baseAdminModelFactory.PrepareManufacturersAsync(searchModel.AvailableManufacturers);

    //    //prepare available stores
    //    await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

    //    //prepare available vendors
    //    await _baseAdminModelFactory.PrepareVendorsAsync(searchModel.AvailableVendors);

    //    //prepare available product types
    //    await _baseAdminModelFactory.PrepareProductTypesAsync(searchModel.AvailableProductTypes);

    //    //prepare page parameters
    //    searchModel.SetPopupGridPageSize();

    //    return searchModel;
    //}

    ///// <summary>
    ///// Prepare paged related product list model to add to the product
    ///// </summary>
    ///// <param name="searchModel">Related product search model to add to the product</param>
    ///// <returns>
    ///// A task that represents the asynchronous operation
    ///// The task result contains the related product list model to add to the product
    ///// </returns>
    public virtual async Task<AddRelatedProductListModel> CustomPrepareAddRelatedProductListModelAsync(AddRelatedProductSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        //a vendor should have access only to his products
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null)
            searchModel.SearchVendorId = currentVendor.Id;

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

        //prepare grid model
        var model = await new AddRelatedProductListModel().PrepareToGridAsync(searchModel, products, () =>
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

    ///// <summary>
    ///// Prepare paged cross-sell product list model
    ///// </summary>
    ///// <param name="searchModel">Cross-sell product search model</param>
    ///// <param name="product">Product</param>
    ///// <returns>
    ///// A task that represents the asynchronous operation
    ///// The task result contains the cross-sell product list model
    ///// </returns>
    public virtual async Task<CrossSellProductListModel> CustomPrepareCrossSellProductListModelAsync(CrossSellProductSearchModel searchModel, Product product)
    {
        ArgumentNullException.ThrowIfNull(searchModel);
        ArgumentNullException.ThrowIfNull(product);

        //get cross-sell products
        var crossSellProducts = (await _productService
            .GetCrossSellProductsByProductId1Async(productId1: product.Id, showHidden: true)).ToPagedList(searchModel);

        //prepare grid model
        var model = await new CrossSellProductListModel().PrepareToGridAsync(searchModel, crossSellProducts, () =>
        {
            return crossSellProducts.SelectAwait(async crossSellProduct =>
            {
                //fill in model values from the entity
                var crossSellProductModel = new CrossSellProductModel
                {
                    Id = crossSellProduct.Id,
                    ProductId2 = crossSellProduct.ProductId2
                };

                //fill in additional values (not existing in the entity)
                crossSellProductModel.Product2Name = (await _productService.GetProductByIdAsync(crossSellProduct.ProductId2))?.Name;
                var defaultProductPicture = (await _pictureService.GetPicturesByProductIdAsync(crossSellProduct.ProductId2, 1)).FirstOrDefault();
                (crossSellProductModel.PictureThumbnailUrl, _) = await _pictureService.GetPictureUrlAsync(defaultProductPicture, 75);
                return crossSellProductModel;
            });
        });

        return model;
    }

    ///// <summary>
    ///// Prepare paged filter level value list model
    ///// </summary>
    ///// <param name="searchModel">Filter level value search model</param>
    ///// <param name="product">Product</param>
    ///// <returns>
    ///// A task that represents the asynchronous operation
    ///// The task result contains the filter level value list model
    ///// </returns>
    //public virtual async Task<FilterLevelValueListModel> PrepareFilterLevelValueListModelAsync(FilterLevelValueSearchModel searchModel, Product product)
    //{
    //    ArgumentNullException.ThrowIfNull(searchModel);
    //    ArgumentNullException.ThrowIfNull(product);

    //    //get filter level values
    //    var filterLevelValues = (await _filterLevelValueService
    //        .GetFilterLevelValuesByProductIdAsync(productId: product.Id)).ToPagedList(searchModel);

    //    var (filterLevel1Disabled, filterLevel2Disabled, filterLevel3Disabled) = _filterLevelValueService.IsFilterLevelDisabled();

    //    //prepare grid model
    //    var model = await new FilterLevelValueListModel().PrepareToGridAsync(searchModel, filterLevelValues, () =>
    //    {
    //        return filterLevelValues.SelectAwait(filterLevelValue =>
    //        {
    //            //fill in model values from the entity
    //            var filterLevelValueModel = new FilterLevelValueModel
    //            {
    //                Id = filterLevelValue.Id,
    //                FilterLevel1Value = filterLevelValue.FilterLevel1Value,
    //                FilterLevel2Value = filterLevelValue.FilterLevel2Value,
    //                FilterLevel3Value = filterLevelValue.FilterLevel3Value,
    //                FilterLevel1ValueEnabled = !filterLevel1Disabled,
    //                FilterLevel2ValueEnabled = !filterLevel2Disabled,
    //                FilterLevel3ValueEnabled = !filterLevel3Disabled
    //            };

    //            return new ValueTask<FilterLevelValueModel>(filterLevelValueModel);
    //        });
    //    });

    //    return model;
    //}

    ///// <summary>
    ///// Prepare cross-sell product search model to add to the product
    ///// </summary>
    ///// <param name="searchModel">Cross-sell product search model to add to the product</param>
    ///// <returns>
    ///// A task that represents the asynchronous operation
    ///// The task result contains the cross-sell product search model to add to the product
    ///// </returns>
    //public virtual async Task<AddCrossSellProductSearchModel> PrepareAddCrossSellProductSearchModelAsync(AddCrossSellProductSearchModel searchModel)
    //{
    //    ArgumentNullException.ThrowIfNull(searchModel);

    //    searchModel.IsLoggedInAsVendor = await _workContext.GetCurrentVendorAsync() != null;

    //    //prepare available categories
    //    await _baseAdminModelFactory.PrepareCategoriesAsync(searchModel.AvailableCategories);

    //    //prepare available manufacturers
    //    await _baseAdminModelFactory.PrepareManufacturersAsync(searchModel.AvailableManufacturers);

    //    //prepare available stores
    //    await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

    //    //prepare available vendors
    //    await _baseAdminModelFactory.PrepareVendorsAsync(searchModel.AvailableVendors);

    //    //prepare available product types
    //    await _baseAdminModelFactory.PrepareProductTypesAsync(searchModel.AvailableProductTypes);

    //    //prepare page parameters
    //    searchModel.SetPopupGridPageSize();

    //    return searchModel;
    //}

    ///// <summary>
    ///// Prepare paged crossSell product list model to add to the product
    ///// </summary>
    ///// <param name="searchModel">CrossSell product search model to add to the product</param>
    ///// <returns>
    ///// A task that represents the asynchronous operation
    ///// The task result contains the crossSell product list model to add to the product
    ///// </returns>
    public virtual async Task<AddCrossSellProductListModel> CustomPrepareAddCrossSellProductListModelAsync(AddCrossSellProductSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        //a vendor should have access only to his products
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null)
            searchModel.SearchVendorId = currentVendor.Id;

        //get products
        var _customProductService = EngineContext.Current.Resolve<IProductExtendedService>();
        var products = await _customProductService.OverriddenSearchProductsAsync(showHidden: true,
            categoryIds: new List<int> { searchModel.SearchCategoryId },
            manufacturerIds: new List<int> { searchModel.SearchManufacturerId },
            storeId: searchModel.SearchStoreId,
            vendorId: searchModel.SearchVendorId,
            productType: searchModel.SearchProductTypeId > 0 ? (ProductType?)searchModel.SearchProductTypeId : null,
            keywords: searchModel.SearchProductName,
            pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        //prepare grid model
        var model = await new AddCrossSellProductListModel().PrepareToGridAsync(searchModel, products, () =>
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

    ///// <summary>
    ///// Prepare paged associated product list model
    ///// </summary>
    ///// <param name="searchModel">Associated product search model</param>
    ///// <param name="product">Product</param>
    ///// <returns>
    ///// A task that represents the asynchronous operation
    ///// The task result contains the associated product list model
    ///// </returns>
    //public virtual async Task<AssociatedProductListModel> PrepareAssociatedProductListModelAsync(AssociatedProductSearchModel searchModel, Product product)
    //{
    //    ArgumentNullException.ThrowIfNull(searchModel);
    //    ArgumentNullException.ThrowIfNull(product);

    //    var vendor = await _workContext.GetCurrentVendorAsync();
    //    //get associated products
    //    var associatedProducts = (await _productService.GetAssociatedProductsAsync(showHidden: true,
    //        parentGroupedProductId: product.Id,
    //        vendorId: vendor?.Id ?? 0)).ToPagedList(searchModel);

    //    //prepare grid model
    //    var model = new AssociatedProductListModel().PrepareToGrid(searchModel, associatedProducts, () =>
    //    {
    //        return associatedProducts.Select(associatedProduct =>
    //        {
    //            var associatedProductModel = associatedProduct.ToModel<AssociatedProductModel>();
    //            associatedProductModel.ProductName = associatedProduct.Name;

    //            return associatedProductModel;
    //        });
    //    });

    //    return model;
    //}

    ///// <summary>
    ///// Prepare associated product search model to add to the product
    ///// </summary>
    ///// <param name="searchModel">Associated product search model to add to the product</param>
    ///// <returns>
    ///// A task that represents the asynchronous operation
    ///// The task result contains the associated product search model to add to the product
    ///// </returns>
    //public virtual async Task<AddAssociatedProductSearchModel> PrepareAddAssociatedProductSearchModelAsync(AddAssociatedProductSearchModel searchModel)
    //{
    //    ArgumentNullException.ThrowIfNull(searchModel);

    //    searchModel.IsLoggedInAsVendor = await _workContext.GetCurrentVendorAsync() != null;

    //    //prepare available categories
    //    await _baseAdminModelFactory.PrepareCategoriesAsync(searchModel.AvailableCategories);

    //    //prepare available manufacturers
    //    await _baseAdminModelFactory.PrepareManufacturersAsync(searchModel.AvailableManufacturers);

    //    //prepare available stores
    //    await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

    //    //prepare available vendors
    //    await _baseAdminModelFactory.PrepareVendorsAsync(searchModel.AvailableVendors);

    //    //prepare available product types
    //    await _baseAdminModelFactory.PrepareProductTypesAsync(searchModel.AvailableProductTypes);

    //    //prepare page parameters
    //    searchModel.SetPopupGridPageSize();

    //    return searchModel;
    //}

    ///// <summary>
    ///// Prepare paged associated product list model to add to the product
    ///// </summary>
    ///// <param name="searchModel">Associated product search model to add to the product</param>
    ///// <returns>
    ///// A task that represents the asynchronous operation
    ///// The task result contains the associated product list model to add to the product
    ///// </returns>
    //public virtual async Task<AddAssociatedProductListModel> PrepareAddAssociatedProductListModelAsync(AddAssociatedProductSearchModel searchModel)
    //{
    //    ArgumentNullException.ThrowIfNull(searchModel);

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null)
    //        searchModel.SearchVendorId = currentVendor.Id;

    //    //get products
    //    var products = await _productService.SearchProductsAsync(showHidden: true,
    //        categoryIds: new List<int> { searchModel.SearchCategoryId },
    //        manufacturerIds: new List<int> { searchModel.SearchManufacturerId },
    //        storeId: searchModel.SearchStoreId,
    //        vendorId: searchModel.SearchVendorId,
    //        productType: searchModel.SearchProductTypeId > 0 ? (ProductType?)searchModel.SearchProductTypeId : null,
    //        keywords: searchModel.SearchProductName,
    //        pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

    //    //prepare grid model
    //    var model = await new AddAssociatedProductListModel().PrepareToGridAsync(searchModel, products, () =>
    //    {
    //        return products.SelectAwait(async product =>
    //        {
    //            //fill in model values from the entity
    //            var productModel = product.ToModel<ProductModel>();

    //            //fill in additional values (not existing in the entity)
    //            productModel.SeName = await _urlRecordService.GetSeNameAsync(product, 0, true, false);
    //            var parentGroupedProduct = await _productService.GetProductByIdAsync(product.ParentGroupedProductId);

    //            if (parentGroupedProduct == null)
    //                return productModel;

    //            productModel.AssociatedToProductId = product.ParentGroupedProductId;
    //            productModel.AssociatedToProductName = parentGroupedProduct.Name;

    //            return productModel;
    //        });
    //    });

    //    return model;
    //}

    ///// <summary>
    ///// Prepare paged product picture list model
    ///// </summary>
    ///// <param name="searchModel">Product picture search model</param>
    ///// <param name="product">Product</param>
    ///// <returns>
    ///// A task that represents the asynchronous operation
    ///// The task result contains the product picture list model
    ///// </returns>
    public virtual async Task<LogProductPictureListModel> PrepareProductPictureListModelAsync(LogProductPictureSearchModel searchModel, Product product)
    {
        ArgumentNullException.ThrowIfNull(searchModel);
        ArgumentNullException.ThrowIfNull(product);

        //get product pictures
        var _customPictureService = EngineContext.Current.Resolve<IPictureExtendedService>();
        var productPictures = (await _customPictureService.GetPictureMappingLogsByProductId(product.Id)).ToPagedList(searchModel);

        //prepare grid model
        var model = await new LogProductPictureListModel().PrepareToGridAsync(searchModel, productPictures, () =>
        {
            return productPictures.SelectAwait(async productPicture =>
            {
                //fill in model values from the entity
                var productPictureModel = productPicture.ToModel<LogProductPictureModel>();

                //fill in additional values (not existing in the entity)
                var logPicture = (await _customPictureService.GetPictureLogById(productPicture.PictureId))
                              ?? throw new Exception("Picture cannot be loaded");

                productPictureModel.PictureUrl = (await _pictureService.GetPictureUrlAsync(new Nop.Core.Domain.Media.Picture()
                {
                    AltAttribute = logPicture.AltAttribute,
                    Id = logPicture.ReferenceId,
                    IsNew = false,
                    MimeType = logPicture.MimeType,
                    SeoFilename = logPicture.SeoFilename,
                    TitleAttribute = logPicture.TitleAttribute,
                    VirtualPath = logPicture.VirtualPath

                })).Url;

                productPictureModel.OverrideAltAttribute = logPicture.AltAttribute;
                productPictureModel.OverrideTitleAttribute = logPicture.TitleAttribute;
                var customer = await _customerService.GetCustomerByIdAsync(productPicture.CustomerId);
                if (customer != null)
                {
                    productPictureModel.UserName = await _customerService.GetCustomerFullNameAsync(customer);
                }
                return productPictureModel;
            });
        });

        return model;
    }

    ///// <summary>
    ///// Prepare paged product video list model
    ///// </summary>
    ///// <param name="searchModel">Product video search model</param>
    ///// <param name="product">Product</param>
    ///// <returns>
    ///// A task that represents the asynchronous operation
    ///// The task result contains the product video list model
    ///// </returns>
    //public virtual async Task<ProductVideoListModel> PrepareProductVideoListModelAsync(ProductVideoSearchModel searchModel, Product product)
    //{
    //    ArgumentNullException.ThrowIfNull(searchModel);
    //    ArgumentNullException.ThrowIfNull(product);

    //    //get product videos
    //    var productVideos = (await _productService.GetProductVideosByProductIdAsync(product.Id)).ToPagedList(searchModel);

    //    //prepare grid model
    //    var model = await new ProductVideoListModel().PrepareToGridAsync(searchModel, productVideos, () =>
    //    {
    //        return productVideos.SelectAwait(async productVideo =>
    //        {
    //            //fill in model values from the entity
    //            var productVideoModel = productVideo.ToModel<ProductVideoModel>();

    //            //fill in additional values (not existing in the entity)
    //            var video = (await _videoService.GetVideoByIdAsync(productVideo.VideoId))
    //                        ?? throw new Exception("Video cannot be loaded");

    //            productVideoModel.VideoUrl = video.VideoUrl;

    //            return productVideoModel;
    //        });
    //    });

    //    return model;
    //}

    ///// <summary>
    ///// Prepare paged product specification attribute list model
    ///// </summary>
    ///// <param name="searchModel">Product specification attribute search model</param>
    ///// <param name="product">Product</param>
    ///// <returns>
    ///// A task that represents the asynchronous operation
    ///// The task result contains the product specification attribute list model
    ///// </returns>
    //public virtual async Task<ProductSpecificationAttributeListModel> PrepareProductSpecificationAttributeListModelAsync(
    //    ProductSpecificationAttributeSearchModel searchModel, Product product)
    //{
    //    ArgumentNullException.ThrowIfNull(searchModel);
    //    ArgumentNullException.ThrowIfNull(product);

    //    //get product specification attributes
    //    var productSpecificationAttributes = (await _specificationAttributeService
    //        .GetProductSpecificationAttributesAsync(product.Id)).ToPagedList(searchModel);

    //    //prepare grid model
    //    var model = await new ProductSpecificationAttributeListModel().PrepareToGridAsync(searchModel, productSpecificationAttributes, () =>
    //    {
    //        return productSpecificationAttributes.SelectAwait(async attribute =>
    //        {
    //            //fill in model values from the entity
    //            var productSpecificationAttributeModel = attribute.ToModel<ProductSpecificationAttributeModel>();

    //            var specAttributeOption = await _specificationAttributeService
    //                .GetSpecificationAttributeOptionByIdAsync(attribute.SpecificationAttributeOptionId);
    //            var specAttribute = await _specificationAttributeService
    //                .GetSpecificationAttributeByIdAsync(specAttributeOption.SpecificationAttributeId);

    //            //fill in additional values (not existing in the entity)
    //            productSpecificationAttributeModel.AttributeTypeName = await _localizationService.GetLocalizedEnumAsync(attribute.AttributeType);

    //            productSpecificationAttributeModel.AttributeId = specAttribute.Id;
    //            productSpecificationAttributeModel.AttributeName = await GetSpecificationAttributeNameAsync(specAttribute);
    //            var currentLanguage = await _workContext.GetWorkingLanguageAsync();

    //            switch (attribute.AttributeType)
    //            {
    //                case SpecificationAttributeType.Option:
    //                    productSpecificationAttributeModel.ValueRaw = WebUtility.HtmlEncode(specAttributeOption.Name);
    //                    productSpecificationAttributeModel.SpecificationAttributeOptionId = specAttributeOption.Id;
    //                    break;
    //                case SpecificationAttributeType.CustomText:
    //                    productSpecificationAttributeModel.ValueRaw = WebUtility.HtmlEncode(await _localizationService.GetLocalizedAsync(attribute, x => x.CustomValue, currentLanguage?.Id));
    //                    break;
    //                case SpecificationAttributeType.CustomHtmlText:
    //                    productSpecificationAttributeModel.ValueRaw = await _localizationService
    //                        .GetLocalizedAsync(attribute, x => x.CustomValue, currentLanguage?.Id);
    //                    break;
    //                case SpecificationAttributeType.Hyperlink:
    //                    productSpecificationAttributeModel.ValueRaw = attribute.CustomValue;
    //                    break;
    //            }

    //            return productSpecificationAttributeModel;
    //        });
    //    });

    //    return model;
    //}

    ///// <summary>
    ///// Prepare paged product specification attribute model
    ///// </summary>
    ///// <param name="productId">Product id</param>
    ///// <param name="specificationId">Specification attribute id</param>
    ///// <returns>
    ///// A task that represents the asynchronous operation
    ///// The task result contains the product specification attribute model
    ///// </returns>
    //public virtual async Task<AddSpecificationAttributeModel> PrepareAddSpecificationAttributeModelAsync(int productId, int? specificationId)
    //{
    //    if (!specificationId.HasValue)
    //    {
    //        return new AddSpecificationAttributeModel
    //        {
    //            AvailableAttributes = await (await _specificationAttributeService.GetSpecificationAttributesWithOptionsAsync())
    //                .SelectAwait(async attributeWithOption =>
    //                {
    //                    var attributeName = await GetSpecificationAttributeNameAsync(attributeWithOption);

    //                    return new SelectListItem(attributeName, attributeWithOption.Id.ToString());
    //                }).ToListAsync(),
    //            ProductId = productId,
    //            Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync<AddSpecificationAttributeLocalizedModel>()
    //        };
    //    }

    //    var attribute = await _specificationAttributeService.GetProductSpecificationAttributeByIdAsync(specificationId.Value)
    //                    ?? throw new ArgumentException("No specification attribute found with the specified id");

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null && (await _productService.GetProductByIdAsync(attribute.ProductId)).VendorId != currentVendor.Id)
    //        throw new UnauthorizedAccessException("This is not your product");

    //    var specAttributeOption = await _specificationAttributeService.GetSpecificationAttributeOptionByIdAsync(attribute.SpecificationAttributeOptionId);
    //    var specAttribute = await _specificationAttributeService.GetSpecificationAttributeByIdAsync(specAttributeOption.SpecificationAttributeId);

    //    var model = attribute.ToModel<AddSpecificationAttributeModel>();
    //    model.SpecificationId = attribute.Id;
    //    model.AttributeId = specAttribute.Id;
    //    model.AttributeTypeName = await _localizationService.GetLocalizedEnumAsync(attribute.AttributeType);
    //    model.AttributeName = specAttribute.Name;

    //    model.AvailableAttributes = await (await _specificationAttributeService.GetSpecificationAttributesWithOptionsAsync())
    //        .SelectAwait(async attributeWithOption =>
    //        {
    //            var attributeName = await GetSpecificationAttributeNameAsync(attributeWithOption);

    //            return new SelectListItem(attributeName, attributeWithOption.Id.ToString());
    //        })
    //        .ToListAsync();

    //    model.AvailableOptions = (await _specificationAttributeService
    //            .GetSpecificationAttributeOptionsBySpecificationAttributeAsync(model.AttributeId))
    //        .Select(option => new SelectListItem { Text = option.Name, Value = option.Id.ToString() })
    //        .ToList();

    //    switch (attribute.AttributeType)
    //    {
    //        case SpecificationAttributeType.Option:
    //            model.ValueRaw = WebUtility.HtmlEncode(specAttributeOption.Name);
    //            model.SpecificationAttributeOptionId = specAttributeOption.Id;
    //            break;
    //        case SpecificationAttributeType.CustomText:
    //            model.Value = WebUtility.HtmlDecode(attribute.CustomValue);
    //            break;
    //        case SpecificationAttributeType.CustomHtmlText:
    //            model.ValueRaw = attribute.CustomValue;
    //            break;
    //        case SpecificationAttributeType.Hyperlink:
    //            model.Value = attribute.CustomValue;
    //            break;
    //        default:
    //            throw new ArgumentOutOfRangeException(nameof(attribute.AttributeType));
    //    }

    //    model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(
    //        async (AddSpecificationAttributeLocalizedModel locale, int languageId) =>
    //        {
    //            switch (attribute.AttributeType)
    //            {
    //                case SpecificationAttributeType.CustomHtmlText:
    //                    locale.ValueRaw = await _localizationService.GetLocalizedAsync(attribute, entity => entity.CustomValue, languageId, false, false);
    //                    break;
    //                case SpecificationAttributeType.CustomText:
    //                    locale.Value = await _localizationService.GetLocalizedAsync(attribute, entity => entity.CustomValue, languageId, false, false);
    //                    break;
    //                case SpecificationAttributeType.Option:
    //                    break;
    //                case SpecificationAttributeType.Hyperlink:
    //                    break;
    //                default:
    //                    throw new ArgumentOutOfRangeException();
    //            }
    //        });

    //    return model;
    //}

    ///// <summary>
    ///// Prepare product tag search model
    ///// </summary>
    ///// <param name="searchModel">Product tag search model</param>
    ///// <returns>
    ///// A task that represents the asynchronous operation
    ///// The task result contains the product tag search model
    ///// </returns>
    //public virtual Task<ProductTagSearchModel> PrepareProductTagSearchModelAsync(ProductTagSearchModel searchModel)
    //{
    //    ArgumentNullException.ThrowIfNull(searchModel);

    //    //prepare page parameters
    //    searchModel.SetGridPageSize();

    //    return Task.FromResult(searchModel);
    //}

    ///// <summary>
    ///// Prepare paged product tag list model
    ///// </summary>
    ///// <param name="searchModel">Product tag search model</param>
    ///// <returns>
    ///// A task that represents the asynchronous operation
    ///// The task result contains the product tag list model
    ///// </returns>
    //public virtual async Task<ProductTagListModel> PrepareProductTagListModelAsync(ProductTagSearchModel searchModel)
    //{
    //    ArgumentNullException.ThrowIfNull(searchModel);

    //    //get product tags
    //    var productTags = (await (await _productTagService.GetAllProductTagsAsync(tagName: searchModel.SearchTagName))
    //            .OrderByDescendingAwait(async tag => await _productTagService.GetProductCountByProductTagIdAsync(tag.Id, storeId: 0, showHidden: true)).ToListAsync())
    //        .ToPagedList(searchModel);

    //    //prepare list model
    //    var model = await new ProductTagListModel().PrepareToGridAsync(searchModel, productTags, () =>
    //    {
    //        return productTags.SelectAwait(async tag =>
    //        {
    //            //fill in model values from the entity
    //            var productTagModel = tag.ToModel<ProductTagModel>();

    //            //fill in additional values (not existing in the entity)
    //            productTagModel.ProductCount = await _productTagService.GetProductCountByProductTagIdAsync(tag.Id, storeId: 0, showHidden: true);

    //            return productTagModel;
    //        });
    //    });

    //    return model;
    //}

    ///// <summary>
    ///// Prepare product tag model
    ///// </summary>
    ///// <param name="model">Product tag model</param>
    ///// <param name="productTag">Product tag</param>
    ///// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
    ///// <returns>
    ///// A task that represents the asynchronous operation
    ///// The task result contains the product tag model
    ///// </returns>
    //public virtual async Task<ProductTagModel> PrepareProductTagModelAsync(ProductTagModel model, ProductTag productTag, bool excludeProperties = false)
    //{
    //    Func<ProductTagLocalizedModel, int, Task> localizedModelConfiguration = null;

    //    if (productTag != null)
    //    {
    //        //fill in model values from the entity
    //        if (model == null)
    //        {
    //            model = productTag.ToModel<ProductTagModel>();
    //        }

    //        model.ProductCount = await _productTagService.GetProductCountByProductTagIdAsync(productTag.Id, storeId: 0, showHidden: true);

    //        //define localized model configuration action
    //        localizedModelConfiguration = async (locale, languageId) =>
    //        {
    //            locale.Name = await _localizationService.GetLocalizedAsync(productTag, entity => entity.Name, languageId, false, false);
    //            locale.MetaKeywords = await _localizationService.GetLocalizedAsync(productTag, entity => entity.MetaKeywords, languageId, false, false);
    //            locale.MetaDescription = await _localizationService.GetLocalizedAsync(productTag, entity => entity.MetaDescription, languageId, false, false);
    //            locale.MetaTitle = await _localizationService.GetLocalizedAsync(productTag, entity => entity.MetaTitle, languageId, false, false);
    //        };
    //    }

    //    PrepareTaggedProductsSearchModel(model.ProductTagProductSearchModel, productTag);

    //    //prepare localized models
    //    if (!excludeProperties)
    //        model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);

    //    return model;
    //}

    ///// <summary>
    ///// Prepare tagged product list model
    ///// </summary>
    ///// <param name="searchModel">Product search model</param>
    ///// <returns>
    ///// A task that represents the asynchronous operation
    ///// The task result contains list model for the tagged products
    ///// </returns>
    //public virtual async Task<ProductTagProductListModel> PrepareTaggedProductListModelAsync(ProductTagProductSearchModel searchModel)
    //{
    //    ArgumentNullException.ThrowIfNull(searchModel);

    //    //get products by tag
    //    var products = await _productService.SearchProductsAsync(
    //            productTagId: searchModel.ProductTagId,
    //            showHidden: true,
    //            pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

    //    //prepare list model
    //    return new ProductTagProductListModel().PrepareToGrid(searchModel, products, () => products.Select(product => product.ToModel<ProductModel>()));
    //}

    ///// <summary>
    ///// Prepare paged product order list model
    ///// </summary>
    ///// <param name="searchModel">Product order search model</param>
    ///// <param name="product">Product</param>
    ///// <returns>
    ///// A task that represents the asynchronous operation
    ///// The task result contains the product order list model
    ///// </returns>
    //public virtual async Task<ProductOrderListModel> PrepareProductOrderListModelAsync(ProductOrderSearchModel searchModel, Product product)
    //{
    //    ArgumentNullException.ThrowIfNull(searchModel);
    //    ArgumentNullException.ThrowIfNull(product);

    //    //get orders
    //    var orders = await _orderService.SearchOrdersAsync(productId: searchModel.ProductId,
    //        pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

    //    //prepare grid model
    //    var model = await new ProductOrderListModel().PrepareToGridAsync(searchModel, orders, () =>
    //    {
    //        return orders.SelectAwait(async order =>
    //        {
    //            var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);

    //            //fill in model values from the entity
    //            var orderModel = new OrderModel
    //            {
    //                Id = order.Id,
    //                CustomerEmail = billingAddress.Email,
    //                CustomOrderNumber = order.CustomOrderNumber
    //            };

    //            //convert dates to the user time
    //            orderModel.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(order.CreatedOnUtc, DateTimeKind.Utc);

    //            //fill in additional values (not existing in the entity)
    //            orderModel.StoreName = (await _storeService.GetStoreByIdAsync(order.StoreId))?.Name ?? "Deleted";
    //            orderModel.OrderStatus = await _localizationService.GetLocalizedEnumAsync(order.OrderStatus);
    //            orderModel.PaymentStatus = await _localizationService.GetLocalizedEnumAsync(order.PaymentStatus);
    //            orderModel.ShippingStatus = await _localizationService.GetLocalizedEnumAsync(order.ShippingStatus);

    //            return orderModel;
    //        });
    //    });

    //    return model;
    //}

    ///// <summary>
    ///// Prepare paged tier price list model
    ///// </summary>
    ///// <param name="searchModel">Tier price search model</param>
    ///// <param name="product">Product</param>
    ///// <returns>
    ///// A task that represents the asynchronous operation
    ///// The task result contains the ier price list model
    ///// </returns>
    //public virtual async Task<TierPriceListModel> PrepareTierPriceListModelAsync(TierPriceSearchModel searchModel, Product product)
    //{
    //    ArgumentNullException.ThrowIfNull(searchModel);
    //    ArgumentNullException.ThrowIfNull(product);

    //    //get tier prices
    //    var tierPrices = (await _productService.GetTierPricesByProductAsync(product.Id))
    //        .OrderBy(price => price.StoreId).ThenBy(price => price.Quantity).ThenBy(price => price.CustomerRoleId)
    //        .ToList().ToPagedList(searchModel);

    //    //prepare grid model
    //    var model = await new TierPriceListModel().PrepareToGridAsync(searchModel, tierPrices, () =>
    //    {
    //        return tierPrices.SelectAwait(async price =>
    //        {
    //            //fill in model values from the entity
    //            var tierPriceModel = price.ToModel<TierPriceModel>();

    //            //fill in additional values (not existing in the entity)   
    //            tierPriceModel.Store = price.StoreId > 0
    //                ? ((await _storeService.GetStoreByIdAsync(price.StoreId))?.Name ?? "Deleted")
    //                : await _localizationService.GetResourceAsync("Admin.Catalog.Products.TierPrices.Fields.Store.All");
    //            tierPriceModel.CustomerRoleId = price.CustomerRoleId ?? 0;
    //            tierPriceModel.CustomerRole = price.CustomerRoleId.HasValue
    //                ? (await _customerService.GetCustomerRoleByIdAsync(price.CustomerRoleId.Value))?.Name
    //                : await _localizationService.GetResourceAsync("Admin.Catalog.Products.TierPrices.Fields.CustomerRole.All");

    //            tierPriceModel.FormattedPrice = await _priceFormatter.FormatPriceAsync(price.Price);

    //            return tierPriceModel;
    //        });
    //    });

    //    return model;
    //}

    ///// <summary>
    ///// Prepare tier price model
    ///// </summary>
    ///// <param name="model">Tier price model</param>
    ///// <param name="product">Product</param>
    ///// <param name="tierPrice">Tier price</param>
    ///// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
    ///// <returns>
    ///// A task that represents the asynchronous operation
    ///// The task result contains the ier price model
    ///// </returns>
    //public virtual async Task<TierPriceModel> PrepareTierPriceModelAsync(TierPriceModel model,
    //    Product product, TierPrice tierPrice, bool excludeProperties = false)
    //{
    //    ArgumentNullException.ThrowIfNull(product);

    //    if (tierPrice != null)
    //    {
    //        //fill in model values from the entity
    //        if (model == null)
    //            model = tierPrice.ToModel<TierPriceModel>();
    //    }

    //    model.PrimaryStoreCurrencyCode = (await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId)).CurrencyCode;

    //    //prepare available stores
    //    await _baseAdminModelFactory.PrepareStoresAsync(model.AvailableStores);

    //    //prepare available customer roles
    //    await _baseAdminModelFactory.PrepareCustomerRolesAsync(model.AvailableCustomerRoles);

    //    return model;
    //}

    ///// <summary>
    ///// Prepare paged stock quantity history list model
    ///// </summary>
    ///// <param name="searchModel">Stock quantity history search model</param>
    ///// <param name="product">Product</param>
    ///// <returns>
    ///// A task that represents the asynchronous operation
    ///// The task result contains the stock quantity history list model
    ///// </returns>
    //public virtual async Task<StockQuantityHistoryListModel> PrepareStockQuantityHistoryListModelAsync(StockQuantityHistorySearchModel searchModel, Product product)
    //{
    //    ArgumentNullException.ThrowIfNull(searchModel);
    //    ArgumentNullException.ThrowIfNull(product);

    //    //get stock quantity history
    //    var stockQuantityHistory = await _productService.GetStockQuantityHistoryAsync(product: product,
    //        warehouseId: searchModel.WarehouseId,
    //        pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

    //    var currentCustomer = await _workContext.GetCurrentCustomerAsync();
    //    var currentStore = await _storeContext.GetCurrentStoreAsync();

    //    //prepare grid model
    //    var model = await new StockQuantityHistoryListModel().PrepareToGridAsync(searchModel, stockQuantityHistory, () =>
    //    {
    //        return stockQuantityHistory.SelectAwait(async historyEntry =>
    //        {
    //            //fill in model values from the entity
    //            var stockQuantityHistoryModel = historyEntry.ToModel<StockQuantityHistoryModel>();

    //            //convert dates to the user time
    //            stockQuantityHistoryModel.CreatedOn =
    //                await _dateTimeHelper.ConvertToUserTimeAsync(historyEntry.CreatedOnUtc, DateTimeKind.Utc);

    //            //fill in additional values (not existing in the entity)
    //            var combination = await _productAttributeService.GetProductAttributeCombinationByIdAsync(historyEntry.CombinationId ?? 0);
    //            if (combination != null)
    //            {
    //                stockQuantityHistoryModel.AttributeCombination = await _productAttributeFormatter
    //                    .FormatAttributesAsync(product, combination.AttributesXml, currentCustomer, currentStore, renderGiftCardAttributes: false);
    //            }

    //            stockQuantityHistoryModel.WarehouseName = historyEntry.WarehouseId.HasValue
    //                ? (await _warehouseService.GetWarehouseByIdAsync(historyEntry.WarehouseId.Value))?.Name ?? "Deleted"
    //                : await _localizationService.GetResourceAsync("Admin.Catalog.Products.Fields.Warehouse.None");

    //            return stockQuantityHistoryModel;
    //        });
    //    });

    //    return model;
    //}

    ///// <summary>
    ///// Prepare paged product attribute mapping list model
    ///// </summary>
    ///// <param name="searchModel">Product attribute mapping search model</param>
    ///// <param name="product">Product</param>
    ///// <returns>
    ///// A task that represents the asynchronous operation
    ///// The task result contains the product attribute mapping list model
    ///// </returns>
    public virtual async Task<ProductAttributeMappingListModel> CustomPrepareProductAttributeMappingListModelAsync(ProductAttributeMappingSearchModel searchModel,
        Product product)
    {
        ArgumentNullException.ThrowIfNull(searchModel);
        ArgumentNullException.ThrowIfNull(product);

        //get product attribute mappings
        var productAttributeMappings = (await _productAttributeService
            .GetProductAttributeMappingsByProductIdAsync(product.Id)).ToPagedList(searchModel);

        //prepare grid model
        var model = await new ProductAttributeMappingListModel().PrepareToGridAsync(searchModel, productAttributeMappings, () =>
        {
            return productAttributeMappings.SelectAwait(async attributeMapping =>
            {
                //fill in model values from the entity
                var productAttributeMappingModel = attributeMapping.ToModel<ProductAttributeMappingModel>();

                //fill in additional values (not existing in the entity)
                productAttributeMappingModel.ConditionString = string.Empty;

                productAttributeMappingModel.ValidationRulesString = await PrepareProductAttributeMappingValidationRulesStringAsync(attributeMapping);
                productAttributeMappingModel.ProductAttribute = (await _productAttributeService.GetProductAttributeByIdAsync(attributeMapping.ProductAttributeId))?.Name;
                productAttributeMappingModel.AttributeControlType = await _localizationService.GetLocalizedEnumAsync(attributeMapping.AttributeControlType);
                var conditionAttribute = (await _productAttributeParser
                        .ParseProductAttributeMappingsAsync(attributeMapping.ConditionAttributeXml))
                    .FirstOrDefault();
                productAttributeMappingModel.Published = (await _productAttributeService.GetProductAttributeValuesAsync(attributeMapping.Id)).Where(m => m.Published).Any();
                if (conditionAttribute == null)
                    return productAttributeMappingModel;

                var conditionValue = (await _productAttributeParser
                        .ParseProductAttributeValuesAsync(attributeMapping.ConditionAttributeXml))
                    .FirstOrDefault();
                if (conditionValue != null)
                {
                    productAttributeMappingModel.ConditionString =
                        $"{WebUtility.HtmlEncode((await _productAttributeService.GetProductAttributeByIdAsync(conditionAttribute.ProductAttributeId)).Name)}: {WebUtility.HtmlEncode(conditionValue.Name)}";
                }

                return productAttributeMappingModel;
            });
        });

        return model;
    }

    ///// <summary>
    ///// Prepare product attribute mapping model
    ///// </summary>
    ///// <param name="model">Product attribute mapping model</param>
    ///// <param name="product">Product</param>
    ///// <param name="productAttributeMapping">Product attribute mapping</param>
    ///// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
    ///// <returns>
    ///// A task that represents the asynchronous operation
    ///// The task result contains the product attribute mapping model
    ///// </returns>
    //public virtual async Task<ProductAttributeMappingModel> PrepareProductAttributeMappingModelAsync(ProductAttributeMappingModel model,
    //    Product product, ProductAttributeMapping productAttributeMapping, bool excludeProperties = false)
    //{
    //    Func<ProductAttributeMappingLocalizedModel, int, Task> localizedModelConfiguration = null;

    //    ArgumentNullException.ThrowIfNull(product);

    //    if (productAttributeMapping != null)
    //    {
    //        //fill in model values from the entity
    //        model ??= new ProductAttributeMappingModel
    //        {
    //            Id = productAttributeMapping.Id
    //        };

    //        model.ProductAttribute = (await _productAttributeService.GetProductAttributeByIdAsync(productAttributeMapping.ProductAttributeId)).Name;
    //        model.AttributeControlType = await _localizationService.GetLocalizedEnumAsync(productAttributeMapping.AttributeControlType);

    //        if (!excludeProperties)
    //        {
    //            model.ProductAttributeId = productAttributeMapping.ProductAttributeId;
    //            model.TextPrompt = productAttributeMapping.TextPrompt;
    //            model.IsRequired = productAttributeMapping.IsRequired;
    //            model.AttributeControlTypeId = productAttributeMapping.AttributeControlTypeId;
    //            model.DisplayOrder = productAttributeMapping.DisplayOrder;
    //            model.ValidationMinLength = productAttributeMapping.ValidationMinLength;
    //            model.ValidationMaxLength = productAttributeMapping.ValidationMaxLength;
    //            model.ValidationFileAllowedExtensions = productAttributeMapping.ValidationFileAllowedExtensions;
    //            model.ValidationFileMaximumSize = productAttributeMapping.ValidationFileMaximumSize;
    //            model.DefaultValue = productAttributeMapping.DefaultValue;
    //        }

    //        //prepare condition attributes model
    //        model.ConditionAllowed = true;
    //        await PrepareProductAttributeConditionModelAsync(model.ConditionModel, productAttributeMapping);

    //        //define localized model configuration action
    //        localizedModelConfiguration = async (locale, languageId) =>
    //        {
    //            locale.TextPrompt = await _localizationService.GetLocalizedAsync(productAttributeMapping, entity => entity.TextPrompt, languageId, false, false);
    //            locale.DefaultValue = await _localizationService.GetLocalizedAsync(productAttributeMapping, entity => entity.DefaultValue, languageId, false, false);
    //        };

    //        //prepare nested search model
    //        PrepareProductAttributeValueSearchModel(model.ProductAttributeValueSearchModel, productAttributeMapping);
    //    }

    //    model.ProductId = product.Id;

    //    //prepare localized models
    //    if (!excludeProperties)
    //        model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);

    //    //prepare available product attributes
    //    model.AvailableProductAttributes = (await _productAttributeService.GetAllProductAttributesAsync()).Select(productAttribute => new SelectListItem
    //    {
    //        Text = productAttribute.Name,
    //        Value = productAttribute.Id.ToString()
    //    }).ToList();

    //    await _baseAdminModelFactory.PreparePreTranslationSupportModelAsync(model);

    //    return model;
    //}

    ///// <summary>
    ///// Prepare paged product attribute value list model
    ///// </summary>
    ///// <param name="searchModel">Product attribute value search model</param>
    ///// <param name="productAttributeMapping">Product attribute mapping</param>
    ///// <returns>
    ///// A task that represents the asynchronous operation
    ///// The task result contains the product attribute value list model
    ///// </returns>
    //public virtual async Task<ProductAttributeValueListModel> PrepareProductAttributeValueListModelAsync(ProductAttributeValueSearchModel searchModel,
    //    ProductAttributeMapping productAttributeMapping)
    //{
    //    ArgumentNullException.ThrowIfNull(searchModel);
    //    ArgumentNullException.ThrowIfNull(productAttributeMapping);

    //    //get product attribute values
    //    var productAttributeValues = (await _productAttributeService
    //        .GetProductAttributeValuesAsync(productAttributeMapping.Id)).ToPagedList(searchModel);

    //    //prepare list model
    //    var model = await new ProductAttributeValueListModel().PrepareToGridAsync(searchModel, productAttributeValues, () =>
    //    {
    //        return productAttributeValues.SelectAwait(async value =>
    //        {
    //            //fill in model values from the entity
    //            var productAttributeValueModel = value.ToModel<ProductAttributeValueModel>();

    //            //fill in additional values (not existing in the entity)
    //            productAttributeValueModel.AttributeValueTypeName = await _localizationService.GetLocalizedEnumAsync(value.AttributeValueType);

    //            productAttributeValueModel.Name = productAttributeMapping.AttributeControlType != AttributeControlType.ColorSquares
    //                ? value.Name : $"{value.Name} - {value.ColorSquaresRgb}";
    //            if (value.AttributeValueType == AttributeValueType.Simple)
    //            {
    //                productAttributeValueModel.PriceAdjustmentStr = value.PriceAdjustment.ToString("G29");
    //                if (value.PriceAdjustmentUsePercentage)
    //                    productAttributeValueModel.PriceAdjustmentStr += " %";
    //                productAttributeValueModel.WeightAdjustmentStr = value.WeightAdjustment.ToString("G29");
    //            }

    //            if (value.AttributeValueType == AttributeValueType.AssociatedToProduct)
    //            {
    //                productAttributeValueModel.AssociatedProductName = (await _productService.GetProductByIdAsync(value.AssociatedProductId))?.Name ?? string.Empty;
    //            }

    //            var valuePicture = (await _productAttributeService.GetProductAttributeValuePicturesAsync(value.Id)).FirstOrDefault();
    //            var pictureThumbnailUrl = await _pictureService.GetPictureUrlAsync(valuePicture?.PictureId ?? 0, 75, false);

    //            //little hack here. Grid is rendered wrong way with <img> without "src" attribute
    //            if (string.IsNullOrEmpty(pictureThumbnailUrl))
    //                pictureThumbnailUrl = await _pictureService.GetDefaultPictureUrlAsync(targetSize: 1);

    //            productAttributeValueModel.PictureThumbnailUrl = pictureThumbnailUrl;

    //            return productAttributeValueModel;
    //        });
    //    });

    //    return model;
    //}

    ///// <summary>
    ///// Prepare product attribute value model
    ///// </summary>
    ///// <param name="model">Product attribute value model</param>
    ///// <param name="productAttributeMapping">Product attribute mapping</param>
    ///// <param name="productAttributeValue">Product attribute value</param>
    ///// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
    ///// <returns>
    ///// A task that represents the asynchronous operation
    ///// The task result contains the product attribute value model
    ///// </returns>
    #region Product Attribute Value
    public virtual async Task<ProductAttributeValueModel> CustomPrepareProductAttributeValueModelAsync(ProductAttributeValueModel model,
        ProductAttributeMapping productAttributeMapping, ProductAttributeValue productAttributeValue, bool excludeProperties = false)
    {
        ArgumentNullException.ThrowIfNull(productAttributeMapping);

        Func<ProductAttributeValueLocalizedModel, int, Task> localizedModelConfiguration = null;

        if (productAttributeValue != null)
        {
            //fill in model values from the entity
            model ??= new ProductAttributeValueModel
            {
                ProductAttributeMappingId = productAttributeValue.ProductAttributeMappingId,
                AttributeValueTypeId = productAttributeValue.AttributeValueTypeId,
                AttributeValueTypeName = await _localizationService.GetLocalizedEnumAsync(productAttributeValue.AttributeValueType),
                AssociatedProductId = productAttributeValue.AssociatedProductId,
                Name = productAttributeValue.Name,
                ColorSquaresRgb = productAttributeValue.ColorSquaresRgb,
                DisplayColorSquaresRgb = productAttributeMapping.AttributeControlType == AttributeControlType.ColorSquares,
                ImageSquaresPictureId = productAttributeValue.ImageSquaresPictureId,
                DisplayImageSquaresPicture = productAttributeMapping.AttributeControlType == AttributeControlType.ImageSquares,
                PriceAdjustment = productAttributeValue.PriceAdjustment,
                PriceAdjustmentUsePercentage = productAttributeValue.PriceAdjustmentUsePercentage,
                WeightAdjustment = productAttributeValue.WeightAdjustment,
                Cost = productAttributeValue.Cost,
                CustomerEntersQty = productAttributeValue.CustomerEntersQty,
                Quantity = productAttributeValue.Quantity,
                IsPreSelected = productAttributeValue.IsPreSelected,
                DisplayOrder = productAttributeValue.DisplayOrder,
                PictureIds = (await _productAttributeService.GetProductAttributeValuePicturesAsync(productAttributeValue.Id))
                    .Select(c => c.PictureId).ToList(),
                VariantTitle = productAttributeValue.VariantTitle,
                Dimension = productAttributeValue.Dimension,
                VariantDimension = productAttributeValue.VariantDimension,
                FeaturedPictureId = productAttributeValue.FeaturedPictureId,
                ManufacturerPartNumber = productAttributeValue.ManufacturerPartNumber,
                VariantId = productAttributeValue.VariantId,
                Published = productAttributeValue.Published,
                QueryParameter = productAttributeValue.QueryParameter
            };

            model.AssociatedProductName = (await _productService.GetProductByIdAsync(productAttributeValue.AssociatedProductId))?.Name;

            //define localized model configuration action
            localizedModelConfiguration = async (locale, languageId) =>
            {
                locale.Name = await _localizationService.GetLocalizedAsync(productAttributeValue, entity => entity.Name, languageId, false, false);
            };
        }

        model.ProductAttributeMappingId = productAttributeMapping.Id;
        model.DisplayColorSquaresRgb = productAttributeMapping.AttributeControlType == AttributeControlType.ColorSquares;
        model.DisplayImageSquaresPicture = productAttributeMapping.AttributeControlType == AttributeControlType.ImageSquares;

        //set default values for the new model
        if (productAttributeValue == null)
            model.Quantity = 1;

        //prepare localized models
        if (!excludeProperties)
            model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);

        //prepare picture models
        var productPictures = await _productService.GetProductPicturesByProductIdAsync(productAttributeMapping.ProductId);
        model.ProductPictureModels = await productPictures.SelectAwait(async productPicture => new ProductPictureModel
        {
            Id = productPicture.Id,
            ProductId = productPicture.ProductId,
            PictureId = productPicture.PictureId,
            PictureUrl = await _pictureService.GetPictureUrlAsync(productPicture.PictureId),
            DisplayOrder = productPicture.DisplayOrder
        }).ToListAsync();

        return model;
    }
    #endregion
    ///// <summary>
    ///// Prepare product model to associate to the product attribute value
    ///// </summary>
    ///// <param name="searchModel">Product model to associate to the product attribute value</param>
    ///// <returns>
    ///// A task that represents the asynchronous operation
    ///// The task result contains the product model to associate to the product attribute value
    ///// </returns>
    //public virtual async Task<AssociateProductToAttributeValueSearchModel> PrepareAssociateProductToAttributeValueSearchModelAsync(
    //    AssociateProductToAttributeValueSearchModel searchModel)
    //{
    //    ArgumentNullException.ThrowIfNull(searchModel);

    //    searchModel.IsLoggedInAsVendor = await _workContext.GetCurrentVendorAsync() != null;

    //    //prepare available categories
    //    await _baseAdminModelFactory.PrepareCategoriesAsync(searchModel.AvailableCategories);

    //    //prepare available manufacturers
    //    await _baseAdminModelFactory.PrepareManufacturersAsync(searchModel.AvailableManufacturers);

    //    //prepare available stores
    //    await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

    //    //prepare available vendors
    //    await _baseAdminModelFactory.PrepareVendorsAsync(searchModel.AvailableVendors);

    //    //prepare available product types
    //    await _baseAdminModelFactory.PrepareProductTypesAsync(searchModel.AvailableProductTypes);

    //    //prepare page parameters
    //    searchModel.SetPopupGridPageSize();

    //    return searchModel;
    //}

    ///// <summary>
    ///// Prepare paged product model to associate to the product attribute value
    ///// </summary>
    ///// <param name="searchModel">Product model to associate to the product attribute value</param>
    ///// <returns>
    ///// A task that represents the asynchronous operation
    ///// The task result contains the product model to associate to the product attribute value
    ///// </returns>
    //public virtual async Task<AssociateProductToAttributeValueListModel> PrepareAssociateProductToAttributeValueListModelAsync(
    //    AssociateProductToAttributeValueSearchModel searchModel)
    //{
    //    ArgumentNullException.ThrowIfNull(searchModel);

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null)
    //        searchModel.SearchVendorId = currentVendor.Id;

    //    //get products
    //    var products = await _productService.SearchProductsAsync(showHidden: true,
    //        categoryIds: new List<int> { searchModel.SearchCategoryId },
    //        manufacturerIds: new List<int> { searchModel.SearchManufacturerId },
    //        storeId: searchModel.SearchStoreId,
    //        vendorId: searchModel.SearchVendorId,
    //        productType: searchModel.SearchProductTypeId > 0 ? (ProductType?)searchModel.SearchProductTypeId : null,
    //        keywords: searchModel.SearchProductName,
    //        pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

    //    //prepare grid model
    //    var model = await new AssociateProductToAttributeValueListModel().PrepareToGridAsync(searchModel, products, () =>
    //    {
    //        //fill in model values from the entity
    //        return products.SelectAwait(async product =>
    //        {
    //            var productModel = product.ToModel<ProductModel>();

    //            productModel.SeName = await _urlRecordService.GetSeNameAsync(product, 0, true, false);

    //            return productModel;
    //        });
    //    });

    //    return model;
    //}

    ///// <summary>
    ///// Prepare paged product attribute combination list model
    ///// </summary>
    ///// <param name="searchModel">Product attribute combination search model</param>
    ///// <param name="product">Product</param>
    ///// <returns>
    ///// A task that represents the asynchronous operation
    ///// The task result contains the product attribute combination list model
    ///// </returns>
    public virtual async Task<ProductAttributeCombinationListModel> CustomPrepareProductAttributeCombinationListModelAsync(
        ProductAttributeCombinationSearchModel searchModel, Product product,
   List<int> filterIds = null)
    {
        ArgumentNullException.ThrowIfNull(searchModel);
        ArgumentNullException.ThrowIfNull(product);

        // 1. Get ALL combinations (unpaged) to avoid missing records on other pages
        var allCombinations = await _productAttributeService.GetAllProductAttributeCombinationsAsync(product.Id);

        // 2. Apply Filters: Published Check + Smart Injector Selections
        var filteredList = new List<ProductAttributeCombination>();
        foreach (var combination in allCombinations)
        {
            // A. Filter by JS Selections: Skip if not in the valid ID list
            if (filterIds != null && filterIds.Any())
            {
                if (filterIds.Count == 1 && filterIds[0] == -1) break;
                if (!filterIds.Contains(combination.Id)) continue;
            }

            // B. Published Check: Exclude if any attribute value is unpublished
            var attributeValues = await _productAttributeParser.ParseProductAttributeValuesAsync(combination.AttributesXml);
            if (attributeValues.Any(av => !av.Published)) continue;

            filteredList.Add(combination);
        }



        // 3. Page the results AFTER they are filtered
        var pagedList = filteredList.ToPagedList(searchModel);
        // 4. Map to Model

        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        var currentStore = await _storeContext.GetCurrentStoreAsync();

        //prepare grid model
        var model = await new ProductAttributeCombinationListModel().PrepareToGridAsync(searchModel, pagedList, () =>
        {
            return pagedList.SelectAwait(async combination =>
            {
                //fill in model values from the entity
                var comboModel = combination.ToModel<ProductAttributeCombinationModel>();

                // Format Attributes and Thumbnails
                comboModel.AttributesXml = await _productAttributeFormatter
                    .FormatAttributesAsync(product, combination.AttributesXml, currentCustomer, currentStore, "<br />", true, true, true, false);

                var combinationPicture = (await _productAttributeService.GetProductAttributeCombinationPicturesAsync(combination.Id)).FirstOrDefault();
                var pictureThumbnailUrl = await _pictureService.GetPictureUrlAsync(combinationPicture?.PictureId ?? 0, 75, false);

                //little hack here. Grid is rendered wrong way with <img> without "src" attribute
                if (string.IsNullOrEmpty(pictureThumbnailUrl))
                    pictureThumbnailUrl = await _pictureService.GetDefaultPictureUrlAsync(targetSize: 1);

                comboModel.PictureThumbnailUrl = pictureThumbnailUrl;
                var warnings = (await _shoppingCartService.GetShoppingCartItemAttributeWarningsAsync(currentCustomer,
                        ShoppingCartType.ShoppingCart, product,
                        attributesXml: combination.AttributesXml,
                        ignoreNonCombinableAttributes: true)
                    ).Aggregate(string.Empty, (message, warning) => $"{message}{warning}<br />");
                comboModel.Warnings = new List<string> { warnings };

                return comboModel;
            });
        });

        return model;
    }

    ///// <summary>
    ///// Prepare product attribute combination model
    ///// </summary>
    ///// <param name="model">Product attribute combination model</param>
    ///// <param name="product">Product</param>
    ///// <param name="productAttributeCombination">Product attribute combination</param>
    ///// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
    ///// <returns>
    ///// A task that represents the asynchronous operation
    ///// The task result contains the product attribute combination model
    ///// </returns>
    public virtual async Task<ProductAttributeCombinationModel> PrepareCustomProductAttributeCombinationModelAsync(ProductAttributeCombinationModel model,
        Product product, ProductAttributeCombination productAttributeCombination, bool excludeProperties = false)
    {
        ArgumentNullException.ThrowIfNull(product);

        if (productAttributeCombination != null)
        {
            //fill in model values from the entity
            model ??= new ProductAttributeCombinationModel
            {
                AllowOutOfStockOrders = productAttributeCombination.AllowOutOfStockOrders,
                AttributesXml = productAttributeCombination.AttributesXml,
                Gtin = productAttributeCombination.Gtin,
                Id = productAttributeCombination.Id,
                ManufacturerPartNumber = productAttributeCombination.ManufacturerPartNumber,
                NotifyAdminForQuantityBelow = productAttributeCombination.NotifyAdminForQuantityBelow,
                OverriddenPrice = productAttributeCombination.OverriddenPrice,
                OverriddenOldPrice = productAttributeCombination.OverriddenOldPrice,
                OverriddenMsrp = productAttributeCombination.OverriddenMsrp,
                PictureIds = (await _productAttributeService.GetProductAttributeCombinationPicturesAsync(productAttributeCombination.Id))
                    .Select(c => c.PictureId).ToList(),
                ProductId = productAttributeCombination.ProductId,
                Sku = productAttributeCombination.Sku,
                StockQuantity = productAttributeCombination.StockQuantity,
                MinStockQuantity = productAttributeCombination.MinStockQuantity
            };
          
            //model.GalleryPictures = await _productAttributeService.GetProductAttributeCombinationGalleryPicturesByProductAttributeCombinationId(productAttributeCombination.Id);
        }

        model.ProductId = product.Id;

        //set default values for the new model
        if (productAttributeCombination == null)
        {
            model.ProductId = product.Id;
            model.StockQuantity = 10000;
            model.NotifyAdminForQuantityBelow = 1;
        }

        //prepare picture models
        var productPictures = await _productService.GetProductPicturesByProductIdAsync(product.Id);
        model.ProductPictureModels = await productPictures.SelectAwait(async productPicture => new ProductPictureModel
        {
            Id = productPicture.Id,
            ProductId = productPicture.ProductId,
            PictureId = productPicture.PictureId,
            PictureUrl = await _pictureService.GetPictureUrlAsync(productPicture.PictureId),
            DisplayOrder = productPicture.DisplayOrder
        }).ToListAsync();

        //prepare product attribute mappings (exclude non-combinable attributes)
        var attributes = (await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id))
            .Where(productAttributeMapping => !productAttributeMapping.IsNonCombinable()).ToList();

        foreach (var attribute in attributes)
        {
            var attributeModel = new ProductAttributeCombinationModel.ProductAttributeModel
            {
                Id = attribute.Id,
                ProductAttributeId = attribute.ProductAttributeId,
                Name = (await _productAttributeService.GetProductAttributeByIdAsync(attribute.ProductAttributeId)).Name,
                TextPrompt = attribute.TextPrompt,
                IsRequired = attribute.IsRequired,
                AttributeControlType = attribute.AttributeControlType
            };

            if (attribute.ShouldHaveValues())
            {
                //values
                var attributeValues = await _productAttributeService.GetProductAttributeValuesAsync(attribute.Id);
                var preSelectedValue = _productAttributeParser.ParseValues(model.AttributesXml, attribute.Id);
                foreach (var attributeValue in attributeValues)
                {
                    var attributeValueModel = new ProductAttributeCombinationModel.ProductAttributeValueModel
                    {
                        Id = attributeValue.Id,
                        Name = attributeValue.Name,
                        IsPreSelected = preSelectedValue.Contains(attributeValue.Id.ToString())
                    };
                    attributeModel.Values.Add(attributeValueModel);
                }
            }

            model.ProductAttributes.Add(attributeModel);
        }

        return model;
    }
    public async Task SyncAttributeCombinations(Product product, ProductAttributeMapping productAttributeMapping, ProductAttributeValue pav)
    {
        List<ProductAttributeCombination> combinations = new();
        List<ProductAttributeCombination> existingCombinations = new();
        var _customProductService = EngineContext.Current.Resolve<ICustomProductAttributeService>();
        var mappings = (await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id));
        List<int> mappingIds = new List<int>();
        foreach (var mapping in mappings)
        {
            if ((await _customProductService.CustomGetProductAttributeValuesAsync(mapping.Id)).Where(attrval => attrval.Published).Count() > 0)
            {
                mappingIds.Add(mapping.Id);
            }
        }
        if (pav != null)
        {
            if (pav.Published == true)
            {


                // 2. Determine if this is the "Primary" mapping (the base row)


                combinations = (await _productAttributeService.GetAllProductAttributeCombinationsAsync(product.Id)).ToList();
                if (mappingIds.Count == 1)
                {
                    await _productAttributeService.InsertProductAttributeCombinationAsync(new ProductAttributeCombination
                    {
                        ProductId = product.Id,
                        StockQuantity = 10000,
                        AllowOutOfStockOrders = true,
                        AttributesXml = _productAttributeParser.AddProductAttribute("", productAttributeMapping, pav.Id.ToString())
                    });
                }
                else
                {
                    var allAttributesXml = await _productAttributeParser.GenerateAllCombinationsAsync(product, true, null);
                    Dictionary<string, int[]> dicValidCombinations = new();
                    foreach (var xml in allAttributesXml)
                    {
                        var attributes = await _productAttributeParser.ParseProductAttributeValuesAsync(xml);
                        if (mappingIds.Count == attributes.Count && attributes.Where(attrval => mappingIds.Contains(attrval.ProductAttributeMappingId)).Count() == mappingIds.Count && attributes.Where(attrVal => attrVal.Id == pav.Id).Any())
                        {
                            if (!dicValidCombinations.Keys.Where(k => k == xml).Any())
                                dicValidCombinations.Add(xml, attributes.Select(attrval => attrval.Id).ToArray());
                        }
                    }


                    Dictionary<int, int[]> dicExistingCombinations = new();
                    existingCombinations = (await _productAttributeService.GetAllProductAttributeCombinationsAsync(product.Id)).ToList();
                    foreach (var combination in existingCombinations)
                    {
                        var attributes = await _productAttributeParser.ParseProductAttributeValuesAsync(combination.AttributesXml);
                        if (!attributes.Where(attrval => attrval.ProductAttributeMappingId == pav.ProductAttributeMappingId).Any())
                        {

                            if (!dicExistingCombinations.Keys.Where(k => k == combination.Id).Any())
                                dicExistingCombinations.Add(combination.Id, attributes.Select(attrval => attrval.Id).ToArray());
                        }
                    }

                    foreach (var validCombination in dicValidCombinations)
                    {
                        var values = validCombination.Value;

                        foreach (var inCompleteCombination in dicExistingCombinations
    .Where(item =>
        item.Value.Length == values.Length - 1 &&
        item.Value.All(v => values.Contains(v)))
    .ToList())

                        {
                            var combination = existingCombinations.Where(cmb => cmb.Id == inCompleteCombination.Key).FirstOrDefault();
                            if (combination != null)
                            {
                                var existing = await _productAttributeParser.FindProductAttributeCombinationAsync(product, validCombination.Key);
                                if (existing == null)
                                {
                                    combination.AttributesXml = validCombination.Key;
                                    await _productAttributeService.UpdateProductAttributeCombinationAsync(combination);
                                }
                            }
                        }

                    }

                    foreach (var validCombination in dicValidCombinations)
                    {
                        var existing = await _productAttributeParser.FindProductAttributeCombinationAsync(product, validCombination.Key);
                        if (existing == null)
                        {
                            await _productAttributeService.InsertProductAttributeCombinationAsync(new ProductAttributeCombination
                            {
                                ProductId = product.Id,
                                StockQuantity = 10000,
                                AllowOutOfStockOrders = true,
                                AttributesXml = validCombination.Key
                            });
                        }
                    }
                }
            }
        }
        else
        {
            var _customproductAttributeParser = EngineContext.Current.Resolve<ICustomProductAttributeParser>();
            existingCombinations = (await _productAttributeService.GetAllProductAttributeCombinationsAsync(product.Id)).ToList();
            foreach (var combination in existingCombinations)
            {
                var attributes = await _productAttributeParser.ParseProductAttributeValuesAsync(combination.AttributesXml);
                var attrValueIds = await _customproductAttributeParser.CustomParseProductAttributeValuesAsync(combination.AttributesXml);
                string validXml = string.Empty;
                int noOfAttrributeInCombinations = 0;
                bool isEligibleForUpdate = false;
                bool isEligibleForDelete = false;
                if (attributes.Count < attrValueIds.Count)
                    isEligibleForDelete = true;
                foreach (var attribute in attributes)
                {
                    isEligibleForUpdate = false;
                    var mapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(attribute.ProductAttributeMappingId);
                    if (attribute.Published && mapping != null)
                    {
                        validXml = _productAttributeParser.AddProductAttribute(validXml, mapping, attribute.Id.ToString());
                        noOfAttrributeInCombinations++;
                    }
                    else
                    {
                        isEligibleForUpdate = true;
                    }
                }

                if (isEligibleForUpdate || isEligibleForDelete)
                {
                    if (noOfAttrributeInCombinations != mappingIds.Count() || (isEligibleForDelete && attrValueIds.Count == mappingIds.Count()))
                    {
                        await _productAttributeService.DeleteProductAttributeCombinationAsync(combination);
                    }
                    else
                    {
                        combination.AttributesXml = validXml;
                        await _productAttributeService.UpdateProductAttributeCombinationAsync(combination);
                    }
                }
            }


        }



        existingCombinations = (await _productAttributeService.GetAllProductAttributeCombinationsAsync(product.Id)).ToList();
        combinations.Clear();
        foreach (var combination in existingCombinations)
        {
            if (combinations.Where(c => c.AttributesXml == combination.AttributesXml).Any())
            {
                await _productAttributeService.DeleteProductAttributeCombinationAsync(combination);
            }
            else
            {
                combinations.Add(combination);
            }
        }




    }

    #endregion
 
    public virtual async Task PrepareSmartInjectionDataAsync(ProductModel model, Product product)
    {
        var blueprintList = new List<AttributeMatrixModel>();
        var productAttributeMappings = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
        productAttributeMappings = productAttributeMappings.OrderBy(x => x.DisplayOrder).ToList();
        foreach (var mapping in productAttributeMappings)
        {
            var productAttribute = await _productAttributeService.GetProductAttributeByIdAsync(mapping.ProductAttributeId);
            var values = await _productAttributeService.GetProductAttributeValuesAsync(mapping.Id);
            values = values.Where(x => x.Published).ToList();
            if (values.Count > 0)
            {
                var attributeDto = new AttributeMatrixModel
                {
                    AttributeMappingId = mapping.Id,
                    Name = productAttribute.Name,
                    DisplayOrder = mapping.DisplayOrder,
                    Values = values.Select(v => new AttributeValueMatrixModel
                    {
                        ValueId = v.Id,
                        Name = v.Name
                    }).ToList()
                };
                blueprintList.Add(attributeDto);
            }
        }


        model.FullAttributeDefinitionsJson = System.Text.Json.JsonSerializer.Serialize(blueprintList);


        var realityMapList = new List<CombinationMatrixMNodel>();


        var combinations = await _productAttributeService.GetAllProductAttributeCombinationsAsync(product.Id);

        foreach (var combo in combinations)
        {

            var attributeValues =
                await _productAttributeParser
                    .ParseProductAttributeValuesAsync(combo.AttributesXml);

            // Combination is unpublished if ANY attribute value is unpublished
            bool combinationUnpublished =
                attributeValues.Any(av => !av.Published);

            if (!combinationUnpublished)
            {

                var comboDto = new CombinationMatrixMNodel
                {
                    CombinationId = combo.Id
                };


                foreach (var value in attributeValues)
                {

                    comboDto.Attributes[value.ProductAttributeMappingId] = value.Id;
                }

                realityMapList.Add(comboDto);
            }
        }

        model.LogicMasterListJson = System.Text.Json.JsonSerializer.Serialize(realityMapList);

        // 1. Fetch the pictures and generate real URLs for the thumbnails
        var pictureList = new List<object>();
        var productPictures = await _productService.GetProductPicturesByProductIdAsync(product.Id);

        foreach (var pp in productPictures)
        {
            var pic = await _pictureService.GetPictureByIdAsync(pp.PictureId);
            if (pic != null)
            {
                // Get a 75px thumbnail URL (matches nopCommerce default)
                var pictureUrl = await _pictureService.GetPictureUrlAsync(pic, 75);
                pictureList.Add(new
                {
                    PictureId = pic.Id,
                    ImageUrl = pictureUrl.Url
                });
            }
        }
        model.AvailablePicturesJson = System.Text.Json.JsonSerializer.Serialize(pictureList);
    }
 
}