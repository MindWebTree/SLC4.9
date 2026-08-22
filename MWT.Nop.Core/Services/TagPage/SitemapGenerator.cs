//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Infrastructure;
//using Microsoft.AspNetCore.Mvc.Routing;
//using Nop.Core;
//using Nop.Core.Domain.Common;
//using Nop.Core.Domain.Localization;
//using Nop.Core.Events;
//using Nop.Core.Infrastructure;
//using Nop.Services.Localization;
//using Nop.Services.Seo;
//using System.Globalization;
//using System.Text;
//using System.Xml;

//namespace MWT.Nop.Core.Services.TagPage
//{
//    public partial class SitemapGenerator : ISitemapGenerator
//    {
//        #region Fields


//        private readonly IActionContextAccessor _actionContextAccessor;
//        private readonly IEventPublisher _eventPublisher;
//        private readonly ILanguageService _languageService;
//        private readonly IStoreContext _storeContext;
//        private readonly IUrlHelperFactory _urlHelperFactory;
//        private readonly IUrlRecordService _urlRecordService;
//        private readonly IWebHelper _webHelper;
//        private readonly LocalizationSettings _localizationSettings;
//        private readonly SitemapXmlSettings _sitemapXmlSettings;
//        private readonly ITagSlugService _tagSlugService;

//        #endregion

//        #region Ctor

//        public SitemapGenerator(
//            IActionContextAccessor actionContextAccessor,
//     IEventPublisher eventPublisher,
//            ILanguageService languageService,
//            IStoreContext storeContext,
//            IUrlHelperFactory urlHelperFactory,
//            IUrlRecordService urlRecordService,
//            IWebHelper webHelper,
//            LocalizationSettings localizationSettings,
//            SitemapXmlSettings sitemapSettings,
//            ITagSlugService tagSlugService)
//        {

//            _actionContextAccessor = actionContextAccessor;
//            _eventPublisher = eventPublisher;
//            _languageService = languageService;
//            _storeContext = storeContext;
//            _urlHelperFactory = urlHelperFactory;
//            _urlRecordService = urlRecordService;
//            _webHelper = webHelper;
//            _localizationSettings = localizationSettings;
//            _sitemapXmlSettings = sitemapSettings;
//            _tagSlugService = tagSlugService;
//        }

//        #endregion

//        #region   Methods

//        public async Task<string> TagsGenerateAsync(int? id)
//        {
//            await using var stream = new MemoryStream();
//            await TagsGenerateAsync(stream, id);

//            return Encoding.UTF8.GetString(stream.ToArray());
//        }
//        #endregion

//        #region Utilities
//        protected virtual async Task TagsGenerateAsync(Stream stream, int? id)
//        {
//            //generate all URLs for the sitemap
//            var sitemapUrls = await TagsGenerateUrlsAsync();

//            //split URLs into separate lists based on the max size 
//            var sitemaps = sitemapUrls
//                .Select((url, index) => new { Index = index, Value = url })
//                .GroupBy(group => group.Index / NopSeoDefaults.SitemapMaxUrlNumber)
//                .Select(group => group
//                    .Select(url => url.Value)
//                    .ToList()).ToList();

//            if (!sitemaps.Any())
//                return;

//            if (id.HasValue)
//            {
//                //requested sitemap does not exist
//                if (id.Value == 0 || id.Value > sitemaps.Count)
//                    return;

//                //otherwise write a certain numbered sitemap file into the stream
//                await WriteSitemapAsync(stream, sitemaps.ElementAt(id.Value - 1));
//            }
//            else
//            {
//                //URLs more than the maximum allowable, so generate a sitemap index file
//                if (sitemapUrls.Count >= NopSeoDefaults.SitemapMaxUrlNumber)
//                {
//                    //write a sitemap index file into the stream
//                    await WriteSitemapIndexAsync(stream, sitemaps.Count);
//                }
//                else
//                {
//                    //otherwise generate a standard sitemap
//                    await WriteSitemapAsync(stream, sitemaps.First());
//                }
//            }
//        }
//        protected virtual async Task<IList<SiteMapUrlExtendedModel>> TagsGenerateUrlsAsync()
//        {
//            var sitemapUrls = new List<SitemapUrl>();
//            sitemapUrls.AddRange(await GetTagsUrlsAsync());
//            return sitemapUrls;
//        }
//        protected virtual async Task<IEnumerable<SitemapUrl>> GetTagsUrlsAsync()
//        {
//            var _questionAnswerService = EngineContext.Current.Resolve<IQuestionAnswerService>();

//            return await (await _tagSlugService.GetAllAsync())
//                .SelectAwait(async tag => await GetLocalizedSitemapUrlAsync("TagPage", GetTagSeoRouteParamsAwait(tag), tag.UpdatedOnUtc)).ToListAsync();

//        }
//        protected virtual Func<int?, Task<object>> GetTagSeoRouteParamsAwait(TagSlugMapping model)

//        {
//            return async lang => new { tagSlug = model.Slug };
//        }
//        public virtual async Task<SitemapUrl> GetLocalizedSitemapUrlAsync(string routeName,
//           Func<int?, Task<object>> getRouteParamsAwait = null,
//           DateTime? dateTimeUpdatedOn = null,
//           UpdateFrequency updateFreq = UpdateFrequency.Weekly)
//        {
//            var urlHelper = GetUrlHelper();

//            //url for current language
//            var url = urlHelper.RouteUrl(routeName,
//                getRouteParamsAwait != null ? await getRouteParamsAwait(null) : null,
//                await GetHttpProtocolAsync());

//            var updatedOn = dateTimeUpdatedOn ?? DateTime.UtcNow;
//            var languages = _localizationSettings.SeoFriendlyUrlsForLanguagesEnabled
//                ? await _languageService.GetAllLanguagesAsync()
//                : null;

//            if (languages == null)
//                return new SitemapUrl(url, new List<string>(), updateFreq, updatedOn);

//            var pathBase = _actionContextAccessor.ActionContext.HttpContext.Request.PathBase;
//            //return list of localized urls
//            var localizedUrls = await languages
//                .SelectAwait(async lang =>
//                {
//                    var currentUrl = urlHelper.RouteUrl(routeName,
//                        getRouteParamsAwait != null ? await getRouteParamsAwait(lang.Id) : null,
//                        await GetHttpProtocolAsync());

//                    if (string.IsNullOrEmpty(currentUrl))
//                        return null;

//                    //Extract server and path from url
//                    var scheme = new Uri(currentUrl).GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped);
//                    var path = new Uri(currentUrl).PathAndQuery;

//                    //Replace seo code
//                    var localizedPath = path
//                        .RemoveLanguageSeoCodeFromUrl(pathBase, true)
//                        .AddLanguageSeoCodeToUrl(pathBase, true, lang);

//                    return new Uri(new Uri(scheme), localizedPath).ToString();
//                })
//                .Where(value => !string.IsNullOrEmpty(value))
//                .ToListAsync();

//            return new SitemapUrl(url, localizedUrls, updateFreq, updatedOn);
//        }
//        protected virtual IUrlHelper GetUrlHelper()
//        {
//            return _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
//        }
//        protected virtual async Task<string> GetHttpProtocolAsync()
//        {
//            return (await _storeContext.GetCurrentStoreAsync()).SslEnabled ? Uri.UriSchemeHttps : Uri.UriSchemeHttp;
//        }
//        protected virtual async Task WriteSitemapAsync(Stream stream, IList<SitemapUrl> sitemapUrls)
//        {
//            await using var writer = new XmlTextWriter(stream, Encoding.UTF8)
//            {
//                Formatting = Formatting.Indented
//            };
//            writer.WriteStartDocument();
//            writer.WriteStartElement("urlset");
//            writer.WriteAttributeString("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9");
//            writer.WriteAttributeString("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
//            writer.WriteAttributeString("xmlns:xhtml", "http://www.w3.org/1999/xhtml");
//            writer.WriteAttributeString("xsi:schemaLocation", "http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd");

//            //write URLs from list to the sitemap
//            foreach (var sitemapUrl in sitemapUrls)
//            {
//                //write base url
//                await WriteSitemapUrlAsync(writer, sitemapUrl);

//                //write all alternate url if exists
//                foreach (var alternate in sitemapUrl.AlternateLocations
//                    .Where(p => !p.Equals(sitemapUrl.Location, StringComparison.InvariantCultureIgnoreCase)))
//                {
//                    await WriteSitemapUrlAsync(writer, new SitemapUrl(alternate, sitemapUrl));
//                }
//            }

//            writer.WriteEndElement();
//        }
//        protected virtual async Task WriteSitemapUrlAsync(XmlTextWriter writer, SitemapUrl sitemapUrl)
//        {
//            if (string.IsNullOrEmpty(sitemapUrl.Location))
//                return;

//            writer.WriteStartElement("url");

//            var loc = await XmlHelper.XmlEncodeAsync(sitemapUrl.Location);
//            writer.WriteElementString("loc", loc);

//            //write all related url
//            foreach (var alternate in sitemapUrl.AlternateLocations)
//            {
//                if (string.IsNullOrEmpty(alternate))
//                    continue;

//                //extract seo code
//                var altLoc = await XmlHelper.XmlEncodeAsync(alternate);
//                var altLocPath = new Uri(altLoc).PathAndQuery;
//                var (_, lang) = await altLocPath.IsLocalizedUrlAsync(_actionContextAccessor.ActionContext.HttpContext.Request.PathBase, true);

//                if (string.IsNullOrEmpty(lang?.UniqueSeoCode))
//                    continue;

//                writer.WriteStartElement("xhtml:link");
//                writer.WriteAttributeString("rel", "alternate");
//                writer.WriteAttributeString("hreflang", lang.UniqueSeoCode);
//                writer.WriteAttributeString("href", altLoc);
//                writer.WriteEndElement();
//            }

//            writer.WriteElementString("changefreq", sitemapUrl.UpdateFrequency.ToString().ToLowerInvariant());
//            writer.WriteElementString("lastmod", sitemapUrl.UpdatedOn.ToString(NopSeoDefaults.SitemapDateFormat, CultureInfo.InvariantCulture));
//            writer.WriteEndElement();
//        }
//        protected virtual async Task WriteSitemapIndexAsync(Stream stream, int sitemapNumber)
//        {
//            var urlHelper = GetUrlHelper();

//            await using var writer = new XmlTextWriter(stream, Encoding.UTF8)
//            {
//                Formatting = Formatting.Indented
//            };
//            writer.WriteStartDocument();
//            writer.WriteStartElement("sitemapindex");
//            writer.WriteAttributeString("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9");
//            writer.WriteAttributeString("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
//            writer.WriteAttributeString("xmlns:xhtml", "http://www.w3.org/1999/xhtml");
//            writer.WriteAttributeString("xsi:schemaLocation", "http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd");

//            //write URLs of all available sitemaps
//            for (var id = 1; id <= sitemapNumber; id++)
//            {
//                var url = urlHelper.RouteUrl("sitemap-indexed.xml", new { Id = id }, await GetHttpProtocolAsync());
//                var location = await XmlHelper.XmlEncodeAsync(url);

//                writer.WriteStartElement("sitemap");
//                writer.WriteElementString("loc", location);
//                writer.WriteElementString("lastmod", DateTime.UtcNow.ToString(NopSeoDefaults.SitemapDateFormat));
//                writer.WriteEndElement();
//            }

//            writer.WriteEndElement();
//        }

//        #endregion  
//    }
//}
