using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Domain.Custom.LandingPage_Management;
using MWT.Nop.Core.Services.Customers;
using MWT.Nop.Core.Services.LandingPage_Management;
using MWT.Plugin.Misc.MwtStorefront.Area.Admin.Factories;
using MWT.Plugin.Misc.MwtStorefront.Area.Admin.Models.Catalog.LandingPage_Management;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Mvc.Filters;

namespace MWT.Plugin.Misc.MwtStorefront.Area.Admin.Controllers.LandingPage_Management
{
    public class LandingPageController : BaseAdminController
    {


        private readonly IAclService _aclService;
        private readonly ILandingPageModelFactory _landingPageModelFactory;
        private readonly ILandingPageService _landingPageService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IStoreService _storeService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWorkContext _workContext;

        #region Ctor

        public LandingPageController(IAclService aclService,
            ILandingPageModelFactory LandingPageModelFactory,
           ILandingPageService LandingPageService,
                ICustomerActivityService customerActivityService,
                ICustomerService customerService,
                ILocalizationService localizationService,
                ILocalizedEntityService localizedEntityService,
                INotificationService notificationService,
                IPermissionService permissionService,
                IPictureService pictureService,
                IStoreMappingService storeMappingService,
                IStoreService storeService,
                IUrlRecordService urlRecordService,
                IWorkContext workContext
                )
        {
            _aclService = aclService;
            _landingPageModelFactory = LandingPageModelFactory;
            _landingPageService = LandingPageService;
            _customerActivityService = customerActivityService;
            _customerService = customerService;
            _localizationService = localizationService;
            _localizedEntityService = localizedEntityService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _pictureService = pictureService;
            _storeMappingService = storeMappingService;
            _storeService = storeService;
            _urlRecordService = urlRecordService;
            _workContext = workContext;
        }

        #endregion

        #region Utilities
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task UpdateLocalesAsync(LandingPage landingPage, LandingPageModel model)
        {
            foreach (var localized in model.Locales)
            {
                await _localizedEntityService.SaveLocalizedValueAsync(landingPage,
                    x => x.Name,
                    localized.Name,
                    localized.LanguageId);

                await _localizedEntityService.SaveLocalizedValueAsync(landingPage,
                    x => x.Description,
                    localized.Description,
                    localized.LanguageId);

                await _localizedEntityService.SaveLocalizedValueAsync(landingPage,
                    x => x.MetaKeywords,
                    localized.MetaKeywords,
                    localized.LanguageId);

                await _localizedEntityService.SaveLocalizedValueAsync(landingPage,
                    x => x.MetaDescription,
                    localized.MetaDescription,
                localized.LanguageId);


                await _localizedEntityService.SaveLocalizedValueAsync(landingPage,
                    x => x.MetaTitle,
                    localized.MetaTitle,
                    localized.LanguageId);
                //search engine name
                var seName = await _urlRecordService.ValidateSeNameAsync(landingPage, localized.SeName, localized.Name, false);
                await _urlRecordService.SaveSlugAsync(landingPage, seName, localized.LanguageId);
            }
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task UpdatePictureSeoNamesAsync(LandingPage landingPage)
        {
            var picture = await _pictureService.GetPictureByIdAsync(landingPage.PictureId);
            if (picture != null)
                await _pictureService.SetSeoFilenameAsync(picture.Id, await _pictureService.GetPictureSeNameAsync(landingPage.Name));
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task SaveLandingPageAclAsync(LandingPage landingPage, LandingPageModel model)
        {
            landingPage.SubjectToAcl = model.SelectedCustomerRoleIds.Any();
            await _landingPageService.UpdateLandingPageAsync(landingPage);

            var existingAclRecords = await _aclService.GetAclRecordsAsync(landingPage);
            var allCustomerRoles = await _customerService.GetAllCustomerRolesAsync(true);
            foreach (var customerRole in allCustomerRoles)
            {
                if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                {
                    //new role
                    if (existingAclRecords.Count(acl => acl.CustomerRoleId == customerRole.Id) == 0)
                        await _aclService.InsertAclRecordAsync(landingPage, customerRole.Id);
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
        protected virtual async Task SaveStoreMappingsAsync(LandingPage landingPage, LandingPageModel model)
        {
            landingPage.LimitedToStores = model.SelectedStoreIds.Any();
            await _landingPageService.UpdateLandingPageAsync(landingPage);

            var existingStoreMappings = await _storeMappingService.GetStoreMappingsAsync(landingPage);
            var allStores = await _storeService.GetAllStoresAsync();
            foreach (var store in allStores)
            {
                if (model.SelectedStoreIds.Contains(store.Id))
                {
                    //new store
                    if (existingStoreMappings.Count(sm => sm.StoreId == store.Id) == 0)
                        await _storeMappingService.InsertStoreMappingAsync(landingPage, store.Id);
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
        [CheckPermission(StandardPermission.ContentManagement.TOPICS_VIEW)]
        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        [CheckPermission(StandardPermission.ContentManagement.TOPICS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> List()
        {
            //prepare model
            var model = await _landingPageModelFactory.PrepareLandingPageSearchModelAsync(new LandingPageSearchModel());

            return View(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.ContentManagement.TOPICS_CREATE_EDIT_DELETE)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> List(LandingPageSearchModel searchModel)
        { 
            try
            {

                //prepare model
                var model = await _landingPageModelFactory.PrepareLandingPageListModelAsync(searchModel);

                return Json(model);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Create / Edit / Delete

        /// <returns>A task that represents the asynchronous operation</returns>
        [CheckPermission(StandardPermission.ContentManagement.TOPICS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> Create()
        {  
            //prepare model
            var model = await _landingPageModelFactory.PrepareLandingPageModelAsync(new LandingPageModel(), null);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [CheckPermission(StandardPermission.ContentManagement.TOPICS_CREATE_EDIT_DELETE)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> Create(LandingPageModel model, bool continueEditing)
        {  

            if (ModelState.IsValid)
            {
                try
                {
                    var landingPage = model.ToEntity<LandingPage>();
                    landingPage.CreatedOnUtc = DateTime.UtcNow;
                    landingPage.UpdatedOnUtc = DateTime.UtcNow;
                    await _landingPageService.InsertLandingPageAsync(landingPage);

                    //search engine name
                    model.SeName = await _urlRecordService.ValidateSeNameAsync(landingPage, model.SeName, landingPage.Name, true);
                    await _urlRecordService.SaveSlugAsync(landingPage, model.SeName, 0);

                    //locales
                    await UpdateLocalesAsync(landingPage, model);

                    await _landingPageService.UpdateLandingPageAsync(landingPage);

                    //update picture seo file name
                    await UpdatePictureSeoNamesAsync(landingPage);

                    //ACL (customer roles)
                    await SaveLandingPageAclAsync(landingPage, model);

                    //stores
                    await SaveStoreMappingsAsync(landingPage, model);

                    //activity log
                    await _customerActivityService.InsertActivityAsync("AddNewLandingPages",
                        string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewLandingPages"), landingPage.Name), landingPage);

                    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.LandingPages.Added"));

                    if (!continueEditing)
                        return RedirectToAction("List");

                    return RedirectToAction("Edit", new { id = landingPage.Id });
                }
                catch (Exception ex)
                {

                    throw ex;
                }
            }

            //prepare model
            model = await _landingPageModelFactory.PrepareLandingPageModelAsync(model, null, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }



        [CheckPermission(StandardPermission.ContentManagement.TOPICS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> Edit(int id)
        { 

            //try to get a LandingPages with the specified id
            var LandingPages = await _landingPageService.GetLandingPageByIdAsync(id);
            if (LandingPages == null || LandingPages.Deleted)
                return RedirectToAction("List");

            //prepare model
            var model = await _landingPageModelFactory.PrepareLandingPageModelAsync(null, LandingPages);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [CheckPermission(StandardPermission.ContentManagement.TOPICS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> Edit(LandingPageModel model, bool continueEditing)
        { 

            //try to get a LandingPages with the specified id
            var landingPage = await _landingPageService.GetLandingPageByIdAsync(model.Id);
            if (landingPage == null || landingPage.Deleted)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                var prevPictureId = landingPage.PictureId;
                landingPage = model.ToEntity(landingPage);
                landingPage.UpdatedOnUtc = DateTime.UtcNow;
                await _landingPageService.UpdateLandingPageAsync(landingPage);

                //locales
                await UpdateLocalesAsync(landingPage, model);

                await _landingPageService.UpdateLandingPageAsync(landingPage);

                //search engine name
                model.SeName = await _urlRecordService.ValidateSeNameAsync(landingPage, model.SeName, landingPage.Name, true);
                await _urlRecordService.SaveSlugAsync(landingPage, model.SeName, 0);

                //delete an old picture (if deleted or updated)
                if (prevPictureId > 0 && prevPictureId != landingPage.PictureId)
                {
                    var prevPicture = await _pictureService.GetPictureByIdAsync(prevPictureId);
                    if (prevPicture != null)
                        await _pictureService.DeletePictureAsync(prevPicture);
                }

                //update picture seo file name
                await UpdatePictureSeoNamesAsync(landingPage);

                //ACL
                await SaveLandingPageAclAsync(landingPage, model);

                //stores
                await SaveStoreMappingsAsync(landingPage, model);

                //activity log
                await _customerActivityService.InsertActivityAsync("EditLandingPages",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditLandingPages"), landingPage.Name), landingPage);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.LandingPages.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = landingPage.Id });
            }

            //prepare model
            model = await _landingPageModelFactory.PrepareLandingPageModelAsync(model, landingPage, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.ContentManagement.TOPICS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> Delete(int id)
        { 
            //try to get a LandingPages with the specified id
            var LandingPages = await _landingPageService.GetLandingPageByIdAsync(id);
            if (LandingPages == null)
                return RedirectToAction("List");

            await _landingPageService.DeleteLandingPageAsync(LandingPages);

            //activity log
            await _customerActivityService.InsertActivityAsync("DeleteLandingPages",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteLandingPages"), LandingPages.Name), LandingPages);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.LandingPages.Deleted"));

            return RedirectToAction("List");
        }

        [HttpPost]
        [CheckPermission(StandardPermission.ContentManagement.TOPICS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        { 
            if (selectedIds != null)
            {
                await _landingPageService.DeleteLandingPageAsync(await (await _landingPageService.GetLandingPagesByIdsAsync(selectedIds.ToArray())).WhereAwait(async p => await _workContext.GetCurrentVendorAsync() == null).ToListAsync());
            }

            return Json(new { Result = true });
        }
        #endregion
    }
}
