using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Service.StoreWideDiscount;
using MWT.Plugin.Misc.MwtStorefront.Area.Admin.Factories.Customization;
using MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.Utilities;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.ScheduleTasks;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;

using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Web.Areas.Admin.Controllers.Customizations
{
    public class UtilitiesController : BaseAdminController
    {
        #region Fields

        private readonly IUtilitiesModelFactory _utilitiesModelFactory;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IPermissionService _permissionService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IProductTemplateService _productTemplateService;
        private readonly IStoreWideDiscountService _storeWideDiscountService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IWorkContext _workContext;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly ILogger _logger;


        #endregion
        public UtilitiesController(IUtilitiesModelFactory utilitiesModelFactory,
       IProductService productService,
       ICategoryService categoryService,
       IPermissionService permissionService,
       INotificationService notificationService,
       ILocalizationService localizationService,
       IProductTemplateService productTemplateService,
       IStoreWideDiscountService storeWideDiscountService,
       IWorkContext workContext,
       IStaticCacheManager staticCacheManager,
       IScheduleTaskService scheduleTaskService,
       ILogger logger
       )
        {
            this._utilitiesModelFactory = utilitiesModelFactory;
            this._productService = productService;
            this._categoryService = categoryService;
            this._permissionService = permissionService;
            this._notificationService = notificationService;
            this._localizationService = localizationService;
            this._productTemplateService = productTemplateService;
            this._storeWideDiscountService = storeWideDiscountService;
            this._workContext = workContext;
            this._staticCacheManager = staticCacheManager;
            this._scheduleTaskService = scheduleTaskService;
            this._logger = logger;
        }
        //public virtual async Task<IActionResult> ProductNotesManagement()
        //{
        //    if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
        //        return await AccessDeniedDataTablesJson();
        //    return View(await this._utilitiesModelFactory.PrepareProductNotesManagementModel(new ProductNotesManagementModel()));
        //}

        //[HttpPost]
        //public virtual async Task<IActionResult> ProductNotesManagement(ProductNotesManagementModel model)
        //{
        //    if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
        //        return await AccessDeniedDataTablesJson();
        //    if (ModelState.IsValid)
        //    {
        //        List<Product> products = new List<Product>();
        //        List<int> lstProductIds = new List<int>();
        //        if (model.EntityType == "Product")
        //        {
        //            foreach (var _productId in model.ProductIds.Split(','))
        //            {
        //                int.TryParse(_productId, out int productId);
        //                if (productId != 0 && !lstProductIds.Contains(productId))
        //                    lstProductIds.Add(productId);

        //            }

        //        }
        //        else
        //        {
        //            foreach (var _categoryId in model.CategoryIds.Split(','))
        //            {
        //                int.TryParse(_categoryId, out int categoryId);
        //                if (categoryId != 0)
        //                {
        //                    var lstPrdcategory = await _categoryService.GetProductCategoriesByCategoryIdAsync(categoryId, 0, int.MaxValue);
        //                    foreach (var prdCategory in lstPrdcategory)
        //                    {
        //                        if (!lstProductIds.Contains(prdCategory.ProductId))
        //                            lstProductIds.Add(prdCategory.ProductId);
        //                    }
        //                }
        //            }

        //        }
        //        if (lstProductIds.Count > 0)
        //        {
        //            products = (await _productService.GetProductsByIdsAsync(lstProductIds.ToArray())).ToList();
        //            foreach (var product in products)
        //            {
        //                product.Notes = model.Notes;
        //                await _productService.UpdateProductWithoutEvent(product);
        //            }
        //        }

        //        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Utilities.Notes.Updated"));
        //    }
        //    return View(await this._utilitiesModelFactory.PrepareProductNotesManagementModel(new ProductNotesManagementModel()));
        //}


        //public virtual async Task<IActionResult> BulkProductMapping()
        //{
        //    var model = new ProductMappingModel();
        //    var templates = (await this._productTemplateService.GetAllProductTemplatesAsync()).Where(t => !string.Equals(t.ViewPath, "ProductTemplate.Grouped", System.StringComparison.InvariantCultureIgnoreCase));
        //    foreach (var template in templates)
        //    {
        //        model.ProductTemplates.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem()
        //        {
        //            Text = template.Name,
        //            Value = template.Id.ToString()

        //        });
        //    }

        //    var customizationFormTemplates = await this._productService.GetAllProductCustomizationFormTemplatesAsync();
        //    foreach (var template in customizationFormTemplates)
        //    {
        //        model.CustomizationFormTemplates.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem()
        //        {
        //            Text = template.Name,
        //            Value = template.Id.ToString()

        //        });
        //    }
        //    return View(model);
        //}
        //[HttpPost]
        //public virtual async Task<IActionResult> BulkProductMapping(ProductMappingModel model)
        //{
        //    if (ModelState.IsValid)

        //    {

        //        switch (model.Action)
        //        {

        //            case "Product Quantity Update":
        //                await this._utilitiesModelFactory.BulkUpdateInventory(model.ProductIds, model.Inventory);
        //                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Utilities.Bulk.Product.Quantity.Updated"));
        //                break;
        //            case "Category Products Publish/Unpublish":
        //                await this._utilitiesModelFactory.BulkPublishUnpublishProductCategories(model.ProductIds, model.CategoryIds, model.SpecificationAttributeIds, model.Published);

        //                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Utilities.Bulk.Product.category.Updated"));
        //                break;
        //            case "Product Category Department Mapping":
        //                await this._utilitiesModelFactory.BulkUpdateProductCategorySpecificationAttributeMapping(model.ProductIds, model.CategoryIds, model.SpecificationAttributeIds, model.Remove);

        //                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Utilities.Bulk.Mapping.Updated"));
        //                break;
        //            case "Product Listing Section":
        //                await this._utilitiesModelFactory.BulkUpdateProductListing(model.ProductIds, model.ProductId, model.ProductListingType);

        //                _notificationService.SuccessNotification(string.Format((await _localizationService.GetResourceAsync("Admin.Utilities.Bulk.Listing.Mapping.Updated")), model.ProductListingType));
        //                break;
        //            case "Product Template Mapping":
        //                await this._utilitiesModelFactory.BulkUpdateTemplate(model.CategoryIds, model.ProductTemplateId);

        //                _notificationService.SuccessNotification(string.Format((await _localizationService.GetResourceAsync("Admin.Utilities.Bulk.Listing.Template.Updated")), model.ProductListingType));
        //                break;
        //            case "Customization Form Template":
        //                await this._utilitiesModelFactory.BulkUpdateCustomizationFormTemplates(model.ProductIds, model.CategoryIds, model.CustomizationFormTemplateId);

        //                _notificationService.SuccessNotification(string.Format((await _localizationService.GetResourceAsync("Admin.Utilities.Bulk.Listing.CustomizationForm.Updated")), model.ProductListingType));
        //                break;
        //        }

        //    }
        //    var templates = (await this._productTemplateService.GetAllProductTemplatesAsync()).Where(t => !string.Equals(t.ViewPath, "ProductTemplate.Grouped", System.StringComparison.InvariantCultureIgnoreCase));
        //    foreach (var template in templates)
        //    {
        //        model.ProductTemplates.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem()
        //        {
        //            Text = template.Name,
        //            Value = template.Id.ToString()

        //        });
        //    }
        //    var customizationFormTemplates = await this._productService.GetAllProductCustomizationFormTemplatesAsync();
        //    foreach (var template in customizationFormTemplates)
        //    {
        //        model.CustomizationFormTemplates.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem()
        //        {
        //            Text = template.Name,
        //            Value = template.Id.ToString()

        //        });
        //    }
        //    return View(model);
        //}


        //public virtual async Task<IActionResult> Marketing()
        //{
        //    if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
        //        return await AccessDeniedDataTablesJson();
        //    return View(await this._utilitiesModelFactory.PrepareMarketingModel());
        //}

        //[HttpPost]
        //public virtual async Task<IActionResult> Marketing(MarketingModel model)
        //{
        //    if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
        //        return await AccessDeniedDataTablesJson();
        //    if (ModelState.IsValid)
        //    {
        //        await this._utilitiesModelFactory.SaveMarketingSettings(model);
        //        await _staticCacheManager.ClearAsync();
        //        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Utilities.Marketing.Updated"));
        //        return View(await this._utilitiesModelFactory.PrepareMarketingModel());
        //    }
        //    return View(model);
        //}

        //#region StoreWideDiscount 

        //[Route("/Admin/Utilities/StoreWideDiscount/List")]
        //public virtual async Task<IActionResult> StoreWideDiscountList()
        //{
        //    if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
        //        return await AccessDeniedDataTablesJson();
        //    var model = new StoreWideDiscountSearchModel();

        //    return View("StoreWideDiscount", await _utilitiesModelFactory.PrepareStoreWideDiscountSearchModel(model));
        //}

        //[Route("/Admin/Utilities/StoreWideDiscount/List")]
        //[HttpPost]
        //public virtual async Task<IActionResult> StoreWideDiscountList(StoreWideDiscountSearchModel searchModel)
        //{
        //    if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
        //        return await AccessDeniedDataTablesJson();

        //    //prepare model
        //    var model = await _utilitiesModelFactory.PrepareStoreWideDiscountListModelAsync(searchModel);

        //    return Json(model);
        //}

        //[Route("/Admin/Utilities/StoreWideDiscount/Create")]
        //[Route("/Admin/Utilities/StoreWideDiscount/Edit/{id}")]
        //public virtual async Task<IActionResult> StoreWideDiscountCreate(int id = 0)
        //{
        //    if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
        //        return await AccessDeniedDataTablesJson();

        //    var storeWideDiscount = new StoreWideDiscount();
        //    if (id != 0)
        //    {
        //        storeWideDiscount = await _storeWideDiscountService.GetStoreWideDiscountByIdAsync(id);
        //        if (storeWideDiscount == null)
        //        {
        //            return RedirectToAction("StoreWideDiscountList");
        //        }
        //    }
        //    return View("_CreateOrUpdate.StoreWideDiscount", await _utilitiesModelFactory.PrepareStoreWideDiscountModel(null, storeWideDiscount));
        //}

        //[Route("/Admin/Utilities/StoreWideDiscount/Create")]
        //[Route("/Admin/Utilities/StoreWideDiscount/Edit/{storewidediscountid}")]
        //[HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        //public virtual async Task<IActionResult> StoreWideDiscountCreate(StoreWideDiscountModel model, bool continueEditing)
        //{
        //    if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
        //        return await AccessDeniedDataTablesJson();


        //    var storeWideDiscount = new StoreWideDiscount();
        //    if (model.Id != 0)
        //    {
        //        storeWideDiscount = await _storeWideDiscountService.GetStoreWideDiscountByIdAsync(model.Id);
        //        if (storeWideDiscount == null)
        //        {
        //            return RedirectToAction("StoreWideDiscountList");
        //        }
        //    }

        //    var discountExist = await _storeWideDiscountService.IsStoreWideDiscountExistInDateRange(model.StartDate, model.EndDate, model.Id);
        //    if (ModelState.IsValid && !discountExist)
        //    {
        //        if (model.Id != 0)
        //        {
        //            storeWideDiscount = model.ToEntity(storeWideDiscount);
        //            storeWideDiscount.UpdatedOn = System.DateTime.UtcNow;
        //            storeWideDiscount.IsProcessed = false;
        //            await _storeWideDiscountService.UpdateStoreWideDiscountAsync(storeWideDiscount);
        //            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Utilities.Marketing.Offer.StoreWideDiscount.Updated"));
        //            if (!continueEditing)
        //            {
        //                return RedirectToAction("StoreWideDiscountList");
        //            }

        //            return RedirectToAction("StoreWideDiscountCreate", new { id = storeWideDiscount.Id });
        //        }
        //        else
        //        {
        //            storeWideDiscount = model.ToEntity<StoreWideDiscount>();
        //            storeWideDiscount.IsProcessed = false;
        //            storeWideDiscount.CreatedOn = System.DateTime.UtcNow;
        //            storeWideDiscount.UpdatedOn = System.DateTime.UtcNow;
        //            storeWideDiscount.CreatedBy = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0;
        //            await _storeWideDiscountService.InsertStoreWideDiscountAsync(storeWideDiscount);
        //            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Utilities.Marketing.Offer.StoreWideDiscount.Added"));
        //            if (!continueEditing)
        //            {
        //                return RedirectToAction("StoreWideDiscountList");
        //            }

        //            return RedirectToAction("StoreWideDiscountCreate", new { id = storeWideDiscount.Id });
        //        }
        //    }
        //    if (discountExist)
        //    {
        //        ModelState.AddModelError("", string.Format(await _localizationService.GetResourceAsync("Admin.Utilities.Marketing.Offer.Exist"), model.StartDate, model.EndDate));
        //    }
        //    return View("_CreateOrUpdate.StoreWideDiscount", await _utilitiesModelFactory.PrepareStoreWideDiscountModel(model, storeWideDiscount));
        //}

        //[HttpPost]
        ///// <returns>A task that represents the asynchronous operation</returns>
        //public virtual async Task<IActionResult> StoreWideDiscountDelete(int id)
        //{
        //    if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
        //        return AccessDeniedView();

        //    //try to get a PairWith product with the specified id
        //    var storeWideDiscount = await _storeWideDiscountService.GetStoreWideDiscountByIdAsync(id)
        //        ?? throw new ArgumentException("No Store Wide Discount found with the specified id");
        //    storeWideDiscount.IsProcessed = false;
        //    storeWideDiscount.IsDeleted = true;
        //    await _storeWideDiscountService.UpdateStoreWideDiscountAsync(storeWideDiscount);
        //    return new NullJsonResult();
        //}

        //#endregion

        //#region StoreWideDiscountSetting


        //[HttpPost]
        //public virtual async Task<IActionResult> StoreWideDiscountSettingList(StoreWideDiscountSettingSearchModel searchModel)
        //{
        //    if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
        //        return await AccessDeniedDataTablesJson();

        //    //prepare model
        //    var model = await _utilitiesModelFactory.PrepareStoreWideDiscountSettingListModelAsync(searchModel);

        //    return Json(model);
        //}

        //[Route("/Admin/Utilities/StoreWideDiscount/Setting/Create/{storewidediscountid}")]
        //[Route("/Admin/Utilities/StoreWideDiscount/Setting/Edit/{storewidediscountid}/{settingid}")]
        //public virtual async Task<IActionResult> StoreWideDiscountSettingCreateUpdate(int storewidediscountid, int settingid = 0)
        //{
        //    if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
        //        return await AccessDeniedDataTablesJson();

        //    var storeWideDiscount = new StoreWideDiscount();
        //    var storeWideDiscountSetting = new StoreWideDiscountSetting();
        //    storeWideDiscount = await _storeWideDiscountService.GetStoreWideDiscountByIdAsync(storewidediscountid);
        //    if (storeWideDiscount == null)
        //    {
        //        throw new ArgumentException("No StoreWideDiscount found with the specified id");
        //    }
        //    if (settingid != 0)
        //    {
        //        storeWideDiscountSetting = await _storeWideDiscountService.GetStoreWideDiscountSettingByIdAsync(settingid);
        //        if (storeWideDiscountSetting == null)
        //        {
        //            throw new ArgumentException("No StoreWideDiscountSetting found with the specified id");
        //        }
        //    }


        //    return View("_CreateOrUpdate.StoreWideDiscount.Setting", await _utilitiesModelFactory.PrepareStoreWideDiscountSettingModel(null, storeWideDiscountSetting));
        //}

        //[Route("/Admin/Utilities/StoreWideDiscount/Setting/Create/{storewidediscountid}")]
        //[Route("/Admin/Utilities/StoreWideDiscount/Setting/Edit/{storewidediscountid}/{settingid}")]
        //[HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        //public virtual async Task<IActionResult> StoreWideDiscountSettingCreateUpdate(StoreWideDiscountSettingModel model)
        //{
        //    if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
        //        return await AccessDeniedDataTablesJson();


        //    if (ModelState.IsValid)
        //    {
        //        var storeWideDiscount = new StoreWideDiscount();
        //        var storeWideDiscountSetting = new StoreWideDiscountSetting();
        //        storeWideDiscount = await _storeWideDiscountService.GetStoreWideDiscountByIdAsync(model.StoreWideDiscountId);
        //        if (storeWideDiscount == null)
        //        {
        //            throw new ArgumentException("No StoreWideDiscount found with the specified id");
        //        }
        //        if (model.Id != 0)
        //        {
        //            storeWideDiscountSetting = await _storeWideDiscountService.GetStoreWideDiscountSettingByIdAsync(model.Id);
        //            if (storeWideDiscountSetting == null)
        //            {
        //                throw new ArgumentException("No StoreWideDiscountSetting found with the specified id");
        //            }
        //        }

        //        storeWideDiscount.IsProcessed = false;
        //        await _storeWideDiscountService.UpdateStoreWideDiscountAsync(storeWideDiscount);
        //        if (model.Id != 0)
        //        {
        //            storeWideDiscountSetting = model.ToEntity(storeWideDiscountSetting);
        //            storeWideDiscountSetting.CategoryIds = String.Join(",", model.SelectedCategoryIds);
        //            storeWideDiscountSetting.UpdatedOn = System.DateTime.UtcNow;

        //            await _storeWideDiscountService.UpdateStoreWideDiscountSettingAsync(storeWideDiscountSetting);
        //            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Utilities.Marketing.Offer.StoreWideDiscount.Setting.Updated"));

        //            ViewBag.RefreshPage = true;
        //            return View("_CreateOrUpdate.StoreWideDiscount.Setting", model);
        //        }
        //        else
        //        {
        //            storeWideDiscountSetting = model.ToEntity<StoreWideDiscountSetting>();
        //            storeWideDiscountSetting.CategoryIds = String.Join(",", model.SelectedCategoryIds);
        //            storeWideDiscountSetting.CreatedOn = System.DateTime.UtcNow;
        //            storeWideDiscountSetting.UpdatedOn = System.DateTime.UtcNow;
        //            await _storeWideDiscountService.InsertStoreWideDiscountSettingAsync(storeWideDiscountSetting);
        //            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Utilities.Marketing.Offer.StoreWideDiscount.Setting.Added"));

        //            ViewBag.RefreshPage = true;
        //            return View("_CreateOrUpdate.StoreWideDiscount.Setting", model);
        //        }
        //    }
        //    else
        //    {
        //        await _utilitiesModelFactory.PrepareStoreWideDiscountSettingModel(model, new StoreWideDiscountSetting());
        //        return View("_CreateOrUpdate.StoreWideDiscount.Setting", model);
        //    }

        //}

        //[HttpPost]
        ///// <returns>A task that represents the asynchronous operation</returns>
        //public virtual async Task<IActionResult> StoreWideDiscountSettingDelete(int id)
        //{
        //    if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
        //        return AccessDeniedView();

        //    //try to get a PairWith product with the specified id
        //    var storeWideDiscountSetting = await _storeWideDiscountService.GetStoreWideDiscountSettingByIdAsync(id)
        //        ?? throw new ArgumentException("No Store Wide Discount found with the specified id");

        //    storeWideDiscountSetting.IsDeleted = true;
        //    await _storeWideDiscountService.UpdateStoreWideDiscountSettingAsync(storeWideDiscountSetting);

        //    var storeWideDiscount = await _storeWideDiscountService.GetStoreWideDiscountByIdAsync(storeWideDiscountSetting.StoreWideDiscountId);
        //    if (storeWideDiscount != null)
        //    {
        //        storeWideDiscount.IsProcessed = false;
        //        await _storeWideDiscountService.UpdateStoreWideDiscountAsync(storeWideDiscount);
        //    }

        //    return new NullJsonResult();
        //}
        //#endregion

        //#region StoreWideProductDiscountInfo
        //[HttpPost]
        //public virtual async Task<IActionResult> OfferDiscountList(StoreWideProductDiscountInfoSearchModel searchModel)
        //{
        //    if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
        //        return await AccessDeniedDataTablesJson();
        //    //prepare model
        //    var model = await _utilitiesModelFactory.PrepareOfferDiscountListModelAsync(searchModel);

        //    return Json(model);
        //}
        //#endregion

        //#region  StoreWideProductDiscountHistory
        //public virtual async Task<IActionResult> ProductDiscountHistorySearch(int searchProductId)
        //{
        //    if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
        //        return await AccessDeniedDataTablesJson();
        //    var model = new StoreWideProductDiscountHistorySearchModel();
        //    model.SearchProductId = searchProductId;
        //    return View(await _utilitiesModelFactory.PrepareProductDiscountHistorySearchModel(model));
        //}


        //[HttpPost]
        //public virtual async Task<IActionResult> ProductOfferDiscountList(StoreWideProductDiscountHistorySearchModel searchModel)
        //{
        //    if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
        //        return await AccessDeniedDataTablesJson();

        //    //prepare model
        //    var model = await _utilitiesModelFactory.PrepareOfferProductDiscountHistoryListModelAsync(searchModel);

        //    return Json(model);
        //}
        //#endregion
        //public virtual async Task<IActionResult> StoreWideDiscount()
        //{
        //    if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
        //        return await AccessDeniedDataTablesJson();
        //    return View(await this._utilitiesModelFactory.PrepareOfferModel());
        //}

        //[HttpPost]
        //public virtual async Task<IActionResult> StoreWideDiscount(StoreWideDiscountSettingModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        await this._storeWideDiscountService.ApplyDiscount(new Core.Domain.Customization.Custom.StoreWideDiscount.StoreWideDiscountSetting()
        //        {
        //            CategoryIds = string.Join(',', model.SelectedCategoryIds),
        //            Discount = model.Discount,
        //            FullStore = model.FullStore,

        //            ProductIds = model.ProductIds,
        //            InfoText = model.InfoHelpText,
        //            InfoHelpText = model.InfoHelpText
        //        });


        //        #region re run schedule tasks
        //        var scheduleTask = await _scheduleTaskService.GetTaskByTypeAsync("Nop.Web.Customizations.Tasks.PriceFilterVariantPriceRangeTaskIScheduleTask");
        //        if (scheduleTask != null)
        //        {
        //            scheduleTask.LastSuccessUtc = scheduleTask.LastSuccessUtc.HasValue ? scheduleTask.LastSuccessUtc.Value.AddHours(-12) : null;
        //            scheduleTask.LastStartUtc = scheduleTask.LastStartUtc.HasValue ? scheduleTask.LastStartUtc.Value.AddHours(-12) : null;
        //            scheduleTask.LastEndUtc = scheduleTask.LastEndUtc.HasValue ? scheduleTask.LastEndUtc.Value.AddHours(-12) : null;
        //            await _scheduleTaskService.UpdateTaskAsync(scheduleTask);
        //        }

        //        scheduleTask = await _scheduleTaskService.GetTaskByTypeAsync("Nop.Web.Customizations.Tasks.ElasticSearchTask");
        //        if (scheduleTask != null)
        //        {
        //            scheduleTask.LastSuccessUtc = scheduleTask.LastSuccessUtc.HasValue ?
        //            System.DateTime.Now.AddHours(-12).AddMinutes(10) : null;
        //            scheduleTask.LastStartUtc = scheduleTask.LastStartUtc.HasValue ? System.DateTime.Now.AddHours(-12).AddMinutes(10) : null;
        //            scheduleTask.LastEndUtc = scheduleTask.LastEndUtc.HasValue ? System.DateTime.Now.AddHours(-12).AddMinutes(10) : null;
        //            await _scheduleTaskService.UpdateTaskAsync(scheduleTask);
        //        }
        //        #endregion
        //        await _staticCacheManager.ClearAsync();
        //        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Utilities.Offer.Updated"));


        //        return View(await this._utilitiesModelFactory.PrepareOfferModel());
        //    }
        //    else
        //    {
        //        return View(await this._utilitiesModelFactory.PrepareOfferModel());
        //    }
        //}


        //#region Import UnPubshed Product Variant

        //public virtual async Task<IActionResult> ImportProductUnpubishedVariant()
        //{
        //    return View(new ImportProductUnpubishedVariantModel());
        //}

        //[HttpPost]
        //public virtual async Task<IActionResult> ImportProductUnpubishedVariant(ImportProductUnpubishedVariantModel model)
        //{
        //    try
        //    {
        //        var product = await _productService.GetProductByIdAsync(model.ProductId);
        //        if (product != null)
        //        {
        //            await _productService.ImportProductUnpubishedVariant(model.ProductId);
        //            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Utilities.Product.Variants.Imported"));
        //        }
        //        else
        //        {
        //            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Product.NotFound"));
        //        }
        //    }
        //    catch (Exception exp)
        //    {
        //        await this._logger.InsertLogAsync(Core.Domain.Logging.LogLevel.Error, "Variant Un Publish Task", exp.Message);
        //        _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Utilities.Product.Variants.Import.Failed"));
        //    }
        //    return View(model);
        //}

        //#endregion

        #region Testomonials
        [CheckPermission(StandardPermission.Orders.ORDERS_VIEW)]
        public virtual async Task<IActionResult> Testimonials()
        {
            return View(new TestimonialSearchModel());
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Orders.ORDERS_VIEW)]
        public virtual async Task<IActionResult> SearchTestimonials(TestimonialSearchModel searchModel)
        {

            return Json(await _utilitiesModelFactory.SearchTestimonials(searchModel));
        }

        #endregion

    }
}
