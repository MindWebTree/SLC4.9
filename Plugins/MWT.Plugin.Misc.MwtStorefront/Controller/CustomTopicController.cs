using Microsoft.AspNetCore.Mvc;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Topics;
using Nop.Web.Models.Topics;
using Nop.Web.Framework;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Seo;
using Microsoft.AspNetCore.Http.Extensions;
using System.Security.Policy;
using System.Collections.Generic;
using Nop.Services.Configuration;
using Nop.Web.Controllers;
using Nop.Web.Factories;
using MWT.Plugin.Misc.MwtStorefront.Factories.Topics;
using MWT.Plugin.Misc.MwtStorefront.Models.Topics;
using MWT.Plugin.Misc.MwtStorefront.Components;

namespace MWT.Plugin.Misc.MwtStorefront.Controllers;

[AutoValidateAntiforgeryToken]
public partial class CustomTopicController : TopicController
{
    #region Fields

    private readonly ICustomTopicModelFactory _customTopicModelFactory;

    #endregion
    public CustomTopicController(IAclService aclService, ILocalizationService localizationService,
        IPermissionService permissionService, IStoreMappingService storeMappingService, ITopicModelFactory topicModelFactory,
        ITopicService topicService, ICustomTopicModelFactory customTopicModelFactory) :
        base(aclService, localizationService, permissionService, storeMappingService, topicModelFactory, topicService)
    {
        _customTopicModelFactory = customTopicModelFactory;
    }

    #region Methods

    public virtual async Task<IActionResult> CustomTopicDetails(int topicId)
    {
        var topic = await _topicService.GetTopicByIdAsync(topicId);

        if (topic == null)
            return InvokeHttp404();

        var notAvailable = !topic.Published ||
                   //availability dates
                   !_topicService.TopicIsAvailable(topic) ||
                   //ACL (access control list)
                   !await _aclService.AuthorizeAsync(topic) ||
                   //store mapping
                   !await _storeMappingService.AuthorizeAsync(topic);

        //allow administrators to preview any topic
        var hasAdminAccess = await _permissionService.AuthorizeAsync(StandardPermission.Security.ACCESS_ADMIN_PANEL) && await _permissionService.AuthorizeAsync(StandardPermission.ContentManagement.TOPICS_VIEW);

        if (notAvailable && !hasAdminAccess)
            return InvokeHttp404();


        var model = await _customTopicModelFactory.CustomPrepareTopicModelByIdAsync(topic);
        if (model == null)
            return InvokeHttp404();

        //display "edit" (manage) link
        if (hasAdminAccess)
            DisplayEditLink(Url.Action("Edit", "Topic", new { id = model.Id, area = AreaNames.ADMIN }));

        model = await ProcessTokens(model);
        //template
        var templateViewPath = await _topicModelFactory.PrepareTemplateViewPathAsync(model.TopicTemplateId);
        return View(templateViewPath, model);
    }
    #endregion

    #region utilities

    public async Task<CustomTopicModel> ProcessTokens(CustomTopicModel model)
    {
        if (!string.IsNullOrEmpty(model.Body))
        {
            var _categoryService = EngineContext.Current.Resolve<ICategoryService>();
            int categoryId = 0;
            string pattern = @"\[%(?'shortcode'[^\]]+)\%]";
            string sename = "";
            string shortcodeForHeadLinks = "";
            MatchCollection matches = Regex.Matches(model.Body, pattern);
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
                                model.Body = model.Body.Replace(shortcode,
                                    await RenderViewComponentToStringAsync(typeof(CategoryGroupedProductsViewComponent), new { entityId = categoryId }));
                                sename = Url.RouteUrl("Category", new
                                {
                                    SeName = await EngineContext.Current.Resolve<IUrlRecordService>().GetSeNameAsync(category),
                                    id = categoryId
                                });
                            }
                            else
                                model.Body = model.Body.Replace(shortcode, "");
                        }


                    }
                    else
                        model.Body = model.Body.Replace(shortcode, "");
                }
                else if (match.Groups["shortcode"].Value.ToLower().Contains("headlinks"))
                    shortcodeForHeadLinks = shortcode;
                else if (match.Groups["shortcode"].Value.ToLower().Contains("testimonials"))
                {
                    model.Body = model.Body.Replace(shortcode,
                        await RenderViewComponentToStringAsync(typeof(TestimonialsBlockViewComponent)));
                }
                else if (match.Groups["shortcode"].Value.ToLower().Contains("custom_form"))
                {
                    string formName = GetShortCodeAttrValue(shortcode, "name");
                    if (!string.IsNullOrEmpty(formName))
                    {
                        string heading = GetShortCodeAttrValue(shortcode, "heading");
                        model.Body = model.Body.Replace(shortcode,
                            await RenderViewComponentToStringAsync(typeof(CustomFormViewComponent), new { formname = formName, heading = heading }));
                    }
                    else
                        model.Body = model.Body.Replace(shortcode, "");
                }
                else if (match.Groups["shortcode"].Value.ToLower().Contains("best_seller "))
                {
                    var _settingService = EngineContext.Current.Resolve<ISettingService>();
                    model.Body = model.Body.Replace(shortcode,
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

                        model.Body = model.Body.Replace(shortcode,
                                     await RenderViewComponentToStringAsync(typeof(Custom_ProductListingByProductIdsViewComponent), new { entityId = categoryId, type = _type, productids = _productids }));

                    }
                    else
                        model.Body = model.Body.Replace(shortcode, "");
                }
                else if (match.Groups["shortcode"].Value.ToLower().Contains("questionanswer_grid"))
                {
                    int.TryParse(GetShortCodeAttrValue(shortcode, "pagesize"), out int pageSize);
                    pageSize = pageSize == 0 ? 15 : pageSize;
                    model.Body = model.Body.Replace(shortcode,
                            await RenderViewComponentToStringAsync(typeof(Custom_QuestionAnswerGridBlockViewComponent), new { pageSize = pageSize }));
                }
                else
                    model.Body = model.Body.Replace(shortcode, "");
            }

            if (!string.IsNullOrEmpty(shortcodeForHeadLinks))
            {
                if (!string.IsNullOrEmpty(sename))
                    model.Body = model.Body.Replace(shortcodeForHeadLinks, await RenderViewComponentToStringAsync(typeof(HeadLinksViewComponent), new { catSeName = sename, categoryId = categoryId, currentPageurl = Url.RouteUrl("Topic", new { SeName = model.SeName }) }));
                else
                    model.Body = model.Body.Replace(shortcodeForHeadLinks, "");
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
    #endregion


}
