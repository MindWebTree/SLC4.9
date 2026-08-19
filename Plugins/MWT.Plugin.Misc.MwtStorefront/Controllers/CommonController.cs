using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Services.MailChimp;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Infrastructure;
using Nop.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MWT.Plugin.Misc.MwtStorefront.Controllers
{
    [AutoValidateAntiforgeryToken]
    public partial class CommonController : BasePublicController
    {

        #region Fields
        private readonly IWorkContext _workContext;
        private readonly IMailchimpService _mailchimpService;

        #endregion

        #region Ctor

        public CommonController(IWorkContext workContext, IMailchimpService mailchimpService)
        {
            _workContext = workContext;
            _mailchimpService = mailchimpService;
        }

        #endregion

        #region Google Feed

        public virtual async Task<IActionResult> CustomGoogleFeed()
        {
            var _nopFileProvider = EngineContext.Current.Resolve<INopFileProvider>();
            var filePath = _nopFileProvider.MapPath($"/wwwroot/data-feed/SLC_data-feed.xml");
            if (_nopFileProvider.FileExists(filePath))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(filePath);

                string xmlContent = xmlDoc.OuterXml;
                Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0";
                Response.Headers["Pragma"] = "no-cache";
                Response.Headers["Expires"] = "0";
                return Content(xmlContent, "text/xml");
            }

            return Content(string.Empty);
        }

        #region MailChimp Events

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> MailchimpEvents(string email, string fromwhere)
        {
    

            string name = "";
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (string.IsNullOrEmpty(email) && customer != null)
            {
                if (!string.IsNullOrEmpty(customer.Email))
                    email = customer.Email;
                else
                    email = customer.Email;
            }
            if (!string.IsNullOrEmpty(email))
            {
                string firstName = customer.FirstName;
                string lastName = customer.LastName;
                name = (firstName ?? "" + " " + lastName ?? "").Trim();
                var _httpContextAccessor = EngineContext.Current.Resolve<IHttpContextAccessor>();
                string url = _httpContextAccessor.HttpContext?.Request.Headers["Referer"];
                string absoluteUrl = _httpContextAccessor.HttpContext?.Request.Headers["Referer"];
                string userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"];
                if (string.IsNullOrEmpty(name))
                    name = customer.Username;
                await _mailchimpService.CustomerSignup(email, name ?? "", new System.Collections.Generic.List<string>()
                {
                   fromwhere
                }, url, userAgent);
            }
            return new JsonResult(new { Status = HttpStatusCode.OK });
        }
        #endregion
        #endregion
    }
}
