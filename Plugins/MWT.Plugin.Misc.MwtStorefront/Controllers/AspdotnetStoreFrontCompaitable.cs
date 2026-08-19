using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Catalog;
using Nop.Services.Seo;
using Nop.Services.Topics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Controllers
{
    public partial class AspdotnetStoreFrontCompaitable : Controller
    {
        #region Fields


        private readonly ITopicService _topicService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IProductService _productService;

        #endregion

        #region Ctor

        public AspdotnetStoreFrontCompaitable(
            ITopicService topicService,
            IUrlRecordService urlRecordService,
            IHttpContextAccessor httpContextAccessor,
            IProductService productService
            )
        {

            _topicService = topicService;
            _urlRecordService = urlRecordService;
            _httpContextAccessor = httpContextAccessor;
            _productService = productService;

        }

        #endregion

        #region Methods



        //in versions 2.00-3.20 we had SystemName in topic URLs
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> RedirectTopicBySystemName(string systemName)
        {
            var topic = await _topicService.GetTopicBySystemNameAsync(systemName);
            if (topic == null)
                return RedirectPermanent("Homepage"); ;

            return RedirectToRoutePermanent("Topic", new { SeName = await _urlRecordService.GetSeNameAsync(topic) });
        }

        public virtual async Task<IActionResult> RedirectCartPage()
        {
            string url = "/cart";

            foreach (var query in _httpContextAccessor.HttpContext.Request.Query)
            {
                url += (url.IndexOf("?") > 0 ? "&" : "?") + query.Key + "=" + query.Value;
            }

            return RedirectPermanent(url);
        }

        public virtual async Task<IActionResult> RedirectCheckoutPage()
        {
            string url = "/onepagecheckout";

            foreach (var query in _httpContextAccessor.HttpContext.Request.Query)
            {
                url += (url.IndexOf("?") > 0 ? "&" : "?") + query.Key + "=" + query.Value;
            }

            return RedirectPermanent(url);
        }

        public virtual async Task<IActionResult> RedirectAccountPage()
        {
            string url = "/customer/info";

            foreach (var query in _httpContextAccessor.HttpContext.Request.Query)
            {
                url += (url.IndexOf("?") > 0 ? "&" : "?") + query.Key + "=" + query.Value;
            }

            return RedirectPermanent(url);
        }

        public virtual async Task<IActionResult> RedirectAddressPage()
        {
            string url = "/customer/addresses";

            foreach (var query in _httpContextAccessor.HttpContext.Request.Query)
            {
                url += (url.IndexOf("?") > 0 ? "&" : "?") + query.Key + "=" + query.Value;
            }

            return RedirectPermanent(url);
        }
        public virtual async Task<IActionResult> RedirectAddressDetailPage()
        {
            int.TryParse(_httpContextAccessor.HttpContext.Request.Query["addressId"], out int addressId);

            string url = addressId == 0 ? "/customer/addressadd" : "/customer/addressedit/" + addressId;

            foreach (var query in _httpContextAccessor.HttpContext.Request.Query)
            {
                if (query.Key != "addressId")
                    url += (url.IndexOf("?") > 0 ? "&" : "?") + query.Key + "=" + query.Value;
            }

            return RedirectPermanent(url);
        }
        public virtual async Task<IActionResult> RedirectConfirmationDetailPage()
        {
            int.TryParse(_httpContextAccessor.HttpContext.Request.Query["orderNumber"], out int orderNumber);

            string url = "/checkout/completed?orderId=" + orderNumber;

            foreach (var query in _httpContextAccessor.HttpContext.Request.Query)
            {
                if (query.Key != "orderNumber")
                    url += (url.IndexOf("?") > 0 ? "&" : "?") + query.Key + "=" + query.Value;
            }

            return RedirectPermanent(url);
        }

        public virtual async Task<IActionResult> HandleCustomOrderRedirect()
        {
            string url = "/CheckoutCustomOrder";
            foreach (var query in _httpContextAccessor.HttpContext.Request.Query)
            {
                if (query.Key == "Invid")
                    url += (url.IndexOf("?") > 0 ? "&" : "?") + "OrderId" + "=" + query.Value;
                else
                    url += (url.IndexOf("?") > 0 ? "&" : "?") + query.Key + "=" + query.Value;
            }

            return RedirectPermanent(url);
        }


        public virtual async Task<IActionResult> HandleImageRedirect(string imagename)
        {
            if (string.IsNullOrEmpty(imagename))
            {
                return RedirectToRoutePermanent("Homepage");
            }

            string extractedString = imagename.Split(new char[] { '_', '.' })[0];
            if (!int.TryParse(extractedString, out int extractedValue))
            {
                return RedirectToRoutePermanent("Homepage");
            }

            var product = await _productService.GetProductByIdAsync(extractedValue);
            if (product != null && product.Published && !product.Deleted)
            {
                return RedirectToRoutePermanent("Product", new { product.Id, seName = await _urlRecordService.GetSeNameAsync(product) });
            }

            return RedirectToRoutePermanent("Homepage");
        }

        #endregion
    }
}
