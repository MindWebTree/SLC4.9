using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customization.Custom;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customizations.Custom;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Areas.Admin.Models.Customization.Custom;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using System;
using Nop.Core;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Services.Customizations.Custom.KW;
using System.Collections.Generic;
using Nop.Core.Domain.Discounts;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Web.Areas.Admin.Controllers
{
    [CategoryPermission("ManageCategories")]
    public partial class CategoryController : BaseAdminController
    {

        #region List



        /// <returns>A task that represents the asynchronous operation</returns>

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> CustomList(CategorySearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _categoryModelFactory.PrepareCustomCategoryListModelAsync(searchModel);

            return Json(model);
        }

        #endregion

        #region Create / Edit / Delete

        /// <returns>A task that represents the asynchronous operation</returns>
        /// 

        public virtual async Task<IActionResult> CustomCreate()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessCategoryCreate))
                return AccessDeniedView();

            //prepare model
            var model = await _categoryModelFactory.PrepareCategoryModelAsync(new CategoryModel(), null);

            return View("Create",model);
        }



        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> CustomCreate(CategoryModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessCategoryCreate))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var category = model.ToEntity<Category>();
                category.CreatedOnUtc = DateTime.UtcNow;
                category.UpdatedOnUtc = DateTime.UtcNow;
                await _categoryService.InsertCategoryAsync(category);

                //search engine name
                model.SeName = await _urlRecordService.ValidateSeNameAsync(category, model.SeName, category.Name, true);
                await _urlRecordService.SaveSlugAsync(category, model.SeName, 0);

                //locales
                await UpdateLocalesAsync(category, model);

                //discounts
                var allDiscounts = await _discountService.GetAllDiscountsAsync(DiscountType.AssignedToCategories, showHidden: true);
                foreach (var discount in allDiscounts)
                {
                    if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                        await _categoryService.InsertDiscountCategoryMappingAsync(new DiscountCategoryMapping { DiscountId = discount.Id, EntityId = category.Id });
                }

                await _categoryService.UpdateCategoryAsync(category);

                //update picture seo file name
                await UpdatePictureSeoNamesAsync(category);

                //ACL (customer roles)
                await SaveCategoryAclAsync(category, model);

                //stores
                await SaveStoreMappingsAsync(category, model);

                //activity log
                await _customerActivityService.InsertActivityAsync("AddNewCategory",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewCategory"), category.Name), category);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Categories.Added"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = category.Id });
            }

            //prepare model
            model = await _categoryModelFactory.PrepareCategoryModelAsync(model, null, true);

            //if we got this far, something failed, redisplay form
            return View("Create",model);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> CustomEdit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            //try to get a category with the specified id
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null || category.Deleted)
                return RedirectToAction("List");

            //prepare model
            var model = await _categoryModelFactory.PrepareCategoryModelAsync(null, category);

            return View("Edit", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> CustomEdit(CategoryModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            //try to get a category with the specified id
            var category = await _categoryService.GetCategoryByIdAsync(model.Id);
            if (category == null || category.Deleted)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                var prevPictureId = category.PictureId;

                //if parent category changes, we need to clear cache for previous parent category
                if (category.ParentCategoryId != model.ParentCategoryId)
                {
                    await _staticCacheManager.RemoveByPrefixAsync(NopCatalogDefaults.CategoriesByParentCategoryPrefix, category.ParentCategoryId);
                    await _staticCacheManager.RemoveByPrefixAsync(NopCatalogDefaults.CategoriesChildIdsPrefix, category.ParentCategoryId);
                }

                category = model.ToEntity(category);
                category.UpdatedOnUtc = DateTime.UtcNow;
                await _categoryService.UpdateCategoryAsync(category);

                //search engine name
                model.SeName = await _urlRecordService.ValidateSeNameAsync(category, model.SeName, category.Name, true);
                await _urlRecordService.SaveSlugAsync(category, model.SeName, 0);

                //locales
                await UpdateLocalesAsync(category, model);

                //discounts
                var allDiscounts = await _discountService.GetAllDiscountsAsync(DiscountType.AssignedToCategories, showHidden: true);
                foreach (var discount in allDiscounts)
                {
                    if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                    {
                        //new discount
                        if (await _categoryService.GetDiscountAppliedToCategoryAsync(category.Id, discount.Id) is null)
                            await _categoryService.InsertDiscountCategoryMappingAsync(new DiscountCategoryMapping { DiscountId = discount.Id, EntityId = category.Id });
                    }
                    else
                    {
                        //remove discount
                        if (await _categoryService.GetDiscountAppliedToCategoryAsync(category.Id, discount.Id) is DiscountCategoryMapping mapping)
                            await _categoryService.DeleteDiscountCategoryMappingAsync(mapping);
                    }
                }

                await _categoryService.UpdateCategoryAsync(category);

                //delete an old picture (if deleted or updated)
                if (prevPictureId > 0 && prevPictureId != category.PictureId)
                {
                    var prevPicture = await _pictureService.GetPictureByIdAsync(prevPictureId);
                    if (prevPicture != null)
                        await _pictureService.DeletePictureAsync(prevPicture);
                }

                //update picture seo file name
                await UpdatePictureSeoNamesAsync(category);

                //ACL
                await SaveCategoryAclAsync(category, model);

                //stores
                await SaveStoreMappingsAsync(category, model);

                //activity log
                await _customerActivityService.InsertActivityAsync("EditCategory",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditCategory"), category.Name), category);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Categories.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = category.Id });
            }

            //prepare model
            model = await _categoryModelFactory.PrepareCategoryModelAsync(model, category, true);

            //if we got this far, something failed, redisplay form
            return View("Edit",model);
        }




        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> CustomDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessCategoryDelete))
                return AccessDeniedView();

            //try to get a category with the specified id
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
                return RedirectToAction("List");

            await _categoryService.DeleteCategoryAsync(category);

            //activity log
            await _customerActivityService.InsertActivityAsync("DeleteCategory",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteCategory"), category.Name), category);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Categories.Deleted"));

            return RedirectToAction("List");
        }

       
        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> CustomDeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessCategoryDelete))
                return AccessDeniedView();

            if (selectedIds != null)
            {
                await _categoryService.DeleteCategoriesAsync(await (await _categoryService.GetCategoriesByIdsAsync(selectedIds.ToArray())).WhereAwait(async p => await _workContext.GetCurrentVendorAsync() == null).ToListAsync());
            }

            return Json(new { Result = true });
        }

        
        
        #endregion

        #region FiltersMapping

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> FiltersMappingByEntityList(FiltersMappingByEntitySearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return await AccessDeniedDataTablesJson();

            //try to get a product with the specified id
            if (searchModel.EntityType == "Category" || searchModel.EntityType == "KwTerm")
            {
                if (searchModel.EntityType == "Category")
                {
                    var category = await _categoryService.GetCategoryByIdAsync(searchModel.EntityId)
                        ?? throw new ArgumentException("No Category found with the specified id");
                }
                else
                {
                    var _kwTermService = EngineContext.Current.Resolve<IKwTermService>();
                    var kwTerm = await _kwTermService.GetKwTermByIdAsync(searchModel.EntityId)
                       ?? throw new ArgumentException("No Category found with the specified id");
                }
            }
            else
                throw new ArgumentException("No Entity found with entityType " + searchModel.EntityType);


            //prepare model
            var model = await _categoryModelFactory.PrepareFiltersMappingByEntityListModelAsync(searchModel, searchModel.EntityId, searchModel.EntityType, searchModel.FilterType);

            return Json(model);
        }
        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> FiltersMappingByEntityUpdate(FiltersMappingByEntityModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var _filtersMappingByEntityService = EngineContext.Current.Resolve<IFiltersMappingByEntityService>();

            //try to get a Collection product with the specified id
            var filtersMappingByEntity = await _filtersMappingByEntityService.GetById(model.Id)
                ?? throw new ArgumentException("No Filter Mapping By Entity found with the specified id");

            filtersMappingByEntity.DisplayOrder = model.DisplayOrder;
            filtersMappingByEntity.FilterId = model.FilterId;
            filtersMappingByEntity.Disabled = model.Disabled;
            filtersMappingByEntity.EntityId = model.EntityId;
            filtersMappingByEntity.EntityType = model.EntityType;
            filtersMappingByEntity.Filtertype = model.Filtertype;

            await _filtersMappingByEntityService.UpdateAsync(filtersMappingByEntity);
            return new NullJsonResult();
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> FiltersMappingByEntityDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();
            var _filtersMappingByEntityService = EngineContext.Current.Resolve<IFiltersMappingByEntityService>();

            //try to get a Collection product with the specified id
            var filtersMappingByEntity = await _filtersMappingByEntityService.GetById(id)
               ?? throw new ArgumentException("No Filter Mapping By Entity found with the specified id");

            await _filtersMappingByEntityService.DeleteAsync(filtersMappingByEntity);

            return new NullJsonResult();
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> FiltersMappingByEntityAddPopup(int entityId, string entityType, string filterType, int id = 0)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();
            FiltersMappingByEntityModel model = new FiltersMappingByEntityModel();



            if (id == 0)
            {
                model.EntityId = entityId;
                model.EntityType = entityType;
                model.Filtertype = filterType;
            }
            else
            {
                var _filtersMappingByEntityService = EngineContext.Current.Resolve<IFiltersMappingByEntityService>();
                var filtersMappingByEntity = await _filtersMappingByEntityService.GetById(id)
                 ?? throw new ArgumentException("No Filter Mapping By Entity found with the specified id");
                model.Disabled = filtersMappingByEntity.Disabled;
                model.DisplayOrder = filtersMappingByEntity.DisplayOrder;
                model.EntityId = filtersMappingByEntity.EntityId;
                model.EntityType = filtersMappingByEntity.EntityType;
                model.FilterId = filtersMappingByEntity.FilterId;
                model.Filtertype = filtersMappingByEntity.Filtertype;
                model.Id = filtersMappingByEntity.Id;

            }

            var _settingService = EngineContext.Current.Resolve<ISettingService>();
            var filterGroupId = await _settingService.GetSettingByKeyAsync<int>("specification.filter.id");
            var _specificationAttributeService = EngineContext.Current.Resolve<ISpecificationAttributeService>();
            var specsAttrs = await _specificationAttributeService.GetAllSpecificationAttributesAsync();
            specsAttrs = specsAttrs.Where(specAttr => specAttr.SpecificationAttributeGroupId == filterGroupId).ToList();


            if (filterType == "SpecificationAttribute")
            {
                foreach (var specAttr in specsAttrs)
                {
                    model.filters.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem()
                    {
                        Value = specAttr.Id.ToString(),
                        Text = specAttr.Name,
                        Selected = specAttr.Id == model.FilterId ? true : false
                    });
                }
            }
            else
            {
                var specsOptions = await _specificationAttributeService.GetAllSpecificationOptionsAsync();
                specsOptions = (from specsOption in specsOptions
                                join specAttr in specsAttrs
                                on specsOption.SpecificationAttributeId equals specAttr.Id
                                select specsOption).ToList();
                foreach (var specsOption in specsOptions)
                {
                    model.filters.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem()
                    {
                        Value = specsOption.Id.ToString(),
                        Text = specsOption.Name,
                        Selected = specsOption.Id == model.FilterId ? true : false
                    });
                }
            }
            return View(model);
        }



        [HttpPost]
        [FormValueRequired("save")]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> FiltersMappingByEntityInsert(FiltersMappingByEntityModel model)
        {
            if (ModelState.IsValid)
            {
                if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                    return AccessDeniedView();
                var _filtersMappingByEntityService = EngineContext.Current.Resolve<IFiltersMappingByEntityService>();
                FiltersMappingByEntityModel filtersMappingByEntityModel = new FiltersMappingByEntityModel();
                if (!await _filtersMappingByEntityService.IsMappingExist(model.EntityId, model.EntityType, model.FilterId, model.Filtertype, model.Id))
                {


                    FiltersMappingByEntity filtersMappingByEntity = new FiltersMappingByEntity();
                    if (model.Id > 0)
                    {
                        filtersMappingByEntity = await _filtersMappingByEntityService.GetById(model.Id)
                   ?? throw new ArgumentException("No Filter Mapping By Entity found with the specified id");

                    }
                    filtersMappingByEntity.Disabled = model.Disabled;
                    filtersMappingByEntity.DisplayOrder = model.DisplayOrder;
                    filtersMappingByEntity.EntityType = model.EntityType;
                    filtersMappingByEntity.EntityId = model.EntityId;
                    filtersMappingByEntity.FilterId = model.FilterId;
                    filtersMappingByEntity.Filtertype = model.Filtertype;
                    if (model.Id > 0)
                    {
                        await _filtersMappingByEntityService.UpdateAsync(filtersMappingByEntity);
                    }
                    else
                    {
                        await _filtersMappingByEntityService.InsertAsync(filtersMappingByEntity);
                    }


                    ViewBag.RefreshPage = true;
                    return View("FiltersMappingByEntityAddPopup", filtersMappingByEntityModel);
                }
                else
                {
                    ModelState.AddModelError("", "FiltersMapping already exist!!");
                }
            }
            var _specificationAttributeService = EngineContext.Current.Resolve<ISpecificationAttributeService>();
            if (model.Filtertype == "SpecificationAttribute")
            {
                var specsAttrs = await _specificationAttributeService.GetAllSpecificationAttributesAsync();
                foreach (var specAttr in specsAttrs)
                {
                    model.filters.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem()
                    {
                        Value = specAttr.Id.ToString(),
                        Text = specAttr.Name,
                        Selected = specAttr.Id == model.FilterId ? true : false
                    });
                }
            }
            else
            {
                var specsOptions = await _specificationAttributeService.GetAllSpecificationOptionsAsync();
                foreach (var specsOption in specsOptions)
                {
                    model.filters.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem()
                    {
                        Value = specsOption.Id.ToString(),
                        Text = specsOption.Name,
                        Selected = specsOption.Id == model.FilterId ? true : false
                    });
                }
            }
            return View("FiltersMappingByEntityAddPopup", model);

        }


        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> CategorySpecificationAttributeList(SpecificationAttributeSearchModel searchModel)
        {

            var _specificationAttributeService = EngineContext.Current.Resolve<ISpecificationAttributeService>();
            var _specificationAttributeModelFactory = EngineContext.Current.Resolve<ISpecificationAttributeModelFactory>();
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return await AccessDeniedDataTablesJson();
            var group = await _specificationAttributeService.GetSpecificationAttributeGroupByIdAsync(searchModel.SpecificationAttributeGroupId)
                                ?? throw new ArgumentException("No specification attribute group found with the specified id");
            var model = await _specificationAttributeModelFactory.CustomPrepareCategorySpecificationAttributeListModelAsync("category", searchModel.CategoryId, searchModel, group);
            return Json(model);
        }


        public virtual async Task<IActionResult> CategorySpecificationAttributeMappingUpdate(SpecificationAttributeModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();
            string entityType = "category";
            string filtertype = "SpecificationAttribute";

            var category = await _categoryService.GetCategoryByIdAsync(model.CategoryId);
            if (category == null || category.Deleted)
                return new NullJsonResult();

            var _filtersMappingByEntityService = EngineContext.Current.Resolve<IFiltersMappingByEntityService>();
            var GetFiltersMappingExist = await _filtersMappingByEntityService.GetFilterMapping(entityType, model.CategoryId, model.Id, filtertype);
            if (GetFiltersMappingExist != null)
            {
                GetFiltersMappingExist.Disabled = !model.Disabled;
                GetFiltersMappingExist.DisplayOnTop = model.DisplayOnTop;
                GetFiltersMappingExist.DisplayOrder = model.DisplayOrder;
                await _filtersMappingByEntityService.UpdateAsync(GetFiltersMappingExist);
            }
            else
            {
                var newMapping = new FiltersMappingByEntity
                {
                    Disabled = !model.Disabled,
                    DisplayOnTop = model.DisplayOnTop,
                    DisplayOrder = model.DisplayOrder,
                    EntityType = entityType,
                    EntityId = model.CategoryId,
                    FilterId = model.Id,
                    Filtertype = filtertype
                };
                await _filtersMappingByEntityService.InsertAsync(newMapping);
            }
            return new NullJsonResult();
        }

        [Route("Admin/Category/specificattribute/option")]
        public virtual async Task<IActionResult> CategorySpecificationAttributeOptionSearchList(int specificationAttributeId, int categoryId)
        {
            var _specificationAttributeModelFactory = EngineContext.Current.Resolve<ISpecificationAttributeModelFactory>();
            var _specificationAttributeService = EngineContext.Current.Resolve<ISpecificationAttributeService>();
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();
            var category = await _categoryService.GetCategoryByIdAsync(categoryId);
            if (category == null || category.Deleted)
                return RedirectToAction("List");
            var specificationAttribute = await _specificationAttributeService.GetSpecificationAttributeByIdAsync(specificationAttributeId);
            if (specificationAttribute == null)
                return RedirectToAction("List");
            var model = await _specificationAttributeModelFactory.CustomPrepareSpecificationAttributeModelAsync(null, specificationAttribute);
            model.CategoryId = categoryId;
            return View("CategorySpecificationAttributeOptionSearchList", model);
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> CategorySpecificationAttributeOptionList(SpecificationAttributeOptionSearchModel searchModel)
        {
            var _specificationAttributeModelFactory = EngineContext.Current.Resolve<ISpecificationAttributeModelFactory>();
            var _specificationAttributeService = EngineContext.Current.Resolve<ISpecificationAttributeService>();
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return await AccessDeniedDataTablesJson();

            var category = await _categoryService.GetCategoryByIdAsync(searchModel.CategoryId);
            if (category == null || category.Deleted)
                return new NullJsonResult();

            var specificationAttribute = await _specificationAttributeService.GetSpecificationAttributeByIdAsync(searchModel.SpecificationAttributeId)
                ?? throw new ArgumentException("No specification attribute found with the specified id");
            var model = await _specificationAttributeModelFactory.CustomPrepareCategorySpecificationAttributeOptionListModelAsync("category", searchModel, specificationAttribute);
            return Json(model);
        }

        public virtual async Task<IActionResult> CategorySpecificationAttributeOptionMappingUpdate(SpecificationAttributeOptionModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            var category = await _categoryService.GetCategoryByIdAsync(model.CategoryId);
            if (category == null || category.Deleted)
                return new NullJsonResult();

            string entityType = "category";
            string filtertype = "SpecificationAttributeOption";
            var _filtersMappingByEntityService = EngineContext.Current.Resolve<IFiltersMappingByEntityService>();
            var GetFiltersMappingExist = await _filtersMappingByEntityService.GetFilterMapping(entityType, model.CategoryId, model.Id, filtertype);
            if (GetFiltersMappingExist != null)
            {
                GetFiltersMappingExist.Disabled = !model.Disabled;
                GetFiltersMappingExist.DisplayOrder = model.DisplayOrder;
                await _filtersMappingByEntityService.UpdateAsync(GetFiltersMappingExist);
            }
            else
            {
                var newMapping = new FiltersMappingByEntity
                {
                    Disabled = !model.Disabled,
                    DisplayOrder = model.DisplayOrder,
                    EntityType = entityType,
                    EntityId = model.CategoryId,
                    FilterId = model.Id,
                    Filtertype = filtertype
                };
                await _filtersMappingByEntityService.InsertAsync(newMapping);
            }
            return new NullJsonResult();
        }



        [Route("Admin/Category/specificattribute/option/products")]
        public virtual async Task<IActionResult> CategorySpecificationOptionUsedByProductsSearchList(int specificationAttributeOptionId, int categoryId)
        {
            var _specificationAttributeService = EngineContext.Current.Resolve<ISpecificationAttributeService>();
            var _specificationAttributeModelFactory = EngineContext.Current.Resolve<ISpecificationAttributeModelFactory>();
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();
            var category = await _categoryService.GetCategoryByIdAsync(categoryId);
            if (category == null || category.Deleted)
                return RedirectToAction("List");
            var specificationAttributeOption = await _specificationAttributeService.GetSpecificationAttributeOptionByIdAsync(specificationAttributeOptionId);
            if (specificationAttributeOption == null)
                return RedirectToAction("List");
            var specificationAttribute = await _specificationAttributeService
                .GetSpecificationAttributeByIdAsync(specificationAttributeOption.SpecificationAttributeId);
            if (specificationAttribute == null)
                return RedirectToAction("List");
            var model = await _specificationAttributeModelFactory
                .CustomPrepareSpecificationAttributeOptionModelAsync(null, specificationAttribute, specificationAttributeOption);
            model.CategoryId = categoryId;
            return View("CategorySpecificationOptionUsedByProducts", model);
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> CategorySpecificationOptionUsedByProductsList(SpecificationAttributeOptionProductSearchModel searchModel)
        {
            var _specificationAttributeService = EngineContext.Current.Resolve<ISpecificationAttributeService>();
            var _specificationAttributeModelFactory = EngineContext.Current.Resolve<ISpecificationAttributeModelFactory>();
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return await AccessDeniedDataTablesJson();

            var category = await _categoryService.GetCategoryByIdAsync(searchModel.SearchCategoryId);
            if (category == null || category.Deleted)
                return new NullJsonResult();


            var specificationAttributeOption = await _specificationAttributeService.GetSpecificationAttributeOptionByIdAsync(searchModel.SpecificationAttributeOptionId)
                ?? throw new ArgumentException("No specification attribute found with the specified id");
            var model = await _specificationAttributeModelFactory.CustomPrepareCategorySpecificationOptionUsedByProductsListModelAsync(searchModel, specificationAttributeOption);
            return Json(model);
        }




        public virtual async Task<IActionResult> CategorySpecificationOptionMappingAddPopup(int specificationAttributeOptionId, int categoryId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();
            var model = new CategorySpecificationOptionProductSearchModel
            {
                CategoryId = categoryId,
                SpecificationAttributeOptionId = specificationAttributeOptionId
            };
            model.SetPopupGridPageSize();
            return View(model);
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> CategorySpecificationOptionMappingAddPopupList(CategorySpecificationOptionProductSearchModel searchModel)
        {
            var _specificationAttributeService = EngineContext.Current.Resolve<ISpecificationAttributeService>();
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return await AccessDeniedDataTablesJson();
            var category = await _categoryService.GetCategoryByIdAsync(searchModel.CategoryId)
                ?? throw new ArgumentException("No category found with the specified id");
            var model = await _categoryModelFactory.CustomPrepareCategorySpecificationOptionMappingAddPopupListModelAsync(searchModel, category);
            return Json(model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public virtual async Task<IActionResult> CategorySpecificationOptionMappingAddPopup(CategorySpecificationOptionProductSearchModel model)
        {
            var _specificationAttributeService = EngineContext.Current.Resolve<ISpecificationAttributeService>();
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();
            var mappedproducts = await _specificationAttributeService.CustomGetProductsSpecificationAttributeOptionByCategoryWiseAsync(model.CategoryId, model.SpecificationAttributeOptionId
               );
            var alreadyMappedProductIds = mappedproducts.Select(p => p.ProductId).ToList();

            var productIdsToAdd = model.SelectedProductIds.Except(alreadyMappedProductIds).ToList();

            var productIdsToRemove = (model.ProductIds.Except(model.SelectedProductIds).ToList()).Intersect(alreadyMappedProductIds);

            var addAllProducts = await _productService.GetProductsByIdsAsync(productIdsToAdd.ToArray());
            if (addAllProducts.Any())
            {
                foreach (var product in addAllProducts)
                {
                    var psa = new ProductSpecificationAttribute
                    {
                        ProductId = product.Id,
                        SpecificationAttributeOptionId = model.SpecificationAttributeOptionId,
                        DisplayOrder = product.DisplayOrder,
                        MobileDisplayOrder = 0,
                        AllowFiltering = true
                    };
                    await _specificationAttributeService.InsertProductSpecificationAttributeAsync(psa);
                }
            }
            if (productIdsToRemove.Any())
            {
                foreach (var id in productIdsToRemove)
                {
                    var mapingsNeedToDelete = await _specificationAttributeService
                    .GetProductSpecificationAttributesAsync(id, model.SpecificationAttributeOptionId);
                    foreach (var mapping in mapingsNeedToDelete)
                        await _specificationAttributeService.DeleteProductSpecificationAttributeAsync(mapping);
                }
            }
            ViewBag.RefreshPage = true;
            return View(model);
        }


        public virtual async Task<IActionResult> DeleteProductOptionMapping(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();
            var _specificationAttributeService = EngineContext.Current.Resolve<ISpecificationAttributeService>();
            //try to get a product category with the specified id
            var productSpecificationAttribute = await _specificationAttributeService.GetProductSpecificationAttributeByIdAsync(id)
                ?? throw new ArgumentException("No product category mapping found with the specified id", nameof(id));

            await _specificationAttributeService.DeleteProductSpecificationAttributeAsync(productSpecificationAttribute);

            return new NullJsonResult();
        }
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public virtual async Task<IActionResult> BulkUpdateSpecificationAttributeOption([FromBody] List<SpecificationAttributeOptionModel> models)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            string entityType = "category";
            string filtertype = "SpecificationAttributeOption";

            foreach (var model in models)
            {
                var referer = Request.Headers["Referer"].ToString();
                var categoryId = 0;
                if (Uri.TryCreate(referer, UriKind.Absolute, out var uri))
                {
                    var queryParams = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
                    if (queryParams.TryGetValue("categoryId", out var id))
                        int.TryParse(id, out categoryId);
                }

                var _filtersMappingByEntityService = EngineContext.Current.Resolve<IFiltersMappingByEntityService>();
                var filtersMapping = await _filtersMappingByEntityService.GetFilterMapping(entityType, categoryId, model.Id, "SpecificationAttributeOption");


                if (filtersMapping != null)
                {
                    filtersMapping.Disabled = !model.Disabled;
                    await _filtersMappingByEntityService.UpdateAsync(filtersMapping);
                }
                else
                {
                    var newMapping = new FiltersMappingByEntity
                    {
                        Disabled = !model.Disabled,
                        DisplayOrder = model.DisplayOrder,
                        EntityType = entityType,
                        EntityId = categoryId,
                        FilterId = model.Id,
                        Filtertype = filtertype
                    };
                    await _filtersMappingByEntityService.InsertAsync(newMapping);
                }
            }

            return Json(new { success = true });
        }
        #endregion

        #region QuickFilters

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> QuickFilterList(RelatedProductSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return await AccessDeniedDataTablesJson();

            //try to get a product with the specified id
            if (searchModel.EntityType == "Product")
            {
                var product = await _productService.GetProductByIdAsync(searchModel.ProductId)
                    ?? throw new ArgumentException("No product found with the specified id");
            }
            else if (searchModel.EntityType == "Category")
            {
                var category = await _categoryService.GetCategoryByIdAsync(searchModel.ProductId)
      ?? throw new ArgumentException("No Category found with the specified id");
            }
            var _quickFilterModelFactory = EngineContext.Current.Resolve<IQuickFilterModelFactory>();

            //prepare model
            var model = await _quickFilterModelFactory.PrepareQuickFilterListModelAsync(searchModel, searchModel.ProductId, searchModel.EntityType);

            return Json(model);
        }
        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> QuickFilterUpdate(QuickFilterModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var _quickFilterService = EngineContext.Current.Resolve<IQuickFilterService>();

            //try to get a Collection product with the specified id
            var quickFilter = await _quickFilterService.GetById(model.Id)
                ?? throw new ArgumentException("No Quick Filter found with the specified id");

            quickFilter.DisplayOrder = model.DisplayOrder;
            quickFilter.TermName = model.TermName;
            quickFilter.Link = model.Link;
            quickFilter.UpdatedOnUtc = DateTime.UtcNow;
            quickFilter.PictureId = model.PictureId;
            await _quickFilterService.UpdateAsync(quickFilter);
            return new NullJsonResult();
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> QuickFilterDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a Collection product with the specified id
            var _quickFilterService = EngineContext.Current.Resolve<IQuickFilterService>();

            //try to get a Collection product with the specified id
            var quickFilter = await _quickFilterService.GetById(id)
                ?? throw new ArgumentException("No Quick Filter found with the specified id");
            await _quickFilterService.DeleteAsync(quickFilter);

            return new NullJsonResult();
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> QuickFilterAddPopup(int entityId, string entityType, int id = 0)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();
            QuickFilterModel model = new QuickFilterModel();
            if (id != 0)
            {
                var _quickFilterService = EngineContext.Current.Resolve<IQuickFilterService>();
                var quickFilter = await _quickFilterService.GetById(id);
                model.DisplayOrder = quickFilter.DisplayOrder;
                model.Link = quickFilter.Link;
                model.TermName = quickFilter.TermName;
                model.PictureId = quickFilter.PictureId;
                model.EntityId = quickFilter.EntityId;
                model.EntityType = quickFilter.EntityType;
            }
            else
            {
                model.EntityId = entityId;
                model.EntityType = entityType;
            }
            return View(model);
        }



        [HttpPost]
        [FormValueRequired("save")]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> QuickFilterInsert(QuickFilterModel model)
        {
            if (ModelState.IsValid)
            {
                if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                    return AccessDeniedView();
                var _quickFilterService = EngineContext.Current.Resolve<IQuickFilterService>();

                if (model.Id == 0)
                {
                    QuickFilter quickFilterSearch = new QuickFilter();
                    quickFilterSearch.Link = model.Link;
                    quickFilterSearch.TermName = model.TermName;
                    quickFilterSearch.EntityType = model.EntityType;
                    quickFilterSearch.EntityId = model.EntityId;
                    quickFilterSearch.DisplayOrder = model.DisplayOrder;
                    quickFilterSearch.PictureId = model.PictureId;
                    quickFilterSearch.CreatedOnUtc = DateTime.UtcNow;
                    quickFilterSearch.UpdatedOnUtc = DateTime.UtcNow;

                    await _quickFilterService.InsertAsync(quickFilterSearch);
                }
                else
                {
                    var quickFilter = await _quickFilterService.GetById(model.Id);
                    quickFilter.Link = model.Link;
                    quickFilter.TermName = model.TermName;
                    quickFilter.EntityType = model.EntityType;
                    quickFilter.EntityId = model.EntityId;
                    quickFilter.DisplayOrder = model.DisplayOrder;
                    quickFilter.PictureId = model.PictureId;
                    quickFilter.UpdatedOnUtc = DateTime.UtcNow;
                    await _quickFilterService.UpdateAsync(quickFilter);
                }

                ViewBag.RefreshPage = true;

                QuickFilterModel quickFilterSearcModel = new QuickFilterModel();
                quickFilterSearcModel.EntityType = model.EntityType;
                quickFilterSearcModel.EntityId = model.EntityId;
                return View("QuickFilterAddPopup", quickFilterSearcModel);
            }
            else
                return View("QuickFilterAddPopup", model);
        }
        #endregion


        #region Category_CollectionLinks

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> CollectionLinkList(CollectionLinkSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return await AccessDeniedDataTablesJson();

            //try to get a product with the specified id

            var category = await _categoryService.GetCategoryByIdAsync(searchModel.EntityId)
  ?? throw new ArgumentException("No Category found with the specified id");

            var _categoryCollectionLinkModelFactory = EngineContext.Current.Resolve<ICategoryCollectionLinkModelFactory>();

            //prepare model
            var model = await _categoryCollectionLinkModelFactory.PrepareCategoryCollectionLinkListModelAsync(searchModel, searchModel.EntityId);

            return Json(model);
        }
        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> CollectionLinkUpdate(CategoryCollectionLinkModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            var _categoryCollectionLinkService = EngineContext.Current.Resolve<ICategoryCollectionLinkService>();

            //try to get a Collection product with the specified id
            var categoryCollectionLink = await _categoryCollectionLinkService.GetById(model.Id)
                ?? throw new ArgumentException("No Category Collection Link found with the specified id");

            categoryCollectionLink.DisplayOrder = model.DisplayOrder;
            categoryCollectionLink.Title = model.Title;
            categoryCollectionLink.Link = model.Link;
            categoryCollectionLink.UpdatedOnUtc = DateTime.UtcNow;

            await _categoryCollectionLinkService.UpdateAsync(categoryCollectionLink);
            return new NullJsonResult();
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> CollectionLinkDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            //try to get a Collection product with the specified id
            var _categoryCollectionLinkService = EngineContext.Current.Resolve<ICategoryCollectionLinkService>();

            //try to get a Collection product with the specified id
            var categoryCollectionLink = await _categoryCollectionLinkService.GetById(id)
                ?? throw new ArgumentException("No Category Collection Link found with the specified id");
            await _categoryCollectionLinkService.DeleteAsync(categoryCollectionLink);

            return new NullJsonResult();
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> CollectionLinkAddPopup(int entityId, int id = 0)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();
            CategoryCollectionLinkModel model = new CategoryCollectionLinkModel();
            if (id != 0)
            {
                var _categoryCollectionLinkService = EngineContext.Current.Resolve<ICategoryCollectionLinkService>();
                var categoryCollectionLink = await _categoryCollectionLinkService.GetById(id);
                model.DisplayOrder = categoryCollectionLink.DisplayOrder;
                model.Link = categoryCollectionLink.Link;
                model.Title = categoryCollectionLink.Title;
                model.EntityId = categoryCollectionLink.EntityId;

            }
            else
                model.EntityId = entityId;
            return View(model);
        }



        [HttpPost]
        [FormValueRequired("save")]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> CollectionLinkInsert(CategoryCollectionLinkModel model)
        {
            if (ModelState.IsValid)
            {
                if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                    return AccessDeniedView();
                var _categoryCollectionLinkService = EngineContext.Current.Resolve<ICategoryCollectionLinkService>();

                if (model.Id == 0)
                {
                    CategoryCollectionLink categoryCollectionLink = new CategoryCollectionLink();
                    categoryCollectionLink.Link = model.Link;
                    categoryCollectionLink.Title = model.Title;
                    categoryCollectionLink.EntityId = model.EntityId;
                    categoryCollectionLink.DisplayOrder = model.DisplayOrder;
                    categoryCollectionLink.CreatedOnUtc = DateTime.UtcNow;
                    categoryCollectionLink.UpdatedOnUtc = DateTime.UtcNow;

                    await _categoryCollectionLinkService.InsertAsync(categoryCollectionLink);
                }
                else
                {
                    var categoryCollectionLink = await _categoryCollectionLinkService.GetById(model.Id);
                    categoryCollectionLink.Link = model.Link;
                    categoryCollectionLink.Title = model.Title;
                    categoryCollectionLink.EntityId = model.EntityId;
                    categoryCollectionLink.DisplayOrder = model.DisplayOrder;
                    categoryCollectionLink.UpdatedOnUtc = DateTime.UtcNow;
                    await _categoryCollectionLinkService.UpdateAsync(categoryCollectionLink);
                }

                ViewBag.RefreshPage = true;

                CategoryCollectionLinkModel categoryCollectionLinkModel = new CategoryCollectionLinkModel();
                categoryCollectionLinkModel.EntityId = model.EntityId;
                return View("CollectionLinkAddPopup", categoryCollectionLinkModel);
            }
            else
                return View("CollectionLinkAddPopup", model);
        }
        #endregion

        #region CategoryProductsMapping Export

        [HttpPost, ActionName("ExportExcel")]
        [FormValueRequired("exportexcel-product")]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> ExportCategoryProductsMappings(CategoryModel model)
        {
            var products = await _categoryService.GetProductCategoriesByCategoryIdAsync(model.Id, 0, int.MaxValue);
            if (!products.Any())
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Category.NoProducts"));
                return RedirectToAction("Edit", new { id = model.Id });
            }
            try
            {
                var bytes = await _exportManager.ExportCategoryProductsToXlsxAsync(products);
                return File(bytes, MimeTypes.TextXlsx, "category-products.xlsx");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("Edit", new { id = model.Id });
            }
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> ImportProductMappingExcel(IFormFile importexcelfile, int id)
        {
            if (importexcelfile != null && importexcelfile.Length > 0)
                await _importManager.ImportCategoryProductsFromXlsxAsync(importexcelfile.OpenReadStream(), id);
            else
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Common.UploadFile"));


            return RedirectToAction("Edit", new { id = id });
        }

        #endregion

        #region Products

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> CustomProductAddPopupList(AddProductToCategorySearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _categoryModelFactory.CustomPrepareAddProductToCategoryListModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> CustomProductList(CategoryProductSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return await AccessDeniedDataTablesJson();

            //try to get a category with the specified id
            var category = await _categoryService.GetCategoryByIdAsync(searchModel.CategoryId)
                ?? throw new ArgumentException("No category found with the specified id");

            //prepare model
            var model = await _categoryModelFactory.CustomPrepareCategoryProductListModelAsync(searchModel, category);

            return Json(model);
        }
        #endregion

        #region SuggestedKeyword

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> SuggestedKeywordList(CategorySuggestedKeywordSearchModel model, int categoryId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return await AccessDeniedDataTablesJson();

            //try to get a product with the specified id
            if (categoryId != 0)
            {
                var categorySuggestedKeywords = await _categoryModelFactory.CustomPrepareSuggestedKeywordListModelAsync(model, categoryId);

                return Json(categorySuggestedKeywords);

            }
            else
                throw new ArgumentException("please provide categoryId " + categoryId);
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> SuggestedKeywordDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();
            //try to get a Collection product with the specified id
            var categorySuggestedKeyword = await _categoryModelFactory.GetCategorySuggestedKeywordById(id)
              ?? throw new ArgumentException("No Suggested Keyword found with the specified id");
            await _categoryModelFactory.DeleteCategorySuggestedKeyword(categorySuggestedKeyword);
            return new NullJsonResult();
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> SuggestedKeywordAddPopup(int categoryId, int id = 0)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();
            CategorySuggestedKeywordModel model = new CategorySuggestedKeywordModel();
            if (id != 0)
            {
                var x = await _categoryModelFactory.GetCategorySuggestedKeywordById(id);
                model.Id = x.Id;
                model.CategoryId = x.CategoryId;
                model.KeyWord = x.KeyWord;
            }
            return View(model);
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> SuggestedKeywordCreateUpdate(CategorySuggestedKeywordModel model)
        {
            if (ModelState.IsValid)
            {
                if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                    return AccessDeniedView();
                if (await _categoryModelFactory.IsCategoryKeyWordExist(model.Id, model.KeyWord))
                {
                    ModelState.AddModelError("", "Suggested Keyword already exist!!");
                }
                else
                {
                    CategorySuggestedKeywordModel categorySuggestedKeyword = new CategorySuggestedKeywordModel();
                    if (model.Id > 0)
                    {
                        categorySuggestedKeyword = await _categoryModelFactory.GetCategorySuggestedKeywordById(model.Id)
                       ?? throw new ArgumentException("No Suggested Keyword found with the specified id");
                        categorySuggestedKeyword.KeyWord = model.KeyWord;
                        categorySuggestedKeyword.Id = model.Id;
                        categorySuggestedKeyword.CategoryId = model.CategoryId;
                        await _categoryModelFactory.UpdateCategorySuggestedKeyword(categorySuggestedKeyword);
                    }
                    else
                    {
                        await _categoryModelFactory.CreateCategorySuggestedKeyword(model);
                    }
                    ViewBag.RefreshPage = true;
                    return View("SuggestedKeywordAddPopup", categorySuggestedKeyword);
                }
            }
            return View("SuggestedKeywordAddPopup", model);
        }
        #endregion
    }
}