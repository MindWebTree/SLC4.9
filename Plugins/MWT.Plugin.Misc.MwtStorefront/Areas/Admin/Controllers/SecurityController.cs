using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Domain.Security;
using MWT.Nop.Core.Services.Security;
using MWT.Plugin.Misc.MwtStorefront.Area.Admin.Factories;
using MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.Security;
using Nop.Core.Infrastructure;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Controllers
{
    public partial class SecurityController : BaseAdminController
    {
        #region List Create Delete
        private readonly IPermissionExtendedService _permissionService;
        private readonly INotificationService _notificationService;
        private readonly ISecurityModelExtendedFactory _securityModelFactory;
        private readonly ILocalizationService _localizationService;
        public SecurityController(IPermissionExtendedService permissionService, INotificationService notificationservice,
            ISecurityModelExtendedFactory securityModelFactory, ILocalizationService localizationService)
        {
            _notificationService = notificationservice;
                _permissionService = permissionService;
            _securityModelFactory = securityModelFactory;
            _localizationService = localizationService;
        } 

        /// <returns>A task that represents the asynchronous operation</returns>
        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_ACCESS_MANAGEACL)]
        public virtual async Task<IActionResult> CategoryPermissionList()
        {  
            //prepare model
            var searchModel = new CategoryPermissionSearchModel();
            searchModel.SetGridPageSize();
            return View(searchModel);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_ACCESS_MANAGEACL)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> CategoryPermissionList(CategoryPermissionSearchModel searchModel)
        { 
            try
            {

                //prepare model
                var model = await _securityModelFactory.PrepareCategoryPermissionListModelAsync(searchModel);

                return Json(model);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_ACCESS_MANAGEACL)]
        public virtual async Task<IActionResult> CategoryPermissionDelete(int id)
        { 

            //try to get a Category QuestionAnswers with the specified id
            var categoryUserMappings = await _permissionService.GetCategoryPermissionByIdAsync(id)
                ?? throw new ArgumentException("No Category User Mapping found with the specified id", nameof(id));

            await _permissionService.DeleteCategoryPermissionAsync(categoryUserMappings);

            return new NullJsonResult();
        }

        #endregion

        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_ACCESS_MANAGEACL)]
        public virtual async Task<IActionResult> CategoryPermissionAddPopup()
        { 
            var model = new AddCategoryToCategoryPermissionModel();
            var _baseAdminModelFactory = EngineContext.Current.Resolve<IBaseAdminModelFactory>();
            await _baseAdminModelFactory.PrepareCategoriesAsync(model.AvailableCategories, false, null);
            await _securityModelFactory.PrepareCustomersAsync(model.AvailableCustomerIds);
            return View(model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_ACCESS_MANAGEACL)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> CategoryPermissionAddPopup(AddCategoryToCategoryPermissionModel model)
        { 
            var hasPermission = await _permissionService.HasCategoryPermission(model.SelectedCategoryId, model.SelectedCustomerId);
            if (hasPermission)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.CategoryPermission.UserAlreadyHasPermission"));
            }
            else
            {
                var categoryUserMapping = new CategoryUserMapping();
                categoryUserMapping.CategoryId = model.SelectedCategoryId;
                categoryUserMapping.UserId = model.SelectedCustomerId;
                await _permissionService.InsertCategoryPermissionAsync(categoryUserMapping);
                ViewBag.RefreshPage = true;
            }
            var _baseAdminModelFactory = EngineContext.Current.Resolve<IBaseAdminModelFactory>();
            await _baseAdminModelFactory.PrepareCategoriesAsync(model.AvailableCategories, false, null);
            await _securityModelFactory.PrepareCustomersAsync(model.AvailableCustomerIds);
            return View(model);
        }
    }
}