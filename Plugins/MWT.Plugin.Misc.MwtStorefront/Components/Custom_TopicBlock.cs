using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MWT.Nop.Core.Infrastructure;
using MWT.Plugin.Misc.MwtStorefront.Models.Topics;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Seo;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Topics;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace MWT.Plugin.Misc.MwtStorefront.Components
{
    [ViewComponent(Name = "Custom_TopicBlock")]
    public class Custom_TopicBlockViewComponent : NopViewComponent
    {
        private readonly ITopicModelFactory _topicModelFactory;
        private readonly ViewComponentRenderHelper _viewComponentRenderer;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly ICategoryService _categoryService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly ILogger<Custom_TopicBlockViewComponent> _logger;
        public Custom_TopicBlockViewComponent(ITopicModelFactory topicModelFactory, ViewComponentRenderHelper viewComponentRenderer,
            ILocalizationService localizationService, ISettingService settingService, ICategoryService categoryService, IUrlRecordService urlRecordService
            , ILogger<Custom_TopicBlockViewComponent> logger)
        {
            _topicModelFactory = topicModelFactory;
            _viewComponentRenderer = viewComponentRenderer;
            _localizationService = localizationService;
            _settingService = settingService;
            _categoryService = categoryService;
            _urlRecordService = urlRecordService;
            _logger = logger;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync(string systemName)
        {
            var model = await _topicModelFactory.PrepareTopicModelBySystemNameAsync(systemName);

          
            if (model == null)
                return Content("");
            try
            {
                model = await ProcessTokens(model);
           
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing tokens in Custom_TopicBlockViewComponent");
            }
            CustomTopicModel customTopicModel = new CustomTopicModel
            {
                Id = model.Id,
                SystemName = model.SystemName,
                IncludeInSitemap = model.IncludeInSitemap,
                IsPasswordProtected = model.IsPasswordProtected,
                Title = model.Title,
                Body = model.Body,
                MetaKeywords = model.MetaKeywords,
                MetaDescription = model.MetaDescription,
                MetaTitle = model.MetaTitle,
                SeName = model.SeName,
                TopicTemplateId = model.TopicTemplateId,
                HideDefualtTitle = true
            };
            return View(customTopicModel);
        }


        public async Task<TopicModel> ProcessTokens(TopicModel model)
        {
            if (!string.IsNullOrEmpty(model.Body))
            {   
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
                                        await _viewComponentRenderer.RenderViewComponentToStringAsync("CategoryGroupedProducts", new { entityId = categoryId }));
                                    sename = Url.RouteUrl("Category", new
                                    {
                                        SeName = await _urlRecordService.GetSeNameAsync(category),
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
                            await _viewComponentRenderer.RenderViewComponentToStringAsync("TestimonialsBlock"));
                    }

                    else if (match.Groups["shortcode"].Value.ToLower().Contains("custom_form"))
                    {
                        string formName = GetShortCodeAttrValue(shortcode, "name");
                        if (!string.IsNullOrEmpty(formName))
                        {
                            string heading = GetShortCodeAttrValue(shortcode, "heading");
                            model.Body = model.Body.Replace(shortcode,
                                await _viewComponentRenderer.RenderViewComponentToStringAsync("CustomForm", new { formname = formName, heading = heading }));
                        }
                        else
                            model.Body = model.Body.Replace(shortcode, "");
                    }
                    else if (match.Groups["shortcode"].Value.ToLower().Contains("best_seller "))
                    {

                        model.Body = model.Body.Replace(shortcode,
                                     await _viewComponentRenderer.RenderViewComponentToStringAsync("Custom_Product_Listing_by_Setting", new
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
                                         await _viewComponentRenderer.RenderViewComponentToStringAsync("Custom_ProductListingByProductIds", new { entityId = categoryId, type = _type, productids = _productids }));

                        }
                        else
                            model.Body = model.Body.Replace(shortcode, "");
                    }
                    else if (match.Groups["shortcode"].Value.ToLower().Contains("questionanswer_grid"))
                    {
                        int.TryParse(GetShortCodeAttrValue(shortcode, "pagesize"), out int pageSize);
                        pageSize = pageSize == 0 ? 15 : pageSize;
                        model.Body = model.Body.Replace(shortcode,
                                await _viewComponentRenderer.RenderViewComponentToStringAsync("Custom_QuestionAnswerGridBlock", new { pageSize = pageSize }));
                    }
                    else if (match.Groups["shortcode"].Value.ToLower().Contains("newarrivalproducts"))
                    {
                        string heading = GetShortCodeAttrValue(shortcode, "heading");
                        model.Body = model.Body.Replace(shortcode,
                            await _viewComponentRenderer.RenderViewComponentToStringAsync("NewArrivalProducts", new { heading = heading }));
                    }
                    else if (match.Groups["shortcode"].Value.ToLower().Contains("topseller"))
                    {
                        string heading = GetShortCodeAttrValue(shortcode, "heading");
                        model.Body = model.Body.Replace(shortcode,
                            await _viewComponentRenderer.RenderViewComponentToStringAsync("Custom_Product_Listing_by_Setting",
                            new
                            {
                                heading = heading,
                                productIds = await _settingService.GetSettingByKeyAsync<string>("CatalogSettings.HomePage.TopSeller.productids"),
                                sectionId = "section_topseller",
                                noOfProductsToDisplay = await _settingService.GetSettingByKeyAsync<int>("CatalogSettings.HomePage.TopSeller.NoOfProductsToDisplay"),
                                type = ProductType.SimpleProduct
                            }));
                    }
                    else
                        model.Body = model.Body.Replace(shortcode, "");
                }

                if (!string.IsNullOrEmpty(shortcodeForHeadLinks))
                {
                    if (!string.IsNullOrEmpty(sename))
                        model.Body = model.Body.Replace(shortcodeForHeadLinks, await _viewComponentRenderer.RenderViewComponentToStringAsync("HeadLinks", new { catSeName = sename, categoryId = categoryId, currentPageurl = Url.RouteUrl("Topic", new { SeName = model.SeName }) }));
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
    }
}

