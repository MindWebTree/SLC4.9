using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Infrastructure;
using MWT.Nop.Core.Services.TagPage;
using MWT.Plugin.Misc.MwtStorefront.Factories.TagPage;
using MWT.Plugin.Misc.MwtStorefront.Models.Catalog;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Seo;
using Nop.Web.Controllers;
using Nop.Web.Factories;

namespace MWT.Plugin.Misc.MwtStorefront.Controllers
{
    public class TagCategoryController : BasePublicController
    {
        private readonly ITagSlugService _tagSlugService;
        private readonly ISegmentSlugService _segmentSlugService;
        private readonly IProductTagService _productTagService;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ISpecificationAttributeService _specService;
        private readonly ICatalogModelFactory _catalogModelFactory;
        private readonly IWebHelper _webHelper;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IWorkContext _workContext;
        private readonly ICustomWorkContext _customWorkContext;
        private readonly IStoreContext _storeContext;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly ITagModelFactory _tagModelFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUrlRecordService _urlRecordService;
        private readonly SitemapXmlSettings _sitemapXmlSettings;
        public TagCategoryController(
            ITagSlugService tagSlugService,
            ISegmentSlugService segmentSlugService,
            IProductTagService productTagService,
            IProductService productService,
            ICategoryService categoryService,
            ISpecificationAttributeService specService,
            ICatalogModelFactory catalogModelFactory,
            IWebHelper webHelper,
            IGenericAttributeService genericAttributeService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            ITagModelFactory tagModelFactory,
            IHttpContextAccessor httpContextAccessor,
            IUrlRecordService urlRecordService,
            SitemapXmlSettings sitemapXmlSettings,
            ICustomWorkContext customWorkContext)
        {
            _tagSlugService = tagSlugService;
            _segmentSlugService = segmentSlugService;
            _productTagService = productTagService;
            _productService = productService;
            _categoryService = categoryService;
            _specService = specService;
            _catalogModelFactory = catalogModelFactory;
            _webHelper = webHelper;
            _genericAttributeService = genericAttributeService;
            _workContext = workContext;
            _storeContext = storeContext;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _tagModelFactory = tagModelFactory;
            _httpContextAccessor = httpContextAccessor;
            _urlRecordService = urlRecordService;
            _sitemapXmlSettings = sitemapXmlSettings;
            _customWorkContext = customWorkContext;
        }

        // GET /{tagSlug}
        // GET /{tagSlug}/{segmentSlug}
        [HttpGet]
        public async Task<IActionResult> Index(
            string tagSlug,
            string segmentSlug = "all",
            CustomCatalogProductsCommand command = null)
        {
            command ??= new CustomCatalogProductsCommand();

            var tagMapping = await _tagSlugService.GetBySlugAsync(tagSlug);
            if (tagMapping == null)
                return InvokeHttp404();


            var tag = await _productTagService.GetProductTagByIdAsync(tagMapping.TagId);
            if (tag == null)
                return InvokeHttp404();

            int categoryId = 0;
            Category category = new Category();
            string categoryName = null;
            if (!string.Equals(segmentSlug, "all", StringComparison.InvariantCultureIgnoreCase))
            {
                var urlRecord = await _urlRecordService.GetBySlugAsync(segmentSlug);
                if (urlRecord == null || !urlRecord.IsActive)
                    return InvokeHttp404();

                if (!urlRecord.EntityName.Equals("Category", StringComparison.InvariantCultureIgnoreCase))
                    return InvokeHttp404();
                categoryId = urlRecord.EntityId;
                category = await _categoryService.GetCategoryByIdAsync(categoryId);
            }


            await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
              NopCustomerDefaults.LastContinueShoppingPageAttribute,
              _webHelper.GetThisPageUrl(false),
              (await _storeContext.GetCurrentStoreAsync()).Id);

            await _customerActivityService.InsertActivityAsync("PublicStore.View.Tag.Category",
               string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.ViewTagCategory"), $"{tagMapping.Label} {categoryName}"), tag);

            string templateViewPath = "CategoryTemplate-FilterOnSide-3x3";
            string listingViewPath = "_ProductsInGridOrLines-3x3";
            string filterViewPath = "_AjaxFilterSpecsBox";
            string FilterViewPathForMobile = "_AjaxFilterSpecsBox";
            int pictureSize = 415;
            bool isHorizontal = false;


            var model = await _tagModelFactory.PrepareTagModelAsync(tagMapping, category, segmentSlug, command, _httpContextAccessor.HttpContext.Request.QueryString.Value, pictureSize);

            if (model.CatalogProductsModel != null)
            {
                model.CatalogProductsModel.CategoryID = model.Id;
                model.CatalogProductsModel.CategoryName = model.Name;
                model.CatalogProductsModel.Description = model.Description;
                model.CatalogProductsModel.Sename = model.SeName;
            }

            model.GridLineViewPath = listingViewPath;
            model.FilterViewPath = filterViewPath;
            model.FilterViewPathForMobile = FilterViewPathForMobile;


            return View($"{templateViewPath}", model);


        }
        public virtual async Task<IActionResult> GetProducts(int categoryId, int? specificationOptionId, CustomCatalogProductsCommand command, string queryString)
        {
            var tagSlug = await _tagSlugService.GetById(categoryId);
            if (tagSlug == null)
                return InvokeHttp404();

            var tag = await _productTagService.GetProductTagByIdAsync(tagSlug.TagId);
            if (tag == null)
                return InvokeHttp404();
            var category = new Category();
            string sename = string.Empty;
            if ((specificationOptionId ?? 0) > 0)
            {
                category = await _categoryService.GetCategoryByIdAsync((int)specificationOptionId);
                if (category == null)
                {
                    return InvokeHttp404();
                }
                else
                {
                    sename = await _urlRecordService.GetSeNameAsync(category);
                }
            }
            if (tag == null)
                return InvokeHttp404();



            string templateViewPath = "CategoryTemplate-FilterOnSide-3x3";
            string listingViewPath = "_ProductsInGridOrLines-3x3";
            string filterViewPath = "_AjaxFilterSpecsBox";
            string FilterViewPathForMobile = "_AjaxFilterSpecsBox";
            int pictureSize = 415;
            bool isHorizontal = false;

            var model = await _tagModelFactory.PrepareTagProductsModelAsync(tagSlug, category, command,
                _httpContextAccessor.HttpContext.Request.QueryString.Value, pictureSize: pictureSize);


            model.CategoryID = categoryId;
            model.CategoryName = $"{category?.Name ?? tagSlug.Label}";
            model.Description = tagSlug.Description;
            model.Sename = sename == string.Empty ? $"{tagSlug.Slug}" : $"{tagSlug.Slug}/{sename}";
            ViewData["openTabs"] = command.openTabs;
            ViewData["topOpenTabs"] = command.topOpenTabs;
            ViewData["popupOpenTabs"] = command.popupOpenTabs;
            if (_customWorkContext.IsMobileDevice())
            {
                return Json(new
                {

                    products = await RenderPartialViewToStringAsync(listingViewPath, model),
                    mobileFilters = await RenderPartialViewToStringAsync(FilterViewPathForMobile, model.SpecificationFilter),
                    topFilters = await RenderPartialViewToStringAsync("_AjaxFilterSpecsBoxTop", model),
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
                        horizontalFilters = await RenderPartialViewToStringAsync(filterViewPath, model.SpecificationFilter),
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
                        topFilters = await RenderPartialViewToStringAsync("_AjaxFilterSpecsBoxTop", model),
                        filters = string.Join(",", model.SpecificationFilter.Attributes.Select(m => m.Name.ToLower().Trim()).ToArray()),
                    });
                }
            }

        }

        //public virtual async Task<IActionResult> TagsSitemapXml(int? id)
        //{
        //    var siteMap = _sitemapXmlSettings.SitemapXmlEnabled
        //        ? await _tagModelFactory.PrepareTagSitemapXmlAsync(id) : string.Empty;
        //    return Content(siteMap, "text/xml");
        //}

    }
}
