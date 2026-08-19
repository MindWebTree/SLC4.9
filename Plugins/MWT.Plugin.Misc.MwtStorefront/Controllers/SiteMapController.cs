using Microsoft.AspNetCore.Mvc;
using MWT.Plugin.Misc.MwtStorefront.Factories;

using MWT.Plugin.Misc.MwtStorefront.Models.Sitemap;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Vendors;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Vendors;
using Nop.Web.Controllers;
using Nop.Web.Factories;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Themes;

namespace MWT.Plugin.Misc.MwtStorefront.Controllers
{

    [AutoValidateAntiforgeryToken]
    public partial class SiteMapController : BasePublicController
    {

        #region Fields

        protected readonly CaptchaSettings _captchaSettings;
        protected readonly CommonSettings _commonSettings;
        protected readonly ICurrencyService _currencyService;
        protected readonly ICustomerActivityService _customerActivityService;
        protected readonly IGenericAttributeService _genericAttributeService;
        protected readonly IHtmlFormatter _htmlFormatter;
        protected readonly ILanguageService _languageService;
        protected readonly ILocalizationService _localizationService;
        protected readonly ISiteMapExtendedModelFactory _sitemapModelFactory;
        protected readonly IStoreContext _storeContext;
        protected readonly IThemeContext _themeContext;
        protected readonly IVendorService _vendorService;
        protected readonly IWorkContext _workContext;
        protected readonly IWorkflowMessageService _workflowMessageService;
        protected readonly LocalizationSettings _localizationSettings;
        protected readonly SitemapSettings _sitemapSettings;
        protected readonly SitemapXmlSettings _sitemapXmlSettings;
        protected readonly StoreInformationSettings _storeInformationSettings;
        protected readonly VendorSettings _vendorSettings;

        #endregion

        #region Ctor

        public SiteMapController(CaptchaSettings captchaSettings,
      CommonSettings commonSettings,
      ICurrencyService currencyService,
      ICustomerActivityService customerActivityService,
      IGenericAttributeService genericAttributeService,
      IHtmlFormatter htmlFormatter,
      ILanguageService languageService,
      ILocalizationService localizationService,
      ISiteMapExtendedModelFactory sitemapModelFactory,
      IStoreContext storeContext,
      IThemeContext themeContext,
      IVendorService vendorService,
      IWorkContext workContext,
      IWorkflowMessageService workflowMessageService,
      LocalizationSettings localizationSettings,
      SitemapSettings sitemapSettings,
      SitemapXmlSettings sitemapXmlSettings,
      StoreInformationSettings storeInformationSettings,
      VendorSettings vendorSettings)
        {
            _captchaSettings = captchaSettings;
            _commonSettings = commonSettings;
            _currencyService = currencyService;
            _customerActivityService = customerActivityService;
            _genericAttributeService = genericAttributeService;
            _htmlFormatter = htmlFormatter;
            _languageService = languageService;
            _localizationService = localizationService;
            _sitemapModelFactory = sitemapModelFactory;
            _storeContext = storeContext;
            _themeContext = themeContext;
            _vendorService = vendorService;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
            _sitemapSettings = sitemapSettings;
            _sitemapXmlSettings = sitemapXmlSettings;
            _storeInformationSettings = storeInformationSettings;
            _vendorSettings = vendorSettings;
        }

        #endregion

        #region Methods

        public virtual async Task<IActionResult> ExtendedSitemap(SitemapPageExtendedModel pageModel)
        {
            if (!_sitemapSettings.SitemapEnabled)
                return RedirectToRoute("Homepage");

            var model = await _sitemapModelFactory.PrepareSitemapExtendedModelAsync(pageModel);

            return View("Sitemap", model);
        }

        //SEO sitemap page
        //available even when a store is closed
        [CheckAccessClosedStore(ignore: true)]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(ignore: true)]
        //ignore SEO friendly URLs checks
        [CheckLanguageSeoCode(ignore: true)]
        public virtual async Task<IActionResult> ExtendedSitemapXml(int? id)
        {
            var siteMap = _sitemapXmlSettings.SitemapXmlEnabled
                 ? await _sitemapModelFactory.PrepareSitemapExtendedIndexXml() : string.Empty;

            return Content(siteMap, "text/xml");
        }
        //SEO sitemap page
        //available even when a store is closed
        [CheckAccessClosedStore(ignore: true)]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(ignore: true)]
        //ignore SEO friendly URLs checks

        public virtual async Task<IActionResult> GenerateProductSitemapXml()
        {
            var siteMap = _sitemapXmlSettings.SitemapXmlEnabled
          ? await _sitemapModelFactory.PrepareProductSitemapXml() : string.Empty;

            return Content(siteMap, "text/xml");
        }
        //SEO sitemap page
        //available even when a store is closed
        [CheckAccessClosedStore(ignore: true)]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(ignore: true)]
        //ignore SEO friendly URLs checks
        public virtual async Task<IActionResult> GeneratePageSitemapXml()
        {
            var siteMap = _sitemapXmlSettings.SitemapXmlEnabled
          ? await _sitemapModelFactory.PreparePageSitemapXml() : string.Empty;

            return Content(siteMap, "text/xml");
        }
        //SEO sitemap page
        //available even when a store is closed
        [CheckAccessClosedStore(ignore: true)]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(ignore: true)]
        //ignore SEO friendly URLs checks
        public virtual async Task<IActionResult> GenerateCategorySitemapXml()
        {
            var siteMap = _sitemapXmlSettings.SitemapXmlEnabled
          ? await _sitemapModelFactory.PrepareCategorySitemapXml() : string.Empty;

            return Content(siteMap, "text/xml");
        }
        //SEO sitemap page
        //available even when a store is closed
        [CheckAccessClosedStore(ignore: true)]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(ignore: true)]
        //ignore SEO friendly URLs checks
        public virtual async Task<IActionResult> GenerateQuestionAnswerSitemapXml()
        {
            var siteMap = _sitemapXmlSettings.SitemapXmlEnabled
          ? await _sitemapModelFactory.PrepareQuestionAnswerSitemapXml() : string.Empty;

            return Content(siteMap, "text/xml");
        }
        //SEO sitemap page
        //available even when a store is closed
        [CheckAccessClosedStore(ignore: true)]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(ignore: true)]
        //ignore SEO friendly URLs checks
        public virtual async Task<IActionResult> GenerateKwTermSitemapXml()
        {
            var siteMap = _sitemapXmlSettings.SitemapXmlEnabled
          ? await _sitemapModelFactory.PrepareKwTermSitemapXml() : string.Empty;

            return Content(siteMap, "text/xml");
        }

        //SEO sitemap page
        //available even when a store is closed
        [CheckAccessClosedStore(ignore: true)]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(ignore: true)]
        //ignore SEO friendly URLs checks
        public virtual async Task<IActionResult> GeneratePictureSitemapXml(int? id)
        {
            var siteMap = _sitemapXmlSettings.SitemapXmlEnabled
                ? await _sitemapModelFactory.PreparePictureSitemapXmlAsync(id) : string.Empty;

            return Content(siteMap, "text/xml");
        }

        #endregion
    }
}
