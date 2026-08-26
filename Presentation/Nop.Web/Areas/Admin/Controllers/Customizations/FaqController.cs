using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Domain.FAQModule;
using MWT.Nop.Core.Service.FAQModule;
using Nop.Services.Catalog;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.FAQModule;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;

namespace Nop.Web.Areas.Admin.Controllers.Customizations
{
    public partial class FaqController : BaseAdminController
    {

        #region Fields

        private readonly IFaqModelFactory _faqModelFactory;
        private readonly IFaqService _faqService;
        private readonly ICategoryService _categoryService;
        private readonly IPermissionService _permissionService;
        private readonly IProductService _productService;

        #endregion

        #region Ctor

        public FaqController(IFaqService faqService,
            ICategoryService categoryService,
            IFaqModelFactory faqModelFactory,
            IPermissionService permissionService,
            IProductService productService)
        {
            _faqService = faqService;
            _categoryService = categoryService;
            _faqModelFactory = faqModelFactory;
            _permissionService = permissionService;
            _productService = productService;
        }

        #endregion

        #region Method

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> FaqList(FaqSearchModel searchModel)
        {


            //try to get a product and Category with the specified id
            if (searchModel.EntityType == "Product")
            {
                if (!await _permissionService.AuthorizeAsync(StandardPermission.CustomPermission.CUSTOM_FAQ_PRODUCT_ACCESS))
                    return await AccessDeniedJsonAsync();
                var product = await _productService.GetProductByIdAsync(searchModel.EntityId)
                    ?? throw new ArgumentException("No product found with the specified id");
            }
            else if (searchModel.EntityType == "Category")
            {
                if (!await _permissionService.AuthorizeAsync(StandardPermission.CustomPermission.CUSTOM_FAQ_CATEGORY_ACCESS))
                    return await AccessDeniedJsonAsync();
                var category = await _categoryService.GetCategoryByIdAsync(searchModel.EntityId)
                    ?? throw new ArgumentException("No Category found with the specified id");
            }
            var model = await _faqModelFactory.PrepareFaqListModelAsync(searchModel);
            return Json(model);
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> FaqUpdate(FaqModel model)
        {
            if (model.EntityType == "Product")
            {
                if (!await _permissionService.AuthorizeAsync(StandardPermission.CustomPermission.CUSTOM_FAQ_PRODUCT_ACCESS))
                    return await AccessDeniedJsonAsync();
            }
            else if (model.EntityType == "Category")
            {
                if (!await _permissionService.AuthorizeAsync(StandardPermission.CustomPermission.CUSTOM_FAQ_CATEGORY_ACCESS))
                    return await AccessDeniedJsonAsync();
            }

            //try to get a Collection Faq with the specified id
            var faq = await _faqService.GetById(model.Id)
                ?? throw new ArgumentException("No Faq found with the specified id");
            faq.EntityId = model.EntityId;
            faq.EntityType = model.EntityType;
            faq.Question = model.Question;
            faq.Answer = model.Answer;
            faq.DisplayOrder = model.DisplayOrder;
            faq.UpdatedOnUtc = DateTime.UtcNow;
            await _faqService.UpdateAsync(faq);
            return new NullJsonResult();
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> FaqDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.CustomPermission.CUSTOM_FAQ_PRODUCT_ACCESS) && !await _permissionService.AuthorizeAsync(StandardPermission.CustomPermission.CUSTOM_FAQ_CATEGORY_ACCESS))
                return AccessDeniedView();

            var faq = await _faqService.GetById(id)
                ?? throw new ArgumentException("No Faq found with the specified id");
            await _faqService.DeleteAsync(faq);
            return new NullJsonResult();
        }

        // <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> FaqAddPopup(int entityId, string entityType)
        {
            if (entityType == "Product")
            {
                if (!await _permissionService.AuthorizeAsync(StandardPermission.CustomPermission.CUSTOM_FAQ_PRODUCT_ACCESS))
                    return await AccessDeniedJsonAsync();
            }
            else if (entityType == "Category")
            {
                if (!await _permissionService.AuthorizeAsync(StandardPermission.CustomPermission.CUSTOM_FAQ_CATEGORY_ACCESS))
                    return await AccessDeniedJsonAsync();
            }
            var model = new AddFaqModel();
            model.EntityId = entityId;
            model.EntityType = entityType;
            return View(model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> FaqAddPopup(AddFaqModel model)
        {
            if (model.EntityType == "Product")
            {
                if (!await _permissionService.AuthorizeAsync(StandardPermission.CustomPermission.CUSTOM_FAQ_PRODUCT_ACCESS))
                    return await AccessDeniedJsonAsync();
            }
            else if (model.EntityType == "Category")
            {
                if (!await _permissionService.AuthorizeAsync(StandardPermission.CustomPermission.CUSTOM_FAQ_CATEGORY_ACCESS))
                    return await AccessDeniedJsonAsync();
            }
            var faqs = await _faqService.GetFaqsByEntityIdAsync(model.EntityId, model.EntityType);
            if (model != null)
            {
                var faq = new Faq();
                faq.EntityId = model.EntityId;
                faq.EntityType = model.EntityType;
                faq.Question = model.Question;
                faq.Answer = model.Answer;
                faq.CreatedOnUtc = DateTime.UtcNow;
                faq.UpdatedOnUtc = DateTime.UtcNow;
                faq.Answer = model.Answer;
                faq.DisplayOrder = (faqs.Count + 1);
                await _faqService.InsertAsync(faq);
            }
            ViewBag.RefreshPage = true;
            return View(model);
        }

        #endregion
    }
}
