using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using MWT.Nop.Core.Domain.Campaign_Management;
using MWT.Nop.Core.Domain.IpAddress;
using MWT.Nop.Core.Domain.Mailchimp;
using MWT.Nop.Core.Infrastructure;
using MWT.Nop.Core.Service.Campaign_Management;
using MWT.Nop.Core.Services.Catalog;
using MWT.Nop.Core.Services.Message;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Messages;
using Nop.Core.Http;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customizations.IpAddress;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Seo;
using Nop.Services.Stores;
using System.Data;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;

namespace MWT.Nop.Core.Services.MailChimp
{
    public class MailchimpService : IMailchimpService
    {

        #region Fields
        public string ApiKey { get; set; }
        public string ListID { get; set; }
        public string ListID_newltter { get; set; }
        public string ListID_newltter_Mobile { get; set; }
        public string Host { get; set; }
        public string ListID_AbandonedCard { get; set; }

        private readonly ISettingService _settingService;
        private readonly IMailchimpSegmentsService _mailchimpSegmentsService;
        private readonly ILogger _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICustomNewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ICategoryService _categoryService;
        private readonly ICustomWorkContext _workContext;
        private readonly IIpAddressService _ipAddressService;
        private readonly ICustomSpecificationAttributeService _specificationAttributeService;
        private readonly ICampaignManagementService _campaignManagementService;
        private readonly IStoreService _storeService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IStoreContext _storeContext;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IProductService _productService;
        private readonly IUrlRecordService _urlRecordService;
        #endregion
        public MailchimpService(ISettingService settingService,
            IMailchimpSegmentsService mailchimpSegmentsService,
             ILogger logger, IHttpContextAccessor httpContextAccessor,
              ICustomNewsLetterSubscriptionService newsLetterSubscriptionService,
              IHttpClientFactory httpClientFactory,
              ICategoryService categoryService,
              ICustomWorkContext workContext,
              IIpAddressService ipAddressService,
              ICustomSpecificationAttributeService specificationAttributeService,
              ICampaignManagementService campaignManagementService,
              IRepository<QueuedMailChimpCartSignUp> queuedMailChimpCartSignUpRepository,
              IStoreService storeService,
              IUrlHelperFactory urlHelperFactory,
              IStoreContext storeContext,
              IActionContextAccessor actionContextAccessor,
               IProductService productService,
               IUrlRecordService urlRecordService
              )
        {
            this._settingService = settingService;
            this._mailchimpSegmentsService = mailchimpSegmentsService;
            this._logger = logger;
            this._httpContextAccessor = httpContextAccessor;
            this._newsLetterSubscriptionService = newsLetterSubscriptionService;
            this._httpClientFactory = httpClientFactory;
            this._categoryService = categoryService;
            this._workContext = workContext;
            this._ipAddressService = ipAddressService;
            this._campaignManagementService = campaignManagementService;
            this._specificationAttributeService = specificationAttributeService;
            this._storeService = storeService;
            this._urlHelperFactory = urlHelperFactory;
            this._storeContext = storeContext;
            this._productService = productService;
            this._urlRecordService = urlRecordService;
            this._actionContextAccessor = actionContextAccessor;
            ListID = (_settingService.GetSettingByKeyAsync<string>("MailChimp.ItemStock.ListId")).Result;
            ApiKey = (_settingService.GetSettingByKeyAsync<string>("MailChimp.ApiKey")).Result;
            Host = (_settingService.GetSettingByKeyAsync<string>("MailChimp.Api.EndPoint")).Result;
        }

        #region front end Methods

        public async Task PopupSignup(int campaignid, string email, string url, string ip, string userAgent, IpBasedUserAddress address,
               string CategoryID = "", string phone = "")
        {

            var integrations = await this._campaignManagementService.GetCampaignIntegration(campaignid);
            if (integrations.Count > 0)
            {
                root root = new root();
                root.email_address = email;
                root.status = "subscribed";
                MergeFields merge_fields = new MergeFields();
                string productID = GetProductIdFromUrl(url).ToString();
                if (!string.IsNullOrEmpty(productID) && CategoryID != "")
                    merge_fields.CATNAME = GetCategoryIdFromUrl(productID, ref CategoryID);
                else
                    merge_fields.CATNAME = "";
                merge_fields.CATNAME = CategoryID.ToString();
                merge_fields.PRODUCTID = "";
                merge_fields.FNAME = "";
                if (phone != "")
                {
                    merge_fields.CONTACT = phone;
                    merge_fields.SMSPHONE = phone;
                }
                root.merge_fields = merge_fields;
                root.location = new location() { latitude = address == null ? 0 : address.Latitude, longitude = address == null ? 0 : address.Latitude };

                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.NullValueHandling = NullValueHandling.Ignore;
                string data = JsonConvert.SerializeObject(root, settings);

      
                var response = await Request("PUT", data, integrations.FirstOrDefault().Host + "/lists/" + integrations.FirstOrDefault().ListId + "/members/" + CalculateMD5Hash(email.ToLower()), "", integrations.FirstOrDefault().ApiKey);
                if (response == "" && root.merge_fields.SMSPHONE != "")
                {
                    root.merge_fields.SMSPHONE = "";
                    data = JsonConvert.SerializeObject(root);
                    response = await Request("PUT", data, integrations.FirstOrDefault().Host + "/lists/" + integrations.FirstOrDefault().ListId + "/members/" + CalculateMD5Hash(email.ToLower()), "", integrations.FirstOrDefault().ApiKey);

                }
                if (response != "")
                {

                    await AttachCampaignSegment(email, integrations, url, integrations.FirstOrDefault().ApiKey, integrations.FirstOrDefault().Host, integrations.FirstOrDefault().ListId);
                    var infoTags = await getInfoTags(url, userAgent);
                    infoTags.Add("PopUP");
                    await AttachInfoSegment(email, integrations.FirstOrDefault().Host, integrations.FirstOrDefault().ListId, integrations.FirstOrDefault().ApiKey, infoTags);
                }
            }
        }

        public async Task CustomerSignup(string email, string firstName, List<string> infoTags, string url = "", string userAgent = "")
        {
            infoTags.AddRange(await getInfoTags(url, userAgent, url));
            await MailchimpOperation_newltter(email, "", 0, infoTags, "", url, userAgent, false, firstName);
        }
        public async Task NewsLetterSignup(string email, string fromWhere, string url, string absoluteUrl, string userAgent, string segments = "", int productID = 0, int CategoryID = 0, string firstName = "")
        {
            List<string> infoTags = new List<string>();
            try
            {
                infoTags = await getInfoTags(absoluteUrl, userAgent, url);
            }
            catch
            {

            }
            if (!string.IsNullOrEmpty(segments))
                infoTags.AddRange(segments.Split(','));
            if (fromWhere.IndexOf("~~~") > 0)
            {
                string[] location = fromWhere.Split(new string[] { "~~~" }, StringSplitOptions.None);
                if (location.Length > 1)
                {
                    fromWhere = location[1];
                    infoTags.Add(location[0]);
                }
            }
            if (fromWhere == "HomePageBanner")
                infoTags.Add("Source:HeaderSignup");

            if (fromWhere.ToLower().Contains("footer") && !fromWhere.Equals("footer", StringComparison.InvariantCultureIgnoreCase))
                infoTags.Add("footer");

            if (productID != 0)
                await MailchimpOperation_newltter(email, "PSection", productID, infoTags, GetCategoryIdFromUrl(productID.ToString()),
                    url, userAgent, true, firstName);
            else if (fromWhere == "to-the-trade")
                await mailchimpTTT(email, fromWhere, infoTags, url, userAgent, firstName);
            else if (fromWhere == "Farmhouse")
                await mailchimpFarmhouse(email, fromWhere, infoTags, url, userAgent, firstName);
            else if (fromWhere != "")
            {
                productID = GetProductIdFromUrl(absoluteUrl);
                await MailchimpOperation_newltter(email, fromWhere, productID, infoTags, productID == 0 ? "" : GetCategoryIdFromUrl(productID.ToString()), url, userAgent, true, firstName);
            }
        }

        #endregion
        private async Task AttachInfoSegment(string Email, string host, string listId, string apiKey, List<string> infoTags)
        {
            try
            {
                var segments = await this._mailchimpSegmentsService.Segments();
                if (segments.Count > 0)
                {

                    string segmentID = "";
                    if (infoTags != null)
                    {
                        foreach (string tag in infoTags)
                        {
                            segmentID = (from segment in segments
                                         where segment.NewsLetterForm.Equals(tag, StringComparison.InvariantCultureIgnoreCase)
                                         select segment.SegmentID.ToString()).FirstOrDefault();
                            if (!string.IsNullOrEmpty(segmentID))
                                await Request("POST", "{\"members_to_add\":[\"" + Email + "\"]}", host + "/lists/" + listId
                                    + "/segments/" + segmentID, "", apiKey);
                        }
                    }

                }
            }
            catch (Exception exp)
            {
                await this._logger.InsertLogAsync(LogLevel.Error,
                       "MailChimp Error In AttachSegment", exp.Message);
            }
        }
        private async Task AttachCampaignSegment(string email, IList<MailchimpCampaignIntegration> integrations, string url, string apiKey, string host, string listID)
        {
            Int64 segmentID = 0;
            try
            {
                if (!string.IsNullOrEmpty(url))
                {
                    Uri myUri = new Uri(url.ToLower());
                    var rows = from row in integrations
                               where row.UrlCondition != ""
                               && url == row.UrlCondition
                               select row;
                    if (rows.Count() > 0)
                        segmentID = 1;
                    string compareUrl = "";
                    if (myUri.Segments.Length == 1 && myUri.Segments[0] == "/")
                    {

                        var matchingRecords = from row in integrations
                                              where row.UrlCondition == "/"
                                              select row;
                        if (matchingRecords.Count() > 0)
                        {
                            matchingRecords = matchingRecords.OrderByDescending(c => c.UrlCondition.Length);
                            segmentID = Convert.ToInt64(matchingRecords.FirstOrDefault().SegmentID);
                        }
                    }
                    else
                    {
                        for (int i = myUri.Segments.Length; i > 0; i--)
                        {
                            compareUrl = "";
                            for (int j = 1; j < i; j++)
                            {
                                compareUrl = (compareUrl == "" ? "/" : compareUrl) + myUri.Segments[j];

                            }
                            compareUrl = compareUrl == "" ? "/" : compareUrl;
                            if (i != 0)
                            {

                                var matchingRecords = from row in integrations
                                                      where row.UrlCondition != ""
                                                      && row.UrlCondition.Length > 1
                                                      && compareUrl.Contains(row.UrlCondition)
                                                      select row;
                                if (matchingRecords.Count() > 0)
                                {
                                    matchingRecords = matchingRecords.OrderByDescending(c => c.UrlCondition.Length);
                                    segmentID = Convert.ToInt64(matchingRecords.FirstOrDefault().SegmentID);
                                }
                            }
                            if (segmentID > 0)
                                break;
                        }
                    }
                    if (segmentID > 0)
                        await Request("POST", "{\"members_to_add\":[\"" + email + "\"]}", host + "/lists/" + listID + "/segments/" + segmentID, "", apiKey);
                }
            }
            catch (Exception exp) { }
        }
        public async Task AttachSegment(string Email, string SegmentName, List<string> infoTags = null)
        {
            SegmentName = SegmentName.ToLower().Trim();
            try
            {

                var segments = await this._mailchimpSegmentsService.Segments();
                if (segments.Count > 0)
                {
                    string segmentID = "";
                    if (!string.IsNullOrEmpty(SegmentName))
                    {
                        segmentID = (from segment in segments
                                     where segment.NewsLetterForm.Equals(SegmentName, StringComparison.InvariantCultureIgnoreCase)
                                     select segment.SegmentID.ToString()).FirstOrDefault();

                        if (!string.IsNullOrEmpty(segmentID))
                            await Request("POST", "{\"members_to_add\":[\"" + Email + "\"]}", Host + "/lists/" + ListID + "/segments/" + segmentID);
                    }
                    if (infoTags != null)
                    {
                        foreach (string tag in infoTags)
                        {
                            segmentID = (from segment in segments
                                         where segment.NewsLetterForm.Equals(tag, StringComparison.InvariantCultureIgnoreCase)
                                         select segment.SegmentID.ToString()).FirstOrDefault();
                            if (!string.IsNullOrEmpty(segmentID))
                                await Request("POST", "{\"members_to_add\":[\"" + Email + "\"]}", Host + "/lists/" + ListID + "/segments/" + segmentID);
                            else
                            {
                                var content = await Request("POST", "{\"name\":\"" + tag + "\",\"type\":\"static\",\"static_segment\":[],\"list_id\":\"" + ListID + "\"}", Host + "/lists/" + ListID + "/segments/");

                                if (content != "error" && content != "")
                                {
                                    dynamic result = JsonConvert.DeserializeObject(content);
                                    await _mailchimpSegmentsService.Insert(new MailchimpSegments()
                                    {
                                        CreatedBy = "ScheduleTask",
                                        CreatedOn = DateTime.UtcNow,
                                        Listid = ListID,
                                        ListName = "",
                                        NewsLetterForm = tag,
                                        SegmentID = Convert.ToInt32(result.id),
                                        SegmentName = tag,
                                        UpdatedBY = "ScheduleTask",
                                        UpdatedON = DateTime.UtcNow
                                    });
                                    await Request("POST", "{\"members_to_add\":[\"" + Email + "\"]}", Host + "/lists/" + ListID + "/segments/" + result.id);
                                }
                            }
                        }
                    }

                }
            }
            catch (Exception exp)
            {
                await this._logger.InsertLogAsync(LogLevel.Error,
                       "MailChimp Error In AttachSegment", exp.Message);
            }
        }
        public async Task Operation(string Email, string SegmentName, string ProductID, string FNAME, string IpAddress, bool IsSynced = true, List<string> infoTags = null, string categoryID = "", string url = "", string userAgent = "", bool isNewsLetter = true)
        {
            try
            {
                Email = Email.Trim();
                if (!IsSynced || SegmentName == "OutOfStock" || SegmentName == "AbandonedCard" || await CheckEmailSubscriber(Email, ListID))
                {
                    root root = new root();
                    root.email_address = Email;
                    root.status = "subscribed";
                    MergeFields merge_fields = new MergeFields();
                    merge_fields.PRODUCTID = ProductID;
                    merge_fields.CATNAME = categoryID;
                    merge_fields.FNAME = FNAME;
                    root.merge_fields = merge_fields;
                    await GetLocationOfCustomer(root, IpAddress);

                    JsonSerializerSettings settings = new JsonSerializerSettings();
                    settings.NullValueHandling = NullValueHandling.Ignore;
                    string data = JsonConvert.SerializeObject(root, settings);
                    await Request("PUT", data, Host + "/lists/" + ListID + "/members/" + CalculateMD5Hash(Email.ToLower()));
                    if (isNewsLetter)
                        await InsertSubscriber(Email, ListID, IpAddress, SegmentName, url, userAgent);
                }
                await AttachSegment(Email, SegmentName, infoTags);
            }
            catch (Exception exp)
            {
                await this._logger.InsertLogAsync(LogLevel.Error,
                       "MailChimp Error In Email Subscribe", exp.Message);


            }
        }
        public async Task GetLocationOfCustomer(root obj, string IpAddress)
        {
            try
            {
                IpBasedUserAddress address = await GetStateAndCity(IpAddress);
                obj.location = new location() { latitude = address.Latitude, longitude = address.Longitude };
            }
            catch (Exception exp)
            {
                await this._logger.InsertLogAsync(LogLevel.Error,
                  "Geo Plugin Error in Getting Location", exp.Message);
            }
        }
        public async Task<bool> CartMailChimpOperations(string Email, string FName, string FromWhere, List<string> infoTags = null)
        {
            return await CartOperation(Email, FName, FromWhere, infoTags);
        }
        public async Task<bool> CartOperation(string Email, string FNAME, string SegmentName,
            List<string> infoTags = null, string url = "", string userAgent = "", string productId = "", string ipaddress = "")
        {
            if (infoTags == null)
                infoTags = new List<string>();
            if (!string.IsNullOrEmpty(url))
            {
                infoTags.AddRange(await getInfoTags(url, userAgent, url));
            }

            try
            {

                Email = Email.Trim();
                root root = new root();
                root.email_address = Email;
                root.status = "subscribed";
                MergeFields merge_fields = new MergeFields();
                merge_fields.FNAME = FNAME;
                merge_fields.PRODUCTID = productId;
          
                if (productId != "" && infoTags.Contains("Product ID " + productId) && infoTags.Contains("Add to cart"))
                {
                    int.TryParse(productId, out int _productId);
                    if (_productId > 0)
                    {
                        var product = await _productService.GetProductByIdAsync(_productId);
                        merge_fields.ADDTOCART = "Product ID " + productId;
                        merge_fields.CARTURL = await RouteUrlAsync(routeName: "Product", routeValues: new { id = product.Id, SeName = await _urlRecordService.GetSeNameAsync(product) });
                    }
                }

                root.merge_fields = merge_fields;
                string ipAddress = "";
                if (string.IsNullOrEmpty(ipaddress))
                {
                    try
                    {
                        ipAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString();
                    }
                    catch { }
                }
                await GetLocationOfCustomer(root, ipAddress);

                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.NullValueHandling = NullValueHandling.Ignore;
                string data = JsonConvert.SerializeObject(root, settings);

              
                var response = await Request("PUT", data, Host + "/lists/" + ListID + "/members/" + CalculateMD5Hash(Email.ToLower()));
                if (response != "")
                {
                    await AttachSegment(Email, SegmentName, infoTags);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception exp)
            {
                await this._logger.InsertLogAsync(LogLevel.Error,
      "MailChimp Error In Email Subscribe", exp.Message);

                return false;
            }
        }
        public async Task<string> mailchimpTTT(string Email, string fromwhere, List<string> infoTags = null, string url = "", string useragent = "", string firstName = "")
        {
            try
            {
                string ipAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString();
                new Task(async () => { await Operation(Email, fromwhere, "", firstName, ipAddress, false, infoTags, "", url, useragent); }).Start();
                return "yes";
            }
            catch
            {
                return "Error";
            }
        }
        public async Task<string> mailchimpFarmhouse(string Email, string fromwhere, List<string> infoTags = null, string url = "", string userAgent = "", string firstName = "")
        {
            try
            {
                if (!await CheckEmailSubscriber(Email.Trim(), ListID))
                {
                    string ipAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString();
                    new Task(async () =>
                    {
                        await Operation(Email, fromwhere, "", firstName, ipAddress, false, infoTags, "", url, userAgent);
                    }).Start();
                    return "yes";
                }
                else
                {
                    return "error";
                }
            }
            catch
            {
                return "Error";
            }
        }
        public async Task MailchimpOperation(string ProductId, string Email, List<string> infoTags = null, string categoryID = "")
        {
            try
            {
                string ipAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString();
                new Task(async () => { await Operation(Email, "OutOfStock", ProductId, "", ipAddress, true, infoTags, categoryID); }).Start();
            }
            catch { }
        }
        public async Task MailchimpOperation_AbandonedCard(string Email, string FNAME, List<string> infoTags = null)
        {
            try
            {
                string ipAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString();
                new Task(async () => { await Operation(Email, "AbandonedCard", "", FNAME, ipAddress, true, infoTags); }).Start();
            }
            catch { }
        }


        public async Task<string> MailchimpOperation_newltter(string Email, string fromWhere, int productID = 0, List<string> infoTags = null, string categoryID = "", string url = "", string userAgent = "", bool isNewsLetter = true, string firstName = "") // For  signups--Lokesh 
        {
            try
            {
                if (!isNewsLetter || !await CheckEmailSubscriber(Email.Trim(), ListID))
                {
                    string ipAddress = "";
                    try
                    {
                        ipAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString();
                    }
                    catch
                    {

                    }
                    if (fromWhere != "PSection")
                    {
                        new Task(async () => { await Operation(Email, fromWhere, productID.ToString(), firstName, ipAddress, false, infoTags, categoryID, url, userAgent, isNewsLetter); }).Start();
                        return "yes";
                    }
                    else
                    {
                        await Operation(Email, fromWhere, productID.ToString(), firstName, ipAddress, false, infoTags, categoryID, url, userAgent, isNewsLetter);
                        // Send Campaign
                        try

                        {
                            //  var runtimeParameters = string.Format("ProductID={0}", productID);
                            //  var xmlpackage = new XmlPackage(
                            // packageName: "Mailer.productPageContent.xml.config",
                            // customer: Customer.Current,
                            // additionalRuntimeParms: runtimeParameters
                            //);
                            //  var parser = new Parser();
                            //  string html = AppLogic.RunXmlPackage(xmlpackage, parser, Customer.Current, Customer.Current.SkinID, true, true);
                            //  //Task.Run(() => CampaignOperations(ListID, Email, html, "A warm welcome & exclusive offer for you!"));
                            string subject = "A warm welcome & exclusive offer for you!";
                            //AppLogic.SendMail(subject, html, true, AppLogic.AppConfig("MailMe_FromAddress"), AppLogic.AppConfig("MailMe_FromName"), Email, "", "", "", AppLogic.MailServer());
                        }
                        catch { }


                        // end
                        return "yes";
                    }
                }
                else
                {
                    return "error";
                }
            }
            catch { return "yes"; }
        }

        public void ProductPageSendCampaign(string html)
        {

        }

        public async Task<string> MailchimpOperation_newltter(string Email, List<string> infoTags = null)
        {
            try
            {
                if (!await CheckEmailSubscriber(Email.Trim(), ListID))
                {
                    string ipAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString();
                    new Task(async () => { await Operation(Email, "", "", "", ipAddress, false, infoTags); }).Start();
                    return "yes";
                }
                else
                {
                    return "error";
                }
            }
            catch { return "yes"; }
        }

        public async Task<string> MailchimpOperation_newltter_Mobile(string Email, List<string> infoTags = null)
        {
            try
            {
                if (!await CheckEmailSubscriber(Email.Trim(), ListID))
                {
                    string ipAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString();
                    new Task(async () => { await Operation(Email, "", "", "", ipAddress, false, infoTags); }).Start();
                    return "yes";
                }
                else
                {
                    return "error";
                }
            }
            catch { return "yes"; }
        }
        public async Task<bool> CheckEmailSubscriber(string Email, string ListID)
        {
            bool result = false;
            try
            {
                if (!(String.IsNullOrEmpty(Email)) && !(String.IsNullOrEmpty(ListID)))
                    result = await _newsLetterSubscriptionService.CheckEmailSubscriber(Email, ListID);
            }
            catch (Exception ex)
            {
            }
            return result;

        }
        public async Task<bool> InsertSubscriber(string Email, string ListID, string ipAddress, string SegmentName, string url, string UserAgent)
        {
            bool result = false;
            try
            {

                if (!(String.IsNullOrEmpty(Email)) && !(String.IsNullOrEmpty(ListID)))
                {
                    await this._newsLetterSubscriptionService.InsertNewsLetterSubscriptionAsync(new NewsLetterSubscription()
                    {
                        Email = Email,
                        MailChimpListId = ListID,
                        Active = true,
                        MailChimpAppId = ApiKey,
                        IpAddress = ipAddress,
                        Section = SegmentName,
                        Url = url,
                        UserAgent = UserAgent,
                        CreatedOnUtc = DateTime.Now
                    });

                }
            }
            catch (Exception ex)
            {

            }
            return result;

        }
        private async Task<string> Request(string Method, string Data, string host, string type = "", string apiKey = "")
        {
            string Response = string.Empty;
            try
            {
                var httpClient = _httpClientFactory.CreateClient(NopHttpDefaults.DefaultHttpClient);
                if (type == "campaign")
                {
                    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                    httpClient.DefaultRequestHeaders.Add("Authorization", "apikey " + (string.IsNullOrEmpty(apiKey) ? ApiKey : apiKey));
                    var requestContent = new StringContent(Data,
              Encoding.UTF8, MimeTypes.ApplicationJson);
                    var response = Method == "PUT" ? await httpClient.PutAsync(host, requestContent) :
                        await httpClient.PostAsync(host, requestContent);

                    var content = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == HttpStatusCode.OK)
                        return string.IsNullOrEmpty(content) ? "Success" : content;
                    throw new Exception(content);


                }
                else
                {

                    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                    httpClient.DefaultRequestHeaders.Add("Authorization", "apikey " + (string.IsNullOrEmpty(apiKey) ? ApiKey : apiKey));
                    var requestContent = new StringContent(Data,
        Encoding.UTF8, MimeTypes.ApplicationJson);
                    requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    var response = Method == "PUT" ? await httpClient.PutAsync(host, requestContent)
                        : await httpClient.PostAsync(host, requestContent);
                    var content = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == HttpStatusCode.OK)
                        return content;
                    throw new Exception(content);

                }
            }
            catch (Exception exp)
            {
                await this._logger.InsertLogAsync(LogLevel.Error,
                       "MailChimp Error In Calling Api", exp.Message);

                return "";
            }
        }




        #region Zoho Lead Section Start
        public async Task SendZohoLeadToMailchimp(string TagName, string LeadID, string LeadSource, string Email, string FirstName, string Description)
        {
            List<string> infoTags = new List<string>();
            try
            {
                string ipAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString();
                infoTags = await getInfoTags(_httpContextAccessor.HttpContext.Request.Headers["Referer"].ToString(), _httpContextAccessor.HttpContext.Request.Headers["User-Agent"].ToString());
                infoTags.Add(TagName);
                new Task(async () => { await LeadOperation(Email, FirstName, LeadSource, ipAddress, LeadID, Description, infoTags); }).Start();
            }
            catch { }
        }
        public async Task LeadAttachSegment(string Email, List<string> infoTags = null)
        {
            try
            {
                var segments = await this._mailchimpSegmentsService.Segments();
                if (segments.Count > 0)
                {
                    if (infoTags != null)
                    {
                        foreach (string tag in infoTags)
                        {
                            string segmentID = (from segment in segments
                                                where segment.NewsLetterForm.Equals(tag, StringComparison.InvariantCultureIgnoreCase)
                                                select segment.SegmentID.ToString()).FirstOrDefault();

                            if (!string.IsNullOrEmpty(segmentID))
                                await Request("POST", "{\"members_to_add\":[\"" + Email + "\"]}", Host + "/lists/" + ListID + "/segments/" + segmentID);
                        }
                    }
                }

            }
            catch (Exception exp)
            {
                await this._logger.InsertLogAsync(LogLevel.Error,
                      "MailChimp Error In AttachSegment", exp.Message);

            }
        }
        public async Task LeadOperation(string Email, string FirstName, string LeadSource, string IpAddress, string LeadID, string Description, List<string> infoTags = null)
        {
            try
            {
                Email = Email.Trim();
                if (!await CheckEmailSubscriber(Email, ListID))
                {
                    leadroot l_root = new leadroot();
                    l_root.email_address = Email;
                    l_root.status = "subscribed";
                    LeadMergeFields merge_fields = new LeadMergeFields();
                    merge_fields.FNAME = FirstName;
                    merge_fields.LEADID = LeadID;
                    merge_fields.LEADSOURCE = LeadSource;
                    merge_fields.DESC = Description;
                    l_root.merge_fields = merge_fields;
                    await GetLeadLocationOfCustomer(l_root, IpAddress);
                    JsonSerializerSettings settings = new JsonSerializerSettings();
                    settings.NullValueHandling = NullValueHandling.Ignore;
                    string data = JsonConvert.SerializeObject(l_root, settings);
                 
                    await Request("PUT", data, Host + "/lists/" + ListID + "/members/" + CalculateMD5Hash(Email.ToLower()));
                    await InsertSubscriber(Email, ListID, IpAddress, LeadSource, "", "");
                }
                await LeadAttachSegment(Email, infoTags);
            }
            catch (Exception exp)
            {
                await this._logger.InsertLogAsync(LogLevel.Error,
                 "MailChimp Error In Email Subscribe", exp.Message);
            }
        }
        public async Task GetLeadLocationOfCustomer(leadroot obj, string IpAddress)
        {
            try
            {
                IpBasedUserAddress address = await GetStateAndCity(IpAddress);
                obj.location = new location() { latitude = address.Latitude, longitude = address.Longitude };
            }
            catch (Exception exp)
            {
                await this._logger.InsertLogAsync(LogLevel.Error,
                 "Geo Plugin Error in Getting Location", exp.Message);
            }
        }
        public async Task<List<string>> getInfoTags(string url, string agent, string absoluteUrl = "")
        {
            url = urlWithoutProtocol(url);
            List<string> infoTags = new List<string>();
            string[] segments = url.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length > 0)
                if (segments.Length == 1)
                    infoTags.Add("home");
                else
                {// Author: Mind Web Tree 03-10-2023
                 // Implemented Tag "Parent Category" in case category is parent else "Category"
                    string categoryName = "";
                    int parentCategoryId = -1;
                    try
                    {

                        if (segments.Length > 2 && segments[1].Equals("category", StringComparison.InvariantCultureIgnoreCase))
                        {
                            int.TryParse(segments[2], out int categoryId);
                            var category = await _categoryService.GetCategoryByIdAsync(categoryId);
                            if (category != null)
                            {
                                infoTags.Add(category.Name);
                                parentCategoryId = category.ParentCategoryId;
                            }
                            // Author: Mind Web Tree 04-10-2023
                            // Qick Ship Tag
                            if (segments[2] == "426")
                                infoTags.AddRange(FiltersTags(absoluteUrl, segments[2]));
                        }
                    }
                    catch { }
                    // Author: Mind Web Tree 03-10-2023
                    // Implemented Tag "Main Category tag" for Product"


                    if (segments[1].Equals("category", StringComparison.InvariantCultureIgnoreCase) && parentCategoryId == 0)
                        infoTags.Add("parent " + segments[1]);
                    // Author: Mind Web Tree 04-10-2023
                    // Implemented Tag for Topic Page Name"
                    else if (segments[1].Equals("topic", StringComparison.InvariantCultureIgnoreCase) && segments.Length > 2)
                    {
                        infoTags.Add(segments[2]);
                        infoTags.Add(segments[1]);
                    }
                    else
                        infoTags.Add(segments[1]);

                    // Author: Mind Web Tree 03-10-2023
                    // Code to add Product Main Category tag
                    #region Main Category tag for Product page

                    if (segments.Length > 2 && segments[1].Equals("product", StringComparison.InvariantCultureIgnoreCase))
                    {
                        int.TryParse(segments[2], out int productId);
                        if (productId != 0)
                        {
                            var mainCategoryId = await _specificationAttributeService.GetMainCategoryOfProduct(productId);
                            if (mainCategoryId != 0)
                            {
                                string mainCategoryOFproduct = (await _categoryService.GetCategoryByIdAsync(mainCategoryId))?.Name;

                                if (!string.IsNullOrEmpty(mainCategoryOFproduct))
                                    infoTags.Add(mainCategoryOFproduct);
                            }
                        }
                    }

                    #endregion
                }
            int devType = _workContext.GetActualDevice(agent);
            switch (devType)
            {
                case 1:
                    infoTags.Add("desktop");
                    break;
                case 2:
                    infoTags.Add("tablet");
                    break;
                case 3:
                    infoTags.Add("mobile");
                    break;
            }
            return infoTags;
        }
        public int GetProductIdFromUrl(string url)
        {
            url = urlWithoutProtocol(url);
            int productID = 0;
            try
            {
                string[] segments = url.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                if (segments != null && segments.Length > 2)
                {
                    if (segments[1].Equals("product", StringComparison.InvariantCultureIgnoreCase))
                        int.TryParse(segments[2], out productID);
                }
            }
            catch { }
            return productID;
        }
        public string GetCategoryIdFromUrl(string productID)
        {
            //string categoryID = "";
            //try
            //{
            //    categoryID = HttpContext.Profile != null && CommonLogic.IsInteger(HttpContext.Profile.GetPropertyValue("LastViewedEntityInstanceID").ToString()) ? HttpContext.Profile.GetPropertyValue("LastViewedEntityInstanceID").ToString() : string.Empty;
            //    if ((HttpContext.Profile != null && HttpContext.Profile.GetPropertyValue("LastViewedEntityName") != null ? HttpContext.Profile.GetPropertyValue("LastViewedEntityName").ToString() : string.Empty).CompareTo("Category") == 0 && !string.IsNullOrEmpty(categoryID) && !string.IsNullOrEmpty(productID))
            //    {
            //        categoryID = DB.GetSqlS(string.Format("select category.name as S from productcategory join category on category.categoryid=productcategory.categoryid where productID={0} and category.categoryid={1}", productID, categoryID));
            //        if (string.IsNullOrEmpty(categoryID))
            //            categoryID = "DirectLanding";
            //    }
            //    else
            //        categoryID = "";
            //}
            //catch { }

            return "";
        }

        private string GetCategoryIdFromUrl(string productID, ref string categoryID)
        {
            try
            {
                //categoryID = DB.GetSqlS(string.Format("select category.name as S from productcategory join category on category.categoryid=productcategory.categoryid where productID={0} and category.categoryid={1}", productID, categoryID));
                //if (string.IsNullOrEmpty(categoryID))
                categoryID = "DirectLanding";
            }
            catch { }
            return categoryID;
        }
        public List<string> FiltersTags(string url, string categoryId)
        {
            List<string> tags = new List<string>();
            var urlParts = url.Split('?');
            if (urlParts.Length > 1)
            {
                var queryString = urlParts[1].Split('&');
                foreach (var query in queryString)
                {
                    if (query.Split('=').Length > 1)
                        tags.Add(categoryId + "-" + Regex.Replace(query.Split('=')[1], @"[^a-zA-Z0-9\s]+", "", RegexOptions.Compiled));
                }
            }
            return tags;
        }
        public string urlWithoutProtocol(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                url = url.Replace("http://", "").Replace("https://", "");
                if (url.IndexOf("?") > 0)
                    url = url.Substring(0, url.IndexOf("?"));

            }
            return url ?? "";
        }
        public class LeadMergeFields
        {
            public string FNAME { get; set; }
            public string LEADID { get; set; }
            public string LEADSOURCE { get; set; }
            public string DESC { get; set; }


        }
        public class leadroot
        {
            public string status { get; set; }
            public LeadMergeFields merge_fields { get; set; }
            public location location { get; set; }
            public string email_address { get; set; }
        }
        #endregion Zoho Lead Section End


        #region Campaign Operations

        public async void CampaignOperations(string listID, string email, string html, string subject)
        {
            await Weblegs_createcampaign(listID, subject, email, html);
        }

        public async Task<bool> Weblegs_createcampaign(string listID, string Subject, string email, string html)
        {
            string response = "";
            string json = "{\"type\":\"regular\",\"recipients\":{\"list_id\":\"" + listID + "\",\"segment_opts\":{\"match\":\"any\",\"conditions\":[{\"condition_type\":\"EmailAddress\",\"field\":\"email\",\"op\":\"is\",\"value\":\"" + email + "\"}]}},\"settings\":{\"subject_line\":\"" + Subject + "\",\"from_name\":\"Sierra Living Concepts\",\"reply_to\":\"sales@sierralivingconcepts.com\"}}";
            response = await Request("POST", json, Host + "/campaigns");
            if (response != "error")
            {
                dynamic campaignDetails = JsonConvert.DeserializeObject(response);
                return await AddContentToCampaign((string)campaignDetails.id, html);
            }
            else
                return false;
        }

        public async Task<bool> AddContentToCampaign(string campaignID, string Html)
        {
            string json = "{\"html\":\"" + Html.Replace("\"", "\\\"") + "\"}";
            string response = await Request("PUT", json, Host + "/campaigns/" + campaignID + "/content", "campaign");
            if (response != "error")
                return await Send_Campaign(campaignID);
            else
                return false;
        }

        public async Task<bool> Send_Campaign(string campaignID)
        {
            string response = await Request("POST", "", Host + "/campaigns/" + campaignID + "/actions/send");
            return string.IsNullOrEmpty(response) ? false : true;
        }

        #endregion Campaign Operations

        #region IpBased Functions
        public async Task<IpBasedUserAddress> GetStateAndCity(string ip)
        {
            IpBasedUserAddress address = new IpBasedUserAddress();
            try
            {
                long ipAddressIntFormat = (long)(uint)IPAddress.NetworkToHostOrder(
                    (int)IPAddress.Parse(ip).Address);
                await ReadDataFromDB(ipAddressIntFormat, address);
                if (address == null || address.CountryID == null)
                    GetDataFromAPI(ipAddressIntFormat, ip, address);
            }
            catch { }
            return address;
        }
        public async Task ReadDataFromDB(long ip, IpBasedUserAddress address)
        {
            try
            {
                var _address = await _ipAddressService.GetDetailsByIpAddress(ip.ToString());
                double latitude = 0;
                double longitude = 0;
                address.City = _address.City;
                address.Country = _address.Country;
                address.CountryID = _address.Country_Code;
                double.TryParse(_address.Latitude, out latitude);
                address.Latitude = latitude;
                double.TryParse(_address.Longitude, out longitude);
                address.Longitude = longitude;

            }
            catch (Exception exp)
            {
                await this._logger.InsertLogAsync(LogLevel.Error,
                      "Campaign Popup Module. Failed to Read Data from Database", exp.Message);
            }
        }
        public async void SaveDataInDB(IpBasedUserAddress address, long ip)
        {
            try
            {
                await _ipAddressService.Save(new IpAddressRecord()
                {
                    IpAddress = ip.ToString(),
                    Country_Code = address.CountryID,
                    Country = address.Country,
                    Region = address.State,
                    City = address.City,
                    Latitude = address.Latitude.ToString(),
                    Longitude = address.Longitude.ToString(),
                    Time_Zone = address.offset,
                    Zip_Code = address.zipCode
                });

            }
            catch (Exception exp)
            {
                await this._logger.InsertLogAsync(LogLevel.Error,
                      "Campaign Popup Module. Failed to Sava Data In Database", exp.Message);
            }
        }
        public async void GetDataFromAPI(long ip, string ipAddress, IpBasedUserAddress address)
        {
            try
            {

                using (WebClient client = new WebClient())
                {
                    dynamic ipInfo = JsonConvert.DeserializeObject(client.DownloadString(
                        (await _settingService.GetSettingByKeyAsync<string>("IpServiceUrl"))
                       .Replace("{ip}", ipAddress)));
                    address.City = ipInfo.city;
                    address.CountryID = ipInfo.country_code;
                    address.Country = ipInfo.country_name;
                    address.Longitude = ipInfo.longitude;
                    address.Latitude = ipInfo.latitude;
                    address.State = ipInfo.region;
                    address.zipCode = ipInfo.postal;
                    try
                    {
                        address.offset = ipInfo.time_zone.offset;
                    }
                    catch { }
                    IpBasedUserAddress usrAddress = address;
                    Task.Run(() => { SaveDataInDB(usrAddress, ip); });
                }
            }
            catch (Exception exp)
            {
                await this._logger.InsertLogAsync(LogLevel.Error,
                      "Campaign Popup Module. Failed to get Data from API", exp.Message);

            }
        }
        #endregion  IpBased Functions

        #region Common
        protected virtual async Task<string> RouteUrlAsync(int storeId = 0, string routeName = null, object routeValues = null)
        {
            //try to get a store by the passed identifier
            var store = await _storeService.GetStoreByIdAsync(storeId) ?? await _storeContext.GetCurrentStoreAsync()
                ?? throw new Exception("No store could be loaded");

            //ensure that the store URL is specified
            if (string.IsNullOrEmpty(store.Url))
                throw new Exception("URL cannot be null");

            //generate a URL with an absolute path
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            var url = new PathString(urlHelper.RouteUrl(routeName, routeValues));

            //remove the application path from the generated URL if exists
            var pathBase = _actionContextAccessor.ActionContext?.HttpContext?.Request?.PathBase ?? PathString.Empty;
            url.StartsWithSegments(pathBase, out url);

            //compose the result
            return Uri.EscapeUriString(WebUtility.UrlDecode($"{store.Url.TrimEnd('/')}{url}"));
        }
        #endregion

        private static string CalculateMD5Hash(string input)
        {
            // Step 1, calculate MD5 hash from input.
            var md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // Step 2, convert byte array to hex string.
            var sb = new StringBuilder();
            foreach (var @byte in hash)
            {
                sb.Append(@byte.ToString("X2"));
            }
            return sb.ToString();
        }
        public class MergeFields
        {
            public MergeFields()
            {
                CONTACT = "";
                SMSPHONE = "";
                CATNAME = "";
           
            }
            public string PRODUCTID { get; set; }
            public string FNAME { get; set; }
            public string CATNAME { get; set; }
            public string CONTACT { get; set; }
            public string SMSPHONE { get; set; }

            public string CARTURL { get; set; }
            public string ADDTOCART { get; set; }
        }
        public class root
        {
            public string status { get; set; }
            public MergeFields merge_fields { get; set; }
            public location location { get; set; }
            public string email_address { get; set; }
        }
        public class location
        {
            public double latitude { get; set; }
            public double longitude { get; set; }
        }
        public class IpBasedUserAddress
        {
            public string City { get; set; }
            public string State { get; set; }
            public string Country { get; set; }
            public string CountryID { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public string offset { get; set; }
            public string zipCode { get; set; }
        }
    }
}

