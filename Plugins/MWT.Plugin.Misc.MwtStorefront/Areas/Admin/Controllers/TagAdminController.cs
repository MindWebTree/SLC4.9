using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MWT.Nop.Core.Domain.TagPage;
using MWT.Nop.Core.Services.Catalog;
using MWT.Nop.Core.Services.TagPage;
using MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.TagAdmin;
using Nop.Services.Catalog;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Controllers
{
    [Area(AreaNames.ADMIN)]
    [AuthorizeAdmin]
    [AutoValidateAntiforgeryToken]
    public class TagAdminController : BasePluginController
    {
        private readonly ITagSlugService _tagSlugService;
        private readonly ISegmentSlugService _segmentSlugService;
        private readonly IProductTagService _productTagService;
        private readonly ICategoryService _categoryService;
        private readonly ICustomSpecificationAttributeService _specService;
        private readonly IPermissionService _permissionService;
        private readonly IUrlRecordService _urlRecordService;

        public TagAdminController(
            ITagSlugService tagSlugService,
            ISegmentSlugService segmentSlugService,
            IProductTagService productTagService,
            ICategoryService categoryService,
            ICustomSpecificationAttributeService specService,
            IPermissionService permissionService,
            IUrlRecordService urlRecordService)
        {
            _tagSlugService = tagSlugService;
            _segmentSlugService = segmentSlugService;
            _productTagService = productTagService;
            _categoryService = categoryService;
            _specService = specService;
            _permissionService = permissionService;
            _urlRecordService = urlRecordService;
        }

        // ════════════════════════════════════════════════════════════════
        //  TAG SLUG MAPPINGS  (first segment)
        // ════════════════════════════════════════════════════════════════
        [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
        public async Task<IActionResult> TagSlugs()
        { 

            return View(new TagSlugMappingSearchModel());
        }

        [HttpPost]
        public async Task<IActionResult> TagSlugList(TagSlugMappingSearchModel searchModel)
        {
            var all = await _tagSlugService.GetAllAsync();

            var pagedData = all
                .Skip((searchModel.Page - 1) * searchModel.PageSize)
                .Take(searchModel.PageSize)
                .ToList();

            var tagSlugs = all.ToPagedList(searchModel);

            //prepare grid model
            var model = await new TagSlugMappingListModel().PrepareToGridAsync(searchModel, tagSlugs, () =>
            {
                return tagSlugs.SelectAwait(async x =>
                {
                    return new TagSlugMappingModel
                    {
                        Id = x.Id,
                        Slug = x.Slug,
                        TagId = x.TagId,
                        Label = x.Label,
                        SortOrder = x.SortOrder,
                        IsActive = x.IsActive
                    };
                });
            });


            return Json(model);
        }

        public async Task<IActionResult> CreateTagSlug()
        {
            var model = new TagSlugMappingModel();
            await PopulateTagDropdownAsync(model);
            return View("CreateEditTagSlug", model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTagSlug(TagSlugMappingModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateTagDropdownAsync(model);
                return View("CreateEditTagSlug", model);
            }
            model.Slug = await _urlRecordService.ValidateSeNameAsync(model.Id, "Tag", model.Slug, model.Label, false);
            if ((await _tagSlugService.GetAllAsync()).Where(x => x.Slug == model.Slug).Any())
            {
                ModelState.AddModelError(string.Empty, "Admin.TagPages.TagSlug.Error.Slug.AlreadyInUse");
                await PopulateTagDropdownAsync(model);
                return View("CreateEditTagSlug", model);
            }
            await _tagSlugService.InsertAsync(new TagSlugMapping
            {
                Slug = model.Slug.ToLowerInvariant().Trim(),
                TagId = model.TagId,
                Label = model.Label,
                SortOrder = model.SortOrder,
                IsActive = model.IsActive,
                AdditionalDescription = model.AdditionalDescription,
                Description = model.Description,
                EnableInfiniteScroll = model.EnableInfiniteScroll,
                MetaDescription = model.MetaDescription,
                MetaKeywords = model.MetaKeywords,
                MetaTitle = model.MetaTitle,
                ExploreMoreLinks = model.ExploreMoreLinks
            });

            return RedirectToAction(nameof(TagSlugs));
        }

        public async Task<IActionResult> EditTagSlug(int id)
        {
            var entity = (await _tagSlugService.GetAllAsync()).FirstOrDefault(x => x.Id == id);
            if (entity == null) return NotFound();

            var model = new TagSlugMappingModel
            {
                Id = entity.Id,
                Slug = entity.Slug,
                TagId = entity.TagId,
                Label = entity.Label,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive,
                AdditionalDescription = entity.AdditionalDescription,
                Description = entity.Description,
                EnableInfiniteScroll = entity.EnableInfiniteScroll,
                MetaDescription = entity.MetaDescription,
                MetaKeywords = entity.MetaKeywords,
                MetaTitle = entity.MetaTitle,
                ExploreMoreLinks = entity.ExploreMoreLinks
            };

            await PopulateTagDropdownAsync(model);
            return View("CreateEditTagSlug", model);
        }

        [HttpPost]
        public async Task<IActionResult> EditTagSlug(TagSlugMappingModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateTagDropdownAsync(model);
                return View("CreateEditTagSlug", model);
            }
            model.Slug = await _urlRecordService.ValidateSeNameAsync(model.Id, "Tag", model.Slug, model.Label, false);
            var all = await _tagSlugService.GetAllAsync();
            if (all.Where(x => x.Slug == model.Slug && x.Id != model.Id).Any())
            {
                ModelState.AddModelError(string.Empty, "Admin.TagPages.TagSlug.Error.Slug.AlreadyInUse");
                await PopulateTagDropdownAsync(model);
                return View("CreateEditTagSlug", model);
            }


            var entity = all.FirstOrDefault(x => x.Id == model.Id);
            if (entity == null) return NotFound();

            entity.Slug = model.Slug.ToLowerInvariant().Trim();
            entity.TagId = model.TagId;
            entity.Label = model.Label;
            entity.SortOrder = model.SortOrder;
            entity.IsActive = model.IsActive;
            entity.AdditionalDescription = model.AdditionalDescription;
            entity.Description = model.Description;
            entity.EnableInfiniteScroll = model.EnableInfiniteScroll;
            entity.MetaDescription = model.MetaDescription;
            entity.MetaKeywords = model.MetaKeywords;
            entity.MetaTitle = model.MetaTitle;
            entity.ExploreMoreLinks = model.ExploreMoreLinks;
            await _tagSlugService.UpdateAsync(entity);
            return RedirectToAction(nameof(TagSlugs));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTagSlug(int id)
        {
            var all = await _tagSlugService.GetAllAsync();
            var entity = all.FirstOrDefault(x => x.Id == id);
            if (entity != null)
                await _tagSlugService.DeleteAsync(entity);

            return new NullJsonResult();
        }

        // ════════════════════════════════════════════════════════════════
        //  SEGMENT SLUG MAPPINGS  (second segment)
        // ════════════════════════════════════════════════════════════════
        [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
        public async Task<IActionResult> SegmentSlugs()
        {
    

            return View(
                new SegmentSlugMappingSearchModel());
        }

        [HttpPost]
        public async Task<IActionResult> SegmentSlugList(SegmentSlugMappingSearchModel searchModel)
        {
            var all = await _segmentSlugService.GetAllAsync();

            var pagedData = all.ToPagedList(searchModel);



            //prepare grid model
            var model = await new SegmentSlugMappingListModel().PrepareToGridAsync(searchModel, pagedData, () =>
            {
                return pagedData.SelectAwait(async x =>
                {
                    return new SegmentSlugMappingModel
                    {
                        Id = x.Id,
                        Slug = x.Slug,
                        Label = x.Label,
                        SlugType = x.SlugType,
                        CategoryId = x.CategoryId,
                        SpecificationAttributeId = x.SpecificationAttributeId,
                        SpecificationAttributeOptionId = x.SpecificationAttributeOptionId,
                        SortOrder = x.SortOrder,
                        IsActive = x.IsActive,
                        TagSlug = (await _tagSlugService.GetById(x.TagId))?.Slug ?? string.Empty
                    };
                });
            });
            return Json(model);
        }

        public async Task<IActionResult> CreateSegmentSlug()
        {
            var model = new SegmentSlugMappingModel();
            await PopulateSegmentDropdownsAsync(model);
            return View("CreateEditSegmentSlug", model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSegmentSlug(SegmentSlugMappingModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateSegmentDropdownsAsync(model);
                return View("CreateEditSegmentSlug",model);
            }
            model.Slug = await _urlRecordService.ValidateSeNameAsync(model.Id, "Segment", model.Slug, model.Label, false);
            var segments = await _segmentSlugService.GetAllAsync();
            if (segments.Where(x => x.Slug == model.Slug).Any())
            {
                ModelState.AddModelError(string.Empty, "Admin.TagPages.SegmentSlug.Error.Slug.AlreadyInUse");
                await PopulateSegmentDropdownsAsync(model);
                return View("CreateEditSegmentSlug", model);
            }

            await _segmentSlugService.InsertAsync(new SegmentSlugMapping
            {
                Slug = model.Slug.ToLowerInvariant().Trim(),
                Label = model.Label,
                TagId = model.TagId,
                SlugType = model.SlugType,
                CategoryId = model.SlugType == (int)SegmentSlugType.Category
                                                    ? model.CategoryId : null,
                SpecificationAttributeId = model.SlugType == (int)SegmentSlugType.Specification
                                                    ? model.SpecificationAttributeId : null,
                SpecificationAttributeOptionId = model.SlugType == (int)SegmentSlugType.Specification
                                                    ? model.SpecificationAttributeOptionId : null,
                SortOrder = model.SortOrder,
                IsActive = model.IsActive,
                MetaKeywords = model.MetaKeywords,
                MetaDescription = model.MetaDescription,
                MetaTitle = model.MetaTitle,
                Description = model.Description,
                AdditionalDescription = model.AdditionalDescription,
                EnableInfiniteScroll = model.EnableInfiniteScroll,
            });

            return RedirectToAction(nameof(SegmentSlugs));
        }

        public async Task<IActionResult> EditSegmentSlug(int id)
        {
            var entity = (await _segmentSlugService.GetAllAsync()).FirstOrDefault(x => x.Id == id);
            if (entity == null) return NotFound();

            var model = new SegmentSlugMappingModel
            {
                Id = entity.Id,
                Slug = entity.Slug,
                TagId = entity.TagId,
                Label = entity.Label,
                SlugType = entity.SlugType,
                CategoryId = entity.CategoryId,
                SpecificationAttributeId = entity.SpecificationAttributeId,
                SpecificationAttributeOptionId = entity.SpecificationAttributeOptionId,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive,
                MetaKeywords = entity.MetaKeywords,
                MetaDescription = entity.MetaDescription,
                MetaTitle = entity.MetaTitle,
                Description = entity.Description,
                AdditionalDescription = entity.AdditionalDescription,
                EnableInfiniteScroll = entity.EnableInfiniteScroll
            };

            await PopulateSegmentDropdownsAsync(model);
            return View("CreateEditSegmentSlug", model);
        }

        [HttpPost]
        public async Task<IActionResult> EditSegmentSlug(SegmentSlugMappingModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateSegmentDropdownsAsync(model);
                return View("CreateEditSegmentSlug",model);
            }

            var segments = await _segmentSlugService.GetAllAsync();
            var entity = segments.FirstOrDefault(x => x.Id == model.Id);

            if (entity == null) return NotFound();
            model.Slug = await _urlRecordService.ValidateSeNameAsync(model.Id, "Segment", model.Slug, model.Label, false);

            if (segments.Where(x => x.Slug == model.Slug && x.Id != model.Id).Any())
            {
                ModelState.AddModelError(string.Empty, "Admin.TagPages.SegmentSlug.Error.Slug.AlreadyInUse");
                await PopulateSegmentDropdownsAsync(model);
                return View("CreateEditSegmentSlug", model);
            }



            entity.Slug = model.Slug.ToLowerInvariant().Trim();
            entity.Label = model.Label;
            entity.SlugType = model.SlugType;
            entity.TagId = model.TagId;
            entity.CategoryId = model.SlugType == (int)SegmentSlugType.Category
                                                        ? model.CategoryId : null;
            entity.SpecificationAttributeId = model.SlugType == (int)SegmentSlugType.Specification
                                                        ? model.SpecificationAttributeId : null;
            entity.SpecificationAttributeOptionId = model.SlugType == (int)SegmentSlugType.Specification
                                                        ? model.SpecificationAttributeOptionId : null;
            entity.SortOrder = model.SortOrder;
            entity.IsActive = model.IsActive;
            entity.MetaKeywords = model.MetaKeywords;
            entity.MetaDescription = model.MetaDescription;
            entity.MetaTitle = model.MetaTitle;
            entity.Description = model.Description;
            entity.AdditionalDescription = model.AdditionalDescription;
            entity.EnableInfiniteScroll = model.EnableInfiniteScroll;

            await _segmentSlugService.UpdateAsync(entity);
            return RedirectToAction(nameof(SegmentSlugs));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSegmentSlug(int id)
        {
            var entity = (await _segmentSlugService.GetAllAsync()).FirstOrDefault(x => x.Id == id);
            if (entity != null)
                await _segmentSlugService.DeleteAsync(entity);

            return new NullJsonResult();
        }

        // AJAX: load spec options for a given spec attribute
        [HttpGet]
        public async Task<IActionResult> GetSpecOptions(int specAttributeId)
        {
            var options = await _specService
                .GetSpecificationAttributeOptionsBySpecificationAttributeAsync(specAttributeId);

            var result = options.Select(o => new { value = o.Id, text = o.Name });
            return Json(result);
        }

        // ════════════════════════════════════════════════════════════════
        //  Helpers
        // ════════════════════════════════════════════════════════════════

        private async Task PopulateTagDropdownAsync(TagSlugMappingModel model)
        {
            var tags = await _productTagService.GetAllProductTagsAsync();
            model.AvailableTags = tags.Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = t.Name,
                Selected = t.Id == model.TagId
            }).ToList();
        }

        private async Task PopulateSegmentDropdownsAsync(SegmentSlugMappingModel model)
        {
            model.AvailableSlugTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "All (no filter)" },
                new SelectListItem { Value = "1", Text = "Category" }//,
            //    new SelectListItem { Value = "2", Text = "Specification Attribute" }
            };
            var tags = await _tagSlugService.GetAllAsync();
            model.AvailableTagSlugs = tags.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Label,
                Selected = c.Id == model.TagId
            }).ToList();
            // Categories
            var categories = await _categoryService.GetAllCategoriesAsync();
            model.AvailableCategories = categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name,
                Selected = c.Id == model.CategoryId
            }).ToList();
            model.AvailableCategories.Insert(0,
                new SelectListItem { Value = "", Text = "— Select Category —" });

            // Specification Attributes
            var specs = await _specService.GetAllSpecificationAttributesAsync();
            model.AvailableSpecAttributes = specs.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.Name,
                Selected = s.Id == model.SpecificationAttributeId
            }).ToList();
            model.AvailableSpecAttributes.Insert(0,
                new SelectListItem { Value = "", Text = "— Select Attribute —" });

            // Spec options (pre-load for currently selected attribute)
            if (model.SpecificationAttributeId.HasValue)
            {
                var options = await _specService
                    .GetSpecificationAttributeOptionsBySpecificationAttributeAsync(
                        model.SpecificationAttributeId.Value);

                model.AvailableSpecOptions = options.Select(o => new SelectListItem
                {
                    Value = o.Id.ToString(),
                    Text = o.Name,
                    Selected = o.Id == model.SpecificationAttributeOptionId
                }).ToList();
            }

            model.AvailableSpecOptions.Insert(0,
                new SelectListItem { Value = "", Text = "— Select Option —" });
        }
    }
}
