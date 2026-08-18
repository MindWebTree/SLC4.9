using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using MWT.Nop.Core.Domain.KW;
using MWT.Nop.Core.Services.KW;
using MWT.Nop.Core.Services.Media;
using MWT.Nop.Core.Services.QA;

using MWT.Plugin.Misc.MwtStorefront.Models.Sitemap;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Seo;
using Nop.Core.Events;
using Nop.Core.Http;
using Nop.Core.Infrastructure;
using Nop.Services.Blogs;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.News;
using Nop.Services.Seo;
using Nop.Services.Topics;
using Nop.Web.Factories;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Sitemap;
using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace MWT.Plugin.Misc.MwtStorefront.Factories
{
    public partial class SiteMapExtendedModelFactory : SitemapModelFactory, ISiteMapExtendedModelFactory
    {
        #region Fields

        private readonly IQuestionAnswerService _questionAnswerService;
        private readonly IKwTermService _kwTermService;
        private readonly IPictureExtendedService _pictureService;
        private readonly MediaSettings _mediaSettings;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IUrlRecordService _urlRecordService;

        #endregion
        public SiteMapExtendedModelFactory(BlogSettings blogSettings, ForumSettings forumSettings, IBlogService blogService, ICategoryService categoryService, ICustomerService customerService,
            IEventPublisher eventPublisher, IHttpContextAccessor httpContextAccessor, ILanguageService languageService, ILocalizationService localizationService,
            ILocker locker, IManufacturerService manufacturerService, INewsService newsService, INopFileProvider nopFileProvider, INopUrlHelper nopUrlHelper,
            IProductService productService, IProductTagService productTagService, IStaticCacheManager staticCacheManager, IStoreContext storeContext, ITopicService topicService,
            IWebHelper webHelper, IWorkContext workContext, LocalizationSettings localizationSettings, NewsSettings newsSettings, SitemapSettings sitemapSettings, SitemapXmlSettings sitemapXmlSettings,
            IQuestionAnswerService questionAnswerService, IKwTermService kwTermService, IPictureExtendedService pictureService, MediaSettings mediaSettings,
            IActionContextAccessor actionContextAccessor, IUrlHelperFactory urlHelperFactory, IUrlRecordService urlRecordService)
            : base(blogSettings, forumSettings, blogService, categoryService, customerService, eventPublisher, httpContextAccessor, languageService, localizationService, locker, manufacturerService, newsService, nopFileProvider, nopUrlHelper, productService, productTagService, staticCacheManager, storeContext, topicService, webHelper, workContext, localizationSettings, newsSettings, sitemapSettings, sitemapXmlSettings)
        {
            _questionAnswerService = questionAnswerService;
            _kwTermService = kwTermService;
            _pictureService = pictureService;
            _mediaSettings = mediaSettings;
            _actionContextAccessor = actionContextAccessor;
            _urlHelperFactory = urlHelperFactory;
            _urlRecordService = urlRecordService;
        }





        #region Methoods


        public virtual async Task<SitemapExtendedModel> PrepareSitemapExtendedModelAsync(SitemapPageExtendedModel pageModel)
        {
            if (pageModel == null)
                throw new ArgumentNullException(nameof(pageModel));

            var language = await _workContext.GetWorkingLanguageAsync();
            var customer = await _workContext.GetCurrentCustomerAsync();
            var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
            var store = await _storeContext.GetCurrentStoreAsync();
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.SitemapPageModelKey,
                language, customerRoleIds, store);

            var cachedModel = await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                var model = new SitemapExtendedModel();

                //prepare common items
                var commonGroupTitle = await _localizationService.GetResourceAsync("Sitemap.General");

                //home page
                model.Items.Add(new SitemapExtendedModel.SitemapItemExtendedModel
                {
                    GroupTitle = commonGroupTitle,
                    Name = await _localizationService.GetResourceAsync("Homepage"),
                    Url = _nopUrlHelper.RouteUrl(NopRouteNames.General.HOMEPAGE)
                });

                //search
                model.Items.Add(new SitemapExtendedModel.SitemapItemExtendedModel
                {
                    GroupTitle = commonGroupTitle,
                    Name = await _localizationService.GetResourceAsync("sitemap.Search"),
                    Url = _nopUrlHelper.RouteUrl(NopRouteNames.General.SEARCH)
                });

                //news
                if (_newsSettings.Enabled)
                {
                    model.Items.Add(new SitemapExtendedModel.SitemapItemExtendedModel
                    {
                        GroupTitle = commonGroupTitle,
                        Name = await _localizationService.GetResourceAsync("News"),
                        Url = _nopUrlHelper.RouteUrl(NopRouteNames.General.NEWS)
                    });
                }

                //blog
                if (_blogSettings.Enabled)
                {
                    model.Items.Add(new SitemapExtendedModel.SitemapItemExtendedModel
                    {
                        GroupTitle = commonGroupTitle,
                        Name = await _localizationService.GetResourceAsync("Blog"),
                        Url = _nopUrlHelper.RouteUrl(NopRouteNames.General.BLOG)
                    });
                }

                //forums
                if (_forumSettings.ForumsEnabled)
                {
                    model.Items.Add(new SitemapExtendedModel.SitemapItemExtendedModel
                    {
                        GroupTitle = commonGroupTitle,
                        Name = await _localizationService.GetResourceAsync("Forum.Forums"),
                        Url = _nopUrlHelper.RouteUrl(NopRouteNames.General.BOARDS)
                    });
                }

                //contact us
                model.Items.Add(new SitemapExtendedModel.SitemapItemExtendedModel
                {
                    GroupTitle = commonGroupTitle,
                    Name = await _localizationService.GetResourceAsync("ContactUs"),
                    Url = _nopUrlHelper.RouteUrl(NopRouteNames.General.CONTACT_US)
                });

                //customer info
                model.Items.Add(new SitemapExtendedModel.SitemapItemExtendedModel
                {
                    GroupTitle = commonGroupTitle,
                    Name = await _localizationService.GetResourceAsync("Account.MyAccount"),
                    Url = _nopUrlHelper.RouteUrl(NopRouteNames.General.CUSTOMER_INFO)
                });

                //at the moment topics are in general category too
                if (_sitemapSettings.SitemapIncludeTopics)
                {
                    var topics = (await _topicService.GetAllTopicsAsync(storeId: store.Id))
                        .Where(topic => topic.IncludeInSitemap);

                    model.Items.AddRange(await topics.SelectAwait(async topic => new SitemapExtendedModel.SitemapItemExtendedModel
                    {
                        GroupTitle = commonGroupTitle,
                        Name = await _localizationService.GetLocalizedAsync(topic, x => x.Title),
                        Url = await _nopUrlHelper.RouteGenericUrlAsync(topic)
                    }).ToListAsync());
                }

                //blog posts
                if (_sitemapSettings.SitemapIncludeBlogPosts && _blogSettings.Enabled)
                {
                    var blogPostsGroupTitle = await _localizationService.GetResourceAsync("Sitemap.BlogPosts");
                    var blogPosts = (await _blogService.GetAllBlogPostsAsync(storeId: store.Id))
                        .Where(p => p.IncludeInSitemap);

                    model.Items.AddRange(await blogPosts.SelectAwait(async post => new SitemapExtendedModel.SitemapItemExtendedModel
                    {
                        GroupTitle = blogPostsGroupTitle,
                        Name = post.Title,
                        Url = await _nopUrlHelper.RouteGenericUrlAsync(post, languageId: post.LanguageId, ensureTwoPublishedLanguages: false)
                    }).ToListAsync());
                }

                //news
                if (_sitemapSettings.SitemapIncludeNews && _newsSettings.Enabled)
                {
                    var newsGroupTitle = await _localizationService.GetResourceAsync("Sitemap.News");
                    var news = await _newsService.GetAllNewsAsync(storeId: store.Id);
                    model.Items.AddRange(await news.SelectAwait(async newsItem => new SitemapExtendedModel.SitemapItemExtendedModel
                    {
                        GroupTitle = newsGroupTitle,
                        Name = newsItem.Title,
                        Url = await _nopUrlHelper.RouteGenericUrlAsync(newsItem, languageId: newsItem.LanguageId, ensureTwoPublishedLanguages: false)
                    }).ToListAsync());
                }

                //categories
                if (_sitemapSettings.SitemapIncludeCategories)
                {
                    var categoriesGroupTitle = await _localizationService.GetResourceAsync("Sitemap.Categories");
                    var categories = await _categoryService.GetAllCategoriesAsync(storeId: store.Id);
                    model.Items.AddRange(await categories.SelectAwait(async category => new SitemapExtendedModel.SitemapItemExtendedModel
                    {
                        Id = category.Id,
                        GroupTitle = categoriesGroupTitle,
                        ParentId = category.ParentCategoryId,
                        Name = await _localizationService.GetLocalizedAsync(category, x => x.Name),
                        Url = (await GetLocalizedSitemapUrlAsync("Category", CustomGetSeoRouteParamsAwait(category), category.UpdatedOnUtc)).Location
                    }).ToListAsync());
                }

                //manufacturers
                if (_sitemapSettings.SitemapIncludeManufacturers)
                {
                    var manufacturersGroupTitle = await _localizationService.GetResourceAsync("Sitemap.Manufacturers");
                    var manufacturers = await _manufacturerService.GetAllManufacturersAsync(storeId: store.Id);
                    model.Items.AddRange(await manufacturers.SelectAwait(async manufacturer => new SitemapExtendedModel.SitemapItemExtendedModel
                    {
                        GroupTitle = manufacturersGroupTitle,
                        Name = await _localizationService.GetLocalizedAsync(manufacturer, x => x.Name),
                        Url = await _nopUrlHelper.RouteGenericUrlAsync(manufacturer)
                    }).ToListAsync());
                }

                //products
                if (_sitemapSettings.SitemapIncludeProducts)
                {
                    var productsGroupTitle = await _localizationService.GetResourceAsync("Sitemap.Products");
                    var products = await _productService.SearchProductsAsync(0, storeId: store.Id, visibleIndividuallyOnly: true);
                    model.Items.AddRange(await products.SelectAwait(async product => new SitemapExtendedModel.SitemapItemExtendedModel
                    {
                        GroupTitle = productsGroupTitle,
                        Name = await _localizationService.GetLocalizedAsync(product, x => x.Name),
                        Url = (await GetLocalizedSitemapUrlAsync("Product", CustomGetSeoRouteParamsAwait(product), product.UpdatedOnUtc)).Location
                    }).ToListAsync());
                }

                //product tags
                if (_sitemapSettings.SitemapIncludeProductTags)
                {
                    var productTagsGroupTitle = await _localizationService.GetResourceAsync("Sitemap.ProductTags");
                    var productTags = await _productTagService.GetAllProductTagsAsync();
                    model.Items.AddRange(await productTags.SelectAwait(async productTag => new SitemapExtendedModel.SitemapItemExtendedModel
                    {
                        GroupTitle = productTagsGroupTitle,
                        Name = await _localizationService.GetLocalizedAsync(productTag, x => x.Name),
                        Url = await _nopUrlHelper.RouteGenericUrlAsync(productTag)
                    }).ToListAsync());
                }

                return model;
            });

            //prepare model with pagination
            pageModel.PageSize = Math.Max(pageModel.PageSize, _sitemapSettings.SitemapPageSize);
            pageModel.PageNumber = Math.Max(pageModel.PageNumber, 1);

            var pagedItems = new PagedList<SitemapExtendedModel.SitemapItemExtendedModel>(cachedModel.Items, pageModel.PageNumber - 1, pageModel.PageSize);
            var sitemapModel = new SitemapExtendedModel { Items = pagedItems };
            sitemapModel.PageModel.LoadPagedList(pagedItems);

            return sitemapModel;
        }
        public virtual async Task<string> PrepareSitemapExtendedIndexXml()
        {
            var language = await _workContext.GetWorkingLanguageAsync();
            var customer = await _workContext.GetCurrentCustomerAsync();
            var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
            var store = await _storeContext.GetCurrentStoreAsync();
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.CustomSitemapSeoModelKey,
                "All", language, store);

            var siteMap = await _staticCacheManager.GetAsync(cacheKey, async () => await GenerateSitemapIndexXml());

            return siteMap;
        }
        public virtual async Task<string> PrepareProductSitemapXml()
        {
            var language = await _workContext.GetWorkingLanguageAsync();
            var customer = await _workContext.GetCurrentCustomerAsync();
            var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
            var store = await _storeContext.GetCurrentStoreAsync();
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.CustomSitemapSeoModelKey,
                "Product", language, store);

            var siteMap = await _staticCacheManager.GetAsync(cacheKey, async () => await GenerateProductSitemapXml());

            return siteMap;
        }
        public virtual async Task<string> PreparePageSitemapXml()
        {
            var language = await _workContext.GetWorkingLanguageAsync();
            var customer = await _workContext.GetCurrentCustomerAsync();
            var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
            var store = await _storeContext.GetCurrentStoreAsync();
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.CustomSitemapSeoModelKey,
                "Page", language, store);

            var siteMap = await _staticCacheManager.GetAsync(cacheKey, async () => await GeneratePageSitemapXml());

            return siteMap;
        }
        public virtual async Task<string> PrepareCategorySitemapXml()
        {
            var language = await _workContext.GetWorkingLanguageAsync();
            var customer = await _workContext.GetCurrentCustomerAsync();
            var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
            var store = await _storeContext.GetCurrentStoreAsync();
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.CustomSitemapSeoModelKey,
                "Category", language, store);

            var siteMap = await _staticCacheManager.GetAsync(cacheKey, async () => await GenerateCategorySitemapXml());

            return siteMap;
        }
        public virtual async Task<string> PrepareQuestionAnswerSitemapXml()
        {
            var language = await _workContext.GetWorkingLanguageAsync();
            var customer = await _workContext.GetCurrentCustomerAsync();
            var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
            var store = await _storeContext.GetCurrentStoreAsync();
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.CustomSitemapSeoModelKey,
                "QuestionAnswer", language, store);

            var siteMap = await _staticCacheManager.GetAsync(cacheKey, async () => await GenerateQuestionAnswerSitemapXml());

            return siteMap;
        }
        public virtual async Task<string> PrepareKwTermSitemapXml()
        {
            var language = await _workContext.GetWorkingLanguageAsync();
            var customer = await _workContext.GetCurrentCustomerAsync();
            var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
            var store = await _storeContext.GetCurrentStoreAsync();
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.CustomSitemapSeoModelKey,
                "KwTerm", language, store);

            var siteMap = await _staticCacheManager.GetAsync(cacheKey, async () => await GenerateKwTermSitemapXml());

            return siteMap;
        }
        public virtual async Task<string> PreparePictureSitemapXmlAsync(int? id)
        {
            var language = await _workContext.GetWorkingLanguageAsync();
            var customer = await _workContext.GetCurrentCustomerAsync();
            var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
            var store = await _storeContext.GetCurrentStoreAsync();
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.SitemapImageModelKey,
                id, language, customerRoleIds, store);

            var siteMap = await _staticCacheManager.GetAsync(cacheKey, async () => await GeneratePictureSiteMapAsync(id));

            return siteMap;
        }
        #endregion

        #region Utilities

        protected async Task<string> GenerateSitemapIndexXml()
        {
            string SitemapNamespace = "http://www.sitemaps.org/schemas/sitemap/0.9";
            var baseUrl = _webHelper.GetStoreLocation();
            var sitemapFiles = new List<string>
            {
                $"{baseUrl}products_sitemap.xml",
                $"{baseUrl}picturesitemap.xml",
                $"{baseUrl}pages_sitemap.xml",
                $"{baseUrl}categories_sitemap.xml",
                $"{baseUrl}qa_sitemap.xml",
                $"{baseUrl}kw_sitemap.xml",
                $"{baseUrl}tags.xml",
                "https://www.sierralivingconcepts.com/blog/sitemap_index.xml"
            };

            var sitemapIndex = new XElement(XName.Get("sitemapindex", SitemapNamespace));

            foreach (var file in sitemapFiles)
            {
                sitemapIndex.Add(new XElement(XName.Get("sitemap", SitemapNamespace),
                    new XElement(XName.Get("loc", SitemapNamespace), file)
                 ));
            }

            var document = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                sitemapIndex
            );

            // Return XML as string
            return document.ToString();
        }

        #region Picture

        public async Task<string> GeneratePictureSiteMapAsync(int? id)
        {
            await using var stream = new MemoryStream();
            await GeneratePictureSiteMapAsync(stream, id);

            return Encoding.UTF8.GetString(stream.ToArray());
        }

        protected virtual async Task GeneratePictureSiteMapAsync(Stream stream, int? id)
        {
            //generate all URLs for the sitemap
            var sitemapUrls = await GeneratePicturSiteMapUrlsAsync();

            //split URLs into separate lists based on the max size 
            var sitemaps = sitemapUrls
                .Select((url, index) => new { Index = index, Value = url })
                .GroupBy(group => group.Index / NopSeoDefaults.SitemapMaxUrlNumber)
                .Select(group => group
                    .Select(url => url.Value)
                    .ToList()).ToList();

            if (!sitemaps.Any())
                return;
            //URLs more than the maximum allowable, so generate a sitemap index file

            //otherwise generate a standard sitemap
            await WritePictureSitemapAsync(stream, sitemaps.First());


        }

        protected virtual async Task<IList<SitemapUrlExtendedModel>> GeneratePicturSiteMapUrlsAsync()
        {
            var sitemapUrls = new List<SitemapUrlExtendedModel>();
            sitemapUrls.AddRange(await GetProductPictureurlsAsync());
            return sitemapUrls;
        }
        protected virtual async Task<IEnumerable<SitemapUrlExtendedModel>> GetProductPictureurlsAsync()
        {
            List<SitemapUrlExtendedModel> lsSiteMapUrl = new List<SitemapUrlExtendedModel>();
            var products = await _productService.SearchProductsAsync(0, storeId: (await _storeContext.GetCurrentStoreAsync()).Id,
                visibleIndividuallyOnly: true, orderBy: ProductSortingEnum.CreatedOn);


            foreach (var product in products)
            {
                var sitemap = await GetLocalizedSitemapUrlAsync("Product", CustomGetSeoRouteParamsAwait(product), product.UpdatedOnUtc, UpdateFrequency.Daily);

                #region picture

                var pictures = await _pictureService.CustomGetPicturesOfProducAsync(product.Id);
                var defaultPicture = new Picture();
                defaultPicture = pictures.FirstOrDefault();

                string imageUrl;
                (imageUrl, defaultPicture) = await _pictureService.GetPictureUrlAsync(defaultPicture);

                sitemap.Image = new ImageSitemap()
                {
                    Caption = product.Name,
                    Title = product.Name,
                    url = imageUrl
                };

                #endregion

                lsSiteMapUrl.Add(sitemap);

            }
            return lsSiteMapUrl;
        }
        protected virtual async Task WritePictureSitemapAsync(Stream stream, IList<SitemapUrlExtendedModel> sitemapUrls)
        {
            await using var writer = new XmlTextWriter(stream, Encoding.UTF8)
            {
                Formatting = Formatting.Indented
            };
            writer.WriteStartDocument();
            writer.WriteStartElement("urlset");
            writer.WriteAttributeString("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9");
            writer.WriteAttributeString("xmlns:image", "http://www.google.com/schemas/sitemap-image/1.1");


            //write URLs from list to the sitemap
            foreach (var sitemapUrl in sitemapUrls)
            {
                //write base url
                await WriteSitemapPicturUrlAsync(writer, sitemapUrl);


            }

            writer.WriteEndElement();
        }

        protected virtual async Task WriteSitemapPicturUrlAsync(XmlTextWriter writer, SitemapUrlExtendedModel sitemapUrl)
        {
            if (string.IsNullOrEmpty(sitemapUrl.Location))
                return;

            writer.WriteStartElement("url");

            var loc = await XmlHelper.XmlEncodeAsync(sitemapUrl.Location);
            writer.WriteElementString("loc", loc);
            writer.WriteElementString("changefreq", sitemapUrl.UpdateFrequency.ToString().ToLowerInvariant());
            writer.WriteElementString("lastmod", sitemapUrl.UpdatedOn.ToString(NopSeoDefaults.SitemapDateFormat, CultureInfo.InvariantCulture));
            //write all related url

            writer.WriteStartElement("image:image");

            writer.WriteStartElement("image:loc");
            writer.WriteString(sitemapUrl.Image.url);
            writer.WriteEndElement();

            writer.WriteStartElement("image:title");
            writer.WriteString(sitemapUrl.Image.Title);
            writer.WriteEndElement();

            writer.WriteStartElement("image:caption");
            writer.WriteString(sitemapUrl.Image.Caption);
            writer.WriteEndElement();
            writer.WriteEndElement();


            writer.WriteEndElement();
        }
        #endregion

        #region KwTerm

        public async Task<string> GenerateKwTermSitemapXml()
        {
            await using var stream = new MemoryStream();
            await CustomGenerateKwTermSitemapXml(stream);

            return Encoding.UTF8.GetString(stream.ToArray());
        }

        protected virtual async Task CustomGenerateKwTermSitemapXml(MemoryStream stream)
        {
            //generate all URLs for the sitemap
            var sitemapUrls = new List<SitemapUrlExtendedModel>();
            if (_sitemapXmlSettings.SitemapXmlIncludeCategories)
                sitemapUrls.AddRange(await GetKwTermUrlsAsync());

            //   await _eventPublisher.PublishAsync(new SitemapCreatedEvent(sitemapUrls));
            //split URLs into separate lists based on the max size 
            var sitemaps = sitemapUrls
                .Select((url, index) => new { Index = index, Value = url })
                .GroupBy(group => group.Index / NopSeoDefaults.SitemapMaxUrlNumber)
                .Select(group => group
                    .Select(url => url.Value)
                    .ToList()).ToList();

            if (!sitemaps.Any())
                return;
            if (sitemapUrls.Count >= NopSeoDefaults.SitemapMaxUrlNumber)
            {
                //write a sitemap index file into the stream
                await WriteSitemapIndexAsync(stream, sitemaps.Count);
            }
            else
            {
                //otherwise generate a standard sitemap
                await WriteSitemapExtendedAsync(stream, sitemaps.First());
            }

        }

        protected virtual async Task<IEnumerable<SitemapUrlExtendedModel>> GetKwTermUrlsAsync()
        {

            return await (await _kwTermService.GetAllKwTermsAsync(storeId: (await _storeContext.GetCurrentStoreAsync()).Id, pageIndex: 0, pageSize: int.MaxValue))
               .SelectAwait(async kwTerm => await GetLocalizedSitemapUrlAsync("KWTerm", CustomGetSeoRouteParamsAwait(kwTerm), kwTerm.UpdatedOnUtc)).ToListAsync();

        }

        #endregion

        #region QuestionAnser
        public async Task<string> GenerateQuestionAnswerSitemapXml()
        {
            await using var stream = new MemoryStream();
            await GenerateQuestionAnswerSitemapXml(stream);

            return Encoding.UTF8.GetString(stream.ToArray());
        }
        protected virtual async Task GenerateQuestionAnswerSitemapXml(MemoryStream stream)
        {
            //generate all URLs for the sitemap
            var sitemapUrls = new List<SitemapUrlExtendedModel>();

            if (_sitemapXmlSettings.SitemapXmlIncludeCategories)
                sitemapUrls.AddRange(await GeQuestionAnswerUrlsAsync());

            //  await _eventPublisher.PublishAsync(new SitemapCreatedEvent(sitemapUrls));
            //split URLs into separate lists based on the max size 
            var sitemaps = sitemapUrls
                .Select((url, index) => new { Index = index, Value = url })
                .GroupBy(group => group.Index / NopSeoDefaults.SitemapMaxUrlNumber)
                .Select(group => group
                    .Select(url => url.Value)
                    .ToList()).ToList();

            if (!sitemaps.Any())
                return;
            if (sitemapUrls.Count >= NopSeoDefaults.SitemapMaxUrlNumber)
            {
                //write a sitemap index file into the stream
                await WriteSitemapIndexAsync(stream, sitemaps.Count);
            }
            else
            {
                //otherwise generate a standard sitemap
                await WriteSitemapExtendedAsync(stream, sitemaps.First());
            }

        }
        protected virtual async Task<IEnumerable<SitemapUrlExtendedModel>> GeQuestionAnswerUrlsAsync()
        {


            return await (await _questionAnswerService.GetAllQuestionAnswersAsync(storeId: (await _storeContext.GetCurrentStoreAsync()).Id, pageIndex: 0, pageSize: int.MaxValue))
               .SelectAwait(async questionAnswer => await GetLocalizedSitemapUrlAsync("QuestionAnswer", CustomGetSeoRouteParamsAwait(questionAnswer), questionAnswer.UpdatedOnUtc)).ToListAsync();


        }

        #endregion

        #region Category

        public async Task<string> GenerateCategorySitemapXml()
        {
            await using var stream = new MemoryStream();
            await GenerateCategorySitemapXml(stream);

            return Encoding.UTF8.GetString(stream.ToArray());
        }

        protected virtual async Task GenerateCategorySitemapXml(MemoryStream stream)
        {
            //generate all URLs for the sitemap
            var sitemapUrls = new List<SitemapUrlExtendedModel>();
            if (_sitemapXmlSettings.SitemapXmlIncludeCategories)
                sitemapUrls.AddRange(await GetExtendedCategoryUrlsAsync());

            //  await _eventPublisher.PublishAsync(new SitemapCreatedEvent(sitemapUrls));
            //split URLs into separate lists based on the max size 
            var sitemaps = sitemapUrls
                .Select((url, index) => new { Index = index, Value = url })
                .GroupBy(group => group.Index / NopSeoDefaults.SitemapMaxUrlNumber)
                .Select(group => group
                    .Select(url => url.Value)
                    .ToList()).ToList();

            if (!sitemaps.Any())
                return;
            if (sitemapUrls.Count >= NopSeoDefaults.SitemapMaxUrlNumber)
            {
                //write a sitemap index file into the stream
                await WriteSitemapIndexAsync(stream, sitemaps.Count);
            }
            else
            {
                //otherwise generate a standard sitemap
                await WriteSitemapExtendedAsync(stream, sitemaps.First());
            }

        }
        protected virtual async Task<IEnumerable<SitemapUrlExtendedModel>> GetExtendedCategoryUrlsAsync()
        {
            return await (await _categoryService.GetAllCategoriesAsync(storeId: (await _storeContext.GetCurrentStoreAsync()).Id))
               .SelectAwait(async category => await GetLocalizedSitemapUrlAsync("Category", CustomGetSeoRouteParamsAwait(category), category.UpdatedOnUtc)).ToListAsync();


        }
        #endregion

        #region Page
        public async Task<string> GeneratePageSitemapXml()
        {
            await using var stream = new MemoryStream();
            await GeneratePageSitemapXml(stream);

            return Encoding.UTF8.GetString(stream.ToArray());
        }
        protected virtual async Task GeneratePageSitemapXml(MemoryStream stream)
        {
            var sitemapUrls = new List<SitemapUrlExtendedModel>();

            if (_sitemapXmlSettings.SitemapXmlIncludeTopics)
                sitemapUrls.AddRange(await GetExtendedTopicUrlsAsync());

            //    await _eventPublisher.PublishAsync(new SitemapCreatedEvent(sitemapUrls));
            //split URLs into separate lists based on the max size 
            var sitemaps = sitemapUrls
                .Select((url, index) => new { Index = index, Value = url })
                .GroupBy(group => group.Index / NopSeoDefaults.SitemapMaxUrlNumber)
                .Select(group => group
                    .Select(url => url.Value)
                    .ToList()).ToList();

            if (!sitemaps.Any())
                return;
            if (sitemapUrls.Count >= NopSeoDefaults.SitemapMaxUrlNumber)
            {
                //write a sitemap index file into the stream
                await WriteSitemapIndexAsync(stream, sitemaps.Count);
            }
            else
            {
                //otherwise generate a standard sitemap
                await WriteSitemapExtendedAsync(stream, sitemaps.First());
            }

        }
        protected virtual async Task<IEnumerable<SitemapUrlExtendedModel>> GetExtendedTopicUrlsAsync()
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var topics = await _topicService.GetAllTopicsAsync(store.Id);

            return await topics
                .Where(t => t.IncludeInSitemap)
                .SelectAwait(async topic => await PrepareExtendedLocalizedSitemapUrlAsync(topic))
                .ToListAsync();
        }

        #endregion

        #region Product
        public async Task<string> GenerateProductSitemapXml()
        {
            await using var stream = new MemoryStream();
            await GenerateProductSitemapXml(stream);

            return Encoding.UTF8.GetString(stream.ToArray());
        }

        protected virtual async Task GenerateProductSitemapXml(MemoryStream stream)
        {
            var sitemapUrls = new List<SitemapUrlExtendedModel>();

            if (_sitemapXmlSettings.SitemapXmlIncludeProducts)
                sitemapUrls.AddRange(await GetExtendedProductUrlsAsync());

            //await _eventPublisher.PublishAsync(new SitemapCreatedEvent(sitemapUrls));
            //split URLs into separate lists based on the max size 
            var sitemaps = sitemapUrls
                .Select((url, index) => new { Index = index, Value = url })
                .GroupBy(group => group.Index / NopSeoDefaults.SitemapMaxUrlNumber)
                .Select(group => group
                    .Select(url => url.Value)
                    .ToList()).ToList();

            if (!sitemaps.Any())
                return;
            if (sitemapUrls.Count >= NopSeoDefaults.SitemapMaxUrlNumber)
            {
                //write a sitemap index file into the stream
                await WriteSitemapIndexAsync(stream, sitemaps.Count);
            }
            else
            {
                //otherwise generate a standard sitemap
                await WriteSitemapExtendedAsync(stream, sitemaps.First());
            }

        }

        protected virtual async Task<IEnumerable<SitemapUrlExtendedModel>> GetExtendedProductUrlsAsync()
        {
            return await (await _productService.SearchProductsAsync(0, storeId: (await _storeContext.GetCurrentStoreAsync()).Id,
                visibleIndividuallyOnly: true, orderBy: ProductSortingEnum.CreatedOn))
                .SelectAwait(async product => await GetLocalizedSitemapUrlAsync("Product", CustomGetSeoRouteParamsAwait(product), product.UpdatedOnUtc)).ToListAsync();
        }

        #endregion



        #region Common
        protected virtual Func<int?, Task<object>> CustomGetSeoRouteParamsAwait<T>(T model)
         where T : BaseEntity, ISlugSupported
        {
            if (string.Equals(typeof(T).Name, "KWTerm", StringComparison.InvariantCultureIgnoreCase) || string.Equals(typeof(T).Name, "QuestionAnswer", StringComparison.InvariantCultureIgnoreCase))
                return async lang => new { SeName = await _urlRecordService.GetSeNameAsync(model, lang) };
            else
                return async lang => new { SeName = await _urlRecordService.GetSeNameAsync(model, lang), id = model.Id };
        }

        protected virtual IUrlHelper GetUrlHelper()
        {
            return _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
        }

        public virtual async Task<SitemapUrlExtendedModel> GetLocalizedSitemapUrlAsync(string routeName,
           Func<int?, Task<object>> getRouteParamsAwait = null,
           DateTime? dateTimeUpdatedOn = null,
           UpdateFrequency updateFreq = UpdateFrequency.Weekly)
        {
            var urlHelper = GetUrlHelper();

            //url for current language
            var url = urlHelper.RouteUrl(routeName,
                getRouteParamsAwait != null ? await getRouteParamsAwait(null) : null,
                await GetHttpProtocolAsync());

            var updatedOn = dateTimeUpdatedOn ?? DateTime.UtcNow;
            var languages = _localizationSettings.SeoFriendlyUrlsForLanguagesEnabled
                ? await _languageService.GetAllLanguagesAsync()
                : null;

            if (languages == null)
                return new SitemapUrlExtendedModel(url, new List<string>(), updateFreq, updatedOn);

            var pathBase = _actionContextAccessor.ActionContext.HttpContext.Request.PathBase;
            //return list of localized urls
            var localizedUrls = await languages
                .SelectAwait(async lang =>
                {
                    var currentUrl = urlHelper.RouteUrl(routeName,
                        getRouteParamsAwait != null ? await getRouteParamsAwait(lang.Id) : null,
                        await GetHttpProtocolAsync());

                    if (string.IsNullOrEmpty(currentUrl))
                        return null;

                    //Extract server and path from url
                    var scheme = new Uri(currentUrl).GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped);
                    var path = new Uri(currentUrl).PathAndQuery;

                    //Replace seo code
                    var localizedPath = path
                        .RemoveLanguageSeoCodeFromUrl(pathBase, true)
                        .AddLanguageSeoCodeToUrl(pathBase, true, lang);

                    return new Uri(new Uri(scheme), localizedPath).ToString();
                })
                .Where(value => !string.IsNullOrEmpty(value))
                .ToListAsync();

            return new SitemapUrlExtendedModel(url, localizedUrls, updateFreq, updatedOn);
        }

        protected virtual async Task<SitemapUrlExtendedModel> PrepareExtendedLocalizedSitemapUrlAsync<TEntity>(TEntity entity,
       DateTime? dateTimeUpdatedOn = null,
       UpdateFrequency updateFreq = UpdateFrequency.Weekly) where TEntity : BaseEntity, ISlugSupported
        {
            var isSingleLanguageEntity = entity is BlogPost or NewsItem;
            var url = await _nopUrlHelper
                .RouteGenericUrlAsync(entity, await GetHttpProtocolAsync(), ensureTwoPublishedLanguages: !isSingleLanguageEntity);
            if (entity.Id == 7576)
            {

            }
            var store = await _storeContext.GetCurrentStoreAsync();

            var updatedOn = dateTimeUpdatedOn ?? DateTime.UtcNow;
            var languages = _localizationSettings.SeoFriendlyUrlsForLanguagesEnabled
                ? await _languageService.GetAllLanguagesAsync(storeId: store.Id)
                : null;

            if (languages == null || languages.Count == 1)
                return new SitemapUrlExtendedModel(url, new List<string>(), updateFreq, updatedOn);

            if (isSingleLanguageEntity)
            {
                var languageId = entity is BlogPost blogPost ? blogPost.LanguageId : (entity is NewsItem newsItem ? newsItem.LanguageId : 0);
                if (await _languageService.GetLanguageByIdAsync(languageId) is not Language language || !language.Published)
                    return new SitemapUrlExtendedModel(url, new List<string>(), updateFreq, updatedOn);

                var localizedUrl = await _nopUrlHelper
                    .RouteGenericUrlAsync(entity, await GetHttpProtocolAsync(), languageId: languageId, ensureTwoPublishedLanguages: false);
                localizedUrl = GetLocalizedUrl(url, language);
                return new SitemapUrlExtendedModel(localizedUrl, new List<string>(), updateFreq, updatedOn);
            }

            //return list of localized urls
            var localizedUrls = await languages
                .SelectAwait(async lang =>
                {
                    var currentUrl = await _nopUrlHelper.RouteGenericUrlAsync(entity, await GetHttpProtocolAsync(), languageId: lang.Id);
                    return GetLocalizedUrl(currentUrl, lang);
                })
                .Where(value => !string.IsNullOrEmpty(value))
                .ToListAsync();

            return new SitemapUrlExtendedModel(url, localizedUrls, updateFreq, updatedOn);
        }

        protected virtual async Task WriteSitemapExtendedAsync(MemoryStream stream, IList<SitemapUrlExtendedModel> sitemapUrls)
        {
            await using var writer = XmlWriter.Create(stream, new XmlWriterSettings
            {
                Async = true,
                Encoding = Encoding.UTF8,
                Indent = true,
                ConformanceLevel = ConformanceLevel.Auto
            });

            await writer.WriteStartDocumentAsync();
            await writer.WriteStartElementAsync(prefix: null, localName: "urlset", ns: "http://www.sitemaps.org/schemas/sitemap/0.9");
            await writer.WriteAttributeStringAsync(prefix: "xsi", "schemaLocation",
                "http://www.w3.org/2001/XMLSchema-instance",
                "http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd http://www.w3.org/1999/xhtml http://www.w3.org/2002/08/xhtml/xhtml1-strict.xsd");
            await writer.WriteAttributeStringAsync(prefix: "xmlns", "xhtml", null, "http://www.w3.org/1999/xhtml");

            //write URLs from list to the sitemap
            foreach (var sitemapUrl in sitemapUrls)
            {
                //write base url
                await WriteSitemapUrlAsync(writer, sitemapUrl);

                //write all alternate url if exists
                foreach (var alternate in sitemapUrl.AlternateLocations
                             .Where(p => !p.Equals(sitemapUrl.Location, StringComparison.InvariantCultureIgnoreCase)))
                {
                    await WriteSitemapUrlAsync(writer, new SitemapUrlModel(alternate, sitemapUrl));
                }
            }

            await writer.WriteEndElementAsync();
        }

        #endregion
        #endregion
    }
}
