using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Domain.QA;
using MWT.Nop.Core.Services.Customers;
using MWT.Nop.Core.Services.QA;
using MWT.Plugin.Misc.MwtStorefront.Area.Admin.Factories.QA;
using MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.QA;
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
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;

namespace MWT.Plugin.Misc.MwtStorefront.Area.Admin.Controllers.QA
{
    public class QuestionAnswerController : BaseAdminController
    {


        private readonly IAclService _aclService;
        private readonly IQuestionAnswerModelFactory _QuestionAnswerModelFactory;
        private readonly IQuestionAnswerService _questionAnswerService;
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

        #region Ctor

        public QuestionAnswerController(IAclService aclService,
            IQuestionAnswerModelFactory QuestionAnswerModelFactory,
           IQuestionAnswerService QuestionAnswerService,
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
                ICategoryModelFactory categoryModelFactory
                )
        {
            _aclService = aclService;
            _QuestionAnswerModelFactory = QuestionAnswerModelFactory;
            _questionAnswerService = QuestionAnswerService;
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
        }

        #endregion

        #region Utilities

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task UpdateLocalesAsync(QuestionAnswer QuestionAnswer, QuestionAnswerModel model)
        {
            foreach (var localized in model.Locales)
            {
                await _localizedEntityService.SaveLocalizedValueAsync(QuestionAnswer,
                    x => x.Name,
                    localized.Name,
                    localized.LanguageId);

                await _localizedEntityService.SaveLocalizedValueAsync(QuestionAnswer,
                    x => x.Description,
                    localized.Description,
                    localized.LanguageId);

                await _localizedEntityService.SaveLocalizedValueAsync(QuestionAnswer,
                    x => x.MetaKeywords,
                    localized.MetaKeywords,
                    localized.LanguageId);

                await _localizedEntityService.SaveLocalizedValueAsync(QuestionAnswer,
                    x => x.MetaDescription,
                    localized.MetaDescription,
                    localized.LanguageId);

                await _localizedEntityService.SaveLocalizedValueAsync(QuestionAnswer,
                    x => x.MetaTitle,
                    localized.MetaTitle,
                    localized.LanguageId);

                //search engine name
                var seName = await _urlRecordService.ValidateSeNameAsync(QuestionAnswer, localized.SeName, localized.Name, false);
                await _urlRecordService.SaveSlugAsync(QuestionAnswer, seName, localized.LanguageId);
            }
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task UpdatePictureSeoNamesAsync(QuestionAnswer QuestionAnswer)
        {
            var picture = await _pictureService.GetPictureByIdAsync(QuestionAnswer.PictureId);
            if (picture != null)
                await _pictureService.SetSeoFilenameAsync(picture.Id, await _pictureService.GetPictureSeNameAsync(QuestionAnswer.Name));
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task SaveQuestionAnswersAclAsync(QuestionAnswer QuestionAnswer, QuestionAnswerModel model)
        {
            QuestionAnswer.SubjectToAcl = model.SelectedCustomerRoleIds.Any();
            await _questionAnswerService.UpdateQuestionAnswerAsync(QuestionAnswer);

            var existingAclRecords = await _aclService.GetAclRecordsAsync(QuestionAnswer);
            var allCustomerRoles = await _customerService.GetAllCustomerRolesAsync(true);
            foreach (var customerRole in allCustomerRoles)
            {
                if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                {
                    //new role
                    if (existingAclRecords.Count(acl => acl.CustomerRoleId == customerRole.Id) == 0)
                        await _aclService.InsertAclRecordAsync(QuestionAnswer, customerRole.Id);
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
        protected virtual async Task SaveStoreMappingsAsync(QuestionAnswer QuestionAnswers, QuestionAnswerModel model)
        {
            QuestionAnswers.LimitedToStores = model.SelectedStoreIds.Any();
            await _questionAnswerService.UpdateQuestionAnswerAsync(QuestionAnswers);

            var existingStoreMappings = await _storeMappingService.GetStoreMappingsAsync(QuestionAnswers);
            var allStores = await _storeService.GetAllStoresAsync();
            foreach (var store in allStores)
            {
                if (model.SelectedStoreIds.Contains(store.Id))
                {
                    //new store
                    if (existingStoreMappings.Count(sm => sm.StoreId == store.Id) == 0)
                        await _storeMappingService.InsertStoreMappingAsync(QuestionAnswers, store.Id);
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

        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_QA_VIEW)]
        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_QA_VIEW)]
        public virtual async Task<IActionResult> List()
        {


            //prepare model
            var model = await _QuestionAnswerModelFactory.PrepareQuestionAnswerSearchModelAsync(new QuestionAnswerSearchModel());

            return View(model);
        }


        [HttpPost]
        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_QA_VIEW)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> List(QuestionAnswerSearchModel searchModel)
        {

            try
            {

                //prepare model
                var model = await _QuestionAnswerModelFactory.PrepareQuestionAnswerListModelAsync(searchModel);

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
        /// 

        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_QA_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> Create()
        {
            var model = await _QuestionAnswerModelFactory.PrepareQuestionAnswerModelAsync(new QuestionAnswerModel(), null);

            return View(model);
        }


        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_QA_CREATE_EDIT_DELETE)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> Create(QuestionAnswerModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var QuestionAnswer = new QuestionAnswer();
                    QuestionAnswer.Name = model.Name;
                    QuestionAnswer.QuestionAnswerTemplateId = model.QuestionAnswerTemplateId;
                    QuestionAnswer.MetaTitle = model.MetaTitle;
                    QuestionAnswer.MetaDescription = model.MetaDescription;
                    QuestionAnswer.Description = model.Description;
                    QuestionAnswer.PageSize = model.PageSize;
                    QuestionAnswer.CreatedOnUtc = DateTime.UtcNow;
                    QuestionAnswer.UpdatedOnUtc = DateTime.UtcNow;
                    QuestionAnswer.PageSizeOptions = model.PageSizeOptions;
                    QuestionAnswer.PictureId = model.PictureId;
                    QuestionAnswer.Published = model.Published;
                    QuestionAnswer.AllowCustomersToSelectPageSize = model.AllowCustomersToSelectPageSize;
                    QuestionAnswer.Deleted = model.Deleted;
                    QuestionAnswer.DisplayOrder = model.DisplayOrder;
                    QuestionAnswer.Id = model.Id;
                    QuestionAnswer.MetaKeywords = model.MetaKeywords;
                    QuestionAnswer.EnableInfiniteScroll = model.EnableInfiniteScroll;
                    QuestionAnswer.ListingLink = model.ListingLink;
                    QuestionAnswer.PublishedOn = model.PublishedOn;
                    QuestionAnswer.BackgroundColor = model.BackgroundColor;
                    QuestionAnswer.AuthorName = model.AuthorName;
                    QuestionAnswer.AuthorPictureId = model.AuthorPictureId;
                    QuestionAnswer.ListingTitle = model.ListingTitle;
                    await _questionAnswerService.InsertQuestionAnswerAsync(QuestionAnswer);

                    //search engine name
                    model.SeName = await _urlRecordService.ValidateSeNameAsync(QuestionAnswer, model.SeName, QuestionAnswer.Name, true);
                    await _urlRecordService.SaveSlugAsync(QuestionAnswer, model.SeName, 0);

                    //locales
                    await UpdateLocalesAsync(QuestionAnswer, model);

                    await _questionAnswerService.UpdateQuestionAnswerAsync(QuestionAnswer);

                    //update picture seo file name
                    await UpdatePictureSeoNamesAsync(QuestionAnswer);

                    //ACL (customer roles)
                    await SaveQuestionAnswersAclAsync(QuestionAnswer, model);

                    //stores
                    await SaveStoreMappingsAsync(QuestionAnswer, model);

                    //activity log
                    await _customerActivityService.InsertActivityAsync("AddNewQuestionAnswers",
                        string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewQuestionAnswers"), QuestionAnswer.Name), QuestionAnswer);

                    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.QuestionAnswers.Added"));

                    if (!continueEditing)
                        return RedirectToAction("List");

                    return RedirectToAction("Edit", new { id = QuestionAnswer.Id });
                }
                catch (Exception ex)
                {

                    throw ex;
                }
            }

            //prepare model
            model = await _QuestionAnswerModelFactory.PrepareQuestionAnswerModelAsync(model, null, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        /// 
        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_QA_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> Edit(int id)
        {
            var QuestionAnswers = await _questionAnswerService.GetQuestionAnswerByIdAsync(id);
            if (QuestionAnswers == null || QuestionAnswers.Deleted)
                return RedirectToAction("List");

            //prepare model
            var model = await _QuestionAnswerModelFactory.PrepareQuestionAnswerModelAsync(null, QuestionAnswers);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_QA_CREATE_EDIT_DELETE)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> Edit(QuestionAnswerModel model, bool continueEditing)
        {
            var QuestionAnswer = await _questionAnswerService.GetQuestionAnswerByIdAsync(model.Id);
            if (QuestionAnswer == null || QuestionAnswer.Deleted)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                var prevPictureId = QuestionAnswer.PictureId;

                //if parent QuestionAnswers changes, we need to clear cache for previous parent QuestionAnswers


                QuestionAnswer.Name = model.Name;
                QuestionAnswer.QuestionAnswerTemplateId = model.QuestionAnswerTemplateId;
                QuestionAnswer.MetaTitle = model.MetaTitle;
                QuestionAnswer.MetaDescription = model.MetaDescription;
                QuestionAnswer.Description = model.Description;
                QuestionAnswer.PageSize = model.PageSize;
                QuestionAnswer.CreatedOnUtc = DateTime.UtcNow;
                QuestionAnswer.UpdatedOnUtc = DateTime.UtcNow;
                QuestionAnswer.PageSizeOptions = model.PageSizeOptions;
                QuestionAnswer.PictureId = model.PictureId;
                QuestionAnswer.Published = model.Published;
                QuestionAnswer.AllowCustomersToSelectPageSize = model.AllowCustomersToSelectPageSize;
                QuestionAnswer.Deleted = model.Deleted;
                QuestionAnswer.DisplayOrder = model.DisplayOrder;
                QuestionAnswer.Id = model.Id;
                QuestionAnswer.MetaKeywords = model.MetaKeywords;
                QuestionAnswer.EnableInfiniteScroll = model.EnableInfiniteScroll;
                QuestionAnswer.ListingLink = model.ListingLink;
                QuestionAnswer.ListingTitle = model.ListingTitle;
                QuestionAnswer.PublishedOn = model.PublishedOn;
                QuestionAnswer.BackgroundColor = model.BackgroundColor;
                QuestionAnswer.AuthorName = model.AuthorName;
                QuestionAnswer.AuthorPictureId = model.AuthorPictureId;
                await _questionAnswerService.UpdateQuestionAnswerAsync(QuestionAnswer);

                //search engine name
                model.SeName = await _urlRecordService.ValidateSeNameAsync(QuestionAnswer, model.SeName, QuestionAnswer.Name, true);
                await _urlRecordService.SaveSlugAsync(QuestionAnswer, model.SeName, 0);

                //locales
                await UpdateLocalesAsync(QuestionAnswer, model);

                await _questionAnswerService.UpdateQuestionAnswerAsync(QuestionAnswer);

                //delete an old picture (if deleted or updated)
                if (prevPictureId > 0 && prevPictureId != QuestionAnswer.PictureId)
                {
                    var prevPicture = await _pictureService.GetPictureByIdAsync(prevPictureId);
                    if (prevPicture != null)
                        await _pictureService.DeletePictureAsync(prevPicture);
                }

                //update picture seo file name
                await UpdatePictureSeoNamesAsync(QuestionAnswer);

                //ACL
                await SaveQuestionAnswersAclAsync(QuestionAnswer, model);

                //stores
                await SaveStoreMappingsAsync(QuestionAnswer, model);

                //activity log
                await _customerActivityService.InsertActivityAsync("EditQuestionAnswers",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditQuestionAnswers"), QuestionAnswer.Name), QuestionAnswer);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.QuestionAnswers.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = QuestionAnswer.Id });
            }

            //prepare model
            model = await _QuestionAnswerModelFactory.PrepareQuestionAnswerModelAsync(model, QuestionAnswer, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_QA_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            //try to get a QuestionAnswers with the specified id
            var QuestionAnswers = await _questionAnswerService.GetQuestionAnswerByIdAsync(id);
            if (QuestionAnswers == null)
                return RedirectToAction("List");

            await _questionAnswerService.DeleteQuestionAnswerAsync(QuestionAnswers);

            //activity log
            await _customerActivityService.InsertActivityAsync("DeleteQuestionAnswers",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteQuestionAnswers"), QuestionAnswers.Name), QuestionAnswers);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.QuestionAnswers.Deleted"));

            return RedirectToAction("List");
        }

        [HttpPost]
        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_QA_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (selectedIds != null)
            {
                await _questionAnswerService.DeleteQuestionAnswerAsync(await (await _questionAnswerService.GetQuestionAnswersByIdsAsync(selectedIds.ToArray())).WhereAwait(async p => await _workContext.GetCurrentVendorAsync() == null).ToListAsync());
            }

            return Json(new { Result = true });
        }

        #endregion

        #region Products

        /// <returns>A task that represents the asynchronous operation</returns>
        /// 

        [HttpPost]
        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_QA_PRODUCTS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ProductAddPopupList(AddProductToQuestionAnswerSearchModel searchModel)
        {
            var model = await _QuestionAnswerModelFactory.PrepareAddProductToQuestionAnswerListModelAsync(searchModel);

            return Json(model);
        }

        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_QA_PRODUCTS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ProductAddPopup(int QuestionAnswerId)
        {
            //prepare model
            var model = await _QuestionAnswerModelFactory.PrepareAddProductToQuestionAnswerSearchModelAsync(new AddProductToQuestionAnswerSearchModel());

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_QA_PRODUCTS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ProductAddPopup(AddProductToQuestionAnswerModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_SETTINGS))
                return AccessDeniedView();

            //get selected products
            var selectedProducts = await _productService.GetProductsByIdsAsync(model.SelectedProductIds.ToArray());
            if (selectedProducts.Any())
            {

                var existingProductQuestionAnswer = await _questionAnswerService.GetProductQuestionAnswersByQuestionAnswerIdAsync(model.QuestionAnswerId, showHidden: true);
                foreach (var product in selectedProducts)
                {
                    //whether product category with such parameters already exists
                    if (_questionAnswerService.FindProductQuestionAnswer(existingProductQuestionAnswer, product.Id, model.QuestionAnswerId) != null)
                        continue;

                    //insert the new product category mapping
                    await _questionAnswerService.InsertProductQuestionAnswerAsync(new ProductQuestionAnswer
                    {
                        QuestionAnswerId = model.QuestionAnswerId,
                        ProductId = product.Id,
                        DisplayOrder = 1,
                        MobileDisplayOrder = 1
                    });
                }
            }

            ViewBag.RefreshPage = true;

            return View(new AddProductToQuestionAnswerSearchModel());
        }

        [HttpPost]
        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_QA_PRODUCTS_VIEW)]
        public virtual async Task<IActionResult> ProductList(QuestionAnswerProductSearchModel searchModel)
        {


            //try to get a QuestionAnswers with the specified id
            var QuestionAnswers = await _questionAnswerService.GetQuestionAnswerByIdAsync(searchModel.QuestionAnswerId)
                ?? throw new ArgumentException("No QuestionAnswers found with the specified id");

            //prepare model
            var model = await _QuestionAnswerModelFactory.PrepareQuestionAnswerProductListModelAsync(searchModel, QuestionAnswers);

            return Json(model);
        }


        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_QA_PRODUCTS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ProductUpdate(QuestionAnswerProductModel model)
        {

            //try to get a product QuestionAnswers with the specified id
            var productQuestionAnswers = await _questionAnswerService.GetProductQuestionAnswerByIdAsync(model.Id)
                ?? throw new ArgumentException("No product QuestionAnswers mapping found with the specified id");

            //fill entity from product
            productQuestionAnswers.MobileDisplayOrder = model.MobileDisplayOrder;
            productQuestionAnswers.DisplayOrder = model.DisplayOrder;
            productQuestionAnswers.Id = model.Id;

            await _questionAnswerService.UpdateProductQuestionAnswerAsync(productQuestionAnswers);

            return new NullJsonResult();
        }

        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_QA_PRODUCTS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ProductDelete(int id)
        {

            //try to get a product QuestionAnswers with the specified id
            var productQuestionAnswers = await _questionAnswerService.GetProductQuestionAnswerByIdAsync(id)
                ?? throw new ArgumentException("No product QuestionAnswers mapping found with the specified id", nameof(id));

            await _questionAnswerService.DeleteProductQuestionAnswerAsync(productQuestionAnswers);

            return new NullJsonResult();
        }


        #endregion

        #region Related

        [HttpPost]
        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_QA_VIEW)]
        public virtual async Task<IActionResult> RelatedQuestionAnswerList(RelatedQuestionAnswerSearchModel searchModel)
        {
            var questionAnswer = await _questionAnswerService.GetQuestionAnswerByIdAsync(searchModel.QuestionAnswerId)
                ?? throw new ArgumentException("No question Answer found with the specified id");



            //prepare model
            var model = await _QuestionAnswerModelFactory.PrepareRelatedQuestionAnswerListModelAsync(searchModel, questionAnswer);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_QA_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> RelatedQuestionAnswerUpdate(RelatedQuestionAnswerModel model)
        {
            //try to get a Collection product with the specified id
            var relatedQuestionAnswer = await _questionAnswerService.GetRelatedQuestionAnswerByIdAsync(model.Id)
                ?? throw new ArgumentException("No Question answer found with the specified id");

            //a vendor should have access only to his products

            var questionAnswer = await _questionAnswerService.GetQuestionAnswerByIdAsync(relatedQuestionAnswer.QuestionAnswerId1);
            relatedQuestionAnswer.DisplayOrder = model.DisplayOrder;
            await _questionAnswerService.UpdateRelatedQuestionAnswerAsync(relatedQuestionAnswer);

            return new NullJsonResult();
        }

        [HttpPost]
        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_QA_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> RelatedQuestionAnswerDelete(int id)
        { //try to get a Collection product with the specified id
            var relatedQuestionAnswer = await _questionAnswerService.GetRelatedQuestionAnswerByIdAsync(id)
                ?? throw new ArgumentException("No Question answer found with the specified id");
            await _questionAnswerService.DeleteRelatedQuestionAnswerAsync(relatedQuestionAnswer);

            return new NullJsonResult();
        }

        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_QA_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> RelatedQuestionAnswerAddPopup(int questionAnswerId)
        {
            var model = await _QuestionAnswerModelFactory.PrepareAddRelatedQuestionAnswerSearchModelAsync(new AddRelatedQuestionAnswerSearchModel());

            return View(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_QA_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> RelatedQuestionAnswerAddPopupList(AddRelatedQuestionAnswerSearchModel searchModel)
        {
            //prepare model
            var model = await _QuestionAnswerModelFactory.PrepareAddRelatedQuestionAnswerListModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_QA_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> RelatedQuestionAnswerAddPopup(AddRelatedQuestionAnswerModel model)
        {
            var selectedQuestionAnswers = await _questionAnswerService.GetQuestionAnswersByIdsAsync(model.SelectedQuestionAnswerIds.ToArray());
            if (selectedQuestionAnswers.Any())
            {
                var existingQuestionAnswers = await _questionAnswerService.GetRelatedQuestionAnswersByQuestionAnswerId1Async(model.QuestionAnswerId, showHidden: true);
                foreach (var questionAnswer in selectedQuestionAnswers)
                {


                    if (_questionAnswerService.FindRelatedQuestionAnswer(existingQuestionAnswers, model.QuestionAnswerId, questionAnswer.Id) != null)
                        continue;

                    await _questionAnswerService.InsertRelatedQuestionAnswerAsync(new RelatedQuestionAnswer
                    {
                        QuestionAnswerId1 = model.QuestionAnswerId,
                        QuestionAnswerId2 = questionAnswer.Id,
                        DisplayOrder = 1
                    });
                }
            }

            ViewBag.RefreshPage = true;

            return View(new AddRelatedQuestionAnswerSearchModel());
        }

        #endregion

    }
}
