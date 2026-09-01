using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Services.Catalog;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.ArtificialIntelligence;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Customization.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.FilterLevels;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
using Nop.Core.Http;
using Nop.Core.Infrastructure;
using Nop.Services.ArtificialIntelligence;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.ExportImport;
using Nop.Services.FilterLevels;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Translation;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Validators;
using Nop.Web.Infrastructure.Cache;
using System.Globalization;
using System.Text;

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
                //tokens.Add(new Token("Product.RelatedProducts",
                //    await RenderViewComponentToStringAsync("Custom_Upsell_Mailer", new { productId = product.Id, noOfRelatedProducts = 4 }), true));
                //tokens.Add(new Token("Product.info.Mailer",
                // await RenderViewComponentToStringAsync("Custom_Product_Info_Mailer", new { productId = product.Id, moduleType = "InStock" }), true));
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
            var newProduct = await _customCopyProductService.CustomCopyProductAsync(originalProduct, copyModel.Name,copyModel.Sku, copyModel.Published, copyModel.CopyMultimedia);

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

  

    #endregion

    //#region Required products

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
    //public virtual async Task<IActionResult> LoadProductFriendlyNames(string productIds)
    //{
    //    var result = string.Empty;

    //    if (string.IsNullOrWhiteSpace(productIds))
    //        return Json(new { Text = result });

    //    var ids = new List<int>();
    //    var rangeArray = productIds
    //        .Split(_separator, StringSplitOptions.RemoveEmptyEntries)
    //        .Select(x => x.Trim())
    //        .ToList();

    //    foreach (var str1 in rangeArray)
    //    {
    //        if (int.TryParse(str1, out var tmp1))
    //            ids.Add(tmp1);
    //    }

    //    var products = await _productService.GetProductsByIdsAsync(ids.ToArray());
    //    for (var i = 0; i <= products.Count - 1; i++)
    //    {
    //        result += products[i].Name;
    //        if (i != products.Count - 1)
    //            result += ", ";
    //    }

    //    return Json(new { Text = result });
    //}

    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> RequiredProductAddPopup()
    //{
    //    //prepare model
    //    var model = await _productModelFactory.PrepareAddRequiredProductSearchModelAsync(new AddRequiredProductSearchModel());

    //    return View(model);
    //}

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> RequiredProductAddPopupList(AddRequiredProductSearchModel searchModel)
    //{
    //    //prepare model
    //    var model = await _productModelFactory.PrepareAddRequiredProductListModelAsync(searchModel);

    //    return Json(model);
    //}

    //#endregion

    //#region Related products

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
    //public virtual async Task<IActionResult> RelatedProductList(RelatedProductSearchModel searchModel)
    //{
    //    //try to get a product with the specified id
    //    var product = await _productService.GetProductByIdAsync(searchModel.ProductId)
    //        ?? throw new ArgumentException("No product found with the specified id");

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null && product.VendorId != currentVendor.Id)
    //        return Content("This is not your product");

    //    //prepare model
    //    var model = await _productModelFactory.PrepareRelatedProductListModelAsync(searchModel, product);

    //    return Json(model);
    //}

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> RelatedProductUpdate(RelatedProductModel model)
    //{
    //    //try to get a related product with the specified id
    //    var relatedProduct = await _productService.GetRelatedProductByIdAsync(model.Id)
    //        ?? throw new ArgumentException("No related product found with the specified id");

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null)
    //    {
    //        var product = await _productService.GetProductByIdAsync(relatedProduct.ProductId1);
    //        if (product != null && product.VendorId != currentVendor.Id)
    //            return Content("This is not your product");
    //    }

    //    relatedProduct.DisplayOrder = model.DisplayOrder;
    //    await _productService.UpdateRelatedProductAsync(relatedProduct);

    //    return new NullJsonResult();
    //}

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> RelatedProductDelete(int id)
    //{
    //    //try to get a related product with the specified id
    //    var relatedProduct = await _productService.GetRelatedProductByIdAsync(id)
    //        ?? throw new ArgumentException("No related product found with the specified id");

    //    var productId = relatedProduct.ProductId1;

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null)
    //    {
    //        var product = await _productService.GetProductByIdAsync(productId);
    //        if (product != null && product.VendorId != currentVendor.Id)
    //            return Content("This is not your product");
    //    }

    //    await _productService.DeleteRelatedProductAsync(relatedProduct);

    //    return new NullJsonResult();
    //}

    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> RelatedProductAddPopup(int productId)
    //{
    //    //prepare model
    //    var model = await _productModelFactory.PrepareAddRelatedProductSearchModelAsync(new AddRelatedProductSearchModel());

    //    return View(model);
    //}

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> RelatedProductAddPopupList(AddRelatedProductSearchModel searchModel)
    //{
    //    //prepare model
    //    var model = await _productModelFactory.PrepareAddRelatedProductListModelAsync(searchModel);

    //    return Json(model);
    //}

    //[HttpPost]
    //[FormValueRequired("save")]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> RelatedProductAddPopup(AddRelatedProductModel model)
    //{
    //    var selectedProducts = await _productService.GetProductsByIdsAsync(model.SelectedProductIds.ToArray());
    //    if (selectedProducts.Any())
    //    {
    //        var existingRelatedProducts = await _productService.GetRelatedProductsByProductId1Async(model.ProductId, showHidden: true);
    //        var currentVendor = await _workContext.GetCurrentVendorAsync();
    //        foreach (var product in selectedProducts)
    //        {
    //            //a vendor should have access only to his products
    //            if (currentVendor != null && product.VendorId != currentVendor.Id)
    //                continue;

    //            if (_productService.FindRelatedProduct(existingRelatedProducts, model.ProductId, product.Id) != null)
    //                continue;

    //            await _productService.InsertRelatedProductAsync(new RelatedProduct
    //            {
    //                ProductId1 = model.ProductId,
    //                ProductId2 = product.Id,
    //                DisplayOrder = 1
    //            });
    //        }
    //    }

    //    ViewBag.RefreshPage = true;

    //    return View(new AddRelatedProductSearchModel());
    //}

    //#endregion

    //#region Cross-sell products

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
    //public virtual async Task<IActionResult> CrossSellProductList(CrossSellProductSearchModel searchModel)
    //{
    //    //try to get a product with the specified id
    //    var product = await _productService.GetProductByIdAsync(searchModel.ProductId)
    //        ?? throw new ArgumentException("No product found with the specified id");

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null && product.VendorId != currentVendor.Id)
    //        return Content("This is not your product");

    //    //prepare model
    //    var model = await _productModelFactory.PrepareCrossSellProductListModelAsync(searchModel, product);

    //    return Json(model);
    //}

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> CrossSellProductDelete(int id)
    //{
    //    //try to get a cross-sell product with the specified id
    //    var crossSellProduct = await _productService.GetCrossSellProductByIdAsync(id)
    //        ?? throw new ArgumentException("No cross-sell product found with the specified id");

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null)
    //    {
    //        var product = await _productService.GetProductByIdAsync(crossSellProduct.ProductId1);
    //        if (product != null && product.VendorId != currentVendor.Id)
    //            return Content("This is not your product");
    //    }

    //    await _productService.DeleteCrossSellProductAsync(crossSellProduct);

    //    return new NullJsonResult();
    //}

    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> CrossSellProductAddPopup(int productId)
    //{
    //    //prepare model
    //    var model = await _productModelFactory.PrepareAddCrossSellProductSearchModelAsync(new AddCrossSellProductSearchModel());

    //    return View(model);
    //}

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> CrossSellProductAddPopupList(AddCrossSellProductSearchModel searchModel)
    //{
    //    //prepare model
    //    var model = await _productModelFactory.PrepareAddCrossSellProductListModelAsync(searchModel);

    //    return Json(model);
    //}

    //[HttpPost]
    //[FormValueRequired("save")]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> CrossSellProductAddPopup(AddCrossSellProductModel model)
    //{
    //    var selectedProducts = await _productService.GetProductsByIdsAsync(model.SelectedProductIds.ToArray());
    //    if (selectedProducts.Any())
    //    {
    //        var existingCrossSellProducts = await _productService.GetCrossSellProductsByProductId1Async(model.ProductId, showHidden: true);
    //        var currentVendor = await _workContext.GetCurrentVendorAsync();
    //        foreach (var product in selectedProducts)
    //        {
    //            //a vendor should have access only to his products
    //            if (currentVendor != null && product.VendorId != currentVendor.Id)
    //                continue;

    //            if (_productService.FindCrossSellProduct(existingCrossSellProducts, model.ProductId, product.Id) != null)
    //                continue;

    //            await _productService.InsertCrossSellProductAsync(new CrossSellProduct
    //            {
    //                ProductId1 = model.ProductId,
    //                ProductId2 = product.Id
    //            });
    //        }
    //    }

    //    ViewBag.RefreshPage = true;

    //    return View(new AddCrossSellProductSearchModel());
    //}

    //#endregion

    //#region Filter level values

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.FILTER_LEVEL_VALUE_VIEW)]
    //public virtual async Task<IActionResult> FilterLevelValueList(FilterLevelValueSearchModel searchModel)
    //{
    //    //try to get a product with the specified id
    //    var product = await _productService.GetProductByIdAsync(searchModel.ProductId)
    //        ?? throw new ArgumentException("No product found with the specified id");

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null && product.VendorId != currentVendor.Id)
    //        return Content("This is not your product");

    //    //prepare model
    //    var model = await _productModelFactory.PrepareFilterLevelValueListModelAsync(searchModel, product);

    //    return Json(model);
    //}

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.FILTER_LEVEL_VALUE_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> FilterLevelValueDelete(int productId, int id)
    //{
    //    //try to get a filter level value mapping with the specified id
    //    var existingProductFilterLevelValues = await _filterLevelValueService.GetFilterLevelValueProductsByFilterLevelValueIdAsync(id);

    //    var filterLevelValueMapping = existingProductFilterLevelValues.FirstOrDefault(pc => pc.ProductId == productId && pc.FilterLevelValueId == id)
    //        ?? throw new ArgumentException("No filter level value mapping found with the specified id");

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null)
    //    {
    //        var product = await _productService.GetProductByIdAsync(filterLevelValueMapping.ProductId);
    //        if (product != null && product.VendorId != currentVendor.Id)
    //            return Content("This is not your product");
    //    }

    //    await _filterLevelValueService.DeleteFilterLevelValueProductAsync(filterLevelValueMapping);

    //    return new NullJsonResult();
    //}

    //[CheckPermission(StandardPermission.Catalog.FILTER_LEVEL_VALUE_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> FilterLevelValuesAddPopup(int productId)
    //{
    //    //prepare model
    //    var model = await _filterLevelValueModelFactory.PrepareFilterLevelValueSearchModelAsync(new FilterLevelValueSearchModel());

    //    return View(model);
    //}

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.FILTER_LEVEL_VALUE_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> FilterLevelValuesAddPopupList(FilterLevelValueSearchModel searchModel)
    //{
    //    //prepare model
    //    var model = await _filterLevelValueModelFactory.PrepareFilterLevelValueListModelAsync(searchModel);

    //    return Json(model);
    //}

    //[HttpPost]
    //[FormValueRequired("save")]
    //[CheckPermission(StandardPermission.Catalog.FILTER_LEVEL_VALUE_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> FilterLevelValuesAddPopup(AddFilterLevelValueModel model)
    //{
    //    var selectedFilterLevelValues = await _filterLevelValueService.GetFilterLevelValuesByIdsAsync(model.SelectedFilterLevelValueIds.ToArray());
    //    if (selectedFilterLevelValues.Any())
    //    {
    //        foreach (var filterLevelValue in selectedFilterLevelValues)
    //        {
    //            //whether product filter level value with such parameters already exists
    //            var existingProductFilterLevelValues = await _filterLevelValueService.GetFilterLevelValueProductsByFilterLevelValueIdAsync(filterLevelValue.Id);
    //            if (existingProductFilterLevelValues.FirstOrDefault(pc => pc.ProductId == model.ProductId && pc.FilterLevelValueId == filterLevelValue.Id) != null)
    //                continue;

    //            await _filterLevelValueService.InsertProductFilterLevelValueAsync(new FilterLevelValueProductMapping
    //            {
    //                FilterLevelValueId = filterLevelValue.Id,
    //                ProductId = model.ProductId
    //            });
    //        }
    //    }

    //    ViewBag.RefreshPage = true;

    //    return View(new FilterLevelValueSearchModel());
    //}

    //#endregion

    //#region Associated products

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
    //public virtual async Task<IActionResult> AssociatedProductList(AssociatedProductSearchModel searchModel)
    //{
    //    //try to get a product with the specified id
    //    var product = await _productService.GetProductByIdAsync(searchModel.ProductId)
    //        ?? throw new ArgumentException("No product found with the specified id");

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null && product.VendorId != currentVendor.Id)
    //        return Content("This is not your product");

    //    //prepare model
    //    var model = await _productModelFactory.PrepareAssociatedProductListModelAsync(searchModel, product);

    //    return Json(model);
    //}

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> AssociatedProductUpdate(AssociatedProductModel model)
    //{
    //    //try to get an associated product with the specified id
    //    var associatedProduct = await _productService.GetProductByIdAsync(model.Id)
    //        ?? throw new ArgumentException("No associated product found with the specified id");

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null && associatedProduct.VendorId != currentVendor.Id)
    //        return Content("This is not your product");

    //    associatedProduct.DisplayOrder = model.DisplayOrder;
    //    await _productService.UpdateProductAsync(associatedProduct);

    //    return new NullJsonResult();
    //}

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> AssociatedProductDelete(int id)
    //{
    //    //try to get an associated product with the specified id
    //    var product = await _productService.GetProductByIdAsync(id)
    //        ?? throw new ArgumentException("No associated product found with the specified id");

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null && product.VendorId != currentVendor.Id)
    //        return Content("This is not your product");

    //    product.ParentGroupedProductId = 0;
    //    await _productService.UpdateProductAsync(product);

    //    return new NullJsonResult();
    //}

    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> AssociatedProductAddPopup(int productId)
    //{
    //    //prepare model
    //    var model = await _productModelFactory.PrepareAddAssociatedProductSearchModelAsync(new AddAssociatedProductSearchModel());

    //    return View(model);
    //}

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> AssociatedProductAddPopupList(AddAssociatedProductSearchModel searchModel)
    //{
    //    //prepare model
    //    var model = await _productModelFactory.PrepareAddAssociatedProductListModelAsync(searchModel);

    //    return Json(model);
    //}

    //[HttpPost]
    //[FormValueRequired("save")]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> AssociatedProductAddPopup(AddAssociatedProductModel model)
    //{
    //    var selectedProducts = await _productService.GetProductsByIdsAsync(model.SelectedProductIds.ToArray());

    //    var tryToAddSelfGroupedProduct = selectedProducts
    //        .Select(p => p.Id)
    //        .Contains(model.ProductId);

    //    if (selectedProducts.Any())
    //    {
    //        foreach (var product in selectedProducts)
    //        {
    //            if (product.Id == model.ProductId)
    //                continue;

    //            //a vendor should have access only to his products
    //            var currentVendor = await _workContext.GetCurrentVendorAsync();
    //            if (currentVendor != null && product.VendorId != currentVendor.Id)
    //                continue;

    //            product.ParentGroupedProductId = model.ProductId;
    //            await _productService.UpdateProductAsync(product);
    //        }
    //    }

    //    if (tryToAddSelfGroupedProduct)
    //    {
    //        _notificationService.WarningNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.AssociatedProducts.TryToAddSelfGroupedProduct"));

    //        var addAssociatedProductSearchModel = await _productModelFactory.PrepareAddAssociatedProductSearchModelAsync(new AddAssociatedProductSearchModel());
    //        //set current product id
    //        addAssociatedProductSearchModel.ProductId = model.ProductId;

    //        ViewBag.RefreshPage = true;

    //        return View(addAssociatedProductSearchModel);
    //    }

    //    ViewBag.RefreshPage = true;

    //    ViewBag.ClosePage = true;

    //    return View(new AddAssociatedProductSearchModel());
    //}

    //#endregion

    //#region Product pictures

    //[HttpPost]
    //[IgnoreAntiforgeryToken]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> ProductPictureAdd(int productId, IFormCollection form)
    //{
    //    if (productId == 0)
    //        throw new ArgumentException();

    //    //try to get a product with the specified id
    //    var product = await _productService.GetProductByIdAsync(productId)
    //        ?? throw new ArgumentException("No product found with the specified id");

    //    var files = form.Files.ToList();
    //    if (!files.Any())
    //        return Json(new { success = false });

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null)
    //    {
    //        if (product.VendorId != currentVendor.Id)
    //            return RedirectToAction("List");

    //        var existingPictures = await _pictureService.GetPicturesByProductIdAsync(product.Id);
    //        if (existingPictures.Count >= _vendorSettings.MaximumProductPicturesNumber)
    //        {
    //            return Json(new
    //            {
    //                success = false,
    //                message = await _localizationService.GetResourceAsync("Admin.Catalog.Products.Multimedia.Pictures.Alert.VendorNumberPicturesLimit"),
    //            });
    //        }
    //    }

    //    try
    //    {
    //        foreach (var file in files)
    //        {
    //            //insert picture
    //            var picture = await _pictureService.InsertPictureAsync(file);

    //            await _pictureService.SetSeoFilenameAsync(picture.Id, await _pictureService.GetPictureSeNameAsync(product.Name));

    //            await _productService.InsertProductPictureAsync(new ProductPicture
    //            {
    //                PictureId = picture.Id,
    //                ProductId = product.Id,
    //                DisplayOrder = 0
    //            });
    //        }
    //    }
    //    catch (Exception exc)
    //    {
    //        return Json(new
    //        {
    //            success = false,
    //            message = $"{await _localizationService.GetResourceAsync("Admin.Catalog.Products.Multimedia.Pictures.Alert.PictureAdd")} {exc.Message}",
    //        });
    //    }

    //    return Json(new { success = true });
    //}

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
    //public virtual async Task<IActionResult> ProductPictureList(ProductPictureSearchModel searchModel)
    //{
    //    //try to get a product with the specified id
    //    var product = await _productService.GetProductByIdAsync(searchModel.ProductId)
    //        ?? throw new ArgumentException("No product found with the specified id");

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null && product.VendorId != currentVendor.Id)
    //        return Content("This is not your product");

    //    //prepare model
    //    var model = await _productModelFactory.PrepareProductPictureListModelAsync(searchModel, product);

    //    return Json(model);
    //}

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> ProductPictureUpdate(ProductPictureModel model)
    //{
    //    //try to get a product picture with the specified id
    //    var productPicture = await _productService.GetProductPictureByIdAsync(model.Id)
    //        ?? throw new ArgumentException("No product picture found with the specified id");

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null)
    //    {
    //        var product = await _productService.GetProductByIdAsync(productPicture.ProductId);
    //        if (product != null && product.VendorId != currentVendor.Id)
    //            return Content("This is not your product");
    //    }

    //    //try to get a picture with the specified id
    //    var picture = await _pictureService.GetPictureByIdAsync(productPicture.PictureId)
    //        ?? throw new ArgumentException("No picture found with the specified id");

    //    await _pictureService.UpdatePictureAsync(picture.Id,
    //        await _pictureService.LoadPictureBinaryAsync(picture),
    //        picture.MimeType,
    //        picture.SeoFilename,
    //        model.OverrideAltAttribute,
    //        model.OverrideTitleAttribute);

    //    productPicture.DisplayOrder = model.DisplayOrder;
    //    await _productService.UpdateProductPictureAsync(productPicture);

    //    return new NullJsonResult();
    //}

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> ProductPictureDelete(int id)
    //{
    //    //try to get a product picture with the specified id
    //    var productPicture = await _productService.GetProductPictureByIdAsync(id)
    //        ?? throw new ArgumentException("No product picture found with the specified id");

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null)
    //    {
    //        var product = await _productService.GetProductByIdAsync(productPicture.ProductId);
    //        if (product != null && product.VendorId != currentVendor.Id)
    //            return Content("This is not your product");
    //    }

    //    var pictureId = productPicture.PictureId;
    //    await _productService.DeleteProductPictureAsync(productPicture);

    //    //try to get a picture with the specified id
    //    var picture = await _pictureService.GetPictureByIdAsync(pictureId)
    //        ?? throw new ArgumentException("No picture found with the specified id");

    //    await _pictureService.DeletePictureAsync(picture);

    //    return new NullJsonResult();
    //}

    //#endregion

    //#region Product videos

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> ProductVideoAdd(int productId, [Validate] ProductVideoModel model)
    //{
    //    if (productId == 0)
    //        throw new ArgumentException();

    //    //try to get a product with the specified id
    //    var product = await _productService.GetProductByIdAsync(productId)
    //        ?? throw new ArgumentException("No product found with the specified id");

    //    if (string.IsNullOrEmpty(model.VideoUrl))
    //        ModelState.AddModelError(string.Empty,
    //            await _localizationService.GetResourceAsync("Admin.Catalog.Products.Multimedia.Videos.Alert.VideoAdd.EmptyUrl"));

    //    if (!ModelState.IsValid)
    //        return ErrorJson(ModelState.SerializeErrors());

    //    var videoUrl = model.VideoUrl.TrimStart('~');

    //    try
    //    {
    //        await PingVideoUrlAsync(videoUrl);
    //    }
    //    catch (Exception exc)
    //    {
    //        return Json(new
    //        {
    //            success = false,
    //            error = $"{await _localizationService.GetResourceAsync("Admin.Catalog.Products.Multimedia.Videos.Alert.VideoAdd")} {exc.Message}",
    //        });
    //    }

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null && product.VendorId != currentVendor.Id)
    //        return RedirectToAction("List");
    //    try
    //    {
    //        var video = new Video
    //        {
    //            VideoUrl = videoUrl
    //        };

    //        //insert video
    //        await _videoService.InsertVideoAsync(video);

    //        await _productService.InsertProductVideoAsync(new ProductVideo
    //        {
    //            VideoId = video.Id,
    //            ProductId = product.Id,
    //            DisplayOrder = model.DisplayOrder
    //        });
    //    }
    //    catch (Exception exc)
    //    {
    //        return Json(new
    //        {
    //            success = false,
    //            error = $"{await _localizationService.GetResourceAsync("Admin.Catalog.Products.Multimedia.Videos.Alert.VideoAdd")} {exc.Message}",
    //        });
    //    }

    //    return Json(new { success = true });
    //}

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
    //public virtual async Task<IActionResult> ProductVideoList(ProductVideoSearchModel searchModel)
    //{
    //    //try to get a product with the specified id
    //    var product = await _productService.GetProductByIdAsync(searchModel.ProductId)
    //        ?? throw new ArgumentException("No product found with the specified id");

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null && product.VendorId != currentVendor.Id)
    //        return Content("This is not your product");

    //    //prepare model
    //    var model = await _productModelFactory.PrepareProductVideoListModelAsync(searchModel, product);

    //    return Json(model);
    //}

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> ProductVideoUpdate([Validate] ProductVideoModel model)
    //{
    //    //try to get a product picture with the specified id
    //    var productVideo = await _productService.GetProductVideoByIdAsync(model.Id)
    //        ?? throw new ArgumentException("No product video found with the specified id");

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null)
    //    {
    //        var product = await _productService.GetProductByIdAsync(productVideo.ProductId);
    //        if (product != null && product.VendorId != currentVendor.Id)
    //            return Content("This is not your product");
    //    }

    //    //try to get a video with the specified id
    //    var video = await _videoService.GetVideoByIdAsync(productVideo.VideoId)
    //        ?? throw new ArgumentException("No video found with the specified id");

    //    var videoUrl = model.VideoUrl.TrimStart('~');

    //    try
    //    {
    //        await PingVideoUrlAsync(videoUrl);
    //    }
    //    catch (Exception exc)
    //    {
    //        return Json(new
    //        {
    //            success = false,
    //            error = $"{await _localizationService.GetResourceAsync("Admin.Catalog.Products.Multimedia.Videos.Alert.VideoUpdate")} {exc.Message}",
    //        });
    //    }

    //    video.VideoUrl = videoUrl;

    //    await _videoService.UpdateVideoAsync(video);

    //    productVideo.DisplayOrder = model.DisplayOrder;
    //    await _productService.UpdateProductVideoAsync(productVideo);

    //    return new NullJsonResult();
    //}

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> ProductVideoDelete(int id)
    //{
    //    //try to get a product video with the specified id
    //    var productVideo = await _productService.GetProductVideoByIdAsync(id)
    //        ?? throw new ArgumentException("No product video found with the specified id");

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null)
    //    {
    //        var product = await _productService.GetProductByIdAsync(productVideo.ProductId);
    //        if (product != null && product.VendorId != currentVendor.Id)
    //            return Content("This is not your product");
    //    }

    //    var videoId = productVideo.VideoId;
    //    await _productService.DeleteProductVideoAsync(productVideo);

    //    //try to get a video with the specified id
    //    var video = await _videoService.GetVideoByIdAsync(videoId)
    //        ?? throw new ArgumentException("No video found with the specified id");

    //    await _videoService.DeleteVideoAsync(video);

    //    return new NullJsonResult();
    //}

    //#endregion

    //#region Product specification attributes

    //[HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> ProductSpecificationAttributeAdd(AddSpecificationAttributeModel model, bool continueEditing)
    //{
    //    var product = await _productService.GetProductByIdAsync(model.ProductId);
    //    if (product == null)
    //    {
    //        _notificationService.ErrorNotification("No product found with the specified id");
    //        return RedirectToAction("List");
    //    }

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null && product.VendorId != currentVendor.Id)
    //    {
    //        return RedirectToAction("List");
    //    }

    //    //we allow filtering only for "Option" attribute type
    //    if (model.AttributeTypeId != (int)SpecificationAttributeType.Option)
    //        model.AllowFiltering = false;

    //    //we don't allow CustomValue for "Option" attribute type
    //    if (model.AttributeTypeId == (int)SpecificationAttributeType.Option)
    //        model.ValueRaw = null;

    //    //store raw html if field allow this
    //    if (model.AttributeTypeId == (int)SpecificationAttributeType.CustomText || model.AttributeTypeId == (int)SpecificationAttributeType.Hyperlink)
    //        model.ValueRaw = model.Value;

    //    var psa = model.ToEntity<ProductSpecificationAttribute>();
    //    psa.CustomValue = model.ValueRaw;
    //    await _specificationAttributeService.InsertProductSpecificationAttributeAsync(psa);

    //    switch (psa.AttributeType)
    //    {
    //        case SpecificationAttributeType.CustomText:
    //            foreach (var localized in model.Locales)
    //            {
    //                await _localizedEntityService.SaveLocalizedValueAsync(psa,
    //                    x => x.CustomValue,
    //                    localized.Value,
    //                    localized.LanguageId);
    //            }

    //            break;
    //        case SpecificationAttributeType.CustomHtmlText:
    //            foreach (var localized in model.Locales)
    //            {
    //                await _localizedEntityService.SaveLocalizedValueAsync(psa,
    //                    x => x.CustomValue,
    //                    localized.ValueRaw,
    //                    localized.LanguageId);
    //            }

    //            break;
    //        case SpecificationAttributeType.Option:
    //            break;
    //        case SpecificationAttributeType.Hyperlink:
    //            break;
    //        default:
    //            throw new ArgumentOutOfRangeException();
    //    }

    //    if (continueEditing)
    //        return RedirectToAction("ProductSpecAttributeAddOrEdit",
    //            new { productId = psa.ProductId, specificationId = psa.Id });

    //    //select an appropriate card
    //    SaveSelectedCardName("product-specification-attributes");
    //    return RedirectToAction("Edit", new { id = model.ProductId });
    //}

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
    //public virtual async Task<IActionResult> ProductSpecAttrList(ProductSpecificationAttributeSearchModel searchModel)
    //{
    //    //try to get a product with the specified id
    //    var product = await _productService.GetProductByIdAsync(searchModel.ProductId)
    //        ?? throw new ArgumentException("No product found with the specified id");

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null && product.VendorId != currentVendor.Id)
    //        return Content("This is not your product");

    //    //prepare model
    //    var model = await _productModelFactory.PrepareProductSpecificationAttributeListModelAsync(searchModel, product);

    //    return Json(model);
    //}

    //[HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> ProductSpecAttrUpdate(AddSpecificationAttributeModel model, bool continueEditing)
    //{
    //    //try to get a product specification attribute with the specified id
    //    var psa = await _specificationAttributeService.GetProductSpecificationAttributeByIdAsync(model.SpecificationId);
    //    if (psa == null)
    //    {
    //        //select an appropriate card
    //        SaveSelectedCardName("product-specification-attributes");
    //        _notificationService.ErrorNotification("No product specification attribute found with the specified id");

    //        return RedirectToAction("Edit", new { id = model.ProductId });
    //    }

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null
    //        && (await _productService.GetProductByIdAsync(psa.ProductId)).VendorId != currentVendor.Id)
    //    {
    //        _notificationService.ErrorNotification("This is not your product");

    //        return RedirectToAction("List");
    //    }

    //    //we allow filtering and change option only for "Option" attribute type
    //    //save localized values for CustomHtmlText and CustomText
    //    switch (model.AttributeTypeId)
    //    {
    //        case (int)SpecificationAttributeType.Option:
    //            psa.AllowFiltering = model.AllowFiltering;
    //            psa.SpecificationAttributeOptionId = model.SpecificationAttributeOptionId;

    //            break;
    //        case (int)SpecificationAttributeType.CustomHtmlText:
    //            psa.CustomValue = model.ValueRaw;
    //            foreach (var localized in model.Locales)
    //            {
    //                await _localizedEntityService.SaveLocalizedValueAsync(psa,
    //                    x => x.CustomValue,
    //                    localized.ValueRaw,
    //                    localized.LanguageId);
    //            }

    //            break;
    //        case (int)SpecificationAttributeType.CustomText:
    //            psa.CustomValue = model.Value;
    //            foreach (var localized in model.Locales)
    //            {
    //                await _localizedEntityService.SaveLocalizedValueAsync(psa,
    //                    x => x.CustomValue,
    //                    localized.Value,
    //                    localized.LanguageId);
    //            }

    //            break;
    //        default:
    //            psa.CustomValue = model.Value;

    //            break;
    //    }

    //    psa.ShowOnProductPage = model.ShowOnProductPage;
    //    psa.DisplayOrder = model.DisplayOrder;
    //    await _specificationAttributeService.UpdateProductSpecificationAttributeAsync(psa);

    //    if (continueEditing)
    //    {
    //        return RedirectToAction("ProductSpecAttributeAddOrEdit",
    //            new { productId = psa.ProductId, specificationId = model.SpecificationId });
    //    }

    //    //select an appropriate card
    //    SaveSelectedCardName("product-specification-attributes");

    //    return RedirectToAction("Edit", new { id = psa.ProductId });
    //}

    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
    //public virtual async Task<IActionResult> ProductSpecAttributeAddOrEdit(int productId, int? specificationId)
    //{
    //    if (!specificationId.HasValue && !await _permissionService.AuthorizeAsync(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE))
    //        return AccessDeniedView();

    //    if (await _productService.GetProductByIdAsync(productId) == null)
    //    {
    //        _notificationService.ErrorNotification("No product found with the specified id");
    //        return RedirectToAction("List");
    //    }

    //    //try to get a product specification attribute with the specified id
    //    try
    //    {
    //        var model = await _productModelFactory.PrepareAddSpecificationAttributeModelAsync(productId, specificationId);
    //        return View(model);
    //    }
    //    catch (Exception ex)
    //    {
    //        await _notificationService.ErrorNotificationAsync(ex);

    //        //select an appropriate card
    //        SaveSelectedCardName("product-specification-attributes");
    //        return RedirectToAction("Edit", new { id = productId });
    //    }
    //}

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> ProductSpecAttrDelete(AddSpecificationAttributeModel model)
    //{
    //    //try to get a product specification attribute with the specified id
    //    var psa = await _specificationAttributeService.GetProductSpecificationAttributeByIdAsync(model.SpecificationId);
    //    if (psa == null)
    //    {
    //        //select an appropriate card
    //        SaveSelectedCardName("product-specification-attributes");
    //        _notificationService.ErrorNotification("No product specification attribute found with the specified id");
    //        return RedirectToAction("Edit", new { id = model.ProductId });
    //    }

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null && (await _productService.GetProductByIdAsync(psa.ProductId)).VendorId != currentVendor.Id)
    //    {
    //        _notificationService.ErrorNotification("This is not your product");
    //        return RedirectToAction("List", new { id = model.ProductId });
    //    }

    //    await _specificationAttributeService.DeleteProductSpecificationAttributeAsync(psa);

    //    //select an appropriate card
    //    SaveSelectedCardName("product-specification-attributes");

    //    return RedirectToAction("Edit", new { id = psa.ProductId });
    //}

    //#endregion

    //#region Product tags

    //[CheckPermission(StandardPermission.Catalog.PRODUCT_TAGS_VIEW)]
    //public virtual async Task<IActionResult> ProductTags()
    //{
    //    //prepare model
    //    var model = await _productModelFactory.PrepareProductTagSearchModelAsync(new ProductTagSearchModel());

    //    return View(model);
    //}

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCT_TAGS_VIEW)]
    //public virtual async Task<IActionResult> ProductTags(ProductTagSearchModel searchModel)
    //{
    //    //prepare model
    //    var model = await _productModelFactory.PrepareProductTagListModelAsync(searchModel);

    //    return Json(model);
    //}

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCT_TAGS_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> ProductTagDelete(int id)
    //{
    //    //try to get a product tag with the specified id
    //    var tag = await _productTagService.GetProductTagByIdAsync(id)
    //        ?? throw new ArgumentException("No product tag found with the specified id");

    //    await _productTagService.DeleteProductTagAsync(tag);

    //    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.ProductTags.Deleted"));

    //    return RedirectToAction("ProductTags");
    //}

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCT_TAGS_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> ProductTagsDelete(ICollection<int> selectedIds)
    //{
    //    if (selectedIds == null || !selectedIds.Any())
    //        return NoContent();

    //    var tags = await _productTagService.GetProductTagsByIdsAsync(selectedIds.ToArray());
    //    await _productTagService.DeleteProductTagsAsync(tags);

    //    return Json(new { Result = true });
    //}

    //[CheckPermission(StandardPermission.Catalog.PRODUCT_TAGS_VIEW)]
    //public virtual async Task<IActionResult> EditProductTag(int id)
    //{
    //    //try to get a product tag with the specified id
    //    var productTag = await _productTagService.GetProductTagByIdAsync(id);
    //    if (productTag == null)
    //        return RedirectToAction("List");

    //    //prepare tag model
    //    var model = await _productModelFactory.PrepareProductTagModelAsync(null, productTag);

    //    return View(model);
    //}

    //[HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    //[CheckPermission(StandardPermission.Catalog.PRODUCT_TAGS_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> EditProductTag(ProductTagModel model, bool continueEditing)
    //{
    //    //try to get a product tag with the specified id
    //    var productTag = await _productTagService.GetProductTagByIdAsync(model.Id);
    //    if (productTag == null)
    //        return RedirectToAction("List");

    //    if (ModelState.IsValid)
    //    {
    //        productTag.Name = model.Name;
    //        productTag.MetaDescription = model.MetaDescription;
    //        productTag.MetaKeywords = model.MetaKeywords;
    //        productTag.MetaTitle = model.MetaTitle;
    //        await _productTagService.UpdateProductTagAsync(productTag);

    //        //locales
    //        await UpdateLocalesAsync(productTag, model);

    //        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.ProductTags.Updated"));

    //        return continueEditing ? RedirectToAction("EditProductTag", new { id = productTag.Id }) : RedirectToAction("ProductTags");
    //    }

    //    //prepare model
    //    model = await _productModelFactory.PrepareProductTagModelAsync(model, productTag, true);

    //    //if we got this far, something failed, redisplay form
    //    return View(model);
    //}

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCT_TAGS_VIEW)]
    //public virtual async Task<IActionResult> TaggedProducts(ProductTagProductSearchModel searchModel)
    //{
    //    //prepare model
    //    var model = await _productModelFactory.PrepareTaggedProductListModelAsync(searchModel);

    //    return Json(model);
    //}

    //#endregion

    //#region Purchased with order

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
    //public virtual async Task<IActionResult> PurchasedWithOrders(ProductOrderSearchModel searchModel)
    //{
    //    //try to get a product with the specified id
    //    var product = await _productService.GetProductByIdAsync(searchModel.ProductId)
    //        ?? throw new ArgumentException("No product found with the specified id");

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null && product.VendorId != currentVendor.Id)
    //        return Content("This is not your product");

    //    //prepare model
    //    var model = await _productModelFactory.PrepareProductOrderListModelAsync(searchModel, product);

    //    return Json(model);
    //}

    //#endregion

    //#region Export / Import

    //[HttpPost, ActionName("DownloadCatalogPDF")]
    //[FormValueRequired("download-catalog-pdf")]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_IMPORT_EXPORT)]
    //public virtual async Task<IActionResult> DownloadCatalogAsPdf(ProductSearchModel model)
    //{
    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null)
    //    {
    //        model.SearchVendorId = currentVendor.Id;
    //    }

    //    var categoryIds = new List<int> { model.SearchCategoryId };
    //    //include subcategories
    //    if (model.SearchIncludeSubCategories && model.SearchCategoryId > 0)
    //        categoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: model.SearchCategoryId, showHidden: true));

    //    //0 - all (according to "ShowHidden" parameter)
    //    //1 - published only
    //    //2 - unpublished only
    //    bool? overridePublished = null;
    //    if (model.SearchPublishedId == 1)
    //        overridePublished = true;
    //    else if (model.SearchPublishedId == 2)
    //        overridePublished = false;

    //    var products = await _productService.SearchProductsAsync(0,
    //        categoryIds: categoryIds,
    //        manufacturerIds: new List<int> { model.SearchManufacturerId },
    //        storeId: model.SearchStoreId,
    //        vendorId: model.SearchVendorId,
    //        warehouseId: model.SearchWarehouseId,
    //        productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
    //        keywords: model.SearchProductName,
    //        showHidden: true,
    //        overridePublished: overridePublished);

    //    try
    //    {
    //        byte[] bytes;
    //        await using (var stream = new MemoryStream())
    //        {
    //            await _pdfService.PrintProductsToPdfAsync(stream, products);
    //            bytes = stream.ToArray();
    //        }

    //        return File(bytes, MimeTypes.ApplicationPdf, "pdfcatalog.pdf");
    //    }
    //    catch (Exception exc)
    //    {
    //        await _notificationService.ErrorNotificationAsync(exc);
    //        return RedirectToAction("List");
    //    }
    //}

    //[HttpPost, ActionName("ExportToXml")]
    //[FormValueRequired("exportxml-all")]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_IMPORT_EXPORT)]
    //public virtual async Task<IActionResult> ExportXmlAll(ProductSearchModel model)
    //{
    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null)
    //    {
    //        model.SearchVendorId = currentVendor.Id;
    //    }

    //    var categoryIds = new List<int> { model.SearchCategoryId };
    //    //include subcategories
    //    if (model.SearchIncludeSubCategories && model.SearchCategoryId > 0)
    //        categoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: model.SearchCategoryId, showHidden: true));

    //    //0 - all (according to "ShowHidden" parameter)
    //    //1 - published only
    //    //2 - unpublished only
    //    bool? overridePublished = null;
    //    if (model.SearchPublishedId == 1)
    //        overridePublished = true;
    //    else if (model.SearchPublishedId == 2)
    //        overridePublished = false;

    //    var products = await _productService.SearchProductsAsync(0,
    //        categoryIds: categoryIds,
    //        manufacturerIds: new List<int> { model.SearchManufacturerId },
    //        storeId: model.SearchStoreId,
    //        vendorId: model.SearchVendorId,
    //        warehouseId: model.SearchWarehouseId,
    //        productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
    //        keywords: model.SearchProductName,
    //        showHidden: true,
    //        overridePublished: overridePublished);

    //    try
    //    {
    //        var xml = await _exportManager.ExportProductsToXmlAsync(products);

    //        return File(Encoding.UTF8.GetBytes(xml), MimeTypes.ApplicationXml, "products.xml");
    //    }
    //    catch (Exception exc)
    //    {
    //        await _notificationService.ErrorNotificationAsync(exc);
    //        return RedirectToAction("List");
    //    }
    //}

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_IMPORT_EXPORT)]
    //public virtual async Task<IActionResult> ExportXmlSelected(string selectedIds)
    //{
    //    var products = new List<Product>();
    //    if (selectedIds != null)
    //    {
    //        var ids = selectedIds
    //            .Split(_separator, StringSplitOptions.RemoveEmptyEntries)
    //            .Select(x => Convert.ToInt32(x))
    //            .ToArray();
    //        products.AddRange(await _productService.GetProductsByIdsAsync(ids));
    //    }
    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null)
    //    {
    //        products = products.Where(p => p.VendorId == currentVendor.Id).ToList();
    //    }

    //    try
    //    {
    //        var xml = await _exportManager.ExportProductsToXmlAsync(products);
    //        return File(Encoding.UTF8.GetBytes(xml), MimeTypes.ApplicationXml, "products.xml");
    //    }
    //    catch (Exception exc)
    //    {
    //        await _notificationService.ErrorNotificationAsync(exc);
    //        return RedirectToAction("List");
    //    }
    //}

    //[HttpPost, ActionName("ExportToExcel")]
    //[FormValueRequired("exportexcel-all")]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_IMPORT_EXPORT)]
    //public virtual async Task<IActionResult> ExportExcelAll(ProductSearchModel model)
    //{
    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null)
    //    {
    //        model.SearchVendorId = currentVendor.Id;
    //    }

    //    var categoryIds = new List<int> { model.SearchCategoryId };
    //    //include subcategories
    //    if (model.SearchIncludeSubCategories && model.SearchCategoryId > 0)
    //        categoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: model.SearchCategoryId, showHidden: true));

    //    //0 - all (according to "ShowHidden" parameter)
    //    //1 - published only
    //    //2 - unpublished only
    //    bool? overridePublished = null;
    //    if (model.SearchPublishedId == 1)
    //        overridePublished = true;
    //    else if (model.SearchPublishedId == 2)
    //        overridePublished = false;

    //    var products = await _productService.SearchProductsAsync(0,
    //        categoryIds: categoryIds,
    //        manufacturerIds: new List<int> { model.SearchManufacturerId },
    //        storeId: model.SearchStoreId,
    //        vendorId: model.SearchVendorId,
    //        warehouseId: model.SearchWarehouseId,
    //        productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
    //        keywords: model.SearchProductName,
    //        showHidden: true,
    //        overridePublished: overridePublished);

    //    try
    //    {
    //        var bytes = await _exportManager.ExportProductsToXlsxAsync(products);

    //        return File(bytes, MimeTypes.TextXlsx, "products.xlsx");
    //    }
    //    catch (Exception exc)
    //    {
    //        await _notificationService.ErrorNotificationAsync(exc);

    //        return RedirectToAction("List");
    //    }
    //}

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_IMPORT_EXPORT)]
    //public virtual async Task<IActionResult> ExportExcelSelected(string selectedIds)
    //{
    //    var products = new List<Product>();
    //    if (selectedIds != null)
    //    {
    //        var ids = selectedIds
    //            .Split(_separator, StringSplitOptions.RemoveEmptyEntries)
    //            .Select(x => Convert.ToInt32(x))
    //            .ToArray();
    //        products.AddRange(await _productService.GetProductsByIdsAsync(ids));
    //    }
    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null)
    //    {
    //        products = products.Where(p => p.VendorId == currentVendor.Id).ToList();
    //    }

    //    try
    //    {
    //        var bytes = await _exportManager.ExportProductsToXlsxAsync(products);

    //        return File(bytes, MimeTypes.TextXlsx, "products.xlsx");
    //    }
    //    catch (Exception exc)
    //    {
    //        await _notificationService.ErrorNotificationAsync(exc);
    //        return RedirectToAction("List");
    //    }
    //}

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_IMPORT_EXPORT)]
    //public virtual async Task<IActionResult> ImportExcel(IFormFile importexcelfile)
    //{
    //    if (await _workContext.GetCurrentVendorAsync() != null && !_vendorSettings.AllowVendorsToImportProducts)
    //        //a vendor can not import products
    //        return AccessDeniedView();

    //    try
    //    {
    //        if (importexcelfile != null && importexcelfile.Length > 0)
    //        {
    //            await _importManager.ImportProductsFromXlsxAsync(importexcelfile.OpenReadStream());
    //        }
    //        else
    //        {
    //            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Common.UploadFile"));

    //            return RedirectToAction("List");
    //        }

    //        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Imported"));

    //        return RedirectToAction("List");
    //    }
    //    catch (Exception exc)
    //    {
    //        await _notificationService.ErrorNotificationAsync(exc);

    //        return RedirectToAction("List");
    //    }
    //}

    //#endregion

    //#region Tier prices

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
    //public virtual async Task<IActionResult> TierPriceList(TierPriceSearchModel searchModel)
    //{
    //    //try to get a product with the specified id
    //    var product = await _productService.GetProductByIdAsync(searchModel.ProductId)
    //        ?? throw new ArgumentException("No product found with the specified id");

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null && product.VendorId != currentVendor.Id)
    //        return Content("This is not your product");

    //    //prepare model
    //    var model = await _productModelFactory.PrepareTierPriceListModelAsync(searchModel, product);

    //    return Json(model);
    //}

    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> TierPriceCreatePopup(int productId)
    //{
    //    //try to get a product with the specified id
    //    var product = await _productService.GetProductByIdAsync(productId)
    //        ?? throw new ArgumentException("No product found with the specified id");

    //    //prepare model
    //    var model = await _productModelFactory.PrepareTierPriceModelAsync(new TierPriceModel(), product, null);

    //    return View(model);
    //}

    //[HttpPost]
    //[FormValueRequired("save")]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> TierPriceCreatePopup(TierPriceModel model)
    //{
    //    //try to get a product with the specified id
    //    var product = await _productService.GetProductByIdAsync(model.ProductId)
    //        ?? throw new ArgumentException("No product found with the specified id");

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null && product.VendorId != currentVendor.Id)
    //        return RedirectToAction("List", "Product");

    //    if (ModelState.IsValid)
    //    {
    //        //fill entity from model
    //        var tierPrice = model.ToEntity<TierPrice>();
    //        tierPrice.ProductId = product.Id;
    //        tierPrice.CustomerRoleId = model.CustomerRoleId > 0 ? model.CustomerRoleId : (int?)null;

    //        await _productService.InsertTierPriceAsync(tierPrice);

    //        ViewBag.RefreshPage = true;

    //        return View(model);
    //    }

    //    //prepare model
    //    model = await _productModelFactory.PrepareTierPriceModelAsync(model, product, null, true);

    //    //if we got this far, something failed, redisplay form
    //    return View(model);
    //}

    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
    //public virtual async Task<IActionResult> TierPriceEditPopup(int id)
    //{
    //    //try to get a tier price with the specified id
    //    var tierPrice = await _productService.GetTierPriceByIdAsync(id);
    //    if (tierPrice == null)
    //        return RedirectToAction("List", "Product");

    //    //try to get a product with the specified id
    //    var product = await _productService.GetProductByIdAsync(tierPrice.ProductId)
    //        ?? throw new ArgumentException("No product found with the specified id");

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null && product.VendorId != currentVendor.Id)
    //        return RedirectToAction("List", "Product");

    //    //prepare model
    //    var model = await _productModelFactory.PrepareTierPriceModelAsync(null, product, tierPrice);

    //    return View(model);
    //}

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> TierPriceEditPopup(TierPriceModel model)
    //{
    //    //try to get a tier price with the specified id
    //    var tierPrice = await _productService.GetTierPriceByIdAsync(model.Id);
    //    if (tierPrice == null)
    //        return RedirectToAction("List", "Product");

    //    //try to get a product with the specified id
    //    var product = await _productService.GetProductByIdAsync(tierPrice.ProductId)
    //        ?? throw new ArgumentException("No product found with the specified id");

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null && product.VendorId != currentVendor.Id)
    //        return RedirectToAction("List", "Product");

    //    if (ModelState.IsValid)
    //    {
    //        //fill entity from model
    //        tierPrice = model.ToEntity(tierPrice);
    //        tierPrice.CustomerRoleId = model.CustomerRoleId > 0 ? model.CustomerRoleId : (int?)null;
    //        await _productService.UpdateTierPriceAsync(tierPrice);

    //        ViewBag.RefreshPage = true;

    //        return View(model);
    //    }

    //    //prepare model
    //    model = await _productModelFactory.PrepareTierPriceModelAsync(model, product, tierPrice, true);

    //    //if we got this far, something failed, redisplay form
    //    return View(model);
    //}

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> TierPriceDelete(int id)
    //{
    //    //try to get a tier price with the specified id
    //    var tierPrice = await _productService.GetTierPriceByIdAsync(id)
    //        ?? throw new ArgumentException("No tier price found with the specified id");

    //    //try to get a product with the specified id
    //    var product = await _productService.GetProductByIdAsync(tierPrice.ProductId)
    //        ?? throw new ArgumentException("No product found with the specified id");

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null && product.VendorId != currentVendor.Id)
    //        return Content("This is not your product");

    //    await _productService.DeleteTierPriceAsync(tierPrice);

    //    return new NullJsonResult();
    //}

    //#endregion

    //#region Product attributes

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
    //public virtual async Task<IActionResult> ProductAttributeMappingList(ProductAttributeMappingSearchModel searchModel)
    //{
    //    //try to get a product with the specified id
    //    var product = await _productService.GetProductByIdAsync(searchModel.ProductId)
    //        ?? throw new ArgumentException("No product found with the specified id");

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null && product.VendorId != currentVendor.Id)
    //        return Content("This is not your product");

    //    //prepare model
    //    var model = await _productModelFactory.PrepareProductAttributeMappingListModelAsync(searchModel, product);

    //    return Json(model);
    //}

    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> ProductAttributeMappingCreate(int productId)
    //{
    //    //try to get a product with the specified id
    //    var product = await _productService.GetProductByIdAsync(productId)
    //        ?? throw new ArgumentException("No product found with the specified id");

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null && product.VendorId != currentVendor.Id)
    //    {
    //        _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("This is not your product"));
    //        return RedirectToAction("List");
    //    }

    //    //prepare model
    //    var model = await _productModelFactory.PrepareProductAttributeMappingModelAsync(new ProductAttributeMappingModel(), product, null);

    //    return View(model);
    //}

    //[HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> ProductAttributeMappingCreate(ProductAttributeMappingModel model, bool continueEditing)
    //{
    //    //try to get a product with the specified id
    //    var product = await _productService.GetProductByIdAsync(model.ProductId)
    //        ?? throw new ArgumentException("No product found with the specified id");

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null && product.VendorId != currentVendor.Id)
    //    {
    //        _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("This is not your product"));
    //        return RedirectToAction("List");
    //    }

    //    //ensure this attribute is not mapped yet
    //    if ((await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id))
    //        .Any(x => x.ProductAttributeId == model.ProductAttributeId))
    //    {
    //        //redisplay form
    //        _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.AlreadyExists"));

    //        model = await _productModelFactory.PrepareProductAttributeMappingModelAsync(model, product, null, true);

    //        return View(model);
    //    }

    //    //insert mapping
    //    var productAttributeMapping = model.ToEntity<ProductAttributeMapping>();

    //    await _productAttributeService.InsertProductAttributeMappingAsync(productAttributeMapping);
    //    await UpdateLocalesAsync(productAttributeMapping, model);

    //    //predefined values
    //    var predefinedValues = await _productAttributeService.GetPredefinedProductAttributeValuesAsync(model.ProductAttributeId);
    //    foreach (var predefinedValue in predefinedValues)
    //    {
    //        var pav = new ProductAttributeValue
    //        {
    //            ProductAttributeMappingId = productAttributeMapping.Id,
    //            AttributeValueType = AttributeValueType.Simple,
    //            Name = predefinedValue.Name,
    //            PriceAdjustment = predefinedValue.PriceAdjustment,
    //            PriceAdjustmentUsePercentage = predefinedValue.PriceAdjustmentUsePercentage,
    //            WeightAdjustment = predefinedValue.WeightAdjustment,
    //            Cost = predefinedValue.Cost,
    //            IsPreSelected = predefinedValue.IsPreSelected,
    //            DisplayOrder = predefinedValue.DisplayOrder
    //        };
    //        await _productAttributeService.InsertProductAttributeValueAsync(pav);

    //        //locales
    //        var languages = await _languageService.GetAllLanguagesAsync(true);

    //        //localization
    //        foreach (var lang in languages)
    //        {
    //            var name = await _localizationService.GetLocalizedAsync(predefinedValue, x => x.Name, lang.Id, false, false);
    //            if (!string.IsNullOrEmpty(name))
    //                await _localizedEntityService.SaveLocalizedValueAsync(pav, x => x.Name, name, lang.Id);
    //        }
    //    }

    //    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.Added"));

    //    if (!continueEditing)
    //    {
    //        //select an appropriate card
    //        SaveSelectedCardName("product-product-attributes");
    //        return RedirectToAction("Edit", new { id = product.Id });
    //    }

    //    return RedirectToAction("ProductAttributeMappingEdit", new { id = productAttributeMapping.Id });
    //}

    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
    //public virtual async Task<IActionResult> ProductAttributeMappingEdit(int id)
    //{
    //    //try to get a product attribute mapping with the specified id
    //    var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(id)
    //        ?? throw new ArgumentException("No product attribute mapping found with the specified id");

    //    //try to get a product with the specified id
    //    var product = await _productService.GetProductByIdAsync(productAttributeMapping.ProductId)
    //        ?? throw new ArgumentException("No product found with the specified id");

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null && product.VendorId != currentVendor.Id)
    //    {
    //        _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("This is not your product"));
    //        return RedirectToAction("List");
    //    }

    //    //prepare model
    //    var model = await _productModelFactory.PrepareProductAttributeMappingModelAsync(null, product, productAttributeMapping);

    //    return View(model);
    //}

    //[HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> ProductAttributeMappingEdit(ProductAttributeMappingModel model, bool continueEditing, IFormCollection form)
    //{
    //    //try to get a product attribute mapping with the specified id
    //    var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(model.Id)
    //        ?? throw new ArgumentException("No product attribute mapping found with the specified id");

    //    //try to get a product with the specified id
    //    var product = await _productService.GetProductByIdAsync(productAttributeMapping.ProductId)
    //        ?? throw new ArgumentException("No product found with the specified id");

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null && product.VendorId != currentVendor.Id)
    //    {
    //        _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("This is not your product"));
    //        return RedirectToAction("List");
    //    }

    //    //ensure this attribute is not mapped yet
    //    if ((await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id))
    //        .Any(x => x.ProductAttributeId == model.ProductAttributeId && x.Id != productAttributeMapping.Id))
    //    {
    //        //redisplay form
    //        _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.AlreadyExists"));

    //        model = await _productModelFactory.PrepareProductAttributeMappingModelAsync(model, product, productAttributeMapping, true);

    //        return View(model);
    //    }

    //    //fill entity from model
    //    productAttributeMapping = model.ToEntity(productAttributeMapping);
    //    await _productAttributeService.UpdateProductAttributeMappingAsync(productAttributeMapping);

    //    await UpdateLocalesAsync(productAttributeMapping, model);

    //    await SaveConditionAttributesAsync(productAttributeMapping, model.ConditionModel, form);

    //    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.Updated"));

    //    if (!continueEditing)
    //    {
    //        //select an appropriate card
    //        SaveSelectedCardName("product-product-attributes");
    //        return RedirectToAction("Edit", new { id = product.Id });
    //    }

    //    return RedirectToAction("ProductAttributeMappingEdit", new { id = productAttributeMapping.Id });
    //}

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> PreTranslateProductAttribute(int itemId)
    //{
    //    var translationModel = new TranslationModel();

    //    //try to get a product attribute mapping with the specified id
    //    var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(itemId);
    //    if (productAttributeMapping == null)
    //        return Json(translationModel);

    //    var product = await _productService.GetProductByIdAsync(productAttributeMapping.ProductId);

    //    if (product == null)
    //        return Json(translationModel);

    //    var model = await _productModelFactory.PrepareProductAttributeMappingModelAsync(null, product, productAttributeMapping);

    //    translationModel = await _translationModelFactory.PrepareTranslationModelAsync(model, nameof(ProductAttributeMappingModel.TextPrompt));

    //    return Json(translationModel);
    //}

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> ProductAttributeMappingDelete(int id)
    //{
    //    //try to get a product attribute mapping with the specified id
    //    var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(id)
    //        ?? throw new ArgumentException("No product attribute mapping found with the specified id");

    //    //try to get a product with the specified id
    //    var product = await _productService.GetProductByIdAsync(productAttributeMapping.ProductId)
    //        ?? throw new ArgumentException("No product found with the specified id");

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null && product.VendorId != currentVendor.Id)
    //        return Content("This is not your product");

    //    //check if existed combinations contains the specified attribute
    //    var existedCombinations = await _productAttributeService.GetAllProductAttributeCombinationsAsync(product.Id);
    //    if (existedCombinations?.Any() == true)
    //    {
    //        foreach (var combination in existedCombinations)
    //        {
    //            var mappings = await _productAttributeParser
    //                .ParseProductAttributeMappingsAsync(combination.AttributesXml);

    //            if (mappings?.Any(m => m.Id == productAttributeMapping.Id) == true)
    //            {
    //                _notificationService.ErrorNotification(
    //                    string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.AlreadyExistsInCombination"),
    //                        await _productAttributeFormatter.FormatAttributesAsync(product, combination.AttributesXml, await _workContext.GetCurrentCustomerAsync(), await _storeContext.GetCurrentStoreAsync(), ", ")));

    //                return RedirectToAction("ProductAttributeMappingEdit", new { id = productAttributeMapping.Id });
    //            }
    //        }
    //    }

    //    await _productAttributeService.DeleteProductAttributeMappingAsync(productAttributeMapping);

    //    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.Deleted"));

    //    //select an appropriate card
    //    SaveSelectedCardName("product-product-attributes");
    //    return RedirectToAction("Edit", new { id = productAttributeMapping.ProductId });
    //}

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
    //public virtual async Task<IActionResult> ProductAttributeValueList(ProductAttributeValueSearchModel searchModel)
    //{
    //    //try to get a product attribute mapping with the specified id
    //    var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(searchModel.ProductAttributeMappingId)
    //        ?? throw new ArgumentException("No product attribute mapping found with the specified id");

    //    //try to get a product with the specified id
    //    var product = await _productService.GetProductByIdAsync(productAttributeMapping.ProductId)
    //        ?? throw new ArgumentException("No product found with the specified id");

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null && product.VendorId != currentVendor.Id)
    //        return Content("This is not your product");

    //    //prepare model
    //    var model = await _productModelFactory.PrepareProductAttributeValueListModelAsync(searchModel, productAttributeMapping);

    //    return Json(model);
    //}

    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> ProductAttributeValueCreatePopup(int productAttributeMappingId)
    //{
    //    //try to get a product attribute mapping with the specified id
    //    var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(productAttributeMappingId)
    //        ?? throw new ArgumentException("No product attribute mapping found with the specified id");

    //    //try to get a product with the specified id
    //    var product = await _productService.GetProductByIdAsync(productAttributeMapping.ProductId)
    //        ?? throw new ArgumentException("No product found with the specified id");

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null && product.VendorId != currentVendor.Id)
    //        return RedirectToAction("List", "Product");

    //    //prepare model
    //    var model = await _productModelFactory.PrepareProductAttributeValueModelAsync(new ProductAttributeValueModel(), productAttributeMapping, null);

    //    return View(model);
    //}

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> ProductAttributeValueCreatePopup(ProductAttributeValueModel model)
    //{
    //    //try to get a product attribute mapping with the specified id
    //    var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(model.ProductAttributeMappingId);
    //    if (productAttributeMapping == null)
    //        return RedirectToAction("List", "Product");

    //    //try to get a product with the specified id
    //    var product = await _productService.GetProductByIdAsync(productAttributeMapping.ProductId)
    //        ?? throw new ArgumentException("No product found with the specified id");

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null && product.VendorId != currentVendor.Id)
    //        return RedirectToAction("List", "Product");

    //    if (productAttributeMapping.AttributeControlType == AttributeControlType.ColorSquares)
    //    {
    //        //ensure valid color is chosen/entered
    //        if (string.IsNullOrEmpty(model.ColorSquaresRgb))
    //            ModelState.AddModelError(string.Empty, "Color is required");
    //        try
    //        {
    //            //ensure color is valid (can be instantiated)
    //            System.Drawing.ColorTranslator.FromHtml(model.ColorSquaresRgb);
    //        }
    //        catch (Exception exc)
    //        {
    //            ModelState.AddModelError(string.Empty, exc.Message);
    //        }
    //    }

    //    //ensure a picture is uploaded
    //    if (productAttributeMapping.AttributeControlType == AttributeControlType.ImageSquares && model.ImageSquaresPictureId == 0)
    //    {
    //        ModelState.AddModelError(string.Empty, "Image is required");
    //    }

    //    if (ModelState.IsValid)
    //    {
    //        //fill entity from model
    //        var pav = model.ToEntity<ProductAttributeValue>();

    //        pav.Quantity = model.CustomerEntersQty ? 1 : model.Quantity;

    //        await _productAttributeService.InsertProductAttributeValueAsync(pav);
    //        await UpdateLocalesAsync(pav, model);
    //        await SaveAttributeValuePicturesAsync(product, pav, model);

    //        ViewBag.RefreshPage = true;

    //        return View(model);
    //    }

    //    //prepare model
    //    model = await _productModelFactory.PrepareProductAttributeValueModelAsync(model, productAttributeMapping, null, true);

    //    //if we got this far, something failed, redisplay form
    //    return View(model);
    //}

    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
    //public virtual async Task<IActionResult> ProductAttributeValueEditPopup(int id)
    //{
    //    //try to get a product attribute value with the specified id
    //    var productAttributeValue = await _productAttributeService.GetProductAttributeValueByIdAsync(id);
    //    if (productAttributeValue == null)
    //        return RedirectToAction("List", "Product");

    //    //try to get a product attribute mapping with the specified id
    //    var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(productAttributeValue.ProductAttributeMappingId);
    //    if (productAttributeMapping == null)
    //        return RedirectToAction("List", "Product");

    //    //try to get a product with the specified id
    //    var product = await _productService.GetProductByIdAsync(productAttributeMapping.ProductId)
    //        ?? throw new ArgumentException("No product found with the specified id");

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null && product.VendorId != currentVendor.Id)
    //        return RedirectToAction("List", "Product");

    //    //prepare model
    //    var model = await _productModelFactory.PrepareProductAttributeValueModelAsync(null, productAttributeMapping, productAttributeValue);

    //    return View(model);
    //}

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> ProductAttributeValueEditPopup(ProductAttributeValueModel model)
    //{
    //    //try to get a product attribute value with the specified id
    //    var productAttributeValue = await _productAttributeService.GetProductAttributeValueByIdAsync(model.Id);
    //    if (productAttributeValue == null)
    //        return RedirectToAction("List", "Product");

    //    //try to get a product attribute mapping with the specified id
    //    var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(productAttributeValue.ProductAttributeMappingId);
    //    if (productAttributeMapping == null)
    //        return RedirectToAction("List", "Product");

    //    //try to get a product with the specified id
    //    var product = await _productService.GetProductByIdAsync(productAttributeMapping.ProductId)
    //        ?? throw new ArgumentException("No product found with the specified id");

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null && product.VendorId != currentVendor.Id)
    //        return RedirectToAction("List", "Product");

    //    if (productAttributeMapping.AttributeControlType == AttributeControlType.ColorSquares)
    //    {
    //        //ensure valid color is chosen/entered
    //        if (string.IsNullOrEmpty(model.ColorSquaresRgb))
    //            ModelState.AddModelError(string.Empty, "Color is required");
    //        try
    //        {
    //            //ensure color is valid (can be instantiated)
    //            System.Drawing.ColorTranslator.FromHtml(model.ColorSquaresRgb);
    //        }
    //        catch (Exception exc)
    //        {
    //            ModelState.AddModelError(string.Empty, exc.Message);
    //        }
    //    }

    //    //ensure a picture is uploaded
    //    if (productAttributeMapping.AttributeControlType == AttributeControlType.ImageSquares && model.ImageSquaresPictureId == 0)
    //    {
    //        ModelState.AddModelError(string.Empty, "Image is required");
    //    }

    //    if (ModelState.IsValid)
    //    {
    //        //fill entity from model
    //        productAttributeValue = model.ToEntity(productAttributeValue);
    //        productAttributeValue.Quantity = model.CustomerEntersQty ? 1 : model.Quantity;
    //        await _productAttributeService.UpdateProductAttributeValueAsync(productAttributeValue);

    //        await UpdateLocalesAsync(productAttributeValue, model);
    //        await SaveAttributeValuePicturesAsync(product, productAttributeValue, model);

    //        ViewBag.RefreshPage = true;

    //        return View(model);
    //    }

    //    //prepare model
    //    model = await _productModelFactory.PrepareProductAttributeValueModelAsync(model, productAttributeMapping, productAttributeValue, true);

    //    //if we got this far, something failed, redisplay form
    //    return View(model);
    //}

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> ProductAttributeValueDelete(int id)
    //{
    //    //try to get a product attribute value with the specified id
    //    var productAttributeValue = await _productAttributeService.GetProductAttributeValueByIdAsync(id)
    //        ?? throw new ArgumentException("No product attribute value found with the specified id");

    //    //try to get a product attribute mapping with the specified id
    //    var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(productAttributeValue.ProductAttributeMappingId)
    //        ?? throw new ArgumentException("No product attribute mapping found with the specified id");

    //    //try to get a product with the specified id
    //    var product = await _productService.GetProductByIdAsync(productAttributeMapping.ProductId)
    //        ?? throw new ArgumentException("No product found with the specified id");

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null && product.VendorId != currentVendor.Id)
    //        return Content("This is not your product");

    //    //check if existed combinations contains the specified attribute value
    //    var existedCombinations = await _productAttributeService.GetAllProductAttributeCombinationsAsync(product.Id);
    //    if (existedCombinations?.Any() == true)
    //    {
    //        foreach (var combination in existedCombinations)
    //        {
    //            var attributeValues = await _productAttributeParser.ParseProductAttributeValuesAsync(combination.AttributesXml);

    //            if (attributeValues.Where(attribute => attribute.Id == id).Any())
    //            {
    //                return Conflict(string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.Values.AlreadyExistsInCombination"),
    //                    await _productAttributeFormatter.FormatAttributesAsync(product, combination.AttributesXml, await _workContext.GetCurrentCustomerAsync(), await _storeContext.GetCurrentStoreAsync(), ", ")));
    //            }
    //        }
    //    }

    //    await _productAttributeService.DeleteProductAttributeValueAsync(productAttributeValue);

    //    return new NullJsonResult();
    //}

    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> AssociateProductToAttributeValuePopup()
    //{
    //    //prepare model
    //    var model = await _productModelFactory.PrepareAssociateProductToAttributeValueSearchModelAsync(new AssociateProductToAttributeValueSearchModel());

    //    return View(model);
    //}

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> AssociateProductToAttributeValuePopupList(AssociateProductToAttributeValueSearchModel searchModel)
    //{
    //    //prepare model
    //    var model = await _productModelFactory.PrepareAssociateProductToAttributeValueListModelAsync(searchModel);

    //    return Json(model);
    //}

    //[HttpPost]
    //[FormValueRequired("save")]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> AssociateProductToAttributeValuePopup([Bind(Prefix = nameof(AssociateProductToAttributeValueModel))] AssociateProductToAttributeValueModel model)
    //{
    //    //try to get a product with the specified id
    //    var associatedProduct = await _productService.GetProductByIdAsync(model.AssociatedToProductId);
    //    if (associatedProduct == null)
    //        return Content("Cannot load a product");

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null && associatedProduct.VendorId != currentVendor.Id)
    //        return Content("This is not your product");

    //    ViewBag.RefreshPage = true;
    //    ViewBag.productId = associatedProduct.Id;
    //    ViewBag.productName = associatedProduct.Name;

    //    return View(new AssociateProductToAttributeValueSearchModel());
    //}

    ////action displaying notification (warning) to a store owner when associating some product
    //public virtual async Task<IActionResult> AssociatedProductGetWarnings(int productId)
    //{
    //    var associatedProduct = await _productService.GetProductByIdAsync(productId);
    //    if (associatedProduct == null)
    //        return Json(new { Result = string.Empty });

    //    //attributes
    //    if (await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(associatedProduct.Id) is IList<ProductAttributeMapping> mapping && mapping.Any())
    //    {
    //        if (mapping.Any(attribute => attribute.IsRequired))
    //            return Json(new { Result = await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.Values.Fields.AssociatedProduct.HasRequiredAttributes") });

    //        return Json(new { Result = await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.Values.Fields.AssociatedProduct.HasAttributes") });
    //    }

    //    //gift card
    //    if (associatedProduct.IsGiftCard)
    //    {
    //        return Json(new { Result = await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.Values.Fields.AssociatedProduct.GiftCard") });
    //    }

    //    //downloadable product
    //    if (associatedProduct.IsDownload)
    //    {
    //        return Json(new { Result = await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.Values.Fields.AssociatedProduct.Downloadable") });
    //    }

    //    return Json(new { Result = string.Empty });
    //}

    //#endregion

    //#region Product attribute combinations

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
    //public virtual async Task<IActionResult> ProductAttributeCombinationList(ProductAttributeCombinationSearchModel searchModel)
    //{
    //    //try to get a product with the specified id
    //    var product = await _productService.GetProductByIdAsync(searchModel.ProductId)
    //        ?? throw new ArgumentException("No product found with the specified id");

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null && product.VendorId != currentVendor.Id)
    //        return Content("This is not your product");

    //    //prepare model
    //    var model = await _productModelFactory.PrepareProductAttributeCombinationListModelAsync(searchModel, product);

    //    return Json(model);
    //}

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> ProductAttributeCombinationDelete(int id)
    //{
    //    //try to get a combination with the specified id
    //    var combination = await _productAttributeService.GetProductAttributeCombinationByIdAsync(id)
    //        ?? throw new ArgumentException("No product attribute combination found with the specified id");

    //    //try to get a product with the specified id
    //    var product = await _productService.GetProductByIdAsync(combination.ProductId)
    //        ?? throw new ArgumentException("No product found with the specified id");

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null && product.VendorId != currentVendor.Id)
    //        return Content("This is not your product");

    //    await _productAttributeService.DeleteProductAttributeCombinationAsync(combination);

    //    return new NullJsonResult();
    //}

    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> ProductAttributeCombinationCreatePopup(int productId)
    //{
    //    //try to get a product with the specified id
    //    var product = await _productService.GetProductByIdAsync(productId);
    //    if (product == null)
    //        return RedirectToAction("List", "Product");

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null && product.VendorId != currentVendor.Id)
    //        return RedirectToAction("List", "Product");

    //    //prepare model
    //    var model = await _productModelFactory.PrepareProductAttributeCombinationModelAsync(new ProductAttributeCombinationModel(), product, null);

    //    return View(model);
    //}

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //public virtual async Task<IActionResult> ProductAttributeCombinationCreatePopup(int productId, ProductAttributeCombinationModel model, IFormCollection form)
    //{
    //    //try to get a product with the specified id
    //    var product = await _productService.GetProductByIdAsync(productId);
    //    if (product == null)
    //        return RedirectToAction("List", "Product");

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null && product.VendorId != currentVendor.Id)
    //        return RedirectToAction("List", "Product");

    //    //attributes
    //    var warnings = new List<string>();
    //    var attributesXml = await GetAttributesXmlForProductAttributeCombinationAsync(form, warnings, product.Id);

    //    //check whether the attribute value is specified
    //    if (string.IsNullOrEmpty(attributesXml))
    //        warnings.Add(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.AttributeCombinations.Alert.FailedValue"));

    //    warnings.AddRange(await _shoppingCartService.GetShoppingCartItemAttributeWarningsAsync(await _workContext.GetCurrentCustomerAsync(),
    //        ShoppingCartType.ShoppingCart, product, 1, attributesXml, true));

    //    //check whether the same attribute combination already exists
    //    var existingCombination = await _productAttributeParser.FindProductAttributeCombinationAsync(product, attributesXml);
    //    if (existingCombination != null)
    //        warnings.Add(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.AttributeCombinations.AlreadyExists"));

    //    if (!warnings.Any())
    //    {
    //        //save combination
    //        var combination = model.ToEntity<ProductAttributeCombination>();

    //        //fill attributes
    //        combination.AttributesXml = attributesXml;

    //        await _productAttributeService.InsertProductAttributeCombinationAsync(combination);

    //        await SaveAttributeCombinationPicturesAsync(product, combination, model);

    //        //quantity change history
    //        await _productService.AddStockQuantityHistoryEntryAsync(product, combination.StockQuantity, combination.StockQuantity,
    //            message: await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.Combination.Edit"), combinationId: combination.Id);

    //        ViewBag.RefreshPage = true;

    //        return View(model);
    //    }

    //    //prepare model
    //    model = await _productModelFactory.PrepareProductAttributeCombinationModelAsync(model, product, null, true);
    //    model.Warnings = warnings;

    //    //if we got this far, something failed, redisplay form
    //    return View(model);
    //}

    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
    //public virtual async Task<IActionResult> ProductAttributeCombinationGeneratePopup(int productId)
    //{
    //    //try to get a product with the specified id
    //    var product = await _productService.GetProductByIdAsync(productId);
    //    if (product == null)
    //        return RedirectToAction("List", "Product");

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null && product.VendorId != currentVendor.Id)
    //        return RedirectToAction("List", "Product");

    //    //prepare model
    //    var model = await _productModelFactory.PrepareProductAttributeCombinationModelAsync(new ProductAttributeCombinationModel(), product, null);

    //    return View(model);
    //}

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
    //public virtual async Task<IActionResult> ProductAttributeCombinationGeneratePopup(IFormCollection form, ProductAttributeCombinationModel model)
    //{
    //    //try to get a product with the specified id
    //    var product = await _productService.GetProductByIdAsync(model.ProductId);

    //    if (product == null)
    //        return RedirectToAction("List", "Product");

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null && product.VendorId != currentVendor.Id)
    //        return RedirectToAction("List", "Product");

    //    var allowedAttributeIds = form.Keys.Where(key => key.Contains("attribute_value_"))
    //        .Select(key => int.TryParse(form[key], out var id) ? id : 0).Where(id => id > 0).ToList();

    //    var requiredAttributeNames = await (await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id))
    //        .Where(pam => pam.IsRequired)
    //        .Where(pam => !pam.IsNonCombinable())
    //        .WhereAwait(async pam => !(await _productAttributeService.GetProductAttributeValuesAsync(pam.Id)).Any(v => allowedAttributeIds.Any(id => id == v.Id)))
    //        .SelectAwait(async pam => (await _productAttributeService.GetProductAttributeByIdAsync(pam.ProductAttributeId)).Name).ToListAsync();

    //    if (requiredAttributeNames.Any())
    //    {
    //        model = await _productModelFactory.PrepareProductAttributeCombinationModelAsync(model, product, null, true);
    //        var pavModels = model.ProductAttributes.SelectMany(pa => pa.Values)
    //            .Where(v => allowedAttributeIds.Any(id => id == v.Id))
    //            .ToList();
    //        foreach (var pavModel in pavModels)
    //        {
    //            pavModel.Checked = "checked";
    //        }

    //        model.Warnings.Add(string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.AttributeCombinations.SelectRequiredAttributes"), string.Join(", ", requiredAttributeNames)));

    //        return View(model);
    //    }

    //    await GenerateAttributeCombinationsAsync(product, allowedAttributeIds);

    //    ViewBag.RefreshPage = true;

    //    return View(new ProductAttributeCombinationModel());
    //}

    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
    //public virtual async Task<IActionResult> ProductAttributeCombinationEditPopup(int id)
    //{
    //    //try to get a combination with the specified id
    //    var combination = await _productAttributeService.GetProductAttributeCombinationByIdAsync(id);
    //    if (combination == null)
    //        return RedirectToAction("List", "Product");

    //    //try to get a product with the specified id
    //    var product = await _productService.GetProductByIdAsync(combination.ProductId);
    //    if (product == null)
    //        return RedirectToAction("List", "Product");

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null && product.VendorId != currentVendor.Id)
    //        return RedirectToAction("List", "Product");

    //    //prepare model
    //    var model = await _productModelFactory.PrepareProductAttributeCombinationModelAsync(null, product, combination);

    //    return View(model);
    //}

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
    //public virtual async Task<IActionResult> ProductAttributeCombinationEditPopup(ProductAttributeCombinationModel model, IFormCollection form)
    //{
    //    //try to get a combination with the specified id
    //    var combination = await _productAttributeService.GetProductAttributeCombinationByIdAsync(model.Id);
    //    if (combination == null)
    //        return RedirectToAction("List", "Product");

    //    //try to get a product with the specified id
    //    var product = await _productService.GetProductByIdAsync(combination.ProductId);
    //    if (product == null)
    //        return RedirectToAction("List", "Product");

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null && product.VendorId != currentVendor.Id)
    //        return RedirectToAction("List", "Product");

    //    //attributes
    //    var warnings = new List<string>();
    //    var attributesXml = await GetAttributesXmlForProductAttributeCombinationAsync(form, warnings, product.Id);

    //    //check whether the attribute value is specified
    //    if (string.IsNullOrEmpty(attributesXml))
    //        warnings.Add(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.AttributeCombinations.Alert.FailedValue"));

    //    warnings.AddRange(await _shoppingCartService.GetShoppingCartItemAttributeWarningsAsync(await _workContext.GetCurrentCustomerAsync(),
    //        ShoppingCartType.ShoppingCart, product, 1, attributesXml, true));

    //    //check whether the same attribute combination already exists
    //    var existingCombination = await _productAttributeParser.FindProductAttributeCombinationAsync(product, attributesXml);
    //    if (existingCombination != null && existingCombination.Id != model.Id && existingCombination.AttributesXml.Equals(attributesXml))
    //        warnings.Add(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.AttributeCombinations.AlreadyExists"));

    //    if (!warnings.Any() && ModelState.IsValid)
    //    {
    //        var previousStockQuantity = combination.StockQuantity;

    //        //save combination
    //        //fill entity from model
    //        combination = model.ToEntity(combination);
    //        combination.AttributesXml = attributesXml;

    //        await _productAttributeService.UpdateProductAttributeCombinationAsync(combination);

    //        await SaveAttributeCombinationPicturesAsync(product, combination, model);

    //        //quantity change history
    //        await _productService.AddStockQuantityHistoryEntryAsync(product, combination.StockQuantity - previousStockQuantity, combination.StockQuantity,
    //            message: await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.Combination.Edit"), combinationId: combination.Id);

    //        ViewBag.RefreshPage = true;

    //        return View(model);
    //    }

    //    //prepare model
    //    model = await _productModelFactory.PrepareProductAttributeCombinationModelAsync(model, product, combination, true);
    //    model.Warnings = warnings;

    //    //if we got this far, something failed, redisplay form
    //    return View(model);
    //}

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
    //public virtual async Task<IActionResult> GenerateAllAttributeCombinations(int productId)
    //{
    //    //try to get a product with the specified id
    //    var product = await _productService.GetProductByIdAsync(productId)
    //        ?? throw new ArgumentException("No product found with the specified id");

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null && product.VendorId != currentVendor.Id)
    //        return Content("This is not your product");

    //    await GenerateAttributeCombinationsAsync(product);

    //    return Json(new { Success = true });
    //}

    //#endregion

    //#region Product editor settings

    //[HttpPost]
    //[CheckPermission(StandardPermission.Configuration.MANAGE_SETTINGS)]
    //public virtual async Task<IActionResult> SaveProductEditorSettings(ProductModel model, string returnUrl = "")
    //{
    //    //vendors cannot manage these settings
    //    if (await _workContext.GetCurrentVendorAsync() != null)
    //        return RedirectToAction("List");

    //    var productEditorSettings = await _settingService.LoadSettingAsync<ProductEditorSettings>();
    //    productEditorSettings = model.ProductEditorSettingsModel.ToSettings(productEditorSettings);
    //    await _settingService.SaveSettingAsync(productEditorSettings);

    //    //product list
    //    if (string.IsNullOrEmpty(returnUrl))
    //        return RedirectToAction("List");

    //    //prevent open redirection attack
    //    if (!Url.IsLocalUrl(returnUrl))
    //        return RedirectToAction("List");

    //    return Redirect(returnUrl);
    //}

    //#endregion

    //#region Stock quantity history

    //[HttpPost]
    //[CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
    //public virtual async Task<IActionResult> StockQuantityHistory(StockQuantityHistorySearchModel searchModel)
    //{
    //    var product = await _productService.GetProductByIdAsync(searchModel.ProductId)
    //        ?? throw new ArgumentException("No product found with the specified id");

    //    //a vendor should have access only to his products
    //    var currentVendor = await _workContext.GetCurrentVendorAsync();
    //    if (currentVendor != null && product.VendorId != currentVendor.Id)
    //        return Content("This is not your product");

    //    //prepare model
    //    var model = await _productModelFactory.PrepareStockQuantityHistoryListModelAsync(searchModel, product);

    //    return Json(model);
    //}

    //#endregion

    #endregion
     
}