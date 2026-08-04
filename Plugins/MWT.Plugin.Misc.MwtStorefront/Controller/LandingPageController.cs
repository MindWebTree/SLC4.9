using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Services.LandingPage_Management;
using MWT.Nop.Core.Services.Seo;
using MWT.Plugin.Misc.MwtStorefront.Components;
using MWT.Plugin.Misc.MwtStorefront.Factories;
using MWT.Plugin.Misc.MwtStorefront.Models.LandingPage_Management;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Web.Controllers;
using Nop.Web.Framework;
using System.Text.RegularExpressions;

namespace MWT.Plugin.Misc.MwtStorefront.Controllers
{
    public partial class LandingPageController : BasePublicController
    {
        private readonly ILandingPageModelFactory _landingPageModelFactory;
        private readonly ILandingPageService _landingPageService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomUrlRecordService _urlRecordService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IPermissionService _permissionService;
        private ICustomerActivityService _customerActivityService;

        public LandingPageController(
            ILandingPageModelFactory landingPageModelFactory,
            ILandingPageService landingPageService,
            ILocalizationService localizationService,
            ICustomUrlRecordService urlRecordService,
            IGenericAttributeService genericAttributeService,
            IWebHelper webHelper,
            IWorkContext workContext,
            IStoreContext storeContext,
            IPermissionService permissionService, ICustomerActivityService customerActivityService)
        {
            this._landingPageModelFactory = landingPageModelFactory;
            this._landingPageService = landingPageService;
            this._localizationService = localizationService;
            this._urlRecordService = urlRecordService;
            this._genericAttributeService = genericAttributeService;
            this._webHelper = webHelper;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._permissionService = permissionService;
            this._customerActivityService = customerActivityService;
        }
        public virtual async Task<IActionResult> Index(string SeName)
        {
            var urlRecord = await _urlRecordService.GetByLandingPageSlugAsync(SeName);

            if (urlRecord == null)
            {
                return StatusCode(404);
            }
            var landingPage = await _landingPageService.GetLandingPageByIdAsync(urlRecord.EntityId);

            if (landingPage == null || !landingPage.Published)
            {
                return StatusCode(404);
            }

            await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
        NopCustomerDefaults.LastContinueShoppingPageAttribute,
            _webHelper.GetThisPageUrl(false),
        (await _storeContext.GetCurrentStoreAsync()).Id);


            if (await _permissionService.AuthorizeAsync(StandardPermission.Security.ACCESS_ADMIN_PANEL) && await _permissionService.AuthorizeAsync(StandardPermission.ContentManagement.TOPICS_CREATE_EDIT_DELETE))
                DisplayEditLink(Url.Action("Edit", "LandingPage", new { id = landingPage.Id, area = AreaNames.ADMIN }));


            await _customerActivityService.InsertActivityAsync("PublicStore.LandingPage",
     string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.LandingPage"), landingPage.Name), landingPage);



            var model = await _landingPageModelFactory.PrepareCustomLandingPageModelAsync(landingPage);
            model = await ProcessTokens(model);
            return View("LandingPageDetails",model);
        }

        public async Task<LandingPageModel> ProcessTokens(LandingPageModel model)
        {
            if (!string.IsNullOrEmpty(model.PageContent))
            {
                var _categoryService = EngineContext.Current.Resolve<ICategoryService>();
                int categoryId = 0;
                string pattern = @"\[%(?'shortcode'[^\]]+)\%]";
                string sename = "";
                string shortcodeForHeadLinks = "";
                MatchCollection matches = Regex.Matches(model.PageContent, pattern);
                foreach (Match match in matches)
                {
                    string shortcode = "[%" + match.Groups["shortcode"].Value + "%]";
                    if (match.Groups["shortcode"].Value.ToLower().Contains("grouped_products") && match.Groups["shortcode"].Value.ToLower().Contains("categoryid"))
                    {
                        string _categoryid = GetShortCodeAttrValue(shortcode, "categoryid");


                        if (!string.IsNullOrEmpty(_categoryid))
                        {
                            int.TryParse(_categoryid, out categoryId);
                            if (categoryId != 0)
                            {
                                var category = await _categoryService.GetCategoryByIdAsync(categoryId);
                                if (category != null)
                                {
                                    model.PageContent = model.PageContent.Replace(shortcode,
                                        await RenderViewComponentToStringAsync(typeof(CategoryGroupedProductsViewComponent), new { entityId = categoryId }));
                                    sename = Url.RouteUrl("Category", new
                                    {
                                        SeName = await EngineContext.Current.Resolve<IUrlRecordService>().GetSeNameAsync(category),
                                        id = categoryId
                                    });
                                }
                                else
                                    model.PageContent = model.PageContent.Replace(shortcode, "");
                            }


                        }
                        else
                            model.PageContent = model.PageContent.Replace(shortcode, "");
                    }
                    else if (match.Groups["shortcode"].Value.ToLower().Contains("headlinks"))
                        shortcodeForHeadLinks = shortcode;
                    else if (match.Groups["shortcode"].Value.ToLower().Contains("description"))
                    {
                        model.PageContent = model.PageContent.Replace(shortcode,
                            model.Description??string.Empty);
                    }
                    else if (match.Groups["shortcode"].Value.ToLower().Contains("testimonials"))
                    {
                        model.PageContent = model.PageContent.Replace(shortcode,
                            await RenderViewComponentToStringAsync(typeof(TestimonialsBlockViewComponent)));
                    }
                    else if (match.Groups["shortcode"].Value.ToLower().Contains("custom_form"))
                    {
                        string formName = GetShortCodeAttrValue(shortcode, "name");
                        if (!string.IsNullOrEmpty(formName))
                        {
                            string heading = GetShortCodeAttrValue(shortcode, "heading");
                            model.PageContent = model.PageContent.Replace(shortcode,
                                await RenderViewComponentToStringAsync(typeof(CustomFormViewComponent), new { formname = formName, heading = heading }));
                        }
                        else
                            model.PageContent = model.PageContent.Replace(shortcode, "");
                    }
                    else if (match.Groups["shortcode"].Value.ToLower().Contains("best_sellers "))
                    {
                        var _settingService = EngineContext.Current.Resolve<ISettingService>();
                        model.PageContent = model.PageContent.Replace(shortcode,
                                     await RenderViewComponentToStringAsync(typeof(Custom_Product_Listing_by_SettingViewComponent), new
                                     {
                                         productIds = await _settingService.GetSettingByKeyAsync<string>("CatalogSettings.HomePage.TopSeller.productids"),
                                         heading = await _localizationService.GetResourceAsync("Topic.Heading.BestSeller"),
                                         sectionId = "section_bestseller",
                                         noOfProductsToDisplay = await _settingService.GetSettingByKeyAsync<int>("CatalogSettings.HomePage.TopSeller.NoOfProductsToDisplay"),
                                         type = ProductType.SimpleProduct
                                     }));
                    }
                    else if (match.Groups["shortcode"].Value.ToLower().Contains("product_listing ")
                        && match.Groups["shortcode"].Value.ToLower().Contains("type") && match.Groups["shortcode"].Value.ToLower().Contains("productids"))
                    {
                        string _type = GetShortCodeAttrValue(shortcode, "type");
                        string _productids = GetShortCodeAttrValue(shortcode, "productids");

                        if (!string.IsNullOrEmpty(_type))
                        {

                            model.PageContent = model.PageContent.Replace(shortcode,
                                         await RenderViewComponentToStringAsync(typeof(Custom_ProductListingByProductIdsViewComponent), new { entityId = categoryId, type = _type, productids = _productids }));

                        }
                        else
                            model.PageContent = model.PageContent.Replace(shortcode, "");
                    }
                    else if (match.Groups["shortcode"].Value.ToLower().Contains("questionanswer_grid"))
                    {
                        int.TryParse(GetShortCodeAttrValue(shortcode, "pagesize"), out int pageSize);
                        pageSize = pageSize == 0 ? 15 : pageSize;
                        model.PageContent = model.PageContent.Replace(shortcode,
                                await RenderViewComponentToStringAsync(typeof(Custom_QuestionAnswerGridBlockViewComponent), new { pageSize = pageSize }));
                    }
                    else if (match.Groups["shortcode"].Value.ToLower().Contains("reviews_page "))
                    {
                        int.TryParse(GetShortCodeAttrValue(shortcode, "pagesize"), out int pageSize);
                        int.TryParse(GetShortCodeAttrValue(shortcode, "categoryid"), out categoryId);
                        int.TryParse(GetShortCodeAttrValue(shortcode, "productid"), out int productid);
                        string sku = GetShortCodeAttrValue(shortcode, "sku");
                 
                        pageSize = pageSize == 0 ? 15 : pageSize;
                        model.PageContent = model.PageContent.Replace(shortcode,
                                await RenderViewComponentToStringAsync(typeof(Custom_FeedBackBlockViewComponent), new
                                {
                                    pageSize = pageSize,
                                    productId = productid,
                                    sku = sku??String.Empty,
                                    mainCategoryId = categoryId,
                                    IsReviewPage = false
                                }));
                    }
                    else
                        model.PageContent = model.PageContent.Replace(shortcode, "");
                }

                if (!string.IsNullOrEmpty(shortcodeForHeadLinks))
                {
                    if (!string.IsNullOrEmpty(sename))
                        model.PageContent = model.PageContent.Replace(shortcodeForHeadLinks, await RenderViewComponentToStringAsync(typeof(HeadLinksViewComponent), new { catSeName = sename, categoryId = categoryId, currentPageurl = Url.RouteUrl("Topic", new { SeName = model.SeName }) }));
                    else
                        model.PageContent = model.PageContent.Replace(shortcodeForHeadLinks, "");
                }
            }
            return model;

        }

        protected string GetShortCodeAttrValue(string shortcode, string attrName)
        {
            string attrValue = "";
            List<string> patterns = new List<string>();
            patterns.Add(@"" + attrName + @"(.*?)=\""(.*?)\""");
            patterns.Add(@"" + attrName + @"(.*?)=(.*?)\""(.*?)\""");
            patterns.Add(@"" + attrName + @"(.*?)=(.*?)\""");
            patterns.Add(@"" + attrName + @"(.*?)=(.*?) ");
            patterns.Add(@"" + attrName + @"(.*?)=(.*?)%");

            MatchCollection matches = null;
            foreach (var _pattern in patterns)
            {
                matches = Regex.Matches(shortcode, _pattern);
                if (matches.Count > 0)
                    break;
            }
            if (matches.Count > 0)
            {
                attrValue = Regex.Replace(matches[0].Value.ToLower(), attrName + "(.*?)=", "");
                attrValue = attrValue.Replace("=", "").Replace("%", "").Replace("\"", "").Trim().Replace("%", "").Trim();
            }
            return attrValue;
        }
    }
}
