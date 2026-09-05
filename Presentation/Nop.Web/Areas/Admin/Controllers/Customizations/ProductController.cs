using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Domain;
using MWT.Nop.Core.Domain.Catalog;
using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Services;
using MWT.Nop.Core.Services.Catalog;
using MWT.Nop.Core.Services.Configuration;
using MWT.Nop.Core.Services.ExportImport;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customization.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure;
using Nop.Services.ExportImport;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Customizations.Components;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Areas.Admin.Models.Customization.Catalog;
using Nop.Web.Areas.Admin.Models.Customization.Custom;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Infrastructure.Cache;

namespace Nop.Web.Areas.Admin.Controllers;

public partial class ProductController : BaseAdminController
{

    #region Methods

    #region Product list / create / edit / delete



    [HttpPost]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
    public virtual async Task<IActionResult> CustomProductList(ProductSearchModel searchModel)
    {
        //prepare model
        var model = await _productModelFactory.CustomPrepareProductListModelAsync(searchModel);

        return Json(model);
    }

    [CheckPermission(StandardPermission.CustomPermission.CUSTOM_ACCESS_PRODUCT_CREATE)]
    public virtual async Task<IActionResult> CustomCreate()
    {
        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (_vendorSettings.MaximumProductNumber > 0 &&
            currentVendor != null &&
            await _productService.GetNumberOfProductsByVendorIdAsync(currentVendor.Id) >= _vendorSettings.MaximumProductNumber)
        {
            _notificationService.ErrorNotification(string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ExceededMaximumNumber"),
                _vendorSettings.MaximumProductNumber));
            return RedirectToAction("List");
        }

        //prepare model
        var model = await _productModelFactory.PrepareProductModelAsync(new ProductModel(), null);

        return View("Create", model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    [CheckPermission(StandardPermission.CustomPermission.CUSTOM_ACCESS_PRODUCT_CREATE)]
    public virtual async Task<IActionResult> CustomCreate(ProductModel model, bool continueEditing)
    {
        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (_vendorSettings.MaximumProductNumber > 0 &&
            currentVendor != null &&
            await _productService.GetNumberOfProductsByVendorIdAsync(currentVendor.Id) >= _vendorSettings.MaximumProductNumber)
        {
            _notificationService.ErrorNotification(string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ExceededMaximumNumber"),
                _vendorSettings.MaximumProductNumber));
            return RedirectToAction("List");
        }

        if (ModelState.IsValid)
        {
            //a vendor should have access only to his products
            if (currentVendor != null)
                model.VendorId = currentVendor.Id;

            //vendors cannot edit "Show on home page" property
            if (currentVendor != null && model.ShowOnHomepage)
                model.ShowOnHomepage = false;

            //product
            var product = model.ToEntity<Product>();
            product.CreatedOnUtc = DateTime.UtcNow;
            product.UpdatedOnUtc = DateTime.UtcNow;
            await _productService.InsertProductAsync(product);

            //search engine name
            model.SeName = await _urlRecordService.ValidateSeNameAsync(product, model.SeName, product.Name, true);
            await _urlRecordService.SaveSlugAsync(product, model.SeName, 0);

            //locales
            await UpdateLocalesAsync(product, model);

            //categories
            await SaveCategoryMappingsAsync(product, model);

            //manufacturers
            await SaveManufacturerMappingsAsync(product, model);

            //stores
            await _storeMappingService.SaveStoreMappingsAsync(product, model.SelectedStoreIds);

            //discounts
            await SaveDiscountMappingsAsync(product, model);

            //tags
            await _productTagService.UpdateProductTagsAsync(product, model.SelectedProductTags.ToArray());

            //warehouses
            await SaveProductWarehouseInventoryAsync(product, model);

            //quantity change history
            await _productService.AddStockQuantityHistoryEntryAsync(product, product.StockQuantity, product.StockQuantity, product.WarehouseId,
                await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.Edit"));

            //activity log
            await _customerActivityService.InsertActivityAsync("AddNewProduct",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewProduct"), product.Name), product);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Added"));

            if (!continueEditing)
                return RedirectToAction("List");

            return RedirectToAction("Edit", new { id = product.Id });
        }

        //prepare model
        model = await _productModelFactory.PrepareProductModelAsync(model, null, true);

        //if we got this far, something failed, redisplay form
        return View("Create", model);
    }

    [CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
    public virtual async Task<IActionResult> CustomEdit(int id)
    {
        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null || product.Deleted)
            return RedirectToAction("List");

        //a vendor should have access only to his products
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return RedirectToAction("List");

        //prepare model
        var model = await _productModelFactory.CustomPrepareProductModelAsync(null, product);

        return View("Edit", model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    [HttpPost, ParameterBasedOnFormName("update-picture-sename", "updatePictureSename")]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> CustomEdit(ProductModel model, bool continueEditing, bool updatePictureSename)
    {
        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(model.Id);
        if (product == null || product.Deleted)
            return RedirectToAction("List");

        //a vendor should have access only to his products
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return RedirectToAction("List");

        //check if the product quantity has been changed while we were editing the product
        //and if it has been changed then we show error notification
        //and redirect on the editing page without data saving
        if (product.StockQuantity != model.LastStockQuantity)
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Fields.StockQuantity.ChangedWarning"));
            return RedirectToAction("Edit", new { id = product.Id });
        }

        if (ModelState.IsValid)
        {

            var _customProductService = EngineContext.Current.Resolve<IProductExtendedService>();
            //a vendor should have access only to his products
            if (currentVendor != null)
                model.VendorId = currentVendor.Id;

            //we do not validate maximum number of products per vendor when editing existing products (only during creation of new products)
            //vendors cannot edit "Show on home page" property
            if (currentVendor != null && model.ShowOnHomepage != product.ShowOnHomepage)
                model.ShowOnHomepage = product.ShowOnHomepage;

            //some previously used values
            var prevTotalStockQuantity = await _productService.GetTotalStockQuantityAsync(product);
            var prevDownloadId = product.DownloadId;
            var prevSampleDownloadId = product.SampleDownloadId;
            var previousStockQuantity = product.StockQuantity;
            var previousWarehouseId = product.WarehouseId;
            var previousProductType = product.ProductType;

            //product
            #region Product Conbinations Inventory Updates
            if (product.StockQuantity != model.StockQuantity)
            {
                var existedCombinations = await _productAttributeService.GetAllProductAttributeCombinationsAsync(product.Id);
                var _customProductAttributeService = EngineContext.Current.Resolve<ICustomProductAttributeService>();
                foreach (var combination in existedCombinations)
                {
                    combination.StockQuantity = model.StockQuantity;
                    await _customProductAttributeService.CustomUpdateProductAttributeCombinationAsync(combination);
                }
            }

            #endregion

            #region Clear Cache of Child Section
            if (product.Published != model.Published || product.StockQuantity != model.StockQuantity)
            {
                var _staticCacheManager = EngineContext.Current.Resolve<IStaticCacheManager>();
                await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.CrossSellProductsPrefix);
                await _staticCacheManager.RemoveByPrefixAsync(CustomNopCatalogDefaults.CollectionPrefix);
                await _staticCacheManager.RemoveByPrefixAsync(CustomNopCatalogDefaults.PairWithPrefix);
                await _staticCacheManager.RemoveByPrefixAsync(CustomNopCatalogDefaults.FBTPrefix);
                await _staticCacheManager.RemoveByPrefixAsync(CustomNopModelCacheDefaults.FBTPrefix);
                await _staticCacheManager.RemoveByPrefixAsync(CustomNopCatalogDefaults.RelatedPrefix);
            }

            #endregion
            product = model.ToEntity(product);

            product.UpdatedOnUtc = DateTime.UtcNow;
            await _productService.UpdateProductAsync(product);

            if (product.ATCRecommendType == ATCRecommendType.SimilarItems || product.ATCRecommendType == ATCRecommendType.Collection || product.ATCRecommendType == ATCRecommendType.None)
            {
                product.ATCRecommendedProductIds = string.Empty;
            }

            //remove associated products
            if (previousProductType == ProductType.GroupedProduct && product.ProductType == ProductType.SimpleProduct)
            {
                var store = await _storeContext.GetCurrentStoreAsync();
                var storeId = store?.Id ?? 0;
                var vendorId = currentVendor?.Id ?? 0;

                var associatedProducts = await _productService.GetAssociatedProductsAsync(product.Id, storeId, vendorId);
                foreach (var associatedProduct in associatedProducts)
                {
                    associatedProduct.ParentGroupedProductId = 0;
                    await _productService.UpdateProductAsync(associatedProduct);
                }
            }

            //search engine name
            model.SeName = await _urlRecordService.ValidateSeNameAsync(product, model.SeName, product.Name, true);
            await _urlRecordService.SaveSlugAsync(product, model.SeName, 0);

            //locales
            await UpdateLocalesAsync(product, model);

            //tags
            await _productTagService.UpdateProductTagsAsync(product, model.SelectedProductTags.ToArray());

            //warehouses
            await SaveProductWarehouseInventoryAsync(product, model);

            //categories
            await SaveCategoryMappingsAsync(product, model);

            //manufacturers
            await SaveManufacturerMappingsAsync(product, model);

            //stores
            await _storeMappingService.SaveStoreMappingsAsync(product, model.SelectedStoreIds);

            //discounts
            await SaveDiscountMappingsAsync(product, model);
            if (updatePictureSename)
            {
                //picture seo names
                await UpdatePictureSeoNamesAsync(product);
            }
            //back in stock notifications
            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                product.BackorderMode == BackorderMode.NoBackorders &&
                product.AllowBackInStockSubscriptions &&
                await _customProductService.GetProductInventory(product) > 0 &&
                prevTotalStockQuantity <= 0 &&
                product.Published &&
                !product.Deleted)
            {
                List<Token> tokens = new List<Token>();
                tokens.Add(
                    new Token("Product.link", Url.RouteUrl("Product", new { id = model.Id, SeName = _urlRecordService.GetSeNameAsync(product) })
                    ));
                tokens.Add(new Token("Product.Name", product.Name));
                tokens.Add(new Token("Product.Msrp", product.Msrp));
                tokens.Add(new Token("Product.Price", product.OldPrice));
                tokens.Add(new Token("Product.SalePrice", product.Price));
                tokens.Add(new Token("Product.RelatedProducts",
                    await RenderViewComponentToStringAsync(typeof(Custom_Upsell_MailerViewComponent), new { productId = product.Id, noOfRelatedProducts = 4 }), true));
                tokens.Add(new Token("Product.info.Mailer",
                 await RenderViewComponentToStringAsync(typeof(Custom_Product_Info_MailerViewComponent), new { productId = product.Id, moduleType = "InStock" }), true));
                var _customBackInStockSubscriptionService = EngineContext.Current.Resolve<ICustomBackInStockSubscriptionService>();
                await _customBackInStockSubscriptionService.CustomSendNotificationsToSubscribersAsync(product, tokens);
            }

            //delete an old "download" file (if deleted or updated)
            if (prevDownloadId > 0 && prevDownloadId != product.DownloadId)
            {
                var prevDownload = await _downloadService.GetDownloadByIdAsync(prevDownloadId);
                if (prevDownload != null)
                    await _downloadService.DeleteDownloadAsync(prevDownload);
            }

            //delete an old "sample download" file (if deleted or updated)
            if (prevSampleDownloadId > 0 && prevSampleDownloadId != product.SampleDownloadId)
            {
                var prevSampleDownload = await _downloadService.GetDownloadByIdAsync(prevSampleDownloadId);
                if (prevSampleDownload != null)
                    await _downloadService.DeleteDownloadAsync(prevSampleDownload);
            }

            //quantity change history
            if (previousWarehouseId != product.WarehouseId)
            {
                //warehouse is changed 
                //compose a message
                var oldWarehouseMessage = string.Empty;
                if (previousWarehouseId > 0)
                {
                    var oldWarehouse = await _warehouseService.GetWarehouseByIdAsync(previousWarehouseId);
                    if (oldWarehouse != null)
                        oldWarehouseMessage = string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.EditWarehouse.Old"), oldWarehouse.Name);
                }

                var newWarehouseMessage = string.Empty;
                if (product.WarehouseId > 0)
                {
                    var newWarehouse = await _warehouseService.GetWarehouseByIdAsync(product.WarehouseId);
                    if (newWarehouse != null)
                        newWarehouseMessage = string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.EditWarehouse.New"), newWarehouse.Name);
                }

                var message = string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.EditWarehouse"), oldWarehouseMessage, newWarehouseMessage);

                //record history
                await _productService.AddStockQuantityHistoryEntryAsync(product, -previousStockQuantity, 0, previousWarehouseId, message);
                await _productService.AddStockQuantityHistoryEntryAsync(product, product.StockQuantity, product.StockQuantity, product.WarehouseId, message);
            }
            else
            {
                await _productService.AddStockQuantityHistoryEntryAsync(product, product.StockQuantity - previousStockQuantity, product.StockQuantity,
                    product.WarehouseId, await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.Edit"));
            }

            //activity log
            await _customerActivityService.InsertActivityAsync("EditProduct",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditProduct"), product.Name), product);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Updated"));
            if (product.ProductType == ProductType.GroupedProduct)
                await _customProductService.UpdateGroupProductPrice(product.Id);
            if (!continueEditing)
                return RedirectToAction("List");

            return RedirectToAction("Edit", new { id = product.Id });
        }

        //prepare model
        model = await _productModelFactory.PrepareProductModelAsync(model, product, true);

        //if we got this far, something failed, redisplay form
        return View("Edit", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.CustomPermission.CUSTOM_ACCESS_PRODUCT_DELETE)]
    public virtual async Task<IActionResult> CustomDelete(int id)
    {
        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
            return RedirectToAction("List");

        //a vendor should have access only to his products
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return RedirectToAction("List");

        await _productService.DeleteProductAsync(product);

        //activity log
        await _customerActivityService.InsertActivityAsync("DeleteProduct",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteProduct"), product.Name), product);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Deleted"));

        return RedirectToAction("List");
    }

    [HttpPost]
    [CheckPermission(StandardPermission.CustomPermission.CUSTOM_ACCESS_PRODUCT_DELETE)]
    public virtual async Task<IActionResult> CustomDeleteSelected(ICollection<int> selectedIds)
    {
        if (selectedIds == null || !selectedIds.Any())
            return NoContent();

        var currentVendor = await _workContext.GetCurrentVendorAsync();

        var products = (await _productService.GetProductsByIdsAsync(selectedIds.ToArray()))
            .Where(p => currentVendor == null || p.VendorId == currentVendor.Id).ToList();

        await _productService.DeleteProductsAsync(products);

        //activity log
        var activityLogFormat = await _localizationService.GetResourceAsync("ActivityLog.DeleteProduct");

        foreach (var product in products)
            await _customerActivityService.InsertActivityAsync("DeleteProduct",
                string.Format(activityLogFormat, product.Name), product);

        return Json(new { Result = true });
    }

    [HttpPost]
    [CheckPermission(StandardPermission.CustomPermission.CUSTOM_ACCESS_PRODUCT_CREATE)]
    public virtual async Task<IActionResult> CustomCopyProduct(ProductModel model)
    {
        var copyModel = model.CopyProductModel;
        try
        {
            var originalProduct = await _productService.GetProductByIdAsync(copyModel.Id);

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && originalProduct.VendorId != currentVendor.Id)
                return RedirectToAction("List");
            var _customCopyProductService = EngineContext.Current.Resolve<ICopyProductExtendedService>();
            var newProduct = await _customCopyProductService.CustomCopyProductAsync(originalProduct, copyModel.Name, copyModel.Sku, copyModel.Published, copyModel.CopyMultimedia);

            //publishing post copy product event
            await _eventPublisher.PublishAsync(new PostCopyProductEvent(originalProduct, newProduct));

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Copied"));

            return RedirectToAction("Edit", new { id = newProduct.Id });
        }
        catch (Exception exc)
        {
            _notificationService.ErrorNotification(exc.Message);
            return RedirectToAction("Edit", new { id = copyModel.Id });
        }
    }

    #region Collection products

    [HttpPost]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> CollectionProductList(RelatedProductSearchModel searchModel)
    {
        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(searchModel.ProductId)
            ?? throw new ArgumentException("No product found with the specified id");

        //a vendor should have access only to his products
        if (await _workContext.GetCurrentVendorAsync() != null && product.VendorId != (await _workContext.GetCurrentVendorAsync()).Id)
            return Content("This is not your product");

        //prepare model
        var model = await _productModelFactory.PrepareCollectionProductListModelAsync(searchModel, product);

        return Json(model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> CollectionProductUpdate(CollectionProductModel model)
    {
        var _productService = EngineContext.Current.Resolve<IProductExtendedService>();
        //try to get a Collection product with the specified id
        var CollectionProduct = await _productService.GetCollectionProductByIdAsync(model.Id)
            ?? throw new ArgumentException("No Collection product found with the specified id");

        //a vendor should have access only to his products
        if (await _workContext.GetCurrentVendorAsync() != null)
        {
            var product = await _productService.GetProductByIdAsync(CollectionProduct.ProductId1);
            if (product != null && product.VendorId != (await _workContext.GetCurrentVendorAsync()).Id)
                return Content("This is not your product");
        }

        CollectionProduct.DisplayOrder = model.DisplayOrder;
        await _productService.UpdateCollectionProductAsync(CollectionProduct);

        return new NullJsonResult();
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> CollectionProductDelete(int id)
    {
        var _productService = EngineContext.Current.Resolve<IProductExtendedService>();
        //try to get a Collection product with the specified id
        var CollectionProduct = await _productService.GetCollectionProductByIdAsync(id)
            ?? throw new ArgumentException("No Collection product found with the specified id");

        var productId = CollectionProduct.ProductId1;

        //a vendor should have access only to his products
        if (await _workContext.GetCurrentVendorAsync() != null)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product != null && product.VendorId != (await _workContext.GetCurrentVendorAsync()).Id)
                return Content("This is not your product");
        }

        await _productService.DeleteCollectionProductAsync(CollectionProduct);

        return new NullJsonResult();
    }

    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> CollectionProductAddPopup(int productId)
    {
        //prepare model
        var model = await _productModelFactory.PrepareAddCollectionProductSearchModelAsync(new AddCollectionProductSearchModel());

        return View(model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> CollectionProductAddPopupList(AddCollectionProductSearchModel searchModel)
    {
        //prepare model
        var model = await _productModelFactory.PrepareAddCollectionProductListModelAsync(searchModel);

        return Json(model);
    }

    [HttpPost]
    [FormValueRequired("save")]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> CollectionProductAddPopup(AddCollectionProductModel model)
    {
        var selectedProducts = await _productService.GetProductsByIdsAsync(model.SelectedProductIds.ToArray());
        if (selectedProducts.Any())
        {
            var _productService = EngineContext.Current.Resolve<IProductExtendedService>();
            var existingCollectionProducts = await _productService.GetCollectionProductsByProductId1ListAsync(model.ProductId, showHidden: true);
            foreach (var product in selectedProducts)
            {
                //a vendor should have access only to his products
                if (await _workContext.GetCurrentVendorAsync() != null && product.VendorId != (await _workContext.GetCurrentVendorAsync()).Id)
                    continue;

                if (_productService.FindCollectionProduct(existingCollectionProducts, model.ProductId, product.Id) != null)
                    continue;

                await _productService.InsertCollectionProductAsync(new CollectionProduct
                {
                    ProductId1 = model.ProductId,
                    ProductId2 = product.Id,
                    DisplayOrder = 1
                });
            }
        }

        ViewBag.RefreshPage = true;

        return View(new AddCollectionProductSearchModel());
    }

    #endregion

    #region RelatedSearchList

    [HttpPost]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> RelatedSearchList(RelatedProductSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE) && !await _permissionService.AuthorizeAsync(StandardPermission.Catalog.CATEGORIES_CREATE_EDIT_DELETE))
            return await AccessDeniedJsonAsync();

        //try to get a product with the specified id
        if (searchModel.EntityType == "Product")
        {
            var product = await _productService.GetProductByIdAsync(searchModel.ProductId)
                ?? throw new ArgumentException("No product found with the specified id");
        }
        else if (searchModel.EntityType == "Category")
        {
            var category = await _categoryService.GetCategoryByIdAsync(searchModel.ProductId)
  ?? throw new ArgumentException("No Category found with the specified id");
        }
        var _relatedSearchModelFactory = EngineContext.Current.Resolve<IRelatedSearchModelFactory>();

        //prepare model
        var model = await _relatedSearchModelFactory.PrepareRelatedSearchListModelAsync(searchModel, searchModel.ProductId, searchModel.EntityType);

        return Json(model);
    }
    [HttpPost]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> RelatedSearchUpdate(RelatedSearchModel model)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE) && !await _permissionService.AuthorizeAsync(StandardPermission.Catalog.CATEGORIES_CREATE_EDIT_DELETE))
            return await AccessDeniedJsonAsync();

        var _relatedSearchService = EngineContext.Current.Resolve<IRelatedSearchService>();

        //try to get a Collection product with the specified id
        var relatedSearch = await _relatedSearchService.GetById(model.Id)
            ?? throw new ArgumentException("No Related search found with the specified id");

        relatedSearch.DisplayOrder = model.DisplayOrder;
        relatedSearch.TermName = model.TermName;
        relatedSearch.Link = model.Link;
        relatedSearch.UpdatedOnUtc = DateTime.UtcNow;
        await _relatedSearchService.UpdateAsync(relatedSearch);
        return new NullJsonResult();
    }

    [HttpPost]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> RelatedSearchDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE) && !await _permissionService.AuthorizeAsync(StandardPermission.Catalog.CATEGORIES_CREATE_EDIT_DELETE))
            return await AccessDeniedJsonAsync();
        var _relatedSearchService = EngineContext.Current.Resolve<IRelatedSearchService>();

        //try to get a Collection product with the specified id
        var relatedSearch = await _relatedSearchService.GetById(id)
            ?? throw new ArgumentException("No Related search found with the specified id");

        await _relatedSearchService.DeleteAsync(relatedSearch);

        return new NullJsonResult();
    }

    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> RelatedSearchAddPopup(int entityId, string entityType)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE) && !await _permissionService.AuthorizeAsync(StandardPermission.Catalog.CATEGORIES_CREATE_EDIT_DELETE))
            return await AccessDeniedJsonAsync();
        RelatedSearchModel model = new RelatedSearchModel();
        model.EntityId = entityId;
        model.EntityType = entityType;

        return View(model);
    }



    [HttpPost]
    [FormValueRequired("save")]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> RelatedSearchInsert(RelatedSearchModel model)
    {
        if (ModelState.IsValid)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE) && !await _permissionService.AuthorizeAsync(StandardPermission.Catalog.CATEGORIES_CREATE_EDIT_DELETE))
                return await AccessDeniedJsonAsync();
            var _relatedSearchService = EngineContext.Current.Resolve<IRelatedSearchService>();

            RelatedSearch relatedSearch = new RelatedSearch();
            relatedSearch.Link = model.Link;
            relatedSearch.TermName = model.TermName;
            relatedSearch.EntityType = model.EntityType;
            relatedSearch.EntityId = model.EntityId;
            relatedSearch.DisplayOrder = model.DisplayOrder;
            relatedSearch.CreatedOnUtc = DateTime.UtcNow;
            relatedSearch.UpdatedOnUtc = DateTime.UtcNow;

            await _relatedSearchService.InsertAsync(relatedSearch);


            ViewBag.RefreshPage = true;

            RelatedSearchModel relatedSearchModel = new RelatedSearchModel();
            relatedSearchModel.EntityType = model.EntityType;
            relatedSearchModel.EntityId = model.EntityId;
            return View("RelatedSearchAddPopup", relatedSearchModel);
        }
        else
            return View(model);
    }
    #endregion

    #region Fbq products

    [HttpPost]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> FBTProductList(FBTProductSearchModel searchModel)
    {
        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(searchModel.ProductId)
            ?? throw new ArgumentException("No product found with the specified id");

        //a vendor should have access only to his products
        if (await _workContext.GetCurrentVendorAsync() != null && product.VendorId != (await _workContext.GetCurrentVendorAsync()).Id)
            return Content("This is not your product");

        //prepare model
        var model = await _productModelFactory.PrepareFBTProductListModelAsync(searchModel, product);

        return Json(model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> FBTProductUpdate(FBTProductModel model)
    {
        //try to get a FBT product with the specified id
        var _productService = EngineContext.Current.Resolve<IProductExtendedService>();
        var FBTProduct = await _productService.GetFBTProductByIdAsync(model.Id)
            ?? throw new ArgumentException("No FBT product found with the specified id");

        //a vendor should have access only to his products
        if (await _workContext.GetCurrentVendorAsync() != null)
        {
            var product = await _productService.GetProductByIdAsync(FBTProduct.ProductId1);
            if (product != null && product.VendorId != (await _workContext.GetCurrentVendorAsync()).Id)
                return Content("This is not your product");
        }

        FBTProduct.DisplayOrder = model.DisplayOrder;
        FBTProduct.DefaultQuantity = model.DefaultQuantity;
        await _productService.UpdateFBTProductAsync(FBTProduct);

        return new NullJsonResult();
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]

    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> FBTProductDelete(int id)
    {
        var _productService = EngineContext.Current.Resolve<IProductExtendedService>();
        //try to get a FBT product with the specified id
        var FBTProduct = await _productService.GetFBTProductByIdAsync(id)
            ?? throw new ArgumentException("No FBT product found with the specified id");

        var productId = FBTProduct.ProductId1;

        //a vendor should have access only to his products
        if (await _workContext.GetCurrentVendorAsync() != null)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product != null && product.VendorId != (await _workContext.GetCurrentVendorAsync()).Id)
                return Content("This is not your product");
        }

        await _productService.DeleteFBTProductAsync(FBTProduct);

        return new NullJsonResult();
    }

    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> FBTProductAddPopup(int productId)
    {
        //prepare model
        var model = await _productModelFactory.PrepareAddFBTProductSearchModelAsync(new AddFBTProductSearchModel());

        return View(model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> FBTProductAddPopupList(AddFBTProductSearchModel searchModel)
    {
        //prepare model
        var model = await _productModelFactory.PrepareAddFBTProductListModelAsync(searchModel);

        return Json(model);
    }

    [HttpPost]
    [FormValueRequired("save")]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> FBTProductAddPopup(AddFBTProductModel model)
    {
        var selectedProducts = await _productService.GetProductsByIdsAsync(model.SelectedProductIds.ToArray());
        if (selectedProducts.Any())
        {
            var _productService = EngineContext.Current.Resolve<IProductExtendedService>();
            var existingFBTProducts = await _productService.GetFBTProductsByProductId1Async(model.ProductId, showHidden: true);
            foreach (var product in selectedProducts)
            {
                //a vendor should have access only to his products
                if (await _workContext.GetCurrentVendorAsync() != null && product.VendorId != (await _workContext.GetCurrentVendorAsync()).Id)
                    continue;

                if (_productService.FindFBTProduct(existingFBTProducts, model.ProductId, product.Id) != null)
                    continue;

                await _productService.InsertFBTProductAsync(new FBTProduct
                {
                    ProductId1 = model.ProductId,
                    ProductId2 = product.Id,
                    DefaultQuantity = 1,
                    DisplayOrder = 1
                });
            }
        }

        ViewBag.RefreshPage = true;

        return View(new AddFBTProductSearchModel());
    }

    #endregion

    #endregion





    [HttpPost]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
    public virtual async Task<IActionResult> CustomRelatedProductList(RelatedProductSearchModel searchModel)
    {
        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(searchModel.ProductId)
            ?? throw new ArgumentException("No product found with the specified id");

        //a vendor should have access only to his products
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return Content("This is not your product");

        //prepare model
        var model = await _productModelFactory.CustomPrepareRelatedProductListModelAsync(searchModel, product);

        return Json(model);
    }



    [HttpPost]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> CustomRelatedProductAddPopupList(AddRelatedProductSearchModel searchModel)
    {
        //prepare model
        var model = await _productModelFactory.CustomPrepareAddRelatedProductListModelAsync(searchModel);

        return Json(model);
    }



    [HttpPost]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
    public virtual async Task<IActionResult> CustomCrossSellProductList(CrossSellProductSearchModel searchModel)
    {
        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(searchModel.ProductId)
            ?? throw new ArgumentException("No product found with the specified id");

        //a vendor should have access only to his products
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return Content("This is not your product");

        //prepare model
        var model = await _productModelFactory.CustomPrepareCrossSellProductListModelAsync(searchModel, product);

        return Json(model);
    }



    [HttpPost]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> CustomCrossSellProductAddPopupList(AddCrossSellProductSearchModel searchModel)
    {
        //prepare model
        var model = await _productModelFactory.CustomPrepareAddCrossSellProductListModelAsync(searchModel);

        return Json(model);
    }

    #region Dimension images

    [HttpPost]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> DimesionImagesList(ProductDimensionPictureSearchModel searchModel)
    {
        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(searchModel.ProductId)
            ?? throw new ArgumentException("No product found with the specified id");

        //a vendor should have access only to his products
        if (await _workContext.GetCurrentVendorAsync() != null && product.VendorId != (await _workContext.GetCurrentVendorAsync()).Id)
            return Content("This is not your product");

        //prepare model

        string imagesPath = Path.Combine(_fileProvider.MapPath("/wwwroot/images/product/dimensionimages"));

        DirectoryInfo info = new DirectoryInfo(imagesPath);
        FileInfo[] files = info.GetFiles().Where(p => p.Name.StartsWith(searchModel.ProductId + "_")).OrderByDescending(p => p.CreationTime).ToArray();


        // Generate URLs and creation dates for the images
        List<ProductDimensionPictureModel> productDimensionImages = new List<ProductDimensionPictureModel>();
        foreach (FileInfo file in files)
        {

            // Create an object with URL and creation date
            var imageInfo = new ProductDimensionPictureModel()
            {
                Url = "/images/product/dimensionimages/" + file.Name,
                CreatedOn = file.CreationTime
            };

            productDimensionImages.Add(imageInfo);
        }

        ProductDimensionPictureListModel model = new ProductDimensionPictureListModel();
        model.Data = productDimensionImages;
        model.Draw = searchModel.Draw;
        model.RecordsFiltered = model.RecordsTotal = productDimensionImages.Count;
        return Json(model);
    }



    [HttpPost]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> DeleteDimensionImage(string id)
    {
        // Validate the image URL
        if (string.IsNullOrEmpty(id))
        {
            return BadRequest("Invalid image URL.");
        }

        string imagePath = Path.Combine(_fileProvider.MapPath("/wwwroot/images/product/dimensionimages"), Path.GetFileName(id));

        if (System.IO.File.Exists(imagePath))
        {
            try
            {
                System.IO.File.Delete(imagePath);
                return new NullJsonResult();
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }
        else
        {
            return Content("Not Found!!");
        }
    }

    [HttpPost]
    public virtual async Task<IActionResult> BulkUploadDimensionImages(int productId, string producyType, IFormCollection images)
    {
        string imagesPath = _fileProvider.MapPath("/wwwroot/images/product/dimensionimages");

        if (images != null && images.Files.Count > 0)
        {
            foreach (var file in images.Files)
            {
                if (file != null && file.Length > 0)
                {
                    string uniqueFileName = productId + "_" + file.FileName;
                    string filePath = Path.Combine(imagesPath, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                }
            }
        }
        return Ok();
    }


    #region SuggestedKeyword

    [HttpPost]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> SuggestedKeywordList(ProductSuggestedKeywordSearchModel model, int productId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE) && !await _permissionService.AuthorizeAsync(StandardPermission.Catalog.CATEGORIES_CREATE_EDIT_DELETE))
            return await AccessDeniedJsonAsync();

        //try to get a product with the specified id
        if (productId != 0)
        {
            var categorySuggestedKeywords = await _productModelFactory.CustomPrepareSuggestedKeywordListModelAsync(model, productId);

            return Json(categorySuggestedKeywords);

        }
        else
            throw new ArgumentException("Please provide productId " + productId);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> SuggestedKeywordDelete(int id)
    {
        //try to get a Collection product with the specified id
        var categorySuggestedKeyword = await _productModelFactory.GetProductSuggestedKeywordById(id)
          ?? throw new ArgumentException("No Filter Mapping By Entity found with the specified id");
        await _productModelFactory.DeleteProductSuggestedKeyword(categorySuggestedKeyword);
        return new NullJsonResult();
    }

    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> SuggestedKeywordAddPopup(int productId)
    {
        ProductSuggestedKeywordModel model = new ProductSuggestedKeywordModel();
        return View(model);
    }



    [HttpPost]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> SuggestedKeywordCreate(ProductSuggestedKeywordModel model)
    {
        if (ModelState.IsValid)
        {
            if (await _productModelFactory.IsProductKeywordExist(model.SuggestedKeyWordId, model.KeyWord, model.ProductId))
            {
                ModelState.AddModelError("", "Suggested Keyword already exist");
            }
            else
            {
                ProductSuggestedKeywordModel productSuggestedKeyword = new ProductSuggestedKeywordModel();
                productSuggestedKeyword.Id = model.Id;
                productSuggestedKeyword.KeyWord = model.KeyWord;
                productSuggestedKeyword.ProductId = model.ProductId;
                productSuggestedKeyword.SuggestedKeyWordId = model.SuggestedKeyWordId;

                await _productModelFactory.CreateProductSuggestedKeyword(productSuggestedKeyword);

                ViewBag.RefreshPage = true;
                return View("SuggestedKeywordAddPopup", productSuggestedKeyword);
            }
        }
        return View("SuggestedKeywordAddPopup", model);
    }
    #endregion

    #region Variants 

    [HttpPost]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> VariantList(VariantSearchModel searchModel)
    {
        //try to get a product with the specified id

        var product = await _productService.GetProductByIdAsync(searchModel.ProductId)
            ?? throw new ArgumentException("No product found with the specified id");



        //prepare model
        var model = await _productModelFactory.PrepareVariantListModelAsync(searchModel, product);

        return Json(model);
    }


    /// <returns>A task that represents the asynchronous operation</returns>
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> VariantEdit(int id)
    {
        var variantModel = await _productModelFactory.PrepareVariantModelAsync(id);
        return View(variantModel);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> VariantEdit(VariantModel model, bool continueEditing)
    {
        var _productService = EngineContext.Current.Resolve<IProductExtendedService>();
        var variant = await _productService.GetVariantById(model.Id);
        if (variant == null)
            return RedirectToAction("Edit", new { id = model.ProductId });

        var product = await _productService.GetProductByIdAsync(variant.ProductId);
        if (!string.IsNullOrWhiteSpace(model.Title) && !model.Title.Trim().Equals(product.Name.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            variant.SeName = await _urlRecordService.ValidateSeNameAsync(variant.VariantId, "VariantCombination", model.SeName, model.Title, false);

        }
        else if (!string.IsNullOrEmpty(model.SeName) && variant.SeName != model.SeName)
        {
            string prdSeName = await _urlRecordService.GetSeNameAsync(product, (await _workContext.GetWorkingLanguageAsync())?.Id ?? 0, false, false);
            if (prdSeName != model.SeName.Trim())
            {
                model.SeName = variant.SeName = await _urlRecordService.ValidateSeNameAsync(variant.VariantId, "VariantCombination", model.SeName, model.Title, false);

            }
        }





        variant.OldPrice = model.OldPrice;
        variant.Price = model.Price;
        variant.Msrp = model.Msrp;
        variant.Title = model.Title;
        variant.EnableSurcharge = model.EnableSurcharge;
        variant.EstimatedDeliveryDate = model.EstimatedDeliveryDate;
        variant.WgsRequired = model.WgsRequired;
        variant.QueryParameter = model.QueryParameter;
        variant.Weight = model.Weight;
        variant.ManufacturerPartNumber = model.ManufacturerPartNumber;
        variant.DimensionPictureId = model.DimensionPictureId;
        await _productService.UpdateVariant(variant);
        if (!continueEditing)
            return RedirectToAction("Edit", new { id = model.ProductId });


        return RedirectToAction("VariantEdit", new { id = model.Id });

    }


    #endregion
    #endregion


    [HttpPost]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> CustomAssociatedProductDelete(int id)
    {
        //try to get an associated product with the specified id
        var _productService = EngineContext.Current.Resolve<IProductExtendedService>();
        var product = await _productService.GetProductByIdAsync(id)
            ?? throw new ArgumentException("No associated product found with the specified id");

        //a vendor should have access only to his products
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return Content("This is not your product");
        var parentGroupId = product.ParentGroupedProductId;
        product.ParentGroupedProductId = 0;
        await _productService.UpdateProductAsync(product);
        await _productService.UpdateGroupProductPrice(parentGroupId);
        return new NullJsonResult();
    }

    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> CustomAssociatedProductAddPopup(AddAssociatedProductModel model)
    {
        var _productService = EngineContext.Current.Resolve<IProductExtendedService>();
        var selectedProducts = await _productService.GetProductsByIdsAsync(model.SelectedProductIds.ToArray());
        //prepare model
        var tryToAddSelfGroupedProduct = selectedProducts
               .Select(p => p.Id)
               .Contains(model.ProductId);

        if (selectedProducts.Any())
        {
            foreach (var product in selectedProducts)
            {
                if (product.Id == model.ProductId)
                    continue;

                //a vendor should have access only to his products
                if (await _workContext.GetCurrentVendorAsync() != null && product.VendorId != (await _workContext.GetCurrentVendorAsync()).Id)
                    continue;

                product.ParentGroupedProductId = model.ProductId;
                await _productService.UpdateProductAsync(product);
            }
        }

        if (tryToAddSelfGroupedProduct)
        {
            _notificationService.WarningNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.AssociatedProducts.TryToAddSelfGroupedProduct"));

            var addAssociatedProductSearchModel = await _productModelFactory.PrepareAddAssociatedProductSearchModelAsync(new AddAssociatedProductSearchModel());
            //set current product id
            addAssociatedProductSearchModel.ProductId = model.ProductId;

            ViewBag.RefreshPage = true;

            return View("AssociatedProductAddPopup", addAssociatedProductSearchModel);
        }

        ViewBag.RefreshPage = true;

        ViewBag.ClosePage = true;
        await _productService.UpdateGroupProductPrice(model.ProductId);
        return View("AssociatedProductAddPopup", new AddAssociatedProductSearchModel());

    }

    #region Grouped Products Configuration

    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> GroupedProductConfigurationList(GroupedProductConfigurationSearchModel searchModel)
    {
        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(searchModel.ProductId)
            ?? throw new ArgumentException("No product found with the specified id");

        //a vendor should have access only to his products
        if (await _workContext.GetCurrentVendorAsync() != null && product.VendorId != (await _workContext.GetCurrentVendorAsync()).Id)
            return Content("This is not your product");

        //prepare model
        var model = await _productModelFactory.PrepareGroupedProductConfiguration(searchModel.ProductId);

        return View("_GroupedProductConfigurationList", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> GroupedProductUpdate(List<GroupedProductConfigurationModel> model)
    {
        //try to get a FBT product with the specified id
        var _groupedProductConfigurationService = EngineContext.Current.Resolve<IGroupedProductConfigurationService>();

        var configurations = await _groupedProductConfigurationService.GetConfigurationOfGroupedProduct(model.First().ProductId);

        foreach (var configuration in model)
        {
            await _groupedProductConfigurationService.UpdateGroupProductConfiguration(new GroupedProductConfiguration()
            {
                ProductId = configuration.ProductId,
                Id = configuration.Id,
                ProductAttributeOptionId = configuration.ProductAttributeOptionId,
                Raw = JsonConvert.SerializeObject(configuration.Configurations)

            });
        }

        foreach (var configuration in configurations)
        {
            if (!model.Where(m => m.Id == configuration.Id).Any())
                await _groupedProductConfigurationService.DeleteGroupProductConfiguration(configuration);
        }

        ViewBag.RefreshPage = true;
        return View("_GroupedProductConfigurationList", await _productModelFactory.PrepareGroupedProductConfiguration(model.First().ProductId));
    }



    #endregion



    [HttpPost]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
    public virtual async Task<IActionResult> CustomProductAttributeMappingList(ProductAttributeMappingSearchModel searchModel)
    {
        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(searchModel.ProductId)
            ?? throw new ArgumentException("No product found with the specified id");

        //a vendor should have access only to his products
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return Content("This is not your product");

        //prepare model
        var model = await _productModelFactory.CustomPrepareProductAttributeMappingListModelAsync(searchModel, product);

        return Json(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProductAttributeMappingPublishStatus(int productId, int mappingId, bool isPublished)
    {
        var _productService = EngineContext.Current.Resolve<IProductExtendedService>();
        var product = await _productService.GetProductByIdAsync(productId);
        if (product != null)
        {
            var mapping = (await _productAttributeService.GetProductAttributeMappingByIdAsync(mappingId));
            if (mapping != null)
            {
                var attributeValues = await _productAttributeService.GetProductAttributeValuesAsync(mapping.Id);
                foreach (var attributeValue in attributeValues)
                {
                    if (attributeValue.Published != isPublished)
                    {
                        attributeValue.Published = isPublished;
                        await _productAttributeService.UpdateProductAttributeValueAsync(attributeValue);
                        if (!isPublished)
                            await this._productModelFactory.SyncAttributeCombinations(product, mapping, null);
                    }
                }

            }
        }
        await _productService.GenerateVariantCombinations(product.Id);
        return new NullJsonResult();
    }

    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> CustomProductAttributeMappingCreate(int productId)
    {
        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(productId)
            ?? throw new ArgumentException("No product found with the specified id");

        //a vendor should have access only to his products
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("This is not your product"));
            return RedirectToAction("List");
        }

        //prepare model
        var model = await _productModelFactory.PrepareProductAttributeMappingModelAsync(new ProductAttributeMappingModel(), product, null);

        return View(model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> CustomProductAttributeMappingCreate(ProductAttributeMappingModel model, bool continueEditing)
    {
        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(model.ProductId)
            ?? throw new ArgumentException("No product found with the specified id");

        //a vendor should have access only to his products
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("This is not your product"));
            return RedirectToAction("List");
        }

        //ensure this attribute is not mapped yet
        if ((await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id))
            .Any(x => x.ProductAttributeId == model.ProductAttributeId))
        {
            //redisplay form
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.AlreadyExists"));

            model = await _productModelFactory.PrepareProductAttributeMappingModelAsync(model, product, null, true);

            return View(model);
        }

        //insert mapping
        var productAttributeMapping = model.ToEntity<ProductAttributeMapping>();

        await _productAttributeService.InsertProductAttributeMappingAsync(productAttributeMapping);
        await UpdateLocalesAsync(productAttributeMapping, model);

        //predefined values
        //var predefinedValues = await _productAttributeService.GetPredefinedProductAttributeValuesAsync(model.ProductAttributeId);
        //foreach (var predefinedValue in predefinedValues)
        //{
        //    var pav = new ProductAttributeValue
        //    {
        //        ProductAttributeMappingId = productAttributeMapping.Id,
        //        AttributeValueType = AttributeValueType.Simple,
        //        Name = predefinedValue.Name,
        //        PriceAdjustment = predefinedValue.PriceAdjustment,
        //        PriceAdjustmentUsePercentage = predefinedValue.PriceAdjustmentUsePercentage,
        //        WeightAdjustment = predefinedValue.WeightAdjustment,
        //        Cost = predefinedValue.Cost,
        //        IsPreSelected = predefinedValue.IsPreSelected,
        //        DisplayOrder = predefinedValue.DisplayOrder
        //    };
        //       await _productAttributeService.InsertProductAttributeValueAsync(pav);

        //    //locales
        //    var languages = await _languageService.GetAllLanguagesAsync(true);

        //    //localization
        //    foreach (var lang in languages)
        //    {
        //        var name = await _localizationService.GetLocalizedAsync(predefinedValue, x => x.Name, lang.Id, false, false);
        //        if (!string.IsNullOrEmpty(name))
        //            await _localizedEntityService.SaveLocalizedValueAsync(pav, x => x.Name, name, lang.Id);
        //    }
        //}

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.Added"));

        if (!continueEditing)
        {
            //select an appropriate card
            SaveSelectedCardName("product-product-attributes");
            return RedirectToAction("Edit", new { id = product.Id });
        }

        return RedirectToAction("CustomProductAttributeMappingEdit", new { id = productAttributeMapping.Id });
    }

    [CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
    public virtual async Task<IActionResult> CustomProductAttributeMappingEdit(int id)
    {
        //try to get a product attribute mapping with the specified id
        var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(id)
            ?? throw new ArgumentException("No product attribute mapping found with the specified id");

        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(productAttributeMapping.ProductId)
            ?? throw new ArgumentException("No product found with the specified id");

        //a vendor should have access only to his products
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("This is not your product"));
            return RedirectToAction("List");
        }

        //prepare model
        var model = await _productModelFactory.PrepareProductAttributeMappingModelAsync(null, product, productAttributeMapping);
        model.IsExpanded = productAttributeMapping.IsExpanded;
        model.EnableHoverImpact = productAttributeMapping.EnableHoverImpact;

        return View(model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> CustomProductAttributeMappingEdit(ProductAttributeMappingModel model, bool continueEditing, IFormCollection form)
    {
        //try to get a product attribute mapping with the specified id
        var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(model.Id)
            ?? throw new ArgumentException("No product attribute mapping found with the specified id");

        //try to get a product with the specified id
        var _productService = EngineContext.Current.Resolve<IProductExtendedService>();
        var product = await _productService.GetProductByIdAsync(productAttributeMapping.ProductId)
            ?? throw new ArgumentException("No product found with the specified id");

        //a vendor should have access only to his products
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("This is not your product"));
            return RedirectToAction("List");
        }

        //ensure this attribute is not mapped yet
        if ((await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id))
            .Any(x => x.ProductAttributeId == model.ProductAttributeId && x.Id != productAttributeMapping.Id))
        {
            //redisplay form
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.AlreadyExists"));

            model = await _productModelFactory.PrepareProductAttributeMappingModelAsync(model, product, productAttributeMapping, true);
            model.IsExpanded = productAttributeMapping.IsExpanded;
            model.EnableHoverImpact = productAttributeMapping.EnableHoverImpact;
            return View(model);
        }

        //fill entity from model
        productAttributeMapping = model.ToEntity(productAttributeMapping);
        await _productAttributeService.UpdateProductAttributeMappingAsync(productAttributeMapping);
        await this._productModelFactory.SyncAttributeCombinations(product, productAttributeMapping, null);

        await _productService.GenerateVariantCombinations(product.Id);
        await UpdateLocalesAsync(productAttributeMapping, model);

        await SaveConditionAttributesAsync(productAttributeMapping, model.ConditionModel, form);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.Updated"));

        if (!continueEditing)
        {
            //select an appropriate card
            SaveSelectedCardName("product-product-attributes");
            return RedirectToAction("Edit", new { id = product.Id });
        }

        return RedirectToAction("CustomProductAttributeMappingEdit", new { id = productAttributeMapping.Id });
    }
    #region Product Attribute Combinations Import/Export
    [HttpPost]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> BulkDeleteCombinations(ICollection<int> ids)
    {
        int productId = 0;


        foreach (var id in ids)
        {
            await _productAttributeService.DeleteProductAttributeCombinationAsync(await _productAttributeService.GetProductAttributeCombinationByIdAsync(id));
        }

        return Json(new { Result = true });


    }

    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> ExportProductAttributeCombination(int productid)
    {
        var attributeCombinations = await _productAttributeService.GetAllProductAttributeCombinationsAsync(productid);

        if (!attributeCombinations.Any())
        {
            return RedirectToAction("Edit", new { id = productid });
        }
        try
        {
            var product = await _productService.GetProductByIdAsync(productid);

            if (product == null)
            {
                return RedirectToAction("Edit", new { id = productid });
            }
            var _productAttributeFormatter = EngineContext.Current.Resolve<ICustomProductAttributeFormatter>();
            List<ExportAttrCombination> exprtpacFormat = new List<ExportAttrCombination>();
            foreach (var combination in attributeCombinations)
            {
                ExportAttrCombination epac = new ExportAttrCombination();
                epac.ProductId = combination.ProductId;
                epac.AttributeXml = combination.AttributesXml;
                epac.AttributeDescription = await _productAttributeFormatter.CustomFormatAttributesAsync(product, combination.AttributesXml);
                epac.Msrp = combination.OverriddenMsrp;
                epac.Price = combination.OverriddenOldPrice;
                epac.SalePrice = combination.OverriddenPrice;
                exprtpacFormat.Add(epac);
            }
            var _exportManager = EngineContext.Current.Resolve<IExportExtendedManager>();
            var bytes = await _exportManager.ExportProductAttributeCombinationToXlsxAsync(exprtpacFormat);
            return File(bytes, MimeTypes.TextXlsx, "product-attribute-combinations.xlsx");
        }
        catch (Exception exc)
        {
            await _notificationService.ErrorNotificationAsync(exc);
            return RedirectToAction("Edit", new
            {
                id = productid
            });
        }
    }


    [HttpPost]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> ImportProductAttributeCombination(IFormFile importexcelfile, int id)
    {
        var _importManager = EngineContext.Current.Resolve<IImportExtendedManager>();
        if (importexcelfile != null && importexcelfile.Length > 0)
            await _importManager.ImportProductAttributeCombinationFromXlsxAsync(importexcelfile.OpenReadStream());
        else
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Common.UploadFile"));
        return RedirectToAction("Edit", new
        {
            id = id
        });
    }

    #endregion


    [HttpPost]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> CustomProductAttributeMappingDelete(int id)
    {
        //try to get a product attribute mapping with the specified id
        var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(id)
            ?? throw new ArgumentException("No product attribute mapping found with the specified id");

        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(productAttributeMapping.ProductId)
            ?? throw new ArgumentException("No product found with the specified id");

        //a vendor should have access only to his products
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return Content("This is not your product");

        //check if existed combinations contains the specified attribute
        var existedCombinations = await _productAttributeService.GetAllProductAttributeCombinationsAsync(product.Id);
        if (existedCombinations?.Any() == true)
        {
            foreach (var combination in existedCombinations)
            {
                await _productAttributeService.DeleteProductAttributeCombinationAsync(combination);
            }
        }

        await _productAttributeService.DeleteProductAttributeMappingAsync(productAttributeMapping);
        await GenerateAttributeCombinationsAsync(product);
        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.Deleted"));

        //select an appropriate card
        SaveSelectedCardName("product-product-attributes");
        return RedirectToAction("Edit", new { id = productAttributeMapping.ProductId });
    }

    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> CustomProductAttributeValueCreatePopup(int productAttributeMappingId)
    {
        //try to get a product attribute mapping with the specified id
        var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(productAttributeMappingId)
            ?? throw new ArgumentException("No product attribute mapping found with the specified id");

        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(productAttributeMapping.ProductId)
            ?? throw new ArgumentException("No product found with the specified id");

        //a vendor should have access only to his products
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return RedirectToAction("List", "Product");

        //prepare model
        var model = await _productModelFactory.PrepareProductAttributeValueModelAsync(new ProductAttributeValueModel(), productAttributeMapping, null);

        var pv = await _productAttributeService.GetProductAttributeByIdAsync(productAttributeMapping.ProductAttributeId);
         return View("ProductAttributeValueCreatePopup", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> CustomProductAttributeValueCreatePopup(ProductAttributeValueModel model)
    {
        //try to get a product attribute mapping with the specified id
        var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(model.ProductAttributeMappingId);
        if (productAttributeMapping == null)
            return RedirectToAction("List", "Product");

        var _productService = EngineContext.Current.Resolve<IProductExtendedService>();
        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(productAttributeMapping.ProductId)
            ?? throw new ArgumentException("No product found with the specified id");

        //a vendor should have access only to his products
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return RedirectToAction("List", "Product");

        if (productAttributeMapping.AttributeControlType == AttributeControlType.ColorSquares)
        {
            //ensure valid color is chosen/entered
            if (string.IsNullOrEmpty(model.ColorSquaresRgb))
                ModelState.AddModelError(string.Empty, "Color is required");
            try
            {
                //ensure color is valid (can be instantiated)
                System.Drawing.ColorTranslator.FromHtml(model.ColorSquaresRgb);
            }
            catch (Exception exc)
            {
                ModelState.AddModelError(string.Empty, exc.Message);
            }
        }

        //ensure a picture is uploaded
        if (productAttributeMapping.AttributeControlType == AttributeControlType.ImageSquares && model.ImageSquaresPictureId == 0)
        {
            ModelState.AddModelError(string.Empty, "Image is required");
        }

        if (ModelState.IsValid)
        {
            //fill entity from model
            var pav = model.ToEntity<ProductAttributeValue>();

            pav.Quantity = model.CustomerEntersQty ? 1 : model.Quantity;

            await _productAttributeService.InsertProductAttributeValueAsync(pav);
            await this._productModelFactory.SyncAttributeCombinations(product, productAttributeMapping, pav);

            int variantId = await _productService.GenerateVariantIdAsync(productAttributeMapping, pav.Id, pav.Name);

            if (variantId > 0)
            {
                pav.VariantId = variantId;
                await _productAttributeService.UpdateProductAttributeValueAsync(pav);
            }

            //if (model.GalleryPictures != null && model.GalleryPictures.Count > 0)
            //{
            //    var count = 1;
            //    foreach (var pictureId in model.GalleryPictures)

            //    {
            //        await _productAttributeService.CreateProductAttributeValueGalleryPictureMapping(pav.Id, pictureId, count);

            //        count++;
            //    }
            //}

            await UpdateLocalesAsync(pav, model);
            await SaveAttributeValuePicturesAsync(product, pav, model);

            ViewBag.RefreshPage = true;

            return View("ProductAttributeValueCreatePopup", model);
        }

        //prepare model
        model = await _productModelFactory.CustomPrepareProductAttributeValueModelAsync(model, productAttributeMapping, null, true);

        //if we got this far, something failed, redisplay form
        return View("ProductAttributeValueCreatePopup", model);
    }

    [CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
    public virtual async Task<IActionResult> CustomProductAttributeValueEditPopup(int id)
    {
        //try to get a product attribute value with the specified id
        var productAttributeValue = await _productAttributeService.GetProductAttributeValueByIdAsync(id);
        if (productAttributeValue == null)
            return RedirectToAction("List", "Product");

        //try to get a product attribute mapping with the specified id
        var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(productAttributeValue.ProductAttributeMappingId);
        if (productAttributeMapping == null)
            return RedirectToAction("List", "Product");

        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(productAttributeMapping.ProductId)
            ?? throw new ArgumentException("No product found with the specified id");

        //a vendor should have access only to his products
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return RedirectToAction("List", "Product");

        //prepare model
        var model = await _productModelFactory.CustomPrepareProductAttributeValueModelAsync(null, productAttributeMapping, productAttributeValue);
        var pv = await _productAttributeService.GetProductAttributeByIdAsync(productAttributeMapping.ProductAttributeId);
      
        return View("ProductAttributeValueEditPopup", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> CustomProductAttributeValueEditPopup(ProductAttributeValueModel model)
    {
        //try to get a product attribute value with the specified id
        var productAttributeValue = await _productAttributeService.GetProductAttributeValueByIdAsync(model.Id);
        if (productAttributeValue == null)
            return RedirectToAction("List", "Product");

        //try to get a product attribute mapping with the specified id
        var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(productAttributeValue.ProductAttributeMappingId);
        if (productAttributeMapping == null)
            return RedirectToAction("List", "Product");

        var _productService = EngineContext.Current.Resolve<IProductExtendedService>();

        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(productAttributeMapping.ProductId)
            ?? throw new ArgumentException("No product found with the specified id");

        //a vendor should have access only to his products
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return RedirectToAction("List", "Product");

        if (productAttributeMapping.AttributeControlType == AttributeControlType.ColorSquares)
        {
            //ensure valid color is chosen/entered
            if (string.IsNullOrEmpty(model.ColorSquaresRgb))
                ModelState.AddModelError(string.Empty, "Color is required");
            try
            {
                //ensure color is valid (can be instantiated)
                System.Drawing.ColorTranslator.FromHtml(model.ColorSquaresRgb);
            }
            catch (Exception exc)
            {
                ModelState.AddModelError(string.Empty, exc.Message);
            }
        }

        //ensure a picture is uploaded
        if (productAttributeMapping.AttributeControlType == AttributeControlType.ImageSquares && model.ImageSquaresPictureId == 0)
        {
            ModelState.AddModelError(string.Empty, "Image is required");
        }

        if (ModelState.IsValid)
        {
            //fill entity from model
            productAttributeValue = model.ToEntity(productAttributeValue);
            productAttributeValue.Quantity = model.CustomerEntersQty ? 1 : model.Quantity;
            await _productAttributeService.UpdateProductAttributeValueAsync(productAttributeValue);

            if (!model.Published)
            {
                var productAttributeValues = await _productAttributeService.GetProductAttributeValuesAsync(productAttributeValue.ProductAttributeMappingId);
                if (productAttributeValues.All(x => !x.Published))
                {
                    productAttributeMapping.IsRequired = false;
                    await _productAttributeService.UpdateProductAttributeMappingAsync(productAttributeMapping);
                }
                await this._productModelFactory.SyncAttributeCombinations(product, productAttributeMapping, null);
            }

            int variantId = await _productService.GenerateVariantIdAsync(productAttributeMapping, productAttributeValue.Id, productAttributeValue.Name);

            if (variantId != productAttributeValue.VariantId)
            {
                productAttributeValue.VariantId = variantId;
                await _productAttributeService.UpdateProductAttributeValueAsync(productAttributeValue);
            }



            //string attrName = (await _productAttributeService.GetProductAttributeByIdAsync(productAttributeMapping.ProductAttributeId)).Name;
            //if (string.Equals(attrName, await _localizationService.GetResourceAsync("product.attr.shades")) ||
            //    string.Equals(attrName, await _localizationService.GetResourceAsync("product.attr.configuration"))
            //    || string.Equals(attrName, await _localizationService.GetResourceAsync("product.attr.Size"))
            //    )

            //{
            //    var existingPictures = await _productAttributeService.GetProductAttributeValueGalleryPicturesByAttributeValueId(model.Id);
            //    if (model.GalleryPictures == null)
            //    {
            //        model.GalleryPictures = new List<int>();
            //    }
            //    if (model.GalleryPictures.Count > 0 || existingPictures.Count > 0)
            //    {
            //        List<int> toRemove = new List<int>();

            //        toRemove.AddRange(existingPictures.Except(model.GalleryPictures));
            //        var count = 1;
            //        foreach (var pictureId in model.GalleryPictures)
            //        {
            //            if (existingPictures.Contains(pictureId))
            //            {
            //                await _productAttributeService.UpdateProductAttributeValueGalleryPictureMapping(model.Id, pictureId, count);
            //            }
            //            else
            //            {
            //                await _productAttributeService.CreateProductAttributeValueGalleryPictureMapping(model.Id, pictureId, count);
            //            }
            //            count++;
            //        }
            //        if (toRemove.Count > 0)
            //        {
            //            foreach (var pictureId in toRemove)
            //            {
            //                await _productAttributeService.DeleteProductAttributeValueGalleryPictureMapping(model.Id, pictureId);
            //            }
            //        }
            //    }
            //}

            await UpdateLocalesAsync(productAttributeValue, model);
            await SaveAttributeValuePicturesAsync(product, productAttributeValue, model);

            ViewBag.RefreshPage = true;

            return View("ProductAttributeValueEditPopup", model);
        }

        //prepare model
        model = await _productModelFactory.CustomPrepareProductAttributeValueModelAsync(model, productAttributeMapping, productAttributeValue, true);
        var pv = await _productAttributeService.GetProductAttributeByIdAsync(productAttributeMapping.ProductAttributeId);
          //if we got this far, something failed, redisplay form
        return View("ProductAttributeValueEditPopup", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> CustomProductAttributeValueDelete(int id)
    {
        //try to get a product attribute value with the specified id
        var productAttributeValue = await _productAttributeService.GetProductAttributeValueByIdAsync(id)
            ?? throw new ArgumentException("No product attribute value found with the specified id");

        //try to get a product attribute mapping with the specified id
        var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(productAttributeValue.ProductAttributeMappingId)
            ?? throw new ArgumentException("No product attribute mapping found with the specified id");


        var _productService = EngineContext.Current.Resolve<IProductExtendedService>();


        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(productAttributeMapping.ProductId)
            ?? throw new ArgumentException("No product found with the specified id");

        //a vendor should have access only to his products
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return Content("This is not your product");
        // await _productAttributeService.DeleteAttributeValueGalleryPicturesByAttributeValueId(productAttributeValue.Id);
        //check if existed combinations contains the specified attribute value
        var existedCombinations = await _productAttributeService.GetAllProductAttributeCombinationsAsync(product.Id);
        if (existedCombinations?.Any() == true)
        {
            foreach (var combination in existedCombinations)
            {
                var attributeValues = await _productAttributeParser.ParseProductAttributeValuesAsync(combination.AttributesXml);

                if (attributeValues.Where(attribute => attribute.Id == id).Any())
                {
                    return Conflict(string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.Values.AlreadyExistsInCombination"),
                        await _productAttributeFormatter.FormatAttributesAsync(product, combination.AttributesXml, await _workContext.GetCurrentCustomerAsync(), await _storeContext.GetCurrentStoreAsync(), ", ")));
                }
            }
        }

        await _productAttributeService.DeleteProductAttributeValueAsync(productAttributeValue);
        await this._productModelFactory.SyncAttributeCombinations(product, productAttributeMapping, null);

        await _productService.GenerateVariantCombinations(product.Id);


        return new NullJsonResult();
    }



    [HttpPost]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
    public virtual async Task<IActionResult> CustomProductAttributeCombinationList(ProductAttributeCombinationSearchModel searchModel)
    {
        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(searchModel.ProductId)
            ?? throw new ArgumentException("No product found with the specified id");

        // 1. Get the Valid IDs from the form post sent by the JS Brain
        var validIdsJson = Request.Form["validCombinationIds"].FirstOrDefault();
        List<int> validIds = null;

        if (!string.IsNullOrEmpty(validIdsJson))
        {
            try
            {
                validIds = System.Text.Json.JsonSerializer.Deserialize<List<int>>(validIdsJson);
            }
            catch { /* Fallback to null if JSON is invalid */ }
        }
        // 2. Prepare the model using the factory, passing the filter IDs
        var model = await _productModelFactory.CustomPrepareProductAttributeCombinationListModelAsync(searchModel, product, validIds);

        return Json(model);
    }

    [HttpGet]
    public virtual async Task<IActionResult> GetLatestSmartInjectorData(int productId)
    {
        // Logic to rebuild the blueprint and master list
        var product = await _productService.GetProductByIdAsync(productId);
        ProductModel model = new ProductModel();
        await _productModelFactory.PrepareSmartInjectionDataAsync(model, product);

        return Json(new
        {
            logicMasterList = model.LogicMasterListJson,
            fullAttributeDefinitions = model.FullAttributeDefinitionsJson,
            availablePicturesJson = model.AvailablePicturesJson
        });
    }


    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> ProductCustomAttributeCombinationCreatePopup(int productId)
    {
        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null)
            return RedirectToAction("List", "Product");

        //a vendor should have access only to his products
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return RedirectToAction("List", "Product");

        //prepare model
        var model = await _productModelFactory.PrepareProductAttributeCombinationModelAsync(new ProductAttributeCombinationModel(), product, null);

        return View("ProductAttributeCombinationCreatePopup", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> ProductCustomAttributeCombinationCreatePopup(int productId, ProductAttributeCombinationModel model, IFormCollection form)
    {
        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null)
            return RedirectToAction("List", "Product");

        //a vendor should have access only to his products
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return RedirectToAction("List", "Product");

        //attributes
        var warnings = new List<string>();
        var attributesXml = await GetAttributesXmlForProductAttributeCombinationAsync(form, warnings, product.Id);

        //check whether the attribute value is specified
        if (string.IsNullOrEmpty(attributesXml))
            warnings.Add(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.AttributeCombinations.Alert.FailedValue"));

        warnings.AddRange(await _shoppingCartService.GetShoppingCartItemAttributeWarningsAsync(await _workContext.GetCurrentCustomerAsync(),
            ShoppingCartType.ShoppingCart, product, 1, attributesXml, true));

        //check whether the same attribute combination already exists
        var existingCombination = await _productAttributeParser.FindProductAttributeCombinationAsync(product, attributesXml);
        if (existingCombination != null)
            warnings.Add(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.AttributeCombinations.AlreadyExists"));

        if (!warnings.Any())
        {
            //save combination
            var combination = model.ToEntity<ProductAttributeCombination>();

            //fill attributes
            combination.AttributesXml = attributesXml;

            await _productAttributeService.InsertProductAttributeCombinationAsync(combination);

            await SaveAttributeCombinationPicturesAsync(product, combination, model);

            //quantity change history
            await _productService.AddStockQuantityHistoryEntryAsync(product, combination.StockQuantity, combination.StockQuantity,
                message: await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.Combination.Edit"), combinationId: combination.Id);
            //if (model.GalleryPictures != null && model.GalleryPictures.Count > 0)
            //{
            //    var count = 1;
            //    foreach (var pictureId in model.GalleryPictures)

            //    {
            //        await _productAttributeService.CreateProductAttributeCombinationGalleryPictureMapping(combination.Id, pictureId, count);

            //        count++;
            //    }
            //}
            ViewBag.RefreshPage = true;

            return View("ProductAttributeCombinationCreatePopup", model);
        }

        //prepare model
        model = await _productModelFactory.PrepareProductAttributeCombinationModelAsync(model, product, null, true);
        model.Warnings = warnings;

        //if we got this far, something failed, redisplay form
        return View("ProductAttributeCombinationCreatePopup", model);
    }



    [CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
    public virtual async Task<IActionResult> ProductCustomAttributeCombinationEditPopup(int id)
    {
        //try to get a combination with the specified id
        var combination = await _productAttributeService.GetProductAttributeCombinationByIdAsync(id);
        if (combination == null)
            return RedirectToAction("List", "Product");

        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(combination.ProductId);
        if (product == null)
            return RedirectToAction("List", "Product");

        //a vendor should have access only to his products
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return RedirectToAction("List", "Product");

        //prepare model
        var model = await _productModelFactory.PrepareCustomProductAttributeCombinationModelAsync(null, product, combination);

        return View("ProductAttributeCombinationEditPopup", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
    public virtual async Task<IActionResult> ProductCustomAttributeCombinationEditPopup(ProductAttributeCombinationModel model, IFormCollection form)
    {
        //try to get a combination with the specified id
        var combination = await _productAttributeService.GetProductAttributeCombinationByIdAsync(model.Id);
        if (combination == null)
            return RedirectToAction("List", "Product");

        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(combination.ProductId);
        if (product == null)
            return RedirectToAction("List", "Product");

        //a vendor should have access only to his products
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return RedirectToAction("List", "Product");

        //attributes
        var warnings = new List<string>();
        var attributesXml = await GetAttributesXmlForProductAttributeCombinationAsync(form, warnings, product.Id);

        //check whether the attribute value is specified
        if (string.IsNullOrEmpty(attributesXml))
            warnings.Add(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.AttributeCombinations.Alert.FailedValue"));

        warnings.AddRange(await _shoppingCartService.GetShoppingCartItemAttributeWarningsAsync(await _workContext.GetCurrentCustomerAsync(),
            ShoppingCartType.ShoppingCart, product, 1, attributesXml, true));

        //check whether the same attribute combination already exists
        var existingCombination = await _productAttributeParser.FindProductAttributeCombinationAsync(product, attributesXml);
        if (existingCombination != null && existingCombination.Id != model.Id && existingCombination.AttributesXml.Equals(attributesXml))
            warnings.Add(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.AttributeCombinations.AlreadyExists"));

        if (!warnings.Any() && ModelState.IsValid)
        {
            var previousStockQuantity = combination.StockQuantity;

            //save combination
            //fill entity from model
            combination = model.ToEntity(combination);
            combination.AttributesXml = attributesXml;

            await _productAttributeService.UpdateProductAttributeCombinationAsync(combination);

            await SaveAttributeCombinationPicturesAsync(product, combination, model);

            //quantity change history
            await _productService.AddStockQuantityHistoryEntryAsync(product, combination.StockQuantity - previousStockQuantity, combination.StockQuantity,
                message: await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.Combination.Edit"), combinationId: combination.Id);

            //var existingPictures = await _productAttributeService.GetProductAttributeCombinationGalleryPicturesByProductAttributeCombinationId(model.Id);
            //if (model.GalleryPictures == null)
            //{
            //    model.GalleryPictures = new List<int>();
            //}
            //if (model.GalleryPictures.Count > 0 || existingPictures.Count > 0)
            //{
            //    List<int> toRemove = new List<int>();

            //    toRemove.AddRange(existingPictures.Except(model.GalleryPictures));
            //    var count = 1;
            //    foreach (var pictureId in model.GalleryPictures)
            //    {
            //        if (existingPictures.Contains(pictureId))
            //        {
            //            await _productAttributeService.   (model.Id, pictureId, count);
            //        }
            //        else
            //        {
            //            await _productAttributeService.CreateProductAttributeCombinationGalleryPictureMapping(model.Id, pictureId, count);
            //        }
            //        count++;
            //    }
            //    if (toRemove.Count > 0)
            //    {
            //        foreach (var pictureId in toRemove)
            //        {
            //            await _productAttributeService.DeleteProductAttributeCombinationGalleryPictureMapping(model.Id, pictureId);
            //        }
            //    }
            //}


            ViewBag.RefreshPage = true;

            return View("ProductAttributeCombinationEditPopup", model);
        }

        //prepare model
        model = await _productModelFactory.PrepareProductAttributeCombinationModelAsync(model, product, combination, true);
        model.Warnings = warnings;

        //if we got this far, something failed, redisplay form
        return View("ProductAttributeCombinationEditPopup", model);
    }
    [HttpPost]
    public virtual async Task<IActionResult> InjectAttributeValue(AttributeCombinationInjectionRequestModel model)
    {
        // 1. Check for nulls
        if (model == null || model.TargetCombinationId == 0 || string.IsNullOrEmpty(model.AttributesToInjectJson))
            return Json(new { success = false, message = "Invalid injection payload." });

        // 2. Deserialize the JSON string back into our Dictionary
        var attributesToInject = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, int>>(model.AttributesToInjectJson);

        if (attributesToInject == null || !attributesToInject.Any())
            return Json(new { success = false, message = "No attributes to inject." });

        var combination = await _productAttributeService.GetProductAttributeCombinationByIdAsync(model.TargetCombinationId);
        if (combination == null)
            return Json(new { success = false, message = "Target combination not found." });

        var existingXml = combination.AttributesXml;

        // 3. Loop through our deserialized dictionary (Everything else stays the same!)
        foreach (var injection in attributesToInject)
        {
            int mappingId = injection.Key;
            int valueId = injection.Value;

            var attributeMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(mappingId);
            if (attributeMapping != null)
            {
                existingXml = _productAttributeParser.AddProductAttribute(
                    existingXml,
                    attributeMapping,
                    valueId.ToString()
                );
            }
        }

        combination.AttributesXml = existingXml;
        await _productAttributeService.UpdateProductAttributeCombinationAsync(combination);

        return Json(new { success = true, message = "Attributes successfully injected." });
    }

    #region PairWithproducts

    [HttpPost]

    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> PairWithProductList(RelatedProductSearchModel searchModel)
    {

        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(searchModel.ProductId)
            ?? throw new ArgumentException("No product found with the specified id");

        //a vendor should have access only to his products
        if (await _workContext.GetCurrentVendorAsync() != null && product.VendorId != (await _workContext.GetCurrentVendorAsync()).Id)
            return Content("This is not your product");

        //prepare model
        var model = await _productModelFactory.PreparePairWithProductListModelAsync(searchModel, product);

        return Json(model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> PairWithProductUpdate(PairWithProductModel model)
    {
        var _productService = EngineContext.Current.Resolve<IProductExtendedService>();
        //try to get a PairWith product with the specified id
        var PairWithProduct = await _productService.GetPairWithProductByIdAsync(model.Id)
            ?? throw new ArgumentException("No PairWith product found with the specified id");

        //a vendor should have access only to his products
        if (await _workContext.GetCurrentVendorAsync() != null)
        {
            var product = await _productService.GetProductByIdAsync(PairWithProduct.ProductId1);
            if (product != null && product.VendorId != (await _workContext.GetCurrentVendorAsync()).Id)
                return Content("This is not your product");
        }

        PairWithProduct.DisplayOrder = model.DisplayOrder;
        await _productService.UpdatePairWithProductAsync(PairWithProduct);

        return new NullJsonResult();
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> PairWithProductDelete(int id)
    {
        var _productService = EngineContext.Current.Resolve<IProductExtendedService>();
        //try to get a PairWith product with the specified id
        var PairWithProduct = await _productService.GetPairWithProductByIdAsync(id)
            ?? throw new ArgumentException("No PairWith product found with the specified id");

        var productId = PairWithProduct.ProductId1;

        //a vendor should have access only to his products
        if (await _workContext.GetCurrentVendorAsync() != null)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product != null && product.VendorId != (await _workContext.GetCurrentVendorAsync()).Id)
                return Content("This is not your product");
        }

        await _productService.DeletePairWithProductAsync(PairWithProduct);

        return new NullJsonResult();
    }

    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> PairWithProductAddPopup(int productId)
    {
        //prepare model
        var model = await _productModelFactory.PrepareAddPairWithProductSearchModelAsync(new AddPairWithProductSearchModel());

        return View(model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> PairWithProductAddPopupList(AddPairWithProductSearchModel searchModel)
    {
        //prepare model
        var model = await _productModelFactory.PrepareAddPairWithProductListModelAsync(searchModel);

        return Json(model);
    }

    [HttpPost]
    [FormValueRequired("save")]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IActionResult> PairWithProductAddPopup(AddPairWithProductModel model)
    {
        var _productService = EngineContext.Current.Resolve<IProductExtendedService>();
        var selectedProducts = await _productService.GetProductsByIdsAsync(model.SelectedProductIds.ToArray());
        if (selectedProducts.Any())
        {
            var existingPairWithProducts = await _productService.GetPairWithProductsByProductId1ListAsync(model.ProductId, showHidden: true);
            foreach (var product in selectedProducts)
            {
                //a vendor should have access only to his products
                if (await _workContext.GetCurrentVendorAsync() != null && product.VendorId != (await _workContext.GetCurrentVendorAsync()).Id)
                    continue;

                if (_productService.FindPairWithProduct(existingPairWithProducts, model.ProductId, product.Id) != null)
                    continue;

                await _productService.InsertPairWithProductAsync(new PairWithProduct
                {
                    ProductId1 = model.ProductId,
                    ProductId2 = product.Id,
                    DisplayOrder = 1
                });
            }
        }

        ViewBag.RefreshPage = true;

        return View(new AddPairWithProductSearchModel());
    }

    #endregion

    #region Picture
      public virtual async Task<IActionResult> CustomProductPictureAdd(int pictureId, int displayOrder,
    string overrideAltAttribute, string overrideTitleAttribute, int productId, bool displayOnListingModules = false, bool hideOnProductPage = false, bool displayOnCategoryPage = false, bool isDimensionImage = false)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if (pictureId == 0)
                throw new ArgumentException();

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(productId)
                ?? throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (await _workContext.GetCurrentVendorAsync() != null && product.VendorId != (await _workContext.GetCurrentVendorAsync()).Id)
                return RedirectToAction("List");

            if ((await _productService.GetProductPicturesByProductIdAsync(productId)).Any(p => p.PictureId == pictureId))
                return Json(new { Result = false });

            //try to get a picture with the specified id
            var picture = await _pictureService.GetPictureByIdAsync(pictureId)
                ?? throw new ArgumentException("No picture found with the specified id");

            await _pictureService.UpdatePictureAsync(picture.Id,
                await _pictureService.LoadPictureBinaryAsync(picture),
                picture.MimeType,
                picture.SeoFilename,
                overrideAltAttribute,
                overrideTitleAttribute);

            await _pictureService.SetSeoFilenameAsync(pictureId, await _pictureService.GetPictureSeNameAsync(product.Name));

            var productPicture = new ProductPicture
            {
                PictureId = pictureId,
                ProductId = productId,
                DisplayOrder = displayOrder,
                DisplayOnListingModules = displayOnListingModules,
                HideOnProductPage = hideOnProductPage,
                DisplayOnCategoryPage = displayOnCategoryPage,
                IsDimensionImage = isDimensionImage
            };
            await _productService.InsertProductPictureAsync(productPicture);

            #region Product Picture Log




            //if (logPictureIds.Count() > 1)
            //{
            //    logPictureIds = logPictureIds.Skip(logPictureIds.Count() - 1).ToList();

            //    #region Delete Old Logs Except Latest one

            //    foreach (var logPictureId in logPictureIds)
            //    {
            //        var logPicture = await _pictureService.GetPictureLogById(logPictureId);
            //        if (logPicture != null)
            //        {
            //            await _pictureService.DeletePictureImagesAsync(new Core.Domain.Media.Picture()
            //            {
            //                AltAttribute = logPicture.AltAttribute,
            //                Id = logPicture.ReferenceId,
            //                IsNew = logPicture.IsNew,
            //                MimeType = logPicture.MimeType,
            //                SeoFilename = logPicture.SeoFilename,
            //                TitleAttribute = logPicture.TitleAttribute,
            //                VirtualPath = logPicture.VirtualPath
            //            });
            //        }

            //        var pictureMappingLogsByPictureId= await 

            //        foreach (var pictureMappingLog in pictureMappingLogs.Where(pl => pl.PictureId == logPictureId))
            //        {
            //            await _pictureService.DeletePictureMappingLog(pictureMappingLog);
            //        }
            //        if (logPicture != null)
            //            await _pictureService.DeletePictureLog(logPicture);
            //    }
            //    #endregion
            //}

            var pictureLog = new Core.Domain.Media.LogPicture()
            {
                AltAttribute = picture.AltAttribute,
                IsNew = picture.IsNew,
                MimeType = picture.MimeType,
                ReferenceId = picture.Id,
                SeoFilename = picture.SeoFilename,
                TitleAttribute = picture.TitleAttribute,
                VirtualPath = picture.VirtualPath
            };
            await _pictureService.InsertPictureLog(pictureLog);

            await _pictureService.InsertPictureMappingLog(new LogProductPicture()
            {
                CreatedOn = DateTime.Now,
                CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                Action = "Add",
                ReferenceId = productPicture.Id,
                DisplayOnCategoryPage = displayOnCategoryPage,
                DisplayOnListingModules = displayOnListingModules,
                HideOnProductPage = hideOnProductPage,
                ProductId = productId,
                PictureId = pictureLog.Id,
                DisplayOrder = displayOrder
            });


            #endregion


            return Json(new { Result = true });
        }
        public virtual async Task<IActionResult> CustomProductPictureUpdate(ProductPictureModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a product picture with the specified id
            var productPicture = await _productService.GetProductPictureByIdAsync(model.Id)
                ?? throw new ArgumentException("No product picture found with the specified id");

            //a vendor should have access only to his products
            if (await _workContext.GetCurrentVendorAsync() != null)
            {
                var product = await _productService.GetProductByIdAsync(productPicture.ProductId);
                if (product != null && product.VendorId != (await _workContext.GetCurrentVendorAsync()).Id)
                    return Content("This is not your product");
            }

            //try to get a picture with the specified id
            var picture = await _pictureService.GetPictureByIdAsync(productPicture.PictureId)
                ?? throw new ArgumentException("No picture found with the specified id");



            (string fullSizeImageUrl, _) = await _pictureService.GetPictureUrlAsync(picture);
            if (!string.IsNullOrEmpty(fullSizeImageUrl))
            {
                var _cloudflareSettings = EngineContext.Current.Resolve<CloudflareSettings>();
                List<string> images = new List<string>();
                images.Add(fullSizeImageUrl);

                foreach (var size in _cloudflareSettings.ImageSizes.Split(','))
                {
                    int.TryParse(size, out int imageSize);
                    if (imageSize > 0)
                    {
                        (string imageUrl, _) = await _pictureService.GetPictureUrlAsync(picture, imageSize);
                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            images.Add(imageUrl);
                        }
                    }
                }
                var _cloudflareService = EngineContext.Current.Resolve<ICloudflareService>();

                await _cloudflareService.ClearCacheOfFiles(images);
            }

            await _pictureService.UpdatePictureAsync(picture.Id,
                await _pictureService.LoadPictureBinaryAsync(picture),
                picture.MimeType,
                picture.SeoFilename,
                model.OverrideAltAttribute,
                model.OverrideTitleAttribute);

            productPicture.DisplayOrder = model.DisplayOrder;
            productPicture.DisplayOnCategoryPage = model.DisplayOnCategoryPage;
            productPicture.HideOnProductPage = model.HideOnProductPage;
            productPicture.DisplayOnListingModules = model.DisplayOnListingModules;
            productPicture.IsDimensionImage = model.IsDimensionImage;
            await _productService.UpdateProductPictureAsync(productPicture);



            #region Product Picture Log

            var pictureLog = (await _pictureService.GetPictureLogsByRefrenceId(picture.Id)).FirstOrDefault();

            if (pictureLog != null)
            {
                await _pictureService.InsertPictureMappingLog(new LogProductPicture()
                {
                    CreatedOn = DateTime.Now,
                    CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                    Action = "Update",
                    ReferenceId = productPicture.Id,
                    DisplayOnCategoryPage = model.DisplayOnCategoryPage,
                    DisplayOnListingModules = model.DisplayOnListingModules,
                    HideOnProductPage = model.HideOnProductPage,
                    ProductId = productPicture.ProductId,
                    PictureId = pictureLog.Id,
                    DisplayOrder = model.DisplayOrder
                });

            }
            else
            {
                pictureLog = new Core.Domain.Media.LogPicture()
                {
                    AltAttribute = picture.AltAttribute,
                    IsNew = picture.IsNew,
                    MimeType = picture.MimeType,
                    ReferenceId = picture.Id,
                    SeoFilename = picture.SeoFilename,
                    TitleAttribute = picture.TitleAttribute,
                    VirtualPath = picture.VirtualPath
                };
                await _pictureService.InsertPictureLog(pictureLog);

                await _pictureService.InsertPictureMappingLog(new LogProductPicture()
                {
                    CreatedOn = DateTime.Now,
                    CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                    Action = "Update",
                    ReferenceId = productPicture.Id,
                    DisplayOnCategoryPage = model.DisplayOnCategoryPage,
                    DisplayOnListingModules = model.DisplayOnListingModules,
                    HideOnProductPage = model.HideOnProductPage,
                    ProductId = productPicture.ProductId,
                    PictureId = pictureLog.Id,
                    DisplayOrder = model.DisplayOrder
                });
            }


            #endregion

            return View("_CreateOrUpdate.Pictures", model);
        }


        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> CustomProductPictureDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a product picture with the specified id
            var productPicture = await _productService.GetProductPictureByIdAsync(id)
                ?? throw new ArgumentException("No product picture found with the specified id");

            //a vendor should have access only to his products
            if (await _workContext.GetCurrentVendorAsync() != null)
            {
                var product = await _productService.GetProductByIdAsync(productPicture.ProductId);
                if (product != null && product.VendorId != (await _workContext.GetCurrentVendorAsync()).Id)
                    return Content("This is not your product");
            }

            var pictureId = productPicture.PictureId;
            await _productService.DeleteProductPictureAsync(productPicture);

            //try to get a picture with the specified id
            var picture = await _pictureService.GetPictureByIdAsync(pictureId)
                ?? throw new ArgumentException("No picture found with the specified id");

            await _pictureService.DeletePictureExcludeImagesAsync(picture);

            #region Product Picture Log

            var pictureLog = (await _pictureService.GetPictureLogsByRefrenceId(picture.Id)).FirstOrDefault();

            //if (pictureLog != null)
            //{
            //    var pictureMappingLogs = await _pictureService.GetPictureMappingLogs(productPicture.ProductId, productPicture.DisplayOrder);
            //    var logPictureIds = pictureMappingLogs.Select(pl => pl.PictureId).Distinct().OrderBy(p => p).ToList();
            //    if (logPictureIds.Count() > 1)
            //    {
            //        logPictureIds = logPictureIds.Skip(logPictureIds.Count() - 1).ToList();

            //        #region Delete Old Logs Except Latest one

            //        foreach (var logPictureId in logPictureIds)
            //        {
            //            var logPicture = await _pictureService.GetPictureLogById(logPictureId);
            //            if (logPicture != null)
            //            {
            //                await _pictureService.DeletePictureImagesAsync(new Core.Domain.Media.Picture()
            //                {
            //                    AltAttribute = logPicture.AltAttribute,
            //                    Id = logPicture.ReferenceId,
            //                    IsNew = logPicture.IsNew,
            //                    MimeType = logPicture.MimeType,
            //                    SeoFilename = logPicture.SeoFilename,
            //                    TitleAttribute = logPicture.TitleAttribute,
            //                    VirtualPath = logPicture.VirtualPath
            //                });
            //            }

            //            foreach (var pictureMappingLog in pictureMappingLogs.Where(pl => pl.PictureId == logPictureId))
            //            {
            //                await _pictureService.DeletePictureMappingLog(pictureMappingLog);
            //            }
            //            if (logPicture != null)
            //                await _pictureService.DeletePictureLog(logPicture);
            //        }
            //    }
            //    #endregion

            //}
            //else
            //{
            if (pictureLog == null)
            {
                pictureLog = new Core.Domain.Media.LogPicture()
                {
                    AltAttribute = picture.AltAttribute,
                    IsNew = picture.IsNew,
                    MimeType = picture.MimeType,
                    ReferenceId = picture.Id,
                    SeoFilename = picture.SeoFilename,
                    TitleAttribute = picture.TitleAttribute,
                    VirtualPath = picture.VirtualPath
                };
                await _pictureService.InsertPictureLog(pictureLog);
            }
            //  }

            await _pictureService.InsertPictureMappingLog(new LogProductPicture()
            {
                CreatedOn = DateTime.Now,
                CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                Action = "Delete",
                ReferenceId = productPicture.Id,
                DisplayOnCategoryPage = productPicture.DisplayOnCategoryPage,
                DisplayOnListingModules = productPicture.DisplayOnListingModules,
                HideOnProductPage = productPicture.HideOnProductPage,
                ProductId = productPicture.ProductId,
                PictureId = pictureLog.Id,
                DisplayOrder = productPicture.DisplayOrder
            });


            #endregion

            return new NullJsonResult();
        }
        [HttpPost]

        public virtual async Task<IActionResult> ProductPictureLogList(LogProductPictureSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return await AccessDeniedDataTablesJson();

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(searchModel.ProductId)
                ?? throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (await _workContext.GetCurrentVendorAsync() != null && product.VendorId != (await _workContext.GetCurrentVendorAsync()).Id)
                return Content("This is not your product");

            //prepare model
            var model = await _productModelFactory.PrepareProductPictureListModelAsync(searchModel, product);

            return Json(model);
        }

        public virtual async Task<IActionResult> PictureHistory(int productId)
        {
            return View(new LogProductPictureSearchModel()
            {
                ProductId = productId
            });
        }
        public virtual async Task<IActionResult> ProductPictureAddUpdatePopup(ProductPictureModel? model)
        {
            if (model.Id != 0)
            {
                //try to get a product picture with the specified id
                var productPicture = await _productService.GetProductPictureByIdAsync(model.Id)
                    ?? throw new ArgumentException("No product picture found with the specified id");

                //try to get a picture with the specified id
                var picture = await _pictureService.GetPictureByIdAsync(productPicture.PictureId)
                    ?? throw new ArgumentException("No picture found with the specified id");
                model.Id = productPicture.Id;
                model.PictureId = productPicture.PictureId;
                model.ProductId = productPicture.ProductId;
                model.DisplayOrder = productPicture.DisplayOrder;
                model.DisplayOnListingModules = productPicture.DisplayOnListingModules ?? false;
                model.HideOnProductPage = productPicture.HideOnProductPage ?? false;
                model.DisplayOnCategoryPage = productPicture.DisplayOnCategoryPage ?? false;
                model.OverrideAltAttribute = picture.AltAttribute;
                model.OverrideTitleAttribute = picture.TitleAttribute;
                model.PictureUrl = picture.VirtualPath;
                model.IsDimensionImage = productPicture.IsDimensionImage;

                return View("_CreateOrUpdate.Pictures", model);
            }
            else
            {
                return View("_CreateOrUpdate.Pictures", model);
            }
        }
        [HttpPost]
        public virtual async Task<IActionResult> CustomProductPictureAdd(int pictureId, int displayOrder,
string overrideAltAttribute, string overrideTitleAttribute, int productId, bool displayOnListingModules = false, bool hideOnProductPage = false, bool displayOnCategoryPage = false, int id = 0, string? pictureUrl = "", bool isDimensionImage = false)
        {
            if (id != 0)
            {
                ProductPictureModel model = new ProductPictureModel()
                {
                    Id = id,
                    PictureId = pictureId,
                    DisplayOrder = displayOrder,
                    OverrideAltAttribute = overrideAltAttribute,
                    OverrideTitleAttribute = overrideTitleAttribute,
                    ProductId = productId,
                    DisplayOnListingModules = displayOnListingModules,
                    HideOnProductPage = hideOnProductPage,
                    IsDimensionImage = isDimensionImage,
                    DisplayOnCategoryPage = displayOnCategoryPage,
                    PictureUrl = pictureUrl ?? "",
                };
                await CustomProductPictureUpdate(model);
            }
            else
            {
                if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                    return AccessDeniedView();

                if (pictureId == 0)
                    throw new ArgumentException();

                //try to get a product with the specified id
                var product = await _productService.GetProductByIdAsync(productId)
                    ?? throw new ArgumentException("No product found with the specified id");

                //a vendor should have access only to his products
                if (await _workContext.GetCurrentVendorAsync() != null && product.VendorId != (await _workContext.GetCurrentVendorAsync()).Id)
                    return RedirectToAction("List");

                if ((await _productService.GetProductPicturesByProductIdAsync(productId)).Any(p => p.PictureId == pictureId))
                    return Json(new { Result = false });

                //try to get a picture with the specified id
                var picture = await _pictureService.GetPictureByIdAsync(pictureId)
                    ?? throw new ArgumentException("No picture found with the specified id");

                await _pictureService.UpdatePictureAsync(picture.Id,
                    await _pictureService.LoadPictureBinaryAsync(picture),
                    picture.MimeType,
                    picture.SeoFilename,
                    overrideAltAttribute,
                    overrideTitleAttribute);

                await _pictureService.SetSeoFilenameAsync(pictureId, await _pictureService.GetPictureSeNameAsync(product.Name));

                var productPicture = new ProductPicture
                {
                    PictureId = pictureId,
                    ProductId = productId,
                    DisplayOrder = displayOrder,
                    DisplayOnListingModules = displayOnListingModules,
                    HideOnProductPage = hideOnProductPage,
                    DisplayOnCategoryPage = displayOnCategoryPage,
                    IsDimensionImage = isDimensionImage
                };
                await _productService.InsertProductPictureAsync(productPicture);

                #region Product Picture Log

                var pictureLog = new Core.Domain.Media.LogPicture()
                {
                    AltAttribute = picture.AltAttribute,
                    IsNew = picture.IsNew,
                    MimeType = picture.MimeType,
                    ReferenceId = picture.Id,
                    SeoFilename = picture.SeoFilename,
                    TitleAttribute = picture.TitleAttribute,
                    VirtualPath = picture.VirtualPath
                };
                await _pictureService.InsertPictureLog(pictureLog);

                await _pictureService.InsertPictureMappingLog(new LogProductPicture()
                {
                    CreatedOn = DateTime.Now,
                    CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                    Action = "Add",
                    ReferenceId = productPicture.Id,
                    DisplayOnCategoryPage = displayOnCategoryPage,
                    DisplayOnListingModules = displayOnListingModules,
                    HideOnProductPage = hideOnProductPage,
                    ProductId = productId,
                    PictureId = pictureLog.Id,
                    DisplayOrder = displayOrder
                });
            }
            #endregion
            ViewBag.RefreshPage = true;
            return View("_CreateOrUpdate.Pictures", new ProductPictureModel());
        }

        #endregion


    #endregion

    #endregion

}