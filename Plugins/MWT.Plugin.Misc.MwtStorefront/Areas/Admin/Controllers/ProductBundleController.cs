
using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Domain.ProductBundle;
using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Services.BundleProduct;
using MWT.Plugin.Misc.MwtStorefront.Area.Admin.BundleLogs;
using MWT.Plugin.Misc.MwtStorefront.Area.Admin.Models;
using MWT.Plugin.Misc.MwtStorefront.Area.Admin.Models.BundleLogs;
using MWT.Plugin.Misc.MwtStorefront.Area.Admin.Models.Variant;
using MWT.Plugin.Misc.MwtStorefront.Area.Admin.Variant;
using MWT.Plugin.Misc.MwtStorefront.Factories;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Web.Factories;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;

namespace MWT.Nop.Plugin.Misc.ProductBundle.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [AutoValidateAntiforgeryToken]
    public class ProductBundleController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IProductExtendedService _productService;
        private readonly IProductBundleModelFactory _bundleService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly ProductBundleSettings _productBundleSettings;
        private readonly ISettingService _settingService;
        private readonly IWorkContext _workContext;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductAttributeFormatter _productformatter;
        private readonly IProductAttributeParser _productParser;
        private readonly INopFileProvider _fileProvider;
        private readonly IBundleLoggerService _bundleLoggerService;
        private readonly ICustomerService _customerService;
        public ProductBundleController(
            IPermissionService permissionService,
            IProductBundleModelFactory bundleService,
            IProductAttributeService productAttributeService,
            ILocalizationService localizationService,
            IProductExtendedService productService,
            IUrlRecordService urlRecordService,
            IPictureService pictureService,
            INotificationService notificationService,
            ProductBundleSettings productBundleSettings,
    ISettingService settingService,
    IWorkContext workContext,
    IProductModelFactory productModelFactory,
    IProductAttributeFormatter productformatter,
    IProductAttributeParser productParser,
        INopFileProvider fileProvider,
        IBundleLoggerService bundleLoggerService,
        ICustomerService customerService

    )
        {
            _permissionService = permissionService;
            _pictureService = pictureService;
            _urlRecordService = urlRecordService;
            _productService = productService;
            _bundleService = bundleService;
            _productAttributeService = productAttributeService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _productBundleSettings = productBundleSettings;
            _settingService = settingService;
            _workContext = workContext;
            _productModelFactory = productModelFactory;
            _productformatter = productformatter;
            _productParser = productParser;
            _fileProvider = fileProvider;
            _bundleLoggerService = bundleLoggerService;
            _customerService = customerService;
        }



        #region VariantsRegion


        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        /// 
        [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> VariantList(VariantBundleSearchModel searchModel)
        { 
            //try to get a product with the specified id

            var product = await _productService.GetProductByIdAsync(searchModel.ProductId)
                ?? throw new ArgumentException("No product found with the specified id");




            var model = await _bundleService.PrepareVariantListModelAsync(searchModel, product);

            return Json(model);
        }

        #endregion


        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
        public async Task<IActionResult> BundleEdit(BundleConfigurationModel model, bool continueEditing)
        { 

            if (!ModelState.IsValid)
                return View("BundleEdit", model);

            var bundle = await _bundleService.GetBundleByIdAsync(model.Id);

            if (bundle == null)
                return RedirectToAction("List", "Product");

            bundle.Name = model.Name;
            bundle.NoOfPieces = model.NoOfPieces;
            bundle.UpdatedOnUtc = DateTime.UtcNow;
            bundle.UpdatedBy = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0;
        
            await _bundleService.UpdateBundleAsync(bundle);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Bundle.Updated"));

            if (!continueEditing)
            {
                SaveSelectedCardName("product-hbc-bundles");
                return RedirectToAction("Edit", "Product", new { id = bundle.ProductId });
            }

            return RedirectToAction("BundleEdit", new { id = bundle.Id });
        }


        public async Task<IActionResult> DisableBundle(int variantId)
        {
            await _bundleService.DisableBundle(variantId);
            return new NullJsonResult();
        }

        [HttpGet]
        public virtual async Task<IActionResult> GetNestedVariants(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            var variants = await _productService.GetProductVariants(productId);

            var model = new List<VariantBundleModel>();

            foreach (var variant in variants)
            {
                bool isPublished = true;
                var attrValueIds = (variant.ProductAttributeValueIds ?? string.Empty)
                    .Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var idStr in attrValueIds)
                {
                    if (int.TryParse(idStr, out int valId))
                    {
                        var attrValue = await _productAttributeService.GetProductAttributeValueByIdAsync(valId);
                        if (attrValue == null || !attrValue.Published)
                        {
                            isPublished = false;
                            break;
                        }
                    }
                }
                if (isPublished)
                {
                    model.Add(new VariantBundleModel
                    {
                        ProductId = productId,
                        VariantId = variant.VariantId,
                        Title = variant.Title ?? product.Name,
                        CreatedOn = variant.CreatedOn,
                        UpdatedOn = variant.UpdatedOn,
                        Attributes = await _productformatter.FormatAttributesAsync(product, variant.Combination)
                    });
                }
            }

            return PartialView("_NestedVariants", model);
        }
        #region Bundle Items List, Delete, Edit 

        [HttpPost]
        [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
        public async Task<IActionResult> BulkEditBundleItemList(BundleItemSearchModel searchModel)
        { 

            // UPDATED: Pass both BundleId and AttributeValueId to filter the correct tab's items
            var bundleItems = await _bundleService.GetBundleItemsAsync(searchModel.BundleId);

            var pagedBundleItems = bundleItems.ToPagedList(searchModel);
            var gridModel = await new BundleItemListModel().PrepareToGridAsync(searchModel, pagedBundleItems, () =>
            {
                return pagedBundleItems.SelectAwait(async x =>
                {
                    var product = await _productService.GetProductByIdAsync(x.ProductId);
                    var bundleItemModel = new BundleItemModel();
                    bundleItemModel.Id = x.Id;
                    bundleItemModel.ProductId = x.ProductId;
                    bundleItemModel.VariantId = x.VariantId;
                    bundleItemModel.BundleId = x.BundleId;
                    bundleItemModel.Quantity = x.Quantity;
                    var vcombination = await _productService.GetVariantByVariantId(x.VariantId);

                    if (product != null && vcombination != null)
                    {
                        bundleItemModel.Attributes = await _productformatter.FormatAttributesAsync(
                            product,
                            vcombination.Combination);
                    }
                    bundleItemModel.VariantId = x.VariantId;

                    var defaultProductPicture = (await _pictureService.GetPicturesByProductIdAsync(x.ProductId, 1)).FirstOrDefault();
                    (bundleItemModel.Picture, _) = await _pictureService.GetPictureUrlAsync(defaultProductPicture, 75);

                    bundleItemModel.ProductName = product?.Name;

                    return bundleItemModel;
                });
            });

            return Json(gridModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditBundleItems(BundleItemModel model)
        {
            var bundle = await _bundleService.GetBundleItemByIdAsync(model.Id);
            if (bundle == null)
                return RedirectToAction("BundleList");

            var bundleDetails = await _bundleService.GetBundleByIdAsync(bundle.BundleId);
            var allBundleItems = await _bundleService.GetBundleItemsAsync(bundle.BundleId);

            int spaceTakenByOtherItems = allBundleItems.Where(x => x.Id != model.Id).Sum(x => x.Quantity);

            int maxAllowedForThisItem = bundleDetails.NoOfPieces - spaceTakenByOtherItems;

            if (model.Quantity > maxAllowedForThisItem)
            {
                return ErrorJson($"This bundle allows {bundleDetails.NoOfPieces} pieces, but you have selected more than allowed.. Please adjust your selection.");
            }
            if (model.Quantity <= 0)
            {
                return ErrorJson("Quantity must be at least 1.If you want to remove it, use the Delete button.");
            }

            // 6. If we get here, the number is 100% valid. Save it!
            bundle.Quantity = model.Quantity;
            bundle.UpdatedBy = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0;
            bundle.UpdatedOnUtc = DateTime.UtcNow;
            await _bundleService.UpdateBundleItemAsync(bundle);

            return new NullJsonResult(); // Tells the grid it saved successfully
        }

        public async Task<IActionResult> DeleteBundleItems(int id)
        {
            var bundleItem = await _bundleService.GetBundleItemByIdAsync(id);
            bundleItem.DeletedOnUtc = DateTime.UtcNow;
            bundleItem.DeletedBy = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0;
            bundleItem.IsActive = false;
            if (bundleItem != null)
                await _bundleService.UpdateBundleItemAsync(bundleItem);


            var allBundleItems = await _bundleService.GetBundleItemsAsync(bundleItem.BundleId);
            bool hasActiveItems = allBundleItems.Any(x => x.IsActive);
            if (!hasActiveItems)
            {
                var parentBundle = await _bundleService.GetBundleByIdAsync(bundleItem.BundleId);
                if (parentBundle != null)
                {

                    await _bundleService.RestoreVariantPriceFromBackupAsync(parentBundle.VariantId);
                    // await _bundleService.DisableBundle(parentBundle.VariantId);
                }
            }
            return new NullJsonResult();
        }

        #endregion

        #region Bundle Items Product Add Poup
        [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> BundleItemProductAddPopup(int bundleId)
        { 

            var bundledetails = await _bundleService.GetBundleByIdAsync(bundleId);
            var searchModel = new BundleItemProductSearchModel();
            int copyvariantId = 0;
            string variantIds = null;
            // Fetch items currently mapped to this specific bundle
            var bundleItems = await _bundleService.GetBundleItemsAsync(bundleId);
            // 2. Intelligent Pre-population Logic:
            // If this bundle is completely empty, we want to look at the parent product's OTHER bundles
            // to see if we can find a similar one to copy from.
            if (bundleItems.Count == 0)
            {
                var allBundles = await _bundleService.GetBundlesByProductIdAsync(bundledetails.ProductId);
                if (allBundles != null && allBundles.Any())
                {
                    // Sort the other bundles to find the closest match:
                    // - Ignore bundles with 0 pieces and ignore the current bundle itself.
                    // - Order primarily by the closest piece count (e.g., if we want a 5-piece, look for a 4 or 6-piece first).
                    // - Tie-breaker: Prefer the larger bundle (Descending).
                    var sortedBundles = allBundles
                        .Where(x => x.NoOfPieces > 0 && x.Id != bundleId)
                        .OrderBy(x => Math.Abs(x.NoOfPieces - bundledetails.NoOfPieces))
                        .ThenByDescending(x => x.NoOfPieces)
                        .ToList();

                    foreach (var bundle in sortedBundles)
                    {
                        var previousItems = await _bundleService.GetBundleItemsAsync(bundle.Id);

                        if (previousItems != null && previousItems.Any())
                        {
                            // Create a comma-separated list of Variant IDs from the template bundle
                            // so the UI can pre-select or highlight them for the admin.
                            copyvariantId = bundle.VariantId;
                            var variantIdList = previousItems.Select(x => x.VariantId).ToList();
                            variantIds = string.Join(",", variantIdList);
                            // Template found, break out of the loop
                            break;
                        }
                    }
                }
            }
            searchModel.BundleId = bundleId;
            searchModel.VariantIds = variantIds;
            searchModel.CopyVariantId = copyvariantId;
            searchModel.SetPopupGridPageSize();
            return View("BundleProductAddPopup", searchModel);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> BundleItemProductAddPopup(AddBundleItemProductModel model)
        { 

            if (model.SelectedProducts != null && model.SelectedProducts.Any())
            {
                var bundeldetails = await _bundleService.GetBundleByIdAsync(model.BundleId);
                var bundleItems = await _bundleService.GetBundleItemsAsync(model.BundleId);

                if (bundeldetails != null)
                {
                    // --- PHASE 1: CAPACITY VALIDATION ---

                    // Calculate how many items are currently saved in this bundle

                    int currentTotalPieces = bundleItems.Sum(x => x.Quantity);

                    int netPiecesTryingToAdd = 0;
                    // Figure out the *actual* net increase in pieces. 
                    // We do this because updating an existing item from Qty 1 to Qty 2 is only a net increase of 1, not 2.
                    foreach (var variant in model.SelectedProducts.Values)
                    {
                        var existingProductInBundle = await _bundleService.GetBundleItemByProductIdAsync(model.BundleId, variant.ProductId);

                        if (existingProductInBundle != null)
                        {
                            // Calculate the difference between the new requested quantity and the current database quantity
                            netPiecesTryingToAdd += (variant.Quantity - existingProductInBundle.Quantity);
                        }
                        else
                        {
                            // Brand new item being added, so add the full quantity to the net calculation
                            netPiecesTryingToAdd += variant.Quantity;
                        }
                    }
                    // Enforce the strict NoOfPieces rule
                    if (currentTotalPieces + netPiecesTryingToAdd > bundeldetails.NoOfPieces)
                    {
                        return Json(new { success = false, message = $"This bundle allows up to {bundeldetails.NoOfPieces} pieces. You have selected more than allowed." });
                    }

                    // --- PHASE 2: DATABASE EXECUTION ---

                    foreach (var variant in model.SelectedProducts.Values)
                    {
                        var existingProductInBundle = await _bundleService.GetBundleItemByProductIdAsync(model.BundleId, variant.ProductId);

                        if (existingProductInBundle != null)
                        {
                            // Scenario A: The product exists, but the admin changed the Variant (e.g., changed from King to Queen)
                            if (existingProductInBundle.VariantId != variant.VariantId)
                            {
                                // Delete the old configuration. We set it to null so the insert logic below triggers.
                                await _bundleService.DeleteBundleItemAsync(existingProductInBundle);
                                existingProductInBundle = null;
                            }
                            // Scenario B: The product and variant are exactly the same, only the quantity changed
                            else
                            {
                                existingProductInBundle.Quantity = variant.Quantity;
                                existingProductInBundle.UpdatedOnUtc = DateTime.UtcNow;
                                existingProductInBundle.IsActive = true;
                                await _bundleService.UpdateBundleItemAsync(existingProductInBundle);
                                continue; // Done with this item, skip the insert logic
                            }
                        }
                        // Scenario C: This is either a brand new product for this bundle, 
                        // or the old variant was deleted in Scenario A.
                        if (existingProductInBundle == null)
                        {
                            var bundleItem = new BundleItem
                            {
                                BundleId = model.BundleId,
                                ProductId = variant.ProductId,
                                Quantity = variant.Quantity,
                                VariantId = variant.VariantId,
                                IsActive = true,
                                CreatedOnUtc = DateTime.UtcNow,
                                CreatedBy = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0
                            };

                            await _bundleService.InsertBundleItemAsync(bundleItem);
                        }
                    }
                }
            }

            ViewBag.RefreshPage = true;
            ViewBag.btnId = model.btnId;

            var searchModel = new BundleItemProductSearchModel { BundleId = model.BundleId };
            return View("BundleProductAddPopup", searchModel);
        }

        [HttpPost] 
        [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> BundleItemProductAddPopupList(BundleItemProductSearchModel searchModel)
        {
             

            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));
            IPagedList<Product> products = null;
            //get products
        



            if (searchModel.CopyVariantId > 0)
            {
                var bundle = await _bundleService.GetBundleByVariantIdAsync(searchModel.CopyVariantId);
                var copyBundleItems = await _bundleService.GetBundleItemsAsync(bundle.Id);
                if (copyBundleItems.Count > 0)
                {
                    var productIdsToInclude = copyBundleItems.Select(x => x.ProductId).ToArray();
                    var prefillProductsList = await _productService.GetProductsByIdsAsync(productIdsToInclude);
                    products = new PagedList<Product>(prefillProductsList.ToList(), searchModel.Page - 1, searchModel.PageSize);
                }
                else
                {
                    products = await _productService.OverriddenSearchProductsAsync(
           showHidden: true,
           keywords: searchModel.SearchProductName,
           pageIndex: searchModel.Page - 1,
           pageSize: searchModel.PageSize);
                    searchModel.CopyVariantId = 0;
                }
            }
            else
            {
                products = await _productService.OverriddenSearchProductsAsync(
                showHidden: true,
                keywords: searchModel.SearchProductName,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);
            }
            //prepare grid model
            var model = await new BundleItemProductListModel().PrepareToGridAsync(searchModel, products, () =>
            {
                return products.SelectAwait(async product =>
                {
                    var productModel = new BundleItemProductModel();
                    productModel.ProductId = product.Id;
                    productModel.Published = product.Published;
                    productModel.Quantity = 0;
                    var defaultProductPicture = (await _pictureService.GetPicturesByProductIdAsync(product.Id, 1)).FirstOrDefault();
                    (productModel.Picture, _) = await _pictureService.GetPictureUrlAsync(defaultProductPicture, 75);
                    productModel.Name = product.Name;
                    productModel.BundleId = searchModel.BundleId;
                    return productModel;
                });
            });

            return Json(model);
        }

        #endregion

        #region Configuration
        [AuthorizeAdmin]
        [Area(AreaNames.ADMIN)]
        [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
        public async Task<IActionResult> Configure()
        { 

            // This line now works because _productBundleSettings was injected!
            var model = new ConfigurationModel
            {
                DiscountPercentage = _productBundleSettings.DiscountPercentage
            };

            return View("Configure", model);
        }
        [HttpPost]
        [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        { 

            // Save the settings using your new settings class
            _productBundleSettings.DiscountPercentage = model.DiscountPercentage;
            await _settingService.SaveSettingAsync(_productBundleSettings);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));


            return await Configure();
        }
        #endregion

        [HttpGet]
        [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> Manage(int id)
        { 


            var variantdetails = await _productService.GetVariantByVariantId(id);
            if (variantdetails == null)
                return RedirectToAction("List", "Product", new { area = AreaNames.ADMIN });

            var product = await _productService.GetProductByIdAsync(variantdetails.ProductId);
            if (product == null)
                return RedirectToAction("List", "Product", new { area = AreaNames.ADMIN });


            var bundledetails = await _bundleService.GetBundleByVariantIdAsync(id);

            // added PictureUrl and DisplayOnProductPage
            var model = new BundleConfigurationModel
            {
                ProductId = variantdetails.ProductId,
                ProductName = product.Name,
                VariantId = id,
                VariantName = !string.IsNullOrEmpty(variantdetails.Title) ? variantdetails.Title : product.Name,
                CreatedOnUtc = bundledetails == null ? null : bundledetails.CreatedOnUtc,
                UpdatedOnUtc = bundledetails == null ? null : bundledetails.UpdatedOnUtc,
                BundleId = bundledetails == null ? 0 : bundledetails.Id,
                NoOfPieces = bundledetails == null ? 0 : bundledetails.NoOfPieces,
                PictureId = bundledetails?.PictureId ?? 0,
                DisplayOnProductPage = bundledetails == null ? false : bundledetails.DisplayOnProductPage,
            
            };

            BundleConfiguration siblingBundleDetails = null;

            if (bundledetails == null)
            {
                var allBundles = await _bundleService.GetBundlesByProductIdAsync(product.Id);

                if (allBundles != null && allBundles.Any())
                {
                    var sortedBundles = allBundles.OrderByDescending(x => x.CreatedOnUtc).ToList();

                    foreach (var bundle in sortedBundles)
                    {
                        var previousItems = await _bundleService.GetBundleItemsAsync(bundle.Id);

                        if (previousItems != null && previousItems.Any())
                        {
                            siblingBundleDetails = await _bundleService.GetBundleByIdAsync(bundle.Id);
                            break;
                        }
                    }
                }
            }

            if (bundledetails != null)
            {
                model.Id = bundledetails.Id;
                model.Name = bundledetails.Name;
                model.NoOfPieces = bundledetails.NoOfPieces;
           
                if (!bundledetails.IsActive)
                {
                    bundledetails.IsActive = true;
                    bundledetails.UpdatedOnUtc = DateTime.UtcNow;
                    bundledetails.UpdatedBy = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0;
                    bundledetails.DeletedOnUtc = null;
                    bundledetails.DeletedBy = 0;
                    await _bundleService.UpdateBundleAsync(bundledetails);
                }
            }
            else if (siblingBundleDetails != null)
            {
                model.Name = siblingBundleDetails.Name;
            }
            else
            {
                model.Name = $"Bundle for {model.VariantName}";
            }

            model.BundleItemSearchModel.BundleId = model.Id;
            model.BundleItemSearchModel.SetGridPageSize();
            return View("Default", model);
        }
        //#endregion


        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> UpdateBundleName(
            int bundleId, int productId, int variantId, string name, int noOfPieces, bool continueEditing,
            bool displayOnProductPage, int pictureId )
        { 

            var bundleValueConfig = await _bundleService.GetBundleByIdAsync(bundleId);

            if (bundleValueConfig != null)
            {
                // UPDATE EXISTING
                bundleValueConfig.Name = name;
                bundleValueConfig.UpdatedOnUtc = DateTime.UtcNow;
                bundleValueConfig.UpdatedBy = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0;
                bundleValueConfig.NoOfPieces = noOfPieces;
                bundleValueConfig.DisplayOnProductPage = displayOnProductPage;
            bundleValueConfig.PictureId = pictureId;
                await _bundleService.UpdateBundleAsync(bundleValueConfig);

            }
            else
            {

                var newBundleValueConfig = new BundleConfiguration
                {
                    ProductId = productId,
                    VariantId = variantId,
                    Name = name,
                    IsActive = true,
                    CreatedOnUtc = DateTime.UtcNow,
                    CreatedBy = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0,
                    NoOfPieces = noOfPieces,
                    DisplayOnProductPage = displayOnProductPage,
                    PictureId = pictureId
                };

                await _bundleService.InsertBundleAsync(newBundleValueConfig);
            }



            if (continueEditing)
            {
                return RedirectToAction("Manage", new { id = variantId });
            }
            return RedirectToAction("Edit", "Product", new { id = productId, area = AreaNames.ADMIN });

        }

        #region BundleLog
        [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
        public async Task<IActionResult> Log(int productId = 0, int variantId = 0)
        {  
            var model = new BundleAuditLogSearchModel
            {
                ProductId = productId,
                VariantId = variantId
            };

            model.SetGridPageSize();

            return View("LogList", model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
        public async Task<IActionResult> LogListData(BundleAuditLogSearchModel searchModel)
        { 

            // 3. Pass the IDs from the search model directly into your service!
            var logs = await _bundleLoggerService.GetAllLogsAsync(
                searchModel.ProductId,
                searchModel.VariantId,
                searchKeyword: searchModel.SearchKeyword,
                searchModel.Page - 1,
                searchModel.PageSize);

            var model = await new BundleAuditLogListModel().PrepareToGridAsync(searchModel, logs, () =>
            {
                return logs.SelectAwait(async log =>
                {
                    string userIdentifier = string.Empty;

                    //setting user email 
                    var customer = await _customerService.GetCustomerByIdAsync(log.UserIdentifier);

                    if (customer != null)
                    {
                        userIdentifier = await _customerService.IsRegisteredAsync(customer)
                            ? customer.Email
                            : await _localizationService.GetResourceAsync("Admin.Customers.Guest");
                    }
                    // return  loglist
                    return new BundleAuditLogModel
                    {
                        Id = log.Id,
                        BundleId = log.BundleId,
                        ProductId = log.ProductId,
                        VariantId = log.VariantId,
                        ActionType = log.ActionType,
                        UserIdentifier = userIdentifier,
                        OldPrice = log.OldPrice,
                        NewPrice = log.NewPrice,
                        IsValid = log.IsValid,
                        RequiredPieces = log.RequiredPieces,
                        ActualQuantity = log.ActualQuantity,
                        LogMessage = log.LogMessage,
                        CreatedOnUtc = log.CreatedOnUtc.ToLocalTime()
                    };
                });
            });

            return Json(model);
        }

        #endregion
    }
}