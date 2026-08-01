
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Web.Framework.Mvc.Routing;

namespace MWT.Nop.Plugin.Widgets.Catalog.Infrastructure
{
    /// <summary>
    /// Represents provider that provided routes used for backward compatibility with 2.x versions of nopCommerce
    /// </summary>
    public partial class AspdotnetStoreFrontCompaitableRouteProvider : IRouteProvider
    {
        #region Methods

        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="endpointRouteBuilder">Route builder</param>
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapControllerRoute("", "topic/{SystemName}",
           new { controller = "AspdotnetStoreFrontCompaitable", action = "RedirectTopicBySystemName" });

            endpointRouteBuilder.MapControllerRoute("", "shoppingcartitems",
         new { controller = "AspdotnetStoreFrontCompaitable", action = "RedirectCartPage" });

            endpointRouteBuilder.MapControllerRoute("", "checkout",
        new { controller = "AspdotnetStoreFrontCompaitable", action = "RedirectCheckoutPage" });

            endpointRouteBuilder.MapControllerRoute("", "account",
        new { controller = "AspdotnetStoreFrontCompaitable", action = "RedirectAccountPage" });

            endpointRouteBuilder.MapControllerRoute("", "address",
 new { controller = "AspdotnetStoreFrontCompaitable", action = "RedirectAddressPage" });

            endpointRouteBuilder.MapControllerRoute("", "address/detail",
new { controller = "AspdotnetStoreFrontCompaitable", action = "RedirectAddressDetailPage" });

            endpointRouteBuilder.MapControllerRoute("", "/checkoutconfirmation/confirmation",
new { controller = "AspdotnetStoreFrontCompaitable", action = "RedirectConfirmationDetailPage" });

            endpointRouteBuilder.MapControllerRoute("", "images/product/large/{imagename}",
   new { controller = "AspdotnetStoreFrontCompaitable", action = "HandleImageRedirect" });


            endpointRouteBuilder.MapControllerRoute("", "images/product/medium/{imagename}",
               new { controller = "AspdotnetStoreFrontCompaitable", action = "HandleImageRedirect" });

            endpointRouteBuilder.MapControllerRoute("", "images/product/icon/{imagename}",
               new { controller = "AspdotnetStoreFrontCompaitable", action = "HandleImageRedirect" });


        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => -1000;

        #endregion
    }
}