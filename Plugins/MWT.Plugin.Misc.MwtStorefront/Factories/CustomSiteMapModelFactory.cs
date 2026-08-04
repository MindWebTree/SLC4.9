using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Services.Customers;
using MWT.Plugin.Misc.MwtStorefront.Models.Common;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.News;
using Nop.Core.Events;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.util;

namespace MWT.Plugin.Misc.MwtStorefront.Factories
{
    public class CustomSiteMapModelFactory : SitemapModelFactory, ICustomSiteMapModelFactory
    {
        protected readonly INopUrlHelper _nopUrlHelper; 
        private readonly SitemapSettings _sitemapSettings;
        private readonly BlogSettings _blogSettings;
        private readonly ITopicService _topicService;
        private readonly IBlogService _blogService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly INewsService _newsService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ICustomProductService _customProductService;
        private readonly IProductTagService _productTagService;
        public CustomSiteMapModelFactory(BlogSettings blogSettings, ForumSettings forumSettings, IBlogService blogService, ICategoryService categoryService, ICustomerService customerService, IEventPublisher eventPublisher, IHttpContextAccessor httpContextAccessor, ILanguageService languageService, ILocalizationService localizationService, ILocker locker, IManufacturerService manufacturerService, INewsService newsService, INopFileProvider nopFileProvider, INopUrlHelper nopUrlHelper, IProductService productService, IProductTagService productTagService, IStaticCacheManager staticCacheManager, IStoreContext storeContext, ITopicService topicService, IWebHelper webHelper, IWorkContext workContext, LocalizationSettings localizationSettings, NewsSettings newsSettings, SitemapSettings sitemapSettings,
            SitemapXmlSettings sitemapXmlSettings,
   
           IUrlRecordService urlRecordService ,
 ICustomProductService customProductService
 
            ) : base(blogSettings, forumSettings, blogService, categoryService, customerService, eventPublisher, httpContextAccessor, languageService, localizationService, locker, manufacturerService, newsService, nopFileProvider, nopUrlHelper, productService, productTagService, staticCacheManager, storeContext, topicService, webHelper, workContext, localizationSettings, newsSettings, sitemapSettings, sitemapXmlSettings)
        {
            _nopUrlHelper = nopUrlHelper; 
            _sitemapSettings = sitemapSettings;
            _blogSettings = blogSettings;
            _topicService = topicService;
            _blogService = blogService;
            _urlRecordService = urlRecordService;
            _newsService = newsService;
            _categoryService = categoryService;
            _manufacturerService = manufacturerService;
            _customProductService = customProductService;
            _productTagService = productTagService;
        }

        #region Methods
        public async Task<string> CustomGeneratePictureSiteMapAsync(int? id)
        {
            await using var stream = new MemoryStream();
            //await CustomGeneratePictureSiteMapAsync(stream, id);

            //return Encoding.UTF8.GetString(stream.ToArray());
            return string.Empty;
        }
        public async Task<string> CustomGenerateAsync(int? id)
        {
            await using var stream = new MemoryStream();
            //await GenerateAsync(stream, id);

            //return Encoding.UTF8.GetString(stream.ToArray());
            return string.Empty;
        }
        public async Task<string> CustomKwTermGenerateAsync(int? id)
        {
            await using var stream = new MemoryStream();
            //await CustomKwTermGenerateAsync(stream, id);

            //return Encoding.UTF8.GetString(stream.ToArray());
            return string.Empty;    
        }
        public async Task<string> CustomQuestionAnswerGenerateAsync(int? id)
        {
            await using var stream = new MemoryStream();
            //await CustomQuestionAnswerGenerateAsync(stream, id);

            //return Encoding.UTF8.GetString(stream.ToArray());
            return string.Empty;
        }
        public virtual async Task<CustomSitemapModel> PrepareCustomSitemapModelAsync(CustomSitemapPageModel pageModel)
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
                //get URL helper


                var model = new CustomSitemapModel();

                //prepare common items
                var commonGroupTitle = await _localizationService.GetResourceAsync("Sitemap.General");

                //home page
                model.Items.Add(new CustomSitemapModel.SitemapItemModel
                {
                    GroupTitle = commonGroupTitle,
                    Name = await _localizationService.GetResourceAsync("Homepage"),
                    Url = _nopUrlHelper.RouteUrl("Homepage")
                });

                //search
                model.Items.Add(new CustomSitemapModel.SitemapItemModel
                {
                    GroupTitle = commonGroupTitle,
                    Name = await _localizationService.GetResourceAsync("sitemap.Search"),
                    Url = _nopUrlHelper.RouteUrl("ProductSearch")
                });

                //news
                if (_newsSettings.Enabled)
                {
                    model.Items.Add(new CustomSitemapModel.SitemapItemModel
                    {
                        GroupTitle = commonGroupTitle,
                        Name = await _localizationService.GetResourceAsync("News"),
                        Url = _nopUrlHelper.RouteUrl("NewsArchive")
                    });
                }

                //blog
                if (_blogSettings.Enabled)
                {
                    model.Items.Add(new CustomSitemapModel.SitemapItemModel
                    {
                        GroupTitle = commonGroupTitle,
                        Name = await _localizationService.GetResourceAsync("Blog"),
                        Url = _nopUrlHelper.RouteUrl("Blog") + "/"
                    });
                }

                //forums
                if (_forumSettings.ForumsEnabled)
                {
                    model.Items.Add(new CustomSitemapModel.SitemapItemModel
                    {
                        GroupTitle = commonGroupTitle,
                        Name = await _localizationService.GetResourceAsync("Forum.Forums"),
                        Url = _nopUrlHelper.RouteUrl("Boards")
                    });
                }

                //contact us
                model.Items.Add(new CustomSitemapModel.SitemapItemModel
                {
                    GroupTitle = commonGroupTitle,
                    Name = await _localizationService.GetResourceAsync("ContactUs"),
                    Url = _nopUrlHelper.RouteUrl("ContactUs")
                });

                //customer info
                model.Items.Add(new CustomSitemapModel.SitemapItemModel
                {
                    GroupTitle = commonGroupTitle,
                    Name = await _localizationService.GetResourceAsync("Account.MyAccount"),
                    Url = _nopUrlHelper.RouteUrl("CustomerInfo")
                });

                //at the moment topics are in general category too
                if (_sitemapSettings.SitemapIncludeTopics)
                {
                    var topics = (await _topicService.GetAllTopicsAsync(storeId: store.Id))
                        .Where(topic => topic.IncludeInSitemap);

                    model.Items.AddRange(await topics.SelectAwait(async topic => new CustomSitemapModel.SitemapItemModel
                    {
                        GroupTitle = commonGroupTitle,
                        Name = await _localizationService.GetLocalizedAsync(topic, x => x.Title),
                        Url = _nopUrlHelper.RouteUrl("Topic", new
                        {
                            SeName = await _urlRecordService.GetSeNameAsync(topic)
                        })

                    }).ToListAsync());
                }

                //blog posts
                if (_sitemapSettings.SitemapIncludeBlogPosts && _blogSettings.Enabled)
                {
                    var blogPostsGroupTitle = await _localizationService.GetResourceAsync("Sitemap.BlogPosts");
                    var blogPosts = (await _blogService.GetAllBlogPostsAsync(storeId: store.Id))
                        .Where(p => p.IncludeInSitemap);

                    model.Items.AddRange(await blogPosts.SelectAwait(async post => new CustomSitemapModel.SitemapItemModel
                    {
                        GroupTitle = blogPostsGroupTitle,
                        Name = post.Title,
                        Url = _nopUrlHelper.RouteUrl("BlogPost", new
                        {
                            SeName = await _urlRecordService.GetSeNameAsync(
        post,
        post.LanguageId,
        ensureTwoPublishedLanguages: false)
                        })
                    }).ToListAsync());
                }

                //news
                if (_sitemapSettings.SitemapIncludeNews && _newsSettings.Enabled)
                {
                    var newsGroupTitle = await _localizationService.GetResourceAsync("Sitemap.News");
                    var news = await _newsService.GetAllNewsAsync(storeId: store.Id);
                    model.Items.AddRange(await news.SelectAwait(async newsItem => new CustomSitemapModel.SitemapItemModel
                    {
                        GroupTitle = newsGroupTitle,
                        Name = newsItem.Title,
                        Url = _nopUrlHelper.RouteUrl("NewsItem", new
                        {
                            SeName = await _urlRecordService.GetSeNameAsync(
        newsItem,
        newsItem.LanguageId,
        ensureTwoPublishedLanguages: false)
                        })
                    }).ToListAsync());
                }

                //categories
                if (_sitemapSettings.SitemapIncludeCategories)
                {
                    var categoriesGroupTitle = await _localizationService.GetResourceAsync("Sitemap.Categories");
                    var categories = await _categoryService.GetAllCategoriesAsync(storeId: store.Id);
                    model.Items.AddRange(await categories.SelectAwait(async category => new CustomSitemapModel.SitemapItemModel
                    {
                        Id = category.Id,
                        GroupTitle = categoriesGroupTitle,
                        ParentId = category.ParentCategoryId,
                        Name = await _localizationService.GetLocalizedAsync(category, x => x.Name),
                        Url = _nopUrlHelper.RouteUrl("Category", new
                        {
                            id = category.Id,
                            SeName = await _urlRecordService.GetSeNameAsync(category)
                        })
                    }).ToListAsync());
                }

                //manufacturers
                if (_sitemapSettings.SitemapIncludeManufacturers)
                {
                    var manufacturersGroupTitle = await _localizationService.GetResourceAsync("Sitemap.Manufacturers");
                    var manufacturers = await _manufacturerService.GetAllManufacturersAsync(storeId: store.Id);
                    model.Items.AddRange(await manufacturers.SelectAwait(async manufacturer => new CustomSitemapModel.SitemapItemModel
                    {
                        GroupTitle = manufacturersGroupTitle,
                        Name = await _localizationService.GetLocalizedAsync(manufacturer, x => x.Name),
                        Url = _nopUrlHelper.RouteUrl("Manufacturer", new { id = manufacturer.Id, SeName = await _urlRecordService.GetSeNameAsync(manufacturer) })
                    }).ToListAsync());
                }

                //products
                if (_sitemapSettings.SitemapIncludeProducts)
                {
                    var productsGroupTitle = await _localizationService.GetResourceAsync("Sitemap.Products");
                    var products = await _customProductService.SearchProductsAsync(0, storeId: store.Id, visibleIndividuallyOnly: true);
                    model.Items.AddRange(await products.SelectAwait(async product => new CustomSitemapModel.SitemapItemModel
                    {
                        GroupTitle = productsGroupTitle,
                        Name = await _localizationService.GetLocalizedAsync(product, x => x.Name),
                        Url = _nopUrlHelper.RouteUrl("Product", new { id = product.Id, SeName = await _urlRecordService.GetSeNameAsync(product) })
                    }).ToListAsync());
                }

                //product tags
                if (_sitemapSettings.SitemapIncludeProductTags)
                {
                    var productTagsGroupTitle = await _localizationService.GetResourceAsync("Sitemap.ProductTags");
                    var productTags = await _productTagService.GetAllProductTagsAsync();
                    model.Items.AddRange(await productTags.SelectAwait(async productTag => new CustomSitemapModel.SitemapItemModel
                    {
                        GroupTitle = productTagsGroupTitle,
                        Name = await _localizationService.GetLocalizedAsync(productTag, x => x.Name),
                        Url = _nopUrlHelper.RouteUrl("ProductsByTag", new { SeName = await _urlRecordService.GetSeNameAsync(productTag) })
                    }).ToListAsync());
                }

                return model;
            });

            //prepare model with pagination
            pageModel.PageSize = Math.Max(pageModel.PageSize, _sitemapSettings.SitemapPageSize);
            pageModel.PageNumber = Math.Max(pageModel.PageNumber, 1);

            var pagedItems = new PagedList<CustomSitemapModel.SitemapItemModel>(cachedModel.Items, pageModel.PageNumber - 1, pageModel.PageSize);
            var sitemapModel = new CustomSitemapModel { Items = pagedItems };
            sitemapModel.PageModel.LoadPagedList(pagedItems);

            return sitemapModel;
        }
        public virtual async Task<string> CustomPrepareSitemapXmlAsync(int? id)
        {
            //var language = await _workContext.GetWorkingLanguageAsync();
            //var customer = await _workContext.GetCurrentCustomerAsync();
            //var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
            //var store = await _storeContext.GetCurrentStoreAsync();
            //var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.SitemapSeoModelKey,
            //    id, language, customerRoleIds, store);

            //var siteMap = await _staticCacheManager.GetAsync(cacheKey, async () => await _sitemapGenerator.CustomGenerateAsync(id));

            //    return siteMap;
            return string.Empty;
        }

        public virtual async Task<string> CustomPreparePictureSitemapXmlAsync(int? id)
        {
            //var language = await _workContext.GetWorkingLanguageAsync();
            //var customer = await _workContext.GetCurrentCustomerAsync();
            //var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
            //var store = await _storeContext.GetCurrentStoreAsync();
            //var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.SitemapImageModelKey,
            //    id, language, customerRoleIds, store);

            //var siteMap = await _staticCacheManager.GetAsync(cacheKey, async () => await _sitemapGenerator.CustomGeneratePictureSiteMapAsync(id));

            //return siteMap;
            return string.Empty;
        } 

        #region KwTerm
        public virtual async Task<string> CustomPrepareKwTermSitemapXmlAsync(int? id)
        {
            var language = await _workContext.GetWorkingLanguageAsync();
            var customer = await _workContext.GetCurrentCustomerAsync();
            var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
            var store = await _storeContext.GetCurrentStoreAsync();
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.KwTermSitemapSeoModelKey,
                id, language, customerRoleIds, store);
            //   var siteMap = await _staticCacheManager.GetAsync(cacheKey, async () => await _sitemapGenerator.CustomKwTermGenerateAsync(id));
            //   return siteMap;
            return string.Empty;
        }
        #endregion

        #region QuestionAnswer
        public virtual async Task<string> CustomPrepareQuestionAnswerSitemapXmlAsync(int? id)
        {
            var language = await _workContext.GetWorkingLanguageAsync();
            var customer = await _workContext.GetCurrentCustomerAsync();
            var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
            var store = await _storeContext.GetCurrentStoreAsync();
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.QuestionAnswerSitemapSeoModelKey,
                id, language, customerRoleIds, store);
            //var siteMap = await _staticCacheManager.GetAsync(cacheKey, async () => await _sitemapGenerator.CustomQuestionAnswerGenerateAsync(id));
            //return siteMap;
            return string.Empty;
        }
        #endregion


        #region  SiteMap Version2
        public virtual async Task<string> CustomGenerateSitemapIndexXml()
        {
            var language = await _workContext.GetWorkingLanguageAsync();
            var customer = await _workContext.GetCurrentCustomerAsync();
            var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
            var store = await _storeContext.GetCurrentStoreAsync();
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.CustomSitemapSeoModelKey,
                "All", language, store);

            //var siteMap = await _staticCacheManager.GetAsync(cacheKey, async () => await _sitemapGenerator.CustomGenerateSitemapIndexXml());

            //return siteMap;
            return string.Empty;
        }

        public virtual async Task<string> CustomGenerateProductSitemapXml()
        {
            var language = await _workContext.GetWorkingLanguageAsync();
            var customer = await _workContext.GetCurrentCustomerAsync();
            var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
            var store = await _storeContext.GetCurrentStoreAsync();
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.CustomSitemapSeoModelKey,
                "Product", language, store);

            //var siteMap = await _staticCacheManager.GetAsync(cacheKey, async () => await _sitemapGenerator.CustomGenerateProductSitemapXml());

            //return siteMap;
            return string.Empty;
        }
        public virtual async Task<string> CustomGeneratePageSitemapXml()
        {
            var language = await _workContext.GetWorkingLanguageAsync();
            var customer = await _workContext.GetCurrentCustomerAsync();
            var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
            var store = await _storeContext.GetCurrentStoreAsync();
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.CustomSitemapSeoModelKey,
                "Page", language, store);

            //var siteMap = await _staticCacheManager.GetAsync(cacheKey, async () => await _sitemapGenerator.CustomGeneratePageSitemapXml());

            //return siteMap;
            return string.Empty;
        }


        public virtual async Task<string> CustomGenerateCategorySitemapXml()
        {
            var language = await _workContext.GetWorkingLanguageAsync();
            var customer = await _workContext.GetCurrentCustomerAsync();
            var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
            var store = await _storeContext.GetCurrentStoreAsync();
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.CustomSitemapSeoModelKey,
                "Category", language, store);

            //var siteMap = await _staticCacheManager.GetAsync(cacheKey, async () => await _sitemapGenerator.CustomGenerateCategorySitemapXml());

            //return siteMap;
            return string.Empty;
        }
        public virtual async Task<string> CustomGenerateQuestionAnswerSitemapXml()
        {
            var language = await _workContext.GetWorkingLanguageAsync();
            var customer = await _workContext.GetCurrentCustomerAsync();
            var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
            var store = await _storeContext.GetCurrentStoreAsync();
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.CustomSitemapSeoModelKey,
                "QuestionAnswer", language, store);

            //var siteMap = await _staticCacheManager.GetAsync(cacheKey, async () => await _sitemapGenerator.CustomGenerateQuestionAnswerSitemapXml());

            //return siteMap;
            return string.Empty;
        }
        public virtual async Task<string> CustomGenerateKwTermSitemapXml()
        {
            var language = await _workContext.GetWorkingLanguageAsync();
            var customer = await _workContext.GetCurrentCustomerAsync();
            var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
            var store = await _storeContext.GetCurrentStoreAsync();
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.CustomSitemapSeoModelKey,
                "KwTerm", language, store);

            //var siteMap = await _staticCacheManager.GetAsync(cacheKey, async () => await _sitemapGenerator.CustomGenerateKwTermSitemapXml());

            //return siteMap;
            return string.Empty;
        }

        #endregion
     
 

 


        #endregion
  

    }
}
