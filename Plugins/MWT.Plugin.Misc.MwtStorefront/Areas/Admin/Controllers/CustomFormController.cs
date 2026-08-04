    using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Domain;
using MWT.Nop.Core.Services.Custom;
using MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Factories.Custom;
using MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System.Text.RegularExpressions;

namespace MWT.Plugin.Misc.MwtStorefront.Area.Admin.Controllers
{
    public partial class CustomFormController : BaseAdminController
    {
        #region fields

         
        private readonly ICustomFormModelFactory _customFormModelFactory;
        private readonly ICustomFormService _customFormService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;

        #endregion

        public CustomFormController( 
                                     ICustomFormModelFactory customFormModelFactory,
                                     ICustomFormService customFormService,
                                     INotificationService notificationService,
                                     ILocalizationService localizationService)
        { 
            this._customFormModelFactory = customFormModelFactory;
            this._customFormService = customFormService;
            this._notificationService = notificationService;
            this._localizationService = localizationService;
        }


        #region Custom Form

        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_FORM_VIEW)]
        public virtual async Task<IActionResult> Index()
        {  
            //prepare model
            var model = await _customFormModelFactory.PrepareCustomFormSearchModelAsync(new CustomFormSearchModel());

            return View(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_FORM_VIEW)]
        public virtual async Task<IActionResult> Index(CustomFormSearchModel searchModel)
        { 

            //prepare model
            var model = await _customFormModelFactory.PrepareCustomFormSearchListModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]

        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_FORM_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> CustomFormDelete(int id)
        { 
            //try to get a product tag with the specified id
            var customform = await _customFormService.GetCustomformById(id)
                ?? throw new ArgumentException("No Custom Form found with the specified id");

            await _customFormService.DeleteCustomformAsync(customform);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.CustomForm.Deleted"));

            return RedirectToAction("Index");
        }



        /// <returns>A task that represents the asynchronous operation</returns>
        /// 
        [Route("Admin/CustomForm/Edit/{id}")]
        [Route("Admin/CustomForm/Add")]
        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_FORM_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> Edit(int id)
        { 
            if (id != 0)
            {
                //try to get a product tag with the specified id
                var customForm = await _customFormService.GetCustomformById(id);
                if (customForm == null)
                    return RedirectToAction("Index");
                var model = _customFormModelFactory.PrepareCustomFormModelAsync(null, customForm);

                return View(model);
            }
            return View(new CustomFormModel());
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [Route("Admin/CustomForm/Edit/{id}")]
        [Route("Admin/CustomForm/Add")]
        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_FORM_CREATE_EDIT_DELETE)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> Edit(CustomFormModel model, bool continueEditing)
        { 

            //try to get a product tag with the specified id
            var customForm = new CustomForm();
            if (model.Id != 0)
            {
                customForm = await _customFormService.GetCustomformById(model.Id);
                if (customForm == null)
                    return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                customForm.FormHtml = RemoveFormTag(model.FormHtml);
                customForm.FormName = model.FormName;
                customForm.ShowInPopup = model.ShowInPopup;
                if (model.UseHtml)
                {
                    customForm.ThankYouHtml = model.ThankYouHtml;
                    customForm.ThankYouPageLink = "";
                }
                else
                {
                    customForm.ThankYouHtml = "";
                    customForm.ThankYouPageLink = model.ThankYouPageLink;
                }
                customForm.BccEmailAddresses = model.BccEmailAddresses;
                customForm.SendCustomerNotification = model.SendCustomerNotification;
                customForm.CustomerNotificationEmailBody = model.CustomerNotificationEmailBody;
                customForm.CustomerNotificationSubject = model.CustomerNotificationSubject;
                customForm.Published = model.Published;
                customForm.Body = model.Body;
                customForm.Subject = model.Subject;
                customForm.RenderActions = model.RenderActions;
                if (model.Id != 0)
                    customForm.CreatedOnUtc = DateTime.UtcNow;
                customForm.UpdatedOnUtc = DateTime.UtcNow;
                if (model.Id != 0)
                    await _customFormService.UpdateCustomformAsync(customForm);
                else
                    await _customFormService.InserCustomformAsync(customForm);


                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.CustomForm.Updated"));

                return continueEditing ? RedirectToAction("Edit", new { id = customForm.Id }) : RedirectToAction("Index");
            }

            //prepare model
            if (model.Id != 0)
                model = _customFormModelFactory.PrepareCustomFormModelAsync(model, customForm);
            return View(model);
        }

        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_FORM_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> Leads(int id)
        { 

            var customForm = await _customFormService.GetCustomformById(id);
            if (customForm == null)
                return RedirectToAction("Index");

            //prepare model
            var model = await _customFormModelFactory.PrepareCustomFormEntrySearchModelAsync(new CustomFormSearchModel(), id);
            model.Name = customForm.FormName;
            return View(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_FORM_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> Leads(CustomFormSearchModel searchModel)
        { 
            var customForm = await _customFormService.GetCustomformById(searchModel.Id);
            if (customForm == null)
                return RedirectToAction("Index");
            //prepare model
            var model = await _customFormModelFactory.PrepareCustomFormEntrySearchListModelAsync(searchModel, customForm);

            return Json(model);
        }


        [Route("Admin/CustomForm/Leads/{id}/Details")]
        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_FORM_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> Details(int id)
        { 

            var customFormEntry = await _customFormService.GetCustomFormsEntryById(id);
            if (customFormEntry == null)
                return RedirectToAction("Leads");

            //prepare model
            var model = await _customFormModelFactory.PrepareCustomFormEntryMetasSearchModelAsync(new CustomFormSearchModel(), id);

            return View(model);
        }
        [Route("Admin/CustomForm/Leads/{id}/Details")]
        [HttpPost]
        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_FORM_CREATE_EDIT_DELETE)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> Details(CustomFormSearchModel searchModel)
        { 
            var customFormEntry = await _customFormService.GetCustomFormsEntryById(searchModel.Id);
            if (customFormEntry == null)
                return RedirectToAction("Leads");
            //prepare model
            var model = await _customFormModelFactory.PrepareCustomFormEntryMetasEntrySearchListModelAsync(searchModel);

            return Json(model);
        }

        #endregion

        #region Utilities

        public static string RemoveFormTag(string formHtml)
        {
            if (!string.IsNullOrEmpty(formHtml))
            {
                var regex = new Regex(@"(?i)<form[^>]*>", RegexOptions.Compiled | RegexOptions.Multiline);
                formHtml = regex.Replace(formHtml, "");
                regex = new Regex(@"(?i)</form[^>]*>", RegexOptions.Compiled | RegexOptions.Multiline);
                formHtml = regex.Replace(formHtml, "");
            }
            return formHtml;
        }

        #endregion
    }
}