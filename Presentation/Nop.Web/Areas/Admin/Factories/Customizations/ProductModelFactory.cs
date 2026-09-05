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