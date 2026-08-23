using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Services.MailChimp;
using MWT.Nop.Core.Services.Message;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Web.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Controllers
{
    public partial class NewsletterController : BasePublicController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly ICustomNewsLetterSubscriptionService _newsLetterSubscriptionService;

        #endregion

        #region Ctor

        public NewsletterController(ILocalizationService localizationService,
            ICustomNewsLetterSubscriptionService newsLetterSubscriptionService)
        {
            _localizationService = localizationService;
            _newsLetterSubscriptionService=newsLetterSubscriptionService;
        }

        #endregion

        #region Methods

        [CheckAccessClosedStore(true)]
        [HttpPost]
        [IgnoreAntiforgeryToken]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> CustomSubscribeNewsletter(string email, bool subscribe, string fromWhere, int productId = 0,
            int categoryId = 0)
        {
            string result;
            var success = false;
            var _mailChimpService = EngineContext.Current.Resolve<IMailchimpService>();
            var _httpContextAccessor = EngineContext.Current.Resolve<IHttpContextAccessor>();
            var _settingService = EngineContext.Current.Resolve<ISettingService>();
            string listId = await _settingService.GetSettingByKeyAsync<string>("MailChimp.ItemStock.ListId");
            if (!CommonHelper.IsValidEmail(email))
            {
                result = await _localizationService.GetResourceAsync("Newsletter.Email.Wrong");
            }
            else if (await _newsLetterSubscriptionService.CheckEmailSubscriber(email, listId))
            {
                result = await _localizationService.GetResourceAsync("Newsletter.Email.Exist");
            }
            else
            {
                success = true;
                result = await _localizationService.GetResourceAsync("Newsletter.SubscribeEmailSent");
                string url = _httpContextAccessor.HttpContext?.Request.Headers["Referer"];
                string absoluteUrl = _httpContextAccessor.HttpContext?.Request.Headers["Referer"];
                string userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"];
                await _mailChimpService.NewsLetterSignup(email, fromWhere, url, absoluteUrl, userAgent, fromWhere, productId, categoryId);
            }
            return Json(new
            {
                Success = success,
                Result = result,
            });
        }

        #endregion

    }
}
