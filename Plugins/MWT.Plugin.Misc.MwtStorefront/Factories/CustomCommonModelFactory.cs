using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using MWT.Nop.Core.Service.Catalog;
using MWT.Plugin.Misc.MwtStorefront.Models.Common;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Security;
using Nop.Core.Infrastructure;
using Nop.Services.Blogs;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Forums;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.News;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Themes;
using Nop.Services.Topics;
using Nop.Web.Factories;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Framework.Themes;
using Nop.Web.Framework.UI;
using Nop.Web.Infrastructure.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Factories
{
    public class CustomCommonModelFactory : CommonModelFactory, ICustomCommonModelFactory
    {
        protected readonly INopUrlHelper _nopUrlHelper;
        private readonly IActionContextAccessor _actionContextAccessor;
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
        public CustomCommonModelFactory(CaptchaSettings captchaSettings, CatalogSettings catalogSettings, CommonSettings commonSettings, CurrencySettings currencySettings, CustomerSettings customerSettings, ForumSettings forumSettings, ICurrencyService currencyService, ICustomerService customerService, IForumService forumService, IGenericAttributeService genericAttributeService, IHttpContextAccessor httpContextAccessor, ILanguageService languageService, ILocalizationService localizationService, INopFileProvider fileProvider, INopHtmlHelper nopHtmlHelper, IPermissionService permissionService, IPictureService pictureService, IShoppingCartService shoppingCartService, IStaticCacheManager staticCacheManager, IStoreContext storeContext, IThemeContext themeContext, IThemeProvider themeProvider, IWebHelper webHelper, IWorkContext workContext, LocalizationSettings localizationSettings, MediaSettings mediaSettings, MessagesSettings messagesSettings, NewsSettings newsSettings, RobotsTxtSettings robotsTxtSettings, SitemapXmlSettings sitemapXmlSettings, StoreInformationSettings storeInformationSettings, INopUrlHelper nopUrlHelper ,
            IActionContextAccessor actionContextAccessor ,
            SitemapSettings sitemapSettings,
            BlogSettings blogSettings , ITopicService topicService , IBlogService blogService , IUrlRecordService urlRecordService , INewsService newsService
            ,ICategoryService categoryService,
            IManufacturerService manufacturerService, ICustomProductService customProductService,
            IProductTagService productTagService) : base(captchaSettings, catalogSettings, commonSettings, currencySettings, customerSettings, forumSettings, currencyService, customerService, forumService, genericAttributeService, httpContextAccessor, languageService, localizationService, fileProvider, nopHtmlHelper, permissionService, pictureService, shoppingCartService, staticCacheManager, storeContext, themeContext, themeProvider, webHelper, workContext, localizationSettings, mediaSettings, messagesSettings, newsSettings, robotsTxtSettings, sitemapXmlSettings, storeInformationSettings)
        {
            _nopUrlHelper = nopUrlHelper;
            _actionContextAccessor = actionContextAccessor;
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
        public virtual async Task<(string, int)> GetRefererDetails()
        {
            int entityId = 0;
            string entityType = "";
            try
            {
                string referer = _httpContextAccessor.HttpContext.Request.Headers["Referer"].ToString();
                if (!string.IsNullOrEmpty(referer))
                {
                    (entityType, entityId) = GetUrlFraments(referer);
                }
            }
            catch
            {

            }
            return (entityType, entityId);
        }

        #endregion

        #region Utilities

        public (string, int) GetUrlFraments(string url)
        {
            int entityId = 0;
            string entityType = "Home";
            url = url.ToLower().Replace("http://", "").Replace("https://", "");
            string[] fragments = url.Split('/');
            if (fragments.Length > 1)
            {
                entityType = fragments[1].Trim();
                if (fragments.Length > 2)
                {
                    int.TryParse(fragments[2], out entityId);
                }
            }
            return (entityType, entityId);
        }

        #endregion
    }
}
