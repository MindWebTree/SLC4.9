using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Infrastructure;
using MWT.Nop.Core.Services.QA;
using MWT.Nop.Core.Services.Seo;
using MWT.Plugin.Misc.MwtStorefront.Factories.QA;
using MWT.Plugin.Misc.MwtStorefront.Models.Catalog;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Infrastructure;
using Nop.Services.Common;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Controllers;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Models.Catalog;
using System.Linq;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Controllers
{
    [AutoValidateAntiforgeryToken]
    public partial class QuestionAnswerController : BasePublicController
    {
        #region Fields
        private readonly IQuestionAnswerService _questionAnswerService;
        private readonly IQuestionAnswerModelFactory _questionAnswerModelFactory;
        private readonly ICustomUrlRecordService _customurlRecordService;
        private readonly IAclService _aclService;
        private readonly IPermissionService _permissionService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IWorkContext _workContext;
        private readonly ICustomWorkContext  _customWorkContext;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IWebHelper _webHelper;
        private readonly IStoreContext _storeContext;
        private ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        #endregion

        #region Ctor
        public QuestionAnswerController(ICustomUrlRecordService customurlRecordService, IQuestionAnswerService QuestionAnswerService, IQuestionAnswerModelFactory QuestionAnswerModelFactory, IAclService aclService, IPermissionService permissionService, IStoreMappingService storeMappingService, IWorkContext workContext, IGenericAttributeService genericAttributeService, IWebHelper webHelper,
            IStoreContext storeContext, ICustomerActivityService customerActivityService, ILocalizationService localizationService, IHttpContextAccessor httpContextAccessor , ICustomWorkContext customWorkContext)
        {
            _customurlRecordService = customurlRecordService;
            _questionAnswerModelFactory = QuestionAnswerModelFactory;
            _questionAnswerService = QuestionAnswerService;
            _aclService = aclService;
            _permissionService = permissionService;
            _storeMappingService = storeMappingService;
            _workContext = workContext;
            _genericAttributeService = genericAttributeService;
            _webHelper = webHelper;
            _storeContext = storeContext;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _httpContextAccessor = httpContextAccessor;
            _customWorkContext = customWorkContext;
        }


        #endregion
        public virtual async Task<IActionResult> Index(string SeName, CustomCatalogProductsCommand command)
        {
            var urlRecord = await _customurlRecordService.GetByQuestionAnswerSlugAsync(SeName);
         
            if (urlRecord == null)
            {
                return StatusCode(404);
            }
            var QuestionAnswer = await _questionAnswerService.GetQuestionAnswerByIdAsync(urlRecord.EntityId);

            if(!QuestionAnswer.Published)
            {
                return StatusCode(404);
            }
            //'Continue shopping' URL
            await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
                NopCustomerDefaults.LastContinueShoppingPageAttribute,
                _webHelper.GetThisPageUrl(false),
                (await _storeContext.GetCurrentStoreAsync()).Id);

            //display "edit" (manage) link
            if (await _permissionService.AuthorizeAsync(StandardPermission.Security.ACCESS_ADMIN_PANEL) &&
                await _permissionService.AuthorizeAsync(StandardPermission.CustomPermission.CUSTOM_QA_PRODUCTS_CREATE_EDIT_DELETE))
                DisplayEditLink(Url.Action("Edit", "QuestionAnswer", new { id = QuestionAnswer.Id, area = AreaNames.ADMIN }));

   
            //activity log
            await _customerActivityService.InsertActivityAsync("PublicStore.QuestionAnswer",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.QuestionAnswer"), QuestionAnswer.Name), QuestionAnswer);

            //model

   

            (string templateViewPath, string listingViewPath, string filterViewPath, string FilterViewPathForMobile, int pictureSize, bool isHorizontal) =
               await _questionAnswerModelFactory.PrepareQuestionAnswerTemplateViewPathAsync(QuestionAnswer.QuestionAnswerTemplateId);

            var model = await _questionAnswerModelFactory.PrepareQuestionAnswerModelAsync(QuestionAnswer, command, _httpContextAccessor.HttpContext.Request.QueryString.Value, pictureSize);

            if (model.CatalogProductsModel != null)
                model.CatalogProductsModel.QuestionAnswerId = model.Id;

            //template

            model.GridLineViewPath = listingViewPath;
            model.FilterViewPath = filterViewPath;
            model.FilterViewPathForMobile = FilterViewPathForMobile;
            return View(templateViewPath, model);
        }

        [CheckLanguageSeoCode(true)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> GetQuestionAnswerProducts(int QuestionAnswerid, CustomCatalogProductsCommand command, string queryString)
        {
            var QuestionAnswer = await _questionAnswerService.GetQuestionAnswerByIdAsync(QuestionAnswerid);
           
 

            (string templateViewPath, string listingViewPath, string filterViewPath, string FilterViewPathForMobile, int pictureSize, bool isHorizontal) =
                 await _questionAnswerModelFactory.PrepareQuestionAnswerTemplateViewPathAsync(QuestionAnswer.QuestionAnswerTemplateId);

            var model = await _questionAnswerModelFactory.PrepareQuestionAnswerProductsModelAsync(QuestionAnswer, command,
                _httpContextAccessor.HttpContext.Request.QueryString.Value, pictureSize: pictureSize);
            model.QuestionAnswerId = QuestionAnswerid;


            ViewData["openTabs"] = command.openTabs;
            ViewData["popupOpenTabs"] = command.popupOpenTabs;
            if (_customWorkContext.IsMobileDevice())
            {
                return Json(new
                {

                    products = await RenderPartialViewToStringAsync(listingViewPath, model),
                    mobileFilters = await RenderPartialViewToStringAsync(FilterViewPathForMobile, model.SpecificationFilter),
                    filters = string.Join(",", model.SpecificationFilter.Attributes.Select(m => m.Name.ToLower().Trim()).ToArray())
                }); ;
            }
            else
            {
                if (isHorizontal)
                {
                    return Json(new
                    {
                        products = await RenderPartialViewToStringAsync(listingViewPath, model),
                        horiZontalFilters = await RenderPartialViewToStringAsync(filterViewPath, model.SpecificationFilter),
                        popupFilters = await RenderPartialViewToStringAsync("_AjaxFilterSpecsBoxPopup", model.SpecificationFilter),
                        filters = string.Join(",", model.SpecificationFilter.Attributes.Select(m => m.Name.ToLower().Trim()).ToArray()),

                    });
                }
                else
                {
                    return Json(new
                    {
                        products = await RenderPartialViewToStringAsync(listingViewPath, model),
                        sideFilters = await RenderPartialViewToStringAsync(filterViewPath, model.SpecificationFilter),
                        filters = string.Join(",", model.SpecificationFilter.Attributes.Select(m => m.Name.ToLower().Trim()).ToArray()),
                    });
                }
            }

        }
    }
}
