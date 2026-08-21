using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Domain.Custom.Campaign_Management;
using MWT.Nop.Core.Domain.Marketing;
using MWT.Nop.Core.Infrastructure;
using MWT.Nop.Core.Service.Campaign_Management;
using MWT.Nop.Core.Services.MailChimp;
using MWT.Nop.Core.Services.Message;
using MWT.Plugin.Misc.MwtStorefront.Models.Catalog.Campaign_Management;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Services.Helpers;
using Nop.Services.Messages;
using Nop.Web.Controllers;
using System.Net;
using static MWT.Nop.Core.Services.MailChimp.MailchimpService;

namespace MWT.Plugin.Misc.MwtStorefront.Controllers
{
    public class CampaignManagementController : BasePublicController
    {
        private readonly IWebHelper _webHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICampaignManagementService _campaignManagementService;
        private readonly MarketingSettings _marketingSettings;
        private readonly IWorkContext _workContext;
        private readonly ICustomWorkContext _customWorkContext;
        private readonly IMailchimpService _mailchimpService;
        private readonly ICustomNewsLetterSubscriptionService _newsLetterSubscriptionService;

        public CampaignManagementController(IHttpContextAccessor httpContextAccessor, ICampaignManagementService campaignManagementService,
            MarketingSettings marketingSettings, IWorkContext workContext, ICustomWorkContext customWorkContext, IMailchimpService mailchimpService, ICustomNewsLetterSubscriptionService newsLetterSubscriptionService,
            IWebHelper webHelper)
        {
            _httpContextAccessor = httpContextAccessor;
            _campaignManagementService = campaignManagementService;
            _marketingSettings = marketingSettings;
            _workContext = workContext;
            _customWorkContext = customWorkContext;
            _mailchimpService = mailchimpService;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _webHelper = webHelper;
        }

        #region Campaign
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> ShowPopup(string url, string ip, bool isexitpopup)
        {
            ip = _webHelper.GetCurrentIpAddress();
            int openedCampaignID = 0;
            List<int> activatedCampaignsIds = new List<int>();
            List<int> activatedCampaignStrip = new List<int>();

            string campaignCookie = _httpContextAccessor.HttpContext?.Request?.Cookies["campaigncookie"];
            foreach (var cookie in _httpContextAccessor.HttpContext?.Request?.Cookies)
            {

                if (cookie.Key.StartsWith("campaigncookie"))
                {
                    int.TryParse(cookie.Value, out openedCampaignID);
                    activatedCampaignsIds.Add(openedCampaignID);
                }
            }
            foreach (var cookie in _httpContextAccessor.HttpContext?.Request?.Cookies)
            {

                if (cookie.Key.StartsWith("stripcampaigncookie"))
                {
                    int.TryParse(cookie.Value, out openedCampaignID);
                    activatedCampaignStrip.Add(openedCampaignID);
                }
            }
            var result = new CampaignPopupResultModel();

            Guid customerGuid = (await _workContext.GetCurrentCustomerAsync())?.CustomerGuid ?? Guid.NewGuid();


            int deviceType = _customWorkContext.GetActualDevice(_httpContextAccessor.HttpContext?.Request.Headers["User-Agent"]);

            var campaigns = await _campaignManagementService.GetCampaignPopup(url, deviceType, customerGuid.ToString(), _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"], isexitpopup, _marketingSettings.EnableEmailExclusiveOffer);

            foreach (var campaign in campaigns)
            {
                if (
                     (string.IsNullOrEmpty(campaign.iconImage) && string.IsNullOrEmpty(campaign.IconHtml) && !activatedCampaignsIds.Contains(campaign.CampaignId))
                    ||

                    ((!string.IsNullOrEmpty(campaign.iconImage) || !string.IsNullOrEmpty(campaign.IconHtml) && _marketingSettings.EnableEmailExclusiveOffer) || (!activatedCampaignsIds.Contains(campaign.CampaignId)
                    ||
                   ((!string.IsNullOrEmpty(campaign.iconImage) || !string.IsNullOrEmpty(campaign.IconHtml))
                   && !activatedCampaignStrip.Contains(campaign.CampaignId) && _marketingSettings.EnableEmailExclusiveOffer
                   ))))
                {
                    result = new CampaignPopupResultModel
                    {
                        CampaignId = campaign.CampaignId,
                        Title = campaign.Title,
                        showOnExit = campaign.showOnExit,
                        triggerOn_Secs = campaign.triggerOn_Secs,
                        htmlContent = campaign.htmlContent,
                        duration_Days = campaign.duration_Days,
                        positionStyle = campaign.positionStyle,
                        iconPositionStyle = campaign.iconPositionStyle,
                        iconImage = campaign.iconImage,
                        iconHtml = campaign.IconHtml,
                        isPopupDisplayed = campaign.IsPopupDisplayed,
                        isUserSubscribed = campaign.IsUserSubscribed,
                        isSideBarDisplayed = campaign.IsUserSubscribed ? true : activatedCampaignStrip.Contains(campaign.CampaignId),
                        isEmailExclusiveOfferEnabled =
                        !string.IsNullOrEmpty(campaign.iconImage) || !string.IsNullOrEmpty(campaign.IconHtml) ? _marketingSettings.EnableEmailExclusiveOffer : false
                    };
                }
            }
            return new JsonResult(result);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> ImpressionPopup(int campaignid, string ip, string country, string city, string url, int durationdays,
            bool isStripCloseEvent = false)
        {

            IpBasedUserAddress address = await _mailchimpService.GetStateAndCity(ip);

            Guid customerGuid = (await _workContext.GetCurrentCustomerAsync())?.CustomerGuid ?? Guid.NewGuid();
            if (address != null)
            {
                country = string.IsNullOrEmpty(address.CountryID) ? "" : address.CountryID;
                city = string.IsNullOrEmpty(address.City) ? "undefined" : address.City;
            }
            var options = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.Now.AddMonths(durationdays),
                Secure = _webHelper.IsCurrentConnectionSecured()
            };
            if (isStripCloseEvent)
            {
                options.Expires = null;
            }
            _httpContextAccessor.HttpContext.Response.Cookies.Append(
                (isStripCloseEvent ? "stripcampaigncookie" : "campaigncookie") + campaignid
                , campaignid.ToString(), options);


            int customerid = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0;
            int deviceType = _customWorkContext.GetActualDevice(_httpContextAccessor.HttpContext?.Request.Headers["User-Agent"]);

            try
            {
                await _campaignManagementService.SavePopupImpression(customerid, customerGuid.ToString(), country, city, isStripCloseEvent ? 4 : 2,
                      url, campaignid, deviceType, _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"]);
            }
            catch
            {

            }

            return new JsonResult(new { Status = HttpStatusCode.OK });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> ConversionPopup(int campaignid, string ip, string country, string city, string url, string email, int productid = 0, string phone = "")
        {

            email = email.Trim();
            phone = (phone ?? "").Trim();
            if (!await _newsLetterSubscriptionService.CheckEmailSubscriber(email, ""))
            {

                ip = _webHelper.GetCurrentIpAddress();
                IpBasedUserAddress address = await _mailchimpService.GetStateAndCity(ip);
                if (address != null)
                {
                    country = string.IsNullOrEmpty(address.CountryID) ? "" : address.CountryID;
                    city = string.IsNullOrEmpty(address.City) ? "undefined" : address.City;
                }

                int customerid = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0;
                int deviceType = _customWorkContext.GetActualDevice(_httpContextAccessor.HttpContext?.Request.Headers["User-Agent"]);
                string message = "";
                bool static_message = false;
                Guid customerGuid = (await _workContext.GetCurrentCustomerAsync())?.CustomerGuid ?? Guid.NewGuid();
                await _campaignManagementService.SavePopupConversion(customerid, customerGuid.ToString(), country, city, 3,
                     url, campaignid, email, phone, deviceType, _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"]);



                await _mailchimpService.PopupSignup(campaignid, email, url, ip, _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"], address, "", phone);

                //Marketo marketo = new Marketo();
                //marketo.MarketoOperation(email, "", "", "", "SignUpPopUp", productid, "", "", "", "","",Convert.ToInt32(categoryID));

                var template = await _campaignManagementService.GetCampaignResponseTemplate(campaignid);
                if (template == null)
                {
                    static_message = true;
                    message = "<span id=\"close-thanks-campaign-popup\" class=\"static\">×</span><h3 style='text-align: center;'>Thank You for Your Subscription.</h3>";
                }
                else
                    message = template.HtmlContent;

                var result = new CampaignPopupResult
                {
                    static_message = static_message,
                    message = message,
                    isError = false
                };

                return new JsonResult(result);
            }
            else
            {
                var result = new CampaignPopupResult
                {
                    message = "Email is already subscribed!!.",
                    isError = true
                };
                return new JsonResult(result);
            }
        }
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> ExitPopup(int campaignid, string ip, string country, string city, string url)
        {
            IpBasedUserAddress address = await _mailchimpService.GetStateAndCity(ip);
            if (address != null)
            {
                country = string.IsNullOrEmpty(address.CountryID) ? "" : address.CountryID;
                city = string.IsNullOrEmpty(address.City) ? "undefined" : address.City;
            }
            int customerid = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0;
            int deviceType = _customWorkContext.GetActualDevice(_httpContextAccessor.HttpContext?.Request.Headers["User-Agent"]);
            ;
            try
            {
                Guid customerGuid = (await _workContext.GetCurrentCustomerAsync())?.CustomerGuid ?? Guid.NewGuid();
                await _campaignManagementService.SavePopupImpression(customerid, customerGuid.ToString(), country, city, 9,
                      url, campaignid, deviceType, _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"]);
            }
            catch
            {

            }
            return new JsonResult(new { Status = HttpStatusCode.OK });
        }



        #endregion

    }
}
