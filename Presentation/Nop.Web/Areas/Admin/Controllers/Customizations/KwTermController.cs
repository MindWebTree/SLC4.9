using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Domain.KW;
using MWT.Nop.Core.Services.KW;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.ExportImport;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Factories.Customization;
using Nop.Web.Areas.Admin.Models.Customization.Custom.KW;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Web.Areas.Admin.Controllers.Customizations
{
    public class KwTermController : BaseAdminController
    {


        private readonly IAclService _aclService;
        private readonly IKwTermModelFactory _kwTermModelFactory;
        private readonly IKwTermService _kwTermService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerService _customerService;
        private readonly IDiscountService _discountService;
        private readonly IExportManager _exportManager;
        private readonly IImportManager _importManager;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly IProductService _productService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IStoreService _storeService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWorkContext _workContext;
        private readonly ICategoryModelFactory _categoryModelFactory;
        private readonly ICategoryService _categoryService;

        #region Ctor

        public KwTermController(IAclService aclService,
            IKwTermModelFactory kwTermModelFactory,
           IKwTermService kwTermService,
                ICustomerActivityService customerActivityService,
                ICustomerService customerService,
                IDiscountService discountService,
                IExportManager exportManager,
                IImportManager importManager,
                ILocalizationService localizationService,
                ILocalizedEntityService localizedEntityService,
                INotificationService notificationService,
                IPermissionService permissionService,
                IPictureService pictureService,
                IProductService productService,
                IStaticCacheManager staticCacheManager,
                IStoreMappingService storeMappingService,
                IStoreService storeService,
                IUrlRecordService urlRecordService,
                IWorkContext workContext,
                ICategoryModelFactory categoryModelFactory,
                ICategoryService categoryService
                )
        {
            _aclService = aclService;
            _kwTermModelFactory = kwTermModelFactory;
            _kwTermService = kwTermService;
            _customerActivityService = customerActivityService;
            _customerService = customerService;
            _discountService = discountService;
            _exportManager = exportManager;
            _importManager = importManager;
            _localizationService = localizationService;
            _localizedEntityService = localizedEntityService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _pictureService = pictureService;
            _productService = productService;
            _staticCacheManager = staticCacheManager;
            _storeMappingService = storeMappingService;
            _storeService = storeService;
            _urlRecordService = urlRecordService;
            _workContext = workContext;
            _categoryModelFactory = categoryModelFactory;
            _categoryService = categoryService;
        }

        #endregion

        #region Utilities

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task UpdateLocalesAsync(KwTerm kwTerm, KwTermModel model)
        {
            foreach (var localized in model.Locales)
            {
                await _localizedEntityService.SaveLocalizedValueAsync(kwTerm,
                    x => x.Name,
                    localized.Name,
                    localized.LanguageId);

                await _localizedEntityService.SaveLocalizedValueAsync(kwTerm,
                    x => x.Description,
                    localized.Description,
                    localized.LanguageId);

                await _localizedEntityService.SaveLocalizedValueAsync(kwTerm,
                    x => x.MetaKeywords,
                    localized.MetaKeywords,
                    localized.LanguageId);

                await _localizedEntityService.SaveLocalizedValueAsync(kwTerm,
                    x => x.MetaDescription,
                    localized.MetaDescription,
                    localized.LanguageId);

                await _localizedEntityService.SaveLocalizedValueAsync(kwTerm,
                    x => x.MetaTitle,
                    localized.MetaTitle,
                    localized.LanguageId);

                //search engine name
                var seName = await _urlRecordService.ValidateSeNameAsync(kwTerm, localized.SeName, localized.Name, false);
                await _urlRecordService.SaveSlugAsync(kwTerm, seName, localized.LanguageId);
            }
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task UpdatePictureSeoNamesAsync(KwTerm kwTerm)
        {
            var picture = await _pictureService.GetPictureByIdAsync(kwTerm.PictureId);
            if (picture != null)
                await _pictureService.SetSeoFilenameAsync(picture.Id, await _pictureService.GetPictureSeNameAsync(kwTerm.Name));
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task SaveKwTermsAclAsync(KwTerm kwTerm, KwTermModel model)
        {
            kwTerm.SubjectToAcl = model.SelectedCustomerRoleIds.Any();
            await _kwTermService.UpdateKwTermAsync(kwTerm);

            var existingAclRecords = await _aclService.GetAclRecordsAsync(kwTerm);
            var allCustomerRoles = await _customerService.GetAllCustomerRolesAsync(true);
            foreach (var customerRole in allCustomerRoles)
            {
                if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                {
                    //new role
                    if (existingAclRecords.Count(acl => acl.CustomerRoleId == customerRole.Id) == 0)
                        await _aclService.InsertAclRecordAsync(kwTerm, customerRole.Id);
                }
                else
                {
                    //remove role
                    var aclRecordToDelete = existingAclRecords.FirstOrDefault(acl => acl.CustomerRoleId == customerRole.Id);
                    if (aclRecordToDelete != null)
                        await _aclService.DeleteAclRecordAsync(aclRecordToDelete);
                }
            }
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task SaveStoreMappingsAsync(KwTerm kwTerms, KwTermModel model)
        {
            kwTerms.LimitedToStores = model.SelectedStoreIds.Any();
            await _kwTermService.UpdateKwTermAsync(kwTerms);

            var existingStoreMappings = await _storeMappingService.GetStoreMappingsAsync(kwTerms);
            var allStores = await _storeService.GetAllStoresAsync();
            foreach (var store in allStores)
            {
                if (model.SelectedStoreIds.Contains(store.Id))
                {
                    //new store
                    if (existingStoreMappings.Count(sm => sm.StoreId == store.Id) == 0)
                        await _storeMappingService.InsertStoreMappingAsync(kwTerms, store.Id);
                }
                else
                {
                    //remove store
                    var storeMappingToDelete = existingStoreMappings.FirstOrDefault(sm => sm.StoreId == store.Id);
                    if (storeMappingToDelete != null)
                        await _storeMappingService.DeleteStoreMappingAsync(storeMappingToDelete);
                }
            }
        }

        #endregion

        #region List

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_ACCESS_MANAGEKWTERMS)]
        public virtual async Task<IActionResult> List()
        { 
            //prepare model
            var model = await _kwTermModelFactory.PrepareKwTermSearchModelAsync(new KwTermSearchModel());

            return View(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_ACCESS_MANAGEKWTERMS)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> List(KwTermSearchModel searchModel)
        { 
            try
            {

                //prepare model
                var model = await _kwTermModelFactory.PrepareKwTermListModelAsync(searchModel);

                return Json(model);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Create / Edit / Delete
        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_ACCESS_MANAGEKWTERMS)]

        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> Create()
        { 
            //prepare model
            var model = await _kwTermModelFactory.PrepareKwTermModelAsync(new KwTermModel(), null);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_ACCESS_MANAGEKWTERMS)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> Create(KwTermModel model, bool continueEditing)
        { 
            if (ModelState.IsValid)
            {
                try
                {
                    var kwTerm = new KwTerm();
                    kwTerm.Name = model.Name;
                    kwTerm.KwTermsTemplateId = model.KwTermTemplateId;
                    kwTerm.MetaTitle = model.MetaTitle;
                    kwTerm.MetaDescription = model.MetaDescription;
                    kwTerm.Description = model.Description;
                    kwTerm.PageSize = model.PageSize;
                    kwTerm.CreatedOnUtc = DateTime.UtcNow;
                    kwTerm.UpdatedOnUtc = DateTime.UtcNow;
                    kwTerm.PageSizeOptions = model.PageSizeOptions;
                    kwTerm.PictureId = model.PictureId;
                    kwTerm.Published = model.Published;
                    kwTerm.AllowCustomersToSelectPageSize = model.AllowCustomersToSelectPageSize;
                    kwTerm.Deleted = model.Deleted;
                    kwTerm.DisplayOrder = model.DisplayOrder;
                    kwTerm.Id = model.Id;
                    kwTerm.MetaKeywords = model.MetaKeywords;
                    kwTerm.EnableInfiniteScroll = model.EnableInfiniteScroll;
                    await _kwTermService.InsertKwTermAsync(kwTerm);

                    //search engine name
                    model.SeName = await _urlRecordService.ValidateSeNameAsync(kwTerm, model.SeName, kwTerm.Name, true);
                    await _urlRecordService.SaveSlugAsync(kwTerm, model.SeName, 0);

                    //locales
                    await UpdateLocalesAsync(kwTerm, model);

                    await _kwTermService.UpdateKwTermAsync(kwTerm);

                    //update picture seo file name
                    await UpdatePictureSeoNamesAsync(kwTerm);

                    //ACL (customer roles)
                    await SaveKwTermsAclAsync(kwTerm, model);

                    //stores
                    await SaveStoreMappingsAsync(kwTerm, model);

                    //activity log
                    await _customerActivityService.InsertActivityAsync("AddNewKwTerms",
                        string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewKwTerms"), kwTerm.Name), kwTerm);

                    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.KwTerms.Added"));

                    if (!continueEditing)
                        return RedirectToAction("List");

                    return RedirectToAction("Edit", new { id = kwTerm.Id });
                }
                catch (Exception ex)
                {

                    throw ex;
                }
            }

            //prepare model
            model = await _kwTermModelFactory.PrepareKwTermModelAsync(model, null, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_ACCESS_MANAGEKWTERMS)]
        public virtual async Task<IActionResult> Edit(int id)
        { 
            //try to get a kwTerms with the specified id
            var kwTerms = await _kwTermService.GetKwTermByIdAsync(id);
            if (kwTerms == null || kwTerms.Deleted)
                return RedirectToAction("List");

            //prepare model
            var model = await _kwTermModelFactory.PrepareKwTermModelAsync(null, kwTerms);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_ACCESS_MANAGEKWTERMS)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> Edit(KwTermModel model, bool continueEditing)
        { 
            //try to get a kwTerms with the specified id
            var kwTerm = await _kwTermService.GetKwTermByIdAsync(model.Id);
            if (kwTerm == null || kwTerm.Deleted)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                var prevPictureId = kwTerm.PictureId;

                //if parent kwTerms changes, we need to clear cache for previous parent kwTerms


                kwTerm.Name = model.Name;
                kwTerm.KwTermsTemplateId = model.KwTermTemplateId;
                kwTerm.MetaTitle = model.MetaTitle;
                kwTerm.MetaDescription = model.MetaDescription;
                kwTerm.Description = model.Description;
                kwTerm.PageSize = model.PageSize;
                kwTerm.CreatedOnUtc = DateTime.UtcNow;
                kwTerm.UpdatedOnUtc = DateTime.UtcNow;
                kwTerm.PageSizeOptions = model.PageSizeOptions;
                kwTerm.PictureId = model.PictureId;
                kwTerm.Published = model.Published;
                kwTerm.AllowCustomersToSelectPageSize = model.AllowCustomersToSelectPageSize;
                kwTerm.Deleted = model.Deleted;
                kwTerm.DisplayOrder = model.DisplayOrder;
                kwTerm.Id = model.Id;
                kwTerm.MetaKeywords = model.MetaKeywords;
                kwTerm.EnableInfiniteScroll = model.EnableInfiniteScroll;

                await _kwTermService.UpdateKwTermAsync(kwTerm);

                //search engine name
                model.SeName = await _urlRecordService.ValidateSeNameAsync(kwTerm, model.SeName, kwTerm.Name, true);
                await _urlRecordService.SaveSlugAsync(kwTerm, model.SeName, 0);

                //locales
                await UpdateLocalesAsync(kwTerm, model);

                await _kwTermService.UpdateKwTermAsync(kwTerm);

                //delete an old picture (if deleted or updated)
                if (prevPictureId > 0 && prevPictureId != kwTerm.PictureId)
                {
                    var prevPicture = await _pictureService.GetPictureByIdAsync(prevPictureId);
                    if (prevPicture != null)
                        await _pictureService.DeletePictureAsync(prevPicture);
                }

                //update picture seo file name
                await UpdatePictureSeoNamesAsync(kwTerm);

                //ACL
                await SaveKwTermsAclAsync(kwTerm, model);

                //stores
                await SaveStoreMappingsAsync(kwTerm, model);

                //activity log
                await _customerActivityService.InsertActivityAsync("EditKwTerms",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditKwTerms"), kwTerm.Name), kwTerm);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.KwTerms.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = kwTerm.Id });
            }

            //prepare model
            model = await _kwTermModelFactory.PrepareKwTermModelAsync(model, kwTerm, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_ACCESS_MANAGEKWTERMS)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> Delete(int id)
        { 
            //try to get a kwTerms with the specified id
            var kwTerms = await _kwTermService.GetKwTermByIdAsync(id);
            if (kwTerms == null)
                return RedirectToAction("List");

            await _kwTermService.DeleteKwTermAsync(kwTerms);

            //activity log
            await _customerActivityService.InsertActivityAsync("DeleteKwTerms",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteKwTerms"), kwTerms.Name), kwTerms);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.KwTerms.Deleted"));

            return RedirectToAction("List");
        }

        [HttpPost]
        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_ACCESS_MANAGEKWTERMS)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        { 
            if (selectedIds != null)
            {
                await _kwTermService.DeleteKwTermAsync(await (await _kwTermService.GetkwTermsByIdsAsync(selectedIds.ToArray())).WhereAwait(async p => await _workContext.GetCurrentVendorAsync() == null).ToListAsync());
            }

            return Json(new { Result = true });
        }

        #endregion

        #region Products

        /// <returns>A task that represents the asynchronous operation</returns>
        /// 

        [HttpPost]
        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_ACCESS_MANAGEKWTERMS)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> CustomProductAddPopupList(AddProductToKwTermSearchModel searchModel)
        {  
            //prepare model
            var model = await _kwTermModelFactory.CustomPrepareAddProductToKwTermListModelAsync(searchModel);

            return Json(model);
        }


        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_ACCESS_MANAGEKWTERMS)]
        public virtual async Task<IActionResult> ProductAddPopup(int kwTermId)
        {  
            //prepare model
            var model = await _kwTermModelFactory.PrepareAddProductToKwTermSearchModelAsync(new AddProductToKwTermSearchModel());

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_ACCESS_MANAGEKWTERMS)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> ProductAddPopup(AddProductToKwTermModel model)
        { 
            //get selected products
            var selectedProducts = await _productService.GetProductsByIdsAsync(model.SelectedProductIds.ToArray());
            if (selectedProducts.Any())
            {

                var existingProductKwTerm = await _kwTermService.GetProductKwTermsByKwTermIdAsync(model.KwTermId, showHidden: true);
                foreach (var product in selectedProducts)
                {
                    //whether product category with such parameters already exists
                    if (_kwTermService.FindProductKwTerm(existingProductKwTerm, product.Id, model.KwTermId) != null)
                        continue;

                    //insert the new product category mapping
                    await _kwTermService.InsertProductKwTermAsync(new ProductKwTerm
                    {
                        KwTermId = model.KwTermId,
                        ProductId = product.Id,
                        DisplayOrder = 1,
                        MobileDisplayOrder = 1
                    });
                }
            }

            ViewBag.RefreshPage = true;

            return View(new AddProductToKwTermSearchModel());
        }

        [HttpPost]
        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_ACCESS_MANAGEKWTERMS)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> CustomProductList(KwTermProductSearchModel searchModel)
        { 
            //try to get a kwTerms with the specified id
            var kwTerms = await _kwTermService.GetKwTermByIdAsync(searchModel.KwTermId)
                ?? throw new ArgumentException("No kwTerms found with the specified id");

            //prepare model
            var model = await _kwTermModelFactory.CustomPrepareKwTermProductListModelAsync(searchModel, kwTerms);

            return Json(model);
        }


        /// <returns>A task that represents the asynchronous operation</returns>
        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_ACCESS_MANAGEKWTERMS)]
        public virtual async Task<IActionResult> ProductUpdate(KwTermProductModel model)
        { 
            //try to get a product kwTerms with the specified id
            var productKwTerms = await _kwTermService.GetProductKwTermByIdAsync(model.Id)
                ?? throw new ArgumentException("No product kwTerms mapping found with the specified id");

            //fill entity from product
            productKwTerms.MobileDisplayOrder = model.MobileDisplayOrder;
            productKwTerms.DisplayOrder = model.DisplayOrder;
            productKwTerms.Id = model.Id;

            await _kwTermService.UpdateProductKwTermAsync(productKwTerms);

            return new NullJsonResult();
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_ACCESS_MANAGEKWTERMS)]
        public virtual async Task<IActionResult> ProductDelete(int id)
        { 
            //try to get a product kwTerms with the specified id
            var productKwTerms = await _kwTermService.GetProductKwTermByIdAsync(id)
                ?? throw new ArgumentException("No product kwTerms mapping found with the specified id", nameof(id));

            await _kwTermService.DeleteProductKwTermAsync(productKwTerms);

            return new NullJsonResult();
        }


        #endregion


        #region Categories

        [HttpPost]
        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_ACCESS_MANAGEKWTERMS)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> CustomCategoryAddPopupList(AddCategoryToKwTermSearchModel searchModel)
        { 
            //prepare model
            var model = await _kwTermModelFactory.CustomPrepareAddCategoryToKwTermListModelAsync(searchModel);

            return Json(model);
        }


        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_ACCESS_MANAGEKWTERMS)]
        public virtual async Task<IActionResult> CategoryAddPopup(int kwTermId)
        { 
            //prepare model
            var model = await _kwTermModelFactory.PrepareAddCategoryToKwTermSearchModelAsync(new AddCategoryToKwTermSearchModel());

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_ACCESS_MANAGEKWTERMS)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> CategoryAddPopup(AddCategoryToKwTermModel model)
        { 
            //get selected products
            var selectedCategories = await _categoryService.GetCategoriesByIdsAsync(model.SelectedCategoryIds.ToArray());
            if (selectedCategories.Any())
            {

                var existingCategoryKwTerm = await _kwTermService.GetCategoryKwTermsByKwTermIdAsync(model.KwTermId, showHidden: true);
                foreach (var category in selectedCategories)
                {
                    //whether product category with such parameters already exists

                    if (existingCategoryKwTerm.Where(t => t.KwTermId == model.KwTermId && t.CategoryId == category.Id).Any())
                        continue;

                    //insert the new product category mapping
                    await _kwTermService.InsertCategoryKwTermAsync(new CategoryKwTerm
                    {
                        KwTermId = model.KwTermId,
                        CategoryId = category.Id,
                        DisplayOrder = 1
                      
                    });
                }
            }

            ViewBag.RefreshPage = true;

            return View(new AddCategoryToKwTermSearchModel());
        }

        [HttpPost]
        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_ACCESS_MANAGEKWTERMS)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> CustomCategoryList(KwTermCategorySearchModel searchModel)
        { 
            //try to get a kwTerms with the specified id
            var kwTerms = await _kwTermService.GetKwTermByIdAsync(searchModel.KwTermId)
                ?? throw new ArgumentException("No kwTerms found with the specified id");

            //prepare model
            var model = await _kwTermModelFactory.CustomPrepareKwTermCategoryListModelAsync(searchModel, kwTerms);

            return Json(model);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_ACCESS_MANAGEKWTERMS)]
        public virtual async Task<IActionResult> CategoryUpdate(KwTermCategoryModel model)
        { 
            //try to get a Category kwTerms with the specified id
            var categoryKwTerms = await _kwTermService.GetCategoryKwTermByIdAsync(model.Id)
                ?? throw new ArgumentException("No Category kwTerms mapping found with the specified id");

            categoryKwTerms.DisplayOrder = model.DisplayOrder;
            categoryKwTerms.Id = model.Id;

            await _kwTermService.UpdateCategoryKwTermAsync(categoryKwTerms);

            return new NullJsonResult();
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_ACCESS_MANAGEKWTERMS)]
        public virtual async Task<IActionResult> CategoryDelete(int id)
        { 
            //try to get a Category kwTerms with the specified id
            var categoryKwTerms = await _kwTermService.GetCategoryKwTermByIdAsync(id)
                ?? throw new ArgumentException("No Category kwTerms mapping found with the specified id", nameof(id));

            await _kwTermService.DeleteCategoryKwTermAsync(categoryKwTerms);

            return new NullJsonResult();
        }
        #endregion

    }
}
