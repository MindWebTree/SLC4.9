using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Controller
{

    [AutoValidateAntiforgeryToken]
    public partial class SiteMapController : BasePublicController
    {

        #region Fields

        protected readonly CaptchaSettings _captchaSettings;
        protected readonly CommonSettings _commonSettings;
        protected readonly ICommonModelFactory _commonModelFactory;
        protected readonly ICurrencyService _currencyService;
        protected readonly ICustomerActivityService _customerActivityService;
        protected readonly IGenericAttributeService _genericAttributeService;
        protected readonly IHtmlFormatter _htmlFormatter;
        protected readonly ILanguageService _languageService;
        protected readonly ILocalizationService _localizationService;
        protected readonly ISitemapModelFactory _sitemapModelFactory;
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
      ICommonModelFactory commonModelFactory,
      ICurrencyService currencyService,
      ICustomerActivityService customerActivityService,
      IGenericAttributeService genericAttributeService,
      IHtmlFormatter htmlFormatter,
      ILanguageService languageService,
      ILocalizationService localizationService,
      ISitemapModelFactory sitemapModelFactory,
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
            _commonModelFactory = commonModelFactory;
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

        //#region Methods

        ////SEO sitemap page
        ////available even when a store is closed
        //[CheckAccessClosedStore(ignore: true)]
        ////available even when navigation is not allowed
        //[CheckAccessPublicStore(ignore: true)]
        ////ignore SEO friendly URLs checks
        //[CheckLanguageSeoCode(ignore: true)]
        //public virtual async Task<IActionResult> SitemapXml(int? id)
        //{
        //    if (!_sitemapXmlSettings.SitemapXmlEnabled)
        //        return StatusCode(StatusCodes.Status403Forbidden);

        //}

        //public virtual async Task<IActionResult> CustomGenerateProductSitemapXml()
        //{
        //    var siteMap = _sitemapXmlSettings.SitemapXmlEnabled
        //  ? await _commonModelFactory.CustomGenerateProductSitemapXml() : string.Empty;

        //    return Content(siteMap, "text/xml");
        //}
        //public virtual async Task<IActionResult> CustomGeneratePageSitemapXml()
        //{
        //    var siteMap = _sitemapXmlSettings.SitemapXmlEnabled
        //  ? await _commonModelFactory.CustomGeneratePageSitemapXml() : string.Empty;

        //    return Content(siteMap, "text/xml");
        //}
        //public virtual async Task<IActionResult> CustomGenerateCategorySitemapXml()
        //{
        //    var siteMap = _sitemapXmlSettings.SitemapXmlEnabled
        //  ? await _commonModelFactory.CustomGenerateCategorySitemapXml() : string.Empty;

        //    return Content(siteMap, "text/xml");
        //}
        //public virtual async Task<IActionResult> CustomGenerateQuestionAnswerSitemapXml()
        //{
        //    var siteMap = _sitemapXmlSettings.SitemapXmlEnabled
        //  ? await _commonModelFactory.CustomGenerateQuestionAnswerSitemapXml() : string.Empty;

        //    return Content(siteMap, "text/xml");
        //}
        //public virtual async Task<IActionResult> CustomGenerateKwTermSitemapXml()
        //{
        //    var siteMap = _sitemapXmlSettings.SitemapXmlEnabled
        //  ? await _commonModelFactory.CustomGenerateKwTermSitemapXml() : string.Empty;

        //    return Content(siteMap, "text/xml");
        //}


        //public virtual async Task<IActionResult> CustomPictureSitemapXml(int? id)
        //{
        //    var siteMap = _sitemapXmlSettings.SitemapXmlEnabled
        //        ? await _commonModelFactory.CustomPreparePictureSitemapXmlAsync(id) : string.Empty;

        //    return Content(siteMap, "text/xml");
        //}

        //#endregion
    }
}
