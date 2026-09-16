using Microsoft.AspNetCore.Mvc;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Models.Catalog;
using System.Threading.Tasks;
using System;
using Nop.Core.Domain.Catalog;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Mvc;
using Microsoft.AspNetCore.Http;
using Nop.Web.Framework.Controllers;
using System.Linq;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.ExportImport;
using Nop.Core;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Core.Domain.Customization.Catalog;
using System.Collections.Generic;
using MWT.Nop.Core.Domain.Catalog;
using MWT.Nop.Core.Services.Catalog;
using MWT.Nop.Core.Services.ExportImport;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class SpecificationAttributeController : BaseAdminController
    {
        #region Specification Mapped products
        [CheckPermission(StandardPermission.Catalog.SPECIFICATION_ATTRIBUTES_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> CustomSpecificationAttributeUsedByProducts(SpecificationAttributeProductSearchModel searchModel)
        {
            //try to get a specification attribute with the specified id
            var specificationAttribute = await _specificationAttributeService.GetSpecificationAttributeByIdAsync(searchModel.SpecificationAttributeId)
                ?? throw new ArgumentException("No specification attribute found with the specified id");

            //prepare model
            var model = await _specificationAttributeModelFactory.CustomPrepareSpecificationAttributeProductListModelAsync(searchModel, specificationAttribute);

            return Json(model);
        }

        [CheckPermission(StandardPermission.Catalog.SPECIFICATION_ATTRIBUTES_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> CustomEditSpecificationAttribute(int id)
        {
            //try to get a specification attribute with the specified id
            var specificationAttribute = await _specificationAttributeService.GetSpecificationAttributeByIdAsync(id);
            if (specificationAttribute == null)
                return RedirectToAction("List");

            //prepare model
            var model = await _specificationAttributeModelFactory.CustomPrepareSpecificationAttributeModelAsync(null, specificationAttribute);

            return View("EditSpecificationAttribute", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [CheckPermission(StandardPermission.Catalog.SPECIFICATION_ATTRIBUTES_CREATE_EDIT_DELETE)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> CustomEditSpecificationAttribute(SpecificationAttributeModel model, bool continueEditing)
        {
            //try to get a specification attribute with the specified id
            var specificationAttribute = await _specificationAttributeService.GetSpecificationAttributeByIdAsync(model.Id);
            if (specificationAttribute == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                specificationAttribute = model.ToEntity(specificationAttribute);
                await _specificationAttributeService.UpdateSpecificationAttributeAsync(specificationAttribute);

                await UpdateAttributeLocalesAsync(specificationAttribute, model);

                //activity log
                await _customerActivityService.InsertActivityAsync("EditSpecAttribute",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditSpecAttribute"), specificationAttribute.Name), specificationAttribute);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Attributes.SpecificationAttributes.SpecificationAttribute.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("CustomEditSpecificationAttribute", new { id = specificationAttribute.Id });
            }

            //prepare model
            model = await _specificationAttributeModelFactory.CustomPrepareSpecificationAttributeModelAsync(model, specificationAttribute, true);

            //if we got this far, something failed, redisplay form
            return View("EditSpecificationAttribute", model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Catalog.SPECIFICATION_ATTRIBUTES_CREATE_EDIT_DELETE)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> OptionUsedByProducts(SpecificationAttributeOptionProductSearchModel searchModel)
        {
            //try to get a specification attribute with the specified id
            var specificationAttributeOption = await _specificationAttributeService.GetSpecificationAttributeOptionByIdAsync(searchModel.SpecificationAttributeOptionId)
                ?? throw new ArgumentException("No specification attribute found with the specified id");

            //prepare model
            var model = await _specificationAttributeModelFactory.PrepareSpecificationAttributeOptionProductListModelAsync(searchModel, specificationAttributeOption);

            return Json(model);
        }



        /// <returns>A task that represents the asynchronous operation</returns>
        /// 
        [CheckPermission(StandardPermission.Catalog.SPECIFICATION_ATTRIBUTES_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> CreateSpecificationAttributeOption(int id)
        {
            //try to get a specification attribute with the specified id
            var specificationAttribute = await _specificationAttributeService.GetSpecificationAttributeByIdAsync(id);
            if (specificationAttribute == null)
                return RedirectToAction("List");

            //prepare model
            var model = await _specificationAttributeModelFactory
                .CustomPrepareSpecificationAttributeOptionModelAsync(new SpecificationAttributeOptionModel(), specificationAttribute, null);

            return View(model);
        }



        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [CheckPermission(StandardPermission.Catalog.SPECIFICATION_ATTRIBUTES_CREATE_EDIT_DELETE)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> CreateSpecificationAttributeOption(SpecificationAttributeOptionModel model, bool continueEditing)
        {
            //try to get a specification attribute with the specified id
            var specificationAttribute = await _specificationAttributeService.GetSpecificationAttributeByIdAsync(model.SpecificationAttributeId);
            if (specificationAttribute == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                var sao = model.ToEntity<SpecificationAttributeOption>();

                //clear "Color" values if it's disabled
                if (!model.EnableColorSquaresRgb)
                    sao.ColorSquaresRgb = null;

                await _specificationAttributeService.InsertSpecificationAttributeOptionAsync(sao);

                await UpdateOptionLocalesAsync(sao, model);
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Attributes.SpecificationAttributes.SpecificationAttributeOption.Added"));

                if (!continueEditing)
                    return RedirectToAction("CustomEditSpecificationAttribute", new { id = specificationAttribute.Id });

                return RedirectToAction("EditSpecificationAttributeOption", new { id = sao.Id });


            }

            //prepare model
            model = await _specificationAttributeModelFactory.CustomPrepareSpecificationAttributeOptionModelAsync(model, specificationAttribute, null, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        [CheckPermission(StandardPermission.Catalog.SPECIFICATION_ATTRIBUTES_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> EditSpecificationAttributeOption(int id)
        {
            //try to get a specification attribute option with the specified id
            var specificationAttributeOption = await _specificationAttributeService.GetSpecificationAttributeOptionByIdAsync(id);
            if (specificationAttributeOption == null)
                return RedirectToAction("List");

            //try to get a specification attribute with the specified id
            var specificationAttribute = await _specificationAttributeService
                .GetSpecificationAttributeByIdAsync(specificationAttributeOption.SpecificationAttributeId);
            if (specificationAttribute == null)
                return RedirectToAction("List");

            //prepare model
            var model = await _specificationAttributeModelFactory
                .CustomPrepareSpecificationAttributeOptionModelAsync(null, specificationAttribute, specificationAttributeOption);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [CheckPermission(StandardPermission.Catalog.SPECIFICATION_ATTRIBUTES_CREATE_EDIT_DELETE)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> EditSpecificationAttributeOption(SpecificationAttributeOptionModel model, bool continueEditing)
        {
            //try to get a specification attribute option with the specified id
            var specificationAttributeOption = await _specificationAttributeService.GetSpecificationAttributeOptionByIdAsync(model.Id);
            if (specificationAttributeOption == null)
                return RedirectToAction("CustomEditSpecificationAttribute", new { id = model.SpecificationAttributeId });

            //try to get a specification attribute with the specified id
            var specificationAttribute = await _specificationAttributeService
                .GetSpecificationAttributeByIdAsync(specificationAttributeOption.SpecificationAttributeId);
            if (specificationAttribute == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                specificationAttributeOption = model.ToEntity(specificationAttributeOption);

                //clear "Color" values if it's disabled
                if (!model.EnableColorSquaresRgb)
                    specificationAttributeOption.ColorSquaresRgb = null;

                await _specificationAttributeService.UpdateSpecificationAttributeOptionAsync(specificationAttributeOption);

                await UpdateOptionLocalesAsync(specificationAttributeOption, model);
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Attributes.SpecificationAttributes.SpecificationAttributeOption.Updated"));


                if (!continueEditing)
                    return RedirectToAction("CustomEditSpecificationAttribute", new { id = model.SpecificationAttributeId });

                return RedirectToAction("EditSpecificationAttributeOption", new { id = model.Id });
            }

            //prepare model
            model = await _specificationAttributeModelFactory
                .CustomPrepareSpecificationAttributeOptionModelAsync(model, specificationAttribute, specificationAttributeOption, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [CheckPermission(StandardPermission.Catalog.SPECIFICATION_ATTRIBUTES_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> SpecificationOptionProductUpdate(SpecificationAttributeOptionProductModel model)
        {
            //try to get a product category with the specified id
            var productSpecificationAttribute = await _specificationAttributeService.GetProductSpecificationAttributeByIdAsync(model.Id)
                ?? throw new ArgumentException("No  Product Specification Attribute Option found with the specified id");

            //fill entity from product
            productSpecificationAttribute.DisplayOrder = model.DisplayOrder;
            productSpecificationAttribute.MobileDisplayOrder = model.MobileDisplayOrder;
            await _specificationAttributeService.UpdateProductSpecificationAttributeAsync(productSpecificationAttribute);

            return new NullJsonResult();
        }

        [CheckPermission(StandardPermission.Catalog.SPECIFICATION_ATTRIBUTES_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> ProductDelete(int id)
        {
            //try to get a product category with the specified id
            var productSpecificationAttribute = await _specificationAttributeService.GetProductSpecificationAttributeByIdAsync(id)
                ?? throw new ArgumentException("No product category mapping found with the specified id", nameof(id));

            await _specificationAttributeService.DeleteProductSpecificationAttributeAsync(productSpecificationAttribute);

            return new NullJsonResult();
        }

        #endregion


        #region CategoryProductsMapping Export

        [HttpPost, ActionName("ExportExcelOptionProductMapping")]
        [FormValueRequired("exportexcel-option-product")]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> ExportProductsMappings(SpecificationAttributeOptionModel model)
        {
            var _specificationAttributeService = EngineContext.Current.Resolve<ICustomSpecificationAttributeService>();
            var products = await _specificationAttributeService.GetProductsBySpecificationAttributeOptionIdAsync(model.Id, 0, int.MaxValue);
            if (!products.Any())
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.SpecificationAttributeOption.NoProducts"));
                return RedirectToAction("EditSpecificationAttributeOption", new { id = model.Id });
            }
            try
            {
                var _categoryService = EngineContext.Current.Resolve<ICategoryService>();
                List<ExportProductSpecFormat> exprtPrdSpecFormat = new List<ExportProductSpecFormat>();
                foreach (var product in products)
                {
                    ExportProductSpecFormat prd = new ExportProductSpecFormat();
                    prd.ProductId = product.ProductId;
                    prd.SpecificationAttributeOptionId = product.SpecificationAttributeOptionId;
                    prd.DisplayOrder = product.DisplayOrder;
                    var categories = await _categoryService.GetProductCategoriesByProductIdAsync(prd.ProductId);
                    prd.CategoryIds = string.Join(",", categories.Select(c => c.CategoryId).ToArray());
                    exprtPrdSpecFormat.Add(prd);
                }
                var _exportManager = EngineContext.Current.Resolve<IExportExtendedManager>();
                var bytes = await _exportManager.ExportSpecificationAttributeOptionProductsToXlsxAsync(exprtPrdSpecFormat);
                return File(bytes, MimeTypes.TextXlsx, "specification-option-products.xlsx");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("EditSpecificationAttributeOption", new { id = model.Id });
            }
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> ImportProductMappingExcel(IFormFile importexcelfile, int id)
        {
            var _importManager = EngineContext.Current.Resolve<IImportExtendedManager>();
            if (importexcelfile != null && importexcelfile.Length > 0)
                await _importManager.ImportSpecificationAttributeOptionProductsFromXlsxAsync(importexcelfile.OpenReadStream(), id);
            else
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Common.UploadFile"));


            return RedirectToAction("EditSpecificationAttributeOption", new { id = id });
        }



        #endregion
    }
}
