using Microsoft.AspNetCore.Http;
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
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Models.Sitemap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Factories
{
    public partial class SitemapModelFactory : ISitemapModelFactory
    {


        #region Fields

        protected readonly BlogSettings _blogSettings;
        protected readonly ForumSettings _forumSettings;
        protected readonly IBlogService _blogService;
        protected readonly ICategoryService _categoryService;
        protected readonly ICustomerService _customerService;
        protected readonly IEventPublisher _eventPublisher;
        protected readonly IHttpContextAccessor _httpContextAccessor;
        protected readonly ILanguageService _languageService;
        protected readonly ILocalizationService _localizationService;
        protected readonly ILocker _locker;
        protected readonly IManufacturerService _manufacturerService;
        protected readonly INewsService _newsService;
        protected readonly INopFileProvider _nopFileProvider;
        protected readonly INopUrlHelper _nopUrlHelper;
        protected readonly IProductService _productService;
        protected readonly IProductTagService _productTagService;
        protected readonly IStaticCacheManager _staticCacheManager;
        protected readonly IStoreContext _storeContext;
        protected readonly ITopicService _topicService;
        protected readonly IWebHelper _webHelper;
        protected readonly IWorkContext _workContext;
        protected readonly LocalizationSettings _localizationSettings;
        protected readonly NewsSettings _newsSettings;
        protected readonly SitemapSettings _sitemapSettings;
        protected readonly SitemapXmlSettings _sitemapXmlSettings;

        #endregion

        #region Ctor

        public SitemapModelFactory(BlogSettings blogSettings,
            ForumSettings forumSettings,
            IBlogService blogService,
            ICategoryService categoryService,
            ICustomerService customerService,
            IEventPublisher eventPublisher,
            IHttpContextAccessor httpContextAccessor,
            ILanguageService languageService,
            ILocalizationService localizationService,
            ILocker locker,
            IManufacturerService manufacturerService,
            INewsService newsService,
            INopFileProvider nopFileProvider,
            INopUrlHelper nopUrlHelper,
            IProductService productService,
            IProductTagService productTagService,
            IStaticCacheManager staticCacheManager,
            IStoreContext storeContext,
            ITopicService topicService,
            IWebHelper webHelper,
            IWorkContext workContext,
            LocalizationSettings localizationSettings,
            NewsSettings newsSettings,
            SitemapSettings sitemapSettings,
            SitemapXmlSettings sitemapXmlSettings)
        {
            _blogSettings = blogSettings;
            _forumSettings = forumSettings;
            _blogService = blogService;
            _categoryService = categoryService;
            _customerService = customerService;
            _eventPublisher = eventPublisher;
            _httpContextAccessor = httpContextAccessor;
            _languageService = languageService;
            _localizationService = localizationService;
            _locker = locker;
            _manufacturerService = manufacturerService;
            _newsService = newsService;
            _nopFileProvider = nopFileProvider;
            _nopUrlHelper = nopUrlHelper;
            _productService = productService;
            _productTagService = productTagService;
            _staticCacheManager = staticCacheManager;
            _storeContext = storeContext;
            _topicService = topicService;
            _webHelper = webHelper;
            _workContext = workContext;
            _localizationSettings = localizationSettings;
            _newsSettings = newsSettings;
            _sitemapSettings = sitemapSettings;
            _sitemapXmlSettings = sitemapXmlSettings;
        }

        #endregion

        #region Methoods

        public virtual async Task<SitemapXmlModel> PrepareSitemapXmlModelAsync(int id = 0)
        {
            //var language = await _workContext.GetWorkingLanguageAsync();
            //var store = await _storeContext.GetCurrentStoreAsync();

            //var fileName = string.Format(NopSeoDefaults.SitemapXmlFilePattern, store.Id, language.Id, id);
            //var fullPath = _nopFileProvider.GetAbsolutePath(NopSeoDefaults.SitemapXmlDirectory, fileName);

            //if (_nopFileProvider.FileExists(fullPath) && _nopFileProvider.GetLastWriteTimeUtc(fullPath) > DateTime.UtcNow.AddHours(-_sitemapXmlSettings.RebuildSitemapXmlAfterHours))
            //{
            //    return new SitemapXmlModel { SitemapXmlPath = fullPath };
            //}

            ////execute task with lock
            //if (!await _locker.PerformActionWithLockAsync(
            //        fullPath,
            //        TimeSpan.FromSeconds(_sitemapXmlSettings.SitemapBuildOperationDelay),
            //        async () => await GenerateAsync(fullPath, id)))
            //{
            //    throw new InvalidOperationException();
            //}

            return new SitemapXmlModel { SitemapXmlPath = null };
        }

        #endregion
    }
}
